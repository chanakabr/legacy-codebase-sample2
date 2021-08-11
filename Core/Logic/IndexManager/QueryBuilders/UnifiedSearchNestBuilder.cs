using ApiObjects.SearchObjects;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class UnifiedSearchNestBuilder
    {
        public UnifiedSearchDefinitions Definitions { get; set; }
        
        public UnifiedSearchNestBuilder()
        {

        }

        public int GetSize()
        {
            return this.Definitions.pageSize;
        }

        public int GetFrom()
        {
            return this.Definitions.pageIndex * this.Definitions.pageSize;
        }

        public object GetSort()
        {
            return null;
        }

        public QueryContainer GetQuery()
        {
            return new QueryContainer();
        }

        public AggregationDictionary GetAggs()
        {
            return new AggregationDictionary();
        }

        public string[] GetIndices()
        {
            return new[] { "test" };
        }
    }
}
