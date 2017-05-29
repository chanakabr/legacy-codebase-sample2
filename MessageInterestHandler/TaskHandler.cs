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

namespace MessageInterestHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting message interest request. data={0}", data);

                MessageInterestRequest request = JsonConvert.DeserializeObject<MessageInterestRequest>(data);

                bool success = Core.Notification.Module.SendMessageInterest(request.GroupId, request.StartTime, request.MessageInterestId);

                if (!success)
                    throw new Exception(string.Format("Message interest did not finish successfully. data: {0}", data));
                else
                    result = "success";
            }
            catch (Exception ex)
            {
                log.Error("Message interest did not finish successfully. Exception occurred. Data: " + data, ex);
                throw ex;
            }

            return result;
        }
    }
}
