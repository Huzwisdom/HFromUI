using System.ComponentModel;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using HFromUI.HAttribute;

namespace HFromUI.HMath
{
    /// <summary>
    /// 动画执行类型
    /// </summary>
    [HDescriptionLanguage("动画执行类型")]
    public enum AnimationRunType
    {
        /// <summary>
        /// 按顺序执行，上一个动画执行完才能执行
        /// </summary>
        InSequence,
        /// <summary>
        /// 立即执行，中断上一个动画后执行
        /// </summary>
        Interrupt
    }

    /// <summary>
    /// 动画类型
    /// </summary>
    [HDescriptionLanguage("动画类型")]
    public enum AnimationType
    {
        /// <summary>
        /// 匀速
        /// </summary>
        UniformMotion,

        /// <summary>
        /// 变速
        /// </summary>
        EaseIn,

        /// <summary>
        /// 变速
        /// </summary>
        EaseOut,

        /// <summary>
        /// 变速
        /// </summary>
        EaseBoth,

        /// <summary>
        /// 收缩
        /// </summary>
        BackIn,

        /// <summary>
        /// 收缩
        /// </summary>
        BackOut,

        /// <summary>
        /// 收缩
        /// </summary>
        BackBoth,

        /// <summary>
        /// 弹球减振 加速
        /// </summary>
        BounceIn,

        /// <summary>
        /// 弹球减振 加速
        /// </summary>
        BounceOut,

        /// <summary>
        /// 弹球减振 加速
        /// </summary>
        BounceBoth,

        /// <summary>
        /// 弹簧来回滑动
        /// </summary>
        ElasticIn,

        /// <summary>
        /// 弹簧来回滑动
        /// </summary>
        ElasticOut,

        /// <summary>
        /// 弹簧来回滑动
        /// </summary>
        ElasticBoth,

        /// <summary>
        /// 二次
        /// </summary>
        QuadraticIn,

        /// <summary>
        /// 二次
        /// </summary>
        QuadraticOut
    }

    /// <summary>
    /// 线性动画函数(算法基本由Silverlight提取出来),来自花木兰动画相关代码
    /// </summary>
    [HDescriptionLanguage("线性动画函数(算法基本由Silverlight提取出来)")]
    public static class AnimationCore
    {
        #region UniformMotion

        /// <summary>
        /// 匀速
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <returns></returns>
        public static double UniformMotion(double origin, double transform, double usedTime, double allTime)
        {
            return origin + transform * UniformMotionCore(usedTime, allTime);
        }

        /// <summary>
        /// 匀速
        /// </summary>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <returns></returns>
        public static double UniformMotionCore(double usedTime, double allTime)
        {
            return usedTime / allTime;
        }

        #endregion UniformMotion

        #region Ease

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseBoth(double origin, double transform, double usedTime, double allTime, double power)
        {
            return origin + transform * EaseBothCore(usedTime / allTime, power);
        }

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseBothCore(double progressTime, double power)
        {
            if (progressTime >= 0.5)
                return (1.0 - EaseInCore((1.0 - progressTime) * 2.0, power)) * 0.5 + 0.5;
            return EaseInCore(progressTime * 2.0, power) * 0.5;
        }

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseIn(double origin, double transform, double usedTime, double allTime, double power)
        {
            return origin + transform * EaseInCore(usedTime / allTime, power);
        }

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseInCore(double progressTime, double power)
        {
            power = Math.Max(0.0, power);
            return Math.Pow(progressTime, power);
        }

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseOut(double origin, double transform, double usedTime, double allTime, double power)
        {
            return origin + transform * (EaseOutCore(usedTime / allTime, power));
        }

        /// <summary>
        /// 变速
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值2)</param>
        /// <returns></returns>
        public static double EaseOutCore(double progressTime, double power)
        {
            return 1.0 - EaseInCore(1.0 - progressTime, power);
        }
        #endregion Ease

        #region Back

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackBoth(double origin, double transform, double usedTime, double allTime, double power, double amplitude)
        {
            return origin + transform * BackBothCore(usedTime / allTime, power, amplitude);
        }

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackBothCore(double progressTime, double power, double amplitude)
        {
            if (progressTime >= 0.5)
                return (1.0 - BackInCore((1.0 - progressTime) * 2.0, power, amplitude)) * 0.5 + 0.5;
            return BackInCore(progressTime * 2.0, power, amplitude) * 0.5;
        }

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackIn(double origin, double transform, double usedTime, double allTime, double power, double amplitude)
        {
            return origin + transform * BackInCore((usedTime / allTime), power, amplitude);
        }

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackInCore(double progressTime, double power, double amplitude)
        {
            amplitude = Math.Max(0.0, amplitude);
            return Math.Pow(progressTime, power) - progressTime * amplitude * Math.Sin(Math.PI * progressTime);
        }

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackOut(double origin, double transform, double usedTime, double allTime, double power, double amplitude)
        {
            return origin + transform * BackOutCore((usedTime / allTime), power, amplitude);
        }

        /// <summary>
        /// 收缩
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="power">动画曲线幂(默认值3)</param>
        /// <param name="amplitude">收缩与相关联的幅度动画。此值必须大于或等于 0。 默认值为 1。</param>
        /// <returns></returns>
        public static double BackOutCore(double progressTime, double power, double amplitude)
        {
            return 1.0 - BackInCore(1.0 - progressTime, power, amplitude);
        }
        #endregion Back

        #region Bounce

        /// <summary>
        /// 弹球减振 加速 弹球减振
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceBoth(double origin, double transform, double usedTime, double allTime, int bounces, double bounciness)
        {
            return origin + transform * BounceBothCore(usedTime / allTime, bounces, bounciness);
        }

        /// <summary>
        /// 弹球减振 加速 弹球减振
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceBothCore(double progressTime, int bounces, double bounciness)
        {
            if (progressTime >= 0.5)
                return (1.0 - BounceInCore((1.0 - progressTime) * 2.0, bounces, bounciness)) * 0.5 + 0.5;
            return BounceInCore(progressTime * 2.0, bounces, bounciness) * 0.5;
        }

        /// <summary>
        /// 弹球减振 加速
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceIn(double origin, double transform, double usedTime, double allTime, int bounces, double bounciness)
        {
            return origin + transform * BounceInCore((usedTime / allTime), bounces, bounciness);
        }

        /// <summary>
        /// 弹球减振 加速
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceInCore(double progressTime, int bounces, double bounciness)
        {
            double y1 = Math.Max(0.0, (double)bounces);
            double num1 = bounciness;
            if (num1 < 1.0 || Math.Abs(num1 - 1.0) < 2.22044604925031E-15)
                num1 = 1001.0 / 1000.0;
            double num2 = Math.Pow(num1, y1);
            double num3 = 1.0 - num1;
            double num4 = (1.0 - num2) / num3 + num2 * 0.5;
            double y2 = Math.Floor(Math.Log(-(progressTime * num4) * (1.0 - num1) + 1.0, num1));
            double y3 = y2 + 1.0;
            double num5 = (1.0 - Math.Pow(num1, y2)) / (num3 * num4);
            double num6 = (1.0 - Math.Pow(num1, y3)) / (num3 * num4);
            double num7 = (num5 + num6) * 0.5;
            double num8 = progressTime - num7;
            double num9 = num7 - num5;
            return -Math.Pow(1.0 / num1, y1 - y2) / (num9 * num9) * (num8 - num9) * (num8 + num9);
        }

        /// <summary>
        /// 加速 弹球减振
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceOut(double origin, double transform, double usedTime, double allTime, int bounces, double bounciness)
        {
            return origin + transform * BounceOutCore((usedTime / allTime), bounces, bounciness);
        }

        /// <summary>
        /// 加速 弹球减振
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="bounces">反弹次数。值必须大于或等于零(默认值为3)</param>
        /// <param name="bounciness">
        /// 指定反弹动画的弹性大小。虽然较高的值都会导致反弹 （弹性较小），此属性中的结果很少或者反弹低值会丢失反弹 （弹性较大） 之间的高度。此值必须是正数(默认值为 2)
        /// </param>
        /// <returns></returns>
        public static double BounceOutCore(double progressTime, int bounces, double bounciness)
        {
            return 1.0 - BounceInCore(1.0 - progressTime, bounces, bounciness);
        }
        #endregion Bounce

        #region Elastic

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticBoth(double origin, double transform, double usedTime, double allTime, int oscillations, double springiness)
        {
            return origin + transform * ElasticBothCore(usedTime / allTime, oscillations, springiness);
        }

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticBothCore(double progressTime, int oscillations, double springiness)
        {
            if (progressTime >= 0.5)
                return (1.0 - ElasticInCore((1.0 - progressTime) * 2.0, oscillations, springiness)) * 0.5 + 0.5;
            return ElasticInCore(progressTime * 2.0, oscillations, springiness) * 0.5;
        }

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticIn(double origin, double transform, double usedTime, double allTime, int oscillations, double springiness)
        {
            return origin + transform * ElasticInCore((usedTime / allTime), oscillations, springiness);
        }

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticInCore(double progressTime, int oscillations, double springiness)
        {
            oscillations = Math.Max(0, oscillations);
            springiness = Math.Max(0.0, springiness);
            return (!(Math.Abs(springiness) < 2.22044604925031E-15) ? (Math.Exp(springiness * progressTime) - 1.0) / (Math.Exp(springiness) - 1.0) : progressTime) * Math.Sin((2.0 * Math.PI * oscillations + Math.PI / 2.0) * progressTime);
        }

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticOut(double origin, double transform, double usedTime, double allTime, int oscillations, double springiness)
        {
            return origin + transform * ElasticOutCore((usedTime / allTime), oscillations, springiness);
        }

        /// <summary>
        /// Elastic
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <param name="oscillations">目标来回滑过动画目标的次数。此值必须大于或等于0 (默认值为3)</param>
        /// <param name="springiness">弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性。一个正数(默认值为3)</param>
        /// <returns></returns>
        public static double ElasticOutCore(double progressTime, int oscillations, double springiness)
        {
            return 1.0 - ElasticInCore(1.0 - progressTime, oscillations, springiness);
        }
        #endregion Elastic

        #region Quadratic

        /// <summary>
        /// 二次
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <returns></returns>
        public static double QuadraticIn(double origin, double transform, double usedTime, double allTime)
        {
            return origin + transform * QuadraticInCore(usedTime / allTime);
        }

        /// <summary>
        /// 二次
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <returns></returns>
        public static double QuadraticInCore(double progressTime)
        {
            return -4 * Math.Pow((progressTime - 0.5), 2.0) + 1.0;
        }

        /// <summary>
        /// 二次
        /// </summary>
        /// <param name="origin">要变换的起始值</param>
        /// <param name="transform">要变换的总值</param>
        /// <param name="usedTime">已进行动画时间</param>
        /// <param name="allTime">总动画时间</param>
        /// <returns></returns>
        public static double QuadraticOut(double origin, double transform, double usedTime, double allTime)
        {
            return origin + transform * QuadraticOutCore(usedTime / allTime);
        }

        /// <summary>
        /// 二次
        /// </summary>
        /// <param name="progressTime">已进行动画时间/总动画时间</param>
        /// <returns></returns>
        public static double QuadraticOutCore(double progressTime)
        {
            if (progressTime >= 0.5)
            {
                return 4 * Math.Pow((progressTime - 0.5 - 0.5), 2.0);
            }
            return 4 * Math.Pow((progressTime), 2.0);
        }

        #endregion Quadratic
    }

    /// <summary>
    /// 动画方法扩展
    /// </summary>
    public static class AnimationExtension
    {
        private static ConcurrentDictionary<IntPtr, HAnimation> ControlsDic = new ConcurrentDictionary<IntPtr, HAnimation>();

        /// <summary>PPAnimation 方法。</summary>
        public static bool PPAnimation(this Control control, Dictionary<string, float> propertyList, int animetime, AnimationType animationType = AnimationType.UniformMotion, AnimationRunType animationRunType = AnimationRunType.InSequence)
        {
            try
            {
                if (control == null)
                    return false;
                if (ControlsDic.ContainsKey(control.Handle))
                {
                    var mulan = ControlsDic[control.Handle];
                    mulan.AnimationType = animationType;
                    mulan.AnimationRunType = animationRunType;
                    return mulan.AnimationControl(propertyList, animetime);
                }
                else
                {
                    var mulan = new HAnimation(control);
                    mulan.AnimationType = animationType;
                    mulan.AnimationRunType = animationRunType;
                    if (!ControlsDic.TryAdd(control.Handle, mulan))
                    {
                        return false;
                    }
                    return mulan.AnimationControl(propertyList, animetime);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>PPAnimation 方法。</summary>
        public static bool PPAnimation(this Control control, Rectangle rect, int animetime, AnimationType animationType = AnimationType.UniformMotion, AnimationRunType animationRunType = AnimationRunType.InSequence)
        {
            try
            {
                Dictionary<string, float> propertyList = new Dictionary<string, float>();
                propertyList.Add("X", rect.X);
                propertyList.Add("Y", rect.Y);
                propertyList.Add("Width", rect.Width);
                propertyList.Add("Height", rect.Height);

                if (control == null)
                    return false;
                if (ControlsDic.ContainsKey(control.Handle))
                {
                    var mulan = ControlsDic[control.Handle];
                    mulan.AnimationType = animationType;
                    mulan.AnimationRunType = animationRunType;
                    return mulan.AnimationControl(propertyList, animetime);
                }
                else
                {
                    var mulan = new HAnimation(control);
                    mulan.AnimationType = animationType;
                    mulan.AnimationRunType = animationRunType;
                    if (!ControlsDic.TryAdd(control.Handle, mulan))
                    {
                        return false;
                    }
                    return mulan.AnimationControl(propertyList, animetime);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static bool PPAnimationStop(this Control control)
        {
            try
            {
                if (control == null)
                    return false;
                if (ControlsDic.ContainsKey(control.Handle))
                {
                    var mulan = ControlsDic[control.Handle];
                    mulan.Stop();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }
    }

    /// <summary>
    /// 动画数据
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// 动画持续时间
        /// </summary>
        public int AnimationTime { get; set; } = 200;

        /// <summary>
        /// 要改变的动画属性数据
        /// </summary>
        public Dictionary<string, float> PropertyList { get; set; }
    }

    /// <summary>
    /// 动画控制类
    /// </summary>
    public class HAnimation : IDisposable
    {

        #region 私有属性

        private Control AnimaControl;
        /// <summary>Animatimer 字段。</summary>
        private System.Timers.Timer Animatimer = null;
        /// <summary>AnimationDatas 字段。</summary>
        private Queue<AnimationData> AnimationDatas = new Queue<AnimationData>();

        /// <summary>isActive 字段。</summary>
        private bool isActive = false;
        /// <summary>isStart 字段。</summary>
        private bool isStart = false;
        /// <summary>
        /// 初始动画参数表
        /// </summary>
        private Dictionary<string, float> OriginalPropertyList = new Dictionary<string, float>();
        /// <summary>
        /// 要执行的目标参数表
        /// </summary>
        private Dictionary<string, float> TargetPropertyList = new Dictionary<string, float>();
        //动画已进行时间
        private int runTime = 0;
        //动画要进行的时间
        private int allTime = 300;
        //动画帧时间ms
        private int animaInterval = 6;
        #endregion

        #region 公共属性
        /// <summary>
        /// 回弹收缩幅度,AnimationType为Back时有效
        /// </summary>
        public double Amplipude = 2.1;

        /// <summary>
        /// 动画执行方式
        /// </summary>
        public AnimationRunType AnimationRunType = AnimationRunType.Interrupt;

        /// <summary>
        /// 动画运动方式
        /// </summary>
        public AnimationType AnimationType = AnimationType.UniformMotion;//动画模式

        //动画执行模式

        /// <summary>
        /// 动画反弹次数,AnimationType为Bounce时有效
        /// </summary>
        public int Bounces = 3;

        /// <summary>
        /// 动画反弹弹性,AnimationType为Bounce时有效
        /// </summary>
        public double Bounciness = 2;

        /// <summary>
        /// 设置动画是否阻塞线程
        /// </summary>
        public bool isBlock = false;

        /// <summary>
        /// 目标来回滑过动画目标的次数。此值必须大于或等于0,AnimationType为Elastic时有效
        /// </summary>
        public int Oscillations = 3;

        /// <summary>
        /// 动画内插指定数字指数,AnimationType为Ease、Back时有效
        /// </summary>
        public int Power = 2;
        /// <summary>
        /// 弹簧铡度。 越小的 Springiness 值为，严格 spring，通过每个振动的强度减小的速度越快弹性,一个正数,AnimationType为Elastic时有效
        /// </summary>
        public double Springiness = 1;
        #endregion

        public HAnimation(Control c)
        {
            AnimaControl = c;
            isActive = true;
            Animatimer = new System.Timers.Timer();
            Animatimer.Interval = animaInterval;
            Animatimer.Elapsed += Animatimer_Tick;
            AnimaControl.Disposed += AnimaControl_Disposed;
            new Thread(delegate () { AnimationRunThread(); }).Start();
        }

        /// <summary>
        /// 自定义属性动画
        /// </summary>
        /// <param name="propertyList">属性名称字符串和对应float类型数值的字典,属性值必须可读可写</param>
        /// <param name="animetime">动画时间。毫秒</param>
        /// <returns>true：动画添加成功；false：动画添加失败</returns>
        public bool AnimationControl(Dictionary<string, float> propertyList, int animetime)
        {
            if (AnimaControl == null || AnimaControl.IsDisposed)
                return false;

            #region 先校验要执行动画的属性及数据是否符合
            var OriginalProperty = new Dictionary<string, float>();
            try
            {
                foreach (string key in propertyList.Keys)
                {
                    float value = 0;
                    if (propertyList.TryGetValue(key, out value))
                    {
                        float originalProperty = 0;
                        if (key == "X" || key == "Y")
                        {
                            originalProperty = Convert.ToInt32(AnimaControl.Location.GetType().GetProperty(key).GetValue(AnimaControl.Location, null));
                        }
                        else if (AnimaControl.GetType().GetProperty(key).PropertyType == typeof(Color))
                        {
                            var v = (Color)(AnimaControl.GetType().GetProperty(key).GetValue(AnimaControl, null));
                            originalProperty = v.ToArgb();
                        }
                        else
                        {
                            var property = AnimaControl.GetType().GetProperty(key);
                            if (property == null || !property.CanRead || !property.CanWrite)
                                return false;
                            originalProperty = Convert.ToInt32(property.GetValue(AnimaControl, null));
                        }

                        OriginalProperty.Add(key, originalProperty);//添加到前属性值里
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;//获取属性值时出错，可能控件无此属性或属性值不可读
            }
            #endregion

            if (!isBlock)
            {
                //立即执行方式
                if (this.AnimationRunType == AnimationRunType.Interrupt)
                {
                    if (isStart)
                    {
                        AnimationDatas.Clear();
                        this.Stop();
                    }
                    AnimationDatas.Enqueue(new AnimationData() { PropertyList = propertyList, AnimationTime = animetime });
                }
                else//加入动画队列，等队列中前面的动画执行完成后再执行
                {
                    AnimationDatas.Enqueue(new AnimationData() { PropertyList = propertyList, AnimationTime = animetime });
                }
            }
            else//阻塞模式
            {
                InitTimer();//初始化动画参数
                allTime = animetime;
                TargetPropertyList = propertyList;
                OriginalPropertyList = OriginalProperty;
                while (runTime <= allTime)
                {
                    Animatimer_Tick(null, null);
                    Thread.Sleep((int)Animatimer.Interval);
                }
            }

            return true;
        }

        /// <summary>
        /// 移动控件到新的矩形大小和位置
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="animetime"></param>
        /// <returns></returns>
        public bool AnimationRectangle(Rectangle rect, int animetime)
        {
            Dictionary<string, float> dic = new Dictionary<string, float>();
            dic.Add("X", rect.X);
            dic.Add("Y", rect.Y);
            dic.Add("Width", rect.Width);
            dic.Add("Height", rect.Height);
            return AnimationControl(dic, animetime);
        }

        public void Dispose()
        {
            isActive = false;
        }

        /// <summary>Stop 方法。</summary>
        public void Stop()
        {
            InitTimer();
            allTime = 0;
            isStart = false;
        }

        /// <summary>AnimaControl_Disposed 方法。</summary>
        private void AnimaControl_Disposed(object sender, EventArgs e)
        {
            Animatimer.Stop();
            isActive = false;
        }

        /// <summary>Animatimer_Tick 方法。</summary>
        private void Animatimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                runTime += (int)Animatimer.Interval;

                if (runTime >= allTime)//若是时间到最后一步确保动画进行到目标位置
                {
                    if (AnimaControl.IsDisposed || AnimaControl == null)
                        return;

                    if (AnimationType == AnimationType.QuadraticOut || AnimationType == AnimationType.QuadraticIn)//二次模式最后会回到初始位置
                    {
                        foreach (string key in OriginalPropertyList.Keys)
                        {
                            float targetValue = 0;
                            OriginalPropertyList.TryGetValue(key, out targetValue);
                            if (key.ToUpper() == "X")
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.Location = new Point((int)targetValue, AnimaControl.Location.Y); }));
                            }
                            else if (key.ToUpper() == "Y")
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.Location = new Point(AnimaControl.Location.X, (int)targetValue); }));
                            }
                            else if (AnimaControl.GetType().GetProperty(key).PropertyType == typeof(Color))
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.GetType().GetProperty(key).SetValue(AnimaControl, Color.FromArgb((int)targetValue), null); }));
                            }
                            else
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.GetType().GetProperty(key).SetValue(AnimaControl, (int)targetValue, null); }));
                            }
                        }
                    }
                    else
                    {
                        foreach (string key in TargetPropertyList.Keys)
                        {
                            float targetValue = 0;
                            TargetPropertyList.TryGetValue(key, out targetValue);
                            if (key.ToUpper() == "X")
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.Location = new Point((int)targetValue, AnimaControl.Location.Y); }));
                            }
                            else if (key.ToUpper() == "Y")
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.Location = new Point(AnimaControl.Location.X, (int)targetValue); }));
                            }
                            else if (AnimaControl.GetType().GetProperty(key).PropertyType == typeof(Color))
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.GetType().GetProperty(key).SetValue(AnimaControl, Color.FromArgb((int)targetValue), null); }));
                            }
                            else
                            {
                                AnimaControl.BeginInvoke(new MethodInvoker(() => { AnimaControl.GetType().GetProperty(key).SetValue(AnimaControl, (int)targetValue, null); }));
                            }
                        }
                    }
                    isStart = false;
                    Animatimer.Stop();
                    return;
                }

                if (TargetPropertyList.Count == 4 && TargetPropertyList.ContainsKey("X") && TargetPropertyList.ContainsKey("Y") && TargetPropertyList.ContainsKey("Width") && TargetPropertyList.ContainsKey("Height"))
                {
                    if (AnimaControl.IsDisposed || AnimaControl == null)
                        return;
                    string[] keys = new string[4] { "X", "Y", "Width", "Height" };
                    float[] targetValue = new float[4] { 0, 0, 0, 0 };
                    float[] originalValue = new float[4] { 0, 0, 0, 0 };
                    float[] transformValue = new float[4] { 0, 0, 0, 0 };
                    double[] currentValue = new double[4] { 0, 0, 0, 0 };
                    for (int i = 0; i < 4; i++)
                    {
                        TargetPropertyList.TryGetValue(keys[i], out targetValue[i]);
                        OriginalPropertyList.TryGetValue(keys[i], out originalValue[i]);//获取初始值
                        transformValue[i] = targetValue[i] - originalValue[i];

                        currentValue[i] = GetCurrentValue(this.AnimationType, originalValue[i], transformValue[i], this.runTime, this.allTime);
                    }

                    if (isBlock)
                    {
                        AnimaControl.Invoke(new MethodInvoker(() =>
                        {
                            AnimaControl.SetBounds((int)currentValue[0], (int)currentValue[1], (int)currentValue[2], (int)currentValue[3]);
                        }));
                    }
                    else
                    {
                        AnimaControl.BeginInvoke(new MethodInvoker(() =>
                        {
                            AnimaControl.SetBounds((int)currentValue[0], (int)currentValue[1], (int)currentValue[2], (int)currentValue[3]);
                        }));
                    }
                }
                else
                {
                    //TargetPropertyList.Keys.AsParallel().ForAll(key =>
                    foreach (var key in TargetPropertyList.Keys)
                    {
                        if (AnimaControl.IsDisposed || AnimaControl == null)
                            return;

                        float targetValue = 0;
                        TargetPropertyList.TryGetValue(key, out targetValue);
                        float originalValue = 0;
                        OriginalPropertyList.TryGetValue(key, out originalValue);//获取初始值
                        float transformValue = targetValue - originalValue;//要变换的总值

                        double currentValue = 0;
                        if (AnimaControl.GetType().GetProperty(key).PropertyType == typeof(Color))
                        {
                            var originalColor = Color.FromArgb((int)originalValue);
                            var targetColor = Color.FromArgb((int)targetValue);

                            var a = (int)GetCurrentValue(this.AnimationType, originalColor.A, targetColor.A - originalColor.A, this.runTime, this.allTime);
                            var r = (int)GetCurrentValue(this.AnimationType, originalColor.R, targetColor.R - originalColor.R, this.runTime, this.allTime);
                            var g = (int)GetCurrentValue(this.AnimationType, originalColor.G, targetColor.G - originalColor.G, this.runTime, this.allTime);
                            var b = (int)GetCurrentValue(this.AnimationType, originalColor.B, targetColor.B - originalColor.B, this.runTime, this.allTime);

                            if (a < 0)
                                a = 0;
                            else if (a > 255)
                                a = 255;
                            if (r < 0)
                                r = 0;
                            else if (r > 255)
                                r = 255;
                            if (g < 0)
                                g = 0;
                            else if (g > 255)
                                g = 255;
                            if (b < 0)
                                b = 0;
                            else if (b > 255)
                                b = 255;

                            currentValue = Color.FromArgb(a, r, g, b).ToArgb();
                        }
                        else
                        {
                            currentValue = GetCurrentValue(this.AnimationType, originalValue, transformValue, this.runTime, this.allTime);
                        }




                        if (isBlock)
                        {
                            AnimaControl.Invoke(new MethodInvoker(() =>
                            {
                                System.Reflection.PropertyInfo propertyInfo = AnimaControl.GetType().GetProperty(key);
                                if (propertyInfo.PropertyType == typeof(int))
                                {
                                    propertyInfo.SetValue(AnimaControl, (int)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(long))
                                {
                                    propertyInfo.SetValue(AnimaControl, (long)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(short))
                                {
                                    propertyInfo.SetValue(AnimaControl, (short)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(float))
                                {
                                    propertyInfo.SetValue(AnimaControl, (float)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(double))
                                {
                                    propertyInfo.SetValue(AnimaControl, currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(decimal))
                                {
                                    propertyInfo.SetValue(AnimaControl, (decimal)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(Color))
                                {
                                    propertyInfo.SetValue(AnimaControl, Color.FromArgb((int)currentValue), null);
                                }
                            }));
                        }
                        else
                        {
                            AnimaControl.BeginInvoke(new MethodInvoker(() =>
                            {
                                System.Reflection.PropertyInfo propertyInfo = AnimaControl.GetType().GetProperty(key);
                                if (propertyInfo.PropertyType == typeof(int))
                                {
                                    propertyInfo.SetValue(AnimaControl, (int)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(long))
                                {
                                    propertyInfo.SetValue(AnimaControl, (long)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(short))
                                {
                                    propertyInfo.SetValue(AnimaControl, (short)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(float))
                                {
                                    propertyInfo.SetValue(AnimaControl, (float)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(double))
                                {
                                    propertyInfo.SetValue(AnimaControl, currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(decimal))
                                {
                                    propertyInfo.SetValue(AnimaControl, (decimal)currentValue, null);
                                }
                                else if (propertyInfo.PropertyType == typeof(Color))
                                {
                                    propertyInfo.SetValue(AnimaControl, Color.FromArgb((int)currentValue), null);
                                }

                            }));
                        }

                    }//);

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>获取 currentValue。</summary>
        private double GetCurrentValue(AnimationType animationType, double originalValue, double transformValue, int runTime, int allTime)
        {
            double currentValue = 0;
            switch (animationType)
            {
                case AnimationType.UniformMotion: currentValue = AnimationCore.UniformMotion(originalValue, transformValue, runTime, allTime); break;
                case AnimationType.EaseIn: currentValue = AnimationCore.EaseIn(originalValue, transformValue, runTime, allTime, Power); break;
                case AnimationType.EaseOut: currentValue = AnimationCore.EaseOut(originalValue, transformValue, runTime, allTime, Power); break;
                case AnimationType.EaseBoth: currentValue = AnimationCore.EaseBoth(originalValue, transformValue, runTime, allTime, Power); break;
                case AnimationType.BackIn: currentValue = AnimationCore.BackIn(originalValue, transformValue, runTime, allTime, Power, Amplipude); break;
                case AnimationType.BackOut: currentValue = AnimationCore.BackOut(originalValue, transformValue, runTime, allTime, Power, Amplipude); break;
                case AnimationType.BackBoth: currentValue = AnimationCore.BackBoth(originalValue, transformValue, runTime, allTime, Power, Amplipude); break;
                case AnimationType.BounceIn: currentValue = AnimationCore.BounceIn(originalValue, transformValue, runTime, allTime, Bounces, Bounciness); break;
                case AnimationType.BounceOut: currentValue = AnimationCore.BounceOut(originalValue, transformValue, runTime, allTime, Bounces, Bounciness); break;
                case AnimationType.BounceBoth: currentValue = AnimationCore.BounceBoth(originalValue, transformValue, runTime, allTime, Bounces, Bounciness); break;
                case AnimationType.ElasticIn: currentValue = AnimationCore.ElasticIn(originalValue, transformValue, runTime, allTime, Oscillations, Springiness); break;
                case AnimationType.ElasticOut: currentValue = AnimationCore.ElasticOut(originalValue, transformValue, runTime, allTime, Oscillations, Springiness); break;
                case AnimationType.ElasticBoth: currentValue = AnimationCore.ElasticBoth(originalValue, transformValue, runTime, allTime, Oscillations, Springiness); break;
                case AnimationType.QuadraticIn: currentValue = AnimationCore.QuadraticIn(originalValue, transformValue, runTime, allTime); break;
                case AnimationType.QuadraticOut: currentValue = AnimationCore.QuadraticOut(originalValue, transformValue, runTime, allTime); break;
            }
            return currentValue;
        }

        /// <summary>
        /// 循环检测是否有动画任务
        /// </summary>
        private void AnimationRunThread()
        {
            while (isActive)
            {
                if (!Animatimer.Enabled && !isStart && !isBlock)//若已停止动画则去检测是否有新的动画任务
                {


                    if (AnimationDatas.Count > 0)//队列取出动画
                    {
                        AnimationData animation = animation = AnimationDatas.Dequeue();

                        if (animation != null)
                        {
                            InitTimer();//初始化动画参数
                            allTime = animation.AnimationTime;
                            TargetPropertyList = animation.PropertyList;


                            try
                            {
                                foreach (string key in animation.PropertyList.Keys)
                                {
                                    float value = 0;
                                    if (animation.PropertyList.TryGetValue(key, out value))//取出定义的控件的属性参数
                                    {
                                        float OriginalProperty = 0;
                                        if (key == "X" || key == "Y")
                                        {
                                            OriginalProperty = (float)Convert.ToInt32(AnimaControl.Location.GetType().GetProperty(key).GetValue(AnimaControl.Location, null));
                                        }
                                        else if (AnimaControl.GetType().GetProperty(key).PropertyType == typeof(Color))
                                        {
                                            var v = (Color)(AnimaControl.GetType().GetProperty(key).GetValue(AnimaControl, null));
                                            OriginalProperty = v.ToArgb();
                                        }
                                        else
                                        {
                                            OriginalProperty = (float)Convert.ToInt32(AnimaControl.GetType().GetProperty(key).GetValue(AnimaControl, null));
                                        }
                                        OriginalPropertyList.Add(key, OriginalProperty);//添加到前属性值里
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            isStart = true;
                            Animatimer.Start();
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 初始化计时器
        /// </summary>
        private void InitTimer()
        {
            if (Animatimer != null)
            {
                Animatimer.Stop();
            }
            else
            {
                Animatimer = new System.Timers.Timer();
                Animatimer.Elapsed += Animatimer_Tick;
            }
            Animatimer.Interval = animaInterval;
            OriginalPropertyList.Clear();
            TargetPropertyList.Clear();
            runTime = 0;
        }
    }
}
