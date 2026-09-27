using System;
using System.Threading;
using ThreadingTimer = System.Threading.Timer;

namespace HFromUI.HThread.Help.Timer
{
    /// <summary>
    /// System.Threading.Timer 帮助类：.NET 最轻量的线程池定时器（单方法回调式）。
    /// 【是什么】System.Threading.Timer 是基于内核可等待定时器（线程池底层维护）的轻量计时器：
    /// new 之后按 dueTime（首次延迟）/ period（重复间隔）把 TimerCallback 委托排队到 CLR 线程池执行，
    /// 不依赖任何 UI 消息循环；构造后不可更改回调与状态对象，只能用 Change 重新调度、用 Dispose 停止。
    /// net48 共 5 个构造重载：Timer(TimerCallback)（默认永不启动）；
    /// Timer(TimerCallback, object, int, int)；Timer(TimerCallback, object, long, long)；
    /// Timer(TimerCallback, object, TimeSpan, TimeSpan)；Timer(TimerCallback, object, uint, uint)（不符合 CLS）。
    /// Change 共 4 个重载：Change(int,int) / Change(long,long) / Change(TimeSpan,TimeSpan) / Change(uint,uint)，
    /// 返回 false 表示定时器已被释放。释放：Dispose()（不等待在途回调）与 Dispose(WaitHandle)（回调排空后信号）。
    /// 【是否跨进程】否。定时器回调排队到本进程的 CLR 线程池；系统时钟分辨率约 15ms（Win7/8 起文档明确，
    /// 与 GetTickCount 同源，不受 timeBeginPeriod 影响），不适合硬实时。
    /// 【典型适用场景】后台心跳、轮询、缓存/会话过期刷新、超时看门狗、一次性延迟任务；
    /// 需要在回调里更新 UI 时自行 Control.Invoke / Dispatcher.Invoke 切回 UI 线程。
    /// 【使用步骤】
    /// 1) 选定 dueTime/period 的数值类型（int 最常用；负数只允许 Timeout.Infinite(-1)；long 版上限 4294967294 毫秒；
    ///    TimeSpan 版只允许零到 InfiniteTimeSpan(-1ms)；uint 版用 Timeout.Infinite 的强转）；
    /// 2) new ThreadingTimer(callback, state, dueTime, period)，period=Infinite 表示只触发一次；
    /// 3) 运行期用 Change 重排（Change 会取消防火墙前已排队但未执行的一次回调，重新计时）；
    /// 4) 停止用 Dispose；必须确保回调已结束再清理共享资源时用 Dispose(WaitHandle) 并 WaitOne。
    /// 【注意事项与坑】
    /// - 重入原理：回调在线程池线程触发，period 到点时上一拍若没执行完，下一拍会在【另一个线程池线程】并发执行，
    ///   即同一个回调可能并发重入；回调体必须保证线程安全，或用本类 CreateNonReentrant / CreateSkipReentrant 防重入；
    /// - 回调并发：线程池可能在任意线程派发回调，连续两次回调不保证在同一线程，不能依赖线程本地状态/TLS；
    /// - dueTime=0 立即排队（不等间隔），period=0 在 int 版会抛 ArgumentOutOfRangeException（不能为 0，只允许正数或 -1）；
    /// - Dispose() 返回后仍可能有一次在途回调正在执行；Dispose(waitHandle) 的信号在所有回调排空后发出，
    ///   但它不保证"释放后不再触发"之外的内存可见性顺序，waitHandle 用完仍需自己 Dispose；
    /// - Change 与回调并发时是安全的（内部加锁），但 Change 返回 false 仅表示定时器已释放；
    /// - 回调内不要做长阻塞/重活，避免占满线程池；异常必须在回调内吞掉，未处理异常在 .NET 2.0+ 会终止进程；
    /// - state 对象通过回调参数原样带回，可用来传递 CancellationTokenSource/停止标志等；
    /// - long/uint 重载存在但不常用；uint 版标记了 CLS 非兼容，跨语言库暴露时注意。
    /// 【版本可用性】net48（mscorlib.dll/System.Threading.Timer.dll）全部 5 构造、4 Change、Dispose/Dispose(WaitHandle)
    /// 均可用；Dispose(WaitHandle) 文档保证：定时器被释放且所有当前回调完成后 waitHandle 收到信号。
    /// .NET Core/.NET 5+ API 形状相同；本类全部代码 net48 可编译。
    /// </summary>
    /// <example>
    /// 周期任务与安全停止：
    /// <code>
    /// int running = 0;
    /// Timer t = new Timer(delegate(object state)
    /// {
    ///     if (Interlocked.Exchange(ref running, 1) == 1) return;  // 防重入
    ///     try { DoWork(); }
    ///     finally { Interlocked.Exchange(ref running, 0); }
    /// }, null, 1000, 500);                              // 1秒后开始，每500ms一拍
    /// // ……停止：
    /// ManualResetEvent done = new ManualResetEvent(false);
    /// t.Dispose(done);                                   // 等在途回调排空
    /// done.WaitOne(5000);
    /// done.Dispose();
    /// </code>
    /// </example>
    public static class HThreadingTimerHelp
    {
        /// <summary>
        /// 示例1：用 int 毫秒版构造创建周期定时器（最常用重载）：dueMs 后开始，每 periodMs 毫秒回调一次。
        /// 只创建并返回，不等待任何一次回调，调用方负责 Dispose。
        /// </summary>
        /// <param name="tick">每拍执行的回调（在 CLR 线程池线程触发，可能并发重入，需自行保证线程安全）</param>
        /// <param name="dueMs">首次触发延迟（毫秒）；0=立即排队，Timeout.Infinite=不启动</param>
        /// <param name="periodMs">重复间隔（毫秒）；必须 &gt;0，Timeout.Infinite=只触发一次</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null（TimerCallback 不能为 null）</exception>
        /// <exception cref="ArgumentOutOfRangeException">dueMs/periodMs 为负数且不等于 Timeout.Infinite，或 periodMs 为 0</exception>
        public static ThreadingTimer CreatePeriodic(Action tick, int dueMs, int periodMs)
        {
            // int/int 构造重载：state 传 null，回调内转调无参 Action
            return new ThreadingTimer(delegate (object state)
            {
                tick();                                    // 回调在线程池线程执行
            }, null, dueMs, periodMs);
        }

        /// <summary>
        /// 示例2：兼容旧调用的周期定时器：1秒后开始、每500ms一拍。
        /// </summary>
        /// <param name="tick">每拍执行的回调</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        public static ThreadingTimer CreatePeriodic(Action tick)
        {
            // 固定参数的便捷重载，内部仍走 int 构造
            return CreatePeriodic(tick, 1000, 500);
        }

        /// <summary>
        /// 示例3：创建带状态对象的周期定时器（int 版），state 原样传给每一次回调。
        /// </summary>
        /// <param name="tick">每拍回调，入参即构造时传入的 state</param>
        /// <param name="state">自定义状态对象（可为 null），常用于传停止标志/业务上下文</param>
        /// <param name="dueMs">首次触发延迟（毫秒）；0=立即，Timeout.Infinite=不启动</param>
        /// <param name="periodMs">重复间隔（毫秒）；&gt;0，Timeout.Infinite=一次性</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">时间参数非法（负数且非 Infinite，或 periodMs 为 0）</exception>
        public static ThreadingTimer CreatePeriodic(Action<object> tick, object state, int dueMs, int periodMs)
        {
            // state 不做拷贝，引用同一对象；回调内若改它需自行同步
            return new ThreadingTimer(delegate (object s)
            {
                tick(s);
            }, state, dueMs, periodMs);
        }

        /// <summary>
        /// 示例4：用 long 毫秒版构造创建周期定时器；取值范围比 int 大，上限 4294967294 毫秒。
        /// </summary>
        /// <param name="tick">每拍回调，入参为 state</param>
        /// <param name="state">自定义状态对象（可为 null）</param>
        /// <param name="dueMs">首次延迟（毫秒）；0=立即，-1(Timeout.Infinite)=不启动，不可为其他负数</param>
        /// <param name="periodMs">重复间隔（毫秒）；&gt;0，-1=一次性，最大 4294967294</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">参数为非法负数、为 0 周期或超过 4294967294</exception>
        public static ThreadingTimer CreatePeriodicLong(Action<object> tick, object state, long dueMs, long periodMs)
        {
            // long/long 构造重载：适合超长间隔（数十天）场景
            return new ThreadingTimer(delegate (object s)
            {
                tick(s);
            }, state, dueMs, periodMs);
        }

        /// <summary>
        /// 示例5：用 TimeSpan 版构造创建周期定时器；可读性最好。
        /// </summary>
        /// <param name="tick">每拍回调，入参为 state</param>
        /// <param name="state">自定义状态对象（可为 null）</param>
        /// <param name="dueTime">首次延迟；TimeSpan.Zero=立即，Timeout.InfiniteTimeSpan=不启动，不可为负的其他值</param>
        /// <param name="period">重复间隔；须为正，Timeout.InfiniteTimeSpan=一次性，最大等价 4294967294 毫秒</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">时间为负（且非 InfiniteTimeSpan）或超出毫秒上限</exception>
        public static ThreadingTimer CreatePeriodicTimeSpan(Action<object> tick, object state, TimeSpan dueTime, TimeSpan period)
        {
            // TimeSpan/TimeSpan 构造重载：内部按 TotalMilliseconds 校验
            return new ThreadingTimer(delegate (object s)
            {
                tick(s);
            }, state, dueTime, period);
        }

        /// <summary>
        /// 示例6：用 uint 毫秒版构造创建周期定时器（CLS 非兼容重载，C# 可用，部分其他语言不可用）。
        /// </summary>
        /// <param name="tick">每拍回调，入参为 state</param>
        /// <param name="state">自定义状态对象（可为 null）</param>
        /// <param name="dueMs">首次延迟（毫秒）；0=立即，0xFFFFFFFF 等价 Infinite 不启动</param>
        /// <param name="periodMs">重复间隔（毫秒）；&gt;0，0xFFFFFFFF=一次性</param>
        /// <returns>已开始计时的 System.Threading.Timer 实例</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        public static ThreadingTimer CreatePeriodicUInt(Action<object> tick, object state, uint dueMs, uint periodMs)
        {
            // uint/uint 构造重载：参数表为 UInt32，因此不会出现负数异常
            return new ThreadingTimer(delegate (object s)
            {
                tick(s);
            }, state, dueMs, periodMs);
        }

        /// <summary>
        /// 示例7：先用 Timer(TimerCallback) 重载创建一个【未启动】的定时器（dueTime/period 默认均 Infinite），
        /// 稍后必须调用 Change 才会真正计时。
        /// </summary>
        /// <param name="callback">定时器回调</param>
        /// <returns>构造完成但尚未启动的 System.Threading.Timer</returns>
        /// <exception cref="ArgumentNullException">callback 为 null</exception>
        public static ThreadingTimer CreateUnstarted(TimerCallback callback)
        {
            // 单参数构造：等价 new Timer(callback, null, Timeout.Infinite, Timeout.Infinite)
            return new ThreadingTimer(callback);
        }

        /// <summary>
        /// 示例8：一次性延迟执行（period 传 Infinite，触发后不再重复）。
        /// </summary>
        /// <param name="dueMs">延迟毫秒数；0=立即排队</param>
        /// <param name="work">触发时执行一次的工作</param>
        /// <returns>已开始计时的一次性 System.Threading.Timer</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">dueMs 为负数且不等于 Timeout.Infinite</exception>
        public static ThreadingTimer OneShot(int dueMs, Action work)
        {
            // period = Infinite：只在 dueMs 后触发一次
            return new ThreadingTimer(delegate (object state)
            {
                work();
            }, null, dueMs, Timeout.Infinite);
        }

        /// <summary>
        /// 示例9：Change(int,int) 运行中动态重排首次延迟与周期。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="dueMs">新的首次延迟（毫秒）；0=立即，Infinite=暂停</param>
        /// <param name="periodMs">新的周期（毫秒）；&gt;0，Infinite=改成一次性</param>
        /// <returns>true=更新成功；false=定时器已释放</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">时间参数非法</exception>
        public static bool Reschedule(ThreadingTimer timer, int dueMs, int periodMs)
        {
            // 已排队但未执行的回调会被本次 Change 取消并按新时间重新计时
            return timer.Change(dueMs, periodMs);
        }

        /// <summary>
        /// 示例10：Change(long,long) 重排（超长间隔）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="dueMs">新的首次延迟（毫秒）</param>
        /// <param name="periodMs">新的周期（毫秒）</param>
        /// <returns>true=更新成功；false=定时器已释放</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">参数非法（负且非 -1、为 0 周期或超上限）</exception>
        public static bool RescheduleLong(ThreadingTimer timer, long dueMs, long periodMs)
        {
            // long 版 Change
            return timer.Change(dueMs, periodMs);
        }

        /// <summary>
        /// 示例11：Change(TimeSpan,TimeSpan) 重排。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="dueTime">新的首次延迟；Zero=立即，InfiniteTimeSpan=暂停</param>
        /// <param name="period">新的周期；正值或 InfiniteTimeSpan</param>
        /// <returns>true=更新成功；false=定时器已释放</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">时间为负（且非 InfiniteTimeSpan）或超上限</exception>
        public static bool RescheduleTimeSpan(ThreadingTimer timer, TimeSpan dueTime, TimeSpan period)
        {
            // TimeSpan 版 Change
            return timer.Change(dueTime, period);
        }

        /// <summary>
        /// 示例12：Change(uint,uint) 重排（CLS 非兼容）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="dueMs">新的首次延迟（毫秒）</param>
        /// <param name="periodMs">新的周期（毫秒）；0xFFFFFFFF=Infinite</param>
        /// <returns>true=更新成功；false=定时器已释放</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static bool RescheduleUInt(ThreadingTimer timer, uint dueMs, uint periodMs)
        {
            // uint 版 Change
            return timer.Change(dueMs, periodMs);
        }

        /// <summary>
        /// 示例13：暂停定时器（Change 两个 Infinite），之后可用任意 Change 重载恢复；比 Dispose 后重建更省。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <returns>true=暂停成功；false=定时器已释放</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static bool Pause(ThreadingTimer timer)
        {
            // 两个 Infinite：不再排队新回调
            return timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 示例14：防重入方案A（暂停-恢复式）：回调开始先 Change(Infinite,Infinite) 停表，
        /// 工作完成后在 finally 里 Change 恢复，从机制上保证同一时刻只有一个回调在跑。
        /// 代价：工作耗时会"吃掉"间隔，恢复后重新等满一拍；恢复间隔固定 500ms。
        /// </summary>
        /// <param name="tick">每拍工作（不会并发重入）</param>
        /// <returns>已开始计时的 System.Threading.Timer</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        public static ThreadingTimer CreateNonReentrant(Action tick)
        {
            // 回调闭包捕获 timer 自身；赋值发生在构造之前，回调触发时它必已非 null
            ThreadingTimer timer = null;
            timer = new ThreadingTimer(delegate (object state)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);  // 先停：阻止下一拍排队
                try
                {
                    tick();                                         // 串行执行实际工作
                }
                finally
                {
                    timer.Change(500, 500);                         // 无论成功异常都恢复计时
                }
            }, null, 500, 500);
            return timer;
        }

        /// <summary>
        /// 示例15：防重入方案B（跳过式）：用 Interlocked 当门闩，上一拍没跑完时本拍直接跳过，
        /// 不动定时器节奏；适合"宁可丢拍不可堆积"的采样/刷新任务。
        /// </summary>
        /// <param name="tick">每拍工作；发生重入时新来的一拍被跳过</param>
        /// <param name="dueMs">首次延迟（毫秒）</param>
        /// <param name="periodMs">重复间隔（毫秒）</param>
        /// <returns>已开始计时的 System.Threading.Timer</returns>
        /// <exception cref="ArgumentNullException">tick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">时间参数非法</exception>
        public static ThreadingTimer CreateSkipReentrant(Action tick, int dueMs, int periodMs)
        {
            int gate = 0;                                           // 0=空闲 1=有回调在执行
            return new ThreadingTimer(delegate (object state)
            {
                // 原子置 1：若原值已是 1，说明上一拍未结束，跳过本拍
                if (Interlocked.Exchange(ref gate, 1) == 1)
                {
                    return;
                }
                try
                {
                    tick();
                }
                finally
                {
                    Interlocked.Exchange(ref gate, 0);             // 释放门闩
                }
            }, null, dueMs, periodMs);
        }

        /// <summary>
        /// 示例16：Dispose() 立即释放定时器；不等待在途回调，返回后可能仍有一次回调正在另一线程执行。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static void Stop(ThreadingTimer timer)
        {
            // 普通释放：不提供排空保证
            timer.Dispose();
        }

        /// <summary>
        /// 示例17：Dispose(WaitHandle) 停止并等待在途回调排空（无限等待版本，仅在确认回调会很快结束时使用）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static void StopSafely(ThreadingTimer timer)
        {
            // 回调全部结束后 done 才会被 Set
            ManualResetEvent done = new ManualResetEvent(false);
            timer.Dispose(done);
            done.WaitOne();                                        // 生产代码建议用带超时重载
            done.Dispose();                                        // WaitHandle 本身也要释放
        }

        /// <summary>
        /// 示例18：Dispose(WaitHandle) 停止并带超时等待回调排空（自测/库代码推荐）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="timeoutMs">等待超时（毫秒）；-1=无限等待，0=不等待只探测</param>
        /// <returns>true=回调已全部排空；false=等待超时（定时器仍会在未来排空，waitHandle 仍需释放）</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeoutMs 为非法负数</exception>
        public static bool StopSafely(ThreadingTimer timer, int timeoutMs)
        {
            ManualResetEvent done = new ManualResetEvent(false);
            timer.Dispose(done);                                   // 排队一个排空完成信号
            bool drained;
            try
            {
                drained = done.WaitOne(timeoutMs);                 // 带超时等待，避免被死循环回调永久卡住
            }
            finally
            {
                done.Dispose();
            }
            return drained;
        }

        /// <summary>
        /// 示例19：生命周期自测演示：构造未启动定时器 → Change 立即触发 → Dispose(waitHandle) 排空，
        /// 全流程不等待定时器真正打点、不依赖消息循环，可在任意线程安全调用。
        /// </summary>
        /// <returns>描述本次 Change/Dispose 返回值的字符串（Change 为 true；Dispose(waitHandle) 首次调用为 true）</returns>
        public static string QuickLifecycleDemo()
        {
            int ticks = 0;
            // 构造后不启动：dueTime/period 均 Infinite
            ThreadingTimer timer = new ThreadingTimer(
                delegate (object state) { Interlocked.Increment(ref ticks); },
                null, Timeout.Infinite, Timeout.Infinite);
            bool changed = timer.Change(0, Timeout.Infinite);      // 改为"立即触发一次"，可能在另一线程池线程执行
            ManualResetEvent done = new ManualResetEvent(false);
            bool disposeQueued = timer.Dispose(done);              // true=已登记排空信号；false=定时器此前已释放
            done.WaitOne(1000);                                    // 等可能存在的在途回调结束（通常瞬时返回）
            done.Dispose();
            // 不校验 ticks：0 或 1 都合法，重点是全程不阻塞、不依赖消息泵
            return string.Format("Change={0},Dispose(waitHandle)={1},ticks={2}", changed, disposeQueued, ticks);
        }

        /// <summary>
        /// 定时器回调状态袋示例：演示如何把多个共享数据通过 state 参数带入每一次回调。
        /// </summary>
        public sealed class TimerStateBag
        {
            private int hitCount;

            /// <summary>
            /// 初始化状态袋。
            /// </summary>
            /// <param name="name">业务名称（仅用于辨识）</param>
            public TimerStateBag(string name)
            {
                Name = name;
            }

            /// <summary>
            /// 业务名称。
            /// </summary>
            /// <returns>构造时传入的名称</returns>
            public string Name { get; private set; }

            /// <summary>
            /// 累计命中次数（多线程回调可能并发访问，用 Increment 原子自增）。
            /// </summary>
            /// <returns>当前累计次数</returns>
            public int HitCount
            {
                get { return Interlocked.CompareExchange(ref hitCount, 0, 0); }
            }

            /// <summary>
            /// 原子递增命中次数。
            /// </summary>
            /// <returns>递增后的新值</returns>
            public int IncrementHit()
            {
                // 回调可能在不同线程池线程触发，必须用原子操作
                return Interlocked.Increment(ref hitCount);
            }
        }
    }
}
