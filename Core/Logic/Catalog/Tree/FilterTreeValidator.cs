using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects;
using ApiObjects.SearchObjects;
using Core.Catalog;

namespace ApiLogic.Catalog.Tree
{
    public class FilterTreeValidator : IFilterTreeValidator
    {
        private readonly IFilterTreeResultProcessor _filterTreeResultProcessor;
        private readonly string _programAssetStructId;

        private static readonly string[] EpgFields =
            new[] { CatalogLogic.EPG_CHANNEL_ID, CatalogLogic.EPG_ID, CatalogLogic.LINEAR_MEDIA_ID, CatalogLogic.RECORDING_ID }
                .Select(_ => _.ToLowerInvariant())
                .ToArray();
        private static readonly string[] MediaFields = new[] { CatalogLogic.MEDIA_ID }.Select(_ => _.ToLowerInvariant()).ToArray();

        public FilterTreeValidator(IFilterTreeResultProcessor filterTreeResultProcessor, long? programAssetStructId)
        {
            _filterTreeResultProcessor = filterTreeResultProcessor;
            _programAssetStructId = programAssetStructId.HasValue ? programAssetStructId.ToString() : string.Empty;
        }

        public IndexesModel ValidateTree(BooleanPhraseNode tree)
        {
            return tree != null ? ReadNode(tree) : null;
        }

        public IndexesModel ValidateTree(BooleanPhraseNode tree, IEnumerable<int> mediaTypes)
        {
            var filterTree = PrepareTreeWithMediaTypes(tree, mediaTypes);
            return ValidateTree(filterTree);
        }

        private IndexesModel ReadNode(BooleanPhraseNode node)
        {
            if (node.type == BooleanNodeType.Parent)
            {
                var phraseNode = (BooleanPhrase)node;
                var results = phraseNode.nodes.Select(ReadNode).ToArray();

                return _filterTreeResultProcessor.ProcessResults(phraseNode.operand, results);
            }

            if (node.type == BooleanNodeType.Leaf)
            {
                var leafNode = (BooleanLeaf)node;
                var indexes = new IndexesModel();
                
                if (EpgFields.Contains(leafNode.field.ToLowerInvariant()))
                {
                    indexes.Indexes |= ElasticSearchIndexes.Epg;
                    return indexes;
                }

                if (MediaFields.Contains(leafNode.field.ToLowerInvariant()))
                {
                    indexes.Indexes |= ElasticSearchIndexes.Media;
                    return indexes;
                }

                if (string.Equals(leafNode.field, CatalogLogic.ASSET_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((string)leafNode.value == UnifiedSearchDefinitions.EPG_ASSET_TYPE.ToString() || (string)leafNode.value == "epg" || (!string.IsNullOrEmpty(_programAssetStructId) && (string)leafNode.value == _programAssetStructId))
                    {
                        indexes.Indexes |= ElasticSearchIndexes.Epg;
                        return indexes;
                    }
                    
                    if((string)leafNode.value == UnifiedSearchDefinitions.RECORDING_ASSET_TYPE.ToString())
                    {
                        return indexes;
                    }

                    indexes.Indexes |= ElasticSearchIndexes.Media;
                    return indexes;
                }

                indexes.Indexes |= ElasticSearchIndexes.Common;
                return indexes;
            }

            return new IndexesModel();
        }
        
        private static BooleanPhraseNode PrepareTreeWithMediaTypes(BooleanPhraseNode tree, IEnumerable<int> mediaTypes)
        {
            if (mediaTypes == null)
            {
                return tree;
            }

            if (!mediaTypes.Any())
            {
                return tree;
            }
            
            var newNodes = new List<BooleanPhraseNode>();
            var assetTypeNodes = mediaTypes.Select(mediaType => new BooleanLeaf("asset_type", mediaType.ToString())).Cast<BooleanPhraseNode>().ToList();

            var assetTypesPhrase = new BooleanPhrase(assetTypeNodes);
            newNodes.Add(assetTypesPhrase);

            if (tree != null)
            {
                newNodes.Add(tree);
            }

            var filterTree = new BooleanPhrase(newNodes, eCutType.And);
            return filterTree;
        }
    }
}
