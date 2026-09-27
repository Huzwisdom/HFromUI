using HFromUI.HAttribute;
using HFromUI.HBase;
using HFromUI.HConvert;
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
    using HFromUI.HColor;
    using HFromUI.HLangage;
    public class HDrawGroup : HDrawBase
    {
        [HCategoryLanguage("显示"), HDescriptionLanguage("IsShowRect 成员。")]
        [HDisplayNameLanguage("是否显示矩形")]
        /// <summary>IsShowRect 成员。</summary>
        /// <summary>IsShowRect 字段。</summary>
        [Browsable(false)]
        public HBool IsShowRect { set; get; } = true;
        public HDrawGroup(HList<HDrawBase> group)
        {
            Group = group;
        }
        public HDrawGroup()
        {

        }
        /// <summary>获取 rect。</summary>
        public HRectangle GetRect(int mode, Func<HPoint, HPoint> worldToScreen=null)
        {
            List<HDrawBase> group1 = Group.GetList(true);
            List<HPoint3D> points1 = new List<HPoint3D>();
            foreach (var item in group1)
            {
                points1.AddRange(item.GetPoint(1, 0));
            }
            HPoints hPoints = new HPoints(points1);
            HPoint hPoint1 = new HPoint(hPoints.Bounds.minX, hPoints.Bounds.minY);
            HPoint hPoint2 = new HPoint(hPoints.Bounds.maxX, hPoints.Bounds.maxY);
            if (worldToScreen!=null)
            {
                hPoint1 = worldToScreen(hPoint1);
                hPoint2 = worldToScreen(hPoint2);
            }
            return new HRectangle(hPoint1, hPoint2);
        }



        /// <summary>group 字段。</summary>
        private HList<HDrawBase> group=new HList<HDrawBase>();

        [Browsable(false)]
        public HList<HDrawBase> Group
        {
            get
            { 
                return group;
            }
            set
            {
                group = value;
            }
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            foreach (var item in group)
            {
                item.Draw(g, worldToScreen, isSelected, scale);
                if (Color == null)
                {
                    Color = item.Color;
                }
                if (LineWidth <= 0)
                {
                    LineWidth = item.LineWidth;
                }
            }
            if (IsShowRect)
            {
                Color = HColors.Blues.AegeanBlue; LineWidth = 1.5;
                using (var pen = new Pen(isSelected ? HDrawList.SelectedColor : Color, (isSelected ? LineWidth + 2 : LineWidth).ToSingle()))
                {
                    HRectangle hRectangle = GetRect(1, worldToScreen);
                    pen.DashStyle = DashStyle.DashDot; ;
                    g.DrawRectangle(pen, hRectangle.ToDrawingRectangle());
                    using (Brush brush = new SolidBrush(Color.FromArgb(50, pen.Color.R, pen.Color.G, pen.Color.B))) // 20% transparency = 0.2 * 255 ≈ 51
                    {
                        g.FillRectangle(brush, hRectangle.ToDrawingRectangleF());
                    }
                }
            }
        }

        /// <summary>
        /// 从字符串数组反序列化还原为 HDrawGroup 对象。
        /// </summary>
        /// <param name="data">
        /// 待解析的文本行数组，格式与 <see cref="Save(int)"/> 中 mode=2 产生的格式一致：
        ///   [HDrawGroup_Start]GUID
        ///   Name=xxx
        ///   GUID=xxx
        ///   ... 嵌套子图元的 Save(2) 数据块 ...
        ///   [HDrawGroup_End]GUID
        /// </param>
        /// <param name="mode">
        /// 操作模式：
        ///   1 - 仅校验 data 是否以 HDrawGroup 标记开头；
        ///   2 - 完整反序列化，返回填充好的 HDrawGroup 对象；
        ///   3 - 校验数据块是否属于当前实例（通过 GUID 匹配）。
        /// </param>
        /// <returns>
        /// 成功时 IsSuccess = true，且：
        ///   - mode=2 时 Value 为重建的 HDrawGroup 对象；
        ///   - mode=1、3 时 Value 可能为空。
        /// 失败时 IsSuccess = false。
        /// </returns>
        public override OK<HDrawBase> Load(string[] data, int mode)
        {
            OK<HDrawBase> oK = true;

            // ── mode == 1：快速头部校验 ─────────────────────────────────
            if (mode == 1)
            {
                // 仅验证数据是否以 HDrawGroup 开头
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawGroup"))
                {
                    return false;   // 隐式转换为 OK<HDrawBase>，表示失败
                }
            }
            // ── mode == 2：完整反序列化（递归）─────────────────────────
            else if (mode == 2)
            {
                // 创建目标对象，并赋予默认颜色与线宽
                HDrawGroup HDrawBase = new HDrawGroup();
                HDrawBase.Color = HDrawList.DefaultColor;
                HDrawBase.LineWidth = HDrawList.DefaultLineWidth;
                int index = 0;

                // 第一步：读取 HDrawGroup 自身的属性（跳过首行，直到遇到第一个子图元开始标记）
                for (; index < data.Length; index++)
                {
                    string line = data[index];
                    if (index == 0) continue;   // 跳过 "[HDrawGroup_Start]GUID" 行

                    // 尝试解析 Name 属性
                    string buffer = HStringPath.GetStartsWithStringEnd(line, "Name=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    { HDrawBase.Name = buffer; }

                    // 尝试解析 GUID 属性
                    buffer = HStringPath.GetStartsWithStringEnd(line, "GUID=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.GuidCode = buffer;
                    }
                    buffer = HStringPath.GetStartsWithStringEnd(line, "Layer=");
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        HDrawBase.Layer = Convert.ToInt32(buffer);
                    }
                    // 当遇到任意子图元的开始标记时，停止读取自身属性
                    if (line.StartsWith("[") && line.Contains("_Start"))
                        break;

                    // 本行既不是属性行也不是子图元开始标记，则继续（例如空行或注释）
                }

                // 第二步：解析嵌套的子图元数据块（从 index 开始）
                for (int i = index; i < data.Length; i++)
                {
                    string line = data[i];

                    // 识别任意子图元开始标记 [TypeName_Start]
                    if (!line.StartsWith("[") || !line.Contains("_Start"))
                        continue;

                    // 提取类型名（例如 "HDrawLine"、"HDraw3PArc"、"HDrawPoint"、"HDrawGroup"）
                    int underscoreIdx = line.IndexOf('_');
                    if (underscoreIdx < 0) continue;
                    string typeName = line.Substring(1, underscoreIdx - 1);   // 去掉开头的 '['，截取到 '_' 前

                    // 使用深度计数器找到对应结束行，支持嵌套（例如 HDrawGroup 内部再嵌套 HDrawGroup）
                    int depth = 1;
                    int j = i + 1;
                    while (j < data.Length && depth > 0)
                    {
                        if (data[j].StartsWith("[") && data[j].Contains("_Start"))
                            depth++;                // 遇到嵌套的开始标记，深度+1
                        else if (data[j].StartsWith("[") && data[j].Contains("_End"))
                            depth--;                // 遇到结束标记，深度-1
                        j++;
                    }
                    // 循环结束时 j 指向结束标记的下一行，故实际结束行索引为 j-1
                    int endIdx = j - 1; // 若 depth 一直大于 0 直到末尾，endIdx 为最后一行（但正常数据不应出现）

                    // 收集该子图元的完整数据块（包括开始和结束标记行）
                    List<string> subData = new List<string>();
                    for (int k = i; k <= endIdx; k++)
                        subData.Add(data[k]);

                    // 根据类型名创建对应的空对象
                    HDrawBase subItem = null;
                    switch (typeName)
                    {
                        case "HDrawLine": subItem = new HDrawLine(); break;
                        case "HDraw3PArc": subItem = new HDraw3PArc(); break;
                        case "HDrawPoint": subItem = new HDrawPoint(); break;
                        case "HDrawGroup": subItem = new HDrawGroup(); break;
                            // 未来若有新的图元类型，可在此扩展
                    }

                    if (subItem != null)
                    {
                        // 递归调用子对象的 Load 方法（对于 HDrawGroup 会再次进入本 mode==2 逻辑，实现嵌套解析）
                        OK<HDrawBase> subOK = subItem.Load(subData.ToArray(), 2);
                        if (subOK.IsSuccess)
                            HDrawBase.Group.Add(subOK.Value);
                    }

                    // 跳过已解析的整个子块，下次循环从 endIdx+1 开始
                    i = endIdx;
                }

                // 将构建好的对象放入返回值
                oK.Value = HDrawBase;
                oK.IsSuccess = true;
            }
            // ── mode == 3：数据块归属验证（通过 GUID 匹配）─────────────
            else if (mode == 3)
            {
                // 验证数据块是否属于当前对象（用于更新或匹配）
                if (data == null || data.Length == 0 || !data[0].Contains("HDrawGroup"))
                {
                    return false;
                }

                // 从首行 "[HDrawGroup_Start]GUID" 中提取 GUID 并进行比对
                string buffer = HStringPath.GetStartsWithStringEnd(data[0], "[" + "HDrawGroup" + "_Start" + "]").Trim();
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
                stringBuilder.Append("HDrawGroup");
                stringBuilder.Append("_");
                stringBuilder.Append("Start");
                stringBuilder.Append("]");
                stringBuilder.Append(GuidCode);
                stringBuilder.Append("\r\n");

                stringBuilder.Append("GUID=" + GuidCode + "\r\n");
                stringBuilder.Append("Layer=" + Layer.ToString().Trim('(').Trim(')') + "\r\n");

                // 修复：使用 o.IsSuccess 判断，并追加 o.Value
                foreach (var item in Group.GetList(true))
                {
                    OK<string> o = item.Save(2);
                    if (o.IsSuccess)                     // ← 检查子图元的保存结果
                    {
                        stringBuilder.Append(o.Value);   // ← 追加子图元的内容
                    }
                }

                stringBuilder.Append("[");
                stringBuilder.Append("HDrawGroup");
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
        /// <summary>hPoint3Ds 字段。</summary>
        private HList<HPoint3D> hPoint3Ds = new HList<HPoint3D>();
        /// <summary>Move 方法。</summary>
        public override OK Move(HPoint3D[] hPoint3D, HInt mode)
        {
            // 用 FromMessage 构造 OK（string 不能再隐式转 OK）
            OK ok = OK.FromMessage(HTranslation.GetContent("初始化{0}", mode.Value.ToString()));
            List<HDrawBase> group = Group.GetList(true);
            foreach (var item in group)
            {
                ok = item.Move(hPoint3D, mode);
                if (!ok)
                {
                    break;
                }
            }
            return ok;
        }
        /// <summary>获取 point。</summary>
        public override HPoint3D[] GetPoint(HInt mode, HDouble scale)
        {
            if (mode == 2)
            {
                HRectangle hRectangle = GetRect(mode.Value);
                return new HPoint3D[] { new HPoint3D(hRectangle.BottomLeft, 0), new HPoint3D(hRectangle.BottomRight, 0), new HPoint3D(hRectangle.TopLeft, 0), new HPoint3D(hRectangle.TopRight, 0) };
            }
            List<HDrawBase> group = Group.GetList(true);
            List<HPoint3D> points = new List<HPoint3D>();
            foreach (var item in group)
            {
                points.AddRange(item.GetPoint(mode, scale));
            }
            return points.ToArray();
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint worldPoint, HDouble scaleHeight)
        {
            HRectangle hRectangle = GetRect(1);
            return hRectangle.ContainPointLineRect(new HPoint[] { worldPoint });

            //List<HDrawBase> group = Group.GetList(true);
            //foreach (var item in group)
            //{
            //    if (item.HitTest(worldPoint, scaleHeight))
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }
        public override HInt Layer
        {
            get
            { 
            return base.Layer;
            }
            set
            {
                base.Layer = value;
                for (int i = 0; i < Group.Count; i++)
                {
                    Group[i].Layer= value;
                }
            }
        }
        /// <summary>清空。</summary>
        public override void Clear()
        {
            this.Name = this.ShowIndexName = this.GuidCode = string.Empty;
            for (int i = 0; i < group.Count; i++)
            {
                group[i].Clear();
                group[i] = null;
            }
            group.Clear();
            group = null;
        }
        /// <summary>克隆。</summary>
        public override HDrawBase Clone(int mode=0)
        {
            HList<HDrawBase> bases = new HList<HDrawBase>();
            foreach (var item in Group)
            {
                bases.Add(item.Clone());
            }
            HDrawGroup HDrawGroup = new HDrawGroup();
            HDrawGroup.Group = bases;
            HDrawGroup.Layer = Layer;
            HDrawGroup.Index = Index;
            if (mode==0)
            {
                HDrawGroup.GuidCode = GuidCode;
            }
          
            HDrawGroup.LineWidth = LineWidth;
            HDrawGroup.Color = Color;
            HDrawGroup.IsShowRect = IsShowRect;
            HDrawGroup.Name = Name;
            HDrawGroup.PenMode = PenMode;
            HDrawGroup.ShowIndexName = ShowIndexName;
            return HDrawGroup;
        }
    }
}