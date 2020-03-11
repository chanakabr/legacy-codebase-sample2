using ApiObjects.Billing;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledPurchaseHandler
{
    public class TaskHandler : ITaskHandler
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting ScheduledPurchaseTaskRequest request. data={0}", data);
                ScheduledPurchaseTaskRequest request = JsonConvert.DeserializeObject<ScheduledPurchaseTaskRequest>(data);

                TransactionResponse transactionResult = Core.ConditionalAccess.Module.Purchase(request.GroupId, request.Siteguid, request.Household, request.Price, request.Currency, request.ContentId,
                                                                                               request.ProductId, request.TransactionType, request.Coupon, request.UserIp, request.DeviceName,
                                                                                               request.PaymentGwId, request.PaymentMethodId, request.adapterData);
                if (transactionResult != null && transactionResult.Status.Code == (int)eResponseStatus.OK)
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Scheduled Purchase with data: {0}", data), ex);
                throw ex;
            }

            return result;
        }

    }
}
