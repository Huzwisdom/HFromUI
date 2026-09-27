using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 相机标定完整封装（create_calib_data / find_calib_object / calibrate_cameras）：
    /// 面向最常用的单目 + 平面圆点/棋盘格标定板流程——设置内参初值与标定板描述、
    /// 逐图自动找标定板、统一解算内参与各拍位姿、输出重投影误差，并可把 calib_data 存盘。
    /// 手眼标定（calibrate_hand_eye）在 <see cref="HHalconRobot"/> 中提供。
    /// </summary>
    public class HHalconCalibration : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>标定数据模型句柄。</summary>
        public HTuple CalibData { get; private set; } = new HTuple();

        /// <summary>相机类型："area_scan_division"（除法畸变模型，默认，参数少、最常用）
        /// 或 "area_scan_polynomial"（多项式畸变模型，大畸变更精确，参数多）。</summary>
        public string CameraType { get; private set; } = "area_scan_division";

        /// <summary>图像宽（内参第 7 项）。</summary>
        public int ImageWidth { get; private set; }

        /// <summary>图像高（内参第 8 项）。</summary>
        public int ImageHeight { get; private set; }

        /// <summary>已成功加入的标定板观测（拍）数。</summary>
        public int ObservationCount { get; private set; }

        /// <summary>最近一次 calibrate_cameras 的各拍重投影误差数组（像素）。</summary>
        public double[] Errors { get; private set; } = new double[0];

        /// <summary>标定后的相机内参数组（标定前为初值）。</summary>
        public double[] CameraParams { get; private set; } = new double[0];

        /// <summary>标定数据是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return CalibData != null && CalibData.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 建模 ====================

        /// <summary>
        /// 创建单相机 + 单标定板的标定模型并写入内参初值。
        /// division 模型初值顺序：[焦距Focus, 畸变Kappa(0), 像元宽Sx, 像元高Sy, 主点Cx, 主点Cy, 图像宽, 图像高]。
        /// </summary>
        /// <param name="imageWidth">图像宽（像素）。</param>
        /// <param name="imageHeight">图像高（像素）。</param>
        /// <param name="focus">焦距初值（像素，未知时可近似取图像宽）。</param>
        /// <param name="cellSizeX">像元宽（微米/物理单位，决定 world 单位；未知可用 1）。</param>
        /// <param name="cellSizeY">像元高。</param>
        /// <param name="polynomial">true=使用 area_scan_polynomial 多项式模型。</param>
        public bool Create(int imageWidth, int imageHeight,
            double focus, double cellSizeX, double cellSizeY, bool polynomial = false)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple cd;
                HOperatorSet.CreateCalibData("calibration_object", 1, 1, out cd);
                CalibData = cd;
                CameraType = polynomial ? "area_scan_polynomial" : "area_scan_division";
                ImageWidth = imageWidth;
                ImageHeight = imageHeight;

                HTuple startParams;
                if (!polynomial)
                {
                    startParams = new HTuple(new[]
                    {
                        focus, 0.0, cellSizeX, cellSizeY,
                        imageWidth / 2.0, imageHeight / 2.0,
                        (double)imageWidth, (double)imageHeight
                    });
                }
                else
                {
                    // 多项式模型 12 项：Focus,K1,K2,K3,P1,P2,Sx,Sy,Cx,Cy,Width,Height
                    startParams = new HTuple(new[]
                    {
                        focus, 0.0, 0.0, 0.0, 0.0, 0.0,
                        cellSizeX, cellSizeY,
                        imageWidth / 2.0, imageHeight / 2.0,
                        (double)imageWidth, (double)imageHeight
                    });
                }
                HOperatorSet.SetCalibDataCamParam(CalibData, 0, CameraType, startParams);
                CameraParams = startParams.DArr;
            });
        }

        /// <summary>
        /// 设置标定板几何描述（set_calib_data 'calib_object' 'params'）。
        /// </summary>
        /// <param name="markRows">圆点/角点行数。</param>
        /// <param name="markColumns">圆点/角点列数。</param>
        /// <param name="markDistRows">行方向点间距（毫米等物理单位）。</param>
        /// <param name="markDistColumns">列方向点间距。</param>
        /// <param name="z">标定板平面的世界 Z 坐标（一般 0）。</param>
        public bool SetPlate(int markRows, int markColumns,
            double markDistRows, double markDistColumns, double z = 0)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
            {
                HTuple plateParams = new HTuple(new[]
                {
                    0.0, 0.0, z,
                    (double)markRows, (double)markColumns,
                    markDistRows, markDistColumns
                });
                HOperatorSet.SetCalibData(CalibData, "calib_object", 0, "params", plateParams);
            });
        }

        #endregion

        #region ==================== 逐图标定 ====================

        /// <summary>
        /// 在一张图像中自动查找标定板并登记观测（find_calib_object）。
        /// 建议采集 10~20 张覆盖全视场、不同倾角/位置的图片。
        /// </summary>
        /// <param name="image">标定图像。</param>
        /// <param name="poseIndex">输出该图对应的位姿索引（从 0 起）。</param>
        /// <returns>true=找到标定板并登记成功；false=未检出或算子异常。</returns>
        public bool AddImage(HObject image, out int poseIndex)
        {
            poseIndex = -1;
            if (!IsCreated || image == null) return false;
            int idx = poseIndex;
            bool ok = SafeRun(() =>
            {
                HOperatorSet.FindCalibObject(image, CalibData, 0, 0, ObservationCount,
                    new HTuple(), new HTuple());
                idx = ObservationCount;
                ObservationCount++;
            });
            poseIndex = idx;
            return ok;
        }

        /// <summary>
        /// 执行标定解算（calibrate_cameras）：更新内参、各图位姿并输出重投影误差。
        /// </summary>
        /// <param name="maxError">输出最大单拍误差（像素），越小越好，通常 &lt;0.3 为佳。</param>
        /// <param name="meanError">输出平均误差（像素）。</param>
        public bool Run(out double maxError, out double meanError)
        {
            maxError = 0;
            meanError = 0;
            if (!IsCreated || ObservationCount < 3)
            {
                Error = HTranslation.GetContent("标定图片不足，至少需要 3 张成功观测");
                return false;
            }
            double maxE = 0, meanE = 0;
            bool ok = SafeRun(() =>
            {
                HTuple err;
                HOperatorSet.CalibrateCameras(CalibData, out err);
                Errors = err.DArr;
                if (Errors.Length > 0)
                {
                    maxE = double.MinValue;
                    double sum = 0;
                    foreach (double e in Errors) { if (e > maxE) maxE = e; sum += e; }
                    meanE = sum / Errors.Length;
                }
                HTuple p;
                HOperatorSet.GetCalibData(CalibData, "camera", 0, "params", out p);
                CameraParams = p.DArr;
            });
            maxError = maxE;
            meanError = meanE;
            return ok;
        }

        /// <summary>
        /// 取第 <paramref name="poseIndex"/> 张图中标定板相对相机的位姿
        /// （[Tx,Ty,Tz,Rx,Ry,Rz,点序]，平移单位与标定板单位一致，旋转为旋转向量）。
        /// </summary>
        public double[] GetPlatePose(int poseIndex)
        {
            if (!IsCreated) return new double[0];
            double[] r = new double[0];
            SafeRun(() =>
            {
                HTuple p;
                HOperatorSet.GetCalibData(CalibData, "calib_obj_pose",
                    new HTuple(new double[] { 0, 0, poseIndex }), "pose", out p);
                r = p.DArr;
            });
            return r;
        }

        #endregion

        #region ==================== 持久化与释放 ====================

        /// <summary>把标定模型存盘（write_calib_data），可在 HALCON 其它程序中复用。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteCalibData(CalibData, filePath));
        }

        /// <summary>销毁标定数据（clear_calib_data）。</summary>
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
                    CameraParams = new double[0];
                }
                catch { }
            }
        }

        /// <summary>释放标定模型。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
