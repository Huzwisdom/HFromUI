using HalconDotNet;
using System;
using System.Text;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>OCR 识别结果（字符 + 置信度）。</summary>
    public struct HOcrChar
    {
        /// <summary>识别出的字符。</summary>
        public string ClassName;
        /// <summary>置信度（0~1，越大越可信）。</param>
        public double Confidence;
    }

    /// <summary>
    /// HALCON OCR 光学字符识别封装（read_ocr_class_mlp / do_ocr_multi_class_mlp / do_ocr_single_class_mlp）：
    /// 载入 MLP（或 SVM）训练好的字符分类器，对一组字符区域（Blob 分割得到）批量识别并给置信度，
    /// 可直接拼接成整串文本。字符分割可使用 <see cref="HHalconBlob"/> 的阈值+连通+筛选流程，
    /// 或 HALCON 自带的 OCR 分割辅助算子。句柄由实例持有，Dispose 时 clear_ocr_class_mlp。
    /// 深度学习 OCR（read_ocr_class_cnn，如 pretrained 工业字体模型）需 Deep Learning 授权，
    /// 可在 <see cref="HHalconDeepLearning"/> 中按 DL 模型方式调用。
    /// </summary>
    public class HHalconOcr : HHalconBase
    {
        #region ==================== 字段 ====================

        /// <summary>OCR 分类器句柄。</summary>
        public HTuple Handle { get; private set; } = new HTuple();

        /// <summary>分类器类型："mlp" 或 "svm"。</summary>
        public string ClassifierType { get; private set; } = "mlp";

        /// <summary>是否已加载模型。</summary>
        public bool IsLoaded
        {
            get
            {
                try { return Handle != null && Handle.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 加载/释放 ====================

        /// <summary>
        /// 加载 MLP 字符分类器（read_ocr_class_mlp，.omc 文件）。
        /// HALCON 自带预训练模型如 Industrial_0-9A-Z_NoRej.omc、Document_A-Z+_NoRej.omc 等。
        /// </summary>
        /// <param name="modelFile">分类器文件完整路径。</param>
        public bool LoadMlp(string modelFile)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.ReadOcrClassMlp(modelFile, out h);
                Handle = h;
                ClassifierType = "mlp";
            });
        }

        /// <summary>
        /// 加载 SVM 字符分类器（read_ocr_class_svm，.osc 文件）。
        /// </summary>
        public bool LoadSvm(string modelFile)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.ReadOcrClassSvm(modelFile, out h);
                Handle = h;
                ClassifierType = "svm";
            });
        }

        /// <summary>释放分类器。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsLoaded)
                    {
                        try
                        {
                            if (ClassifierType == "svm") HOperatorSet.ClearOcrClassSvm(Handle);
                            else HOperatorSet.ClearOcrClassMlp(Handle);
                        }
                        catch { }
                        Handle = new HTuple();
                    }
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 识别 ====================

        /// <summary>
        /// 多字符批量识别（do_ocr_multi_class_*）：按字符区域数组顺序返回类别与置信度。
        /// 区域应已按阅读顺序排列（可用 sort_region 'character' 或先做行列排序）。
        /// </summary>
        /// <param name="characters">字符区域集（多个 region 的 HObject 容器）。</param>
        /// <param name="image">字符所在原图（分类器按原图灰度特征判定）。</param>
        public HOcrChar[] Recognize(HObject characters, HObject image)
        {
            if (!IsLoaded || characters == null || image == null) return new HOcrChar[0];
            HOcrChar[] res = new HOcrChar[0];
            SafeRun(() =>
            {
                HTuple classes, confidences;
                if (ClassifierType == "svm")
                {
                    // do_ocr_multi_class_svm 只输出类别，不输出置信度
                    HOperatorSet.DoOcrMultiClassSvm(characters, image, Handle, out classes);
                    confidences = new HTuple();
                }
                else
                    HOperatorSet.DoOcrMultiClassMlp(characters, image, Handle, out classes, out confidences);
                string[] names = classes.SArr;
                double[] conf = confidences.DArr;
                res = new HOcrChar[names.Length];
                for (int i = 0; i < names.Length; i++)
                    res[i] = new HOcrChar
                    {
                        ClassName = names[i],
                        Confidence = i < conf.Length ? conf[i] : 0
                    };
            });
            return res;
        }

        /// <summary>
        /// 批量识别并直接拼接为字符串（字符区域按阅读顺序排列时使用）。
        /// </summary>
        /// <param name="minConfidence">低于该置信度的字符用 '?' 代替（0=不替换）。</param>
        public string ReadText(HObject characters, HObject image, double minConfidence = 0)
        {
            HOcrChar[] arr = Recognize(characters, image);
            StringBuilder sb = new StringBuilder(arr.Length);
            foreach (HOcrChar c in arr)
                sb.Append(minConfidence > 0 && c.Confidence < minConfidence ? "?" : c.ClassName);
            return sb.ToString();
        }

        /// <summary>
        /// 单字符 Top-N 候选识别（do_ocr_single_class_mlp）：返回评分最高的若干候选，
        /// 用于易混字符（0/O、1/I）的二次判断。
        /// </summary>
        /// <param name="character">单个字符区域。</param>
        /// <param name="image">原图。</param>
        /// <param name="topN">候选个数。</param>
        public HOcrChar[] RecognizeSingle(HObject character, HObject image, int topN = 1)
        {
            if (!IsLoaded || ClassifierType != "mlp" || character == null || image == null)
                return new HOcrChar[0];
            HOcrChar[] res = new HOcrChar[0];
            SafeRun(() =>
            {
                HTuple classes, confidences;
                HOperatorSet.DoOcrSingleClassMlp(character, image, Handle, topN,
                    out classes, out confidences);
                string[] names = classes.SArr;
                double[] conf = confidences.DArr;
                res = new HOcrChar[names.Length];
                for (int i = 0; i < names.Length; i++)
                    res[i] = new HOcrChar { ClassName = names[i], Confidence = i < conf.Length ? conf[i] : 0 };
            });
            return res;
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放分类器句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
