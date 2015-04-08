using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Objects.Responses;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;

namespace RestfulTVPApi.Clients
{
    public class BillingClient : BaseClient
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(BillingClient));

        #endregion

        #region C'tor
        public BillingClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
           
        }

        public BillingClient()
        {
            
        }

        #endregion C'tor

        #region Properties

        protected RestfulTVPApi.Billing.module Billing
        {
            get
            {
                return (Module as RestfulTVPApi.Billing.module);
            }
        }

        #endregion

        #region Public methods
        public AdyenBillingDetail GetLastBillingUserInfo(string siteGuid, int billingMethod)
        {
            AdyenBillingDetail response = null;

            response = Execute(() =>
                {
                    var res = Billing.GetLastBillingUserInfo(WSUserName, WSPassword, siteGuid, billingMethod);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as AdyenBillingDetail;

            return response;
        }

        public string GetClientMerchantSig(string sParamaters)
        {
            string response = null;

            response = Execute(() =>
                {
                    response = Billing.GetClientMerchantSig(WSUserName, WSPassword, sParamaters);
                    return response;
                }) as string;

            return response;
        }
        
        public AdyenBillingDetail GetLastBillingTypeUserInfo(string siteGuid)
        {
            AdyenBillingDetail billingDetail = null;

            billingDetail = Execute(() =>
                {
                    var res = Billing.GetLastBillingTypeUserInfo(WSUserName, WSPassword, siteGuid);
                    if (res != null)
                    {
                        billingDetail = res.ToApiObject();
                    }

                    return billingDetail;
                }) as AdyenBillingDetail;

            return billingDetail;
        }
        #endregion
    }
}