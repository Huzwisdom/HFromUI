using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace HFromUI.HData
{
    public class HRunTimer : IDisposable
    {
        private const string DefaultName = "time";

        private static readonly ConcurrentDictionary<string, HRunTimer> concurrentDictionaryHRunTimer =
            new ConcurrentDictionary<string, HRunTimer>();

        // ---------- 静态方法：全局管理 ----------

        /// <summary>
        /// 启动指定名称的计时器（若不存在则创建）
        /// </summary>
        public static void Start(string name = null)
        {
            name = NormalizeName(name);
            GetOrAdd(name).Start();
        }

        /// <summary>
        /// 停止指定名称的计时器（若不存在则创建）
        /// </summary>
        public static void Stop(string name = null)
        {
            name = NormalizeName(name);
            GetOrAdd(name).Stop();
        }

        /// <summary>
        /// 重置指定名称的计时器（若不存在则创建）
        /// </summary>
        public static void Reset(string name = null)
        {
            name = NormalizeName(name);
            GetOrAdd(name).Reset();
        }

        /// <summary>
        /// 重置指定名称的计时器（若不存在则创建）
        /// </summary>
        public static void ResetAll()
        {
            foreach (var kvp in concurrentDictionaryHRunTimer)
            { kvp.Value.Reset(); }
        }
        /// <summary>
        /// 获取指定名称计时器的累计毫秒数
        /// </summary>
        public static double GetMilliseconds(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name).TotalMilliseconds;
        }

        /// <summary>
        /// 获取指定名称计时器的累计秒数
        /// </summary>
        public static double GetSeconds(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name).TotalSeconds;
        }

        /// <summary>
        /// 获取指定名称计时器的累计分钟数
        /// </summary>
        public static double GetMinutes(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name).TotalMinutes;
        }

        /// <summary>
        /// 获取指定名称计时器的累计小时数
        /// </summary>
        public static double GetHours(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name).TotalHours;
        }

        /// <summary>
        /// 获取指定名称计时器的格式化时间字符串，例如：01:02:03.456 //d\.hh\:mm\:ss\.fff
        /// </summary>
        public static string ToTimeString(string name = null, string format = @"hh\:mm\:ss\.fff")
        {
            name = NormalizeName(name);
            return GetOrAdd(name).ToTimeString(format);
        }

        /// <summary>
        /// 获取指定名称计时器的启动/停止状态记录列表（副本）
        /// </summary>
        public static List<HRunTimerStatus> GetStatusList(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name).GetStatusList();
        }

        /// <summary>
        /// 移除指定名称的计时器（不调用 Dispose，仅移除字典引用）
        /// </summary>
        public static bool Remove(string name)
        {
            name = NormalizeName(name);
            return concurrentDictionaryHRunTimer.TryRemove(name, out _);
        }

        /// <summary>
        /// 释放指定名称的计时器（从字典移除并调用 Dispose）
        /// </summary>
        public static bool Dispose(string name)
        {
            name = NormalizeName(name);
            if (concurrentDictionaryHRunTimer.TryRemove(name, out HRunTimer timer))
            {
                timer.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放所有计时器并清空字典
        /// </summary>
        public static void DisposeAll()
        {
            foreach (var kvp in concurrentDictionaryHRunTimer)
            {
                kvp.Value.Dispose();
            }
            concurrentDictionaryHRunTimer.Clear();
        }

        /// <summary>
        /// 清空所有计时器（仅移除引用，不调用 Dispose）
        /// 建议使用 DisposeAll 来同时释放资源
        /// </summary>
        public static void Clear()
        {
            concurrentDictionaryHRunTimer.Clear();
        }

        /// <summary>
        /// 判断指定名称的计时器是否存在
        /// </summary>
        public static bool Contains(string name)
        {
            name = NormalizeName(name);
            return concurrentDictionaryHRunTimer.ContainsKey(name);
        }

        /// <summary>
        /// 获取或创建指定名称的计时器实例（供高级用户直接操作）
        /// </summary>
        public static HRunTimer GetTimer(string name = null)
        {
            name = NormalizeName(name);
            return GetOrAdd(name);
        }

        // 内部辅助：名称标准化
        private static string NormalizeName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? DefaultName : name;
        }

        // 内部辅助：从字典获取或创建实例
        private static HRunTimer GetOrAdd(string name)
        {
            return concurrentDictionaryHRunTimer.GetOrAdd(name, _ => new HRunTimer());
        }

        // ---------- 实例成员 ----------

        private readonly Stopwatch _stopwatch = new Stopwatch();
        /// <summary>_lock 字段。</summary>
        private readonly object _lock = new object();
        /// <summary>_isRunning 字段。</summary>
        private bool _isRunning = false;
        /// <summary>_disposed 字段。</summary>
        private bool _disposed = false; // 标记是否已释放
        /// <summary>上一次计时记录的CT（毫秒）：Reset清零前自动把当前累计值保存到这里，新一轮运行后仍可查询上轮CT</summary>
        private double _lastMilliseconds = 0;
        /// <summary>
        /// 状态变更记录列表（每次启动/停止都会追加一条记录）
        /// </summary>
        public List<HRunTimerStatus> RunTimerStatusList = new List<HRunTimerStatus>();

        public HRunTimer()
        {
        }

        /// <summary>
        /// 析构函数（安全网，防止忘记调用 Dispose）
        /// </summary>
        ~HRunTimer()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源的内部实现
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                if (disposing)
                {
                    // 停止计时器（如果正在运行）
                    if (_isRunning)
                    {
                        _stopwatch.Stop();
                        _isRunning = false;
                    }

                    // 清空状态记录列表
                    RunTimerStatusList?.Clear();
                }

                // 释放非托管资源（如果有）
                // 这里 Stopwatch 不持有非托管资源，但可根据需要添加
            }

            _disposed = true;
        }

        /// <summary>
        /// 清零并保持原有运行状态。
        /// 清零前先把当前累计CT保存到 LastMilliseconds（上一次记录的CT）；
        /// 如果正在运行，则继续从 0 开始计时；
        /// 如果已停止，则保持停止，累计时间为 0。
        /// 同时清空状态记录列表。
        /// </summary>
        public void Reset()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HRunTimer));

            lock (_lock)
            {
                _lastMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;
                _stopwatch.Reset();
                RunTimerStatusList.Clear();
                if (_isRunning)
                    _stopwatch.Start();
            }
        }

        /// <summary>Start 方法。</summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HRunTimer));

            lock (_lock)
            {
                if (_isRunning)
                    return;
                _stopwatch.Start();
                _isRunning = true;

                // 记录启动状态
                RunTimerStatusList.Add(new HRunTimerStatus
                {
                    IsRunning = true,
                    StatusDateTime = DateTime.Now
                });
            }
        }

        /// <summary>Stop 方法。</summary>
        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HRunTimer));

            lock (_lock)
            {
                if (!_isRunning)
                    return;
                _stopwatch.Stop();
                _isRunning = false;

                // 记录停止状态
                RunTimerStatusList.Add(new HRunTimerStatus
                {
                    IsRunning = false,
                    StatusDateTime = DateTime.Now
                });
            }
        }

        public bool IsRun
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(HRunTimer));

                lock (_lock)
                {
                    return _isRunning;
                }
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        /// <summary>
        /// 获取累计毫秒数
        /// </summary>
        public double GetCumulative()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HRunTimer));

            lock (_lock)
            {
                return _stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        /// <summary>
        /// 获取启动/停止状态记录列表的副本（线程安全）
        /// </summary>
        public List<HRunTimerStatus> GetStatusList()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HRunTimer));

            lock (_lock)
            {
                return new List<HRunTimerStatus>(RunTimerStatusList);
            }
        }

        // ---- 毫秒转秒、转分钟、字符串 ----
        public double TotalMilliseconds => GetCumulative();
        /// <summary>TotalSeconds 成员。</summary>
        public double TotalSeconds => GetCumulative() / 1000.0;
        /// <summary>TotalMinutes 成员。</summary>
        public double TotalMinutes => GetCumulative() / 60000.0;
        /// <summary>TotalHours 成员。</summary>
        public double TotalHours => GetCumulative() / 3600000.0;

        /// <summary>获取上一次记录的CT（毫秒）：最近一次Reset清零前保存的累计值</summary>
        public double LastMilliseconds
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(HRunTimer));

                lock (_lock)
                {
                    return _lastMilliseconds;
                }
            }
        }
        /// <summary>上一次记录的CT（秒）</summary>
        public double LastSeconds => LastMilliseconds / 1000.0;
        /// <summary>上一次记录的CT（分钟）</summary>
        public double LastMinutes => LastMilliseconds / 60000.0;
        /// <summary>上一次记录的CT（小时）</summary>
        public double LastHours => LastMilliseconds / 3600000.0;
        /// <summary>上一次记录CT的格式化时间字符串，例如：01:02:03.456</summary>
        public string ToLastTimeString(string format = @"hh\:mm\:ss\.fff")
        {
            return TimeSpan.FromMilliseconds(LastMilliseconds).ToString(format);
        }

        /// <summary>
        /// 格式化为字符串，例如：01:02:03.456////d\.hh\:mm\:ss\.fff
        /// </summary>
        public string ToTimeString(string format = @"hh\:mm\:ss\.fff")
        {
            return TimeSpan.FromMilliseconds(GetCumulative()).ToString(format);
        }

        public override string ToString()
        {
            return this.ToTimeString();
        }
    }

    public struct HRunTimerStatus
    {
        /// <summary>IsRunning 成员。</summary>
        public bool IsRunning { get; set; }
        /// <summary>StatusDateTime 成员。</summary>
        public DateTime StatusDateTime { get; set; }
    }
}
