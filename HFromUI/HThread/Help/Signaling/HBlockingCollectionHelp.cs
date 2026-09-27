using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="BlockingCollection{T}"/> 帮助类：带阻塞与限额能力的线程安全集合，
    /// 是 .NET Framework 内置的生产者-消费者容器。它本身不实现存储，默认在内部包一个
    /// <see cref="ConcurrentQueue{T}"/>（FIFO）；构造时也可传入任意
    /// <see cref="IProducerConsumerCollection{T}"/> 实现来改变顺序：
    /// <see cref="ConcurrentStack{T}"/>（LIFO）、<see cref="ConcurrentBag{T}"/>（无序、适合同线程产消）。
    /// 核心语义：集合满时生产者 Add 阻塞、空时消费者 Take 阻塞；<see cref="BlockingCollection{T}.CompleteAdding"/>
    /// 标记“不会再有新数据”，之后 Add 抛异常，而 <see cref="BlockingCollection{T}.GetConsumingEnumerable()"/>
    /// 会在剩余数据被取空后自动结束枚举，无需手写“队列+信号+结束标志”。
    ///
    /// 【是否跨进程】否。进程内跨线程容器；需要跨进程请用命名 <see cref="EventWaitHandle"/> + 共享文件/MMF。
    ///
    /// 【典型适用场景】
    /// 1) 同步线程版生产者-消费者管线（一个或多个生产者/消费者）；
    /// 2) 用 boundedCapacity 做背压限流：生产快于消费时限满自动阻塞生产者，防止内存爆掉；
    /// 3) 多路归并/分发：静态 AddToAny/TakeFromAny 在多个集合间任选可用的一个；
    /// 4) 需要取消时使用带 <see cref="CancellationToken"/> 的 Add/Take/GetConsumingEnumerable 重载。
    /// 异步（async/await）管线请使用 Channel（见 <see cref="HChannelHelp"/>，net48 需 NuGet 包）。
    ///
    /// 【使用步骤】
    /// 1) using new BlockingCollection&lt;T&gt;(boundedCapacity) 建集合（不传容量为无界）；
    /// 2) 生产者 Add/TryAdd；消费者 Take/TryTake 或 foreach GetConsumingEnumerable；
    /// 3) 生产者结束后必须调 CompleteAdding；消费者取空后枚举自动退出；
    /// 4) 用 IsAddingCompleted / IsCompleted 判断阶段，禁止靠 Count 做业务判断；
    /// 5) using 结束 Dispose（必须释放）。
    ///
    /// 【注意事项与坑】
    /// 1) BlockingCollection 没有 ToEnumerable() 方法：直接 foreach 集合得到的是“某一时刻快照”，
    ///    不消费、不阻塞；要边阻塞边消费必须用 GetConsumingEnumerable；
    /// 2) CompleteAdding 之后再 Add/TryAdd 抛 <see cref="InvalidOperationException"/>；
    ///    已完成且取空后再 Take 抛 InvalidOperationException（消费循环应优先用枚举或 TryTake）；
    /// 3) TryAdd/TryTake 超时返回 false（集合满/空），不是错误；
    /// 4) 有界集合满时 Add 会无限期阻塞，关停管线时先 CompleteAdding 或用带 token 的 Add，
    ///    否则 Dispose 时仍有线程阻塞在集合上会引发异常；
    /// 5) 多集合 AddToAny/TakeFromAny：返回实际操作的集合下标，-1 表示所有集合都不可用（Try 版超时）；
    /// 6) Count/BoundedCapacity 仅用于监控诊断，并发下是瞬时值，不能作为并发控制依据；
    /// 7) 实现了 <see cref="IDisposable"/>，必须 Dispose；不要在仍有阻塞线程时释放。
    ///
    /// 【版本可用性】.NET Framework 4.0 起（System.dll / System.Collections.Concurrent 命名空间）；
    /// .NET Core/.NET 5+ 均内置。
    /// </summary>
    /// <example>
    /// 有界背压的一生产者一消费者（完整生命周期）：
    /// <code>
    /// using (BlockingCollection&lt;int&gt; bc = new BlockingCollection&lt;int&gt;(2))   // 容量 2：满则生产者阻塞
    /// {
    ///     Thread consumer = new Thread((ThreadStart)delegate
    ///     {
    ///         foreach (int item in bc.GetConsumingEnumerable())   // 空时阻塞；完成且取空后退出
    ///         {
    ///             Process(item);
    ///         }
    ///     }) { IsBackground = true };
    ///     consumer.Start();
    ///
    ///     for (int i = 0; i &lt; 10; i++) bc.Add(i);   // 背压：消费者没取走就自动等待
    ///     bc.CompleteAdding();                        // 必须声明“生产结束”
    ///     consumer.Join(5000);                        // 带超时 Join，自测不挂死
    /// }
    /// </code>
    /// </example>
    public static class HBlockingCollectionHelp
    {
        /// <summary>
        /// 创建无界阻塞集合（无参构造，默认底层为 <see cref="ConcurrentQueue{T}"/>，FIFO）。
        /// 集合永不满、Add 永不阻塞，注意用无限流可能耗尽内存。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <returns>无界 BlockingCollection；调用方负责 Dispose。</returns>
        public static BlockingCollection<T> CreateUnbounded<T>()
        {
            return new BlockingCollection<T>();
        }

        /// <summary>
        /// 创建有界阻塞集合（<see cref="BlockingCollection{T}(int)"/>，默认 FIFO），容量满后 Add 阻塞，形成背压。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="boundedCapacity">容量上限，必须 ≥ 1。</param>
        /// <returns>有界 BlockingCollection；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundedCapacity"/> 小于 1。</exception>
        public static BlockingCollection<T> CreateBounded<T>(int boundedCapacity)
        {
            return new BlockingCollection<T>(boundedCapacity);
        }

        /// <summary>
        /// 用指定底层存储创建阻塞集合（<see cref="BlockingCollection{T}(IProducerConsumerCollection{T}, int)"/>），
        /// 传 <see cref="ConcurrentStack{T}"/> 得 LIFO、<see cref="ConcurrentBag{T}"/> 得无序集合；
        /// boundedCapacity 传 <see cref="int.MaxValue"/> 表示无界。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="backingStore">底层生产者-消费者集合，不能为 null；其初始元素计入容量。</param>
        /// <param name="boundedCapacity">容量上限，必须 ≥ 1，且不小于底层集合现有元素数。</param>
        /// <returns>使用指定存储的 BlockingCollection；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="backingStore"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundedCapacity"/> 小于 1。</exception>
        /// <exception cref="ArgumentException"><paramref name="boundedCapacity"/> 小于底层集合现有元素数。</exception>
        public static BlockingCollection<T> CreateWithBackingStore<T>(IProducerConsumerCollection<T> backingStore, int boundedCapacity)
        {
            if (backingStore == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("后备存储不能为空"));
            }
            return new BlockingCollection<T>(backingStore, boundedCapacity);
        }

        /// <summary>
        /// 阻塞式添加（<see cref="BlockingCollection{T}.Add(T)"/>）：有界集合满时等待空位；
        /// 已 CompleteAdding 后调用抛 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <param name="item">要添加的元素。</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException">集合已标记 CompleteAdding 或容量不足且已释放。</exception>
        /// <exception cref="ObjectDisposedException">集合已 Dispose。</exception>
        public static void Add<T>(BlockingCollection<T> collection, T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            collection.Add(item);
        }

        /// <summary>
        /// 带毫秒超时的尝试添加（<see cref="BlockingCollection{T}.TryAdd(T, int)"/>）：
        /// 超时仍满或已完成添加时返回 false，不抛异常、不阻塞调用方。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <param name="item">要添加的元素。</param>
        /// <param name="timeoutMs">等待空位的毫秒数；0 表示不等待，-1 表示无限等待。</param>
        /// <returns>添加成功 true；超时/已完成添加返回 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        public static bool TryAdd<T>(BlockingCollection<T> collection, T item, int timeoutMs)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.TryAdd(item, timeoutMs);
        }

        /// <summary>
        /// 阻塞式取出（<see cref="BlockingCollection{T}.Take()"/>）：集合空时等待新元素。
        /// 集合已 CompleteAdding 且被取空（IsCompleted）后调用抛 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">源集合，不能为 null。</param>
        /// <returns>取出的元素。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException">集合已完成添加且已无元素。</exception>
        public static T Take<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.Take();
        }

        /// <summary>
        /// 带毫秒超时的尝试取出（<see cref="BlockingCollection{T}.TryTake(T, int)"/>）：
        /// 超时仍为空返回 false。CompleteAdding 且取空后也返回 false。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">源集合，不能为 null。</param>
        /// <param name="timeoutMs">等待新元素的毫秒数；0 不等待，-1 无限等待。</param>
        /// <param name="item">输出：取到的元素；失败时为类型默认值。</param>
        /// <returns>取到元素为 true；超时/已取空为 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        public static bool TryTake<T>(BlockingCollection<T> collection, int timeoutMs, out T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.TryTake(out item, timeoutMs);
        }

        /// <summary>
        /// 声明生产结束（<see cref="BlockingCollection{T}.CompleteAdding"/>）。此后不能再 Add；
        /// 消费者枚举完剩余元素后自动结束。重复调用不会抛异常（以第一次为准）。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">集合已 Dispose。</exception>
        public static void CompleteAdding<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            collection.CompleteAdding();
        }

        /// <summary>
        /// 查询是否已调用 CompleteAdding（<see cref="BlockingCollection{T}.IsAddingCompleted"/>）。
        /// 为 true 时可能还有未消费的剩余元素。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <returns>生产已结束返回 true。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        public static bool IsAddingCompleted<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.IsAddingCompleted;
        }

        /// <summary>
        /// 查询是否“已完成添加且集合已取空”（<see cref="BlockingCollection{T}.IsCompleted"/>）。
        /// 为 true 时消费枚举已无数据可取。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <returns>生产结束且消费完毕返回 true。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        public static bool IsCompleted<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.IsCompleted;
        }

        /// <summary>
        /// 查询容量上限（<see cref="BlockingCollection{T}.BoundedCapacity"/>）；无界集合返回 <see cref="int.MaxValue"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <returns>有界容量；无界为 <see cref="int.MaxValue"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        public static int GetBoundedCapacity<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.BoundedCapacity;
        }

        /// <summary>
        /// 查询当前元素数（<see cref="BlockingCollection{T}.Count"/>）。并发下是瞬时近似值，仅用于诊断，
        /// 不要据此做 Add/Take 决策（应直接用 TryAdd/TryTake）。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">目标集合，不能为 null。</param>
        /// <returns>调用瞬间集合中的元素数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        public static int GetCount<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.Count;
        }

        /// <summary>
        /// 获取当前元素的非消费快照（<see cref="BlockingCollection{T}.ToArray"/>）：不阻塞、不移除元素。
        /// 再次强调：本类型没有 ToEnumerable()，直接 foreach 集合也是同样的快照语义；
        /// 需要“边消费边阻塞等待”必须用 <see cref="BlockingCollection{T}.GetConsumingEnumerable()"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">源集合，不能为 null。</param>
        /// <returns>调用瞬间元素的新数组副本。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">集合已 Dispose。</exception>
        public static T[] SnapshotToArray<T>(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.ToArray();
        }

        /// <summary>
        /// 可取消的消费枚举（<see cref="BlockingCollection{T}.GetConsumingEnumerable(CancellationToken)"/>）：
        /// 空时阻塞等待，CompleteAdding 且取空后结束；令牌取消时枚举抛出
        /// <see cref="OperationCanceledException"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">源集合，不能为 null。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>消费型可枚举序列；每次 MoveNext 都可能阻塞等待新元素。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException">枚举期间令牌被取消。</exception>
        public static IEnumerable<T> GetConsumingEnumerable<T>(BlockingCollection<T> collection, CancellationToken token)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            return collection.GetConsumingEnumerable(token);
        }

        /// <summary>
        /// 阻塞式“任选一个集合添加”（<see cref="BlockingCollection{T}.AddToAny(BlockingCollection{T}[], T)"/>）：
        /// 任一集合有空位即加入，返回该集合下标；全部满则阻塞。典型用于分片/多队列分流。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collections">候选集合数组，不能为 null/空，元素不能为 null。</param>
        /// <param name="item">要添加的元素。</param>
        /// <returns>实际加入的集合在数组中的下标。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collections"/> 或其中元素为 null。</exception>
        /// <exception cref="ArgumentException"><paramref name="collections"/> 为空数组。</exception>
        /// <exception cref="InvalidOperationException">所有集合都已 CompleteAdding。</exception>
        public static int AddToAny<T>(BlockingCollection<T>[] collections, T item)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合数组不能为空"));
            }
            return BlockingCollection<T>.AddToAny(collections, item);
        }

        /// <summary>
        /// 带超时的“任选一个集合尝试添加”（TryAddToAny）：超时内全部满/不可用返回 -1。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collections">候选集合数组。</param>
        /// <param name="item">要添加的元素。</param>
        /// <param name="timeoutMs">等待毫秒；0 不等待，-1 无限等待。</param>
        /// <returns>实际加入的集合下标；全部不可用且超时返回 -1。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collections"/> 或其中元素为 null。</exception>
        public static int TryAddToAny<T>(BlockingCollection<T>[] collections, T item, int timeoutMs)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合数组不能为空"));
            }
            return BlockingCollection<T>.TryAddToAny(collections, item, timeoutMs);
        }

        /// <summary>
        /// 阻塞式“任选一个集合取出”（<see cref="BlockingCollection{T}.TakeFromAny(BlockingCollection{T}[], T)"/>）：
        /// 任一集合有元素即取出，返回该集合下标；全部空则阻塞。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collections">候选集合数组。</param>
        /// <param name="item">输出：取到的元素。</param>
        /// <returns>元素来源集合的下标。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collections"/> 或其中元素为 null。</exception>
        /// <exception cref="InvalidOperationException">所有集合都已完成并取空。</exception>
        public static int TakeFromAny<T>(BlockingCollection<T>[] collections, out T item)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合数组不能为空"));
            }
            return BlockingCollection<T>.TakeFromAny(collections, out item);
        }

        /// <summary>
        /// 带超时的“任选一个集合尝试取出”（TryTakeFromAny）：超时内全部空返回 -1。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collections">候选集合数组。</param>
        /// <param name="timeoutMs">等待毫秒；0 不等待，-1 无限等待。</param>
        /// <param name="item">输出：取到的元素；失败时为默认值。</param>
        /// <returns>元素来源集合下标；全部为空且超时返回 -1。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collections"/> 或其中元素为 null。</exception>
        public static int TryTakeFromAny<T>(BlockingCollection<T>[] collections, int timeoutMs, out T item)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合数组不能为空"));
            }
            return BlockingCollection<T>.TryTakeFromAny(collections, out item, timeoutMs);
        }

        /// <summary>
        /// 示例：一个生产者、一个消费者的标准用法，容量上限形成背压；
        /// 自行 CompleteAdding、Join 带 10 秒超时、using 保证 Dispose，任何分支都不会让调用挂死。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="boundedCapacity">容量上限（背压阈值），必须 ≥ 1。</param>
        /// <param name="consume">消费处理回调，不能为 null。</param>
        /// <param name="source">生产数据源，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="consume"/> 或 <paramref name="source"/> 为 null。</exception>
        public static void ProducerConsumer<T>(int boundedCapacity, Action<T> consume, IEnumerable<T> source)
        {
            if (consume == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("消费委托不能为空"));
            }
            if (source == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("源不能为空"));
            }

            using (BlockingCollection<T> collection = new BlockingCollection<T>(boundedCapacity))
            {
                // 消费者线程：集合空时枚举阻塞；CompleteAdding 且取空后枚举结束
                Thread consumer = new Thread((ThreadStart)delegate
                {
                    foreach (T item in collection.GetConsumingEnumerable())
                    {
                        consume(item);
                    }
                }) { IsBackground = true };
                consumer.Start();

                try
                {
                    // 生产者：集合满时 Add 自动阻塞，等消费者取走（背压）
                    foreach (T item in source)
                    {
                        collection.Add(item);
                    }
                }
                finally
                {
                    collection.CompleteAdding();   // 无论正常/异常都声明不会再有新数据
                }

                consumer.Join(10000);             // 带超时 Join，杜绝自测挂死
            }
        }

        /// <summary>
        /// 启动多个消费者共享同一集合（BlockingCollection 原生支持多线程并行消费，元素不会重复分发）。
        /// 注意：本方法只负责启动；调用方必须在结束时对集合 CompleteAdding，并自行 Dispose 集合，
        /// 否则消费者线程会永久阻塞在枚举上（它们是后台线程，进程退出时才会终止）。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="collection">共享集合，不能为 null。</param>
        /// <param name="consumerCount">消费者线程数，建议 ≥ 1。</param>
        /// <param name="consume">每个元素的处理回调，不能为 null。</param>
        /// <returns>已启动的消费者线程数组，便于调用方 Join 等待。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 或 <paramref name="consume"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="consumerCount"/> 小于 1。</exception>
        public static Thread[] StartConsumers<T>(BlockingCollection<T> collection, int consumerCount, Action<T> consume)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("集合不能为空"));
            }
            if (consume == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("消费委托不能为空"));
            }
            if (consumerCount < 1)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("消费者数量不能小于 1"));
            }

            Thread[] threads = new Thread[consumerCount];
            for (int i = 0; i < consumerCount; i++)
            {
                threads[i] = new Thread((ThreadStart)delegate
                {
                    // 多个消费者同时枚举同一个集合：每个元素只会被其中一个取走
                    foreach (T item in collection.GetConsumingEnumerable())
                    {
                        consume(item);
                    }
                }) { IsBackground = true };
                threads[i].Start();
            }
            return threads;
        }

        /// <summary>
        /// 完整自包含的“生产者-消费者 + 背压”示例：容量仅 2，生产 10 个元素；
        /// 消费者每取一个稍作停顿，生产者的 Add 会反复被容量上限阻塞。
        /// 自行 CompleteAdding、两个线程均带 5000ms Join、using 自动 Dispose，可安全反复自测。
        /// </summary>
        /// <returns>实际消费到的元素数，正常为 10。</returns>
        public static int BackPressureExample()
        {
            int consumed = 0;
            using (BlockingCollection<int> collection = new BlockingCollection<int>(2))   // 小容量放大背压效果
            {
                Thread producer = new Thread((ThreadStart)delegate
                {
                    try
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            collection.Add(i);        // 满 2 个就阻塞，直到消费者取走
                        }
                    }
                    finally
                    {
                        collection.CompleteAdding();  // 必须声明生产结束
                    }
                }) { IsBackground = true };

                Thread consumer = new Thread((ThreadStart)delegate
                {
                    foreach (int item in collection.GetConsumingEnumerable())
                    {
                        Thread.Sleep(10);             // 故意消费慢一点，迫使生产者被背压阻塞
                        Interlocked.Increment(ref consumed);
                    }
                }) { IsBackground = true };

                producer.Start();
                consumer.Start();

                bool producerDone = producer.Join(5000);
                bool consumerDone = consumer.Join(5000);

                // 防御性兜底：若极端情况下 Join 超时，也要确保枚举能退出（正常路径不会走到）
                if (!producerDone || !consumerDone)
                {
                    try { if (!collection.IsAddingCompleted) { collection.CompleteAdding(); } }
                    catch (ObjectDisposedException) { /* 已释放则忽略 */ }
                }

                return consumed;
            }
        }

        /// <summary>
        /// 演示同一 BlockingCollection 切换底层存储带来的顺序差异：
        /// 队列 FIFO（先进先出）、栈 LIFO（后进先出）、Bag 无序。每次新建集合并立即取空，安全无阻塞。
        /// </summary>
        /// <returns>FIFO 与 LIFO 两种存储取空后首元素分别符合预期时返回 true。</returns>
        public static bool BackingStoreOrderExample()
        {
            // FIFO：底层 ConcurrentQueue（也是默认行为）
            using (BlockingCollection<int> fifo = CreateWithBackingStore(new ConcurrentQueue<int>(), int.MaxValue))
            {
                fifo.Add(1);
                fifo.Add(2);
                fifo.Add(3);
                fifo.CompleteAdding();
                int first = fifo.Take();              // 1（最先进入最先取出）
                if (first != 1)
                {
                    return false;
                }
            }

            // LIFO：底层 ConcurrentStack
            using (BlockingCollection<int> lifo = CreateWithBackingStore(new ConcurrentStack<int>(), int.MaxValue))
            {
                lifo.Add(1);
                lifo.Add(2);
                lifo.Add(3);
                lifo.CompleteAdding();
                int first = lifo.Take();              // 3（最后进入最先取出）
                if (first != 3)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
