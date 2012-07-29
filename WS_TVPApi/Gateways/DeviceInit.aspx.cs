using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

public partial class Gateways_DeviceInit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Get querystring values for device bundle ID and device UDID
        string sBundleID = string.Empty;
        string sUDID = string.Empty;
        {
            try
            {
                sUDID = Request.QueryString["udid"].ToString();
                sBundleID = Request.QueryString["bid"].ToString();
            }
            catch (Exception ex) { }
            
            // change to check if method is post or get
            if (string.IsNullOrEmpty(sUDID) && string.IsNullOrEmpty(sBundleID))
            {
                Response.ContentType = "application/json; charset=utf-8";

                System.IO.Stream body = Request.InputStream;
                System.Text.Encoding encoding = Request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

                string sJsonRequest = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(sJsonRequest))
                {
                    Newtonsoft.Json.Linq.JObject param = Newtonsoft.Json.Linq.JObject.Parse(sJsonRequest);
                    sBundleID = param["bid"].ToString();
                    sUDID = param["udid"].ToString();
                }
            }

            if (string.IsNullOrEmpty(sUDID) && string.IsNullOrEmpty(sBundleID))
            {
                Response.Clear();
                Response.Write(@"{""Error"" : ""Missing 'bid' or 'udid' parameters""}");
            }

            // Get device init object
            DeviceInit deviceInit = GetInit(sUDID, sBundleID);

            // Serialize to json object
            //JavaScriptSerializer jsonSer = new JavaScriptSerializer();
            //jsonSer.RegisterConverters(new JavaScriptConverter[] { new InitEnumConverter() });
            //string sResponseJSON = jsonSer.Serialize(deviceInit);

            string sResponseJSON = string.Empty;
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(deviceInit.GetType());
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    serializer.WriteObject(ms, deviceInit);
            //    sResponseJSON = Encoding.UTF8.GetString(ms.ToArray());
            //}

            sResponseJSON = JsonConvert.SerializeObject(deviceInit, new MyStringEnumConverter());

            // write json string
            Response.Clear();
            Response.Write(sResponseJSON);
        }
    }
    private DeviceInit GetInit(string sUDID, string sBundleID)
    {
        DeviceInit retDeviceInit = new DeviceInit();
        if (sBundleID.ToLower().Equals("com.tvinci.development.kanguroo"))
        {
            retDeviceInit.initObj = new TVPApi.InitializationObject();
            retDeviceInit.initObj.ApiUser = "tvpapi_144";
            retDeviceInit.initObj.ApiPass = "11111";
            retDeviceInit.initObj.SiteGuid = string.Empty;
            retDeviceInit.initObj.UDID = sUDID;
            retDeviceInit.initObj.Platform = TVPApi.PlatformType.Cellular;
            retDeviceInit.initObj.Locale = new TVPApi.Locale();
            retDeviceInit.initObj.Locale.LocaleUserState = TVPApi.LocaleUserState.Unknown;
            retDeviceInit.initObj.Locale.LocaleCountry = string.Empty;
            retDeviceInit.initObj.Locale.LocaleDevice = string.Empty;
            retDeviceInit.initObj.Locale.LocaleLanguage = string.Empty;

            retDeviceInit.GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsonpostgw.aspx";
            retDeviceInit.MainMenuID = "43";
            retDeviceInit.AllowBrowseMode = "true";
        }
        else if (sBundleID.ToLower().Equals("com.tvinci.development.toggle"))
        {
            retDeviceInit.initObj = new TVPApi.InitializationObject();
            retDeviceInit.initObj.ApiUser = "tvpapi_147";
            retDeviceInit.initObj.ApiPass = "11111";
            retDeviceInit.initObj.SiteGuid = string.Empty;
            retDeviceInit.initObj.UDID = sUDID;
            retDeviceInit.initObj.Platform = TVPApi.PlatformType.Cellular;
            retDeviceInit.initObj.Locale = new TVPApi.Locale();
            retDeviceInit.initObj.Locale.LocaleUserState = TVPApi.LocaleUserState.Unknown;
            retDeviceInit.initObj.Locale.LocaleCountry = string.Empty;
            retDeviceInit.initObj.Locale.LocaleDevice = string.Empty;
            retDeviceInit.initObj.Locale.LocaleLanguage = string.Empty;

            retDeviceInit.FacebookURL = "http://173.231.146.5/social/facebook_api.aspx?action=getdata&groupId=147&platform=1&domain=1";

            retDeviceInit.GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsonpostgw.aspx";
            retDeviceInit.MainMenuID = "45";
            retDeviceInit.AllowBrowseMode = "false";

            retDeviceInit.FilesFormat = new DeviceInit.oFilesFormat() { Main = "iOS Clear", Trailer = "Trailer" };
        }
        else if (sBundleID.ToLower().Equals("com.tvinci2.activities") || sBundleID.ToLower().Equals("com.tvinci.development.ipadtvincidemo"))
        {
            retDeviceInit.initObj = new TVPApi.InitializationObject();
            retDeviceInit.initObj.ApiUser = "tvpapi_125";
            retDeviceInit.initObj.ApiPass = "11111";
            retDeviceInit.initObj.SiteGuid = string.Empty;
            retDeviceInit.initObj.UDID = sUDID;
            retDeviceInit.initObj.Platform = TVPApi.PlatformType.iPad;
            retDeviceInit.initObj.Locale = new TVPApi.Locale();
            retDeviceInit.initObj.Locale.LocaleUserState = TVPApi.LocaleUserState.Unknown;
            retDeviceInit.initObj.Locale.LocaleCountry = string.Empty;
            retDeviceInit.initObj.Locale.LocaleDevice = string.Empty;
            retDeviceInit.initObj.Locale.LocaleLanguage = string.Empty;

            retDeviceInit.FacebookURL = "http://173.231.146.5/social/facebook_api.aspx?action=getdata&groupId=125&platform=3&domain=1";

            retDeviceInit.GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsonpostgw.aspx";
            retDeviceInit.MainMenuID = "26";
            retDeviceInit.AllowBrowseMode = "true";

            if (sBundleID.ToLower().Equals("com.tvinci2.activities"))
                retDeviceInit.FilesFormat = new DeviceInit.oFilesFormat() { Main = "Android Tablet Main", Trailer = "Trailer" };
            else
                retDeviceInit.FilesFormat = new DeviceInit.oFilesFormat() { Main = "IPad Main", Trailer = "Trailer" };

            retDeviceInit.MediaTypes = new List<KeyValuePair<int, string>>();
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(272, "Movie")); 
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(273, "Episode"));
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(289, "Live"));
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(277, "Series"));
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(278, "Person"));
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(279, "Package"));
            retDeviceInit.MediaTypes.Add(new KeyValuePair<int, string>(215, "Article"));
        }

        return retDeviceInit;
    }
}



public class DeviceInit
{
    public TVPApi.InitializationObject initObj { get; set; }
    public string GatewayURL { get; set; }
    public string LogoURL { get; set; }
    public string FacebookURL { get; set; }
    public string SmallPicSize { get; set; }
    public string MediumPicSize { get; set; }
    public string LargePicSize { get; set; }
    public string HD { get; set; }
    public string SD { get; set; }
    public string MainMenuID { get; set; }
    public string AllowBrowseMode { get; set; }
    public oFilesFormat FilesFormat { get; set; }
    public List<KeyValuePair<int, string>> MediaTypes { get; set; }

    public struct oFilesFormat
    {
        public string Trailer;
        public string Main;
    }
}

public class MyStringEnumConverter : Newtonsoft.Json.Converters.StringEnumConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        
        if (value is TVPApi.LocaleUserState)
        {
            writer.WriteValue(Enum.GetName(typeof(TVPApi.LocaleUserState), (TVPApi.LocaleUserState)value));
            return;
        }
        else if(value is TVPApi.PlatformType)
        {
            writer.WriteValue(Enum.GetName(typeof(TVPApi.PlatformType), (TVPApi.PlatformType)value));
            return;
        }

        base.WriteJson(writer, value, serializer);
    }
}