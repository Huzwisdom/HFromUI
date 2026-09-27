using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HThread.HTask;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HFromUI.HThread
{
    using HFromUI.HLangage;
    /// <summary>
    /// 任务调度器：按每个 HTaskData 预先设置的 PreconditionGUID / NextGUID 组成GUID依赖网（支持扇出/扇入并行）。
    /// 调度器自身只有一个调度 Thread，循环轮询各任务的 RunStepState：全部前置令牌已释放时调用 HTaskData.Run 放行，
    /// HTaskData.Run 仅启动其内部工作 Thread（后台线程异步执行）后立即返回，因此多个同时就绪的任务可并行运行（如C、D）；
    /// 轮询发现任务 Runned 后释放其 NextGUID 令牌驱动后续任务（如E、F、G需C、D都完成；M需I、J、K和L都完成）；
    /// 全部任务 Runned 后调度退出。
    /// 接线约定：上游任务把自己的GUID放入 NextGUID，下游任务把上游GUID放入 PreconditionGUID，
    /// 例如 A.NextGUID={A.GuidCode}，B.PreconditionGUID={A.GuidCode}；无前置的起始任务 PreconditionGUID 留空。
    /// 异步约定：Initialization/GetData/Run/Stop/Pause/EStop 全部不阻塞调用线程；
    /// Run 启动调度线程后立即返回，需要等待调度结束时调用 Wait / Wait(超时毫秒)。
    ///
    /// 【使用说明】
    /// 1. 创建任务节点：new 若干 HTaskData，设置 Name、TaskType（Start/Middle/End）、GroupName，
    ///    并通过 InitializationFunc/GetDataFunc/RunFunc/PauseFunc/ContinueFunc/StopFunc/EStopFunc 挂接业务委托（详见 HTaskData）。
    /// 2. 注册任务：把每个任务以其自身 GuidCode 为键放入 HTaskDatas（调度器按键识别任务，未注册的GUID只会被当作永不满足的前置条件）。
    /// 3. GUID接线：上游任务把“本任务完成后要释放的令牌”加入 NextGUID（约定填自己的 GuidCode）；
    ///    下游任务把“必须先完成的上游令牌”加入 PreconditionGUID。放行判定为 AND 语义：PreconditionGUID 中所有令牌都已释放才启动。
    ///    3.1 扇出（一对多并行）：B、C 的 PreconditionGUID 都填 A.GuidCode，A 完成后 B、C 在各自工作线程上并行运行；
    ///    3.2 扇入（多对一汇聚）：E 的 PreconditionGUID 同时填 C.GuidCode、D.GuidCode，必须 C 与 D 都 Runned 才放行 E。
    /// 4. 分组：每组必须恰好一个 TaskType=Start 和一个 TaskType=End 且 GroupName 相同；Middle 可有多个、GroupName 可空。
    ///    调度器以“所有组的 End 任务全部 Runned”作为整轮结束条件（因此接线时必须保证 End 是真正的流程汇点）。
    /// 5. 生命周期：先 Initialization()（校验分组并复位全部任务/令牌）→ 可选 GetData() 取数 → Run() 非阻塞启动调度
    ///    → Wait()/Wait(超时) 阻塞等待整轮结束并取最终 OK；运行中可 Pause()（再次调用为继续）、Stop()（正常停止）、EStop()（急停）。
    /// 6. 循环模式：IsLoopRun=true 时，所有 End 完成后不退出，而是自动重新 Initialization 复位令牌与任务状态，从头再跑一轮，如此反复；
    ///    运行期间把 IsLoopRun 置 false，当前轮跑完即正常结束。
    /// 7. 计时：RunTimer 为整轮CT计时器，Run 时 Reset+Start，正常/失败结束 Stop 冻结；循环模式跨轮连续计时；Stop/EStop 复位清零。
    /// 8. 线程模型：本类只有一个调度 Thread（后台，名为 HTaskStart），负责轮询放行、令牌释放、无进展判定与统一停止；
    ///    每个 HTaskData.Run 再各起一个工作 Thread 执行业务，故同时就绪的任务天然并行。Stop/EStop/Pause 均为协作式：仅置状态，
    ///    真正的停止动作（含 Join 工作线程）在调度线程内执行，保证外部调用不阻塞，需要等停止完成再调用 Wait。
    /// </summary>
    /// <example>
    /// <code>
    /// // （一）A→B→E 直线接线
    /// HTaskStart start = new HTaskStart { Name = "调度器" };
    /// HTaskData a = new HTaskData { Name = "A", TaskType = HTaskType.Start, GroupName = "G" };
    /// HTaskData b = new HTaskData { Name = "B", TaskType = HTaskType.Middle };
    /// HTaskData e = new HTaskData { Name = "E", TaskType = HTaskType.End, GroupName = "G" };
    /// start.HTaskDatas[a.GuidCode] = a;
    /// start.HTaskDatas[b.GuidCode] = b;
    /// start.HTaskDatas[e.GuidCode] = e;
    /// // 上游释放自己的令牌，下游登记同一令牌作为前置
    /// a.NextGUID.Add(a.GuidCode);
    /// b.PreconditionGUID.Add(a.GuidCode);
    /// b.NextGUID.Add(b.GuidCode);
    /// e.PreconditionGUID.Add(b.GuidCode);
    ///
    /// // （二）扇出+扇入：A 后 C、D 并行，C 与 D 都完成才放行 E（即 C &amp;&amp; D → E）
    /// HTaskData c = new HTaskData { Name = "C" };
    /// HTaskData d = new HTaskData { Name = "D" };
    /// start.HTaskDatas[c.GuidCode] = c;
    /// start.HTaskDatas[d.GuidCode] = d;
    /// c.PreconditionGUID.Add(a.GuidCode);   // C 等 A
    /// d.PreconditionGUID.Add(a.GuidCode);   // D 也等 A：A 完成后 C、D 并行（扇出）
    /// c.NextGUID.Add(c.GuidCode);
    /// d.NextGUID.Add(d.GuidCode);
    /// e.PreconditionGUID.Clear();
    /// e.PreconditionGUID.Add(c.GuidCode);   // E 等 C
    /// e.PreconditionGUID.Add(d.GuidCode);   // E 同时等 D：两个令牌都释放才放行（扇入，AND 语义）
    ///
    /// // 启动并等待整轮结果
    /// if (start.Initialization())
    /// {
    ///     start.Run();
    ///     OK ok = start.Wait();
    /// }
    /// </code>
    /// </example>
    public class HTaskStart : HTaskDataBase
    {
        /// <summary>
        /// 全部待调度任务表：键约定为任务自身的 <see cref="HFromUI.HBase.HDataBase.GuidCode"/>，值为任务节点。
        /// 使用 ConcurrentDictionary 是为了允许外部线程在调度前后安全增删任务；调度线程只遍历不修改该表。
        /// 接线中的 PreconditionGUID/NextGUID 必须与这里的键对应，引用了未注册GUID的任务将永远无法被放行（最终触发无进展失败）。
        /// </summary>
        public ConcurrentDictionary<string, HTaskData> HTaskDatas { internal set; get; } = new ConcurrentDictionary<string, HTaskData>();

        /// <summary>
        /// 循环运行开关：打开后，全部任务从开头到结尾跑完一轮不退出，而是重新Initialization复位所有任务和令牌状态，
        /// 再从开头调度到结尾，如此反复；外部关闭开关后，当前轮跑完即正常结束。
        /// </summary>
        public bool IsLoopRun { set; get; } = false;

        /// <summary>
        /// 整轮CT计时器（实例化使用，非静态）：Run启动调度时Reset（上次CT存入LastMilliseconds）并Start，
        /// 循环模式下跨轮连续计时（从开始到最终结束），全部End完成/失败时Stop冻结CT；Stop/EStop复位清零。
        /// </summary>
        public HRunTimer RunTimer { get; } = new HRunTimer();

        private Thread thread;                  // 调度线程（HTaskStart内唯一的Thread）
        private readonly List<string> runnedGUID = new List<string>();       // 已释放的令牌集合（完成任务NextGUID的并集，List允许同一GUID重复存在）
        private readonly List<string> startedTaskGUID = new List<string>();  // 已放行的任务GUID，防止重复启动（允许重复记录）
        private List<HTaskData> endTasks = new List<HTaskData>();                  // 各组的End结束任务，全部Runned即整轮结束
        private int lastFinishedCount = -1;     // 上一轮已完成任务数（与本轮比较判断是否有进展；-1表示尚未记录过任何一轮）
        private int lastStartedCount = -1;      // 上一轮已放行任务数（与finishedCount同时比较，连续两轮双零增长且无运行中任务才判定死锁/无进展）
        private OK runOK = true;                // 调度结果（调度线程写入，Wait读取并返回给外部）

        private RunStepState runStepState = RunStepState.None;
        /// <summary>
        /// 调度器运行状态（外部只读，状态迁移仅由本类内部完成）。
        /// 典型流转：None → Ready（Initialization 成功）→ Run（调用 Run）→ Running（调度循环中）
        /// → Runned（全部 End 完成）；旁路：Pause（暂停，可回 Running）、Stop/EStop（外部停止）、RunFail（任务失败或前置条件无法满足）。
        /// </summary>
        /// <value>枚举含义详见 <see cref="HFromUI.HThread.HTask.RunStepState"/> 各成员注释。</value>
        public RunStepState RunStepState
        {
            get { return runStepState; }
            protected set { runStepState = value; }
        }

        /// <summary>
        /// 初始化整轮调度：调用时机为 Run 之前（循环模式下每轮结束后也会由调度线程自动再调一次做幂等复位）。
        /// 依次完成：分组结构校验并缓存 End 任务列表 → 清空已释放令牌/已放行记录/无进展计数 → 逐个调用各任务的 Initialization。
        /// 该方法本身同步执行但不启动任何线程；任一任务初始化失败立即中断并返回失败。
        /// </summary>
        /// <returns>OK：全部任务初始化成功返回成功（状态置 Ready）；分组非法或任一任务初始化失败返回带中文 Message 的失败结果，状态回退 None。</returns>
        public override OK Initialization()
        {
            RunStepState = RunStepState.None;
            // 先校验分组结构：每组恰好一个Start、一个End，Middle可有多个；同时缓存End任务列表
            OK checkOK = CheckTaskGroups();
            if (!checkOK)
            {
                return checkOK;
            }
            // 复位调度器内部状态（循环模式下靠这里把上一轮的令牌与计数全部清零）
            runnedGUID.Clear();
            startedTaskGUID.Clear();
            lastFinishedCount = -1;
            lastStartedCount = -1;
            runOK = true;
            // 逐任务初始化：每个HTaskData复位自身状态并执行其InitializationFunc多播委托
            foreach (HTaskData task in HTaskDatas.Values)
            {
                if (!task.Initialization())
                {
                    RunStepState = RunStepState.None;
                    return false;
                }
            }
            RunStepState = RunStepState.Ready;
            return true;
        }

        /// <summary>
        /// 分组结构检查：只对Start/End任务按GroupName成对配对——每个组名必须恰好有一个Start和一个End；
        /// Start/End不允许组名为空，且不允许出现孤立的Start（无同组End）或孤立的End（无同组Start）；
        /// Middle中间任务不参与检查（可以不设置GroupName）。同时缓存全部End任务作为整轮结束判定。
        /// </summary>
        /// <returns>OK：校验通过返回成功（endTasks 已缓存）；任务表为空、缺少 Start/End、组名为空或同组 Start/End 数量不是各1个时，返回包含全部问题（分号分隔）的失败结果。</returns>
        public OK CheckTaskGroups()
        {
            endTasks = new List<HTaskData>();
            if (HTaskDatas.Count == 0)
            {
                return new OK() { Message = HTranslation.GetContent("HTaskDatas为空，没有可调度的任务"), IsSuccess = false, Error = -1 };
            }
            List<HTaskData> startTasks = HTaskDatas.Values.Where(t => t.TaskType == HTaskType.Start).ToList();
            List<HTaskData> endTaskList = HTaskDatas.Values.Where(t => t.TaskType == HTaskType.End).ToList();
            if (startTasks.Count == 0 || endTaskList.Count == 0)
            {
                return new OK() { Message = HTranslation.GetContent("任务结构非法：Start开始任务=") + startTasks.Count + HTranslation.GetContent("个，End结束任务=") + endTaskList.Count + HTranslation.GetContent("个（两者至少各1个）"), IsSuccess = false, Error = -1 };
            }
            StringBuilder error = new StringBuilder();
            // 1) Start/End组名不允许为空
            foreach (HTaskData t in startTasks.Where(t => string.IsNullOrWhiteSpace(t.GroupName)))
            {
                AppendGroupError(error, HTranslation.GetContent("Start开始任务[") + t.Name + HTranslation.GetContent("]未设置GroupName"));
            }
            foreach (HTaskData t in endTaskList.Where(t => string.IsNullOrWhiteSpace(t.GroupName)))
            {
                AppendGroupError(error, HTranslation.GetContent("End结束任务[") + t.Name + HTranslation.GetContent("]未设置GroupName"));
            }
            // 2) 按组名配对：同组名必须恰好一个Start和一个End，不允许多个、不允许孤立
            HashSet<string> groupNames = new HashSet<string>();
            foreach (HTaskData t in startTasks) { if (!string.IsNullOrWhiteSpace(t.GroupName)) { groupNames.Add(t.GroupName); } }
            foreach (HTaskData t in endTaskList) { if (!string.IsNullOrWhiteSpace(t.GroupName)) { groupNames.Add(t.GroupName); } }
            foreach (string groupName in groupNames)
            {
                int startCount = startTasks.Count(t => t.GroupName == groupName);
                int endCount = endTaskList.Count(t => t.GroupName == groupName);
                if (startCount == 1 && endCount == 1)
                {
                    continue;
                }
                AppendGroupError(error, HTranslation.GetContent("分组[") + groupName + HTranslation.GetContent("]未成对：Start开始任务=") + startCount + HTranslation.GetContent("个（要求1个），End结束任务=") + endCount + HTranslation.GetContent("个（要求1个）"));
            }
            if (error.Length > 0)
            {
                return new OK() { Message = error.ToString(), IsSuccess = false, Error = -1 };
            }
            endTasks = endTaskList;
            return true;
        }

        /// <summary>向分组检查错误信息追加一条（多条之间自动以中文分号“；”分隔）。</summary>
        /// <param name="error">错误信息收集缓冲，已有内容时先补一个分号再追加。</param>
        /// <param name="message">本条要追加的错误描述。</param>
        private static void AppendGroupError(StringBuilder error, string message)
        {
            if (error.Length > 0)
            {
                error.Append("；");
            }
            error.Append(message);
        }

        /// <summary>
        /// 取数阶段：Initialization 之后、Run 之前按需调用，同步逐个执行各任务的 GetDataFunc（用于运行前准备输入数据）。
        /// 调度循环本身不会自动调用本方法，需要由外部显式调用；任一任务取数失败立即返回失败（后续任务不再执行）。
        /// </summary>
        /// <returns>OK：全部任务取数成功返回成功；首个取数失败的任务返回失败时，本方法立即返回失败。</returns>
        public override OK GetData()
        {
            OK Isok = true;
            foreach (HTaskData task in HTaskDatas.Values)
            {
                if (!task.GetData())
                { return false; }
            }
            return Isok;
        }

        /// <summary>
        /// 非阻塞启动调度：仅启动调度线程后立即返回，不等待任何任务完成；需要等待请调用 Wait。
        /// 启动时复位无进展计数、RunTimer 重新开表；若调度线程已在运行则直接返回成功（不重复启动）。
        /// </summary>
        /// <returns>OK：调度线程已启动（或本就在运行）返回成功；本方法不反映任务成败，最终结果以 Wait 返回值为准。</returns>
        public override OK Run()
        {
            OK Isok = true;
            if (thread != null && thread.IsAlive)
            {
                return Isok;   // 已在调度中，直接返回
            }
            RunStepState = RunStepState.Run;
            runOK = true;
            lastFinishedCount = -1;
            lastStartedCount = -1;
            // 再次运行CT复位重新计时：Reset先把上次CT存入LastMilliseconds，再从0开始
            RunTimer.Reset();
            RunTimer.Start();
            thread = new Thread(ScheduleRun) { IsBackground = true, Name = "HTaskStart" };
            thread.Start();
            return Isok;
        }

        /// <summary>
        /// 阻塞等待调度全部结束（正常完成/失败/停止），返回最终调度结果。
        /// 可在 Run 之后任意时刻调用；Stop/EStop 仅下发状态，调用本方法才能等到停止动作真正执行完毕。
        /// </summary>
        /// <returns>OK：所有 End 正常 Runned 返回成功；任务失败、前置条件无法满足、Stop/EStop 结束时返回失败（Message/Error 标明原因）。</returns>
        public OK Wait()
        {
            Thread t = thread;
            if (t != null)
            {
                t.Join();
                thread = null;
            }
            return runOK;
        }

        /// <summary>
        /// 阻塞等待调度结束，最多等待 millisecondsTimeout 毫秒；超时未结束返回失败结果（不强制中断，仍为协作式停止）。
        /// 注意超时只是放弃等待，调度线程与任务工作线程仍在后台继续运行，可再次调用 Wait 继续等待。
        /// </summary>
        /// <param name="millisecondsTimeout">最大等待毫秒数；超时不清算调度状态，仅返回一次超时失败。</param>
        /// <returns>OK：等待期间调度结束则返回最终调度结果；超时返回 Message 为“等待…超时”的失败结果。</returns>
        public OK Wait(int millisecondsTimeout)
        {
            Thread t = thread;
            if (t != null)
            {
                if (!t.Join(millisecondsTimeout))
                {
                    return new OK() { Message = HTranslation.GetContent("等待HTaskStart调度结束超时(") + millisecondsTimeout + "ms)", IsSuccess = false, Error = -1 };
                }
                thread = null;
            }
            return runOK;
        }

        /// <summary>
        /// 调度轮询（HTaskStart唯一的线程入口）：放行全部前置令牌就绪的任务（HTaskData内部工作Thread异步并行执行），
        /// 发现 Runned 的任务即释放其 NextGUID 令牌，直到全部任务完成。
        /// 每轮扫描间隔 20ms；一轮内依次处理“停止/急停 → 暂停 → 扫描任务（完成放令牌/失败急停/已放行计数/就绪放行）
        /// → End整轮结束判定（含循环复位）→ 无进展死锁判定”。
        /// </summary>
        private void ScheduleRun()
        {
            while (true)
            {
                if (RunStepState == RunStepState.Stop || RunStepState == RunStepState.EStop)
                {
                    // 停止动作在调度线程自身执行（HTaskData.Stop/EStop 内部会 Join 等待其工作Thread退出），
                    // 保证外部调用 Stop/EStop 不阻塞；需要等停止完成由调用方再 Wait
                    StopAllTasks(RunStepState == RunStepState.EStop);
                    // 停止/急停即复位整轮CT：中断前的累计CT先存入LastMilliseconds，再清零停表
                    RunTimer.Reset();
                    RunTimer.Stop();
                    runOK = new OK() { Message = RunStepState.ToString(), IsSuccess = false, Error = (int)RunStepState };
                    return;
                }
                if (RunStepState == RunStepState.Pause)
                {
                    // 调度器暂停期间不放行新任务，正在运行的任务由 Pause() 联动暂停
                    Thread.Sleep(20);
                    continue;
                }

                RunStepState = RunStepState.Running;
                int finishedCount = 0;     // 已 Runned 的任务数
                int runningCount = 0;      // 已放行且正在运行的任务数
                bool startedAny = false;   // 本轮是否新放行了任务
                foreach (KeyValuePair<string, HTaskData> kv in HTaskDatas)
                {
                    HTaskData task = kv.Value;
                    if (task.RunStepState == RunStepState.Runned)
                    {
                        finishedCount++;
                        // 释放该任务的后续令牌；同一GUID被多个任务重复释放时在List中保留多条记录，
                        // 下游AND判定只看Contains是否命中，重复记录不影响放行
                        foreach (string next in task.NextGUID)
                        {
                            runnedGUID.Add(next);
                        }
                        continue;
                    }
                    if (task.RunStepState == RunStepState.RunFail)
                    {
                        // 一个任务失败：在调度线程内急停其余未完成任务（不阻塞外部调用方），调度以失败结束
                        StopAllTasks(true);
                        RunTimer.Stop();   // 冻结失败时刻的整轮CT（不清零，可直接读取）
                        RunStepState = RunStepState.RunFail;
                        runOK = new OK() { Message = HTranslation.GetContent("任务[") + task.Name + HTranslation.GetContent("]运行失败"), IsSuccess = false, Error = (int)RunStepState.RunFail };
                        return;
                    }
                    if (startedTaskGUID.Contains(kv.Key))
                    {
                        // 已放行、内部工作Thread正在 Run/Running/RunHold（或被联动 Pause）
                        if (task.RunStepState == RunStepState.Run
                            || task.RunStepState == RunStepState.Running
                            || task.RunStepState == RunStepState.RunHold)
                        {
                            runningCount++;
                        }
                        continue;
                    }
                    // 未放行任务：全部前置令牌已就绪才启动（无前置的起始任务立即放行）
                    if (task.PreconditionGUID.All(p => runnedGUID.Contains(p)))
                    {
                        startedTaskGUID.Add(kv.Key);
                        // Run 仅启动 HTaskData 内部工作 Thread 后立即返回，任务在其自身线程上异步执行
                        task.Run();
                        startedAny = true;
                        if (task.RunStepState == RunStepState.Run
                            || task.RunStepState == RunStepState.Running
                            || task.RunStepState == RunStepState.RunHold)
                        {
                            runningCount++;
                        }
                    }
                }

                // 结束条件：所有分组的End结束任务全部Runned（不再以全部任务计数为准）
                if (endTasks.All(t => t.RunStepState == RunStepState.Runned))
                {
                    if (IsLoopRun)
                    {
                        // 整轮完成：重新Initialization幂等复位令牌集合和所有任务状态，从开头再跑一轮；
                        // Initialization 内部已把 runOK、lastXxxCount、runnedGUID、startedTaskGUID 全部复位
                        if (!Initialization())
                        {
                            RunStepState = RunStepState.RunFail;
                            runOK = new OK() { Message = HTranslation.GetContent("循环运行重新Initialization失败"), IsSuccess = false, Error = (int)RunStepState.RunFail };
                            return;
                        }
                        continue;
                    }
                    // 全部End完成：停表冻结从Run开始到结束的整轮CT（循环模式下为多轮累计）
                    RunTimer.Stop();
                    RunStepState = RunStepState.Runned;
                    runOK = true;
                    return;
                }
                int startedCount = startedTaskGUID.Count;
                // 连续两轮放行数、完成数都没有增长且无运行中任务，才说明前置条件永远无法满足（环依赖或GUID未注册）；
                // 不能只看单轮：字典迭代顺序不保证，可能本轮上游刚Runned、下游排在其前面尚未放行，下一轮即可继续推进
                if (!startedAny && runningCount == 0
                    && finishedCount == lastFinishedCount && startedCount == lastStartedCount)
                {
                    RunTimer.Stop();   // 冻结失败时刻的整轮CT（不清零，可直接读取）
                    RunStepState = RunStepState.RunFail;
                    runOK = new OK() { Message = HTranslation.GetContent("任务前置条件无法满足（可能存在环依赖或未注册的GUID）"), IsSuccess = false, Error = -1 };
                    return;
                }
                lastFinishedCount = finishedCount;
                lastStartedCount = startedCount;
                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// 非阻塞停止：仅下发停止状态，实际停止动作由调度线程执行；需要等待停止完成请调用 Wait。
        /// 与 EStop 的区别：Stop 表示正常停止，各任务执行 StopFunc 做收尾；EStop 表示紧急中断，执行 EStopFunc。
        /// </summary>
        /// <returns>OK：状态已置为 Stop 即返回成功（不等待任务真正停止）。</returns>
        public override OK Stop()
        {
            RunStepState = RunStepState.Stop;
            return true;
        }

        /// <summary>
        /// 在调度线程内停止/急停所有尚未完成的任务（HTaskData.Stop/EStop 内部会 Join 等待其工作Thread退出）。
        /// 已 Runned 的任务跳过（结果已产生，不再打扰）。
        /// </summary>
        /// <param name="eStop">true=急停（调用各任务 EStop，语义为立即中断）；false=正常停止（调用各任务 Stop，允许业务收尾）。</param>
        private void StopAllTasks(bool eStop)
        {
            foreach (HTaskData task in HTaskDatas.Values)
            {
                if (task.RunStepState == RunStepState.Runned)
                {
                    continue;
                }
                if (eStop)
                {
                    task.EStop();
                }
                else
                {
                    task.Stop();
                }
            }
        }

        /// <summary>
        /// 暂停/继续切换（同一个方法按当前状态翻转）：调度器处于 Pause 时调用为继续（置 Running），其他运行态调用为暂停（置 Pause）。
        /// 暂停期间调度循环不放行任何新任务，同时把暂停联动下发给所有已放行、处于 Run/Running/Pause 的任务；
        /// 任务工作线程在其 TaskRun 轮询点自旋等待，继续时执行各任务的 ContinueFunc。与 Stop/EStop 不同：Pause 不结束调度、不 Join 线程。
        /// </summary>
        /// <returns>OK：所有被联动任务的 Pause/Continue 委托都成功返回成功；任一任务返回失败则聚合为失败（仍继续切换其余任务）。</returns>
        public override OK Pause()
        {
            OK Isok = true;
            // 暂停/继续切换，同时联动所有已放行且处于 Run/Running/Pause 的任务
            if (RunStepState == RunStepState.Pause)
            {
                RunStepState = RunStepState.Running;
            }
            else
            {
                RunStepState = RunStepState.Pause;
            }
            // 仅联动已放行且在运行态的任务；未放行任务不受影响，放行后会先看到调度器的 Pause 状态
            foreach (HTaskData task in HTaskDatas.Values)
            {
                if (task.RunStepState == RunStepState.Run
                    || task.RunStepState == RunStepState.Running
                    || task.RunStepState == RunStepState.Pause)
                {
                    if (!task.Pause())
                    { Isok = false; }
                }
            }
            return Isok;
        }

        /// <summary>
        /// 非阻塞急停：仅下发急停状态，实际急停动作由调度线程执行；需要等待急停完成请调用 Wait。
        /// 与 Stop 的区别：EStop 语义为紧急中断，不等业务收尾；任务失败时调度器内部也是以 EStop 方式收敛其余任务。
        /// </summary>
        /// <returns>OK：状态已置为 EStop 即返回成功（不等待任务真正停止）。</returns>
        public override OK EStop()
        {
            RunStepState = RunStepState.EStop;
            return true;
        }

    }
}
