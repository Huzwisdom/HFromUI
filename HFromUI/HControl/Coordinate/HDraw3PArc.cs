using HFromUI.HAttribute;
using HFromUI.HBase;
using HFromUI.HConvert;
using HFromUI.HData;
using HFromUI.HInterface;
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
    public class HDraw3PArc : HDrawBase, iHDrawContinuousLine, iHGode
    {
        public HPoint3D this[string n]
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    if (n == "Start")
                    {
                        return Start;
                    }
                    else if (n == "End")
                    {
                        return End;
                    }
                    else if (n == "Middle")
                    {
                        return Middle;
                    }
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    if (n == "Start")
                    {
                        Start = value;
                    }
                    else if (n == "End")
                    {
                        End = value;
                    }
                    else if (n == "Middle")
                    {
                        Middle = value;
                    }
                }
            }
        }
        private HPoint3D start;
        private HPoint3D end;
        private HPoint3D middle;

        [HCategoryLanguage("工艺参数")]
        [HDisplayNameLanguage("参数")]
        [HDescriptionLanguage("设置设备的工艺参数")]
        /// <summary>ProcessParameters 成员。</summary>
        /// <summary>ProcessParameters 字段。</summary>
        [Browsable(true)]
        public HDrawProcessParameters ProcessParameters { get; set; } = new HDrawProcessParametersDefault();
        [HCategoryLanguage("几何"), HDescriptionLanguage("起点")]
        [HDisplayNameLanguage("起点")]
        [Browsable(true)]
        public  HPoint3D Start
        {
            get { return start; }
            set
            {
                start = value;
                OnPropertyChanged();
            }
        }
        [HCategoryLanguage("几何"), HDescriptionLanguage("终点")]
        [HDisplayNameLanguage("终点")]
        [Browsable(true)]
        public HPoint3D End
        {
            get { return end; }
            set
            {
                end = value;
                OnPropertyChanged();
            }
        }
        [HCategoryLanguage("几何"), HDescriptionLanguage("中间点")]
        [HDisplayNameLanguage("中间点")]
        [Browsable(true)]
        public HPoint3D Middle
        {
            get { return middle; }
            set
            {
                middle = value;
                OnPropertyChanged();
            }
        }
        [HCategoryLanguage("几何"), HDescriptionLanguage("圆心")]
        [HDisplayNameLanguage("圆心")]
        [Browsable(true)]
        public HPoint CircleCenter
        {
            get
            {
                return CalculateCircleCenterAlt(start, end, middle);
            }
        }

        [HCategoryLanguage("几何"), HDescriptionLanguage("半径")]
        [HDisplayNameLanguage("半径")]
        [Browsable(true)]
        public HDouble CircleRadius2D
        {
            get
            {
                return HDrawList.Distance(CircleCenter,start);
            }
        }
        [HCategoryLanguage("几何"), Browsable(true), HDescriptionLanguage("方向")]
        [HDisplayNameLanguage("方向")]
        public bool Direction
        {
            get
            {
                return IsPointUpper();
            }
        }
        [HCategoryLanguage("几何"), HDescriptionLanguage("获取二维圆弧的弧长（周长）；若三点共线则返回线段长度。")]
        [HDisplayNameLanguage("2D长度")]
        [Browsable(true)]
        /// <summary>
        /// 获取二维圆弧的弧长（周长）。若三点共线则返回线段长度。
        /// </summary>
        public HDouble ArcLength2D
        {
            get
            {
                HPoint center = CircleCenter;
                if (center == null) // 共线退化为线段
                {
                    return HDrawList.Distance(Start, End);
                }

                // 计算扫掠角（与 Draw 方法完全一致）
                HDouble startDeg = HDrawList.CircleAngle(center, Start);
                HDouble endDeg = HDrawList.CircleAngle(center, End);
                HDouble sweepDeg = endDeg - startDeg;
                if (sweepDeg < 0) sweepDeg += 360;
                if (!Direction) // 顺时针时扫掠角为负
                    sweepDeg -= 360;

                double sweepRad = Math.Abs(sweepDeg.Value) * Math.PI / 180.0;
                return CircleRadius2D.Value * sweepRad;
            }
        }
        HPoint iHDrawContinuousLine.Start
        {
            get
            { return start; }
        }
        HPoint iHDrawContinuousLine.End
        {
            get
            { return end; }
        }
        HDouble iHDrawContinuousLine.Length2D
        {
            get
            {
                HPoint center = CircleCenter;
                if (center == null) // 共线退化为线段
                {
                    return HDrawList.Distance(Start, End);
                }

                // 计算扫掠角（与 Draw 方法完全一致）
                HDouble startDeg = HDrawList.CircleAngle(center, Start);
                HDouble endDeg = HDrawList.CircleAngle(center, End);
                HDouble sweepDeg = endDeg - startDeg;
                if (sweepDeg < 0) sweepDeg += 360;
                if (!Direction) // 顺时针时扫掠角为负
                    sweepDeg -= 360;

                double sweepRad = Math.Abs(sweepDeg.Value) * Math.PI / 180.0;
                return CircleRadius2D.Value * sweepRad;
            }
        }
        HDouble iHDrawContinuousLine.Speed
        {
            get
            {
                if (ProcessParameters!=null)
                {
                    return ProcessParameters.Speed;
                }
                return 1200d; }
        }

        GCodeText iHGode.GCode
        {
            get
            {
                if (ProcessParameters != null)
                {
                    GCodeText gCodeText = new GCodeText(ProcessParameters.GCode);
                    gCodeText.GUID = this.GuidCode;
                    return gCodeText;
                }
                return new GCodeText("");
            }
            set
            {
                ProcessParameters.GCode=value;
            }
        }
        GCodeText iHGode.Prefix
        {
            get
            {
                if (ProcessParameters != null)
                {
                    GCodeText gCodeText = new GCodeText(ProcessParameters.Prefix);
                    gCodeText.GUID = this.GuidCode;
                    return gCodeText;
                }
                return new GCodeText("");
            }
            set
            {
                ProcessParameters.Prefix = value;
            }
        }
        GCodeText iHGode.Suffix
        {
            get
            {
                if (ProcessParameters != null)
                {
                    GCodeText gCodeText = new GCodeText(ProcessParameters.Suffix);
                    gCodeText.GUID = this.GuidCode;
                    return gCodeText;
                }
                return new GCodeText("");
            }
            set
            {
                ProcessParameters.Suffix = value;
            }
        }
        /// <summary>
        /// 获取从起点沿圆弧方向前进指定弧长距离后的二维点。
        /// 距离可为负（反向延伸）。三点共线时沿线段移动。
        /// </summary>
        /// <param name="distance">从起点沿圆弧前进的距离（世界单位）</param>
        /// <returns>目标点（二维），若弧长为零则返回起点。</returns>
        public HPoint GetPointAtDistance2D(HDouble distance)
        {
            HPoint center = CircleCenter;
            double dist = distance.Value;

            // 退化为线段
            if (center == null)
            {
                double totalLen = HDrawList.Distance(Start, End).Value;
                if (totalLen < 1e-10) return new HPoint(Start.X.Value, Start.Y.Value);
                double t = dist / totalLen;
                return new HPoint(
                    start.X.Value + t * (end.X.Value - start.X.Value),
                    start.Y.Value + t * (end.Y.Value - start.Y.Value));
            }

            double radius = CircleRadius2D.Value;
            if (radius < 1e-10) return new HPoint(Start.X.Value, Start.Y.Value);

            // 与 Draw 完全一致的扫掠方向计算
            double startDeg = HDrawList.CircleAngle(center, Start).Value;
            double endDeg = HDrawList.CircleAngle(center, End).Value;
            double sweepDeg = endDeg - startDeg;
            if (sweepDeg < 0) sweepDeg += 360;
            if (!Direction) sweepDeg -= 360;   // 正=顺时针绘制，负=逆时针绘制

            // 沿弧移动的角度量（弧度）
            double angleDelta = dist / radius;
            // 根据绘制方向确定旋转的符号
            double signedDelta = sweepDeg >= 0 ? angleDelta : -angleDelta;

            // 圆心到起点的向量
            double vx = start.X.Value - center.X.Value;
            double vy = start.Y.Value - center.Y.Value;

            // 在屏幕坐标系（y下）中顺时针旋转 signedDelta 弧度
            // 顺时针旋转公式：x' = x·cos + y·sin, y' = -x·sin + y·cos
            double cosA = Math.Cos(signedDelta);
            double sinA = Math.Sin(signedDelta);
            double newVx = vx * cosA + vy * sinA;
            double newVy = -vx * sinA + vy * cosA;

            return new HPoint(center.X.Value + newVx, center.Y.Value + newVy);
        }
        public HDraw3PArc(HPoint3D start, HPoint3D end, HPoint3D middle, Color color, HDouble lineWidth, string name)
        {
            this.middle = middle;
            this.start = start;
            this.end = end;
            Color = color;
            LineWidth = lineWidth;
            Name = name;
        }
        public HDraw3PArc()
        { 
        
        }
        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            HPoint Center = CircleCenter;
            if (Center != null)
            {
                HDouble StartAngle = HDrawList.CircleAngle(Center, start);
                HDouble EndAngle = HDrawList.CircleAngle(Center, end);
                HDouble SweepAngle = EndAngle - StartAngle;
                if (SweepAngle < 0)
                {
                    SweepAngle = (360 + SweepAngle);
                }
                if (!Direction)
                {
                    SweepAngle = SweepAngle - 360;
                }


                HPoint screenCenter = worldToScreen(Center);
                HDouble screenRadius = HDrawList.Distance(start, Center) * scale;

                HRect rect = new HRect(
                    screenCenter.X.Value - screenRadius,
                    screenCenter.Y.Value - screenRadius,
                    screenRadius * 2,
                    screenRadius * 2);

                using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
                {
                    pen.EndCap = LineCap.ArrowAnchor;
                    pen.DashStyle = PenMode;
                    g.DrawArc(pen, rect.ToRectangleF(), StartAngle.ToSingle(), SweepAngle.ToSingle());
                }
            }
            else
            {
                using (var pen = new Pen(isSelected ? Color.Yellow : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
                {
                    pen.DashStyle = PenMode;
                    pen.EndCap = LineCap.ArrowAnchor;
                    g.DrawLine(pen,(PointF) worldToScreen(Start), (PointF)worldToScreen(End));
                }
            }
        }
        private HPoint3D startTemp;
        private HPoint3D endTemp;
        private HPoint3D middleTemp;
        /// <summary>Move 方法。</summary>
        public override OK Move(HPoint3D[] hPoint3D, HInt mode)
        {
            OK ok = OK.FromMessage(HTranslation.GetContent("初始化{0}",mode.Value.ToString()));
            if (hPoint3D==null)
            {
                ok = OK.FromMessage(HTranslation.GetContent("入参hPoint3D为Null{0}", mode.Value.ToString()));
            }
            if (mode==1)
            {
                if (hPoint3D.Length==3)
                {
                    start = hPoint3D[0]; end = hPoint3D[1]; middle = hPoint3D[2];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为3，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 2)
            {
                if (hPoint3D.Length > 0)
                {
                    start = start+ hPoint3D[0]; end = end+ hPoint3D[0]; middle = middle+ hPoint3D[0];
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
                    HPoint3D hPoint3D1 = hPoint3D[0] - start;
                    start =  hPoint3D[0]; end = end + hPoint3D1; middle = middle + hPoint3D1;
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 4)
            {
                if (hPoint3D.Length > 0)
                {
                    HPoint3D hPoint3D1 = hPoint3D[0] - end;
                    start = start+ hPoint3D1; end = hPoint3D[0]; middle = middle + hPoint3D1;
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 5)
            {
                if (hPoint3D.Length > 0)
                {
                    HPoint3D hPoint3D1 = hPoint3D[0] - middle;
                    start = start + hPoint3D1; end = end + hPoint3D1; middle = hPoint3D[0];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            else if (mode == 6)
            {
                startTemp = start.Clone3D();
                endTemp = end.Clone3D();
                middleTemp = middle.Clone3D();
                ok = true;
            }
            else if (mode == 7)
            {
                if (hPoint3D.Length > 0)
                {
                    if (startTemp == null || endTemp == null || middleTemp == null)
                    {
                        return false;
                    }
                    start = (startTemp + hPoint3D[0]); end = (endTemp + hPoint3D[0]); middle = (middleTemp + hPoint3D[0]);
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            else if (mode == 8)
            {
                startTemp = endTemp = middleTemp = null;
            }
            return ok;
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint worldPoint, HDouble scaleHeight)
        {
            HPoint Center = CircleCenter;
            if (Center!=null)
            {
                HDouble StartAngle = HDrawList.CircleAngle(Center, start);
                HDouble EndAngle = HDrawList.CircleAngle(Center, end);
                HDouble SweepAngle = EndAngle - StartAngle;
                if (SweepAngle < 0)
                {
                    SweepAngle = (360 + SweepAngle);
                }
                if (!IsPointUpper())
                {
                    SweepAngle = SweepAngle - 360;
                }
                HDouble dx = worldPoint.X.Value - Center.X.Value;
                HDouble dy = worldPoint.Y.Value - Center.Y.Value;
                HDouble distance = Math.Sqrt((dx * dx + dy * dy).Value);

                if (Math.Abs((distance - HDrawList.Distance(Center, start)).Value) >  scaleHeight) return false;
                while (!(StartAngle >= 0 && StartAngle < 360))
                {
                    if (StartAngle < 0)
                    {
                        StartAngle += 360;
                    }
                    else
                    {
                        StartAngle -= 360;
                    }
                }
                HDouble wAngle = 0;
                if (Direction)
                {
                    wAngle = HDrawList.CircleAngle(worldPoint, Center) - 180;
                }
                else
                {
                    wAngle = HDrawList.CircleAngle(worldPoint, Center) + 180;
                }

                while (!(wAngle >= 0 && wAngle < 360))
                {
                    if (wAngle < 0)
                    {
                        wAngle += 360;
                    }
                    else
                    {
                        wAngle -= 360;
                    }
                }
                if (SweepAngle > 0)
                {
                    if (StartAngle + SweepAngle > 360)
                    {
                        if ((wAngle >= StartAngle && wAngle <= 360) || (wAngle >= 0 && wAngle <= StartAngle + SweepAngle - 360))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (wAngle >= StartAngle && wAngle <= StartAngle + SweepAngle)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (StartAngle + SweepAngle < 0)
                    {
                        if ((wAngle >= 0 && wAngle <= StartAngle) || (StartAngle + SweepAngle + 360 <= wAngle && wAngle <= 360))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (wAngle >= StartAngle + SweepAngle && wAngle <= StartAngle)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return HLine.IsOnLine(start, end, worldPoint,scaleHeight);
            }
        }
        /// <summary>加载。</summary>
        public override OK<HDrawBase> Load(string[] data, int mode)
        {
            OK<HDrawBase> oK = false;
            if (mode == 1)
            {
                if (data == null || data.Length == 0|| !data[0].Contains("HDraw3PArc"))
                {
                    return false;
                }
            }
            else if (mode == 2)
            {
                HDraw3PArc HDrawBase =new HDraw3PArc();
                HDrawBase.Color = HDrawList.DefaultColor;
                HDrawBase.LineWidth = HDrawList.DefaultLineWidth;
                string Parameters = "";
                foreach (var item in data)
                {
                    string buffer = HStringPath.GetStartsWithStringEnd(item, "GUID=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.GuidCode= buffer;
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Name=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Name = buffer;
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Start=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.start = HPoint3D.Parse(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "End=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.end = HPoint3D.Parse(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Layer=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Layer =Convert.ToInt32( buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Middle=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.middle = HPoint3D.Parse(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "ProcessParameters=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        Parameters = buffer;
                        Type type = buffer.Contains(".")? Type.GetType(buffer): Type.GetType("HFromUI.HControl." + buffer);
                        HDrawBase.ProcessParameters = (HDrawProcessParameters)Activator.CreateInstance(type);
                    }
                    if (HDrawBase.ProcessParameters!=null&&!string.IsNullOrWhiteSpace(Parameters))
                    {
                        int index = item.IndexOf('=');
                        if (index >= 0)
                        {
                            string key = item.Substring(0, index);
                            buffer= item.Substring(index+1);
                            string[] keys= key.Split(new char[] { '.'}, StringSplitOptions.RemoveEmptyEntries);

                            if (!string.IsNullOrWhiteSpace(buffer)&& keys.Length>=2)
                            {
                                HDrawBase.ProcessParameters[keys[1]] = buffer;
                            }
                        }
    
                    }
                }
                oK.Value = HDrawBase;
                oK.IsSuccess = true;
            }
            else if (mode == 3)
            {
                if (data == null || data.Length == 0 || !data[0].Contains("HDraw3PArc"))
                {
                    return false;
                }
                string buffer = HStringPath.GetStartsWithStringEnd(data[0], "["+ "HDraw3PArc" + "_Start"+"]").Trim();
                if (buffer!=this.GuidCode.Trim())
                {
                    return false;
                }
            }
            return oK;
        }

        /// <summary>保存。</summary>
        public override OK<string> Save(int mode)
        {
            OK<string> oK = false;

            if (mode==2)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                stringBuilder.Append("HDraw3PArc");
                stringBuilder.Append("_");
                stringBuilder.Append("Start");
                stringBuilder.Append("]");
                stringBuilder.Append(GuidCode);
                stringBuilder.Append("\r\n");

                stringBuilder.Append("GUID="+  GuidCode+"\r\n");
                stringBuilder.Append("Name=" + Name + "\r\n");
                stringBuilder.Append("Start=" + Start.ToString().Trim('(').Trim(')') + "\r\n");
                stringBuilder.Append("End=" + End.ToString().Trim('(').Trim(')') + "\r\n");
                stringBuilder.Append("Middle=" + Middle.ToString().Trim('(').Trim(')') + "\r\n");
                stringBuilder.Append("Layer=" + Layer.ToString().Trim('(').Trim(')') + "\r\n");


                if (ProcessParameters != null)
                {
                    stringBuilder.Append("ProcessParameters=" + ProcessParameters.GetType().FullName.Trim('(').Trim(')') + "\r\n");
                    Dictionary<string, object> keyValuePairs = ProcessParameters.ToRawDictionary();
                    foreach (var item in keyValuePairs.Keys)
                    {
                        stringBuilder.Append($@"{ProcessParameters.GetType().Name}.{item}=" + keyValuePairs[item] + "\r\n");
                    }
                }

                stringBuilder.Append("[");
                stringBuilder.Append("HDraw3PArc");
                stringBuilder.Append("_");
                stringBuilder.Append("End");
                stringBuilder.Append("]");
                stringBuilder.Append(GuidCode);
                stringBuilder.Append("\r\n");
                oK = true;
                oK.Value = stringBuilder.ToString();
            }
            return oK;

        }
        /// <summary>获取 point。</summary>
        public override HPoint3D[] GetPoint(HInt mode, HDouble scale)
        {
            if (mode == 1)
            {
                H3PArc h3PArc = new H3PArc(Start, Middle, End);
                List<HPoint> points = h3PArc.ToPolyline();
                List<HPoint3D> point3Ds = new List<HPoint3D>();
                foreach (var item in points)
                {
                    point3Ds.Add(new HPoint3D(item, 0));
                }
                return point3Ds.ToArray();
            }
            else if (mode == 2)
            {
                HPoint center = CircleCenter;
                if (center==null)
                {
                    return null;
                }
                HDouble r = HDrawList.Distance(start, center);
                return new HPoint3D[] { new HPoint3D(center.X.Value - r.Value, center.Y.Value - r.Value,0),
                new HPoint3D(center.X.Value + r.Value, center.Y.Value + r.Value,0),
                new HPoint3D(center.X.Value - r.Value, center.Y.Value + r.Value,0),
                new HPoint3D(center.X.Value + r.Value, center.Y.Value - r.Value,0),};

            }
            else if (mode == 9)
            {
                return new HPoint3D[] {  end };
            }
                return new HPoint3D[] { start, end,middle };
        }

        /// <summary>CalculateCircleCenterAlt 方法。</summary>
        public HPoint CalculateCircleCenterAlt(HPoint A, HPoint B, HPoint C)
        {
            if (IsCollinear(A, B, C))
            { return null; }

            // AB中点
            HPoint midAB = new HPoint((A.X.Value + B.X.Value) / 2, (A.Y.Value + B.Y.Value) / 2);
            // AB垂直向量
            HPoint perpAB = new HPoint(B.Y.Value - A.Y.Value, A.X.Value - B.X.Value);

            // AC中点
            HPoint midAC = new HPoint((A.X.Value + C.X.Value) / 2, (A.Y.Value + C.Y.Value) / 2);
            // AC垂直向量
            HPoint perpAC = new HPoint(C.Y.Value - A.Y.Value, A.X.Value - C.X.Value);

            // 解参数方程：midAB + t * perpAB = midAC + s * perpAC
            // 转换为线性方程组
            HDouble a1 = perpAB.X.Value, b1 = -perpAC.X.Value, c1 = midAC.X.Value - midAB.X.Value;
            HDouble a2 = perpAB.Y.Value, b2 = -perpAC.Y.Value, c2 = midAC.Y.Value - midAB.Y.Value;

            HDouble det = a1 * b2 - a2 * b1;
            if (Math.Abs(det.Value) < HAppData.Epsilon)
            { return null; }

            HDouble t = (c1 * b2 - c2 * b1) / det;

            return new HPoint(
                midAB.X.Value + t * perpAB.X.Value,
                midAB.Y.Value + t * perpAB.Y.Value
            );
        }
        /// <summary>
        /// 获取圆弧的中点
        /// </summary>
        /// <returns></returns>
        public HPoint GetArcMidPoint()
        {
            HDouble halfLength = ArcLength2D / 2;
            return GetPointAtDistance2D(halfLength);
        }

        /// <summary>判断是否 PointUpper。</summary>
        private bool IsPointUpper()
        {
            // 向量 SE = End - Start
            HDouble seX = end.X.Value - start.X.Value;
            HDouble seY = end.Y.Value - start.Y.Value;
            // 向量 SM = Middle - Start
            HDouble smX = middle.X.Value - start.X.Value;
            HDouble smY = middle.Y.Value - start.Y.Value;

            // 二维叉积：SE × SM
            HDouble cross = seX * smY - seY * smX;

            // cross > 0 表示 Middle 在 SE 的左侧（逆时针方向）
            return cross > 0;
        }
        /// <summary>判断是否 Collinear。</summary>
        private bool IsCollinear(HPoint A, HPoint B, HPoint C)
        {
            // 计算向量AB和AC的叉积
            HDouble cross = (B.X.Value - A.X.Value) * (C.Y.Value - A.Y.Value) - (B.Y.Value - A.Y.Value) * (C.X.Value - A.X.Value);
            return Math.Abs(cross.Value) < HAppData.EpsilonSmall;
        }
        /// <summary>清空。</summary>
        public override void Clear()
        {
            this.Name=  this.ShowIndexName = this.GuidCode=string.Empty;
            this.ProcessParameters.Clear();
            start = end = middle = null;
        }
        /// <summary>克隆。</summary>
        public override HDrawBase Clone(int mode=0)
        {
            HDraw3PArc HDraw3PArc=new HDraw3PArc(start, end, Middle, Color, LineWidth, Name);
            HDraw3PArc.Layer = this.Layer;
            if (mode==0)
            {
                HDraw3PArc.GuidCode = this.GuidCode;
            }
            HDraw3PArc.Index = this.Index;
            HDraw3PArc.ShowIndexName = this.ShowIndexName;
            HDraw3PArc.PenMode=this.PenMode;
            
            HDraw3PArc.ProcessParameters=this.ProcessParameters.Clone();
            HDraw3PArc.PenMode=this.PenMode;
            return HDraw3PArc;
        }
    }
}