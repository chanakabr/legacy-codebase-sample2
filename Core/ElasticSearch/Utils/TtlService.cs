using System;
using ApiObjects;
using ConfigurationManager;

namespace ElasticSearch.Utilities
{
    public class TtlService : ITtlService
    {
        private static readonly double EXPIRY_DATE_DELTA = ApplicationConfiguration.Current.EPGDocumentExpiry.Value > 0 ? ApplicationConfiguration.Current.EPGDocumentExpiry.Value : 7;

        public static readonly ITtlService Instance = new TtlService();

        /// <summary>
        /// This expiration date is equal to January 1, 2000 0:00:00. We need it to invalidate document in Couchbase immediately.
        /// </summary>
        private const uint EXPIRATION_DATE = 946684800;

        public double GetEpgTtlMinutes(EpgCB epg)
        {
            return Math.Ceiling(GetEpgTtlSeconds(epg) / 60);
        }

        public uint GetEpgCouchbaseTtlSeconds(EpgCB epg)
        {
            var ttlSeconds = GetEpgTtlSeconds(epg);
            if (ttlSeconds <= 0)
            {
                return EXPIRATION_DATE;
            }

            return (uint)ttlSeconds;
        }

        private static double GetEpgTtlSeconds(EpgCB epg)
        {
            var expiryDate = epg.EndDate.AddDays(EXPIRY_DATE_DELTA);
            if (epg.SearchEndDate > expiryDate)
            {
                expiryDate = epg.SearchEndDate;
            }

            return Math.Ceiling((expiryDate - DateTime.UtcNow).TotalSeconds);
        }
    }
}