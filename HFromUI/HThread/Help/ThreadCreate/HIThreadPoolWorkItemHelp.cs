namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// IThreadPoolWorkItem 帮助类：自定义线程池工作项接口。
    /// 【是什么】System.Threading.IThreadPoolWorkItem 只有一个 void Execute() 方法；实现该接口的对象
    /// 可通过 ThreadPool.UnsafeQueueUserWorkItem(item, preferLocal) 直接入队执行，省去 WaitCallback
    /// 委托分配；用 readonly struct 实现时泛型重载按约束特化，热路径上可做到零分配。
    /// 【是否跨进程】否。工作项仍在本进程线程池线程上执行。
    /// 【典型适用场景】高性能基础库（通道、调度器、Actor 框架等）高频投递极小工作项，需要压到零/少分配。
    /// 【使用步骤】
    /// 1) class 或 readonly struct 实现 IThreadPoolWorkItem.Execute()；
    /// 2) 调 ThreadPool.UnsafeQueueUserWorkItem(item, preferLocal: true/false) 入队；
    /// 3) preferLocal=true 倾向当前线程本地队列（缓存友好），false 走全局队列更公平；
    /// 4) Execute 内必须自行 try/catch，未捕获异常与普通线程池回调一样会终止进程。
    /// 【注意事项与坑】
    /// - 该入队方式不流转 ExecutionContext（Unsafe 语义），需要上下文请自行捕获/恢复；
    /// - struct 实现要保证不可变、无引用装箱路径，否则失去零分配意义。
    /// 【版本可用性】（重要）.NET Framework 4.8【不支持】。该接口及 UnsafeQueueUserWorkItem&lt;T&gt; 泛型重载
    /// 仅 .NET Core 3.0 / .NET 5+ 可用（System.Threading.ThreadPool 程序集）。
    /// 因此本类在 net48 下保持为空静态类，真实写法只写在下方 example 注释中，不参与编译；
    /// net48 下请改用 ThreadPool.QueueUserWorkItem（见 HQueueUserWorkItemHelp）。
    /// </summary>
    ///
    /// <example>
    /// 【以下代码仅 .NET Core 3.0 / .NET 5+ 可编译，net48 请只当参考】
    /// <code>
    /// using System.Threading;
    ///
    /// // 1) class 实现：适合携带引用状态
    /// public sealed class MyWorkItem : IThreadPoolWorkItem
    /// {
    ///     private readonly int id;
    ///     public MyWorkItem(int id) { this.id = id; }
    ///
    ///     public void Execute()
    ///     {
    ///         // 该方法在线程池线程上被调用
    ///         System.Console.WriteLine(id);
    ///     }
    /// }
    ///
    /// // 入队（preferLocal=true 走当前线程本地队列）
    /// ThreadPool.UnsafeQueueUserWorkItem(new MyWorkItem(1), preferLocal: true);
    ///
    /// // 2) readonly struct 实现：零分配（泛型重载特化，不装箱）
    /// public readonly struct FastWorkItem : IThreadPoolWorkItem
    /// {
    ///     private readonly int x;
    ///     public FastWorkItem(int x) { this.x = x; }
    ///     public void Execute()
    ///     {
    ///         try { /* 用 x 干活 */ }
    ///         catch { /* 异常不能逃出 Execute */ }
    ///     }
    /// }
    ///
    /// ThreadPool.UnsafeQueueUserWorkItem(new FastWorkItem(42), preferLocal: false);
    /// </code>
    /// 说明：Unsafe 版本不流转 ExecutionContext，需要安全/逻辑上下文时请改用
    /// QueueUserWorkItem(WaitCallback, object)（net48 也有，但需要一次委托分配）。
    /// </example>
    public static class HIThreadPoolWorkItemHelp
    {
        /// <summary>最低可用版本提示：仅 .NET Core 3.0 / .NET 5+ 支持 IThreadPoolWorkItem，net48 不支持。</summary>
        private const string AvailabilityNote = ".NET Core 3.0+ / .NET 5+ only; not available on .NET Framework 4.8";

        /// <summary>net48 下的替代入口说明：请改用 ThreadPool.QueueUserWorkItem（HQueueUserWorkItemHelp 类）。</summary>
        private const string Net48Alternative = "HFromUI.HThread.Help.ThreadCreate.HQueueUserWorkItemHelp";

        // 本类故意不放任何真实示例方法：IThreadPoolWorkItem 在 net48 不存在，
        // 可执行替代写法见 HQueueUserWorkItemHelp；升级到 .NET 6+ 后可直接复制上方 example 中的代码使用。
    }
}
