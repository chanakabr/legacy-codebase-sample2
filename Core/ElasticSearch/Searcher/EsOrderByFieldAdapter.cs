using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using Elasticsearch.Net;
using ElasticSearch.Utils;
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
                        return new Field(GetEsOrderByFieldValueV7(esOrderByField));
                    case EsOrderByMetaField esOrderByMetaField:
                        return new Field(GetEsOrderByMetaFieldValueV7(esOrderByMetaField, false));
                    default:
                        return null;
                }
            }
        }

        public string EsV7ExtraReturnField
        {
            get
            {
                switch (Field)
                {
                    case EsOrderByField esOrderByField:
                        return GetExtraReturnFieldValueV7(esOrderByField);
                    case EsOrderByMetaField esOrderByMetaField:
                        return GetEsOrderByMetaFieldValueV7(esOrderByMetaField, true);
                    default:
                        return null;
                }
            }
        }

        private static string GetEsOrderByMetaFieldValueV7(EsOrderByMetaField field, bool isExtraReturnField)
        {
            var paddedPrefix = field.IsPadded
                ? "padded_"
                : string.Empty;
            var sortSuffix = SpecialSortingServiceV7.Instance.IsSpecialSortingMeta(field.Language?.Code, field.MetaType)
                ? ".sort"
                : string.Empty;
            return isExtraReturnField
                ? $"metas.{paddedPrefix}{field.MetaName.ToLower()}{sortSuffix}"
                : $"metas.{field.Language.Code}.{paddedPrefix}{field.MetaName.ToLower()}{sortSuffix}";
        }

        private static string GetEsOrderByMetaFieldValue(EsOrderByMetaField field)
        {
            var paddedPrefix = field.IsPadded
                ? "padded_"
                : string.Empty;
            var languageSuffix = field.Language != null && !field.Language.IsDefault
                ? $"_{field.Language.Code}"
                : string.Empty;
            var sortSuffix = SpecialSortingServiceV2.Instance.IsSpecialSortingMeta(field.Language?.Code, field.MetaType)
                ? ".sort"
                : string.Empty;

            return $"metas.{paddedPrefix}{field.MetaName.ToLower()}{languageSuffix}{sortSuffix}";
        }

        private static string GetEsOrderByFieldValueV7(EsOrderByField field)
        {
            switch (field.OrderByField)
            {
                case OrderBy.NAME:
                    return GetNameFieldValueV7(field, false);
                case OrderBy.ID:
                    return "document_id";
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    return SortSpecialField.Score.GetStringValue();
                default:
                    return Enum.GetName(typeof(OrderBy), field.OrderByField)?.ToLower();
            }
        }

        private static string GetExtraReturnFieldValueV7(EsOrderByField field)
        {
            switch (field.OrderByField)
            {
                case OrderBy.NAME:
                    return GetNameFieldValueV7(field, true);
                case OrderBy.ID:
                    return "document_id";
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    return SortSpecialField.Score.GetStringValue();
                default:
                    return Enum.GetName(typeof(OrderBy), field.OrderByField)?.ToLower();
            }
        }

        private static string GetNameFieldValueV7(EsOrderByField field, bool isExtraReturnField)
        {
            var sortSuffix = SpecialSortingServiceV7.Instance.IsSpecialSortingField(field.Language?.Code)
                ? ".sort"
                : string.Empty;

            return isExtraReturnField
                ? $"name{sortSuffix}"
                : $"name.{field.Language.Code}{sortSuffix}";
        }

        private static string GetEsOrderByFieldValue(EsOrderByField field)
        {
            if (ProjectableOrderings.Contains(field.OrderByField))
            {
                var formattedValue = Enum.GetName(typeof(OrderBy), field.OrderByField)?.ToLower();
                // case when not default language was requested.
                if (field.OrderByField == OrderBy.NAME && field.Language != null && !field.Language.IsDefault)
                {
                    formattedValue = $"{formattedValue}_{field.Language.Code}";
                }

                if (SpecialSortingServiceV2.Instance.IsSpecialSortingField(field.Language?.Code))
                {
                    formattedValue = $"{formattedValue}.sort";
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