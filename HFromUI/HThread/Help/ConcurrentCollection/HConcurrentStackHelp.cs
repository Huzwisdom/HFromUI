using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HFromUI.HThread.Help.ConcurrentCollection
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// <see cref="ConcurrentStack{T}"/> 帮助类：线程安全的 LIFO（后进先出）无锁栈，
    /// 与 <see cref="ConcurrentQueue{T}"/> 一样基于无锁 CAS 算法，多线程 Push/TryPop 无需外部 lock，
    /// 并提供 PushRange / TryPopRange 批量操作（一次 CAS 区间处理多个元素，竞争更少、吞吐更高）。
    /// 文档：https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.concurrent.concurrentstack-1?view=netframework-4.8</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 仅进程内内存结构，不跨进程/机器。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 深度优先的并行任务分发、工作窃取（work stealing）调度；
    /// 2) 撤销/重做（Undo）操作暂存、递归状态的并发展开；
    /// 3) 希望“最新提交的任务最先被处理”的场景（热点数据优先）。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// new 实例 → <see cref="ConcurrentStack{T}.Push(T)"/> 压栈 / PushRange 批量压入 →
    /// <see cref="ConcurrentStack{T}.TryPop(out T)"/> 弹出 / TryPopRange 批量弹出；
    /// 只看栈顶用 <see cref="ConcurrentStack{T}.TryPeek(out T)"/>；整体副本用 ToArray()。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>顺序约定</b>：PushRange(items) 等价于按数组顺序逐个 Push，因此数组<b>最后一个元素在栈顶</b>；
    ///    TryPopRange(buffer) 把弹出结果按“栈顶在前”写入，即 buffer[0] 是最后压入的元素。
    ///    例如 PushRange([10,20,30]) 后逐个 Pop 得到 30、20、10。
    /// 2) 栈无界、Push 不阻塞，生产者过快同样会撑内存；需要限容用 BlockingCollection(new ConcurrentStack&lt;T&gt;(), 容量)。
    /// 3) TryPop/TryPeek 空栈返回 false 而非抛异常；结果参数为 default(T)。
    /// 4) Count/IsEmpty 是并发瞬时值，仅用于诊断；“排空”以 TryPop 返回 false 为准。
    /// 5) <b>net48 没有 Clear()</b>（.NET Core 之后新增），清空只能弹空或整体替换引用。
    /// 6) 批量 API 还有带 (items, startIndex, count) 的重载；批量操作是一次原子区间推进，
    ///    比“循环单元素 Push/Pop”在高并发下显著减少 CAS 重试。
    /// 7) ToArray()/枚举器是快照，遍历并发安全、不抛异常。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.0+ 内置（System.Collections.Concurrent），net48 直接可用，无需 NuGet。</para>
    /// </summary>
    ///
    /// <example>
    /// 数据就地构造、就地释放，不依赖文件/网络/UI：
    /// <code>
    /// var stack = new ConcurrentStack&lt;int&gt;();
    /// stack.Push(1);
    /// stack.PushRange(new[] { 10, 20, 30 }); // 栈顶到栈底：30,20,10,1
    /// int passed = HConcurrentStackHelp.RunSelfTest();
    /// </code>
    /// </example>
    public static class HConcurrentStackHelp
    {
        /// <summary>
        /// 压栈（原子操作）：把元素插到栈顶。
        /// 对应 <see cref="ConcurrentStack{T}.Push(T)"/>，无界、不阻塞。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="item">要压入的元素。</param>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static void Push<T>(ConcurrentStack<T> stack, T item)
        {
            // 无锁头插：新元素成为新的栈顶
            stack.Push(item);
        }

        /// <summary>
        /// 批量压栈（原子操作）：等价于按数组顺序逐个 Push，但只做一次区间推进、竞争更小。
        /// 压入后 items 的最后一个元素位于栈顶。
        /// 对应 <see cref="ConcurrentStack{T}.PushRange(T[])"/>（另有 (array,index,count) 重载）。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="items">要压入的元素数组，不能为 null；按数组顺序压入。</param>
        /// <exception cref="ArgumentNullException">stack 或 items 为 null 时抛出。</exception>
        public static void PushRange<T>(ConcurrentStack<T> stack, T[] items)
        {
            // 一次 CAS 区间操作压入整段；items[items.Length-1] 最终位于栈顶
            stack.PushRange(items);
        }

        /// <summary>
        /// 尝试弹出栈顶（原子操作）：取出并移除栈顶元素。
        /// 对应 <see cref="ConcurrentStack{T}.TryPop(out T)"/>。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="result">成功时带出栈顶元素；栈为空时为 default(T)。</param>
        /// <returns>弹出成功返回 true；栈为空返回 false（不抛异常）。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static bool TryPop<T>(ConcurrentStack<T> stack, out T result)
        {
            // 弹出最新压入的元素；空栈返回 false
            return stack.TryPop(out result);
        }

        /// <summary>
        /// 批量弹出（原子操作）：从栈顶开始尽量多弹，顺序写入 buffer（buffer[0] 为栈顶）。
        /// 对应 <see cref="ConcurrentStack{T}.TryPopRange(T[])"/>（另有 (array,index,count) 重载）。
        /// 返回值可能小于 buffer.Length（元素不够时），剩余槽位保持原值/default，使用时只取前返回值个。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="buffer">接收弹出元素的缓冲区，不能为 null；按栈顶在前顺序填充。</param>
        /// <returns>实际弹出的元素个数（0 ~ buffer.Length）；栈为空时返回 0。</returns>
        /// <exception cref="ArgumentNullException">stack 或 buffer 为 null 时抛出。</exception>
        public static int TryPopRange<T>(ConcurrentStack<T> stack, T[] buffer)
        {
            // 返回实际弹出条数；调用方必须按返回值截断使用，不能假定整个 buffer 都有效
            return stack.TryPopRange(buffer);
        }

        /// <summary>
        /// 压栈一个元素并立刻尝试弹出（单线程下必然取回同一个元素）。
        /// 用于快速演示“压栈 + 尝试弹出”这对最小操作；真实生产消费请直接用
        /// <see cref="Push{T}"/> 与 <see cref="TryPop{T}"/>。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="item">先压入的元素。</param>
        /// <param name="result">弹出得到的元素；极端并发下被其他线程抢先时为 default(T)。</param>
        /// <returns>成功弹出返回 true；栈被其他线程抢先取空时返回 false。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static bool TryPopOne<T>(ConcurrentStack<T> stack, T item, out T result)
        {
            // 先压入再立即弹出；单线程自测场景 result 即 item
            stack.Push(item);
            return stack.TryPop(out result);
        }

        /// <summary>
        /// 压入再批量弹出的组合演示：PushRange 后用 TryPopRange 取回，
        /// 展示“后入先出”与“buffer[0] 为栈顶”两个顺序约定。真实业务按需分别调用
        /// <see cref="PushRange{T}"/> / <see cref="TryPopRange{T}"/>。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="items">要压入的元素数组，不能为 null。</param>
        /// <returns>弹出的元素数组（长度等于实际弹出条数），顺序与 items 相反。</returns>
        /// <exception cref="ArgumentNullException">stack 或 items 为 null 时抛出。</exception>
        public static T[] PushAndPopRange<T>(ConcurrentStack<T> stack, T[] items)
        {
            stack.PushRange(items);                          // 整段压入
            T[] popped = new T[items.Length];
            int n = stack.TryPopRange(popped);              // 整段弹出，n 为实际条数
            T[] result = new T[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = popped[i];                      // 只截取有效部分返回
            }
            return result;
        }

        /// <summary>
        /// 查看栈顶但不弹出（原子操作）。
        /// 对应 <see cref="ConcurrentStack{T}.TryPeek(out T)"/>。
        /// Peek 与后续 Pop 是两次独立调用，高并发下“看到”和“拿到”之间可能被其他线程抢先。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <param name="top">成功时带出栈顶元素（不移除）；栈空时为 default(T)。</param>
        /// <returns>栈非空返回 true；栈为空返回 false。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static bool TryPeek<T>(ConcurrentStack<T> stack, out T top)
        {
            // 只窥探栈顶，不改变栈内容
            return stack.TryPeek(out top);
        }

        /// <summary>
        /// 当前元素数量（瞬时值）。
        /// 对应 <see cref="ConcurrentStack{T}.Count"/>，统计需要遍历内部链表，并发下仅为近似值，用于诊断。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <returns>调用瞬间的元素个数（近似值）。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static int Count<T>(ConcurrentStack<T> stack)
        {
            // 瞬时近似计数，不要用于业务判断
            return stack.Count;
        }

        /// <summary>
        /// 栈是否为空（瞬时值）。
        /// 对应 <see cref="ConcurrentStack{T}.IsEmpty"/>；排空应以 TryPop 返回 false 为准。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <returns>探测瞬间无元素返回 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static bool IsEmpty<T>(ConcurrentStack<T> stack)
        {
            // 仅表达瞬时状态
            return stack.IsEmpty;
        }

        /// <summary>
        /// 把栈当前全部元素复制到新数组（快照）。
        /// 对应 <see cref="ConcurrentStack{T}.ToArray"/>：数组按“栈顶在前”排列，不弹出任何元素，
        /// 复制期间其他线程的压入/弹出不影响已返回的数组。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <returns>调用瞬间栈内容的数组副本（栈顶在前），归调用方所有。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static T[] ToArraySnapshot<T>(ConcurrentStack<T> stack)
        {
            // 快照导出：只看不弹
            return stack.ToArray();
        }

        /// <summary>
        /// 非阻塞地排空栈：反复 TryPop 直到取不到，返回本次弹出的元素（栈顶在前，即 LIFO 顺序）。
        /// 并发生产者仍在压栈时，只保证“返回后某一刻为空”。
        /// </summary>
        /// <typeparam name="T">栈元素类型。</typeparam>
        /// <param name="stack">并发栈实例，不能为 null。</param>
        /// <returns>本次弹出的元素列表；栈原本为空时返回空列表。</returns>
        /// <exception cref="ArgumentNullException">stack 为 null 时抛出。</exception>
        public static List<T> Drain<T>(ConcurrentStack<T> stack)
        {
            List<T> list = new List<T>();
            T item;
            // 取不到即“此刻已空”，结果天然是后进先出顺序
            while (stack.TryPop(out item))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 可自测示例：就地构造栈，验证单/批量压弹顺序、Peek、快照与排空，不依赖文件/网络/UI。
        /// </summary>
        /// <returns>通过的断言条数；全部符合预期时为 14。</returns>
        public static int RunSelfTest()
        {
            ConcurrentStack<int> stack = new ConcurrentStack<int>();
            int passed = 0;

            // 1) 空栈行为
            if (IsEmpty(stack)) passed++;
            if (!TryPeek(stack, out int peekEmpty) && peekEmpty == 0) passed++;
            if (!TryPop(stack, out int popEmpty) && popEmpty == 0) passed++;

            // 2) 压入 1，再批量压 [10,20,30]：栈顶到栈底为 30,20,10,1
            Push(stack, 1);
            PushRange(stack, new[] { 10, 20, 30 });
            if (Count(stack) == 4) passed++;

            // 3) Peek 看到栈顶 30 且不移除
            if (TryPeek(stack, out int top) && top == 30 && Count(stack) == 4) passed++;

            // 4) 弹出栈顶 30
            if (TryPop(stack, out int popped30) && popped30 == 30) passed++;

            // 5) 批量弹出最多 5 个，实际剩 3 个：应按栈顶顺序得到 20,10,1
            int[] buffer = new int[5];
            int n = TryPopRange(stack, buffer);
            if (n == 3 && buffer[0] == 20 && buffer[1] == 10 && buffer[2] == 1) passed++;
            if (IsEmpty(stack) && Count(stack) == 0) passed++;

            // 6) 组合助手：压 [10,20,30] 再整段弹回，顺序必须反转
            int[] roundTrip = PushAndPopRange(stack, new[] { 10, 20, 30 });
            if (roundTrip.Length == 3 && roundTrip[0] == 30 && roundTrip[1] == 20 && roundTrip[2] == 10) passed++;
            Drain(stack);                                              // 清空，准备下一段

            // 7) 快照顺序：压 1、2 后 ToArray，栈顶在前 => [2,1]
            Push(stack, 1);
            Push(stack, 2);
            int[] snapshot = ToArraySnapshot(stack);
            if (snapshot.Length == 2 && snapshot[0] == 2 && snapshot[1] == 1 && Count(stack) == 2) passed++;

            // 8) 排空顺序为 LIFO：2 先出、1 后出
            List<int> drained = Drain(stack);
            if (drained.Count == 2 && drained[0] == 2 && drained[1] == 1) passed++;
            if (IsEmpty(stack)) passed++;

            // 9) 空栈批量弹出返回 0
            if (TryPopRange(stack, new int[3]) == 0) passed++;

            // 10) 压入+立即弹出的组合助手，单线程下取回同一个对象
            object sentinel = new object();
            ConcurrentStack<object> s2 = new ConcurrentStack<object>();
            if (TryPopOne(s2, sentinel, out object got) && object.ReferenceEquals(got, sentinel)) passed++;

            return passed;                               // 期望 14
        }
    }
}
