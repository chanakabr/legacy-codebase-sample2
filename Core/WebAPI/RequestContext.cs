using System;
using System.Collections.Generic;
using System.Web;
using ApiObjects.User;
using ConfigurationManager;
using KalturaRequestContext;
using KLogMonitor;
using KlogMonitorHelper;
using Newtonsoft.Json.Linq;
using TVinciShared;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace WebAPI
{
    public static class RequestContext
    {       
        private const string CB_SECTION_NAME = "tokens";
        private static string accessTokenKeyFormat = ApplicationConfiguration.Current.RequestParserConfiguration.AccessTokenKeyFormat.Value;

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);

        public static void SetContext(IDictionary<string, object> requestParams, string service, string action, bool globalScope = true)
        {
            SetRequestFormatContext();
            SetKsContext(requestParams, globalScope);
            SetImpersonateUserContext(requestParams, globalScope);
            SetLanguageContext(requestParams, globalScope);
            SetCurrencyContext(requestParams, globalScope);
            SetRequestTypeContext(action);
            SetResponseProfile(requestParams, globalScope);
            SetUserIpContext();
            SetRequestVersion(requestParams);

            var loggingContext = new ContextData();
            loggingContext.Load();
        }

        private static void SetUserIpContext()
        {
            if (HttpContext.Current.Items[RequestContextConstants.USER_IP] == null)
            {
                HttpContext.Current.Items.Add(RequestContextConstants.USER_IP, Utils.Utils.GetClientIP());
            }
        }

        private static void SetResponseProfile(IDictionary<string, object> requestParams, bool globalScope)
        {
            if (requestParams.ContainsKey("responseProfile") && requestParams["responseProfile"] != null)
            {

                //If object
                KalturaOTTObject responseProfile = null;
                Type type = typeof(KalturaBaseResponseProfile);
                if (requestParams["responseProfile"] is JObject)
                {
                    responseProfile = Deserializer.deserialize(type,
                        ((JObject)requestParams["responseProfile"]).ToObject<Dictionary<string, object>>());
                }
                else if (requestParams["responseProfile"] is Dictionary<string, object>)
                {
                    responseProfile = Deserializer.deserialize(type,
                       ((JObject)requestParams["responseProfile"]).ToObject<Dictionary<string, object>>());
                }

                if (globalScope && HttpContext.Current.Items[RequestContextConstants.REQUEST_RESPONSE_PROFILE] == null)
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_RESPONSE_PROFILE, responseProfile);
            }
            else if (HttpContext.Current.Items[RequestContextConstants.REQUEST_RESPONSE_PROFILE] != null)
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_RESPONSE_PROFILE, HttpContext.Current.Items[RequestContextConstants.REQUEST_RESPONSE_PROFILE]);
            }
        }

        private static void SetRequestTypeContext(string action)
        {
            if (HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] != null)
                HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_TYPE);

            if (action != null)
            {
                switch (action)
                {
                    case "register":
                    case "add":
                        HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] = RequestType.INSERT;
                        break;

                    case "update":
                        HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] = RequestType.UPDATE;
                        break;

                    case "get":
                    case "list":
                        HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] = RequestType.READ;
                        break;

                    default:
                        break;
                }
            }
        }

        private static void SetRequestFormatContext()
        {
            if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_FORMAT))
            {
                HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_FORMAT);
            }
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.GetQueryString()[RequestContextConstants.REQUEST_FORMAT]))
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_FORMAT, HttpContext.Current.Request.GetQueryString()[RequestContextConstants.REQUEST_FORMAT]);
            }
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.GetQueryString()[RequestContextConstants.RESPONSE_FORMAT]))
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_FORMAT, HttpContext.Current.Request.GetQueryString()[RequestContextConstants.RESPONSE_FORMAT]);
            }
        }

        private static void SetCurrencyContext(IDictionary<string, object> requestParams, bool globalScope)
        {
            HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_CURRENCY);
            if (requestParams.ContainsKey("currency") && requestParams["currency"] != null)
            {
                string currency;
                if (requestParams["currency"].GetType() == typeof(JObject) || requestParams["currency"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    currency = ((JObject)requestParams["currency"]).ToObject<string>();
                }
                else
                {
                    currency = (string)Convert.ChangeType(requestParams["currency"], typeof(string));
                }

                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_CURRENCY, currency);
                if (globalScope && HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_CURRENCY] == null)
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GLOBAL_CURRENCY, currency);
            }
            else if (HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_CURRENCY] != null)
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_CURRENCY, HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_CURRENCY]);
            }
        }

        private static void SetLanguageContext(IDictionary<string, object> requestParams, bool globalScope)
        {
            HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_LANGUAGE);
            if (requestParams.ContainsKey("language") && requestParams["language"] != null)
            {
                string language;
                if (requestParams["language"].GetType() == typeof(JObject) || requestParams["language"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    language = ((JObject)requestParams["language"]).ToObject<string>();
                }
                else
                {
                    language = (string)Convert.ChangeType(requestParams["language"], typeof(string));
                }

                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_LANGUAGE, language);
                if (globalScope && HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_LANGUAGE] == null)
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GLOBAL_LANGUAGE, language);
            }
            else if (HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_LANGUAGE] != null)
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_LANGUAGE, HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_LANGUAGE]);
            }
        }

        private static void SetImpersonateUserContext(IDictionary<string, object> requestParams, bool globalScope)
        {
            HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_USER_ID);
            if ((requestParams.ContainsKey("user_id") && requestParams["user_id"] != null) || (requestParams.ContainsKey("userId") && requestParams["userId"] != null))
            {
                object userIdObject = requestParams.ContainsKey("userId") ? requestParams["userId"] : requestParams["user_id"];
                string userId;
                if (userIdObject.GetType() == typeof(JObject) || userIdObject.GetType().IsSubclassOf(typeof(JObject)))
                {
                    userId = ((JObject)userIdObject).ToObject<string>();
                }
                else
                {
                    userId = (string)Convert.ChangeType(userIdObject, typeof(string));
                }

                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_USER_ID, userId);
                if (globalScope && HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_USER_ID] == null)
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GLOBAL_USER_ID, userId);
            }
            else if (HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_USER_ID] != null)
            {
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_USER_ID, HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_USER_ID]);
            }
            else
            {
                var userId = KS.GetFromRequest()?.UserId;
                if (userId.IsAnonymous())
                {
                    return;
                }

                if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_USER_ID))
                {
                    HttpContext.Current.Items[RequestContextConstants.REQUEST_USER_ID] = userId;
                }
                else
                {
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_USER_ID, userId);
                }
            }
        }

        private static void SetKsContext(IDictionary<string, object> requestParams, bool globalScope)
        {
            KS.ClearOnRequest();
            string ks = null;
            if (requestParams.ContainsKey("ks") && requestParams["ks"] != null)
            {
                if (requestParams["ks"].GetType() == typeof(JObject) || requestParams["ks"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    ks = ((JObject)requestParams["ks"]).ToObject<string>();
                }
                else
                {
                    ks = (string)Convert.ChangeType(requestParams["ks"], typeof(string));
                }

                InitKS(ks);

                if (globalScope && HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_KS] == null)
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GLOBAL_KS, ks);
            }
            else if (HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_KS] != null)
            {
                InitKS((string)HttpContext.Current.Items[RequestContextConstants.REQUEST_GLOBAL_KS]);
            }
            else if (requestParams.ContainsKey("partnerId") && requestParams["partnerId"] != null)
            {
                HttpContext.Current.Items[Constants.GROUP_ID] = requestParams["partnerId"];
            }

            KLogger.LogContextData[Constants.KS] = ks;
        }

        private static void InitKS(string ksVal)
        {
            // the supplied ks is in KS format (project phoenix's)
            if (KS.HasKsFormat(ksVal))
            {
                parseKS(ksVal);
            }
            // the supplied is in access token format (TVPAPI's)
            else
            {
                GetUserDataFromCB(ksVal);
            }
        }

        private static void parseKS(string ksVal)
        {
            KS ks = KS.ParseKS(ksVal);
            ks.SaveOnRequest();
        }
        
        private static void GetUserDataFromCB(string ksVal)
        {
            // get token from CB
            string tokenKey = string.Format(accessTokenKeyFormat, ksVal);
            var token = cbManager.Get<ApiToken>(tokenKey, true);

            if (token == null)
            {
                throw new RequestParserException(RequestParserException.INVALID_KS_FORMAT);
            }

            KS ks = KS.CreateKSFromApiToken(token, ksVal);

            if (!ks.IsValid)
            {
                throw new RequestParserException(RequestParserException.INVALID_KS_FORMAT);
            }

            ks.SaveOnRequest();
        }

        private static void SetRequestVersion(IDictionary<string, object> requestParams)
        {
            if (requestParams.ContainsKey("apiVersion"))
            {
                Version versionFromParams;
                if (Version.TryParse((string)requestParams["apiVersion"], out versionFromParams))
                {
                    HttpContext.Current.Items[RequestContextConstants.REQUEST_VERSION] = versionFromParams;
                }
                else
                {
                    throw new RequestParserException(RequestParserException.INVALID_VERSION, requestParams["apiVersion"]);
                }
            }
        }
    }
}