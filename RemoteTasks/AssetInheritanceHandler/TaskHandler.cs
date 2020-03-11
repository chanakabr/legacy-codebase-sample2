using ApiObjects;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;

namespace AssetInheritanceHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("Asset inheritance request. data={0}", data);

                AssetInheritanceRequest request = JsonConvert.DeserializeObject<AssetInheritanceRequest>(data);

                bool success = false;

                InheritanceType requestType = InheritanceType.AssetStructMeta;

                if (request.Type != null && request.Type.HasValue)
                {
                    requestType = request.Type.Value;
                }

                switch (requestType)
                {
                    case InheritanceType.AssetStructMeta:
                        {
                            InheritanceAssetStructMeta inheritanceAssetStructMeta = JsonConvert.DeserializeObject<InheritanceAssetStructMeta>(request.Data);
                            success = Core.Catalog.CatalogManagement.CatalogManager.HandleHeritage(request.GroupId, inheritanceAssetStructMeta.AssetStructId, inheritanceAssetStructMeta.MetaId, request.UserId);
                            break;
                        }
                    case InheritanceType.ParentUpdate:
                        {
                            InheritanceParentUpdate inheritanceParentUpdate = JsonConvert.DeserializeObject<InheritanceParentUpdate>(request.Data);
                            success = Core.Catalog.CatalogManagement.CatalogManager.HandleParentUpdate(request.GroupId, request.UserId, inheritanceParentUpdate.AssetId, inheritanceParentUpdate.TopicsIds);
                            break;
                        }
                    default:
                        break;
                }

                if (!success)
                {
                    throw new Exception(string.Format("Asset inheritance request did not finish successfully. GroupId = {0}, UserId = {1}, Type = {2}, Data = {3}",
                        request != null ? request.GroupId : 0,
                        request != null ? request.UserId : 0,
                        request != null && request.Type.HasValue ? request.Type.ToString() : string.Empty,
                        request != null ? request.Data : string.Empty));
                }

                result = "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}