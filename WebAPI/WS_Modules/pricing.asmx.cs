using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using ApiObjects.Response;
using KLogMonitor;
using ApiObjects;
using Core.Pricing;

namespace WS_Pricing
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://pricing.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class mdoule : System.Web.Services.WebService
    {

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        public virtual Currency GetCurrencyValues(string sWSUserName, string sWSPassword, string sCurrencyCode3)
        {
            Currency t = new Currency();
            t.InitializeByCode3(sCurrencyCode3);
            return t;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsList(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsContainingUserTypes(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName, int nIsActive, int[] userTypesIDs)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsContainingUserTypes(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName, nIsActive, userTypesIDs);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsContainingMedia(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nFileTypeID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsContainingMedia(nGroupID, nMediaID, nFileTypeID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual string GetSubscriptionsContainingMediaSTR(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nFileTypeID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsContainingMediaSTR(nGroupID, nMediaID, nFileTypeID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetIndexedSubscriptionsContainingMedia(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nFileTypeID, int count)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetIndexedSubscriptionsContainingMedia(nGroupID, nMediaID, nFileTypeID, count);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsContainingMediaShrinked(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nFileTypeID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsContainingMediaShrinked(nGroupID, nMediaID, nFileTypeID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseSubscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsContainingMediaFile(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nMediaFileID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsContainingMediaFile(nGroupID, nMediaID, nMediaFileID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public virtual ApiObjects.Response.IdsResponse GetSubscriptionIDsContainingMediaFile(string sWSUserName, string sWSPassword, Int32 nMediaID, Int32 nMediaFileID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionIDsContainingMediaFile(nGroupID, nMediaID, nMediaFileID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription[] GetSubscriptionsShrinkList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsShrinkList(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Campaign))]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual Campaign[] GetMediaCampaigns(string sWSUserName, string sWSPassword, int nMediaID
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {

            BaseCampaign t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetMediaCampaigns(nMediaID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Campaign))]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual Campaign[] GetCampaignsByType(string sWSUserName, string sWSPassword, CampaignTrigger triggerType
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {

            BaseCampaign t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCampaignsByType(triggerType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Campaign))]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual Campaign GetCampaignsByHash(string sWSUserName, string sWSPassword, string hashCode)
        {

            BaseCampaign t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCampaignByHash(hashCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Campaign))]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual Campaign GetCampaignData(string sWSUserName, string sWSPassword, long nCampaignID)
        {
            BaseCampaign t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCampaignData(nCampaignID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription GetSubscriptionData(string sWSUserName, string sWSPassword, string sSubscriptionCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionData(nGroupID, sSubscriptionCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Collection GetCollectionData(string sWSUserName, string sWSPassword, string sCollectionCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            BaseCollection t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionData(sCollectionCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Subscription GetSubscriptionDataByProductCode(string sWSUserName, string sWSPassword, string sProductCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionDataByProductCode(nGroupID, sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public int[] GetSubscriptionMediaList(string sWSUserName, string sWSPassword, string sSubscriptionCode,
            Int32 nFileTypeID, string sDevice)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionMediaList(nGroupID, sSubscriptionCode, nFileTypeID, sDevice);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<int> GetSubscriptionMediaList2(string sWSUserName, string sWSPassword, string sSubscriptionCode,
            Int32 nFileTypeID, string sDevice)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionMediaList2(nGroupID, sSubscriptionCode, nFileTypeID, sDevice);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool DoesMediaBelongToSubscription(string sWSUserName, string sWSPassword, string sSubscriptionCode, Int32 nMediaID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.DoesMediaBelongToSubscription(nGroupID, sSubscriptionCode, nMediaID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        public virtual PPVModule[] GetPPVModuleList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleList(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModuleContainer))]
        public virtual PPVModuleContainer[] GetPPVModuleListForAdmin(string sWSUserName, string sWSPassword, Int32 nMediaFileID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleListForAdmin(nGroupID, nMediaFileID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        public virtual DiscountModule[] GetDiscountsModuleListForAdmin(string sWSUserName, string sWSPassword)
        {
            BaseDiscount t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetDiscountsModuleListForAdmin();
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModuleContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFilePPVModule))]
        public virtual MediaFilePPVModule[] GetPPVModuleListForMediaFiles(string sWSUserName, string sWSPassword, Int32[] nMediaFileIDs,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleListForMediaFiles(nGroupID, nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModuleContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFilePPVModule))]
        public virtual MediaFilePPVModule[] GetPPVModuleListForMediaFilesST(string sWSUserName, string sWSPassword,
            string sMediaFileIDsCommaSeperated, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            string[] sSep = { ";" };
            Int32[] nMediaFileIDs = null;
            string[] sMediaIDs = sMediaFileIDsCommaSeperated.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            if (sMediaIDs.Length > 0)
                nMediaFileIDs = new int[sMediaIDs.Length];
            for (int j = 0; j < sMediaIDs.Length; j++)
                nMediaFileIDs[j] = int.Parse(sMediaIDs[j]);

            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleListForMediaFiles(nGroupID, nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModuleContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFilePPVContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModuleWithExpiry))]
        public virtual MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(string sWSUserName, string sWSPassword, Int32[] nMediaFileIDs,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleListForMediaFilesWithExpiry(nGroupID, nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        public virtual PPVModule[] GetPPVModuleShrinkList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleShrinkList(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        public virtual PPVModule GetPPVModuleData(string sWSUserName, string sWSPassword, string sPPVCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleData(nGroupID, sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePrePaidModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidModule))]
        public virtual PrePaidModule GetPrePaidModuleData(string sWSUserName, string sWSPassword, int nPrePaidCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPrePaidModuleData(nGroupID, nPrePaidCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        public virtual PriceCode[] GetPriceCodeList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPriceCodeList(nGroupID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        public virtual PriceCode GetPriceCodeData(string sWSUserName, string sWSPassword, string sPriceCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPriceCodeData(nGroupID, sPriceCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        public virtual DiscountModule GetDiscountCodeData(string sWSUserName, string sWSPassword, string sDiscountCode)
        {
            BaseDiscount t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetDiscountCodeData(sDiscountCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual UsageModule GetUsageModuleData(string sWSUserName, string sWSPassword, string sUsageModuleCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return (new UsageModuleCacheWrapper(t)).GetUsageModuleData(sUsageModuleCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual UsageModule[] GetUsageModuleList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return (new UsageModuleCacheWrapper(t)).GetUsageModuleList();
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual UsageModule GetOfflineUsageModule(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return (new UsageModuleCacheWrapper(t)).GetOfflineUsageModuleData();
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponsGroup GetCouponGroupData(string sWSUserName, string sWSPassword, string sCouponGroupID)
        {
            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCouponGroupData(sCouponGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponsGroup[] GetCouponGroupListForAdmin(string sWSUserName, string sWSPassword)
        {
            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCouponGroupListForAdmin();
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponsGroup[] GetVoucherGroupList(string sWSUserName, string sWSPassword)
        {
            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetCouponGroupListForAdmin(true);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponDataResponse GetCouponStatus(string sWSUserName, string sWSPassword, string sCouponCode)
        {
            CouponDataResponse response = new CouponDataResponse();
            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                CouponData coupon = t.GetCouponStatus(sCouponCode);
                response.Status = new Status((int)eResponseStatus.Error, "Error");

                if (coupon != null)
                {
                    response.Coupon = coupon;
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                    if (coupon.m_CouponStatus == CouponsStatus.NotExists)
                    {
                        response.Status = new Status((int)eResponseStatus.CouponNotValid, "Coupon Not Valid");
                    }
                }
                
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Coupon = new CouponData();
                response.Coupon.Initialize(null, CouponsStatus.NotExists);
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponsStatus SetCouponUsed(string sWSUserName, string sWSPassword, string sCouponCode, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, 0, 0, 0, 0);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return CouponsStatus.NotExists;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        public virtual CouponsStatus SetCouponUses(string sWSUserName, string sWSPassword, string sCouponCode, string sSiteGUID, Int32 nMediaFileID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, nMediaFileID, nSubCode, nCollectionCode, nPrePaidCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return CouponsStatus.NotExists;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TvinciPreviewModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePreviewModule))]
        public virtual PreviewModule GetPreviewModuleByID(string sWSUserName, string sWSPassword, long lPreviewModuleID)
        {
            BasePreviewModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetPreviewModuleByID(lPreviewModuleID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TvinciPreviewModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePreviewModule))]
        public virtual PreviewModule[] GetPreviewModulesArrayByGroupIDForAdmin(string sWSUserName, string sWSPassword)
        {
            BasePreviewModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetPreviewModulesArrayByGroupID(nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UsageModule))]
        public virtual UsageModule GetUsageModule(string sWSUserName, string sWSPassword, string sAssetCode, eTransactionType transactionType)
        {
            BasePreviewModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return t.GetUsageModule(nGroupID, sAssetCode, transactionType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PreviewModule))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsResponse))]
        public virtual SubscriptionsResponse GetSubscriptionsData(string sWSUsername, string sWSPassword, string[] oSubCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            return GetSubscriptions(sWSUsername, sWSPassword, oSubCodes, sCountryCd2, sLanguageCode3, sDeviceName, SubscriptionOrderBy.StartDateAsc);
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PreviewModule))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsResponse))]
        public virtual SubscriptionsResponse GetSubscriptions(string sWSUsername, string sWSPassword, string[] oSubCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = new SubscriptionsResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptions(nGroupID, oSubCodes, sCountryCd2, sLanguageCode3, sDeviceName, orderBy);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Price))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceCode))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePricing))]
        [System.Xml.Serialization.XmlInclude(typeof(DiscountModule))]
        [System.Xml.Serialization.XmlInclude(typeof(WhenAlgo))]
        [System.Xml.Serialization.XmlInclude(typeof(Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseDiscount))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponsGroup))]
        [System.Xml.Serialization.XmlInclude(typeof(CouponData))]
        [System.Xml.Serialization.XmlInclude(typeof(BasePPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(Subscription))]
        [System.Xml.Serialization.XmlInclude(typeof(Collection))]
        [System.Xml.Serialization.XmlInclude(typeof(BundleCodeContainer))]
        public virtual Collection[] GetCollectionsData(string sWSUserName, string sWSPassword, string[] oCollCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseCollection t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionsData(oCollCodes, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PPVModule))]
        public virtual PPVModule ValidatePPVModuleForMediaFile(string sWSUserName, string sWSPassword, Int32 mediaFileID, long ppvModuleCode)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.ValidatePPVModuleForMediaFile(nGroupID, mediaFileID, ppvModuleCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual Subscription[] GetSubscriptionsByProductCodes(string sWSUserName, string sWSPassword, string[] productCodes)
        {            
                        
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetSubscriptionsByProductCodes(nGroupID, productCodes);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual PPVModule[] GetPPVModulesByProductCodes(string sWSUserName, string sWSPassword, string[] productCodes)
        {
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModulesByProductCodes(nGroupID, productCodes);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual PPVModuleResponse GetPPVModulesData(string sWSUserName, string sWSPassword, string[] sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            PPVModuleResponse response = new PPVModuleResponse();
            BasePPVModule t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                try
                {
                    return Core.Pricing.Module.GetPPVModulesData(nGroupID, sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception)
                {
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }                
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        // [WebMethod]
        //public virtual Status InsertPriceCode(string sWSUserName, string sWSPassword, string code, Price price)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return t.InsertPriceCode(nGroupID, code, price);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //        {
        //            HttpContext.Current.Response.StatusCode = 404;
        //        }
        //        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
        //    }
        //}

        //[WebMethod]
        //public virtual Status InsertDiscountCode(string sWSUserName, string sWSPassword, DiscountModule discount)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return t.InsertDiscountCode(nGroupID, discount);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //        {
        //            HttpContext.Current.Response.StatusCode = 404;
        //        }
        //        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
        //    }
        //}

        //[WebMethod]
        //public virtual Status InsertCouponGroup(string sWSUserName, string sWSPassword, CouponsGroup coupon)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return t.InsertCouponGroup(nGroupID, coupon);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //        {
        //            HttpContext.Current.Response.StatusCode = 404;
        //        }
        //        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
        //    }
        //}

        //[WebMethod]
        //public virtual Status InsertUsageModule(string sWSUserName, string sWSPassword, UsageModule usageModule)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return t.InsertUsageModule(nGroupID, usageModule);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //        {
        //            HttpContext.Current.Response.StatusCode = 404;
        //        }
        //        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
        //    }
        //}

        //[WebMethod]
        //public virtual Status InsertPreviewModule(string sWSUserName, string sWSPassword, PreviewModule previewModule)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return t.InsertPreviewModule(nGroupID, previewModule);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //        {
        //            HttpContext.Current.Response.StatusCode = 404;
        //        }
        //        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
        //    }
        //}

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse InsertPPV(int nGroupID, ApiObjects.IngestPPV ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse(); ;

            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.InsertPPV(nGroupID, ppv);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }
        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse UpdatePPV(int nGroupID, ApiObjects.IngestPPV ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse(); ;

            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.UpdatePPV(nGroupID, ppv);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }
        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse DeletePPV(int nGroupID, string ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.DeletePPV(nGroupID, ppv);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse InsertMPP(int nGroupID, ApiObjects.IngestMultiPricePlan multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.InsertMPP(nGroupID, multiPricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse UpdateMPP(int nGroupID, ApiObjects.IngestMultiPricePlan multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.UpdateMPP(nGroupID, multiPricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse DeleteMPP(int nGroupID, string multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.DeleteMPP(nGroupID, multiPricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse InsertPricePlan(int nGroupID, ApiObjects.IngestPricePlan pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.InsertPricePlan(nGroupID, pricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse UpdatePricePlan(int nGroupID, ApiObjects.IngestPricePlan pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();


            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.UpdatePricePlan(nGroupID, pricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse DeletePricePlan(int nGroupID, string pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            if (nGroupID != 0)
            {
                response = Core.Pricing.Module.DeletePricePlan(nGroupID, pricePlan);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.BusinessModuleResponse test(string sWSUserName, string sWSPassword, string name)
        {

            ApiObjects.IngestMultiPricePlan mpp = new ApiObjects.IngestMultiPricePlan();
            


            //ApiObjects.IngestPPV ppv = new ApiObjects.IngestPPV();

            //ppv.Code = "test liat 123";
            //ppv.CouponGroup = "50% Coupon";
            //ppv.Discount = "100%";
            //ppv.FileTypes = new List<string>();
            //ppv.FileTypes.Add("iPhone Main");
            //ppv.FileTypes.Add("web hd");
            //ppv.FileTypes.Add("");
            //ppv.PriceCode = "10 USD";
            //ppv.UsageModule = "4.99 7-Days 48-Hours";

            //ApiObjects.BusinessModuleResponse response = InsertPPV(215, ppv);

            mpp.Code = "MPP_3412307456_7";
            mpp.Action = ApiObjects.eIngestAction.Insert;
            mpp.StartDate = DateTime.UtcNow;
            mpp.EndDate = DateTime.UtcNow.AddDays(24);

            mpp.Channels = new List<string>();
            mpp.Channels.Add("Shai_Channel_Regression");
            //mpp.Channels.Add("Shai_Channel_Regression  gdkb");

            mpp.PricePlansCodes = new List<string>();
            mpp.PricePlansCodes.Add("Price Plan for Ingest Sharon");

            mpp.FileTypes = new List<string>();            
            mpp.FileTypes.Add("shdhsdfhsdfhdfs");
            mpp.FileTypes.Add("");

            ApiObjects.KeyValuePair kv = new ApiObjects.KeyValuePair();
            mpp.Titles = new List<ApiObjects.KeyValuePair>();
            kv.key = "eng";
            kv.value = "Ingest MPP title";
            mpp.Titles.Add(kv);

            mpp.Descriptions = new List<ApiObjects.KeyValuePair>();
            kv = new ApiObjects.KeyValuePair();
            kv.key = "eng";
            kv.value = "Ingest MPP description";
            mpp.Descriptions.Add(kv);

            mpp.InternalDiscount = "100% discount";
            ApiObjects.BusinessModuleResponse response = InsertMPP(203, mpp);
            //nGroupID, discount.m_sCode, discount.m_oPrise.m_dPrice, discount.m_oPrise.m_oCurrency.m_nCurrencyID ,discount.m_dPercent, (int)discount.m_eTheRelationType, discount.m_dStartDate, discount.m_dEndDate, 
            //    (int)discount.m_oWhenAlgo.m_eAlgoType, discount.m_oWhenAlgo.m_nNTimes

            //ApiObjects.IngestPricePlan pp = new ApiObjects.IngestPricePlan();
            //pp.Code = "pricPlan2_16032016";
            //pp.FullLifeCycle = "3 Weeks";
            //pp.ViewLifeCycle = "2 Hours";
            //pp.IsRenewable = true;
            //pp.MaxViews = 5;
            //pp.PriceCode = "priceCodeTest7";
            //pp.RecurringPeriods = 6;
            //pp.IsActive = true;
            //ApiObjects.BusinessModuleResponse response = InsertPricePlan(215, pp);


            // BusinessModuleResponse response = DeletePricePlan(215, "pricPlan2");
            //BusinessModuleResponse response = UpdatePricePlan(215, pp);

            //DeleteMPP(215, "test07.03.2016_5");

            // ApiObjects.IngestMultiPricePlan mpp = new ApiObjects.IngestMultiPricePlan();
            // mpp.Code = "test07.03.2016_5";
            // mpp.Action = ApiObjects.eIngestAction.Insert;
            // mpp.StartDate = DateTime.UtcNow;
            // mpp.EndDate = DateTime.UtcNow.AddDays(24);

            // mpp.Channels = new List<string>();
            // mpp.Channels.Add("KSQL Sunny3");
            //// mpp.Channels.Add("AutoChannelByTagDirectorMaxim");

            // mpp.PricePlansCodes = new List<string>();
            // mpp.PricePlansCodes.Add("PP1 - 5 min not renewable");
            // mpp.PricePlansCodes.Add("PP2");

            // mpp.FileTypes = new List<string>();
            // mpp.FileTypes.Add("iPhone Main");
            // mpp.FileTypes.Add("web hd");
            // mpp.FileTypes.Add("");

            // ApiObjects.KeyValuePair kv = new ApiObjects.KeyValuePair();
            // mpp.Titles = new List<ApiObjects.KeyValuePair>();
            // kv.key = "eng";
            // kv.value = "liat_t_1";
            // mpp.Titles.Add(kv);

            // mpp.Descriptions = new List<ApiObjects.KeyValuePair>();
            // kv = new ApiObjects.KeyValuePair();
            // kv.key = "eng";
            // kv.value = "liat_d_1";
            // mpp.Descriptions.Add(kv);

            // mpp.InternalDiscount = "100%";
            // ApiObjects.BusinessModuleResponse response = InsertMPP(215, mpp);

            //mpp.InternalDiscount = "100%";
            //mpp.PreviewModule = "t1";
            //mpp.IsActive = true;
            //mpp.IsRenewable = false;
            //mpp.OrderNum = 0;
            //mpp.NumOfRecPeriods = 5;
            //mpp.GracePeriodMinutes = 6;


            //MultiPricePlanResponse respo = UpdateMPP(215, mpp);
            //return respo.Status;
            //return response;

            return new ApiObjects.BusinessModuleResponse();
        }

        [WebMethod]
        public virtual PPVModuleDataResponse GetPPVModuleResponse(string sWSUserName, string sWSPassword, string sPPVCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Pricing.Module.GetPPVModuleResponse(nGroupID, sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        }
    }
