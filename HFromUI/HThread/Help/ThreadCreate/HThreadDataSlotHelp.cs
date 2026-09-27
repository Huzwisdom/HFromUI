using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// 【是什么】Thread 命名/匿名数据槽帮助类：封装 .NET 1.x 时代的“按托管线程隔离的弱类型数据槽” API——
    /// <see cref="Thread.GetNamedDataSlot(string)"/>、<see cref="Thread.AllocateNamedDataSlot(string)"/>、
    /// <see cref="Thread.AllocateDataSlot"/>、<see cref="Thread.SetData(LocalDataStoreSlot, object)"/>、
    /// <see cref="Thread.GetData"/>、<see cref="Thread.FreeNamedDataSlot(string)"/>。
    /// 槽（<see cref="LocalDataStoreSlot"/>）本身是进程级的（同名槽全进程唯一），但槽里的数据按“托管线程”
    /// 各自隔离：同一个槽，每个线程 GetData 看到的都是自己的那份 object，互不可见。
    ///
    /// 【是否跨进程】否。槽与数据都在当前进程内；且数据只在同一托管线程内可见，天然不跨线程、更不跨进程。
    ///
    /// 【典型适用场景】
    /// 1) 维护 .NET 1.x 遗留代码中基于命名数据槽的线程上下文（如老库隐式传递的调用方信息）；
    /// 2) 需要按线程隔离、又无法改写方法签名传参的极老框架扩展点；
    /// 3) 读懂 GetData/SetData 老代码后，将其迁移到强类型方案。
    /// 新代码应优先使用 [ThreadStatic] 静态字段（见 <see cref="HThreadStaticHelp"/>）或
    /// <see cref="ThreadLocal&lt;T&gt;"/>（见 <see cref="HThreadLocalHelp"/>）：二者强类型、无需装箱、
    /// 生命周期清晰；需要随 async/await 流转则用 AsyncLocal&lt;T&gt;。
    ///
    /// 【使用步骤】
    /// 1) 命名槽：首次用 <see cref="Thread.AllocateNamedDataSlot(string)"/> 分配（已存在同名槽会抛异常），
    ///    或直接用 <see cref="Thread.GetNamedDataSlot(string)"/>（不存在则自动分配）；
    /// 2) 任意线程内用 <see cref="Thread.SetData(LocalDataStoreSlot, object)"/> 写入自己那份值，
    ///    <see cref="Thread.GetData"/> 读出本线程那份值（未写过为 null）；
    /// 3) 匿名槽：<see cref="Thread.AllocateDataSlot"/> 返回槽实例，靠持有引用访问，无需名字；
    /// 4) 进程退出前不需要释放；确需释放命名槽才考虑 FreeNamedDataSlot（注意坑见下）。
    ///
    /// 【注意事项与坑】
    /// 1) 弱类型：值是 object，值类型会反复装箱拆箱，编译期无类型检查；
    /// 2) 按“托管线程”而非 OS 线程隔离，也不会随 async/await 跨线程流转（这正是推荐 AsyncLocal 的原因）；
    /// 3) GetNamedDataSlot 不存在时会“自动创建”，因此多处用同一名字拿到的是同一个进程级槽；
    ///    AllocateNamedDataSlot 在同名槽已存在时抛 <see cref="ArgumentException"/>；
    /// 4) FreeNamedDataSlot 的坑：释放后 CLR 可能复用该名字再分配一个新槽——如果老槽引用仍被其他线程
    ///    持有并继续 SetData/GetData，数据会写到“新旧两个不同槽”上产生难以排查的错乱。
    ///    只有在确定进程卸载前再没有任何代码会用到这个名字时才调用，一般干脆不要调用；
    /// 5) 线程池线程复用时，上一个任务遗留在槽中的数据会被下一个任务看到，任务边界需自行清空。
    ///
    /// 【版本可用性】net48 全部位于 mscorlib，可正常使用；.NET Core/.NET 5+ 已整体移除
    /// LocalDataStoreSlot 及 GetData/SetData/GetNamedDataSlot/AllocateDataSlot/
    /// AllocateNamedDataSlot/FreeNamedDataSlot，迁移目标是 ThreadLocal&lt;T&gt; 或 AsyncLocal&lt;T&gt;。
    /// </summary>
    /// <example>
    /// 典型遗留写法与迁移方向：
    /// <code>
    /// // 老 API：弱类型、进程级槽 + 线程级数据
    /// LocalDataStoreSlot slot = Thread.GetNamedDataSlot("request-context");
    /// Thread.SetData(slot, context);          // 装箱，仅当前托管线程可见
    /// object ctx = Thread.GetData(slot);      // 拆箱，其他线程读到的是自己的份（可能为 null）
    ///
    /// // 推荐迁移：强类型
    /// ThreadLocal&lt;MyContext&gt; current = new ThreadLocal&lt;MyContext&gt;();
    /// current.Value = context;                // 无装箱
    /// </code>
    /// </example>
    public static class HThreadDataSlotHelp
    {
        /// <summary>
        /// 跨线程携带读取结果的私有载体：MainSaw=主线程最终读到的值，
        /// WorkerSaw=子线程启动后首次读到的值（应为 null），WorkerOwnValue=子线程写入后回读的值。
        /// </summary>
        private sealed class SlotResult
        {
            /// <summary>主线程在子线程结束后从同名槽读到的值（应仍为 "main"）。</summary>
            public object MainSaw { get; set; }

            /// <summary>子线程首次从同名槽读到的值（数据按线程隔离，应为 null）。</summary>
            public object WorkerSaw { get; set; }

            /// <summary>子线程写入 "worker" 后自己回读到的值。</summary>
            public object WorkerOwnValue { get; set; }
        }

        /// <summary>
        /// 真实演示命名数据槽“槽是进程级、数据按托管线程隔离”：主线程取得名为 demo-slot-1 的槽并写入
        /// "main"；子线程用同一名字取得同一槽，首次 GetData 必须为 null，再写入 "worker" 并回读；
        /// 子线程结束后主线程再读，值必须仍是 "main"——两边互不可见、互不影响。
        /// 全程使用 <see cref="ManualResetEventSlim"/> 与带超时的 Join，2 秒内必定结束。
        /// </summary>
        /// <returns>
        /// 同时满足以下条件返回 true：子线程在 2000ms 内完成并被 Join 回收；主线程值非空且为 "main"；
        /// 子线程初始读到 null；子线程自己写入并回读到 "worker"。
        /// </returns>
        public static bool SlotsAreIsolatedAcrossThreads()
        {
            using (ManualResetEventSlim done = new ManualResetEventSlim(false))
            {
                SlotResult result = new SlotResult();
                const string slotName = "demo-slot-1";

                // 主线程：取得（不存在则自动分配）进程级命名槽，写入主线程自己那份值
                LocalDataStoreSlot mainSlot = Thread.GetNamedDataSlot(slotName);
                Thread.SetData(mainSlot, "main");

                Thread worker = new Thread((ThreadStart)delegate
                {
                    // 同一名字拿到的是同一个进程级槽对象
                    LocalDataStoreSlot workerSlot = Thread.GetNamedDataSlot(slotName);

                    // 数据按托管线程隔离：子线程从未写过，首读必须为 null
                    result.WorkerSaw = Thread.GetData(workerSlot);

                    // 子线程写入自己那份值，不影响主线程
                    Thread.SetData(workerSlot, "worker");
                    result.WorkerOwnValue = Thread.GetData(workerSlot);

                    done.Set();
                }) { IsBackground = true, Name = "HThreadDataSlotHelp.IsolationDemo" };

                worker.Start();
                bool signaled = done.Wait(2000);   // 等待子线程完成读取/写入
                bool joined = worker.Join(2000);   // 带超时回收
                result.MainSaw = Thread.GetData(mainSlot);   // 主线程再读：不应受子线程影响

                return signaled && joined
                    && "main" == (string)result.MainSaw
                    && result.WorkerSaw == null
                    && "worker" == (string)result.WorkerOwnValue;
            }
        }

        /// <summary>
        /// 真实演示匿名数据槽：<see cref="Thread.AllocateDataSlot"/> 分配一个无需名字的槽，
        /// 在当前线程上 SetData 后立即 GetData 往返校验。匿名槽靠槽实例引用访问，
        /// 不存在重名冲突与 FreeNamedDataSlot 的复用问题。
        /// </summary>
        /// <returns>同一槽在当前线程上写入 "ping" 后能原样读回时返回 true；否则 false。</returns>
        public static bool UnnamedSlotDemo()
        {
            // 匿名槽：无需名字，谁持有引用谁访问；数据同样按托管线程隔离
            LocalDataStoreSlot slot = Thread.AllocateDataSlot();
            Thread.SetData(slot, "ping");

            object value = Thread.GetData(slot);
            return "ping" == (string)value;
        }
    }
}
