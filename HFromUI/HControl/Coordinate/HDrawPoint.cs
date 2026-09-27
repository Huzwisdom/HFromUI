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
    public class HDrawPoint : HDrawBase, iHDrawContinuousLine, iHGode
    {
        private HPoint3D center;

        public HPoint3D this[string n]
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    if (n == "Center")
                    {
                        return Center;
                    }
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(n))
                {
                    if (n == "Center")
                    {
                        Center = value;
                    }
                }
            }
        }
        [HCategoryLanguage("工艺参数")]
        [HDisplayNameLanguage("参数")]
        [HDescriptionLanguage("设置设备的工艺参数")]
        /// <summary>ProcessParameters 成员。</summary>
        /// <summary>ProcessParameters 字段。</summary>
        [Browsable(true)]
        public HDrawProcessParameters ProcessParameters { get; set; } = new HDrawProcessParametersDefault();
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
        HPoint iHDrawContinuousLine.Start
        {
            get
            { return center; }
        }
        HPoint iHDrawContinuousLine.End
        {
            get
            { return center; }
        }
        HDouble iHDrawContinuousLine.Length2D
        {
            get
            { return 0; }
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
        /// <summary>获取 pointAtDistance2D。</summary>
        public HPoint GetPointAtDistance2D(HDouble distance)
        {
            return center;
        }

        /// <summary>加载。</summary>
        public override OK<HDrawBase> Load(string[] data, int mode)
        {
            OK<HDrawBase> oK = true;
            if (mode == 1)
            {
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawPoint"))
                {
                    return false;
                }
            }
            else if (mode == 2)
            {
                string Parameters = "";
                HDrawPoint HDrawBase = new HDrawPoint();
                HDrawBase.Color = HDrawList.DefaultColor;
                HDrawBase.LineWidth = HDrawList.DefaultLineWidth;
                foreach (var item in data)
                {
                    string buffer = HStringPath.GetStartsWithStringEnd(item, "GUID=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.GuidCode = buffer;
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Center=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Center = HPoint3D.Parse(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Layer=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Layer = Convert.ToInt32(buffer);
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(item, "Name=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Name = buffer;
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
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawPoint"))
                {
                    return false;
                }
                string buffer = HStringPath.GetStartsWithStringEnd(data[0], "[" + "HDrawPoint" + "_Start" + "]").Trim();
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
                stringBuilder.Append("HDrawPoint");
                stringBuilder.Append("_");
                stringBuilder.Append("Start");
                stringBuilder.Append("]");
                stringBuilder.Append(GuidCode);
                stringBuilder.Append("\r\n");

                stringBuilder.Append("Name=" + Name + "\r\n");
                stringBuilder.Append("GUID=" + GuidCode + "\r\n");
                stringBuilder.Append("Center=" + Center.ToString().Trim('(').Trim(')') + "\r\n");
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
                stringBuilder.Append("HDrawPoint");
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
        private HPoint3D centerTemp;
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
            else if (mode == 6)
            {
                centerTemp = center.Clone3D();
                ok = true;
            }
            else if (mode == 7)
            {
                if (hPoint3D.Length > 0)
                {
                    if (centerTemp == null )
                    {
                        return false;
                    }
                    center = centerTemp + hPoint3D[0]; 
                    ok = true;
                }
                else
                {
                    ok = OK.FromMessage(HTranslation.GetContent("长度不对，应为1，实际：{0}", hPoint3D.Length.ToString()));
                }
            }
            else if (mode == 8)
            {
                centerTemp = null;
            }
            return ok;
        }
        public HDrawPoint()
        { }
        public HDrawPoint(HPoint3D center,  Color color, HDouble lineWidth, string name)
        {
            this.center = center;
            Color = color;
            LineWidth = lineWidth;
            Name = name;
        }
        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen ,HBool isSelected, HDouble scale)
        {
            HPoint screenCenter = worldToScreen(Center);
            HDouble screenRadius =Math.Min(5.0, (HDrawList.DrawPointSize * scale).Value);

            HRect rect = new HRect(
                screenCenter.X.Value - screenRadius,
                screenCenter.Y.Value - screenRadius,
                screenRadius * 2,
                screenRadius * 2);
            using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color,( isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
            {
                pen.DashStyle = PenMode;
                g.DrawEllipse(pen, rect.ToRectangleF());
                g.FillEllipse(new SolidBrush(isSelected ? HDrawList.SelectedColor : Color), rect.ToRectangleF());
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
            this.ProcessParameters.Clear();
            center  = null;
        }
        /// <summary>克隆。</summary>
        public override HDrawBase Clone(int mode=0)
        {
            HDrawPoint HDrawPoint = new HDrawPoint(center, Color, LineWidth, Name);
            HDrawPoint.Layer = this.Layer;
            HDrawPoint.ProcessParameters = this.ProcessParameters.Clone();
            HDrawPoint.PenMode = this.PenMode;
            if (mode==0)
            {
                HDrawPoint.GuidCode = this.GuidCode;
            }
            HDrawPoint.Index = this.Index;
            HDrawPoint.ShowIndexName = this.ShowIndexName;
            return HDrawPoint;
        }
    }
}