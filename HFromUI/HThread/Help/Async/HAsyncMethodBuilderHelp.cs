using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    /// <summary>
    /// 【是什么】async 方法背后“状态机构建器”全集帮助类（命名空间 System.Runtime.CompilerServices）。
    /// 写 async 方法时编译器自动生成一个实现 <see cref="IAsyncStateMachine"/> 的结构体状态机，并为它配一个构建器：
    /// 1) <see cref="AsyncTaskMethodBuilder"/>：async Task 方法的构建器；
    /// 2) <see cref="AsyncTaskMethodBuilder{TResult}"/>：async Task&lt;TResult&gt; 方法的构建器；
    /// 3) <see cref="AsyncVoidMethodBuilder"/>：async void 方法的构建器（仅 UI 事件处理器使用，见注意事项）；
    /// 4) AsyncValueTaskMethodBuilder / AsyncValueTaskMethodBuilder&lt;TResult&gt;：async ValueTask 系列的构建器，
    ///    net48 的 mscorlib 不提供，由 NuGet 包 System.Threading.Tasks.Extensions 4.6.3（lib/net462）提供——
    ///    已对 bin 目录与 packages 目录中的程序集做反射实测，确认该类型及其 Create/Start/SetResult/SetException/Task
    ///    成员均存在（AsyncIteratorMethodBuilder 则在 Microsoft.Bcl.AsyncInterfaces 包内）。
    /// 构建器固定工作流：Create() 创建 → Start(ref stateMachine) 启动状态机 →
    /// 状态机 MoveNext 内成功调 SetResult（泛型版带返回值）/失败调 SetException(Exception) →
    /// 消费方通过构建器 Task 属性拿到 Task/Task&lt;T&gt;（ValueTask 构建器对应 ValueTask/ValueTask&lt;T&gt;）；
    /// 遇到未完成的 awaiter 时由 AwaitOnCompleted（或 AwaitUnsafeOnCompleted）把“再次调用 MoveNext”挂为续体。
    /// <see cref="IAsyncStateMachine"/> 两个接口方法：MoveNext() 推进状态机（每次 await 完成都回调它），
    /// SetStateMachine(IAsyncStateMachine) 接收状态机的装箱副本。
    ///
    /// 【是否跨进程】否。构建器与状态机都是进程内编译器生成/手工驱动的 CLR 对象，Task 跨进程只用其字符串结果。
    ///
    /// 【典型适用场景】
    /// 1) 理解 async/await 的底层执行模型（面试/排查续体调度问题时的心智模型）；
    /// 2) 编写极端高性能的自定义异步原语时手工控制状态机（99.9% 业务代码不需要）；
    /// 3) 实现自定义可等待类型时提供配套构建器（.NET Core 高版本可用
    ///    [AsyncMethodBuilder(typeof(MyBuilder))] 把特性【标注在返回类型】上）。
    ///
    /// 【使用步骤（以 AsyncTaskMethodBuilder&lt;T&gt; 为例）】
    /// 1) 定义 struct XxxStateMachine : IAsyncStateMachine，内含 public AsyncTaskMethodBuilder&lt;T&gt; builder；
    /// 2) MoveNext 内 try 执行业务并 builder.SetResult(结果)，catch(Exception ex) 时 builder.SetException(ex)；
    ///    SetStateMachine 内调 builder.SetStateMachine(stateMachine)（可空实现，仅教学也建议照写）；
    /// 3) 驱动方：var b = AsyncTaskMethodBuilder&lt;T&gt;.Create(); 把 b 赋给 sm.builder 后，
    ///    必须改由字段实例调用 sm.builder.Start(ref sm)（ref 必须指向【局部变量】sm）——
    ///    构建器是 struct，Start/Task 都只能经 sm.builder 这一份实例，旧拷贝 b 会拿到永不完成的 Task；
    /// 4) Task&lt;T&gt; t = sm.builder.Task; 同步完成时 t.Result 立即拿到结果，未完成时正常 await；
    /// 5) 需要挂续体时在 MoveNext 内调 builder.AwaitOnCompleted(ref awaiter, ref this)。
    ///
    /// 【注意事项与坑】
    /// 1) AsyncVoidMethodBuilder 极度危险：async void 没有 Task 载体，异常无人可接——构建器直接把异常 Post 到
    ///    当前 <see cref="SynchronizationContext"/>（UI 程序即 UI 线程消息循环），未处理即触发
    ///    Application.ThreadException/未处理异常策略，【默认可直接终止进程】；async void 只允许用于 UI 事件处理器；
    /// 2) 手写状态机仅用于教学：编译器会自动生成 Create/Start/MoveNext/AwaitOnCompleted 的全部样板，
    ///    手写极易因 struct 装箱拷贝（SetStateMachine 的 boxed 副本）造成状态分叉，业务代码一律用 async/await；
    /// 3) Start(ref sm) 首次 MoveNext 是在栈上以 ref 直接调用（同步完成路径不发生装箱），
    ///    一旦遇到未完成 awaiter，构建器会装箱状态机并只操作装箱副本——所以 Start 之后不能再读写局部 sm 的字段；
    /// 4) SetResult/SetException 只能生效一次（重复设置抛 <see cref="InvalidOperationException"/>），
    ///    与 TaskCompletionSource 的 Set/TrySet 语义一致；
    /// 5) 自定义 async 返回类型（给 MyTask 标 [AsyncMethodBuilder(typeof(MyTaskMethodBuilder))]）
    ///    在 .NET Core/.NET 5+ 高版本工具链才完整支持；net48 即使引用包拿到了 AsyncMethodBuilderAttribute 类型，
    ///    也不能用它在本机编译出自定义 async 返回类型，本类仅以注释讲解，不提供可跑示例。
    ///
    /// 【版本可用性】AsyncTaskMethodBuilder/&lt;T&gt;、AsyncVoidMethodBuilder、IAsyncStateMachine、
    /// AwaitOnCompleted 均为 .NET Framework 4.5+（net48 可用）；AsyncValueTaskMethodBuilder 随
    /// System.Threading.Tasks.Extensions NuGet 包在 net48 可用（反射已确认）；
    /// 类型级 [AsyncMethodBuilder] 自定义返回类型仅 .NET Core 高版本支持。
    /// </summary>
    /// <example>
    /// 自定义可等待返回类型 + 构建器特性（.NET Core 高版本；net48 不可用，仅注释示意，
    /// 注意 XML/源码中的泛型尖括号要写作 &amp;lt; / &amp;gt;）：
    /// <code>
    /// // [AsyncMethodBuilder(typeof(MyTaskMethodBuilder&lt;&gt;))]   // 标注在“返回类型”上才是自定义 async 返回类型
    /// // public struct MyTask&lt;T&gt; : INotifyCompletion { /* GetAwaiter/IsCompleted/Result/GetResult */ }
    /// //
    /// // public struct MyTaskMethodBuilder&lt;T&gt;
    /// // {
    /// //     public static MyTaskMethodBuilder&lt;T&gt; Create() { ... }
    /// //     public void Start&lt;TStateMachine&gt;(ref TStateMachine sm) where TStateMachine : IAsyncStateMachine { ... }
    /// //     public void SetResult(T result) { ... }
    /// //     public void SetException(Exception ex) { ... }
    /// //     public MyTask&lt;T&gt; Task { get { ... } }
    /// //     public void AwaitOnCompleted&lt;TAwaiter, TStateMachine&gt;(ref TAwaiter a, ref TStateMachine sm) { ... }
    /// // }
    /// </code>
    /// </example>
    public static class HAsyncMethodBuilderHelp
    {
        /// <summary>
        /// 最小教学状态机：MoveNext 一次性 SetResult(777)，无真正的挂起/续体。
        /// 编译器生成的 async Task&lt;int&gt; 方法用的正是同一套 Create/Start/SetResult/Task 调用序列。
        /// </summary>
        private struct SimpleStateMachine : IAsyncStateMachine
        {
            /// <summary>构建器：由外部 Create 后赋值进来（保持 public 字段，与编译器生成的字段布局一致）。</summary>
            public AsyncTaskMethodBuilder<int> builder;

            /// <summary>推进状态机：本例同步成功，直接交付结果；任何异常都通过 SetException 挂到 Task 上。</summary>
            void IAsyncStateMachine.MoveNext()
            {
                try
                {
                    // 同步完成路径：直接设置 Task 的结果（真实 async 方法中此处可能先经历多次 await 续体回调）
                    builder.SetResult(777);
                }
                catch (Exception ex)
                {
                    // 状态机内抛出的异常不外泄到调用栈，而是令关联 Task 进入 Faulted
                    builder.SetException(ex);
                }
            }

            /// <summary>接收状态机的装箱副本：有真正异步挂起时构建器靠它在续体回调时操作同一份堆状态。</summary>
            /// <param name="stateMachine">装箱后的状态机实例。</param>
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                // 照编译器生成代码的写法转交给构建器；本例同步完成，此方法实际不会被调用
                builder.SetStateMachine(stateMachine);
            }
        }

        /// <summary>
        /// 最小教学状态机（ValueTask 版）：演示 AsyncValueTaskMethodBuilder&lt;T&gt; 与 Task 版完全同形的工作流。
        /// 该构建器类型不在 net48 mscorlib 中，由已引用的 System.Threading.Tasks.Extensions 4.6.3 包提供
        /// （反射已确认成员齐全）。
        /// </summary>
        private struct SimpleValueTaskStateMachine : IAsyncStateMachine
        {
            /// <summary>ValueTask 构建器：由外部 Create 后赋值。</summary>
            public AsyncValueTaskMethodBuilder<int> builder;

            /// <summary>推进状态机：同步交付 888；异常改挂到 ValueTask 背后的 ValueTaskSource 上。</summary>
            void IAsyncStateMachine.MoveNext()
            {
                try
                {
                    // SetResult 后 builder.Task 得到的 ValueTask&lt;int&gt; 即成功完成、结果为 888
                    builder.SetResult(888);
                }
                catch (Exception ex)
                {
                    // 与 Task 版相同：异常进入构建器，await ValueTask 时解包抛出
                    builder.SetException(ex);
                }
            }

            /// <summary>接收装箱副本并转交构建器（本例同步路径不会触发）。</summary>
            /// <param name="stateMachine">装箱后的状态机实例。</param>
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                builder.SetStateMachine(stateMachine);
            }
        }

        /// <summary>
        /// 示例1：手工驱动 <see cref="AsyncTaskMethodBuilder{TResult}"/> 跑完整条 Create → 赋 builder →
        /// Start(ref sm) → Task → Result 流程。MoveNext 同步 SetResult，故 Start 返回时 Task 已完成，
        /// Result 立即返回，不阻塞、不弹 UI。
        /// 关键：构建器是 struct，赋给 sm.builder 后就是两个独立拷贝；Start 与 Task 必须始终经由
        /// 【sm.builder 这一个字段实例】访问（编译器生成代码也是 stateMachine.builder.Start(ref stateMachine)），
        /// 否则 MoveNext 完成的是字段拷贝、外面从旧拷贝取 Task 会得到永不完成的承诺任务而永久阻塞。
        /// </summary>
        /// <returns>状态机交付的结果，恒为 777。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出业务异常：状态机内部已 try/catch 并经 SetException 挂载。</exception>
        public static int RunManualStateMachineDemo()
        {
            // 1) 创建 Task<int> 的构建器（等价编译器在 async Task<int> 方法开头生成的代码）
            AsyncTaskMethodBuilder<int> b = AsyncTaskMethodBuilder<int>.Create();

            // 2) 构造栈上状态机并把构建器交给它；此后只通过 sm.builder 这一份实例操作
            SimpleStateMachine sm = new SimpleStateMachine();
            sm.builder = b;

            // 3) 启动：首次 MoveNext 以 ref 在栈上直接执行；sm 必须是局部变量，struct 的 ref 语义才正确。
            //    注意接收者写 sm.builder 而非旧拷贝 b——MoveNext 内 SetResult 修改的是 sm.builder 的任务槽
            sm.builder.Start(ref sm);

            // 4) 从同一个字段实例取出构建器产出的 Task；本例 MoveNext 已同步 SetResult，状态为 RanToCompletion
            Task<int> t = sm.builder.Task;

            // 已完成任务取 Result 不会阻塞，直接得到 777
            return t.Result;
        }

        /// <summary>
        /// 示例2：手工驱动 AsyncValueTaskMethodBuilder&lt;T&gt;（System.Threading.Tasks.Extensions 包提供）。
        /// 与 <see cref="RunManualStateMachineDemo"/> 同形，仅返回载体从 Task&lt;int&gt; 换成
        /// <see cref="ValueTask{TResult}"/>；同步成功完成时直接读 Result，不经过 AsTask 拆箱。
        /// </summary>
        /// <returns>ValueTask 状态机交付的结果，恒为 888。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出业务异常：状态机内部已 try/catch。</exception>
        public static int RunManualValueTaskStateMachineDemo()
        {
            // 1) 创建 ValueTask<int> 构建器（类型来自 System.Threading.Tasks.Extensions，net48 经包引用可用）
            AsyncValueTaskMethodBuilder<int> b = AsyncValueTaskMethodBuilder<int>.Create();

            // 2) 栈上状态机持有同一个构建器；后续只经 sm.builder 这一份 struct 实例操作（理由同示例1）
            SimpleValueTaskStateMachine sm = new SimpleValueTaskStateMachine();
            sm.builder = b;

            // 3) Start 首次同步执行 MoveNext（ref 局部变量；接收者必须是字段 sm.builder，不能是旧拷贝 b）
            sm.builder.Start(ref sm);

            // 4) Task 属性对 ValueTask 构建器返回 ValueTask<int>，同样从字段实例读取
            ValueTask<int> vt = sm.builder.Task;

            // 已成功完成直接读 Result；保险分支：理论上未完成时退化为 AsTask().Result（本例恒走前者）
            int result = vt.IsCompletedSuccessfully ? vt.Result : vt.AsTask().Result;
            return result;
        }
    }
}
