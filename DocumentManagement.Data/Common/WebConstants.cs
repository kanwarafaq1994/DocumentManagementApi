using System;

namespace DocumentManagement.Data.Common
{
    /// <summary>
    /// This class is exemplary class
    /// you can create or edit according to your requirements
    /// </summary>
    public static class WebConstants
    {

        public const int FileSize = 2048;
        public const string FileExtension = "pdf,doc,docx,xls,xlsx,txt,jpg,gif";

        public static DateTime baseDate = DateTime.Today;

        public static DateTime today = baseDate;
        public static DateTime yesterday = baseDate.AddDays(-1);
        public static DateTime thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
        public static DateTime thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
        public static DateTime lastWeekStart = thisWeekStart.AddDays(-7);
        public static DateTime lastWeekEnd = thisWeekStart.AddSeconds(-1);
        public static DateTime thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
        public static DateTime thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
        public static DateTime lastMonthStart = thisMonthStart.AddMonths(-1);
        public static DateTime lastMonthEnd = thisMonthStart.AddSeconds(-1);
        public static DateTime thisYearStart = new DateTime(baseDate.Year, 1, 1);
        public static DateTime thisYearEnd = thisYearStart.AddYears(1).AddSeconds(-1);

        public const int TokenTimeoutHours = 8;
    }
}
