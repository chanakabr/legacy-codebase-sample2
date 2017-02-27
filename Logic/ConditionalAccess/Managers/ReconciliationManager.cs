using ApiObjects;
using ApiObjects.Response;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.ConditionalAccess
{
    public class ReconciliationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const long DEFAULT_RECONCILIATION_FREQUENCY_SECONDS = 7200;

        public static ApiObjects.Response.Status ReconcileEntitlements(BaseConditionalAccess cas, int groupId, string userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // validate user
            long householdId = 0;
            var userValidStatus = Utils.ValidateUser(groupId, userId, ref householdId);

            if (userValidStatus != ResponseStatus.OK)
            {
                // user validation failed
                response = Utils.SetResponseStatus(userValidStatus);
                log.ErrorFormat("User validation failed: {0}, userId: {1}", response.Message, userId);
                return response;
            }

            // validate household
            if (householdId < 1)
            {
                response.Message = "Illegal household";
                log.ErrorFormat("Error: {0}, userId: {1}", response.Message, userId);
                return response;
            }

            // frequency (tcm)
            long frequency = TVinciShared.WS_Utils.GetTcmIntValue("reconciliation_frequency_seconds");
            if (frequency == 0)
            {
                frequency = DEFAULT_RECONCILIATION_FREQUENCY_SECONDS;
            }

            // get household last reconciliation date
            DateTime? householdLastReconciliationDate = null;
            var householdLastReconciliation = ODBCWrapper.Utils.GetTableSingleVal("domains", "LAST_RECONCILIATION_DATE", "ID", "=", householdId, "USERS_CONNECTION_STRING");
            if (!(householdLastReconciliation is DBNull))
            {
                householdLastReconciliationDate = Convert.ToDateTime(householdLastReconciliation);
            }

            // check if reconciliation is allowed for the household now
            if (householdLastReconciliationDate != null && householdLastReconciliationDate.HasValue && DateTime.UtcNow <= householdLastReconciliationDate.Value.AddSeconds(frequency))
            {
                log.ErrorFormat("Entitlements reconciliation was not done due to frequency. userId = {0}, householdId = {1}, groupId = {2}, householdLastReconciliation = {3}",
                    userId, householdId, groupId, householdLastReconciliationDate);
                response = new ApiObjects.Response.Status((int)eResponseStatus.ReconciliationFrequencyLimitation, "reconciliation too frequent");
                return response;
            }

            // call oss adapter through ws api to get the entitlements
            OSSAdapterEntitlementsResponse entitlementsResponse = null;

            try
            {
                entitlementsResponse = Core.Api.Module.GetExternalEntitlements(groupId, userId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ReconcileEntitlements: Error while calling WS API GetExternalEntitlements. groupId = {0}, userId = {1}", groupId, userId), ex);
                return response;
            }

            // validate response
            if (entitlementsResponse == null || entitlementsResponse.Status == null)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to get entitlements");
                return response;
            }

            if (entitlementsResponse.Status.Code != (int)eResponseStatus.OK)
            {
                response = new ApiObjects.Response.Status(entitlementsResponse.Status.Code, entitlementsResponse.Status.Message);
                return response;
            }

            if (entitlementsResponse.Entitlements != null && entitlementsResponse.Entitlements.Count > 0)
            {
                // handle subscriptions entitlements
                ReconcileSubscriptions(cas, groupId, userId, householdId, entitlementsResponse.Entitlements);

                // handle ppv entitlements
                ReconcilePPVs(cas, groupId, userId, householdId, entitlementsResponse.Entitlements);
            }

            DomainDal.Set_DomainLastReconciliationDate(groupId, householdId, DateTime.UtcNow);

            response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        private static void ReconcilePPVs(BaseConditionalAccess cas, int groupId, string userId, long householdId, List<ExternalEntitlement> entitlements)
        {
            var ppvEntitlementsToInsert = entitlements.Where(e => e.EntitlementType == eTransactionType.PPV).ToList();
            List<EntitlementObject> ppvEntitlementsToDelete = new List<EntitlementObject>();

            // get ppvs ids by product codes if there are external ppv entitlements
            Dictionary<long, PPVModule> ppvsModulesDictionary = GetPpvModulesDataForExternalEntitlements(groupId, userId, ref ppvEntitlementsToInsert);

            var ppvDictionary = DAL.ConditionalAccessDAL.Get_AllUsersEntitlements((int)householdId, null);
            if (ppvDictionary != null && ppvDictionary.Count > 0)
            {
                ExternalEntitlement ppvEntitlement;

                foreach (var ppv in ppvDictionary.Values)
                {
                    ppvEntitlement = ppvEntitlementsToInsert.Where(p => p.ProductId == ppv.ppvCode && p.ContentId == ppv.purchasedAsMediaFileID.ToString()).FirstOrDefault();
                    if (ppvEntitlement != null)
                    {
                        if (DateUtils.UnixTimeStampToDateTime(ppvEntitlement.EndDateSeconds) != ppv.endDate || DateUtils.UnixTimeStampToDateTime(ppvEntitlement.StartDateSeconds) != ppv.startDate)
                        {
                            ppvEntitlement.PurchaseId = ppv.ID;

                            // update ppv
                            UpdateReconciledPPVEntitlement(householdId, ppvEntitlement, ppvsModulesDictionary);
                        }

                        ppvEntitlementsToInsert.Remove(ppvEntitlement);
                    }
                    else
                    {
                        ppvEntitlementsToDelete.Add(ppv);
                    }
                }
            }

            // insert ppvs
            InsertReconciledPPVEntitlements(cas, groupId, userId, householdId, ppvEntitlementsToInsert);

            // delete ppvs
            DeleteReconciledPPVEntitlements(householdId, ppvEntitlementsToDelete);
        }

        private static Dictionary<long, PPVModule> GetPpvModulesDataForExternalEntitlements(int groupId, string userId, ref List<ExternalEntitlement> ppvEntitlementsToInsert)
        {
            Dictionary<long, PPVModule> ppvsModulesDictionary = new Dictionary<long, PPVModule>();

            if (ppvEntitlementsToInsert.Count > 0)
            {
                try
                {
                    // get the ppv modules
                    var ppvModules = Core.Pricing.Module.GetPPVModulesByProductCodes(groupId, ppvEntitlementsToInsert.Select(pe => pe.ProductCode).ToArray());

                    if (ppvModules != null)
                    {
                        // set the ppv module for each entitlement
                        foreach (var ppvEntitlement in ppvEntitlementsToInsert)
                        {
                            var ppvModule = ppvModules.Where(pm => pm.m_Product_Code == ppvEntitlement.ProductCode).FirstOrDefault();
                            if (ppvModule != null)
                            {
                                ppvEntitlement.ProductId = int.Parse(ppvModule.m_sObjectCode);
                                if (!ppvsModulesDictionary.ContainsKey(ppvEntitlement.ProductId))
                                {
                                    ppvsModulesDictionary.Add(ppvEntitlement.ProductId, ppvModule);
                                }
                            }
                            else
                            {
                                ppvEntitlementsToInsert.Remove(ppvEntitlement);
                                log.ErrorFormat("ReconcilePPVs: subscription with productCode = {0} was not found for userId = {1} and will be ignored.", ppvEntitlement.ProductCode, userId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("ReconcilePPVs: Error while calling WSPricing GetPPVModulesByProductCodes. groupId = {0}, userId = {1}", groupId, userId), ex);
                }
            }

            return ppvsModulesDictionary;
        }

        private static void DeleteReconciledPPVEntitlements(long householdId, List<EntitlementObject> ppvsToDelete)
        {
            if (ppvsToDelete.Count > 0)
            {
                if (ConditionalAccessDAL.Delete_PPVPurchases(ppvsToDelete.Select(pi => pi.ID).ToList()))
                {
                    log.ErrorFormat("ReconcileEntitlements: failed to delete PPV entitlements for household = {0}", householdId);
                }
            }
        }

        private static void UpdateReconciledPPVEntitlement(long householdId, ExternalEntitlement ppvToUpdate, Dictionary<long, PPVModule> ppvs)
        {
            if (ppvToUpdate != null)
            {
                DateTime startDate, endDate;
                startDate = ppvToUpdate.StartDateSeconds != 0 ? DateUtils.UnixTimeStampToDateTime(ppvToUpdate.StartDateSeconds) : DateTime.UtcNow;

                if (ppvToUpdate.EndDateSeconds == 0)
                {
                    // get the end date for the ppv module
                    if (ppvs.ContainsKey(ppvToUpdate.ProductId))
                    {
                        endDate = Utils.GetEndDateTime(startDate, ppvs[ppvToUpdate.ProductId].m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                    }
                    else
                    {
                        log.ErrorFormat("ReconcileEntitlements: trying to update dates for PPV module with invalid productId or contentId. productCode = {0}, contentId = {1}, for household = {2}",
                                ppvToUpdate.ProductCode, ppvToUpdate.ContentId, householdId);
                        return;
                    }
                }
                else
                {
                    endDate = DateUtils.UnixTimeStampToDateTime(ppvToUpdate.EndDateSeconds);
                }

                if (!ConditionalAccessDAL.Update_PPVPurchaseDates(ppvToUpdate.PurchaseId, startDate, endDate))
                {
                    log.ErrorFormat("ReconcileEntitlements: failed to update dates for PPV entitlement. ppv purchase id = {0}, for household = {1}",
                                ppvToUpdate.ProductId, householdId);
                }
            }
        }

        private static void InsertReconciledPPVEntitlements(BaseConditionalAccess cas, int groupId, string userId, long householdId, List<ExternalEntitlement> ppvsToInsert)
        {
            if (ppvsToInsert.Count != 0)
            {
                //grant entitlements
                int contentId = 0;

                foreach (var ppv in ppvsToInsert)
                {
                    if (ppv != null && int.TryParse(ppv.ContentId, out contentId))
                    {
                        DateTime? startDate = null;
                        if (ppv.StartDateSeconds != 0)
                            startDate = DateUtils.UnixTimeStampToDateTime(ppv.StartDateSeconds);

                        DateTime? endDate = null;
                        if (ppv.EndDateSeconds != 0)
                            endDate = DateUtils.UnixTimeStampToDateTime(ppv.EndDateSeconds);

                        var res = GrantManager.GrantPPV(cas, groupId, userId, householdId, contentId, (int)ppv.ProductId, string.Empty, string.Empty, false, startDate, endDate);
                        string logString = string.Format("userId = {0}, ppv productCode = {1}, ppv contentId = {2}", userId, ppv.ProductCode, ppv.ContentId);
                        if (res.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("failed to reconcile external PPV entitlement for {0}", logString);
                        }
                        else
                        {
                            log.DebugFormat("Reconciled external PPV entitlement for {0}", logString);
                        }
                    }
                }
            }
        }

        private static void ReconcileSubscriptions(BaseConditionalAccess cas, int groupId, string userId, long householdId, List<ExternalEntitlement> entitlements)
        {
            var subscriptionEntitlementsToInsert = entitlements.Where(e => e.EntitlementType == eTransactionType.Subscription).ToList();
            List<int> subscriptionEntitlementsToDelete = new List<int>();

            // get subscriptions ids by product codes if there are external subscription entitlements
            Dictionary<long, Subscription> subscriptionsDictionary = GetSubscriptionsDataForExternalEntitlements(groupId, userId, ref subscriptionEntitlementsToInsert);

            DataTable dt = DAL.ConditionalAccessDAL.Get_AllSubscriptionsPurchasesByUsersIDsOrDomainID((int)householdId, null, groupId);
            if (dt != null && dt.Rows.Count > 0)
            {
                string subscriptionCode;
                ExternalEntitlement subscription;
                DateTime startDate, endDate;

                foreach (DataRow dr in dt.Rows)
                {
                    subscriptionCode = ODBCWrapper.Utils.GetSafeStr(dr["SUBSCRIPTION_CODE"]);
                    if (!string.IsNullOrEmpty(subscriptionCode))
                    {
                        subscription = subscriptionEntitlementsToInsert.Where(s => s.ProductId.ToString() == subscriptionCode).FirstOrDefault();

                        if (subscription != null)
                        {
                            subscription.PurchaseId = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);
                            startDate = ODBCWrapper.Utils.GetDateSafeVal(dr["START_DATE"]);
                            endDate = ODBCWrapper.Utils.GetDateSafeVal(dr["END_DATE"]);

                            if (DateUtils.UnixTimeStampToDateTime(subscription.EndDateSeconds) != endDate || DateUtils.UnixTimeStampToDateTime(subscription.StartDateSeconds) != startDate)
                            {
                                // update subscription
                                UpdateReconciledSubscriptionEntitlement(householdId, subscription, subscriptionsDictionary);
                            }
                            subscriptionEntitlementsToInsert.Remove(subscription);
                        }
                        else
                        {
                            subscriptionEntitlementsToDelete.Add(ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]));
                        }
                    }
                }
            }

            // insert subscriptions
            InsertReconciledSubscriptionEntitlements(cas, groupId, userId, householdId, subscriptionEntitlementsToInsert);

            // delete subscriptions 
            DeleteReconciledSubscriptionEntitlements(householdId, subscriptionEntitlementsToDelete);
        }

        private static void UpdateReconciledSubscriptionEntitlement(long householdId, ExternalEntitlement subscriptionEntitlementToUpdate, Dictionary<long, Subscription> subscriptions)
        {
            if (subscriptionEntitlementToUpdate != null)
            {
                DateTime startDate, endDate;
                startDate = subscriptionEntitlementToUpdate.StartDateSeconds != 0 ? DateUtils.UnixTimeStampToDateTime(subscriptionEntitlementToUpdate.StartDateSeconds) : DateTime.UtcNow;

                if (subscriptionEntitlementToUpdate.EndDateSeconds == 0)
                {
                    // get the end date for the ppv module
                    if (subscriptions.ContainsKey(subscriptionEntitlementToUpdate.ProductId))
                    {
                        endDate = Utils.CalcSubscriptionEndDate(subscriptions[subscriptionEntitlementToUpdate.ProductId], false, startDate);
                    }
                    else
                    {
                        log.ErrorFormat("ReconcileEntitlements: trying to update dates for subscription but subscription not found. productCode = {0}, contentId = {1}, for household = {2}",
                            subscriptionEntitlementToUpdate.ProductCode, subscriptionEntitlementToUpdate.ContentId, householdId);
                        return;
                    }
                }
                else
                {
                    endDate = DateUtils.UnixTimeStampToDateTime(subscriptionEntitlementToUpdate.EndDateSeconds);
                }

                if (!ConditionalAccessDAL.Update_SubscriptionPurchaseDates(subscriptionEntitlementToUpdate.PurchaseId, startDate, endDate))
                {
                    log.ErrorFormat("ReconcileEntitlements: failed to update dates for subscription. subscription purchase id = {0}, for household = {1}",
                                subscriptionEntitlementToUpdate.ProductId, householdId);
                }
            }
        }

        private static Dictionary<long, Subscription> GetSubscriptionsDataForExternalEntitlements(int groupId, string userId, ref List<ExternalEntitlement> subscriptionEntitlementsToInsert)
        {
            Dictionary<long, Subscription> subscriptionsDictionary = new Dictionary<long, Subscription>();

            if (subscriptionEntitlementsToInsert.Count > 0)
            {
                try
                {
                    // get the subscriptions modules
                    var subscriptions = Core.Pricing.Module.GetSubscriptionsByProductCodes(groupId, subscriptionEntitlementsToInsert.Select(se => se.ProductCode).ToArray());

                    if (subscriptions != null)
                    {
                        // set the subscription for each entitlement
                        foreach (var entitlement in subscriptionEntitlementsToInsert)
                        {
                            var subscription = subscriptions.Where(s => s.m_ProductCode == entitlement.ProductCode).FirstOrDefault();
                            if (subscription != null)
                            {
                                entitlement.ProductId = int.Parse(subscription.m_SubscriptionCode);
                                if (!subscriptionsDictionary.ContainsKey(entitlement.ProductId))
                                {
                                    subscriptionsDictionary.Add(entitlement.ProductId, subscription);
                                }
                            }
                            else
                            {
                                subscriptionEntitlementsToInsert.Remove(entitlement);
                                log.ErrorFormat("ReconcileSubscriptions: subscription with productCode = {0} was not found for userId = {1} and will be ignored.", entitlement.ProductCode, userId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("ReconcileSubscriptions: Error while calling WSPricing GetPPVModulesByProductCodes. groupId = {0}, userId = {1}", groupId, userId), ex);
                }
            }

            return subscriptionsDictionary;
        }

        private static void DeleteReconciledSubscriptionEntitlements(long householdId, List<int> subscriptionsToDelete)
        {
            if (subscriptionsToDelete.Count > 0)
            {
                if (ConditionalAccessDAL.Delete_SubscriptionPurchases(subscriptionsToDelete))
                {
                    log.ErrorFormat("ReconcileEntitlements: failed to delete subscription entitlements for household = {0}", householdId);
                }
            }
        }

        private static void InsertReconciledSubscriptionEntitlements(BaseConditionalAccess cas, int groupId, string userId, long householdId, List<ExternalEntitlement> subscriptionsToInsert)
        {
            if (subscriptionsToInsert.Count > 0)
            {
                //grant entitlements            
                foreach (var subscription in subscriptionsToInsert)
                {
                    if (subscription != null)
                    {
                        DateTime? startDate = null;
                        if (subscription.StartDateSeconds != 0)
                            startDate = DateUtils.UnixTimeStampToDateTime(subscription.StartDateSeconds);

                        DateTime? endDate = null;
                        if (subscription.EndDateSeconds != 0)
                            endDate = DateUtils.UnixTimeStampToDateTime(subscription.EndDateSeconds);
                        var res = GrantManager.GrantSubscription(cas, groupId, userId, householdId, (int)subscription.ProductId, string.Empty, string.Empty, false, 0, startDate, endDate, GrantContext.Grant);
                        string logString = string.Format("userId = {0}, subscriptionId = {1}, subscriptionproductCode = {2}", userId, subscription.ProductId, subscription.ProductCode);
                        if (res.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("failed to reconcile external subscription entitlement for {0}", logString);
                        }
                        else
                        {
                            log.DebugFormat("Reconciled external subscription entitlement for {0}", logString);
                        }
                    }
                }
            }
        }
    }
}
