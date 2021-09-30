using System;

namespace ElasticSearch.Common
{
    public static class DateTimeExtensions
    {
        public static string ToEsDateFormatString(this DateTime? dateTime)
        {
            return dateTime?.ToString(Utils.ES_DATE_FORMAT);
        }

        public static string ToEsDateOnlyFormatString(this DateTime? dateTime)
        {
            return dateTime?.ToString(Utils.ES_DATEONLY_FORMAT);
        }
    }
}