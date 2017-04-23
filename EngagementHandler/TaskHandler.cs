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

             //   success = Core.Notification.Module.InitiateNotificationAction(request.GroupId, action, request.UserId, request.Udid, request.pushToken);

                if (!success)
                {
                    //throw new Exception(string.Format(
                    //    "Engagement did not finish successfully. group: {0} user id: {1} Udid: {2}, push token: {3}", request.GroupId, request.UserId, request.Udid, request.pushToken));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
