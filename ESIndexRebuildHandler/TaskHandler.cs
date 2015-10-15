using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace ESIndexRebuildHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            log.Debug("Info - " + string.Concat("starting index rebuild request. data=", data));

            try
            {
                IndexBuildRequest request = JsonConvert.DeserializeObject<IndexBuildRequest>(data);

                IndexBuilders.IIndexBuilder oIndexBuilder = IndexBuilders.IndexBuilderFactory.CreateIndexBuilder(request.GroupID, request.Type);

                if (oIndexBuilder != null)
                {
                    oIndexBuilder.SwitchIndexAlias = request.SwitchIndexAlias;
                    oIndexBuilder.DeleteOldIndices = request.DeleteOldIndices;
                    oIndexBuilder.StartDate = request.StartDate;
                    oIndexBuilder.EndDate = request.EndDate;

                    bool bResult = oIndexBuilder.Build();

                    if (bResult)
                    {
                        res = "success";
                    }
                    else
                    {
                        throw new Exception(string.Format("Rebuilding {0} index for group id {1} has failed.", request.Type.ToString(), request.GroupID));
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
