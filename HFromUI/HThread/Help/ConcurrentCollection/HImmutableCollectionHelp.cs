namespace HFromUI.HThread.Help.ConcurrentCollection
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// System.Collections.Immutable 下的不可变集合族（ImmutableArray / ImmutableList / ImmutableDictionary /
    /// ImmutableQueue / ImmutableStack / ImmutableHashSet，另有 ImmutableSortedDictionary / ImmutableSortedSet 等）。
    /// “不可变”指实例一旦创建内容永不变：Add/Remove/SetItem 等<b>修改 API 都返回一个新集合</b>，
    /// 旧引用原样保留。底层用结构共享（树节点复用、数组复制写）降低复制成本，但每次写仍有分配。
    /// 文档入口：https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.immutable?view=netframework-4.8</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 仍是进程内托管对象，不跨进程；价值在于“进程内多线程无锁安全发布 + 历史版本不被篡改”。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 配置表/路由表/规则集等“读极多、写偶发”的全局快照，发布后任何线程只读无需加锁；
    /// 2) 撤销/重做、事件溯源、快照回溯——每个版本都是独立引用，天然保留历史；
    /// 3) 并行/异步管线中跨线程安全传递集合，不担心接收方偷偷改坏；
    /// 4) 作为字典值被多个读者长期持有的“冻结”数据。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// 从每个类型的 Empty 静态属性起步（如 ImmutableArray&lt;int&gt;.Empty），
    /// 链式调用 Add/Remove/SetItem 得到新版本并自行保存引用；批量构造用 CreateBuilder() + AddRange + ToImmutable()，
    /// 避免 N 次 Add 产生 N 次复制；多线程共享“当前版本”用一个字段保存，更新时
    /// Interlocked.Exchange（或 Volatile.Write）整引用原子切换。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>修改 API 有返回值</b>：list.Add(x) 不改 list，新集合在返回值里，丢弃返回值等于白改。
    /// 2) <b>不要循环里逐个 Add 给同一引用</b>：每次都全量/树形复制，应改用 Builder 或 AddRange。
    /// 3) ImmutableArray&lt;T&gt; 是<b>结构（struct）</b>：default(ImmutableArray&lt;T&gt;) 未初始化，
    ///    访问其成员会抛 NullReferenceException；判断用 IsDefault / IsDefaultOrEmpty，判空集合用 IsEmpty。
    /// 4) ImmutableQueue / ImmutableStack 没有 Add：队列用 Enqueue/Dequeue（或 TryDequeue/TryPeek），
    ///    栈用 Push/Pop（或 TryPop/TryPeek）；Dequeue/Pop 也返回新实例。
    /// 5) ImmutableDictionary 无序；要排序键用 ImmutableSortedDictionary；
    ///    线程安全靠“不可变”而非加锁——发布之后不能再改，要改就造新版本再切换引用。
    /// 6) 高频写入（每毫秒大量增删）场景复制成本高，应选 Concurrent* 可变并发集合。
    /// 7) ToBuilder() 得到的 Builder <b>是可变的</b>，仅用于单线程批量构造期，构造完 ToImmutable() 再发布。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.8 <b>不内置</b>，需 NuGet 安装 System.Collections.Immutable（net48 受支持，装后 netstandard 表面可用）；
    /// .NET Core/.NET 5+ 随平台内置。本帮助类保持空静态类可编译，所有真实用法见下方 example，
    /// 装好包后其中代码可直接复制使用。</para>
    /// </summary>
    ///
    /// <example>
    /// 【安装 System.Collections.Immutable NuGet 包后，net48 亦可编译运行；示例数据就地构造、无外部依赖】
    /// <code>
    /// using System;
    /// using System.Collections.Immutable;
    /// using System.Threading;
    ///
    /// // ============ 1) ImmutableArray&lt;T&gt;：底层是数组的不可变列表，按下标遍历最快 ============
    /// ImmutableArray&lt;int&gt; empty = ImmutableArray&lt;int&gt;.Empty;     // 规范起点
    /// ImmutableArray&lt;int&gt; a = empty.Add(1).Add(2);             // 每次 Add 返回新实例，empty 仍是空
    /// ImmutableArray&lt;int&gt; b = a.Remove(1);                     // a 仍是 [1,2]，b 是 [2]
    /// int first = a[0];                                        // 下标只读访问，first == 1
    /// bool badDefault = default(ImmutableArray&lt;int&gt;).IsDefault;// true：未初始化的 struct 不能直接用
    /// bool reallyEmpty = ImmutableArray&lt;int&gt;.Empty.IsEmpty;   // true：已初始化但长度为 0
    ///
    /// // 批量构建：Builder 期间可变、只复制一次，避免逐个 Add 的 O(n^2) 复制
    /// ImmutableArray&lt;int&gt;.Builder ab = ImmutableArray.CreateBuilder&lt;int&gt;();
    /// ab.Add(1); ab.Add(2); ab.Add(3);
    /// ImmutableArray&lt;int&gt; built = ab.ToImmutable();            // 此刻起冻结发布
    ///
    /// // ============ 2) ImmutableList&lt;T&gt;：树形结构，两端/中间增删比 Array 版省，但下标访问 O(log n) ============
    /// ImmutableList&lt;string&gt; list = ImmutableList&lt;string&gt;.Empty
    ///     .Add("a").Add("b").Add("c");
    /// ImmutableList&lt;string&gt; list2 = list.Insert(1, "x");       // a,x,b,c
    /// ImmutableList&lt;string&gt; list3 = list2.Remove("x");         // a,b,c
    /// ImmutableList&lt;string&gt; list4 = list3.SetItem(0, "A");     // A,b,c：按下标替换，返回新实例
    /// ImmutableList&lt;string&gt; onlyB = list4.FindAll(s => s == "B"); // 不可变版本的过滤结果
    /// ImmutableList&lt;string&gt;.Builder lb = ImmutableList.CreateBuilder&lt;string&gt;();
    /// lb.AddRange(new[] { "x", "y", "z" });
    /// ImmutableList&lt;string&gt; bulk = lb.ToImmutable();           // 批量构造同样推荐
    ///
    /// // ============ 3) ImmutableDictionary&lt;TKey,TValue&gt;：发布后只读访问无需任何锁 ============
    /// ImmutableDictionary&lt;string, int&gt; d0 = ImmutableDictionary&lt;string, int&gt;.Empty;
    /// ImmutableDictionary&lt;string, int&gt; d1 = d0.Add("a", 1);    // Add 键已存在会抛异常
    /// ImmutableDictionary&lt;string, int&gt; d2 = d1.SetItem("a", 10)// SetItem：存在则覆盖，不存在则新增
    ///     .SetItem("b", 2);
    /// ImmutableDictionary&lt;string, int&gt; d3 = d2.Remove("b");    // 删除返回新版本
    /// int got;
    /// bool has = d2.TryGetValue("a", out got);                 // 只读查询，has==true、got==10
    /// bool contains = d2.ContainsKey("b");                     // true
    /// int count = d2.Count;                                    // 2
    ///
    /// // Builder 批量灌字典
    /// ImmutableDictionary&lt;string, int&gt;.Builder db =
    ///     ImmutableDictionary.CreateBuilder&lt;string, int&gt;();
    /// db["x"] = 1; db["y"] = 2;
    /// ImmutableDictionary&lt;string, int&gt; bulkDict = db.ToImmutable();
    ///
    /// // ============ 4) ImmutableQueue&lt;T&gt;：不可变 FIFO，没有 Add，用 Enqueue/Dequeue ============
    /// ImmutableQueue&lt;int&gt; q0 = ImmutableQueue&lt;int&gt;.Empty;
    /// ImmutableQueue&lt;int&gt; q1 = q0.Enqueue(1).Enqueue(2);       // 队尾追加，返回新队列
    /// int qHead;
    /// bool qOk = q1.TryPeek(out qHead);                        // qHead==1，队列不变
    /// int qDeq;
    /// ImmutableQueue&lt;int&gt; q2 = q1.Dequeue(out qDeq);           // qDeq==1，q2 只剩 2
    /// ImmutableQueue&lt;int&gt; q3;
    /// bool qAgain = q2.TryDequeue(out q3);                     // 不抛异常风格：q3 是弹出后的新队列
    ///
    /// // ============ 5) ImmutableStack&lt;T&gt;：不可变 LIFO，Push/Pop 都返回新栈 ============
    /// ImmutableStack&lt;int&gt; s0 = ImmutableStack&lt;int&gt;.Empty;
    /// ImmutableStack&lt;int&gt; s1 = s0.Push(1).Push(2);             // 栈顶 2
    /// int sTop;
    /// bool sOk = s1.TryPeek(out sTop);                         // sTop==2
    /// int sPop;
    /// ImmutableStack&lt;int&gt; s2 = s1.Pop(out sPop);               // sPop==2，s2 栈顶 1
    /// ImmutableStack&lt;int&gt; s3;
    /// bool sAgain = s2.TryPop(out s3);                         // TryPop：空栈返回 false 而非抛异常
    ///
    /// // ============ 6) ImmutableHashSet&lt;T&gt;：不可变去重集合与集合运算 ============
    /// ImmutableHashSet&lt;int&gt; h0 = ImmutableHashSet&lt;int&gt;.Empty;
    /// ImmutableHashSet&lt;int&gt; h1 = h0.Add(1).Add(2).Add(2);      // 重复 Add 被忽略，仍为 {1,2}
    /// ImmutableHashSet&lt;int&gt; h2 = h1.Remove(1);                 // {2}
    /// bool memberOf = h1.Contains(2);                          // true
    /// ImmutableHashSet&lt;int&gt; h3 = h1.Union(h2);                 // 并集 {1,2}
    /// ImmutableHashSet&lt;int&gt; h4 = h1.Intersect(h2);             // 交集 {2}
    /// ImmutableHashSet&lt;int&gt; h5 = h1.Except(h2);                // 差集 {1}
    ///
    /// // ============ 7) 多线程共享当前快照：Interlocked.Exchange 原子切换整引用 ============
    /// // 读者永远看到“整张完整表”，绝无半成品；旧读者继续用旧版本，互不影响
    /// sealed class ConfigStore
    /// {
    ///     private ImmutableDictionary&lt;string, string&gt; current =
    ///         ImmutableDictionary&lt;string, string&gt;.Empty;
    ///
    ///     public string Lookup(string key)                     // 读路径：无锁
    ///     {
    ///         string value;
    ///         return this.current.TryGetValue(key, out value) ? value : null;
    ///     }
    ///
    ///     public void Update(string key, string value)         // 写路径：造新版本再原子换引用
    ///     {
    ///     retry:
    ///         ImmutableDictionary&lt;string, string&gt; old = this.current;
    ///         ImmutableDictionary&lt;string, string&gt; next = old.SetItem(key, value);
    ///         if (Interlocked.CompareExchange(ref this.current, next, old) != old)
    ///             goto retry;                                  // 被别人先改则基于最新版重试（CAS 循环）
    ///     }
    ///
    ///     public void Publish(ImmutableDictionary&lt;string, string&gt; next)
    ///     {
    ///         Interlocked.Exchange(ref this.current, next);    // 简单场景：直接整表替换发布
    ///     }
    /// }
    ///
    /// // ============ 8) 与 Concurrent* 的选型对比 ============
    /// // Immutable*  ：不可变+复制写，无锁安全发布/快照/历史版本；读远多于写、写偶发批量
    /// // Concurrent* ：可变+内部锁/无锁算法，原地高频增删改；多线程持续并发读写
    /// // 普通 Dictionary+lock：写很少且全部访问点都受控、还需兼容老代码时
    /// </code>
    /// </example>
    public static class HImmutableCollectionHelp
    {
        // net48 使用前请先 NuGet 安装 System.Collections.Immutable；
        // 安装后上方 <example> 中覆盖 Empty / Add / Remove / Builder / SetItem /
        // Interlocked.Exchange 快照切换 / 与 Concurrent* 选型的代码均可直接复制使用。
        // 本类故意保持空实现，以免未安装该 NuGet 包的工程产生编译错误。
    }
}
