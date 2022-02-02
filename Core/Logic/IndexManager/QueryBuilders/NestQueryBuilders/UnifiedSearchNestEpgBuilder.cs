using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders.Queries;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Phx.Lib.Log;
using Nest;

namespace ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders
{
    public class UnifiedSearchNestEpgBuilder: IUnifiedSearchNestBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());


        private readonly NestEpgQueries _nestEpgQueries;

        private readonly NestBaseQueries _nestBaseQueries;

        public UnifiedSearchNestEpgBuilder()
        {
            _nestEpgQueries = new NestEpgQueries();
            _nestBaseQueries = new NestBaseQueries();
        }


        public bool MinimizeQuery{ get; set; }
        
        public EpgSearchObj Definitions { get; set; }

        public eQueryType QueryType { get; set; }

        
        public List<string> GetIndices()
        {
            var indices = new List<string>();
            indices.Add(NamingHelper.GetEpgIndexAlias(Definitions.m_nGroupID));
            return indices;
        }

        public AggregationDictionary GetAggs()
        {
            return new AggregationDictionary();
        }

        public SearchDescriptor<T> SetSizeAndFrom<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            var pageSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            searchDescriptor = searchDescriptor.From(0).Size(pageSize);
            return searchDescriptor;
        }

        public QueryContainer GetQuery()
        {
            var mainDescriptor = new QueryContainerDescriptor<NestEpg>();
            var mainEpgQuery = GetMainEpgQuery(Definitions);
            return mainDescriptor.Bool(b => b.Must(mainEpgQuery));
        }

        public SearchDescriptor<NestBaseAsset> SetFields(SearchDescriptor<NestBaseAsset> searchRequest)
        {
            throw new NotImplementedException();
        }

        public SortDescriptor<NestBaseAsset> GetSort()
        {
            /*var order = Definitions.m_sOrderBy;
            var languageCode = Definitions..Code;
            return _nestBaseQueries.GetSortDescriptor(order,languageCode);*/
            return null;
        }

        private QueryContainer GetMainEpgQuery(EpgSearchObj definitions)
        {
            var queryContainerDescriptor = new QueryContainerDescriptor<NestEpg>();

            if (CalcNumOfRequests() == 1 && !definitions.m_bSearchOnlyDatesAndChannels)
            {
                var wildCardsShould = new List<QueryContainer>();
                var wildCardsMust = new List<QueryContainer>();

                var wildCardsOr = _nestEpgQueries.GetWildCardSearchOr(definitions);
                if (wildCardsOr != null)
                {
                    wildCardsShould.AddRange(wildCardsOr);
                }

                var wildCardsAnd = _nestEpgQueries.GetWildCardSearchAnd(definitions);
                if (wildCardsAnd != null)
                {
                    wildCardsMust.AddRange(wildCardsAnd);
                }

                return new QueryContainerDescriptor<NestEpg>().Bool(x =>
                    x.Must(wildCardsMust.ToArray())
                        .Should(wildCardsOr.ToArray())
                );
            }

            if (definitions.m_bIsCurrent)
            {
                //todo gil wtf? we need this we dont really use the epgChannelId here
                var startDate = DateTime.UtcNow;
                var isActiveTerm = _nestBaseQueries.GetIsActiveTerm();
                var should = new List<QueryContainer>();
                var must = new List<QueryContainer>();

                definitions.m_oEpgChannelIDs.ForEach(epgChannelId =>
                {
                    //prev date
                    var prevDateRange =
                        new QueryContainerDescriptor<NestEpg>().DateRange(x =>
                            x.Field(f => f.StartDate).LessThanOrEquals(startDate));
                    var isActivePrevQueryContainer =
                        new QueryContainerDescriptor<NestEpg>().Bool(x => x.Must(isActiveTerm, prevDateRange));
                    should.Add(isActivePrevQueryContainer);

                    //current date
                    var currDateRange =
                        new QueryContainerDescriptor<NestEpg>().DateRange(x =>
                            x.Field(f => f.StartDate).GreaterThan(startDate));
                    
                    var isActiveCurrQueryContainer =
                        new QueryContainerDescriptor<NestEpg>().Bool(x => x.Must(isActiveTerm, currDateRange));
                    should.Add(isActiveCurrQueryContainer);

                    var queryContainer = new QueryContainerDescriptor<NestEpg>().Bool(b =>
                        b.Filter(x => x.Ids(i => i.Values(epgChannelId))
                            , b1 => b1.Bool(b2 => b2.Should(should.ToArray()))
                        )
                    );

                    must.Add(queryContainer);
                });

                return new QueryContainerDescriptor<NestEpg>().Bool(x => x.Must(must.ToArray()));
            }
            
            var shouldContainer = new List<QueryContainer>();
            var mustContainer = new List<QueryContainer>();

            var startDateRange = new QueryContainerDescriptor<NestEpg>().DateRange(x =>
                x.Field(f => f.StartDate)
                    .LessThanOrEquals(definitions.m_dEndDate)
                    .GreaterThanOrEquals(definitions.m_dStartDate)
            );
            
            shouldContainer.Add(startDateRange);
            
            var endDateRange = new QueryContainerDescriptor<NestEpg>().DateRange(x =>
                x.Field(f => f.EndDate)
                    .LessThanOrEquals(definitions.m_dEndDate)
                    .GreaterThanOrEquals(definitions.m_dStartDate)
            );
            shouldContainer.Add(endDateRange);

            
            if (definitions.m_bSearchEndDate)
            {
                var searchEndDateRange = new QueryContainerDescriptor<NestEpg>().DateRange(x =>
                    x.Field(f => f.SearchEndDate)
                        .GreaterThan(definitions.m_dSearchEndDate)
                );
                mustContainer.Add(searchEndDateRange);
            }

            var activeTerm = _nestBaseQueries.GetIsActiveTerm();
            mustContainer.Add(activeTerm);

            return new QueryContainerDescriptor<NestEpg>().Bool(x =>
                x.Must(mustContainer.ToArray()).Should(shouldContainer.ToArray()));
        }

        private int CalcNumOfRequests()
        {
            var count = 1;
            if (!Definitions.m_bSearchOnlyDatesAndChannels)
            {
                return 1;
            }

            if (Definitions.m_bIsCurrent)
            {
                // 2 es requests for each epg channel id
                return Definitions.m_oEpgChannelIDs.Count << 1;
            }
            
            // es request per epg channel id
            return Definitions.m_oEpgChannelIDs.Count;
        }


    } }