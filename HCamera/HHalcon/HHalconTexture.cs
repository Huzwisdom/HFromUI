using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 纹理检测（Texture Inspection）完整封装：
    /// create_texture_inspection_model 创建无监督纹理模型，加入多张良品纹理图训练
    /// （add_texture_inspection_model_image / train_texture_inspection_model，内部自动做频域特征
    /// 与 novelty 判定），apply_texture_inspection_model 输出异常区域与结果句柄，
    /// get_texture_inspection_result_object 可取 novelty 区域/分数图等明细。
    /// 适合木材、金属、织物、塑料、皮革等表面纹理的异色、划伤、凹坑、脏污检测，
    /// 无需缺陷样本。模型类型 "soft"（纹理变化小，如金属面，默认）或 "rough"（纹理粗糙，如织物/木材）。
    /// 句柄由实例持有，Dispose 时 clear_texture_inspection_model。
    /// </summary>
    public class HHalconTexture : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>纹理检测模型句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>最近一次 apply 产生的结果句柄（下一次检测前自动清理）。</summary>
        public HTuple LastResult { get; private set; } = new HTuple();

        /// <summary>已加入的训练图数量。</summary>
        public int TrainedImageCount { get; private set; }

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

        #region ==================== 创建/训练 ====================

        /// <summary>
        /// 创建纹理检测模型（create_texture_inspection_model）。
        /// </summary>
        /// <param name="modelType">"soft"（默认，细腻纹理：金属、玻璃、印刷面）或
        /// "rough"（粗糙纹理：木材、织物、皮革）。</param>
        public bool Create(string modelType = "soft")
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateTextureInspectionModel(modelType ?? "soft", out m);
                Model = m;
                TrainedImageCount = 0;
            });
        }

        /// <summary>
        /// 设置模型参数（set_texture_inspection_model_param），常用：
        /// "patch_rotations"（旋转方向增强数量，默认 0）、"novelty_threshold"
        /// （默认 8~12，越小越严格）、"do_apply_novelty_threshold_regions" 等。
        /// </summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetTextureInspectionModelParam(Model, name ?? string.Empty,
                    HHalconEnv.ToTuple(value)));
        }

        /// <summary>
        /// 追加一张训练用良品图（add_texture_inspection_model_image）。
        /// 建议加入 10~30 张覆盖正常纹理与光照的良品图。
        /// </summary>
        public bool AddTrainingImage(HObject image)
        {
            if (!IsCreated || image == null) return false;
            return SafeRun(() =>
            {
                HTuple indices;
                HOperatorSet.AddTextureInspectionModelImage(image, Model, out indices);
                TrainedImageCount++;
            });
        }

        /// <summary>
        /// 执行训练（train_texture_inspection_model）：基于已加入的良品图学习正常纹理特征。
        /// </summary>
        public bool Train()
        {
            if (!IsCreated || TrainedImageCount < 2)
            {
                Error = HTranslation.GetContent("训练图不足，纹理检测至少需要 2 张良品图");
                return false;
            }
            return SafeRun(() => HOperatorSet.TrainTextureInspectionModel(Model));
        }

        #endregion

        #region ==================== 检测 ====================

        /// <summary>
        /// 检测一张图（apply_texture_inspection_model）：输出 novelty 异常区域，
        /// 结果句柄存入 <see cref="LastResult"/> 供 <see cref="GetResultObject"/> 取分数图等。
        /// </summary>
        /// <param name="image">待检图。</param>
        /// <param name="noveltyRegion">输出异常区域（已按 novelty_threshold 二值化），调用方 Dispose。</param>
        public bool Apply(HObject image, out HObject noveltyRegion)
        {
            noveltyRegion = null;
            if (!IsCreated || image == null) return false;
            ClearLastResult();
            HObject region = null;
            HTuple resultID = new HTuple();
            bool ok = SafeRun(() =>
            {
                HOperatorSet.ApplyTextureInspectionModel(image, out region, Model, out resultID);
                LastResult = resultID;
            });
            noveltyRegion = region;
            return ok;
        }

        /// <summary>
        /// 取最近一次检测的明细对象（get_texture_inspection_result_object）。
        /// </summary>
        /// <param name="resultName">
        /// "novelty_region"（异常区域）、"novelty_score"（逐像素新颖度分数图，越大越异常）。
        /// </param>
        public HObject GetResultObject(string resultName = "novelty_score")
        {
            if (LastResult == null || LastResult.Length <= 0) return null;
            return SafeRun(delegate
            {
                HObject obj;
                HOperatorSet.GetTextureInspectionResultObject(out obj, LastResult, resultName ?? "novelty_score");
                return obj;
            }, (HObject)null);
        }

        /// <summary>
        /// 便捷判定：异常区域连通后，存在面积 ≥ <paramref name="minArea"/> 的块即判 NG。
        /// </summary>
        public bool IsDefective(HObject image, double minArea, out double maxArea, out HObject noveltyRegion)
        {
            maxArea = 0;
            noveltyRegion = null;
            if (!Apply(image, out noveltyRegion)) return false;
            try
            {
                HObject conn, big;
                HOperatorSet.Connection(noveltyRegion, out conn);
                HOperatorSet.SelectShape(conn, out big, "area", "and", minArea, 1e12);
                conn.Dispose();
                try
                {
                    HTuple num;
                    HOperatorSet.CountObj(big, out num);
                    if (num.I == 0) return false;
                    HTuple areas, rows, cols;
                    HOperatorSet.AreaCenter(big, out areas, out rows, out cols);
                    foreach (double a in areas.DArr) if (a > maxArea) maxArea = a;
                    return true;
                }
                finally { big.Dispose(); }
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return false;
            }
        }

        /// <summary>清理上一次检测结果句柄。</summary>
        private void ClearLastResult()
        {
            try
            {
                if (LastResult != null && LastResult.Length > 0)
                {
                    try { HOperatorSet.ClearTextureInspectionResult(LastResult); } catch { }
                    LastResult = new HTuple();
                }
            }
            catch { }
        }

        #endregion

        #region ==================== 持久化与释放 ====================

        /// <summary>模型存盘（write_texture_inspection_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteTextureInspectionModel(Model, filePath));
        }

        /// <summary>模型读盘（read_texture_inspection_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadTextureInspectionModel(filePath, out m);
                Model = m;
            });
        }

        /// <summary>销毁纹理模型及结果句柄。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                ClearLastResult();
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearTextureInspectionModel(Model); } catch { }
                        Model = new HTuple();
                    }
                    TrainedImageCount = 0;
                }
                catch { }
            }
        }

        /// <summary>释放模型资源。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
