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
                log.DebugFormat("starting free asset index update handler request. data={0}", data);
                
                FreeAssetUpdateRequest request = JsonConvert.DeserializeObject<FreeAssetUpdateRequest>(data);
                ElasticSearchHandler.Updaters.IElasticSearchUpdater updater = ElasticSearchHandler.Updaters.UpdaterFactory.CreateUpdater(request.group_id, request.type);

                if (updater != null)
                {
                    updater.Action = ApiObjects.eAction.Update;
                    updater.IDs = request.asset_ids;

                    bool result = updater.Start();

                    if (result)
                    {
                        res = "success";
                    }
                    else
                    {
                        throw new Exception(
                            string.Format("Performing update action on asset of type {0} with id: [{1}] did not finish successfully.", 
                            request.ToString(), string.Join(",", request.asset_ids)));
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