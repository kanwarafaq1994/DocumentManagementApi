using DocumentManagement.Data.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DocumentManagement.Data.Common
{
    public static class StaticDateTimeProvider
    {
        private static IDateTimeProvider _dateTimeProvider = new DateTimeProvider();
        private static CultureInfo _culture = new CultureInfo("de-DE");

        /// <summary>
        /// Use <see cref="ResetDateTimeProvider"/> to Reset on Teardown during Testing
        /// </summary>
        /// <param name="dateTimeProvider"></param>
        public static void SetDateTimeProvider(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public static void ResetDateTimeProvider()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        public static DateTime Now => _dateTimeProvider.Now.ToUnspecifiedKind();

        public static DateTime Today => Now.Date;

        public static DateTime InactiveTimeLimit { get => Now.AddHours(-WebConstants.TokenTimeoutHours); }

        public static string ToLocalString(this DateTime date)
        {
            return date.ToString("d", _culture);
        }
    }
}
