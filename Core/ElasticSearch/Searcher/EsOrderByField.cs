using System;
using System.Linq;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByField : EsBaseOrderByField
    {
        private readonly LanguageObj _language;
        private static readonly OrderBy[] ProjectableOrderings =
        {
            OrderBy.NAME,
            OrderBy.EPG_ID,
            OrderBy.MEDIA_ID,
            OrderBy.START_DATE,
            OrderBy.CREATE_DATE,
            OrderBy.UPDATE_DATE
        };

        public EsOrderByField(OrderBy field, OrderDir direction)
            : this (field, direction, null)
        {
        }

        public EsOrderByField(OrderBy field, OrderDir direction, LanguageObj language) : base(direction)
        {
            OrderByField = field;
            _language = language;
        }

        public OrderBy OrderByField { get; }

        public override string EsField
        {
            get
            {
                if (ProjectableOrderings.Contains(OrderByField))
                {
                    var formattedValue = Enum.GetName(typeof(OrderBy), OrderByField)?.ToLower();
                    // case when not default language was requested.
                    if (OrderByField == OrderBy.NAME && _language != null && !_language.IsDefault)
                    {
                        return $"{formattedValue}_{_language.Code}";
                    }

                    return formattedValue;
                }

                switch (OrderByField)
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
}
