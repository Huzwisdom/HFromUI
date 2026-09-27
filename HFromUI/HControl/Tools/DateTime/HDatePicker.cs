using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.DateTime
{
    using DateTime = System.DateTime;
    /// <summary>
    /// 日期选择控件：显示格式固定 yyyy-MM-dd，数字段可点选编辑（↑↓/滚轮/直接键入），
    /// 右侧下拉箭头展开纯自绘月历（点标题年/月可选年选月、‹›«» 翻页、今日快捷项、悬停反馈、橙色选中日）。
    /// 仅承载日期，不含时分秒。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class HDatePicker : HDateTimeFieldBox
    {
        private DateTime _value = DateTime.Today;
        private HCalendarDrop _drop;

        public HDatePicker()
        {
            Size = new Size(150, 30);
            ShowDropArrow = true;
            Initialize(BuildText, BuildFields, ApplyField);
            DropClicked += (s, e) => ToggleDrop();
        }

        /// <summary>
        /// 是否启用本控件自带的日历弹层。范围控件内嵌两个日期框时关闭，
        /// 改由外层范围控件统一弹单月日历（DropClicked 事件仍会转发）。
        /// </summary>
        [DefaultValue(true)]
        public bool ShowCalendar { get; set; } = true;

        /// <summary>当前日期（时分秒恒为 0）。</summary>
        public DateTime Value
        {
            get => _value;
            set
            {
                value = value.Date;
                if (value == _value) return;
                _value = value;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>日期变化事件。</summary>
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        // 定稿显示串（标准 yyyy-MM-dd，含前导零）；仅在字段逐位编辑期间显示实际键入缓冲
        private string BuildText() => _value.ToString("yyyy-MM-dd");

        /// <summary>yyyy/MM/dd 三段；日期上限随年月实际天数动态给出。</summary>
        private HDateTimeField[] BuildFields()
        {
            return new[]
            {
                new HDateTimeField(0, 4, 1, 9999, false),
                new HDateTimeField(5, 2, 1, 12, false),
                new HDateTimeField(8, 2, 1, DateTime.DaysInMonth(_value.Year, _value.Month), false)
            };
        }

        /// <summary>写回某一段；改变年月后若原日号超过新月天数，自动夹到月末。</summary>
        private void ApplyField(int field, int v)
        {
            int y = _value.Year, m = _value.Month, d = _value.Day;
            if (field == 0) y = v;
            else if (field == 1) m = v;
            else d = v;
            d = Math.Min(d, DateTime.DaysInMonth(y, m));
            _value = new DateTime(y, m, d);
            // 日历弹层打开期间用键盘/滚轮/微调改了值：弹层月份与选中日已陈旧，直接收起
            if (IsPopupOpen) CloseDrop();
            OnValueChanged(EventArgs.Empty);
        }

        /// <summary>
        /// ↑↓ 仿 Windows 日期框跨字段进位：月在 12/1 边界进退位年、日在月末/月初进退位月（年日号自动夹）；
        /// 整体到达 0001-01-01 / 9999-12-31 时停在边界不动（不回绕）。
        /// </summary>
        protected override void BumpField(int field, int delta)
        {
            DateTime nd;
            if (field == 0)
            {
                int y = _value.Year + delta;
                if (y < 1 || y > 9999) return;
                nd = new DateTime(y, _value.Month, Math.Min(_value.Day, DateTime.DaysInMonth(y, _value.Month)));
            }
            else if (field == 1)
            {
                // 年月序号 0001-01 = 12（不是 0），下界必须是 12
                long index = _value.Year * 12L + _value.Month - 1 + delta;
                if (index < 12L || index > 9999L * 12 + 11) return;
                int y = (int)(index / 12), m = (int)(index % 12) + 1;
                nd = new DateTime(y, m, Math.Min(_value.Day, DateTime.DaysInMonth(y, m)));
            }
            else
            {
                long ticks = _value.Ticks + (long)delta * TimeSpan.TicksPerDay;
                if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks) return;
                nd = new DateTime(ticks);
            }
            _value = nd.Date;
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
            _drop.DatePicked += (s, e) => Value = e.Date;
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
