using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiObjects.Catalog;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class MediaIngestProtectProcessor : IMediaIngestProtectProcessor
    {
        private static readonly Lazy<IMediaIngestProtectProcessor> Lazy = new Lazy<IMediaIngestProtectProcessor>(() => new MediaIngestProtectProcessor(), LazyThreadSafetyMode.PublicationOnly);

        public static IMediaIngestProtectProcessor Instance => Lazy.Value;

        public void ProcessIngestProtect(MediaAsset oldAsset, MediaAsset newAsset, CatalogGroupCache catalogGroupCache)
        {
            if (oldAsset == null)
            {
                return;
            }

            if (!catalogGroupCache.AssetStructsMapById.TryGetValue(oldAsset.MediaType.m_nTypeID, out var assetStruct))
            {
                return;
            }
            
            var protectedTopics = RetrieveProtectedTopics(catalogGroupCache, assetStruct);
            var assetWriteProperties = RetrieveAssetWriteProperties(oldAsset);

            foreach (var protectedTopic in protectedTopics)
            {
                ProtectMeta(newAsset, protectedTopic);
                ProtectTag(newAsset, protectedTopic);
                ProtectProperty(oldAsset, newAsset, assetWriteProperties, protectedTopic);
            }
        }

        private IEnumerable<string> RetrieveProtectedTopics(CatalogGroupCache catalogGroupCache, AssetStruct assetStruct)
        {
            var protectedMetasAndTagsById = assetStruct.AssetStructMetas?.Values
                .Where(x => x.ProtectFromIngest == true)
                .Select(x => x.MetaId)
                .ToArray();
            if (protectedMetasAndTagsById == null || !protectedMetasAndTagsById.Any())
            {
                return Array.Empty<string>();
            }

            var protectedTopics = catalogGroupCache.TopicsMapById
                .Where(x => protectedMetasAndTagsById.Contains(x.Key))
                .Select(x => x.Value.SystemName);

            return protectedTopics;
        }

        private static IDictionary<string, PropertyInfo> RetrieveAssetWriteProperties(Asset asset)
        {
            var publicWriteProperties = asset.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite)
                .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

            return publicWriteProperties;
        }

        private void ProtectMeta(Asset newAsset, string protectedMeta)
        {
            newAsset.Metas.RemoveAll(x => x.m_oTagMeta.m_sName == protectedMeta);
        }

        private void ProtectTag(Asset newAsset, string protectedTag)
        {
            newAsset.Tags.RemoveAll(x => x.m_oTagMeta.m_sName == protectedTag);
        }

        private void ProtectProperty(Asset oldAsset, Asset newAsset, IDictionary<string, PropertyInfo> assetProperties, string protectedProperty)
        {
            if (assetProperties.TryGetValue(protectedProperty, out var propertyInfo))
            {
                var oldValue = propertyInfo.GetValue(oldAsset);
                propertyInfo.SetValue(newAsset, oldValue);
            }
        }
    }
}