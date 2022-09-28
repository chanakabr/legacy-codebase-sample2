using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.Transaction;
using ApiObjects.SearchObjects;
using Nest;
using TVinciShared;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class NestEpgQueries
    {
        public NestEpgQueries()
        {
        }

        public QueryContainer GetEpgPrefixQuery()
        {
            return new QueryContainerDescriptor<NestEpg>().Exists(x => x.Field(f => f.EpgID));
        }

        public QueryContainer GetEpgExcludeCrids(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var excludeCrids = unifiedSearchDefinitions.excludedCrids != null &&
                               unifiedSearchDefinitions.excludedCrids.Any();
            if (!excludeCrids)
                return null;

            var termsQueryDescriptor = new TermsQueryDescriptor<NestEpg>();
            var excludedCrids = unifiedSearchDefinitions.excludedCrids.ConvertAll(x => x.ToLower());
            termsQueryDescriptor.Field(t => t.Crid).Terms(excludedCrids);
            var queryContainer = new QueryContainerDescriptor<NestEpg>()
                .Terms(t =>
                    t.Terms(termsQueryDescriptor
                    )
                );
            return queryContainer;
        }

        public QueryContainer GetEpgWithoutAutoFillTerm(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            //this is the logic in v2
            //we dont want auto filled programs to be returned alone to the user, only with other programs
            //so when ShouldSearchAutoFill is set to false
            //we set the query to only return programs that are not autofilled
            //or in other words we filter out auto fill programs
            //I know its confusing but we cannot change it at this point :(
            if (unifiedSearchDefinitions.ShouldSearchAutoFill)
                return null;
            
            var termsQueryDescriptor = new TermQueryDescriptor<NestEpg>();
            termsQueryDescriptor.Field(f=>f.IsAutoFill).Value(false);
            var queryContainerDescriptor = new QueryContainerDescriptor<NestEpg>();
            queryContainerDescriptor.Terms(t => t.Terms(termsQueryDescriptor));
            return queryContainerDescriptor;
        }

        public QueryContainer GetEpgRegionTerms(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var anyRegions = unifiedSearchDefinitions.regionIds != null && unifiedSearchDefinitions.regionIds.Any();
            if (!anyRegions)
                return null;

            var termsQueryDescriptor = new TermsQueryDescriptor<NestEpg>();
            var termsValues = unifiedSearchDefinitions.regionIds.Select(region => region.ToString());
            termsQueryDescriptor.Field(x=>x.RecordingId).Terms(termsValues);
            var queryContainerDescriptor = new QueryContainerDescriptor<NestEpg>();
             queryContainerDescriptor.Terms(t => t.Terms(termsQueryDescriptor));
             return queryContainerDescriptor;
        }

        public List<QueryContainer> GetEpgParentalRulesTerms(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            if (!unifiedSearchDefinitions.epgParentalRulesTags.Any())
                return null;

            var terms = new List<QueryContainer>();
            // Run on all tags and their values
            foreach (var tagValues in unifiedSearchDefinitions.epgParentalRulesTags)
            {
                // Create a Not-in terms for each of the tags
                var termsValues = tagValues.Value.Select(x => x.ToLower());
                var termsQueryDescriptor = new QueryContainerDescriptor<NestEpg>();
                termsQueryDescriptor.Terms(t=>t.Field(f=>f.Tags[unifiedSearchDefinitions.langauge.Code]).Terms(termsValues));
                terms.Add(termsQueryDescriptor);
            }

            return terms;
        }

        public List<QueryContainer> GetEpgDateRanges(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            List<QueryContainer> queryContainers = new List<QueryContainer>();
            var nowPlusOffsetDateString = SystemDateTime.UtcNow.AddDays(unifiedSearchDefinitions.epgDaysOffest);
            var nowMinusOffsetDateString = SystemDateTime.UtcNow.AddDays(-unifiedSearchDefinitions.epgDaysOffest);

            if (unifiedSearchDefinitions.shouldUseStartDateForEpg)
            {
                var queryDescriptor =
                    GetEpgStartDateRange(nowMinusOffsetDateString,
                        nowPlusOffsetDateString);
                queryContainers.Add(queryDescriptor);
            }

            if (unifiedSearchDefinitions.shouldUseEndDateForEpg)
            {
                var queryDescriptor =
                    GetEpgShouldUseDateForEndDateRange(nowMinusOffsetDateString,
                        nowPlusOffsetDateString);
                queryContainers.Add(queryDescriptor);
            }

            if (unifiedSearchDefinitions.shouldUseSearchEndDate)
            {
                var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
                queryDescriptor.Bool(b => b.Must(
                    m => m.DateRange(t => t.GreaterThan(nowMinusOffsetDateString).Field(x => x.SearchEndDate))
                ));
                queryContainers.Add(queryDescriptor);
            }

            return queryContainers;
        }

        public QueryContainerDescriptor<NestEpg> GetEpgStartDateRange(
            DateTime nowMinusOffsetDateString, DateTime nowPlusOffsetDateString)
        {
            var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
            queryDescriptor.Bool(b=>b.Must(
                m => m.DateRange(t => t.GreaterThanOrEquals(nowMinusOffsetDateString).Field(f=>f.StartDate)),
                m => m.DateRange(t => t.LessThanOrEquals(nowPlusOffsetDateString).Field(f=>f.StartDate))
            ));
            return queryDescriptor;
        }

        public QueryContainerDescriptor<NestEpg> GetEpgShouldUseDateForEndDateRange(DateTime nowMinusOffsetDateString, DateTime nowPlusOffsetDateString)
        {
            var queryDescriptor = new QueryContainerDescriptor<NestEpg>();
            queryDescriptor.Bool(b=>b.Must(
                m => m.DateRange(t => t.GreaterThanOrEquals(nowMinusOffsetDateString).Field(f=>f.EndDate)),
                m => m.DateRange(t => t.LessThanOrEquals(nowPlusOffsetDateString).Field(f=>f.EndDate))
            ));
            return queryDescriptor;
        }
        
        public QueryContainer GetRecordingPrefixQuery()
        {
            return new QueryContainerDescriptor<NestEpg>().Exists(x => x.Field(f => f.RecordingId));
        }

        public List<QueryContainer> GetWildCardSearchOr(EpgSearchObj definitions)
        {
            var searchOr = definitions.m_lSearchOr;
            if (!searchOr.Any())
                return null;
            
            var exact = definitions.m_bExact;
            var wildCardList = new List<QueryContainer>();
            
            foreach (var searchValue in searchOr)
            {
                var field = ElasticSearch.Common.Utils.ReplaceQueryReservedCharacters(searchValue.m_sKey);
                var value = ElasticSearch.Common.Utils.ReplaceQueryReservedCharacters(searchValue.m_sValue);
                var formattedValue = exact ? string.Format("{0}", value) : string.Format("*{0}*", value);
                var wildcard = new QueryContainerDescriptor<NestEpg>().Wildcard(x => x.Value(formattedValue).Field(field));
                wildCardList.Add(wildcard);
            }

            return wildCardList;
        }

        public List<QueryContainer> GetWildCardSearchAnd(EpgSearchObj definitions)
        {
            var anyExactAndValues = definitions.m_bExact && definitions.m_lSearchAnd.Any();
            
            if (!anyExactAndValues)
                return null;
            
            var wildCardList = new List<QueryContainer>();

            foreach (var searchKvp in definitions.m_lSearchAnd)
            {
                var formattedValue = ElasticSearch.Common.Utils.ReplaceQueryReservedCharacters(searchKvp.m_sKey);
                var field = ElasticSearch.Common.Utils.ReplaceQueryReservedCharacters(searchKvp.m_sKey);
                var wildcard =
                    new QueryContainerDescriptor<NestEpg>().Wildcard(x => x.Value(formattedValue).Field(field));
                wildCardList.Add(wildcard);
            }


            return wildCardList;
        }

        public QueryContainer GetSearchPrefix(EpgSearchObj epgSearchObj)
        {
            if (epgSearchObj.m_lSearch==null||!epgSearchObj.m_lSearch.Any())
            {
                return null;
            }
            var prefixList = new List<QueryContainer>();
            foreach (EpgSearchValue kvp in epgSearchObj.m_lSearch)
            {
                var queryContainer = new QueryContainerDescriptor<NestEpg>();
                queryContainer.Prefix(p => p.Field(kvp.m_sKey).Value(kvp.m_sValue));
                prefixList.Add(queryContainer);
            }

            var queryContainerDescriptor = new QueryContainerDescriptor<NestEpg>();
             queryContainerDescriptor.Bool(b => b.Should(prefixList.ToArray()));
             return queryContainerDescriptor;
        }
    }
}