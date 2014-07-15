using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESIndexRebuildHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = "failure";

            Logger.Logger.Log("Info", string.Concat("starting index rebuild request. data=", data), "ESBuildHandler");

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
