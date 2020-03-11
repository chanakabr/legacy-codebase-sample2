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



    public partial class idc : System.Web.UI.Page
    {
        static protected api_ws.InitializationObject ConstructInitObj(string sLanguage,
            Int32 nPicWidth, Int32 nPicHeight, string sSiteGUID,
            string sTvinciGUID, string sUserIP)
        {
            api_ws.InitializationObject initObj = new api_ws.InitializationObject();
            initObj.m_oExtraRequestObject = new api_ws.ExtraRequestObject();
            initObj.m_oExtraRequestObject.m_bNoCache = false;
            initObj.m_oExtraRequestObject.m_bZip = false;
            initObj.m_oExtraRequestObject.m_bWithFileTypes = false;
            initObj.m_oExtraRequestObject.m_bWithInfo = true;

            initObj.m_oFileRequestObjects = new api_ws.FileRequestObject[1];
            initObj.m_oFileRequestObjects[0] = new api_ws.FileRequestObject();
            initObj.m_oFileRequestObjects[0].m_sFileFormat = "Video"; // Can bve also "WMP"
            initObj.m_oFileRequestObjects[0].m_sFileQuality = "HIGH";


            initObj.m_oLanguageRequestObject = new api_ws.LanguageRequestObject();
            initObj.m_oLanguageRequestObject.m_sFullName = sLanguage;

            initObj.m_oPicObjects = new api_ws.PicObject[1];
            initObj.m_oPicObjects[0] = new api_ws.PicObject();
            initObj.m_oPicObjects[0].m_nPicHeight = nPicHeight;
            initObj.m_oPicObjects[0].m_nPicWidth = nPicWidth;

            initObj.m_oPlayerIMRequestObject = new api_ws.PlayerIMRequestObject();
            initObj.m_oPlayerIMRequestObject.m_sPalyerID = "IDC-main";
            initObj.m_oPlayerIMRequestObject.m_sPlayerKey = "IDC-main";

            initObj.m_oUserIMRequestObject = new api_ws.UserIMRequestObject();
            initObj.m_oUserIMRequestObject.m_sSiteGuid = sSiteGUID;
            initObj.m_oUserIMRequestObject.m_sTvinciGuid = sTvinciGUID;
            initObj.m_oUserIMRequestObject.m_sUserAgent = "";
            initObj.m_oUserIMRequestObject.m_sUserIP = sUserIP;

            return initObj;
        }

        protected void TVAPI_SearchMedia(ref api_ws.tvapi t)
        {
            DateTime dUTC = DateTime.Now;
            api_ws.PageDefinition thePage = new api_ws.PageDefinition();
            thePage.m_nNumberOfItems = 20;
            thePage.m_nStartIndex = 0;
            api_ws.InitializationObject initObj = ConstructInitObj("hebrew", 544, 408, "Site GUID", "Tvinci GUID", "1.1.1.1");
            api_ws.SearchDefinitionObject theSearchDef = new api_ws.SearchDefinitionObject();
            theSearchDef.m_bExact = false;
            theSearchDef.m_eAndOr = api_ws.AndOr.And;
            theSearchDef.m_oPageDefinition = thePage;
            theSearchDef.m_sOrderByObjects = new api_ws.SearchOrderByObject[2];
            theSearchDef.m_sOrderByObjects[0] = new api_ws.SearchOrderByObject();
            theSearchDef.m_sOrderByObjects[0].m_eOrderBy = api_ws.OrderBy.Asc;
            theSearchDef.m_sOrderByObjects[0].m_sOrderField = "name";
            theSearchDef.m_sOrderByObjects[0].m_nOrderNum = 1;
            theSearchDef.m_sTitle = "Congress";
            //theSearchDef.m_sDescription = "Heidi";

            theSearchDef.m_oTagObjects = new api_ws.MetaM2MObject[1];
            theSearchDef.m_oTagObjects[0] = new api_ws.MetaM2MObject();
            theSearchDef.m_oTagObjects[0].m_sMetaName = "Mood";
            theSearchDef.m_oTagObjects[0].m_sMetaValues = new string[1];
            theSearchDef.m_oTagObjects[0].m_sMetaValues[0] = "אומנותי";

            api_ws.ChannelObject resp = t.TVAPI_SearchMedia("api_56", "IDCdh34ss", dUTC, initObj, theSearchDef, null);
            Serliaze(resp);
        }

        protected void Serliaze(object o)
        {
            if (o == null)
            {
                Response.Write("<NULL/>");
                return;
            }
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(o.GetType());

            System.IO.StringWriter w = new System.IO.StringWriter();
            x.Serialize(w, o);
            Response.ContentType = "text/xml";
            Response.Expires = -1;
            Response.Write(w.GetStringBuilder().ToString().Substring(39));
            Response.End();
        }

        protected void TVAPI_CategoriesTree(ref api_ws.tvapi t)
        {
            DateTime dUTC = DateTime.Now;
            api_ws.PageDefinition thePage = new api_ws.PageDefinition();
            thePage.m_nNumberOfItems = 20;
            thePage.m_nStartIndex = 0;
            api_ws.InitializationObject initObj = ConstructInitObj("hebrew", 544, 408, "Site GUID", "Tvinci GUID", "1.1.1.1");

            api_ws.CategoryObject[] resp = t.TVAPI_CategoriesTree("api_56", "IDCdh34ss", dUTC, initObj, null, 0, true);
            Serliaze(resp);
        }

        protected void TVAPI_ChannelsMedia(ref api_ws.tvapi t)
        {
            DateTime dUTC = DateTime.Now;
            api_ws.PageDefinition thePage = new api_ws.PageDefinition();
            thePage.m_nNumberOfItems = 20;
            thePage.m_nStartIndex = 0;
            api_ws.InitializationObject initObj = ConstructInitObj("hebrew", 544, 408, "Site GUID", "Tvinci GUID", "1.1.1.1");

            api_ws.ChannelRequestObject[] theChannels = new api_ws.ChannelRequestObject[2];
            theChannels[0] = new api_ws.ChannelRequestObject();
            theChannels[0].m_nChannelID = 316461;

            theChannels[1] = new api_ws.ChannelRequestObject();
            theChannels[1].m_nChannelID = 316460;
            api_ws.ChannelObject[] resp = t.TVAPI_ChannelsMedia("api_56", "IDCdh34ss", dUTC, initObj, null, theChannels);
            Serliaze(resp);
        }

        protected void TVAPI_GetMedias(ref api_ws.tvapi t)
        {
            DateTime dUTC = DateTime.Now;
            api_ws.InitializationObject initObj = ConstructInitObj("hebrew", 544, 408, "Site GUID", "Tvinci GUID", "1.1.1.1");
            Int32[] n = { 84070 };
            api_ws.ChannelObject resp = t.TVAPI_GetMedias("api_56", "IDCdh34ss", dUTC, initObj, n, null);
            Serliaze(resp);
        }


        protected void TVAPI_CategoryChannels(ref api_ws.tvapi t, int CategoryID)
        {
            DateTime dUTC = DateTime.Now;
            api_ws.InitializationObject initObj = ConstructInitObj("hebrew", 544, 408, "Site GUID", "Tvinci GUID", "1.1.1.1");

            //api_ws.MediaInfoStructObject mISO = new api_ws.MediaInfoStructObject();
            //mISO.

            api_ws.MediaInfoStructObject infoSt = new api_ws.MediaInfoStructObject();
            infoSt.m_bDescription = true;
            infoSt.m_bPersonal = false;
            infoSt.m_bStatistics = false;
            infoSt.m_bTitle = true;
            infoSt.m_bType = true;
            infoSt.m_sMetaStrings = new string[2];
            infoSt.m_sMetaStrings[0] = "Short Description";
            infoSt.m_sMetaStrings[1] = "Production Date";
            

            api_ws.ChannelObject[] resp = t.TVAPI_CategoryChannels("api_56", "IDCdh34ss", dUTC, initObj, infoSt, CategoryID);

            Serliaze(resp);


        }


        protected void Page_Load(object sender, EventArgs e)
        {
            api_ws.tvapi t = new api_ws.tvapi();
            //TVAPI_SearchMedia(ref t);
            //TVAPI_CategoriesTree(ref t);
            //TVAPI_ChannelsMedia(ref t);
            //TVAPI_GetMedias(ref t);

            TVAPI_CategoryChannels(ref t, 642);
        }
    }

