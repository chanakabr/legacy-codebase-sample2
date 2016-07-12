using Newtonsoft.Json;
using PendingTransactionHandler.WS_CAS;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvinciCache;
using TVinciShared;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using System.Net;
using System.Web;

namespace PendingTransactionHandler
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
                log.DebugFormat("starting pending charge request. data={0}", data);
                
                PendingTransactionRequest request = JsonConvert.DeserializeObject<PendingTransactionRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                bool success = false;
                
                Status status = cas.CheckPendingTransaction(username, password, 
                    request.PaymentGatewayPendingId, request.NumberOfRetries, request.BillingGuide, request.PaymentGatewayTransactionId,
                    request.SiteGuid,request.ProductId, request.ProductType);

                if (status != null && status.Code == 0)
                {
                    success = true;
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Pending charge request on {0} did not finish successfully.", request.PaymentGatewayPendingId));
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

namespace PendingTransactionHandler.WS_CAS
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
            {
                request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
            }
            return request;
        }
    }
}