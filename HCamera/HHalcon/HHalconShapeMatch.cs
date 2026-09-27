using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>形状模板匹配命中结果。</summary>
    public struct HShapeMatchResult
    {
        /// <summary>模板参考点（默认模板重心）所在行。</summary>
        public double Row;
        /// <summary>模板参考点所在列。</summary>
        public double Column;
        /// <summary>旋转角（弧度）。</summary>
        public double Angle;
        /// <summary>匹配得分（0~1，越大越可信）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 基于形状的模板匹配（Shape-Based Matching）完整封装：
    /// create_shape_model 建模板（支持从 ROI 建模板）、find_shape_model 多目标带角度查找、
    /// get_shape_model_contours 取模板轮廓、set_shape_model_param 参数调整、模板存盘/读盘，
    /// 并提供结果轮廓按命中位姿做仿射变换的可视化辅助。
    /// 句柄由实例持有，Dispose 时 clear_shape_model。可变形（局部形变）匹配见
    /// <see cref="HHalconDeformableMatch"/>，3D/表面匹配见 <see cref="HHalconSurfaceMatch"/>。
    /// </summary>
    public class HHalconShapeMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>形状模板句柄。</summary>
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
        /// 用已裁剪好的模板图（域内即模板特征）创建形状模板（create_shape_model）。
        /// </summary>
        /// <param name="template">模板图（建议先用 ROI reduce_domain）。</param>
        /// <param name="numLevels">金字塔层数："auto"（默认）或 1~10 整数。</param>
        /// <param name="angleStart">查找起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">查找角度范围（弧度，默认 360°）。</param>
        /// <param name="angleStep">角度步长（弧度，"auto"/0 自动）。</param>
        /// <param name="optimization">优化："auto"/"none"/"point_reduction_low/medium/high"。</param>
        /// <param name="metric">极性度量："use_polarity"（严格极性，最快）/"ignore_global_polarity"/"ignore_local_polarity"。</param>
        /// <param name="contrast">模板特征对比度："auto" 或数值（可给 [低,高,最小尺寸] 数组）。</param>
        /// <param name="minContrast">查找时最小对比度（常用 "auto" 或 5~30）。</param>
        public bool Create(HObject template,
            object numLevels, double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            object angleStep = null, string optimization = "auto",
            string metric = "use_polarity", object contrast = null, object minContrast = null)
        {
            Clear();
            if (template == null) return false;
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateShapeModel(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent,
                    HHalconEnv.ToTuple(angleStep ?? "auto"),
                    optimization ?? "auto", metric ?? "use_polarity",
                    HHalconEnv.ToTuple(contrast ?? "auto"),
                    HHalconEnv.ToTuple(minContrast ?? "auto"),
                    out m);
                Model = m;
            });
        }

        /// <summary>
        /// 从样图 + ROI 区域建模板：自动 reduce_domain 裁剪并居中（crop 后重心更贴近实际目标中心）。
        /// </summary>
        /// <param name="sample">样图（包含一个目标，灰度）。</param>
        /// <param name="roi">目标所在区域（矩形/圆等）。</param>
        /// <param name="angleStart">查找起始角。</param>
        /// <param name="angleExtent">查找角度范围。</param>
        /// <param name="minContrast">最小对比度。</param>
        public bool CreateFromRoi(HObject sample, HObject roi,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2, object minContrast = null)
        {
            Clear();
            if (sample == null || roi == null) return false;
            HObject reduced = null;
            try
            {
                HOperatorSet.ReduceDomain(sample, roi, out reduced);
                return Create(reduced, "auto", angleStart, angleExtent, null,
                    "auto", "use_polarity", "auto", minContrast ?? "auto");
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
        /// 在图中查找模板（find_shape_model）。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="minScore">最低得分（0~1，常用 0.5~0.8）。</param>
        /// <param name="numMatches">最多返回个数（0=全部）。</param>
        /// <param name="maxOverlap">允许的最大重叠度（0~1，常用 0.5）。</param>
        /// <param name="subPixel">亚像素模式："least_squares"（默认，亚像素+角度）/"least_squares_high" 等。</param>
        /// <param name="numLevels">金字塔层数（0=建模板时的设置）。</param>
        /// <param name="greediness">贪婪度（0~0.99，越大越快但可能漏检，常用 0.8）。</param>
        /// <param name="angleStart">本次查找起始角（默认用建模板范围起点 -π）。</param>
        /// <param name="angleExtent">本次查找角度范围（默认 2π）。</param>
        public HShapeMatchResult[] Find(HObject image,
            double minScore = 0.5, int numMatches = 0, double maxOverlap = 0.5,
            string subPixel = "least_squares", int numLevels = 0, double greediness = 0.8,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2)
        {
            if (!IsCreated || image == null) return new HShapeMatchResult[0];
            HShapeMatchResult[] res = new HShapeMatchResult[0];
            SafeRun(() =>
            {
                HTuple rows, cols, angles, scores;
                HOperatorSet.FindShapeModel(image, Model, angleStart, angleExtent,
                    minScore, numMatches, maxOverlap, subPixel ?? "least_squares",
                    numLevels, greediness, out rows, out cols, out angles, out scores);
                double[] ra = rows.DArr, ca = cols.DArr, aa = angles.DArr, sa = scores.DArr;
                res = new HShapeMatchResult[ra.Length];
                for (int i = 0; i < ra.Length; i++)
                    res[i] = new HShapeMatchResult { Row = ra[i], Column = ca[i], Angle = aa[i], Score = sa[i] };
            });
            return res;
        }

        /// <summary>取模板轮廓（get_shape_model_contours，金字塔第 1 层即原图分辨率）；返回对象由调用方释放。</summary>
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

        /// <summary>
        /// 生成模板轮廓在参考位姿（0,0,0）下的仿射矩阵（vector_angle_to_rigid），
        /// 配合命中结果即可把模板轮廓变换到目标位置用于显示/对位。
        /// </summary>
        /// <param name="templateRow">模板参考点行（建模板图像域中心/重心）。</param>
        /// <param name="templateCol">模板参考点列。</param>
        /// <param name="targetRow">目标点行。</param>
        /// <param name="targetCol">目标点列。</param>
        /// <param name="targetAngle">目标角度。</param>
        public static HTuple GetPoseMatrix(double templateRow, double templateCol,
            double targetRow, double targetCol, double targetAngle)
        {
            HTuple mat;
            HOperatorSet.VectorAngleToRigid(templateRow, templateCol, 0,
                targetRow, targetCol, targetAngle, out mat);
            return mat;
        }

        /// <summary>
        /// 便捷可视化：把模板轮廓按命中结果变换后的 XLD 全部生成到一个 XLD 对象中（affine_trans_contour_xld）。
        /// </summary>
        /// <param name="templateRow">模板参考点行。</param>
        /// <param name="templateCol">模板参考点列。</param>
        /// <param name="results">命中结果数组。</param>
        public HObject GetHitContours(double templateRow, double templateCol, HShapeMatchResult[] results)
        {
            HObject modelContours = GetModelContours(1);
            if (modelContours == null || results == null || results.Length == 0) return modelContours;
            HObject all = null;
            try
            {
                foreach (HShapeMatchResult r in results)
                {
                    HTuple mat = GetPoseMatrix(templateRow, templateCol, r.Row, r.Column, r.Angle);
                    HObject moved;
                    HOperatorSet.AffineTransContourXld(modelContours, out moved, mat);
                    HObject merged;
                    if (all == null) merged = moved;
                    else
                    {
                        HOperatorSet.ConcatObj(all, moved, out merged);
                        moved.Dispose();
                    }
                    if (all != null) all.Dispose();
                    all = merged;
                }
                modelContours.Dispose();
                return all;
            }
            catch (HalconException)
            {
                if (modelContours != null) modelContours.Dispose();
                if (all != null) all.Dispose();
                return null;
            }
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>模板存盘（write_shape_model，.shm）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteShapeModel(Model, filePath));
        }

        /// <summary>从文件读模板（read_shape_model）。</summary>
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

        /// <summary>销毁模板（clear_shape_model）。</summary>
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
