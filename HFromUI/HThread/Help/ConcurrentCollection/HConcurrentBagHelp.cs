using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ConcurrentCollection
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// <see cref="ConcurrentBag{T}"/> 帮助类：线程安全的<b>无序</b>元素袋。
    /// 内部为每个访问线程维护一个<b>线程本地队列 + 跨线程工作窃取（work stealing）</b>结构：
    /// 同一线程又存又取时只操作本地队列，几乎无锁竞争；本地空了再去偷别的线程队列里的元素。
    /// 文档：https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.concurrent.concurrentbag-1?view=netframework-4.8</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 仅进程内内存结构，不跨进程/机器。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) <see cref="Parallel"/> / Task 并行循环中收集结果（各线程写自己的本地分区，几乎无竞争）；
    /// 2) 不关心消费顺序的并行工作池、无序结果聚合；
    /// 3) “线程私有暂存、空闲时互相帮忙偷任务”的工作窃取调度。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// new 实例 → 并行任务里 <see cref="ConcurrentBag{T}.Add(T)"/> →
    /// 消费端 <see cref="ConcurrentBag{T}.TryTake(out T)"/>；只看不取 TryPeek；整体导出 ToArray()。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>无序</b>：不保证任何 FIFO/LIFO 顺序（同线程存取时呈现近似栈的行为，但不要依赖）。
    ///    要顺序请选 <see cref="ConcurrentQueue{T}"/> / <see cref="ConcurrentStack{T}"/>。
    /// 2) <b>线程亲和性陷阱</b>：bag 按线程建本地队列。如果“很多线程 Add、只有另一个长期存活的单独消费者线程 TryTake”，
    ///    消费者要不停跨线程窃取，数据结构还会随线程数累积本地队列——bag 在这种“多产单消且消费线程从不生产”的形态下并不优；
    ///    它最强的形态是<b>同一批线程既产又消</b>（如 Parallel 循环里各取各的）。
    ///    net48 上还需注意：线程退出后其遗留元素可被窃取，但若进程生命周期内频繁创建大量 Add 完就退出的线程，回收代价较高。
    /// 3) TryTake/TryPeek 空袋返回 false 而非抛异常。
    /// 4) <b>Count 很重</b>：要获取并累加所有线程本地队列的锁，高频调用代价大，仅用于诊断；IsEmpty 同样不轻。
    /// 5) ToArray()/枚举器是快照，并发安全、不抛异常，顺序无定义。
    /// 6) <b>net48 没有 Clear()</b>（.NET Core 之后新增），清空只能取空或整体替换引用。
    /// 7) 无界、Add 不阻塞，生产者远快于消费者会持续占用内存；要限容/阻塞用 BlockingCollection 包 ConcurrentBag。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.0+ 内置（System.Collections.Concurrent），net48 直接可用，无需 NuGet。</para>
    /// </summary>
    ///
    /// <example>
    /// 数据就地构造、就地释放，不依赖文件/网络/UI：
    /// <code>
    /// var bag = new ConcurrentBag&lt;int&gt;();
    /// Parallel.For(0, 100, i => bag.Add(i * i)); // 各线程写本地分区，几乎无竞争
    /// int total = 0; while (bag.TryTake(out int x)) total += x; // 合并不关心顺序
    /// int passed = HConcurrentBagHelp.RunSelfTest();
    /// </code>
    /// </example>
    public static class HConcurrentBagHelp
    {
        /// <summary>
        /// 添加元素（原子操作）：写入当前线程的本地队列，同线程随后 TryTake 几乎无竞争；
        /// 其他线程在自己本地为空时可通过工作窃取取走它。
        /// 对应 <see cref="ConcurrentBag{T}.Add(T)"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <param name="item">要加入的元素。</param>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static void Add<T>(ConcurrentBag<T> bag, T item)
        {
            // 优先写线程本地队列：bag 高吞吐的关键来源
            bag.Add(item);
        }

        /// <summary>
        /// 尝试取出并移除一个元素（原子操作）：优先取当前线程本地队列的元素，本地为空时去窃取其他线程的。
        /// 对应 <see cref="ConcurrentBag{T}.TryTake(out T)"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <param name="item">成功时带出取出的元素；袋为空时为 default(T)；具体取到谁无序。</param>
        /// <returns>成功取出返回 true；袋为空返回 false（不抛异常）。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static bool TryTake<T>(ConcurrentBag<T> bag, out T item)
        {
            // 先取本地、本地空再窃取；返回元素顺序无定义
            return bag.TryTake(out item);
        }

        /// <summary>
        /// 加入一个元素并立刻尝试取出（单线程下会取回本地元素，但具体值与顺序不要做假设）。
        /// 用于演示 Add + TryTake 这对最小操作；真实并行收集直接用 <see cref="Add{T}"/> 与 <see cref="TryTake{T}"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <param name="item">先加入的元素。</param>
        /// <param name="taken">取出的元素；极端并发被他线程抢先时为 default(T)。</param>
        /// <returns>成功取出返回 true；袋为空返回 false。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static bool AddAndTryTake<T>(ConcurrentBag<T> bag, T item, out T taken)
        {
            // 同线程即存即取走本地队列，几乎零竞争
            bag.Add(item);
            return bag.TryTake(out taken);
        }

        /// <summary>
        /// 尝试查看一个元素但不移除（原子操作），优先看本地队列。
        /// 对应 <see cref="ConcurrentBag{T}.TryPeek(out T)"/>；看到哪个元素无序，
        /// 且 Peek 与后续 Take 是两次调用，中间可能被其他线程窃取。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <param name="item">成功时带出看到的元素；袋为空时为 default(T)。</param>
        /// <returns>袋非空返回 true；袋为空返回 false。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static bool TryPeek<T>(ConcurrentBag<T> bag, out T item)
        {
            // 只窥探不移除；不保证窥探到的就是随后能 Take 到的
            return bag.TryPeek(out item);
        }

        /// <summary>
        /// 当前元素数量。
        /// 对应 <see cref="ConcurrentBag{T}.Count"/>：<b>该实现需要锁遍所有线程本地分区，开销明显</b>，
        /// 并发下是瞬时值，仅用于诊断，不要放在热循环里。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <returns>调用瞬间的元素总数（近似值）。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static int Count<T>(ConcurrentBag<T> bag)
        {
            // 需要汇总全部线程本地分区，代价大：禁止高频调用
            return bag.Count;
        }

        /// <summary>
        /// 袋子是否为空（瞬时值）。
        /// 对应 <see cref="ConcurrentBag{T}.IsEmpty"/>；并发下仅表达探测那一刻的状态。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <returns>探测瞬间无元素返回 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static bool IsEmpty<T>(ConcurrentBag<T> bag)
        {
            // 瞬时状态；排空处理仍应以 TryTake 返回 false 为准
            return bag.IsEmpty;
        }

        /// <summary>
        /// 把袋中当前全部元素复制到新数组（快照）。
        /// 对应 <see cref="ConcurrentBag{T}.ToArray"/>：遍历所有线程本地分区复制，不取出元素，
        /// 顺序无定义；复制期间其他线程增删不影响已返回的数组。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <returns>调用瞬间袋中元素的数组副本（无序），归调用方所有。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static T[] ToArraySnapshot<T>(ConcurrentBag<T> bag)
        {
            // 快照导出，顺序不保证；适合做无序的整体统计
            return bag.ToArray();
        }

        /// <summary>
        /// 非阻塞地排空袋子：反复 TryTake 直到取不到，返回本次取出的全部元素（无序）。
        /// 调用线程会优先取自己的本地产物，不足时跨线程窃取。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="bag">并发袋实例，不能为 null。</param>
        /// <returns>本次取出的元素列表；原本为空时返回空列表。</returns>
        /// <exception cref="ArgumentNullException">bag 为 null 时抛出。</exception>
        public static List<T> Drain<T>(ConcurrentBag<T> bag)
        {
            List<T> list = new List<T>();
            T item;
            // 本地取空后自动工作窃取，直到所有分区都没有元素
            while (bag.TryTake(out item))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 典型场景：<see cref="Parallel.For(int,int,Action{int})"/> 并行循环里收集平方结果。
        /// 各工作线程写入各自本地分区，几乎没有竞争；返回顺序不保证，只保证元素齐全。
        /// 数据就地构造、就地释放，不依赖文件/网络/UI。
        /// </summary>
        /// <param name="fromInclusive">起始整数（含）。</param>
        /// <param name="toExclusive">结束整数（不含）；小于等于起始值时返回空列表。</param>
        /// <returns>区间内每个整数的平方组成的列表，顺序无序，元素个数等于区间长度。</returns>
        public static List<int> ParallelCollect(int fromInclusive, int toExclusive)
        {
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            // 并行计算：bag 的线程本地设计让“多线程各自 Add”几乎无锁
            Parallel.For(fromInclusive, toExclusive,
                delegate (int i) { bag.Add(i * i); });
            return bag.ToList();                            // 顺序不保证，需要排序由调用方处理
        }

        /// <summary>
        /// 工作窃取演示：生产者线程先放入 itemCount 个元素，然后由<b>另一个线程全部取走</b>。
        /// 消费者线程本地队列为空，所有元素都通过跨线程“窃取”获得，用于直观验证窃取路径可用、可取全。
        /// 数据全部就地构造，两个线程在方法内 Join 结束后释放。
        /// </summary>
        /// <param name="itemCount">要放入并窃取的元素个数（建议小的非负整数）。</param>
        /// <returns>消费者线程实际窃取到的元素个数；正常应等于 itemCount。</returns>
        public static int WorkStealingDemo(int itemCount)
        {
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            for (int i = 0; i < itemCount; i++)
            {
                bag.Add(i);                                 // 全部由“生产者侧”（当前线程）写入
            }

            int stolen = 0;
            Thread thief = new Thread(delegate ()
            {
                int item;
                // 该线程从没 Add 过，本地为空，全部走工作窃取
                while (bag.TryTake(out item))
                {
                    stolen++;
                }
            }) { IsBackground = true };
            thief.Start();
            thief.Join();                                   // 等窃取线程排空后再返回，结果确定
            return stolen;
        }

        /// <summary>
        /// 可自测示例：就地构造袋子，验证增/取/窥探/计数/快照/排空与并行收集、工作窃取，
        /// 不依赖文件/网络/UI。除并行收集外均为单线程确定性断言。
        /// </summary>
        /// <returns>通过的断言条数；全部符合预期时为 10。</returns>
        public static int RunSelfTest()
        {
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            int passed = 0;

            // 1) 空袋行为
            if (IsEmpty(bag)) passed++;
            if (!TryPeek(bag, out int peekEmpty) && peekEmpty == 0) passed++;
            if (!TryTake(bag, out int takeEmpty) && takeEmpty == 0) passed++;

            // 2) 加入 3 个元素：只校验数量（无序，不断言取到谁）
            Add(bag, 1);
            Add(bag, 2);
            Add(bag, 3);
            if (Count(bag) == 3 && !IsEmpty(bag)) passed++;

            // 3) 取走一个后剩 2 个
            int one;
            if (TryTake(bag, out one) && Count(bag) == 2) passed++;

            // 4) 快照长度与“不消费”语义
            int[] snapshot = ToArraySnapshot(bag);
            if (snapshot.Length == 2 && Count(bag) == 2) passed++;

            // 5) 排空：取完剩余 2 个且袋为空
            List<int> drained = Drain(bag);
            if (drained.Count == 2 && IsEmpty(bag) && Count(bag) == 0) passed++;

            // 6) Add+Take 组合：同线程即存即取能取回所存对象
            ConcurrentBag<object> bag2 = new ConcurrentBag<object>();
            object sentinel = new object();
            object got;
            if (AddAndTryTake(bag2, sentinel, out got) && object.ReferenceEquals(got, sentinel)) passed++;

            // 7) 并行收集：100 个平方，元素齐全（顺序不断言），求和确定
            List<int> squares = ParallelCollect(0, 100);
            int expectedSum = 0;
            for (int i = 0; i < 100; i++) expectedSum += i * i;
            int actualSum = 0;
            foreach (int x in squares) actualSum += x;
            if (squares.Count == 100 && actualSum == expectedSum) passed++;

            // 8) 工作窃取：另一线程应把 500 个元素全部偷走
            if (WorkStealingDemo(500) == 500) passed++;

            return passed;                               // 期望 10
        }
    }
}
