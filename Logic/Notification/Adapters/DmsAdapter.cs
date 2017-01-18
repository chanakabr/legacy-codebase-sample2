using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using Newtonsoft.Json;
using TVinciShared;
using APILogic.DmsService;

namespace Core.Notification.Adapters
{
    public class DmsAdapter
    {
        private static string DmsAdapterUrlKey = WS_Utils.GetTcmConfigValue("DMS_ADAPTER_URL");
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<PushData> GetPushData(int groupId, List<string> udids)
        {
            List<PushData> pushData = null;

            // validate DMS URL exists
            if (string.IsNullOrEmpty(DmsAdapterUrlKey))
            {
                log.Error("couldn't find DMS_ADAPTER_URL");
                return pushData;
            }

            try
            {
                using (ServiceClient client = new ServiceClient())
                {
                    client.Endpoint.Address = new EndpointAddress(DmsAdapterUrlKey);

                    pushData = client.GetPushData(groupId, udids.ToArray()).ToList();
                    if (pushData == null ||
                        pushData.Count != udids.Count)
                    {
                        log.ErrorFormat("Error while trying to retrieve push data from DMS. udids: {0}", JsonConvert.SerializeObject(udids));
                    }
                    else
                        log.DebugFormat("successfully received push data from DMS.");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to retrieve push data from DMS. udids: {0}, ex: {1}", JsonConvert.SerializeObject(udids), ex);
            }
            return pushData;
        }

        public static PushData GetPushData(int groupId, string udid)
        {
            List<PushData> pushData = GetPushData(groupId, new List<string> { udid });
            if (pushData != null && pushData.Count > 0)
                return pushData[0];
            else
                return null;
        }

        public static bool SetPushData(int groupId, SetPushData pushData)
        {
            bool result = false;

            try
            {
                using (ServiceClient client = new ServiceClient())
                {
                    client.Endpoint.Address = new EndpointAddress(DmsAdapterUrlKey);

                    result = client.SetPushData(groupId, pushData);
                    if (!result)
                        log.ErrorFormat("Error while trying to set push data in the DMS. pushData: {0}", JsonConvert.SerializeObject(pushData));
                    else
                        log.DebugFormat("successfully updated push data in the DMS.");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to set push data in the DMS. pushData: {0}, ex: {1}", JsonConvert.SerializeObject(pushData), ex);
            }
            return result;
        }
    }
}
