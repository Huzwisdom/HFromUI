using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.LegacyAsync
{
    using HFromUI.HLangage;
    /// <summary>
    /// APM（Asynchronous Programming Model，异步编程模型）帮助类：BeginXxx/EndXxx 经典模式（.NET 1.0）。
    /// 【是什么】APM 用一对方法表达异步：BeginXxx(参数..., AsyncCallback callback, object state) 立即返回
    /// IAsyncResult（操作凭证），操作在线程池/IOCP 上执行；完成后调用方通过四种模式之一收尾，并【必须恰好调用一次】
    /// EndXxx(IAsyncResult) 取回结果/异常。AsyncCallback 是委托 void(IAsyncResult)，由完成线程回调；
    /// state 原样保存在 IAsyncResult.AsyncState 中带回。
    /// IAsyncResult 四成员：IsCompleted（可轮询）、AsyncWaitHandle（内核句柄可阻塞等待）、
    /// AsyncState（Begin 传入的自定义对象）、CompletedSynchronously（是否同步完成的优化提示，回调可能内联执行）。
    /// 四种等待模式（本类逐一示例）：
    /// 1) 回调模式：BeginXxx 传 AsyncCallback，在回调里 EndXxx（资源及时释放，推荐的 APM 用法）；
    /// 2) 阻塞模式：BeginXxx 后直接 EndXxx，End 内部阻塞（UI 线程禁用）；
    /// 3) WaitHandle 轮询/阻塞：ar.AsyncWaitHandle.WaitOne(timeout) 后 EndXxx（可带超时）；
    /// 4) IsCompleted 轮询：循环检查 ar.IsCompleted + Sleep，适合轮询期间还要干别的活。
    /// 【是否跨进程】否；但 APM 的异步 IO（NetworkStream/BeginRead 等）借助 IOCP 等待内核 IO，等待期间不占线程。
    /// 【典型适用场景】维护/包装老式框架 API：Stream.BeginRead/EndRead、Dns.BeginGetHostEntry、Socket.BeginXxx、
    /// SqlCommand.BeginExecuteReader 等；新代码一律 Task+await，互操作用 Task.Factory.FromAsync 包装。
    /// 【使用步骤】
    /// 1) 调 BeginXxx 取得 IAsyncResult（需要完成通知就传 AsyncCallback，需要上下文就传 state）；
    /// 2) 四选一等待；3) 恰好调用一次 EndXxx（操作中的异常在 EndXxx 处重新抛出）；
    /// 4) 现代代码用 Task.Factory.FromAsync 把 Begin/End 对包装成 Task 后 await（FromAsync 保证只调一次 End）。
    /// 【注意事项与坑】
    /// - EndXxx 必须恰好一次：不调可能泄漏未释放的异步资源（尤其 IO 类）；重复调用行为未定义（多数实现抛 InvalidOperationException）；
    /// - EndXxx 必须用【同一个】实现该操作的对象与对应的 IAsyncResult，错配通常抛 ArgumentException；
    /// - 回调运行在哪个线程不保证（线程池/IOCP 线程，CompletedSynchronously=true 时甚至内联在 Begin 调用栈上），
    ///   更新 UI 必须 Invoke；
    /// - 委托类型（Func/Action/自定义 delegate）的 BeginInvoke/EndInvoke 仅 .NET Framework 可用；
    ///   .NET Core/.NET 5+【已移除】委托 BeginInvoke（编译不过，PlatformNotSupported/缺失方法），
    ///   迁移方式：Task.Run(work) 或 Function.BeginInvoke 等框架自带 APM 用 FromAsync 包装；
    /// - AsyncWaitHandle 用不显式 Close 也可（终结器兜底），但频繁创建时建议用完释放（Close/Dispose）；
    /// - FromAsync 是把 APM 接入 async/await 的官方适配器，会替你调 EndXxx 并把异常放进 Task。
    /// 【版本可用性】net48：委托 BeginInvoke/EndInvoke、IAsyncResult、AsyncCallback、TaskFactory.FromAsync 全套可用；
    /// .NET Core/.NET 5+：框架自带 APM（Stream/Socket 等）多数保留并可 FromAsync；委托 BeginInvoke 不可用。
    /// </summary>
    /// <example>
    /// 包装真实框架 APM（net48 与 .NET Core 均可用的形态，Begin/End 由框架提供，非委托 BeginInvoke）：
    /// <code>
    /// Task<int> read = Task.Factory.FromAsync(
    ///     (cb, st) => stream.BeginRead(buffer, 0, buffer.Length, cb, st),
    ///     ar => stream.EndRead(ar),
    ///     null);
    /// int n = await read;
    /// </code>
    /// </example>
    public static class HApmHelp
    {
        /// <summary>
        /// 示例1：Begin 发起（用委托 BeginInvoke 演示 net48 的 APM 形态；返回 IAsyncResult 凭证）。
        /// </summary>
        /// <param name="work">实际计算（入参 int，返回 int）</param>
        /// <param name="argument">传给 work 的实参</param>
        /// <param name="callback">完成回调（可为 null；在完成线程上被调用）</param>
        /// <param name="state">自定义状态对象，原样存入 IAsyncResult.AsyncState（可为 null）</param>
        /// <returns>表示该异步操作的 IAsyncResult</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static IAsyncResult BeginCompute(Func<int, int> work, int argument, AsyncCallback callback, object state)
        {
            // 委托 BeginInvoke：net48 可用，.NET Core/.NET 5+ 已移除
            return work.BeginInvoke(argument, callback, state);
        }

        /// <summary>
        /// 示例2：End 取结果——必须恰好调用一次；工作中的异常在此处重新抛出。
        /// </summary>
        /// <param name="work">与 BeginCompute 同一个委托实例</param>
        /// <param name="ar">BeginCompute 返回的 IAsyncResult</param>
        /// <returns>计算结果</returns>
        /// <exception cref="ArgumentNullException">work/ar 为 null</exception>
        /// <exception cref="ArgumentException">IAsyncResult 不属于该委托（跨实例错配）</exception>
        /// <exception cref="InvalidOperationException">重复 End（行为依实现，通常抛此异常）</exception>
        public static int EndCompute(Func<int, int> work, IAsyncResult ar)
        {
            return work.EndInvoke(ar);   // 操作中的异常在这里重新抛出；未完成时阻塞等待
        }

        /// <summary>
        /// 示例3：等待模式①回调模式：完成线程上调用 End，再自行投递回 UI。
        /// </summary>
        /// <param name="work">计算委托</param>
        /// <param name="argument">计算实参</param>
        /// <param name="onResult">完成回调（运行在线程池线程，更新 UI 需 Invoke）</param>
        /// <exception cref="ArgumentNullException">work/onResult 为 null</exception>
        public static void CallbackPattern(Func<int, int> work, int argument, Action<int> onResult)
        {
            BeginCompute(work, argument, delegate (IAsyncResult ar)
            {
                int result = EndCompute(work, ar);
                onResult(result);        // 此处一般在线程池线程，更新UI需 Invoke
            }, null);
        }

        /// <summary>
        /// 示例4：回调模式 + state 透传：Begin 的 state 经 ar.AsyncState 原样带回回调（典型用于关联请求上下文）。
        /// </summary>
        /// <param name="work">计算委托</param>
        /// <param name="argument">计算实参</param>
        /// <param name="state">自定义关联对象（如请求 Id、控件引用包装）</param>
        /// <param name="onResult">回调：入参依次为结果与原样带回的 state</param>
        /// <exception cref="ArgumentNullException">work/onResult 为 null</exception>
        public static void CallbackPatternWithState(Func<int, int> work, int argument, object state,
                                                    Action<int, object> onResult)
        {
            BeginCompute(work, argument, delegate (IAsyncResult ar)
            {
                int result = EndCompute(work, ar);
                onResult(result, ar.AsyncState);   // AsyncState 即 Begin 的最后一个参数
            }, state);
        }

        /// <summary>
        /// 示例5：等待模式②阻塞模式：Begin 后直接 End（End 内部等待完成）；会卡住调用线程，UI 线程禁用。
        /// </summary>
        /// <param name="work">计算委托</param>
        /// <param name="argument">计算实参</param>
        /// <returns>计算结果</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static int BlockingWait(Func<int, int> work, int argument)
        {
            IAsyncResult ar = BeginCompute(work, argument, null, null);
            return EndCompute(work, ar);   // EndInvoke 内部等待完成
        }

        /// <summary>
        /// 示例6：等待模式③AsyncWaitHandle 阻塞（带超时）：WaitOne 成功后再 End；超时抛 TimeoutException。
        /// </summary>
        /// <param name="work">计算委托</param>
        /// <param name="argument">计算实参</param>
        /// <param name="timeoutMs">等待超时（毫秒）</param>
        /// <returns>计算结果</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        /// <exception cref="TimeoutException">timeoutMs 内操作未完成</exception>
        public static int WaitHandlePattern(Func<int, int> work, int argument, int timeoutMs)
        {
            IAsyncResult ar = BeginCompute(work, argument, null, null);
            if (ar.AsyncWaitHandle.WaitOne(timeoutMs))
            {
                return EndCompute(work, ar);   // 收到内核信号后取结果
            }
            throw new TimeoutException();       // 超时：操作仍在后台进行，注意后续仍需有人 End
        }

        /// <summary>
        /// 示例7：等待模式④IsCompleted 轮询：等待期间可穿插其他工作，到点再 End。
        /// </summary>
        /// <param name="work">计算委托</param>
        /// <param name="argument">计算实参</param>
        /// <param name="pollIntervalMs">轮询间隔（毫秒）</param>
        /// <param name="timeoutMs">总超时（毫秒）</param>
        /// <returns>计算结果</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">pollIntervalMs 小于等于 0</exception>
        /// <exception cref="TimeoutException">timeoutMs 内操作未完成</exception>
        public static int PollIsCompletedPattern(Func<int, int> work, int argument, int pollIntervalMs, int timeoutMs)
        {
            if (pollIntervalMs <= 0)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("轮询间隔必须大于 0"));
            }
            IAsyncResult ar = BeginCompute(work, argument, null, null);
            int waited = 0;
            while (!ar.IsCompleted)
            {
                if (waited >= timeoutMs)
                {
                    throw new TimeoutException();   // 同样：超时后后台操作仍会完成，需有兜底 End
                }
                Thread.Sleep(pollIntervalMs);      // 轮询间隙可在此穿插其他工作
                waited += pollIntervalMs;
            }
            return EndCompute(work, ar);
        }

        /// <summary>
        /// 示例8：现代推荐——Task.Factory.FromAsync 把 APM Begin/End 对包装成 Task&lt;T&gt; 后 await；
        /// FromAsync 负责在完成时调用恰好一次 EndInvoke 并把异常封进 Task。
        /// </summary>
        /// <param name="work">计算委托（net48 演示用；框架自带 Begin/End API 同样套路）</param>
        /// <param name="argument">计算实参</param>
        /// <returns>可 await 的 Task&lt;int&gt;</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static Task<int> WrapToTask(Func<int, int> work, int argument)
        {
            // 泛型 FromAsync：begin 工厂 + end 函数 + state
            return Task.Factory.FromAsync(
                delegate (AsyncCallback cb, object state) { return work.BeginInvoke(argument, cb, state); },
                delegate (IAsyncResult ar) { return work.EndInvoke(ar); },
                null);
        }

        /// <summary>
        /// 示例9：FromAsync 无返回值版本：把 Action（无结果的 APM）包装成 Task。
        /// </summary>
        /// <param name="work">无参无返回工作</param>
        /// <returns>可 await 的 Task（完成或携带异常）</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static Task WrapActionToTask(Action work)
        {
            // 非泛型 FromAsync：end 委托只负责 EndInvoke，不取结果
            return Task.Factory.FromAsync(
                delegate (AsyncCallback cb, object state) { return work.BeginInvoke(cb, state); },
                delegate (IAsyncResult ar) { work.EndInvoke(ar); },
                null);
        }
    }
}
