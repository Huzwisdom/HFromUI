using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>双目矫正输出：两张重映射表、矫正后相机内参与相对位姿。</summary>
    public class HStereoRectification
    {
        /// <summary>左图重映射表。</summary>
        public HObject Map1;
        /// <summary>右图重映射表。</summary>
        public HObject Map2;
        /// <summary>矫正后左相机内参。</summary>
        public HTuple CamParamRect1;
        /// <summary>矫正后右相机内参。</summary>
        public HTuple CamParamRect2;
        /// <summary>矫正后右相机相对左相机位姿。</summary>
        public HTuple RelPoseRect;

        /// <summary>释放两张映射表。</summary>
        public void Dispose()
        {
            if (Map1 != null) { try { Map1.Dispose(); } catch { } Map1 = null; }
            if (Map2 != null) { try { Map2.Dispose(); } catch { } Map2 = null; }
        }
    }

    /// <summary>
    /// HALCON 双目立体视觉（Binocular Stereo）3D 重建完整封装：
    /// 用双目标定得到的内参/相对位姿创建相机装配与立体模型（create_camera_setup_model +
    /// create_stereo_model "binocular"），先 gen_binocular_rectification_map 生成矫正映射，
    /// binocular_disparity 在矫正图对上算视差图，再 reconstruct_surface_stereo /
    /// disparity_image_to_xyz 把视差还原成 3D 点云；disparity_to_distance 可由指定点视差求距离。
    /// 另提供 binocular_calibration 静态标定入口与 reconstruct_points_stereo 亚像素点重建。
    /// 模型由实例持有，Dispose 时 clear_stereo_model。
    /// </summary>
    public class HHalconStereo : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>立体模型句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>模型是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return Model != null && Model.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 创建/参数 ====================

        /// <summary>
        /// 创建双目立体模型。
        /// </summary>
        /// <param name="camParam1">左相机内参（calibrate_cameras 输出的 CameraParam）。</param>
        /// <param name="camParam2">右相机内参。</param>
        /// <param name="relPose">右相机相对左相机位姿（7 元素 pose）。</param>
        /// <param name="paramNames">立体模型参数名（可空），如 "rectif_interpolation"、"disparity_method"。</param>
        /// <param name="paramValues">参数值（可空）。</param>
        public bool Create(HTuple camParam1, HTuple camParam2, HTuple relPose,
            string[] paramNames = null, object[] paramValues = null)
        {
            Clear();
            return SafeRun(() =>
            {
                // 1. 两相机装配模型
                HTuple setup;
                HOperatorSet.CreateCameraSetupModel(2, out setup);
                try
                {
                    // 相机 0（左）：内参 + 单位位姿
                    HOperatorSet.SetCameraSetupParam(setup, 0, "params", camParam1);
                    HOperatorSet.SetCameraSetupParam(setup, 0, "pose", new HTuple(new double[] { 0, 0, 0, 0, 0, 0, 0 }));
                    // 相机 1（右）：内参 + 相对位姿
                    HOperatorSet.SetCameraSetupParam(setup, 1, "params", camParam2);
                    HOperatorSet.SetCameraSetupParam(setup, 1, "pose", relPose);

                    // 2. 立体模型
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
                    HOperatorSet.CreateStereoModel(setup, "binocular",
                        paramNames != null ? new HTuple(paramNames) : new HTuple(),
                        vals, out m);
                    Model = m;
                }
                finally
                {
                    try { HOperatorSet.ClearCameraSetupModel(setup); } catch { }
                }
            });
        }

        /// <summary>设置立体模型参数（set_stereo_model_param），如 "bounding_box"、
        /// "persistence"、"min_disparity"/"max_disparity"、"sub_disparity" 等。</summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetStereoModelParam(Model, name ?? string.Empty, HHalconEnv.ToTuple(value)));
        }

        /// <summary>读立体模型参数（get_stereo_model_param）。</summary>
        public HTuple GetParam(string name)
        {
            if (!IsCreated) return new HTuple();
            return SafeRun(delegate
            {
                HTuple v;
                HOperatorSet.GetStereoModelParam(Model, name ?? string.Empty, out v);
                return v;
            }, new HTuple());
        }

        /// <summary>
        /// 设置用于 surface 重建的图像对索引范围（set_stereo_model_image_pairs）。
        /// </summary>
        /// <param name="from">起始相机对索引（一般 0）。</param>
        /// <param name="to">结束相机对索引（双目一般 0）。</param>
        public bool SetImagePairs(int from, int to)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetStereoModelImagePairs(Model, from, to));
        }

        #endregion

        #region ==================== 矫正/视差/重建 ====================

        /// <summary>
        /// 生成双目矫正映射（gen_binocular_rectification_map）：同时返回矫正后内参与位姿，
        /// 后续视差计算与 XYZ 还原均需使用矫正后参数。
        /// </summary>
        /// <param name="camParam1">左相机内参。</param>
        /// <param name="camParam2">右相机内参。</param>
        /// <param name="relPose">右相对左位姿。</param>
        /// <param name="subSampling">抽稀因子（1=原分辨率，常用 1）。</param>
        /// <param name="mapType">映射类型："bilinear"（默认，平滑）/"nn"（最近邻，快）。</param>
        public static HStereoRectification GenerateRectification(
            HTuple camParam1, HTuple camParam2, HTuple relPose,
            int subSampling = 1, string mapType = "bilinear")
        {
            HStereoRectification r = new HStereoRectification();
            try
            {
                HObject map1, map2;
                HTuple c1, c2, p1, p2, rel;
                HOperatorSet.GenBinocularRectificationMap(out map1, out map2,
                    camParam1, camParam2, relPose,
                    subSampling, "geometric", mapType ?? "bilinear",
                    out c1, out c2, out p1, out p2, out rel);
                r.Map1 = map1;
                r.Map2 = map2;
                r.CamParamRect1 = c1;
                r.CamParamRect2 = c2;
                r.RelPoseRect = rel;
                return r;
            }
            catch (HalconException)
            {
                r.Dispose();
                return null;
            }
        }

        /// <summary>按映射表矫正图像（map_image），调用方 Dispose 返回图。</summary>
        public static HObject RectifyImage(HObject image, HObject map)
        {
            if (image == null || map == null) return null;
            try
            {
                HObject mapped;
                HOperatorSet.MapImage(image, map, out mapped);
                return mapped;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 计算视差图（binocular_disparity）：输入必须是矫正后的左右图。
        /// </summary>
        /// <param name="imageRect1">矫正后左图。</param>
        /// <param name="imageRect2">矫正后右图。</param>
        /// <param name="disparity">输出视差图（调用方 Dispose）。</param>
        /// <param name="score">匹配置信度图（调用方 Dispose）。</param>
        /// <param name="maskWidth">匹配窗口宽（奇数，默认 21）。</param>
        /// <param name="maskHeight">匹配窗口高（奇数，默认 21）。</param>
        /// <param name="minDisparity">最小视差。</param>
        /// <param name="maxDisparity">最大视差。</param>
        /// <param name="numLevels">金字塔层数（0~4，默认 1）。</param>
        /// <param name="scoreThresh">置信度阈值（0~100，默认 0 不过滤）。</param>
        /// <param name="filter">一致性过滤："none"/"left_right_check"。</param>
        /// <param name="subDisparity">亚像素："interpolation"（默认）/"none"。</param>
        public bool ComputeDisparity(HObject imageRect1, HObject imageRect2,
            out HObject disparity, out HObject score,
            int maskWidth = 21, int maskHeight = 21, int textureThresh = 0,
            double minDisparity = 0, double maxDisparity = 40, int numLevels = 1,
            double scoreThresh = 0, string filter = "none", string subDisparity = "interpolation")
        {
            disparity = null;
            score = null;
            if (imageRect1 == null || imageRect2 == null) return false;
            HObject disp = null, sco = null;
            bool ok = SafeRun(() =>
                HOperatorSet.BinocularDisparity(imageRect1, imageRect2,
                    out disp, out sco, "binocular",
                    maskWidth, maskHeight, textureThresh,
                    minDisparity, maxDisparity, numLevels,
                    scoreThresh, filter ?? "none", subDisparity ?? "interpolation"));
            disparity = disp;
            score = sco;
            return ok;
        }

        /// <summary>
        /// 由图像对整体重建 3D 表面（reconstruct_surface_stereo）。
        /// 输入为左右图组成的 HObject 通道容器（concat_obj 后的两图，须已矫正），
        /// 输出 object_model_3d 句柄（HTuple，用后 clear_object_model_3d）。
        /// </summary>
        public bool ReconstructSurface(HObject rectifiedImagePair, out HTuple objectModel3D)
        {
            objectModel3D = new HTuple();
            if (!IsCreated || rectifiedImagePair == null) return false;
            HTuple model = new HTuple();
            bool ok = SafeRun(() =>
                HOperatorSet.ReconstructSurfaceStereo(rectifiedImagePair, Model, out model));
            objectModel3D = model;
            return ok;
        }

        /// <summary>
        /// 视差图直接转 X/Y/Z 坐标图（disparity_image_to_xyz），需传入矫正后内参与位姿。
        /// </summary>
        public static bool DisparityToXyz(HObject disparity,
            HTuple camParamRect1, HTuple camParamRect2, HTuple relPoseRect,
            out HObject x, out HObject y, out HObject z)
        {
            x = y = z = null;
            try
            {
                HOperatorSet.DisparityImageToXyz(disparity, out x, out y, out z,
                    camParamRect1, camParamRect2, relPoseRect);
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>
        /// 指定点视差求距离（disparity_to_distance）。
        /// </summary>
        /// <param name="disparities">待求点的视差值数组。</param>
        public static double[] DisparityToDistance(double[] disparities,
            HTuple camParamRect1, HTuple camParamRect2, HTuple relPoseRect)
        {
            try
            {
                HTuple dist;
                HOperatorSet.DisparityToDistance(camParamRect1, camParamRect2, relPoseRect,
                    new HTuple(disparities), out dist);
                return dist.DArr;
            }
            catch (HalconException)
            {
                return new double[0];
            }
        }

        /// <summary>
        /// 亚像素特征点 3D 重建（reconstruct_points_stereo）：输入左右图中对应点行列，
        /// 输出世界坐标 X/Y/Z。
        /// </summary>
        /// <param name="rowsL">左图点行。</param>
        /// <param name="colsL">左图点列。</param>
        /// <param name="rowsR">右图对应点行。</param>
        /// <param name="colsR">右图对应点列。</param>
        public bool ReconstructPoints(double[] rowsL, double[] colsL, double[] rowsR, double[] colsR,
            out double[] xs, out double[] ys, out double[] zs)
        {
            xs = ys = zs = new double[0];
            if (!IsCreated) return false;
            double[] lx = xs, ly = ys, lz = zs;
            bool ok = SafeRun(() =>
            {
                // row/column 为 [camIdx, pointIdx] 两组拼接
                HTuple row = new HTuple(rowsL).TupleConcat(new HTuple(rowsR));
                HTuple col = new HTuple(colsL).TupleConcat(new HTuple(colsR));
                HTuple camIdx = new HTuple(new int[rowsL.Length]).TupleConcat(
                                 new HTuple(CreateIndexArray(rowsR.Length, 1)));
                HTuple pointIdx = new HTuple(CreateIndexArray(rowsL.Length, 0)).TupleConcat(
                                  new HTuple(CreateIndexArray(rowsR.Length, 0)));
                HTuple x, y, z, cov, pidx;
                HOperatorSet.ReconstructPointsStereo(Model, row, col, new HTuple(),
                    camIdx, pointIdx, out x, out y, out z, out cov, out pidx);
                lx = x.DArr;
                ly = y.DArr;
                lz = z.DArr;
            });
            xs = lx;
            ys = ly;
            zs = lz;
            return ok;
        }

        /// <summary>生成从 0 开始的 int 索引数组。</summary>
        private static int[] CreateIndexArray(int n, int fixedValue)
        {
            int[] a = new int[n];
            for (int i = 0; i < n; i++) a[i] = fixedValue == 1 ? 1 : i;
            return a;
        }

        #endregion

        #region ==================== 标定与释放 ====================

        /// <summary>
        /// 双目一次性标定（binocular_calibration）：输入多组标定板角点世界坐标与双相机像素坐标，
        /// 输出两台内参、各自世界位姿、右相对左位姿与误差。参数语义与 HDevelop 同名算子一致，
        /// 均为扁平 HTuple（每组 pose 7 元素顺序拼接）。
        /// </summary>
        public static bool Calibrate(
            HTuple worldX, HTuple worldY, HTuple worldZ,
            HTuple rows1, HTuple cols1, HTuple rows2, HTuple cols2,
            HTuple startCamParam1, HTuple startCamParam2,
            HTuple startPose1, HTuple startPose2,
            string estimateParams,
            out HTuple camParam1, out HTuple camParam2,
            out HTuple finalPose1, out HTuple finalPose2,
            out HTuple relPose, out double error)
        {
            error = 0;
            camParam1 = camParam2 = finalPose1 = finalPose2 = relPose = new HTuple();
            try
            {
                HTuple errors;
                HOperatorSet.BinocularCalibration(worldX, worldY, worldZ,
                    rows1, cols1, rows2, cols2,
                    startCamParam1, startCamParam2, startPose1, startPose2,
                    estimateParams ?? "all",
                    out camParam1, out camParam2,
                    out finalPose1, out finalPose2, out relPose, out errors);
                double[] ea = errors.DArr;
                error = ea.Length > 0 ? ea[ea.Length - 1] : 0;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>销毁立体模型。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearStereoModel(Model); } catch { }
                        Model = new HTuple();
                    }
                }
                catch { }
            }
        }

        /// <summary>释放模型句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
