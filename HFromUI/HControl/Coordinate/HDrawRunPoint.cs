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
    public class HDrawRunPoint : HDrawBase
    {
        /// <summary>center 字段。</summary>
        private HPoint3D center=null;

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
        /// <summary>Set 方法。</summary>
        public void Set(HPoint3D center3)
        {
            Center = center3;
        }
        /// <summary>Set 方法。</summary>
        public void Set(HPoint center2)
        {
            HDouble z = (Center == null || Center.Z == null) ? 0 : Center.Z.Value;
            Center = new HPoint3D(center2, z.Value); ;
        }
        /// <summary>PointSize 成员。</summary>
        /// <summary>PointSize 字段。</summary>
        [Browsable(false)]
        public HDouble PointSize { set; get; } = 10;
        public HDrawRunPoint(HPoint3D center, Color color, HDouble lineWidth, string name)
        {
            this.center = center;
            Color = color;
            LineWidth = lineWidth;
            Name = name;
        }
        public HDrawRunPoint()
        { 
        
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
        //public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        //{
        //    if (Center == null)
        //    {
        //        return;
        //    }
        //    HPoint screenCenter = worldToScreen(Center);
        //    HDouble pointSize = Math.Min(10, PointSize);


        //    //// --- 变换顺序：平移 → 旋转 → 平移（回移）
        //    //// 1. 先将原点平移到矩形的中心
        //    //g.TranslateTransform(centerX, centerY);
        //    //// 2. 绕当前原点（即矩形中心）旋转
        //    //g.RotateTransform(rotationAngle);
        //    //// 3. 再平移到矩形左上角（以中心为基准）
        //    //g.TranslateTransform(-width / 2, -height / 2);

        //    //// 现在在变换后的坐标系中绘制矩形，其左上角在 (0,0)
        //    //g.FillRectangle(Brushes.CornflowerBlue, 0, 0, width, height);
        //    //g.DrawRectangle(Pens.Black, 0, 0, width, height);

        //    //// 如需重置，避免影响后续绘制
        //    //g.ResetTransform();

        //    using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
        //    {
        //        pen.DashStyle = PenMode= DashStyle.DashDot;
        //        g.RotateTransform(angle.ToSingle());
        //        angle += 10;
        //        g.DrawLine(pen, (PointF)new HPoint(screenCenter.X.Value - pointSize, screenCenter.Y.Value - pointSize), (PointF)new HPoint(screenCenter.X.Value - pointSize + pointSize * 2, screenCenter.Y.Value - pointSize + pointSize * 2));
        //        g.DrawLine(pen, (PointF)new HPoint(screenCenter.X.Value - pointSize + pointSize * 2, screenCenter.Y.Value - pointSize), (PointF)new HPoint(screenCenter.X.Value - pointSize, screenCenter.Y.Value - pointSize + pointSize * 2));
        //        HDouble screenRadius = HDouble.Random(0.5* pointSize,0.8* pointSize);
        //        HRect rect = new HRect(screenCenter.X.Value - screenRadius,screenCenter.Y.Value - screenRadius,screenRadius * 2, screenRadius * 2);
        //        g.DrawEllipse(pen, (RectangleF)rect);
        //        g.FillEllipse(new SolidBrush(isSelected ? HDrawList.SelectedColor : Color), (RectangleF)rect);
        //        g.ResetTransform();
        //    }
        //}
        //public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        //{
        //    if (Center == null) return;

        //    HPoint screenCenter = worldToScreen(Center);
        //    float size = (float)Math.Min(10.0, PointSize.Value);

        //    using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color,
        //                             (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
        //    {
        //        pen.DashStyle = DashStyle.DashDot;

        //        var state = g.Save();
        //        // 平移 → 旋转
        //        g.TranslateTransform(screenCenter.X.ToSingle(), screenCenter.Y.ToSingle());
        //        g.RotateTransform(angle.ToSingle());  // 这里 angle 应该由外部设置为需要的值，不要在 Draw 里累加

        //        // 现在 (0,0) 就是图形中心，绘制相对坐标
        //        float half = size / 2;
        //        g.DrawLine(pen, -half, -half, half, half);
        //        g.DrawLine(pen, -half, half, half, -half);

        //        float r = (float)(HDouble.Random(0.5 * size, 0.8 * size)); // 如果每次随机，圆的大小会闪烁，建议固定
        //        g.DrawEllipse(pen, -r, -r, r * 2, r * 2);
        //        g.FillEllipse(new SolidBrush(pen.Color), -r, -r, r * 2, r * 2);

        //        g.Restore(state);
        //    }
        //}
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            if (Center == null) return;

            HPoint screenCenter = worldToScreen(Center);
            float cx = screenCenter.X.ToSingle();
            float cy = screenCenter.Y.ToSingle();
            scale = 1.8; // 原有放大系数，保留
            float size = (float)Math.Min(10.0, PointSize.Value);
            float totalDiameter = size * scale.ToSingle();
            float R = totalDiameter * 0.5f;   // 星形外接圆半径
            float innerR = R * 0.45f;         // 内顶点半径
            float outlineW = ((isSelected ? LineWidth + 2 : LineWidth) * scale).ToSingle();

            // 自动旋转角度
            float autoAngle = (Environment.TickCount % 36000) / 100.0f;

            var state = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(autoAngle);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // ---- 构建十二角星形路径（蓝色主题）----
            using (GraphicsPath starPath = new GraphicsPath())
            {
                int points = 12;
                float angleStep = 360f / (points * 2);
                List<PointF> pointList = new List<PointF>();

                for (int i = 0; i < points * 2; i++)
                {
                    float r = (i % 2 == 0) ? R : innerR;
                    float angle = i * angleStep - 90;
                    float rad = angle * (float)Math.PI / 180f;
                    float x = r * (float)Math.Cos(rad);
                    float y = r * (float)Math.Sin(rad);
                    pointList.Add(new PointF(x, y));
                }

                starPath.AddPolygon(pointList.ToArray());

                // 渐变：中心亮蓝白，边缘深蓝金属
                using (PathGradientBrush brush = new PathGradientBrush(starPath))
                {
                    brush.CenterColor = Color.FromArgb(255, 200, 225, 255);   // 浅蓝白高光
                    brush.SurroundColors = new Color[] { Color.FromArgb(200, 10, 50, 140) }; // 深蓝
                    brush.CenterPoint = new PointF(0, 0);
                    g.FillPath(brush, starPath);
                }

                // 星形轮廓（暗蓝边）
                using (Pen starPen = new Pen(Color.FromArgb(160, 20, 60, 140), 0.8f * scale.ToSingle()))
                {
                    g.DrawPath(starPen, starPath);
                }
            }

            // ---- 外围蓝色虚线扫描弧 ----
            float haloR = R * 1.05f;
            using (Pen scanPen = new Pen(Color.FromArgb(100, 80, 180, 255), 1.2f * scale.ToSingle()))
            {
                scanPen.DashStyle = DashStyle.Dash;
                scanPen.DashPattern = new float[] { 3f, 4f };
                g.DrawArc(scanPen, -haloR, -haloR, haloR * 2, haloR * 2, -20, 60);
            }

            // ---- 外围淡蓝光晕 ----
            using (Pen glowPen = new Pen(Color.FromArgb(40, 60, 160, 255), 1.5f * scale.ToSingle()))
            {
                g.DrawEllipse(glowPen, -haloR, -haloR, haloR * 2, haloR * 2);
            }

            // ---- 动态高光点（白色微蓝）----
            float highlightAngle = autoAngle * 2.5f;
            float hx = (float)(R * 0.6 * Math.Cos(highlightAngle * Math.PI / 180.0));
            float hy = (float)(R * 0.6 * Math.Sin(highlightAngle * Math.PI / 180.0));
            using (GraphicsPath highlightPath = new GraphicsPath())
            {
                highlightPath.AddEllipse(hx - R * 0.1f, hy - R * 0.1f, R * 0.2f, R * 0.2f);
                using (PathGradientBrush hBrush = new PathGradientBrush(highlightPath))
                {
                    hBrush.CenterColor = Color.FromArgb(200, 240, 250, 255); // 冷白
                    hBrush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillEllipse(hBrush, hx - R * 0.1f, hy - R * 0.1f, R * 0.2f, R * 0.2f);
                }
            }

            // ---- 选中高亮 ----
            if (isSelected)
            {
                using (Pen selPen = new Pen(HDrawList.SelectedColor, outlineW))
                {
                    g.DrawEllipse(selPen, -R - 1f * scale.ToSingle(), -R - 1f * scale.ToSingle(),
                                  (R + 1f * scale.ToSingle()) * 2, (R + 1f * scale.ToSingle()) * 2);
                }
            }

            g.Restore(state);
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
        public override HDrawBase Clone(int mode = 0)
        {
            return new HDrawPointCross(center, Color, LineWidth, Name);
        }
    }
}