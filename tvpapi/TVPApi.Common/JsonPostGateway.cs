using Phx.Lib.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using KalturaRequestContext;
using TVPApiServices;
using TVPPro.SiteManager.Helper;

namespace TVPApi.Common
{
    public class JsonPostGateway 
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected MediaService m_MediaService;
        protected SiteService m_SiteService;
        protected PricingService m_PricingService;
        protected DomainService m_DomainService;
        protected BillingService m_BillingService;
        protected ConditionalAccessService m_ConditionalAccessService;
        protected SocialService m_SocialService;
        protected UsersService m_UsersService;
        protected NotificationService m_NotificationService;

        static JsonPostGateway()
        {

        }

        public JsonPostGateway()
        {
            m_MediaService = new MediaService();
            m_SiteService = new SiteService();
            m_PricingService = new PricingService();
            m_DomainService = new DomainService();
            m_BillingService = new BillingService();
            m_ConditionalAccessService = new ConditionalAccessService();
            m_SocialService = new SocialService();
            m_UsersService = new UsersService();
            m_NotificationService = new NotificationService();
        }

        #region Main Logic

        public string ProcessRequest(string sJsonRequest)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(sJsonRequest))
            {
                JObject json = JObject.Parse(sJsonRequest);

                try
                {
                    if (json["initObj"] is JValue)
                    {
                        json = DecryptInitObj(json);
                    }
                }
                catch (Exception ex) { }

                foreach (KeyValuePair<string, JToken> pair in json)
                {
                    string sValue = string.Empty;

                    if (pair.Value.GetType() == typeof(JArray))
                        sValue = pair.Value.ToString(Newtonsoft.Json.Formatting.None);
                    else
                    {
                        sValue = pair.Value.ToString(Newtonsoft.Json.Formatting.None);

                        // Remove opening and closing ""
                        if ((!pair.Key.Equals("initObj") &&
                             !pair.Key.Equals("tagPairs") &&
                             !pair.Key.Equals("metaPairs") &&
                             !pair.Key.Equals("userBasicData") &&
                             !pair.Key.Equals("userDynamicData")) &&
                             !pair.Key.Equals("orderObj") &&
                             !pair.Key.Equals("recordedEPGOrderObj"))
                        {
                            if (sValue[0] == '\"' && sValue[sValue.Length - 1] == '\"')
                            {
                                sValue = sValue.Remove(sValue.Length - 1).Substring(1);
                            }
                        }

                        if (pair.Key.Equals("initObj"))
                        {
                            InitializationObject initObj = JsonConvert.DeserializeObject<InitializationObject>(pair.Value.ToString());
                            if (initObj != null)
                            {
                                // get user ID
                                if (initObj.SiteGuid != null)
                                {
                                    HttpContext.Current.Items[Constants.USER_ID] = initObj.SiteGuid;
                                    HttpContext.Current.Items[RequestContextConstants.REQUEST_USER_ID] = initObj.SiteGuid;
                                }

                                // get group ID
                                if (initObj.ApiUser != null && initObj.ApiUser != null)
                                {
                                    //this will print a monitor log with wrong group ID but we need the DB call to identify the group id, so we just clear the group id from klogger
                                    KLogger.SetGroupId("");
                                    HttpContext.Current.Items[Constants.GROUP_ID] = "";
                                    
                                    int groupId = ConnectionHelper.GetGroupID("tvpapi", "Gateways_JsonPostGW", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
                                    HttpContext.Current.Items[Constants.GROUP_ID] = groupId;
                                    HttpContext.Current.Items[RequestContextConstants.REQUEST_GROUP_ID] = groupId;
                                    KLogger.SetGroupId(groupId.ToString());
                                }
                            }
                        }
                    }

                    HttpContext.Current.Items[pair.Key] = sValue;
                }

                // mask api user and password before logging request

                #if !NETCOREAPP3_1

                try
                {
                    var initObj = json["initObj"];

                    if (initObj != null && initObj is JObject jInitObj)
                    {
                        MaskField(jInitObj, "ApiUser");
                        MaskField(jInitObj, "ApiPass");
                    }

                    // mask known parameters of passwords
                    MaskField(json, "sPassword");
                    MaskField(json, "sOldPass");
                    MaskField(json, "sPass");
                    MaskField(json, "sNewPassword");
                    MaskField(json, "sEncryptedPassword");
                    MaskField(json, "password");

                    var userBasicData = json["userBasicData"];

                    if (userBasicData != null)
                    {
                        MaskField(userBasicData, "m_sEmailField");
                        MaskField(userBasicData, "m_sEmail");
                    }

                    // log request body
                    logger.DebugFormat("API Request - \n{0}", json.ToString(Formatting.Indented));
                }
                catch (Exception ex)
                {
                    logger.Error($"Error when trying to remove user/password from request body before logging it. ex = {ex}");
                }
                #endif
            }

            using (_ = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
            {
                // add web service
                MethodFinder queryServices = new MethodFinder(m_MediaService,
                    m_SiteService,
                    m_PricingService,
                    m_DomainService,
                    m_BillingService,
                    m_ConditionalAccessService,
                    m_SocialService,
                    m_UsersService,
                    m_NotificationService);

                result = queryServices.ProcessRequest(sJsonRequest);
            }

            return result;
        }

        private static void MaskField(JToken source, string field)
        {
            var subField = source[field];
            if (subField != null && subField is JValue jSubField)
            {
                jSubField.Value = "*****";
            }
        }

        #endregion

        #region Getters

        public object[] GetWebServices()
        {
            object[] result = {
                m_MediaService,
                                                        m_SiteService,
                                                        m_PricingService,
                                                        m_DomainService,
                                                        m_BillingService,
                                                        m_ConditionalAccessService,
                                                        m_SocialService,
                                                        m_UsersService,
                                                        m_NotificationService };
            return result;
        }

        public MediaService GetMediaService()
        {
            return m_MediaService;
        }

        public DomainService GetDomainService()
        {
            return m_DomainService;
        }

        public SiteService GetSiteService()
        {
            return m_SiteService;

        }

        #endregion

        #region Utility methods

        private JObject DecryptInitObj(JObject data)
        {
            if (data != null && data["initObj"] != null)
            {
                string initObj = data["initObj"].ToString();

                string plain_initObj = DescryptAES256(Convert.FromBase64String(initObj));
                data["initObj"] = JObject.Parse(plain_initObj);
            }

            return data;
        }

        private string DescryptAES256(byte[] cipherText)
        {
            string plaintext = null;
            //var message = "this is my message"
            var key = System.Configuration.ConfigurationManager.AppSettings["initObj_key"];
            var sha256 = new SHA256CryptoServiceProvider();
            var pwBytes = Encoding.UTF8.GetBytes(key);
            var res = sha256.ComputeHash(pwBytes);

            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.IV = new byte[16];
            aes.Key = res;
            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, crypto, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream 
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        #endregion

    }
}
