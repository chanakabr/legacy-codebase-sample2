using System;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByMetaField : EsBaseOrderByField
    {
        public EsOrderByMetaField(string metaName, OrderDir direction, bool isPadded, Type metaType, LanguageObj language)
            : base(direction)
        {
            MetaType = metaType;
            MetaName = metaName;
            IsPadded = isPadded;
            Language = language;
        }
        
        public bool IsPadded { get; }
        public LanguageObj Language { get; }
        public string MetaName { get; }
        public Type MetaType { get; }
    }
}
