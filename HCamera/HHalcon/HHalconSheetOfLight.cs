using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// HALCON Sheet-of-Light（激光线扫/光切法）3D 重建完整封装：
    /// create_sheet_of_light_model 以测量区域建模型，逐张激光轮廓图 measure_profile_sheet_of_light
    /// （相机/激光头随编码器逐行扫描），扫描完成后 get_sheet_of_light_result 取视差图、
    /// 置信度图及 X/Y/Z 图和 "xyz" 3D 物体模型（点云）；set_sheet_of_light_param 配置
    /// "method"（默认/bilateral）、"min_gray"、"num_profiles"、"scale_x/y/z"、
    /// "calibration_x/y/z"、"ambiguity_solving" 等参数。模型由实例持有，Dispose 时释放。
    /// 适用于焊缝、胶路、台阶、平面度等线激光 3D 测量场景。
    /// </summary>
    public class HHalconSheetOfLight : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>Sheet-of-Light 模型句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>已处理的轮廓图数量。</summary>
        public int ProfileCount { get; private set; }

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
        /// 创建线激光模型（create_sheet_of_light_model）。
        /// </summary>
        /// <param name="profileRegion">
        /// 激光线搜索区域（矩形 ROI；用 <see cref="HHalconRoi"/> 生成，缩小范围可提速抗扰）。
        /// </param>
        /// <param name="paramNames">参数名数组（可空）。</param>
        /// <param name="paramValues">参数值数组（可空），常用：
        /// "num_profiles"（总轮廓行数）、"min_gray"（最小线灰度）、"method"（"default"/"bilateral"）、
        /// "scale_x"/"scale_y"（像素物理尺寸）、"calibration_x/y/z"（标定后三维缩放/倾斜校正）。</param>
        public bool Create(HObject profileRegion,
            string[] paramNames = null, object[] paramValues = null)
        {
            Clear();
            if (profileRegion == null) return false;
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
                HOperatorSet.CreateSheetOfLightModel(profileRegion,
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    vals, out m);
                Model = m;
                ProfileCount = 0;
            });
        }

        /// <summary>设置模型参数（set_sheet_of_light_param）。</summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetSheetOfLightParam(Model, name ?? string.Empty, HHalconEnv.ToTuple(value)));
        }

        #endregion

        #region ==================== 扫描/重建 ====================

        /// <summary>
        /// 处理一张激光轮廓图（measure_profile_sheet_of_light）：按扫描顺序逐张调用，
        /// 结果在模型内部累积。
        /// </summary>
        /// <param name="profileImage">当前行激光图。</param>
        /// <param name="movementPose">
        /// 该行对应的运动位姿（7 元素 pose；等速直线扫描可省略，用 scale_y 代替）。
        /// </param>
        public bool MeasureProfile(HObject profileImage, double[] movementPose = null)
        {
            if (!IsCreated || profileImage == null) return false;
            bool ok = SafeRun(() =>
                HOperatorSet.MeasureProfileSheetOfLight(profileImage, Model,
                    movementPose != null ? new HTuple(movementPose) : new HTuple()));
            if (ok) ProfileCount++;
            return ok;
        }

        /// <summary>
        /// 直接送入一张已生成的轮廓视差图（set_profile_sheet_of_light），
        /// 适用于外部算法已提线、只借 HALCON 做标定换算的场合；可一次给多行与对应运动位姿。
        /// </summary>
        public bool SetProfileDisparity(HObject disparityImage, double[] movementPoses = null)
        {
            if (!IsCreated || disparityImage == null) return false;
            return SafeRun(() =>
                HOperatorSet.SetProfileSheetOfLight(disparityImage, Model,
                    movementPoses != null ? new HTuple(movementPoses) : new HTuple()));
        }

        /// <summary>
        /// 取重建结果（get_sheet_of_light_result），调用方负责 Dispose。
        /// </summary>
        /// <param name="resultName">
        /// "disparity"（视差/高度图）、"score"（每点置信度）、"x"/"y"/"z"（三维坐标图，
        /// 需先配置标定参数）、"xyz"（3D 物体模型/点云句柄，存盘 write_object_model_3d 或交
        /// <see cref="HHalconSurfaceMatch"/> 匹配）。
        /// </param>
        public HObject GetResult(string resultName = "disparity")
        {
            if (!IsCreated) return null;
            return SafeRun(delegate
            {
                HObject r;
                HOperatorSet.GetSheetOfLightResult(out r, Model, resultName ?? "disparity");
                return r;
            }, (HObject)null);
        }

        /// <summary>便捷获取视差（高度）图。</summary>
        public HObject GetDisparity() => GetResult("disparity");

        /// <summary>便捷获取置信度图。</summary>
        public HObject GetScore() => GetResult("score");

        /// <summary>便捷获取 XYZ 3D 点云模型（object model 3D 句柄，以 HObject 持有，用后 Dispose）。</summary>
        public HObject GetPointCloud() => GetResult("xyz");

        #endregion

        #region ==================== 释放 ====================

        /// <summary>
        /// 重新开始一帧扫描：销毁并按原测量区域重建空模型。
        /// </summary>
        /// <param name="profileRegion">创建时的同一测量区域。</param>
        public bool Restart(HObject profileRegion)
        {
            if (profileRegion == null) return false;
            return SafeRun(() =>
            {
                try { if (IsCreated) HOperatorSet.ClearSheetOfLightModel(Model); } catch { }
                HTuple m;
                HOperatorSet.CreateSheetOfLightModel(profileRegion, new HTuple(), new HTuple(), out m);
                Model = m;
                ProfileCount = 0;
            });
        }

        /// <summary>销毁模型。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearSheetOfLightModel(Model); } catch { }
                        Model = new HTuple();
                    }
                    ProfileCount = 0;
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
