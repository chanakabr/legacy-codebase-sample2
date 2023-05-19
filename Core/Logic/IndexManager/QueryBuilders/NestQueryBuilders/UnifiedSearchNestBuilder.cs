using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders.Queries;
using ApiLogic.IndexManager.Sorting;
using ApiObjects;
using ApiObjects.SearchObjects;
using Core.Api;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using Nest;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using BoolQuery = Nest.BoolQuery;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class UnifiedSearchNestBuilder : BaseEsUnifiedQueryBuilder, IUnifiedSearchNestBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected static readonly List<string> DEFAULT_RETURN_FIELDS = new List<string>()
            { "group_id", "name", "cache_date", "update_date" };
        protected static readonly List<string> EXTRA_FIELDS_WITH_LANGUAGE_PREFIX = new List<string>() { "name", "description" };
        protected internal const string TOP_HITS_DEFAULT_NAME = "top_hits_assets";
        public const int TERMS_AGGREGATION_MISSING_VALUE = 999;
        protected const string LOWERCASE_POSTFIX = "lowercase";
        public bool MinimizeQuery { get; set; }

        private QueryContainer SubscriptionsQuery { get; set; }
        private readonly NestMediaQueries _nestMediaQueries;
        private readonly NestEpgQueries _nestEpgQueries;
        private readonly NestBaseQueries _nestBaseQueries;

        public UnifiedSearchNestBuilder(
            IEsSortingService esSortingService,
            ISortingAdapter sortingAdapter,
            IUnifiedQueryBuilderInitializer queryInitializer)
            : base(esSortingService, sortingAdapter, queryInitializer)
        {
            _nestMediaQueries = new NestMediaQueries();
            _nestEpgQueries = new NestEpgQueries();
            _nestBaseQueries = new NestBaseQueries();
        }

        public SearchDescriptor<T> SetSizeAndFrom<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            int pageSize = this.PageSize;

            if (this.GetAllDocuments)
            {
                pageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            }
            else if (this.SearchDefinitions.topHitsCount > 0)
            {
                pageSize = 0;
            }

            int fromIndex = 0;

            // If we have a defined offset for search, use it
            if (From > 0)
            {
                fromIndex = From;
            }
            else
            {
                // Otherwise, we calculate the "from" by "page size times page index";
                fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;
            }

            // TODO: make it interface
            if (fromIndex + pageSize > ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value)
            {
                log.WarnFormat("changing page size and index to 0 because size*index + size > max results configured, sent size: {0}, sent index: {1}", PageSize, PageIndex);
                fromIndex = 0;
                pageSize = 0;
            }

            searchDescriptor = searchDescriptor.From(fromIndex).Size(pageSize);
            return searchDescriptor;
        }

        public AggregationDictionary GetAggs()
        {
            AggregationDictionary result = new AggregationDictionary();
            
            if (this.SearchDefinitions.groupBy != null && this.SearchDefinitions.groupBy.Count > 0)
            {
                var returnFields = GetFields().ToArray();
                AggregationContainer currentAggregation = null;

                // TODO: make sure order is correct
                //GetAggregationOrder(aggregationsOrderField, aggregationsOrderDirection);

                foreach (var groupBy in this.SearchDefinitions.groupBy)
                {
                    int? size = null;

                    if (this.GetAllDocuments && !ShouldPageGroups)
                    {
                        // not sure
                        size = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
                    }
                    else if (this.SearchDefinitions.topHitsCount > 0 || !string.IsNullOrEmpty(this.SearchDefinitions.distinctGroup?.Key))
                    {
                        size = this.SearchDefinitions.pageSize * (this.SearchDefinitions.pageIndex + 1);
                    }

                    if (currentAggregation == null)
                    {
                        object missing = null;

                        switch (SearchDefinitions.GroupByOption)
                        {
                            case GroupingOption.Include:
                                missing = TERMS_AGGREGATION_MISSING_VALUE;
                                this.SearchDefinitions.topHitsCount = ApplicationConfiguration.Current._elasticSearchConfiguration.MaxInnerResultWindow.Value; //allow missed bucket max results; //allow missed bucket max results
                                break;
                            case GroupingOption.Group:
                                missing = TERMS_AGGREGATION_MISSING_VALUE;
                                break;
                        }

                        var termsAggregation = new TermsAggregation(groupBy.Key)
                        {
                            Field = groupBy.Value.ToLower(),
                            Size = size,
                            Missing = missing,
                        };
                        
                        // TODO: Understand this!
                        // Get top hit as well if necessary
                        if (this.SearchDefinitions.topHitsCount > 0 || !string.IsNullOrEmpty(this.SearchDefinitions.distinctGroup?.Key))
                        {
                            int topHitsSize = -1;

                            if (this.SearchDefinitions.topHitsCount > 0)
                            {
                                topHitsSize = this.SearchDefinitions.topHitsCount;
                            }
                            else if (this.SearchDefinitions.topHitsCount == 0)
                            {
                                topHitsSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
                            }

                            var sourceFilter = new SourceFilter();
                            sourceFilter.Includes = returnFields;

                            // I dunno why we need to cast like this, just to get the value which is the list...
                            var castedSort = EsSortingService.GetSortingV7(OrderByFields) as IPromise<IList<ISort>>;

                            termsAggregation.Aggregations = new AggregationDictionary();
                            termsAggregation.Aggregations.Add(TOP_HITS_DEFAULT_NAME,
                                new TopHitsAggregation(TOP_HITS_DEFAULT_NAME)
                                {
                                    Size = topHitsSize,
                                    // order just like regular search
                                    Sort = castedSort.Value,
                                    Source = new Union<bool, ISourceFilter>(sourceFilter),
                                    DocValueFields = returnFields
                                }
                            );

                            var cardinalityName = $"{groupBy.Key}_count";
                            result.Add(cardinalityName, new CardinalityAggregation(cardinalityName, groupBy.Value.ToLower()));
                        }

                        SetAggregationOrder(termsAggregation);
                        currentAggregation = termsAggregation;
                        result.Add(groupBy.Key, currentAggregation);
                    }
                    else
                    {
                        var subAggregation = new TermsAggregation(groupBy.Key)
                        {
                            Field = groupBy.Value.ToLower(),
                            // previously it was 0 but it doesn't work...
                            Size = size,
                        };
                        SetAggregationOrder(subAggregation);

                        if (currentAggregation.Aggregations == null)
                        {
                            currentAggregation.Aggregations = new AggregationDictionary();
                        }

                        currentAggregation.Aggregations.Add(groupBy.Key, subAggregation);

                        currentAggregation = subAggregation;
                    }
                }
            }

            return result;
        }

        private void SetAggregationOrder(TermsAggregation currentAggregation)
        {
            TermsOrder termsOrder = null;

            if ((SearchDefinitions.topHitsCount > 0 || !string.IsNullOrEmpty(SearchDefinitions.distinctGroup?.Key))
                && OrderByFields?.Count == 1
                && OrderByFields.Single() is EsOrderByField esOrderByField)
            {
                var orderAggregation = GetOrderAggregation(esOrderByField);
                var orderAggregationName = orderAggregation != null ? "order_aggregation" : "_term";
                var sortOrder = esOrderByField.OrderByDirection == OrderDir.DESC
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
                if (orderAggregation != null)
                {
                    if (currentAggregation.Aggregations == null)
                    {
                        currentAggregation.Aggregations = new AggregationDictionary();
                    }

                    currentAggregation.Aggregations.Add(orderAggregationName, orderAggregation);
                }

                termsOrder = new TermsOrder { Key = orderAggregationName, Order = sortOrder };
            }
            else if (SearchDefinitions.groupByOrder != null && SearchDefinitions.groupByOrder.HasValue)
            {
                switch (SearchDefinitions.groupByOrder.Value)
                {
                    case AggregationOrder.Default:
                        break;
                    case AggregationOrder.Count_Asc:
                        {
                            termsOrder = TermsOrder.CountAscending;
                            break;
                        }
                    case AggregationOrder.Count_Desc:
                        {
                            termsOrder = TermsOrder.CountDescending;
                            break;
                        }
                    case AggregationOrder.Value_Asc:
                        {
                            termsOrder = TermsOrder.KeyAscending;
                            break;
                        }
                    case AggregationOrder.Value_Desc:
                        {
                            termsOrder = TermsOrder.KeyDescending;
                            break;
                        }
                    default:
                        break;
                }
            }

            if (termsOrder != null)
            {
                currentAggregation.Order = new List<TermsOrder> { termsOrder };
            }
        }

        private static AggregationContainer GetOrderAggregation(EsOrderByField esOrderByField)
        {
            const string orderAggregationName = "order_aggregation";
            string field = null;
            switch (esOrderByField.OrderByField)
            {
                case OrderBy.START_DATE:
                    field = "start_date";
                    break;
                case OrderBy.CREATE_DATE:
                    field = "create_date";
                    break;
                case OrderBy.RELATED:
                case OrderBy.NONE:
                    field = "_score";
                    break;
            }

            if (string.IsNullOrEmpty(field))
            {
                return null;
            }

            switch (esOrderByField.OrderByDirection)
            {
                case OrderDir.ASC:
                    return new MinAggregation(orderAggregationName, field);
                case OrderDir.DESC:
                    return new MaxAggregation(orderAggregationName, field);
                default:
                    return null;
            }
        }

        public List<string> GetIndices()
        {
            var indices = new List<string>();
            var parentGroupId = this.SearchDefinitions.ExtractParentGroupId();
            if (SearchDefinitions.shouldSearchMedia)
            {
                indices.Add(NamingHelper.GetMediaIndexAlias(parentGroupId));
            }

            if (SearchDefinitions.shouldSearchEpg)
            {
                indices.Add(NamingHelper.GetEpgIndexAlias(parentGroupId));
            }

            if (SearchDefinitions.shouldSearchRecordings)
            {
                indices.Add(NamingHelper.GetRecordingIndexAlias(parentGroupId));
            }

            return indices;
        }

        public QueryContainer GetQuery()
        {
            SubscriptionsQuery = GetEntitledSubscriptionsQuery(SearchDefinitions.entitlementSearchDefinitions);

            var globalQuery = GetGlobalQuery(SearchDefinitions);
            var assetsQuery = GetAssetTypesShouldQuery(SearchDefinitions);

            var mainBoolQuery = new BoolQuery()
            {
                Must = new List<QueryContainer>() { globalQuery, assetsQuery }
            };

            var priorityResult = HandlePrioritySearchGroups(mainBoolQuery);
            var result = !priorityResult.isApplied ? HandleBoostScoreValues(mainBoolQuery) : priorityResult.query;

            return result;
        }

        private (bool isApplied, QueryContainer query) HandlePrioritySearchGroups(QueryContainer mainBoolQuery)
        {
            if (SearchDefinitions.PriorityGroupsMappings != null && SearchDefinitions.PriorityGroupsMappings.Count != 0)
            {
                var priorityQueryBuilder = new PriorityQueryBuilder(this, SearchDefinitions.PriorityGroupsMappings);
                return (true, priorityQueryBuilder.Build(mainBoolQuery));
            }

            return (false, mainBoolQuery);
        }

        private QueryContainer HandleBoostScoreValues(BoolQuery mainBoolQuery)
        {
            if (SearchDefinitions.boostScoreValues != null && SearchDefinitions.boostScoreValues.Any())
            {
                var functions = new List<IScoreFunction>();
                string language = this.SearchDefinitions.langauge != null ? this.SearchDefinitions.langauge.Code : string.Empty;

                foreach (var item in SearchDefinitions.boostScoreValues)
                {
                    string key = GetElasticsearchFieldName(language, item.Key, item.Type, true);
                    functions.Add(new WeightFunction()
                    {
                        Filter = new TermQuery()
                        {
                            Field = key,
                            Value = item.Value
                        },
                        Weight = 100,
                    });
                }

                var functionScoreQuery = new FunctionScoreQuery()
                {
                    Name = "function_score",
                    ScoreMode = FunctionScoreMode.Sum,
                    BoostMode = FunctionBoostMode.Replace,
                    Functions = functions,
                    Query = mainBoolQuery
                };

                return functionScoreQuery;
            }

            return mainBoolQuery;
        }

        private QueryContainer GetEntitledSubscriptionsQuery(EntitlementSearchDefinitions entitlementDefinitions)
        {
            if (entitlementDefinitions == null)
            {
                return null;
            }

            var searchObjects = entitlementDefinitions.subscriptionSearchObjects;
            return BuildMultipleSearchObjectsQuery(searchObjects, true);
        }

        public QueryContainer BuildMultipleSearchObjectsQuery(List<BaseSearchObject> searchObjects, bool minimizeQuery = false)
        {
            if (searchObjects == null || !searchObjects.Any())
            {
                return null;
            }

            var shouldQueryContainer = new List<QueryContainer>();

            // Media
            var mediaBuilder = new UnifiedSearchNestMediaBuilder(){MinimizeQuery = minimizeQuery};
            var mediaSearchList = searchObjects.OfType<MediaSearchObj>();

            foreach (var mediaSearchItem in mediaSearchList)
            {
                // TODO: verify this isn't "damaged"
                // group id should already be inside the search object
                //mediaSearchItem.m_nGroupId = groupId;
                // SearchDefinitions.langauge is always initialized with default value.
                mediaSearchItem.m_oLangauge = mediaSearchItem.m_oLangauge ?? SearchDefinitions.langauge;
                mediaBuilder.Definitions = mediaSearchItem;
                mediaBuilder.QueryType = mediaSearchItem.m_bExact ? eQueryType.EXACT : eQueryType.BOOLEAN;
                var queryContainer = mediaBuilder.GetQuery();
                shouldQueryContainer.Add(queryContainer);
            }

            // Unified
            var unifiedSearchBuilder = new UnifiedSearchNestBuilder(EsSortingService, SortingAdapter, QueryInitializer)
            {
                MinimizeQuery = minimizeQuery
            };
            var unifiedSearchList = searchObjects.OfType<UnifiedSearchDefinitions>();
            foreach (var unifiedSearchItem in unifiedSearchList)
            {
                unifiedSearchItem.shouldAddIsActiveTerm = true;
                unifiedSearchBuilder.SearchDefinitions = unifiedSearchItem;
                var queryContainer = unifiedSearchBuilder.GetQuery();
                shouldQueryContainer.Add(queryContainer);
            }

            return new QueryContainerDescriptor<object>().Bool(x => x.Should(shouldQueryContainer.ToArray()));
        }

        private QueryContainer GetGlobalQuery(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var globalQuery = new QueryContainerDescriptor<NestBaseAsset>();
            var mustQueryContainers = new List<QueryContainer>();
            var mustNotQueryContainers = new List<QueryContainer>();

            var filterPhraseQueryContainer = 
                GetGlobalFilterPhraseQueryContainer(unifiedSearchDefinitions);
            if (filterPhraseQueryContainer != null)
                mustQueryContainers.Add(filterPhraseQueryContainer);

            if (unifiedSearchDefinitions.exactGroupId > 0)
            {
                var groupIdTerm = globalQuery.Term(selector => selector
                    .Field(field => field.GroupID)
                    .Value(SearchDefinitions.exactGroupId));
                mustQueryContainers.Add(groupIdTerm);
            }

            if (unifiedSearchDefinitions.shouldAddIsActiveTerm && !MinimizeQuery)
            {
                var isActiveTerm = _nestBaseQueries.GetIsActiveTerm();
                mustQueryContainers.Add(isActiveTerm);
            }

            if (unifiedSearchDefinitions.assetUserBlockRulePhrase != null)
            {
                var assetUserBlockRuleQuery = ConvertToQuery(unifiedSearchDefinitions.assetUserBlockRulePhrase);
                if (assetUserBlockRuleQuery != null)
                {
                    mustNotQueryContainers.Add(assetUserBlockRuleQuery);
                }
            }

            if (unifiedSearchDefinitions.assetUserRuleFilterPhrase != null)
            {
                var assetUserRuleQuery = ConvertToQuery(unifiedSearchDefinitions.assetUserRuleFilterPhrase);
                if (assetUserRuleQuery != null)
                {
                    mustQueryContainers.Add(assetUserRuleQuery);
                }
            }

            if (unifiedSearchDefinitions.langauge != null)
            {
                var languageQuery = globalQuery.Term(field => field.LanguageId, unifiedSearchDefinitions.langauge.ID);
                mustQueryContainers.Add(languageQuery);
            }

            return globalQuery.Bool(x => x
                .Must(mustQueryContainers.ToArray())
                .MustNot(mustNotQueryContainers.ToArray())
            );
        }

        private QueryContainer GetAssetTypesShouldQuery(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var assetsContainers = new List<QueryContainer>();

            if (unifiedSearchDefinitions.shouldSearchEpg)
            {
                assetsContainers.Add(GetMainEpgQuery(unifiedSearchDefinitions));
            }

            if (unifiedSearchDefinitions.shouldSearchMedia)
            {
                assetsContainers.Add(GetMainMediaQuery(unifiedSearchDefinitions));
            }

            if (unifiedSearchDefinitions.shouldSearchRecordings)
            {
                assetsContainers.Add(GetMainRecordingQuery(unifiedSearchDefinitions));
            }

            return new QueryContainerDescriptor<NestBaseAsset>().Bool(b => b.Should(assetsContainers.ToArray()));
        }

        private QueryContainerDescriptor<NestMedia> GetMainMediaQuery(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var mediaQuery = new QueryContainerDescriptor<NestMedia>();
            var mustQueryContainer = new List<QueryContainer>();
            var mustNotQueryContainer = new List<QueryContainer>();
            var mediaPrefixQuery = _nestMediaQueries.GetMediaPrefixQuery();
            mustQueryContainer.Add(mediaPrefixQuery);

            var assetContainer = CreateSpecificAssetContainer<NestMedia>(unifiedSearchDefinitions, eAssetTypes.MEDIA);
            mustQueryContainer.Add(assetContainer);

            var excludeAssetContainer = CreateExcludedAssetsContainer<NestMedia>(unifiedSearchDefinitions, eAssetTypes.MEDIA);
            mustQueryContainer.Add(excludeAssetContainer);

            var mediaTypeTerms = _nestMediaQueries.GetMediaTypeTerms(unifiedSearchDefinitions);
            if (mediaTypeTerms != null)
                mustQueryContainer.Add(mediaTypeTerms);

            var watchPermissionRules = _nestMediaQueries.GetMediaWatchPermissionRules(unifiedSearchDefinitions);
            if (watchPermissionRules != null)
                mustQueryContainer.Add(watchPermissionRules);

            var dateRangesTermsWithCountries = _nestMediaQueries.GetMediaDateRangesTermsWithCountries(unifiedSearchDefinitions);
            if (dateRangesTermsWithCountries != null)
                mustQueryContainer.Add(dateRangesTermsWithCountries);

            var regionTerms = _nestMediaQueries.GetMediaRegionTerms(unifiedSearchDefinitions);
            if (regionTerms != null)
                mustQueryContainer.Add(regionTerms);

            var geoBlockRules = _nestMediaQueries.GetMediaGeoBlockRules(unifiedSearchDefinitions);
            if (geoBlockRules != null)
                mustQueryContainer.Add(geoBlockRules);

            var parentalRules = _nestMediaQueries.GetMediaParentalRules(unifiedSearchDefinitions);
            if (parentalRules != null)
                mustNotQueryContainer.AddRange(parentalRules);

            var virtualAssetTerms = _nestMediaQueries.GetMediaVirtualAssetTerms(unifiedSearchDefinitions);
            if (virtualAssetTerms != null)
                mustNotQueryContainer.Add(virtualAssetTerms);

            if (!MinimizeQuery)
            {
                var minimizeQueryByMediaType = _nestMediaQueries.GetMediaUserTypeTerms(unifiedSearchDefinitions);
                if (minimizeQueryByMediaType != null)
                    mustQueryContainer.Add(minimizeQueryByMediaType);

                var deviceTypesTerms = _nestMediaQueries.GetMediaDeviceRulesTerms(unifiedSearchDefinitions);
                if (deviceTypesTerms != null)
                    mustQueryContainer.Add(deviceTypesTerms);
            }

            mediaQuery.Bool(b => b
                    .Must(mustQueryContainer.Where(x => x != null).ToArray())
                    .MustNot(mustNotQueryContainer.Where(x => x != null).ToArray())
            );
            return mediaQuery;
        }

        private QueryContainerDescriptor<NestEpg> GetMainRecordingQuery(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var recordingQuery = new QueryContainerDescriptor<NestEpg>();
            var mustContainer = new List<QueryContainer>();
            var mustNotContainer = new List<QueryContainer>();
            var recordingPrefixQuery = _nestEpgQueries.GetRecordingPrefixQuery();
            mustContainer.Add(recordingPrefixQuery);

            var assetContainer = CreateSpecificAssetContainer<NestEpg>(unifiedSearchDefinitions, eAssetTypes.NPVR);
            mustContainer.Add(assetContainer);

            var excludeAssetContainer = CreateExcludedAssetsContainer<NestEpg>(unifiedSearchDefinitions, eAssetTypes.NPVR);
            mustContainer.Add(excludeAssetContainer);

            //get exclude CRIDS terms
            var epgExcludeCrids = _nestEpgQueries.GetEpgExcludeCrids(unifiedSearchDefinitions);
            if (epgExcludeCrids != null)
                mustNotContainer.Add(epgExcludeCrids);

            recordingQuery.Bool(b => b.Must(mustContainer.Where(x => x != null).ToArray()));
            return recordingQuery;
        }

        private QueryContainerDescriptor<NestEpg> GetMainEpgQuery(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var epgQuery = new QueryContainerDescriptor<NestEpg>();
            var mustQueryContainer = new List<QueryContainer>();
            var mustNotQueryContainer = new List<QueryContainer>();
            var epgPrefixQuery = _nestEpgQueries.GetEpgPrefixQuery();
            mustQueryContainer.Add(epgPrefixQuery);

            var assetContainer = CreateSpecificAssetContainer<NestEpg>(unifiedSearchDefinitions, eAssetTypes.EPG);
            mustQueryContainer.Add(assetContainer);

            var excludeAssetContainer = CreateExcludedAssetsContainer<NestEpg>(unifiedSearchDefinitions, eAssetTypes.EPG);
            mustQueryContainer.Add(excludeAssetContainer);

            //handle Epg date ranges
            var epgDateRanges = _nestEpgQueries.GetEpgDateRanges(unifiedSearchDefinitions);
            if (epgDateRanges.Any())
                mustQueryContainer.AddRange(epgDateRanges.ToArray());

            //handle parental rules
            var parentalRulesTerms = _nestEpgQueries.GetEpgParentalRulesTerms(unifiedSearchDefinitions);
            if (parentalRulesTerms != null)
                mustNotQueryContainer.AddRange(parentalRulesTerms);

            // region term 
            var regionTerms = _nestEpgQueries.GetEpgRegionTerms(unifiedSearchDefinitions);
            if (regionTerms != null)
                mustQueryContainer.Add(regionTerms);

            //auto fill term
            var autoFillTerm = _nestEpgQueries.GetEpgWithoutAutoFillTerm(unifiedSearchDefinitions);
            if (autoFillTerm != null)
                mustQueryContainer.Add(autoFillTerm);

            //get exclude CRIDS terms
            var epgExcludeCrids = _nestEpgQueries.GetEpgExcludeCrids(unifiedSearchDefinitions);
            if (epgExcludeCrids != null)
                mustNotQueryContainer.Add(epgExcludeCrids);

            epgQuery.Bool(b => b
                .Must(mustQueryContainer.Where(x => x != null).ToArray())
                .MustNot(mustNotQueryContainer.Where(x => x != null).ToArray())
            );

            return epgQuery;
        }

        #region Global

        private QueryContainer GetGlobalFilterPhraseQueryContainer(UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            if (this.SearchDefinitions.filterPhrase == null)
            {
                return null;
            }

            var result = ConvertToQuery(this.SearchDefinitions.filterPhrase);

            return result;
        }

        internal QueryContainer ConvertToQuery(BooleanPhraseNode root)
        {
            var queryContainerDescriptor = new QueryContainerDescriptor<NestBaseAsset>();
            QueryContainer result = null;

            if (root.type == BooleanNodeType.Leaf)
            {
                BooleanLeaf leaf = root as BooleanLeaf;

                string field = HandleLanguageSpecificLeafField(leaf);
                object value = leaf.value;

                // Special case - if this is the entitled assets leaf, we build a specific term for it
                if (field == NamingHelper.ENTITLED_ASSETS_FIELD)
                {
                    result = BuildEntitledAssetsQuery(leaf);
                }
                else if (field == NamingHelper.USER_INTERESTS_FIELD)
                {
                    result = BuildUserInterestsQuery();
                }
                else if (field == NamingHelper.ASSET_TYPE)
                {
                    result = BuildAssetTypeQuery(leaf);
                }
                else if (field == NamingHelper.RECORDING_ID)
                {
                    result = BuildRecordingIdTerm(leaf);
                } 
                else if (field == NamingHelper.AUTO_FILL_FIELD)
                {
                    //this is the logic in v2
                    //we dont want auto filled programs to be returned alone to the user, only with other programs
                    //so when ShouldSearchAutoFill is set to false
                    //we set the query to only return programs that are not autofilled
                    //or in other words we filter out auto fill programs
                    //I know its confusing but we cannot change it at this point :(
                    return null;
                }
                else if (field == NamingHelper.IS_ACTIVE)
                {
                    result = new TermQuery
                    {
                        Field = field,
                        Value = Convert.ToBoolean(value)
                    };
                }
                else
                {
                    bool isNumeric = leaf.valueType == typeof(int) || leaf.valueType == typeof(long) || leaf.valueType == typeof(bool);

                    bool isDateTime = leaf.valueType == typeof(DateTime);
                    if (isDateTime)
                    {
                        value = Convert.ToDateTime(value).ToString("O");
                    }

                    // "Match" when search is not exact (contains)
                    if (leaf.operand == ApiObjects.ComparisonOperator.Contains ||
                        leaf.operand == ApiObjects.ComparisonOperator.Equals ||
                        leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith ||
                        leaf.operand == ApiObjects.ComparisonOperator.PhraseStartsWith ||
                        leaf.operand == ApiObjects.ComparisonOperator.Phonetic)
                    {
                        var isFuzzySearch = false;

                        if (leaf.operand == ApiObjects.ComparisonOperator.WordStartsWith)
                        {
                            field = $"{field}.autocomplete";
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.PhraseStartsWith)
                        {
                            field = $"{field}.phrase_autocomplete";
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Contains)
                        {
                            field = $"{field}.analyzed";
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Phonetic)
                        {
                            if (leaf.valueType == typeof(string) && !IndexManagerCommonHelpers.IsLanguagePhoneticSupported(leaf.value.ToString()))
                            {
                                isFuzzySearch = true;
                            }
                            else
                            {
                                field = $"{field}.phonetic";
                            }
                        }
                        else if (leaf.operand == ApiObjects.ComparisonOperator.Equals &&
                            leaf.shouldLowercase)
                        {
                            field = $"{field}.{LOWERCASE_POSTFIX}";
                        }

                        if (isFuzzySearch)
                        {
                            result = queryContainerDescriptor.Fuzzy(fuzzy => fuzzy.Field(field).Value(value.ToString()));
                        }
                        else if (!isNumeric)
                        {
                            result = queryContainerDescriptor.Match(match => match.Field(field).Operator(Operator.And).Query(value.ToString()));
                        }
                        else
                        {
                            result = queryContainerDescriptor.Term(field, value);
                        }
                    }
                    // "bool" with "must_not" when no contains
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotContains)
                    {
                        field = $"{field}.analyzed";

                        var matchQuery = queryContainerDescriptor.Match(match => match.Field(field).Operator(Operator.And).Query(value.ToString()));
                        result = queryContainerDescriptor.Bool(b => b.MustNot(matchQuery));
                    }
                    else if (leaf.operand == ComparisonOperator.NotEquals)
                    {
                        if (leaf.shouldLowercase)
                        {
                            field = $"{field}.{LOWERCASE_POSTFIX}";
                        }

                        var matchQuery = queryContainerDescriptor.Match(match => match.Field(field).Operator(Operator.And).Query(value.ToString()));
                        result = queryContainerDescriptor.Bool(b => b.MustNot(matchQuery));
                    }
                    else if (leaf.operand == ComparisonOperator.Equals)
                    {
                        result = queryContainerDescriptor.Term(field, value);
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Prefix)
                    {
                        result = queryContainerDescriptor.Prefix(field, value.ToString());
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.In)
                    {
                        result = queryContainerDescriptor.Terms(terms => terms.Field(field).Terms(value as IEnumerable<string>));
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotIn)
                    {
                        var terms = queryContainerDescriptor.Terms(t => t.Field(field).Terms(value as IEnumerable<string>));
                        result = queryContainerDescriptor.Bool(b => b.MustNot(terms));
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.Exists)
                    {
                        result = queryContainerDescriptor.Exists(exists => exists.Field(field));
                    }
                    else if (leaf.operand == ApiObjects.ComparisonOperator.NotExists)
                    {
                        var exists = queryContainerDescriptor.Exists(e => e.Field(field));
                        result = queryContainerDescriptor.Bool(b => b.MustNot(exists));
                    }
                    // Other cases are "Range"
                    else
                    {
                        result = BuildRangeQuery(leaf);
                    }
                }

                // If this leaf is relevant only to certain asset types - create a bool query connecting the types and the term
                if (leaf.assetTypes != null && leaf.assetTypes.Count > 0)
                {
                    QueryContainer subBool = null;
                    QueryContainer notTypesBool = null;

                    List<QueryContainer> subBoolChildren = new List<QueryContainer>();

                    // the original term is a MUST (and)

                    // Create a prefix term for each asset type
                    foreach (var assetType in leaf.assetTypes)
                    {
                        QueryContainer currentQuery = null;
                        switch (assetType)
                        {
                            case eObjectType.Media:
                                currentQuery = _nestMediaQueries.GetMediaPrefixQuery();
                                break;
                            case eObjectType.EPG:
                                currentQuery = _nestEpgQueries.GetEpgPrefixQuery();
                                break;
                            case eObjectType.Recording:
                                currentQuery = _nestEpgQueries.GetRecordingPrefixQuery();
                                break;
                            default:
                                break;
                        }

                        // the prefixes are SHOULD (or) [at least one of the following....)
                        subBoolChildren.Add(currentQuery);
                    }

                    subBool = queryContainerDescriptor.Bool(b => b.Must(result).Should(subBoolChildren.ToArray()));
                    notTypesBool = queryContainerDescriptor.Bool(b => b.MustNot(subBoolChildren.ToArray()));

                    result = queryContainerDescriptor.Bool(b => b.Should(subBool, notTypesBool));
                }
            }
            else
            {
                List<QueryContainer> mustChildren = new List<QueryContainer>();
                List<QueryContainer> mustNotChildren = new List<QueryContainer>();
                var cut = (root as BooleanPhrase).operand;

                // Add every child node to the boolean query. This is recursive!
                foreach (var childNode in (root as BooleanPhrase).nodes)
                {
                    var newChild = ConvertToQuery(childNode);

                    if (newChild != null)
                    {
                        mustChildren.Add(newChild);
                    }
                }

                result = queryContainerDescriptor.Bool(b =>
                {
                    if (cut == eCutType.Or)
                    {
                        List<QueryContainer> shouldChildren = new List<QueryContainer>(mustChildren);
                        foreach (var item in mustNotChildren)
                        {
                            shouldChildren.Add(queryContainerDescriptor.Bool(innerBool => innerBool.MustNot(item)));
                        }

                        b = b.Should(shouldChildren.ToArray());
                    }
                    else
                    {
                        b = b.MustNot(mustNotChildren.ToArray())
                            .Must(mustChildren.ToArray());
                        ;
                    }

                    return b;
                });
            }

            return result;
        }

        private string HandleLanguageSpecificLeafField(BooleanLeaf leaf)
        {
            string field = leaf.field;
            string language = this.SearchDefinitions.langauge != null ? this.SearchDefinitions.langauge.Code : string.Empty;
            field = GetElasticsearchFieldName(language, field, leaf.fieldType, leaf.isLanguageSpecific);

            return field;
        }

        internal static string GetElasticsearchFieldName(string language, string field, eFieldType type, bool isLanguageSpecific = false)
        {
            string result = field;
            switch (type)
            {
                case eFieldType.Default:
                    if (isLanguageSpecific)
                    {
                        result = $"{field}.{language}";
                    }
                    break;
                case eFieldType.LanguageSpecificField:
                    result = $"{field}.{language}";
                    break;
                case eFieldType.StringMeta:
                    result = $"metas.{language}.{field}";
                    break;
                case eFieldType.NonStringMeta:
                    result = $"metas.{language}.{field}";
                    break;
                case eFieldType.Tag:
                    result = $"tags.{language}.{field}";
                    break;
                default:
                    break;
            }

            return result;
        }

        private QueryContainer BuildRangeQuery(BooleanLeaf leaf)
        {
            QueryContainer result = null;
            QueryContainerDescriptor<object> queryContainerDescriptor = new QueryContainerDescriptor<object>();

            string field = leaf.field;
            object value = leaf.value;
            Type valueType = leaf.valueType;
            string rangeValue = Convert.ToString(value);
            
            if (this.SearchDefinitions.numericEpgMetas.Contains(leaf.field) && (this.SearchDefinitions.shouldSearchEpg || this.SearchDefinitions.shouldSearchRecordings))
            {
                field = $"padded_{field}";
                rangeValue = Media.PadValue(rangeValue);
                valueType = typeof(string);
            }

            string language = this.SearchDefinitions.langauge != null ? this.SearchDefinitions.langauge.Code : string.Empty;
            field = GetElasticsearchFieldName(language, field, leaf.fieldType, leaf.isLanguageSpecific);

            if (valueType == typeof(long) || valueType == typeof(int) || valueType == typeof(double))
            {
                long longValue = Convert.ToInt64(leaf.value);
                result = queryContainerDescriptor.LongRange(range =>
                {
                    range = range.Field(field);

                    switch (leaf.operand)
                    {
                        case ApiObjects.ComparisonOperator.GreaterThanOrEqual:
                            {
                                range = range.GreaterThanOrEquals(longValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.GreaterThan:
                            {
                                range = range.GreaterThan(longValue);

                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThanOrEqual:
                            {
                                range = range.LessThanOrEquals(longValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThan:
                            {
                                range = range.LessThan(longValue);
                                break;
                            }
                        default:
                            break;
                    }
                    return range;
                });
            }
            else if (valueType == typeof(DateTime))
            {
                // By default the metas are created with yyyyMMddHHmmss date format.
                // Consider changing it to default date format in the future for ElasticSearch V7?
                // This is a dirty hack to save time. The story to fix this bug is described there - https://kaltura.atlassian.net/browse/BEO-12693.
                var dateValue = DateMath.FromString(leaf.isReservedUnifiedSearchDate
                    ? Convert.ToDateTime(leaf.value).ToString("O")
                    : Convert.ToDateTime(leaf.value).ToString("yyyyMMddHHmmss"));

                result = queryContainerDescriptor.DateRange(range =>
                {
                    range = range.Field(field);

                    switch (leaf.operand)
                    {
                        case ApiObjects.ComparisonOperator.GreaterThanOrEqual:
                            {
                                range = range.GreaterThanOrEquals(dateValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.GreaterThan:
                            {
                                range = range.GreaterThan(dateValue);

                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThanOrEqual:
                            {
                                range = range.LessThanOrEquals(dateValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThan:
                            {
                                range = range.LessThan(dateValue);
                                break;
                            }
                        default:
                            break;
                    }
                    return range;
                });
            }
            else if (valueType == typeof(string))
            {
                result = queryContainerDescriptor.TermRange(range =>
                {
                    range = range.Field(field);

                    switch (leaf.operand)
                    {
                        case ApiObjects.ComparisonOperator.GreaterThanOrEqual:
                            {
                                range = range.GreaterThanOrEquals(rangeValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.GreaterThan:
                            {
                                range = range.GreaterThan(rangeValue);

                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThanOrEqual:
                            {
                                range = range.LessThanOrEquals(rangeValue);
                                break;
                            }
                        case ApiObjects.ComparisonOperator.LessThan:
                            {
                                range = range.LessThan(rangeValue);
                                break;
                            }
                        default:
                            break;
                    }
                    return range;
                });
            }

            return result;
        }

        private QueryContainer BuildRecordingIdTerm(BooleanLeaf leaf)
        {
            QueryContainer result = null;

            if (leaf.operand == ApiObjects.ComparisonOperator.Equals)
            {
                string recordingId = "0";
                string domainRecordingId = leaf.value.ToString();
                if (this.SearchDefinitions.domainRecordingIdToRecordingIdMapping.ContainsKey(domainRecordingId))
                {
                    recordingId = this.SearchDefinitions.domainRecordingIdToRecordingIdMapping[domainRecordingId];
                }

                result = new TermQuery()
                {
                    Field = NamingHelper.RECORDING_ID,
                    Value = recordingId
                };
            }
            else if (leaf.operand == ComparisonOperator.In)
            {
                var domainRecordingIds = leaf.value is IEnumerable<string>
                    ? ((IEnumerable<string>)leaf.value).Select(item => item.ToLower()).ToList()
                    : Convert.ToString(leaf.value).ToLower().Split(',').ToList();
                var recordingIds = new List<string>();
                foreach (var domainRecordingId in domainRecordingIds)
                {
                    if (!this.SearchDefinitions.domainRecordingIdToRecordingIdMapping.TryGetValue(
                        domainRecordingId,
                        out var recordingId))
                    {
                        recordingId = "0";
                    }

                    recordingIds.Add(recordingId);
                }

                result = new TermsQuery()
                {
                    Field = NamingHelper.RECORDING_ID,
                    Terms = recordingIds
                };
            }

            return result;
        }

        private QueryContainer BuildAssetTypeQuery(BooleanLeaf leaf)
        {
            QueryContainer result = null;

            string loweredValue = leaf.value.ToString().ToLower();

            if (loweredValue == "media")
            {
                result = _nestMediaQueries.GetMediaPrefixQuery();
            }
            else if (loweredValue == "epg")
            {
                result = _nestEpgQueries.GetEpgPrefixQuery();
            }
            else if (loweredValue == "recording")
            {
                result = _nestEpgQueries.GetRecordingPrefixQuery();
            }
            else
            {
                result = new TermQuery() { Field = "media_type_id", Value = leaf.value };
            }

            return result;
        }

        private QueryContainer BuildUserInterestsQuery()
        {
            QueryContainer result = null;
            var userPreferences = this.SearchDefinitions.userPreferences;

            if ((userPreferences != null) &&
                (((userPreferences.Metas != null) &&
                (userPreferences.Metas.Count > 0)) ||
                ((userPreferences.Tags != null) &&
                (userPreferences.Tags.Count > 0))))
            {
                List<QueryContainer> shoulds = new List<QueryContainer>();

                if (userPreferences.Tags != null)
                {
                    foreach (var tag in userPreferences.Tags)
                    {
                        var terms = new TermsQuery()
                        {
                            Field = $"tags.{this.SearchDefinitions.langauge.Code}.{tag.Key.ToLower()}",
                            // TODO: check if should lowercase
                            Terms = tag.Value
                        };

                        shoulds.Add(terms);
                    }
                }

                if (userPreferences.Metas != null)
                {
                    foreach (var meta in userPreferences.Metas)
                    {
                        var terms = new TermsQuery()
                        {
                            Field = $"metas.{this.SearchDefinitions.langauge.Code}.{meta.Key.ToLower()}",
                            // TODO: check if should lowercase
                            Terms = meta.Value
                        };

                        shoulds.Add(terms);
                    }
                }

                result = new BoolQuery()
                {
                    Should = shoulds
                };
            }
            else
            {
                // If user has no preferences at all
                result = new TermQuery() { Field = "_id", Value = -1 };
            }

            return result;
        }

        private QueryContainer BuildEntitledAssetsQuery(BooleanLeaf leaf)
        {
            QueryContainer result = null;
            string entitledAssetsSearchType = Convert.ToString(leaf.value).ToLower();
            var entitlementSearchDefinitions = this.SearchDefinitions.entitlementSearchDefinitions;
            List<QueryContainer> shoulds = new List<QueryContainer>();

            bool shouldGetFreeAssets = entitledAssetsSearchType == "free" || entitledAssetsSearchType == "both" ||
                entitledAssetsSearchType == "not_entitled";
            bool shouldGetPaidForAssets = entitledAssetsSearchType == "entitled" || entitledAssetsSearchType == "both" ||
                entitledAssetsSearchType == "not_entitled" || entitledAssetsSearchType == "entitledsubscriptions";

            if (shouldGetFreeAssets)
            {
                var isFreeTerm = new TermQuery()
                {
                    Field = "is_free",
                    Value = true
                };
                shoulds.Add(isFreeTerm);

                if (entitlementSearchDefinitions.fileTypes != null &&
                    entitlementSearchDefinitions.fileTypes.Count > 0)
                {
                    var fileTypeTerm = new TermsQuery()
                    {
                        Field = "free_file_types",
                        Terms = entitlementSearchDefinitions.fileTypes.Cast<object>()
                    };
                    shoulds.Add(fileTypeTerm);
                }
            }

            // EPG Channel IDs
            if (entitlementSearchDefinitions.epgChannelIds != null)
            {
                var channelsTerms = new TermsQuery()
                {
                    Field = "epg_channel_id",
                    Terms = entitlementSearchDefinitions.epgChannelIds.Cast<object>()
                };

                shoulds.Add(channelsTerms);
            }

            if (shouldGetPaidForAssets)
            {
                BoolQuery entitledPaidForAssetsBoolQuery = null;

                if (entitlementSearchDefinitions.entitledPaidForAssets != null)
                {
                    entitledPaidForAssetsBoolQuery = BuildEntitledPaidForAssetsBoolQuery(entitlementSearchDefinitions);
                }

                // if we want ONLY ENTITLED assets
                // and user has no entitlements:
                // create an empty, dummy query
                if (!shouldGetFreeAssets && SubscriptionsQuery == null && entitledPaidForAssetsBoolQuery == null)
                {
                    var emptyTerm = new TermQuery() { Field = "_id", Value = -1 };
                    shoulds.Add(emptyTerm);
                }
                else
                {
                    if (SubscriptionsQuery != null)
                    {
                        shoulds.Add(SubscriptionsQuery);
                    }

                    if (entitledPaidForAssetsBoolQuery != null)
                    {
                        shoulds.Add(entitledPaidForAssetsBoolQuery);
                    }
                }
            }

            var boolShouldsQuery = new BoolQuery()
            {
                Should = shoulds
            };

            if (entitledAssetsSearchType == "not_entitled")
            {
                result = new BoolQuery()
                {
                    MustNot = new List<QueryContainer>() { boolShouldsQuery }
                };
            }
            else
            {
                result = boolShouldsQuery;
            }

            return result;
        }

        private BoolQuery BuildEntitledPaidForAssetsBoolQuery(EntitlementSearchDefinitions entitlementSearchDefinitions)
        {
            List<QueryContainer> assetsShoulds = new List<QueryContainer>();

            bool isValid = false;
            // Build terms of assets (PPVs) the user purchased and is entitled to watch
            foreach (var item in entitlementSearchDefinitions.entitledPaidForAssets)
            {
                List<QueryContainer> currentItemContainers = new List<QueryContainer>();

                switch (item.Key)
                {
                    case ApiObjects.eAssetTypes.EPG:
                        {
                            currentItemContainers.Add(_nestEpgQueries.GetEpgPrefixQuery());
                            var terms = new TermsQuery()
                            {
                                Field = "epg_id",
                                Terms = item.Value
                            };
                            currentItemContainers.Add(terms);
                            break;
                        }
                    case ApiObjects.eAssetTypes.MEDIA:
                        {
                            currentItemContainers.Add(_nestMediaQueries.GetMediaPrefixQuery());
                            var terms = new TermsQuery()
                            {
                                Field = "media_id",
                                Terms = item.Value
                            };
                            currentItemContainers.Add(terms);
                            break;
                        }
                    case ApiObjects.eAssetTypes.UNKNOWN:
                    case ApiObjects.eAssetTypes.NPVR:
                    default:
                        break;
                }

                var currentItemBoolQuery = new BoolQuery()
                {
                    Must = currentItemContainers
                };
                assetsShoulds.Add(currentItemBoolQuery);
                isValid = true;
            }

            if (!isValid)
            {
                return null;
            }

            var entitledPaidForAssetsBoolQuery = new BoolQuery()
            {
                Should = assetsShoulds
            };
            return entitledPaidForAssetsBoolQuery;
        }
        
        #endregion

        #region Common Queries

        private QueryContainerDescriptor<T> CreateSpecificAssetContainer<T>(
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            eAssetTypes assetTypeHandle)
            where T : class
        {
            if (unifiedSearchDefinitions.specificAssets == null)
                return null;

            bool getResult = unifiedSearchDefinitions.specificAssets.TryGetValue(assetTypeHandle, out var ids);

            if (!getResult || ids == null || !ids.Any())
                return null;

            var specificAssetsContainer = new QueryContainerDescriptor<T>();
            var queryContainers = new List<QueryContainer>();

            string field = "media_id";

            switch (assetTypeHandle)
            {
                case eAssetTypes.EPG:
                    field = "epg_id";
                    break;
                case eAssetTypes.NPVR:
                    field = "recording_id";
                    break;
                default:
                    break;
            }

            var termQueryDescriptor = new TermsQuery() { Field = field, Terms = ids };
            queryContainers.Add(termQueryDescriptor);

            specificAssetsContainer.Bool(b => b.Must(queryContainers.ToArray()));
            return specificAssetsContainer;
        }

        private QueryContainerDescriptor<T> CreateExcludedAssetsContainer<T>(
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            eAssetTypes assetTypeHandle)
            where T : class
        {
            var ids = unifiedSearchDefinitions.excludedAssets?[assetTypeHandle]
                // ES V7 has an identifier with language postfix, be aware.
                .Select(x => $"{x}_{unifiedSearchDefinitions.langauge.Code}")
                .ToArray();

            if (ids == null || !ids.Any())
                return null;

            var specificAssetsContainer = new QueryContainerDescriptor<T>();
            var queryContainers = new List<QueryContainer>();

            var termQueryDescriptor = new TermsQuery() { Field = "_id", Terms = ids };
            queryContainers.Add(termQueryDescriptor);

            specificAssetsContainer.Bool(b => b.MustNot(queryContainers.ToArray()));
            return specificAssetsContainer;
        }

        #endregion

        public IEnumerable<string> GetFields()
        {
            string language = this.SearchDefinitions.langauge != null ? this.SearchDefinitions.langauge.Code : string.Empty;

            var fields = DEFAULT_RETURN_FIELDS.ToHashSet();
            fields.UnionWith(SearchDefinitions.extraReturnFields);
            fields = fields.Select(field => GetExtraFieldName(language, field)).ToHashSet();

            var epg_id_field = "epg_id";

            if (SearchDefinitions.shouldSearchEpg)
            {
                fields.Add(epg_id_field);
            }

            if (SearchDefinitions.shouldSearchMedia)
            {
                fields.Add("media_id");
            }

            if (SearchDefinitions.shouldSearchRecordings)
            {
                if (!fields.Contains(epg_id_field))
                {
                    fields.Add(epg_id_field);
                }

                fields.Add("recording_id");
            }


            if (SearchDefinitions.EpgFeatureVersion != EpgFeatureVersion.V1)
            {
                var doc_id_field = "cb_document_id";
                if (!fields.Contains(doc_id_field))
                {
                    fields.Add(doc_id_field);
                }
            }

            fields.UnionWith(EsSortingService.BuildExtraReturnFields(OrderByFields));

            return new List<string>(fields);
        }

        internal static string GetExtraFieldName(string language, string field)
        {
            if (EXTRA_FIELDS_WITH_LANGUAGE_PREFIX.Contains(field))
            {
                return $"{field}.{language}";
            }
            else if (field.StartsWith("metas."))
            {
                return $"metas.{language}.{field.Substring(6)}";
            }
            else if (field.StartsWith("tags."))
            {
                return $"tags.{language}.{field.Substring(5)}";
            }

            return field;
        }

        public SearchDescriptor<NestBaseAsset> SetFields(SearchDescriptor<NestBaseAsset> searchRequest)
        {
            var returnFields = GetFields();
            searchRequest = searchRequest.Source(source => source.Includes(fields => fields.Fields(returnFields.ToArray())));
            searchRequest.Fields(fields => fields.Fields(returnFields.ToArray()));
            return searchRequest;
        }

        public SortDescriptor<NestBaseAsset> GetSort() => EsSortingService.GetSortingV7(OrderByFields, ShouldUseFunctionScore());

        public bool ShouldUseFunctionScore() => SearchDefinitions.PriorityGroupsMappings?.Count > 0 || SearchDefinitions.boostScoreValues?.Count > 0;

        public override void SetPagingForUnifiedSearch() => QueryInitializer.SetPagingForUnifiedSearch(this);

        public override void SetGroupByValuesForUnifiedSearch()
        {
            if (SearchDefinitions.groupBy != null && SearchDefinitions.groupBy.Any())
            {
                var language = SearchDefinitions.langauge.Code;

                foreach (var groupBy in SearchDefinitions.groupBy)
                {
                    SetGroupByValue(groupBy, language);
                }

                if (SearchDefinitions.distinctGroup != null)
                {
                    SetGroupByValue(SearchDefinitions.distinctGroup, language);
                    SearchDefinitions.extraReturnFields.Add(NamingHelper.GetExtraFieldName(SearchDefinitions.distinctGroup.Key, SearchDefinitions.distinctGroup.Type));
                }
            }
        }

        private static void SetGroupByValue(GroupByDefinition groupBy, string language)
        {
            var key = groupBy.Key.ToLower();
            var type = groupBy.Type;
            string value = GetElasticsearchFieldName(language, key, type);

            // hmmm?
            //if (groupBy.Type == eFieldType.Tag || groupBy.Type == eFieldType.StringMeta)
            //{
            //    value = $"{value}.lowercase";
            //}

            groupBy.Value = value;
        }
    }
}
