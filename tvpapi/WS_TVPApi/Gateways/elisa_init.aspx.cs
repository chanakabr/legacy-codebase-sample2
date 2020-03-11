using System;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        String jsonStr = serializer.Serialize(new InitConfig());
        Response.Write(jsonStr);
    }

    class InitConfig
    {
        public string LogoURL = "http://ibc.cdngc.net/Elisa/Images/EpicTV_CTV_LOGO.png";
        public string HomePageChannelID = "327646";
        public string RootCateroryID = "1161";
        public string GatewayURL = "http://tvpapi.tvinci.com/v1_7/gateways/jsongateway.aspx";
        public string ApiUser = "tvpapi_134";
        public string ApiPass = "11111";
        public string Platform = "ConnectedTV";
        public string SmallPicSize = "";
        public string MediumPicSize = "";
        public string LargePicSize = "";
        public string SubscriptionID = "277";
        //public List<MethodTemplate> MethodTemplates = new List<MethodTemplate>();
        //public ArrayList Users = new ArrayList();

        public InitConfig()
        {
            //MethodTemplates.Add(new MethodTemplate(){
            //    Method = "GetFullCategory", 
            //    RequestURL = "?MethodName=GetFullCategory&LocaleUserState=Unknown&Platform=ConnectedTV&SiteGuid={SiteGuid}&DomainID={DomainID}&UDID={UDID}&ApiUser={ApiUser}&ApiPass={ApiPass}&categoryID={CategoryID}"
            //});

            //MethodTemplates.Add(new MethodTemplate()
            //{
            //    Method = "GetChannelMediaList",
            //    RequestURL = "?MethodName=GetChannelMediaList&LocaleUserState=Unknown&Platform=ConnectedTV&SiteGuid={SiteGuid}&DomainID={DomainID}&UDID={UDID}&ApiUser={ApiUser}&ApiPass={ApiPass}&ChannelID={ChannelID}&picSize={PicSize}&pageSize={PageSize}&pageIndex={PageIndex}"
            //});

            //MethodTemplates.Add(new MethodTemplate()
            //{
            //    Method = "GetMediaInfo",
            //    RequestURL = "?MethodName=GetMediaInfo&LocaleUserState=Unknown&Platform=ConnectedTV&SiteGuid={SiteGuid}&DomainID={DomainID}&UDID={UDID}&ApiUser={ApiUser}&ApiPass={ApiPass}&MediaID={MediaID}&mediaType={MediaType}&picSize={PicSize}&withDynamic=false"
            //});

            //MethodTemplates.Add(new MethodTemplate()
            //{
            //    Method = "GetMediaInfo",
            //    RequestURL = "?MethodName=GetMediaInfo&LocaleUserState=Unknown&Platform=ConnectedTV&SiteGuid={SiteGuid}&DomainID={DomainID}&UDID={UDID}&ApiUser={ApiUser}&ApiPass={ApiPass}&MediaID={MediaID}&mediaType={MediaType}&picSize={PicSize}&withDynamic=false"
            //});
        }

        public struct MethodTemplate
        {
            public string Method;
            public string RequestURL;
        }

        class User
        {

            public string UserName;
            public string Password;

            public User(string userName, string password)
            {
                this.UserName = userName;
                this.Password = password;
            }
        }

        
    }

}