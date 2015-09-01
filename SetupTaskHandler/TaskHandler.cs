using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SetupTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.InfoFormat("starting setup task. data={0}", data);

                SetupTaskRequest request = JsonConvert.DeserializeObject<SetupTaskRequest>(data);

                bool success = false;

                switch (request.Mission)
                {
                    case ApiObjects.eSetupTask.BuildIPToCountry:
                    {
                        var worker = new IPToCountryIndexBuilder();
                        success = worker.BuildIndex();
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Mission setup task. on {0} did not finish successfully.", request.Mission.ToString()));
                }
                else
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        #endregion
    }
}
