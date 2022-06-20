using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiLogic.IndexManager.QueryBuilders.SearchPriority.Models;
using ApiObjects.SearchPriorityGroups;
using ElasticSearch.Searcher;
using Newtonsoft.Json.Linq;

namespace ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders
{
    public class PriorityQueryBuilder
    {
        private readonly ESUnifiedQueryBuilder _queryBuilder;
        private readonly IReadOnlyDictionary<double, IEsPriorityGroup> _priorityGroupsMappings;

        public PriorityQueryBuilder(ESUnifiedQueryBuilder queryBuilder, IReadOnlyDictionary<double, IEsPriorityGroup> priorityGroupsMappings)
        {
            _queryBuilder = queryBuilder;
            _priorityGroupsMappings = priorityGroupsMappings;
        }

        public ESFunctionScore Build(IESTerm queryTerm, QueryFilter filter)
        {
            var internalQuery = BuildInternalQuery(queryTerm, filter);
            
            var functions = new List<ESFunctionScoreFunction>();
            foreach (var priorityGroupMapping in _priorityGroupsMappings)
            {
                IESTerm esTerm = null;
                switch (priorityGroupMapping.Value)
                {
                    case KSqlEsPriorityGroup kSqlGroup:
                    {
                        esTerm = _queryBuilder?.ConvertToQuery(kSqlGroup.Tree);
                        break;
                    }
                }

                if (esTerm != null)
                {
                    var function = new ESFunctionScoreFunction
                    {
                        filter = (JObject)JToken.Parse(esTerm.ToString()),
                        weight = priorityGroupMapping.Key
                    };

                    functions.Add(function);   
                }
            }

            return new ESFunctionScore
            {
                query = internalQuery,
                functions = functions,
                max_boost = _priorityGroupsMappings.Max(k => k.Key),
                // We're using eFunctionScoreScoreMode.max score mode here to prevent additional calculations in case document are eligible for more than 1 function score.
                score_mode = eFunctionScoreScoreMode.max,
                boost_mode = eFunctionScoreBoostMode.replace
            };
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
