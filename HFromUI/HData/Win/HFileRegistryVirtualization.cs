using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 文件与注册表虚拟化工具类（集成多语言翻译）。
    /// 提供注册表 32/64 位视图操作（RegistryView）以及文件系统重定向（Wow64）的禁用/恢复功能。
    /// 适用于在 32 位应用程序中访问 64 位注册表或文件系统路径。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HFileRegistryVirtualization
    {

        #region 注册表视图操作

        /// <summary>使用指定的注册表视图打开基础键。</summary>
        /// <param name="hive">注册表根键（如 LocalMachine）。</param>
        /// <param name="view">注册表视图（Registry32 或 Registry64）。</param>
        /// <returns>打开的 RegistryKey 对象，失败返回 null。</returns>
        public static RegistryKey OpenBaseKey(RegistryHive hive, RegistryView view)
        {
            try
            {
                return RegistryKey.OpenBaseKey(hive, view);
            }
            catch { return null; }
        }

        /// <summary>以只读方式打开指定视图下的注册表子项。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="view">视图。</param>
        /// <returns>只读 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenSubKeyReadOnly(RegistryHive hive, string subKey, RegistryView view)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    if (baseKey == null) return null;
                    return baseKey.OpenSubKey(subKey, false);
                }
            }
            catch { return null; }
        }

        /// <summary>以可写方式打开指定视图下的注册表子项。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="view">视图。</param>
        /// <returns>可写 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenSubKeyWritable(RegistryHive hive, string subKey, RegistryView view)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    if (baseKey == null) return null;
                    return baseKey.OpenSubKey(subKey, true);
                }
            }
            catch { return null; }
        }

        /// <summary>在指定视图下创建或打开注册表子项（可读写）。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="view">视图。</param>
        /// <returns>可读写的 RegistryKey，失败返回 null。</returns>
        public static RegistryKey CreateOrOpenSubKey(RegistryHive hive, string subKey, RegistryView view)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    if (baseKey == null) return null;
                    return baseKey.CreateSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
            }
            catch { return null; }
        }

        /// <summary>从指定视图下的注册表子项读取字符串值。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="valueName">值名称。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <param name="view">视图。</param>
        /// <returns>字符串值，失败返回默认值。</returns>
        public static string ReadString(RegistryHive hive, string subKey, string valueName, string defaultValue, RegistryView view)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    if (key == null) return defaultValue;
                    object val = key.GetValue(valueName, defaultValue);
                    return val?.ToString() ?? defaultValue;
                }
            }
            catch { return defaultValue; }
        }

        /// <summary>在指定视图下的注册表子项写入字符串值。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="valueName">值名称。</param>
        /// <param name="value">字符串值。</param>
        /// <param name="view">视图。</param>
        /// <returns>是否成功。</returns>
        public static bool WriteString(RegistryHive hive, string subKey, string valueName, string value, RegistryView view)
        {
            try
            {
                using (RegistryKey key = CreateOrOpenSubKey(hive, subKey, view))
                {
                    if (key == null) return false;
                    key.SetValue(valueName, value, RegistryValueKind.String);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>删除指定视图下注册表子项中的值。</summary>
        /// <param name="hive">根键。</param>
        /// <param name="subKey">子键路径。</param>
        /// <param name="valueName">值名称。</param>
        /// <param name="view">视图。</param>
        /// <returns>是否成功。</returns>
        public static bool DeleteValue(RegistryHive hive, string subKey, string valueName, RegistryView view)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyWritable(hive, subKey, view))
                {
                    if (key == null) return false;
                    key.DeleteValue(valueName, false);
                    return true;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 文件系统重定向（Wow64）

        // Win32 API
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(out IntPtr oldValue);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr oldValue);

        /// <summary>临时禁用文件系统重定向（用于在 32 位进程中访问 64 位系统目录）。</summary>
        /// <returns>一个令牌对象，需要调用 <see cref="RevertWow64FsRedirection"/> 恢复。</returns>
        public static IDisposable DisableWow64FsRedirection()
        {
            return new Wow64RedirectionDisabler();
        }

        /// <summary>执行一段操作，期间禁用文件系统重定向，操作完成后自动恢复。</summary>
        /// <param name="action">要在禁用重定向期间执行的操作。</param>
        public static void ExecuteWithoutWow64Redirection(Action action)
        {
            if (action == null) return;
            using (DisableWow64FsRedirection())
            {
                action();
            }
        }

        /// <summary>恢复文件系统重定向（由 DisableWow64FsRedirection 返回的令牌自动处理）。</summary>
        public static void RevertWow64FsRedirection(IntPtr oldValue)
        {
            try
            {
                Wow64RevertWow64FsRedirection(oldValue);
            }
            catch { }
        }

        // 内部令牌类，实现 IDisposable
        private class Wow64RedirectionDisabler : IDisposable
        {
            private IntPtr _oldValue;
            private bool _disposed;

            public Wow64RedirectionDisabler()
            {
                _oldValue = IntPtr.Zero;
                try
                {
                    Wow64DisableWow64FsRedirection(out _oldValue);
                }
                catch { }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_oldValue != IntPtr.Zero)
                    {
                        try { Wow64RevertWow64FsRedirection(_oldValue); } catch { }
                    }
                    _disposed = true;
                }
            }
        }

        #endregion
    }
}
