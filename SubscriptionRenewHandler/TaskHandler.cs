using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using SubscriptionRenewHandler.WS_CAS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;
using System.Net;
using System.Web;
using System.ServiceModel;

namespace SubscriptionRenewHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("Renew subscription request. data={0}", data);

                SubscriptionRenewRequest request = JsonConvert.DeserializeObject<SubscriptionRenewRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                using (module cas = new module())
                {
                    if (!string.IsNullOrEmpty(url))
                        cas.Url = url;

                    bool success = cas.Renew(username, password, request.SiteGuid, request.PurchaseId, request.BillingGuid, request.EndDate);

                    if (!success)
                    {
                        throw new Exception(string.Format("Renew subscription request did not finish successfully. Purchase id = {0}, siteguid = {1}, BillingGuid = {2}, EndDate = {3}.",
                            request != null ? request.PurchaseId : 0,                                            // {0}
                            request != null && request.SiteGuid != null ? request.SiteGuid : string.Empty,       // {1}
                            request != null && request.BillingGuid != null ? request.BillingGuid : string.Empty, // {2}
                            request != null ? request.EndDate : 0));                                             // {3}
                    }
                }

                result = "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}

namespace SubscriptionRenewHandler.WS_CAS
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}


