using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using TVinciShared;

namespace FreeAssetUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";
            WS_Catalog.IserviceClient client = null;            
            try
            {
                log.InfoFormat("starting free asset index update handler request. data={0}", data);

                FreeAssetUpdateRequest request = JsonConvert.DeserializeObject<FreeAssetUpdateRequest>(data);
                if (request.AssetIds != null && request.AssetIds.Count > 0 && request.GroupID > 0)
                {
                    string sWSURL = WS_Utils.GetTcmConfigValue("WS_Catalog");
                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        client = new WS_Catalog.IserviceClient();
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);
                        bool isUpdateIndexSucceeded = client.UpdateIndex(request.AssetIds.ToArray(), request.GroupID, ApiObjects.eAction.Update);
                        if (isUpdateIndexSucceeded)
                        {
                            res = "success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Couldn't update index on catalog due to the following error {0}", ex.Message), ex);
                throw ex;
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

            return res;
        }
    }
}