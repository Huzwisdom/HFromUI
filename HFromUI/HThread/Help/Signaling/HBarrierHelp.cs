using System;
using System.Threading;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="Barrier"/> 帮助类：多阶段并行屏障（纯托管同步原语，实现 <see cref="IDisposable"/>）。
    /// N 个参与者线程分阶段协作：每个线程完成本阶段工作后调用 <see cref="Barrier.SignalAndWait()"/>
    /// 发出“我到了”的信号并阻塞，等所有注册参与者都到达屏障后，大家在同一时刻被放行进入下一阶段
    /// （像旅行团每到一个景点集合，人齐再出发）。构造时可提供 postPhaseAction 回调：
    /// 每阶段所有人到齐后、下一阶段放行前，由“最后一个到达”的线程单独执行一次，适合做阶段汇总/数据交换。
    ///
    /// 【是否跨进程】否。纯进程内、跨线程的协作结构，不能命名、不能跨 AppDomain/进程。
    ///
    /// 【典型适用场景】
    /// 1) 多线程分阶段算法（如分块矩阵计算、并行迭代数值算法），每阶段必须对齐后才能开始下一阶段；
    /// 2) 流水线阶段对齐，阶段间需要单线程汇总（postPhaseAction）；
    /// 3) 模拟/游戏中的固定步长并行推进（一帧内多个子系统算完再一起进下一帧）。
    /// 与 <see cref="CountdownEvent"/> 区别：CountdownEvent 是一次性汇合（任意线程都可 Signal，可 Reset 再来）；
    /// Barrier 是固定参与者数下的“反复集合点”，同一个参与者每阶段都要再次 SignalAndWait。
    ///
    /// 【使用步骤】
    /// 1) using new Barrier(participantCount)，可传 postPhaseAction；
    /// 2) 启动参与者线程，各自循环：做本阶段工作 → SignalAndWait（建议带毫秒超时/取消令牌）；
    /// 3) 汇合后 <see cref="Barrier.CurrentPhaseNumber"/> 递增，所有线程同时进入下一阶段；
    /// 4) 运行中可 AddParticipant/AddParticipants 或 RemoveParticipant/RemoveParticipants 动态调整；
    /// 5) 所有阶段结束，线程退出后 using 释放 Barrier。
    ///
    /// 【注意事项与坑】
    /// 1) SignalAndWait 的次数必须与参与者数匹配：实际到达数多于 ParticipantCount 抛
    ///    <see cref="InvalidOperationException"/>；少于它则其余人永远等不到——所以每个分支都要用带超时重载，
    ///    但要注意：某个参与者超时后若自行离开，其他参与者在下一阶段仍会等它，可能形成长期阻塞，
    ///    超时返回 false 的参与者不应再继续参与后续阶段（通常应配合取消令牌让所有人一起退出）；
    /// 2) postPhaseAction 抛异常会被包装成 <see cref="BarrierPostPhaseException"/> 抛给“所有”参与者，
    ///    且屏障的阶段号不会推进；回调内务必只做不会失败的汇总工作；
    /// 3) RemoveParticipants(n) 不能让参与者数降为 0，且不能多于当前总数，否则抛
    ///    <see cref="ArgumentOutOfRangeException"/>；阶段号超过 <see cref="short.MaxValue"/> 后再增员会抛
    ///    <see cref="InvalidOperationException"/>；
    /// 4) 不要在 postPhaseAction 中对同一 Barrier 再做 SignalAndWait；
    /// 5) 用完必须 Dispose；释放后仍在等待的参与者行为不可预期。
    ///
    /// 【版本可用性】.NET Framework 4.0 起；.NET Core/.NET 5+ 均可用。
    /// </summary>
    /// <example>
    /// 3 个参与者、3 个阶段，阶段到齐后由一个线程执行汇总回调：
    /// <code>
    /// using (Barrier barrier = new Barrier(3, delegate(Barrier b)
    /// {
    ///     Console.WriteLine("第 " + b.CurrentPhaseNumber + " 阶段全部完成");
    /// }))
    /// {
    ///     for (int p = 0; p &lt; 3; p++)
    ///     {
    ///         int participant = p;
    ///         new Thread((ThreadStart)delegate
    ///         {
    ///             for (int phase = 0; phase &lt; 3; phase++)
    ///             {
    ///                 DoWork(participant, phase);
    ///                 barrier.SignalAndWait(3000);   // 到齐统一放行；超时保底防挂死
    ///             }
    ///         }) { IsBackground = true }.Start();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HBarrierHelp
    {
        /// <summary>
        /// 示例1：多个参与者分阶段并行处理，每阶段全部到齐后由 postPhaseAction 汇总，再一起进入下一阶段。
        /// 每次 SignalAndWait 都带 3000ms 超时，主线程 Join 也带超时，自测不会挂死。
        /// </summary>
        /// <param name="participantCount">参与者线程数，必须 ≥ 1。</param>
        /// <param name="phaseCount">要执行的阶段数，必须 ≥ 1。</param>
        /// <param name="work">每阶段业务逻辑，参数依次为参与者编号、阶段编号；允许为 null。</param>
        /// <returns>所有参与者线程是否在 5000ms 内跑完全部阶段。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="participantCount"/> 或 <paramref name="phaseCount"/> 小于 1。
        /// </exception>
        public static bool PhasedWorkExample(int participantCount, int phaseCount, Action<int, int> work)
        {
            if (participantCount < 1)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("参与者数量不能小于 1"));
            }
            if (phaseCount < 1)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("阶段数量不能小于 1"));
            }

            using (Barrier barrier = new Barrier(participantCount, delegate (Barrier b)
            {
                // 阶段后回调：本阶段所有人到齐后，由最后到达的那个线程执行一次（如汇总、交换数据）
                Console.WriteLine(HTranslation.GetContent("第 ") + b.CurrentPhaseNumber + HTranslation.GetContent(" 阶段全部完成，ParticipantCount=") + b.ParticipantCount);
            }))
            {
                Thread[] threads = new Thread[participantCount];
                for (int i = 0; i < participantCount; i++)
                {
                    int participant = i;
                    threads[i] = new Thread((ThreadStart)delegate
                    {
                        for (int phase = 0; phase < phaseCount; phase++)
                        {
                            if (work != null)
                            {
                                work(participant, phase);
                            }
                            // 报告本阶段完成并等待其他人；带超时防止某个参与者卡死导致所有人死等
                            bool onTime = barrier.SignalAndWait(3000);
                            if (!onTime)
                            {
                                // 超时说明协同已破裂：本参与者退出，不再进入下一阶段，避免连锁死等
                                return;
                            }
                        }
                    }) { IsBackground = true };
                    threads[i].Start();
                }

                foreach (Thread t in threads)
                {
                    if (!t.Join(5000))   // 带超时回收全部参与者线程
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 示例2：无参 <see cref="Barrier.SignalAndWait()"/> 包装。报告到达并无限期等待其他人（生产代码慎用，
        /// 无法防止参与者崩溃导致的永久阻塞）。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="BarrierPostPhaseException">postPhaseAction 回调抛出异常时，所有参与者均收到此包装异常。</exception>
        /// <exception cref="InvalidOperationException">到达人数超过 <see cref="Barrier.ParticipantCount"/>。</exception>
        public static void SignalAndWait(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            barrier.SignalAndWait();
        }

        /// <summary>带毫秒超时的 <see cref="Barrier.SignalAndWait(int)"/>；超时返回 false 而非异常。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="timeoutMs">超时毫秒；0 表示仅探测（所有人都已到齐才返回 true），-1 为无限等待。</param>
        /// <returns>在超时前所有人到齐返回 true；超时返回 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        /// <exception cref="BarrierPostPhaseException">postPhaseAction 抛出异常。</exception>
        public static bool SignalAndWait(Barrier barrier, int timeoutMs)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.SignalAndWait(timeoutMs);
        }

        /// <summary>带 <see cref="TimeSpan"/> 超时的 <see cref="Barrier.SignalAndWait(TimeSpan)"/>。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="timeout">等待时长；<see cref="TimeSpan.Zero"/> 表示仅探测。</param>
        /// <returns>超时前所有人到齐为 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 超出合法范围。</exception>
        public static bool SignalAndWait(Barrier barrier, TimeSpan timeout)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.SignalAndWait(timeout);
        }

        /// <summary>可取消的 <see cref="Barrier.SignalAndWait(CancellationToken)"/>；令牌取消时抛出 <see cref="OperationCanceledException"/>。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="token">取消令牌。</param>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="token"/> 被取消。</exception>
        public static void SignalAndWait(Barrier barrier, CancellationToken token)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            barrier.SignalAndWait(token);
        }

        /// <summary>
        /// 同时带毫秒超时与取消令牌的 <see cref="Barrier.SignalAndWait(int, CancellationToken)"/>，
        /// 是防止“某个参与者卡死拖死所有人”的推荐重载。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="timeoutMs">超时毫秒，自测建议传小值（如 1000~3000）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>超时前所有人到齐为 true；超时为 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="token"/> 被取消。</exception>
        /// <exception cref="BarrierPostPhaseException">postPhaseAction 抛出异常。</exception>
        public static bool SignalAndWait(Barrier barrier, int timeoutMs, CancellationToken token)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.SignalAndWait(timeoutMs, token);
        }

        /// <summary>同时带 <see cref="TimeSpan"/> 超时与取消令牌的 <see cref="Barrier.SignalAndWait(TimeSpan, CancellationToken)"/>。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="timeout">等待时长。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>超时前所有人到齐为 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="token"/> 被取消。</exception>
        public static bool SignalAndWait(Barrier barrier, TimeSpan timeout, CancellationToken token)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.SignalAndWait(timeout, token);
        }

        /// <summary>
        /// 增加一个参与者（<see cref="Barrier.AddParticipant()"/>）。注意参与者总数在“下一阶段”才生效，
        /// 当前正在进行的阶段不受影响。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <returns>新参与者将加入的阶段编号。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">屏障已 Dispose。</exception>
        /// <exception cref="InvalidOperationException">增加后参与者数超出上限，或阶段号已达 <see cref="short.MaxValue"/>。</exception>
        public static long AddOneParticipant(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.AddParticipant();
        }

        /// <summary>
        /// 一次增加多个参与者（<see cref="Barrier.AddParticipants(int)"/>）。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="participantCount">要增加的参与者数，必须 ≥ 1。</param>
        /// <returns>新参与者将加入的阶段编号。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="participantCount"/> 小于 1。</exception>
        public static long AddParticipants(Barrier barrier, int participantCount)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.AddParticipants(participantCount);
        }

        /// <summary>
        /// 减少一个参与者（<see cref="Barrier.RemoveParticipant()"/>）。同样在下一阶段生效；
        /// 若屏障只剩 1 个参与者，移除会抛 <see cref="ArgumentOutOfRangeException"/>（参与者数不能为 0）。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException">移除后参与者数将为 0。</exception>
        public static void RemoveOneParticipant(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            barrier.RemoveParticipant();
        }

        /// <summary>
        /// 一次减少多个参与者（<see cref="Barrier.RemoveParticipants(int)"/>）。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="participantCount">要减少的参与者数，必须 ≥ 1，且不能大于当前总数。</param>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="participantCount"/> 非法或多于现有参与者。</exception>
        public static void RemoveParticipants(Barrier barrier, int participantCount)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            barrier.RemoveParticipants(participantCount);
        }

        /// <summary>
        /// 运行中动态增减参与者数量（对原有便捷方法的健壮版重写）。
        /// </summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <param name="deltaParticipantCount">正数增加对应人数；负数减少对应人数；0 不做任何事。</param>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException">减少人数多于现有参与者。</exception>
        public static void ChangeParticipants(Barrier barrier, int deltaParticipantCount)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            if (deltaParticipantCount > 0)
            {
                barrier.AddParticipants(deltaParticipantCount);
            }
            else if (deltaParticipantCount < 0)
            {
                barrier.RemoveParticipants(-deltaParticipantCount);
            }
        }

        /// <summary>读取当前阶段编号（<see cref="Barrier.CurrentPhaseNumber"/>），首个阶段为 0，每次汇合后 +1。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <returns>当前阶段编号（从 0 开始）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        public static long GetCurrentPhaseNumber(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.CurrentPhaseNumber;
        }

        /// <summary>读取参与者总数（<see cref="Barrier.ParticipantCount"/>）。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <returns>屏障中注册的参与者总数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        public static int GetParticipantCount(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.ParticipantCount;
        }

        /// <summary>读取本阶段尚未到达的参与者人数（<see cref="Barrier.ParticipantsRemaining"/>）；到齐时为 0。</summary>
        /// <param name="barrier">屏障实例，不能为 null。</param>
        /// <returns>本阶段还差几个参与者 SignalAndWait。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="barrier"/> 为 null。</exception>
        public static int GetParticipantsRemaining(Barrier barrier)
        {
            if (barrier == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("屏障不能为空"));
            }
            return barrier.ParticipantsRemaining;
        }

        /// <summary>
        /// 示例3：完全自包含的多阶段并行演示。3 个参与者执行 3 个阶段，postPhaseAction 统计每阶段到达情况，
        /// 并通过 <see cref="Barrier.ParticipantsRemaining"/> 验证“到齐才放行”。全部等待带小超时，可安全自测。
        /// </summary>
        /// <returns>3 个阶段全部正常汇合且属性变化符合预期时返回 true。</returns>
        public static bool MultiPhaseSelfTestExample()
        {
            const int participantCount = 3;
            const int phaseCount = 3;
            int[] perPhaseArrivals = new int[phaseCount];

            using (Barrier barrier = new Barrier(participantCount, delegate (Barrier b)
            {
                // 回调触发时所有人都已到达：ParticipantsRemaining 必为 0
                if (b.ParticipantsRemaining == 0 && b.CurrentPhaseNumber < phaseCount)
                {
                    perPhaseArrivals[b.CurrentPhaseNumber] = participantCount;
                }
            }))
            {
                Thread[] threads = new Thread[participantCount];
                for (int i = 0; i < participantCount; i++)
                {
                    threads[i] = new Thread((ThreadStart)delegate
                    {
                        for (int phase = 0; phase < phaseCount; phase++)
                        {
                            Thread.Sleep(5);                 // 模拟本阶段极短工作
                            if (!barrier.SignalAndWait(2000))
                            {
                                return;                     // 协同破裂则退出
                            }
                        }
                    }) { IsBackground = true };
                    threads[i].Start();
                }

                foreach (Thread t in threads)
                {
                    if (!t.Join(4000))
                    {
                        return false;
                    }
                }

                // 三个阶段各应到齐 3 人，且阶段号已推进到 3
                for (int phase = 0; phase < phaseCount; phase++)
                {
                    if (perPhaseArrivals[phase] != participantCount)
                    {
                        return false;
                    }
                }
                return barrier.CurrentPhaseNumber == phaseCount;
            }
        }
    }
}
