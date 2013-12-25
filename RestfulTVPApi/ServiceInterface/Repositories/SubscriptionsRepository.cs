using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class SubscriptionsRepository : ISubscriptionsRepository
    {
        public bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID)
        {
            bool response = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                response = new ApiConditionalAccessService(groupId, initObj.Platform).CancelSubscription(initObj.SiteGuid, sSubscriptionID, sSubscriptionPurchaseID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, initObj.SiteGuid, sExtraParameters, sUDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public List<Media> GetMediasInPackage(InitializationObject initObj, int baseID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInPackage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetMediasInPackage(initObj, baseID, mediaType, groupID, picSize, pageSize, pageIndex);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] subIDs, string picSize, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy)
        {
            List<Media> lstMedia = new List<Media>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.SearchMediaBySubIDs(initObj, subIDs, picSize, groupID, orderBy);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs)
        {
            List<SubscriptionPrice> res = new List<SubscriptionPrice>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionDataPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                foreach (int subID in subIDs)
                {
                    var priceObj = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false);

                    res.Add(new SubscriptionPrice
                    {
                        SubscriptionCode = priceObj.m_sObjectCode,
                        Price = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_dPrice,
                        Currency = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_oCurrency.m_sCurrencySign
                    });
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }
    }
}