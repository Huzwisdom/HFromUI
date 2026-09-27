using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadLocal
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="AsyncLocal{T}"/> 帮助类：随【异步控制流】（ExecutionContext）流动的环境变量。
    /// 值不存在 TLS 槽里，而存在当前线程的 <see cref="ExecutionContext"/> 中：await 挂起/恢复、Task.Run、
    /// ThreadPool.QueueUserWorkItem、new Thread 等发生 ExecutionContext 流转时，值会随【不可变快照的副本】
    /// 一起到达续体所在线程，因此 await 后即使换了线程，读出来的仍是逻辑调用链上的值。
    /// 核心成员：Value（读写当前异步流的值）；构造函数
    /// new AsyncLocal&lt;T&gt;(Action&lt;AsyncLocalValueChangedArgs&lt;T&gt;&gt;) 可注册变更通知，
    /// 回调参数含 PreviousValue / CurrentValue / ThreadContextChanged（true=因跨线程上下文切换观察到变更，
    /// false=同一线程上被显式赋值）。
    ///
    /// 【写时复制父子隔离（关键语义）】子异步流第一次【修改】值时，框架复制一份 ExecutionContext，
    /// 修改只对子流及其下游可见，【不会反向流回父流】；但父流在启动子流【之前】赋的值，子流看得到。
    /// 要让子流向父流“回传”数据，可变内容用引用类型对象（父子持有同一引用），或显式传参。
    ///
    /// 【是否跨进程】否；跨线程/跨 await 可见（在同一逻辑异步流内）。
    ///
    /// 【典型适用场景】
    /// 1) 日志 TraceId/CorrelationId：入口赋值，深层方法直接读取，无需层层传参；
    /// 2) 请求级环境数据（当前用户、截止时间、灰度标记），随 async 调用链下传；
    /// 3) 诊断/审计：用变更回调观察 ExecutionContext 在线程间的传播。
    ///
    /// 【使用步骤】
    /// 1) 静态或实例字段持有 AsyncLocal&lt;T&gt;（通常 static readonly）；
    /// 2) 在异步流入口赋 Value；
    /// 3) 下游任意深度、任意线程上 await 之后直接读 Value；
    /// 4) 需要观察传播时用带回调的构造函数。
    ///
    /// 【注意事项与坑】
    /// 1) 不要拿它当“全局变量”乱用：值随 ExecutionContext 复制，线程池中每次流转都有复制成本，
    ///    且槽位数量越多开销越大；
    /// 2) 子流修改不外泄父流（写时复制）；需要共享可变状态请用引用类型或并发集合；
    /// 3) 引用类型值的引用随流复制，但指向的对象只有一个——下游修改对象内容会影响所有流（无隔离）；
    /// 4) ExecutionContext.SuppressFlow() 期间启动的异步工作不接收环境数据（高级用法，少用，
    ///    且必须用 Undo/AsyncFlowControl 还原）；
    /// 5) 与 ThreadLocal/[ThreadStatic] 区别：后两者按物理线程隔离、await 换线程即“丢值”；
    ///    AsyncLocal 按逻辑异步流隔离、会跨线程跟随。
    ///
    /// 【版本可用性】<see cref="AsyncLocal{T}"/> 为 .NET Framework 4.6+（net48 可用）。
    /// 【文档核查更正】带变更回调的构造函数 AsyncLocal&lt;T&gt;(Action&lt;AsyncLocalValueChangedArgs&lt;T&gt;&gt;)
    /// 与 <see cref="AsyncLocalValueChangedArgs{T}"/>（PreviousValue/CurrentValue/ThreadContextChanged）
    /// 在 .NET Framework 4.6/4.6.1/4.6.2/4.7/4.7.1/4.7.2/4.8 中【均已提供】（已对照
    /// learn.microsoft.com netframework-4.8 文档及本机 mscorlib 4.0.30319 核实），并非 .NET Core 独有；
    /// 本类同时给出“真实回调”和“显式取值对比”两种观察方式。
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// private static readonly AsyncLocal&lt;string&gt; TraceId = new AsyncLocal&lt;string&gt;();
    /// TraceId.Value = Guid.NewGuid().ToString("N");   // 请求入口
    /// await Task.Run(() =&gt; Log.Info(TraceId.Value)); // 换线程仍可读
    /// </code>
    /// </example>
    public static class HAsyncLocalHelp
    {
        /// <summary>全链路 TraceId 槽（静态只读，值随 ExecutionContext 流动）。</summary>
        private static readonly AsyncLocal<string> traceId = new AsyncLocal<string>();

        /// <summary>
        /// 示例1：设置后整条异步调用链都能读到（即使中途切换线程池线程）。
        /// </summary>
        public static string TraceId
        {
            get { return traceId.Value; }
            set { traceId.Value = value; }
        }

        /// <summary>
        /// 示例2：深层层级直接读取顶层设置的值，无需把 TraceId 层层传参。
        /// </summary>
        /// <returns>调用开始时的 TraceId 值。</returns>
        public static async Task<string> DeepReadAsync()
        {
            string expected = traceId.Value;                 // 顶层赋值
            await Task.Delay(1).ConfigureAwait(false);      // 换不换线程都不影响
            await Task.Run(delegate
            {
                string id = traceId.Value;                  // 线程池线程上仍能读到
                GC.KeepAlive(id);
            }).ConfigureAwait(false);
            return traceId.Value;                           // 与 expected 相同
        }

        /// <summary>
        /// 示例3：跨线程跟随自测——在父流赋值，Task.Run 内部（另一个线程）读取并比对。
        /// </summary>
        /// <param name="id">要传递的标识。</param>
        /// <returns>子线程读到同样的值返回 true。</returns>
        public static async Task<bool> FlowsAcrossThreadsAsync(string id)
        {
            AsyncLocal<string> slot = new AsyncLocal<string>();
            slot.Value = id;

            bool seen = false;
            await Task.Run(delegate
            {
                // ExecutionContext 在 Task.Run 时流转，子线程拿到快照副本
                seen = string.Equals(slot.Value, id, StringComparison.Ordinal);
            }).ConfigureAwait(false);

            return seen;
        }

        /// <summary>
        /// 示例4：写时复制父子隔离——子流先看到父值；子流内修改只对子流可见，父流读到的仍是旧值。
        /// </summary>
        /// <returns>形如 "childSaw=1,parentSees=1" 的自测描述（两个值都为 1，证明修改不外泄）。</returns>
        public static async Task<string> ParentChildIsolationAsync()
        {
            AsyncLocal<int> flowValue = new AsyncLocal<int>();
            flowValue.Value = 1;

            int childSaw = 0;
            await Task.Run(delegate
            {
                childSaw = flowValue.Value;     // 子流看得到父流启动前的赋值：1
                flowValue.Value = 2;            // 触发写时复制：只在子流副本内生效
            }).ConfigureAwait(false);

            int parentSees = flowValue.Value;   // 父流仍是 1，子流的 2 不会反向流回
            return "childSaw=" + childSaw + ",parentSees=" + parentSees;
        }

        /// <summary>
        /// 示例5：net48 真实可用的【变更回调】（.NET Framework 4.6+ 提供）。
        /// 记录每次 PreviousValue→CurrentValue，并标注是否因跨线程上下文切换观察到。
        /// 回调里不要做重逻辑：它可能在 ExecutionContext 流转路径上被同步调用。
        /// </summary>
        public sealed class ChangeLog
        {
            private readonly AsyncLocal<string> slot;

            /// <summary>
            /// 构造并注册变更回调的 AsyncLocal 槽。
            /// </summary>
            public ChangeLog()
            {
                Changes = new List<string>();
                // 带回调的构造函数 net46/net48 即可用（AsyncLocalValueChangedArgs<T> 同版本提供）
                slot = new AsyncLocal<string>(OnValueChanged);
            }

            /// <summary>按发生顺序记录的变更描述（仅同线程赋值测试时读，跨线程访问需自行加锁）。</summary>
            public List<string> Changes { get; private set; }

            /// <summary>当前异步流的值。</summary>
            public string Value
            {
                get { return slot.Value; }
                set { slot.Value = value; }
            }

            /// <summary>
            /// AsyncLocal 变更回调：值被显式修改或随 ExecutionContext 跨线程观察到变化时触发。
            /// </summary>
            /// <param name="args">变更参数：PreviousValue/CurrentValue/ThreadContextChanged。</param>
            private void OnValueChanged(AsyncLocalValueChangedArgs<string> args)
            {
                string direction = args.ThreadContextChanged ? HTranslation.GetContent("(跨线程观察)") : HTranslation.GetContent("(同线程赋值)");
                Changes.Add((args.PreviousValue ?? "null") + " -> " +
                            (args.CurrentValue ?? "null") + " " + direction);
            }
        }

        /// <summary>
        /// 示例6：工厂方法——创建带变更通知的 AsyncLocal（net48 可直接使用）。
        /// </summary>
        /// <typeparam name="T">值类型。</typeparam>
        /// <param name="valueChangedHandler">变更回调；不能为 null。</param>
        /// <returns>注册了回调的 AsyncLocal 实例。</returns>
        /// <exception cref="ArgumentNullException">valueChangedHandler 为 null 时抛出。</exception>
        public static AsyncLocal<T> CreateWithChangeNotification<T>(
            Action<AsyncLocalValueChangedArgs<T>> valueChangedHandler)
        {
            if (valueChangedHandler == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("值变更回调不能为空"));
            }

            // 回调签名：void(AsyncLocalValueChangedArgs<T>)；ThreadContextChanged 区分变更来源
            return new AsyncLocal<T>(valueChangedHandler);
        }

        /// <summary>
        /// 示例7：【显式取值对比】观察变化的备选方式（不依赖回调；适合轮询式诊断、
        /// 或向没有回调构造函数的其他平台移植时使用）。调用方比较返回的旧值与本次新值即可判断是否变化。
        /// 注意：net48 本身已支持回调（见示例5/6），此处仅演示等价的手工对比思路。
        /// </summary>
        /// <param name="previousValue">上一次记录的值（ref：方法内被更新为 currentValue）。</param>
        /// <param name="currentValue">本次读到的值。</param>
        /// <returns>更新前的旧值；调用方比较 old 与 currentValue 是否相等即知是否变化。</returns>
        public static string DetectChange(ref string previousValue, string currentValue)
        {
            // 等价的 .NET Core / net48 回调写法（两者在 net48 都可编译）：
            // new AsyncLocal<string>(delegate(AsyncLocalValueChangedArgs<string> args)
            // {
            //     bool crossThread = args.ThreadContextChanged;
            //     string old = args.PreviousValue, now = args.CurrentValue;
            // });
            string old = previousValue;
            previousValue = currentValue;
            return old;
        }
    }
}
