using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 用户账户与权限管理工具类（集成多语言翻译）。
    /// 提供本地用户列表、用户组列表及成员、用户创建/删除/禁用/启用、当前登录会话查询等功能。
    /// 底层基于 WMI (Win32_UserAccount, Win32_Group, Win32_LogonSession) 和 net user 命令。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HUserManager
    {

        #region 信息类定义

        /// <summary>本地用户账户信息。</summary>
        public class UserAccountInfo
        {
            /// <summary>用户名</summary>
            public string Name { get; set; }
            /// <summary>全名</summary>
            public string FullName { get; set; }
            /// <summary>描述</summary>
            public string Description { get; set; }
            /// <summary>账户是否禁用（已翻译）</summary>
            public string Disabled { get; set; }
            /// <summary>账户是否锁定（已翻译）</summary>
            public string Lockout { get; set; }
            /// <summary>密码是否永不过期（已翻译）</summary>
            public string PasswordExpires { get; set; }
            /// <summary>SID</summary>
            public string SID { get; set; }
        }

        /// <summary>本地用户组信息。</summary>
        public class GroupInfo
        {
            /// <summary>组名</summary>
            public string Name { get; set; }
            /// <summary>描述</summary>
            public string Description { get; set; }
            /// <summary>组成员（用户名列表）</summary>
            public List<string> Members { get; set; }
        }

        /// <summary>登录会话信息。</summary>
        public class LogonSessionInfo
        {
            /// <summary>会话 ID</summary>
            public string LogonId { get; set; }
            /// <summary>用户名</summary>
            public string UserName { get; set; }
            /// <summary>登录类型（已翻译，如“交互式”）</summary>
            public string LogonType { get; set; }
            /// <summary>身份验证包</summary>
            public string AuthenticationPackage { get; set; }
            /// <summary>开始时间</summary>
            public DateTime StartTime { get; set; }
        }

        #endregion

        #region 用户账户列表

        /// <summary>获取本地所有用户账户信息。</summary>
        /// <returns>用户账户信息列表。</returns>
        public static List<UserAccountInfo> GetLocalUsers()
        {
            var users = new List<UserAccountInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE LocalAccount = TRUE"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var user = new UserAccountInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            FullName = obj["FullName"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? "",
                            SID = obj["SID"]?.ToString() ?? ""
                        };
                        // 账户状态
                        bool disabled = obj["Disabled"] != null && Convert.ToBoolean(obj["Disabled"]);
                        bool locked = obj["Lockout"] != null && Convert.ToBoolean(obj["Lockout"]);
                        bool passwordExpires = obj["PasswordExpires"] != null && Convert.ToBoolean(obj["PasswordExpires"]);

                        user.Disabled = disabled ? HTranslation.GetContent("是") : HTranslation.GetContent("否");
                        user.Lockout = locked ? HTranslation.GetContent("是") : HTranslation.GetContent("否");
                        user.PasswordExpires = passwordExpires ? HTranslation.GetContent("是") : HTranslation.GetContent("否");

                        users.Add(user);
                    }
                }
            }
            catch { }
            return users;
        }

        #endregion

        #region 用户组列表与成员

        /// <summary>获取本地所有用户组及其成员。</summary>
        /// <returns>用户组信息列表。</returns>
        public static List<GroupInfo> GetLocalGroups()
        {
            var groups = new List<GroupInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Group WHERE LocalAccount = TRUE"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string groupName = obj["Name"]?.ToString() ?? "";
                        string groupDescription = obj["Description"]?.ToString() ?? "";

                        var members = new List<string>();
                        // 获取组成员
                        try
                        {
                            using (var memberSearcher = new ManagementObjectSearcher(
                                "SELECT PartComponent FROM Win32_GroupUser WHERE GroupComponent = \"Win32_Group.Domain='" +
                                obj["Domain"] + "',Name='" + groupName + "'\""))
                            {
                                foreach (ManagementObject memberObj in memberSearcher.Get())
                                {
                                    // PartComponent 包含用户路径，如 "Win32_UserAccount.Domain=... Name=..."
                                    string partComponent = memberObj["PartComponent"]?.ToString();
                                    if (partComponent != null)
                                    {
                                        int nameIndex = partComponent.IndexOf("Name=\"");
                                        if (nameIndex >= 0)
                                        {
                                            nameIndex += 6;
                                            int endIndex = partComponent.IndexOf("\"", nameIndex);
                                            if (endIndex >= 0)
                                            {
                                                string memberName = partComponent.Substring(nameIndex, endIndex - nameIndex);
                                                members.Add(memberName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }

                        groups.Add(new GroupInfo
                        {
                            Name = groupName,
                            Description = groupDescription,
                            Members = members
                        });
                    }
                }
            }
            catch { }
            return groups;
        }

        #endregion

        #region 用户创建/删除/禁用（基于 net user 命令）

        /// <summary>创建本地用户账户。</summary>
        /// <param name="userName">用户名。</param>
        /// <param name="password">密码。</param>
        /// <param name="fullName">全名（可选）。</param>
        /// <returns>是否成功。</returns>
        public static bool CreateLocalUser(string userName, string password, string fullName = "")
        {
            try
            {
                string args = "user \"" + userName + "\" \"" + password + "\" /add";
                if (!string.IsNullOrEmpty(fullName))
                    args += " /fullname:\"" + fullName + "\"";
                return RunNetCommand(args);
            }
            catch { return false; }
        }

        /// <summary>删除本地用户账户。</summary>
        /// <param name="userName">用户名。</param>
        /// <returns>是否成功。</returns>
        public static bool DeleteLocalUser(string userName)
        {
            return RunNetCommand("user \"" + userName + "\" /delete");
        }

        /// <summary>禁用本地用户账户。</summary>
        /// <param name="userName">用户名。</param>
        /// <returns>是否成功。</returns>
        public static bool DisableLocalUser(string userName)
        {
            return RunNetCommand("user \"" + userName + "\" /active:no");
        }

        /// <summary>启用本地用户账户。</summary>
        /// <param name="userName">用户名。</param>
        /// <returns>是否成功。</returns>
        public static bool EnableLocalUser(string userName)
        {
            return RunNetCommand("user \"" + userName + "\" /active:yes");
        }

        /// <summary>设置本地用户密码。</summary>
        /// <param name="userName">用户名。</param>
        /// <param name="newPassword">新密码。</param>
        /// <returns>是否成功。</returns>
        public static bool SetLocalUserPassword(string userName, string newPassword)
        {
            return RunNetCommand("user \"" + userName + "\" \"" + newPassword + "\"");
        }

        /// <summary>将用户添加到本地组。</summary>
        /// <param name="userName">用户名。</param>
        /// <param name="groupName">组名。</param>
        /// <returns>是否成功。</returns>
        public static bool AddUserToGroup(string userName, string groupName)
        {
            return RunNetCommand("localgroup \"" + groupName + "\" \"" + userName + "\" /add");
        }

        /// <summary>将用户从本地组中移除。</summary>
        /// <param name="userName">用户名。</param>
        /// <param name="groupName">组名。</param>
        /// <returns>是否成功。</returns>
        public static bool RemoveUserFromGroup(string userName, string groupName)
        {
            return RunNetCommand("localgroup \"" + groupName + "\" \"" + userName + "\" /delete");
        }

        /// <summary>RunNetCommand 方法。</summary>
        private static bool RunNetCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("net.exe", arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return false;
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 当前登录会话

        /// <summary>获取当前登录用户会话信息（基于 Win32_LogonSession 和关联查询）。</summary>
        /// <returns>登录会话信息列表。</returns>
        public static List<LogonSessionInfo> GetLogonSessions()
        {
            var sessions = new List<LogonSessionInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogonSession WHERE LogonType = 2 OR LogonType = 10"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // 逐条容错：单个会话的字段解析失败（如 StartTime 格式异常）不应中断整个枚举
                        try
                        {
                            var session = new LogonSessionInfo
                            {
                                LogonId = obj["LogonId"]?.ToString() ?? "",
                                AuthenticationPackage = obj["AuthenticationPackage"]?.ToString() ?? ""
                            };
                            // StartTime 单独容错：解析失败时回退为本地时间最小值，而不是整个方法抛异常
                            try
                            {
                                string rawStart = obj["StartTime"]?.ToString();
                                session.StartTime = string.IsNullOrEmpty(rawStart)
                                    ? DateTime.MinValue
                                    : ManagementDateTimeConverter.ToDateTime(rawStart);
                            }
                            catch { session.StartTime = DateTime.MinValue; }

                            // 翻译登录类型
                            uint logonType = Convert.ToUInt32(obj["LogonType"] ?? 0);
                            switch (logonType)
                            {
                                case 2: session.LogonType = HTranslation.GetContent("交互式"); break;
                                case 3: session.LogonType = HTranslation.GetContent("网络"); break;
                                case 4: session.LogonType = HTranslation.GetContent("批处理"); break;
                                case 5: session.LogonType = HTranslation.GetContent("服务"); break;
                                case 7: session.LogonType = HTranslation.GetContent("解锁"); break;
                                case 8: session.LogonType = HTranslation.GetContent("网络明文"); break;
                                case 9: session.LogonType = HTranslation.GetContent("新凭证"); break;
                                case 10: session.LogonType = HTranslation.GetContent("远程交互"); break;
                                case 11: session.LogonType = HTranslation.GetContent("缓存交互"); break;
                                default: session.LogonType = HTranslation.GetContent("未知") + " (" + logonType + ")"; break;
                            }

                            // 获取关联的用户名（通过 Win32_LoggedOnUser 或者 Win32_UserAccount 关联）
                            try
                            {
                                using (var userSearcher = new ManagementObjectSearcher(
                                    "ASSOCIATORS OF {Win32_LogonSession.LogonId='" + session.LogonId + "'} WHERE ResultClass = Win32_UserAccount"))
                                {
                                    foreach (ManagementObject userObj in userSearcher.Get())
                                    {
                                        session.UserName = userObj["Name"]?.ToString() ?? "";
                                        break;
                                    }
                                }
                            }
                            catch { }
                            if (string.IsNullOrEmpty(session.UserName))
                                session.UserName = HTranslation.GetContent("未知用户");

                            sessions.Add(session);
                        }
                        catch { /* 跳过单条损坏的会话记录 */ }
                    }
                }
            }
            catch { }
            return sessions;
        }

        #endregion
    }
}
