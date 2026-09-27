using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 网络共享与会话管理工具类（集成多语言翻译，已修复SDDL错误）。
    /// 提供共享文件夹创建/删除/权限设置、连接会话列表、网络会话列表、打开文件列表等功能。
    /// 底层基于 NetAPI32.dll 和 Win32 API，需要管理员权限。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HNetworkShare
    {

        #region 信息类定义

        /// <summary>共享文件夹信息。</summary>
        public class ShareInfo
        {
            /// <summary>ShareName 成员。</summary>
            public string ShareName { get; set; }
            /// <summary>Path 成员。</summary>
            public string Path { get; set; }
            /// <summary>Remark 成员。</summary>
            public string Remark { get; set; }
            /// <summary>ShareType 成员。</summary>
            public string ShareType { get; set; }
            /// <summary>MaxUses 成员。</summary>
            public int MaxUses { get; set; }
            /// <summary>CurrentUses 成员。</summary>
            public int CurrentUses { get; set; }
        }

        /// <summary>连接到共享的会话信息。</summary>
        public class ShareConnectionInfo
        {
            /// <summary>ConnectionId 成员。</summary>
            public int ConnectionId { get; set; }
            /// <summary>Type 成员。</summary>
            public int Type { get; set; }
            /// <summary>NumOpens 成员。</summary>
            public int NumOpens { get; set; }
            /// <summary>NumUsers 成员。</summary>
            public int NumUsers { get; set; }
            /// <summary>Time 成员。</summary>
            public int Time { get; set; }
            /// <summary>UserName 成员。</summary>
            public string UserName { get; set; }
            /// <summary>NetName 成员。</summary>
            public string NetName { get; set; }
        }

        /// <summary>网络会话信息。</summary>
        public class NetworkSessionInfo
        {
            /// <summary>ClientName 成员。</summary>
            public string ClientName { get; set; }
            /// <summary>UserName 成员。</summary>
            public string UserName { get; set; }
            /// <summary>IdleTime 成员。</summary>
            public int IdleTime { get; set; }
            /// <summary>SessionTime 成员。</summary>
            public int SessionTime { get; set; }
            /// <summary>NumOpens 成员。</summary>
            public int NumOpens { get; set; }
            /// <summary>HostName 成员。</summary>
            public string HostName { get; set; }
        }

        /// <summary>远程打开的文件信息。</summary>
        public class OpenFileInfo
        {
            /// <summary>FileId 成员。</summary>
            public int FileId { get; set; }
            /// <summary>UserName 成员。</summary>
            public string UserName { get; set; }
            /// <summary>FilePath 成员。</summary>
            public string FilePath { get; set; }
            /// <summary>Permissions 成员。</summary>
            public int Permissions { get; set; }
            /// <summary>NumLocks 成员。</summary>
            public int NumLocks { get; set; }
        }

        #endregion

        #region 共享枚举与管理

        /// <summary>获取本地计算机上的所有共享文件夹信息。</summary>
        public static List<ShareInfo> GetShares()
        {
            var list = new List<ShareInfo>();
            IntPtr bufPtr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries, resumeHandle = 0;
                int ret = NetShareEnum(null, 2, out bufPtr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);
                if (ret == 0 || ret == ERROR_MORE_DATA)
                {
                    IntPtr currentPtr = bufPtr;
                    for (int i = 0; i < entriesRead; i++)
                    {
                        SHARE_INFO_2 info = (SHARE_INFO_2)Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_2));
                        list.Add(new ShareInfo
                        {
                            ShareName = info.shi2_netname,
                            Path = info.shi2_path,
                            Remark = info.shi2_remark,
                            MaxUses = info.shi2_max_uses,
                            CurrentUses = info.shi2_current_uses,
                            ShareType = TranslateShareType(info.shi2_type)
                        });
                        currentPtr = IntPtr.Add(currentPtr, Marshal.SizeOf(typeof(SHARE_INFO_2)));
                    }
                }
            }
            catch { }
            finally
            {
                if (bufPtr != IntPtr.Zero) NetApiBufferFree(bufPtr);
            }
            return list;
        }

        /// <summary>创建共享文件夹（基础磁盘共享）。</summary>
        public static bool CreateShare(string shareName, string path, string remark, int maxUsers = -1)
        {
            SHARE_INFO_2 info = new SHARE_INFO_2
            {
                shi2_netname = shareName,
                shi2_type = STYPE_DISKTREE,
                shi2_remark = remark,
                shi2_permissions = ACCESS_ALL,
                shi2_max_uses = maxUsers,
                shi2_current_uses = 0,
                shi2_path = path,
                shi2_passwd = null
            };
            int ret = NetShareAdd(null, 2, ref info, out int paramErr);
            return ret == 0;
        }

        /// <summary>删除共享文件夹。</summary>
        public static bool DeleteShare(string shareName)
        {
            int ret = NetShareDel(null, shareName, 0);
            return ret == 0;
        }

        #endregion

        #region 共享权限设置（修正后）

        /// <summary>
        /// 设置共享文件夹的安全描述符（权限）。
        /// </summary>
        /// <param name="shareName">共享名称。</param>
        /// <param name="securityDescriptor">要应用的 CommonSecurityDescriptor 对象。</param>
        /// <returns>是否设置成功。</returns>
        public static bool SetShareSecurityDescriptor(string shareName, CommonSecurityDescriptor securityDescriptor)
        {
            // ConvertStringSecurityDescriptorToSecurityDescriptor 分配的内存按 MSDN 要求
            // 必须由调用方 LocalFree 释放（NetShareSetInfo 只读取该结构，不会接管内存）。
            // 原实现完全未释放，每次调用泄漏一块安全描述符内存。
            IntPtr pSecurityDescriptor = IntPtr.Zero;
            try
            {
                // 将 CommonSecurityDescriptor 转换为 SDDL 字符串
                string sddl = securityDescriptor.GetSddlForm(AccessControlSections.All);

                // 将 SDDL 字符串转换为安全描述符指针
                if (!ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, SDDL_REVISION_1, out pSecurityDescriptor, out _))
                    return false;

                // 准备 SHARE_INFO_502 结构
                SHARE_INFO_502 info = new SHARE_INFO_502
                {
                    shi502_netname = shareName,
                    shi502_security_descriptor = pSecurityDescriptor
                };

                // 调用 NetShareSetInfo 设置 502 级别信息
                int ret = NetShareSetInfo(null, shareName, 502, ref info, out _);
                return ret == 0;
            }
            catch { return false; }
            finally
            {
                if (pSecurityDescriptor != IntPtr.Zero)
                {
                    LocalFree(pSecurityDescriptor);
                }
            }
        }

        #endregion

        #region 共享连接列表

        /// <summary>获取连接到指定共享的会话列表。</summary>
        public static List<ShareConnectionInfo> GetShareConnections(string shareName)
        {
            var list = new List<ShareConnectionInfo>();
            IntPtr bufPtr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries, resumeHandle = 0;
                int ret = NetConnectionEnum(null, shareName, 1, out bufPtr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);
                if (ret == 0 || ret == ERROR_MORE_DATA)
                {
                    IntPtr currentPtr = bufPtr;
                    for (int i = 0; i < entriesRead; i++)
                    {
                        CONNECTION_INFO_1 info = (CONNECTION_INFO_1)Marshal.PtrToStructure(currentPtr, typeof(CONNECTION_INFO_1));
                        list.Add(new ShareConnectionInfo
                        {
                            ConnectionId = info.coni1_id,
                            Type = info.coni1_type,
                            NumOpens = info.coni1_num_opens,
                            NumUsers = info.coni1_num_users,
                            Time = info.coni1_time,
                            UserName = info.coni1_username,
                            NetName = info.coni1_netname
                        });
                        currentPtr = IntPtr.Add(currentPtr, Marshal.SizeOf(typeof(CONNECTION_INFO_1)));
                    }
                }
            }
            catch { }
            finally { if (bufPtr != IntPtr.Zero) NetApiBufferFree(bufPtr); }
            return list;
        }

        #endregion

        #region 网络会话列表

        /// <summary>获取当前与本地计算机的所有网络会话。</summary>
        public static List<NetworkSessionInfo> GetNetworkSessions()
        {
            var list = new List<NetworkSessionInfo>();
            IntPtr bufPtr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries, resumeHandle = 0;
                int ret = NetSessionEnum(null, null, null, 502, out bufPtr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);
                if (ret == 0 || ret == ERROR_MORE_DATA)
                {
                    IntPtr currentPtr = bufPtr;
                    for (int i = 0; i < entriesRead; i++)
                    {
                        SESSION_INFO_502 info = (SESSION_INFO_502)Marshal.PtrToStructure(currentPtr, typeof(SESSION_INFO_502));
                        list.Add(new NetworkSessionInfo
                        {
                            ClientName = info.sesi502_cname,
                            UserName = info.sesi502_username,
                            IdleTime = info.sesi502_idle_time,
                            SessionTime = info.sesi502_time,
                            NumOpens = info.sesi502_num_opens,
                            HostName = info.sesi502_cltype_name
                        });
                        currentPtr = IntPtr.Add(currentPtr, Marshal.SizeOf(typeof(SESSION_INFO_502)));
                    }
                }
            }
            catch { }
            finally { if (bufPtr != IntPtr.Zero) NetApiBufferFree(bufPtr); }
            return list;
        }

        #endregion

        #region 远程打开文件列表

        /// <summary>获取当前远程打开的文件列表（需要管理员权限）。</summary>
        public static List<OpenFileInfo> GetOpenFiles()
        {
            var list = new List<OpenFileInfo>();
            IntPtr bufPtr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries, resumeHandle = 0;
                int ret = NetFileEnum(null, null, null, 3, out bufPtr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);
                if (ret == 0 || ret == ERROR_MORE_DATA)
                {
                    IntPtr currentPtr = bufPtr;
                    for (int i = 0; i < entriesRead; i++)
                    {
                        FILE_INFO_3 info = (FILE_INFO_3)Marshal.PtrToStructure(currentPtr, typeof(FILE_INFO_3));
                        list.Add(new OpenFileInfo
                        {
                            FileId = info.fi3_id,
                            UserName = info.fi3_username,
                            FilePath = info.fi3_pathname,
                            Permissions = info.fi3_permissions,
                            NumLocks = info.fi3_num_locks
                        });
                        currentPtr = IntPtr.Add(currentPtr, Marshal.SizeOf(typeof(FILE_INFO_3)));
                    }
                }
            }
            catch { }
            finally { if (bufPtr != IntPtr.Zero) NetApiBufferFree(bufPtr); }
            return list;
        }

        #endregion

        #region 辅助方法

        /// <summary>TranslateShareType 方法。</summary>
        private static string TranslateShareType(uint type)
        {
            if ((type & STYPE_DISKTREE) != 0) return HTranslation.GetContent("磁盘驱动器");
            if ((type & STYPE_PRINTQ) != 0) return HTranslation.GetContent("打印队列");
            if ((type & STYPE_DEVICE) != 0) return HTranslation.GetContent("通信设备");
            if ((type & STYPE_IPC) != 0) return HTranslation.GetContent("IPC");
            if ((type & STYPE_SPECIAL) != 0) return HTranslation.GetContent("特殊共享");
            return HTranslation.GetContent("未知");
        }

        #endregion

        #region 常量与 Win32 API

        private const int MAX_PREFERRED_LENGTH = -1;
        private const int ERROR_MORE_DATA = 234;
        private const uint STYPE_DISKTREE = 0;
        private const uint STYPE_PRINTQ = 1;
        private const uint STYPE_DEVICE = 2;
        private const uint STYPE_IPC = 3;
        private const uint STYPE_SPECIAL = 0x80000000;
        private const uint ACCESS_ALL = 0xFFFFFFFF;
        private const int SDDL_REVISION_1 = 1;

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(string servername, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, ref int resume_handle);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareAdd(string servername, int level, ref SHARE_INFO_2 buf, out int parm_err);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareDel(string servername, string netname, int reserved);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareSetInfo(string servername, string netname, int level, ref SHARE_INFO_502 buf, out int parm_err);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetConnectionEnum(string servername, string qualifier, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, ref int resume_handle);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetSessionEnum(string servername, string UncClientName, string username, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, ref int resume_handle);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetFileEnum(string servername, string basepath, string username, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, ref int resume_handle);

        [DllImport("netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string StringSecurityDescriptor, int StringSDRevision, out IntPtr SecurityDescriptor, out uint SecurityDescriptorSize);

        /// <summary>释放 LocalAlloc/ConvertStringSecurityDescriptorToSecurityDescriptor 分配的内存。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHARE_INFO_2
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string shi2_netname;
            public uint shi2_type;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi2_remark;
            public uint shi2_permissions;
            public int shi2_max_uses;
            public int shi2_current_uses;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi2_path;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi2_passwd;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHARE_INFO_502
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string shi502_netname;
            public uint shi502_type;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi502_remark;
            public uint shi502_permissions;
            public int shi502_max_uses;
            public int shi502_current_uses;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi502_path;
            [MarshalAs(UnmanagedType.LPWStr)] public string shi502_passwd;
            public IntPtr shi502_security_descriptor;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CONNECTION_INFO_1
        {
            public int coni1_id;
            public int coni1_type;
            public int coni1_num_opens;
            public int coni1_num_users;
            public int coni1_time;
            [MarshalAs(UnmanagedType.LPWStr)] public string coni1_username;
            [MarshalAs(UnmanagedType.LPWStr)] public string coni1_netname;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SESSION_INFO_502
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string sesi502_cname;
            [MarshalAs(UnmanagedType.LPWStr)] public string sesi502_username;
            public int sesi502_idle_time;
            public int sesi502_time;
            public int sesi502_num_opens;
            public int sesi502_flags;
            [MarshalAs(UnmanagedType.LPWStr)] public string sesi502_cltype_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct FILE_INFO_3
        {
            public int fi3_id;
            public int fi3_permissions;
            public int fi3_num_locks;
            [MarshalAs(UnmanagedType.LPWStr)] public string fi3_pathname;
            [MarshalAs(UnmanagedType.LPWStr)] public string fi3_username;
        }

        #endregion
    }
}
