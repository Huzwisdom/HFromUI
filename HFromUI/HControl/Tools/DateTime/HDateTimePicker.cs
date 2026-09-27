using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.DateTime
{
    using DateTime = System.DateTime;
    /// <summary>
    /// 日期时间选择控件（单框）：显示格式固定 yyyy-MM-dd HH:mm:ss.fff（年-月-日 时:分:秒.毫秒）。
    /// 七个数字段均可点选编辑（↑↓/滚轮/直接键入，输满自动跳下一段），右侧下拉箭头展开纯自绘月历
    /// （选日期保留已有时分秒）。时间字段增减越过午夜会自然进/退日期；整体夹在 0001-01-01 与 9999-12-31 之间。
    /// 与 HDatePicker / HTimePicker 同属 HDateTimeFieldBox 分段编辑家族。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class HDateTimePicker : HDateTimeFieldBox
    {
        private DateTime _value = DateTime.Today;
        private HCalendarDrop _drop;

        public HDateTimePicker()
        {
            Size = new Size(250, 30);
            ShowDropArrow = true;
            _value = TruncateToMillisecond(DateTime.Now);
            Initialize(BuildText, BuildFields, ApplyField);
            DropClicked += (s, e) => ToggleDrop();
        }

        /// <summary>是否启用本控件自带的日历弹层（只改日期部分，时分秒保留）。</summary>
        [DefaultValue(true)]
        public bool ShowCalendar { get; set; } = true;

        /// <summary>当前日期时间（毫秒精度）。</summary>
        public DateTime Value
        {
            get => _value;
            set
            {
                value = Normalize(value);
                if (value == _value) return;
                _value = value;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>日期时间变化事件。</summary>
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        private static DateTime TruncateToMillisecond(DateTime v)
            => v.AddTicks(-(v.Ticks % TimeSpan.TicksPerMillisecond));

        private static DateTime Normalize(DateTime v)
        {
            long ticks = v.Ticks - v.Ticks % TimeSpan.TicksPerMillisecond;
            long maxTicks = DateTime.MaxValue.Ticks - (TimeSpan.TicksPerMillisecond - 1);
            if (ticks < DateTime.MinValue.Ticks) ticks = DateTime.MinValue.Ticks;
            if (ticks > maxTicks) ticks = maxTicks;
            return new DateTime(ticks, v.Kind);
        }

        // 定稿显示串（标准 yyyy-MM-dd HH:mm:ss.fff，含前导零）；仅在字段逐位编辑期间显示实际键入缓冲
        private string BuildText() => _value.ToString("yyyy-MM-dd HH:mm:ss.fff");

        /// <summary>yyyy/MM/dd/HH/mm/ss/fff 七段；日期上限随年月实际天数动态给出。</summary>
        private HDateTimeField[] BuildFields()
        {
            return new[]
            {
                new HDateTimeField(0, 4, 1, 9999, false),    // yyyy
                new HDateTimeField(5, 2, 1, 12, false),      // MM
                new HDateTimeField(8, 2, 1, DateTime.DaysInMonth(_value.Year, _value.Month), false), // dd
                new HDateTimeField(11, 2, 0, 23, false),     // HH
                new HDateTimeField(14, 2, 0, 59, false),     // mm
                new HDateTimeField(17, 2, 0, 59, false),     // ss
                new HDateTimeField(20, 3, 0, 999, false)     // fff
            };
        }

        /// <summary>写回某一段：直接键入值按字段范围夹取；改变年月后原日号超过新月天数时自动夹到月末。</summary>
        private void ApplyField(int field, int v)
        {
            int y = _value.Year, mo = _value.Month, d = _value.Day;
            int h = _value.Hour, mi = _value.Minute, s = _value.Second, ms = _value.Millisecond;
            switch (field)
            {
                case 0: y = v; break;
                case 1: mo = v; break;
                case 2: d = v; break;
                case 3: h = v; break;
                case 4: mi = v; break;
                case 5: s = v; break;
                default: ms = v; break;
            }
            d = Math.Min(d, DateTime.DaysInMonth(y, mo));
            _value = new DateTime(y, mo, d, h, mi, s, ms, _value.Kind);
            // 日历弹层打开期间用键盘/滚轮改了值：弹层月份与选中日已陈旧，直接收起
            if (IsPopupOpen) CloseDrop();
            OnValueChanged(EventArgs.Empty);
        }

        /// <summary>
        /// ↑↓ 仿 Windows 日期时间框：月在 12/1 边界进退位年、日按天进退（自动跨月）；
        /// 时/分/秒/毫秒按毫秒单位整体增减——越过午夜自然进/退日期。
        /// 到达 0001-01-01 00:00:00.000 / 9999-12-31 23:59:59.999 时停在边界不动。
        /// </summary>
        protected override void BumpField(int field, int delta)
        {
            DateTime nd;
            if (field == 0)
            {
                int y = _value.Year + delta;
                if (y < 1 || y > 9999) return;
                nd = new DateTime(y, _value.Month,
                    Math.Min(_value.Day, DateTime.DaysInMonth(y, _value.Month)),
                    _value.Hour, _value.Minute, _value.Second, _value.Millisecond, _value.Kind);
            }
            else if (field == 1)
            {
                // 年月序号 0001-01 = 12（不是 0），下界必须是 12
                long index = _value.Year * 12L + _value.Month - 1 + delta;
                if (index < 12L || index > 9999L * 12 + 11) return;
                int y = (int)(index / 12), m = (int)(index % 12) + 1;
                nd = new DateTime(y, m, Math.Min(_value.Day, DateTime.DaysInMonth(y, m)),
                    _value.Hour, _value.Minute, _value.Second, _value.Millisecond, _value.Kind);
            }
            else if (field == 2)
            {
                // 日按天进退（自动跨月/跨年）
                long ticks = _value.Ticks + (long)delta * TimeSpan.TicksPerDay;
                if (ticks < DateTime.MinValue.Ticks ||
                    ticks > DateTime.MaxValue.Ticks - (TimeSpan.TicksPerMillisecond - 1)) return;
                nd = new DateTime(ticks, _value.Kind);
            }
            else
            {
                // 时/分/秒/毫秒按对应毫秒单位整体平移；越过午夜自然进/退日期
                long[] unitMs = { 0, 0, 0, 3600000L, 60000L, 1000L, 1L };
                long ticks = _value.Ticks + (long)delta * unitMs[field] * TimeSpan.TicksPerMillisecond;
                if (ticks < DateTime.MinValue.Ticks ||
                    ticks > DateTime.MaxValue.Ticks - (TimeSpan.TicksPerMillisecond - 1)) return;
                nd = new DateTime(ticks, _value.Kind);
            }
            _value = TruncateToMillisecond(nd);
            // 与 ApplyField 一致：弹层开着时改值直接收层
            if (IsPopupOpen) CloseDrop();
            OnValueChanged(EventArgs.Empty);
        }

        protected override bool OnDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape && IsPopupOpen)
            {
                CloseDrop();
                return true;
            }
            if (keyData == Keys.Enter && IsPopupOpen)
            {
                CloseDrop();
                return true;
            }
            return base.OnDialogKey(keyData);
        }

        #region 日历弹出层

        private void ToggleDrop()
        {
            if (!ShowCalendar) return;
            if (IsPopupOpen) CloseDrop();
            else OpenDrop();
        }

        private void OpenDrop()
        {
            CloseDrop();
            _drop = new HCalendarDrop(false, _value, _value);
            // 日历只定日期部分：选中日与当前时分秒合并
            _drop.DatePicked += (s, e) => Value = e.Date.Date.Add(_value.TimeOfDay);
            _drop.PopupClosed += (s, e) =>
            {
                var d = _drop;
                _drop = null;
                IsPopupOpen = false;
                // 回调执行在弹层自己的调用栈上，借弹层句柄延后释放，避免重入 Dispose
                if (d != null) d.BeginInvoke(new Action(() => d.Dispose()));
            };
            _drop.ShowPopup(this, _drop.Size);
            IsPopupOpen = true;
        }

        private void CloseDrop()
        {
            var d = _drop;
            if (d != null) d.ClosePopup(); // PopupClosed 回调会置空 _drop 并安排释放
            IsPopupOpen = false;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            CloseDrop();
            base.OnHandleDestroyed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) CloseDrop();
            base.Dispose(disposing);
        }

        #endregion
    }
}
