using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace HFromUI.HData
{
    /// <summary>
    /// 线程安全的泛型列表包装器，内部使用 <see cref="List{T}"/> 存储。
    /// 所有公共成员均通过锁保证原子操作，适合多线程环境。
    /// 提供丰富的元素增删、移动、查找、排序、随机化、深拷贝等功能。
    /// </summary>
    /// <typeparam name="T">列表中元素的类型。</typeparam>
    public class HList<T> : IEnumerable<T>
    {
        /// <summary>hlock 字段。</summary>
        private readonly object hlock = new object();
        /// <summary>List 字段。</summary>
        private List<T> List { get; set; } = new List<T>();
        /// <summary>ThreadRandom 字段。</summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        #region 属性与索引器

        /// <summary>
        /// 获取列表中包含的元素总数。
        /// </summary>
        public int Count
        {
            get
            {
                lock (hlock)
                {
                    return List.Count;
                }
            }
        }

        /// <summary>
        /// 获取或设置内部列表的容量。容量是列表在无需调整大小的情况下可存储的元素数。
        /// </summary>
        public int Capacity
        {
            get
            {
                lock (hlock)
                {
                    return List.Capacity;
                }
            }
            set
            {
                lock (hlock)
                {
                    List.Capacity = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index">从零开始的元素索引。</param>
        /// <returns>位于指定索引处的元素。</returns>
        /// <exception cref="ArgumentOutOfRangeException">索引超出有效范围。</exception>
        public T this[int index]
        {
            get
            {
                lock (hlock)
                {
                    return List[index];
                }
            }
            set
            {
                lock (hlock)
                {
                    List[index] = value;
                }
            }
        }

        #endregion

        #region 构造与静态方法

        /// <summary>
        /// 初始化 <see cref="HList{T}"/> 类的新实例。
        /// </summary>
        public HList()
        {
        }

        /// <summary>
        /// 使用指定集合的元素初始化 <see cref="HList{T}"/> 的新实例。
        /// </summary>
        /// <param name="collection">其元素被复制到新列表的集合。</param>
        public HList(IEnumerable<T> collection)
        {
            lock (hlock)
            {
                List.AddRange(collection);
            }
        }

        /// <summary>
        /// 创建一个与当前列表相同的新 <see cref="HList{T}"/>（浅拷贝）。
        /// </summary>
        /// <returns>新 <see cref="HList{T}"/> 实例，包含所有元素的浅拷贝。</returns>
        public HList<T> Clone()
        {
            lock (hlock)
            {
                return new HList<T>(List);
            }
        }

        #endregion

        /// <summary>
        /// 通过对象获取其在列表中的序列号（从零开始的索引）。线程安全。
        /// </summary>
        /// <param name="item">要查找的对象。</param>
        /// <returns>对象的索引；若未找到则返回 -1。</returns>
        public int GetSerialNumber(T item)
        {
            lock (hlock)
            {
                return List.IndexOf(item);
            }
        }
        // 用于多线程间轻量级的同步标志或计数器，读写均为原子操作。
        private int syncToken;

        /// <summary>
        /// 获取或设置原子同步标志值（线程安全，基于 <see cref="Interlocked"/>）。
        /// </summary>
        public int SyncToken
        {
            get { return Interlocked.CompareExchange(ref syncToken, 0, 0); }
            set { Interlocked.Exchange(ref syncToken, value); }
        }

        /// <summary>
        /// 原子递增同步标志并返回新值。
        /// </summary>
        public int IncrementSyncToken()
        {
            return Interlocked.Increment(ref syncToken);
        }

        /// <summary>
        /// 原子递减同步标志并返回新值。
        /// </summary>
        public int DecrementSyncToken()
        {
            return Interlocked.Decrement(ref syncToken);
        }

        #region 获取原始列表或快照
        /// <summary>
        /// 获取内部列表的引用或副本。
        /// </summary>
        /// <param name="isLock">是否加安全锁</param>
        /// <returns>请求的列表实例。</returns>
        public List<T> GetList(bool isLock=false)
        {
            if (isLock)
            {
                lock (hlock)
                {
                    return List;
                }
            }
            else
            {
                return List;
            }
        }

        /// <summary>
        /// 返回一个只读 <see cref="ReadOnlyCollection{T}"/> 包装器，提供列表的线程安全只读视图。
        /// 注意：返回的集合是快照还是实时包装取决于实现，此处返回实时包装，但应在锁内使用。
        /// </summary>
        /// <returns>一个只读集合。</returns>
        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (hlock)
            {
                return List.AsReadOnly();
            }
        }

        #endregion

        #region 添加与插入

        /// <summary>
        /// 将元素插入到列表的开头（索引 0）。
        /// </summary>
        /// <param name="t">要插入的元素。</param>
        public void FirstAdd(T t)
        {
            lock (hlock)
            {
                List.Insert(0, t);
            }
        }

        /// <summary>
        /// 将元素插入到列表的指定索引处。
        /// 若索引超出有效范围，则不执行任何操作。
        /// </summary>
        /// <param name="index">要插入的位置的从零开始的索引。</param>
        /// <param name="t">要插入的元素。</param>
        public void Insert(int index, T t)
        {
            lock (hlock)
            {
                if (index >= 0 && index <= List.Count)
                {
                    List.Insert(index, t);
                }
            }
        }

        /// <summary>
        /// 将数组中的元素批量插入到列表的指定索引处。
        /// 若索引超出有效范围，则不执行任何操作。
        /// </summary>
        /// <param name="index">要插入的位置的从零开始的索引。</param>
        /// <param name="t">要插入的元素数组。</param>
        public void Insert(int index, T[] t)
        {
            lock (hlock)
            {
                if (index >= 0 && index <= List.Count)
                {
                    List.InsertRange(index, t);
                }
            }
        }

        /// <summary>
        /// 将指定集合中的元素批量插入到列表的指定索引处。
        /// 若索引超出有效范围，则不执行任何操作。
        /// </summary>
        /// <param name="index">要插入的位置的从零开始的索引。</param>
        /// <param name="t">要插入的元素集合。</param>
        public void Insert(int index, List<T> t)
        {
            lock (hlock)
            {
                if (index >= 0 && index <= List.Count)
                {
                    List.InsertRange(index, t);
                }
            }
        }

        /// <summary>
        /// 将元素添加到列表的末尾。
        /// </summary>
        /// <param name="t">要添加的元素。</param>
        public void Add(T t)
        {
            lock (hlock)
            {
                List.Add(t);
            }
        }

        /// <summary>
        /// 将数组中的元素批量添加到列表的末尾。
        /// </summary>
        /// <param name="t">要添加的元素数组。</param>
        public void Add(T[] t)
        {
            lock (hlock)
            {
                if (t != null)
                {
                    List.AddRange(t);
                }
            }
        }

        /// <summary>
        /// 将指定集合中的元素批量添加到列表的末尾。
        /// </summary>
        /// <param name="t">要添加的元素集合。</param>
        public void Add(List<T> t)
        {
            lock (hlock)
            {
                if (t != null)
                {
                    List.AddRange(t);
                }
            }
        }

        #endregion

        #region 移除

        /// <summary>
        /// 移除列表指定索引处的元素。
        /// 若索引无效则不执行任何操作。
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引。</param>
        public void RemoveAt(int index)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count)
                {
                    List.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 从列表中移除第一个匹配的特定对象。
        /// </summary>
        /// <param name="item">要从列表中移除的对象。</param>
        /// <returns>如果成功移除了 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Remove(T item)
        {
            lock (hlock)
            {
                return List.Remove(item);
            }
        }

        /// <summary>
        /// 移除与指定谓词匹配的所有元素。
        /// </summary>
        /// <param name="match">定义要移除元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>从列表中移除的元素数量。如果 <paramref name="match"/> 为 <c>null</c> 则返回 0。</returns>
        public int RemoveAll(Predicate<T> match)
        {
            lock (hlock)
            {
                if (match == null)
                {
                    return 0;
                }
                return List.RemoveAll(match);
            }
        }

        /// <summary>
        /// 从列表中移除指定索引开始的指定数量的元素。
        /// 若索引无效或数量非正，则不执行任何操作；实际移除数量会自动调整以防止越界。
        /// </summary>
        /// <param name="index">要移除的元素的起始从零开始的索引。</param>
        /// <param name="count">要移除的元素数量。</param>
        public void RemoveRange(int index, int count)
        {
            lock (hlock)
            {
                if (index < 0 || index >= List.Count || count <= 0)
                {
                    return;
                }
                int actualCount = Math.Min(count, List.Count - index);
                List.RemoveRange(index, actualCount);
            }
        }
        /// <summary>获取 list。</summary>
        public HList<T> GetList(int[] indexs, bool isLock = false)
        {
            if (indexs == null)
            {
                return null;
            }
            if (isLock)
            {
                lock (hlock)
                {
                    return CollectByIndexes(List, indexs);
                }
            }
            else
            {
                return CollectByIndexes(List, indexs);
            }
        }

        /// <summary>
        /// 按索引数组收集元素。索引越界或为负数时安全跳过（不抛异常），不影响其余索引。
        /// </summary>
        private static HList<T> CollectByIndexes(List<T> source, int[] indexs)
        {
            HList<T> lists = new HList<T>();
            foreach (int item in indexs)
            {
                if (item >= 0 && item < source.Count)
                {
                    lists.Add(source[item]);
                }
                // 越界索引跳过，避免 ArgumentOutOfRangeException 导致整批失败
            }
            return lists;
        }

        /// <summary>获取 listAndRemove。</summary>
        public HList<T> GetListAndRemove(int[] indexs, bool isLock = true)
        {
            if (indexs == null)
            {
                return null;
            }
            if (isLock)
            {
                lock (hlock)
                {
                    HList<T> lists = new HList<T>();
                    lists.Clear();
                    // 按传入索引顺序收集，保证返回顺序与索引数组一一对应
                    foreach (var item in indexs)
                    {
                        lists.Add(List[item]);
                    }
                    // 降序移除，避免删除后索引移位影响后续删除
                    List<int> numbers = indexs.ToList();
                    numbers.Sort();
                    numbers.Reverse();
                    foreach (var item in numbers)
                    {
                        List.RemoveAt(item);
                    }
                    return lists;
                }
            }
            else
            {
                HList<T> lists = new HList<T>();
                lists.Clear();
                foreach (var item in indexs)
                {
                    lists.Add(List[item]);
                }
                List<int> numbers = indexs.ToList();
                numbers.Sort();
                numbers.Reverse();
                foreach (var item in numbers)
                {
                    List.RemoveAt(item);
                }
                return lists;
            }
        }
        /// <summary>
        /// 清空列表中的所有元素。
        /// </summary>
        public void Clear()
        {
            lock (hlock)
            {
                List.Clear();
            }
        }

        /// <summary>
        /// 设置容量为列表中的实际元素数（如果该数目小于某个阈值）。
        /// </summary>
        public void TrimExcess()
        {
            lock (hlock)
            {
                List.TrimExcess();
            }
        }

        #endregion

        #region 安全取值/赋值

        /// <summary>
        /// 尝试获取指定索引处的元素。
        /// </summary>
        /// <param name="index">从零开始的索引。</param>
        /// <param name="value">当此方法返回时，如果索引有效，则包含该索引处的值；否则为 <c>default(T)</c>。</param>
        /// <returns>如果索引有效则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool TryGetValue(int index, out T value)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count)
                {
                    value = List[index];
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// 尝试设置指定索引处的元素值。
        /// </summary>
        /// <param name="index">从零开始的索引。</param>
        /// <param name="value">要设置的值。</param>
        /// <returns>如果索引有效并成功设置则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool TrySetValue(int index, T value)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count)
                {
                    List[index] = value;
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region 首尾元素快捷访问

        /// <summary>
        /// 返回列表的第一个元素，如果列表为空则返回 <c>default(T)</c>。
        /// </summary>
        public T FirstOrDefault()
        {
            lock (hlock)
            {
                return List.FirstOrDefault();
            }
        }

        /// <summary>
        /// 返回列表的最后一个元素，如果列表为空则返回 <c>default(T)</c>。
        /// </summary>
        public T LastOrDefault()
        {
            lock (hlock)
            {
                return List.LastOrDefault();
            }
        }

        #endregion

        #region 导航（Next / Last）

        /// <summary>
        /// 获取指定索引的下一个元素。若索引无效或已到列表末尾，则返回 <c>default(T)</c>。
        /// </summary>
        /// <param name="index">当前元素的从零开始的索引。</param>
        /// <returns>下一个元素，如果不存在则返回 <c>default(T)</c>。</returns>
        public T Next(int index)
        {
            lock (hlock)
            {
                if (index < 0 || index >= List.Count - 1)
                {
                    return default(T);
                }
                return List[index + 1];
            }
        }

        /// <summary>
        /// 获取指定索引的上一个元素。若索引无效或已到列表开头，则返回 <c>default(T)</c>。
        /// </summary>
        /// <param name="index">当前元素的从零开始的索引。</param>
        /// <returns>上一个元素，如果不存在则返回 <c>default(T)</c>。</returns>
        public T Last(int index)
        {
            lock (hlock)
            {
                if (index <= 0 || index >= List.Count)
                {
                    return default(T);
                }
                return List[index - 1];
            }
        }

        #endregion

        #region 移动元素

        /// <summary>
        /// 将指定索引处的元素移动到另一个索引处。
        /// 若任一索引无效或源索引等于目标索引，则不执行任何操作。
        /// </summary>
        /// <param name="index">要移动的元素的当前索引。</param>
        /// <param name="toIndex">目标索引。</param>
        public void Move(int index, int toIndex)
        {
            lock (hlock)
            {
                // toIndex 允许等于 List.Count，表示移动到列表末尾
                if (index < 0 || index >= List.Count ||
                    toIndex < 0 || toIndex > List.Count)
                {
                    return;
                }
                if (index == toIndex)
                {
                    return;
                }

                T item = List[index];
                List.RemoveAt(index);
                // 删除源元素后，若源在目标之前，目标位置需要前移一位
                int target = index < toIndex ? toIndex - 1 : toIndex;
                if (target >= List.Count)
                {
                    List.Add(item);
                }
                else
                {
                    List.Insert(target, item);
                }
            }
        }

        /// <summary>
        /// 将指定索引的元素与前一个元素交换位置（索引减 1）。
        /// 若索引无效或已为列表开头，则不执行任何操作。
        /// </summary>
        /// <param name="index">要交换的元素的从零开始的索引。</param>
        public void MoveForward(int index)
        {
            lock (hlock)
            {
                if (index > 0 && index < List.Count)
                {
                    Swap(index, index - 1);
                }
            }
        }

        /// <summary>
        /// 将指定索引的元素与后一个元素交换位置（索引加 1）。
        /// 若索引无效或已为列表末尾，则不执行任何操作。
        /// </summary>
        /// <param name="index">要交换的元素的从零开始的索引。</param>
        public void MoveBackward(int index)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count - 1)
                {
                    Swap(index, index + 1);
                }
            }
        }

        /// <summary>
        /// 将指定索引的元素移动到列表头部（索引 0）。
        /// 若索引无效则不执行任何操作。
        /// </summary>
        /// <param name="index">要移动的元素的从零开始的索引。</param>
        public void MoveToFirst(int index)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count)
                {
                    Move(index, 0);
                }
            }
        }

        /// <summary>
        /// 将指定索引的元素移动到列表尾部（索引 Count-1）。
        /// 若索引无效则不执行任何操作。
        /// </summary>
        /// <param name="index">要移动的元素的从零开始的索引。</param>
        public void MoveToLast(int index)
        {
            lock (hlock)
            {
                if (index >= 0 && index < List.Count)
                {
                    Move(index, List.Count - 1);
                }
            }
        }

        /// <summary>
        /// 将所有与指定谓词匹配的元素移动到指定的目标索引处。
        /// </summary>
        /// <param name="match">定义要移动元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <param name="toIndex">目标索引（移动后的插入位置）。</param>
        /// <returns>移动的元素数量。如果 <paramref name="match"/> 为 <c>null</c> 则返回 0。</returns>
        public int MoveAll(Predicate<T> match, int toIndex)
        {
            lock (hlock)
            {
                if (match == null || toIndex < 0)
                {
                    return 0;
                }

                // 收集要移动的元素
                List<T> itemsToMove = List.FindAll(match);
                if (itemsToMove.Count == 0)
                {
                    return 0;
                }

                // 从原列表中移除
                List.RemoveAll(match);

                // 调整目标索引：由于已经移除了元素，可能需要修正
                int adjustedIndex = Math.Min(toIndex, List.Count);

                // 插入到目标位置
                List.InsertRange(adjustedIndex, itemsToMove);
                return itemsToMove.Count;
            }
        }

        #endregion

        #region 交换元素

        /// <summary>
        /// 交换列表中两个指定索引处的元素。
        /// 若任一索引无效，则不执行任何操作。
        /// </summary>
        /// <param name="indexA">第一个索引。</param>
        /// <param name="indexB">第二个索引。</param>
        public void Swap(int indexA, int indexB)
        {
            lock (hlock)
            {
                if (indexA < 0 || indexA >= List.Count ||
                    indexB < 0 || indexB >= List.Count)
                {
                    return;
                }
                if (indexA == indexB)
                {
                    return;
                }

                T temp = List[indexA];
                List[indexA] = List[indexB];
                List[indexB] = temp;
            }
        }

        #endregion

        #region 替换

        /// <summary>
        /// 从指定索引开始，用新元素集合替换现有元素。
        /// 若索引无效则不执行任何操作。
        /// </summary>
        /// <param name="index">替换的起始索引。</param>
        /// <param name="newItems">新的元素集合。</param>
        public void ReplaceRange(int index, IEnumerable<T> newItems)
        {
            lock (hlock)
            {
                if (index < 0 || index > List.Count)
                {
                    return;
                }
                if (newItems == null)
                {
                    return;
                }

                List<T> newItemsList = newItems.ToList();
                // 从 index 起，用新元素替换原有元素：
                // 可移除的旧元素数量不能超过 index 之后剩余的元素数
                // （原实现在此分支内 overflow 恒为正数，else 是不可达死代码）
                int removable = Math.Min(newItemsList.Count, List.Count - index);
                if (removable > 0)
                {
                    List.RemoveRange(index, removable);
                }
                List.InsertRange(index, newItemsList);
            }
        }

        #endregion

        #region 随机元素与打乱

        /// <summary>
        /// 返回列表中的一个随机元素。如果列表为空，则返回 <c>default(T)</c>。
        /// </summary>
        public T RandomElement()
        {
            lock (hlock)
            {
                if (List.Count == 0)
                {
                    return default(T);
                }
                int randomIndex = ThreadRandom.Value.Next(0, List.Count);
                return List[randomIndex];
            }
        }

        /// <summary>
        /// 随机打乱列表中所有元素的顺序。
        /// </summary>
        public void Shuffle()
        {
            lock (hlock)
            {
                int n = List.Count;
                while (n > 1)
                {
                    n--;
                    int k = ThreadRandom.Value.Next(n + 1);
                    T value = List[k];
                    List[k] = List[n];
                    List[n] = value;
                }
            }
        }

        #endregion

        #region 查找与判断

        /// <summary>
        /// 确定某元素是否在列表中。
        /// </summary>
        /// <param name="item">要在列表中定位的对象。</param>
        /// <returns>如果在列表中找到 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Contains(T item)
        {
            lock (hlock)
            {
                return List.Contains(item);
            }
        }

        /// <summary>
        /// 搜索指定的对象，并返回列表中第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="item">要在列表中定位的对象。</param>
        /// <returns>第一个匹配项的从零开始的索引；若未找到则为 -1。</returns>
        public int IndexOf(T item)
        {
            lock (hlock)
            {
                return List.IndexOf(item);
            }
        }

        /// <summary>
        /// 搜索指定的对象，并返回列表中最后一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="item">要在列表中定位的对象。</param>
        /// <returns>最后一个匹配项的从零开始的索引；若未找到则为 -1。</returns>
        public int LastIndexOf(T item)
        {
            lock (hlock)
            {
                return List.LastIndexOf(item);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回整个列表中第一个匹配项。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>第一个匹配项；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 <c>default(T)</c>。</returns>
        public T Find(Predicate<T> match)
        {
            lock (hlock)
            {
                if (match == null)
                {
                    return default(T);
                }
                return List.Find(match);
            }
        }

        /// <summary>
        /// 检索与指定谓词匹配的所有元素，并返回包含这些元素的新 <see cref="HList{T}"/>。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>包含所有匹配元素的新 <see cref="HList{T}"/>；若 <paramref name="match"/> 为 <c>null</c> 则返回空列表。</returns>
        public HList<T> FindAll(Predicate<T> match)
        {
            lock (hlock)
            {
                if (match == null)
                {
                    return new HList<T>();
                }
                HList<T> result = new HList<T>();
                foreach (T item in List)
                {
                    if (match(item))
                    {
                        result.Add(item);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回整个列表中第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>第一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindIndex(Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindIndex(match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回从指定索引到列表末尾的范围内第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="startIndex">搜索的起始从零开始的索引。</param>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>第一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindIndex(startIndex, match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回从指定索引开始、包含指定元素数量的范围内第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="startIndex">搜索的起始从零开始的索引。</param>
        /// <param name="count">要搜索的元素数量。</param>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>第一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindIndex(startIndex, count, match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回整个列表中最后一个匹配项。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>最后一个匹配项；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 <c>default(T)</c>。</returns>
        public T FindLast(Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? default(T) : List.FindLast(match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回整个列表中最后一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>最后一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindLastIndex(Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindLastIndex(match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回从指定索引到列表开头的范围内最后一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="startIndex">向后搜索的起始从零开始的索引。</param>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>最后一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindLastIndex(startIndex, match);
            }
        }

        /// <summary>
        /// 搜索与指定谓词匹配的元素，并返回从指定索引开始、向前搜索指定元素数量的范围内最后一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="startIndex">向后搜索的起始从零开始的索引。</param>
        /// <param name="count">要搜索的元素数量。</param>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>最后一个匹配项的从零开始的索引；若未找到或 <paramref name="match"/> 为 <c>null</c> 则返回 -1。</returns>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (hlock)
            {
                return match == null ? -1 : List.FindLastIndex(startIndex, count, match);
            }
        }

        /// <summary>
        /// 确定列表中是否存在与指定谓词匹配的元素。
        /// </summary>
        /// <param name="match">定义要搜索元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>若存在匹配元素且 <paramref name="match"/> 不为 <c>null</c>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Exists(Predicate<T> match)
        {
            lock (hlock)
            {
                return match != null && List.Exists(match);
            }
        }

        /// <summary>
        /// 确定列表中的所有元素是否都与指定谓词匹配。
        /// </summary>
        /// <param name="match">定义要检查元素条件的 <see cref="Predicate{T}"/> 委托。</param>
        /// <returns>如果列表中的每个元素都与谓词匹配或列表为空且 <paramref name="match"/> 不为 <c>null</c>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool TrueForAll(Predicate<T> match)
        {
            lock (hlock)
            {
                return match != null && List.TrueForAll(match);
            }
        }

        #endregion

        #region 排序与反转

        /// <summary>
        /// 使用默认比较器对整个列表进行排序。
        /// </summary>
        public void Sort()
        {
            lock (hlock)
            {
                List.Sort();
            }
        }

        /// <summary>
        /// 使用指定的比较器对整个列表进行排序。
        /// </summary>
        /// <param name="comparer">比较元素时要使用的 <see cref="IComparer{T}"/> 实现，或为 <c>null</c> 则使用默认比较器。</param>
        public void Sort(IComparer<T> comparer)
        {
            lock (hlock)
            {
                List.Sort(comparer);
            }
        }

        /// <summary>
        /// 使用指定的 <see cref="Comparison{T}"/> 对整个列表进行排序。
        /// </summary>
        /// <param name="comparison">比较元素时要使用的 <see cref="Comparison{T}"/> 委托。若为 <c>null</c> 则不执行任何操作。</param>
        public void Sort(Comparison<T> comparison)
        {
            lock (hlock)
            {
                if (comparison != null)
                {
                    List.Sort(comparison);
                }
            }
        }

        /// <summary>
        /// 使用指定的比较器对列表中某范围内的元素进行排序。
        /// 若索引或计数无效，则不执行任何操作。
        /// </summary>
        /// <param name="index">要排序的范围的起始从零开始的索引。</param>
        /// <param name="count">要排序的元素数量。</param>
        /// <param name="comparer">比较元素时要使用的 <see cref="IComparer{T}"/> 实现，或为 <c>null</c> 则使用默认比较器。</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (hlock)
            {
                if (index >= 0 && count >= 0 && index + count <= List.Count)
                {
                    List.Sort(index, count, comparer);
                }
            }
        }

        /// <summary>
        /// 反转整个列表中元素的顺序。
        /// </summary>
        public void Reverse()
        {
            lock (hlock)
            {
                List.Reverse();
            }
        }

        /// <summary>
        /// 反转指定范围内元素的顺序。
        /// 若索引或计数无效，则不执行任何操作。
        /// </summary>
        /// <param name="index">要反转的范围的起始从零开始的索引。</param>
        /// <param name="count">要反转的元素数量。</param>
        public void Reverse(int index, int count)
        {
            lock (hlock)
            {
                if (index >= 0 && count >= 0 && index + count <= List.Count)
                {
                    List.Reverse(index, count);
                }
            }
        }

        #endregion

        #region 遍历

        /// <summary>
        /// 对列表中的每个元素执行指定操作。操作在锁内执行，请注意避免长时间阻塞或死锁。
        /// 若 <paramref name="action"/> 为 <c>null</c>，则不执行任何操作。
        /// </summary>
        /// <param name="action">要对每个元素执行的 <see cref="Action{T}"/> 委托。</param>
        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                return;
            }
            lock (hlock)
            {
                foreach (T item in List)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举器。为提高线程安全性，枚举器基于列表的快照（副本）创建。
        /// </summary>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/>。</returns>
        public IEnumerator<T> GetEnumerator()
        {
            T[] snapshot;
            lock (hlock)
            {
                snapshot = List.ToArray();
            }
            foreach (T item in snapshot)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 返回一个循环访问集合的非泛型枚举器。
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region 复制与转换

        /// <summary>
        /// 将列表中的元素复制到新数组中。
        /// </summary>
        /// <returns>包含列表元素副本的数组。</returns>
        public T[] ToArray()
        {
            lock (hlock)
            {
                return List.ToArray();
            }
        }

        /// <summary>
        /// 创建列表的浅拷贝副本并返回为 <see cref="List{T}"/>。
        /// </summary>
        /// <returns>包含列表元素副本的新 <see cref="List{T}"/>。</returns>
        public List<T> ToList()
        {
            lock (hlock)
            {
                return new List<T>(List);
            }
        }

        /// <summary>
        /// 将整个列表复制到兼容的一维数组，从目标数组的指定索引处开始。
        /// </summary>
        /// <param name="array">作为元素复制目标的一维数组。</param>
        /// <param name="arrayIndex"><paramref name="array"/> 中复制开始处的从零开始的索引。</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (hlock)
            {
                List.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// 将整个列表复制到兼容的一维数组，从目标数组的开头开始。
        /// </summary>
        /// <param name="array">作为元素复制目标的一维数组。</param>
        public void CopyTo(T[] array)
        {
            lock (hlock)
            {
                List.CopyTo(array);
            }
        }

        /// <summary>
        /// 将列表中一定范围的元素复制到兼容的一维数组，从目标数组的指定索引处开始。
        /// </summary>
        /// <param name="index">源列表中复制开始处的从零开始的索引。</param>
        /// <param name="array">作为元素复制目标的一维数组。</param>
        /// <param name="arrayIndex"><paramref name="array"/> 中复制开始处的从零开始的索引。</param>
        /// <param name="count">要复制的元素数量。</param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (hlock)
            {
                List.CopyTo(index, array, arrayIndex, count);
            }
        }

        /// <summary>
        /// 创建列表中指定范围元素的浅拷贝副本，并返回为新的 <see cref="HList{T}"/>。
        /// 若索引或计数无效，则返回空列表。
        /// </summary>
        /// <param name="index">范围开始的从零开始的索引。</param>
        /// <param name="count">要复制的元素数量。</param>
        /// <returns>包含指定范围元素的新 <see cref="HList{T}"/>。</returns>
        public HList<T> GetRange(int index, int count)
        {
            lock (hlock)
            {
                if (index < 0 || count < 0 || index + count > List.Count)
                {
                    return new HList<T>();
                }
                HList<T> result = new HList<T>();
                result.Add(List.GetRange(index, count));
                return result;
            }
        }

        /// <summary>
        /// 将列表中的每个元素转换为另一种类型，并返回包含转换后元素的新 <see cref="HList{TOutput}"/>。
        /// </summary>
        /// <typeparam name="TOutput">目标元素的类型。</typeparam>
        /// <param name="converter">将每个元素从一种类型转换为另一种类型的 <see cref="Converter{T, TOutput}"/> 委托。若为 <c>null</c> 则返回空列表。</param>
        /// <returns>包含转换后元素的新 <see cref="HList{TOutput}"/>。</returns>
        public HList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (hlock)
            {
                if (converter == null)
                {
                    return new HList<TOutput>();
                }
                HList<TOutput> result = new HList<TOutput>();
                foreach (T item in List)
                {
                    result.Add(converter(item));
                }
                return result;
            }
        }

        #endregion

        #region 二分查找

        /// <summary>
        /// 使用默认比较器在整个已排序列表中搜索指定元素，并返回该元素的从零开始的索引。
        /// </summary>
        /// <param name="item">要查找的对象。</param>
        /// <returns>找到的元素的从零开始的索引；若未找到则为负数（按位取反的插入点）。</returns>
        public int BinarySearch(T item)
        {
            lock (hlock)
            {
                return List.BinarySearch(item);
            }
        }

        /// <summary>
        /// 使用指定的比较器在整个已排序列表中搜索指定元素，并返回该元素的从零开始的索引。
        /// </summary>
        /// <param name="item">要查找的对象。</param>
        /// <param name="comparer">比较元素时要使用的 <see cref="IComparer{T}"/> 实现，或为 <c>null</c> 则使用默认比较器。</param>
        /// <returns>找到的元素的从零开始的索引；若未找到则为负数（按位取反的插入点）。</returns>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (hlock)
            {
                return List.BinarySearch(item, comparer);
            }
        }

        /// <summary>
        /// 使用指定的比较器在已排序列表的某个范围内搜索指定元素，并返回该元素的从零开始的索引。
        /// </summary>
        /// <param name="index">要搜索的范围的起始从零开始的索引。</param>
        /// <param name="count">要搜索的范围中的元素数量。</param>
        /// <param name="item">要查找的对象。</param>
        /// <param name="comparer">比较元素时要使用的 <see cref="IComparer{T}"/> 实现，或为 <c>null</c> 则使用默认比较器。</param>
        /// <returns>找到的元素的从零开始的索引；若未找到则为负数（按位取反的插入点）。</returns>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (hlock)
            {
                return List.BinarySearch(index, count, item, comparer);
            }
        }

        #endregion

        /// <summary>
        /// 清空列表，释放容量并强制进行垃圾回收，以尽快回收内存。
        /// </summary>
        public void ClearAndCollect()
        {
            lock (hlock)
            {
                List.Clear();
                List.TrimExcess();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 释放未使用的容量并强制进行垃圾回收。
        /// </summary>
        public void TrimAndCollect()
        {
            lock (hlock)
            {
                List.TrimExcess();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
