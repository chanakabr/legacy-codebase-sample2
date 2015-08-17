using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using KLogMonitor;
using System.Reflection;
using TVPPro.SiteManager.Helper;
using System.Security.Cryptography;
using System.Configuration;

public partial class Gateways_JsonPostGW : BaseGateway
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        Response.AppendHeader("Access-Control-Allow-Origin", "*");

        Stream body = Request.InputStream;
        Encoding encoding = Request.ContentEncoding;
        StreamReader reader = new System.IO.StreamReader(body, encoding);

        string sJsonRequest = reader.ReadToEnd();


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
                                HttpContext.Current.Items[Constants.USER_ID] = initObj.SiteGuid;

                            // get group ID
                            if (initObj.ApiUser != null && initObj.ApiUser != null)
                            {
                                int groupId = ConnectionHelper.GetGroupID("tvpapi", "Gateways_JsonPostGW", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
                                HttpContext.Current.Items[Constants.GROUP_ID] = groupId;
                            }
                        }
                    }
                }

                HttpContext.Current.Items[pair.Key] = sValue;
            }

            // log request body
            logger.DebugFormat("API Request - \n{0}", sJsonRequest);
        }


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

        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START, null, null, null, null))
        {
            queryServices.ProcessRequest(sJsonRequest);
        }
    }

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
        var key = ConfigurationManager.AppSettings["initObj_key"];
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
}