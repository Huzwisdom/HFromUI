using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HData
{

    public class OK<T> : IDisposable
    {
        /// <summary>是否成功 / Whether the operation succeeded</summary>
        public bool IsSuccess { get; set; }
        /// <summary>错误码，>=0 成功，负数错误 / Error code, >=0 success, negative error</summary>
        public int Error { get; set; }
        /// <summary>错误消息（中英双语）/ Error message (bilingual)</summary>
        public string Message { get; set; }
        /// <summary>返回的实际值 / The actual returned value</summary>
        public T Value { get; set; }

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：允许直接写 return true/false 返回成功/失败结果。
        /// true  → IsSuccess=true，Error=0；
        /// false → IsSuccess=false，Error=-1，Message="NG"。
        /// 判断成功与否请直接写 if(ok)（由 true/false 运算符支持）或读取 <see cref="IsSuccess"/>；
        /// 取消息用 <see cref="Message"/>，取错误码用 <see cref="Error"/>。
        /// </summary>
        public static implicit operator OK<T>(bool reply)
        {
            int errorInt = 0;
            string error = null;
            if (!reply)
            {
                error = "NG"; errorInt = -1;
            }
            return new OK<T>() { IsSuccess = reply, Error = errorInt, Message = error, };
        }

        // 以下原隐式转换已删除，改用对应成员/方法：
        //   bool(OK<T>)    → if(ok) 条件判断，或读取 IsSuccess 属性；
        //   string(OK<T>)  → Message 属性；
        //   int(OK<T>)     → Error 属性；
        //   OK<T>(string)  → FromMessage() 静态工厂；
        //   OK<T>(int)     → FromError() 静态工厂。

        /// <summary>if(ok) 条件判断支持：成功时为真。</summary>
        public static bool operator true(OK<T> a) => a?.IsSuccess ?? false;

        /// <summary>条件判断支持：失败时为真（用于 &amp;&amp; 短路求值）。</summary>
        public static bool operator false(OK<T> a) => !(a?.IsSuccess ?? false);

        /// <summary>逻辑非：支持 if(!ok) 写法，返回取反后的结果。</summary>
        public static OK<T> operator !(OK<T> a)
            => new OK<T>()
            {
                IsSuccess = !(a?.IsSuccess ?? false),
                Error = (a?.IsSuccess ?? false) ? -1 : 0,
                Message = (a?.IsSuccess ?? false) ? "NG" : null
            };

        /// <summary>逻辑与：两个结果都成功才成功（用于 ok1 &amp;&amp; ok2 条件写法）。</summary>
        public static OK<T> operator &(OK<T> a, OK<T> b)
            => new OK<T>() { IsSuccess = (a?.IsSuccess ?? false) && (b?.IsSuccess ?? false) };

        /// <summary>逻辑或：任一结果成功即成功（用于 ok1 || ok2 条件写法）。</summary>
        public static OK<T> operator |(OK<T> a, OK<T> b)
            => new OK<T>() { IsSuccess = (a?.IsSuccess ?? false) || (b?.IsSuccess ?? false) };

        /// <summary>从布尔值创建结果（与隐式转换逻辑相同，提供显式写法）。</summary>
        public static OK<T> FromBool(bool reply)
        {
            int errorInt = 0;
            string error = null;
            if (!reply)
            {
                error = "NG"; errorInt = -1;
            }
            return new OK<T>() { IsSuccess = reply, Error = errorInt, Message = error, };
        }

        /// <summary>
        /// 从消息字符串创建结果：null 或空白字符串视为成功（错误码 0），
        /// 非空字符串视为失败（错误码 -1，消息为该字符串）。
        /// </summary>
        public static OK<T> FromMessage(string reply)
        {
            bool isSuccess = false;
            int error = -1;
            if (string.IsNullOrWhiteSpace(reply))
            {
                isSuccess = true; error = 0;
            }
            return new OK<T>() { IsSuccess = isSuccess, Message = reply, Error = error, };
        }

        /// <summary>
        /// 从错误码创建结果：0 视为成功，非 0 视为失败（消息为 "NG"）。
        /// </summary>
        public static OK<T> FromError(int reply)
        {
            bool isSuccess = false;
            string error = null;
            if (reply == 0)
            {
                isSuccess = true;
            }
            else
            {
                error = "NG";
            }
            return new OK<T>() { IsSuccess = isSuccess, Error = reply, Message = error, };
        }

        #endregion
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(OK<T> a, OK<T> b) => !(a == b);
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(OK<T> a, OK<T> b)
        {
            // 必须处理 null：原来 a.IsSuccess 对 null 操作数直接抛 NullReferenceException
            // （例如 if (ok == null) 这种最常见写法都会崩）
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.IsSuccess == b.IsSuccess;
        }
        public override bool Equals(object obj)
        {
            if (obj is bool boolean)
            {
                return boolean == this.IsSuccess;
            }
            if (obj is int Int)
            {
                return Int == this.Error;
            }
            if (obj is string STRING)
            {
                return STRING == this.Message;
            }
            if (obj is OK<T> oK)
            {
                return oK.IsSuccess == this.IsSuccess;
            }
            return false;
        }
        public override int GetHashCode()
        {
            // 必须与 Equals 的判定依据一致：原来返回 base.GetHashCode()（引用身份），
            // 导致两个 IsSuccess 相同的 Equals 相等对象哈希码不同，违反 Equals/GetHashCode 契约
            // （放入 Dictionary/HashSet/ LINQ 分组时会出现"相等却找不到"的 bug）
            return IsSuccess.GetHashCode();
        }
        public override string ToString()
        {
            return $@"IsSuccess:{this.IsSuccess} Message:{this.Message} ErrorCode:{this.Error}";
        }
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
        }
    }


    public class OK : IDisposable
    {
        /// <summary>是否成功 / Whether the operation succeeded</summary>
        public bool IsSuccess { get; set; }
        /// <summary>错误码，>=0 成功，负数错误 / Error code, >=0 success, negative error</summary>
        public int Error { get; set; }
        /// <summary>错误消息（中英双语）/ Error message (bilingual)</summary>
        public string Message { get; set; }

        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(OK a, OK b) => !(a == b);
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(OK a, OK b)
        {
            // 处理 null 操作数，避免 if (ok == null) 等写法抛 NullReferenceException
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.IsSuccess == b.IsSuccess;
        }
        public override bool Equals(object obj)
        {
            if (obj is bool boolean)
            {
                return boolean == this.IsSuccess;
            }
            if (obj is int Int)
            {
                return Int == this.Error;
            }
            if (obj is string STRING)
            {
                return STRING == this.Message;
            }
            if (obj is OK oK)
            {
                return oK.IsSuccess == this.IsSuccess;
            }
            return false;
        }
        public override int GetHashCode()
        {
            // 与 Equals 判定依据（IsSuccess）保持一致，遵守哈希契约
            return IsSuccess.GetHashCode();
        }
        public override string ToString()
        {
            return $@"IsSuccess:{this.IsSuccess} Message:{this.Message} ErrorCode:{this.Error}";
        }
        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：允许直接写 return true/false 返回成功/失败结果。
        /// true  → IsSuccess=true，Error=0；
        /// false → IsSuccess=false，Error=-1，Message="NG"。
        /// 判断成功与否请直接写 if(ok)（由 true/false 运算符支持）或读取 <see cref="IsSuccess"/>；
        /// 取消息用 <see cref="Message"/>，取错误码用 <see cref="Error"/>。
        /// </summary>
        public static implicit operator OK(bool reply)
        {
            int errorInt = 0;
            string error = null;
            if (!reply)
            {
                error = "NG"; errorInt = -1;
            }
            return new OK() { IsSuccess = reply, Error = errorInt, Message = error, };
        }

        // 以下原隐式转换已删除，改用对应成员/方法：
        //   bool(OK)    → if(ok) 条件判断，或读取 IsSuccess 属性；
        //   string(OK)  → Message 属性；
        //   int(OK)     → Error 属性；
        //   OK(string)  → FromMessage() 静态工厂；
        //   OK(int)     → FromError() 静态工厂。

        /// <summary>if(ok) 条件判断支持：成功时为真。</summary>
        public static bool operator true(OK a) => a?.IsSuccess ?? false;

        /// <summary>条件判断支持：失败时为真（用于 &amp;&amp; 短路求值）。</summary>
        public static bool operator false(OK a) => !(a?.IsSuccess ?? false);

        /// <summary>逻辑非：支持 if(!ok) 写法，返回取反后的结果。</summary>
        public static OK operator !(OK a)
            => new OK()
            {
                IsSuccess = !(a?.IsSuccess ?? false),
                Error = (a?.IsSuccess ?? false) ? -1 : 0,
                Message = (a?.IsSuccess ?? false) ? "NG" : null
            };

        /// <summary>逻辑与：两个结果都成功才成功（用于 ok1 &amp;&amp; ok2 条件写法）。</summary>
        public static OK operator &(OK a, OK b)
            => new OK() { IsSuccess = (a?.IsSuccess ?? false) && (b?.IsSuccess ?? false) };

        /// <summary>逻辑或：任一结果成功即成功（用于 ok1 || ok2 条件写法）。</summary>
        public static OK operator |(OK a, OK b)
            => new OK() { IsSuccess = (a?.IsSuccess ?? false) || (b?.IsSuccess ?? false) };

        /// <summary>从布尔值创建结果（与隐式转换逻辑相同，提供显式写法）。</summary>
        public static OK FromBool(bool reply)
        {
            int errorInt = 0;
            string error = null;
            if (!reply)
            {
                error = "NG"; errorInt = -1;
            }
            return new OK() { IsSuccess = reply, Error = errorInt, Message = error, };
        }

        /// <summary>
        /// 从消息字符串创建结果：null 或空白字符串视为成功（错误码 0），
        /// 非空字符串视为失败（错误码 -1，消息为该字符串）。
        /// </summary>
        public static OK FromMessage(string reply)
        {
            bool isSuccess = false;
            int error = -1;
            if (string.IsNullOrWhiteSpace(reply))
            {
                isSuccess = true; error = 0;
            }
            return new OK() { IsSuccess = isSuccess, Message = reply, Error = error, };
        }

        /// <summary>
        /// 从错误码创建结果：0 视为成功，非 0 视为失败（消息为 "NG"）。
        /// </summary>
        public static OK FromError(int reply)
        {
            bool isSuccess = false;
            string error = null;
            if (reply == 0)
            {
                isSuccess = true;
            }
            else
            {
                error = "NG";
            }
            return new OK() { IsSuccess = isSuccess, Error = reply, Message = error, };
        }

        #endregion
        /// <summary>
        /// 释放资源，将内部引用置空以帮助GC
        /// Release resources, set internal references to null to help GC
        /// </summary>
        public void Dispose()
        {
            Message = null;
        }
    }
}
