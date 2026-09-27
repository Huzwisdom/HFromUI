using System;

namespace HFromUI.HData
{
    /// <summary>
    /// G 代码文本的引用类型包装。多个持有者共享同一实例，修改会立即同步。
    /// </summary>
    public class GCodeText
    {
        private string _text;
        /// <summary>全局唯一标识符。</summary>
        public string GUID { set; get; }
        public string Text
        {
            get => _text ?? string.Empty;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 当 Text 属性发生改变时触发。
        /// </summary>
        public event EventHandler TextChanged;

        public GCodeText(string text = "")
        {
            _text = text ?? string.Empty;
        }

        public override string ToString() => Text;

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将 GCodeText 转为其承载的 <see cref="string"/> 文本。
        /// 反向构造请直接使用构造函数 new GCodeText(text)。
        /// </summary>
        /// <param name="h">要取文本的 GCodeText 实例。</param>
        public static implicit operator string(GCodeText h) => h.Text;

        // 原隐式转换 GCodeText(string) 已删除：请直接 new GCodeText(text)。

        #endregion
    }
}
