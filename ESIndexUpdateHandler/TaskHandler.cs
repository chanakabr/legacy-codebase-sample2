using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESIndexUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting update request. data=", data), "ESUpdateHandler");

                DocsUpdateRequest request = JsonConvert.DeserializeObject<DocsUpdateRequest>(data);

                Updaters.IUpdateable ESUpdater = Updaters.UpdaterFactory.CreateUpdater(request.GroupID, request.Type);

                if (ESUpdater != null)
                {
                    ESUpdater.Action = request.Action;
                    ESUpdater.IDs = request.DocIDs;

                    bool bResult = ESUpdater.Start();

                    if (bResult)
                    {
                        res = "success";
                    }
                    else
                    {
                        throw new Exception(string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.", request.Action.ToString(), request.Type.ToString(), string.Join(",",request.DocIDs)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return res;
        }
    }
}
