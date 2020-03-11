using KLogMonitor;
using System;
using System.Reflection;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Billing;

namespace TVPApiModule.Services
{
    public class ApiBillingService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private TVPPro.SiteManager.TvinciPlatform.Billing.module m_Module;
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private int m_groupID;
        private PlatformType m_platform;

        public ApiBillingService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public AdyenBillingDetail GetLastBillingUserInfo(string siteGuid, int billingMethod)
        {
            AdyenBillingDetail response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetLastBillingUserInfo(m_wsUserName, m_wsPassword, siteGuid, billingMethod);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLastBillingUserInfo, Error Message: {0}, params: siteGuid: {1}", ex.Message, siteGuid);
            }

            return response;
        }

        public string GetClientMerchantSig(string sParamaters)
        {
            string response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetClientMerchantSig(m_wsUserName, m_wsPassword, sParamaters);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetClientMerchantSig, Error Message: {0}, params: sParamaters: {1}", ex.Message, sParamaters);
            }

            return response;
        }

        public AdyenBillingDetail GetLastBillingTypeUserInfo(string sSiteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail lastBillingInfo = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    lastBillingInfo = m_Module.GetLastBillingTypeUserInfo(m_wsUserName, m_wsPassword, sSiteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLastBillingTypeUserInfo, Error Message: {0}, params: sSiteGuid: {1}", ex.Message, sSiteGuid);
            }

            return lastBillingInfo;
        }

        public TVPApiModule.Objects.Responses.Billing.PaymentGatewayChargeIdResponse GetHouseholdChargeID(string externalIdentifier, int householdId)
        {
            TVPApiModule.Objects.Responses.Billing.PaymentGatewayChargeIdResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = m_Module.GetHouseholdChargeID(m_wsUserName, m_wsPassword, externalIdentifier, householdId);
                    response = new TVPApiModule.Objects.Responses.Billing.PaymentGatewayChargeIdResponse(result);                    

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetHouseholdChargeID, Error Message: {0}, params: externalIdentifier: {1}, householdId: {2} ",
                    ex.Message, externalIdentifier, householdId);
            }

            return response;
        }

        public ClientResponseStatus SetHouseholdChargeID(string externalIdentifier, int householdId, string chargeID)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = m_Module.SetHouseholdChargeID(m_wsUserName, m_wsPassword, externalIdentifier, householdId, chargeID);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetHouseholdChargeID, Error Message: {0}, params: externalIdentifier: {1}, householdId: {2}, chargeID: {3} ",
                    ex.Message, externalIdentifier, householdId, chargeID);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");

            }

            return clientResponse;
        }

    }
}
