using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FreeAssetUpdateHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.InfoFormat("starting free asset handler request. data={0}", data);

                FreeAssetUpdateRequest request = JsonConvert.DeserializeObject<FreeAssetUpdateRequest>(data);

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed performing update of free asset. data = {0} message = {1}, stack trace= {2}, target site = {3}", 
                    data, ex.Message, ex.StackTrace, ex.TargetSite), ex);
                throw ex;
            }
            
            return res;
        }
    }
}
