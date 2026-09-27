using HFromUI.HAttribute;
using HFromUI.HData;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HDrawBase: FlattenableObject
    {
        private string name;
        private string guidCode;
        private HDouble lineWidth;
        private Color color;
        /// <summary>索引。</summary>
        private HULong index = 0;
        /// <summary>layer 字段。</summary>
        private HInt layer = 0;
        internal int id;


        [Browsable(false)]
        public string GuidCode
        {
            get
            {
                if (string.IsNullOrWhiteSpace(guidCode))
                {
                    guidCode= Guid.NewGuid().ToString("N");
                }
                return guidCode;
            }
            set { guidCode = value; }
        }

        [Browsable(false)]
        /// <summary>错误描述。</summary>
        public string StrError { get; private set; }

        [Browsable(false)]
        /// <summary>画笔样式。</summary>
        public DashStyle PenMode { set; get; } = DashStyle.Solid;

        [Browsable(false)]
        /// <summary>显示索引名称。</summary>
        public string ShowIndexName { set; get; }

        [HCategoryLanguage("名称"), HDescriptionLanguage("图形名称")]
        [HDisplayNameLanguage("图形名称")]
        [Browsable(true)]
        public string Name
        {
            get { return name; }
            set
            {
                name = value; OnPropertyChanged();
            }
        }
        [HCategoryLanguage("外观"), HDescriptionLanguage("线条颜色")]
        [HDisplayNameLanguage("线条颜色")]
        [Browsable(false)]
        public Color Color
        {
            get { return color; }
            set
            {
                color = value; OnPropertyChanged();
            }
        }
        [HCategoryLanguage("位置"), HDescriptionLanguage("序号")]
        [HDisplayNameLanguage("序号")]
        [Browsable(true)]
        public int ID
        {
            get { return id+1; }
        }
        [HCategoryLanguage("外观"), HDescriptionLanguage("线宽")]
        [HDisplayNameLanguage("线宽")]
        [Browsable(false)]
        public HDouble LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = value; OnPropertyChanged();
            }
        }

        [HCategoryLanguage("编号"), HDescriptionLanguage("序号")]
        [HDisplayNameLanguage("序号")]
        [Browsable(true)]
        public HULong Index
        {
            get { return index; }
            set
            {
                index = value; OnPropertyChanged();
            }
        }

        [HCategoryLanguage("图层"), HDescriptionLanguage("图层")]
        [HDisplayNameLanguage("图层")]
        [Browsable(true)]
        public virtual HInt Layer
        {
            get
            { return layer; }
            set { layer = value; OnPropertyChanged(); }
        }
        /// <summary>绘制。</summary>
        public abstract void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale);
        /// <summary>命中测试。</summary>
        public abstract bool HitTest(HPoint worldPoint, HDouble scaleHeight);
        /// <summary>获取 point。</summary>
        public abstract HPoint3D[] GetPoint(HInt mode, HDouble scale);
        /// <summary>Move 方法。</summary>
        public abstract OK Move(HPoint3D[] hPoint3D, HInt mode);
        /// <summary>克隆。</summary>
        public abstract HDrawBase Clone(int mode=0);
        /// <summary>清空。</summary>
        public abstract void Clear();

        /// <summary>加载。</summary>
        public virtual OK<HDrawBase> Load(string [] data, int mode)
        { 
             throw new NotImplementedException();
        }
        /// <summary>保存。</summary>
        public virtual OK<string> Save(int mode)
        {
            throw new NotImplementedException();
        }

        public event EventHandler PropertyChanged;

        protected virtual void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}