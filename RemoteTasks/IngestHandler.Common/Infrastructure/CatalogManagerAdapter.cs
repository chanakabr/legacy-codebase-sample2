using System;
using System.Reflection;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;

namespace IngestHandler.Common.Infrastructure
{
    public class CatalogManagerAdapter : ICatalogManagerAdapter
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool DoesGroupUsesTemplates(int groupId)
        {
            return CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
        }

        public CatalogGroupCache GetCatalogGroupCache(int groupId)
        {
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                var message = $"failed to get catalogGroupCache for groupId: {groupId} when calling UpdateAsset";
                Logger.Error(message);
                throw new Exception(message);
            }

            return catalogGroupCache;
        }
    }
}
