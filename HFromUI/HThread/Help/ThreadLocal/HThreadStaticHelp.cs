using System;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadLocal
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】[ThreadStatic] 特性帮助类：标记在【静态字段】上后，CLR 为每个托管线程分配该字段的
    /// 独立存储槽——同一字段，各线程读到/写入的值互不影响。底层是 TLS（Thread Local Storage）槽位，
    /// 无额外对象、无锁，是三种线程隔离手段（[ThreadStatic] / ThreadLocal&lt;T&gt; / AsyncLocal&lt;T&gt;）中
    /// 最轻最快的一种。
    ///
    /// 【是否跨进程】否；连【跨线程】都否——纯按操作系统/托管线程隔离，线程结束其槽位随之消失。
    ///
    /// 【典型适用场景】
    /// 1) 高频路径上的每线程缓冲区、StringBuilder/数组复用（避免每次调用分配）；
    /// 2) 非线程安全旧对象的线程隔离（.NET Framework 的 Random、旧式格式化上下文、第三方非线程安全组件）；
    /// 3) 深度调用链上“隐式传递”每线程状态（深度调用栈参数透传太繁琐时的折中）。
    ///
    /// 【使用步骤】
    /// 1) 声明 static 字段并加 [ThreadStatic]；
    /// 2) 不要写有意义的字段初始化器（见坑1）；
    /// 3) 每次访问前判默认值（引用类型判 null、值类型判 0/false），在本线程内懒初始化；
    /// 4) 需要清理时显式写回默认值（线程池线程会被复用，见坑3）。
    ///
    /// 【注意事项与坑】
    /// 1) 初始化器只执行一次：[ThreadStatic] static int[] Buf = new int[16]; 中的 new 只在
    ///    【类型初始化所在的那一个线程】执行，其他线程拿到的全是 null/0——必须每线程懒初始化；
    /// 2) 只能用于 static 字段：实例字段上加 [ThreadStatic] 能编译但【没有任何线程隔离效果】，
    ///    是常见误用（实例级线程本地存储要用 ThreadLocal&lt;T&gt;）；
    /// 3) 线程池复用残留：线程池线程执行完任务 A 后会被任务 B 租借，[ThreadStatic] 值原样保留，
    ///    任务开始时要显式重置，否则会读到上一个任务的数据；
    /// 4) 不随 async/await 流转：await 后续体可能换到另一线程，槽位是新值；
    ///    需要“随异步流传递”请用 AsyncLocal&lt;T&gt;（见 HAsyncLocalHelp）；
    /// 5) 线程上的槽位值不会被主动 Dispose：若存放 IDisposable 对象，需自行设计清理时机；
    /// 6) 动态数量、需要枚举所有线程值时改用 ThreadLocal&lt;T&gt;（见 HThreadLocalHelp）。
    ///
    /// 【版本可用性】[ThreadStatic]（System.ThreadStaticAttribute）.NET Framework 1.1+ 即有，net48 可用。
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// [ThreadStatic] private static StringBuilder perThreadBuilder;   // 不写初始化器
    ///
    /// private static StringBuilder Builder
    /// {
    ///     get
    ///     {
    ///         if (perThreadBuilder == null)
    ///         {
    ///             perThreadBuilder = new StringBuilder();   // 每个线程各自 new 一次
    ///         }
    ///         return perThreadBuilder;
    ///     }
    /// }
    /// </code>
    /// 以下写法【错误】（实例字段无效，所有实例共享同一值）：
    /// <code>
    /// // [ThreadStatic] private int counter;   // 编译得过，但毫无隔离作用
    /// </code>
    /// </example>
    public static class HThreadStaticHelp
    {
        /// <summary>每个线程各存一份 int 编号（不写初始化器）。</summary>
        [ThreadStatic]
        private static int perThreadId;

        /// <summary>每个线程各存一份 int[] 缓冲区（引用类型必须每线程懒初始化）。</summary>
        [ThreadStatic]
        private static int[] perThreadBuffer;

        /// <summary>
        /// 故意带初始化器的 [ThreadStatic] 字段：仅类型初始化所在线程看到“已初始化”，
        /// 其他线程读到 null。用于演示初始化器陷阱。
        /// </summary>
        [ThreadStatic]
        private static string initializedOnce = HTranslation.GetContent("仅初始化线程可见");

        /// <summary>非线程安全对象（.NET Framework 的 Random）做线程隔离的示例槽位。</summary>
        [ThreadStatic]
        private static Random threadRandom;

        /// <summary>
        /// 示例1：每个线程读写自己的编号，互不影响。
        /// </summary>
        public static int CurrentThreadId
        {
            get { return perThreadId; }
            set { perThreadId = value; }   // 只影响当前线程这一份
        }

        /// <summary>
        /// 示例2：引用类型必须在各线程内懒初始化（初始化器不跨线程执行）。
        /// </summary>
        /// <returns>当前线程专属的 16 元素缓冲区。</returns>
        public static int[] GetBuffer()
        {
            if (perThreadBuffer == null)
            {
                perThreadBuffer = new int[16];   // 每个线程第一次用时各自 new 一个
            }

            return perThreadBuffer;
        }

        /// <summary>
        /// 示例3：读取带初始化器的 [ThreadStatic] 字段——在“非初始化线程”上得到 null。
        /// </summary>
        /// <returns>当前线程看到的字段值（多数线程上为 null）。</returns>
        public static string ReadInitializerValue()
        {
            // 除类型初始化碰巧发生在当前线程外，这里都是 null
            return initializedOnce;
        }

        /// <summary>
        /// 示例4：典型场景——.NET Framework 的 Random 非线程安全，用 [ThreadStatic] 为每线程发一个实例。
        /// </summary>
        public static Random Random
        {
            get
            {
                if (threadRandom == null)
                {
                    // 不用时间种子（多线程会撞种子产生相同序列）；由 Guid 派生各线程独立种子
                    threadRandom = new Random(Guid.NewGuid().GetHashCode());
                }

                return threadRandom;
            }
        }

        /// <summary>
        /// 示例5：线程池复用清理——租借任务开始时把本线程槽位恢复默认，避免读到上一任务的残留。
        /// </summary>
        public static void ResetForNewThreadPoolWorkItem()
        {
            perThreadId = 0;
            perThreadBuffer = null;
            threadRandom = null;
            // initializedOnce 也被清成 null，且永远不会重新变成 "仅初始化线程可见"——再次印证坑1
            initializedOnce = null;
        }

        /// <summary>
        /// 示例6：隔离性自测——两个线程池线程分别写入不同编号，各自读到的都是自己写入的值，
        /// 不依赖 UI/网络，毫秒级完成。
        /// </summary>
        /// <returns>形如 "线程A=101, 线程B=202" 的结果描述（两个值必然独立）。</returns>
        public static async Task<string> IsolationDemoAsync()
        {
            // Task.Run 在线程池线程上执行；两个任务大概率落在不同线程（即使同线程也不影响结论：
            // 若串行复用同线程，第二个任务显式覆盖后读到的是自己写入的值）
            Task<int> a = Task.Run(delegate
            {
                perThreadId = 101;
                return perThreadId;
            });
            Task<int> b = Task.Run(delegate
            {
                perThreadId = 202;
                return perThreadId;
            });

            await Task.WhenAll(a, b).ConfigureAwait(false);
            return HTranslation.GetContent("线程A=") + a.Result + HTranslation.GetContent(", 线程B=") + b.Result;
        }
    }
}
