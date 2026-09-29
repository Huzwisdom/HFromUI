using HFromUI.HData;
using HFromUI.HFile;
using HFromUI.HSocket.HTcpClient.ShengGuang;
using HFromUI.HThread;
using HFromUI.HThread.HTask;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HFromUITestA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            // 抓取任意线程上未处理的异常并落盘，便于定位调度/工作线程崩溃
            string crashFile = Path.Combine(HFromUI.HData.HAppData.AppPath, "HTaskStartCrash.log");
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                File.WriteAllText(crashFile, "IsTerminating=" + args.IsTerminating + Environment.NewLine + args.ExceptionObject, Encoding.UTF8);
            };
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += delegate (object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs args)
            {
                File.AppendAllText(crashFile, Environment.NewLine + "UnobservedTask:" + args.Exception, Encoding.UTF8);
            };
            InitializeComponent();
        }

        private void hLabelBase1_Click(object sender, System.EventArgs e)
        {
            ShengGuangData ShengGuangData=new ShengGuangData();
            DeviceStatus s =new DeviceStatus();
            s.MachineStatus = "运行";
            string json = HJsonFile.Serialize(s,false);
            DeviceStatus ss = HJsonFile.Deserialize<DeviceStatus>(json);
        }

        // ================= HTaskStart 调度测试（A-M 共13个 HTaskData） =================
        // 依赖网：A→B→{C,D}→{E,F,G}→{H,I,J,K}；H→L；{I,J,K,L}→M
        // 其中 C/D、E/F/G、H/I/J/K 应并行；H完成即放L（L与I/J/K并行）；M等I、J、K、L全部完成

        /// <summary>单个任务的运行时间记录，用于事后校验顺序与并行关系</summary>
        private sealed class TaskRecord
        {
            public long StartMs;    // 开始时间（相对测试起点，毫秒）
            public long EndMs;      // 结束时间（相对测试起点，毫秒）
            public int ThreadId;    // 实际执行所在托管线程ID
        }

        private readonly object logLock = new object();
        private string logFile;                        // 日志文件路径（写到exe同目录，便于自动核对）

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 窗口打开后自动串行跑两遍测试：DAG调度测试 → 保持/循环开关测试
            StartDemoThread(delegate ()
            {
                RunTaskStartDemo();
                RunHoldLoopDemo();
            });
        }

        private void buttonTaskTest_Click(object sender, EventArgs e)
        {
            StartDemoThread(RunTaskStartDemo);
        }

        private void buttonHoldTest_Click(object sender, EventArgs e)
        {
            StartDemoThread(RunHoldLoopDemo);
        }

        /// <summary>在独立后台线程上运行一段Demo，避免卡住UI</summary>
        private void StartDemoThread(ThreadStart demo)
        {
            buttonTaskTest.Enabled = false;
            buttonHoldTest.Enabled = false;
            textBoxTaskLog.Clear();
            Thread demoThread = new Thread(delegate ()
            {
                try { demo(); }
                finally
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        buttonTaskTest.Enabled = true;
                        buttonHoldTest.Enabled = true;
                    });
                }
            })
            { IsBackground = true, Name = "HTaskStartDemo" };
            demoThread.Start();
        }

        /// <summary>轮询等待条件成立，最多 timeoutMs 毫秒</summary>
        private static bool SpinWait(Func<bool> condition, int timeoutMs)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (condition()) { return true; }
                Thread.Sleep(10);
            }
            return condition();
        }

        /// <summary>线程安全日志：同时输出到窗口和日志文件</summary>
        private void Log(string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss.fff") + " " + message;
            lock (logLock)
            {
                if (logFile != null)
                {
                    try { File.AppendAllText(logFile, line + Environment.NewLine, Encoding.UTF8); }
                    catch { }
                }
            }
            BeginInvoke((MethodInvoker)delegate
            {
                textBoxTaskLog.AppendText(line + Environment.NewLine);
            });
        }

        /// <summary>Demo主体：构建A-M任务、接线、运行并自动校验</summary>
        private void RunTaskStartDemo()
        {
            try
            {
                logFile = Path.Combine(HFromUI.HData.HAppData.AppPath, "HTaskStartDemo.log");
                File.WriteAllText(logFile, string.Empty, Encoding.UTF8);

                Log("===== HTaskStart DAG 调度测试开始 =====");
                Log("依赖网：A→B→{C,D}→{E,F,G}→{H,I,J,K}；H→L；{I,J,K,L}→M");
                Stopwatch stopwatch = Stopwatch.StartNew();

                // 任务名称与计划耗时（毫秒）：同层给不同耗时，验证扇入会等待最慢分支
                (string Name, int Delay)[] defs = new (string Name, int Delay)[]
                {
                    ("A", 100), ("B", 100),
                    ("C", 300), ("D", 200),
                    ("E", 150), ("F", 250), ("G", 100),
                    ("H", 100), ("I", 300), ("J", 250), ("K", 200),
                    ("L", 200), ("M", 100),
                };

                Dictionary<string, HTaskData> tasks = new Dictionary<string, HTaskData>();
                Dictionary<string, TaskRecord> records = new Dictionary<string, TaskRecord>();
                HTaskStart taskStart = new HTaskStart();

                foreach ((string name, int delay) in defs)
                {
                    TaskRecord record = new TaskRecord();
                    HTaskData task = new HTaskData { Name = name };
                    // A为G1组Start开始任务、M为G1组End结束任务，B-L均为Middle中间任务（GroupName留空不设组）
                    if (name == "A")
                    {
                        task.TaskType = HTaskType.Start;
                        task.GroupName = "G1";
                    }
                    else if (name == "M")
                    {
                        task.TaskType = HTaskType.End;
                        task.GroupName = "G1";
                    }
                    task.RunFunc += delegate
                    {
                        record.StartMs = stopwatch.ElapsedMilliseconds;
                        record.ThreadId = Thread.CurrentThread.ManagedThreadId;
                        Log($"任务 {name} 开始  t={record.StartMs,4}ms  线程={record.ThreadId}  计划耗时={delay}ms");
                        Thread.Sleep(delay);
                        record.EndMs = stopwatch.ElapsedMilliseconds;
                        Log($"任务 {name} 完成  t={record.EndMs,4}ms");
                        return true;
                    };
                    tasks[name] = task;
                    records[name] = record;
                    taskStart.HTaskDatas.TryAdd(task.GuidCode, task);
                }

                // 接线：子任务把上游GUID加入 PreconditionGUID；上游完成后释放自己的GUID令牌（加入自己的NextGUID）
                void Link(string child, params string[] parents)
                {
                    foreach (string parent in parents)
                    {
                        tasks[child].PreconditionGUID.Add(tasks[parent].GuidCode);
                        tasks[parent].NextGUID.Add(tasks[parent].GuidCode);
                    }
                }
                Link("B", "A");
                Link("C", "B"); Link("D", "B");
                Link("E", "C", "D"); Link("F", "C", "D"); Link("G", "C", "D");
                Link("H", "E", "F", "G"); Link("I", "E", "F", "G");
                Link("J", "E", "F", "G"); Link("K", "E", "F", "G");
                Link("L", "H");
                Link("M", "I", "J", "K", "L");

                Log($"任务构建完成，共 {taskStart.HTaskDatas.Count} 个，开始 Initialization");
                OK initOK = taskStart.Initialization();
                Log($"Initialization 返回 {(initOK.IsSuccess ? "成功" : "失败")}，开始 GetData");
                OK getDataOK = taskStart.GetData();
                Log($"GetData 返回 {(getDataOK.IsSuccess ? "成功" : "失败")}，开始非阻塞 Run");
                // Run 必须非阻塞：仅启动调度线程就返回
                long runCallMs = stopwatch.ElapsedMilliseconds;
                OK startOK = taskStart.Run();
                runCallMs = stopwatch.ElapsedMilliseconds - runCallMs;
                Log($"Run 已返回（非阻塞），Run调用耗时={runCallMs}ms，调度仍在后台进行");
                // 此时流程远未结束，Wait(1ms) 应超时返回失败，但不影响后台继续调度
                OK waitTimeoutOK = taskStart.Wait(1);
                Log($"Wait(1ms) 提前探测：{(waitTimeoutOK.IsSuccess ? "意外成功" : "按预期超时:" + waitTimeoutOK.Message)}");
                // 真正等待调度全部结束
                OK runOK = taskStart.Wait();
                Log("Wait 已返回，调度线程结束");
                stopwatch.Stop();
                Log($"调度结束  总耗时={stopwatch.ElapsedMilliseconds}ms  Run返回={(runOK.IsSuccess ? "成功" : "失败:" + runOK.Message)}");

                // ================= 自动校验 =================
                List<string> checkResults = new List<string>();
                bool allPass = true;
                const int tol = 60;  // 调度轮询周期20ms，顺序判定留60ms容差
                void Check(string desc, bool pass)
                {
                    checkResults.Add((pass ? "[PASS] " : "[FAIL] ") + desc);
                    if (!pass) { allPass = false; }
                }
                // 两个任务时间区间是否重叠（并行）
                bool Overlap(string x, string y)
                {
                    TaskRecord rx = records[x], ry = records[y];
                    return rx.StartMs < ry.EndMs && ry.StartMs < rx.EndMs;
                }
                // 一组任务是否整体同时并行（最晚开始早于最早结束）
                bool AllParallel(params string[] names)
                {
                    long maxStart = names.Max(n => records[n].StartMs);
                    long minEnd = names.Min(n => records[n].EndMs);
                    return maxStart < minEnd;
                }
                long MaxEnd(params string[] names) => names.Max(n => records[n].EndMs);
                long MinStart(params string[] names) => names.Min(n => records[n].StartMs);

                Check("Initialization 成功", initOK.IsSuccess);
                Check("GetData 成功", getDataOK.IsSuccess);
                Check("Run 非阻塞启动成功", startOK.IsSuccess);
                Check("Run() 调用立即返回不阻塞（<50ms）", runCallMs < 50);
                Check("Wait(1ms) 在调度未结束时按预期超时", !waitTimeoutOK.IsSuccess);
                Check("Wait() 最终返回成功", runOK.IsSuccess);
                Check("调度器最终状态为 Runned", taskStart.RunStepState == RunStepState.Runned);
                foreach ((string name, int _) in defs)
                {
                    Check($"任务 {name} 已执行完成", records[name].EndMs > 0);
                }

                // 顺序约束（上游结束不晚于下游开始）
                Check("A 完成后 B 才开始", records["A"].EndMs <= records["B"].StartMs + tol);
                Check("B 完成后 C 才开始", records["B"].EndMs <= records["C"].StartMs + tol);
                Check("B 完成后 D 才开始", records["B"].EndMs <= records["D"].StartMs + tol);
                Check("C、D 都完成后 E 才开始", MaxEnd("C", "D") <= records["E"].StartMs + tol);
                Check("C、D 都完成后 F 才开始", MaxEnd("C", "D") <= records["F"].StartMs + tol);
                Check("C、D 都完成后 G 才开始", MaxEnd("C", "D") <= records["G"].StartMs + tol);
                Check("E、F、G 都完成后 H/I/J/K 才开始", MaxEnd("E", "F", "G") <= MinStart("H", "I", "J", "K") + tol);
                Check("H 完成后 L 才开始", records["H"].EndMs <= records["L"].StartMs + tol);
                Check("I、J、K、L 都完成后 M 才开始", MaxEnd("I", "J", "K", "L") <= records["M"].StartMs + tol);

                // 并行约束
                Check("C 与 D 并行", Overlap("C", "D"));
                Check("E、F、G 同时并行", AllParallel("E", "F", "G"));
                Check("H、I、J、K 同时并行", AllParallel("H", "I", "J", "K"));
                Check("L 与 I/J/K 至少一个并行（H先完即放L，不必等IJK）",
                    Overlap("L", "I") || Overlap("L", "J") || Overlap("L", "K"));
                Check("并行任务跑在不同线程上（C≠D）", records["C"].ThreadId != records["D"].ThreadId);
                // 关键路径理论约1150ms，全串行约2400ms；小于1900ms证明并行确实生效
                Check("总耗时体现并行（<1900ms，全串行约2400ms）", stopwatch.ElapsedMilliseconds < 1900);

                // CT计时校验（HRunTimer实例化使用，非静态；Run开表、完成停表、再次Run/Stop/EStop复位）
                double startCT = taskStart.RunTimer.TotalMilliseconds;
                Log($"调度器整轮CT={startCT:F0}ms  任务A CT={tasks["A"].RunTimer.TotalMilliseconds:F0}ms  任务C CT={tasks["C"].RunTimer.TotalMilliseconds:F0}ms");
                Check("调度器RunTimer已停表（!IsRun）", !taskStart.RunTimer.IsRun);
                Check("调度器整轮CT>=900ms（关键路径约1150ms，从Run开始到End结束）", startCT >= 900);
                Check("调度器整轮CT不超过外壁钟总耗时+50ms", startCT <= stopwatch.ElapsedMilliseconds + 50);
                Check("任务A本次CT≈100ms（80~220ms）", tasks["A"].RunTimer.TotalMilliseconds >= 80 && tasks["A"].RunTimer.TotalMilliseconds < 220);
                Check("任务C本次CT≈300ms（250~450ms，各任务独立实例计时）", tasks["C"].RunTimer.TotalMilliseconds >= 250 && tasks["C"].RunTimer.TotalMilliseconds < 450);
                Check("任务A首次运行LastMilliseconds=0（尚无上次CT记录）", tasks["A"].RunTimer.LastMilliseconds == 0);

                Log("---------------- 校验结果 ----------------");
                foreach (string line in checkResults)
                {
                    Log(line);
                }
                Log("==========================================");
                Log(allPass ? "================ 全部 PASS ================" : "================ 存在 FAIL ================");
            }
            catch (Exception ex)
            {
                Log("测试异常：" + ex);
            }
        }

        /// <summary>
        /// 开关测试：
        /// 1) HTaskData.IsHoldAfterRun：业务跑完阻塞在RunHold，关闭开关才Runned，EStop可立即解除；
        /// 2) HTaskStart.IsLoopRun：P→Q整轮完成后自动重新Initialization再跑，Q第二次跑完关闭循环，应恰好执行2轮。
        /// </summary>
        private void RunHoldLoopDemo()
        {
            try
            {
                if (logFile == null)
                {
                    logFile = Path.Combine(HFromUI.HData.HAppData.AppPath, "HTaskStartDemo.log");
                    File.WriteAllText(logFile, string.Empty, Encoding.UTF8);
                }
                List<string> checkResults = new List<string>();
                bool allPass = true;
                void Check(string desc, bool pass)
                {
                    checkResults.Add((pass ? "[PASS] " : "[FAIL] ") + desc);
                    if (!pass) { allPass = false; }
                    Log(checkResults[checkResults.Count - 1]);
                }

                Log("===== 开关测试0：CheckTaskGroups 开始/结束成对分组检查 =====");
                // 构造一个仅用于结构检查的临时调度器
                HTaskStart CheckStart(params HTaskData[] list)
                {
                    HTaskStart s = new HTaskStart();
                    foreach (HTaskData t in list)
                    {
                        s.HTaskDatas.TryAdd(t.GuidCode, t);
                    }
                    return s;
                }
                HTaskData MkNode(string name, HTaskType type, string group = null)
                {
                    return new HTaskData { Name = name, TaskType = type, GroupName = group };
                }
                // 合法：1组，Start+End成对，Middle不设组
                OK g0 = CheckStart(MkNode("A", HTaskType.Start, "G1"), MkNode("B", HTaskType.Middle),
                    MkNode("C", HTaskType.Middle), MkNode("M", HTaskType.End, "G1")).CheckTaskGroups();
                Check("合法组：1Start+1End+2个不设组Middle 检查通过", g0.IsSuccess);
                Log("  合法组返回：" + (g0.IsSuccess ? "成功" : g0.Message));
                // 合法：两个独立组G1/G2 + Middle不设组
                OK g0b = CheckStart(MkNode("A", HTaskType.Start, "G1"), MkNode("M", HTaskType.End, "G1"),
                    MkNode("P", HTaskType.Start, "G2"), MkNode("Q", HTaskType.End, "G2"), MkNode("X", HTaskType.Middle)).CheckTaskGroups();
                Check("合法组：G1/G2两组各自成对+1个不设组Middle 检查通过", g0b.IsSuccess);
                // 非法：同组两个Start
                OK g1 = CheckStart(MkNode("A1", HTaskType.Start, "G1"), MkNode("A2", HTaskType.Start, "G1"),
                    MkNode("M", HTaskType.End, "G1")).CheckTaskGroups();
                Check("非法组：同组2个Start 检查失败", !g1.IsSuccess);
                Log("  非法返回信息：" + g1.Message);
                // 非法：孤立Start（Start组名无配对End）+孤立End
                OK g2 = CheckStart(MkNode("A", HTaskType.Start, "G1"), MkNode("M", HTaskType.End, "G2")).CheckTaskGroups();
                Check("非法组：Start的G1无End、End的G2无Start 检查失败", !g2.IsSuccess);
                // 非法：Start未设组名
                OK g3 = CheckStart(MkNode("A", HTaskType.Start, null), MkNode("M", HTaskType.End, "G1")).CheckTaskGroups();
                Check("非法组：Start未设置GroupName 检查失败", !g3.IsSuccess);
                // 非法：没有Start
                OK g4 = CheckStart(MkNode("B", HTaskType.Middle), MkNode("M", HTaskType.End, "G1")).CheckTaskGroups();
                Check("非法组：缺少Start 检查失败", !g4.IsSuccess);
                // 非法：空集合
                OK g5 = new HTaskStart().CheckTaskGroups();
                Check("非法组：空任务集合 检查失败", !g5.IsSuccess);

                Log("===== 开关测试1：HTaskData.IsHoldAfterRun 运行完保持 =====");
                int xRan = 0;
                HTaskData x = new HTaskData { Name = "X", IsHoldAfterRun = true };
                x.RunFunc += delegate
                {
                    xRan++;
                    Log("X 业务函数执行中...");
                    Thread.Sleep(50);
                    Log("X 业务函数执行完，因保持开关打开应阻塞在 RunHold");
                    return true;
                };
                x.Initialization();
                x.Run();
                Thread.Sleep(300);
                Check("X 业务跑完后状态保持为 RunHold（不宣布Runned）", x.RunStepState == RunStepState.RunHold);
                Check("X 保持期间业务函数只执行1次", xRan == 1);
                Log("外部关闭 X 的保持开关");
                x.IsHoldAfterRun = false;
                Check("关闭开关后 X 在1秒内变为 Runned", SpinWait(() => x.RunStepState == RunStepState.Runned, 1000));
                Check("X 释放后业务函数仍只执行1次（未重跑）", xRan == 1);
                Log($"X 本次CT={x.RunTimer.TotalMilliseconds:F0}ms（业务50ms+保持等待300ms，CT只应含业务时间）");
                Check("X 本次CT只含业务50ms、不含300ms保持等待（40~180ms）", x.RunTimer.TotalMilliseconds >= 40 && x.RunTimer.TotalMilliseconds < 180);

                // EStop 立即解除保持
                int yRan = 0;
                HTaskData y = new HTaskData { Name = "Y", IsHoldAfterRun = true };
                y.RunFunc += delegate { yRan++; Thread.Sleep(30); return true; };
                y.Initialization();
                y.Run();
                Thread.Sleep(150);
                Check("Y 业务跑完后进入 RunHold", SpinWait(() => y.RunStepState == RunStepState.RunHold, 1000));
                Stopwatch eSw = Stopwatch.StartNew();
                OK eOK = y.EStop();   // 内部Join等待工作Thread退出，hold循环应在一个20ms周期内响应
                eSw.Stop();
                Check("保持中 EStop 能快速解除（<500ms）并返回成功", eOK.IsSuccess && eSw.ElapsedMilliseconds < 500);
                Check("EStop 后 Y 状态为 EStop", y.RunStepState == RunStepState.EStop);
                Check("Y 业务函数只执行1次", yRan == 1);
                Log($"Y EStop复位后：当前CT={y.RunTimer.TotalMilliseconds:F0}ms  LastMilliseconds={y.RunTimer.LastMilliseconds:F0}ms");
                Check("Y EStop后当前CT复位清零", y.RunTimer.TotalMilliseconds == 0);
                Check("Y EStop复位后LastMilliseconds保留中断前CT（>10ms）", y.RunTimer.LastMilliseconds > 10);

                Log("===== 开关测试2：HTaskStart.IsLoopRun 整轮循环（P→Q） =====");
                HTaskStart loopStart = new HTaskStart { IsLoopRun = true };
                HTaskData p = new HTaskData { Name = "P", TaskType = HTaskType.Start, GroupName = "G2" };
                HTaskData q = new HTaskData { Name = "Q", TaskType = HTaskType.End, GroupName = "G2" };
                int pCount = 0, qCount = 0;
                p.RunFunc += delegate
                {
                    pCount++;
                    Log($"P 第 {pCount} 次执行");
                    Thread.Sleep(30);
                    return true;
                };
                q.RunFunc += delegate
                {
                    qCount++;
                    Log($"Q 第 {qCount} 次执行");
                    Thread.Sleep(30);
                    if (qCount == 2)
                    {
                        // 第二轮Q跑完关闭循环：本轮结束后调度器应正常退出，不再开启第三轮
                        loopStart.IsLoopRun = false;
                        Log("外部已关闭循环开关（Q第2次执行中）");
                    }
                    return true;
                };
                loopStart.HTaskDatas.TryAdd(p.GuidCode, p);
                loopStart.HTaskDatas.TryAdd(q.GuidCode, q);
                q.PreconditionGUID.Add(p.GuidCode);
                p.NextGUID.Add(p.GuidCode);

                loopStart.Initialization();
                loopStart.GetData();
                loopStart.Run();
                OK loopOK = loopStart.Wait(10000);
                Check("循环调度 Wait 成功返回（10秒内收敛）", loopOK.IsSuccess);
                Check("P 恰好执行2轮", pCount == 2);
                Check("Q 恰好执行2轮", qCount == 2);
                Check("循环结束后调度器状态为 Runned", loopStart.RunStepState == RunStepState.Runned);
                Check("循环结束后 P、Q 均为 Runned",
                    p.RunStepState == RunStepState.Runned && q.RunStepState == RunStepState.Runned);
                Log($"循环调度整轮CT={loopStart.RunTimer.TotalMilliseconds:F0}ms（跨2轮连续计时）  P本次CT={p.RunTimer.TotalMilliseconds:F0}ms  P上次CT={p.RunTimer.LastMilliseconds:F0}ms");
                Check("调度器循环CT跨2轮连续计时（>=80ms，未在轮间复位）", loopStart.RunTimer.TotalMilliseconds >= 80);
                Check("P 第2次运行CT已复位重新计时（>=15ms）", p.RunTimer.TotalMilliseconds >= 15);
                Check("P LastMilliseconds保留第1轮CT（>10ms）", p.RunTimer.LastMilliseconds > 10);

                Log("===== 开关测试3：再次Run复位并保留上次CT（Z连跑2次） =====");
                HTaskData z = new HTaskData { Name = "Z" };
                z.RunFunc += delegate { Thread.Sleep(30); return true; };
                z.Initialization();
                z.Run();
                Check("Z 第1次运行到达Runned", SpinWait(() => z.RunStepState == RunStepState.Runned, 1000));
                double ct1 = z.RunTimer.TotalMilliseconds;
                z.Initialization();
                z.Run();
                Check("Z 第2次运行到达Runned", SpinWait(() => z.RunStepState == RunStepState.Runned, 1000));
                double ct2 = z.RunTimer.TotalMilliseconds;
                double lastCt = z.RunTimer.LastMilliseconds;
                Log($"Z 第1次CT={ct1:F0}ms  第2次CT={ct2:F0}ms  LastMilliseconds={lastCt:F0}ms");
                Check("Z 第1次CT>=20ms", ct1 >= 20);
                Check("Z 第2次CT>=20ms（已从0重新计时）", ct2 >= 20);
                Check("Z LastMilliseconds保留第1次CT（20~120ms）", lastCt >= 20 && lastCt < 120);
                Check("Z 第2次CT与保留的上次CT是两份独立记录（差值<60ms）", Math.Abs(ct2 - lastCt) < 60);

                Log("==========================================");
                Log(allPass ? "============ 开关测试全部 PASS ============" : "============ 开关测试存在 FAIL ============");
            }
            catch (Exception ex)
            {
                Log("开关测试异常：" + ex);
            }
        }
    }
}
