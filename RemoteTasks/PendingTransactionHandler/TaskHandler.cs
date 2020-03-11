using Newtonsoft.Json;
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
using System.ServiceModel;
using ApiObjects.Response;

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

                bool success = false;

                Status status = Core.ConditionalAccess.Module.CheckPendingTransaction(request.GroupID, 
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