using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;
using System.Net;
using System.Web;
using System.ServiceModel;
using ApiObjects;

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

                bool success = false;

                eSubscriptionRenewRequestType requestType = eSubscriptionRenewRequestType.Renew;

                if (request.Type != null && request.Type.HasValue)
                {
                    requestType = request.Type.Value;
                }

                switch (requestType)
                {
                    case eSubscriptionRenewRequestType.Renew:
                        {
                            success = Core.ConditionalAccess.Module.Renew(request.GroupID, request.SiteGuid, request.PurchaseId, request.BillingGuid, request.EndDate);
                            break;
                        }
                    case eSubscriptionRenewRequestType.RenewUnifiedTransaction:
                        {
                            success = Core.ConditionalAccess.Module.RenewUnifiedTransaction(request.GroupID, request.HouseholdId, request.ProcessId, request.EndDate);
                            break;
                        }
                    case eSubscriptionRenewRequestType.Reminder:
                    case eSubscriptionRenewRequestType.GiftCardReminder:
                        {
                            success = Core.ConditionalAccess.Module.GiftCardReminder(request.GroupID, request.SiteGuid, request.PurchaseId, request.BillingGuid, request.EndDate);
                            break;
                        }
                    case eSubscriptionRenewRequestType.Downgrade:
                        {
                            success = Core.ConditionalAccess.Module.HandleDowngrade(request.GroupID, request.SiteGuid, request.PurchaseId);
                            break;
                        }
                    case eSubscriptionRenewRequestType.RenewalReminder:
                        {
                            if (request.ProcessId > 0)
                            {
                                success = Core.ConditionalAccess.Module.UnifiedRenewalReminder(
                                    request.GroupID, request.SiteGuid, request.HouseholdId, request.ProcessId, request.EndDate);
                            }
                            else if (request.PurchaseId > 0)
                            {
                                success = Core.ConditionalAccess.Module.RenewalReminder(request.GroupID, request.SiteGuid, request.PurchaseId, request.EndDate);
                            }
                            break;
                        }
                    case eSubscriptionRenewRequestType.SubscriptionEnds:
                        {
                            success = Core.ConditionalAccess.Module.SubscriptionEnds(request.GroupID, request.SiteGuid, request.HouseholdId, 
                                request.PurchaseId, request.ProcessId, request.EndDate);
                            break;
                        }
                    default:
                        break;
                }

                if (!success)
                {
                    throw new Exception(string.Format("Renew subscription request did not finish successfully. Purchase id = {0}, siteguid = {1}, BillingGuid = {2}, EndDate = {3}, requestType = {4}",
                        request != null ? request.PurchaseId : 0,                                            // {0}
                        request != null && request.SiteGuid != null ? request.SiteGuid : string.Empty,       // {1}
                        request != null && request.BillingGuid != null ? request.BillingGuid : string.Empty, // {2}
                        request != null ? request.EndDate : 0,                                               // {3}
                        requestType));                                                                       // {4}
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