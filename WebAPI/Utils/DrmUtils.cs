using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Utils
{
    public static class DrmUtils
    {
        private const string BASE_UDRM_URL_TCM_KEY = "UDRM_URL";
        private const string FAIRPLAY_CERTIFICATE_TCM_KEY = "FAIRPLAY_CERTIFICATE";
        private const string UDRM_CENC_LICENSED_URL_FORMAT = "{0}/cenc/{1}/license?custom_data={2}&signature={3}";
        private const string UDRM_LICENSED_URL_FORMAT = "{0}/{1}/license?custom_data={2}&signature={3}";
        private const string PLAYREADY = "playready";
        private const string WIDEVINE = "widevine";
        private const string FAIRPLAY = "fps";

        public static string BuildCencCustomDataString(string fileExternalId)
        {
            string response = null;

            KS ks = KS.GetFromRequest();
            Group group = GroupsManager.GetGroup(ks.GroupId);

            CencCustomData customData = new CencCustomData()
            {
                AccountId = group.MediaPrepAccountId,
                CaSystem = "OTT",
                Files = string.Empty,
                UserToken = ks.ToString(),
                ContentId = fileExternalId.ToString(),
                AdditionalCasSystem = ks.GroupId,
                UDID = KSUtils.ExtractKSPayload().UDID
            };

            response = JsonConvert.SerializeObject(customData);

            return response;
        }

        public static string BuildCencSignatureString(string customDataString)
        {
            string response = null;

            KS ks = KS.GetFromRequest();
            Group group = GroupsManager.GetGroup(ks.GroupId);

            response = string.Concat(group.AccountPrivateKey, customDataString);

            return Convert.ToBase64String(EncryptionUtils.HashSHA1(Encoding.ASCII.GetBytes(response)));
        }

        internal static string BuildUDrmUrl(KalturaDrmSchemeName schemeName, string customDataString, string signature)
        {
            string response = null;
            string baseUdrmUrl = TCMClient.Settings.Instance.GetValue<string>(BASE_UDRM_URL_TCM_KEY);
            customDataString = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.ASCII.GetBytes(customDataString)));
            signature = HttpUtility.UrlEncode(signature);
            switch (schemeName)
            {
                case KalturaDrmSchemeName.PLAYREADY_CENC:
                    response = string.Format(UDRM_CENC_LICENSED_URL_FORMAT, baseUdrmUrl, PLAYREADY, customDataString, signature);
                    break;
                case KalturaDrmSchemeName.WIDEVINE_CENC:
                    response = string.Format(UDRM_CENC_LICENSED_URL_FORMAT, baseUdrmUrl, WIDEVINE, customDataString, signature);
                    break;
                case KalturaDrmSchemeName.FAIRPLAY:
                    response = string.Format(UDRM_LICENSED_URL_FORMAT, baseUdrmUrl, FAIRPLAY, customDataString, signature);
                    break;
                case KalturaDrmSchemeName.WIDEVINE:
                    response = string.Format(UDRM_LICENSED_URL_FORMAT, baseUdrmUrl, WIDEVINE, customDataString, signature);
                    break;
                default:
                    break;
            }

            return response;
        }

        internal static List<KalturaDrmSchemeName> GetDrmSchemeName(string fileFormat)
        {
            List<KalturaDrmSchemeName> response = new List<KalturaDrmSchemeName>();

            switch (fileFormat)
            {
                case "applehttp":
                    response.Add(KalturaDrmSchemeName.FAIRPLAY);
                    break;
                case "mpegdash":
                    {
                        response.Add(KalturaDrmSchemeName.PLAYREADY_CENC);
                        response.Add(KalturaDrmSchemeName.WIDEVINE_CENC);
                    }
                    break;
                case "url":
                    response.Add(KalturaDrmSchemeName.WIDEVINE);
                    break;
                case "smothstreaming":
                    {
                        response.Add(KalturaDrmSchemeName.PLAYREADY_CENC);
                        response.Add(KalturaDrmSchemeName.PLAYREADY);
                    }
                    break;
            }

            return response;
        }

        internal static KalturaDrmPlaybackPluginData GetDrmPlaybackPluginData(KalturaDrmSchemeName scheme, KalturaPlaybackSource source)
        {
            KalturaDrmPlaybackPluginData drmData;
            switch (scheme)
            {
                case KalturaDrmSchemeName.FAIRPLAY:
                    {
                        drmData = new KalturaFairPlayPlaybackPluginData();
                        ((KalturaFairPlayPlaybackPluginData)drmData).Certificate = TCMClient.Settings.Instance.GetValue<string>(FAIRPLAY_CERTIFICATE_TCM_KEY);
                    }
                    break;
                case KalturaDrmSchemeName.PLAYREADY_CENC:
                case KalturaDrmSchemeName.WIDEVINE_CENC:
                case KalturaDrmSchemeName.WIDEVINE:
                default:
                    drmData = new KalturaDrmPlaybackPluginData();
                    break;
            }
            var customDataString = DrmUtils.BuildCencCustomDataString(source.ExternalId);
            var signature = DrmUtils.BuildCencSignatureString(customDataString);
            drmData.Scheme = scheme;
            drmData.LicenseURL = DrmUtils.BuildUDrmUrl(scheme, customDataString, signature);
            return drmData;
        }
    }
}