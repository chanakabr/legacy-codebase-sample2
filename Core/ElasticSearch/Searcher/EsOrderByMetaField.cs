using System;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByMetaField : EsBaseOrderByField
    {
        private readonly bool _isPadded;
        private readonly LanguageObj _language;

        public EsOrderByMetaField(
            string metaName,
            OrderDir direction,
            bool isPadded,
            Type metaType,
            LanguageObj language)
            : base(direction)
        {
            MetaType = metaType;
            MetaName = metaName;
            _isPadded = isPadded;
            _language = language;
        }

        public string MetaName { get; }

        public Type MetaType { get; }

        public override string EsField
        {
            get
            {
                var prefix = _isPadded ? "padded_" : string.Empty;
                var suffix = _language != null && !_language.IsDefault ? $"_{_language.Code}" : string.Empty;

                return $"metas.{prefix}{MetaName.ToLower()}{suffix}";
            }
        }
    }
}