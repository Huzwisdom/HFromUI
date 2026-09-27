using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// TaskFactory 帮助类：批量创建和调度 Task 的工厂。
    /// 【是什么】System.Threading.Tasks.TaskFactory（实例属性 Task.Factory）是一组任务的"创建模板"，
    /// 可统一默认取消令牌、创建选项（TaskCreationOptions）、延续选项（TaskContinuationOptions）和调度器；
    /// Task&lt;TResult&gt;.Factory 是带返回值版本 TaskFactory&lt;TResult&gt;。
    /// 【是否跨进程】否。工厂只在本进程内创建 Task。
    /// 【典型适用场景】
    /// 1) StartNew：Task.Run 出现之前的老牌启动方式，可指定状态/选项/调度器；
    /// 2) ContinueWhenAll / ContinueWhenAny：一组任务全部完成 / 任一完成后做汇总或竞速；
    /// 3) FromAsync：把旧的 APM 模式（BeginXxx/EndXxx）包装成 Task；
    /// 4) AttachedToParent 子任务：父任务自动等待所有附加子任务；
    /// 5) 需要给一批任务统一 CancellationToken/Scheduler 时定制 TaskFactory。
    /// 【使用步骤】
    /// 1) 直接用 Task.Factory 的静态工厂，或 new TaskFactory(token, creationOptions, continuationOptions, scheduler)；
    /// 2) 调 StartNew（可传 state、options、scheduler）创建任务；
    /// 3) 用 ContinueWhenAll/ContinueWhenAny 挂组合延续；
    /// 4) StartNew 返回 Task&lt;Task&gt;（异步委托）时用 Unwrap 解包，或改用 Task.Run。
    /// 【注意事项与坑】
    /// - StartNew 是"危险的老牌 API"：传 async 委托得到的是 Task&lt;Task&gt;，默认不会解包，
    ///   Wait 它只等到"异步工作启动"而非完成；要么 Unwrap()，要么直接用 Task.Run；
    /// - StartNew 默认创建选项 None（Task.Run 默认 DenyChildAttach），AttachedToParent 行为有差异；
    /// - ContinueWhenAll 会等待所有任务结束（含出错/取消），延续内自行检查 antecedents 状态；
    /// - FromAsync 必须传入匹配的 Begin/End 对，EndXxx 中重新抛出异步异常。
    /// 【版本可用性】net48 全量可用：StartNew(多重重载)、ContinueWhenAll、ContinueWhenAny、FromAsync(多重重载)、
    /// Unwrap（TaskExtensions，4.0/4.5）、TaskFactory 与 TaskFactory&lt;TResult&gt; 构造函数。
    /// </summary>
    public static class HTaskFactoryHelp
    {
        /// <summary>示例1：StartNew 启动一个带返回值的工作项（等价 Task.Run，但可指定创建选项）</summary>
        /// <returns>结果为 "done" 的任务</returns>
        public static Task<string> StartNewWork()
        {
            return Task.Factory.StartNew(delegate
            {
                return "done";
            });
        }

        /// <summary>示例2：StartNew 带状态对象、取消令牌与创建选项（避免闭包分配，状态可从 task.AsyncState 取回）</summary>
        /// <param name="work">接收状态对象的工作</param>
        /// <param name="state">透传给任务的状态</param>
        /// <param name="token">取消令牌</param>
        /// <param name="options">创建选项，如 LongRunning/PreferFairness</param>
        /// <returns>已启动的 Task</returns>
        public static Task StartNewWithState(Action<object> work, object state,
                                             CancellationToken token, TaskCreationOptions options)
        {
            return Task.Factory.StartNew(delegate (object s) { work(s); }, state, token, options,
                                         TaskScheduler.Default);
        }

        /// <summary>示例3：定制工厂：统一取消令牌、创建选项、延续选项与调度器，后续创建都套用该模板</summary>
        /// <param name="token">工厂内所有任务共享的取消令牌</param>
        /// <returns>定制后的 TaskFactory</returns>
        public static TaskFactory CreateCustomFactory(CancellationToken token)
        {
            return new TaskFactory(token,
                                   TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
                                   TaskContinuationOptions.None,
                                   TaskScheduler.Default);
        }

        /// <summary>示例4：ContinueWhenAll：一组任务全部完成后执行延续（fan-out 后汇总）</summary>
        /// <param name="tasks">要等待的任务数组</param>
        /// <param name="continuation">全部结束后执行的动作，参数为任务数组</param>
        /// <returns>延续任务</returns>
        public static Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuation)
        {
            return Task.Factory.ContinueWhenAll(tasks, continuation);
        }

        /// <summary>示例5：ContinueWhenAll 带返回值版本（TaskFactory&lt;TResult&gt;.ContinueWhenAll）</summary>
        /// <typeparam name="TResult">延续的返回类型</typeparam>
        /// <param name="tasks">要等待的任务数组</param>
        /// <param name="continuation">全部结束后执行并产出结果的函数</param>
        /// <returns>产出汇总结果的任务</returns>
        public static Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuation)
        {
            return Task.Factory.ContinueWhenAll<TResult>(tasks, continuation);
        }

        /// <summary>示例6：ContinueWhenAny：任一任务完成即执行延续（多源竞速取最快）</summary>
        /// <param name="tasks">竞速任务数组</param>
        /// <param name="continuation">最先完成的任务触发的动作</param>
        /// <returns>延续任务</returns>
        public static Task ContinueWhenAny(Task[] tasks, Action<Task> continuation)
        {
            return Task.Factory.ContinueWhenAny(tasks, continuation);
        }

        /// <summary>示例7：StartNew 配合 Unwrap 处理异步委托（StartNew(async ...) 得到 Task&lt;Task&gt;，需解包）</summary>
        /// <param name="asyncWork">异步工作工厂</param>
        /// <returns>解包后的内部 Task</returns>
        public static Task StartNewAsync(Func<Task> asyncWork)
        {
            // Task.Factory.StartNew(asyncWork) 的类型是 Task<Task>，Unwrap 后才是真正的异步工作
            return Task.Factory.StartNew(asyncWork).Unwrap();
        }

        /// <summary>示例8：AttachedToParent：在父任务内启动"附加子任务"，父任务会等所有附加子任务结束才算完成</summary>
        /// <param name="childWork">子任务工作</param>
        /// <returns>父任务（Wait 它会连带等待附加子任务）</returns>
        public static Task StartAttachedToParent(Action childWork)
        {
            Task parent = Task.Factory.StartNew(delegate
            {
                // 注意：仅 StartNew(None) 下附加有效；Task.Run 默认 DenyChildAttach 会剥掉附加关系
                Task.Factory.StartNew(childWork, TaskCreationOptions.AttachedToParent);
            });
            parent.Wait(1000);    // 父任务等待附加子任务；带超时保证自测不卡死
            return parent;
        }

        /// <summary>示例9：FromAsync：把 APM（BeginInvoke/EndInvoke）包装成 Task（旧接口接入 TPL 的标准桥接）</summary>
        /// <returns>异步结果字符串（本示例为 "apm-result"）</returns>
        public static string FromApmExample()
        {
            Func<string> func = delegate () { return "apm-result"; };
            // BeginInvoke 签名 (AsyncCallback, object) 与 EndInvoke(IAsyncResult) 正好匹配 FromAsync 重载
            Task<string> task = Task.Factory.FromAsync(func.BeginInvoke, func.EndInvoke, null);
            task.Wait(1000);
            return task.Result;
        }
    }
}
