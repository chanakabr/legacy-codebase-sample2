using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;

namespace TVPPro.SiteManager.Helper
{
    public class WatchLaterHelper
    {
        public static object[] GetWatchLaterList()
        {
            object[] responseObj = null;

            var requestObject = new
            {
                initObj = TVPApiHelper.GetInitObj(),
                itemObjects = new List<object>(),
                itemType = "Media",
                listType = "Watch"
            };

            string response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetItemFromList, JsonConvert.SerializeObject(requestObject));

            TVPApiHelper.CastResponse<object[]>(response, out responseObj);

            return responseObj;
        }

        public static bool RemoveItemFromWatchList(int mediaId, int order = 0)
        {
           return invoke<bool>(TVPApiHelper.TVPAPI_METHODS.RemoveItemFromList, mediaId, order);
        }

        public static bool AddItemToList(int mediaId, int order = 0)
        {
            return invoke<bool>(TVPApiHelper.TVPAPI_METHODS.AddItemToList, mediaId, order);
        }

        public static bool IsItemExistsInList(int mediaId, int order = 0)
        {
            bool isExist = false;
            object[] responseArr = invoke<object[]>(TVPApiHelper.TVPAPI_METHODS.IsItemExistsInList,mediaId,order);

            if (responseArr != null && responseArr.Length > 0)
            {
                Dictionary<string, object> responseDic = (Dictionary<string, object>)responseArr[0];
                if (responseDic != null && responseDic.ContainsKey("value"))
                    bool.TryParse(responseDic["value"].ToString(), out isExist);
            }

            return isExist;
        }
        private static T invoke<T>(TVPApiHelper.TVPAPI_METHODS method, int mediaId, int order = 0)
        {
            T result = default(T);

            var requestObject = new
         {
             initObj = TVPApiHelper.GetInitObj(),
             itemObjects = new List<object>() { new { item = mediaId, orderNum = order } },
             itemType = "Media",
             listType = "Watch"
         };
            string response = TVPApiHelper.MakeRequest(method, JsonConvert.SerializeObject(requestObject));
            TVPApiHelper.CastResponse<T>(response, out result);
            return result;
        }

    }
}
