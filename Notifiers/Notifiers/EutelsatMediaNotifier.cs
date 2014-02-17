using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (IsNotifyProduct(sMediaID))
            {
                //tikle_ws.Service t = new Notifiers.tikle_ws.Service();
                //string sTikleWSURL = Utils.GetWSURL("tikle_ws");
                //t.Url = sTikleWSURL;
                //tikle_ws.Response resp = t.NotifyProduct(sMediaID, m_nGroupID);
                //Logger.Logger.Log("Notify", sMediaID + " : " + resp.ResultDetail, "media_notifier");

                EutelsatTransactionResponse resp = MakePackageNotification(sMediaID);

                Logger.Logger.Log("Notify", sMediaID + " : " + resp.Message, "package_notifier");
            }
            else
            {
                Logger.Logger.Log("Notify", sMediaID + " : " + "No need to notify - media is off or expired", "media_notifier");
            }
        }

        private EutelsatTransactionResponse MakePackageNotification(string sMediaID)
        {
            EutelsatTransactionResponse res = new EutelsatTransactionResponse();
            res.Success = false;

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

                //List<string> lOperatorCoGuids = DAL.TvmDAL.GetSubscriptionOperatorCoGuids(m_nGroupID, sMediaID);
                //pack.IPNO_IDs = lOperatorCoGuids.Select(id => new ObjIPNO_ID() { IPNO_ID = id }).ToList();

                EutelsatPackageNotification packNotification = new EutelsatPackageNotification() { Product = pack };

                //string jsonTransactionContent = trans.Serialize();
                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(packNotification);

                //string requestURL = MakeTransNotificationURL(sWSURL, sHouseholdUID, dPrice, sCurrency, nExternalAssetID, sPpvModuleCode, sCouponCode, nRoviID, nTransactionID, nDeviceBrandID);
                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

                if (isGoodUri)
                {
                    res = Utils.MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent) as EutelsatTransactionResponse;
                    //object 3ssRes = Utils.MakeJsonRequest(checkTvodUrl, 
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
            int nMediaID;

            if (int.TryParse(sMediaID, out nMediaID))
            {
                DataSetSelectQuery selectQuery = new DataSetSelectQuery();
                selectQuery += " select ID from media with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                selectQuery += " and (end_date > getdate() or end_date IS NULL)";
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = true;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            return retVal;
        }
    }
}
