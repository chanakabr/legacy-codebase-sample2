using System;
using System.Collections.Generic;
using ApiLogic.IndexManager.NestData;
using ApiObjects.SearchObjects;
using Nest;

namespace ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders.Queries
{
    public class NestBaseQueries
    {
        public QueryContainer GetIsActiveTerm()
        {
            var isActiveTerm = new QueryContainerDescriptor<NestBaseAsset>()
                .Term(x => x.Field(f => f.IsActive).Value(true));
            
            return isActiveTerm;
        }
        
        public string GetMetaSortField(OrderObj order,string langCode)
        {
            if (order == null || order.m_eOrderBy != OrderBy.META) return null;
            string metaName = order.m_sOrderValue.ToLower();
            return order.shouldPadString
                ? $"metas.{langCode}.padded_{metaName}"
                : $"metas.{langCode}.{metaName}";
        }


        public SortDescriptor<NestBaseAsset> GetSortDescriptor(
            OrderObj order, string languageCode, List<BoostScoreValueDefinition> definitionsBoostScoreValues = null)
        {
            var sort = new SortDescriptor<NestBaseAsset>();
            var functionScoreSort = definitionsBoostScoreValues?.Count > 0;
            if (functionScoreSort)
            {
                sort = sort.Descending(SortSpecialField.Score);
            }

            var sortOrder = order.m_eOrderDir == OrderDir.ASC ? SortOrder.Ascending : SortOrder.Descending;
            sort = SetSortField(order, languageCode, sort, sortOrder, functionScoreSort);

            if (order.m_eOrderBy != OrderBy.ID)
            {
                sort.Descending(path => path.DocumentId);
            }

            return sort;
        }
        
        private SortDescriptor<NestBaseAsset> SetSortField(OrderObj order, 
            string languageCode,
            SortDescriptor<NestBaseAsset> sort,
            SortOrder sortOrder,
            bool functionScoreSort)
        {
            if (order.m_eOrderBy==OrderBy.NAME)
            {
                sort = sort.Field($"name.{languageCode}", sortOrder);
                return sort;
            }
            if (order.m_eOrderBy == OrderBy.META)
            {
                var metaFieldName = GetMetaSortField(order, languageCode);
                sort = sort.Field(metaFieldName, sortOrder);
                return sort;
            }
            
            if (order.m_eOrderBy == OrderBy.ID)
            {
                sort = sort.Field(f => f.DocumentId, sortOrder);
                return sort;                
            }
            
            // if we didn't sort by score yet, but it was asked
            if (!functionScoreSort && (order.m_eOrderBy == OrderBy.RELATED || order.m_eOrderBy == OrderBy.NONE))
            {
                var isAsc = sortOrder == SortOrder.Ascending;
                sort = isAsc ? sort.Ascending(SortSpecialField.Score) : sort.Descending(SortSpecialField.Score);
                return sort;
            }
            
            sort = sort.Field(Enum.GetName(typeof(OrderBy), order.m_eOrderBy)?.ToLower(), sortOrder);
            return sort;
        }
    }
}