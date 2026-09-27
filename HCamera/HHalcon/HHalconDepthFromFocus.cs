using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 景深重建（Depth From Focus / Defocus）封装：
    /// 对同一视野沿 Z 轴等距拍摄的多张不同对焦图像（多焦栈，multi-focus stack），
    /// depth_from_focus 逐像素挑选清晰度最大的图层，输出深度图（图层编号/物理深度）与
    /// 置信度图。适合显微视觉、短景深高倍率镜头下的三维形貌与高度差测量。
    /// 本类无状态，全部静态方法，错误信息见 <see cref="LastError"/>。
    /// </summary>
    public static class HHalconDepthFromFocus
    {
        #region ==================== 属性 ====================

        /// <summary>最近一次错误信息（成功为空）。</summary>
        public static string LastError { get; private set; } = string.Empty;

        #endregion

        #region ==================== 景深重建 ====================

        /// <summary>
        /// 从多焦图像栈重建深度（depth_from_focus）。
        /// </summary>
        /// <param name="multiFocusImages">
        /// 多张同视野、不同对焦距离的图像组成的 HObject 容器（通道顺序=由近到远或由远到近，
        /// 顺序决定深度方向；可用 concat_obj 合并）。
        /// </param>
        /// <param name="depth">输出深度图：每像素取最清晰层的索引（乘以层间距即物理高度），调用方 Dispose。</param>
        /// <param name="confidence">输出清晰度置信度图（越大越可信），调用方 Dispose。</param>
        /// <param name="filter">
        /// 清晰度高通滤波器："bandpass"（默认，通用）、"highpass"（对亮纹理敏感）、
        /// "deriche1"/"deriche2" 等。
        /// </param>
        /// <param name="selection">
        /// 深度选择方式："next_max"（默认，在最大响应对间插值，平滑）、"local"（仅取局部最大）、
        /// "only_max"（不插值，最快）。
        /// </param>
        public static bool Compute(HObject multiFocusImages,
            out HObject depth, out HObject confidence,
            string filter = "bandpass", string selection = "next_max")
        {
            depth = null;
            confidence = null;
            LastError = string.Empty;
            if (multiFocusImages == null)
            {
                LastError = HTranslation.GetContent("多焦图像栈为空");
                return false;
            }
            try
            {
                HOperatorSet.DepthFromFocus(multiFocusImages, out depth, out confidence,
                    filter ?? "bandpass", selection ?? "next_max");
                return true;
            }
            catch (HalconException hex)
            {
                LastError = hex.GetErrorMessage();
                return false;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 把图层索引深度图换算成物理深度：物理深度 = 图层索引 × <paramref name="zStep"/>
        /// + <paramref name="z0"/>（等距拍摄时每层的 Z 向移动量与起始深度）。
        /// 返回新图像（scale_image），调用方 Dispose。
        /// </summary>
        public static HObject IndexToPhysicalDepth(HObject depthIndexImage, double z0, double zStep)
        {
            if (depthIndexImage == null) return null;
            try
            {
                HObject phys;
                HOperatorSet.ScaleImage(depthIndexImage, out phys, zStep, z0);
                return phys;
            }
            catch (HalconException hex)
            {
                LastError = hex.GetErrorMessage();
                return null;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
        }

        #endregion
    }
}
