using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ApiObjects.SearchObjects;
using ElasticSearch.NEST;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearch.Searcher
{
    public class EsOrderByFieldAdapter
    {
        private static readonly HashSet<OrderBy> ProjectableOrderings =
            new HashSet<OrderBy>
            {
                OrderBy.NAME,
                OrderBy.EPG_ID,
                OrderBy.MEDIA_ID,
                OrderBy.START_DATE,
                OrderBy.CREATE_DATE,
                OrderBy.UPDATE_DATE
            };

        public EsOrderByFieldAdapter(IEsOrderByField field)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public IEsOrderByField Field { get; }

        public string EsField
        {
            get
            {
                switch (Field)
                {
                    case EsOrderByField esOrderByField:
                        return GetEsOrderByFieldValue(esOrderByField);
                    case EsOrderByMetaField esOrderByMetaField:
                        return GetEsOrderByMetaFieldValue(esOrderByMetaField);
                    default:
                        return null;
                }
            }
        }

        public Field EsV7Field
        {
            get
            {
                switch (Field)
                {
                    case EsOrderByField esOrderByField:
                        return GetEsOrderByFieldValueV7(esOrderByField);
                    case EsOrderByMetaField esOrderByMetaField:
                        return GetEsOrderByMetaFieldValueV7(esOrderByMetaField);
                    default:
                        return null;
                }
            }
        }
        
        private static Field GetEsOrderByMetaFieldValueV7(EsOrderByMetaField field)
        {
            var prefix = field.IsPadded ? "padded_" : string.Empty;
            
            return new Field($"metas.{field.Language.Code}.{prefix}{field.MetaName.ToLower()}");
        }
        
        private static string GetEsOrderByMetaFieldValue(EsOrderByMetaField field)
        {
            var prefix = field.IsPadded ? "padded_" : string.Empty;
            var suffix = field.Language != null && !field.Language.IsDefault
                ? $"_{field.Language.Code}"
                : string.Empty;

            return $"metas.{prefix}{field.MetaName.ToLower()}{suffix}";
        }

        private static Field GetEsOrderByFieldValueV7(EsOrderByField field)
        {
            switch (field.OrderByField)
            {
                case OrderBy.NAME:
                    return new Field($"name.{field.Language.Code}");
                case OrderBy.ID:
                {
                    Expression<Func<NestBaseAsset, string>> expression = f => f.DocumentId;

                    return new Field(expression);
                }
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    return new Field(SortSpecialField.Score.GetStringValue());
                default:
                    return new Field(Enum.GetName(typeof(OrderBy), field.OrderByField)?.ToLower());
            }
        }

        private static string GetEsOrderByFieldValue(EsOrderByField field)
        {
            if (ProjectableOrderings.Contains(field.OrderByField))
            {
                var formattedValue = Enum.GetName(typeof(OrderBy), field.OrderByField)?.ToLower();
                // case when not default language was requested.
                if (field.OrderByField == OrderBy.NAME && field.Language != null && !field.Language.IsDefault)
                {
                    return $"{formattedValue}_{field.Language.Code}";
                }

                return formattedValue;
            }

            switch (field.OrderByField)
            {
                case OrderBy.ID:
                    return "_uid";
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    return "_score";
                default:
                    return null;
            }
        }
    }
}
