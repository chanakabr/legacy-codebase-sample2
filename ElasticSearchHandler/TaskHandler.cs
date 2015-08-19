using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ElasticSearchHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.InfoFormat("starting elasticsearch request. data={0}", data);

                ElasticSearchRequest request = JsonConvert.DeserializeObject<ElasticSearchRequest>(data);

                if (request.Action == ApiObjects.eAction.Rebuild)
                {
                    IndexBuilders.IIndexBuilder builder = IndexBuilders.IndexBuilderFactory.CreateIndexBuilder(request.GroupID, request.Type);

                    if (builder != null)
                    {
                        if (request.SwitchIndexAlias.HasValue)
                        {
                            builder.SwitchIndexAlias = request.SwitchIndexAlias.Value;
                        }

                        if (request.DeleteOldIndices.HasValue)
                        {
                            builder.DeleteOldIndices = request.DeleteOldIndices.Value;
                        }

                        builder.StartDate = request.StartDate;
                        builder.EndDate = request.EndDate;

                        bool result = builder.Build();

                        if (result)
                        {
                            res = "success";
                        }
                        else
                        {
                            throw new Exception(string.Format("Rebuilding {0} index for group id {1} has failed.", 
                                request.Type.ToString(), request.GroupID));
                        }
                    }
                }
                else
                {
                    Updaters.IUpdateable updater = Updaters.UpdaterFactory.CreateUpdater(request.GroupID, request.Type);

                    if (updater != null)
                    {
                        updater.Action = request.Action;
                        updater.IDs = request.DocumentIDs;

                        bool result = updater.Start();

                        if (result)
                        {
                            res = "success";
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.",
                                request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs)));
                        }
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
