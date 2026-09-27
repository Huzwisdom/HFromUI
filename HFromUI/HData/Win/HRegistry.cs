using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using HFromUI;

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全面的 Windows 注册表操作工具类（.NET Framework 4.8 / C# 7.3）。
    /// 提供创建/删除/枚举、读写、权限管理、监视变化、搜索、导入/导出、远程连接、视图切换、
    /// 启动项管理、右键菜单管理（文件/文件夹/驱动器/新建菜单等）等高级功能。
    /// 所有面向用户的文本均通过 <see cref="HTranslation.GetContent"/> 进行多语言翻译。
    /// </summary>
    public class HRegistry
    {

        #region 常用根键快捷属性
        /// <summary>HKEY_CLASSES_ROOT 根键。</summary>
        public static RegistryKey ClassesRoot => Registry.ClassesRoot;
        /// <summary>HKEY_CURRENT_USER 根键。</summary>
        public static RegistryKey CurrentUser => Registry.CurrentUser;
        /// <summary>HKEY_LOCAL_MACHINE 根键。</summary>
        public static RegistryKey LocalMachine => Registry.LocalMachine;
        /// <summary>HKEY_USERS 根键。</summary>
        public static RegistryKey Users => Registry.Users;
        /// <summary>HKEY_CURRENT_CONFIG 根键。</summary>
        public static RegistryKey CurrentConfig => Registry.CurrentConfig;
        #endregion

        #region 打开/创建注册表项（支持 64 位视图）

        /// <summary>根据注册表蜂巢和视图打开根键。</summary>
        /// <param name="hive">注册表蜂巢，如 RegistryHive.LocalMachine。</param>
        /// <param name="view">注册表视图（32 位或 64 位），默认为 Default。</param>
        /// <returns>打开的 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenBaseKey(RegistryHive hive, RegistryView view = RegistryView.Default)
        {
            return RegistryKey.OpenBaseKey(hive, view);
        }

        /// <summary>创建或打开指定子项（可读写）。如果子项不存在则创建，存在则打开。</summary>
        /// <param name="hive">注册表蜂巢。</param>
        /// <param name="subKey">子键路径，相对于根键。</param>
        /// <param name="view">注册表视图。</param>
        /// <returns>可读写的 RegistryKey，失败返回 null。</returns>
        public static RegistryKey CreateOrOpenSubKey(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    return baseKey.CreateSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
            }
            catch { return null; }
        }

        /// <summary>以只读方式打开指定子项。</summary>
        /// <returns>只读 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenSubKeyReadOnly(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    return baseKey.OpenSubKey(subKey, false);
                }
            }
            catch { return null; }
        }

        /// <summary>以可写方式打开指定子项。</summary>
        /// <returns>可写 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenSubKeyWritable(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    return baseKey.OpenSubKey(subKey, true);
                }
            }
            catch { return null; }
        }

        #endregion

        #region 子项操作

        /// <summary>创建子项（已存在则打开）。</summary>
        public static RegistryKey CreateSubKey(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default) =>
            CreateOrOpenSubKey(hive, subKey, view);

        /// <summary>删除子项（不递归删除子项）。</summary>
        /// <returns>操作成功返回 true，否则 false。</returns>
        public static bool DeleteSubKey(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    baseKey.DeleteSubKey(subKey, false);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>递归删除子项及其所有子项。</summary>
        /// <returns>操作成功返回 true。</returns>
        public static bool DeleteSubKeyTree(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    baseKey.DeleteSubKeyTree(subKey, false);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>判断指定子项是否存在。</summary>
        public static bool SubKeyExists(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                return key != null;
        }

        /// <summary>获取指定子项下的直接子项名称列表。</summary>
        /// <returns>子项名称数组，失败返回空数组。</returns>
        public static string[] GetSubKeyNames(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                    return key?.GetSubKeyNames() ?? new string[0];
            }
            catch { return new string[0]; }
        }

        /// <summary>重命名子项（通过复制+删除实现）。</summary>
        public static bool RenameSubKey(RegistryHive hive, string parentSubKey, string oldName, string newName)
        {
            try
            {
                CopyKey(hive, $"{parentSubKey}\\{oldName}", hive, $"{parentSubKey}\\{newName}");
                DeleteSubKeyTree(hive, $"{parentSubKey}\\{oldName}");
                return true;
            }
            catch { return false; }
        }

        /// <summary>复制整个注册表键（包含所有子项和值）。
        /// 修复：原先用 regedit 导出/导入实现，但 .reg 文件内嵌原始键路径，
        /// 导入时总是写回原路径，目标路径参数完全无效（RenameSubKey 随后删除源键会导致数据丢失），
        /// 改为托管 API 递归复制，支持跨蜂巢、改名复制。</summary>
        public static bool CopyKey(RegistryHive sourceHive, string sourceSubKey, RegistryHive destHive, string destSubKey)
        {
            try
            {
                using (RegistryKey src = OpenSubKeyReadOnly(sourceHive, sourceSubKey))
                using (RegistryKey dst = CreateOrOpenSubKey(destHive, destSubKey))
                {
                    if (src == null || dst == null) return false;
                    CopyKeyContents(src, dst);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>递归复制源键下的所有值与子键到目标键。</summary>
        private static void CopyKeyContents(RegistryKey src, RegistryKey dst)
        {
            // 复制所有值，保留原始值类型；REG_EXPAND_SZ 不展开环境变量
            foreach (string valueName in src.GetValueNames())
            {
                object value = src.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                RegistryValueKind kind = src.GetValueKind(valueName);
                dst.SetValue(valueName, value, kind);
            }
            // 递归复制子键
            foreach (string childName in src.GetSubKeyNames())
            {
                using (RegistryKey srcChild = src.OpenSubKey(childName, false))
                using (RegistryKey dstChild = dst.CreateSubKey(childName))
                {
                    if (srcChild != null && dstChild != null)
                        CopyKeyContents(srcChild, dstChild);
                }
            }
        }

        #endregion

        #region 值操作（读取、写入、删除、枚举、类型判断）

        /// <summary>读取指定值（返回 object）。</summary>
        /// <param name="defaultValue">键或值不存在时返回的默认值。</param>
        public static object GetValue(RegistryHive hive, string subKey, string valueName, object defaultValue = null, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    return key?.GetValue(valueName, defaultValue);
                }
            }
            catch { return defaultValue; }
        }

        /// <summary>读取字符串值。</summary>
        public static string GetString(RegistryHive hive, string subKey, string valueName, string defaultValue = "", RegistryView view = RegistryView.Default)
        {
            object val = GetValue(hive, subKey, valueName, defaultValue, view);
            return val?.ToString() ?? defaultValue;
        }

        /// <summary>读取 DWORD (Int32) 值。</summary>
        public static int GetDWord(RegistryHive hive, string subKey, string valueName, int defaultValue = 0, RegistryView view = RegistryView.Default)
        {
            object val = GetValue(hive, subKey, valueName, defaultValue, view);
            try { return Convert.ToInt32(val); } catch { return defaultValue; }
        }

        /// <summary>读取 QWORD (Int64) 值。</summary>
        public static long GetQWord(RegistryHive hive, string subKey, string valueName, long defaultValue = 0, RegistryView view = RegistryView.Default)
        {
            object val = GetValue(hive, subKey, valueName, defaultValue, view);
            try { return Convert.ToInt64(val); } catch { return defaultValue; }
        }

        /// <summary>读取二进制值（字节数组）。</summary>
        public static byte[] GetBinary(RegistryHive hive, string subKey, string valueName, RegistryView view = RegistryView.Default)
        {
            return GetValue(hive, subKey, valueName, null, view) as byte[];
        }

        /// <summary>读取多字符串值。</summary>
        public static string[] GetMultiString(RegistryHive hive, string subKey, string valueName, RegistryView view = RegistryView.Default)
        {
            return GetValue(hive, subKey, valueName, null, view) as string[];
        }

        /// <summary>写入值。根据对象类型自动选择或指定 RegistryValueKind。</summary>
        /// <param name="kind">注册表值类型，若为 Unknown 则根据对象自动决定。</param>
        public static bool SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind = RegistryValueKind.Unknown, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = CreateOrOpenSubKey(hive, subKey, view))
                {
                    if (key == null) return false;
                    if (kind == RegistryValueKind.Unknown)
                        key.SetValue(valueName, value);
                    else
                        key.SetValue(valueName, value, kind);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>写入 DWORD 值（Int32）。</summary>
        public static bool SetDWord(RegistryHive hive, string subKey, string valueName, int value, RegistryView view = RegistryView.Default)
        {
            return SetValue(hive, subKey, valueName, value, RegistryValueKind.DWord, view);
        }

        /// <summary>写入 QWORD 值（Int64）。</summary>
        public static bool SetQWord(RegistryHive hive, string subKey, string valueName, long value, RegistryView view = RegistryView.Default)
        {
            return SetValue(hive, subKey, valueName, value, RegistryValueKind.QWord, view);
        }

        /// <summary>写入字符串值（REG_SZ）。</summary>
        public static bool SetString(RegistryHive hive, string subKey, string valueName, string value, RegistryView view = RegistryView.Default)
        {
            return SetValue(hive, subKey, valueName, value, RegistryValueKind.String, view);
        }

        /// <summary>删除指定值。</summary>
        public static bool DeleteValue(RegistryHive hive, string subKey, string valueName, RegistryView view = RegistryView.Default)
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

        /// <summary>判断指定值是否存在。</summary>
        public static bool ValueExists(RegistryHive hive, string subKey, string valueName, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    return key?.GetValueNames().Contains(valueName) == true;
                }
            }
            catch { return false; }
        }

        /// <summary>获取子项下所有值名称。</summary>
        public static string[] GetValueNames(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                    return key?.GetValueNames() ?? new string[0];
            }
            catch { return new string[0]; }
        }

        /// <summary>获取指定值的注册表类型（RegistryValueKind）。</summary>
        public static RegistryValueKind GetValueKind(RegistryHive hive, string subKey, string valueName, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    if (key == null) return RegistryValueKind.Unknown;
                    return key.GetValueKind(valueName);
                }
            }
            catch { return RegistryValueKind.Unknown; }
        }

        /// <summary>将 RegistryValueKind 转换为翻译后的友好名称（例如 "REG_DWORD"）。</summary>
        public static string GetValueKindFriendlyName(RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.String: return HTranslation.GetContent("REG_SZ");
                case RegistryValueKind.ExpandString: return HTranslation.GetContent("REG_EXPAND_SZ");
                case RegistryValueKind.Binary: return HTranslation.GetContent("REG_BINARY");
                case RegistryValueKind.DWord: return HTranslation.GetContent("REG_DWORD");
                case RegistryValueKind.MultiString: return HTranslation.GetContent("REG_MULTI_SZ");
                case RegistryValueKind.QWord: return HTranslation.GetContent("REG_QWORD");
                case RegistryValueKind.None: return HTranslation.GetContent("REG_NONE");
                default: return HTranslation.GetContent("未知");
            }
        }

        #endregion

        #region 权限与安全

        /// <summary>获取注册表键的访问控制列表（ACL）。</summary>
        public static RegistrySecurity GetAccessControl(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    return key?.GetAccessControl();
                }
            }
            catch { return null; }
        }

        /// <summary>设置注册表键的访问控制列表。</summary>
        public static bool SetAccessControl(RegistryHive hive, string subKey, RegistrySecurity security, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = OpenBaseKey(hive, view))
                {
                    // 修复：修改 ACL 需要 CHANGE_PERMISSIONS(WRITE_DAC) 权限，
                    // 普通可写打开（WriteKey）不含该权限，SetAccessControl 会抛异常而永远失败
                    using (RegistryKey key = baseKey.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree,
                        RegistryRights.ChangePermissions | RegistryRights.ReadKey))
                    {
                        if (key == null) return false;
                        key.SetAccessControl(security);
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>检测当前用户对指定键是否具有写权限。</summary>
        public static bool HasWritePermission(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey key = OpenSubKeyWritable(hive, subKey, view))
                {
                    return key != null;
                }
            }
            catch { return false; }
        }

        /// <summary>判断当前进程是否具有管理员权限。</summary>
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region 监视注册表变化（基础占位扩展）

        /// <summary>注册表监视器（可基于 WMI 或轮询扩展，当前为基础占位）。</summary>
        public class RegistryMonitor : IDisposable
        {
            private RegistryKey _key;
            private string _subKey;
            private Action<string> _onValueChanged;   // 参数为值名称
            private bool _disposed;

            internal RegistryMonitor(RegistryHive hive, string subKey, Action<string> onValueChanged)
            {
                _subKey = subKey;
                _onValueChanged = onValueChanged;
                // 修复：监视变化只需读权限，原先以可写方式打开，在 HKLM 等位置无管理员权限时会直接返回 null
                _key = OpenSubKeyReadOnly(hive, subKey);
            }

            /// <summary>启动监视（需自行实现具体逻辑，此处仅为占位）。</summary>
            public void Start()
            {
                // 可在此处实现 WMI __RegistryKeyChangeEvent 或 RegistryWatcher 功能
                Debug.WriteLine($"RegistryMonitor started on {_subKey}");
            }

            /// <summary>停止监视并释放资源。</summary>
            public void Stop() => Dispose();

            /// <summary>释放资源。</summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    _key?.Close();
                    _disposed = true;
                }
            }
        }

        /// <summary>创建一个监视器用于监听注册表键的变化（基础占位）。</summary>
        /// <param name="onValueChanged">当值发生改变时的回调，参数为发生改变的值名称。</param>
        public static RegistryMonitor StartMonitor(RegistryHive hive, string subKey, Action<string> onValueChanged)
        {
            return new RegistryMonitor(hive, subKey, onValueChanged);
        }

        #endregion

        #region 搜索注册表

        /// <summary>在指定根键和子键下递归搜索包含特定字符串的值名称或字符串数据。</summary>
        /// <param name="maxResults">最大结果数。</param>
        /// <returns>包含完整路径、值名、值的元组列表。</returns>
        public static List<(string FullPath, string ValueName, object Value)> Search(RegistryHive hive, string startSubKey, string searchTerm, int maxResults = 100, RegistryView view = RegistryView.Default)
        {
            var results = new List<(string, string, object)>();
            SearchRecursive(hive, startSubKey, searchTerm, results, maxResults, view);
            return results;
        }

        /// <summary>SearchRecursive 方法。</summary>
        private static void SearchRecursive(RegistryHive hive, string subKey, string searchTerm, List<(string, string, object)> results, int maxResults, RegistryView view)
        {
            if (results.Count >= maxResults) return;
            try
            {
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey, view))
                {
                    if (key == null) return;
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (results.Count >= maxResults) return;
                        object val = key.GetValue(valueName);
                        if (valueName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            (val is string s && s.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            results.Add(($"{GetHiveShortName(hive)}\\{subKey}", valueName, val));
                        }
                    }
                    foreach (string child in key.GetSubKeyNames())
                    {
                        if (results.Count >= maxResults) return;
                        string childPath = string.IsNullOrEmpty(subKey) ? child : $"{subKey}\\{child}";
                        SearchRecursive(hive, childPath, searchTerm, results, maxResults, view);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 导入/导出

        /// <summary>将指定注册表分支导出为 .reg 文件（使用 regedit.exe）。</summary>
        /// <returns>成功返回文件路径，失败返回 null。</returns>
        public static string ExportKey(RegistryHive hive, string subKey, string filePath)
        {
            try
            {
                string hiveName = GetHiveShortName(hive);
                string fullKey = string.IsNullOrEmpty(subKey) ? hiveName : $"{hiveName}\\{subKey}";
                var psi = new ProcessStartInfo("regedit.exe", $"/e \"{filePath}\" \"{fullKey}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0 && File.Exists(filePath) ? filePath : null;
                }
            }
            catch { return null; }
        }

        /// <summary>导入 .reg 文件到注册表（静默模式）。</summary>
        public static bool ImportRegFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;
                var psi = new ProcessStartInfo("regedit.exe", $"/s \"{filePath}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 远程注册表

        /// <summary>连接远程计算机的注册表根键。</summary>
        /// <param name="machineName">远程计算机名称或 IP 地址。</param>
        /// <param name="hive">注册表蜂巢。</param>
        /// <returns>成功返回远程 RegistryKey，失败返回 null。</returns>
        public static RegistryKey OpenRemoteBaseKey(string machineName, RegistryHive hive, RegistryView view = RegistryView.Default)
        {
            try
            {
                return RegistryKey.OpenRemoteBaseKey(hive, machineName, view);
            }
            catch { return null; }
        }

        #endregion

        #region 信息获取

        /// <summary>获取注册表键的最后写入时间（通过 RegQueryInfoKey API 获取）。</summary>
        /// <returns>DateTime?，失败返回 null。</returns>
        public static DateTime? GetLastWriteTime(RegistryHive hive, string subKey, RegistryView view = RegistryView.Default)
        {
            // 修复：原实现解析 reg.exe query 的输出，但该命令从不输出 LastWriteTime，方法永远返回 null；
            // 改为直接调用 RegOpenKeyEx + RegQueryInfoKey 获取最后写入时间
            IntPtr hKey = IntPtr.Zero;
            try
            {
                // KEY_READ(0x20019) 并按 32/64 位视图附加 KEY_WOW64_* 标志
                int samDesired = 0x20019;
                if (view == RegistryView.Registry64) samDesired |= 0x0100;
                else if (view == RegistryView.Registry32) samDesired |= 0x0200;

                int ret = RegOpenKeyEx(GetHiveHandle(hive), subKey, 0, samDesired, out hKey);
                if (ret != 0 || hKey == IntPtr.Zero) return null;

                ret = RegQueryInfoKey(hKey, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                    out _, out _, out _, out _, out _, out _, out _, out long lastWriteTime);
                if (ret != 0) return null;
                return DateTime.FromFileTime(lastWriteTime);
            }
            catch { return null; }
            finally
            {
                if (hKey != IntPtr.Zero)
                    RegCloseKey(hKey); // 关闭注册表键句柄，避免句柄泄漏
            }
        }

        /// <summary>将 RegistryHive 转换为对应的预定义根键句柄（HKEY_*）。</summary>
        private static IntPtr GetHiveHandle(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot: return new IntPtr(unchecked((int)0x80000000));
                case RegistryHive.CurrentUser: return new IntPtr(unchecked((int)0x80000001));
                case RegistryHive.LocalMachine: return new IntPtr(unchecked((int)0x80000002));
                case RegistryHive.Users: return new IntPtr(unchecked((int)0x80000003));
                case RegistryHive.PerformanceData: return new IntPtr(unchecked((int)0x80000004));
                case RegistryHive.CurrentConfig: return new IntPtr(unchecked((int)0x80000005));
                default: return new IntPtr(unchecked((int)0x80000006)); // DynData
            }
        }

        /// <summary>将 RegistryHive 转换为注册表编辑器中使用的短名称（例如 HKEY_LOCAL_MACHINE）。</summary>
        public static string GetHiveShortName(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot: return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentUser: return "HKEY_CURRENT_USER";
                case RegistryHive.LocalMachine: return "HKEY_LOCAL_MACHINE";
                case RegistryHive.Users: return "HKEY_USERS";
                case RegistryHive.CurrentConfig: return "HKEY_CURRENT_CONFIG";
                default: return hive.ToString();
            }
        }

        #endregion

        #region 启动项管理（自启动程序）

        /// <summary>启动项位置枚举。</summary>
        public enum StartupLocation
        {
            /// <summary>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run</summary>
            HKCU_Run,
            /// <summary>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce</summary>
            HKCU_RunOnce,
            /// <summary>HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run</summary>
            HKLM_Run,
            /// <summary>HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce</summary>
            HKLM_RunOnce,
            /// <summary>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunServices</summary>
            HKCU_RunServices,
            /// <summary>HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunServices</summary>
            HKLM_RunServices
        }

        /// <summary>获取指定位置的启动项列表（返回程序名称和命令路径）。</summary>
        /// <returns>键为名称，值为命令路径的字典。</returns>
        public static Dictionary<string, string> GetStartupPrograms(StartupLocation location)
        {
            var result = new Dictionary<string, string>();
            try
            {
                string subKey = GetStartupSubKey(location);
                RegistryHive hive = location.ToString().StartsWith("HKLM") ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
                using (RegistryKey key = OpenSubKeyReadOnly(hive, subKey))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            string cmd = key.GetValue(valueName)?.ToString();
                            result[valueName] = cmd ?? "";
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        /// <summary>添加启动项。</summary>
        /// <param name="name">启动项名称。</param>
        /// <param name="command">要执行的程序路径及参数。</param>
        public static bool AddStartupProgram(StartupLocation location, string name, string command)
        {
            string subKey = GetStartupSubKey(location);
            RegistryHive hive = location.ToString().StartsWith("HKLM") ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
            return SetString(hive, subKey, name, command);
        }

        /// <summary>删除指定启动项。</summary>
        /// <param name="name">启动项名称。</param>
        public static bool RemoveStartupProgram(StartupLocation location, string name)
        {
            string subKey = GetStartupSubKey(location);
            RegistryHive hive = location.ToString().StartsWith("HKLM") ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
            return DeleteValue(hive, subKey, name);
        }

        /// <summary>根据 StartupLocation 枚举获取对应的注册表子键路径。</summary>
        private static string GetStartupSubKey(StartupLocation location)
        {
            switch (location)
            {
                case StartupLocation.HKCU_Run: return @"Software\Microsoft\Windows\CurrentVersion\Run";
                case StartupLocation.HKCU_RunOnce: return @"Software\Microsoft\Windows\CurrentVersion\RunOnce";
                case StartupLocation.HKLM_Run: return @"Software\Microsoft\Windows\CurrentVersion\Run";
                case StartupLocation.HKLM_RunOnce: return @"Software\Microsoft\Windows\CurrentVersion\RunOnce";
                case StartupLocation.HKCU_RunServices: return @"Software\Microsoft\Windows\CurrentVersion\RunServices";
                case StartupLocation.HKLM_RunServices: return @"Software\Microsoft\Windows\CurrentVersion\RunServices";
                default: return @"Software\Microsoft\Windows\CurrentVersion\Run";
            }
        }

        #endregion

        #region 右键菜单管理（上下文菜单）

        /// <summary>右键菜单作用域枚举。</summary>
        public enum ContextMenuScope
        {
            /// <summary>所有文件 (*)</summary>
            File,
            /// <summary>文件夹</summary>
            Directory,
            /// <summary>文件夹背景</summary>
            DirectoryBackground,
            /// <summary>驱动器</summary>
            Drive,
            /// <summary>所有文件系统对象（文件与文件夹）</summary>
            AllFileSystemObjects,
            /// <summary>桌面背景</summary>
            DesktopBackground,
            /// <summary>自定义 ProgID 或扩展名</summary>
            Unknown
        }

        /// <summary>获取指定作用域下的右键菜单项列表（返回显示名称和命令）。</summary>
        /// <param name="customProgID">当 scope 为 Unknown 时使用的 ProgID 或扩展名。</param>
        /// <returns>元组列表 (显示名称, 命令)。</returns>
        public static List<(string DisplayName, string Command)> GetContextMenuItems(ContextMenuScope scope, string customProgID = null)
        {
            var items = new List<(string, string)>();
            try
            {
                string baseKeyPath = GetContextMenuBaseKey(scope, customProgID);
                if (string.IsNullOrEmpty(baseKeyPath)) return items;

                RegistryHive hive = RegistryHive.ClassesRoot;
                string shellPath = baseKeyPath + "\\shell";
                using (RegistryKey shellKey = OpenSubKeyReadOnly(hive, shellPath))
                {
                    if (shellKey != null)
                    {
                        foreach (string verb in shellKey.GetSubKeyNames())
                        {
                            using (RegistryKey verbKey = shellKey.OpenSubKey(verb))
                            {
                                string displayName = verbKey?.GetValue(null)?.ToString() ?? verb;
                                using (RegistryKey commandKey = verbKey?.OpenSubKey("command"))
                                {
                                    string command = commandKey?.GetValue(null)?.ToString() ?? "";
                                    items.Add((displayName, command));
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return items;
        }

        /// <summary>添加右键菜单项。</summary>
        /// <param name="verb">动词（如 "openwith"），作为注册表子项名称。</param>
        /// <param name="displayName">显示的菜单文字（默认值即为菜单项文本）。</param>
        /// <param name="command">要执行的命令。</param>
        /// <param name="icon">图标路径（可选）。</param>
        /// <param name="customProgID">自定义 ProgID（scope 为 Unknown 时使用）。</param>
        /// <returns>是否成功添加。</returns>
        public static bool AddContextMenuItem(ContextMenuScope scope, string verb, string displayName, string command, string icon = null, string customProgID = null)
        {
            try
            {
                string baseKeyPath = GetContextMenuBaseKey(scope, customProgID);
                if (string.IsNullOrEmpty(baseKeyPath)) return false;

                RegistryHive hive = RegistryHive.ClassesRoot;
                string verbPath = $"{baseKeyPath}\\shell\\{verb}";
                string commandPath = verbPath + "\\command";

                if (!SetString(hive, verbPath, "", displayName)) return false;
                if (!SetString(hive, commandPath, "", command)) return false;
                if (!string.IsNullOrEmpty(icon))
                    SetString(hive, verbPath, "Icon", icon);
                return true;
            }
            catch { return false; }
        }

        /// <summary>删除右键菜单项。</summary>
        /// <param name="verb">要删除的菜单动词。</param>
        public static bool RemoveContextMenuItem(ContextMenuScope scope, string verb, string customProgID = null)
        {
            try
            {
                string baseKeyPath = GetContextMenuBaseKey(scope, customProgID);
                if (string.IsNullOrEmpty(baseKeyPath)) return false;
                RegistryHive hive = RegistryHive.ClassesRoot;
                string verbPath = $"{baseKeyPath}\\shell\\{verb}";
                return DeleteSubKeyTree(hive, verbPath);
            }
            catch { return false; }
        }

        /// <summary>添加“新建”菜单模板（例如 .txt 的 ShellNew）。</summary>
        /// <param name="extension">文件扩展名（如 ".txt"）。</param>
        /// <param name="newItemData">ShellNew 子项下需要设置的值（如 NullFile 为空字符串，Data 为二进制等）。</param>
        /// <returns>是否成功。</returns>
        public static bool AddShellNewItem(string extension, Dictionary<string, object> newItemData)
        {
            try
            {
                RegistryHive hive = RegistryHive.ClassesRoot;
                string subKey = extension + "\\ShellNew";
                foreach (var kvp in newItemData)
                {
                    if (!SetValue(hive, subKey, kvp.Key, kvp.Value))
                        return false;
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>删除“新建”菜单模板。</summary>
        /// <param name="extension">文件扩展名。</param>
        public static bool RemoveShellNewItem(string extension)
        {
            try
            {
                RegistryHive hive = RegistryHive.ClassesRoot;
                return DeleteSubKeyTree(hive, extension + "\\ShellNew");
            }
            catch { return false; }
        }

        /// <summary>获取指定扩展名关联的右键菜单项（通过 ProgID 查找）。</summary>
        /// <param name="extension">文件扩展名（如 ".txt"）。</param>
        /// <returns>菜单项列表。</returns>
        public static List<(string DisplayName, string Command)> GetExtensionContextMenuItems(string extension)
        {
            string progID = GetString(RegistryHive.ClassesRoot, extension, "", "");
            if (string.IsNullOrEmpty(progID))
                return GetContextMenuItems(ContextMenuScope.Unknown, extension);
            return GetContextMenuItems(ContextMenuScope.Unknown, progID);
        }

        /// <summary>根据作用域获取对应的注册表基键路径（在 HKCR 下）。</summary>
        private static string GetContextMenuBaseKey(ContextMenuScope scope, string customID = null)
        {
            switch (scope)
            {
                case ContextMenuScope.File: return "*";
                case ContextMenuScope.Directory: return "Directory";
                case ContextMenuScope.DirectoryBackground: return "Directory\\Background";
                case ContextMenuScope.Drive: return "Drive";
                case ContextMenuScope.AllFileSystemObjects: return "AllFileSystemObjects";
                case ContextMenuScope.DesktopBackground: return "DesktopBackground";
                case ContextMenuScope.Unknown: return customID;
                default: return null;
            }
        }

        #endregion

        #region Win32 API（键最后写入时间）

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegQueryInfoKey(IntPtr hKey, IntPtr lpClass, IntPtr lpcchClass,
            IntPtr lpReserved, out uint lpcSubKeys, out uint lpcbMaxSubKeyLen, out uint lpcbMaxClassLen,
            out uint lpcValues, out uint lpcbMaxValueNameLen, out uint lpcbMaxValueLen,
            out uint lpcbSecurityDescriptor, out long lpftLastWriteTime);

        #endregion
    }
}
