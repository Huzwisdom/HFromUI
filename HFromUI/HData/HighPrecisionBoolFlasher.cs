using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HData
{
    using HFromUI.HLangage;
    public class HighPrecisionBoolFlasher : IDisposable
    {
        private readonly long _halfPeriodTicks;   // 半个周期的硬件tick数（因为一周期翻转两次）
        private Thread _worker;
        private CancellationTokenSource _cts;
        /// <summary>_stopwatch 字段。</summary>
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private long _nextToggleTicks;            // 下一次翻转的绝对时间点
        private volatile bool _currentValue;      // 当前布尔值（volatile 保证跨线程可见性）
        private bool _disposed;
        /// <summary>_lock 字段。</summary>
        private readonly object _lock = new object();

        // 高精度计时器频率（通常 10MHz = 0.1μs 分辨率）
        private static readonly double Frequency = Stopwatch.Frequency;
        /// <summary>TicksPerMicrosecond 字段。</summary>
        private static readonly double TicksPerMicrosecond = Frequency / 1_000_000.0;

        /// <summary>
        /// 当布尔值翻转时触发（在专用高优先级线程中调用，回调必须极轻量）
        /// </summary>
        public event Action<bool> OnToggled;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="periodMicroseconds">翻转周期（微秒），即两次翻转之间的间隔</param>
        /// <param name="initialValue">初始值</param>
        public HighPrecisionBoolFlasher(double periodMicroseconds, bool initialValue = false)
        {
            if (periodMicroseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(periodMicroseconds), HTranslation.GetContent("周期必须大于0微秒"));

            _halfPeriodTicks = (long)(periodMicroseconds * TicksPerMicrosecond);
            _currentValue = initialValue;
        }

        /// <summary>
        /// 获取当前布尔值（线程安全读取）
        /// </summary>
        public bool CurrentValue => _currentValue;

        /// <summary>
        /// 启动闪烁（若已启动则先停止再启动）
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(HighPrecisionBoolFlasher));
                StopInternal();

                _cts = new CancellationTokenSource();
                // 初始化第一次翻转的绝对时间点：从当前时间开始，经过半个周期
                // 注意：必须使用 ElapsedTicks（Stopwatch 原始节拍），不能用 Elapsed.Ticks（100ns 的 DateTime 单位），
                // 否则在 Stopwatch.Frequency != 10MHz 的机器上周期会完全错误
                _nextToggleTicks = _stopwatch.ElapsedTicks + _halfPeriodTicks;

                _worker = new Thread(PrecisionLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest,
                    Name = "BoolFlasher"
                };
                _worker.Start();
            }
        }

        /// <summary>
        /// 停止闪烁，阻塞直到工作线程退出（最多等待2秒）
        /// </summary>
        public void Stop()
        {
            lock (_lock) { StopInternal(); }
        }

        /// <summary>
        /// 核心高精度循环
        /// </summary>
        private void PrecisionLoop()
        {
            // Windows下将系统定时器分辨率提升至1ms，优化Sleep精度
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                NativeMethods.TimeBeginPeriod(1);

            try
            {
                var token = _cts.Token;
                long halfPeriod = _halfPeriodTicks;
                var spinWait = new SpinWait();

                while (!token.IsCancellationRequested)
                {
                    // ===== 绝对时间点调度：消除累积误差 =====
                    // 全程使用 Stopwatch 原始节拍（ElapsedTicks），与 _halfPeriodTicks 单位一致
                    long now = _stopwatch.ElapsedTicks;
                    long wait = _nextToggleTicks - now;

                    // 如果严重落后（例如系统被挂起过），重置时间点以避免追赶风暴
                    if (wait < -halfPeriod * 10)
                    {
                        _nextToggleTicks = now + halfPeriod;
                        wait = halfPeriod;
                    }
                    // 如果轻微落后，立即翻转并调整下次时间点
                    else if (wait < 0)
                    {
                        _nextToggleTicks = now + halfPeriod;
                        wait = halfPeriod;
                    }

                    // ===== 混合等待策略 =====
                    if (wait > 0)
                    {
                        double waitMs = wait / (Frequency / 1_000.0);
                        if (waitMs > 15.0)
                        {
                            // 剩余时间大于15ms时，短暂休眠释放CPU，留下2ms自旋余量
                            Thread.Sleep(Math.Max(1, (int)(waitMs - 2)));
                        }

                        // 高精度自旋微调，直到达到或超过目标时刻（ElapsedTicks 原始节拍）
                        while (_stopwatch.ElapsedTicks < _nextToggleTicks)
                        {
                            spinWait.SpinOnce();
                        }
                    }

                    // ===== 精确翻转 =====
                    _currentValue = !_currentValue;
                    OnToggled?.Invoke(_currentValue);

                    // 计划下一次翻转的绝对时间
                    _nextToggleTicks += halfPeriod;
                }
            }
            finally
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    NativeMethods.TimeEndPeriod(1);
            }
        }

        /// <summary>StopInternal 方法。</summary>
        private void StopInternal()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _worker?.Join(2000);
                _cts.Dispose();
                _worker = null;
                _cts = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        // Windows API 声明
        private static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
            public static extern uint TimeBeginPeriod(uint uMilliseconds);

            [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
            public static extern uint TimeEndPeriod(uint uMilliseconds);
        }
    }
}
