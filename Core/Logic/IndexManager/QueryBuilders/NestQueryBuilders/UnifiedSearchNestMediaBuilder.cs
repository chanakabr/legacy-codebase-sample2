using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders.Queries;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using KLogMonitor;
using Nest;

namespace ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders
{
    public class UnifiedSearchNestMediaBuilder : IUnifiedSearchNestBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());


        private readonly NestMediaQueries _nestMediaQueries;

        private readonly NestBaseQueries _nestBaseQueries;

        public UnifiedSearchNestMediaBuilder()
        {
            _nestMediaQueries = new NestMediaQueries();
            _nestBaseQueries = new NestBaseQueries();
        }

        public bool MinimizeQuery { get; set; }

        public MediaSearchObj Definitions { get; set; }

        public eQueryType QueryType { get; set; }

        public bool IncludeRegionTerms { get; set; }

        public bool UseMustWhenBooleanQuery { get; set; }

        public List<string> GetIndices()
        {
            var indices = new List<string>();
            indices.Add(NamingHelper.GetMediaIndexAlias(Definitions.m_nGroupId));
            return indices;
        }

        public AggregationDictionary GetAggs()
        {
            return new AggregationDictionary();
        }

        public SearchDescriptor<T> SetSizeAndFrom<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            var defaultPageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            var pageSize = Definitions.m_nPageSize > 0 ? Definitions.m_nPageSize : defaultPageSize;
            var fromSize = 0;

            if (Definitions.m_nPageIndex > 0)
            {
                fromSize = Definitions.m_nPageIndex * pageSize;
            }

            searchDescriptor = searchDescriptor.From(fromSize).Size(pageSize);
            return searchDescriptor;
        }

        public QueryContainer GetQuery()
        {
            var mainDescriptor = new QueryContainerDescriptor<object>();
            var must = new List<QueryContainer>();
            var mediaQuery = GetMainMediaQuery(Definitions, IncludeRegionTerms);

            must.Add(mediaQuery);
            var global = GetMetasTagsConditionsQuery(QueryType, Definitions, UseMustWhenBooleanQuery);
            if (global != null)
            {
                must.Add(global);
            }

            mainDescriptor.Bool(b => b.Must(must.ToArray()));
            return mainDescriptor;
        }

        public SearchDescriptor<NestBaseAsset> SetFields(SearchDescriptor<NestBaseAsset> searchRequest)
        {
            throw new NotImplementedException();
        }

        public SortDescriptor<NestBaseAsset> GetSort()
        {
            var order = Definitions.m_oOrder;
            var languageCode = Definitions.m_oLangauge.Code;
            return _nestBaseQueries.GetSortDescriptor(order, languageCode);
        }

        private QueryContainer GetMetasTagsConditionsQuery(eQueryType queryType, MediaSearchObj definitions,
            bool useMustWhenBoolean = false)
        {
            var descriptor = new QueryContainerDescriptor<object>();
            var containers = new List<QueryContainer>();

            if (queryType == eQueryType.EXACT)
            {
                if (definitions.m_oOrder.m_eOrderBy != OrderBy.RELATED)
                {
                    var andComposite = FilterMetasAndTagsConditions(definitions.m_dAnd, CutWith.AND);
                    var orComposite = FilterMetasAndTagsConditions(definitions.m_dOr, CutWith.OR);
                    var generatedComposite = FilterMetasAndTagsConditions(definitions.m_lFilterTagsAndMetas,
                        (CutWith)definitions.m_eFilterTagsAndMetasCutWith);
                    containers.AddRange(new[] { andComposite, orComposite, generatedComposite });
                    return descriptor.Bool(b => b.Must(containers.Where(x => x != null).ToArray()));
                }
                else
                {
                    var andComposite = FilterMetasAndTagsConditions(definitions.m_dAnd, CutWith.AND);
                    var orComposite = FilterMetasAndTagsConditions(definitions.m_dOr, CutWith.OR);
                    var generatedComposite = FilterMetasAndTagsConditions(definitions.m_lFilterTagsAndMetas,
                        (CutWith)definitions.m_eFilterTagsAndMetasCutWith);
                    containers.AddRange(new[] { andComposite, orComposite, generatedComposite });

                    var data = containers.Where(x => x != null).ToArray();
                    if (!data.Any())
                        return null;

                    return descriptor.Bool(b => b.Should(data));
                }
            }

            if (queryType == eQueryType.BOOLEAN)
            {
                var oAndBoolQuery = QueryMetasAndTagsConditions(definitions.m_dAnd, CutWith.AND);
                var oOrBoolQuery = QueryMetasAndTagsConditions(definitions.m_dOr, CutWith.OR);
                var oMultiFilterBoolQuery = QueryMetasAndTagsConditions(definitions.m_lFilterTagsAndMetas,
                    (CutWith)definitions.m_eFilterTagsAndMetasCutWith);

                containers.AddRange(new[] { oAndBoolQuery, oOrBoolQuery, oMultiFilterBoolQuery });
                var query = containers.Where(x => x != null).ToArray();
                if (!query.Any())
                    return null;

                if (useMustWhenBoolean)
                {
                    return descriptor.Bool(b => b.Must(query));
                }

                return descriptor.Bool(b => b.Should(query));
            }

            if (queryType == eQueryType.PHRASE_PREFIX)
            {
                if (definitions.m_dOr == null || definitions.m_dOr.Any())
                    return null;

                var multiMatchQueryDescriptor = new MultiMatchQueryDescriptor<object>();
                var fields = definitions.m_dOr
                    .Select(x => ElasticSearch.Common.Utils
                        .GetKeyNameWithPrefix(x.m_sKey, x.m_sKeyPrefix)).ToArray();

                return new QueryContainerDescriptor<object>()
                    .MultiMatch(m => m.Fields(x => x.Fields(fields)));
            }

            return null;
        }

        private QueryContainer QueryMetasAndTagsConditions(List<SearchValue> searchList, CutWith andOrCondition)
        {
            var anySearch = searchList != null && searchList.Count > 0;
            if (!anySearch)
            {
                return null;
            }

            var searchValues = searchList.Where(x => !string.IsNullOrEmpty(x.m_sKey)).ToList();
            if (!searchValues.Any())
            {
                return null;
            }

            var boolQueryDescriptors = new List<QueryContainerDescriptor<object>>();

            string language = Definitions.m_oLangauge == null ? string.Empty : Definitions.m_oLangauge.Code;

            foreach (var searchValue in searchValues)
            {
                var boolQueryDescriptor = new BoolQueryDescriptor<object>();

                var searchKey = ElasticSearch.Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(),
                        searchValue.m_sKeyPrefix);
                searchKey = UnifiedSearchNestBuilder.GetElasticsearchFieldName(language, searchKey, searchValue.fieldType);
                searchKey = $"{searchKey}.analyzed";
                var searchQueriesList = searchValue.m_lValue.Where(x => !string.IsNullOrEmpty(x)).ToList();
                var matchContainer = new List<QueryContainer>();
                foreach (var query in searchQueriesList)
                {
                    var matchQueryDescriptor = new MatchQueryDescriptor<object>().Field(searchKey).Query(query);
                    var queryContainer = new QueryContainerDescriptor<object>().Match(x => x.Field(searchKey).Query(query));
                    matchContainer.Add(queryContainer);
                }

                var boolQuery = boolQueryDescriptor.Must(matchContainer.ToArray());
                var queryContainerDescriptor = new QueryContainerDescriptor<object>();
                queryContainerDescriptor.Bool(b => boolQuery);
                boolQueryDescriptors.Add(queryContainerDescriptor);
            }

            return new QueryContainerDescriptor<object>().Bool(b =>
            {
                if (andOrCondition == CutWith.AND)
                    b.Must(boolQueryDescriptors.ToArray());

                if (andOrCondition == CutWith.OR)
                    b.Should(boolQueryDescriptors.ToArray());

                return b;
            });
        }

        private QueryContainer FilterMetasAndTagsConditions(List<SearchValue> searchList, CutWith andOrCondition)
        {
            if (searchList == null)
                return null;

            var searchValues = searchList.Where(x => x.m_lValue != null && x.m_lValue.Any()).ToList();

            if (!searchValues.Any())
            {
                return null;
            }

            var container = new List<QueryContainer>();

            var searchValuesWithInnerCutAnd = searchValues.Where(x => x.m_eInnerCutWith == CutWith.AND);
            var searchValuesWithInnerCutOr = searchValues.Where(x => x.m_eInnerCutWith == CutWith.OR);

            string language = Definitions.m_oLangauge == null ? string.Empty : Definitions.m_oLangauge.Code;

            //create the must container
            foreach (var searchValue in searchValuesWithInnerCutAnd)
            {
                var searchKey =
                    ElasticSearch.Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(),
                        searchValue.m_sKeyPrefix);
                searchKey = UnifiedSearchNestBuilder.GetElasticsearchFieldName(language, searchKey, searchValue.fieldType);

                var termList = searchValue.m_lValue.Select(value =>
                    new QueryContainerDescriptor<object>()
                        .Term(t => t
                            .Field(searchKey)
                            .Value(value)
                        )
                ).ToArray();

                container.Add(new QueryContainerDescriptor<object>().Bool(x => x.Must(termList)));
            }

            //create the should container
            foreach (var searchValue in searchValuesWithInnerCutOr)
            {
                var searchKey =
                    ElasticSearch.Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(),
                        searchValue.m_sKeyPrefix);
                searchKey = UnifiedSearchNestBuilder.GetElasticsearchFieldName(language, searchKey, searchValue.fieldType);

                var queryContainer = new QueryContainerDescriptor<object>().Terms(
                    x => x.Field(searchKey)
                        .Terms(searchValue.m_lValue.Select(s => s.ToLower())
                        )
                );
                container.Add(queryContainer);
            }

            return new QueryContainerDescriptor<object>()
                .Bool(b =>
                    {
                        if (andOrCondition == CutWith.AND)
                            b.Must(container.ToArray());

                        if (andOrCondition == CutWith.OR)
                            b.Should(container.ToArray());

                        return b;
                    }
                );
        }

        private QueryContainer GetMainMediaQuery(MediaSearchObj definitions, bool includeRegionTerms = false)
        {
            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            var must = new List<QueryContainer>();
            var mustNot = new List<QueryContainer>();

            var mediaDateRangesTerms = _nestMediaQueries.GetMediaDateRangesTerms(definitions);
            if (mediaDateRangesTerms != null)
            {
                must.Add(mediaDateRangesTerms);
            }

            var mediaIdTerm = _nestMediaQueries.GetMediaIdTerm(definitions);
            if (mediaIdTerm != null)
            {
                mustNot.Add(mediaIdTerm);
            }

            var mediaTypeTerms = _nestMediaQueries.GetMediaTypeTerms(definitions);
            if (mediaTypeTerms != null)
            {
                must.Add(mediaTypeTerms);
            }

            if (!MinimizeQuery)
            {
                var watchPermissionRules = _nestMediaQueries.GetMediaWatchPermissionRules(definitions);
                if (watchPermissionRules != null)
                {
                    must.Add(watchPermissionRules);
                }

                var userTypeTerms = _nestMediaQueries.GetMediaUserTypeTerms(definitions);
                if (userTypeTerms != null)
                {
                    must.Add(userTypeTerms);
                }

                var deviceRulesTerms = _nestMediaQueries.GetMediaDeviceRulesTerms(definitions);
                if (deviceRulesTerms != null)
                {
                    must.Add(deviceRulesTerms);
                }

                var isActiveTerm = _nestBaseQueries.GetIsActiveTerm();
                must.Add(isActiveTerm);
            }

            if (includeRegionTerms)
            {
                var mediaRegionTerms = _nestMediaQueries.GetMediaRegionTerms(Definitions);
                if (mediaRegionTerms != null)
                {
                    must.Add(mediaRegionTerms);
                }
            }

            return queryContainerDescriptor
                .Bool(b => b
                    .Must(must.ToArray())
                    .MustNot(mustNot.ToArray()
                    )
                );
        }
    }
}