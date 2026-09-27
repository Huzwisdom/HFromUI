using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace HFromUI.HControl.Coordinate
{
    /// <summary>
    /// G 代码双向同步管理器（无分隔符版）。
    /// 每个图形固定占据 3 行（Prefix、GCode、Suffix），即使内容为空也会保留空行。
    /// 头部和尾部各占一行。
    /// </summary>
    public class HDrawGCode
    {
        /// <summary>生成的完整 G 代码文本</summary>
        public StringBuilder GCode = new StringBuilder();

        /// <summary>当前管理的 iHGode 图形列表</summary>
        public HList<iHGode> iHGodes { set; get; } = new HList<iHGode>();

        /// <summary>所有图形的 GCodeText 组件（仅用于外部查询，不影响核心逻辑）</summary>
        public HList<GCodeText> GCodeTexts { set; get; } = new HList<GCodeText>();

        /// <summary>固定的头部 GCode（可空）</summary>
        public GCodeText HeaderGCode { get; set; } = new GCodeText();

        /// <summary>固定的尾部 GCode（可空）</summary>
        public GCodeText FooterGCode { get; set; } = new GCodeText();

        private HList<HDrawBase> _originalShapes;

        // ---------- 原始方法（保持不变） ----------
        public List<iHGode> FlattenToiHGode(HList<HDrawBase> shapes)
        {
            var result = new List<iHGode>();
            if (shapes == null) return result;
            foreach (var shape in shapes)
            {
                if (shape is HDrawGroup group)
                    result.AddRange(FlattenToiHGode(group.Group));
                else if (shape is iHGode line)
                    result.Add(line);
            }
            return result;
        }

        /// <summary>FlattenToCodeText 方法。</summary>
        public HList<GCodeText> FlattenToCodeText(List<iHGode> iHGodes)
        {
            var result = new HList<GCodeText>();
            if (iHGodes == null) return result;
            foreach (var item in iHGodes)
            {
                result.Add(item.Prefix);
                result.Add(item.GCode);
                result.Add(item.Suffix);
            }
            return result;
        }

        // ---------- 双向同步核心 ----------
        /// <summary>
        /// 从图形列表加载，生成初始 GCode。
        /// </summary>
        public void LoadFromShapes(HList<HDrawBase> shapes)
        {
            if (shapes == null) throw new ArgumentNullException(nameof(shapes));
            _originalShapes = shapes;
            var flat = FlattenToiHGode(shapes);
            iHGodes = new HList<iHGode>();
            foreach (var item in flat) iHGodes.Add(item);
            GCodeTexts = FlattenToCodeText(flat);
            RebuildGCode();
        }

        /// <summary>
        /// 根据完整的 G 代码文本（无分隔符，每图形固定 3 行）反向更新所有图形及头尾。
        /// 行数不足或图形过多时自动补空。
        /// </summary>
        public void UpdateFromGCode(string gcode)
        {
            var lines = gcode?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None) ?? new string[0];
            int lineIdx = 0;

            // 头部（第 0 行）
            HeaderGCode = new GCodeText(lineIdx < lines.Length ? lines[lineIdx] : "");
            lineIdx++;

            // 每个图形分配 3 行
            int shapeIdx = 0;
            while (shapeIdx < iHGodes.Count)
            {
                var entity = iHGodes[shapeIdx];
                if (entity != null)
                {
                    entity.Prefix = new GCodeText(lineIdx < lines.Length ? lines[lineIdx] : "");
                    entity.GCode = new GCodeText(lineIdx + 1 < lines.Length ? lines[lineIdx + 1] : "");
                    entity.Suffix = new GCodeText(lineIdx + 2 < lines.Length ? lines[lineIdx + 2] : "");
                }
                lineIdx += 3;
                shapeIdx++;
            }

            // 尾部（剩余的最后一行）
            FooterGCode = new GCodeText(lineIdx < lines.Length ? lines[lineIdx] : "");

            // 保持内部 GCode 与更新后的状态一致
            RebuildGCode();
        }

        /// <summary>
        /// 图形列表变化后重新加载。
        /// </summary>
        public void RefreshFromShapes()
        {
            if (_originalShapes != null) LoadFromShapes(_originalShapes);
        }

        /// <summary>
        /// 图形内容被外部修改后，手动调用以刷新 GCode StringBuilder。
        /// </summary>
        public void NotifyContentChanged()
        {
            RebuildGCode();
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            GCode.Clear();
            iHGodes.Clear();
            GCodeTexts.Clear();
            HeaderGCode = new GCodeText();
            FooterGCode = new GCodeText();
            _originalShapes = null;
        }

        // ---------- 内部构建 ----------
        private void RebuildGCode()
        {
            GCode.Clear();
            // 头部
            GCode.AppendLine(HeaderGCode?.Text ?? "");
            // 每个图形三行
            foreach (var item in iHGodes)
            {
                GCode.AppendLine(item?.Prefix?.Text ?? "");
                GCode.AppendLine(item?.GCode?.Text ?? "");
                GCode.AppendLine(item?.Suffix?.Text ?? "");
            }
            // 尾部
            GCode.AppendLine(FooterGCode?.Text ?? "");
        }
    }
}
