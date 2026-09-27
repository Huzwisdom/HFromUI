using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace HFromUI.HData
{
    /// <summary>
    /// 计时器管理器：
    /// 管理一组以 <see cref="int"/> 为标识的计时器。
    /// 可选择互斥模式（默认）或独立模式。
    /// 互斥模式下同一时刻仅允许一个计时器运行；独立模式下各计时器可同时运行。
    /// 通过静态字典按组名管理多个管理器实例，每个组内部维护自己的计时器集合。
    /// </summary>
    public class HExclusiveTimer
    {
        // ==================== 静态全局管理 ====================

        /// <summary>
        /// 默认组名，当未指定组名时使用。
        /// </summary>
        private const string DefaultGroupName = "default";

        /// <summary>
        /// 全局静态字典：键为组名，值为对应的 <see cref="HExclusiveTimer"/> 实例。
        /// 使用 <see cref="ConcurrentDictionary{TKey, TValue}"/> 保证并发访问安全。
        /// </summary>
        private static readonly ConcurrentDictionary<string, HExclusiveTimer> _groups =
            new ConcurrentDictionary<string, HExclusiveTimer>();

        // ==================== 实例字段 ====================

        /// <summary>
        /// 当前组内的计时器字典：键为 <see cref="int"/> 类型的计时器标识，值为 <see cref="HRunTimer"/> 实例。
        /// </summary>
        private readonly ConcurrentDictionary<int, HRunTimer> _timers =
            new ConcurrentDictionary<int, HRunTimer>();

        /// <summary>
        /// 用于保证实例内部操作原子性的锁对象。
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 指示当前管理器是否采用互斥模式。
        /// <see langword="true"/> 表示互斥（同时只允许一个计时器运行）；
        /// <see langword="false"/> 表示独立（各计时器可同时运行）。
        /// </summary>
        private readonly bool _exclusive;

        // ==================== 构造函数 ====================

        /// <summary>
        /// 初始化 <see cref="HExclusiveTimer"/> 类的新实例。
        /// </summary>
        /// <param name="exclusive">
        /// 是否启用互斥模式。
        /// <see langword="true"/>：启动新计时器时会自动停止其他正在运行的计时器（默认行为）；
        /// <see langword="false"/>：各计时器独立运行，互不影响。
        /// </param>
        public HExclusiveTimer(bool exclusive = true)
        {
            _exclusive = exclusive;
        }

        // ==================== 静态方法：全局按组管理 ====================

        /// <summary>
        /// 获取指定名称的计时器组实例。如果该组不存在，则自动创建一个新的实例（默认互斥模式）。
        /// </summary>
        /// <param name="groupName">组名称。若为 <see langword="null"/> 或空白，则使用默认组名。</param>
        /// <returns>对应组名的 <see cref="HExclusiveTimer"/> 实例。</returns>
        public static HExclusiveTimer GetGroup(string groupName = null)
        {
            groupName = NormalizeGroupName(groupName);
            return _groups.GetOrAdd(groupName, _ => new HExclusiveTimer());
        }

        /// <summary>
        /// 启动指定组中指定 ID 的计时器（若组不存在则创建）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        public static void Start(string groupName, int id)
        {
            GetGroup(groupName).Start(id);
        }

        /// <summary>
        /// 停止指定组中的所有计时器。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        public static void Stop(string groupName)
        {
            GetGroup(groupName).StopAll();
        }

        /// <summary>
        /// 停止指定组中指定 ID 的计时器。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        public static void Stop(string groupName, int id)
        {
            GetGroup(groupName).Stop(id);
        }

        /// <summary>
        /// 重置指定组中的所有计时器（清零累计时间）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        public static void Reset(string groupName)
        {
            GetGroup(groupName).ResetAll();
        }

        /// <summary>
        /// 重置指定组中指定 ID 的计时器（清零累计时间）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        public static void Reset(string groupName, int id)
        {
            GetGroup(groupName).Reset(id);
        }

        /// <summary>
        /// 获取指定组中当前正在运行的计时器 ID（字符串形式）。
        /// 如果没有任何计时器在运行，则返回 <see langword="null"/>。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <returns>正在运行的计时器 ID 的字符串表示；无运行时返回 <see langword="null"/>。</returns>
        public static string GetRunningId(string groupName)
        {
            return GetGroup(groupName).GetRunningId();
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的累计毫秒数。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计毫秒数。</returns>
        public static double GetMilliseconds(string groupName, int id)
        {
            return GetGroup(groupName).GetMilliseconds(id);
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的累计秒数。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计秒数。</returns>
        public static double GetSeconds(string groupName, int id)
        {
            return GetGroup(groupName).GetSeconds(id);
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的累计分钟数。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计分钟数。</returns>
        public static double GetMinutes(string groupName, int id)
        {
            return GetGroup(groupName).GetMinutes(id);
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的累计小时数。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计小时数。</returns>
        public static double GetHours(string groupName, int id)
        {
            return GetGroup(groupName).GetHours(id);
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的格式化时间字符串（默认格式：时:分:秒.毫秒）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <param name="format">自定义时间格式字符串，默认值为 <c>@"hh\:mm\:ss\.fff"</c>。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public static string ToTimeString(string groupName, int id, string format = @"hh\:mm\:ss\.fff")
        {
            return GetGroup(groupName).ToTimeString(id, format);
        }

        /// <summary>
        /// 获取指定组中指定 ID 计时器的启动/停止状态记录列表（副本）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>状态记录列表的副本。</returns>
        public static List<HRunTimerStatus> GetStatusList(string groupName, int id)
        {
            return GetGroup(groupName).GetStatusList(id);
        }

        /// <summary>
        /// 释放指定组中指定 ID 的计时器（从字典中移除并释放资源）。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <param name="id">计时器标识。</param>
        /// <returns>如果成功移除并释放返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool Dispose(string groupName, int id)
        {
            return GetGroup(groupName).Dispose(id);
        }

        /// <summary>
        /// 释放指定组中的所有计时器资源。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        public static void DisposeAll(string groupName)
        {
            GetGroup(groupName).DisposeAll();
        }

        /// <summary>
        /// 移除指定名称的整个计时器组，并释放组内所有计时器资源。
        /// </summary>
        /// <param name="groupName">组名称。</param>
        /// <returns>如果成功移除返回 <see langword="true"/>；如果组不存在返回 <see langword="false"/>。</returns>
        public static bool RemoveGroup(string groupName)
        {
            groupName = NormalizeGroupName(groupName);
            if (_groups.TryRemove(groupName, out HExclusiveTimer group))
            {
                group.DisposeAll();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空所有计时器组，并释放所有组内的计时器资源。
        /// </summary>
        public static void ClearAllGroups()
        {
            foreach (var kvp in _groups)
                kvp.Value.DisposeAll();
            _groups.Clear();
        }

        /// <summary>
        /// 规范化组名称：若传入的组名为 <see langword="null"/> 或空白，则返回默认组名。
        /// </summary>
        /// <param name="groupName">原始组名称。</param>
        /// <returns>规范化后的组名称。</returns>
        private static string NormalizeGroupName(string groupName)
        {
            return string.IsNullOrWhiteSpace(groupName) ? DefaultGroupName : groupName;
        }

        // ==================== 实例方法 ====================

        /// <summary>
        /// 启动指定 ID 的计时器。若该 ID 不存在则自动创建。
        /// 如果当前实例为互斥模式，则会先停止所有其他正在运行的计时器。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        public void Start(int id)
        {
            lock (_lock)
            {
                if (_exclusive)
                {
                    // 互斥模式：遍历所有计时器，停止除目标 ID 外所有正在运行的计时器
                    foreach (var kvp in _timers)
                    {
                        if (kvp.Key != id && kvp.Value.IsRun)
                            kvp.Value.Stop();
                    }
                }

                // 获取或创建目标计时器并启动
                HRunTimer timer = GetOrAdd(id);
                timer.Start();
            }
        }

        /// <summary>
        /// 停止当前组内所有计时器（无论是否正在运行）。
        /// </summary>
        public void StopAll()
        {
            lock (_lock)
            {
                foreach (var kvp in _timers)
                    kvp.Value.Stop();
            }
        }

        /// <summary>
        /// 停止指定 ID 的计时器（如果该 ID 存在）。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        public void Stop(int id)
        {
            lock (_lock)
            {
                if (_timers.TryGetValue(id, out HRunTimer timer))
                    timer.Stop();
            }
        }

        /// <summary>
        /// 重置当前组内所有计时器（清零累计时间，并清空状态记录）。
        /// </summary>
        public void ResetAll()
        {
            lock (_lock)
            {
                foreach (var kvp in _timers)
                    kvp.Value.Reset();
            }
        }

        /// <summary>
        /// 重置指定 ID 的计时器（清零累计时间，并清空状态记录）。若该 ID 不存在则创建。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        public void Reset(int id)
        {
            lock (_lock)
            {
                GetOrAdd(id).Reset();
            }
        }

        /// <summary>
        /// 获取当前正在运行的计时器 ID（字符串形式）。
        /// 如果没有任何计时器在运行，返回 <see langword="null"/>。
        /// </summary>
        /// <returns>正在运行的计时器 ID 字符串；无运行时返回 <see langword="null"/>。</returns>
        public string GetRunningId()
        {
            lock (_lock)
            {
                foreach (var kvp in _timers)
                    if (kvp.Value.IsRun)
                        return kvp.Key.ToString();
                return null;
            }
        }

        /// <summary>
        /// 获取当前正在运行的计时器实例。
        /// 如果没有任何计时器在运行，返回 <see langword="null"/>。
        /// </summary>
        /// <returns>正在运行的 <see cref="HRunTimer"/> 实例；无运行时返回 <see langword="null"/>。</returns>
        public HRunTimer GetRunningTimer()
        {
            lock (_lock)
            {
                foreach (var kvp in _timers)
                    if (kvp.Value.IsRun)
                        return kvp.Value;
                return null;
            }
        }

        /// <summary>
        /// 获取指定 ID 计时器的累计毫秒数。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计毫秒数。</returns>
        public double GetMilliseconds(int id) => GetOrAdd(id).TotalMilliseconds;

        /// <summary>
        /// 获取指定 ID 计时器的累计秒数。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计秒数。</returns>
        public double GetSeconds(int id) => GetOrAdd(id).TotalSeconds;

        /// <summary>
        /// 获取指定 ID 计时器的累计分钟数。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计分钟数。</returns>
        public double GetMinutes(int id) => GetOrAdd(id).TotalMinutes;

        /// <summary>
        /// 获取指定 ID 计时器的累计小时数。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>累计小时数。</returns>
        public double GetHours(int id) => GetOrAdd(id).TotalHours;

        /// <summary>
        /// 获取指定 ID 计时器的格式化时间字符串。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <param name="format">自定义时间格式字符串，默认值为 <c>@"hh\:mm\:ss\.fff"</c>。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public string ToTimeString(int id, string format = @"hh\:mm\:ss\.fff")
        {
            return GetOrAdd(id).ToTimeString(format);
        }

        /// <summary>
        /// 获取指定 ID 计时器的启动/停止状态记录列表（副本）。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>状态记录列表的副本。</returns>
        public List<HRunTimerStatus> GetStatusList(int id)
        {
            return GetOrAdd(id).GetStatusList();
        }

        /// <summary>
        /// 释放指定 ID 的计时器（从字典中移除并释放其资源）。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>如果成功移除并释放返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public bool Dispose(int id)
        {
            lock (_lock)
            {
                if (_timers.TryRemove(id, out HRunTimer timer))
                {
                    timer.Dispose();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 释放当前组内所有计时器资源，并清空计时器集合。
        /// </summary>
        public void DisposeAll()
        {
            lock (_lock)
            {
                foreach (var kvp in _timers)
                    kvp.Value.Dispose();
                _timers.Clear();
            }
        }

        /// <summary>
        /// 获取指定 ID 的 <see cref="HRunTimer"/> 实例（若不存在则创建）。
        /// 注意：此方法会绕过互斥逻辑直接返回底层计时器，使用时请谨慎操作。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>对应的 <see cref="HRunTimer"/> 实例。</returns>
        public HRunTimer GetTimer(int id) => GetOrAdd(id);

        /// <summary>
        /// 私有辅助方法：从内部字典获取或创建指定 ID 的计时器实例。
        /// </summary>
        /// <param name="id">计时器标识。</param>
        /// <returns>已存在或新创建的 <see cref="HRunTimer"/> 实例。</returns>
        private HRunTimer GetOrAdd(int id)
        {
            return _timers.GetOrAdd(id, _ => new HRunTimer());
        }
    }
}