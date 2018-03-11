using ApiObjects;
using ConfigurationManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Utils
{
    public static class DrmUtils
    {
        private const string BASE_UDRM_URL_TCM_KEY = "UDRM_URL";
        private const string UDRM_CENC_LICENSED_URL_FORMAT = "{0}/cenc/{1}/license?custom_data={2}&signature={3}";
        private const string UDRM_LICENSED_URL_FORMAT = "{0}/{1}/license?custom_data={2}&signature={3}";
        private const string PLAYREADY = "playready";
        private const string WIDEVINE = "widevine";
        private const string FAIRPLAY = "fps";

        public static string BuildCencCustomDataString(string fileExternalId, string caSystemUrl)
        {
            string response = null;

            KS ks = KS.GetFromRequest();
            Group group = GroupsManager.GetGroup(ks.GroupId);

            CencCustomData customData = new CencCustomData()
            {
                AccountId = group.MediaPrepAccountId,
                CaSystem = caSystemUrl,
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

        internal static string BuildUDrmUrl(Group group, KalturaDrmSchemeName schemeName, string customDataString, string signature)
        {
            string response = null;
            string baseUdrmUrl = !string.IsNullOrEmpty(group.UDrmUrl) ?
                group.UDrmUrl :
                ApplicationConfiguration.UDRMUrl.Value;

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
                case KalturaDrmSchemeName.PLAYREADY:
                    response = string.Format(UDRM_LICENSED_URL_FORMAT, baseUdrmUrl, PLAYREADY, customDataString, signature);
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

        internal static KalturaDrmPlaybackPluginData GetDrmPlaybackPluginData(Group group, KalturaDrmSchemeName scheme, KalturaPlaybackSource source, string caSystemUrl)
        {
            KalturaDrmPlaybackPluginData drmData;
            switch (scheme)
            {
                case KalturaDrmSchemeName.FAIRPLAY:
                    {
                        drmData = new KalturaFairPlayPlaybackPluginData();
                        ((KalturaFairPlayPlaybackPluginData)drmData).Certificate = group.FairplayCertificate;
                    }
                    break;
                case KalturaDrmSchemeName.PLAYREADY:
                case KalturaDrmSchemeName.PLAYREADY_CENC:
                case KalturaDrmSchemeName.WIDEVINE_CENC:
                case KalturaDrmSchemeName.WIDEVINE:
                default:
                    drmData = new KalturaDrmPlaybackPluginData();
                    break;
            }
            var customDataString = DrmUtils.BuildCencCustomDataString(source.ExternalId, caSystemUrl);
            var signature = DrmUtils.BuildCencSignatureString(customDataString);
            drmData.Scheme = scheme;
            drmData.LicenseURL = DrmUtils.BuildUDrmUrl(group, scheme, customDataString, signature);
            return drmData;
        }

        internal static void BuildSourcesDrmData(string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams, KS ks, ref KalturaPlaybackContext response)
        {
            List<int> sourceIdsToRemove = new List<int>();

            foreach (var source in response.Sources)
            {
                source.Drm = new List<KalturaDrmPlaybackPluginData>();

                if (source.DrmId == (int)DrmType.UDRM)
                {
                    KalturaDrmPlaybackPluginData drmData;
                    List<KalturaDrmSchemeName> schemes = DrmUtils.GetDrmSchemeName(source.Format);
                    if (schemes != null && schemes.Count > 0)
                    {
                        string baseUrl = WebAPI.Utils.Utils.GetCurrentBaseUrl();

                        string caSystemUrl = string.Format("{0}/api_v3/service/assetFile/action/getContext?ks={1}&contextType={2}", baseUrl, ks.ToString(),
                            assetType == KalturaAssetType.recording ? WebAPI.Models.ConditionalAccess.KalturaAssetFileContext.KalturaContextType.recording :
                            WebAPI.Models.ConditionalAccess.KalturaAssetFileContext.KalturaContextType.none);

                        Group group = GroupsManager.GetGroup(ks.GroupId);

                        foreach (var scheme in schemes)
                        {
                            drmData = DrmUtils.GetDrmPlaybackPluginData(group, scheme, source, string.Format("{0}&id={1}", caSystemUrl, source.Id));
                            source.Drm.Add(drmData);
                        }
                    }
                }
                // custom DRM adapter / profile
                else if (source.DrmId > 0)
                {
                    string code, message;
                    string customDrmDate = ClientsManager.ApiClient().GetCustomDrmAssetLicenseData(ks.GroupId, source.DrmId, ks.UserId, assetId, assetType, source.Id.Value, source.ExternalId, KSUtils.ExtractKSPayload().UDID,
                        out code, out message);

                    // no errors
                    if (string.IsNullOrEmpty(code))
                    {
                        source.Drm.Add(new KalturaCustomDrmPlaybackPluginData()
                        {
                            Data = customDrmDate,
                            LicenseURL = null,
                            Scheme = KalturaDrmSchemeName.CUSTOM_DRM
                        });
                    }
                    else
                    {
                        // add the errors to messages
                        if (response.Messages == null)
                        {
                            response.Messages = new List<KalturaAccessControlMessage>();
                        }
                        response.Messages.Add(new KalturaAccessControlMessage()
                        {
                            Code = code,
                            Message = message
                        });

                        sourceIdsToRemove.Add(source.Id.Value);
                    }
                }
            }
            // remove failed sources
            if (sourceIdsToRemove.Count > 0)
            {
                response.Sources = response.Sources.Where(s => !sourceIdsToRemove.Contains(s.Id.Value)).ToList();
            }

            // add block message if there are no sources left
            if (response.Sources.Count == 0)
            {
                if (response.Actions == null)
                {
                    response.Actions = new List<KalturaRuleAction>();
                }
                response.Actions.Add(new KalturaRuleAction()
                {
                    Type = KalturaRuleActionType.BLOCK
                });
            }
        }
    }
}