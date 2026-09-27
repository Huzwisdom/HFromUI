using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>带各向同性缩放的形状模板匹配命中结果。</summary>
    public struct HScaledShapeResult
    {
        /// <summary>模板参考点所在行。</summary>
        public double Row;
        /// <summary>模板参考点所在列。</summary>
        public double Column;
        /// <summary>旋转角（弧度）。</summary>
        public double Angle;
        /// <summary>各向同性缩放系数（1.0=原始大小）。</summary>
        public double Scale;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 缩放形状匹配（Scaled Shape-Based Matching，create_scaled_shape_model /
    /// find_scaled_shape_model）完整封装：在普通形状匹配基础上支持各向同性（等比例）缩放搜索，
    /// 适合目标距离相机远近不固定、同型号不同尺寸的定位；需要 X/Y 不等比缩放或透视梯形变形时
    /// 用 <see cref="HHalconGenericShapeMatch"/> 或 <see cref="HHalconPerspectiveMatch"/>。
    /// 模板轮廓、存读盘与销毁复用形状模板家族算子（get_shape_model_contours /
    /// write_shape_model / read_shape_model / clear_shape_model），但句柄不可与普通
    /// <see cref="HHalconShapeMatch"/> 的句柄混用。
    /// </summary>
    public class HHalconScaledShapeMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>缩放形状模板句柄。</summary>
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
        /// 创建缩放形状模板（create_scaled_shape_model）。
        /// </summary>
        /// <param name="template">模板图（建议先 reduce_domain）。</param>
        /// <param name="numLevels">金字塔层数："auto"（默认）或 1~10。</param>
        /// <param name="angleStart">搜索起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">搜索角度跨度（弧度，默认 360°）。</param>
        /// <param name="angleStep">角度步长（"auto"/0 自动）。</param>
        /// <param name="scaleMin">最小缩放系数（默认 0.9）。</param>
        /// <param name="scaleMax">最大缩放系数（默认 1.1）。</param>
        /// <param name="scaleStep">缩放步长（"auto"/0 自动，范围越宽建模板越久）。</param>
        /// <param name="optimization">"auto"/"none"/"point_reduction_low|medium|high"。</param>
        /// <param name="metric">"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"。</param>
        /// <param name="contrast">特征对比度："auto" 或数值（可 [低,高,最小尺寸]）。</param>
        /// <param name="minContrast">查找时最小对比度（"auto" 或 5~30）。</param>
        public bool Create(HObject template, object numLevels = null,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2, object angleStep = null,
            double scaleMin = 0.9, double scaleMax = 1.1, object scaleStep = null,
            string optimization = "auto", string metric = "use_polarity",
            object contrast = null, object minContrast = null)
        {
            Clear();
            if (template == null) return false;
            if (scaleMax < scaleMin)
            {
                double t = scaleMin;
                scaleMin = scaleMax;
                scaleMax = t;
            }
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateScaledShapeModel(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent,
                    HHalconEnv.ToTuple(angleStep ?? "auto"),
                    scaleMin, scaleMax,
                    HHalconEnv.ToTuple(scaleStep ?? "auto"),
                    optimization ?? "auto", metric ?? "use_polarity",
                    HHalconEnv.ToTuple(contrast ?? "auto"),
                    HHalconEnv.ToTuple(minContrast ?? "auto"),
                    out m);
                Model = m;
            });
        }

        /// <summary>设置模板参数（set_shape_model_param），如 "timeout"、"min_contrast"。</summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetShapeModelParam(Model, name ?? string.Empty,
                    HHalconEnv.ToTuple(value)));
        }

        /// <summary>
        /// 取模板轮廓（get_shape_model_contours），调用方 Dispose。
        /// </summary>
        /// <param name="level">金字塔层（1=原图分辨率）。</param>
        public HObject GetModelContours(int level = 1)
        {
            if (!IsCreated) return null;
            return SafeRun(delegate
            {
                HObject contours;
                HOperatorSet.GetShapeModelContours(out contours, Model, level);
                return contours;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 查找缩放模板（find_scaled_shape_model）。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="scaleMin">本次最小缩放（默认 0=用 0~1000 全范围）。</param>
        /// <param name="scaleMax">本次最大缩放。</param>
        /// <param name="minScore">最低得分。</param>
        /// <param name="numMatches">最多命中数（0=全部）。</param>
        /// <param name="maxOverlap">最大重叠度。</param>
        /// <param name="subPixel">亚像素："least_squares"（默认高精度）/"none" 等，可附加角度/缩放项。</param>
        /// <param name="numLevels">金字塔层数（0=建模板设置）。</param>
        /// <param name="greediness">贪婪度 0~1（越大越快，可能漏检）。</param>
        public HScaledShapeResult[] Find(HObject image,
            double scaleMin = 0, double scaleMax = 1000,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            string subPixel = "least_squares", int numLevels = 0, double greediness = 0.9,
            double? angleStart = null, double? angleExtent = null)
        {
            if (!IsCreated) return new HScaledShapeResult[0];
            HScaledShapeResult[] res = new HScaledShapeResult[0];
            double a0 = angleStart ?? -Math.PI;
            double ae = angleExtent ?? Math.PI * 2;
            SafeRun(() =>
            {
                HTuple rows, cols, angles, scales, scores;
                HOperatorSet.FindScaledShapeModel(image, Model,
                    a0, ae, scaleMin, scaleMax,
                    minScore, numMatches, maxOverlap,
                    subPixel ?? "least_squares", numLevels, greediness,
                    out rows, out cols, out angles, out scales, out scores);
                double[] ra = rows.DArr, ca = cols.DArr, aa = angles.DArr;
                double[] ka = scales.DArr, sa = scores.DArr;
                res = new HScaledShapeResult[ra.Length];
                for (int i = 0; i < ra.Length; i++)
                    res[i] = new HScaledShapeResult
                    {
                        Row = ra[i],
                        Column = ca[i],
                        Angle = i < aa.Length ? aa[i] : 0,
                        Scale = i < ka.Length ? ka[i] : 1.0,
                        Score = i < sa.Length ? sa[i] : 0
                    };
            });
            return res;
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>模板存盘（write_shape_model，.shm）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteShapeModel(Model, filePath));
        }

        /// <summary>读入模板（read_shape_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadShapeModel(filePath, out m);
                Model = m;
            });
        }

        /// <summary>销毁模板。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearShapeModel(Model); } catch { }
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
