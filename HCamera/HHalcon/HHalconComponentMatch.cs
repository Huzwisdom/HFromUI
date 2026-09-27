using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>组件匹配的单个部件实例结果。</summary>
    public struct HComponentPart
    {
        /// <summary>部件在模型中的编号（与建模板 componentRegions 顺序一致）。</summary>
        public int PartIndex;
        /// <summary>部件中心行。</summary>
        public double Row;
        /// <summary>部件中心列。</summary>
        public double Column;
        /// <summary>部件角度（弧度）。</summary>
        public double Angle;
        /// <summary>部件得分。</summary>
        public double Score;
    }

    /// <summary>组件匹配命中的一个完整模型实例（由多个部件按相对关系装配而成）。</summary>
    public struct HComponentMatchResult
    {
        /// <summary>整模型实例得分（所有部件关系匹配综合分，0~1）。</summary>
        public double Score;
        /// <summary>该实例包含的各部件位姿。</summary>
        public HComponentPart[] Parts;
        /// <summary>根部件（root component）中心行。</summary>
        public double RootRow;
        /// <summary>根部件中心列。</summary>
        public double RootColumn;
        /// <summary>根部件角度（弧度）。</summary>
        public double RootAngle;
    }

    /// <summary>组件关系树信息（get_component_model_tree 输出）；Tree/Relations 为 HALCON 对象，用后须 Dispose。</summary>
    public sealed class HComponentTreeInfo : IDisposable
    {
        /// <summary>各部件参考轮廓/区域容器（调用方 Dispose）。</summary>
        public HObject Tree;
        /// <summary>部件间相对关系的连线区域容器（调用方 Dispose）。</summary>
        public HObject Relations;
        /// <summary>关系边的起始部件编号（1 基）。</summary>
        public double[] StartNode;
        /// <summary>关系边的结束部件编号（1 基）。</summary>
        public double[] EndNode;
        /// <summary>关系参考点行。</summary>
        public double[] Row;
        /// <summary>关系参考点列。</summary>
        public double[] Column;
        /// <summary>关系方向角（弧度）。</summary>
        public double[] Phi;
        /// <summary>关系矩形半长 1。</summary>
        public double[] Length1;
        /// <summary>关系矩形半长 2。</summary>
        public double[] Length2;
        /// <summary>各部件搜索起始角（弧度）。</summary>
        public double[] AngleStart;
        /// <summary>各部件搜索角度跨度（弧度）。</summary>
        public double[] AngleExtent;

        /// <summary>释放 Tree/Relations 对象。</summary>
        public void Dispose()
        {
            if (Tree != null) { try { Tree.Dispose(); } catch { } Tree = null; }
            if (Relations != null) { try { Relations.Dispose(); } catch { } Relations = null; }
        }
    }

    /// <summary>训练部件观察结果（get_training_components 输出）；Components 用后须 Dispose。</summary>
    public sealed class HTrainingComponentsView : IDisposable
    {
        /// <summary>训练得到的部件参考轮廓容器（调用方 Dispose）。</summary>
        public HObject Components;
        /// <summary>各观察实例中心行。</summary>
        public double[] Row;
        /// <summary>各观察实例中心列。</summary>
        public double[] Column;
        /// <summary>各观察实例角度（弧度）。</summary>
        public double[] Angle;
        /// <summary>各观察实例得分。</summary>
        public double[] Score;

        /// <summary>释放 Components 对象。</summary>
        public void Dispose()
        {
            if (Components != null) { try { Components.Dispose(); } catch { } Components = null; }
        }
    }

    /// <summary>
    /// HALCON 组件匹配（Component-Based Matching，create_component_model / find_component_model）
    /// 完整封装：目标由多个允许<b>相对运动</b>的部件组成（如铰链、插头+线缆、剪刀开合、
    ///  articulated 机构），算法先找根部件，再在其邻域按训练得到的相对位置/角度关系查找其余部件，
    /// 输出每个完整实例的各部件位姿。
    /// 支持两种建模板方式：
    /// <list type="number">
    /// <item>已知各部件区域：<see cref="Create"/>（create_component_model，模板图+部件区域集一次成型）。</item>
    /// <item>仅有多张不同姿态良品图：<see cref="TrainComponents"/>（train_model_components）
    /// 自动提取/聚类部件，再 <see cref="CreateFromTraining"/>（create_trained_component_model）
    /// 生成模型；训练句柄可 write/read_training_components 存读盘复用。</item>
    /// </list>
    /// 根部件由 root_ranking 自动选择（取排名最高者，也可指定）。
    /// 句柄由实例持有，Dispose 时 clear_component_model（连同训练句柄）。
    /// </summary>
    public class HHalconComponentMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>组件模板句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>建模板后推荐的根部件排名（部件编号数组，越靠前越适合作根）。</summary>
        public int[] RootRanking { get; private set; } = new int[0];

        /// <summary>多张良品训练句柄（train_model_components 输出，create_trained_component_model 的输入）。</summary>
        public HTuple Training { get; private set; } = new HTuple();

        /// <summary>训练句柄是否存在。</summary>
        public bool IsTrained
        {
            get
            {
                try { return Training != null && Training.Length > 0; }
                catch { return false; }
            }
        }

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
        /// 创建组件模型（create_component_model）。
        /// </summary>
        /// <param name="modelImage">模板图（域覆盖全部部件）。</param>
        /// <param name="componentRegions">各部件区域集（多 region 容器，顺序=部件编号 0..n-1）。</param>
        /// <param name="variationRow">部件相对参考位置允许的行偏移（0=刚性）。</param>
        /// <param name="variationColumn">部件相对参考位置允许的列偏移（0=刚性）。</param>
        /// <param name="variationAngle">部件相对参考角度允许的变化（弧度，0=刚性）。</param>
        /// <param name="angleStart">根部件搜索起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">根部件搜索角度跨度（弧度，默认 360°）。</param>
        /// <param name="contrastLowComp">部件特征低对比度阈值（"auto" 或数值）。</param>
        /// <param name="contrastHighComp">部件特征高对比度阈值。</param>
        /// <param name="minSizeComp">部件最小特征尺寸（"auto" 或数值）。</param>
        /// <param name="minContrastComp">查找时部件最小对比度。</param>
        /// <param name="minScoreComp">部件最小得分（0~1）。</param>
        /// <param name="numLevelsComp">部件金字塔层数（"auto"/0 或层数）。</param>
        /// <param name="angleStepComp">部件角度步长（"auto"/0）。</param>
        /// <param name="optimizationComp">"auto"/"none"/"point_reduction_*"。</param>
        /// <param name="metricComp">"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"。</param>
        /// <param name="pregenerationComp">是否预生成搜索空间："none"（默认，省内存）/"exhaustive"。</param>
        public bool Create(HObject modelImage, HObject componentRegions,
            double variationRow = 0, double variationColumn = 0, double variationAngle = 0,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            object contrastLowComp = null, object contrastHighComp = null,
            object minSizeComp = null, object minContrastComp = null,
            double minScoreComp = 0.5, object numLevelsComp = null, object angleStepComp = null,
            string optimizationComp = "none", string metricComp = "use_polarity",
            string pregenerationComp = "none")
        {
            Clear();
            if (modelImage == null || componentRegions == null) return false;
            return SafeRun(() =>
            {
                HTuple id, ranking;
                HOperatorSet.CreateComponentModel(modelImage, componentRegions,
                    variationRow, variationColumn, variationAngle,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    HHalconEnv.ToTuple(contrastLowComp ?? "auto"),
                    HHalconEnv.ToTuple(contrastHighComp ?? "auto"),
                    HHalconEnv.ToTuple(minSizeComp ?? "auto"),
                    HHalconEnv.ToTuple(minContrastComp ?? "auto"),
                    minScoreComp,
                    HHalconEnv.ToTuple(numLevelsComp ?? "auto"),
                    HHalconEnv.ToTuple(angleStepComp ?? "auto"),
                    optimizationComp ?? "none", metricComp ?? "use_polarity",
                    pregenerationComp ?? "none",
                    out id, out ranking);
                Model = id;
                RootRanking = ranking.IArr;
            });
        }

        /// <summary>推荐根部件编号（root_ranking 第一）；未建模板返回 0。</summary>
        public int BestRoot
        {
            get { return RootRanking.Length > 0 ? RootRanking[0] : 0; }
        }

        #endregion

        #region ==================== 多张良品自动训练 ====================

        /// <summary>
        /// 用多张不同姿态良品图自动提取并训练部件关系（train_model_components）。
        /// 典型流程：人工给出初始部件区域（initialComponents，可只在参考图上粗略圈出），
        /// 算法在全部训练图中定位各部件、统计相对运动、聚类出真正的模型部件，
        /// 输出训练句柄（存入 <see cref="Training"/>）与聚类后部件区域，
        /// 随后调用 <see cref="CreateFromTraining"/> 生成可查找的组件模型。
        /// </summary>
        /// <param name="modelImage">参考模板图（初始部件区域在该图上给出）。</param>
        /// <param name="initialComponents">初始部件区域集（粗略圈定，多 region 容器）。</param>
        /// <param name="trainingImages">多张不同姿态良品图（多图容器）。</param>
        /// <param name="contrastLow">部件特征低对比度阈值（"auto" 或数值）。</param>
        /// <param name="contrastHigh">部件特征高对比度阈值。</param>
        /// <param name="minSize">部件最小特征尺寸（"auto" 或数值）。</param>
        /// <param name="minScore">训练图中部件实例的最低得分（0~1，默认 0.5）。</param>
        /// <param name="searchRowTol">部件在训练图间允许的行偏移（0=自动估计）。</param>
        /// <param name="searchColumnTol">部件在训练图间允许的列偏移（0=自动估计）。</param>
        /// <param name="searchAngleTol">部件在训练图间允许的角度变化（弧度，0=自动估计）。</param>
        /// <param name="trainingEmphasis">训练侧重："component"（默认，部件定位准）/"speed"（训练快）。</param>
        /// <param name="ambiguityCriterion">歧义部件判别准则："select_search_tree"（默认，按搜索树可靠性）/"all"。</param>
        /// <param name="maxContourOverlap">部件轮廓允许的最大重叠度（默认 0.2）。</param>
        /// <param name="clusterThreshold">部件聚类阈值 0~1（默认 0.5，越大越容易合并）。</param>
        /// <returns>聚类后的模型部件区域（调用方 Dispose）；失败返回 null。</returns>
        public HObject TrainComponents(HObject modelImage, HObject initialComponents, HObject trainingImages,
            object contrastLow = null, object contrastHigh = null, object minSize = null,
            double minScore = 0.5, double searchRowTol = 0, double searchColumnTol = 0,
            double searchAngleTol = 0, string trainingEmphasis = "component",
            string ambiguityCriterion = "select_search_tree",
            double maxContourOverlap = 0.2, double clusterThreshold = 0.5)
        {
            if (modelImage == null || initialComponents == null || trainingImages == null) return null;
            return SafeRun(delegate
            {
                HObject modelComponents;
                HTuple tid;
                HOperatorSet.TrainModelComponents(modelImage, initialComponents, trainingImages,
                    out modelComponents,
                    HHalconEnv.ToTuple(contrastLow ?? "auto"),
                    HHalconEnv.ToTuple(contrastHigh ?? "auto"),
                    HHalconEnv.ToTuple(minSize ?? "auto"),
                    minScore, searchRowTol, searchColumnTol, searchAngleTol,
                    trainingEmphasis ?? "component",
                    ambiguityCriterion ?? "select_search_tree",
                    maxContourOverlap, clusterThreshold, out tid);
                if (IsTrained)
                {
                    try { HOperatorSet.ClearTrainingComponents(Training); } catch { }
                }
                Training = tid;
                return modelComponents;
            }, (HObject)null);
        }

        /// <summary>
        /// 用新参数对已有训练结果重新聚类部件（cluster_model_components）：
        /// 训练图不变，只调整歧义准则/重叠度/聚类阈值，返回新的模型部件区域（调用方 Dispose）。
        /// 须在 <see cref="TrainComponents"/> 之后、<see cref="CreateFromTraining"/> 之前调用。
        /// </summary>
        public HObject ClusterComponents(HObject trainingImages,
            string ambiguityCriterion = "select_search_tree",
            double maxContourOverlap = 0.2, double clusterThreshold = 0.5)
        {
            if (!IsTrained || trainingImages == null) return null;
            return SafeRun(delegate
            {
                HObject modelComponents;
                HOperatorSet.ClusterModelComponents(trainingImages, out modelComponents, Training,
                    ambiguityCriterion ?? "select_search_tree",
                    maxContourOverlap, clusterThreshold);
                return modelComponents;
            }, (HObject)null);
        }

        /// <summary>
        /// 由训练句柄创建组件模型（create_trained_component_model）。
        /// 模型句柄写入 <see cref="Model"/>，之后即可用 <see cref="Find"/> 查找。
        /// </summary>
        /// <param name="angleStart">根部件搜索起始角（弧度，默认 -180°）。</param>
        /// <param name="angleExtent">根部件搜索角度跨度（弧度，默认 360°）。</param>
        /// <param name="minContrastComp">查找时部件最小对比度（"auto" 或数值）。</param>
        /// <param name="minScoreComp">部件最低得分（0~1）。</param>
        /// <param name="numLevelsComp">金字塔层数（"auto"/0 或层数）。</param>
        /// <param name="angleStepComp">部件角度步长（"auto"/0）。</param>
        /// <param name="optimizationComp">"auto"/"none"/"point_reduction_*"。</param>
        /// <param name="metricComp">"use_polarity"/"ignore_global_polarity"/"ignore_local_polarity"。</param>
        /// <param name="pregenerationComp">"none"（默认，省内存）/"exhaustive"。</param>
        public bool CreateFromTraining(
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            object minContrastComp = null, double minScoreComp = 0.5,
            object numLevelsComp = null, object angleStepComp = null,
            string optimizationComp = "none", string metricComp = "use_polarity",
            string pregenerationComp = "none")
        {
            if (!IsTrained) return false;
            Clear();
            return SafeRun(() =>
            {
                HTuple id, ranking;
                HOperatorSet.CreateTrainedComponentModel(Training,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    HHalconEnv.ToTuple(minContrastComp ?? "auto"),
                    minScoreComp,
                    HHalconEnv.ToTuple(numLevelsComp ?? "auto"),
                    HHalconEnv.ToTuple(angleStepComp ?? "auto"),
                    optimizationComp ?? "none", metricComp ?? "use_polarity",
                    pregenerationComp ?? "none",
                    out id, out ranking);
                Model = id;
                RootRanking = ranking.IArr;
            });
        }

        /// <summary>
        /// 查看训练得到的部件参考形态（get_training_components）。
        /// </summary>
        /// <param name="components">部件选择："all"（默认）或部件编号。</param>
        /// <param name="image">观察来源："all"（全部训练图）/"first"（首张）或图序号。</param>
        /// <param name="markOrientation">是否在轮廓上标出方向。</param>
        public HTrainingComponentsView GetTrainingComponents(
            string components = "all", string image = "all", bool markOrientation = false)
        {
            if (!IsTrained) return null;
            return SafeRun(delegate
            {
                HObject objs;
                HTuple rows, cols, angles, scores;
                HOperatorSet.GetTrainingComponents(out objs, Training,
                    new HTuple(components ?? "all"), new HTuple(image ?? "all"),
                    new HTuple(markOrientation ? "true" : "false"),
                    out rows, out cols, out angles, out scores);
                return new HTrainingComponentsView
                {
                    Components = objs,
                    Row = rows.DArr,
                    Column = cols.DArr,
                    Angle = angles.DArr,
                    Score = scores.DArr
                };
            }, (HTrainingComponentsView)null);
        }

        /// <summary>训练句柄存盘（write_training_components），便于跨会话复用训练结果。</summary>
        public bool SaveTraining(string filePath)
        {
            if (!IsTrained) return false;
            return SafeRun(() => HOperatorSet.WriteTrainingComponents(Training, filePath));
        }

        /// <summary>读入训练句柄（read_training_components）。</summary>
        public bool LoadTraining(string filePath)
        {
            ClearTraining();
            return SafeRun(() =>
            {
                HTuple t;
                HOperatorSet.ReadTrainingComponents(filePath, out t);
                Training = t;
            });
        }

        /// <summary>释放训练句柄（clear_training_components）。</summary>
        public void ClearTraining()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsTrained)
                    {
                        try { HOperatorSet.ClearTrainingComponents(Training); } catch { }
                        Training = new HTuple();
                    }
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 模型参数与关系树 ====================

        /// <summary>
        /// 读模型参数（get_component_model_params）：部件最低得分、根部件排名、
        /// 各部件内部形状模板句柄数组（可用于 get_shape_model_contours 等调试）。
        /// </summary>
        public bool GetModelParams(out double minScoreComp, out int[] rootRanking, out int[] shapeModelIds)
        {
            minScoreComp = 0;
            rootRanking = new int[0];
            shapeModelIds = new int[0];
            if (!IsCreated) return false;
            double ms = minScoreComp;
            int[] rr = rootRanking, sm = shapeModelIds;
            bool ok = SafeRun(() =>
            {
                HTuple m, r, s;
                HOperatorSet.GetComponentModelParams(Model, out m, out r, out s);
                ms = m.Length > 0 ? m.D : 0;
                rr = r.IArr;
                sm = s.IArr;
            });
            minScoreComp = ms;
            rootRanking = rr;
            shapeModelIds = sm;
            return ok;
        }

        /// <summary>
        /// 取部件关系树用于可视化/检查（get_component_model_tree）。
        /// </summary>
        /// <param name="image">建模板用的参考图（关系矩形按该图域裁剪；可传模型图）。</param>
        /// <param name="rootComponent">根部件编号；-1=用 <see cref="BestRoot"/>。</param>
        public HComponentTreeInfo GetTree(HObject image, int rootComponent = -1)
        {
            if (!IsCreated || image == null) return null;
            int root = rootComponent < 0 ? BestRoot : rootComponent;
            return SafeRun(delegate
            {
                HObject tree, relations;
                HTuple sn, en, rows, cols, phi, len1, len2, angS, angE;
                HOperatorSet.GetComponentModelTree(out tree, out relations,
                    Model, root, new HTuple(image.Key),
                    out sn, out en, out rows, out cols, out phi, out len1, out len2,
                    out angS, out angE);
                return new HComponentTreeInfo
                {
                    Tree = tree,
                    Relations = relations,
                    StartNode = sn.DArr,
                    EndNode = en.DArr,
                    Row = rows.DArr,
                    Column = cols.DArr,
                    Phi = phi.DArr,
                    Length1 = len1.DArr,
                    Length2 = len2.DArr,
                    AngleStart = angS.DArr,
                    AngleExtent = angE.DArr
                };
            }, (HComponentTreeInfo)null);
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 查找组件模型（find_component_model）。
        /// </summary>
        /// <param name="image">待查图。</param>
        /// <param name="rootComponent">根部件编号（-1=用 "best_match" 自动选；默认 -1）。</param>
        /// <param name="angleStart">根部件本次搜索起始角。</param>
        /// <param name="angleExtent">根部件本次搜索角度跨度。</param>
        /// <param name="minScore">整模型最低得分。</param>
        /// <param name="numMatches">最多完整实例数（0=全部）。</param>
        /// <param name="maxOverlap">根部件最大重叠度。</param>
        /// <param name="ifRootNotFound">找不到根时："stop"（默认，终止该实例）/"prune_branch"。</param>
        /// <param name="ifComponentNotFound">找不到部件时："prune_branch"（默认，剪枝继续）/"stop"。</param>
        /// <param name="minScoreComp">本次部件最低得分（0=沿用建模板值）。</param>
        /// <param name="subPixelComp">部件亚像素："least_squares"（默认）/"none"。</param>
        /// <param name="numLevelsComp">金字塔层数（0=沿用）。</param>
        /// <param name="greedinessComp">贪婪度 0~1。</param>
        public HComponentMatchResult[] Find(HObject image, int rootComponent = -1,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            string ifRootNotFound = "stop", string ifComponentNotFound = "prune_branch",
            double minScoreComp = 0, string subPixelComp = "least_squares",
            int numLevelsComp = 0, double greedinessComp = 0.9)
        {
            if (!IsCreated || image == null) return new HComponentMatchResult[0];
            HComponentMatchResult[] res = new HComponentMatchResult[0];
            SafeRun(() =>
            {
                HTuple root = rootComponent < 0 ? new HTuple("best_match") : new HTuple(rootComponent);
                HTuple modelStart, modelEnd, instScores;
                HTuple rows, cols, angles, partScores, partIds;
                HOperatorSet.FindComponentModel(image, Model, root,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    minScore, numMatches, maxOverlap,
                    ifRootNotFound ?? "stop", ifComponentNotFound ?? "prune_branch",
                    new HTuple("none"),
                    minScoreComp, subPixelComp ?? "least_squares",
                    numLevelsComp, greedinessComp,
                    out modelStart, out modelEnd, out instScores,
                    out rows, out cols, out angles, out partScores, out partIds);

                double[] sa = instScores.DArr;
                double[] ra = rows.DArr, ca = cols.DArr, aa = angles.DArr;
                double[] psa = partScores.DArr;
                int[] starts = modelStart.IArr, ends = modelEnd.IArr;
                int[] pids = partIds.IArr;
                res = new HComponentMatchResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                {
                    // ModelStart/ModelEnd 为 1 基闭区间
                    int s = starts[i] - 1;
                    int e = ends[i] - 1;
                    int cnt = e - s + 1;
                    if (cnt < 0) cnt = 0;
                    HComponentPart[] parts = new HComponentPart[cnt];
                    for (int k = 0; k < cnt; k++)
                    {
                        int idx = s + k;
                        parts[k] = new HComponentPart
                        {
                            PartIndex = idx < pids.Length ? pids[idx] : idx,
                            Row = idx < ra.Length ? ra[idx] : 0,
                            Column = idx < ca.Length ? ca[idx] : 0,
                            Angle = idx < aa.Length ? aa[idx] : 0,
                            Score = idx < psa.Length ? psa[idx] : 0
                        };
                    }
                    res[i] = new HComponentMatchResult
                    {
                        Score = sa[i],
                        Parts = parts,
                        RootRow = parts.Length > 0 ? parts[0].Row : 0,
                        RootColumn = parts.Length > 0 ? parts[0].Column : 0,
                        RootAngle = parts.Length > 0 ? parts[0].Angle : 0
                    };
                }
            });
            return res;
        }

        /// <summary>
        /// 取某次完整实例的各部件轮廓用于显示（get_found_component_model）。
        /// 输入须为 <see cref="Find"/> 对应次调用的原始输出片段，这里直接重新执行一次查找
        /// 成本高，因此本方法封装为：传入 Find 的图像与同样参数并指定要取轮廓的实例序号
        /// （0 基），返回该实例轮廓容器（调用方 Dispose）。
        /// </summary>
        public HObject GetFoundContours(HObject image, int instanceIndex,
            int rootComponent = -1,
            double angleStart = -Math.PI, double angleExtent = Math.PI * 2,
            double minScore = 0.5, int numMatches = 1, double maxOverlap = 0.5,
            double minScoreComp = 0, double greedinessComp = 0.9)
        {
            if (!IsCreated || image == null) return null;
            return SafeRun(delegate
            {
                HTuple root = rootComponent < 0 ? new HTuple("best_match") : new HTuple(rootComponent);
                HTuple modelStart, modelEnd, instScores;
                HTuple rows, cols, angles, partScores, partIds;
                HOperatorSet.FindComponentModel(image, Model, root,
                    angleStart, angleExtent <= 0 ? Math.PI * 2 : angleExtent,
                    minScore, numMatches, maxOverlap, "stop", "prune_branch",
                    new HTuple("none"), minScoreComp, "least_squares", 0, greedinessComp,
                    out modelStart, out modelEnd, out instScores,
                    out rows, out cols, out angles, out partScores, out partIds);
                HObject found;
                HTuple ri, ci, ai, si;
                HOperatorSet.GetFoundComponentModel(out found, Model,
                    modelStart, modelEnd, rows, cols, angles, partScores, partIds,
                    instanceIndex, 0, out ri, out ci, out ai, out si);
                return found;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>组件模板存盘（write_component_model）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteComponentModel(Model, filePath));
        }

        /// <summary>读入组件模板（read_component_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadComponentModel(filePath, out m);
                Model = m;
            });
        }

        /// <summary>销毁组件模板。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearComponentModel(Model); } catch { }
                        Model = new HTuple();
                    }
                    RootRanking = new int[0];
                }
                catch { }
            }
        }

        /// <summary>释放模板句柄（含训练句柄）。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
            ClearTraining();
        }

        #endregion
    }
}
