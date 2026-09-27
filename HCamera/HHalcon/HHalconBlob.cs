using HalconDotNet;
using System;
using System.Collections.Generic;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// 单个 Blob（连通域）的几何特征结果。
    /// </summary>
    public struct HBlobInfo
    {
        /// <summary>面积（像素²）。</summary>
        public double Area;
        /// <summary>质心行坐标。</summary>
        public double Row;
        /// <summary>质心列坐标。</summary>
        public double Column;
        /// <summary>包围矩形宽。</summary>
        public double Width;
        /// <summary>包围矩形高。</summary>
        public double Height;
        /// <summary>圆度（circularity，0~1，越接近 1 越圆）。</summary>
        public double Circularity;
        /// <summary>矩形度（rectangularity，0~1）。</summary>
        public double Rectangularity;
        /// <summary>紧密度（compactness，越接近 1 越紧致）。</summary>
        public double Compactness;
    }

    /// <summary>
    /// Blob 分析完整封装（实例类，继承 <see cref="HHalconBase"/>）：
    /// 连通（connection）、填孔（fill_up/fill_up_shape）、合并（union1/union2）、
    /// 交差集、形状/灰度筛选（select_shape/select_gray）、形状变换（shape_trans）、
    /// 面积中心与多特征提取（area_center/region_features），以及“阈值→连通→面积过滤→特征输出”
    /// 的一站式 <see cref="Analyze"/>。中间区域对象由实例持有并在 Dispose 时统一释放。
    /// </summary>
    public class HHalconBlob : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>当前工作区域集（连通/筛选结果），null 表示尚未生成。</summary>
        public HObject Regions { get; private set; }

        #endregion

        #region ==================== 连通与填孔 ====================

        /// <summary>把阈值区域拆成独立连通域（connection），结果存入 <see cref="Regions"/>。</summary>
        /// <param name="region">输入区域（threshold/binary_threshold 的输出）。</param>
        public bool Connection(HObject region)
        {
            DisposeRegions();
            return SafeRun(() =>
            {
                HObject conn;
                HOperatorSet.Connection(region, out conn);
                Regions = conn;
            });
        }

        /// <summary>填充区域内孔洞（fill_up）。</summary>
        public static HObject FillUp(HObject region)
        {
            try
            {
                HObject o;
                HOperatorSet.FillUp(region, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 按孔洞形状选择性填孔（fill_up_shape）：只填面积/圆度等落在范围内的孔。
        /// </summary>
        /// <param name="region">输入区域。</param>
        /// <param name="feature">孔洞特征名："area"/"circularity"/"compactness" 等。</param>
        /// <param name="min">特征下限。</param>
        /// <param name="max">特征上限。</param>
        public static HObject FillUpShape(HObject region, string feature = "area",
            double min = 1, double max = 100)
        {
            try
            {
                HObject o;
                HOperatorSet.FillUpShape(region, out o, feature ?? "area", min, max);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 合并与集合运算 ====================

        /// <summary>把多个区域合并为一个区域（union1）。</summary>
        public static HObject Union1(HObject regions)
        {
            try
            {
                HObject o;
                HOperatorSet.Union1(regions, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>两个区域求并集（union2）。</summary>
        public static HObject Union2(HObject r1, HObject r2)
        {
            try
            {
                HObject o;
                HOperatorSet.Union2(r1, r2, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>两个区域求交集（intersection）。</summary>
        public static HObject Intersection(HObject r1, HObject r2)
        {
            try
            {
                HObject o;
                HOperatorSet.Intersection(r1, r2, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>区域差集 r1 - r2（difference）。</summary>
        public static HObject Difference(HObject r1, HObject r2)
        {
            try
            {
                HObject o;
                HOperatorSet.Difference(r1, r2, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 区域形状变换（shape_trans）：凸包、外接矩形、外接圆、平行区域等。
        /// </summary>
        /// <param name="region">输入区域。</param>
        /// <param name="type">"convex"（凸包，默认）/"rectangle1"/"rectangle2"/"inner_circle"/"outer_circle"/"inner_rectangle1"。</param>
        public static HObject ShapeTrans(HObject region, string type = "convex")
        {
            try
            {
                HObject o;
                HOperatorSet.ShapeTrans(region, out o, type ?? "convex");
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 筛选与排序 ====================

        /// <summary>
        /// 按形状特征筛选区域（select_shape），结果直接替换 <see cref="Regions"/>。
        /// </summary>
        /// <param name="feature">特征名："area"/"width"/"height"/"circularity"/"rectangularity"/"row"/"column" 等。</param>
        /// <param name="operation">"and"（区间，默认）/"or"；配合 min/max。</param>
        /// <param name="min">特征下限。</param>
        /// <param name="max">特征上限。</param>
        public bool SelectShape(string feature = "area", string operation = "and",
            double min = 100, double max = 1e9)
        {
            if (Regions == null)
            {
                Error = HTranslation.GetContent("尚无区域集，请先 Connection");
                return false;
            }
            return SafeRun(() =>
            {
                HObject o;
                HOperatorSet.SelectShape(Regions, out o, feature ?? "area", operation ?? "and", min, max);
                HObject old = Regions;
                Regions = o;
                old.Dispose();
            });
        }

        /// <summary>
        /// 按区域在原图上的灰度特征筛选（select_gray）：如 "mean"/"stddev"/"min"/"max"/"deviation"。
        /// 结果替换 <see cref="Regions"/>。
        /// </summary>
        public bool SelectGray(HObject image, string feature = "mean", string operation = "and",
            double min = 0, double max = 255)
        {
            if (Regions == null) return false;
            return SafeRun(() =>
            {
                HObject o;
                HOperatorSet.SelectGray(Regions, image, out o, feature ?? "mean", operation ?? "and", min, max);
                HObject old = Regions;
                Regions = o;
                old.Dispose();
            });
        }

        /// <summary>
        /// 对区域按特征排序（sort_region），返回排序后的新区域集（同时替换 <see cref="Regions"/>）。
        /// </summary>
        /// <param name="mode">"first_point"/"last_point"/"upper_left"/"lower_right"/"character" 等。</param>
        /// <param name="order">"true"=递增，"false"=递减。</param>
        public HObject Sort(string mode = "upper_left", string order = "true")
        {
            if (Regions == null) return null;
            return SafeRun(delegate
            {
                HObject o;
                HOperatorSet.SortRegion(Regions, out o, mode ?? "upper_left", order ?? "true", "row");
                HObject old = Regions;
                Regions = o;
                old.Dispose();
                return o;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 特征提取 ====================

        /// <summary>当前 <see cref="Regions"/> 中区域个数（count_obj）；无区域返回 0。</summary>
        public int Count
        {
            get
            {
                if (Regions == null) return 0;
                try
                {
                    HTuple n;
                    HOperatorSet.CountObj(Regions, out n);
                    return n.I;
                }
                catch (HalconException) { return 0; }
            }
        }

        /// <summary>提取所有区域的面积与质心（area_center），输出三个等长数组；失败返回 false。</summary>
        public bool GetAreaCenter(out double[] areas, out double[] rows, out double[] cols)
        {
            areas = rows = cols = new double[0];
            if (Regions == null) return false;
            double[] la = areas, lr = rows, lc = cols;
            bool ok = SafeRun(() =>
            {
                HTuple a, r, c;
                HOperatorSet.AreaCenter(Regions, out a, out r, out c);
                la = a.DArr;
                lr = r.DArr;
                lc = c.DArr;
            });
            areas = la;
            rows = lr;
            cols = lc;
            return ok;
        }

        /// <summary>
        /// 对当前每个区域计算常用特征集合（area/row/column/width/height/circularity/rectangularity/compactness），
        /// 返回 <see cref="HBlobInfo"/> 列表，顺序与区域索引一致。
        /// </summary>
        public List<HBlobInfo> GetInfos()
        {
            var list = new List<HBlobInfo>();
            if (Regions == null) return list;
            string[] feats = { "area", "row", "column", "width", "height", "circularity", "rectangularity", "compactness" };
            try
            {
                HTuple n;
                HOperatorSet.CountObj(Regions, out n);
                int count = n.I;
                for (int i = 1; i <= count; i++)
                {
                    HObject one;
                    HOperatorSet.SelectObj(Regions, out one, i);
                    try
                    {
                        HTuple vals;
                        HOperatorSet.RegionFeatures(one, new HTuple(feats), out vals);
                        double[] d = vals.DArr;
                        HBlobInfo info = new HBlobInfo();
                        if (d.Length >= 8)
                        {
                            info.Area = d[0];
                            info.Row = d[1];
                            info.Column = d[2];
                            info.Width = d[3];
                            info.Height = d[4];
                            info.Circularity = d[5];
                            info.Rectangularity = d[6];
                            info.Compactness = d[7];
                        }
                        list.Add(info);
                    }
                    finally { one.Dispose(); }
                }
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
            }
            return list;
        }

        /// <summary>取第 index 个区域（select_obj，索引从 1 开始），返回副本由调用方释放。</summary>
        public HObject GetRegion(int index)
        {
            if (Regions == null || index < 1 || index > Count) return null;
            try
            {
                HObject one;
                HOperatorSet.SelectObj(Regions, out one, index);
                return one;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 一站式分析 ====================

        /// <summary>
        /// 一站式 Blob 分析：阈值分割 → 连通 → 面积过滤 →（可选填孔/凸包）→ 提取特征。
        /// </summary>
        /// <param name="image">输入图像（灰度）。</param>
        /// <param name="minGray">阈值下限。</param>
        /// <param name="maxGray">阈值上限。</param>
        /// <param name="minArea">面积过滤下限（&lt;=0 不过滤）。</param>
        /// <param name="maxArea">面积过滤上限。</param>
        /// <param name="fillHoles">true=阈值后先 fill_up 填孔。</param>
        /// <param name="infos">输出各 Blob 特征列表。</param>
        /// <returns>true=流程执行成功（即使没有合格 Blob 也为 true）；false=算子失败。</returns>
        public bool Analyze(HObject image, double minGray, double maxGray,
            double minArea, double maxArea, bool fillHoles, out List<HBlobInfo> infos)
        {
            infos = new List<HBlobInfo>();
            HObject region = null;
            HObject filled = null;
            try
            {
                HOperatorSet.Threshold(image, out region, minGray, maxGray);
                if (fillHoles)
                {
                    HOperatorSet.FillUp(region, out filled);
                    region.Dispose();
                    region = filled;
                    filled = null;
                }
                if (!Connection(region)) return false;
                if (minArea > 0) SelectShape("area", "and", minArea, maxArea);
                infos = GetInfos();
                return Success;
            }
            catch (HalconException hex)
            {
                Error = hex.GetErrorMessage();
                return false;
            }
            finally
            {
                if (region != null) region.Dispose();
            }
        }

        /// <summary>清空当前区域集。</summary>
        public void Clear()
        {
            DisposeRegions();
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放并清空当前区域集。</summary>
        private void DisposeRegions()
        {
            if (Regions != null)
            {
                try { Regions.Dispose(); } catch { }
                Regions = null;
            }
        }

        /// <summary>释放区域资源。</summary>
        protected override void DisposeUnmanaged()
        {
            DisposeRegions();
        }

        #endregion
    }
}
