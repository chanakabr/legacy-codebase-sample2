using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using System.Web.Script.Serialization;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.Helper
{
    public class ORCAHelper
    {
        private static string TVPAPI_BASE_URL = System.Configuration.ConfigurationManager.AppSettings["BASE_TVPAPI_URL"];
        private static JavaScriptSerializer json = new JavaScriptSerializer();
        private static int ORCA_REQUEST_TIMEOUT = int.Parse(System.Configuration.ConfigurationManager.AppSettings["OrcaCallsTimeOut"]);

        public enum ORCA_CALLS
        {
            HomeVODPromotions,
            HomeVODPersonal,
            HomeLivePromotions,
            HomeLivePersonal,
            CatalogVODPromotions,
            CategoryCatalogVODPromotions,
            MovieRelated,
            SeriesRelated,
            EpisodeRelated,
            PersonalRecommendationsVOD,
            PersonalRecommendationsLive,
            EndOfMovie,
            EndOfEpisode,
            VODAction,
            VODDrama,
            VODComedy,
            VODThriller,
            VODDoco,
            VODKids
        }
        public static ORCAGalleryResponse getVodORCAResponseByCategory(ORCA_CALLS ORCACallType, string categoryId)
        {
            ORCAGalleryResponse result = null;
            var requestData = getInitObject(ORCACallType.ToString(), DeviceDNAHelper.GetDeviceDNA(), "", categoryId);
            string response = null;

            try
            {
                response = WebRequestHelper.SendRequest<string>(string.Format("{0}GetRecommendationsByGallery", TVPAPI_BASE_URL), json.Serialize(requestData), ORCA_REQUEST_TIMEOUT);
                result = castResponse(response);
            }
            catch { }

            return result;
        }
        public static ORCAGalleryResponse getORCAResponse(ORCA_CALLS ORCACallType)
        {
            ORCAGalleryResponse result = null;
            var requestData = getInitObject(ORCACallType.ToString(), DeviceDNAHelper.GetDeviceDNA());
            string response = null;

            try
            {
                response = WebRequestHelper.SendRequest<string>(string.Format("{0}GetRecommendationsByGallery", TVPAPI_BASE_URL), json.Serialize(requestData), ORCA_REQUEST_TIMEOUT);
                result = castResponse(response);
            }
            catch { }

            return result;
        }

        public static ORCAGalleryResponse getORCAResponse(ORCA_CALLS ORCACallType, string mediaId)
        {
            ORCAGalleryResponse result = null;
            var requestData = getInitObject(ORCACallType.ToString(), DeviceDNAHelper.GetDeviceDNA(), mediaId);
            string response = null;

            try
            {
                response = WebRequestHelper.SendRequest<string>(string.Format("{0}GetRecommendationsByGallery", TVPAPI_BASE_URL), json.Serialize(requestData), ORCA_REQUEST_TIMEOUT);
                result = castResponse(response);
            }
            catch { }

            return result;
        }

        private static ORCAGalleryResponse castResponse(string response)
        {
            ORCAGalleryResponse result = null;
            try
            {
                if (!string.IsNullOrEmpty(response))
                {
                    object respoinseObj = json.DeserializeObject(response);
                    Dictionary<string, object> galleryDataDic = (respoinseObj != null) ? respoinseObj as Dictionary<string, object> : null;
                    if (galleryDataDic != null && !galleryDataDic.ContainsKey("Error"))
                    {
                        result = new ORCAGalleryResponse();
                        result.ContentType = int.Parse(galleryDataDic["ContentType"].ToString());
                        result.Content = galleryDataDic["Content"] as object[];
                    }
                }
            }
            catch { }

            return result;
        }

        private static Object getInitObject(string galleryType, string deviceDNA, string mediaId = "0", string categoryId = "")
        {
            return new
            {
                initObj = new
                {
                    Locale = new
                    {
                        LocaleLanguage = "",
                        LocaleCountry = "",
                        LocaleDevice = "",
                        LocaleUserState = "Unknown"
                    },
                    Platform = "iPad",
                    SiteGuid = UsersService.Instance.GetUserID(),
                    DomainID = UsersService.Instance.GetDomainID(),
                    UDID = deviceDNA,
                    ApiUser = "tvpapi_153",
                    ApiPass = "11111"
                },
                mediaID = mediaId,
                picSize = "",
                parentalLevel = 0,
                galleryType = galleryType,
                coGuid = categoryId
            };
        }
    }
}
