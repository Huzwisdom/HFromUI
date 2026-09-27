using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// 机器人视觉引导与手眼标定完整封装：
    /// 手眼标定（calibrate_hand_eye，支持 moving-cam 移动相机 / stationary-cam-moving-part 固定相机移动工件两种安装），
    /// 位姿求逆/复合（pose_invert/pose_compose）、图像像素反投影到世界平面（image_points_to_world_plane）、
    /// 视线反投影（get_line_of_sight，支持高度先验的三维逆投影）、3D 点正投影（project_3d_point）。
    /// 位姿统一采用 HALCON 7 元素格式 [Tx,Ty,Tz,Rx,Ry,Rz,Order]，平移单位与标定一致（通常毫米）。
    /// </summary>
    public class HHalconRobot : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>手眼标定数据句柄。</summary>
        public HTuple CalibData { get; private set; } = new HTuple();

        /// <summary>安装方式："hand_eye_moving_cam"（相机装在法兰上随机器人运动，默认）
        /// 或 "hand_eye_stationary_cam_moving_part"（相机固定、机器人抓着标定板运动）。</summary>
        public string Setup { get; private set; } = "hand_eye_moving_cam";

        /// <summary>已登记的观测（机器人位姿）数量，一般需要 ≥10 组、旋转与平移姿态尽量多样。</summary>
        public int ObservationCount { get; private set; }

        /// <summary>标定残差数组。</summary>
        public double[] Errors { get; private set; } = new double[0];

        /// <summary>手眼数据是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return CalibData != null && CalibData.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 手眼标定 ====================

        /// <summary>
        /// 创建手眼标定数据（create_calib_data 的手眼模式，numCameras/numCalibObjects 均为 0）。
        /// </summary>
        /// <param name="movingCamera">
        /// true=相机随机器人移动（hand_eye_moving_cam，求相机在法兰坐标系下的位姿）；
        /// false=相机固定、标定板随工件移动（hand_eye_stationary_cam_moving_part，求标定板在基座系位姿）。
        /// </param>
        public bool Create(bool movingCamera = true)
        {
            Clear();
            return SafeRun(() =>
            {
                Setup = movingCamera ? "hand_eye_moving_cam" : "hand_eye_stationary_cam_moving_part";
                HTuple cd;
                HOperatorSet.CreateCalibData(Setup, 0, 0, out cd);
                CalibData = cd;
            });
        }

        /// <summary>
        /// 登记一组观测位姿（set_calib_data）。
        /// </summary>
        /// <param name="toolInBasePose">
        /// 移动相机模式：机器人法兰（工具）在基座系下的位姿，取自机器人控制器；
        /// 固定相机模式：机器人运动后的法兰位姿。
        /// </param>
        /// <param name="objectInCameraPose">
        /// 移动相机模式：标定板在相机坐标系下的位姿（由各拍图像经已标定相机解出）；
        /// 固定相机模式：标定板在相机系下的位姿。
        /// </param>
        public bool AddObservation(double[] toolInBasePose, double[] objectInCameraPose)
        {
            if (!IsCreated || toolInBasePose == null || objectInCameraPose == null) return false;
            return SafeRun(() =>
            {
                int i = ObservationCount;
                HOperatorSet.SetCalibData(CalibData, "tool", i, "trans", new HTuple(toolInBasePose));
                HOperatorSet.SetCalibData(CalibData, "object", i, "trans", new HTuple(objectInCameraPose));
                ObservationCount++;
            });
        }

        /// <summary>
        /// 执行手眼标定（calibrate_hand_eye）。
        /// </summary>
        /// <param name="cameraInToolPose">
        /// 移动相机模式输出：相机在法兰（工具）坐标系下的位姿（视觉引导用 X=A·X·B 的解）。
        /// </param>
        /// <param name="objectInBasePose">
        /// 固定相机模式输出：标定板（工件）在基座坐标系下的位姿。
        /// </param>
        public bool Run(out double[] cameraInToolPose, out double[] objectInBasePose)
        {
            cameraInToolPose = objectInBasePose = new double[0];
            if (!IsCreated || ObservationCount < 3)
            {
                Error = HTranslation.GetContent("手眼观测不足，至少需要 3 组位姿");
                return false;
            }
            double[] camPose = new double[0];
            double[] objPose = new double[0];
            bool ok = SafeRun(() =>
            {
                HTuple err;
                HOperatorSet.CalibrateHandEye(CalibData, out err);
                Errors = err.DArr;
                // 移动相机：标定结果为“工具（法兰）在相机下”的位姿，求逆得相机在法兰下的位姿
                HTuple toolInCam;
                HOperatorSet.GetCalibData(CalibData, "camera", 0, "tool_in_cam_pose", out toolInCam);
                HTuple camInTool;
                HOperatorSet.PoseInvert(toolInCam, out camInTool);
                camPose = camInTool.DArr;
                // 固定相机移动工件：标定板（工件）在机器人基座下的位姿
                HTuple objInBase;
                HOperatorSet.GetCalibData(CalibData, "calib_obj", 0, "obj_in_base_pose", out objInBase);
                objPose = objInBase.DArr;
            });
            cameraInToolPose = camPose;
            objectInBasePose = objPose;
            return ok;
        }

        /// <summary>销毁手眼标定数据。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearCalibData(CalibData); } catch { }
                        CalibData = new HTuple();
                    }
                    ObservationCount = 0;
                    Errors = new double[0];
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 位姿运算（静态） ====================

        /// <summary>位姿求逆（pose_invert）：如法兰→基座逆为基座→法兰；失败返回空数组。</summary>
        public static double[] InvertPose(double[] pose)
        {
            if (pose == null || pose.Length < 7) return null;
            try
            {
                HTuple inv;
                HOperatorSet.PoseInvert(new HTuple(pose), out inv);
                return inv.DArr;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 位姿复合（pose_compose）：返回 poseLeft × poseRight 的合成位姿，
        /// 典型用法：基座→法兰 × 法兰→相机 = 基座→相机。
        /// </summary>
        public static double[] ComposePose(double[] poseLeft, double[] poseRight)
        {
            if (poseLeft == null || poseRight == null) return null;
            try
            {
                HTuple c;
                HOperatorSet.PoseCompose(new HTuple(poseLeft), new HTuple(poseRight), out c);
                return c.DArr;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 视觉引导点解算：像素坐标 → 抓取位姿。
        /// 由 <see cref="ImagePointsToWorld"/> 得到目标在相机系（标定平面）坐标，
        /// 再复合 相机→法兰→基座 两级位姿得到机器人基坐标。
        /// </summary>
        /// <param name="xInCamera">目标在相机系 X（标定平面坐标）。</param>
        /// <param name="yInCamera">目标在相机系 Y。</param>
        /// <param name="zInCamera">目标抓取高度（相机系 Z 方向偏移）。</param>
        /// <param name="cameraInTool">相机在法兰系位姿（手眼标定结果）。</param>
        /// <param name="toolInBase">当前法兰在基座系位姿（机器人反馈）。</param>
        /// <param name="rxRyRz">附加到基座抓取位姿的姿态 [Rx,Ry,Rz]（一般取工具向下等固定姿态）。</param>
        /// <returns>7 元素基座系抓取位姿；失败 null。</returns>
        public static double[] TargetToBase(double xInCamera, double yInCamera, double zInCamera,
            double[] cameraInTool, double[] toolInBase, double[] rxRyRz)
        {
            try
            {
                double[] targetInCamera = new double[]
                {
                    xInCamera, yInCamera, zInCamera,
                    rxRyRz != null && rxRyRz.Length > 2 ? rxRyRz[0] : 0,
                    rxRyRz != null && rxRyRz.Length > 2 ? rxRyRz[1] : Math.PI,
                    rxRyRz != null && rxRyRz.Length > 2 ? rxRyRz[2] : 0,
                    0
                };
                double[] targetInTool = ComposePose(cameraInTool, targetInCamera);
                if (targetInTool == null) return null;
                return ComposePose(toolInBase, targetInTool);
            }
            catch { return null; }
        }

        #endregion

        #region ==================== 逆投影 / 正投影（静态） ====================

        /// <summary>
        /// 像素点反投影到相机系下指定世界平面（image_points_to_world_plane）。
        /// </summary>
        /// <param name="cameraParams">相机内参（标定结果）。</param>
        /// <param name="worldPose">世界平面在相机系下的位姿（标定板位姿即可）。</param>
        /// <param name="rows">像素行数组。</param>
        /// <param name="cols">像素列数组。</param>
        /// <param name="scale">单位换算比例（毫米/米取值：1000 时内参像元单位为米而平面输出毫米）；同单位传 1。</param>
        /// <param name="x">输出世界 X 数组。</param>
        /// <param name="y">输出世界 Y 数组。</param>
        public static bool ImagePointsToWorld(double[] cameraParams, double[] worldPose,
            double[] rows, double[] cols, double scale, out double[] x, out double[] y)
        {
            x = y = new double[0];
            if (cameraParams == null || worldPose == null || rows == null || cols == null) return false;
            try
            {
                HTuple hx, hy;
                HOperatorSet.ImagePointsToWorldPlane(new HTuple(cameraParams), new HTuple(worldPose),
                    new HTuple(rows), new HTuple(cols), scale, out hx, out hy);
                x = hx.DArr;
                y = hy.DArr;
                return true;
            }
            catch (HalconException) { return false; }
        }

        /// <summary>
        /// 单点逆投影便捷重载：返回该像素在世界平面上的 (X,Y)。
        /// </summary>
        public static bool ImagePointToWorld(double[] cameraParams, double[] worldPose,
            double row, double col, double scale, out double x, out double y)
        {
            x = y = 0;
            double[] xs, ys;
            if (!ImagePointsToWorld(cameraParams, worldPose,
                    new[] { row }, new[] { col }, scale, out xs, out ys)) return false;
            if (xs.Length < 1) return false;
            x = xs[0];
            y = ys[0];
            return true;
        }

        /// <summary>
        /// 视线反投影（get_line_of_sight）：给出像素对应空间射线的相机系光心 P 与方向点 Q；
        /// 已知抓取面高度时可用平面相交解算 3D 坐标（无需标定板平面先验）。
        /// </summary>
        public static bool GetLineOfSight(double row, double col, double[] cameraParams,
            out double[] p, out double[] q)
        {
            p = q = null;
            try
            {
                HTuple px, py, pz, qx, qy, qz;
                HOperatorSet.GetLineOfSight(row, col, new HTuple(cameraParams),
                    out px, out py, out pz, out qx, out qy, out qz);
                p = new[] { px.D, py.D, pz.D };
                q = new[] { qx.D, qy.D, qz.D };
                return true;
            }
            catch (HalconException) { return false; }
        }

        /// <summary>
        /// 3D 点正投影到像素（project_3d_point）：用于把规划点/检测结果画回图像做核对。
        /// </summary>
        /// <param name="x">相机系 X。</param>
        /// <param name="y">相机系 Y。</param>
        /// <param name="z">相机系 Z。</param>
        /// <param name="cameraParams">相机内参。</param>
        public static bool Project3DPoint(double x, double y, double z, double[] cameraParams,
            out double row, out double col)
        {
            row = col = 0;
            try
            {
                HTuple r, c;
                HOperatorSet.Project3dPoint(x, y, z, new HTuple(cameraParams), out r, out c);
                row = r.D;
                col = c.D;
                return true;
            }
            catch (HalconException) { return false; }
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放手眼标定数据。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
