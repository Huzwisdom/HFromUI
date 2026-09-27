using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HData
{
    public class ReplyData<T> : IDisposable
    {
        /// <summary>是否成功 / Whether the operation succeeded</summary>
        public bool IsSuccess { get; set; }
        /// <summary>错误码，>=0 成功，负数错误 / Error code, >=0 success, negative error</summary>
        public int Error { get; set; }
        /// <summary>错误消息（中英双语）/ Error message (bilingual)</summary>
        public string Message { get; set; }
        /// <summary>返回的实际值 / The actual returned value</summary>
        public T Value { get; set; }

        /// <summary>地址名称（用于调试或追踪）/ Address name (for debugging or tracing)</summary>
        public string AddressName { get; set; }
        /// <summary>读取的寄存器/线圈数量 / Number of registers/coils read</summary>
        public int Count { get; set; }
        /// <summary>起始地址 / Start address</summary>
        public int Address { get; set; }

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：允许直接当 bool 使用，返回 <see cref="IsSuccess"/>。
        /// 取错误消息请使用 <see cref="Message"/> 属性；取错误码请使用 <see cref="Error"/> 属性。
        /// </summary>
        public static implicit operator bool(ReplyData<T> reply) => reply?.IsSuccess ?? false;

        // 以下原隐式转换已删除，改用对应属性：
        //   string(ReplyData<T>) → Message 属性；
        //   int(ReplyData<T>)    → Error 属性。

        #endregion

        /// <summary>
        /// 释放资源，将内部引用置空以帮助GC
        /// Release resources, set internal references to null to help GC
        /// </summary>
        public void Dispose()
        {
            // 如果 T 是引用类型且实现了 IDisposable，可进一步释放
            if (Value is IDisposable disposableValue)
            {
                disposableValue.Dispose();
            }
            Value = default(T);
            Message = null;
            AddressName = null;
        }
    }

}
