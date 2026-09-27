using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HFromUI.HThread.Help.ConcurrentCollection
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// <see cref="ConcurrentQueue{T}"/> 帮助类：线程安全的 FIFO（先进先出）无界队列，
    /// 采用无锁（lock-free，CAS + 自旋）算法，支持任意多生产者、任意多消费者并发 Enqueue/TryDequeue，全程无需外部 lock。
    /// 文档：https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=netframework-4.8</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 仅当前进程内的内存队列，不跨进程、不跨机器；跨进程排队请用 MSMQ/消息中间件。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 多生产者-多消费者的工作任务排队（日志事件、待处理订单）；
    /// 2) 生产者速率瞬时高于消费者时做内存缓冲；
    /// 3) 需要严格 FIFO 顺序的并发消息交接。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// new 实例 → 生产者 <see cref="ConcurrentQueue{T}.Enqueue(T)"/> →
    /// 消费者循环 <see cref="ConcurrentQueue{T}.TryDequeue(out T)"/>，返回 false 表示当前为空；
    /// 只看不取用 <see cref="ConcurrentQueue{T}.TryPeek(out T)"/>；需要整体副本用 ToArray()。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>队列无界、不会阻塞</b>：Enqueue 永不等待，生产者远快于消费者时内存会持续上涨。
    ///    需要“满则阻塞/限容”请改用 BlockingCollection（见 HBlockingCollectionHelp）；
    ///    async 场景请用 System.Threading.Channels（见 HChannelHelp）。
    /// 2) TryDequeue/TryPeek 在空队列上返回 false，<b>不抛异常</b>；空结果参数为 default(T)。
    /// 3) Count 是瞬时近似值（并发下尤其不精确），IsEmpty 同理，只能用于诊断，不能当“取空了”的业务依据；
    ///    消费者排空应以 TryDequeue 返回 false 为准。
    /// 4) 枚举器、ToArray() 返回的是快照，遍历期间其他线程增删不抛异常、也不影响快照内容。
    /// 5) <b>net48 没有 Clear() 方法</b>（Clear 是 .NET Core 之后新增）。net48 要清空只能排空，
    ///    或把字段引用整体换成新队列（引用赋值本身原子，读者可能短暂看到旧实例，可接受时使用）。
    /// 6) 不保证“先检查再操作”的组合原子性；IsEmpty 为 true 后另一线程可能立刻入队。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.0+ 内置（System.Collections.Concurrent），net48 直接可用，无需 NuGet。</para>
    /// </summary>
    ///
    /// <example>
    /// 数据就地构造、就地释放，不依赖文件/网络/UI：
    /// <code>
    /// var queue = new ConcurrentQueue&lt;int&gt;();
    /// queue.Enqueue(1);
    /// if (queue.TryDequeue(out int item)) { /* item == 1 */ }
    /// int passed = HConcurrentQueueHelp.RunSelfTest();
    /// </code>
    /// </example>
    public static class HConcurrentQueueHelp
    {
        /// <summary>
        /// 入队（原子操作）：把元素追加到队尾。并发多生产者调用安全，且不阻塞、无容量上限。
        /// 对应 <see cref="ConcurrentQueue{T}.Enqueue(T)"/>。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <param name="item">要入队的元素；引用类型是否允许 null 由 T 与调用方约定决定。</param>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static void Enqueue<T>(ConcurrentQueue<T> queue, T item)
        {
            // 无锁入队尾，多线程并发调用互不阻塞
            queue.Enqueue(item);
        }

        /// <summary>
        /// 尝试出队（原子操作）：取出并移除队首元素。
        /// 对应 <see cref="ConcurrentQueue{T}.TryDequeue(out T)"/>。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <param name="result">成功时带出队首元素；队列为空时为 default(T)。</param>
        /// <returns>成功取出返回 true；队列当前为空返回 false（不抛异常）。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static bool TryDequeue<T>(ConcurrentQueue<T> queue, out T result)
        {
            // 空队列返回 false，消费者据此决定“先去干别的/稍后重试”
            return queue.TryDequeue(out result);
        }

        /// <summary>
        /// 入队一个元素并立刻尝试出队（单线程下必然取到同一个元素）。
        /// 用于快速演示“入队 + 尝试出队”这对最小操作；真实消费者循环请直接用
        /// <see cref="TryDequeue{T}"/>。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <param name="item">先入队的元素。</param>
        /// <param name="result">出队得到的元素；极端并发下若被其他消费者抢先则为 default(T)。</param>
        /// <returns>成功出队返回 true；若两次操作之间被其他线程取空则返回 false。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static bool TryDequeueOne<T>(ConcurrentQueue<T> queue, T item, out T result)
        {
            // 先放入再立即取；单线程自测场景 result 即 item
            queue.Enqueue(item);
            return queue.TryDequeue(out result);
        }

        /// <summary>
        /// 尝试查看队首元素但不移除（原子操作）。
        /// 对应 <see cref="ConcurrentQueue{T}.TryPeek(out T)"/>。
        /// 注意 Peek 与后续 Dequeue 是两次独立调用，中间可能被其他消费者抢先，不能假定“看到就能取到”。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <param name="head">成功时带出队首元素（不移除）；队列为空时为 default(T)。</param>
        /// <returns>队列非空、查看成功返回 true；队列为空返回 false。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static bool TryPeek<T>(ConcurrentQueue<T> queue, out T head)
        {
            // 只窥探队首，不改变队列
            return queue.TryPeek(out head);
        }

        /// <summary>
        /// 当前元素数量。
        /// 对应 <see cref="ConcurrentQueue{T}.Count"/>。无锁队列的 Count 需要遍历内部段链表统计，
        /// 并发下是瞬时近似值，仅用于日志/监控/诊断。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <returns>调用瞬间的元素个数（近似值）。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static int Count<T>(ConcurrentQueue<T> queue)
        {
            // 瞬时近似计数，禁止用于“队列是否排空”等业务判断
            return queue.Count;
        }

        /// <summary>
        /// 队列是否为空（瞬时值）。
        /// 对应 <see cref="ConcurrentQueue{T}.IsEmpty"/>。只能表达“探测那一刻”的状态，
        /// 返回 true 后可能立刻有其他线程入队；排空判断请以 TryDequeue 返回 false 为准。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <returns>探测瞬间没有元素返回 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static bool IsEmpty<T>(ConcurrentQueue<T> queue)
        {
            // 仅表达瞬时状态，不构成任何“先判后取”的保证
            return queue.IsEmpty;
        }

        /// <summary>
        /// 把队列当前全部元素复制到一个新数组（快照）。
        /// 对应 <see cref="ConcurrentQueue{T}.ToArray"/>，不消费、不移除任何元素；
        /// 复制进行中其他线程的增删不影响已返回的数组。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <returns>调用瞬间队列元素的数组副本（按 FIFO 顺序），数组归调用方所有。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static T[] ToArraySnapshot<T>(ConcurrentQueue<T> queue)
        {
            // 快照导出：只读不消费，适合监控打印、批量另存
            return queue.ToArray();
        }

        /// <summary>
        /// 非阻塞地排空队列：反复 TryDequeue 直到取不到，返回本次取出的全部元素（按 FIFO 顺序）。
        /// 并发生产者仍在入队时，本方法只保证“返回后某一刻为空”，不保证之后仍为空。
        /// 需要阻塞等待新元素的消费者，请用 BlockingCollection.GetConsumingEnumerable 或 Channel。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <returns>本次排空取到的元素列表；队列原本为空时返回空列表（不返回 null）。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static List<T> Drain<T>(ConcurrentQueue<T> queue)
        {
            List<T> list = new List<T>();
            T item;
            // 取不到即视为“此刻已空”，立即结束；整个过程不阻塞线程
            while (queue.TryDequeue(out item))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 以快照方式安全遍历队列：先 ToArray 再 foreach，遍历期间其他线程增删元素既不抛异常，
        /// 也不会出现“边遍历边被消费”的混乱。与直接 foreach(queue) 等价（其枚举器也是快照）。
        /// </summary>
        /// <typeparam name="T">队列元素类型。</typeparam>
        /// <param name="queue">并发队列实例，不能为 null。</param>
        /// <returns>快照中元素的新列表，按 FIFO 顺序。</returns>
        /// <exception cref="ArgumentNullException">queue 为 null 时抛出。</exception>
        public static List<T> EnumerateSnapshot<T>(ConcurrentQueue<T> queue)
        {
            List<T> list = new List<T>();
            // 先固定快照再遍历，处理过程与队列的实时变化完全隔离
            foreach (T item in queue.ToArray())
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 可自测示例：就地构造队列，顺序演练 Enqueue/TryPeek/TryDequeue/ToArray/Drain/IsEmpty 并校验。
        /// 不依赖文件/网络/UI。
        /// </summary>
        /// <returns>通过的断言条数；全部符合预期时为 12。</returns>
        public static int RunSelfTest()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            int passed = 0;

            // 1) 空队列的各种行为
            if (IsEmpty(queue)) passed++;
            if (!TryPeek(queue, out int peekEmpty) && peekEmpty == 0) passed++;
            if (!TryDequeue(queue, out int deqEmpty) && deqEmpty == 0) passed++;
            if (Count(queue) == 0) passed++;

            // 2) 依次入队 1、2、3，验证 FIFO
            Enqueue(queue, 1);
            Enqueue(queue, 2);
            Enqueue(queue, 3);
            if (Count(queue) == 3 && !IsEmpty(queue)) passed++;

            // 3) Peek 只看不动：队首仍是 1，数量不变
            if (TryPeek(queue, out int head) && head == 1 && Count(queue) == 3) passed++;

            // 4) Dequeue 取走队首 1，剩余 2、3
            if (TryDequeue(queue, out int first) && first == 1 && Count(queue) == 2) passed++;

            // 5) ToArray 是只读快照：内容 [2,3]，队列本身不被消费
            int[] snapshot = ToArraySnapshot(queue);
            if (snapshot.Length == 2 && snapshot[0] == 2 && snapshot[1] == 3 && Count(queue) == 2) passed++;

            // 6) 快照遍历得到同样内容
            List<int> enumerated = EnumerateSnapshot(queue);
            if (enumerated.Count == 2 && enumerated[0] == 2) passed++;

            // 7) 排空：按 FIFO 拿到 2、3，之后队列为空
            List<int> drained = Drain(queue);
            if (drained.Count == 2 && drained[0] == 2 && drained[1] == 3) passed++;
            if (IsEmpty(queue) && Count(queue) == 0) passed++;

            // 8) 入队+立即出队的组合助手，单线程下取回同一个对象
            object sentinel = new object();
            ConcurrentQueue<object> q2 = new ConcurrentQueue<object>();
            if (TryDequeueOne(q2, sentinel, out object got) && object.ReferenceEquals(got, sentinel)) passed++;

            return passed;                               // 期望 12
        }
    }
}
