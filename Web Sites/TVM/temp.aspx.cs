using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using System.Net;
using Dundas.Olap.Data.AdomdNet;
using Dundas.Olap.Data;
using System.Xml;
using System.IO;
using KLogMonitor;
using System.Reflection;


public partial class temp : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    public void GetMainMenu()
    {
        Int32 nMenuID = 0;
        //Response.Write(TVinciShared.Menu.GetMainMenu(12, true, ref nMenuID));

    }

    private void CreateExcel(int groupID)
    {
        Response.Write(groupID.ToString() + "</br>");
        ODBCWrapper.DataSetSelectQuery usersSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        usersSelectQuery.SetConnectionKey("users_connection");
        usersSelectQuery += "select * from users where ";
        usersSelectQuery += "GROUP_ID = " + groupID.ToString();
        usersSelectQuery += " and status = 1 and IS_ACTIVE = 1 and (users.ID not in (select site_user_guid from [ConditionalAccess].dbo.subscriptions_purchases)) and (users.ID not in (select site_user_guid from [ConditionalAccess].dbo.ppv_purchases ))";
        usersSelectQuery += " and id between 120001 and 130000";
        DataTable dt = new DataTable();
        Int32 n = 0;
        string s = string.Empty;
        dt.Columns.Add(PageUtils.GetColumn("UserName", s));
        dt.Columns.Add(PageUtils.GetColumn("E-mail", s));

        if (usersSelectQuery.Execute("usersQuery", true) != null)
        {
            int usersCount = usersSelectQuery.Table("usersQuery").DefaultView.Count;
            GridView gv = new GridView();

            gv.DataSource = usersSelectQuery.Table("usersQuery");
            gv.DataBind();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            usersSelectQuery.Finish();
            usersSelectQuery = null;
            gv.RenderControl(htmlWrite);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
            return;
            Response.Write("Found " + usersCount.ToString() + " users" + "</br>");
            int ppvCount = 0;
            int subCount = 0;
            for (int i = 0; i < usersCount; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = dt.NewRow();
                int guid = int.Parse(usersSelectQuery.Table("usersQuery").DefaultView[i].Row["ID"].ToString());
                string userName = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["USERNAME"].ToString();
                string email = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["EMAIL_ADD"].ToString();

                tmpRow["UserName"] = userName;
                tmpRow["E-mail"] = email;
                dt.Rows.InsertAt(tmpRow, dt.Rows.Count);
                dt.AcceptChanges();
                //ppvSelectQuery.Finish();
                //ppvSelectQuery = null;
                //subSelectQuery.Finish();
                //subSelectQuery = null;
            }

        }


        //Response.Write("Before excel");
        //GridView gv = new GridView();

        //gv.DataSource = dt;
        //gv.DataBind();
        //HttpContext.Current.Response.Clear();
        //HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
        //HttpContext.Current.Response.Charset = "UTF-8";
        //HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        //System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        //HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        //gv.RenderControl(htmlWrite);
        //HttpContext.Current.Response.Write(stringWrite.ToString());
        //HttpContext.Current.Response.End();
    }

    protected void AddPermitionsToAccount(Int32 nAccountID)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select distinct id from admin_menu where id not in (";
        selectQuery += "select distinct menu_id from admin_accounts_permissions where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", nAccountID);
        selectQuery += ")";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nMenuID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("admin_accounts_permissions");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAccountID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", nMenuID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_PERMIT", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDIT_PERMIT", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NEW_PERMIT", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REMOVE_PERMIT", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PUBLISH_PERMIT", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
        }

        selectQuery.Finish();
        selectQuery = null;
    }

    protected void LoadUsers()
    {
        XmlDocument theFeed = new XmlDocument();
        theFeed.Load("D:\\Versions\\users.xml");
        XmlNodeList theUsers = theFeed.DocumentElement.SelectNodes("users");
        IEnumerator relIter = theUsers.GetEnumerator();
        while (relIter.MoveNext())
        {
            XmlNode theRef = (XmlNode)(relIter.Current);
            string sPass = GetSafeValueFromXML(ref theRef, "pass");
            string sEmail = GetSafeValueFromXML(ref theRef, "mail");
            string sCreated = GetSafeValueFromXML(ref theRef, "created");
            DateTime d = new DateTime(1970, 1, 1);
            if (sCreated != "")
                d = d.AddSeconds(long.Parse(sCreated));
            Int32 nUserID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("users_connection");
            selectQuery += "select id from users where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sEmail);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EMAIL_ADD", "=", sEmail);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nUserID == 0 && sEmail != "" && sPass != "")
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users");
                insertQuery.SetConnectionKey("users_connection");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sEmail);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EMAIL_ADD", "=", sEmail);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_ID", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_IMAGE", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_IMAGE_PERMITTED", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_NAME", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_NAME", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADDRESS", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CITY", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ZIP", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PHONE", "=", "");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 93);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", d);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATE_STATUS", "=", 1);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
        }
    }

    static protected string GetSafeValueFromXML(ref XmlNode theRef, string sNodeXpath)
    {
        try
        {
            if (theRef.SelectSingleNode(sNodeXpath) != null)
            {
                string sRet = theRef.SelectSingleNode(sNodeXpath).FirstChild.Value;
                return sRet;
            }
            return "";
        }
        catch
        {
            return "";
        }
    }

    protected Int32 GetMaxID()
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select max(id)+1 as m from admin_menu";
        selectQuery.SetCachedSec(0);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["m"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected Int32 DuplicateFirstBranch(string sNewName, Int32 nMenuID, string sOldVal, string sNewVal)
    {
        Int32 nMaxID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from admin_menu where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMenuID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                nMaxID = GetMaxID();
                string sLink = selectQuery.Table("query").DefaultView[i].Row["MENU_HREF"].ToString();
                string sNewLink = sLink.Replace(sOldVal, sNewVal);
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "insert into admin_menu(ID,MENU_TEXT,MENU_HREF,MENU_ORDER,MENU_ORDER_VIS,PARENT_MENU_ID,IS_GROUP_HEADER,BELONG_TO_GROUP,ONLY_TVINCI,ONLY_CO,UPDATER_ID,CREATE_DATE) select " + nMaxID.ToString() + ",'" + sNewName + "','" + sNewLink + "',MENU_ORDER,MENU_ORDER_VIS,PARENT_MENU_ID,IS_GROUP_HEADER,BELONG_TO_GROUP,ONLY_TVINCI,ONLY_CO,UPDATER_ID,CREATE_DATE from admin_menu where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMenuID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nMaxID;
    }

    protected Int32 DuplicateBranch(Int32 nOldMenuID, Int32 nNewMenuID, string sOldVal, string sNewVal)
    {
        Int32 nMaxID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from admin_menu where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_MENU_ID", "=", nOldMenuID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                nMaxID = GetMaxID();
                Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                string sLink = selectQuery.Table("query").DefaultView[i].Row["MENU_HREF"].ToString();
                string sNewLink = sLink.Replace(sOldVal, sNewVal);
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "insert into admin_menu(ID,MENU_TEXT,MENU_HREF,MENU_ORDER,MENU_ORDER_VIS,PARENT_MENU_ID,IS_GROUP_HEADER,BELONG_TO_GROUP,ONLY_TVINCI,ONLY_CO,UPDATER_ID,CREATE_DATE) select " + nMaxID.ToString() + ",MENU_TEXT,'" + sNewLink + "',MENU_ORDER,MENU_ORDER_VIS," + nNewMenuID.ToString() + ",IS_GROUP_HEADER,BELONG_TO_GROUP,ONLY_TVINCI,ONLY_CO,UPDATER_ID,CREATE_DATE from admin_menu where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
                DuplicateBranch(nID, nMaxID, sOldVal, sNewVal);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nMaxID;
    }

    protected void AddMenuParameter(Int32 nMenuID, string sParameterName, string sParameterVal, bool bParent)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from admin_menu where ";
        if (bParent == false)
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMenuID);
        else
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_MENU_ID", "=", nMenuID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sLink = selectQuery.Table("query").DefaultView[i].Row["MENU_HREF"].ToString();
                Int32 nInnerMenuID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                if (sLink.ToLower().EndsWith(".aspx") == true || (sLink.ToLower().IndexOf(".aspx") != -1 && sLink.ToLower().IndexOf(sParameterName.ToLower() + "=" + sParameterVal.ToLower()) == -1))
                {
                    if (sLink.ToLower().EndsWith(".aspx") == true)
                        sLink += "?" + sParameterName + "=" + sParameterVal;
                    else
                        sLink += "&" + sParameterName + "=" + sParameterVal;
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("admin_menu");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_HREF", "=", sLink);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nInnerMenuID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                if (bParent == false)
                    AddMenuParameter(nMenuID, sParameterName, sParameterVal, true);
                else
                    AddMenuParameter(nInnerMenuID, sParameterName, sParameterVal, false);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

    }

    protected void dupliacteMediaStore(Int32 nOldParent, string sNewMSName, string sOldPlatform, string sNewPlatform)
    {
        Int32 nNewParent = DuplicateFirstBranch(sNewMSName, nOldParent, sOldPlatform, sNewPlatform);
        DuplicateBranch(nOldParent, nNewParent, sOldPlatform, sNewPlatform);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        int groupID = int.Parse(Request.QueryString["GroupID"]);
        string width = Request.QueryString["w"];
        string height = Request.QueryString["h"];
        SetPicToNewSize(groupID, false, width, height, @"C:\ode\TVM\Web Sites\TVM\pics\tele5");
    }

    static protected bool IsNodeExists(ref XmlNode theItem, string sXpath)
    {
        XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
        if (theNodeVal != null)
            return true;
        return false;
    }

    static protected string GetNodeValue(ref XmlNode theItem, string sXpath)
    {
        string sNodeVal = "";

        XmlNode theNodeVal = null;
        if (sXpath != "")
            theNodeVal = theItem.SelectSingleNode(sXpath);
        else
            theNodeVal = theItem;
        if (theNodeVal != null && theNodeVal.FirstChild != null)
            sNodeVal = theNodeVal.FirstChild.Value;
        return sNodeVal;
    }

    static protected string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
    {
        string sVal = "";
        if (theNode != null)
        {
            XmlAttributeCollection theAttr = theNode.Attributes;
            if (theAttr != null)
            {
                Int32 nCount = theAttr.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sName = theAttr[i].Name.ToLower();
                    if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                    {
                        sVal = theAttr[i].Value.ToString();
                        break;
                    }
                }
            }
        }
        return sVal;
    }

    static protected string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
    {
        string sVal = "";
        XmlNode theRoot = theNode.SelectSingleNode(sXpath);
        if (theRoot != null)
        {
            XmlAttributeCollection theAttr = theRoot.Attributes;
            if (theAttr != null)
            {
                Int32 nCount = theAttr.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sName = theAttr[i].Name.ToLower();
                    if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                    {
                        sVal = theAttr[i].Value.ToString();
                        break;
                    }
                }
            }
        }
        return sVal;
    }
    /*
    protected string ParseNanaXMLToImportXML(string sXMLUrl)
    {
        string sXML = "<feed><export>";
        XmlDocument theDoc = new XmlDocument();
        theDoc.Load(sXMLUrl);
        XmlNode thePeriod = theDoc.GetElementsByTagName("period")[0];
        string sPeriodStart = GetNodeValue(ref thePeriod, "start");
        string sPeriodEnd = GetNodeValue(ref thePeriod, "end");

        XmlNodeList theServices = theDoc.GetElementsByTagName("service");
        IEnumerator serviceIter = theServices.GetEnumerator();
        while (serviceIter.MoveNext())
        {
            XmlNode theService = (XmlNode)(serviceIter.Current);
            string sServiceID = GetNodeValue(ref theService, "serviceid");
            string sServiceTitle = GetNodeValue(ref theService, "title");
            string sServiceLink = GetNodeValue(ref theService, "link");
            string sServiceDescription = GetNodeValue(ref theService, "description");
            string sServiceLanguage = GetNodeValue(ref theService, "language");
            string sServiceCopyright = GetNodeValue(ref theService, "copyright");
            string sServiceThumb = GetNodeValue(ref theService, "image/url");
            string sServiceThumbTitle = GetNodeValue(ref theService, "image/title");

            XmlNodeList theItems= ((XmlElement)theService).GetElementsByTagName("item");
            IEnumerator itemsIter = theItems.GetEnumerator();
            while (itemsIter.MoveNext())
            {
                XmlNode theItem = (XmlNode)(itemsIter.Current);
                string sItemUniqueID = GetNodeValue(ref theItem, "articleid");
                string sItemTitle = GetNodeValue(ref theItem, "title");
                string sItemPubDate = GetNodeValue(ref theItem, "pubDate");
                string sItemDescription = GetNodeValue(ref theItem, "description");
                string sItemAuthor = GetNodeValue(ref theItem, "author");
                string sItemAction = GetNodeParameterVal(ref theItem, "status" , "name");
                XmlElement theParagraphs = (XmlElement)(((XmlElement)theItem).GetElementsByTagName("paragraphs")[0]);
                XmlNodeList theParagraphsList = theParagraphs.GetElementsByTagName("paragraph");
                IEnumerator paragraphsIter = theParagraphsList.GetEnumerator();
                while (paragraphsIter.MoveNext())
                {
                    XmlNode theParagraph = (XmlNode)(paragraphsIter.Current);
                    string sParagraphTitle = GetNodeParameterVal(ref theParagraph, ".", "title");
                    string sParagraphContent = GetNodeParameterVal(ref theParagraph, ".", "paragraphContent");
                    string sParagraphSmallThumb = GetNodeParameterVal(ref theParagraph, ".", "thumbPicpath");
                    string sParagraphVideoURL_clean = GetNodeParameterVal(ref theParagraph, ".", "videoURL_clean");
                    string sParagraphVideoURL_segmented = GetNodeParameterVal(ref theParagraph, ".", "videoURL_segmented");
                    string sParagraphVideoStillImageURL = GetNodeParameterVal(ref theParagraph, ".", "videoStillImageURL");
                    string sParagraphVideoShowAdvertisiment = GetNodeParameterVal(ref theParagraph, ".", "videoShowAdvertisiment");
                    if (sParagraphVideoURL_clean.Trim() == "")
                        continue;
                    string sClipMediaID = "";
                    Uri u = new Uri(sParagraphVideoURL_clean);
                    string[] splitArray = { "&","?" };
                    string[] splited = u.Query.Split(splitArray , StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < splited.Length; i++)
                    {
                        if (splited[i].ToLower().StartsWith("clipmediaid=") == true)
                        {
                            sClipMediaID = splited[i].Substring(12);
                        }
                        if (splited[i].ToLower().StartsWith("ar=") == true)
                        {
                            sClipMediaID = splited[i].Substring(3);
                        }
                    }

                    //Parse the segmented url
                    
                    //Here the media should be built
                    if (sItemAction == "deleted")
                        sItemAction = "delete";
                    else
                        sItemAction = "update";

                    if (sServiceLanguage == "he")
                        sServiceLanguage = "heb";
                    string sName = sItemTitle;
                    if (sParagraphTitle != "")
                        sName += " - " + sParagraphTitle;
                    sXML += "<media co_guid=\"" + ProtocolsFuncs.XMLEncode(sServiceID + "_" + sItemUniqueID + "_" + sClipMediaID, true) + "\" action=\"" + sItemAction + "\">";
                    sXML += "<basic>";
 				    sXML += "<name>";
                    sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sName, true) + "</value>";
 				    sXML += "</name>";
 				    sXML += "<description>";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sItemDescription , true) + "</value>";
 				    sXML += "</description>";
 				    sXML += "<media_type>Video</media_type>";
 				    sXML += "<rules>";
 					sXML += "<watch_per_rule>Share</watch_per_rule>";
 					sXML += "<geo_block_rule>Only On Israel</geo_block_rule>";
 				    sXML += "</rules>";
 				    sXML += "<dates>";
 					sXML += "<start>" + sItemPubDate.Replace("T" , " ").Replace("-" , "/").Replace("-" , "/").Replace("-" , "/") + "</start>";
 					sXML += "<catalog_end>01/01/2020 00:00:00</catalog_end>";
                    sXML += "<final_end>01/01/2020 00:00:00</final_end>";
 				    sXML += "</dates>";
 				    sXML += "<thumb url=\"" + ProtocolsFuncs.XMLEncode(sParagraphVideoStillImageURL , true) + "\"/>";
                    sXML += "</basic>";

                    sXML += "<structure>";
 				    sXML += "<strings>";
 					sXML += "<meta name=\"Credit\" ml_handling=\"unique\">";
 				    sXML += "<value lang=\"" + sServiceLanguage + "\">" + "" + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Content\" ml_handling=\"unique\">";
 				    sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sParagraphContent , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Clean URL\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sParagraphVideoURL_clean , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Segmented URL\" ml_handling=\"unique\">";
 				    sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sParagraphVideoURL_segmented , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Service title\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sServiceTitle , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Service link\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sServiceLink , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Service description\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sServiceDescription , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Copywrite\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sServiceCopyright , true) + "</value>";
 					sXML += "</meta>";
					sXML += "<meta name=\"Nana link\" ml_handling=\"unique\">";
 					sXML += "<value lang=\"" + sServiceLanguage + "\"></value>";
 					sXML += "</meta>";
				    sXML += "</strings>";
 				    sXML += "<booleans>";
					sXML += "<meta name=\"Is live\">FALSE</meta>";
                    if (sParagraphVideoShowAdvertisiment.Trim().ToLower() == "1" || sParagraphVideoShowAdvertisiment.Trim().ToLower() == "true")
					    sXML += "<meta name=\"With ads\">TRUE</meta>";
                    else
                        sXML += "<meta name=\"With ads\">FALSE</meta>";
					sXML += "<meta name=\"Segmented\">FALSE</meta>";
 				    sXML += "</booleans>";
 				    sXML += "<metas>";
 					sXML += "<meta name=\"Program/Show\" ml_handling=\"unique\">";
 					sXML += "<container>";
					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sServiceTitle , true) + "</value>";
 					sXML += "</container>";
 					sXML += "</meta>";
 					sXML += "<meta name=\"Author\" ml_handling=\"unique\">";
 					sXML += "<container>";
					sXML += "<value lang=\"" + sServiceLanguage + "\">" + ProtocolsFuncs.XMLEncode(sItemAuthor , true) + "</value>";
 					sXML += "</container>";
 					sXML += "</meta>";
 					sXML += "<meta name=\"Language\" ml_handling=\"unique\">";
 					sXML += "<container>";
					sXML += "<value lang=\"heb\">" + sServiceLanguage + "</value>";
 					sXML += "</container>";
 					sXML += "</meta>";
 				    sXML += "</metas>";
 			        sXML += "</structure>";
                    sXML += "<files>";
 				    sXML += "<file handling_type=\"CLIP\" type=\"Video\" quality=\"HIGH\" cdn_name=\"Nana10 CastUp\" cdn_code=\"" + ProtocolsFuncs.XMLEncode(sParagraphVideoURL_clean , true) + "\" break_points=\"\" overlay_points=\"\" pre_rule=\"\" post_rule=\"\" break_rule=\"\" overlay_rule=\"\" ads_enabled=\"\" pre_skip_enabled=\"\" post_skip_enabled=\"\"/>";
 				    sXML += "<file handling_type=\"IMAGE\" type=\"Video small thumb\" quality=\"HIGH\" cdn_code=\"" + ProtocolsFuncs.XMLEncode(sParagraphSmallThumb , true) + "\"/>";
 				    sXML += "<file handling_type=\"IMAGE\" type=\"Video big thumb\" quality=\"HIGH\" cdn_code=\"" + ProtocolsFuncs.XMLEncode(sParagraphVideoStillImageURL , true) + "\"/>";
 				    sXML += "<file handling_type=\"IMAGE\" type=\"Category thumb\" quality=\"HIGH\" cdn_code=\"" + ProtocolsFuncs.XMLEncode(sServiceThumb , true) + "\"/>";
 			        sXML += "</files>";
                    sXML += "</media>";
                }
            }
        }
        sXML += "</export></feed>";
        string sNotXML = "";
        TvinciImporter.ImporterImpl.DoTheWorkInner(sXML , 86 , "" , ref sNotXML);
        return sXML;
    }
     * */
    /*
    protected void ClearPicsDirectory(Int32 nGroupID, Int32 nWidth, Int32 nHight, bool bCrop)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from pics where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                if (nExtractPos > 0)
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                string sPicBaseName = sUploadedFile.Substring(0, nExtractPos);

                string sEndName = nWidth.ToString() + "X" + nHight.ToString();
                string sTmpImage1 = sBasePath + "/pics/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                string sBasePic = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                try
                {
                    if (System.IO.File.Exists(sTmpImage1) == false && System.IO.File.Exists(sBasePic) == true)
                    {
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, nWidth, nHight, bCrop);
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK<br/>");
                    }
                    else
                        Response.Write(sUploadedFile + " uploaded exists<br/>");

                }
                catch (Exception ex)
                {
                    Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                }
                Response.Flush();
                System.Threading.Thread.Sleep(2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }
    */

    protected bool DoesRemotePicExists(string sURL)
    {
        Int32 nStatus = 0;
        string s = Notifier.SendGetHttpReq(sURL, ref nStatus);
        if (nStatus == 200 && s.IndexOf("404") == -1)
            return true;
        return false;
    }

    protected void SynchronizePics(Int32 nGroupID)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.* from pics p where p.status=1 and create_date>'2010/09/20' and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }

                string sTnPic = sBasePath + "\\pics\\" + sPicBaseName + "_" + "tn" + sUploadedFileExt;
                string sTnRemotePic = sPicsBasePath + "/" + sPicBaseName + "_" + "tn" + sUploadedFileExt;
                string sBasePic = sBasePath + "\\pic\\" + sPicBaseName + "_full" + sUploadedFileExt;
                string sBaseRemotePic = sPicsBasePath + "/" + sPicBaseName + "_" + "full" + sUploadedFileExt;

                if (System.IO.File.Exists(sTnPic) == false)
                {

                    Response.Write(sTnPic + " needs to be created and uploaded)<br/>");
                }
                else if (DoesRemotePicExists(sTnRemotePic) == false)
                {
                    ////TVinciShared.DBManipulator.UploadPicToGroup(sTnPic, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                    Response.Write(sTnPic + " uploaded OK (was on tvinci not on server)<br/>");
                }
                else
                {
                    Response.Write(sTnPic + " uploaded was not needed<br/>");
                }

                if (System.IO.File.Exists(sBasePic) == false)
                {
                    Response.Write(sBasePic + " needs to be created and uploaded)<br/>");
                }
                else if (DoesRemotePicExists(sBaseRemotePic) == false)
                {
                    ////TVinciShared.DBManipulator.UploadPicToGroup(sBasePic, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                    Response.Write(sTnPic + " uploaded OK (was on tvinci not on server)<br/>");
                }
                else
                {
                    Response.Write(sBasePic + " uploaded was not needed<br/>");
                }

                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select * from media_pics_sizes where status=1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                    for (int nI = 0; nI < nCount1; nI++)
                    {
                        string sWidth = selectQuery1.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                        string sHeight = selectQuery1.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();
                        string sEndName = sWidth + "X" + sHeight;
                        string sTmpImage1 = sBasePath + "\\pics\\" + sPicBaseName + "_" + sEndName + sUploadedFileExt;

                        string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;

                        ////TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        try
                        {
                            if (System.IO.File.Exists(sTmpImage1) == false)
                            {
                                Response.Write(sUploadedFile + " needs to be created and uploaded)<br/>");
                                TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), true);
                                ////TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                                Response.Write(sTmpImage1 + " created and uploaded)<br/>");
                            }
                            else if (DoesRemotePicExists(sPicRemoteURL) == false)
                            {
                                ////TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                                Response.Write(sTmpImage1 + " uploaded OK (was on tvinci not on server)<br/>");
                            }
                            else
                            {
                                Response.Write(sUploadedFile + " uploaded was not needed<br/>");
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                        }
                    }
                }
                selectQuery1.Finish();
                selectQuery1 = null;
                Response.Flush();
                System.Threading.Thread.Sleep(2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }

    protected void SeperateMediaTexts()
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,group_id from media (nolock) where status=1 and id not in (select distinct media_id from media_values (nolock) where MEDIA_TEXT_TYPE_ID=7) order by group_id desc,id desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nMediaID = 0;
            try
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {

                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    Int32 nGroupID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString());
                    //ProtocolsFuncs.SeperateMediaTexts(nMediaID);
                    ProtocolsFuncs.SeperateMediaMainTags(nMediaID);
                    ProtocolsFuncs.SeperateMediaTranslateTags(nMediaID);
                    Response.Write("Media: " + nMediaID.ToString() + " (" + nGroupID.ToString() + " - " + (i + 1).ToString() + "/" + nCount.ToString() + ") Finished <br/>");
                    Response.Flush();
                    System.Threading.Thread.Sleep(30);
                    //if (i == 3)
                    //break;
                }
            }
            catch (Exception ex)
            {
                Response.Write("Exception on Media: " + nMediaID.ToString() + ex.StackTrace);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        //
    }

    private string GetPicNameByDescription(string desc)
    {
        string retVal = string.Empty;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select base_url from pics where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", desc);
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                retVal = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return retVal;
    }

    protected void SetPicToNewSize(Int32 nGroupID, bool bCrop, string sWidth, string sHeight, string directory)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);

        string sBasePath = Server.MapPath("");
        string[] pics = Directory.GetFiles(directory);
        if (pics != null && pics.Length > 0)
        {
            foreach (string pic in pics)
            {
                if (!pic.ToLower().Contains("thumbs.db") && !pic.ToLower().Contains("full"))
                {

                    FileInfo fi = new FileInfo(pic);
                    string fileName = GetPicNameByDescription(fi.Name);
                    string sUploadedFileExt = "";
                    int nExtractPos = fileName.LastIndexOf(".");
                    string sPicBaseName = "";
                    if (nExtractPos > 0)
                    {
                        sUploadedFileExt = fileName.Substring(nExtractPos);
                        sPicBaseName = fileName.Substring(0, nExtractPos);

                        string sEndName = sWidth + "X" + sHeight.ToString();
                        //TVinciShared.ImageUtils.RenameImage(sBasePath + "/pics/tele5/" + fi.Name, sBasePath + "/pics/tele5/" + sPicBaseName + "_full" + sUploadedFileExt);
                        string sTmpImage1 = sBasePath + "\\pics\\tele5resized\\" + sPicBaseName + "_" + sEndName + sUploadedFileExt;

                        string sBasePic = sBasePath + "\\pics\\tele5\\" + fi.Name;
                        string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;

                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), bCrop);

                    }
                }
            }
        }
    }

    protected void SetAllPicSize(Int32 nGroupID, bool bCrop, string width, string height)
    {
        object locker = new object();
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //selectQuery += "select p.* from pics p,media m where m.MEDIA_PIC_ID=p.id and m.status=1 and p.status=1 and ";
        selectQuery += "select p.* from pics p where p.status=1 and ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }
                string sEndName = width + "X" + height.ToString();
                string sTmpImage1 = sBasePath + "\\pics\\NoveNetGemPics\\" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                string sBasePic = sBasePath + "\\pics\\" + sPicBaseName + "_full" + sUploadedFileExt;
                string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                try
                {

                    if (System.IO.File.Exists(sBasePic) == true && sEndName != "tn")
                    {
                        if (!DoesRemotePicExists(sPicRemoteURL))
                        {
                            TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, int.Parse(width), int.Parse(height), bCrop);
                            Response.Write("Pic " + sTmpImage1 + " Resized");
                        }
                        else
                        {
                            Response.Write("Pic " + sTmpImage1 + " Already exists");
                        }
                    }
                    else
                    {
                        Response.Write(sUploadedFile + " full pic does not exist<br/>");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                    log.Error("Pic Resize - Exception " + sTmpImage1 + " not uploaded " + ex.Message, ex);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void SetNewPicSize(Int32 nGroupID, Int32 nWidth, Int32 nHight, bool bCrop)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //selectQuery += "select p.* from pics p,media m where m.MEDIA_PIC_ID=p.id and m.status=1 and p.status=1 and ";
        selectQuery += "select p.* from pics p where p.status=1 and id=73541 and ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }

                string sEndName = nWidth.ToString() + "X" + nHight.ToString();
                if (nWidth == 0)
                    sEndName = "tn";
                string sTmpImage1 = sBasePath + "/pics/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                string sBasePic = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                try
                {
                    if (System.IO.File.Exists(sTmpImage1) == false && System.IO.File.Exists(sBasePic) == true && sEndName != "tn")
                    {
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, nWidth, nHight, bCrop);
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK<br/>");
                    }
                    else if (System.IO.File.Exists(sTmpImage1) == true && DoesRemotePicExists(sPicRemoteURL) == false)
                    {
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK (was on tvinci not on server)<br/>");
                    }
                    else
                    {
                        Response.Write(sUploadedFile + " uploaded was not needed<br/>");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                }
                Response.Flush();
                System.Threading.Thread.Sleep(2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }

    protected void SetNewPicSize1(Int32 nGroupID, Int32 nWidth, Int32 nHight, bool bCrop)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.* from pics p,media_files m where m.REF_ID=p.id and m.status=1 and p.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }

                string sEndName = nWidth.ToString() + "X" + nHight.ToString();
                string sTmpImage1 = sBasePath + "/pics/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                string sBasePic = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                try
                {
                    if (System.IO.File.Exists(sTmpImage1) == false && System.IO.File.Exists(sBasePic) == true)
                    {
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, nWidth, nHight, bCrop);
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK<br/>");
                    }
                    else if (System.IO.File.Exists(sTmpImage1) == true && DoesRemotePicExists(sPicRemoteURL) == false)
                    {
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK (was on tvinci not on server)<br/>");
                    }
                    else
                    {
                        Response.Write(sUploadedFile + " uploaded was not needed<br/>");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                }
                Response.Flush();
                System.Threading.Thread.Sleep(2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }

    protected void SetNewPicSize2(Int32 nGroupID, Int32 nWidth, Int32 nHight, bool bCrop)
    {
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.* from pics p,media m where m.MEDIA_PIC_ID=p.id and m.status=1 and p.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }

                string sEndName = nWidth.ToString() + "X" + nHight.ToString();
                string sTmpImage1 = sBasePath + "/pics/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                string sBasePic = sBasePath + "/pics/" + sPicBaseName + "_full" + sUploadedFileExt;
                string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                try
                {
                    if (System.IO.File.Exists(sTmpImage1) == false && System.IO.File.Exists(sBasePic) == true)
                    {
                        TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, nWidth, nHight, bCrop);
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK<br/>");
                    }
                    else if (System.IO.File.Exists(sTmpImage1) == true && DoesRemotePicExists(sPicRemoteURL) == false)
                    {
                        //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                        Response.Write(sUploadedFile + " uploaded OK (was on tvinci not on server)<br/>");
                    }
                    else
                    {
                        Response.Write(sUploadedFile + " uploaded was not needed<br/>");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                }
                Response.Flush();
                System.Threading.Thread.Sleep(2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }

    protected Int32 GetIPToCountryID(string sFrom, string sTo, Int32 nCountryID, string sFromIP, string sToIP)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from ip_to_country_tmp where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "=", Int64.Parse(sFrom));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", "=", Int64.Parse(sTo));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FROM_IP", "=", sFromIP);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TO_IP", "=", sToIP);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected void IpToCountry()
    {
        string sFileName = "C:/temp/GeoIPCountryWhois.csv";
        string[] fields;
        CSVReader reader = new CSVReader(sFileName, System.Text.Encoding.UTF8);
        Int32 nCounter = 0;
        Int32 nFlushCounter = 0;
        while ((fields = reader.GetCSVLine()) != null)
        {
            try
            {
                string sFrom = fields[2];
                string sTo = fields[3];
                string sCountry2 = fields[4];
                string sCountryFull = fields[5];
                string sFromIP = fields[0];
                string sToIP = fields[1];
                Int32 nCountryID = GetCountryID(sCountry2, sCountryFull);
                Int32 nID = GetIPToCountryID(sFrom, sTo, nCountryID, sFromIP, sToIP);
                if (nID == 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ip_to_country_tmp");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "=", Int64.Parse(sFrom));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", "=", Int64.Parse(sTo));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FROM_IP", "=", sFromIP);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TO_IP", "=", sToIP);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
                nCounter++;
                nFlushCounter++;
                if (nFlushCounter == 100)
                {
                    nFlushCounter = 0;
                    Response.Write("Inserted: " + nCounter.ToString() + "<br/>");
                    Response.Flush();
                }
            }
            catch
            {
                continue;
            }
        }
        reader.Dispose();
    }



    protected Int32 GetCountryID(string sCountryCD2, string sFullName)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from countries where  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(COUNTRY_CD2)))", "=", sCountryCD2.Trim().ToLower());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(COUNTRY_NAME)))", "=", sFullName.Trim().ToLower());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nRet == 0)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("countries");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CD2", "=", sCountryCD2);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_NAME", "=", sFullName);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return GetCountryID(sCountryCD2, sFullName);
        }
        return nRet;
    }

}
