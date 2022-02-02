using System;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByMetaField : EsBaseOrderByField
    {
        private readonly bool _isPadded;
        
        public EsOrderByMetaField(string metaName, OrderDir direction, bool isPadded, Type metaType)
            : base(direction)
        {
            MetaType = metaType;
            MetaName = metaName;
            _isPadded = isPadded;
        }

        public string MetaName { get; }
        
        public Type MetaType { get; }

        public override string EsField => $"metas.{(_isPadded ? "padded_": string.Empty)}{MetaName.ToLower()}";
    }
}
