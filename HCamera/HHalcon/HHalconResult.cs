using HalconDotNet;
using System;
using System.Diagnostics;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>结果级别。</summary>
    public enum HHalconResultLevel
    {
        /// <summary>调试/信息。</summary>
        Info = 0,
        /// <summary>成功。</summary>
        Success = 1,
        /// <summary>警告（不影响结果但需关注）。</summary>
        Warning = 2,
        /// <summary>失败/错误。</summary>
        Error = 3
    }

    /// <summary>
    /// HHalcon 全链路统一结果输出对象：封装成功与否、消息、来源模块/算子、耗时、时间戳、
    /// 附加数值数据与可视化对象，风格对齐 HCamera 相机事件参数，便于 UI 层直接显示与日志记录。
    /// 泛型版 <see cref="HHalconResult{T}"/> 可携带强类型结果（区域、数组、位姿等）。
    /// </summary>
    [DebuggerDisplay("{Level} {Source} {Message} ({ElapsedMs:F2}ms)")]
    public class HHalconResult
    {
        #region ==================== 属性 ====================

        /// <summary>是否成功（级别 ≤ Warning）。</summary>
        public bool Ok
        {
            get { return Level != HHalconResultLevel.Error; }
        }

        /// <summary>结果级别。</summary>
        public HHalconResultLevel Level { get; set; } = HHalconResultLevel.Success;

        /// <summary>来源模块或算子名（如 "HHalconBlob.Analyze"）。</summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>结果/错误消息（中文）。</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>执行耗时（毫秒）。</summary>
        public double ElapsedMs { get; set; }

        /// <summary>结果产生时间。</summary>
        public DateTime Time { get; set; } = DateTime.Now;

        /// <summary>附加数值/字符串元组数据（测量值、位姿、面积等，可空）。</summary>
        public HTuple Data { get; set; }

        /// <summary>
        /// 附加可视化对象（结果区域/XLD/图像，可空，用于窗口叠加显示）。
        /// 注意：仅作引用传递，所有权归创建方，本对象不负责 Dispose。
        /// </summary>
        public HObject Visual { get; set; }

        /// <summary>任意调用方附加对象。</summary>
        public object Tag { get; set; }

        #endregion

        #region ==================== 工厂方法 ====================

        /// <summary>构造成功结果。</summary>
        public static HHalconResult Succeed(string source, string message = "",
            double elapsedMs = 0, HTuple data = null, HObject visual = null)
        {
            return new HHalconResult
            {
                Level = HHalconResultLevel.Success,
                Source = source ?? string.Empty,
                Message = string.IsNullOrEmpty(message)
                    ? HTranslation.GetContent("执行成功") : message,
                ElapsedMs = elapsedMs,
                Data = data,
                Visual = visual
            };
        }

        /// <summary>构造失败结果。</summary>
        public static HHalconResult Fail(string source, string error,
            double elapsedMs = 0, object tag = null)
        {
            return new HHalconResult
            {
                Level = HHalconResultLevel.Error,
                Source = source ?? string.Empty,
                Message = error ?? string.Empty,
                ElapsedMs = elapsedMs,
                Tag = tag
            };
        }

        /// <summary>构造警告结果。</summary>
        public static HHalconResult Warn(string source, string message,
            double elapsedMs = 0, HTuple data = null)
        {
            return new HHalconResult
            {
                Level = HHalconResultLevel.Warning,
                Source = source ?? string.Empty,
                Message = message ?? string.Empty,
                ElapsedMs = elapsedMs,
                Data = data
            };
        }

        /// <summary>转中文摘要行（日志/界面直接可用）。</summary>
        public override string ToString()
        {
            string head;
            switch (Level)
            {
                case HHalconResultLevel.Success: head = HTranslation.GetContent("成功"); break;
                case HHalconResultLevel.Warning: head = HTranslation.GetContent("警告"); break;
                case HHalconResultLevel.Error: head = HTranslation.GetContent("失败"); break;
                default: head = HTranslation.GetContent("信息"); break;
            }
            return string.IsNullOrEmpty(Source)
                ? string.Format("[{0}] {1} ({2:F2} ms)", head, Message, ElapsedMs)
                : string.Format("[{0}][{1}] {2} ({3:F2} ms)", head, Source, Message, ElapsedMs);
        }

        #endregion
    }

    /// <summary>
    /// 带强类型返回值的统一结果。
    /// </summary>
    /// <typeparam name="T">结果值类型（如 HObject、double[]、HShapeMatchResult[]）。</typeparam>
    public class HHalconResult<T> : HHalconResult
    {
        /// <summary>结果值（失败时为默认值）。</summary>
        public T Value { get; set; }

        /// <summary>构造成功结果并携带值。</summary>
        public static HHalconResult<T> Succeed(string source, T value, string message = "",
            double elapsedMs = 0, HObject visual = null)
        {
            return new HHalconResult<T>
            {
                Level = HHalconResultLevel.Success,
                Source = source ?? string.Empty,
                Message = string.IsNullOrEmpty(message)
                    ? HTranslation.GetContent("执行成功") : message,
                ElapsedMs = elapsedMs,
                Value = value,
                Visual = visual
            };
        }

        /// <summary>构造失败结果。</summary>
        public static HHalconResult<T> Fail(string source, string error,
            double elapsedMs = 0, T value = default(T))
        {
            return new HHalconResult<T>
            {
                Level = HHalconResultLevel.Error,
                Source = source ?? string.Empty,
                Message = error ?? string.Empty,
                ElapsedMs = elapsedMs,
                Value = value
            };
        }
    }

    /// <summary>
    /// 轻量计时器：using 包裹算子调用，结束时读取 <see cref="ElapsedMs"/> 填入结果对象。
    /// </summary>
    public sealed class HHalconTimer : IDisposable
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly string _source;

        /// <summary>来源名（写入结果的 Source）。</summary>
        public string Source
        {
            get { return _source; }
        }

        /// <summary>当前已计时毫秒。</summary>
        public double ElapsedMs
        {
            get { return _sw.Elapsed.TotalMilliseconds; }
        }

        /// <summary>创建并立即开始计时。</summary>
        /// <param name="source">来源模块/算子名。</param>
        public HHalconTimer(string source)
        {
            _source = source ?? string.Empty;
        }

        /// <summary>停止计时。</summary>
        public void Stop()
        {
            if (_sw.IsRunning) _sw.Stop();
        }

        /// <summary>以当前耗时构造成功结果。</summary>
        public HHalconResult<T> ToSuccess<T>(T value, string message = "", HObject visual = null)
        {
            Stop();
            return HHalconResult<T>.Succeed(_source, value, message, ElapsedMs, visual);
        }

        /// <summary>以当前耗时构造失败结果。</summary>
        public HHalconResult<T> ToFailure<T>(string error, T value = default(T))
        {
            Stop();
            return HHalconResult<T>.Fail(_source, error, ElapsedMs, value);
        }

        /// <summary>以当前耗时构造无类型成功结果。</summary>
        public HHalconResult ToSuccess(string message = "", HTuple data = null)
        {
            Stop();
            return HHalconResult.Succeed(_source, message, ElapsedMs, data);
        }

        /// <summary>停止计时。</summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
