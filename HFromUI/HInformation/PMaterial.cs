using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HInterface;

namespace HFromUI.HInformation
{
    public class PMaterial
    {
        /// <summary>Work 成员。</summary>
        public iHFrom Work { set; get; }
        /// <summary>名称。</summary>
        public string Name { set; get; }
        /// <summary>ID 成员。</summary>
        public int ID { set; get; }
        /// <summary>索引。</summary>
        public int Index { set; get; }
        /// <summary>UserControl 成员。</summary>
        public object UserControl { set; get; }
        /// <summary>IsScore 成员。</summary>
        public bool IsScore { set; get; }

        /// <summary>XScore 成员。</summary>
        public double XScore { set; get; }
        /// <summary>YScore 成员。</summary>
        public double YScore { set; get; }
        /// <summary>X 坐标。</summary>
        public double X { set; get; }
        /// <summary>Y 坐标。</summary>
        public double Y { set; get; }
        /// <summary>宽度。</summary>
        public double Width { set; get; }
        /// <summary>高度。</summary>
        public double Height { set; get; }

    }

}
