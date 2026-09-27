using System;
using System.Collections.Generic;

namespace HFromUI.HFrom.Tables
{
    /// <summary>
    /// 树懒加载事件参数
    /// </summary>
    public class HTreeLazyLoadEventArgs : EventArgs
    {
        /// <summary>需要加载子节点的父节点</summary>
        public object ParentNode { get; }

        /// <summary>加载完成的子节点集合（在回调中填充）</summary>
        public IList<object> Children { get; } = new List<object>();

        /// <summary>是否加载完成</summary>
        public bool IsLoaded { get; set; }

        public HTreeLazyLoadEventArgs(object parentNode)
        {
            ParentNode = parentNode;
        }
    }

    /// <summary>
    /// 树懒加载事件处理器
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">懒加载事件参数</param>
    public delegate void HTreeLazyLoadEventHandler(object sender, HTreeLazyLoadEventArgs e);
}
