using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public class EutelsatSubscriptionNotifier: BaseSubscriptionNotifier
    {
        public EutelsatSubscriptionNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSubscriptionID)
        {
            string errorMessage = "";
            NotifyChange(sSubscriptionID, ref errorMessage, true);
        }

        override public void NotifyChange(string sSubscriptionID, bool update)
        {
            string errorMessage = "";
            NotifyChange(sSubscriptionID, ref errorMessage, update);
        }

        override public void NotifyChange(string sSubscriptionID, ref string errorMessage, bool update)
        {
            //WS_3SS.Service t = new Notifiers.tikle_ws.Service();
            //string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            //t.Url = sTikleWSURL;
            //tikle_ws.Response resp = t.NotifySubscription(sSubscriptionID, m_nGroupID);
            
            //Logger.Logger.Log("Notify", sSubscriptionID + " : "  + resp.ResultDetail, "subscriptions_notifier");

            EutelsatProductNotificationResponse resp = MakeProductNotification(sSubscriptionID, update);

            errorMessage = "";
            
            if (!resp.success)
            {
                string[] errors = resp.errors.Select(e => "type: " + e.error_type + "; error: " + e.error_message).ToArray();
                errorMessage = string.Join("\n",  errors);
            }

            Logger.Logger.Log("Notify", sSubscriptionID + " : " + (resp.success ? "notification success" : errorMessage), "subscriptions_notifier");
            
        }


        protected EutelsatProductNotificationResponse MakeProductNotification(string sSubscriptionID, bool update)
        {

            EutelsatProductNotificationResponse res = new EutelsatProductNotificationResponse();
            res.success = false;

            string sWSURL       = Utils.GetWSURL("Eutelsat_ProductBase") + (update ? "/update" : "/create");
            string sWSUsername  = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Username");
            string sWSPassword  = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Password");

            if (string.IsNullOrEmpty(sSubscriptionID) || string.IsNullOrEmpty(sWSURL))
            {
                return res;
            }
            
            Dictionary<string, string> dbProd = DAL.TvmDAL.GetSubscriptionInfo(m_nGroupID, sSubscriptionID);

            if (dbProd == null || dbProd.Keys.Count == 0)
            {
                return res;
            }

            double price = 0.0;
            DateTime startDate = new DateTime(2000, 1, 1);
            DateTime endDate = new DateTime(2099, 1, 1);

            double.TryParse(dbProd["PriceB2C"], out price);
            DateTime.TryParse(dbProd["StartDate"], out startDate);
            DateTime.TryParse(dbProd["EndDate"], out endDate);

            EutelsatProduct prod = new EutelsatProduct()
            {
                InternalProductID   = int.Parse(dbProd["InternalProductID"]),
                ExternalProductID   = string.IsNullOrEmpty(dbProd["ExternalProductID"]) ? string.Empty : dbProd["ExternalProductID"],
                Title               = dbProd["Title"], 
                Description         = dbProd["Description"],
                Status              = dbProd["Status"],
                PriceB2C            = price,
                StartDate           = startDate, //DateTime.Parse(dbProd["StartDate"]),
                EndDate             = endDate    //DateTime.Parse(dbProd["StartDate"]),
            };

            List<string> lOperatorCoGuids = DAL.TvmDAL.GetSubscriptionOperatorCoGuids(m_nGroupID, sSubscriptionID);
            prod.IPNO_IDs = lOperatorCoGuids.Select(id => new ObjIPNO_ID() { IPNO_ID = id }).ToList();

            List<int> lChannelIDs = DAL.TvmDAL.GetSubscriptionChannelIDs(m_nGroupID, sSubscriptionID);
            prod.PackageIDs = lChannelIDs.Select(id => new ObjPackageID() { PackageID = id }).ToList();

            EutelsatProductNotification prodNotification = new EutelsatProductNotification() { Product = prod };

            //string jsonTransactionContent = trans.Serialize();
            var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(prodNotification);

            //string requestURL = MakeTransNotificationURL(sWSURL, sHouseholdUID, dPrice, sCurrency, nExternalAssetID, sPpvModuleCode, sCouponCode, nRoviID, nTransactionID, nDeviceBrandID);
            Uri requestUri = null;
            bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

            if (isGoodUri)
            {
                res = Utils.MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent) as EutelsatProductNotificationResponse;
            }

            return res;
        }

    }
}
