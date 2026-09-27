using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HFromUI.HThread.Help.ConcurrentCollection
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> 帮助类：多线程可并发读写的线程安全键值字典，
    /// 内部采用“细粒度分段锁 + 无锁 volatile 读 / CAS 写”的混合算法，所有读、写、枚举均无需外部 lock。
    /// 文档：https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=netframework-4.8</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 只在当前进程（托管堆）内有效，不跨进程、不跨 AppDomain 共享。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 全局/静态缓存表、计数器表、对象池索引；
    /// 2) 多线程任务结果按键归并、词频统计；
    /// 3) 需要原子“读-改-写”的复合操作（<see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey,TValue,Func{TKey,TValue,TValue})"/>），
    ///    不必自己 lock 包住“先取后算再存”。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// new 一个实例后，直接并发调用原子 API：
    /// 写入用 <see cref="ConcurrentDictionary{TKey,TValue}.TryAdd(TKey,TValue)"/> / GetOrAdd / AddOrUpdate；
    /// 读取用 TryGetValue / ContainsKey；
    /// 删除用 <see cref="ConcurrentDictionary{TKey,TValue}.TryRemove(TKey,out TValue)"/>；
    /// 需要快照时用 ToArray() / ToList()，或直接 foreach（枚举器并发安全、不抛异常）。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) 大坑：<b>GetOrAdd / AddOrUpdate 的工厂委托在高并发下可能被执行多次</b>，
    ///    只有其中一次返回值真正入表（文档原文如此）。工厂内禁止写“只允许发生一次”的副作用（下单、插库、发消息、new 昂贵资源）。
    ///    副作用请用 <see cref="Lazy{T}"/> 包裹（见 <see cref="GetOrAddOnce{TKey,TValue}"/>），或工厂外加锁/去重。
    /// 2) TryUpdate 是 CAS：仅当键当前值与 comparisonValue 相等（引用同一/Equals 为真）才更新成功。
    /// 3) 复合操作只保证“单次 API 调用”原子；先 TryGetValue 再 TryAdd 这种两步组合不原子。
    /// 4) Keys / Values / ToArray / Count 在大表上都要锁定全部桶并分配集合，高频调用有开销；
    ///    Count / IsEmpty 是瞬时值，不能作为业务判断依据。
    /// 5) 枚举器并发安全、不抛 InvalidOperationException，但不保证严格的某一时刻快照，可能包含枚举开始后别的线程的修改。
    /// 6) 值类型 TValue 用 TryUpdate/CompareExchange 类逻辑时注意默认值与装箱相等性。
    /// 7) net48 的 TryRemove 只有 out TValue 一个重载；返回 KeyValuePair 的重载是 .NET Core 之后才有，勿在 net48 使用。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.0+ 内置（System.Collections.Concurrent 命名空间，程序集 mscorlib/System.Collections.Concurrent），
    /// net48 直接可用，无需任何 NuGet 包。</para>
    /// </summary>
    ///
    /// <example>
    /// 下面示例数据全部就地构造、就地释放，不依赖文件/网络/UI：
    /// <code>
    /// var dict = new ConcurrentDictionary&lt;string, int&gt;();
    /// dict.TryAdd("a", 1);                 // 不存在才添加，键已存在返回 false
    /// int v = dict.GetOrAdd("b", 2);       // 值版本：直接给值，无“工厂多次执行”问题
    /// dict.AddOrUpdate("a", 1, (k, old) => old + 1); // 键存在则用工厂更新为 2
    /// int passed = HConcurrentDictionaryHelp.RunSelfTest(); // 跑一遍可自测示例
    /// </code>
    /// </example>
    public static class HConcurrentDictionaryHelp
    {
        /// <summary>
        /// 尝试添加键值对（原子操作）：仅当键不存在时才写入。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.TryAdd(TKey,TValue)"/>。
        /// </summary>
        /// <typeparam name="TKey">键类型，不能为 null（键不得为空引用）。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要添加的键，不能为 null。</param>
        /// <param name="value">键不存在时写入的值。</param>
        /// <returns>键原本不存在且成功写入返回 true；键已存在（不覆盖）返回 false。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出（由底层字典保证）。</exception>
        /// <exception cref="OverflowException">字典已达到容量上限时底层可能抛出（极端情况）。</exception>
        public static bool TryAdd<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            // 直接转发原子 API：键已存在时不会覆盖原值
            return dict.TryAdd(key, value);
        }

        /// <summary>
        /// 获取或添加——<b>值版本</b>：键存在返回现值；不存在则写入并返回给定的 fallback。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,TValue)"/>。
        /// 此版本直接传入一个现成值，<b>不存在“工厂被执行多次”的坑</b>，值是廉价数据时优先用它。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要查找或添加的键，不能为 null。</param>
        /// <param name="fallback">键不存在时写入并返回的默认值。</param>
        /// <returns>键已存在时返回其现值；不存在时返回 fallback（同时已写入表中）。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static TValue GetOrAdd<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue fallback)
        {
            // 值版本：不存在即写入 fallback，无委托、无副作用风险
            return dict.GetOrAdd(key, fallback);
        }

        /// <summary>
        /// 获取或添加——<b>工厂版本</b>：键存在直接返回现值；不存在时调用 valueFactory 即时算一个值写入并返回。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,Func{TKey,TValue})"/>。
        /// <b>坑：多线程同时为同一个不存在的键调用时，工厂可能被执行多次，只有一次结果入表</b>，
        /// 因此工厂必须保持纯净（只依赖 key 做计算）；一次性副作用请改用 <see cref="GetOrAddOnce{TKey,TValue}"/>。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要查找或添加的键，不能为 null。</param>
        /// <param name="valueFactory">键不存在时用于产生新值的工厂，入参为 key；要求无一次性副作用。</param>
        /// <returns>键已存在时返回现值；否则返回工厂产出且已写入表中的值。</returns>
        /// <exception cref="ArgumentNullException">dict、key 或 valueFactory 为 null 时抛出。</exception>
        /// <exception cref="Exception">工厂内部抛出的异常会原样传播给调用方；注意工厂可能被执行多次。</exception>
        public static TValue GetOrAddWithFactory<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict,
                                                               TKey key, Func<TKey, TValue> valueFactory)
        {
            // 工厂版本：竞争时 valueFactory 可能被多个线程各执行一次，仅一个结果入表
            return dict.GetOrAdd(key, valueFactory);
        }

        /// <summary>
        /// 添加或更新——<b>固定新值版本</b>：键不存在写入 addValue；键已存在则用 updateFactory 基于旧值算出新值。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey,TValue,Func{TKey,TValue,TValue})"/>。
        /// 更新工厂同样可能在竞争重试时被执行多次，工厂内不要放一次性副作用。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要添加或更新的键，不能为 null。</param>
        /// <param name="addValue">键不存在时写入的值。</param>
        /// <param name="updateFactory">键已存在时的更新工厂，入参为 (key, 旧值)，返回要写回的新值。</param>
        /// <returns>调用后该键上的最终值（新写入值或工厂算出的更新值）。</returns>
        /// <exception cref="ArgumentNullException">dict、key 或 updateFactory 为 null 时抛出。</exception>
        public static TValue AddOrUpdateFixedValue<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict,
                                                                 TKey key, TValue addValue,
                                                                 Func<TKey, TValue, TValue> updateFactory)
        {
            // 不存在 -> addValue；存在 -> 把旧值交给 updateFactory 算新值
            return dict.AddOrUpdate(key, addValue, updateFactory);
        }

        /// <summary>
        /// 添加或更新——<b>双工厂版本</b>：不存在时由 addFactory 造值，存在时由 updateFactory 基于旧值更新。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey,Func{TKey,TValue},Func{TKey,TValue,TValue})"/>。
        /// 两个工厂都可能在竞争中被执行多次，禁止在里面写一次性副作用。
        /// 另有带工厂参数 TArg 的重载（AddOrUpdate&lt;TArg&gt;），可把外部状态显式传入、避免闭包分配。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要添加或更新的键，不能为 null。</param>
        /// <param name="addFactory">键不存在时的造值工厂，入参为 key。</param>
        /// <param name="updateFactory">键已存在时的更新工厂，入参为 (key, 旧值)。</param>
        /// <returns>调用后该键上的最终值。</returns>
        /// <exception cref="ArgumentNullException">dict、key、addFactory、updateFactory 为 null 时抛出。</exception>
        public static TValue AddOrUpdateWithFactories<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict,
                                                                    TKey key,
                                                                    Func<TKey, TValue> addFactory,
                                                                    Func<TKey, TValue, TValue> updateFactory)
        {
            // 不存在走 addFactory；存在走 updateFactory；整次调用对字典而言是原子的
            return dict.AddOrUpdate(key, addFactory, updateFactory);
        }

        /// <summary>
        /// 安全计数器：对指定键原子 +1（键不存在时从 1 开始）。
        /// 内部使用 <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey,TValue,Func{TKey,TValue,TValue})"/>，
        /// 比“lock 包住取数+赋值”更轻量。
        /// </summary>
        /// <param name="dict">字符串键的整数字典，不能为 null。</param>
        /// <param name="key">计数项的键，不能为 null。</param>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static void Increment(ConcurrentDictionary<string, int> dict, string key)
        {
            // 不存在写入初始值 1；存在则旧值 +1，整个读改写由字典保证原子
            dict.AddOrUpdate(key, 1, delegate (string k, int oldValue) { return oldValue + 1; });
        }

        /// <summary>
        /// CAS（比较并交换）更新：仅当键当前值与 comparisonValue 相等时，才把值换成 newValue。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.TryUpdate(TKey,TValue,TValue)"/>。
        /// 典型用于乐观并发：先读出旧值、算出新值，再用旧值作比对提交，期间被别人改过则提交失败。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型；引用相等或 <see cref="object.Equals(object,object)"/> 为真即视为匹配。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要更新的键，不能为 null。</param>
        /// <param name="newValue">比对成功时写入的新值。</param>
        /// <param name="comparisonValue">期望中的旧值；与现值不相等则更新失败。</param>
        /// <returns>键存在且现值等于 comparisonValue、更新成功返回 true；键不存在或现值不符返回 false。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static bool CompareUpdate<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict,
                                                       TKey key, TValue newValue, TValue comparisonValue)
        {
            // 现值必须与 comparisonValue 匹配才允许写入，是无锁乐观更新的核心原语
            return dict.TryUpdate(key, newValue, comparisonValue);
        }

        /// <summary>
        /// 原子移除并取出被删的值。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.TryRemove(TKey,out TValue)"/>。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要移除的键，不能为 null。</param>
        /// <param name="value">移除成功时带出被删的值；键不存在时为 TValue 的默认值。</param>
        /// <returns>键存在且移除成功返回 true；键不存在返回 false。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static bool TryRemove<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            // 一步完成“判断存在 + 删除 + 取出”，避免先 ContainsKey 再删的竞态
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 尝试按键取值，键不存在时不抛异常。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/>，
        /// 是并发字典首选读取 API（比索引器安全，索引器键不存在会抛 <see cref="KeyNotFoundException"/>）。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要查找的键，不能为 null。</param>
        /// <param name="value">找到时带出对应值；未找到时为 TValue 默认值。</param>
        /// <returns>找到键返回 true；未找到返回 false。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static bool TryGetValue<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            // 无锁读路径：命中返回 true，未命中返回 false 而不是抛异常
            return dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// 判断是否包含指定键。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.ContainsKey(TKey)"/>。
        /// 注意“ContainsKey 后再 TryAdd/TryRemove”是两步，组合不原子；能直接用 TryAdd/TryGetValue 就不要先判存在。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="key">要探测的键，不能为 null。</param>
        /// <returns>调用瞬间键存在返回 true，否则 false（下一瞬可能已被其他线程改动）。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        public static bool ContainsKey<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            // 仅作存在性探测；结果是瞬时值，别拿它做“先查后写”的依据
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// 获取当前键值对数量。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.Count"/>。
        /// Count 需锁定所有分段统计，大表上有开销且是并发瞬时值，仅用于日志/诊断，不要用于业务分支。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间的键值对数量。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static int GetCount<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // 瞬时统计值：并发写入下只代表“某一瞬间”，不能当作精确依据
            return dict.Count;
        }

        /// <summary>
        /// 获取字典是否为空（瞬时值）。比 Count == 0 语义更直接，开销同样不低，仅用于诊断。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间没有任何键值对返回 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static bool IsEmpty<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // 等价于瞬时 Count == 0，但语义更清晰
            return dict.IsEmpty;
        }

        /// <summary>
        /// 取出全部键的快照列表。
        /// Keys 属性会锁定所有分段并新分配一个集合，调用频繁或表很大时有明显开销，拿到后可脱离字典安全遍历。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间全部键的新列表（快照），遍历时其他线程改字典不影响该列表。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static List<TKey> GetKeysSnapshot<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // Keys 本身即快照集合，再 ToList 固化为 List，避免持有内部枚举对象
            return dict.Keys.ToList();
        }

        /// <summary>
        /// 取出全部值的快照列表。
        /// Values 属性同样会锁定全部分段并分配集合，仅在需要整体导出时使用。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间全部值的新列表（快照，含重复值也原样保留）。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static List<TValue> GetValuesSnapshot<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // Values 为快照集合，ToList 固化后与后续并发改动完全隔离
            return dict.Values.ToList();
        }

        /// <summary>
        /// 以数组形式导出整张字典的快照。
        /// 对应 <see cref="ConcurrentDictionary{TKey,TValue}.ToArray"/>，内部加锁后一次性复制，
        /// 返回数组归调用方所有，其他线程之后的增删改与该数组无关。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间字典内容的键值对数组副本。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static KeyValuePair<TKey, TValue>[] ToArraySnapshot<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // 官方 ToArray：加全锁复制，适合需要确定性快照的场合
            return dict.ToArray();
        }

        /// <summary>
        /// 以 List 形式导出整张字典的快照（LINQ ToList 内部走 GetEnumerator）。
        /// 与 <see cref="ToArraySnapshot{TKey,TValue}"/> 用途相同，按下游需要的容器类型选择。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <returns>调用瞬间字典内容的键值对列表副本。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static List<KeyValuePair<TKey, TValue>> Snapshot<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict)
        {
            // ToList 触发一次完整枚举，遍历期间别的线程增删不会抛异常
            return dict.ToList();
        }

        /// <summary>
        /// 演示枚举器并发安全：一边 foreach 字典，一边用后台线程疯狂增删，
        /// 整个枚举过程不会抛 <see cref="InvalidOperationException"/>（普通 Dictionary&lt;TKey,TValue&gt; 在同样场景会抛）。
        /// 注意：枚举内容不保证是严格某一时刻的快照，可能混入枚举期间新增的键。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dict">并发字典实例，不能为 null。</param>
        /// <param name="mutationCount">后台线程增删操作的总次数，传 0 则不启动后台线程。</param>
        /// <returns>本次 foreach 实际遍历到的键值对数量（并发下不固定，仅证明未抛异常）。</returns>
        /// <exception cref="ArgumentNullException">dict 为 null 时抛出。</exception>
        public static int EnumerateSafelyWhileChanging<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, int mutationCount)
        {
            bool stop = false;                                   // 通知后台写线程收尾
            Thread mutator = new Thread(delegate ()
            {
                int i = 0;
                // 简单的增/删交替，直到做完 mutationCount 次或前台枚举结束
                while (i < mutationCount && !Volatile.Read(ref stop))
                {
                    TKey key = (TKey)Convert.ChangeType(i, typeof(TKey));   // 仅对 int 键有意义，示例用
                    dict.TryAdd(key, default(TValue));
                    if ((i & 1) == 0)
                    {
                        TValue removed;
                        dict.TryRemove(key, out removed);       // 边枚举边删也不影响枚举安全性
                    }
                    i++;
                }
            }) { IsBackground = true };

            mutator.Start();
            int seen = 0;
            // 直接 foreach：拿到的是并发安全枚举器，绝不因集合被改而抛异常
            foreach (KeyValuePair<TKey, TValue> pair in dict)
            {
                seen++;                                        // 模拟对快照内容做只读处理
                if (seen == 1)
                {
                    Thread.SpinWait(100);                       // 故意拉长枚举窗口，制造并发碰撞
                }
            }
            Volatile.Write(ref stop, true);                     // 通知写线程退出
            mutator.Join();                                     // 等它干净结束，方法返回即全部释放
            return seen;
        }

        /// <summary>
        /// 复现“GetOrAdd 工厂可能执行多次”的坑：8 个线程用 <see cref="Barrier"/> 同时撞同一个键，
        /// 返回工厂被实际调用的次数（1~8 之间，竞争越强越可能大于 1）。
        /// 用 <see cref="Interlocked.Increment(ref int)"/> 计数，方法内数据全部就地构造释放。
        /// </summary>
        /// <returns>valueFactory 被实际执行的次数；可能为 1，也可能大于 1，恰好证明不能在工厂里放一次性副作用。</returns>
        public static int CountFactoryInvocationsUnderContention()
        {
            ConcurrentDictionary<int, string> dict = new ConcurrentDictionary<int, string>();
            int factoryCalls = 0;                               // 工厂真实执行次数
            const int threadCount = 8;
            Thread[] threads = new Thread[threadCount];

            using (Barrier barrier = new Barrier(threadCount))  // 让所有线程在同一刻发起调用
            {
                for (int t = 0; t < threadCount; t++)
                {
                    threads[t] = new Thread(delegate ()
                    {
                        barrier.SignalAndWait();                // 全员到齐后同时冲过起跑线
                        dict.GetOrAdd(1, delegate (int k)
                        {
                            Interlocked.Increment(ref factoryCalls); // 竞争时可能多次执行
                            return "value-" + k;
                        });
                    }) { IsBackground = true };
                }
                for (int t = 0; t < threadCount; t++)
                {
                    threads[t].Start();
                }
                for (int t = 0; t < threadCount; t++)
                {
                    threads[t].Join();
                }
            } // Barrier 在此 Dispose，字典随方法栈释放，无外部依赖
            return factoryCalls;
        }

        /// <summary>
        /// 保证“创建副作用只发生一次”的 GetOrAdd 范式：值用 <see cref="Lazy{T}"/> 包一层。
        /// 竞争时虽然可能 new 出多个 Lazy 对象，但只有赢得入表的那个 Lazy 会被访问 <see cref="Lazy{T}.Value"/>，
        /// 因而真正的创建逻辑（new TValue()）在全局只执行一次；落败的 Lazy 从未被求值、可被 GC 回收。
        /// 适用于“每个键对应一个昂贵且只能创建一次的资源”（连接、会话、串行化器实例等）。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型，要求具有公共无参构造函数。</typeparam>
        /// <param name="dict">值为 <see cref="Lazy{T}"/> 的并发字典，不能为 null。</param>
        /// <param name="key">要获取或创建资源的键，不能为 null。</param>
        /// <returns>该键唯一对应的、保证只构造一次的 TValue 实例。</returns>
        /// <exception cref="ArgumentNullException">dict 或 key 为 null 时抛出。</exception>
        /// <exception cref="MissingMethodException">TValue 无公共无参构造函数且首次访问 Value 时由 Lazy 工厂抛出。</exception>
        public static TValue GetOrAddOnce<TKey, TValue>(ConcurrentDictionary<TKey, Lazy<TValue>> dict, TKey key)
            where TValue : new()
        {
            // 竞争时可能 new 多个 Lazy，但只有入表的那个会被 .Value 求值
            Lazy<TValue> lazy = dict.GetOrAdd(key,
                delegate (TKey k)
                {
                    // 默认 LazyThreadSafetyMode.ExecutionAndPublication：工厂内部也只执行一次
                    return new Lazy<TValue>(delegate { return new TValue(); });
                });
            return lazy.Value;                                  // 真正创建发生在这里，且全局仅一次
        }

        /// <summary>
        /// 可自测示例：就地构造字典，顺序演练全部核心 API 并校验结果，不依赖文件/网络/UI。
        /// </summary>
        /// <returns>通过的断言条数；全部符合预期时为 24。</returns>
        /// <exception cref="Exception">工厂演示方法内部异常时原样抛出（正常路径不抛）。</exception>
        public static int RunSelfTest()
        {
            ConcurrentDictionary<string, int> dict = new ConcurrentDictionary<string, int>();
            int passed = 0;

            // 1) TryAdd：首次成功、重复键失败且不覆盖
            if (TryAdd(dict, "a", 1)) passed++;
            if (!TryAdd(dict, "a", 99)) passed++;
            if (TryGetValue(dict, "a", out int aValue) && aValue == 1) passed++;

            // 2) GetOrAdd 值版本：存在返回旧值，不存在写入 fallback
            if (GetOrAdd(dict, "a", 10) == 1) passed++;
            if (GetOrAdd(dict, "b", 2) == 2) passed++;

            // 3) GetOrAdd 工厂版本：不存在时工厂造值
            if (GetOrAddWithFactory(dict, "c", delegate (string k) { return 3; }) == 3) passed++;

            // 4) AddOrUpdate 固定值版本：存在 -> 旧值 1+10=11
            if (AddOrUpdateFixedValue(dict, "a", 9, delegate (string k, int old) { return old + 10; }) == 11) passed++;

            // 5) AddOrUpdate 双工厂版本：首次造 4，再次更新为 5
            if (AddOrUpdateWithFactories(dict, "d", delegate (string k) { return 4; },
                                         delegate (string k, int old) { return old + 1; }) == 4) passed++;
            if (AddOrUpdateWithFactories(dict, "d", delegate (string k) { return 4; },
                                         delegate (string k, int old) { return old + 1; }) == 5) passed++;

            // 6) 计数器：同一键 +1 两次，值为 2
            Increment(dict, "cnt");
            Increment(dict, "cnt");
            if (TryGetValue(dict, "cnt", out int cnt) && cnt == 2) passed++;

            // 7) CAS：旧值匹配才更新；故意用错误比对值制造一次失败
            if (CompareUpdate(dict, "a", 100, 11)) passed++;
            if (!CompareUpdate(dict, "a", 200, 11)) passed++;    // 现值已是 100，比对 11 失败
            if (TryGetValue(dict, "a", out int aFinal) && aFinal == 100) passed++;

            // 8) 探测与快照
            if (ContainsKey(dict, "b") && !ContainsKey(dict, "zzz")) passed++;
            if (GetCount(dict) == 5) passed++;                   // a,b,c,d,cnt
            if (!IsEmpty(dict)) passed++;
            if (GetKeysSnapshot(dict).Count == 5) passed++;
            if (GetValuesSnapshot(dict).Count == 5) passed++;
            if (ToArraySnapshot(dict).Length == 5) passed++;
            if (Snapshot(dict).Count == 5) passed++;

            // 9) 原子移除并取出旧值
            if (TryRemove(dict, "b", out int removed) && removed == 2 && GetCount(dict) == 4) passed++;

            // 10) 枚举安全演示：int 键字典 + 后台增删，不抛异常即符合预期
            ConcurrentDictionary<int, int> intDict = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < 1000; i++) intDict.TryAdd(i, i);
            int seen = EnumerateSafelyWhileChanging(intDict, 2000);
            if (seen >= 0) passed++;                             // 返回值仅证明枚举完整跑完

            // 11) 工厂多次执行坑：次数必然 >=1（具体值随调度变化，不断言上限）
            int calls = CountFactoryInvocationsUnderContention();
            if (calls >= 1) passed++;

            // 12) Lazy 包裹：同一键两次获取必须是同一个实例，副作用只一次
            ConcurrentDictionary<string, Lazy<object>> lazyDict =
                new ConcurrentDictionary<string, Lazy<object>>();
            object o1 = GetOrAddOnce(lazyDict, "resource");
            object o2 = GetOrAddOnce(lazyDict, "resource");
            if (object.ReferenceEquals(o1, o2)) passed++;

            return passed;                                       // 期望 24
        }
    }
}
