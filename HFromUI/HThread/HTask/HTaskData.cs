using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HData.Step;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HFromUI.HThread.HTask
{
    /// <summary>
    /// 任务/调度器运行步骤状态机：数值正负仅为编码习惯（正向为正常推进阶段，负向为中断/失败），判断时请按枚举名比较。
    /// 正常推进链路：None → Ready → Run → Running → Runned（可经 RunHold 保持）；
    /// 中断/失败链路：Pause（可恢复回 Running）、Stop、EStop、RunFail；GetData/Error 为预留状态。
    /// </summary>
    public enum RunStepState
    {
        /// <summary>未初始化/初始状态：刚创建或 Initialization 失败后回退到此状态。</summary>
        None=-1,
        /// <summary>就绪：Initialization 全部成功、等待 Run 放行。</summary>
        Ready=0,
        /// <summary>已启动：Run 已被调用，工作线程刚建立、业务函数尚未开始执行的瞬间状态。</summary>
        Run=1,
        /// <summary>运行中：工作线程正在执行 RunFunc 业务委托。</summary>
        Running=3,
        /// <summary>正常完成：业务函数全部成功且未保持；调度器据此释放 NextGUID 令牌放行下游。</summary>
        Runned=4,
        /// <summary>完成点保持：业务函数已全部执行完成，因 IsHoldAfterRun 开关打开而阻塞保持在完成点（尚未宣布Runned）。</summary>
        RunHold=5,
        /// <summary>取数阶段（预留）：供 GetData 阶段使用，当前调度流程未使用该值。</summary>
        GetData = 50,
        /// <summary>暂停：工作线程在检查点自旋等待，可再次 Pause 恢复为 Running。</summary>
        Pause =-80,
        /// <summary>正常停止：外部请求停止，工作线程退出，允许执行 StopFunc 做收尾。</summary>
        Stop = -100,
        /// <summary>紧急停止：立即中断语义，工作线程退出并执行 EStopFunc，不做业务收尾。</summary>
        EStop=-200,
        /// <summary>运行失败：业务委托返回失败、取消令牌触发或前置条件无法满足（仅调度器）。</summary>
        RunFail=-300,
        /// <summary>错误（预留）：严重错误状态，当前流程未使用。</summary>
        Error=-10000,
    }

    /// <summary>
    /// 任务节点类型标识：Start开始任务与End结束任务必须设置相同的GroupName且成对出现
    /// （每个组名恰好一个Start、一个End），Middle中间运行任务可有多个且可以不设置GroupName；
    /// HTaskStart以所有End任务全部Runned作为整轮结束条件。
    /// </summary>
    public enum HTaskType
    {
        /// <summary>开始任务：每组唯一，无前置令牌即放行</summary>
        Start = 0,
        /// <summary>中间运行任务：每组可有多个，靠PreconditionGUID/NextGUID串接在开始与结束之间</summary>
        Middle = 1,
        /// <summary>结束任务：每组唯一，End任务Runned即代表该组整轮结束</summary>
        End = 2,
    }

    /// <summary>
    /// 任务节点：GUID依赖DAG中的一个执行单元。业务逻辑以 Func&lt;OK&gt; 多播委托形式挂接（InitializationFunc/GetDataFunc/
    /// RunFunc/PauseFunc/ContinueFunc/StopFunc/EStopFunc），Run 时在节点独占的后台工作 Thread 上按挂接顺序串行执行全部 RunFunc，
    /// 执行结束自行把 RunStepState 置为 Runned/RunFail，供 HTaskStart 轮询；节点本身不主动通知调度器。
    ///
    /// 【使用说明】
    /// 1. 创建节点：new HTaskData，设置 Name；按节点在流程中的角色设置 TaskType（Start/Middle/End，默认 Middle），
    ///    Start 与 End 还必须设置相同的 GroupName 且每组各一个。
    /// 2. 挂接业务委托：用 += 挂接各阶段 Func&lt;OK&gt;（可挂多个，按挂接顺序串行调用，任一返回失败即中断该阶段）。
    ///    未挂接的阶段视为空操作直接成功；RunFunc 为空时 Run 立即置 Runned（CT约为0）。
    /// 3. GUID接线：把本任务完成后释放的令牌加入 NextGUID（约定填本节点 GuidCode）；把必须先完成的上游令牌加入 PreconditionGUID。
    ///    多个上游全部完成才会被放行（AND 语义）；同一令牌可被多个下游登记形成扇出，一个下游登记多个上游形成扇入。
    /// 4. 注册与调度：以 GuidCode 为键加入 HTaskStart.HTaskDatas，由调度器统一 Initialization→Run；一般不直接调用本节点的 Run。
    /// 5. 完整生命周期：Initialization（置 Ready）→ GetData（可选）→ Run（置 Run 并起工作线程）→ Running（业务执行）
    ///    → Runned（成功，释放下游令牌）；运行中可被 Pause（再次 Pause 恢复并执行 ContinueFunc）、Stop（收尾后退出）、EStop（立即中断）；
    ///    任一 RunFunc 返回失败或取消令牌触发则置 RunFail。
    /// 6. 完成点保持：IsHoldAfterRun=true 时业务执行完不宣布 Runned，而是停在 RunHold 阻塞点（下游不会被放行，保持时间不计CT），
    ///    外部将其置 false 才继续；Stop/EStop 会立即解除保持。
    /// 7. 计时：RunTimer 为单次运行CT，Run 时 Reset+Start，业务全部完成 Stop 冻结；再次 Run 重新计时；Stop/EStop 复位清零。
    /// 8. 线程模型：每个节点一个后台工作 Thread（名为 "HTaskData-"+Name），与 HTaskStart 的单调度线程配合——
    ///    调度线程只负责“何时放行”，节点工作线程负责“真正干活”，故多个同时就绪节点天然并行。
    /// 9. 数据挂载：InputData/OutputData 为业务自定义的步骤数据列表（HStepDataBase），框架不解释其内容，由各 Func 委托自行读写。
    /// </summary>
    /// <example>
    /// <code>
    /// // 构造 A→B 直线、C 与 D 汇聚到 E（C &amp;&amp; D → E）的任务节点
    /// HTaskData a = new HTaskData { Name = "A", TaskType = HTaskType.Start, GroupName = "G" };
    /// HTaskData b = new HTaskData { Name = "B" };
    /// HTaskData c = new HTaskData { Name = "C" };
    /// HTaskData d = new HTaskData { Name = "D" };
    /// HTaskData e = new HTaskData { Name = "E", TaskType = HTaskType.End, GroupName = "G" };
    /// // 挂接业务（可 += 多个，按顺序串行执行；返回 false 即该节点失败）
    /// a.RunFunc += () =&gt; { /* 业务A */ return true; };
    /// e.RunFunc += () =&gt; { /* 业务E：C、D都完成后才执行 */ return true; };
    /// // A→B
    /// a.NextGUID.Add(a.GuidCode);
    /// b.PreconditionGUID.Add(a.GuidCode);
    /// // B 再驱动 C、D（扇出），C、D 汇聚 E（扇入）
    /// b.NextGUID.Add(b.GuidCode);
    /// c.PreconditionGUID.Add(b.GuidCode);
    /// d.PreconditionGUID.Add(b.GuidCode);
    /// c.NextGUID.Add(c.GuidCode);
    /// d.NextGUID.Add(d.GuidCode);
    /// e.PreconditionGUID.Add(c.GuidCode);
    /// e.PreconditionGUID.Add(d.GuidCode);
    /// </code>
    /// </example>
    public class HTaskData: HTaskDataBase
    {
        private Thread thread;              // 任务工作线程（全部用Thread，不使用Task）；Run时新建，Stop/EStop后Join并置null
        private CancellationTokenSource cts = new CancellationTokenSource();   // 取消令牌源：工作线程每个业务委托执行前检查其Token；当前框架内部未主动Cancel，供外部业务扩展使用

        /// <summary>
        /// 运行CT计时器（实例化使用，非静态）：Run时Reset（上次CT存入LastMilliseconds）并Start重新计时，
        /// 业务函数全部执行完Stop冻结本次CT（RunHold保持时间不计入CT）；Stop/EStop复位清零，再次Run也复位重新计时。
        /// </summary>
        public HRunTimer RunTimer { get; } = new HRunTimer();
        /// <summary>
        /// 前置令牌列表（AND 语义）：列表中每个字符串都必须已出现在调度器的“已释放令牌集合”中，本节点才会被放行。
        /// 内容约定为上游节点的 GuidCode（与上游 NextGUID 中释放的令牌一一对应）；留空表示无前置（起始节点立即放行）。
        /// 多个上游填多个值即扇入；本节点与其他节点填同一个上游令牌即扇出。
        /// </summary>
        public List<string> PreconditionGUID { set; get; }=new List<string>();

        /// <summary>
        /// 后继令牌列表：本节点 Runned 后，调度器把这里的每个字符串释放为“已完成令牌”，供下游 PreconditionGUID 匹配。
        /// 约定只放本节点自身的 GuidCode；若自定义放入多个不同令牌，可实现“一次完成同时点亮多条命名边”的特殊接线。
        /// </summary>
        public List<string> NextGUID { set; get; } = new List<string>();

        /// <summary>节点类型标识：Start开始 / Middle中间 / End结束，默认Middle</summary>
        public HTaskType TaskType { set; get; } = HTaskType.Middle;

        /// <summary>
        /// 分组名：Start/End必须设置相同组名且成对出现（每组名恰好一个Start一个End）；
        /// Middle中间任务可以不设置（null），不参与分组配对检查。
        /// </summary>
        public string GroupName { set; get; } = null;

        /// <summary>
        /// 运行完保持开关：打开后，业务函数全部执行完成也不置Runned，而是进入RunHold状态阻塞在完成点，
        /// 调度器因此不会释放NextGUID令牌（流程停在此任务之后）；外部关闭开关后才置Runned继续放行，
        /// 收到Stop/EStop会立即解除保持。
        /// </summary>
        public bool IsHoldAfterRun { set; get; } = false;

        /// <summary>输入步骤数据列表：框架不解释内容，由业务在 GetDataFunc/RunFunc 中自行读取（通常为运行参数、来料数据等）。</summary>
        public List<HStepDataBase> InputData { set; get; }=new List<HStepDataBase>();

        /// <summary>输出步骤数据列表：框架不解释内容，由业务在 RunFunc 中写入执行产物，供下游节点或外部读取。</summary>
        public List<HStepDataBase> OutputData { set; get; }=new List<HStepDataBase>();

        private RunStepState runStepState= RunStepState.None;

        /// <summary>
        /// 节点运行状态（外部只读，状态迁移由本类内部在对应方法/工作线程中完成；HTaskStart 轮询此属性决定放行/释放令牌/失败收敛）。
        /// </summary>
        /// <value>枚举含义详见 <see cref="RunStepState"/> 各成员注释。</value>
        public RunStepState RunStepState
        { 
          get { return runStepState; }
            protected set {
                runStepState = value;
            }
        }


        /// <summary>初始化阶段业务委托（多播）：Initialization 时按挂接顺序串行执行，全部成功节点才置 Ready；为 null 视为无需初始化。</summary>
        public Func<OK> InitializationFunc;
        /// <summary>取数阶段业务委托（多播）：GetData 时按挂接顺序串行执行；为 null 视为无需取数直接成功。</summary>
        public Func<OK> GetDataFunc;
        /// <summary>运行阶段业务委托（多播）：工作线程 TaskRun 中按挂接顺序串行执行，任一返回失败节点即 RunFail；为 null 时 Run 直接置 Runned。</summary>
        public Func<OK> RunFunc;
        /// <summary>正常停止收尾委托（多播）：Stop 置状态并 Join 工作线程后执行；为 null 时停止无额外收尾动作。</summary>
        public Func<OK> StopFunc;
        /// <summary>急停收尾委托（多播）：EStop 置状态并 Join 工作线程后执行，语义为紧急中断后的必要处置；为 null 时无额外动作。</summary>
        public Func<OK> EStopFunc;
        /// <summary>暂停业务委托（多播）：运行态调用 Pause 时在置 Pause 状态后执行（例如暂停外部设备）；为 null 时仅切换状态。</summary>
        public Func<OK> PauseFunc;
        /// <summary>继续业务委托（多播）：Pause 状态下再次调用 Pause 恢复时执行（例如恢复外部设备运行）；为 null 时仅切换状态。</summary>
        public Func<OK> ContinueFunc;
        /// <summary>
        /// 节点初始化：由 HTaskStart.Initialization 统一调用，也可单独调用。
        /// 先把状态置 None，再按挂接顺序串行执行 InitializationFunc 多播链；全部成功置 Ready，任一失败回退 None 并立即返回。
        /// </summary>
        /// <returns>OK：未挂接委托或全部委托成功返回成功（状态 Ready）；任一委托返回失败则返回失败（状态回退 None，后续委托不再执行）。</returns>
        public override OK Initialization()
        {
            OK Isok = true;
            RunStepState = RunStepState.None;
            if (InitializationFunc==null)
            {
                // 无初始化业务：直接进入就绪态
                RunStepState = RunStepState.Ready;
                return Isok;
            }
            // 多播委托逐个串行执行：GetInvocationList 保证按 += 挂接顺序调用
            foreach (Func<OK> d in InitializationFunc.GetInvocationList())
            {
                if (!d())
                {
                    RunStepState = RunStepState.None;
                    return false;
                }
            }
            RunStepState = RunStepState.Ready;
            return Isok;
        }
        /// <summary>
        /// 节点取数：Initialization 之后、Run 之前按需调用，串行执行 GetDataFunc 多播链，用于准备运行所需输入数据。
        /// </summary>
        /// <returns>OK：未挂接委托或全部委托成功返回成功；任一委托返回失败立即返回失败（后续委托不再执行）。</returns>
        public override OK GetData()
        {
            OK Isok = true;
            if (GetDataFunc == null)
            {
                return Isok;
            }
            foreach (Func<OK> d in GetDataFunc.GetInvocationList())
            {
                if (!d())
                { return false; }
            }
            return Isok;
        }
        /// <summary>
        /// 非阻塞启动本节点：由 HTaskStart 在全部前置令牌就绪时调用，也可单独调用。
        /// 立即置 Run 状态并复位 RunTimer；RunFunc 为空则同步置 Runned 返回，否则新建后台工作线程执行 TaskRun 后立即返回。
        /// 重复调用前若旧工作线程仍在，会先 Join 等其退出再启动新一轮。
        /// </summary>
        /// <returns>OK：工作线程已启动（或空业务直接完成）返回成功；业务成败不在此返回值中，以 RunStepState 为准。</returns>
        public override OK Run()
        {
            OK Isok = true;
            RunStepState = RunStepState.Run;
            if (RunFunc == null)
            {
                // 无业务函数：同样走一次复位开表-停表，CT约为0
                RunTimer.Reset();
                RunTimer.Start();
                RunTimer.Stop();
                RunStepState = RunStepState.Runned;
                return Isok;
            }
            // 防御性汇合：若上一轮工作线程对象还在（理论上Stop/EStop已置null），先等其退出避免多线程并发执行同一节点
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }
            if (cts==null)
            {
                // 正常情况下构造函数已创建；此处仅为 cts 曾被外部置 null 的扩展场景兜底
                cts = new CancellationTokenSource();
            }
            // 再次运行CT复位重新计时：Reset先把上次CT存入LastMilliseconds，再从0开始
            RunTimer.Reset();
            RunTimer.Start();
            // 异步执行：业务在独立后台Thread上运行，Run启动后立即返回；
            // 运行结束后由 TaskRun 把状态置为 Runned / RunFail，供 HTaskStart 轮询调度
            thread = new Thread(TaskRun) { IsBackground = true, Name = "HTaskData-" + Name };
            thread.Start();
            return Isok;
        }
        /// <summary>
        /// 任务工作线程入口：按挂接顺序串行执行全部 RunFunc 委托，结束时自行置 Runned/RunFail。
        /// 每个委托执行前都经过“停止/急停 → 暂停（自旋等待，恢复后回到检查点重判）→ 取消令牌”三道协作式检查点，
        /// 因此正在执行中的单个委托无法被中断，中断响应粒度为委托之间。
        /// </summary>
        private void TaskRun()
        {
            foreach (Func<OK> d in RunFunc.GetInvocationList())
            {
            StartRun:
                // 检查点1：Stop/EStop 由 Stop()/EStop() 置位（它们随后会 Join 本线程），直接退出且不宣布 Runned
                if (RunStepState == RunStepState.Stop || RunStepState == RunStepState.EStop)
                {
                    return;
                }
                else if (RunStepState == RunStepState.Pause)
                {
                    // 检查点2：暂停期间以20ms粒度自旋，不执行任何业务；恢复后 goto 重新过一遍停止/暂停判定，避免漏判暂停期间到来的停止
                    while (RunStepState == RunStepState.Pause)
                    {
                        Thread.Sleep(20);
                    }
                    goto StartRun;
                }
                // 检查点3：外部取消令牌（当前框架内部不Cancel，预留给业务扩展）
                if (cts.Token.IsCancellationRequested)
                {
                    RunTimer.Stop();
                    RunStepState = RunStepState.RunFail;
                    return;
                }
                RunStepState = RunStepState.Running;
                if (!d())
                {
                    // 业务失败：冻结CT并置RunFail，调度器轮询到后会急停其余任务
                    RunTimer.Stop();
                    RunStepState = RunStepState.RunFail;
                    return;
                }
            }
            // 全部业务函数执行成功即停表冻结本次CT（之后的RunHold保持等待时间不计入CT）
            RunTimer.Stop();
            // 全部业务函数执行成功：保持开关打开时阻塞在完成点（RunHold），不宣布Runned，
            // 等外部关闭开关放行下游；Stop/EStop 立即解除
            if (IsHoldAfterRun)
            {
                RunStepState = RunStepState.RunHold;
                while (IsHoldAfterRun)
                {
                    if (RunStepState == RunStepState.Stop || RunStepState == RunStepState.EStop)
                    {
                        return;
                    }
                    Thread.Sleep(20);
                }
            }
            RunStepState = RunStepState.Runned;
        }
        /// <summary>
        /// 正常停止（阻塞调用线程直到工作线程退出）：先置 Stop 状态——工作线程在下一个检查点自行退出——再 Join 汇合，
        /// 然后复位 RunTimer 并串行执行 StopFunc 收尾委托。与 EStop 的区别仅在语义与执行的委托不同（Stop 允许正常收尾）。
        /// 注意：HTaskStart 会在其调度线程上统一调用本方法，故对调度器外部仍是不阻塞的。
        /// </summary>
        /// <returns>OK：工作线程已退出且 StopFunc 全部成功返回成功；任一 StopFunc 返回失败则返回失败。</returns>
        public override OK Stop()
        {
            // 先置状态：工作线程在检查点看到后自行return（协作式，不能强杀正在执行的委托）
            RunStepState = RunStepState.Stop;
            OK Isok = true;
            JoinThread();
            // 停止即复位CT：中断前的累计CT先存入LastMilliseconds，再清零停表
            RunTimer.Reset();
            RunTimer.Stop();
            if (StopFunc == null)
            {
                return Isok;
            }
            // 线程退出后才执行业务收尾，保证收尾动作与业务体不会并发
            foreach (Func<OK> d in StopFunc.GetInvocationList())
            {
                if (!d())
                { return false; }
            }
            return Isok;
        }
        /// <summary>
        /// 暂停/继续切换（同一方法按当前状态翻转，不 Join 线程、不结束运行）：
        /// 当前为 Pause 时调用＝继续（置 Running 并执行 ContinueFunc 链）；其他状态调用＝暂停（置 Pause 并执行 PauseFunc 链）。
        /// 真正的“停住业务”发生在 TaskRun 的暂停检查点：工作线程自旋等待，不释放下游令牌。
        /// </summary>
        /// <returns>OK：对应 PauseFunc/ContinueFunc 链全部成功（或未挂接）返回成功；任一委托返回失败立即返回失败。</returns>
        public override OK Pause()
        {
            OK Isok = true;
            if (RunStepState == RunStepState.Pause)
            {
                // 继续分支：先翻状态，工作线程的暂停自旋随即退出
                RunStepState = RunStepState.Running;
                
                if (ContinueFunc == null)
                {
                    return Isok;
                }
                foreach (Func<OK> d in ContinueFunc.GetInvocationList())
                {
                    if (!d())
                    { return false; }
                }
                return Isok;
            }
            else
            {
                // 暂停分支：置状态后执行外部设备暂停等动作；工作线程在下一个检查点进入自旋
                RunStepState = RunStepState.Pause;
                if (PauseFunc == null)
                {
                    return Isok;
                }
                foreach (Func<OK> d in PauseFunc.GetInvocationList())
                {
                    if (!d())
                    { return false; }
                }
            }
            return Isok;
        }
        /// <summary>
        /// 紧急停止（阻塞调用线程直到工作线程退出）：流程与 <see cref="Stop"/> 相同（置状态 → Join → 复位CT → 执行收尾委托链），
        /// 区别在于状态为 EStop、执行的是 EStopFunc，语义为不等业务收尾的立即中断；任务失败时调度器也以本方法收敛其余节点。
        /// </summary>
        /// <returns>OK：工作线程已退出且 EStopFunc 全部成功返回成功；任一 EStopFunc 返回失败则返回失败。</returns>
        public override OK EStop()
        {
            // 置急停状态：工作线程（含RunHold保持自旋）在下一个检查点立即退出
            RunStepState = RunStepState.EStop;
            OK Isok = true;
            JoinThread();
            // 急停即复位CT：中断前的累计CT先存入LastMilliseconds，再清零停表
            RunTimer.Reset();
            RunTimer.Stop();
            if (EStopFunc == null)
            {
                return Isok;
            }
            // 线程退出后执行急停处置（如下电、抱闸等），与业务体不并发
            foreach (Func<OK> d in EStopFunc.GetInvocationList())
            {
                if (!d())
                { return false; }
            }
            return Isok;
        }

        /// <summary>
        /// 等待任务工作线程退出并清空引用（Stop/EStop 先置状态，工作线程自行解除阻塞后在此汇合）。
        /// thread 为 null（从未运行或已汇合）时直接返回，不阻塞。
        /// </summary>
        private void JoinThread()
        {
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }
        }

    }
}
