using ApiObjects;

namespace ElasticSearch.Utilities
{
    public interface ITtlService
    {
        double GetEpgTtlMinutes(EpgCB epg);
    }
}