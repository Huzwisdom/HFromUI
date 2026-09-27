using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HFromUI; // 引入 HTranslation

namespace HFromUI.HConvert
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全网最全的日期时间转换与计算工具类（终极增强版，支持自定义格式、自然语言解析、时间序列生成、高精度时间统计，集成多语言翻译）。
    /// 提供字符串解析、格式化输出、时间戳互转、日期计算、时区处理、工作日与季度、
    /// 自然语言相对日期（中/英）、工作日计数、日期范围生成、时间差描述、
    /// 按任意时间间隔生成时间序列、统计天数/小时数/分钟数、
    /// 以及基于事件时间戳/状态变化的高精度时间统计（默认100ms粒度）等全面功能。
    /// 可通过 <see cref="AddDateTimeFormat"/> 等方法动态管理自定义解析格式。
    /// 所有方法均异常安全，返回可空类型或默认值，不会抛出异常。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HDateTime
    {

        #region 自定义日期时间格式列表

        /// <summary>CommonDateTimeFormats 字段。</summary>
        private static readonly List<string> CommonDateTimeFormats = new List<string>
        {
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss",
            "yyyy.MM.dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss",
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "dd/MM/yyyy", "MM/dd/yyyy",
            "yyyyMMdd", HTranslation.GetContent("yyyy年M月d日"), HTranslation.GetContent("yyyy年MM月dd日"), HTranslation.GetContent("M月d日"), "MM-dd",
            "dd-MMM-yyyy", "dd-MMM-yy", "HH:mm:ss", "HH:mm", "hh:mm:ss tt", "hh:mm tt",
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff", "yyyyMMddTHHmmssZ",
            "yyyyMMddHHmmss", "yyyyMMddHHmm",
        };

        /// <summary>UnixEpoch 字段。</summary>
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region 自定义格式管理

        /// <summary>添加一个自定义解析格式。</summary>
        public static void AddDateTimeFormat(string format)
        {
            if (!string.IsNullOrWhiteSpace(format) && !CommonDateTimeFormats.Contains(format))
                CommonDateTimeFormats.Add(format);
        }

        /// <summary>移除一个解析格式。</summary>
        public static void RemoveDateTimeFormat(string format) => CommonDateTimeFormats.Remove(format);

        /// <summary>清空所有解析格式（慎用）。</summary>
        public static void ClearDateTimeFormats() => CommonDateTimeFormats.Clear();

        /// <summary>重置为默认格式列表。</summary>
        public static void ResetDateTimeFormats()
        {
            CommonDateTimeFormats.Clear();
            CommonDateTimeFormats.AddRange(new[]
            {
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss",
                "yyyy.MM.dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss",
                "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "dd/MM/yyyy", "MM/dd/yyyy",
                "yyyyMMdd", HTranslation.GetContent("yyyy年M月d日"), HTranslation.GetContent("yyyy年MM月dd日"), HTranslation.GetContent("M月d日"), "MM-dd",
                "dd-MMM-yyyy", "dd-MMM-yy", "HH:mm:ss", "HH:mm", "hh:mm:ss tt", "hh:mm tt",
                "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff", "yyyyMMddTHHmmssZ",
                "yyyyMMddHHmmss", "yyyyMMddHHmm",
            });
        }

        /// <summary>获取当前所有格式的只读列表。</summary>
        public static ReadOnlyCollection<string> GetDateTimeFormats() => CommonDateTimeFormats.AsReadOnly();

        #endregion

        #region 字符串解析

        /// <summary>
        /// 将字符串解析为 <see cref="DateTime"/>。依次尝试：自然语言 → 系统默认解析 → 自定义格式列表 →
        /// 中文月日格式 → Unix 时间戳 → 去空格重试。失败返回 <paramref name="defaultValue"/>。
        /// </summary>
        public static DateTime Parse(string input, DateTime defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;
            input = input.Trim();

            DateTime result;
            if (TryParseNaturalLanguage(input, out result)) return result;
            if (DateTime.TryParse(input, out result)) return result;
            if (DateTime.TryParseExact(input, CommonDateTimeFormats.ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) return result;
            if (TryParseChineseMonthDay(input, out result)) return result;
            if (TryParseUnixTimestamp(input, out result)) return result;

            string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
            if (cleaned != input)
            {
                if (DateTime.TryParse(cleaned, out result)) return result;
                if (DateTime.TryParseExact(cleaned, CommonDateTimeFormats.ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) return result;
            }

            return defaultValue;
        }

        /// <summary>尝试解析，返回是否成功。</summary>
        public static bool TryParse(string input, out DateTime result)
        {
            result = Parse(input, DateTime.MinValue);
            return result != DateTime.MinValue;
        }

        /// <summary>解析标准格式日期（yyyy-MM-dd 等）。</summary>
        public static DateTime? ParseStandard(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string[] fmts = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd", "yyyy/MM/dd HH:mm:ss", "yyyy.MM.dd", "yyyy.MM.dd HH:mm:ss" };
            if (DateTime.TryParseExact(input, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)) return result;
            return null;
        }

        /// <summary>解析中文日期（如“2023年12月5日”）。</summary>
        public static DateTime? ParseChinese(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string[] fmts = { "yyyy年M月d日", "yyyy年MM月dd日", "yyyy年M月d日 HH:mm:ss", "yyyy年MM月dd日 HH:mm:ss" };
            if (DateTime.TryParseExact(input, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)) return result;
            return null;
        }

        /// <summary>解析“月日”格式（如“12月5日”），自动补全年份。</summary>
        public static DateTime? ParseMonthDay(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            DateTime result;
            return TryParseChineseMonthDay(input, out result) ? result : (DateTime?)null;
        }

        /// <summary>解析 Unix 时间戳（秒/毫秒）。</summary>
        public static DateTime? ParseUnixTimestamp(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            DateTime result;
            return TryParseUnixTimestamp(input, out result) ? result : (DateTime?)null;
        }

        #endregion

        #region 格式化输出

        public static string ToString(DateTime dateTime) => dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public static string ToString(DateTime dateTime, string format) => dateTime.ToString(format);
        /// <summary>转换为 Iso8601。</summary>
        public static string ToIso8601(DateTime dateTime) => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        /// <summary>友好相对时间（如“刚刚”、“3分钟前”等，已翻译）。</summary>
        public static string ToFriendlyString(DateTime dateTime)
        {
            TimeSpan span = DateTime.Now - dateTime;
            if (span.TotalSeconds < 0) return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            if (span.TotalSeconds < 60) return HTranslation.GetContent("刚刚");
            if (span.TotalMinutes < 60) return (int)span.TotalMinutes + HTranslation.GetContent("分钟前");
            if (span.TotalHours < 24) return (int)span.TotalHours + HTranslation.GetContent("小时前");
            if (span.TotalDays < 30) return (int)span.TotalDays + HTranslation.GetContent("天前");
            if (span.TotalDays < 365) return (int)(span.TotalDays / 30) + HTranslation.GetContent("个月前");
            return (int)(span.TotalDays / 365) + HTranslation.GetContent("年前");
        }

        /// <summary>中文星期名称（已翻译）。</summary>
        public static string GetChineseDayOfWeek(DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday: return HTranslation.GetContent("星期日");
                case DayOfWeek.Monday: return HTranslation.GetContent("星期一");
                case DayOfWeek.Tuesday: return HTranslation.GetContent("星期二");
                case DayOfWeek.Wednesday: return HTranslation.GetContent("星期三");
                case DayOfWeek.Thursday: return HTranslation.GetContent("星期四");
                case DayOfWeek.Friday: return HTranslation.GetContent("星期五");
                case DayOfWeek.Saturday: return HTranslation.GetContent("星期六");
                default: return "";
            }
        }

        /// <summary>生肖（已翻译）。</summary>
        public static string GetChineseZodiac(DateTime dateTime)
        {
            string[] zodiacs = { HTranslation.GetContent("鼠"), HTranslation.GetContent("牛"), HTranslation.GetContent("虎"), HTranslation.GetContent("兔"), HTranslation.GetContent("龙"), HTranslation.GetContent("蛇"), HTranslation.GetContent("马"), HTranslation.GetContent("羊"), HTranslation.GetContent("猴"), HTranslation.GetContent("鸡"), HTranslation.GetContent("狗"), HTranslation.GetContent("猪") };
            return HTranslation.GetContent(zodiacs[(dateTime.Year - 4) % 12]);
        }

        /// <summary>星座（已翻译）。</summary>
        public static string GetConstellation(DateTime dateTime)
        {
            int m = dateTime.Month, d = dateTime.Day;
            if ((m == 3 && d >= 21) || (m == 4 && d <= 19)) return HTranslation.GetContent("白羊座");
            if ((m == 4 && d >= 20) || (m == 5 && d <= 20)) return HTranslation.GetContent("金牛座");
            if ((m == 5 && d >= 21) || (m == 6 && d <= 21)) return HTranslation.GetContent("双子座");
            if ((m == 6 && d >= 22) || (m == 7 && d <= 22)) return HTranslation.GetContent("巨蟹座");
            if ((m == 7 && d >= 23) || (m == 8 && d <= 22)) return HTranslation.GetContent("狮子座");
            if ((m == 8 && d >= 23) || (m == 9 && d <= 22)) return HTranslation.GetContent("处女座");
            if ((m == 9 && d >= 23) || (m == 10 && d <= 23)) return HTranslation.GetContent("天秤座");
            if ((m == 10 && d >= 24) || (m == 11 && d <= 22)) return HTranslation.GetContent("天蝎座");
            if ((m == 11 && d >= 23) || (m == 12 && d <= 21)) return HTranslation.GetContent("射手座");
            if ((m == 12 && d >= 22) || (m == 1 && d <= 19)) return HTranslation.GetContent("摩羯座");
            if ((m == 1 && d >= 20) || (m == 2 && d <= 18)) return HTranslation.GetContent("水瓶座");
            return HTranslation.GetContent("双鱼座");
        }

        #endregion

        #region 时间戳互转

        /// <summary>转换为 UnixTimestamp。</summary>
        public static long ToUnixTimestamp(DateTime dateTime) => (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        /// <summary>转换为 UnixTimestampMilliseconds。</summary>
        public static long ToUnixTimestampMilliseconds(DateTime dateTime) => (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        /// <summary>从 UnixTimestamp 创建实例。</summary>
        public static DateTime FromUnixTimestamp(long seconds) => UnixEpoch.AddSeconds(seconds).ToLocalTime();
        /// <summary>从 UnixTimestampMilliseconds 创建实例。</summary>
        public static DateTime FromUnixTimestampMilliseconds(long milliseconds) => UnixEpoch.AddMilliseconds(milliseconds).ToLocalTime();
        /// <summary>CurrentUnixTimestamp 方法。</summary>
        public static long CurrentUnixTimestamp() => ToUnixTimestamp(DateTime.Now);
        /// <summary>CurrentUnixTimestampMilliseconds 方法。</summary>
        public static long CurrentUnixTimestampMilliseconds() => ToUnixTimestampMilliseconds(DateTime.Now);

        #endregion

        #region 日期组件获取

        /// <summary>获取 year。</summary>
        public static int GetYear(DateTime dateTime) => dateTime.Year;
        /// <summary>获取 month。</summary>
        public static int GetMonth(DateTime dateTime) => dateTime.Month;
        /// <summary>获取 day。</summary>
        public static int GetDay(DateTime dateTime) => dateTime.Day;
        /// <summary>获取 hour。</summary>
        public static int GetHour(DateTime dateTime) => dateTime.Hour;
        /// <summary>获取 minute。</summary>
        public static int GetMinute(DateTime dateTime) => dateTime.Minute;
        /// <summary>获取 second。</summary>
        public static int GetSecond(DateTime dateTime) => dateTime.Second;
        /// <summary>获取 millisecond。</summary>
        public static int GetMillisecond(DateTime dateTime) => dateTime.Millisecond;
        /// <summary>获取 dayOfWeek。</summary>
        public static DayOfWeek GetDayOfWeek(DateTime dateTime) => dateTime.DayOfWeek;
        /// <summary>获取 dayOfYear。</summary>
        public static int GetDayOfYear(DateTime dateTime) => dateTime.DayOfYear;
        /// <summary>获取 date。</summary>
        public static DateTime GetDate(DateTime dateTime) => dateTime.Date;

        #endregion

        #region 日期计算

        /// <summary>获取 startOfDay。</summary>
        public static DateTime GetStartOfDay(DateTime dateTime) => dateTime.Date;
        /// <summary>获取 endOfDay。</summary>
        public static DateTime GetEndOfDay(DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);
        /// <summary>获取 firstDayOfWeek。</summary>
        public static DateTime GetFirstDayOfWeek(DateTime dateTime)
        {
            int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dateTime.AddDays(-diff).Date;
        }
        /// <summary>获取 lastDayOfWeek。</summary>
        public static DateTime GetLastDayOfWeek(DateTime dateTime) => GetFirstDayOfWeek(dateTime).AddDays(6);
        /// <summary>获取 firstDayOfMonth。</summary>
        public static DateTime GetFirstDayOfMonth(int year, int month) => new DateTime(year, month, 1);
        /// <summary>获取 lastDayOfMonth。</summary>
        public static DateTime GetLastDayOfMonth(int year, int month) => new DateTime(year, month, DateTime.DaysInMonth(year, month));
        /// <summary>获取 age。</summary>
        public static int GetAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }
        /// <summary>判断是否 LeapYear。</summary>
        public static bool IsLeapYear(int year) => DateTime.IsLeapYear(year);
        /// <summary>获取 daysInMonth。</summary>
        public static int GetDaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

        /// <summary>AddYears 方法。</summary>
        public static DateTime AddYears(DateTime dateTime, int years) => dateTime.AddYears(years);
        /// <summary>AddMonths 方法。</summary>
        public static DateTime AddMonths(DateTime dateTime, int months) => dateTime.AddMonths(months);
        /// <summary>AddDays 方法。</summary>
        public static DateTime AddDays(DateTime dateTime, int days) => dateTime.AddDays(days);
        /// <summary>AddHours 方法。</summary>
        public static DateTime AddHours(DateTime dateTime, int hours) => dateTime.AddHours(hours);
        /// <summary>AddMinutes 方法。</summary>
        public static DateTime AddMinutes(DateTime dateTime, int minutes) => dateTime.AddMinutes(minutes);
        /// <summary>AddSeconds 方法。</summary>
        public static DateTime AddSeconds(DateTime dateTime, int seconds) => dateTime.AddSeconds(seconds);
        /// <summary>AddMilliseconds 方法。</summary>
        public static DateTime AddMilliseconds(DateTime dateTime, double milliseconds) => dateTime.AddMilliseconds(milliseconds);

        #endregion

        #region 特殊日期：周末、工作日、季度

        /// <summary>判断是否 Weekend。</summary>
        public static bool IsWeekend(DateTime dateTime) => dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        /// <summary>判断是否 Workday。</summary>
        public static bool IsWorkday(DateTime dateTime) => !IsWeekend(dateTime);
        /// <summary>获取 nextWorkday。</summary>
        public static DateTime GetNextWorkday(DateTime dateTime)
        {
            DateTime next = dateTime.AddDays(1);
            while (IsWeekend(next)) next = next.AddDays(1);
            return next;
        }
        /// <summary>获取 firstDayOfQuarter。</summary>
        public static DateTime GetFirstDayOfQuarter(DateTime dateTime)
        {
            int quarterMonth = ((dateTime.Month - 1) / 3) * 3 + 1;
            return new DateTime(dateTime.Year, quarterMonth, 1);
        }
        /// <summary>获取 lastDayOfQuarter。</summary>
        public static DateTime GetLastDayOfQuarter(DateTime dateTime) => GetFirstDayOfQuarter(dateTime).AddMonths(3).AddDays(-1);

        /// <summary>计算两个日期之间的工作日数量。</summary>
        public static int GetWorkdayCount(DateTime start, DateTime end)
        {
            int count = 0;
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
                if (IsWorkday(date)) count++;
            return count;
        }

        /// <summary>获取日期范围列表（按天）。</summary>
        public static List<DateTime> GetDateRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
                list.Add(date);
            return list;
        }

        #endregion

        #region 时间序列生成

        /// <summary>按小时生成时间序列，从 start 到 end，每次递增1小时。</summary>
        public static List<DateTime> GetHourRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            for (DateTime dt = start; dt <= end; dt = dt.AddHours(1))
                list.Add(dt);
            return list;
        }

        /// <summary>按分钟生成时间序列，从 start 到 end，每次递增1分钟。</summary>
        public static List<DateTime> GetMinuteRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            for (DateTime dt = start; dt <= end; dt = dt.AddMinutes(1))
                list.Add(dt);
            return list;
        }

        /// <summary>按秒生成时间序列，从 start 到 end，每次递增1秒。</summary>
        public static List<DateTime> GetSecondRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            for (DateTime dt = start; dt <= end; dt = dt.AddSeconds(1))
                list.Add(dt);
            return list;
        }

        /// <summary>按月生成时间序列，返回每月1日。</summary>
        public static List<DateTime> GetMonthRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            DateTime firstDay = new DateTime(start.Year, start.Month, 1);
            DateTime lastDay = new DateTime(end.Year, end.Month, 1);
            for (DateTime dt = firstDay; dt <= lastDay; dt = dt.AddMonths(1))
                list.Add(dt);
            return list;
        }

        /// <summary>按年生成时间序列，返回每年1月1日。</summary>
        public static List<DateTime> GetYearRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            int startYear = start.Year;
            int endYear = end.Year;
            for (int y = startYear; y <= endYear; y++)
                list.Add(new DateTime(y, 1, 1));
            return list;
        }

        /// <summary>按周生成时间序列，返回每周一。</summary>
        public static List<DateTime> GetWeekStartRange(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            DateTime firstMonday = GetFirstDayOfWeek(start);
            DateTime lastMonday = GetFirstDayOfWeek(end);
            for (DateTime dt = firstMonday; dt <= lastMonday; dt = dt.AddDays(7))
                list.Add(dt);
            return list;
        }

        /// <summary>按自定义时间间隔生成时间序列。</summary>
        public static List<DateTime> GenerateTimeSeries(DateTime start, DateTime end, TimeSpan interval)
        {
            var list = new List<DateTime>();
            if (interval <= TimeSpan.Zero) return list;
            for (DateTime dt = start; dt <= end; dt = dt.Add(interval))
                list.Add(dt);
            return list;
        }

        #endregion

        #region 时间统计

        /// <summary>计算两个日期之间的天数（包含起止日期）。</summary>
        public static int CountDays(DateTime start, DateTime end)
        {
            if (end < start) return 0;
            return (end.Date - start.Date).Days + 1;
        }

        /// <summary>计算两个时间之间的小时数（向上取整）。</summary>
        public static int CountHours(DateTime start, DateTime end)
        {
            if (end < start) return 0;
            return (int)Math.Ceiling((end - start).TotalHours);
        }

        /// <summary>计算两个时间之间的分钟数（向上取整）。</summary>
        public static int CountMinutes(DateTime start, DateTime end)
        {
            if (end < start) return 0;
            return (int)Math.Ceiling((end - start).TotalMinutes);
        }

        /// <summary>计算两个时间之间的秒数（向上取整）。</summary>
        public static int CountSeconds(DateTime start, DateTime end)
        {
            if (end < start) return 0;
            return (int)Math.Ceiling((end - start).TotalSeconds);
        }

        #endregion

        #region 时间差

        /// <summary>获取 timeSpan。</summary>
        public static TimeSpan GetTimeSpan(DateTime start, DateTime end) => end - start;

        /// <summary>获取详细时间差描述（如“2天3小时15分钟”，已翻译）。</summary>
        public static string GetDetailedTimeSpan(DateTime start, DateTime end)
        {
            TimeSpan span = end - start;
            if (span.TotalSeconds < 0) span = -span;

            List<string> parts = new List<string>();
            if (span.Days > 0) parts.Add(span.Days + HTranslation.GetContent("天"));
            if (span.Hours > 0) parts.Add(span.Hours + HTranslation.GetContent("小时"));
            if (span.Minutes > 0) parts.Add(span.Minutes + HTranslation.GetContent("分钟"));
            if (span.Seconds > 0 || parts.Count == 0) parts.Add(span.Seconds + HTranslation.GetContent("秒"));
            return string.Join("", parts);
        }

        /// <summary>获取简单友好时间差（已翻译）。</summary>
        public static string GetFriendlyTimeSpan(DateTime start, DateTime end)
        {
            TimeSpan span = end - start;
            if (span.TotalSeconds < 0) span = -span;

            if (span.TotalDays >= 365) return (int)(span.TotalDays / 365) + HTranslation.GetContent("年");
            if (span.TotalDays >= 30) return (int)(span.TotalDays / 30) + HTranslation.GetContent("个月");
            if (span.TotalDays >= 1) return (int)span.TotalDays + HTranslation.GetContent("天");
            if (span.TotalHours >= 1) return (int)span.TotalHours + HTranslation.GetContent("小时");
            if (span.TotalMinutes >= 1) return (int)span.TotalMinutes + HTranslation.GetContent("分钟");
            return (int)span.TotalSeconds + HTranslation.GetContent("秒");
        }

        #endregion

        #region 时区

        /// <summary>转换为 Utc。</summary>
        public static DateTime ToUtc(DateTime dateTime) => dateTime.ToUniversalTime();
        /// <summary>从 Utc 创建实例。</summary>
        public static DateTime FromUtc(DateTime utcDateTime) => utcDateTime.ToLocalTime();
        /// <summary>获取 timeInZone。</summary>
        public static DateTime GetTimeInZone(string timeZoneId)
        {
            try
            {
                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTime(DateTime.Now, tzi);
            }
            catch { return DateTime.MinValue; }
        }
        /// <summary>获取 timeZoneNames。</summary>
        public static List<string> GetTimeZoneNames()
        {
            var list = new List<string>();
            foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones())
                list.Add(tzi.DisplayName);
            return list;
        }

        #endregion

        #region 自然语言相对日期解析（支持中英文）

        /// <summary>
        /// 解析中英文自然语言输入，如 "today"、"明天"、"next Monday"、"下周二" 等。
        /// </summary>
        private static bool TryParseNaturalLanguage(string input, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string lower = input.Trim().ToLowerInvariant();
            DateTime today = DateTime.Today;

            if (lower == "today" || lower == HTranslation.GetContent("今天")) { result = today; return true; }
            if (lower == "tomorrow" || lower == HTranslation.GetContent("明天")) { result = today.AddDays(1); return true; }
            if (lower == "yesterday" || lower == HTranslation.GetContent("昨天")) { result = today.AddDays(-1); return true; }
            if (lower == "day after tomorrow" || lower == "后天" || lower == HTranslation.GetContent("后天")) { result = today.AddDays(2); return true; }
            if (lower == "day before yesterday" || lower == "前天" || lower == HTranslation.GetContent("前天")) { result = today.AddDays(-2); return true; }
            if (lower == "three days later" || lower == HTranslation.GetContent("大后天")) { result = today.AddDays(3); return true; }
            if (lower == "three days ago" || lower == HTranslation.GetContent("大前天")) { result = today.AddDays(-3); return true; }

            var matchEn = Regex.Match(lower, @"(next|last|this)\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday)");
            if (matchEn.Success)
            {
                string direction = matchEn.Groups[1].Value;
                DayOfWeek? targetDay = ParseEnglishDayOfWeek(matchEn.Groups[2].Value);
                if (targetDay != null)
                {
                    int offset = direction == "next" ? 7 : (direction == "last" ? -7 : 0);
                    int diff = (7 + (targetDay.Value - today.DayOfWeek)) % 7;
                    if (diff == 0 && direction != "this") diff = 7;
                    result = today.AddDays(diff + offset);
                    return true;
                }
            }

            var matchCn = Regex.Match(input, @"(下周|上周|本周)(.+)");
            if (matchCn.Success)
            {
                string weekPrefix = matchCn.Groups[1].Value;
                string dayName = matchCn.Groups[2].Value;
                DayOfWeek? targetDay = ParseChineseDayOfWeek(dayName);
                if (targetDay != null)
                {
                    int offset = weekPrefix == "下周" ? 7 : (weekPrefix == "上周" ? -7 : 0);
                    int diff = (7 + (targetDay.Value - today.DayOfWeek)) % 7;
                    if (diff == 0 && weekPrefix != "本周") diff = 7;
                    result = today.AddDays(diff + offset);
                    return true;
                }
            }

            var matchCn2 = Regex.Match(input, @"(上|下)星期(.+)");
            if (matchCn2.Success)
            {
                string dir = matchCn2.Groups[1].Value;
                string dayName = matchCn2.Groups[2].Value;
                DayOfWeek? targetDay = ParseChineseDayOfWeek(dayName);
                if (targetDay != null)
                {
                    int offset = dir == "上" ? -7 : 7;
                    int diff = (7 + (targetDay.Value - today.DayOfWeek)) % 7;
                    if (diff == 0 && dir == "上") diff = 7;
                    result = today.AddDays(diff + offset);
                    return true;
                }
            }

            return false;
        }

        /// <summary>ParseEnglishDayOfWeek 方法。</summary>
        private static DayOfWeek? ParseEnglishDayOfWeek(string day)
        {
            switch (day)
            {
                case "monday": return DayOfWeek.Monday;
                case "tuesday": return DayOfWeek.Tuesday;
                case "wednesday": return DayOfWeek.Wednesday;
                case "thursday": return DayOfWeek.Thursday;
                case "friday": return DayOfWeek.Friday;
                case "saturday": return DayOfWeek.Saturday;
                case "sunday": return DayOfWeek.Sunday;
                default: return null;
            }
        }

        /// <summary>ParseChineseDayOfWeek 方法。</summary>
        private static DayOfWeek? ParseChineseDayOfWeek(string dayName)
        {
            if (string.IsNullOrEmpty(dayName)) return null;
            string d = dayName.Replace("星期", "").Replace("周", "");
            switch (d)
            {
                case "一": case "1": return DayOfWeek.Monday;
                case "二": case "2": return DayOfWeek.Tuesday;
                case "三": case "3": return DayOfWeek.Wednesday;
                case "四": case "4": return DayOfWeek.Thursday;
                case "五": case "5": return DayOfWeek.Friday;
                case "六": case "6": return DayOfWeek.Saturday;
                case "日": case "天": case "7": return DayOfWeek.Sunday;
                default: return null;
            }
        }

        #endregion

        #region 其他辅助方法

        /// <summary>判断是否 SameDay。</summary>
        public static bool IsSameDay(DateTime date1, DateTime date2) => date1.Date == date2.Date;
        /// <summary>判断是否 SameMonth。</summary>
        public static bool IsSameMonth(DateTime date1, DateTime date2) => date1.Year == date2.Year && date1.Month == date2.Month;
        /// <summary>判断是否 SameYear。</summary>
        public static bool IsSameYear(DateTime date1, DateTime date2) => date1.Year == date2.Year;
        /// <summary>判断是否 DateInRange。</summary>
        public static bool IsDateInRange(DateTime date, DateTime start, DateTime end) => date >= start && date <= end;
        /// <summary>判断是否 ValidDate。</summary>
        public static bool IsValidDate(string input) => Parse(input, DateTime.MinValue) != DateTime.MinValue;
        /// <summary>NowString 方法。</summary>
        public static string NowString() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>转换为 dayString。</summary>
        public static string TodayString() => DateTime.Today.ToString("yyyy-MM-dd");

        #endregion

        #region 高精度时间统计

        /// <summary>
        /// 将一段时间按指定的时间片长度划分，统计每个时间片内发生的事件数量。
        /// 可用于生成时间直方图，例如以100ms为粒度统计事件频率。
        /// </summary>
        /// <param name="eventTimestamps">事件发生的时间戳集合。</param>
        /// <param name="rangeStart">统计范围的起始时间。</param>
        /// <param name="rangeEnd">统计范围的结束时间。</param>
        /// <param name="sliceDuration">每个时间片的长度，默认100毫秒。</param>
        /// <returns>整数列表，第 i 个元素代表第 i 个时间片内的事件数量。</returns>
        public static List<int> CountEventsByTimeSlice(IEnumerable<DateTime> eventTimestamps,
            DateTime rangeStart, DateTime rangeEnd, TimeSpan? sliceDuration = null)
        {
            TimeSpan slice = sliceDuration ?? TimeSpan.FromMilliseconds(100);
            var counts = new List<int>();

            if (eventTimestamps == null || rangeEnd < rangeStart)
                return counts;

            int totalSlices = (int)Math.Ceiling((rangeEnd - rangeStart).TotalMilliseconds / slice.TotalMilliseconds);
            if (totalSlices <= 0) return counts;

            counts = Enumerable.Repeat(0, totalSlices).ToList();

            foreach (DateTime timestamp in eventTimestamps)
            {
                if (timestamp < rangeStart || timestamp > rangeEnd) continue;
                int index = (int)((timestamp - rangeStart).TotalMilliseconds / slice.TotalMilliseconds);
                if (index >= 0 && index < counts.Count)
                    counts[index]++;
            }

            return counts;
        }

        /// <summary>
        /// 根据一系列状态变化点（时间戳与对应的新值），计算在指定时间范围内每个状态值的总持续时间。
        /// 例如：开始时值为0，10分钟后变为1，再过5分钟变为2…… 则可计算出各值持续了多长时间。
        /// 返回的字典 Key 为状态值，Value 为该状态的总持续时间。
        /// </summary>
        /// <param name="valueChanges">状态变化序列，Key 为变化发生的时间，Value 为新状态值。假设从序列开始前状态为0。</param>
        /// <param name="start">统计范围的起始时间。</param>
        /// <param name="end">统计范围的结束时间。</param>
        /// <returns>状态值到持续时间的映射。</returns>
        public static Dictionary<int, TimeSpan> CalculateValueDurations(
            IEnumerable<KeyValuePair<DateTime, int>> valueChanges, DateTime start, DateTime end)
        {
            var durations = new Dictionary<int, TimeSpan>();
            if (valueChanges == null || end <= start) return durations;

            var sortedChanges = valueChanges
                .Where(c => c.Key >= start && c.Key <= end)
                .OrderBy(c => c.Key)
                .ToList();

            int currentValue = 0;
            DateTime currentTime = start;

            foreach (var change in sortedChanges)
            {
                TimeSpan segment = change.Key - currentTime;
                if (segment > TimeSpan.Zero)
                {
                    if (!durations.ContainsKey(currentValue))
                        durations[currentValue] = TimeSpan.Zero;
                    durations[currentValue] = durations[currentValue].Add(segment);
                }
                currentValue = change.Value;
                currentTime = change.Key;
            }

            TimeSpan finalSegment = end - currentTime;
            if (finalSegment > TimeSpan.Zero)
            {
                if (!durations.ContainsKey(currentValue))
                    durations[currentValue] = TimeSpan.Zero;
                durations[currentValue] = durations[currentValue].Add(finalSegment);
            }

            return durations;
        }

        /// <summary>
        /// 将一段时间按指定的时间片长度划分，返回每个时间片起始时刻的列表（可用于绘图X轴）。
        /// </summary>
        /// <param name="rangeStart">起始时间。</param>
        /// <param name="rangeEnd">结束时间。</param>
        /// <param name="sliceDuration">时间片长度，默认100毫秒。</param>
        /// <returns>时间片起始时刻列表。</returns>
        public static List<DateTime> GetTimeSliceBoundaries(DateTime rangeStart, DateTime rangeEnd,
            TimeSpan? sliceDuration = null)
        {
            var boundaries = new List<DateTime>();
            if (rangeEnd < rangeStart) return boundaries;
            TimeSpan slice = sliceDuration ?? TimeSpan.FromMilliseconds(100);
            for (DateTime dt = rangeStart; dt <= rangeEnd; dt = dt.Add(slice))
                boundaries.Add(dt);
            return boundaries;
        }

        #endregion

        #region 内部辅助方法

        /// <summary>TryParseChineseMonthDay 方法。</summary>
        private static bool TryParseChineseMonthDay(string input, out DateTime result)
        {
            result = default;
            var match = Regex.Match(input, @"(\d{1,2})\s*月\s*(\d{1,2})\s*日");
            if (!match.Success) return false;

            if (int.TryParse(match.Groups[1].Value, out int month) && int.TryParse(match.Groups[2].Value, out int day))
            {
                if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
                {
                    int year = DateTime.Now.Year;
                    try { result = new DateTime(year, month, day); return true; } catch { }
                }
            }
            return false;
        }

        /// <summary>TryParseUnixTimestamp 方法。</summary>
        private static bool TryParseUnixTimestamp(string input, out DateTime result)
        {
            result = default;
            if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out long num)) return false;
            try
            {
                if (num > 1000000000000) result = FromUnixTimestampMilliseconds(num);
                else if (num > 1000000000) result = FromUnixTimestamp(num);
                else return false;
                return true;
            }
            catch { return false; }
        }

        #endregion
    }
}
