using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using ODBCWrapper;

namespace Notifiers
{
    public class EutelsatMediaNotifier : BaseMediaNotifier
    {
        public EutelsatMediaNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        override public void NotifyChange(string sMediaID)
        {
            string errorMessage = "";
            NotifyChange(sMediaID, ref errorMessage);
        }

        override public void NotifyChange(string sMediaID, ref string errorMessage)
        {
            if (IsNotifyProduct(sMediaID))
            {
                PackageNotificationResponse resp = MakePackageNotification(sMediaID);

                errorMessage = "";

                if (resp != null && !resp.success && !string.IsNullOrEmpty(resp.message))
                {
                    errorMessage = resp.message;
                    //string[] errors = resp.errors.Select(e => "type: " + e.error_type + "; error: " + e.error_message).ToArray();
                    //errorMessage = string.Join("\n", errors);
                }

                Logger.Logger.Log("Notify", sMediaID + " : " + (resp.success ? "notification success" : errorMessage), "package_notifier");
            }
            else
            {
                Logger.Logger.Log("Notify", sMediaID + " : " + "No need to notify - media is off or expired", "media_notifier");
            }
        }

        private PackageNotificationResponse MakePackageNotification(string sMediaID)
        {
            PackageNotificationResponse res = new PackageNotificationResponse();
            res.success = false;

            try
            {
                string sWSURL = Utils.GetWSURL("Eutelsat_ProductBase") + "/assign";
                string sWSUsername = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Username");
                string sWSPassword = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Password");

                if (string.IsNullOrEmpty(sMediaID) || string.IsNullOrEmpty(sWSURL))
                {
                    return res;
                }


                Dictionary<string, string> dbPackage = DAL.TvmDAL.GetVirtualPackageInfo(m_nGroupID, sMediaID);

                if (dbPackage == null || dbPackage.Keys.Count == 0)
                {
                    return res;
                }

                if ((string.IsNullOrEmpty(dbPackage["InternalProductID"])) ||
                   (string.IsNullOrEmpty(dbPackage["ExternalProductID"])) ||
                   (string.IsNullOrEmpty(dbPackage["OperatorID"])))
                {
                    return res;
                }

                double price = 0.0;
                DateTime startDate = new DateTime(2000, 1, 1);
                DateTime endDate = new DateTime(2099, 1, 1);

                double.TryParse(dbPackage["Price"], out price);
                DateTime.TryParse(dbPackage["StartDate"], out startDate);
                DateTime.TryParse(dbPackage["EndDate"], out endDate);

                EutelsatPackage pack = new EutelsatPackage()
                {
                    InternalProductID = int.Parse(dbPackage["InternalProductID"]),
                    ExternalProductID = dbPackage["ExternalProductID"],
                    IPNO_ID = dbPackage["OperatorID"],
                    Title = dbPackage["Title"],
                    Price = price,
                    Description = dbPackage["Description"],
                    LogoUrl = string.IsNullOrEmpty(dbPackage["ImageUrl"]) ? string.Empty : dbPackage["ImageUrl"],
                    StartDate = startDate,
                    EndDate = endDate
                };


                EutelsatPackageNotification packNotification = new EutelsatPackageNotification() { Product = pack };

                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(packNotification);

                //string requestURL = MakeTransNotificationURL(sWSURL, sHouseholdUID, dPrice, sCurrency, nExternalAssetID, sPpvModuleCode, sCouponCode, nRoviID, nTransactionID, nDeviceBrandID);

                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

                if (isGoodUri)
                {
                    string sRes = Utils.MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent);
                    res = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(PackageNotificationResponse)) as PackageNotificationResponse;    
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Notify", sMediaID + " : " + ex.Message, "media_notifier");
            }


            return res;
        }

        private bool IsNotifyProduct(string sMediaID)
        {
            bool retVal = false;

            int nMediaID = 0;

            if (int.TryParse(sMediaID, out nMediaID))
            {
                long virtualMediaID = DAL.TvmDAL.GetPackageMediaID(m_nGroupID, sMediaID);

                if (virtualMediaID > 0)
                {
                    retVal = true;
                }
            }

            return retVal;
        }
    }
}
