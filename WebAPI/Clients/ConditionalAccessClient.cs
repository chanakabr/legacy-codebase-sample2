using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class ConditionalAccessClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ConditionalAccessClient()
        {

        }

        #region Properties

        protected WebAPI.ConditionalAccess.module ConditionalAccess
        {
            get
            {
                return (Module as WebAPI.ConditionalAccess.module);
            }
        }

        #endregion

        public bool CancelServiceNow(int groupId, int domain_id, int asset_id, Models.ConditionalAccess.TransactionType transaction_type, bool bIsForce)
        {
            WebAPI.ConditionalAccess.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // convert local enu, to ws enum
                    WebAPI.ConditionalAccess.eTransactionType eTransactionType = Mapper.Map<WebAPI.ConditionalAccess.eTransactionType>(transaction_type);

                    response = ConditionalAccess.CancelServiceNow(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domain_id, asset_id, eTransactionType, bIsForce);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while CancelServiceNow. groupId: {0}, domain_id: {1}, asset_id: {2}, transaction_type: {3}, bIsForce: {4}, exception: {5}", groupId, domain_id, asset_id, transaction_type.ToString(), bIsForce, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool CancelSubscriptionRenewal(int groupId, int domain_id, string subscription_code)
        {
            WebAPI.ConditionalAccess.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.CancelSubscriptionRenewal(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domain_id, subscription_code);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while CancelServiceNow. groupId: {0}, domain_id: {1}, subscription_code: {2}, exception: {3}", groupId, domain_id, subscription_code, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public List<Models.ConditionalAccess.Entitlement> GetUserSubscriptions(int groupId, string user_id)
        {
            List<WebAPI.Models.ConditionalAccess.Entitlement> entitlements = null;
            WebAPI.ConditionalAccess.Entitlement response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetUserSubscriptions(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, user_id);            
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserSubscriptions.  user_id: {0}, exception: {1}", user_id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.entitelments == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            entitlements = Mapper.Map<List<WebAPI.Models.ConditionalAccess.Entitlement>>(response.entitelments);

            return entitlements;
        }
    }
}