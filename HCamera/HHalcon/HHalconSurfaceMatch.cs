using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>3D 表面匹配命中结果（7 元素位姿 + 得分 + 结果句柄）。</summary>
    public struct HSurfaceMatchResult
    {
        /// <summary>目标在场景坐标系下的 3D 位姿（[Tx,Ty,Tz,Rx,Ry,Rz,Order]，平移单位与点云一致）。</summary>
        public double[] Pose;
        /// <summary>匹配得分（0~1）。</summary>
        public double Score;
        /// <summary>匹配结果句柄（用于 get_surface_matching_result 取关键点/引用点等明细）。</summary>
        public HTuple ResultHandle;
    }

    /// <summary>
    /// HALCON 3D 表面匹配（Surface-Based 3D Matching）完整封装：
    /// 读入 3D 物体模型（PLY/OM3 等，read_object_model_3d）、从模型生成表面模板（create_surface_model）、
    /// 在场景 3D 模型/点云中查找六自由度位姿（find_surface_model）、取匹配明细
    /// （get_surface_matching_result：pose、对应点、关键点）以及高精度位姿精修
    /// （refine_surface_model_pose）。模板可存盘复用。3D 点云/立体视觉重建见 <see cref="HHalcon3D"/>；
    /// 可变形表面匹配使用 deformable_surface_model（create_deformable_surface_model），需要时在此扩展。
    /// </summary>
    public class HHalconSurfaceMatch : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>表面模板句柄。</summary>
        public HTuple Model { get; private set; } = new HTuple();

        /// <summary>模板对应的 3D 物体模型句柄（读入时持有，Dispose 时一并释放）。</summary>
        public HTuple ObjectModel { get; private set; } = new HTuple();

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
        /// 从 3D 模型文件读物体并创建表面模板。
        /// </summary>
        /// <param name="modelFile">3D 模型路径（.ply/.om3/.obj 等 HALCON 支持格式）。</param>
        /// <param name="relSamplingDistance">
        /// 表面采样相对距离（模型直径的比例，0=不抽稀，常用 0.03~0.05；越小关键点越密、越慢）。
        /// </param>
        /// <param name="scale">读入单位缩放（1=不缩放）。</param>
        /// <param name="genParamNames">create_surface_model 附加参数名（可空）。</param>
        /// <param name="genParamValues">附加参数值（与名称一一对应，可空）。</param>
        public bool Create(string modelFile, double relSamplingDistance = 0.03,
            double scale = 1.0, string[] genParamNames = null, double[] genParamValues = null)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple obj, status;
                HOperatorSet.ReadObjectModel3d(modelFile, scale, new HTuple(), new HTuple(),
                    out obj, out status);
                ObjectModel = obj;

                HTuple names = genParamNames != null ? new HTuple(genParamNames) : new HTuple();
                HTuple vals = genParamValues != null ? new HTuple(genParamValues) : new HTuple();
                HTuple m;
                HOperatorSet.CreateSurfaceModel(obj, relSamplingDistance, names, vals, out m);
                Model = m;
            });
        }

        /// <summary>直接用已有 3D 物体模型句柄创建表面模板（如立体重建结果）。</summary>
        public bool Create(HTuple objectModel3D, double relSamplingDistance = 0.03,
            string[] genParamNames = null, double[] genParamValues = null)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple names = genParamNames != null ? new HTuple(genParamNames) : new HTuple();
                HTuple vals = genParamValues != null ? new HTuple(genParamValues) : new HTuple();
                HTuple m;
                HOperatorSet.CreateSurfaceModel(objectModel3D, relSamplingDistance, names, vals, out m);
                Model = m;
                // 外部传入的对象模型不由本类释放
                ObjectModel = new HTuple();
            });
        }

        #endregion

        #region ==================== 查找 ====================

        /// <summary>
        /// 在场景 3D 模型中查找目标（find_surface_model）。
        /// </summary>
        /// <param name="sceneObjectModel">场景点云/3D 模型句柄。</param>
        /// <param name="relSamplingDistance">场景采样相对距离（一般与建模板一致）。</param>
        /// <param name="keyPointFraction">使用关键点比例（0~1，0.5 较快，越小越快）。</param>
        /// <param name="minScore">最低得分（常用 0.5 以上）。</param>
        /// <param name="returnResultHandle">"true"=保留结果句柄以便取对应点明细，"false"=不保留（更快）。</param>
        /// <remarks>需要限定返回个数时，可用 genParam "num_matches" 传参（本封装固定传空，默认返回全部达分匹配）。</remarks>
        public HSurfaceMatchResult[] Find(HTuple sceneObjectModel,
            double relSamplingDistance = 0.03, double keyPointFraction = 0.8,
            double minScore = 0.5, string returnResultHandle = "true")
        {
            if (!IsCreated) return new HSurfaceMatchResult[0];
            HSurfaceMatchResult[] res = new HSurfaceMatchResult[0];
            SafeRun(() =>
            {
                HTuple poses, scores, resultIDs;
                HOperatorSet.FindSurfaceModel(Model, sceneObjectModel,
                    relSamplingDistance, keyPointFraction, minScore,
                    returnResultHandle ?? "true", new HTuple(), new HTuple(),
                    out poses, out scores, out resultIDs);

                // poses 为 numMatches×7 的扁平元组
                double[] pa = poses.DArr;
                double[] sa = scores.DArr;
                int n = sa.Length;
                res = new HSurfaceMatchResult[n];
                for (int i = 0; i < n; i++)
                {
                    double[] pose = new double[7];
                    Array.Copy(pa, i * 7, pose, 0, Math.Min(7, pa.Length - i * 7));
                    res[i] = new HSurfaceMatchResult
                    {
                        Pose = pose,
                        Score = sa[i],
                        ResultHandle = resultIDs != null && resultIDs.Length > i
                            ? resultIDs.TupleSelect(i) : new HTuple()
                    };
                }
            });
            return res;
        }

        /// <summary>
        /// 对粗匹配位姿做稠密精修（refine_surface_model_pose），返回精修后的 7 元素位姿。
        /// 精修采样距离由表面模型自身配置决定；如需更密点云可重建表面模型时调小 relSamplingDistance。
        /// </summary>
        /// <param name="sceneObjectModel">场景 3D 模型。</param>
        /// <param name="pose">find_surface_model 给出的粗位姿。</param>
        /// <param name="minScore">精修收敛判定得分。</param>
        public double[] RefinePose(HTuple sceneObjectModel, double[] pose, double minScore = 0.7)
        {
            if (!IsCreated || pose == null) return null;
            double[] r = null;
            SafeRun(() =>
            {
                HTuple refinedPose, score, resultID;
                HOperatorSet.RefineSurfaceModelPose(Model, sceneObjectModel,
                    new HTuple(pose), minScore, "true",
                    new HTuple(), new HTuple(), out refinedPose, out score, out resultID);
                r = refinedPose.DArr;
                try
                {
                    if (resultID != null && resultID.Length > 0)
                        HOperatorSet.ClearSurfaceMatchingResult(resultID);
                }
                catch { }
            });
            return r;
        }

        /// <summary>
        /// 取某次匹配的明细（get_surface_matching_result）：
        /// resultName 常用 "pose"（位姿）、"sampled_scene"（采样场景点）、"key_points_ref"
        /// （模板关键点）、"key_points_unmatched"。返回扁平 HTuple 转 double 数组。
        /// </summary>
        /// <param name="resultHandle">命中结果中的 ResultHandle。</param>
        /// <param name="resultName">明细项名称。</param>
        /// <param name="resultIndex">明细索引（一般 0）。</param>
        public double[] GetResultValue(HTuple resultHandle, string resultName, int resultIndex = 0)
        {
            double[] r = new double[0];
            SafeRun(() =>
            {
                HTuple v;
                HOperatorSet.GetSurfaceMatchingResult(resultHandle, resultName ?? "pose", resultIndex, out v);
                r = v.DArr;
            });
            return r;
        }

        #endregion

        #region ==================== 持久化与销毁 ====================

        /// <summary>表面模板存盘（write_surface_model，.sfm）。</summary>
        public bool Save(string filePath)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.WriteSurfaceModel(Model, filePath));
        }

        /// <summary>从文件读表面模板（read_surface_model）。</summary>
        public bool Load(string filePath)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple m;
                HOperatorSet.ReadSurfaceModel(filePath, out m);
                Model = m;
            });
        }

        /// <summary>销毁表面模板及本类读入的 3D 物体模型。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearSurfaceModel(Model); } catch { }
                        Model = new HTuple();
                    }
                    if (ObjectModel != null && ObjectModel.Length > 0)
                    {
                        try { HOperatorSet.ClearObjectModel3d(ObjectModel); } catch { }
                        ObjectModel = new HTuple();
                    }
                }
                catch { }
            }
        }

        /// <summary>释放模板资源。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
