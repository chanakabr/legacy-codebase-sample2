using System;
using System.Linq;
using System.Linq.Expressions;
using ApiObjects;
using ApiObjects.SearchObjects;
using ElasticSearch.NEST;
using Elasticsearch.Net;
using Nest;

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

        public EsOrderByField(OrderBy field, OrderDir direction)
            : this (field, direction, null)
        {
        }

        public EsOrderByField(OrderBy field, OrderDir direction, LanguageObj language) : base(direction)
        {
            OrderByField = field;
            Language = language;
        }

        public OrderBy OrderByField { get; }
        public LanguageObj Language { get; }
    }
}
