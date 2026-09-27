using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>灰度（NCC 归一化互相关）模板匹配命中结果。</summary>
    public struct HNccMatchResult
    {
        /// <summary>模板参考点（默认模板重心）所在行。</summary>
        public double Row;
        /// <summary>模板参考点所在列。</summary>
        public double Column;
        /// <summary>旋转角（弧度）。</summary>
        public double Angle;
        /// <summary>匹配得分（0~1，NCC 相关系数）。</summary>
        public double Score;
    }

    /// <summary>
    /// HALCON 灰度归一化互相关匹配（NCC / Gray-Value Matching，create_ncc_model / find_ncc_model）
    /// 完整封装。与基于形状的匹配相比，NCC 直接比较灰度块（内部自动归一化，抗线性光照变化），
    /// 适合<b>特征纹理丰富但边缘不清晰</b>的目标（印刷图案、布料、带纹理表面、低对比度工件），
    /// 支持旋转搜索但不支持缩放；需要缩放请用 <see cref="HHalconScaledShapeMatch"/>。
    /// 句柄独立持有（nccModel，不可与形状模板句柄混用），Dispose 时 clear_ncc_model。
    /// </summary>
    public class HHalconNccMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>NCC 模板句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>建模板时的角度搜索范围（弧度，Find 默认沿用）。</summary>
        public double AngleStart { get; private set; } = -Math.PI;

        /// <summary>建模板时的角度搜索跨度（弧度）。</summary>
        public double AngleExtent { get; private set; } = Math.PI * 2;

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
        /// 创建 NCC 灰度模板（create_ncc_model）。
        /// </summary>
        /// <param name="template">模板图（域内即模板图案，建议先 reduce_domain 裁剪）。</param>
        /// <param name="numLevels">金字塔层数："auto"（默认）或 0~10。</param>
        /// <param name="angleStart">搜索起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">搜索角度跨度（弧度，默认 360°）。</param>
        /// <param name="angleStep">角度步长（弧度，"auto"/0 自动）。</param>
        /// <param name="metric">
        /// 极性度量："use_polarity"（模板与目标亮暗关系必须一致，默认）/
        /// "ignore_global_polarity"（允许整体反色，如黑底白字↔白底黑字）。
        /// </param>
        public bool Create(HObject template, object numLevels = null,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            object angleStep = null, string metric = "use_polarity")
        {
            Clear();
            if (template == null) return false;
            AngleStart = angleStart;
            AngleExtent = angleExtent <= 0 ? Math.PI * 2 : angleExtent;
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.CreateNccModel(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    AngleStart, AngleExtent,
                    HHalconEnv.ToTuple(angleStep ?? "auto"),
                    metric ?? "use_polarity", out m);
                Model = m;
            });
        }

        /// <summary>设置 NCC 模板参数（set_ncc_model_param），如 "timeout"（毫秒，0=不限时）。</summary>
        public bool SetParam(string name, object value)
        {
            if (!IsCreated) return false;
            return SafeRun(() =>
                HOperatorSet.SetNccModelParam(Model, name ?? string.Empty,
                    HHalconEnv.ToTuple(value)));
        }

        /// <summary>
        /// 自动评估 NCC 参数（determine_ncc_model_params）：给定模板与角度范围，
        /// 返回推荐参数名/值（num_levels、angle_step 等），供 <see cref="Create"/> 参考。
        /// </summary>
        public static bool DetermineParams(HObject template, out string[] paramNames,
            out double[] paramValues, object numLevels = null,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            string metric = "use_polarity", string parameters = "all")
        {
            paramNames = new string[0];
            paramValues = new double[0];
            if (template == null) return false;
            try
            {
                HTuple names, vals;
                HOperatorSet.DetermineNccModelParams(template,
                    HHalconEnv.ToTuple(numLevels ?? "auto"),
                    angleStart, angleExtent, metric ?? "use_polarity",
                    parameters ?? "all", out names, out vals);
                paramNames = names.SArr;
                paramValues = vals.DArr;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 查找模板（find_ncc_model）。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="minScore">最低相关系数（0~1，常用 0.5~0.8）。</param>
        /// <param name="numMatches">最多返回个数（0=全部达分目标）。</param>
        /// <param name="maxOverlap">两个结果允许的最大重叠比例（0~1）。</param>
        /// <param name="subPixel">亚像素方式："true"（默认，亚像素）/"false"。</param>
        /// <param name="numLevels">查找金字塔层数：0=用建模板设置（默认）。</param>
        /// <param name="angleStart">本次搜索起始角（默认用建模板角度范围）。</param>
        /// <param name="angleExtent">本次搜索角度跨度（默认用建模板角度范围）。</param>
        public HNccMatchResult[] Find(HObject image,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            string subPixel = "true", int numLevels = 0,
            double? angleStart = null, double? angleExtent = null)
        {
            if (!IsCreated) return new HNccMatchResult[0];
            HNccMatchResult[] res = new HNccMatchResult[0];
            SafeRun(() =>
            {
                HTuple rows, cols, angles, scores;
                HOperatorSet.FindNccModel(image, Model,
                    angleStart ?? AngleStart, angleExtent ?? AngleExtent,
                    minScore, numMatches, maxOverlap,
                    subPixel ?? "true", numLevels,
                    out rows, out cols, out angles, out scores);
                double[] ra = rows.DArr, ca = cols.DArr, aa = angles.DArr, sa = scores.DArr;
                res = new HNccMatchResult[ra.Length];
                for (int i = 0; i < ra.Length; i++)
                    res[i] = new HNccMatchResult
                    {
                        Row = ra[i],
                        Column = ca[i],
                        Angle = i < aa.Length ? aa[i] : 0,
                        Score = i < sa.Length ? sa[i] : 0
                    };
            });
            return res;
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>NCC 模板存盘（write_ncc_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteNccModel(Model, filePath));
        }

        /// <summary>读入 NCC 模板（read_ncc_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadNccModel(filePath, out m);
                Model = m;
                // 读回模板内置角度范围
                HTuple n, a0, a1, astep, metric;
                HOperatorSet.GetNccModelParams(Model, out n, out a0, out a1, out astep, out metric);
                AngleStart = a0.D;
                AngleExtent = a1.D;
            });
        }

        /// <summary>销毁 NCC 模板。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearNccModel(Model); } catch { }
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
