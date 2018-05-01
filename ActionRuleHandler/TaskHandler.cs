using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ActionRuleHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting action rule request. data={0}", data);

                ActionRuleRequest request = JsonConvert.DeserializeObject<ActionRuleRequest>(data);

                bool apiResult = false;
                switch (request.Type)
                {
                    case ApiObjects.RuleActioType.LifeCycle:
                        apiResult = Core.Api.Module.DoActionRules();
                        break;
                    case ApiObjects.RuleActioType.Asset:
                        apiResult = Core.Api.Module.DoActionAssetRules();
                        break;
                    default:
                        break;
                }

                if (!apiResult)
                {
                    throw new Exception("api.DoActionRules returned false");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed performing action rule request. data = {0} message = {1}, stack trace= {2}, target site = {3}",
                    data, ex.Message, ex.StackTrace, ex.TargetSite), ex);
                throw ex;
            }

            return result;
        }
    }
}
