using System;
using System.Threading;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="ManualResetEventSlim"/> 完整帮助：纯托管实现的“手动门闩”，
    /// 采用混合等待策略——短等待时先在用户态自旋若干次（spinCount），仍无信号再退化为内核等待，
    /// 实现 <see cref="IDisposable"/>。核心成员：
    /// Set() 开门并【持续保持打开】（一次广播放行全部等待者，之后到达的线程同样不阻塞）、
    /// Reset() 手动关门、IsSet 只读查询门是否开着、Wait()/Wait(int)/Wait(TimeSpan)/
    /// Wait(CancellationToken) 阻塞等待、SpinCount 读取构造时设定的自旋次数。
    /// 构造形式：new ManualResetEventSlim(initialState) 与
    /// new ManualResetEventSlim(initialState, spinCount)（spinCount 合法范围 0 ~ <see cref="short.MaxValue"/>）。
    /// WaitHandle 属性按需【延迟创建】：只有要和其他内核句柄一起 WaitAny/WaitAll，或走 APM
    /// 老接口（Begin/End 模式）时才访问；一旦访问就真的分配一个内核事件句柄，失去轻量优势；
    /// 该句柄生命周期归 Slim 所有，Dispose Slim 时随之释放，禁止单独 Close/Dispose 它。
    ///
    /// 【是否跨进程】否。<see cref="ManualResetEventSlim"/> 不能命名、不能跨进程/跨 AppDomain，
    /// 不访问 WaitHandle 时也不能放进 <see cref="WaitHandle"/> 数组。需要跨进程请用
    /// <c>new EventWaitHandle(initialState, EventResetMode.ManualReset, name)</c>，详见
    /// HManualResetEventHelp。选型口诀：短等待、同机进程内优先 Slim；需要跨进程或参与
    /// WaitAny/WaitAll 组合时用内核版 <see cref="EventWaitHandle"/>。
    ///
    /// 【典型适用场景】
    /// 1) 发令枪：N 个工作线程先 Wait 排队，主线程一次 Set 全部同时开跑（见 OneShotGateAllWaitersPass）；
    /// 2) 一次性就绪门闩：初始化完成前所有访问者等待，Set 后永久放行；
    /// 3) 停止广播：配合 IsSet 与带超时的 Wait 做可及时响应退出的工作循环；
    /// 4) 极短条件等待：等待时间通常很短时，自旋路径可避免昂贵的内核上下文切换。
    ///
    /// 【使用步骤】
    /// 1) using (ManualResetEventSlim gate = new ManualResetEventSlim(false)) 创建（false=初始关门）；
    /// 2) 等待方 gate.Wait(超时毫秒) 或 gate.Wait(token)，生产代码不要用无超时 Wait() 无兜底等待；
    /// 3) 发令方 gate.Set()；需要做第二轮发令时必须先 gate.Reset()；
    /// 4) 只读探测用 gate.IsSet，不要用 Wait(0) 代替属性语义（虽然都能探测）；
    /// 5) 仅在必须与其他内核句柄组合时才访问 gate.WaitHandle（访问即分配）；
    /// 6) using 结束自动 Dispose；手动 new 的不要忘记 Dispose，且先停工作线程再释放。
    ///
    /// 【注意事项与坑】
    /// 1) “手动”指门不会自动关闭：Set 之后不 Reset，之后到达的等待者全部直接通过——做第二轮
    ///    发令枪却忘记 Reset 是最常见 bug；
    /// 2) spinCount 超出 0 ~ <see cref="short.MaxValue"/> 抛 <see cref="ArgumentOutOfRangeException"/>；
    ///    自旋不是越大越好：等待普遍较长时大自旋数只会空耗 CPU；
    /// 3) 访问 WaitHandle 即分配内核句柄，Dispose 前不会自动回收；句柄归 Slim 所有；
    /// 4) Slim 的 Wait 原生支持 <see cref="CancellationToken"/>，内核 ManualResetEvent 的 WaitOne 不支持；
    /// 5) 没有原生 WaitAsync：需要异步等待时用 Task.Run 包装阻塞 Wait（见 HManualResetEventHelp.WaitAsync）；
    /// 6) Dispose 后仍在 Wait 的线程行为不可预期；等待超时返回 false 表示“这段时间内门没开”，不是错误。
    ///
    /// 【版本可用性】<see cref="ManualResetEventSlim"/> 自 .NET Framework 4.0；
    /// .NET Core/.NET 5+ 均内置。本工程为 .NET Framework 4.8 / C# 7.3，可直接使用。
    /// </summary>
    /// <example>
    /// 一次性发令枪：两个等待者先排队，主线程 Set 后全部放行：
    /// <code>
    /// using (ManualResetEventSlim gate = new ManualResetEventSlim(false, 10))
    /// {
    ///     bool[] passed = new bool[2];
    ///     for (int i = 0; i &lt; 2; i++)
    ///     {
    ///         int index = i;
    ///         new Thread((ThreadStart)delegate
    ///         {
    ///             passed[index] = gate.Wait(2000);   // 门关着：先自旋后混合等待
    ///         }) { IsBackground = true }.Start();
    ///     }
    ///     Thread.Sleep(30);
    ///     gate.Set();                                // 开门：所有等待者一次性通过
    /// }
    /// </code>
    /// </example>
    public static class HManualResetEventSlimHelp
    {
        /// <summary>
        /// 示例1（自包含）：一次性门闩广播。new ManualResetEventSlim(false) 后启动 2 个后台子线程，
        /// 各自带 2000ms 超时 Wait 并把“是否通过”写入 bool 容器；主线程 Sleep(30) 确认两人已排队后
        /// 一次 Set，最后 Join(2000) 回收。验证“手动门一次 Set 放行全部等待者”的广播语义。
        /// </summary>
        /// <returns>两个等待者都在超时内通过（Wait 均返回 true）返回 true；任一未通过或线程未按时结束返回 false。</returns>
        public static bool OneShotGateAllWaitersPass()
        {
            using (ManualResetEventSlim gate = new ManualResetEventSlim(false))
            {
                bool[] passed = new bool[2];             // 容器：记录每个等待者是否通过
                Thread[] waiters = new Thread[2];

                for (int i = 0; i < 2; i++)
                {
                    int index = i;
                    waiters[i] = new Thread((ThreadStart)delegate
                    {
                        // 门关着：在此排队（先自旋，自旋耗尽后混合等待）；2000ms 超时保底
                        passed[index] = gate.Wait(2000);
                    }) { IsBackground = true };
                    waiters[i].Start();
                }

                Thread.Sleep(30);                        // 等两个子线程都进入等待
                gate.Set();                              // 开门：门持续保持打开，两个等待者同时放行

                for (int i = 0; i < waiters.Length; i++)
                {
                    if (!waiters[i].Join(2000))          // 带超时回收，杜绝挂死
                    {
                        return false;
                    }
                }

                // 手动门广播：一次 Set 后两个等待者的 Wait 都应返回 true
                return passed[0] && passed[1];
            }
        }

        /// <summary>
        /// 示例2（自包含）：门未 Set 时，Wait(int) 超时返回 false 而非抛异常。
        /// 全程不调用 Set，等待 50ms 必然超时，用结果验证超时语义。
        /// </summary>
        /// <returns>Wait(50) 返回 false（且 IsSet 始终为 false）时返回 true。</returns>
        public static bool WaitTimeoutReturnsFalse()
        {
            using (ManualResetEventSlim gate = new ManualResetEventSlim(false))
            {
                bool isSetBefore = gate.IsSet;           // 初始关门：false
                bool signaled = gate.Wait(50);           // 从未 Set：50ms 超时返回 false
                bool isSetAfter = gate.IsSet;            // 超时不会改变门状态：仍为 false
                return !signaled && !isSetBefore && !isSetAfter;
            }
        }

        /// <summary>
        /// 示例3（自包含）：WaitHandle 属性“访问即分配”的延迟创建行为。
        /// new 出来后只要不访问 WaitHandle，背后不占用任何内核句柄；一旦读取该属性，
        /// 立即创建一个内核事件（其 SafeWaitHandle 非 null）。随后用
        /// WaitHandle.WaitAny(new[]{ gate.WaitHandle }, 0) 以 0 超时做一次非阻塞演示
        /// （门未开时立即返回 <see cref="WaitHandle.WaitTimeout"/>，不会挂住），最后 Dispose。
        /// </summary>
        /// <returns>访问 WaitHandle 后其 SafeWaitHandle 非 null，且 WaitAny 零超时非阻塞返回超时索引时为 true。</returns>
        public static bool WaitHandleOnlyAllocatedOnAccess()
        {
            using (ManualResetEventSlim gate = new ManualResetEventSlim(false))
            {
                // 到此为止都是纯托管对象：没有访问 WaitHandle，就没有内核句柄分配
                WaitHandle kernelHandle = gate.WaitHandle;   // 访问的瞬间：惰性创建内核事件句柄

                bool allocated = kernelHandle.SafeWaitHandle != null;   // 已持有真实内核句柄

                // 放进 WaitHandle 数组参与 WaitAny：超时 0 = 纯探测，门未开立即返回 WaitTimeout(258)，不阻塞
                int waitIndex = WaitHandle.WaitAny(new WaitHandle[] { kernelHandle }, 0);

                // using 结束 Dispose gate 时，这个内核句柄由 gate 一并释放（不要自己 Dispose 它）
                return allocated && waitIndex == WaitHandle.WaitTimeout;
            }
        }

        /// <summary>
        /// 按指定初始状态与自旋次数创建 Slim（构造函数 new ManualResetEventSlim(initialState, spinCount) 的包装）。
        /// </summary>
        /// <param name="initialState">true 创建后即为开门状态；false 初始关门。</param>
        /// <param name="spinCount">
        /// 退化为内核等待前的自旋次数，合法范围 0 ~ <see cref="short.MaxValue"/>；短等待多可适当调大。
        /// </param>
        /// <returns>创建好的 Slim 事件；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="spinCount"/> 小于 0 或大于 <see cref="short.MaxValue"/>。
        /// </exception>
        public static ManualResetEventSlim CreateWithSpinCount(bool initialState, int spinCount)
        {
            return new ManualResetEventSlim(initialState, spinCount);
        }

        /// <summary>开门（<see cref="ManualResetEventSlim.Set"/>）：门持续保持打开，全部当前与后续等待者直接通过。</summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static void SetGate(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            gate.Set();                // 广播开门，信号不被消耗
        }

        /// <summary>
        /// 关门（<see cref="ManualResetEventSlim.Reset"/>）：Reset 之后到达的等待线程重新阻塞；
        /// 已经通过的线程不受影响。做“第二轮发令枪”前必须调用。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static void ResetGate(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            gate.Reset();
        }

        /// <summary>读取门状态（<see cref="ManualResetEventSlim.IsSet"/>）：是否开着；只读，不消耗信号。</summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <returns>门处于打开状态返回 true。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        public static bool IsGateSet(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.IsSet;
        }

        /// <summary>
        /// 带毫秒超时的等待（<see cref="ManualResetEventSlim.Wait(int)"/>）。
        /// 内部先按 SpinCount 自旋，再混合等待，短等待开销远小于内核句柄。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <param name="timeoutMs">超时毫秒；0 为立即探测，-1 为无限等待（生产慎用）。</param>
        /// <returns>超时前门被打开为 true；超时仍关闭为 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static bool WaitMs(ManualResetEventSlim gate, int timeoutMs)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.Wait(timeoutMs);
        }

        /// <summary>
        /// 可取消的等待（<see cref="ManualResetEventSlim.Wait(CancellationToken)"/>）：
        /// Slim 原生支持取消，这是它相对内核 <see cref="ManualResetEvent"/> 的优势之一。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <param name="token">取消令牌；取消时等待抛出 <see cref="OperationCanceledException"/>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="token"/> 被取消。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static void WaitWithToken(ManualResetEventSlim gate, CancellationToken token)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            gate.Wait(token);
        }

        /// <summary>读取构造时设定的自旋次数（<see cref="ManualResetEventSlim.SpinCount"/>）。</summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <returns>自旋等待次数（0 ~ <see cref="short.MaxValue"/>）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        public static int GetSpinCount(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.SpinCount;
        }
    }
}
