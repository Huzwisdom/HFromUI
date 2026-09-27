using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// HALCON OCV 光学字符验证（Optical Character Verification）完整封装：
    /// create_ocv_proj 创建字符验证器、traind_ocv_proj 用良品样本训练（可多张累积平均）、
    /// do_ocr_simple 对待检字符区域打分（quality 越接近 1 与良品越一致，低于阈值判不良）、
    /// 模型 write_ocv/read_ocv 存盘复用。典型用途：喷码/激光码/丝印的有无、对错、深浅一致性验证。
    /// 句柄由实例持有，Dispose 时 close_ocv。
    /// </summary>
    public class HHalconOcv : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>OCV 验证器句柄。</summary>
        public HTuple Handle { get; private set; } = new HTuple();

        /// <summary>是否已创建验证器。</summary>
        public bool IsCreated
        {
            get
            {
                try { return Handle != null && Handle.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 创建/训练 ====================

        /// <summary>
        /// 创建 OCV 验证器（create_ocv_proj）。
        /// </summary>
        /// <param name="patternNames">每个字符位置的名称数组（如 ["A","B","C"]，长度=字符数；
        /// 后续训练/验证按名称对应，不关心字符内容时可填 "p1"、"p2"...）。</param>
        public bool Create(string[] patternNames)
        {
            Close();
            if (patternNames == null || patternNames.Length == 0) return false;
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.CreateOcvProj(new HTuple(patternNames), out h);
                Handle = h;
            });
        }

        /// <summary>
        /// 用一张良品字符图训练（traind_ocv_proj）：可多次调用累加多张良品，
        /// HALCON 内部自动平均。字符区域顺序需与 <see cref="Create"/> 的名称顺序一致。
        /// </summary>
        /// <param name="pattern">良品字符区域集（多 region 容器，已按顺序排列）。</param>
        /// <param name="patternName">对应名称（一般取创建时的名称序列；单字符训练时给单名）。</param>
        /// <param name="mode">"single"=单个/单次；"accumulate"=累积多图后统一平均。</param>
        public bool Train(HObject pattern, string patternName, string mode = "accumulate")
        {
            if (!IsCreated || pattern == null) return false;
            return SafeRun(() =>
                HOperatorSet.TraindOcvProj(pattern, Handle,
                    patternName ?? string.Empty, mode ?? "accumulate"));
        }

        #endregion

        #region ==================== 验证 ====================

        /// <summary>
        /// 对一张待检字符图做验证（do_ocr_simple），返回各字符位置的质量分。
        /// </summary>
        /// <param name="pattern">待检字符区域集（顺序与训练一致）。</param>
        /// <param name="patternName">要验证的字符名（"all" 验证全部）。</param>
        /// <param name="adaptPos">位置容差（像素，允许字符上下左右偏移，常用 5~15）。</param>
        /// <param name="adaptSize">尺寸容差（像素，允许大小偏差）。</param>
        /// <param name="adaptAngle">角度容差（弧度）。</param>
        /// <param name="adaptGray">灰度容差（0~255，允许的平均灰度差）。</param>
        /// <param name="threshold">相似度判定阈值（0~1，低于该值记为不匹配，仅用于 do_ocr_simple 内部分类，质量分仍输出）。</param>
        /// <returns>各字符质量分数组（越接近 1 越好）；失败空数组。</returns>
        public double[] Verify(HObject pattern, string patternName = "all",
            double adaptPos = 8, double adaptSize = 2, double adaptAngle = 0.1,
            double adaptGray = 30, double threshold = 0.5)
        {
            double[] q = new double[0];
            if (!IsCreated || pattern == null) return q;
            SafeRun(() =>
            {
                HTuple quality;
                HOperatorSet.DoOcvSimple(pattern, Handle, patternName ?? "all",
                    adaptPos, adaptSize, adaptAngle, adaptGray, threshold, out quality);
                q = quality.DArr;
            });
            return q;
        }

        /// <summary>
        /// 便捷判定：待检图所有字符质量分均不低于 <paramref name="minQuality"/> 即视为合格。
        /// </summary>
        /// <param name="worstQuality">输出最低单项质量分。</param>
        public bool VerifyPass(HObject pattern, out double worstQuality,
            double minQuality = 0.7, double adaptPos = 8, double adaptSize = 2,
            double adaptAngle = 0.1, double adaptGray = 30)
        {
            worstQuality = 0;
            double[] q = Verify(pattern, "all", adaptPos, adaptSize, adaptAngle, adaptGray, minQuality);
            if (q.Length == 0) return false;
            worstQuality = double.MaxValue;
            foreach (double v in q) if (v < worstQuality) worstQuality = v;
            return worstQuality >= minQuality;
        }

        #endregion

        #region ==================== 持久化与释放 ====================

        /// <summary>OCV 模型存盘（write_ocv）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteOcv(Handle, filePath));
        }

        /// <summary>读入 OCV 模型（read_ocv）。</summary>
        public bool Load(string filePath)
        {
            Close();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.ReadOcv(filePath, out h);
                Handle = h;
            });
        }

        /// <summary>关闭 OCV 验证器（close_ocv）。</summary>
        public void Close()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.CloseOcv(Handle); } catch { }
                        Handle = new HTuple();
                    }
                }
                catch { }
            }
        }

        /// <summary>释放 OCV 句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Close();
        }

        #endregion
    }
}
