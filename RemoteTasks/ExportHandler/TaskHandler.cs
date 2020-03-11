using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using TVinciShared;

namespace ExportHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("Export request. data={0}", data);

                // build request object
                ExportRequest request = JsonConvert.DeserializeObject<ExportRequest>(data);

                if (request == null)
                    throw new Exception(string.Format("Failed to desterilized export request. data = {0}", data != null ? data : string.Empty));

                Core.Api.Module.Export(request.GroupId, request.TaskId, request.Version);

                result = "success";
                //if (!success)
                //{
                //    throw new Exception(string.Format("Export request failed. request.TaskId = {0}, request.Version = {1}",
                //        request.TaskId,                                             // {0}
                //        request.Version != null ? request.Version : string.Empty)); // {1}
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
