using System;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.NEST;
using Nest;

namespace ApiLogic.IndexManager.Models
{
    public class ExtendedUnifiedSearchResult
    {
        private readonly Lazy<long> _assetId;
        
        public UnifiedSearchResult Result { get; }

        public EsAssetAdapter DocAdapter { get; }

        public ExtendedUnifiedSearchResult(UnifiedSearchResult result, ElasticSearchApi.ESAssetDocument esAssetDocument) : this(result)
        {
            DocAdapter = new EsAssetAdapter(esAssetDocument);
        }

        public ExtendedUnifiedSearchResult(UnifiedSearchResult result, IHit<NestBaseAsset> nestBaseAsset) : this(result)
        {
            DocAdapter = new EsAssetAdapter(nestBaseAsset);
        }

        private ExtendedUnifiedSearchResult(UnifiedSearchResult result)
        {
            Result = result;
            _assetId = new Lazy<long>(() => long.Parse(result.AssetId));
        }

        public long AssetId => _assetId.Value;
    }
}
