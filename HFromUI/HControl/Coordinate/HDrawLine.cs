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

    public class HDrawLine: HDrawBase, iHDrawContinuousLine, iHGode
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
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    if (n == "Start")
                    {
                         Start=value;
                    }
                    else if (n == "End")
                    {
                         End =value;
                    }
                }
            }
        }
        private HPoint3D start;
        private HPoint3D end;
        private HDouble length2D;

        [HCategoryLanguage("工艺参数")]
        [HDisplayNameLanguage("参数")]
        [HDescriptionLanguage("设置设备的工艺参数")]
        /// <summary>ProcessParameters 成员。</summary>
        /// <summary>ProcessParameters 字段。</summary>
        [Browsable(true)]
        public HDrawProcessParameters ProcessParameters { get; set; }=new HDrawProcessParametersDefault();

        [HCategoryLanguage("几何"), HDescriptionLanguage("起点")]
        [HDisplayNameLanguage("起点")]
        [Browsable(true)]
        public HPoint3D Start
        {
            get { return start; }
            set
            {
                start = value;
                OnPropertyChanged();
                length2D= HDrawList.Distance(Start, End);
            }
        }
        [HCategoryLanguage("几何"), HDescriptionLanguage("2D长度")]
        [HDisplayNameLanguage("2D长度")]
        [Browsable(true)]
        public HDouble Length2D
        {
            get
            {
                if (Start.X.Value!= End.X.Value|| Start.Y.Value != End.Y.Value)
                {
                    length2D = HDrawList.Distance(Start, End);
                }
                return length2D;
            }
            set
            {
                length2D = value;
                HDouble endz = end.Z.Value;
                End = new HPoint3D( HDrawList.CalculateEndPoint2D(Start,end,value), endz.Value);
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
                length2D = HDrawList.Distance(Start, End);
            }
        }

        HPoint iHDrawContinuousLine.Start { get
            { return start; } }
        HPoint iHDrawContinuousLine.End { get
            { return end; } }
        HDouble iHDrawContinuousLine.Length2D
        {
            get
            { return length2D = HDrawList.Distance(Start, End); }
        }
        HDouble iHDrawContinuousLine.Speed
        {
            get
            {
                if (ProcessParameters != null)
                {
                    return ProcessParameters.Speed;
                }
                return 1200d;
            }
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
                ProcessParameters.GCode = value;
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
        public HDrawLine()
        { }
        public HDrawLine(HPoint3D start, HPoint3D end, Color color, HDouble lineWidth, string name)
        {
            this.start = start;
            this.end = end;
            Color = color;
            LineWidth = lineWidth;
            Name = name;
        }
        /// <summary>加载。</summary>
        public override OK<HDrawBase> Load(string[] data, int mode)
        {
            OK<HDrawBase> oK = true;
            if (mode == 1)
            {
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawLine"))
                {
                    return false;
                }
            }
            else if (mode == 2)
            {
                string Parameters = "";
                HDrawLine HDrawBase = new HDrawLine();
                HDrawBase.Color = HDrawList.DefaultColor;
                HDrawBase.LineWidth = HDrawList.DefaultLineWidth;
                foreach (var item in data)
                {
                    string buffer = HStringPath.GetStartsWithStringEnd(item, "GUID=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.GuidCode = buffer;
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
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Name=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Name = buffer;
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Layer=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Layer = Convert.ToInt32(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "ProcessParameters=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        Parameters = buffer;
                        Type type = buffer.Contains(".") ? Type.GetType(buffer) : Type.GetType("HFromUI.HControl." + buffer);
                        HDrawBase.ProcessParameters = (HDrawProcessParameters)Activator.CreateInstance(type);
                    }
                    if (HDrawBase.ProcessParameters != null && !string.IsNullOrWhiteSpace(Parameters))
                    {
                        int index = item.IndexOf('=');
                        if (index >= 0)
                        {
                            string key = item.Substring(0, index);
                            buffer = item.Substring(index + 1);
                            string[] keys = key.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                            if (!string.IsNullOrWhiteSpace(buffer) && keys.Length >= 2)
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
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawLine"))
                {
                    return false;
                }
                string buffer = HStringPath.GetStartsWithStringEnd(data[0], "[" + "HDrawLine" + "_Start" + "]").Trim();
                if (buffer != this.GuidCode.Trim())
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

            if (mode == 2)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                stringBuilder.Append("HDrawLine");
                stringBuilder.Append("_");
                stringBuilder.Append("Start");
                stringBuilder.Append("]");
                stringBuilder.Append(GuidCode);
                stringBuilder.Append("\r\n");

                stringBuilder.Append("Name=" + Name + "\r\n");
                stringBuilder.Append("GUID=" + GuidCode + "\r\n");
                stringBuilder.Append("Start=" + Start.ToString().Trim('(').Trim(')') + "\r\n");
                stringBuilder.Append("End=" + End.ToString().Trim('(').Trim(')') + "\r\n");
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
                stringBuilder.Append("HDrawLine");
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
        private HPoint3D startTemp;
        private HPoint3D endTemp;
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
                    start = hPoint3D[0]; end = hPoint3D[1];
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为2，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            if (mode == 2)
            {
                if (hPoint3D.Length > 0)
                {
                    start = start + hPoint3D[0]; end = end + hPoint3D[0];
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
                    start = hPoint3D[0]; end = end + hPoint3D1; 
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
                    start = start + hPoint3D1; end = hPoint3D[0];
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
                ok = true;
            }
            else if (mode == 7)
            {
                if (hPoint3D.Length > 0)
                {
                    if (startTemp == null || endTemp == null )
                    {
                        return false;
                    }
                    start = startTemp + hPoint3D[0]; end = endTemp + hPoint3D[0]; 
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            else if (mode == 8)
            {
                startTemp = endTemp  = null;
            }
            return ok;
        }
        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
            {
                pen.DashStyle = PenMode;
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen,(PointF) worldToScreen(Start), (PointF)worldToScreen(End));
            }
        }
        /// <summary>
        /// 获取从起点沿直线前进指定距离后的二维点。
        /// 距离可为负（反向延伸），也可超出线段长度（外插）。
        /// </summary>
        /// <param name="distance">从起点沿直线前进的距离（世界单位）</param>
        /// <returns>目标点（二维），若线段长度为零则返回起点。</returns>
        public HPoint GetPointAtDistance2D(HDouble distance)
        {
            double dist = distance.Value;

            // 计算线段向量
            double dx = (double)(end.X.Value - start.X.Value);
            double dy = (double)(end.Y.Value - start.Y.Value);
            double len = Math.Sqrt(dx * dx + dy * dy);

            // 线段长度为零，无法确定方向，直接返回起点
            if (len < HAppData.EpsilonSmall)
                return new HPoint(start.X.Value, start.Y.Value);

            // 单位方向向量 * 距离
            double t = dist / len;
            return new HPoint(
                start.X.Value + t * dx,
                start.Y.Value + t * dy
            );
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint worldPoint, HDouble scaleHeight)
        {
            return HLine.IsOnLine(start, end, worldPoint, scaleHeight);
        }
        /// <summary>获取 point。</summary>
        public override HPoint3D[] GetPoint(HInt mode, HDouble scale)
        {   
            if (mode == 9)
            {
                return new HPoint3D[] { end };
            }
            return new HPoint3D[] { start, end };
        }
        /// <summary>清空。</summary>
        public override void Clear()
        {
            this.Name = this.ShowIndexName = this.GuidCode = string.Empty;
            this.ProcessParameters.Clear();
            start = end =  null;
        }
        /// <summary>克隆。</summary>
        public override HDrawBase Clone(int mode=0)
        {
            HDrawLine HDrawLine =new HDrawLine(start, end, Color, LineWidth, Name);
            HDrawLine.Layer = this.Layer;
            HDrawLine.ProcessParameters = this.ProcessParameters.Clone();
            HDrawLine.PenMode = this.PenMode;
            if (mode==0)
            {
                HDrawLine.GuidCode = this.GuidCode;
            }
           
            HDrawLine.Index = this.Index;
            HDrawLine.ShowIndexName = this.ShowIndexName;
            return HDrawLine;
        }
    }
}