using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Notifiers
{
    public class Utils
    {
        static public void GetBaseUsersNotifierImpl(ref Notifiers.BaseUsersNotifier t, Int32 nGroupID)
        {
            GetBaseUsersNotifierImpl(ref t, nGroupID, "");
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public void GetBaseUsersNotifierImpl(ref Notifiers.BaseUsersNotifier t, Int32 nGroupID , string sConn)
        {
            int moduleID = 1;
            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);
            //int nImplID = DAL.TvmDAL.GetUsersNotifierImpl(nGroupID, moduleID);

            switch (nImplID)
            {
                case 1:
                    t = new Notifiers.TikleUsersNotifier(nGroupID);
                    break;

                case 2:
                    t = new Notifiers.EutelsatUsersNotifier(nGroupID);
                    break;

                default:
                    break;
            }
        }

        static public void GetBaseSubscriptionsNotifierImpl(ref Notifiers.BaseSubscriptionNotifier t, Int32 nGroupID)
        {
            GetBaseSubscriptionsNotifierImpl(ref t, nGroupID, "");
        }

        static public void GetBaseSubscriptionsNotifierImpl(ref Notifiers.BaseSubscriptionNotifier t, Int32 nGroupID, string sConn)
        {
            int moduleID = 2;
            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);
                
            switch (nImplID)
	        {	                  
                case 1:
                    t = new Notifiers.TikleSubscriptionNotifier(nGroupID);
                    break;

                case 2:
                    t = new Notifiers.EutelsatSubscriptionNotifier(nGroupID);
                    break;

                default:
                    break;
	        }               
        }

        static public void GetBaseMediaNotifierImpl(ref Notifiers.BaseMediaNotifier t, Int32 nGroupID)
        {
            GetBaseMediaNotifierImpl(ref t, nGroupID, "");
        }

        static public void GetBaseMediaNotifierImpl(ref Notifiers.BaseMediaNotifier t, Int32 nGroupID, string sConn)
        {
            int moduleID = 3;
            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);

            switch (nImplID)
	        {                  
                case 1:
                    t = new Notifiers.TikleMediaNotifier(nGroupID);
                    break;   

                case 2:
                    t = new Notifiers.EutelsatMediaNotifier(nGroupID);
                    break;
                default:
                    break;   
	        }
        }

        public static string GetValueFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static object MakeJsonRequest(Uri requestUri, string wsUsername, string wsPassword, string jsonContent = "")
        {
            try
            {
                string sRes = TVinciShared.WS_Utils.SendXMLHttpReq(requestUri.OriginalString, jsonContent, "", "application/json", "UserName", wsUsername, "Password", wsPassword);
                object objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(EutelsatTransactionResponse));

                return objResponse;
            }
            catch
            {
            }

            return null;
        }
    }
}
