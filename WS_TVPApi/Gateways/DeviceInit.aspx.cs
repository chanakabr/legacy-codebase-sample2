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
        if (!Request.QueryString.AllKeys.Contains("bid") || !Request.QueryString.AllKeys.Contains("udid"))
        {
            Response.Clear();
            Response.Write(@"{""Error"" : ""Missing 'bid' or 'udid' parameters""}");
        }
        else
        {
            sUDID = Request.QueryString["udid"].ToString();
            sBundleID = Request.QueryString["bid"].ToString();

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
        if (sBundleID.ToLower().Equals("com.tvinci.kanguroo"))
        {
            retDeviceInit.initObj = new TVPApi.InitializationObject();
            retDeviceInit.initObj.ApiUser = "tvpapi_144";
            retDeviceInit.initObj.ApiPass = "11111";
            retDeviceInit.initObj.SiteGuid = string.Empty;
            retDeviceInit.initObj.UDID = string.Empty;
            retDeviceInit.initObj.Platform = TVPApi.PlatformType.Cellular;
            retDeviceInit.initObj.Locale = new TVPApi.Locale();
            retDeviceInit.initObj.Locale.LocaleUserState = TVPApi.LocaleUserState.Unknown;
            retDeviceInit.initObj.Locale.LocaleCountry = string.Empty;
            retDeviceInit.initObj.Locale.LocaleDevice = string.Empty;
            retDeviceInit.initObj.Locale.LocaleLanguage = string.Empty;

            retDeviceInit.GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsonpostgw.aspx";
            retDeviceInit.SpotlightCatID = "1250";
            retDeviceInit.HomeLeftCatID = "1251";
            retDeviceInit.HomeRightCatID = "1252";
            retDeviceInit.RootCatID = "1235";
        }

        return retDeviceInit;
    }
}



public class DeviceInit
{
    public TVPApi.InitializationObject initObj { get; set; }
    public string GatewayURL { get; set; }
    public string LogoURL { get; set; }
    public string SpotlightCatID { get; set; }
    public string HomeLeftCatID { get; set; }
    public string HomeRightCatID { get; set; }
    public string RootCatID { get; set; }
    public string SmallPicSize { get; set; }
    public string MediumPicSize { get; set; }
    public string LargePicSize { get; set; }
    public string HD { get; set; }
    public string SD { get; set; }
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