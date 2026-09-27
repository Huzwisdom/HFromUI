using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HHalcon 系列封装类的公共基类：
    /// 提供线程互斥锁、最近错误信息、HALCON 算子安全执行包装、HObject 释放与 HTuple 数组转换等通用能力。
    /// 所有具体封装（采集、图像处理、Blob、测量、匹配、识别、3D 等）均继承本类，
    /// 统一采用“算子失败不抛异常、返回 false/null 并写入 <see cref="Error"/>”的风格，
    /// 与 HCamera 目录下海康/大华相机封装类的写法保持一致。
    /// </summary>
    public abstract class HHalconBase : IDisposable
    {
        #region ==================== 字段与属性 ====================

        /// <summary>实例级互斥锁：同一封装对象上的算子调用串行执行，避免多线程抢用同一 HALCON 句柄。</summary>
        protected readonly object SyncRoot = new object();

        /// <summary>最近一次错误信息（成功执行后置空；包含 HALCON 错误号描述与 .NET 异常消息）。</summary>
        public string Error { get; protected set; } = string.Empty;

        /// <summary>最近一次操作是否成功（以 <see cref="Error"/> 是否为空判断）。</summary>
        public bool Success => string.IsNullOrEmpty(Error);

        private bool _disposed;

        #endregion

        #region ==================== 安全执行包装 ====================

        /// <summary>
        /// 执行一个 HALCON 算子动作：成功清空 <see cref="Error"/> 并返回 true；
        /// 捕获 <see cref="HalconException"/> 与普通异常后写入 <see cref="Error"/> 返回 false，不向上抛出。
        /// </summary>
        /// <param name="action">实际的算子调用块。</param>
        /// <returns>true=执行成功；false=算子或参数异常。</returns>
        protected bool SafeRun(Action action)
        {
            try
            {
                action();
                Error = string.Empty;
                return true;
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return false;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 执行带返回值的 HALCON 算子动作：成功返回计算结果；异常时返回 <paramref name="failValue"/> 并记录 <see cref="Error"/>。
        /// </summary>
        /// <typeparam name="T">返回值类型（HObject、HTuple、布尔等）。</typeparam>
        /// <param name="func">实际的算子调用块。</param>
        /// <param name="failValue">失败时回退返回的值（引用类型通常为 null）。</param>
        /// <returns>算子结果或失败回退值。</returns>
        protected T SafeRun<T>(Func<T> func, T failValue = default(T))
        {
            try
            {
                T r = func();
                Error = string.Empty;
                return r;
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return failValue;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return failValue;
            }
        }

        #endregion

        #region ==================== HObject / HTuple 通用辅助 ====================

        /// <summary>释放 HObject（HImage/HRegion/HXLD 等）并置 null；null 或已释放时安全跳过。</summary>
        /// <param name="obj">待释放对象引用。</param>
        protected static void DisposeObj(ref HObject obj)
        {
            if (obj != null)
            {
                try { obj.Dispose(); } catch { }
                obj = null;
            }
        }

        /// <summary>把 HTuple 转换为 double 数组；null 或空元组返回空数组。</summary>
        protected static double[] ToDArray(HTuple t)
        {
            if (t == null || t.Length <= 0) return new double[0];
            try { return t.DArr; } catch { return new double[0]; }
        }

        /// <summary>把 HTuple 转换为 int 数组；null 或空元组返回空数组。</summary>
        protected static int[] ToIArray(HTuple t)
        {
            if (t == null || t.Length <= 0) return new int[0];
            try { return t.IArr; } catch { return new int[0]; }
        }

        /// <summary>把 HTuple 转换为字符串数组；null 或空元组返回空数组。</summary>
        protected static string[] ToSArray(HTuple t)
        {
            if (t == null || t.Length <= 0) return new string[0];
            try { return t.SArr; } catch { return new string[0]; }
        }

        /// <summary>取 HTuple 首个 double 值；空元组返回 0。</summary>
        protected static double ToD(HTuple t)
        {
            if (t == null || t.Length <= 0) return 0.0;
            try { return t.D; } catch { return 0.0; }
        }

        /// <summary>取 HTuple 首个字符串值；空元组返回空串。</summary>
        protected static string ToS(HTuple t)
        {
            if (t == null || t.Length <= 0) return string.Empty;
            try { return t.S ?? string.Empty; } catch { return string.Empty; }
        }

        /// <summary>double 数组转 HTuple；null 按空元组处理。</summary>
        protected static HTuple FromDArray(double[] a)
        {
            return a == null ? new HTuple() : new HTuple(a);
        }

        /// <summary>字符串数组转 HTuple；null 按空元组处理。</summary>
        protected static HTuple FromSArray(string[] a)
        {
            return a == null ? new HTuple() : new HTuple(a);
        }

        #endregion

        #region ==================== IDisposable ====================

        /// <summary>是否已调用过 Dispose。</summary>
        protected bool IsDisposed => _disposed;

        /// <summary>释放封装对象占用的 HALCON 资源（子类重写 <see cref="DisposeUnmanaged"/> 释放句柄）。</summary>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>子类重写以释放 HALCON 句柄/窗口等资源。</summary>
        /// <param name="disposing">true=显式 Dispose；false=终结器调用。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) DisposeUnmanaged();
        }

        /// <summary>实际释放 HALCON 非托管资源的钩子（由子类重写）。</summary>
        protected virtual void DisposeUnmanaged()
        {
        }

        /// <summary>析构时兜底释放，防止 HALCON 句柄泄漏。</summary>
        ~HHalconBase()
        {
            try { Dispose(false); } catch { }
        }

        #endregion
    }
}
