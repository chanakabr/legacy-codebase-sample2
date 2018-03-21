using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Notifiers
{
    public class EutelsatSubscriptionNotifier : BaseSubscriptionNotifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public EutelsatSubscriptionNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSubscriptionID)
        {
            string errorMessage = "";
            int create0update1assign2 = 1;  // default action is update 
            NotifyChange(sSubscriptionID, ref errorMessage, create0update1assign2);
        }

        override public void NotifyChange(string sSubscriptionID, int create0update1assign2)
        {
            string errorMessage = "";
            NotifyChange(sSubscriptionID, ref errorMessage, create0update1assign2);
        }

        override public void NotifyChange(string sSubscriptionID, ref string errorMessage, int create0update1assign2)
        {
            ProductNotificationResponse resp = MakeProductNotification(sSubscriptionID, create0update1assign2);

            errorMessage = "";

            if (!resp.success)
            {
                string[] errors = resp.errors.Select(e => "type: " + e.error_type + "; error: " + e.error_message).ToArray();
                errorMessage = string.Join("\n", errors);
            }

            log.Debug("Notify - sSubscriptionID: " + sSubscriptionID + " : " + (resp.success ? "notification success" : errorMessage));
        }


        protected ProductNotificationResponse MakeProductNotification(string sSubscriptionID, int create0update1assign2)
        {

            ProductNotificationResponse res = new ProductNotificationResponse();
            res.success = false;

            string sWSURL = ApplicationConfiguration.EutelsatSettings.Eutelsat_ProductBase.Value; //+(update ? "/update" : "/create");

            switch (create0update1assign2)
            {
                case 0:
                    sWSURL += "/create";
                    break;
                case 2:
                    sWSURL += "/assign";
                    break;
                case 1:
                default:
                    sWSURL += "/update";
                    break;
            }

            string sWSUsername = ApplicationConfiguration.EutelsatSettings.Eutelsat_3SS_WS_Username.Value;
            string sWSPassword = ApplicationConfiguration.EutelsatSettings.Eutelsat_3SS_WS_Password.Value;

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
                InternalProductID = int.Parse(dbProd["InternalProductID"]),
                ExternalProductID = string.IsNullOrEmpty(dbProd["ExternalProductID"]) ? string.Empty : dbProd["ExternalProductID"],
                Title = dbProd["Title"],
                Description = dbProd["Description"],
                Status = dbProd["Status"],
                PriceB2C = price,
                StartDate = startDate, //DateTime.Parse(dbProd["StartDate"]),
                EndDate = endDate    //DateTime.Parse(dbProd["StartDate"]),
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
                string sRes = Utils.MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent);
                res = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(ProductNotificationResponse)) as ProductNotificationResponse;
                //res = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(NotificationResponse)) as NotificationResponse;    

                //res = Utils.MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent) as EutelsatProductNotificationResponse;
            }

            return res;
        }

    }
}
