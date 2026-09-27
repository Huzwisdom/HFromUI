using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.DateTime
{
    using DateTime = System.DateTime;
    /// <summary>
    /// 时间选择控件：显示格式固定 HH:mm:ss.fff（时:分:秒.毫秒，24 小时制）。
    /// 数字段点选编辑，支持 ↑/↓、鼠标滚轮与右侧上下微调钮，时/分/秒/毫秒越界自动回绕。
    /// 值以 TimeSpan 表达（00:00:00.000 ~ 23:59:59.999）。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class HTimePicker : HDateTimeFieldBox
    {
        private TimeSpan _value;

        public HTimePicker()
        {
            Size = new Size(150, 30);
            ShowSpin = true;
            _value = TimeSpan.Zero;
            Initialize(BuildText, BuildFields, ApplyField);
        }

        /// <summary>当前时间（一天之内）。</summary>
        public TimeSpan Value
        {
            get => _value;
            set
            {
                value = Clamp(value);
                if (value == _value) return;
                _value = value;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>时间变化事件。</summary>
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        private static TimeSpan Clamp(TimeSpan t)
        {
            if (t < TimeSpan.Zero) return TimeSpan.Zero;
            if (t >= TimeSpan.FromDays(1)) return TimeSpan.FromDays(1).Subtract(TimeSpan.FromMilliseconds(1));
            return TimeSpan.FromMilliseconds(Math.Floor(t.TotalMilliseconds));
        }

        // 定稿显示串（标准 HH:mm:ss.fff，含前导零）；仅在字段逐位编辑期间显示实际键入缓冲
        private string BuildText()
        {
            int h = (int)_value.TotalHours;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                h, _value.Minutes, _value.Seconds, _value.Milliseconds);
        }

        private static HDateTimeField[] BuildFields()
        {
            return new[]
            {
                new HDateTimeField(0, 2, 0, 23, true),   // HH
                new HDateTimeField(3, 2, 0, 59, true),   // mm
                new HDateTimeField(6, 2, 0, 59, true),   // ss
                new HDateTimeField(9, 3, 0, 999, true)   // fff
            };
        }

        /// <summary>写回某一段；小时回绕到 0~23（其余段在字段层回绕，不会产生进位越天）。</summary>
        private void ApplyField(int field, int v)
        {
            int h = (int)_value.TotalHours, m = _value.Minutes, s = _value.Seconds, ms = _value.Milliseconds;
            if (field == 0) h = v;
            else if (field == 1) m = v;
            else if (field == 2) s = v;
            else ms = v;
            _value = new TimeSpan(0, h, m, s, ms);
            OnValueChanged(EventArgs.Empty);
        }

        /// <summary>
        /// ↑↓ 仿 Windows 时间框向高一位进位（分满 60 进时、秒满 60 进分、毫秒满 1000 进秒）；
        /// 值是一天内的 TimeSpan，不跨天：到 00:00:00.000 / 23:59:59.999 封顶封底。
        /// </summary>
        protected override void BumpField(int field, int delta)
        {
            int[] unitMs = { 3600000, 60000, 1000, 1 };
            long ms = (long)_value.TotalMilliseconds + (long)unitMs[field] * delta;
            if (ms < 0) ms = 0;
            if (ms > 24L * 3600 * 1000 - 1) ms = 24L * 3600 * 1000 - 1;
            _value = TimeSpan.FromMilliseconds(ms);
            OnValueChanged(EventArgs.Empty);
        }
    }
}
