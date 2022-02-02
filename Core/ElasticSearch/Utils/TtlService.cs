using System;
using ApiObjects;
using Phx.Lib.Appconfig;

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
            return GetEpgTtlMinutes(epg.EndDate, epg.SearchEndDate);
        }
        
        public double GetEpgTtlMinutes(DateTime programEndDate, DateTime searchEndDate)
        {
            return Math.Ceiling(GetEpgTtlSeconds(programEndDate, searchEndDate) / 60);
        }

        public uint GetEpgCouchbaseTtlSeconds(EpgCB epg)
        {
            var ttlSeconds = GetEpgTtlSeconds(epg.EndDate, epg.SearchEndDate);
            if (ttlSeconds <= 0)
            {
                return EXPIRATION_DATE;
            }

            return (uint)ttlSeconds;
        }

        private static double GetEpgTtlSeconds(DateTime programEndDate, DateTime searchEndDate)
        {
            var expiryDate = programEndDate.AddDays(EXPIRY_DATE_DELTA);
            if (searchEndDate > expiryDate)
            {
                expiryDate = searchEndDate;
            }

            return Math.Ceiling((expiryDate - DateTime.UtcNow).TotalSeconds);
        }
    }
}