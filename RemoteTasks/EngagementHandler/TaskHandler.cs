using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using TVinciShared;

namespace EngagementHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting engagement request request. data={0}", data);

                EngagementRequest request = JsonConvert.DeserializeObject<EngagementRequest>(data);

                bool success = false;
                if (request.EngagementBulkId > 0)
                {
                    log.DebugFormat("Calling SendEngagementBulk in remote tasks. Engagement ID: {0}, Engagement Bulk ID :{1}", request.EngagementId, request.EngagementBulkId);
                    success = Core.Notification.Module.SendEngagementBulk(request.GroupId, request.EngagementId, request.EngagementBulkId, request.StartTime);
                }
                else
                {
                    log.DebugFormat("Calling SendEngagement message in remote tasks. Engagement ID: {0}", request.EngagementId, request.EngagementBulkId);
                    success = Core.Notification.Module.SendEngagement(request.GroupId, request.EngagementId, request.StartTime);
                }


                if (!success)
                    throw new Exception(string.Format("Engagement did not finish successfully. Data: {0}", data));
                else
                    result = "success";
            }
            catch (Exception ex)
            {
                log.Error("Engagement did not finish successfully. Exception occurred. Data: " + data, ex);
                throw ex;
            }

            return result;
        }
    }
}
