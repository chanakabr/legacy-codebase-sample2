using System;
using ApiObjects;
using ConfigurationManager;

namespace ElasticSearch.Utilities
{
    public class TtlService : ITtlService
    {
        private static readonly double EXPIRY_DATE_DELTA = ApplicationConfiguration.Current.EPGDocumentExpiry.Value > 0 ? ApplicationConfiguration.Current.EPGDocumentExpiry.Value : 7;

        public double GetEpgTtlMinutes(EpgCB epg)
        {
            var expiryDate = epg.EndDate.AddDays(EXPIRY_DATE_DELTA);
            if (epg.SearchEndDate > expiryDate)
            {
                expiryDate = epg.SearchEndDate;
            }

            var ttlMinutes = Math.Ceiling((expiryDate - DateTime.UtcNow).TotalMinutes);

            return ttlMinutes;
        }
    }
}