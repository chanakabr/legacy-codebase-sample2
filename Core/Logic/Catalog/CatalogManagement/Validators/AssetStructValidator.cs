using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public class AssetStructValidator : IAssetStructValidator
    {
        private static readonly Lazy<IAssetStructValidator> LazyInstance = new Lazy<IAssetStructValidator>(
            () => new AssetStructValidator(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IAssetStructValidator Instance => LazyInstance.Value;

        public Status ValidateBasicMetaIds(CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, bool isProgramStruct)
        {
            Status result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, eResponseStatus.AssetStructMissingBasicMetaIds.ToString());
            List<long> basicMetaIds = new List<long>();
            if (catalogGroupCache.TopicsMapBySystemNameAndByType != null && catalogGroupCache.TopicsMapBySystemNameAndByType.Count > 0)
            {
                if (isProgramStruct)
                {
                    basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => EpgAssetManager.BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                    && x.Value.ContainsKey(EpgAssetManager.BasicMetasSystemNamesToType[x.Key]))
                                                                                                    .Select(x => x.Value[EpgAssetManager.BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                }
                else
                {
                    basicMetaIds = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => AssetManager.BasicMetasSystemNamesToType.ContainsKey(x.Key)
                                                                                                    && x.Value.ContainsKey(AssetManager.BasicMetasSystemNamesToType[x.Key]))
                                                                                                    .Select(x => x.Value[AssetManager.BasicMetasSystemNamesToType[x.Key]].Id).ToList();
                }

                if (assetStruct.MetaIds != null)
                {
                    List<long> noneExistingBasicMetaIds = basicMetaIds.Except(assetStruct.MetaIds).ToList();
                    if (noneExistingBasicMetaIds == null || noneExistingBasicMetaIds.Count == 0)
                    {
                        result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, string.Format("{0} for the following Meta Ids: {1}",
                                            eResponseStatus.AssetStructMissingBasicMetaIds.ToString(), string.Join(",", noneExistingBasicMetaIds)));
                    }
                }
            }

            return result;
        }

        public Status ValidateNoSystemNameDuplicationOnMetaIds(CatalogGroupCache catalogGroupCache, AssetStruct assetStruct)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            HashSet<string> metaSystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (long metaId in assetStruct.MetaIds)
            {
                if (!catalogGroupCache.TopicsMapById.ContainsKey(metaId))
                {
                    result = new Status((int)eResponseStatus.MetaIdsDoesNotExist, eResponseStatus.MetaIdsDoesNotExist.ToString());
                    return result;
                }

                if (metaSystemNames.Contains(catalogGroupCache.TopicsMapById[metaId].SystemName))
                {
                    result = new Status((int)eResponseStatus.AssetStructMetasConatinSystemNameDuplication, eResponseStatus.AssetStructMetasConatinSystemNameDuplication.ToString());
                    return result;
                }

                metaSystemNames.Add(catalogGroupCache.TopicsMapById[metaId].SystemName);
            }

            return result;
        }
    }
}