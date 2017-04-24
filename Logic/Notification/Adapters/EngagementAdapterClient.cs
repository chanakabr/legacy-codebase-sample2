using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APILogic.EngagementAdapterService;
using ApiObjects.Notification;
using KLogMonitor;
using Newtonsoft.Json;

namespace APILogic.Notification.Adapters
{
    public class EngagementAdapterClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool SendConfigurationToAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            try
            {
                if (engagementAdapter == null || string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
                {
                    log.ErrorFormat("Adapter URL was not found. group ID: {0}, adapter: {1}",
                        groupId,
                        engagementAdapter != null ? JsonConvert.SerializeObject(engagementAdapter) : "null");
                    return false;
                }

                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(engagementAdapter.ID, engagementAdapter.Settings != null ? string.Concat(engagementAdapter.Settings.Select(s => string.Concat(s.Key, s.Value))) : string.Empty,
                    groupId, unixTimestamp);

                using (APILogic.EngagementAdapterService.ServiceClient client = new APILogic.EngagementAdapterService.ServiceClient(string.Empty, engagementAdapter.AdapterUrl))
                {
                    APILogic.EngagementAdapterService.AdapterStatus adapterResponse = client.SetConfiguration(
                        engagementAdapter.ID,
                        engagementAdapter.ProviderUrl,
                        engagementAdapter.Settings != null ? engagementAdapter.Settings.Select(s => new APILogic.EngagementAdapterService.KeyValue() { Key = s.Key, Value = s.Value }).ToArray() : null,
                        groupId,
                        unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(engagementAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null && adapterResponse.Code == (int)EngagementAdapterStatus.OK)
                    {
                        log.DebugFormat("Successfully set configuration of engagement adapter. Result: AdapterID = {0}, AdapterStatus = {1}", engagementAdapter.ID, adapterResponse.Code);
                        return true;
                    }
                    else
                    {
                        log.ErrorFormat("Failed to set engagement Adapter configuration. Result: AdapterID = {0}, AdapterStatus = {1}",
                            engagementAdapter.ID, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendConfigurationToAdapter Failed: AdapterID = {0}, ex = {1}", engagementAdapter.ID, ex);
            }

            return false;
        }

        public static List<int> GetAdapterList(int groupId, EngagementAdapter engagementAdapter, string adapterDynamicData)
        {
            APILogic.EngagementAdapterService.EngagementResponse adapterResponse = new EngagementAdapterService.EngagementResponse() { Status = new AdapterStatus() };

            try
            {
                // validate adapter exists
                if (engagementAdapter == null || string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
                {
                    log.ErrorFormat("Adapter URL was not found. group ID: {0}, adapter: {1}",
                        groupId,
                        engagementAdapter != null ? JsonConvert.SerializeObject(engagementAdapter) : "null");
                    return null;
                }

                long unixTimeNow = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                // set signature
                string signature = string.Concat(engagementAdapter.ID, adapterDynamicData, unixTimeNow);

                using (APILogic.EngagementAdapterService.ServiceClient client = new APILogic.EngagementAdapterService.ServiceClient(string.Empty, engagementAdapter.AdapterUrl))
                {
                    // get list
                    adapterResponse = client.GetList(engagementAdapter.ID, adapterDynamicData, unixTimeNow, signature);
                    if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == (int)EngagementAdapterStatus.OK)
                    {
                        log.DebugFormat("Successfully received engagement Adapter List Result: AdapterID = {0}, AdapterStatus = {1}, number of results: {2}",
                                        engagementAdapter.ID,
                                        ((EngagementAdapterStatus)adapterResponse.Status.Code).ToString(),
                                        adapterResponse.UserIds != null ? adapterResponse.UserIds.Count() : 0);
                    }
                    else
                    {
                        log.ErrorFormat("Engagement Adapter GetList error. Adapter: {0}, result: {1}",
                                 JsonConvert.SerializeObject(engagementAdapter),
                                 adapterResponse != null ? JsonConvert.SerializeObject(adapterResponse) : "null");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAdapterList Failed: AdapterID = {0}, ex = {1}", engagementAdapter.ID, ex);
            }

            return adapterResponse.UserIds != null ? adapterResponse.UserIds.ToList() : null;
        }
    }
}
