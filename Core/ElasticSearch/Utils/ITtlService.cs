using System;
using ApiObjects;

namespace ElasticSearch.Utilities
{
    public interface ITtlService
    {
        double GetEpgTtlMinutes(EpgCB epg);

        double GetEpgTtlMinutes(DateTime programEndDate, DateTime searchEndDate);

        uint GetEpgCouchbaseTtlSeconds(EpgCB epg);
    }
}