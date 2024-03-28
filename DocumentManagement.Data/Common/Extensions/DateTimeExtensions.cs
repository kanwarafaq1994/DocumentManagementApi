using System;

namespace DocumentManagement.Data.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo BerlinTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        public static DateTime ToBerlinTimeZoneDateTime(this DateTime dateTimeOffset)
        {
            return TimeZoneInfo.ConvertTime(dateTimeOffset, BerlinTimeZoneInfo);
        }

        public static DateTime ToBerlinTimeZoneDateTimeOffset(this DateTime dateTimeOffset)
        {
            return TimeZoneInfo.ConvertTime(dateTimeOffset, BerlinTimeZoneInfo);
        }

        public static DateTime ToUnspecifiedKind(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }
    }

}
