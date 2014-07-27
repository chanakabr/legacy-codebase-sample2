using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using TVPPro.SiteManager.Services;

namespace TVPPro.SiteManager.Helper
{
    public class WatchLaterHelper
    {
        private static JavaScriptSerializer json = new JavaScriptSerializer();

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

            string response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.GetItemFromList, json.Serialize(requestObject));

            TVPApiHelper.CastResponse<object[]>(response, out responseObj);

            return responseObj;
        }

        public static bool RemoveItemFromWatchList(int mediaId, int order = 0)
        {
            bool isRemoved = false;

            var requestObject = new
           {
               initObj = TVPApiHelper.GetInitObj(),
               itemObjects = new List<object>() { new { item = mediaId, orderNum = order } },
               itemType = "Media",
               listType = "Watch"
           };

            string response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.RemoveItemFromList, json.Serialize(requestObject));

            TVPApiHelper.CastResponse<bool>(response, out isRemoved);

            return isRemoved;
        }

        public static bool AddItemToList(int mediaId, int order = 0)
        {
            bool isAdded = false;

            var requestObject = new
            {
                initObj = TVPApiHelper.GetInitObj(),
                itemObjects = new List<object>() { new { item = mediaId, orderNum = order } },
                itemType = "Media",
                listType = "Watch"
            };

            string response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.AddItemToList, json.Serialize(requestObject));

            TVPApiHelper.CastResponse<bool>(response, out isAdded);

            return isAdded;
        }

        public static bool IsItemExistsInList(int mediaId, int order = 0)
        {
            bool isExist = false;

            var requestObject = new
            {
                initObj = TVPApiHelper.GetInitObj(),
                itemObjects = new List<object>() { new { item = mediaId, orderNum = order } },
                itemType = "Media",
                listType = "Watch"
            };

            string response = TVPApiHelper.MakeRequest(TVPApiHelper.TVPAPI_METHODS.IsItemExistsInList, json.Serialize(requestObject));

            object[] responseArr = null;
            TVPApiHelper.CastResponse<object[]>(response, out responseArr);

            if (responseArr != null && responseArr.Length > 0)
            {
                Dictionary<string, object> responseDic = (Dictionary<string, object>)responseArr[0];
                if (responseDic != null && responseDic.ContainsKey("value"))
                    bool.TryParse(responseDic["value"].ToString(), out isExist);
            }

            return isExist;
        }
    }
}
