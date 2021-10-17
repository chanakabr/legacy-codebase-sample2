using ApiObjects;
using Core.Api.Managers;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;

namespace GeoRuleUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("Geo asset rule update request. data={0}", data);

                GeoRuleUpdateRequest request = JsonConvert.DeserializeObject<GeoRuleUpdateRequest>(data);

                bool success = AssetRuleManager.Instance.HandleRuleUpdate(request.GroupId, request.AssetRuleId, request.CountriesToRemove, request.RemoveBlocked, request.RemoveAllowed, request.UpdateKsql);

                if (!success)
                {
                    throw new Exception(string.Format("Geo asset rule update request did not finish successfully. GroupId = {0}, AssetRuleId = {1}, CountriesToRemove = {2}, RemoveBlocked = {3}, RemoveAllowed = {4}, UpdateKsql = {5}",
                        request != null ? request.GroupId : 0,
                        request != null ? request.AssetRuleId : 0,
                        request != null ? string.Join(",",request.CountriesToRemove) : string.Empty,
                        request != null ? request.RemoveBlocked : false,
                        request != null ? request.RemoveAllowed : false,
                        request != null ? request.UpdateKsql : false));
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