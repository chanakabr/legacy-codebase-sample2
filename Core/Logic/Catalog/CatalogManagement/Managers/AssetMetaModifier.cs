using System;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System.Reflection;
using System.Threading;
using ApiLogic.Api.Managers;
using System.Collections.Generic;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public interface IAssetMetaModifier
    {
        Dictionary<string, T> ReplaceWithAlias<T>(int groupId, string clientTag, int assetStructId,
            Dictionary<string, T> metas, ICustomFieldsPartnerConfigManager configManager);
    }

    public class AssetMetaModifier : IAssetMetaModifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<AssetMetaModifier> lazy = new Lazy<AssetMetaModifier>(() =>
            new AssetMetaModifier(CatalogManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IAssetMetaModifier Instance { get { return lazy.Value; } }

        private readonly ICatalogManager _catalogManager;

        public AssetMetaModifier(ICatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public Dictionary<string, T> ReplaceWithAlias<T>(int groupId, string clientTag, int assetStructId,
            Dictionary<string, T> metas, ICustomFieldsPartnerConfigManager configManager)
        {
            if (configManager.ExistingClientTag(groupId, clientTag))
            {
                return metas;
            }

            var modifiedMetas = new Dictionary<string, T>();
            _catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache);

            var realAssetStructId = catalogGroupCache.GetRealAssetStructId(assetStructId, out bool isProgramStruct);
            foreach (var meta in metas)
            {
                var assetStructMeta = catalogGroupCache.GetAssetStructMetaBySystemName(groupId, realAssetStructId, meta.Key);
                if (assetStructMeta != null && !string.IsNullOrEmpty(assetStructMeta.Alias) && !string.IsNullOrWhiteSpace(assetStructMeta.Alias))
                {
                    modifiedMetas.Add(assetStructMeta.Alias, meta.Value);
                }
                else
                {
                    modifiedMetas.Add(meta.Key, meta.Value);
                }
            }

            return modifiedMetas;
        }
    }
}
