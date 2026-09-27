using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>通用形状模板匹配命中结果（支持行/列方向不等比缩放）。</summary>
    public struct HGenericShapeResult
    {
        /// <summary>模板参考点所在行。</summary>
        public double Row;
        /// <summary>模板参考点所在列。</summary>
        public double Column;
        /// <summary>旋转角（弧度）。</summary>
        public double Angle;
        /// <summary>行方向缩放系数（1.0=原始）。</summary>
        public double ScaleR;
        /// <summary>列方向缩放系数（1.0=原始）。</summary>
        public double ScaleC;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 通用形状匹配（Generic Shape-Based Matching，create_generic_shape_model /
    /// set_generic_shape_model_param / train_generic_shape_model / find_generic_shape_model）
    /// 完整封装。它是新架构的统一形状匹配引擎：通过参数可配置旋转、各向异性缩放
    /// （scale_r/scale_c 分别设置）乃至有限透视（需要时由训练数据与参数决定），
    /// 并统一输出行列缩放，替代老的 scaled/deformable 多套接口的大部分用法。
    /// 查找结果以 MatchResultID 句柄返回，用 get_generic_shape_model_result 逐项解析；
    /// 模型存读盘/销毁复用形状模板家族 write_shape_model/read_shape_model/clear_shape_model。
    /// </summary>
    public class HHalconGenericShapeMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>通用形状模板句柄。</summary>
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

        #region ==================== 创建/配置/训练 ====================

        /// <summary>创建空的通用形状模板（create_generic_shape_model），随后配置参数并 <see cref="Train"/>。</summary>
        public bool Create()
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateGenericShapeModel(out m);
                Model = m;
            });
        }

        /// <summary>
        /// 设置模板参数（set_generic_shape_model_param），常用：
        /// "num_levels"（金字塔，默认 4）、"angle_start"/"angle_extent"（旋转范围，默认 0 不转）、
        /// "angle_step"、"scale_r_min"/"scale_r_max"/"scale_c_min"/"scale_c_max"（缩放范围，默认 1）、
        /// "scale_r_step"/"scale_c_step"、"optimization"（"none"/"byte"/"speed"）、
        /// "metric"（"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"）、
        /// "contrast_low"/"contrast_high"、"min_contrast"、"min_size"。
        /// 须在 <see cref="Train"/> 之前调用。
        /// </summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetGenericShapeModelParam(Model, name ?? string.Empty,
                    HHalconEnv.ToTuple(value)));
        }

        /// <summary>读模板参数（get_generic_shape_model_param）。</summary>
        public HTuple GetParam(string name)
        {
            if (!IsCreated) return new HTuple();
            return SafeRun(delegate
            {
                HTuple v;
                HOperatorSet.GetGenericShapeModelParam(Model, name ?? string.Empty, out v);
                return v;
            }, new HTuple());
        }

        /// <summary>
        /// 便捷配置：一次性设置旋转与缩放范围（均在训练前生效）。
        /// </summary>
        /// <param name="angleStart">起始角（弧度）。</param>
        /// <param name="angleExtent">角度跨度（弧度）。</param>
        /// <param name="scaleMin">行列共同最小缩放（1.0=不缩）。</param>
        /// <param name="scaleMax">行列共同最大缩放。</param>
        public bool Configure(double angleStart, double angleExtent,
            double scaleMin = 1.0, double scaleMax = 1.0)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
            {
                HOperatorSet.SetGenericShapeModelParam(Model, "angle_start", angleStart);
                HOperatorSet.SetGenericShapeModelParam(Model, "angle_extent",
                    angleExtent <= 0 ? Math.PI * 2 : angleExtent);
                HOperatorSet.SetGenericShapeModelParam(Model, "scale_r_min", scaleMin);
                HOperatorSet.SetGenericShapeModelParam(Model, "scale_r_max", scaleMax);
                HOperatorSet.SetGenericShapeModelParam(Model, "scale_c_min", scaleMin);
                HOperatorSet.SetGenericShapeModelParam(Model, "scale_c_max", scaleMax);
            });
        }

        /// <summary>
        /// 用模板图训练（train_generic_shape_model）：参数配置完成后调用；
        /// 可多次调用以多图训练（支持视角变化的综合模板）。模板图域内即特征。
        /// </summary>
        public bool Train(HObject template)
        {
            if (!IsCreated || template == null) return false;
            return SafeRun(() => HOperatorSet.TrainGenericShapeModel(template, Model));
        }

        /// <summary>创建并配置旋转/缩放范围后直接训练的一站式入口。</summary>
        public bool CreateAndTrain(HObject template,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double scaleMin = 1.0, double scaleMax = 1.0)
        {
            return Create() && Configure(angleStart, angleExtent, scaleMin, scaleMax) && Train(template);
        }

        #endregion

        #region ==================== 查找/解析 ====================

        /// <summary>
        /// 查找模板（find_generic_shape_model）：返回命中结果数组，
        /// 结果句柄在方法内部已释放；需要命中轮廓请用 <see cref="FindWithContours"/>。
        /// </summary>
        public HGenericShapeResult[] Find(HObject image)
        {
            HObject contours;
            HGenericShapeResult[] res;
            FindWithContours(image, out res, out contours);
            if (contours != null) contours.Dispose();
            return res;
        }

        /// <summary>
        /// 查找并输出命中轮廓（get_generic_shape_model_result_object "contours" 的拼接容器）。
        /// </summary>
        /// <param name="matchContours">全部命中实例的轮廓容器（调用方 Dispose）。</param>
        public bool FindWithContours(HObject image,
            out HGenericShapeResult[] results, out HObject matchContours)
        {
            results = new HGenericShapeResult[0];
            matchContours = null;
            if (!IsCreated || image == null) return false;
            HGenericShapeResult[] arr = results;
            HObject contourObj = null;
            bool ok = SafeRun(() =>
            {
                HTuple resultIDs, num;
                HOperatorSet.FindGenericShapeModel(image, Model, out resultIDs, out num);
                int n = num.I;
                try
                {
                    arr = ParseResults(resultIDs, n, out contourObj);
                }
                finally
                {
                    // 匹配结果句柄为通用结果容器，统一释放
                    if (resultIDs != null && resultIDs.Length > 0)
                    {
                        try { HOperatorSet.ClearHandle(resultIDs); } catch { }
                    }
                }
            });
            results = arr;
            matchContours = contourObj;
            return ok;
        }

        /// <summary>从 MatchResultID 元组解析全部命中的行/列/角/缩放/得分及轮廓。</summary>
        private HGenericShapeResult[] ParseResults(HTuple resultIDs, int count, out HObject contours)
        {
            contours = null;
            HGenericShapeResult[] arr = new HGenericShapeResult[count];
            for (int i = 0; i < count; i++)
            {
                HTuple id = resultIDs.TupleSelect(i);
                arr[i] = new HGenericShapeResult
                {
                    Row = GetResultOne(id, "row"),
                    Column = GetResultOne(id, "column"),
                    Angle = GetResultOne(id, "angle"),
                    ScaleR = GetResultOne(id, "scale_r", 1.0),
                    ScaleC = GetResultOne(id, "scale_c", 1.0),
                    Score = GetResultOne(id, "score")
                };
            }
            // 一次性取全部命中轮廓
            try
            {
                HObject obj;
                HOperatorSet.GetGenericShapeModelResultObject(out obj, resultIDs, "all", "contours");
                contours = obj;
            }
            catch (HalconException)
            {
                contours = null;
            }
            return arr;
        }

        /// <summary>取单个命中的标量结果字段；取不到时回退 <paramref name="failValue"/>。</summary>
        private double GetResultOne(HTuple resultID, string name, double failValue = 0)
        {
            try
            {
                HTuple v;
                HOperatorSet.GetGenericShapeModelResult(resultID, "all", name, out v);
                return v.Length > 0 ? v.D : failValue;
            }
            catch (HalconException)
            {
                return failValue;
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
