using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using TvinciImporter;
using TVinciShared;
using KLogMonitor;
using ApiObjects;
using System.Collections.Specialized;
using System.Xml;
using ApiObjects.Response;
using System.Data;

public partial class adm_interests : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    private static readonly string sep = "|";
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        Int32 nMenuID = 0;

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_engagements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_engagements.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 7, true);

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int groupId = LoginManager.GetLoginGroupID();
                bool result = false;

                string xml = GetPageData();
                
                if (!string.IsNullOrEmpty(xml))
                {
                    result = SetTopicInterests(xml, groupId);
                }

                if (result == null || !result)
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                }
                else
                {
                    EndOfAction();
                }
            }

            if (Request.QueryString["type"] != null && Request.QueryString["type"].ToString() != "")
            {
                Session["type"] = int.Parse(Request.QueryString["type"].ToString());
            }

            if (Session["error_msg_s"] != null && Session["error_msg_s"].ToString() != "")
            {
                lblError.Visible = true;
                lblError.Text = Session["error_msg_s"].ToString();
                Session["error_msg_s"] = null;
            }
            else
            {
                lblError.Visible = false;
                lblError.Text = "";
            }
        }
    }

    private bool SetTopicInterests(string xml, int groupId)
    {
        return DAL.TvmDAL.SetTopicInterests(xml, groupId);
    }

    private string GetPageData()
    {
        int groupId = LoginManager.GetLoginGroupID();
        NameValueCollection nvc = Request.Form;

        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(rootNode);

        XmlNode rowNode;

        XmlNode idNode;
        XmlNode nameIdNode;
        XmlNode assetTypeIdNode;
        XmlNode enableNotificationIdNode;
        XmlNode parentMetaIdNode;
        XmlNode metaIdNode;
        XmlNode isTagIdNode;
        XmlNode groupIdNode;

        int i = 0;
        string sFieldName = string.Empty;
        string metaOrTagName = string.Empty;
        string metaOrTagId = string.Empty;
        string[] tempFieldName;

        while (i < nvc.Count)
        {
            rowNode = xmlDoc.CreateElement("row");
            groupIdNode = xmlDoc.CreateElement("group_id");
            if (!string.IsNullOrEmpty(groupId.ToString()))
            {
                groupIdNode.InnerText = groupId.ToString();
            }
            enableNotificationIdNode = xmlDoc.CreateElement("enable_notification");
            nameIdNode = xmlDoc.CreateElement("name");
            assetTypeIdNode = xmlDoc.CreateElement("asset_type");
            idNode = xmlDoc.CreateElement("id");
            parentMetaIdNode = xmlDoc.CreateElement("parent_meta_id");
            metaIdNode = xmlDoc.CreateElement("meta_id");
            isTagIdNode = xmlDoc.CreateElement("is_tag");
            bool insertData = false;
            for (int j = i; j <= i + 5; j++)
            {
                if (nvc[j.ToString() + "_fieldName"] != null)
                {
                    tempFieldName = nvc[j.ToString() + "_fieldName"].Split(sep.ToArray());
                    
                    if (tempFieldName != null && tempFieldName.Count() > 2)
                    {                        
                        metaOrTagName = tempFieldName[0];
                        metaOrTagId = tempFieldName[1];
                        sFieldName = tempFieldName[2];
                    }

                    string sVal = "";
                    if (nvc[j.ToString() + "_val"] != null)
                    {
                        sVal = nvc[j.ToString() + "_val"].ToString();
                    }

                    switch (sFieldName)
                    {
                        case "user_interest":
                            if (sVal == "on") insertData = true; ;
                            break;
                        case "id":
                             if (!string.IsNullOrEmpty(sVal))
                             {
                                 idNode.InnerText = sVal;
                             }
                            break;
                        case "asset_type":                            
                            if (!string.IsNullOrEmpty(sVal))
                            {
                                assetTypeIdNode.InnerText = sVal;
                            }                           
                            break;
                        case "enable_notification":                        
                            enableNotificationIdNode.InnerText = sVal == "on" ? "1" : "0";
                            break;
                        case "parent_meta_id":
                            parentMetaIdNode.InnerText = sVal;
                            break;                     
                        case "is_tag":
                            isTagIdNode.InnerText = sVal;
                            break;

                        default:
                            break;

                    }
                }
            }
            if (insertData)
            {
                rowNode.AppendChild(idNode);
                rowNode.AppendChild(groupIdNode);
                rowNode.AppendChild(enableNotificationIdNode);
                rowNode.AppendChild(assetTypeIdNode);

                nameIdNode.InnerText = metaOrTagName;
                rowNode.AppendChild(nameIdNode);

                rowNode.AppendChild(parentMetaIdNode);

                metaIdNode.InnerText = string.Format("{0}_{1}_{2}", groupId, assetTypeIdNode.InnerText, metaOrTagId);
                rowNode.AppendChild(metaIdNode);
                
                rowNode.AppendChild(isTagIdNode);
                rootNode.AppendChild(rowNode);
            }
            i = i + 6;
        }
        return xmlDoc.InnerXml;
    }
    
    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Interests";       
        Response.Write(sRet);
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object t = null;

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object groupId = LoginManager.GetLoginGroupID();
        string sTable = string.Empty;
        string sBack = "adm_interests.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("topic_interest", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        AddFields(ref theRecord);

        sTable = theRecord.GetTableHTML("adm_interests.aspx?submited=1");
        return sTable;
    }

    private void AddFields(ref DBRecordWebEditor theRecord)
    {
        int groupId = LoginManager.GetLoginGroupID();
        DataSet ds = DAL.TvmDAL.GetMetasTagsByGroupId(groupId);

        string name = string.Empty;
        string metaTagId = string.Empty;
        int assetType = 0;
        if (ds != null && ds.Tables != null)
        {
            foreach (DataTable dt in ds.Tables)
            {
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {   
                        name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                        metaTagId = ODBCWrapper.Utils.GetSafeStr(dr, "metaTagId");

                        DataRecordShortIntField dr_asset_type = new DataRecordShortIntField(false, 9, 9);
                        dr_asset_type.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep, "asset_type"));
                        dr_asset_type.Initialize("asset_type", "adm_table_header_nbg", "FormInput", "asset_type", false);
                        assetType = ODBCWrapper.Utils.GetIntSafeVal(dr, "asset_type");
                        dr_asset_type.SetDefault(assetType);
                        theRecord.AddRecord(dr_asset_type);

                        DataRecordShortIntField dr_id = new DataRecordShortIntField(false, 9, 9);
                        dr_id.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep, "id"));
                        dr_id.Initialize("id", "adm_table_header_nbg", "FormInput", "id", false);                        
                        dr_id.SetDefault(ODBCWrapper.Utils.GetIntSafeVal(dr, "id"));
                        theRecord.AddRecord(dr_id);

                        DataRecordShortIntField dr_is_tag = new DataRecordShortIntField(false, 9, 9);
                        dr_is_tag.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep,"is_tag"));
                        dr_is_tag.Initialize("is_tag", "adm_table_header_nbg", "FormInput", "is_tag", false);
                        dr_is_tag.SetDefault(ODBCWrapper.Utils.GetIntSafeVal(dr, "is_tag"));
                        theRecord.AddRecord(dr_is_tag);

                        DataRecordCheckBoxField dr_enable_notification = new DataRecordCheckBoxField(true);
                        dr_enable_notification.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep, "enable_notification"));
                        dr_enable_notification.Initialize(string.Format("{0}-{1}", name, "Enable Notification"), "adm_table_header_nbg", "FormInput", "enable_notification", false);
                        dr_enable_notification.SetDefault(ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_notification"));
                        theRecord.AddRecord(dr_enable_notification);

                        DataRecordCheckBoxField dr_user_interest = new DataRecordCheckBoxField(true);
                        dr_user_interest.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep, "user_interest"));
                        dr_user_interest.Initialize("User Interest", "adm_table_header_nbg", "FormInput", "user_interest", false);
                        dr_user_interest.SetDefault(ODBCWrapper.Utils.GetIntSafeVal(dr, "user_interest"));
                        theRecord.AddRecord(dr_user_interest);

                        System.Data.DataTable parentTopics = GetParentTopics(dt, assetType);

                        DataRecordDropDownField dr_parent = new DataRecordDropDownField("", "txt", "id", "", null, 60, true);
                        dr_parent.setFiledName(string.Format("{0}{1}{2}{3}{4}", name, sep, metaTagId, sep, "parent_meta_id"));
                        dr_parent.SetFieldType("string");
                        dr_parent.Initialize("Parent Topic", "adm_table_header_nbg", "FormInput", "parent_meta_id", false);
                        dr_parent.SetSelectsDT(parentTopics);
                        string defaultParentTopic = ODBCWrapper.Utils.GetSafeStr(dr, "PARENT_META_ID");
                        dr_parent.SetDefaultVal(defaultParentTopic);
                        theRecord.AddRecord(dr_parent);

                    }
                }
            }
        }
    }

    private System.Data.DataTable GetParentTopics(DataTable dt, int asset_type)
    {
        System.Data.DataTable dtP = new System.Data.DataTable();
        
        dtP.Columns.Add("txt", typeof(string));
        dtP.Columns.Add("id", typeof(string));

        int groupId = LoginManager.GetLoginGroupID();
        string name = string.Empty;
        foreach (DataRow dr in dt.Rows)
        {
            name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
            dtP.Rows.Add(name, string.Format("{0}_{1}_{2}", groupId, asset_type.ToString(), name));
        }
        return dtP;
    }

    private void EndOfAction()
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
        {
            // string sFailure = coll["failure_back_page"].ToString();
            if (coll["failure_back_page"] != null)
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
            else
                HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
        }
        else
        {
            if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
            {
                HttpContext.Current.Session["last_page_html"] = null;
                string s = HttpContext.Current.Session["back_n_next"].ToString();
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
                HttpContext.Current.Session["back_n_next"] = null;
            }
            else
            {
                if (coll["success_back_page"] != null)
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
                else
                    HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
        }
    }
}