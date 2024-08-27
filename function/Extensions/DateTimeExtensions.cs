using System;

namespace AHI.Broker.Function.Extension
{
    public static class DateTimeExtensions
    {
        public const string DEFAULT_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss.fff";
        public const string DEFAULT_DATETIME_OFFSET = "00:00";
        public const string TIMESTAMP_FORMAT = "yyyyMMddHHmmss";

        public static DateTimeOffset ToUtcDateTimeOffset(this DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => new DateTimeOffset(dateTime),
                DateTimeKind.Local => new DateTimeOffset(dateTime.ToUniversalTime()),
                _ => new DateTimeOffset(dateTime, TimeSpan.Zero)
            };
        }

        public static string ToTimestamp(this DateTime datetime, string offset = null)
        {
            var utc = datetime.ToUtcDateTimeOffset();
            if (!string.IsNullOrEmpty(offset))
                utc = utc.ToOffset(TimeSpan.Parse(offset));
            return utc.ToString(TIMESTAMP_FORMAT);
        }

        public static string ToValidOffset(string offset)
        {
            var builder = new System.Text.StringBuilder(offset);
            if (offset.StartsWith('+'))
                builder.Remove(0, 1);
            if (!offset.Contains(':'))
                builder.Append(":00");
            return builder.ToString();
        }
    }
}