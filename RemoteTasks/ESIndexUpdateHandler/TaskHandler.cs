using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;

namespace ESIndexUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.DebugFormat("starting update request. data={0}", data);

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
                        log.DebugFormat("Successfully perform {0} action on asset of type {1} with id: [{2}] did not finish successfully", request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocIDs));
                    }
                    else
                    {
                        log.ErrorFormat("Failed perform {0} action on asset of type {1} with id: [{2}] did not finish successfully", request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocIDs));
                        throw new Exception(string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.", request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocIDs)));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while perform action. data: {0}, Exception:{1}", data, ex);
                throw ex;
            }

            return res;
        }
    }
}
