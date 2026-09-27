using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// HALCON 差异模型（Variation Model）缺陷检测完整封装：
    /// 用多张良品训练出“理想图 + 允许波动范围”（train_variation_model），
    /// prepare_variation_model 设定绝对/方差阈值后，compare_variation_model 把超出容差的像素
    /// 直接输出为异常区域——典型用于印刷品错印/漏印、组件缺失（完整性检查）、表面划伤/脏污、
    /// 装配错漏装等位置可对齐场景。训练/检测图像应尺寸一致且位姿对齐（可先用形状匹配矫正）。
    /// 模型可存盘复用，句柄由实例持有，Dispose 时 clear_variation_model。
    /// </summary>
    public class HHalconVariation : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>差异模型句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>是否已创建模型。</summary>
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
        /// 创建差异模型（create_variation_model）。
        /// </summary>
        /// <param name="width">训练/检测图像宽。</param>
        /// <param name="height">训练/检测图像高。</param>
        /// <param name="type">像素类型："byte"（默认，绝大多数场景）/"uint2"/"real"。</param>
        /// <param name="mode">训练模式："standard"（标准，默认）/"robust"（对个别异常训练图稳健）/"parallel"。</param>
        public bool Create(int width, int height, string type = "byte", string mode = "standard")
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateVariationModel(width, height, type ?? "byte", mode ?? "standard", out m);
                Model = m;
            });
        }

        /// <summary>
        /// 追加训练图（train_variation_model）：可多次调用，传入一张或多张同样尺寸、对齐的良品图。
        /// 建议至少 5~20 张覆盖正常波动（光照、轻微位置偏差），训练图越有代表性，误报越少。
        /// </summary>
        /// <param name="goodImages">一张或多张良品图（多通道容器或单图均可）。</param>
        public bool Train(HObject goodImages)
        {
            if (!IsCreated || goodImages == null) return false;
            return SafeRun(() => HOperatorSet.TrainVariationModel(goodImages, Model));
        }

        /// <summary>
        /// 训练完成后设定检测容差（prepare_variation_model）。
        /// </summary>
        /// <param name="absThreshold">
        /// 绝对灰度差阈值：像素与理想图差异超过该值即报异常（常用 10~40，越小越严格）。
        /// </param>
        /// <param name="varThreshold">
        /// 波动系数：在训练方差基础上再放宽的倍数（常用 2~5，训练样本正常波动大时调大）。
        /// </param>
        public bool Prepare(double absThreshold = 20, double varThreshold = 3)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.PrepareVariationModel(Model, absThreshold, varThreshold));
        }

        #endregion

        #region ==================== 检测 ====================

        /// <summary>
        /// 检测一张图（compare_variation_model）：返回超差像素组成的异常区域。
        /// 调用前必须完成训练与 <see cref="Prepare"/>。区域可再交 <see cref="HHalconBlob"/>
        /// 做连通与面积过滤，剔除零星噪声。
        /// </summary>
        /// <param name="image">待检图（与训练图同尺寸、对齐）。</param>
        /// <param name="defectRegion">输出异常区域；调用方负责 Dispose。</param>
        public bool Compare(HObject image, out HObject defectRegion)
        {
            defectRegion = null;
            if (!IsCreated || image == null) return false;
            HObject region = null;
            bool ok = SafeRun(() => HOperatorSet.CompareVariationModel(image, out region, Model));
            defectRegion = region;
            return ok;
        }

        /// <summary>
        /// 一站式检测并判定：异常区域连通后，存在面积 ≥ <paramref name="minDefectArea"/> 的连通块即判 NG。
        /// </summary>
        /// <param name="image">待检图。</param>
        /// <param name="defectRegion">输出原始异常区域（调用方 Dispose，不需要可忽略）。</param>
        /// <param name="minDefectArea">单块缺陷最小面积（像素²），低于此面积视为噪声。</param>
        /// <param name="maxDefectArea">输出最大缺陷块面积。</param>
        /// <returns>true=检出缺陷（NG）；false=合格或执行异常（结合 <see cref="HHalconBase.Error"/> 判断）。</param>
        public bool IsDefective(HObject image, out HObject defectRegion,
            double minDefectArea, out double maxDefectArea)
        {
            defectRegion = null;
            maxDefectArea = 0;
            if (!Compare(image, out defectRegion)) return false;
            try
            {
                HObject conn;
                HOperatorSet.Connection(defectRegion, out conn);
                HObject big;
                HOperatorSet.SelectShape(conn, out big, "area", "and", minDefectArea, 1e12);
                try
                {
                    HTuple num;
                    HOperatorSet.CountObj(big, out num);
                    if (num.I > 0)
                    {
                        HTuple areas, rows, cols;
                        HOperatorSet.AreaCenter(big, out areas, out rows, out cols);
                        double[] aa = areas.DArr;
                        foreach (double a in aa) if (a > maxDefectArea) maxDefectArea = a;
                        return true;
                    }
                    return false;
                }
                finally { big.Dispose(); conn.Dispose(); }
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return false;
            }
        }

        #endregion

        #region ==================== 持久化与释放 ====================

        /// <summary>模型存盘（write_variation_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteVariationModel(Model, filePath));
        }

        /// <summary>模型读盘（read_variation_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadVariationModel(filePath, out m);
                Model = m;
            });
        }

        /// <summary>销毁差异模型。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearVariationModel(Model); } catch { }
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
