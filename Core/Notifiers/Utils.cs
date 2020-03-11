using System;

namespace Notifiers
{
    public class Utils
    {
        public static void GetBaseUsersNotifierImpl(ref Notifiers.BaseUsersNotifier t, Int32 nGroupID)
        {
            GetBaseUsersNotifierImpl(ref t, nGroupID, "");
        }

        public static void GetBaseUsersNotifierImpl(ref Notifiers.BaseUsersNotifier t, Int32 nGroupID , string sConn)
        {
            int moduleID = 1;

            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);

            switch (nImplID)
            {
                case 1:
                    break;

                case 2:
                    t = new Notifiers.EutelsatUsersNotifier(nGroupID);
                    break;

                default:
                    break;
            }
        }

        public static void GetBaseSubscriptionsNotifierImpl(ref Notifiers.BaseSubscriptionNotifier t, Int32 nGroupID)
        {
            GetBaseSubscriptionsNotifierImpl(ref t, nGroupID, "");
        }

        public static void GetBaseSubscriptionsNotifierImpl(ref Notifiers.BaseSubscriptionNotifier t, Int32 nGroupID, string sConn)
        {
            int moduleID = 2;
            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);
                
            switch (nImplID)
	        {	                  
                case 1:                   
                    break;

                case 2:
                    t = new Notifiers.EutelsatSubscriptionNotifier(nGroupID);
                    break;

                default:
                    break;
	        }               
        }

        public static void GetBaseMediaNotifierImpl(ref Notifiers.BaseMediaNotifier t, Int32 nGroupID)
        {
            GetBaseMediaNotifierImpl(ref t, nGroupID, "");
        }

        public static void GetBaseMediaNotifierImpl(ref Notifiers.BaseMediaNotifier t, Int32 nGroupID, string sConn)
        {
            int moduleID = 3;
            int nImplID = DAL.TvmDAL.GetSubscriptionsNotifierImpl(nGroupID, moduleID);

            switch (nImplID)
	        {                  
                case 1:                    
                    break;   

                case 2:
                    t = new Notifiers.EutelsatMediaNotifier(nGroupID);
                    break;
                default:
                    break;   
	        }
        }

        public static string MakeJsonRequest(Uri requestUri, string wsUsername, string wsPassword, string jsonContent = "")
        {
            string sRes = string.Empty;

            throw new NotImplementedException();

            try
            {
                //sRes = TVinciShared.WS_Utils.SendXMLHttpReq(requestUri.OriginalString, jsonContent, "", "application/json", "UserName", wsUsername, "Password", wsPassword);
                
                //object objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(EutelsatProductNotificationResponse));
                //return objResponse;
            }
            catch
            {
            }

            return sRes;
        }
    }
}
