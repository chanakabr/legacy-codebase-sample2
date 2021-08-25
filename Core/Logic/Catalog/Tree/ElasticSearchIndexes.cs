using System;

namespace ApiLogic.Catalog.Tree
{
    [Flags]
    public enum ElasticSearchIndexes
    {
        Common = 1,
        Epg = 2,
        Media = 4
    }
}