using System;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="AggregateException"/> 帮助类：一个 Task 可能承载【多个并行发生】的异常，
    /// TPL 用 AggregateException 把它们打包（它本身也是 Exception，可再嵌套）。四个核心成员：
    /// 1) <see cref="AggregateException.InnerExceptions"/>：<see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/>
    ///    只读集合，装着本层全部内部异常（不要用基类的 InnerException，那只是其中第一个）；
    /// 2) <see cref="AggregateException.Flatten"/>：递归去掉所有嵌套的 AggregateException“包装层”，
    ///    返回一个扁平 AggregateException（叶子业务异常全部提到同一层）；
    /// 3) <see cref="AggregateException.Handle(Func{Exception, bool})"/>：逐个遍历内部异常，
    ///    谓词对【已处理】的异常返回 true；返回 false 的异常会被收集进一个【新的 AggregateException 重新抛出】
    ///    （实测：即使只剩一个未处理异常，抛出的仍然是 AggregateException，其 InnerExceptions 只含未处理者）；
    /// 4) <see cref="AggregateException.GetBaseException"/>：沿唯一内部异常链下钻到底，返回最内层根异常，
    ///    适合“我只关心根因、不关心个数”的日志场景；多分支时取第一个分支的根。
    ///
    /// 【触发来源】（什么情况下会遇到 AggregateException）
    /// 1) 同步阻塞：<see cref="Task.Wait()"/>、<see cref="Task{TResult}.Result"/>、<see cref="Task.WaitAll(Task[])"/>、
    ///    <see cref="Task.WaitAny(Task[])"/>——失败时直接向阻塞线程抛 AggregateException；
    /// 2) 属性观察：读 <see cref="Task.Exception"/>【不会抛异常】，直接拿到 AggregateException（任务未失败时为 null）；
    /// 3) <see cref="Task.WhenAll(Task[])"/> 多任务同时失败：聚合任务携带【全部】异常，
    ///    但【await 它只会解包抛出第一个】内部异常，其余异常不会丢——读 Task.Exception.InnerExceptions 可取全；
    /// 4) 直接 await 单个失败 Task：默认【解包单层】，catch 到的就是原始业务异常，看不到 AggregateException；
    /// 5) 生产推荐的“源头观察”：task.ContinueWith(t =&gt; 处理 t.Exception, TaskContinuationOptions.OnlyOnFaulted)，
    ///    在任务出错那一刻观察，不阻塞、不依赖 GC；
    /// 6) 从不观察的故障任务：GC 终结时触发 <see cref="TaskScheduler.UnobservedTaskException"/>
    ///    （net45+ 默认不再终止进程），全局兜底处理详见 HThreadExceptionHelp。
    ///
    /// 【是否跨进程】否。AggregateException 只是进程内的异常容器；序列化跨 AppDomain/远程后会被重新水合，
    /// 内部异常集合可能变化，不能把它当作跨进程错误协议。
    ///
    /// 【典型适用场景】
    /// 1) Task.WhenAll 批量并发后需要统计/分类处理【每一个】子任务失败（Flatten + Handle）；
    /// 2) Parallel.Invoke/PLINQ 批量项中多个元素同时抛异常，需要把可容忍的错误吞掉、其余重抛；
    /// 3) 日志中间件只打根因：GetBaseException 一路下钻；
    /// 4) 同步桥接代码（.Wait()/.Result）处集中翻译 TPL 异常为业务错误码。
    ///
    /// 【使用步骤】
    /// 1) 在同步捕获处 catch (AggregateException agex)；
    /// 2) 只需根因日志：agex.GetBaseException()；需要逐一分拣：先 agex.Flatten() 再 Handle/遍历 InnerExceptions；
    /// 3) Handle 谓词内：认识且已处置的异常返回 true；不认识的返回 false（框架会重新抛出未处理集合）；
    /// 4) WhenAll 场景不要只靠 await 取异常，另读聚合任务的 Task.Exception.InnerExceptions 保证不丢错；
    /// 5) 库代码优先用 ContinueWith(OnlyOnFaulted) 在源头观察，避免 UnobservedTaskException。
    ///
    /// 【注意事项与坑】
    /// 1) await 只解包【第一个】内部异常：多个子任务失败时其余异常仍挂在 Task.Exception 上，必须显式观察，
    ///    否则可能演变成 UnobservedTaskException；
    /// 2) await 拿到的是【原始异常】，同步 Wait/Result 拿到的是 AggregateException——同一条异步链两种观察方式
    ///    catch 类型不同，catch 块别写错；
    /// 3) Handle 谓词本身抛异常会立刻向外传播（不会被当作“未处理异常”收集）；
    /// 4) Flatten 只移除【嵌套的 AggregateException 包装层】，不会解包普通包装异常（如 TargetInvocationException），
    ///    那类要用 GetBaseException 或手工剥 InnerException；
    /// 5) 不要把业务异常手工 new AggregateException 再交给 TaskCompletionSource.SetException：await 会多剥一层，
    ///    直接传业务异常本身（见 HTaskCompletionSourceHelp）。
    ///
    /// 【版本可用性】AggregateException、InnerExceptions、Flatten、Handle、GetBaseException 均为
    /// .NET Framework 4.0+（net48 全量可用）；Task.WhenAll/await 解包语义为 4.5+；.NET Core/.NET 5+ 行为一致。
    /// </summary>
    /// <example>
    /// Flatten 后用 Handle 分拣：认识的异常吞掉，其余重抛（XML 中泛型尖括号需转义为 &amp;lt; / &amp;gt;）：
    /// <code>
    /// try
    /// {
    ///     Task all = Task.WhenAll(faultedTask1, faultedTask2);
    ///     all.Wait(1000);                       // 同步阻塞处才会抛 AggregateException
    /// }
    /// catch (AggregateException agex)
    /// {
    ///     agex.Flatten().Handle(delegate(Exception ex)
    ///     {
    ///         if (ex is TimeoutException)      // 可容忍：记日志后视为已处理
    ///         {
    ///             return true;
    ///         }
    ///         return false;                    // 未处理：框架打包成新 AggregateException 重抛
    ///     });
    /// }
    /// </code>
    /// </example>
    public static class HAggregateExceptionHelp
    {
        /// <summary>
        /// 示例1：构造两层嵌套 AggregateException 并调 <see cref="AggregateException.Flatten"/> 去包装层。
        /// 外层直接挂 2 个内部异常：一个叶子异常 + 一个内层 AggregateException（内层再挂 1 个叶子）；
        /// Flatten 后两个叶子异常被提到同一层。
        /// </summary>
        /// <returns>Flatten 后内部异常总数，恒为 2。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出任何异常：所有异常仅在方法内构造与统计。</exception>
        public static int FlattenNestedAggregate()
        {
            // 外层：叶子 InvalidOperationException + 内层 AggregateException(叶子 Exception)
            AggregateException nested = new AggregateException(
                new InvalidOperationException(HTranslation.GetContent("异常A")),
                new AggregateException(new Exception(HTranslation.GetContent("异常B"))));

            // Flatten 递归剥掉“内层 AggregateException”这个包装：两个叶子异常被提到同一层
            AggregateException flat = nested.Flatten();

            // 扁平化后总数应为 2：InvalidOperationException 与 Exception 各一
            return flat.InnerExceptions.Count;
        }

        /// <summary>
        /// 示例2：<see cref="AggregateException.Handle(Func{Exception, bool})"/> 的“认识就处理、不认识就重抛”契约。
        /// 谓词对 <see cref="ArgumentException"/> 返回 true（已处理），对 <see cref="InvalidOperationException"/>
        /// 返回 false（未处理）；Handle 会把未处理异常收集进【新的 AggregateException】重新抛出，
        /// 本方法在内部 catch 住该重抛并取出被重抛的叶子异常，不外泄任何异常。
        /// </summary>
        /// <returns>被重新抛出的未处理异常的类型全名，恒为 "System.InvalidOperationException"。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出任何异常：Handle 的重抛在方法内部被捕获。</exception>
        public static string HandleKnownAndRethrowUnknown()
        {
            // 一个聚合里放两类异常：可识别处理的 ArgumentException + 不认识的 InvalidOperationException
            AggregateException aggregate = new AggregateException(
                new ArgumentException(HTranslation.GetContent("已知异常：比如参数重试后可忽略")),
                new InvalidOperationException(HTranslation.GetContent("未知异常：必须向上报告")));

            try
            {
                aggregate.Handle(delegate (Exception ex)
                {
                    if (ex is ArgumentException)
                    {
                        return true;        // true = 该异常已被本谓词处理，不会再出现在重抛集合中
                    }
                    return false;           // false = 未处理：Handle 收集所有 false 项后打包重抛
                });
                return HTranslation.GetContent("不应到达：Handle 至少有一个 false，必然重抛");
            }
            catch (AggregateException rethrown)
            {
                // 实测语义：即使只剩一个未处理异常，重抛的仍是 AggregateException（其 InnerExceptions 只含未处理者）
                Exception unhandled = rethrown.GetBaseException();

                // 取出的叶子即被重抛的 InvalidOperationException，返回其类型全名
                return unhandled.GetType().FullName;
            }
        }

        /// <summary>
        /// 示例3：<see cref="Task.WhenAll(Task[])"/> 两个任务都失败时，聚合任务携带【全部】异常；
        /// 本方法不用 await（await 只解包第一个），而是在同步等待后读
        /// <see cref="Task.Exception"/> 的 InnerExceptions，演示“多异常一个都不丢”的取法。
        /// </summary>
        /// <returns>聚合任务 Task.Exception.InnerExceptions 的条数，恒为 2。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出任何异常：Wait 抛出的聚合异常在内部被捕获并观察。</exception>
        public static int CollectWhenAllFailures()
        {
            // 两个并发任务各自抛不同异常（Task.Run 在线程池立即执行，无 I/O、无 UI）
            Task t1 = Task.Run(delegate { throw new InvalidOperationException(HTranslation.GetContent("异常X")); });
            Task t2 = Task.Run(delegate { throw new ArgumentException(HTranslation.GetContent("异常Y")); });

            Task all = Task.WhenAll(t1, t2);
            try
            {
                // 同步阻塞处：两个子任务都失败后抛 AggregateException（带超时保底，正常几十毫秒内必返回）
                all.Wait(1000);
            }
            catch (AggregateException)
            {
                // 不在此处理；统一通过 all.Exception.InnerExceptions 观察，演示“取全部异常”的标准位置
            }

            // await all 只会抛第一个（InvalidOperationException）；读 InnerExceptions 才能同时拿到两个
            return all.Exception.InnerExceptions.Count;
        }

        /// <summary>
        /// 示例4：<see cref="AggregateException.GetBaseException"/> 沿唯一内部异常链下钻到根。
        /// 构造“外层 AggregateException → 内层 AggregateException → 叶子业务异常”两层嵌套，
        /// GetBaseException 返回最内层叶子异常（不做 Flatten 也能直接拿根因，适合日志只打一行根错误）。
        /// </summary>
        /// <returns>最内层异常的类型全名，恒为 "System.TimeoutException"。</returns>
        /// <exception cref="T:System.Exception">本方法不向外抛出任何异常：所有异常仅在方法内构造。</exception>
        public static string GetBaseExceptionDemo()
        {
            // 两层嵌套：外层包内层，内层包真正的叶子异常 TimeoutException
            AggregateException twoLayers = new AggregateException(
                new AggregateException(new TimeoutException(HTranslation.GetContent("深层异常"))));

            // GetBaseException 沿“唯一内部异常”链一直下钻，跳过两层 AggregateException 包装
            Exception root = twoLayers.GetBaseException();

            // 返回最内层叶子异常的类型全名：System.TimeoutException
            return root.GetType().FullName;
        }
    }
}
