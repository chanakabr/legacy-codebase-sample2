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

namespace MessageReminderHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting message reminder request. data={0}", data);

                MessageReminderRequest request = JsonConvert.DeserializeObject<MessageReminderRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("ws_notifications");

                if (string.IsNullOrEmpty(url))
                {
                    log.Error("did not find ws_notifications URL");
                    throw new Exception("did not find ws_notifications URL");
                }

                bool success = Core.Notification.Module.SendMessageReminder(request.GroupId, request.StartTime, request.MessageReminderId);

                if (!success)
                    throw new Exception(string.Format("Message reminder did not finish successfully. data: {0}", data));
                else
                    result = "success";
            }
            catch (Exception ex)
            {
                log.Error("Message reminder did not finish successfully. Exception occured. Data: " + data, ex);
                throw ex;
            }

            return result;
        }
    }
}
