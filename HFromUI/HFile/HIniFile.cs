using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HFromUI.HFile
{
    /// <summary>
    /// INI 文件操作类，用于读取和写入标准的 .ini 配置文件。
    /// 底层调用 Windows API (kernel32.dll) 的 GetPrivateProfileString / WritePrivateProfileString。
    /// 所有方法均提供异常处理和默认值支持。
    /// </summary>
    public class HIniFile
    {
        #region Windows API 声明

        /// <summary>
        /// 从 INI 文件中读取字符串值。
        /// </summary>
        /// <param name="lpAppName">节（Section）名称。</param>
        /// <param name="lpKeyName">键（Key）名称。</param>
        /// <param name="lpDefault">当键不存在时返回的默认值。</param>
        /// <param name="lpReturnedString">接收返回值的字符串缓冲区。</param>
        /// <param name="nSize">缓冲区大小（字符数）。</param>
        /// <param name="lpFileName">INI 文件完整路径。</param>
        /// <returns>复制到缓冲区的字符数（不包括 null 终止符）。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        /// <summary>
        /// 将字符串值写入 INI 文件。
        /// </summary>
        /// <param name="lpAppName">节名称。</param>
        /// <param name="lpKeyName">键名称，若为 null 则删除整个节。</param>
        /// <param name="lpString">要写入的字符串值，若为 null 则删除键。</param>
        /// <param name="lpFileName">INI 文件完整路径。</param>
        /// <returns>成功返回非零值。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);

        /// <summary>
        /// 获取 INI 文件中指定节下的所有键名，或获取所有节名。
        /// </summary>
        /// <param name="lpAppName">节名称，若为 null 则返回所有节名。</param>
        /// <param name="lpReturnedString">缓冲区。</param>
        /// <param name="nSize">缓冲区大小。</param>
        /// <param name="lpFileName">INI 文件路径。</param>
        /// <returns>实际读取到的字符数。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSection(
            string lpAppName,
            IntPtr lpReturnedString,
            int nSize,
            string lpFileName);

        #endregion

        #region 字段和构造函数

        /// <summary>当前实例绑定的 INI 文件完整路径。</summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 初始化 INI 文件操作实例，并指定文件路径。
        /// 文件不存在时不会自动创建，写入操作时会自动创建。
        /// </summary>
        /// <param name="filePath">INI 文件的完整路径。</param>
        public HIniFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        #endregion

        #region 读取值

        /// <summary>
        /// 读取指定节和键的字符串值。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="defaultValue">键不存在时返回的默认值。</param>
        /// <returns>键对应的字符串值，若不存在则返回默认值。</returns>
        public string ReadString(string section, string key, string defaultValue = "")
        {
            try
            {
                var buffer = new StringBuilder(32768); // 32KB 缓冲区足够大
                int chars = GetPrivateProfileString(section, key, defaultValue, buffer, buffer.Capacity, FilePath);
                return buffer.ToString();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 读取整数值。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="defaultValue">默认整数值。</param>
        /// <returns>键对应的整数，若不存在或转换失败则返回默认值。</returns>
        public int ReadInt(string section, string key, int defaultValue = 0)
        {
            string strValue = ReadString(section, key, defaultValue.ToString());
            return int.TryParse(strValue, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// 读取布尔值（支持 "true"/"false" 或 "1"/"0" 等）。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="defaultValue">默认布尔值。</param>
        /// <returns>键对应的布尔值，若不存在或无法识别则返回默认值。</returns>
        public bool ReadBool(string section, string key, bool defaultValue = false)
        {
            string strValue = ReadString(section, key, defaultValue.ToString());
            if (bool.TryParse(strValue, out bool boolResult))
                return boolResult;
            if (int.TryParse(strValue, out int intVal))
                return intVal != 0;
            return defaultValue;
        }

        #endregion

        #region 写入值

        /// <summary>
        /// 写入字符串值。如果节或键不存在，将自动创建。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        /// <returns>写入成功返回 true，否则 false。</returns>
        public bool WriteString(string section, string key, string value)
        {
            try
            {
                return WritePrivateProfileString(section, key, value, FilePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入整数值。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="value">整数值。</param>
        /// <returns>成功返回 true。</returns>
        public bool WriteInt(string section, string key, int value)
        {
            return WriteString(section, key, value.ToString());
        }

        /// <summary>
        /// 写入布尔值（存储为 "True"/"False"）。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <param name="value">布尔值。</param>
        /// <returns>成功返回 true。</returns>
        public bool WriteBool(string section, string key, bool value)
        {
            return WriteString(section, key, value.ToString());
        }

        #endregion

        #region 删除操作

        /// <summary>
        /// 删除指定节下的一个键。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">要删除的键名称。</param>
        /// <returns>操作成功返回 true。</returns>
        public bool DeleteKey(string section, string key)
        {
            // 传入 null 作为值，Windows API 会删除该键
            return WriteString(section, key, null);
        }

        /// <summary>
        /// 删除整个节（包含该节下所有键）。
        /// </summary>
        /// <param name="section">要删除的节名称。</param>
        /// <returns>操作成功返回 true。</returns>
        public bool DeleteSection(string section)
        {
            // 将键名设为 null，Windows API 会删除整个节
            try
            {
                return WritePrivateProfileString(section, null, null, FilePath);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 枚举与查询

        /// <summary>
        /// 获取 INI 文件中所有的节名称。
        /// </summary>
        /// <returns>节名称列表，如果文件不存在或出错则返回空列表。</returns>
        public List<string> GetSectionNames()
        {
            var sections = new List<string>();
            try
            {
                // 分配 32KB 内存
                IntPtr ptr = Marshal.AllocCoTaskMem(32768);
                int len = GetPrivateProfileSection(null, ptr, 32768, FilePath);
                if (len > 0)
                {
                    string allSections = Marshal.PtrToStringAuto(ptr, len);
                    // 各节名以 \0 分隔，末尾双 \0 结束
                    string[] parts = allSections.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    sections.AddRange(parts);
                }
                Marshal.FreeCoTaskMem(ptr);
            }
            catch
            {
                // 忽略
            }
            return sections;
        }

        /// <summary>
        /// 获取指定节下的所有键名。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <returns>键名列表，如果节不存在则返回空列表。</returns>
        public List<string> GetKeyNames(string section)
        {
            var keys = new List<string>();
            try
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(32768);
                int len = GetPrivateProfileSection(section, ptr, 32768, FilePath);
                if (len > 0)
                {
                    string allKeys = Marshal.PtrToStringAuto(ptr, len);
                    string[] lines = allKeys.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        int equalsPos = line.IndexOf('=');
                        if (equalsPos > 0)
                            keys.Add(line.Substring(0, equalsPos));
                    }
                }
                Marshal.FreeCoTaskMem(ptr);
            }
            catch
            {
                // 忽略
            }
            return keys;
        }

        /// <summary>
        /// 判断指定节是否存在。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <returns>存在返回 true。</returns>
        public bool SectionExists(string section)
        {
            var sections = GetSectionNames();
            return sections.Contains(section);
        }

        /// <summary>
        /// 判断指定节下的键是否存在。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <param name="key">键名称。</param>
        /// <returns>存在返回 true。</returns>
        public bool KeyExists(string section, string key)
        {
            // 直接读取，如果返回的不是默认值且文件中有该键，可认为存在，
            // 更可靠的方法是获取键列表然后判断。
            var keys = GetKeyNames(section);
            return keys.Contains(key);
        }

        /// <summary>
        /// 获取指定节下所有键值对（字典）。
        /// </summary>
        /// <param name="section">节名称。</param>
        /// <returns>键-值字典。</returns>
        public Dictionary<string, string> GetSectionValues(string section)
        {
            var values = new Dictionary<string, string>();
            foreach (string key in GetKeyNames(section))
            {
                string val = ReadString(section, key, null);
                values[key] = val;
            }
            return values;
        }

        #endregion
    }
}