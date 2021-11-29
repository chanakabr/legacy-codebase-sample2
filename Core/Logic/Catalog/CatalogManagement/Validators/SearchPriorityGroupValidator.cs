using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using MetaType = ApiObjects.MetaType;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public class SearchPriorityGroupValidator : ISearchPriorityGroupValidator
    {
        public const int MAX_CONDITION_COUNT = 10;

        private readonly ICatalogManager _catalogManager;

        private static readonly Lazy<SearchPriorityGroupValidator> Lazy = new Lazy<SearchPriorityGroupValidator>(
            () => new SearchPriorityGroupValidator(CatalogManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly IDictionary<string, MetaType> ReservedFieldsWithMetaTypes = new Dictionary<string, MetaType>
        {
            { CatalogLogic.ASSET_TYPE, MetaType.String },
            { CatalogLogic.ENTRY_ID, MetaType.String },
            { CatalogLogic.EXTERNAL_ID, MetaType.String },
            { CatalogLogic.INHERITANCE_POLICY, MetaType.String },
            { CatalogLogic.CREATE_DATE, MetaType.DateTime },
            { CatalogLogic.UPDATE_DATE, MetaType.DateTime },
            { CatalogLogic.CATALOG_START_DATE, MetaType.DateTime },
            { CatalogLogic.FINAL_DATE, MetaType.DateTime },
            { CatalogLogic.IS_ACTIVE, MetaType.Bool },
            { NamingHelper.GEO_BLOCK_FIELD, MetaType.Bool },
            { NamingHelper.PARENTAL_RULES_FIELD, MetaType.Bool },
            { NamingHelper.USER_INTERESTS_FIELD, MetaType.Bool },
            { NamingHelper.AUTO_FILL_FIELD, MetaType.Bool }
        };

        public static SearchPriorityGroupValidator Instance => Lazy.Value;

        public SearchPriorityGroupValidator(ICatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public bool ValidateSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup, out string message)
        {
            if (!string.IsNullOrEmpty(searchPriorityGroup.Criteria?.Value))
            {
                ValidateKsql(groupId, searchPriorityGroup.Criteria.Value, out message);
            }
            else
            {
                message = null;
            }

            return string.IsNullOrEmpty(message);
        }

        private void ValidateKsql(long groupId, string kSql, out string message)
        {
            message = null;

            BooleanPhraseNode node = null;
            var parseStatus = BooleanPhraseNode.ParseSearchExpression(kSql, ref node);
            if (parseStatus.Code == 0)
            {
                var leaves = GetBooleanLeaves(node);
                if (leaves.Count > MAX_CONDITION_COUNT)
                {
                    message = $"SearchPriorityGroup's KSQL can't contain more than {MAX_CONDITION_COUNT} conditions.";
                }
                else
                {
                    var leavesWithMetaType = BuildBooleanLeavesWithMetaType(groupId, leaves);
                    foreach (var leafWithMetaType in leavesWithMetaType)
                    {
                        ValidateLeaf(leafWithMetaType, out message);
                        if (!string.IsNullOrEmpty(message))
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                message = $"SearchPriorityGroup's KSQL {kSql} is invalid.";
            }
        }

        private IReadOnlyCollection<BooleanLeaf> GetBooleanLeaves(BooleanPhraseNode node)
        {
            if (node is BooleanLeaf leafNode)
            {
                return new[] { leafNode };
            }

            if (node is BooleanPhrase phraseNode)
            {
                var nodes = new List<BooleanLeaf>();
                foreach (var item in phraseNode.nodes)
                {
                    nodes.AddRange(GetBooleanLeaves(item));
                }

                return nodes;
            }

            return new BooleanLeaf[0];
        }

        private IEnumerable<BooleanLeafWithMetaType> BuildBooleanLeavesWithMetaType(long groupId, IEnumerable<BooleanLeaf> leaves)
        {
            var lazyCatalogGroupCache = new Lazy<CatalogGroupCache>(
                () => _catalogManager.TryGetCatalogGroupCacheFromCache((int)groupId, out var catalogGroupCache) ? catalogGroupCache : new CatalogGroupCache());

            var result = new List<BooleanLeafWithMetaType>();
            foreach (var leaf in leaves)
            {
                if (CatalogReservedFields.ReservedUnifiedSearchStringFields.Contains(leaf.field))
                {
                    var leafWithStringMetaType = new BooleanLeafWithMetaType(leaf, MetaType.String);
                    result.Add(leafWithStringMetaType);
                }
                else if (CatalogReservedFields.ReservedUnifiedSearchNumericFields.Contains(leaf.field))
                {
                    var leafWithNumberMetaType = new BooleanLeafWithMetaType(leaf, MetaType.Number);
                    result.Add(leafWithNumberMetaType);
                }
                else if (CatalogReservedFields.ReservedUnifiedDateFields.Contains(leaf.field))
                {
                    var leafWithDateTimeType = new BooleanLeafWithMetaType(leaf, MetaType.DateTime);
                    result.Add(leafWithDateTimeType);
                }
                else if (ReservedFieldsWithMetaTypes.TryGetValue(leaf.field, out var metaType))
                {
                    var leafWithMetaType = new BooleanLeafWithMetaType(leaf, metaType);
                    result.Add(leafWithMetaType);
                }
                else
                {
                    var leavesWithMetaTypes = lazyCatalogGroupCache.Value.TopicsMapBySystemNameAndByType.TryGetValue(leaf.field, out var topicsMap)
                        ? topicsMap.Select(x => new BooleanLeafWithMetaType(leaf, x.Value.Type))
                        : new[] { new BooleanLeafWithMetaType(leaf, null) };
                    result.AddRange(leavesWithMetaTypes);
                }
            }

            return result;
        }

        private void ValidateLeaf(BooleanLeafWithMetaType leafWithMetaType, out string message)
        {
            if (leafWithMetaType.MetaType.HasValue)
            {
                switch (leafWithMetaType.MetaType.Value)
                {
                    case MetaType.String:
                    case MetaType.MultilingualString:
                    case MetaType.Bool:
                    case MetaType.Tag:
                        ValidateLeafCondition(leafWithMetaType, new[] { ComparisonOperator.Equals }, out message);
                        break;
                    case MetaType.Number:
                    case MetaType.DateTime:
                        ValidateLeafCondition(leafWithMetaType, new[] { ComparisonOperator.Equals, ComparisonOperator.LessThan, ComparisonOperator.GreaterThan }, out message);
                        break;
                    default:
                        message = $"SearchPriorityGroup's KSQL contains {leafWithMetaType.BooleanLeaf.field} field of unsupported type {leafWithMetaType.MetaType.Value}.";
                        break;
                }
            }
            else
            {
                message = $"SearchPriorityGroup's KSQL contains {leafWithMetaType.BooleanLeaf.field} field of undefined type and can't be validated.";
            }
        }

        private void ValidateLeafCondition(BooleanLeafWithMetaType leafWithMetaType, IReadOnlyCollection<ComparisonOperator> allowedOperators, out string message)
        {
            message = allowedOperators.Contains(leafWithMetaType.BooleanLeaf.operand)
                ? null
                : $"{leafWithMetaType.BooleanLeaf.field} field of {leafWithMetaType.MetaType} type supports only {string.Join(", ", allowedOperators)} operators.";
        }

        private class BooleanLeafWithMetaType
        {
            public BooleanLeaf BooleanLeaf { get; }
            public MetaType? MetaType { get; }

            public BooleanLeafWithMetaType(BooleanLeaf booleanLeaf, MetaType? metaType)
            {
                BooleanLeaf = booleanLeaf;
                MetaType = metaType;
            }
        }
    }
}