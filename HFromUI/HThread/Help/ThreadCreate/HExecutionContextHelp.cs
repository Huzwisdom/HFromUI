using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// ExecutionContext 执行上下文帮助类：让“执行环境信息”随异步工作项跨线程流动。
    ///
    /// 【是什么】<see cref="ExecutionContext"/> 是 CLR 对“当前执行环境”的统一容器：
    /// 通过 <see cref="ExecutionContext.Capture"/> 在当前线程捕获一份不可变快照，
    /// 再通过 <see cref="ExecutionContext.Run(ExecutionContext, ContextCallback, object)"/>
    /// 在另一个线程（通常是线程池线程或手工新建线程）上把它还原后执行回调。
    /// 线程池投递（<see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>）、Task、定时器等
    /// 内部都会自动完成“捕获 → 流动 → 还原”，所以回调里能读到投递之前设置好的环境值。
    /// 执行上下文承载的内容主要有：<see cref="AsyncLocal{T}"/> 的值、逻辑调用上下文
    /// LogicalCallContext（CallContext 逻辑数据槽）、SecurityContext（安全上下文/模拟登录身份）。
    ///
    /// 【是否跨进程】否。上下文快照只在本进程内的线程之间流动，不能跨进程/跨机器传递。
    ///
    /// 【典型适用场景】
    /// 1) 用 <see cref="AsyncLocal{T}"/> 保存“ ambient （环境）数据”（如链路追踪 Id、当前租户），
    ///    期望它自动跟随 await、Task.Run、线程池回调流动；
    /// 2) 需要在线程切换时保留安全身份/模拟令牌（SecurityContext 随上下文一起流动）；
    /// 3) 高性能投递前临时用 <see cref="ExecutionContext.SuppressFlow"/> 抑制流动，减少一次捕获/复制；
    /// 4) 手动在“裸线程”上还原投递方的环境（new Thread 默认也会捕获，手动 Run 可显式控制快照时机）。
    ///
    /// 【使用步骤】
    /// 1) 设置 <see cref="AsyncLocal{T}"/> 等环境值；
    /// 2) <see cref="ExecutionContext.Capture"/> 得到快照（流动被抑制时返回 null）；
    /// 3) 在目标线程调 <see cref="ExecutionContext.Run(ExecutionContext, ContextCallback, object)"/>；
    /// 4) 需要临时切断流动时调 <see cref="ExecutionContext.SuppressFlow"/> 得到
    ///    <see cref="AsyncFlowControl"/>，并【必须】放在 using(){ } 中或在 finally 里
    ///    Dispose/Restore（Undo）恢复流动，否则会污染当前线程后续所有线程池投递。
    ///
    /// 【注意事项与坑】
    /// 1) 上下文承载 <see cref="AsyncLocal{T}"/> 值、LogicalCallContext、SecurityContext；
    ///    但【不承载】<see cref="ThreadStaticAttribute"/> 字段、LocalDataStoreSlot
    ///    （<see cref="Thread.GetData"/>/<see cref="Thread.SetData"/>/<see cref="Thread.AllocateNamedDataSlot"/>）
    ///    这类“线程本地存储”——它们严格绑定物理线程，线程池线程/新线程上一律是默认值；
    /// 2) <see cref="ExecutionContext.SuppressFlow"/> 返回的 <see cref="AsyncFlowControl"/> 是结构体，
    ///    实现了 <see cref="IDisposable"/>；忘记恢复会导致当前线程之后所有异步投递都不再带上下文，
    ///    属于很难排查的“环境污染”，务必 using 或 finally 恢复；重复 SuppressFlow 会抛
    ///    <see cref="InvalidOperationException"/>；
    /// 3) AsyncLocal 在“流动”时传递的是快照：回调线程里修改 AsyncLocal.Value 不会回写到投递方
    ///    （值类型语义是复制；引用类型字段被两边共享引用，可变状态要自行加锁）；
    /// 4) CallContext 逻辑槽（见下）按引用放入可变对象时，要注意上下文复制与跨 AppDomain 深拷贝问题。
    ///
    /// 【关于 HostExecutionContext】<see cref="ExecutionContext"/> 还可携带 HostExecutionContext
    /// （宿主执行上下文），用于把宿主（IIS/ASP.NET、SQL Server CLR 集成等）持有的非托管资源、
    /// 线程亲和状态随工作项一起流动；一般业务代码用不到，HostExecutionContextManager 的
    /// Capture/Set/Revert 只服务于宿主与基础库作者，普通应用理解“宿主资源也能流动”这一概念即可，不要直接操作。
    ///
    /// 【关于 CallContext 逻辑数据槽】<c>System.Runtime.Remoting.Messaging.CallContext</c> 的
    /// LogicalSetData/LogicalGetData/FreeNamedDataSlot 是 <see cref="AsyncLocal{T}"/> 出现之前的
    /// 逻辑上下文方案，数据随执行上下文流动：
    /// - 按引用写入可变对象要格外小心：流动时逻辑调用上下文会被复制，跨 AppDomain 还会对
    ///   可序列化对象做深拷贝，不要依赖对象身份，多线程共享可变引用需自行深拷贝或加锁；
    /// - .NET Core/.NET 5+ 已随 .NET Remoting 一并移除 CallContext，统一用
    ///   <see cref="AsyncLocal{T}"/> 替代；net48 两者都可用，新代码应直接用 AsyncLocal。
    ///
    /// 【版本可用性】<see cref="ExecutionContext"/>、<see cref="AsyncFlowControl"/> net48 全量可用；
    /// <see cref="AsyncLocal{T}"/> 需要 .NET Framework 4.6+（net48 自带 4.6+ 运行时，可用）。
    /// </summary>
    /// <example>
    /// 捕获执行上下文并在手工新建的线程上还原（AsyncLocal 值跟随流动）：
    /// <code>
    /// AsyncLocal&lt;int&gt; asyncLocal = new AsyncLocal&lt;int&gt;();
    /// asyncLocal.Value = 7;
    /// ExecutionContext captured = ExecutionContext.Capture();
    /// Thread thread = new Thread((ThreadStart)delegate
    /// {
    ///     ExecutionContext.Run(captured, delegate(object state)
    ///     {
    ///         Console.WriteLine(asyncLocal.Value);   // 输出 7：值随上下文流动到了新线程
    ///     }, null);
    /// }) { IsBackground = true };
    /// thread.Start();
    /// thread.Join(1000);
    /// </code>
    /// 临时抑制流动必须恢复（推荐 using，等价于 finally 中 Dispose/Restore）：
    /// <code>
    /// using (ExecutionContext.SuppressFlow())
    /// {
    ///     // 此区间内的线程池投递/新线程不携带当前上下文：AsyncLocal 在目标线程读到默认值
    ///     ThreadPool.QueueUserWorkItem(delegate(object s) { Console.WriteLine(asyncLocal.Value); });
    /// }   // using 结束自动恢复流动；不恢复会污染本线程后续所有线程池投递
    /// </code>
    /// </example>
    public static class HExecutionContextHelp
    {
        /// <summary>
        /// 示例1（自包含可执行）：验证 <see cref="ExecutionContext"/> 会把 <see cref="AsyncLocal{T}"/>
        /// 的值流动到手工新建的线程。主线程把 AsyncLocal 设为 7，<see cref="ExecutionContext.Capture"/>
        /// 后 new Thread，并在子线程里用 <see cref="ExecutionContext.Run"/> 还原快照后读取。
        /// 全程不弹 UI、不依赖文件网络；子线程带 2000ms 超时 Join 回收，正常情况下毫秒级结束。
        /// </summary>
        /// <returns>子线程在还原的上下文中读到的 AsyncLocal 值（预期为 7）；线程 2000ms 内未结束返回 -1。</returns>
        public static int FlowToNewThreadDemo()
        {
            AsyncLocal<int> asyncLocal = new AsyncLocal<int>();
            asyncLocal.Value = 7;                              // 投递前设置环境值
            ExecutionContext captured = ExecutionContext.Capture();

            int result = 0;
            Thread worker = new Thread((ThreadStart)delegate
            {
                // 在新线程上还原捕获到的快照后执行回调
                ExecutionContext.Run(captured, delegate (object state)
                {
                    result = asyncLocal.Value;                // 子线程读到随上下文流动来的 7
                }, null);
            }) { IsBackground = true };
            worker.Start();

            if (!worker.Join(2000))                           // 带超时回收，防止异常环境下挂死
            {
                return -1;
            }
            return result;                                    // 预期返回 7
        }

        /// <summary>
        /// 示例2（自包含可执行）：验证 <see cref="ExecutionContext.SuppressFlow"/> 的抑制与恢复。
        /// 抑制流动期间新建线程读取 <see cref="AsyncLocal{T}"/> 只能得到默认值 0；
        /// 在 finally 中无条件 Dispose（即 Restore/Undo）恢复流动后，再次 Capture 并在新线程 Run，
        /// 验证 AsyncLocal 又能正常流动读到 7。两个线程均带 1000ms 超时 Join，正常毫秒级结束。
        /// </summary>
        /// <returns>
        /// 组合结果：抑制期间新线程读到默认值 0，且恢复流动后新线程重新读到 7，两项同时成立返回 true；否则 false。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 当前线程已经处于流动抑制状态时再次调用 <see cref="ExecutionContext.SuppressFlow"/> 会抛出。
        /// </exception>
        public static bool SuppressFlowAndRestoreDemo()
        {
            AsyncLocal<int> asyncLocal = new AsyncLocal<int>();
            asyncLocal.Value = 7;

            int valueWhileSuppressed = -1;
            bool flowWorksAfterRestore = false;

            // 抑制当前线程的执行上下文流动：返回的 AsyncFlowControl 必须恢复
            AsyncFlowControl flowControl = ExecutionContext.SuppressFlow();
            try
            {
                Thread suppressedThread = new Thread((ThreadStart)delegate
                {
                    // 流动被抑制：新线程拿不到父线程快照，AsyncLocal 为默认值 0
                    valueWhileSuppressed = asyncLocal.Value;
                }) { IsBackground = true };
                suppressedThread.Start();
                if (!suppressedThread.Join(1000))             // 带超时回收
                {
                    return false;
                }
            }
            finally
            {
                // 必须无条件恢复：否则当前线程后续所有线程池投递都不再携带上下文
                flowControl.Dispose();                        // 等价于 flowControl.Undo()/Restore()
            }

            // 恢复后重新捕获：此时快照里 AsyncLocal 仍是 7（抑制不影响当前线程自身的值）
            ExecutionContext capturedAfterRestore = ExecutionContext.Capture();
            Thread restoredThread = new Thread((ThreadStart)delegate
            {
                ExecutionContext.Run(capturedAfterRestore, delegate (object state)
                {
                    flowWorksAfterRestore = (asyncLocal.Value == 7);
                }, null);
            }) { IsBackground = true };
            restoredThread.Start();
            if (!restoredThread.Join(1000))
            {
                return false;
            }

            // 组合断言：抑制期间读到 0（默认值），恢复之后又能读到 7
            return valueWhileSuppressed == 0 && flowWorksAfterRestore;
        }
    }
}
