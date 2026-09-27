using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>1D 测量检出的单边边缘信息（measure_pos）。</summary>
    public struct HMeasureEdge
    {
        /// <summary>边缘点行坐标。</summary>
        public double Row;
        /// <summary>边缘点列坐标。</summary>
        public double Column;
        /// <summary>边缘处灰度变化幅度（越大越锐利）。</summary>
        public double Amplitude;
        /// <summary>与前一个边缘的间距（第一个为 0）。</summary>
        public double Distance;
    }

    /// <summary>1D 测量检出的边缘对信息（measure_pairs，用于测宽/测径）。</summary>
    public struct HMeasurePair
    {
        /// <summary>第一条边（入口边）行。</summary>
        public double RowFirst;
        /// <summary>第一条边列。</summary>
        public double ColumnFirst;
        /// <summary>第一条边幅度。</summary>
        public double AmplitudeFirst;
        /// <summary>第二条边（出口边）行。</summary>
        public double RowSecond;
        /// <summary>第二条边列。</summary>
        public double ColumnSecond;
        /// <summary>第二条边幅度。</summary>
        public double AmplitudeSecond;
        /// <summary>对内宽度（入口到出口距离，即被测宽度）。</summary>
        public double IntraDistance;
        /// <summary>与前一对的对间距离。</summary>
        public double InterDistance;
    }

    /// <summary>
    /// HALCON 一维测量完整封装（gen_measure_rectangle2 / gen_measure_arc + measure_pos / measure_pairs）：
    /// 在矩形测量弧或圆弧内提取亚像素边缘，完成长度、宽度、内外径、间距、边位置测量。
    /// 句柄由实例持有，Dispose 时自动 close_measure。
    /// </summary>
    public class HHalconMeasure : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>测量句柄（gen_measure_* 返回）。</summary>
        public HTuple Handle { get; private set; } = new HTuple();

        /// <summary>测量对象是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return Handle != null && Handle.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 创建测量对象 ====================

        /// <summary>
        /// 创建矩形 2D 测量弧（gen_measure_rectangle2）：矩形中心线为测量方向，
        /// 半宽 length2 构成边缘搜索带。
        /// </summary>
        /// <param name="row">中心行。</param>
        /// <param name="column">中心列。</param>
        /// <param name="phi">长边方向角（弧度）。</param>
        /// <param name="length1">半长（沿测量方向）。</param>
        /// <param name="length2">半宽（搜索带宽度的一半）。</param>
        /// <param name="width">图像宽。</param>
        /// <param name="height">图像高。</param>
        /// <param name="interpolation">"nearest neighbor"（默认，快）/"bilinear"/"bicubic"。</param>
        public bool CreateRectangle2(double row, double column, double phi,
            double length1, double length2, int width, int height,
            string interpolation = "nearest neighbor")
        {
            Close();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.GenMeasureRectangle2(row, column, phi, length1, length2,
                    width, height, interpolation ?? "nearest neighbor", out h);
                Handle = h;
            });
        }

        /// <summary>
        /// 创建圆弧测量对象（gen_measure_arc）：沿指定圆弧搜索边缘，适合圆周方向测量。
        /// </summary>
        /// <param name="centerRow">圆心行。</param>
        /// <param name="centerCol">圆心列。</param>
        /// <param name="radius">圆弧半径。</param>
        /// <param name="angleStart">起始角（弧度）。</param>
        /// <param name="angleExtent">角度范围（弧度）。</param>
        /// <param name="annulusRadius">搜索环宽度的一半。</param>
        /// <param name="width">图像宽。</param>
        /// <param name="height">图像高。</param>
        /// <param name="interpolation">插值方式。</param>
        public bool CreateArc(double centerRow, double centerCol, double radius,
            double angleStart, double angleExtent, double annulusRadius,
            int width, int height, string interpolation = "nearest neighbor")
        {
            Close();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.GenMeasureArc(centerRow, centerCol, radius, angleStart, angleExtent,
                    annulusRadius, width, height, interpolation ?? "nearest neighbor", out h);
                Handle = h;
            });
        }

        /// <summary>关闭测量对象（close_measure）。</summary>
        public void Close()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.CloseMeasure(Handle); } catch { }
                        Handle = new HTuple();
                    }
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 边缘提取 ====================

        /// <summary>
        /// 提取测量带内全部单边缘（measure_pos）。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="sigma">高斯平滑系数（常用 1.0）。</param>
        /// <param name="threshold">边缘幅度阈值（迟滞用单阈值，常用 20~80）。</param>
        /// <param name="transition">"all"（全部，默认）/"positive"（暗→亮）/"negative"（亮→暗）。</param>
        /// <param name="select">"all"（全部）/"first"/"last"。</param>
        /// <param name="edges">输出边缘数组。</param>
        /// <returns>true=成功（无边缘也返回 true，数组长度为 0）。</returns>
        public bool Pos(HObject image, double sigma, double threshold,
            string transition, string select, out HMeasureEdge[] edges)
        {
            edges = new HMeasureEdge[0];
            if (!IsCreated)
            {
                Error = HTranslation.GetContent("测量对象尚未创建");
                return false;
            }
            HMeasureEdge[] result = edges;
            bool ok = SafeRun(() =>
            {
                HTuple rows, cols, amps, dists;
                HOperatorSet.MeasurePos(image, Handle, sigma, threshold,
                    transition ?? "all", select ?? "all",
                    out rows, out cols, out amps, out dists);
                double[] ra = rows.DArr, ca = cols.DArr, aa = amps.DArr, da = dists.DArr;
                result = new HMeasureEdge[ra.Length];
                for (int i = 0; i < ra.Length; i++)
                    result[i] = new HMeasureEdge
                    {
                        Row = ra[i], Column = ca[i], Amplitude = aa[i],
                        Distance = i < da.Length ? da[i] : 0
                    };
            });
            edges = result;
            return ok;
        }

        /// <summary>
        /// 提取边缘对（measure_pairs）：自动把成对的入口/出口边配对，直接给出宽度（IntraDistance）。
        /// 适合测量工件宽度、孔径、缝隙等。参数语义同 <see cref="Pos"/>。
        /// </summary>
        /// <param name="transition">
        /// "all"（默认）/"positive"/"negative"/"uniform"（成对边必须同向，抗干扰更强）。
        /// </param>
        public bool Pairs(HObject image, double sigma, double threshold,
            string transition, string select, out HMeasurePair[] pairs)
        {
            pairs = new HMeasurePair[0];
            if (!IsCreated)
            {
                Error = HTranslation.GetContent("测量对象尚未创建");
                return false;
            }
            HMeasurePair[] result = pairs;
            bool ok = SafeRun(() =>
            {
                HTuple r1, c1, a1, r2, c2, a2, intra, inter;
                HOperatorSet.MeasurePairs(image, Handle, sigma, threshold,
                    transition ?? "all", select ?? "all",
                    out r1, out c1, out a1, out r2, out c2, out a2, out intra, out inter);
                double[] x1 = r1.DArr, y1 = c1.DArr, z1 = a1.DArr;
                double[] x2 = r2.DArr, y2 = c2.DArr, z2 = a2.DArr;
                double[] ia = intra.DArr, ta = inter.DArr;
                result = new HMeasurePair[x1.Length];
                for (int i = 0; i < x1.Length; i++)
                    result[i] = new HMeasurePair
                    {
                        RowFirst = x1[i], ColumnFirst = y1[i], AmplitudeFirst = z1[i],
                        RowSecond = x2[i], ColumnSecond = y2[i], AmplitudeSecond = z2[i],
                        IntraDistance = i < ia.Length ? ia[i] : 0,
                        InterDistance = i < ta.Length ? ta[i] : 0
                    };
            });
            pairs = result;
            return ok;
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>关闭测量句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Close();
        }

        #endregion
    }
}
