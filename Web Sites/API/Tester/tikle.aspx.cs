using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class tikle : System.Web.UI.Page
{


    protected void GetUserData(string sUserGUID, string sFuncUN, string sFuncPass)
    {
        users.UsersService t = new users.UsersService();
        users.UserResponseObject resp = t.GetUserData(sFuncUN, sFuncPass, sUserGUID);
        string sCountryName = resp.m_user.m_oBasicData.m_Country.m_sCountryName;
        string sAddress = resp.m_user.m_oBasicData.m_sAddress;
        string sFirstName = resp.m_user.m_oBasicData.m_sFirstName;
        string sLastName = resp.m_user.m_oBasicData.m_sLastName;
        string sPhone = resp.m_user.m_oBasicData.m_sPhone;

        string sUserName = resp.m_user.m_oBasicData.m_sUserName;
        string sZip = resp.m_user.m_oBasicData.m_sZip;
        string sSiteGUID = resp.m_user.m_sSiteGUID;
        if (resp.m_user.m_oDynamicData.m_sUserData != null)
        {
            Int32 nDynamicDataCount = resp.m_user.m_oDynamicData.m_sUserData.Length;
            for (int i = 0; i < nDynamicDataCount; i++)
            {
                string sDynamicType = resp.m_user.m_oDynamicData.m_sUserData[i].m_sDataType;
                string sDynamicValue = resp.m_user.m_oDynamicData.m_sUserData[i].m_sValue;
            }
        }
        return;
    }

    //protected void GetSubscriptionData(string sSubscriptionGUID, string sFuncUN, string sFuncPass)
    //{
    //    pricing.mdoule m = new pricing.mdoule();
    //    pricing.Subscription theSub = m.GetSubscriptionData(sFuncUN, sFuncPass, sSubscriptionGUID);
    //    if (theSub.m_sName != null)
    //    {
    //        Int32 nNameLangLength = theSub.m_sName.Length;
    //        for (int i = 0; i < nNameLangLength; i++)
    //        {
    //            string sLang = theSub.m_sName[i].m_sLanguageCode3;
    //            string sVal = theSub.m_sName[i].m_sValue;
    //        }
    //    }
    //    if (theSub.m_sDescription != null)
    //    {
    //        Int32 nDescLangLength = theSub.m_sDescription.Length;
    //        for (int i = 0; i < nDescLangLength; i++)
    //        {
    //            string sLang = theSub.m_sDescription[i].m_sLanguageCode3;
    //            string sVal = theSub.m_sDescription[i].m_sValue;
    //        }
    //    }


    //    return;
    //}

    static protected api_ws.InitializationObject ConstructInitObj(string sPlayerID , string sPlayerKey)
    {
        api_ws.InitializationObject initObj = new api_ws.InitializationObject();
        initObj.m_oExtraRequestObject = new api_ws.ExtraRequestObject();
        initObj.m_oExtraRequestObject.m_bNoCache = false;
        initObj.m_oExtraRequestObject.m_bZip = false;
        initObj.m_oExtraRequestObject.m_bWithFileTypes = false;
        initObj.m_oExtraRequestObject.m_bWithInfo = true;

        initObj.m_oFileRequestObjects = new api_ws.FileRequestObject[0];

        initObj.m_oLanguageRequestObject = new api_ws.LanguageRequestObject();
        initObj.m_oLanguageRequestObject.m_sFullName = "Turkish";
        
        initObj.m_oPicObjects = new api_ws.PicObject[0];
        initObj.m_oPlayerIMRequestObject = new api_ws.PlayerIMRequestObject();
        initObj.m_oPlayerIMRequestObject.m_sPalyerID = sPlayerID;
        initObj.m_oPlayerIMRequestObject.m_sPlayerKey = sPlayerKey;
        
        initObj.m_oUserIMRequestObject = new api_ws.UserIMRequestObject();
        return initObj;
    }

    protected void GetMediaInfo(Int32 nMediaID , string sFuncUN , string sFuncPass , string sPlayerID , string sPlayerKey)
    {
        api_ws.tvapi t = new api_ws.tvapi();
        Int32[] nMedias = { 83521 };
        api_ws.ChannelObject c = t.TVAPI_GetMediaInfo(sFuncUN, sFuncPass, DateTime.Now, ConstructInitObj(sPlayerID, sPlayerKey), nMedias, null);
        if (c != null && c.m_oMediaObjects != null && c.m_oMediaObjects.Length == 1 && c.m_oMediaObjects[0].m_oMediaInfo != null)
        {
            Int32 nMediaIDFromResp = c.m_oMediaObjects[0].m_nMediaID;
            string sMediaName = c.m_oMediaObjects[0].m_oMediaInfo.m_sTitle;
            string sMediaDescription = c.m_oMediaObjects[0].m_oMediaInfo.m_sDescription;
        }
        return;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //GetUserData("201", "users_96", "ausdgf654w");
        //For regular content (should work with those parameters)
        //GetSubscriptionData("5", "pricing_96", "ausdgf654w");
        //For adult content (should pass different subscription id)
        //GetSubscriptionData("5", "pricing_97", "ausdgf654w");
        //For regular content (should work)
        //GetMediaInfo(83521, "api_96", "ausdgf654w", "TRVideos-main", "TRVideos-main");
        //For adult content (should pass different media id)
        //GetMediaInfo(83521, "api_97", "ausdgf654w", "TAVideos-main", "TAVideos-main");
        //users.UsersService u = new users.UsersService();
        //u.Url = "http://localhost:63535/module.asmx";
        //users.UserResponseObject resp = u.GetUserData("users_93", "11111", "460");
        //string sFacrBookID = resp.m_user.m_oBasicData.m_sFacebookID;
        //resp.m_user.m_oBasicData.m_sFacebookID = "1111111111";
        //resp.m_user.m_oBasicData.m_sFirstName = "moshe";
        //u.SetUserData("users_93", "11111", "460", resp.m_user.m_oBasicData, resp.m_user.m_oDynamicData);
        //users.UsersService u = new users.UsersService();
        //u.Url = "http://platform-us.tvinci.com/users/module.asmx";
        //users.UserBasicData basic = new users.UserBasicData();
        //basic.m_sEmail = "arikgaisler@gmail.com";
        //basic.m_sUserName = "123456789";
        //basic.m_sPhone = "123456789";
        //users.UserDynamicData dynamic = new users.UserDynamicData();

        //users.UserResponseObject resp = u.AddNewUser("users_96", "11111", basic, dynamic, "123456", string.Empty);

        ca.module module = new ca.module();
        module.Url = "http://localhost/TVMCA/module.asmx";
        string url = module.GetLicensedLink("conditionalaccess_96", "11111", "99578", 221664, "rtmpe://cp94204.edgefcs.net/ondemand/mp4:videos/Chantier/milyoner-slumdog-millionaire-video.mp4", string.Empty, string.Empty, "Turkey", "Turkish", "Main Web Site");
    }
}
