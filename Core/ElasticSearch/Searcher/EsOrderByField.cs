using System;
using System.Linq;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByField : EsBaseOrderByField
    {
        private static readonly OrderBy[] ProjectableOrderings =
        {
            OrderBy.NAME,
            OrderBy.EPG_ID,
            OrderBy.MEDIA_ID,
            OrderBy.START_DATE,
            OrderBy.CREATE_DATE,
            OrderBy.UPDATE_DATE
        };
        
        public EsOrderByField(OrderBy field, OrderDir direction) : base(direction)
        {
            OrderByField = field;
        }

        public OrderBy OrderByField { get; }

        public override string EsField
        {
            get
            {
                if (ProjectableOrderings.Contains(OrderByField))
                {
                    return Enum.GetName(typeof(OrderBy), OrderByField)?.ToLower();
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
