using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriority;
using ElasticSearch.Searcher;
using Newtonsoft.Json.Linq;

namespace ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders.SearchPriority
{
    public class PriorityQueryBuilder
    {
        private readonly IReadOnlyDictionary<double, SearchPriorityGroup> _priorityGroupsMapping;
        private readonly ESUnifiedQueryBuilder _queryBuilder;
        private readonly IDictionary<CriteriaType, Func<Criteria, IESTerm>> _functionProcessors;

        private PriorityQueryBuilder()
        {
            _functionProcessors = new Dictionary<CriteriaType, Func<Criteria, IESTerm>>
            {
                {
                    CriteriaType.kSQL, criteria =>
                    {
                        BooleanPhraseNode tree = null;
                        BooleanPhraseNode.ParseSearchExpression(criteria.Value, ref tree);
                        return _queryBuilder?.ConvertToQuery(tree);
                    }
                }
            };
        }

        public PriorityQueryBuilder(ESUnifiedQueryBuilder queryBuilder, IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMapping) : this()
        {
            _queryBuilder = queryBuilder;
            _priorityGroupsMapping = priorityGroupsMapping;
        }

        public ESFunctionScore Build(IESTerm queryTerm, QueryFilter filter)
        {
            var internalQuery = BuildInternalQuery(queryTerm, filter);
            
            var functions = new List<ESFunctionScoreFunction>();
            foreach (var priorityGroup in _priorityGroupsMapping)
            {
                if (!_functionProcessors.TryGetValue(priorityGroup.Value.Criteria.Type, out var processor))
                {
                    continue;
                }
                
                var esTerm = processor(priorityGroup.Value.Criteria);
                var function = new ESFunctionScoreFunction
                {
                    filter = (JObject)JToken.Parse(esTerm.ToString()),
                    weight = priorityGroup.Key
                };

                functions.Add(function);
            }

            return new ESFunctionScore
            {
                query = internalQuery,
                functions = functions,
                max_boost = _priorityGroupsMapping.Max(k => k.Key),
                score_mode = eFunctionScoreScoreMode.max,
                boost_mode = eFunctionScoreBoostMode.replace
            };
        }

        private static IDictionary<double, SearchPriorityGroup> BuildOrderedDic(SearchPriorityGroup[] priorityGroups)
        {
            double maxValue = priorityGroups.Length + 1;
            return priorityGroups.ToDictionary(priorityGroup => maxValue--);
        }

        private static string BuildInternalQuery(IESTerm queryTerm, QueryFilter filter)
        {
            var esFilteredQueryBuilder = new StringBuilder();
            esFilteredQueryBuilder.Append("{ \"filtered\": {");

            if (queryTerm != null && !queryTerm.IsEmpty())
            {
                string queryPart = queryTerm.ToString();

                if (!string.IsNullOrEmpty(queryPart))
                {
                    esFilteredQueryBuilder.AppendFormat(" \"query\": {0},", queryPart);
                }
            }

            esFilteredQueryBuilder.Append(filter);
            esFilteredQueryBuilder.Append(" } }");

            return esFilteredQueryBuilder.ToString();
        }
    }
}
