using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using ApiObjects.Response;
using System.Data;
using System.Reflection;
using System.ServiceModel;

namespace MessageAnnouncementHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting announcement request. data={0}", data);

                MessageAnnouncementRequest request = JsonConvert.DeserializeObject<MessageAnnouncementRequest>(data);

                bool success = Core.Notification.Module.SendMessageAnnouncement(request.GroupId, request.StartTime, request.MessageAnnouncementId);

                if (!success)
                    throw new Exception(string.Format("Announcement did not finish successfully. data: {0}", data));
                else
                    result = "success";
            }
            catch (Exception ex)
            {
                log.Error("Announcement did not finish successfully. Exception occured. data: " + data, ex);
                throw ex;
            }

            return result;
        }
    }
}
