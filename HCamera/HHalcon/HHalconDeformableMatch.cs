using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>局部可变形匹配命中结果。</summary>
    public struct HDeformableMatchResult
    {
        /// <summary>命中位置行。</summary>
        public double Row;
        /// <summary>命中位置列。</summary>
        public double Column;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 局部可变形模板匹配（Local Deformable Matching）完整封装：
    /// create_local_deformable_model 允许目标存在旋转、各向同性/各向异性缩放与局部形变，
    /// find_local_deformable_model 返回矫正后图像（image_rectified）、形变矢量场（vector_field，
    /// 可用于量化变形量）与匹配位置的模板轮廓（deformed_contours）。
    /// 适合印刷缺陷对位、柔性材料、包装标签等目标有轻微形变的场景。
    /// 句柄由实例持有，Dispose 时 clear_deformable_model。
    /// </summary>
    public class HHalconDeformableMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>可变形模板句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>模板是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return Model != null && Model.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 建模板 ====================

        /// <summary>
        /// 创建局部可变形模板（create_local_deformable_model）。
        /// </summary>
        /// <param name="template">模板图（建议先 reduce_domain 到目标区域）。</param>
        /// <param name="numLevels">金字塔层数（"auto" 或整数）。</param>
        /// <param name="angleStart">起始角（弧度，默认 -π）。</param>
        /// <param name="angleExtent">角度范围（默认 2π）。</param>
        /// <param name="angleStep">角度步长（"auto"）。</param>
        /// <param name="scaleRMin">行方向最小缩放（1=原始尺寸）。</param>
        /// <param name="scaleRMax">行方向最大缩放。</param>
        /// <param name="scaleRStep">行方向缩放步长（"auto"/0 自动）。</param>
        /// <param name="scaleCMin">列方向最小缩放。</param>
        /// <param name="scaleCMax">列方向最大缩放。</param>
        /// <param name="scaleCStep">列方向缩放步长。</param>
        /// <param name="optimization">优化模式（"auto"/"none"）。</param>
        /// <param name="metric">极性度量（"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"）。</param>
        /// <param name="contrast">对比度参数（"auto" 或数值数组）。</param>
        /// <param name="minContrast">最小对比度（"auto" 或数值）。</param>
        public bool Create(HObject template,
            object numLevels, double angleStart = -Math.PI, double angleExtent = Math.PI * 2, object angleStep = null,
            double scaleRMin = 1.0, double scaleRMax = 1.0, object scaleRStep = null,
            double scaleCMin = 1.0, double scaleCMax = 1.0, object scaleCStep = null,
            string optimization = "none", string metric = "use_polarity",
            object contrast = null, object minContrast = null)
        {
            Clear();
            if (template == null) return false;
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateLocalDeformableModel(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent, HHalconEnv.ToTuple(angleStep ?? "auto"),
                    scaleRMin, scaleRMax, HHalconEnv.ToTuple(scaleRStep ?? "auto"),
                    scaleCMin, scaleCMax, HHalconEnv.ToTuple(scaleCStep ?? "auto"),
                    optimization ?? "none", metric ?? "use_polarity",
                    HHalconEnv.ToTuple(contrast ?? "auto"),
                    HHalconEnv.ToTuple(minContrast ?? "auto"),
                    new HTuple(), new HTuple(),
                    out m);
                Model = m;
            });
        }

        /// <summary>
        /// 从样图 + ROI 建可变形模板（自动 reduce_domain）。
        /// </summary>
        public bool CreateFromRoi(HObject sample, HObject roi,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double scaleMin = 1.0, double scaleMax = 1.0, object minContrast = null)
        {
            Clear();
            if (sample == null || roi == null) return false;
            HObject reduced = null;
            try
            {
                HOperatorSet.ReduceDomain(sample, roi, out reduced);
                return Create(reduced, "auto", angleStart, angleExtent, null,
                    scaleMin, scaleMax, null, scaleMin, scaleMax, null,
                    "none", "use_polarity", "auto", minContrast ?? "auto");
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return false;
            }
            finally
            {
                if (reduced != null) reduced.Dispose();
            }
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 在图中查找可变形模板（find_local_deformable_model）。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="results">输出命中位置/得分数组。</param>
        /// <param name="rectifiedImage">输出矫正回模板形状的图像（对位/缺陷比对用），可为 null 不取。</param>
        /// <param name="vectorField">输出形变矢量场（像素级偏移，量化变形量用），可为 null 不取。</param>
        /// <param name="deformedContours">输出目标位置处的模板轮廓（显示用），可为 null 不取。</param>
        /// <param name="minScore">最低得分。</param>
        /// <param name="numMatches">最多命中数（0=全部）。</param>
        /// <param name="maxOverlap">最大重叠度。</param>
        /// <param name="numLevels">金字塔层数（0=建模板设置）。</param>
        /// <param name="greediness">贪婪度（0~1）。</param>
        /// <param name="resultType">返回内容控制："image_rectified"/"vector_field"/"deformed_contours"
        /// 不需要的项传 "none"；默认三项都返回。</param>
        public bool Find(HObject image, out HDeformableMatchResult[] results,
            out HObject rectifiedImage, out HObject vectorField, out HObject deformedContours,
            double minScore = 0.5, int numMatches = 0, double maxOverlap = 0.5,
            int numLevels = 0, double greediness = 0.8, string resultType = "all")
        {
            results = new HDeformableMatchResult[0];
            rectifiedImage = vectorField = deformedContours = null;
            if (!IsCreated || image == null) return false;
            HDeformableMatchResult[] resultArr = results;
            HObject rect2 = null, field2 = null, contours2 = null;
            bool ok = SafeRun(() =>
            {
                HObject rect, field, contours;
                HTuple scores, rows, cols;
                HOperatorSet.FindLocalDeformableModel(image,
                    out rect, out field, out contours, Model,
                    -Math.PI, Math.PI * 2,
                    1.0, 1.0, 1.0, 1.0,
                    minScore, numMatches, maxOverlap, numLevels, greediness,
                    resultType ?? "all", new HTuple(), new HTuple(),
                    out scores, out rows, out cols);
                double[] sa = scores.DArr, ra = rows.DArr, ca = cols.DArr;
                resultArr = new HDeformableMatchResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                    resultArr[i] = new HDeformableMatchResult { Row = ra[i], Column = ca[i], Score = sa[i] };
                rect2 = rect;
                field2 = field;
                contours2 = contours;
            });
            results = resultArr;
            rectifiedImage = rect2;
            vectorField = field2;
            deformedContours = contours2;
            return ok;
        }

        /// <summary>
        /// 只取命中结果（矫正图/矢量场/轮廓按 'none' 跳过，速度更快）。
        /// </summary>
        public HDeformableMatchResult[] FindOnly(HObject image, double minScore = 0.5,
            int numMatches = 0, double maxOverlap = 0.5, double greediness = 0.8)
        {
            HObject r, f, c;
            HDeformableMatchResult[] res;
            Find(image, out res, out r, out f, out c, minScore, numMatches, maxOverlap,
                0, greediness, "none");
            if (r != null) r.Dispose();
            if (f != null) f.Dispose();
            if (c != null) c.Dispose();
            return res;
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>可变形模板存盘（write_deformable_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteDeformableModel(Model, filePath));
        }

        /// <summary>销毁可变形模板（clear_deformable_model）。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearDeformableModel(Model); } catch { }
                        Model = new HTuple();
                    }
                }
                catch { }
            }
        }

        /// <summary>释放模板句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
