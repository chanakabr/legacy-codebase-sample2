using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.QueryBuilders.SearchPriority.Models;
using ApiObjects.SearchPriorityGroups;
using Nest;

namespace ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders
{
    public class PriorityQueryBuilder
    {
        private readonly UnifiedSearchNestBuilder _queryBuilder;
        private readonly IReadOnlyDictionary<double,IEsPriorityGroup> _priorityGroupsMappings;

        public PriorityQueryBuilder(UnifiedSearchNestBuilder queryBuilder, IReadOnlyDictionary<double, IEsPriorityGroup> priorityGroupsMappings)
        {
            _queryBuilder = queryBuilder;
            _priorityGroupsMappings = priorityGroupsMappings;
        }

        public QueryContainer Build(QueryContainer mainQuery)
        {
            var functions = new List<IScoreFunction>();
            foreach (var priorityGroupMapping in _priorityGroupsMappings)
            {
                QueryContainer priorityFilter;
                switch (priorityGroupMapping.Value)
                {
                    case KSqlEsPriorityGroup kSqlGroup:
                        priorityFilter = _queryBuilder?.ConvertToQuery(kSqlGroup.Tree);
                        break;
                    default:
                        priorityFilter = null;
                        break;
                }

                if (priorityFilter != null)
                {
                    var function = new WeightFunction
                    {
                        Filter = priorityFilter,
                        Weight = priorityGroupMapping.Key
                    };

                    functions.Add(function);   
                }
            }

            return new FunctionScoreQuery()
            {
                Name = "priority_function_score",
                Query = mainQuery,
                Functions = functions,
                MaxBoost = _priorityGroupsMappings.Max(k => k.Key),
                // We're using eFunctionScoreScoreMode.max score mode here to prevent additional calculations in case document are eligible for more than 1 function score.
                ScoreMode = FunctionScoreMode.Max,
                BoostMode = FunctionBoostMode.Replace
            };
        }
    }
}
