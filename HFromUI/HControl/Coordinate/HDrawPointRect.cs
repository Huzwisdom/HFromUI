using HFromUI.HAttribute;
using HFromUI.HBase;
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

namespace HFromUI.HControl.Coordinate
{
    using HFromUI.HLangage;
    public class HDrawPointRect : HDrawBase
    {
        private HPoint3D center;

        [HCategoryLanguage("几何"), HDescriptionLanguage("点")]
        [HDisplayNameLanguage("点")]
        [Browsable(true)]
        public HPoint3D Center
        {
            get { return center; }
            set
            {
                center = value;
                OnPropertyChanged();
            }
        }
        public HDrawPointRect(HPoint3D center, Color color, HDouble lineWidth, string name)
        {
            this.center = center;
            Color = color;
            LineWidth = lineWidth;
            Name = name;
        }
        /// <summary>Move 方法。</summary>
        public override OK Move(HPoint3D[] hPoint3D, HInt mode)
        {
            OK ok = OK.FromMessage(HTranslation.GetContent("初始化{0}", mode.Value.ToString()));
            if (hPoint3D == null)
            {
                ok = OK.FromMessage(HTranslation.GetContent("入参hPoint3D为Null{0}", mode.Value.ToString()));
            }
            if (mode == 1)
            {
                if (hPoint3D.Length == 2)
                {
                    center = hPoint3D[0];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 2)
            {
                if (hPoint3D.Length > 0)
                {
                    center = center + hPoint3D[0];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 3)
            {
                if (hPoint3D.Length > 0)
                {
                    HPoint3D hPoint3D1 = hPoint3D[0] - center;
                    center = hPoint3D[0];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            return ok;
        }
        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            HPoint screenCenter = worldToScreen(Center);
               HDouble pointSize = Math.Min(5.0, (HDrawList.DrawPointSize * scale).Value);
           
            HRect rect = new HRect(
                screenCenter.X.Value - pointSize,
                screenCenter.Y.Value - pointSize,
                pointSize * 2,
                pointSize * 2);
            using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
            {
                pen.DashStyle = PenMode;
                g.DrawRectangle(pen, rect.X.ToSingle(), rect.Y.ToSingle(), rect.Width.ToSingle(), rect.Height.ToSingle());
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint worldPoint, HDouble scaleHeight)
        {
            return Math.Abs(HDrawList.Distance(worldPoint, center).Value) < scaleHeight;
        }
        /// <summary>获取 point。</summary>
        public override HPoint3D[] GetPoint(HInt mode, HDouble scale)
        {
            return new HPoint3D[] { center };
        }
        /// <summary>清空。</summary>
        public override void Clear()
        {
            this.Name = this.ShowIndexName = this.GuidCode = string.Empty;
            center = null;
        }
        /// <summary>克隆。</summary>
        public override HDrawBase Clone(int mode=0)
        {
            return new HDrawPointRect(center, Color, LineWidth, Name);
        }
    }
}