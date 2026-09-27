using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>DL 目标检测框结果（bbox：行、列、高、宽 + 标签 + 置信度）。</summary>
    public struct HDlDetection
    {
        /// <summary>检测框中心行。</summary>
        public double Row;
        /// <summary>检测框中心列。</summary>
        public double Column;
        /// <summary>检测框高（像素）。</summary>
        public double Height;
        /// <summary>检测框宽（像素）。</summary>
        public double Width;
        /// <summary>类别名称。</summary>
        public string Label;
        /// <summary>类别编号。</summary>
        public int LabelId;
        /// <summary>置信度（0~1）。</summary>
        public double Confidence;
    }

    /// <summary>
    /// HALCON 深度学习推理（Deep Learning Inference）完整封装，覆盖四大任务：
    /// 图像分类（classification）、目标检测（detection，bbox 结果）、
    /// 异常检测（anomaly_detection：异常分数 + 异常热力图）、
    /// 语义/实例分割（segmentation：分割标签图）。
    /// 流程：read_dl_model 载入训练好的 .dl 模型 → create_dict/set_dict_object 构造 DLSample
    /// （放入预处理好的 "image"）→ apply_dl_model 推理 → get_dict_tuple/get_dict_object 取结果。
    /// 支持 gen_dl_model_heatmap 生成类别热力图。DLSample/DLResult 均为字典句柄，
    /// 用 clear_handle 释放。<b>深度学习需要 HALCON 独立的 Deep Learning 授权</b>，
    /// 安装时须勾选 Deep Learning 组件；训练通常在 HDevelop/工作站完成，本类负责产线推理。
    /// </summary>
    public class HHalconDeepLearning : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>深度学习模型句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>模型是否已加载。</summary>
        public bool IsLoaded
        {
            get
            {
                try { return Model != null && Model.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 加载/参数/释放 ====================

        /// <summary>加载深度学习模型（read_dl_model，.dl 文件）。</summary>
        /// <param name="modelFile">模型文件路径。</param>
        public bool Load(string modelFile)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.ReadDlModel(modelFile, out h);
                Model = h;
            });
        }

        /// <summary>读模型参数（get_dl_model_param），如 "type"、"image_width"、"image_height"、
        /// "image_num_channels"、"class_names"（dict 句柄）、"image_dimensions"。</summary>
        public HTuple GetParam(string paramName)
        {
            if (!IsLoaded) return new HTuple();
            return SafeRun(delegate
            {
                HTuple v;
                HOperatorSet.GetDlModelParam(Model, paramName ?? string.Empty, out v);
                return v;
            }, new HTuple());
        }

        /// <summary>便捷读取模型类型（"classification"/"detection"/"segmentation"/"anomaly_detection"）。</summary>
        public string ModelType => ToS(GetParam("type"));

        /// <summary>释放深度学习模型。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsLoaded)
                    {
                        try { HOperatorSet.ClearDlModel(Model); } catch { }
                        Model = new HTuple();
                    }
                }
                catch { }
            }
        }

        #endregion

        #region ==================== DLSample 构造 ====================

        /// <summary>
        /// 用一张图构造单个 DLSample 字典：create_dict 后把图像写入 "image" 键。
        /// 图像须预先按模型要求预处理（缩放到 image_width×image_height、转 3 通道、
        /// 归一化等，可用 <see cref="HHalconImage"/> / <see cref="HHalconFilter"/> 完成）。
        /// 返回的字典句柄用 <see cref="ReleaseHandle"/> 释放。
        /// </summary>
        public HTuple BuildSample(HObject image)
        {
            return SafeRun(delegate
            {
                HTuple sample;
                HOperatorSet.CreateDict(out sample);
                HOperatorSet.SetDictObject(image, sample, "image");
                return sample;
            }, new HTuple());
        }

        /// <summary>用多张图构造批量 DLSample（batch），返回字典句柄元组。</summary>
        public HTuple BuildSampleBatch(HObject[] images)
        {
            HTuple batch = new HTuple();
            if (images == null) return batch;
            foreach (HObject img in images)
            {
                HTuple s = BuildSample(img);
                if (s.Length <= 0)
                {
                    ReleaseHandle(ref batch);
                    return new HTuple();
                }
                batch = batch.TupleConcat(s);
            }
            return batch;
        }

        /// <summary>向 DLSample 字典追加一个元组数据（如额外输入张量）。</summary>
        public bool SetSampleTuple(HTuple sample, string key, HTuple value)
        {
            return SafeRun(() => HOperatorSet.SetDictTuple(sample, key ?? string.Empty, value ?? new HTuple()));
        }

        /// <summary>向 DLSample 字典追加一个对象数据（如图像、区域）。</summary>
        public bool SetSampleObject(HTuple sample, string key, HObject value)
        {
            if (value == null) return false;
            return SafeRun(() => HOperatorSet.SetDictObject(value, sample, key ?? string.Empty));
        }

        /// <summary>释放字典句柄（DLSample / DLResult / class_names dict 等）。</summary>
        public static void ReleaseHandle(ref HTuple handle)
        {
            try
            {
                if (handle != null && handle.Length > 0)
                {
                    try { HOperatorSet.ClearHandle(handle); } catch { }
                    handle = new HTuple();
                }
            }
            catch { }
        }

        #endregion

        #region ==================== 推理 ====================

        /// <summary>
        /// 执行推理（apply_dl_model）。
        /// </summary>
        /// <param name="sampleBatch">单个 DLSample 或批量句柄元组（<see cref="BuildSample"/>）。</param>
        /// <param name="resultBatch">输出 DLResult 字典句柄（每样本一个），用后 <see cref="ReleaseHandle"/> 释放。</param>
        /// <param name="outputs">指定输出键（可空=全部输出）。</param>
        public bool Apply(HTuple sampleBatch, out HTuple resultBatch, string[] outputs = null)
        {
            resultBatch = new HTuple();
            if (!IsLoaded || sampleBatch == null || sampleBatch.Length <= 0) return false;
            HTuple results = new HTuple();
            bool ok = SafeRun(() =>
                HOperatorSet.ApplyDlModel(Model, sampleBatch,
                    outputs != null ? new HTuple(outputs) : new HTuple(),
                    out results));
            resultBatch = results;
            return ok;
        }

        /// <summary>从 DLResult 字典取元组结果（get_dict_tuple）。</summary>
        public HTuple GetResultTuple(HTuple resultHandle, string key)
        {
            return SafeRun(delegate
            {
                HTuple v;
                HOperatorSet.GetDictTuple(resultHandle, key ?? string.Empty, out v);
                return v;
            }, new HTuple());
        }

        /// <summary>从 DLResult 字典取对象结果（get_dict_object，如分割图、异常图）。</summary>
        public HObject GetResultObject(HTuple resultHandle, string key)
        {
            return SafeRun(delegate
            {
                HObject o;
                HOperatorSet.GetDictObject(out o, resultHandle, key ?? string.Empty);
                return o;
            }, (HObject)null);
        }

        /// <summary>批量结果中的样本数。</summary>
        public static int ResultCount(HTuple resultBatch)
        {
            try { return resultBatch != null ? resultBatch.Length : 0; }
            catch { return 0; }
        }

        #endregion

        #region ==================== 分类结果解析 ====================

        /// <summary>
        /// 解析单个分类结果：返回各类别置信度数组与最佳类别。
        /// </summary>
        /// <param name="resultHandle">单样本 DLResult 句柄。</param>
        /// <param name="bestLabel">输出置信度最高的类别名。</param>
        /// <param name="bestConfidence">输出最高置信度。</param>
        /// <returns>类别名数组（顺序与置信度对应）；失败空数组。</returns>
        public string[] ParseClassification(HTuple resultHandle,
            out string bestLabel, out double bestConfidence)
        {
            bestLabel = string.Empty;
            bestConfidence = 0;
            string[] labels = ToSArray(GetResultTuple(resultHandle, "classification_result"));
            double[] conf = ToDArray(GetResultTuple(resultHandle, "classification_confidences"));
            if (labels.Length == 0) return labels;
            // 模型输出的最佳类别与各类别置信度；取置信度最大值兜底
            bestLabel = labels[0];
            for (int i = 0; i < conf.Length; i++)
            {
                if (conf[i] > bestConfidence)
                {
                    bestConfidence = conf[i];
                    if (i < labels.Length) bestLabel = labels[i];
                }
            }
            return labels;
        }

        #endregion

        #region ==================== 检测结果解析 ====================

        /// <summary>
        /// 解析单个目标检测结果：读取 bbox_row/col/height/width/label/label_id/confidence
        /// 七个键，组装成检测框数组。
        /// </summary>
        public HDlDetection[] ParseDetection(HTuple resultHandle)
        {
            double[] rows = ToDArray(GetResultTuple(resultHandle, "bbox_row"));
            double[] cols = ToDArray(GetResultTuple(resultHandle, "bbox_col"));
            double[] hs = ToDArray(GetResultTuple(resultHandle, "bbox_height"));
            double[] ws = ToDArray(GetResultTuple(resultHandle, "bbox_width"));
            string[] labels = ToSArray(GetResultTuple(resultHandle, "bbox_label"));
            double[] ids = ToDArray(GetResultTuple(resultHandle, "bbox_label_id"));
            double[] conf = ToDArray(GetResultTuple(resultHandle, "bbox_confidence"));
            HDlDetection[] arr = new HDlDetection[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                arr[i] = new HDlDetection
                {
                    Row = rows[i],
                    Column = i < cols.Length ? cols[i] : 0,
                    Height = i < hs.Length ? hs[i] : 0,
                    Width = i < ws.Length ? ws[i] : 0,
                    Label = i < labels.Length ? labels[i] : string.Empty,
                    LabelId = i < ids.Length ? (int)ids[i] : 0,
                    Confidence = i < conf.Length ? conf[i] : 0
                };
            }
            return arr;
        }

        #endregion

        #region ==================== 分割/异常结果解析 ====================

        /// <summary>取语义/实例分割标签图（"segmentation_image"），调用方 Dispose。</summary>
        public HObject GetSegmentationImage(HTuple resultHandle)
        {
            return GetResultObject(resultHandle, "segmentation_image");
        }

        /// <summary>
        /// 取异常检测总分（"anomaly_score"：图像级异常分数，越大越异常，阈值由训练时决定）。
        /// </summary>
        public double GetAnomalyScore(HTuple resultHandle)
        {
            return ToD(GetResultTuple(resultHandle, "anomaly_score"));
        }

        /// <summary>取逐像素异常图（"anomaly_image"），调用方 Dispose。</summary>
        public HObject GetAnomalyImage(HTuple resultHandle)
        {
            return GetResultObject(resultHandle, "anomaly_image");
        }

        #endregion

        #region ==================== 热力图 ====================

        /// <summary>
        /// 生成类别热力图（gen_dl_model_heatmap）：返回 "heatmap_image" 图像（调用方 Dispose）。
        /// </summary>
        /// <param name="sample">单个 DLSample。</param>
        /// <param name="method">热力图方法，默认 "gradient"（Grad-CAM 类）。</param>
        /// <param name="targetClasses">目标类别（类名或类 ID；一般给模型类别集合或关注类别）。</param>
        public HObject GetHeatmap(HTuple sample, object targetClasses, string method = "gradient")
        {
            if (!IsLoaded) return null;
            return SafeRun(delegate
            {
                HTuple heatResult;
                HOperatorSet.GenDlModelHeatmap(Model, sample, method ?? "gradient",
                    HHalconEnv.ToTuple(targetClasses), new HTuple(), out heatResult);
                try
                {
                    HObject img;
                    HOperatorSet.GetDictObject(out img, heatResult, "heatmap_image");
                    return img;
                }
                finally
                {
                    try { HOperatorSet.ClearHandle(heatResult); } catch { }
                }
            }, (HObject)null);
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放模型句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
