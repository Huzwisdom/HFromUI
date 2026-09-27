namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】内核同步对象的 Windows 访问控制（ACL/DACL）帮助类：注释类，无真实示例方法。
    /// System.Security.AccessControl 命名空间下为三类命名内核对象提供了对应的安全描述符包装：
    /// MutexSecurity（互斥体）、EventWaitHandleSecurity（自动/手动重置事件）、SemaphoreSecurity（信号量），
    /// 配套的访问规则类型分别是 MutexAccessRule、EventWaitHandleAccessRule、SemaphoreAccessRule，
    /// 权限枚举分别为 MutexRights、EventWaitHandleRights、SemaphoreRights（常用 Modify/Synchronize/FullControl）。
    /// 这些类型用于在【创建】命名内核对象时挂上自定义 DACL（自由访问控制列表），
    /// 精确控制哪些 Windows 用户/用户组可以 OpenExisting、等待、修改该对象。
    ///
    /// 【是否跨进程】是，且这正是本主题存在的意义：命名内核对象由操作系统内核按对象名管理，
    /// 可跨进程、跨会话（会话 0 服务与登录用户桌面之间）甚至在权限允许时跨用户共享，
    /// DACL 决定了“谁有资格按名字找到并操作它”。
    ///
    /// 【典型适用场景】
    /// 1) Windows 服务（以 SYSTEM 或专用服务账户运行）创建 Global\ 命名 Mutex/Event/Semaphore，
    ///    需要普通权限的用户态程序按名 OpenExisting 后等待/通知；
    /// 2) 不同权限级别（提权进程与普通进程）之间用命名内核对象协同，必须显式放行低权限一方；
    /// 3) 需要收紧权限：只允许特定用户组（而非 Everyone）访问同步对象，防止恶意进程抢占/DoS。
    ///
    /// 【使用步骤】
    /// 1) using System.Security.AccessControl; using System.Security.Principal; using System.Threading;
    /// 2) new MutexSecurity()/EventWaitHandleSecurity()/SemaphoreSecurity()；
    /// 3) 用 SecurityIdentifier（如 WellKnownSidType.WorldSid 代表 Everyone，或按组/用户构造 SID）
    ///    + *AccessRule + AddAccessRule 配置允许规则（也可 AddAccessRule(Deny) 拒绝）；
    /// 4) 创建对象时把安全对象传进构造函数：
    ///    new Mutex(bool initiallyOwned, string name, out bool createdNew, MutexSecurity security)；
    ///    EventWaitHandle/Semaphore 同样有带 *Security 参数的构造重载；
    /// 5) 另一方用 Mutex.OpenExisting(name, rights)/EventWaitHandle.OpenExisting/Semaphore.OpenExisting
    ///    按名打开（最好只申请所需最小权限）。
    ///
    /// 【注意事项与坑】
    /// 1) 经典故障：服务程序创建命名对象后，普通权限程序 OpenExisting 抛
    ///    <see cref="T:System.UnauthorizedAccessException"/>。原因：对象由高权限账户创建，默认 DACL 只向
    ///    创建者/SYSTEM/Administrators 授权，普通用户不在名单里——【不是名字写错】（名字错抛
    ///    WaitHandleCannotBeOpenedException），而是安全描述符不放行。解决：创建时显式传入
    ///    *Security 并 AddAccessRule 放行目标用户/用户组；
    /// 2) 名字前缀决定作用域：Local\（默认前缀，等价于不带前缀）按会话隔离；Global\ 跨会话、
    ///    可供服务（会话 0）与用户桌面共享，但在 Global\ 命名空间创建对象需要
    ///    SeCreateGlobalPrivilege（服务账户默认有，普通用户默认没有）；
    /// 3) 同名对象被别人先创建时，out createdNew 返回 false，此时你拿到的是【别人对象的句柄】，
    ///    访问权以对方的 DACL 为准；需要判断“创建者身份”时务必检查 createdNew；
    /// 4) Mutex 只授予 Synchronize 不够释放互斥体，通常需要 Modify（或 FullControl）；
    ///    权限不足时 OpenExisting 成功也可能在 WaitOne/ReleaseMutex 时抛 UnauthorizedAccessException；
    /// 5) 只在 Windows 上有效；非 Windows 平台这些 ACL API 不可用或无意义；
    /// 6) 该机制只管“访问权限”，不保证业务正确性：Everyone 可写的 Mutex 仍可能被无关进程占用/放弃，
    ///    得到 AbandonedMutexException，名字要足够独特避免冲突。
    ///
    /// 【版本可用性】.NET Framework 4.8 下这些 ACL 类型内置在 mscorlib.dll/System.dll 中，直接可用。
    /// .NET Core/.NET 5+ 中已从核心库移出：需安装 NuGet 包 System.Threading.AccessControl，
    /// 并改用扩展类 MutexAcl/SemaphoreAcl/EventWaitHandleAcl（Create/TryOpenExisting/OpenExisting 扩展方法），
    /// 原 *Security/*AccessRule 类型在该包中提供。
    /// </summary>
    ///
    /// <example>
    /// 示例1：net48 下创建带自定义 DACL 的全局命名 Mutex，放行 Everyone 后跨用户打开。
    /// <code>
    /// using System.Security.AccessControl;
    /// using System.Security.Principal;
    /// using System.Threading;
    ///
    /// string mutexName = @"Global\HuzwisdomDemoMutex";
    ///
    /// // 1) 配置安全描述符：允许 Everyone 修改/等待该 Mutex
    /// MutexSecurity security = new MutexSecurity();
    /// SecurityIdentifier everyoneSid =
    ///     new SecurityIdentifier(WellKnownSidType.WorldSid, null);
    /// security.AddAccessRule(new MutexAccessRule(
    ///     everyoneSid,
    ///     MutexRights.Modify | MutexRights.Synchronize,
    ///     AccessControlType.Allow));
    ///
    /// // 2) 创建时挂上 DACL；createdNew 指示是否由本进程首次创建
    /// bool createdNew;
    /// using (Mutex mutex = new Mutex(false, mutexName, out createdNew, security))
    /// {
    ///     if (mutex.WaitOne(1000))
    ///     {
    ///         try
    ///         {
    ///             // 跨进程、跨会话的临界区
    ///         }
    ///         finally
    ///         {
    ///             mutex.ReleaseMutex();
    ///         }
    ///     }
    /// }
    ///
    /// // 3) 普通权限程序按名打开（权限不足会抛 UnauthorizedAccessException）
    /// using (Mutex opened = Mutex.OpenExisting(
    ///            mutexName, MutexRights.Modify | MutexRights.Synchronize))
    /// {
    ///     opened.WaitOne(1000);
    ///     opened.ReleaseMutex();
    /// }
    /// </code>
    ///
    /// 示例2：EventWaitHandle / Semaphore 的安全对象完全同构。
    /// <code>
    /// EventWaitHandleSecurity eventSecurity = new EventWaitHandleSecurity();
    /// eventSecurity.AddAccessRule(new EventWaitHandleAccessRule(
    ///     new SecurityIdentifier(WellKnownSidType.WorldSid, null),
    ///     EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize,
    ///     AccessControlType.Allow));
    ///
    /// bool eventCreated;
    /// using (EventWaitHandle ready = new EventWaitHandle(
    ///           false, EventResetMode.ManualReset,
    ///           @"Global\HuzwisdomReadyEvent", out eventCreated, eventSecurity))
    /// {
    ///     ready.Set();
    /// }
    ///
    /// SemaphoreSecurity semSecurity = new SemaphoreSecurity();
    /// semSecurity.AddAccessRule(new SemaphoreAccessRule(
    ///     new SecurityIdentifier(WellKnownSidType.WorldSid, null),
    ///     SemaphoreRights.Modify | SemaphoreRights.Synchronize,
    ///     AccessControlType.Allow));
    ///
    /// bool semCreated;
    /// using (Semaphore slots = new Semaphore(
    ///           2, 2, @"Global\HuzwisdomSlots", out semCreated, semSecurity))
    /// {
    ///     slots.WaitOne();
    ///     slots.Release();
    /// }
    /// </code>
    ///
    /// 示例3：.NET Core/.NET 5+ 需先安装 NuGet 包 System.Threading.AccessControl，改用 *Acl 扩展。
    /// <code>
    /// // NuGet: Install-Package System.Threading.AccessControl
    /// using System.Security.AccessControl;
    /// using System.Security.Principal;
    /// using System.Threading;
    ///
    /// MutexSecurity security = new MutexSecurity();
    /// security.AddAccessRule(new MutexAccessRule(
    ///     new SecurityIdentifier(WellKnownSidType.WorldSid, null),
    ///     MutexRights.FullControl,
    ///     AccessControlType.Allow));
    ///
    /// bool createdNew;
    /// using (Mutex mutex = MutexAcl.Create(
    ///           false, @"Global\HuzwisdomDemoMutex", out createdNew, security))
    /// {
    ///     // SemaphoreAcl.Create / EventWaitHandleAcl.Create 形态相同
    /// }
    /// </code>
    /// </example>
    public static class HKernelObjectSecurityHelp
    {
        /// <summary>net48 可用性提示：MutexSecurity/EventWaitHandleSecurity/SemaphoreSecurity 内置在 mscorlib.dll/System.dll。</summary>
        private const string Net48Availability = ".NET Framework 4.8: built-in (mscorlib.dll / System.dll)";

        /// <summary>.NET Core/.NET 5+ 迁移提示：ACL API 移至 NuGet 包 System.Threading.AccessControl。</summary>
        private const string CorePackageName = "System.Threading.AccessControl";

        /// <summary>.NET Core/.NET 5+ 的入口扩展类：MutexAcl、SemaphoreAcl、EventWaitHandleAcl。</summary>
        private const string CoreAclClasses = "MutexAcl / SemaphoreAcl / EventWaitHandleAcl";

        /// <summary>典型异常提示：OpenExisting 抛 UnauthorizedAccessException 是 DACL 未放行，而非对象名错误。</summary>
        private const string AccessDeniedNote =
            "UnauthorizedAccessException from OpenExisting means the named object's DACL does not grant the caller; " +
            "a wrong name throws WaitHandleCannotBeOpenedException instead.";

        // 本类故意不放任何真实示例方法：
        // 1) 创建内核对象会在操作系统全局命名空间留下痕迹、需要特定 Windows 权限与账户环境，无法自包含自测；
        // 2) 完整可复制写法见上方三段 example（net48 构造重载 + Event/Semaphore 同构示例 + .NET 5+ 的 *Acl 扩展）；
        // 3) 跨进程命名对象本身的互斥/通知用法见 HMutexHelp、HSemaphoreHelp 及 Signaling 目录下事件帮助类。
    }
}
