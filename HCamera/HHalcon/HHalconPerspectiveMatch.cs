using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>透视（平面标定可变形）匹配命中结果。</summary>
    public struct HPerspectiveMatchResult
    {
        /// <summary>
        /// 模板参考平面相对相机的 3D 位姿 [Tx,Ty,Tz,Rx,Ry,Rz,Order]（平移与标定单位一致，通常毫米）。
        /// </summary>
        public double[] Pose;
        /// <summary>位姿协方差（6 元素：3 平移 + 3 旋转的不确定度，顺序与 pose 对应）。</summary>
        public double[] CovPose;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
    }

    /// <summary>未标定平面透视匹配命中结果（输出图像平面的 3×3 投影单应矩阵）。</summary>
    public struct HUncalibPerspectiveResult
    {
        /// <summary>
        /// 模板→命中位置的 3×3 平面投影单应矩阵（行优先 9 元素，HALCON hom_mat2d 投影约定，
        /// 可直接给 projective_trans_contour_xld/projective_trans_image 使用）。
        /// </summary>
        public double[] HomMat2D;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 透视可变形匹配完整封装，含两种标定形态：
    /// <list type="bullet">
    /// <item><b>已标定</b>（create/find_planar_calib_deformable_model，<see cref="Create"/>/<see cref="Find"/>）：
    /// 需相机内参+参考位姿，查找直接输出目标平面相对相机的六自由度 3D 位姿与协方差，用于机器人引导。</item>
    /// <item><b>未标定</b>（create/find_planar_uncalib_deformable_model，<see cref="CreateUncalib"/>/<see cref="FindUncalib"/>）：
    /// 无需标定，输出每个实例的 3×3 单应矩阵，可做透视矫正、定位与对位，给不出物理尺度的 3D 位姿。</item>
    /// </list>
    /// 目标为平面物体但成像有明显透视梯形变形（相机光轴不垂直于目标平面、倾斜拍摄），
    /// 普通形状/缩放匹配的仿射模型无法覆盖。相机内参由 <see cref="HHalconCalibration"/> 标定得到。
    /// 两种模式的模板存读盘/销毁共用 write/read/clear_deformable_model。
    /// </summary>
    public class HHalconPerspectiveMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>透视可变形模板句柄。</summary>
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
        /// 创建平面标定可变形模板。
        /// </summary>
        /// <param name="template">模板图（域内即特征，模板平面须与 referencePose 描述的平面对应）。</param>
        /// <param name="camParam">相机内参（标定输出的 CameraParam）。</param>
        /// <param name="referencePose">模板平面相对相机的参考位姿（7 元素 pose，标定板位姿即可）。</param>
        /// <param name="numLevels">金字塔层数："auto"（默认）或层数。</param>
        /// <param name="angleStart">搜索起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">搜索角度跨度（弧度，默认 360°）。</param>
        /// <param name="angleStep">角度步长（"auto"/0）。</param>
        /// <param name="scaleRMin">行方向最小缩放（1.0=不缩）。</param>
        /// <param name="scaleRMax">行方向最大缩放。</param>
        /// <param name="scaleRStep">行缩放步长（"auto"/0）。</param>
        /// <param name="scaleCMin">列方向最小缩放。</param>
        /// <param name="scaleCMax">列方向最大缩放。</param>
        /// <param name="scaleCStep">列缩放步长（"auto"/0）。</param>
        /// <param name="optimization">"auto"/"none"/"point_reduction_*"。</param>
        /// <param name="metric">"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"。</param>
        /// <param name="contrast">"auto" 或对比度数值（可 [低,高,最小尺寸]）。</param>
        /// <param name="minContrast">查找最小对比度（"auto" 或数值）。</param>
        /// <param name="paramNames">附加 gen 参数名（可空），如 "distance_min"/"distance_max"/"visibility_min"。</param>
        /// <param name="paramValues">附加参数值（可空）。</param>
        public bool Create(HObject template, HTuple camParam, HTuple referencePose,
            object numLevels = null,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2, object angleStep = null,
            double scaleRMin = 1.0, double scaleRMax = 1.0, object scaleRStep = null,
            double scaleCMin = 1.0, double scaleCMax = 1.0, object scaleCStep = null,
            string optimization = "none", string metric = "use_polarity",
            object contrast = null, object minContrast = null,
            string[] paramNames = null, object[] paramValues = null)
        {
            Clear();
            if (template == null || camParam == null || referencePose == null) return false;
            return SafeRun(() =>
            {
                HTuple vals = new HTuple();
                if (paramValues != null)
                {
                    double[] d = new double[paramValues.Length];
                    bool allNumber = true;
                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        try { d[i] = Convert.ToDouble(paramValues[i], System.Globalization.CultureInfo.InvariantCulture); }
                        catch { allNumber = false; }
                    }
                    vals = allNumber ? new HTuple(d) : HHalconEnv.ToTuple(paramValues);
                }
                HTuple m;
                HOperatorSet.CreatePlanarCalibDeformableModel(template, camParam, referencePose,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    HHalconEnv.ToTuple(angleStep ?? "auto"),
                    scaleRMin, scaleRMax, HHalconEnv.ToTuple(scaleRStep ?? "auto"),
                    scaleCMin, scaleCMax, HHalconEnv.ToTuple(scaleCStep ?? "auto"),
                    optimization ?? "none", metric ?? "use_polarity",
                    HHalconEnv.ToTuple(contrast ?? "auto"),
                    HHalconEnv.ToTuple(minContrast ?? "auto"),
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    vals, out m);
                Model = m;
            });
        }

        /// <summary>
        /// 取模板参考轮廓（get_deformable_model_contours），调用方 Dispose。
        /// </summary>
        public HObject GetModelContours(int level = 1)
        {
            if (!IsCreated) return null;
            return SafeRun(delegate
            {
                HObject contours;
                HOperatorSet.GetDeformableModelContours(out contours, Model, level);
                return contours;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 查找平面目标（find_planar_calib_deformable_model），输出每个实例的 3D 位姿与得分。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="angleStart">本次搜索起始角。</param>
        /// <param name="angleExtent">本次搜索角度跨度。</param>
        /// <param name="scaleRMin/scaleRMax/scaleCMin/scaleCMax">本次缩放范围。</param>
        /// <param name="minScore">最低得分。</param>
        /// <param name="numMatches">最多实例数（0=全部）。</param>
        /// <param name="maxOverlap">最大重叠度。</param>
        /// <param name="numLevels">金字塔层数（0=建模板设置）。</param>
        /// <param name="greediness">贪婪度 0~1。</param>
        /// <param name="paramNames">附加 gen 参数（可空），如 "visibility_min"、"distance_min"。</param>
        /// <param name="paramValues">附加参数值（可空）。</param>
        public HPerspectiveMatchResult[] Find(HObject image,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double scaleRMin = 1.0, double scaleRMax = 1.0,
            double scaleCMin = 1.0, double scaleCMax = 1.0,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            int numLevels = 0, double greediness = 0.9,
            string[] paramNames = null, object[] paramValues = null)
        {
            if (!IsCreated || image == null) return new HPerspectiveMatchResult[0];
            HPerspectiveMatchResult[] res = new HPerspectiveMatchResult[0];
            SafeRun(() =>
            {
                HTuple vals = new HTuple();
                if (paramValues != null)
                {
                    double[] d = new double[paramValues.Length];
                    bool allNumber = true;
                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        try { d[i] = Convert.ToDouble(paramValues[i], System.Globalization.CultureInfo.InvariantCulture); }
                        catch { allNumber = false; }
                    }
                    vals = allNumber ? new HTuple(d) : HHalconEnv.ToTuple(paramValues);
                }
                HTuple poses, covs, scores;
                HOperatorSet.FindPlanarCalibDeformableModel(image, Model,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    scaleRMin, scaleRMax, scaleCMin, scaleCMax,
                    minScore, numMatches, maxOverlap, numLevels, greediness,
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    vals, out poses, out covs, out scores);
                double[] pa = poses.DArr, cv = covs.DArr, sa = scores.DArr;
                res = new HPerspectiveMatchResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                {
                    double[] pose = new double[7];
                    Array.Copy(pa, i * 7, pose, 0, Math.Min(7, pa.Length - i * 7));
                    double[] cov = new double[6];
                    Array.Copy(cv, i * 6, cov, 0, Math.Min(6, cv.Length - i * 6));
                    res[i] = new HPerspectiveMatchResult { Pose = pose, CovPose = cov, Score = sa[i] };
                }
            });
            return res;
        }

        #endregion

        #region ==================== 未标定平面透视（单应矩阵） ====================

        /// <summary>
        /// 创建<b>未标定</b>平面透视可变形模板（create_planar_uncalib_deformable_model）。
        /// 无需相机内参与参考位姿，参数与已标定版本基本一致（去掉 camParam/referencePose）；
        /// 建模板后用 <see cref="FindUncalib"/> 查找，得到 3×3 单应矩阵。
        /// </summary>
        public bool CreateUncalib(HObject template,
            object numLevels = null,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2, object angleStep = null,
            double scaleRMin = 1.0, double scaleRMax = 1.0, object scaleRStep = null,
            double scaleCMin = 1.0, double scaleCMax = 1.0, object scaleCStep = null,
            string optimization = "none", string metric = "use_polarity",
            object contrast = null, object minContrast = null,
            string[] paramNames = null, object[] paramValues = null)
        {
            Clear();
            if (template == null) return false;
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreatePlanarUncalibDeformableModel(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    HHalconEnv.ToTuple(angleStep ?? "auto"),
                    scaleRMin, scaleRMax, HHalconEnv.ToTuple(scaleRStep ?? "auto"),
                    scaleCMin, scaleCMax, HHalconEnv.ToTuple(scaleCStep ?? "auto"),
                    optimization ?? "none", metric ?? "use_polarity",
                    HHalconEnv.ToTuple(contrast ?? "auto"),
                    HHalconEnv.ToTuple(minContrast ?? "auto"),
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    BuildGenValues(paramValues), out m);
                Model = m;
            });
        }

        /// <summary>
        /// 未标定模式查找（find_planar_uncalib_deformable_model），
        /// 输出每个命中实例的 3×3 单应矩阵与得分。
        /// </summary>
        public HUncalibPerspectiveResult[] FindUncalib(HObject image,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double scaleRMin = 1.0, double scaleRMax = 1.0,
            double scaleCMin = 1.0, double scaleCMax = 1.0,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            int numLevels = 0, double greediness = 0.9,
            string[] paramNames = null, object[] paramValues = null)
        {
            if (!IsCreated || image == null) return new HUncalibPerspectiveResult[0];
            HUncalibPerspectiveResult[] res = new HUncalibPerspectiveResult[0];
            SafeRun(() =>
            {
                HTuple homs, scores;
                HOperatorSet.FindPlanarUncalibDeformableModel(image, Model,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    scaleRMin, scaleRMax, scaleCMin, scaleCMax,
                    minScore, numMatches, maxOverlap, numLevels, greediness,
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    BuildGenValues(paramValues), out homs, out scores);
                double[] ha = homs.DArr, sa = scores.DArr;
                res = new HUncalibPerspectiveResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                {
                    double[] h = new double[9];
                    Array.Copy(ha, i * 9, h, 0, Math.Min(9, ha.Length - i * 9));
                    res[i] = new HUncalibPerspectiveResult { HomMat2D = h, Score = sa[i] };
                }
            });
            return res;
        }

        /// <summary>
        /// 把模板参考轮廓按命中单应矩阵投影到当前图（projective_trans_contour_xld），
        /// 用于叠加显示命中的透视梯形轮廓；返回投影轮廓（调用方 Dispose）。
        /// 典型用法：<see cref="GetModelContours"/> 取参考轮廓 → 本方法按
        /// <see cref="HUncalibPerspectiveResult.HomMat2D"/> 投影。
        /// </summary>
        public static HObject ProjectContours(HObject contours, double[] homMat2D)
        {
            if (contours == null || homMat2D == null || homMat2D.Length < 9) return null;
            try
            {
                HObject projected;
                HOperatorSet.ProjectiveTransContourXld(contours, out projected, new HTuple(homMat2D));
                return projected;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        /// <summary>把 gen 参数值数组转换为 HTuple：全部数值走 double，否则按字符串/透传处理。</summary>
        private static HTuple BuildGenValues(object[] paramValues)
        {
            if (paramValues == null) return new HTuple();
            double[] d = new double[paramValues.Length];
            bool allNumber = true;
            for (int i = 0; i < paramValues.Length; i++)
            {
                try { d[i] = Convert.ToDouble(paramValues[i], System.Globalization.CultureInfo.InvariantCulture); }
                catch { allNumber = false; }
            }
            return allNumber ? new HTuple(d) : HHalconEnv.ToTuple(paramValues);
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>模板存盘（write_deformable_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteDeformableModel(Model, filePath));
        }

        /// <summary>读入模板（read_deformable_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadDeformableModel(filePath, out m);
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
