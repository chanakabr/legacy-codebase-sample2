using KLogMonitor;
using RabbitMQ.Client.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using TVinciShared;

public partial class adm_subscription_product_codes : System.Web.UI.Page
{  
    private static readonly KLogger log = new KLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_multi_pricing_plans.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
      

        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int subscriptionId = 0;
                string xml = GetPageData(out subscriptionId);
                bool result = false;
                if (!string.IsNullOrEmpty(xml))
                {
                    result = SetSubscriptionProducts(xml, subscriptionId);
                }

                if (result)
                {
                    EndOfAction();
                    Session["subscription_id"] = 0;
                    return;
                }
                else
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                    Session["subscription_id"] = subscriptionId;
                    return;
                }                
            }
        }

        if (string.IsNullOrEmpty(Request.QueryString["subscription_id"]))
        {
            log.Debug("Session key - subscription_id_removes");
            Session["subscription_id"] = null;
        }
        else
        {
            log.Debug("Session key - subscription_id not null " + Request.QueryString["subscription_id"]);
        }

        if (Request.QueryString["subscription_id"] != null && Request.QueryString["subscription_id"].ToString() != "")
        {
            Session["subscription_id"] = int.Parse(Request.QueryString["subscription_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
            if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return;
        }
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

    private bool SetSubscriptionProducts(string xml, int subscriptionId)
    {
        bool res = false;
        try
        {         
            res = DAL.PricingDAL.Update_SubscriptionsProductCodes(LoginManager.GetLoginGroupID() ,subscriptionId, xml);
        }
        catch (Exception ex)
        {

            return false;
        }
        return res;
    }

    private string GetPageData(out int subscriptionId)
    {
        subscriptionId = 0;
        NameValueCollection nvc = Request.Form;
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(rootNode);

        XmlNode rowNode;
        XmlNode productCodeNode;
        XmlNode verificationPaymentGatewayIdNode;
     
        rowNode = xmlDoc.CreateElement("row");     
        
        verificationPaymentGatewayIdNode = xmlDoc.CreateElement("verification_payment_gateway_id");
        if (!string.IsNullOrEmpty(nvc["1_val"]))
        {
            verificationPaymentGatewayIdNode.InnerText = nvc["1_val"];
        }
        rowNode.AppendChild(verificationPaymentGatewayIdNode);

        if (!string.IsNullOrEmpty(nvc["2_val"]))
        {
            int.TryParse(nvc["2_val"].ToString(), out subscriptionId);
        }
             
        productCodeNode = xmlDoc.CreateElement("product_code");
        if (!string.IsNullOrEmpty(nvc["3_val"]))
        {
            productCodeNode.InnerText = nvc["3_val"];
        }
        rowNode.AppendChild(productCodeNode);

        rootNode.AppendChild(rowNode);

        rowNode = xmlDoc.CreateElement("row");       

        verificationPaymentGatewayIdNode = xmlDoc.CreateElement("verification_payment_gateway_id");
        if (!string.IsNullOrEmpty(nvc["5_val"]))
        {
            verificationPaymentGatewayIdNode.InnerText = nvc["5_val"];
        }
        rowNode.AppendChild(verificationPaymentGatewayIdNode);
     
        productCodeNode = xmlDoc.CreateElement("product_code");
        if (!string.IsNullOrEmpty(nvc["7_val"]))
        {
            productCodeNode.InnerText = nvc["7_val"];
        }
        rowNode.AppendChild(productCodeNode);
        rootNode.AppendChild(rowNode);


        rowNode = xmlDoc.CreateElement("row");

        verificationPaymentGatewayIdNode = xmlDoc.CreateElement("verification_payment_gateway_id");
        if (!string.IsNullOrEmpty(nvc["9_val"]))
        {
            verificationPaymentGatewayIdNode.InnerText = nvc["9_val"];
        }
        rowNode.AppendChild(verificationPaymentGatewayIdNode);

        productCodeNode = xmlDoc.CreateElement("product_code");
        if (!string.IsNullOrEmpty(nvc["11_val"]))
        {
            productCodeNode.InnerText = nvc["11_val"];
        }
        rowNode.AppendChild(productCodeNode);
        rootNode.AppendChild(rowNode);


        return xmlDoc.InnerXml;
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Subscription Product Codes");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }



    protected void AddFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "SELECT spc.ID, spc.GROUP_ID, spc.SUBSCRIPTION_ID, spc.PRODUCT_CODE, spc.verification_payment_gateway_id, spc.IS_ACTIVE, spc.STATUS,  vpg.DESCRIPTION,  vpg.ID as verification_pg_id  ";
        selectQuery += " FROM Billing.dbo.verification_payment_gateway vpg   left  join  Pricing.dbo.subscriptions_product_codes spc    on	vpg.id = spc.verification_payment_gateway_id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("spc.group_id", "=", LoginManager.GetLoginGroupID());
        
        int subscriptionId = 0;
        if (Session["subscription_id"] != null && !string.IsNullOrEmpty(Session["subscription_id"].ToString()) && int.TryParse(Session["subscription_id"].ToString(), out subscriptionId) && subscriptionId>0)
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("spc.SUBSCRIPTION_ID", "=", subscriptionId);
        }

        selectQuery.SetConnectionKey("pricing_connection");

        if (selectQuery.Execute("query", true) != null)
        {
            DataTable dt = selectQuery.Table("query");
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in  dt.Rows)
                {                    
                    DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", false, 60, 3);
                    dr_name.Initialize("Verification Payment Gateway", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
                    dr_name.SetValue(ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION"));
                    theRecord.AddRecord(dr_name);

                    DataRecordShortIntField dr_verification_payment_gateway_id = new DataRecordShortIntField(false, 9, 9);
                    dr_verification_payment_gateway_id.Initialize("verification_payment_gateway_id", "adm_table_header_nbg", "FormInput", "verification_pg_id", false);
                    dr_verification_payment_gateway_id.SetDefault(ODBCWrapper.Utils.GetIntSafeVal(dr, "verification_pg_id"));
                    theRecord.AddRecord(dr_verification_payment_gateway_id);

                    DataRecordShortIntField dr_subscription_id = new DataRecordShortIntField(false, 9, 9);
                    dr_subscription_id.Initialize("Subscription", "adm_table_header_nbg", "FormInput", "SUBSCRIPTION_ID", false);
                    int.TryParse(Session["subscription_id"].ToString(), out subscriptionId);
                    dr_subscription_id.SetDefault(subscriptionId);
                    theRecord.AddRecord(dr_subscription_id);

                    DataRecordLongTextField dr_product_code = new DataRecordLongTextField("ltr", true, 60, 3);
                    dr_product_code.Initialize("Product Code", "adm_table_header_nbg", "FormInput", "PRODUCT_CODE", false);
                    dr_product_code.SetValue(ODBCWrapper.Utils.GetSafeStr(dr, "PRODUCT_CODE"));
                    theRecord.AddRecord(dr_product_code);
                }
            }

        }
        selectQuery.Finish();
        selectQuery = null;
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
                
        bool isParentGroup = IsParentGroup(ODBCWrapper.Utils.GetIntSafeVal(groupId));

        string sTable = string.Empty;
        if (!isParentGroup)
        {
            sTable = (PageUtils.GetPreHeader() + ": Module is not implemented");
        }
        else
        {
            string sBack = "adm_multi_pricing_plans.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("subscriptions_product_codes", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("pricing_connection");

            AddFields(ref theRecord);

            sTable = theRecord.GetTableHTML("adm_subscription_product_codes.aspx?submited=1");
        }
        return sTable;
    }

   

    private System.Data.DataTable GetBaseDT()
    {
        System.Data.DataTable dT = new System.Data.DataTable();
        Int32 n = 0;
        string s = "";
        dT.Columns.Add(PageUtils.GetColumn("ID", n));
        dT.Columns.Add(PageUtils.GetColumn("txt", s));
        return dT.Copy();
    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select PARENT_GROUP_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            res = false;
        }
        return res;
    }

    //private int GetProductCodeID(int groupId)
    //{
    //    int productCodeId = 0;
    //    try
    //    {
    //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
    //        selectQuery.SetConnectionKey("pricing_connection");

    //        selectQuery += "SELECT spc.ID, spc.GROUP_ID, spc.SUBSCRIPTION_ID, spc.PRODUCT_CODE, spc.verification_payment_gateway_id, spc.IS_ACTIVE, spc.STATUS,  vpg.DESCRIPTION, vpg.ID as verification_pg_id ";
    //        selectQuery += " FROM Billing.dbo.verification_payment_gateway vpg   left  join  Pricing.dbo.subscriptions_product_codes spc   on	vpg.id = spc.verification_payment_gateway_id and ";
    //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
    //        if (selectQuery.Execute("query", true) != null)
    //        {
    //            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
    //            if (nCount > 0)
    //            {
    //                productCodeId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
    //            }
    //        }
    //        selectQuery.Finish();
    //        selectQuery = null;
    //    }
    //    catch (Exception ex)
    //    {
    //        log.Error("", ex);
    //        productCodeId = 0;
    //    }
    //    return productCodeId;
    //}


    //protected void Page_Load(object sender, EventArgs e)
    //{
    //    if (LoginManager.CheckLogin() == false)
    //        Response.Redirect("login.html");
    //    if (LoginManager.IsPagePermitted("adm_multi_pricing_plans.aspx") == false)
    //        LoginManager.LogoutFromSite("login.html");
    //    if (AMS.Web.RemoteScripting.InvokeMethod(this))
    //        return;

    //    if (string.IsNullOrEmpty(Request.QueryString["subscription_id"]))
    //    {
    //        log.Debug("Session key - subscription_id_removes");
    //        Session["subscription_id"] = null;
    //    }
    //    else
    //    {
    //        log.Debug("Session key - subscription_id not null " + Request.QueryString["subscription_id"]);
    //    }
    //    if (!IsPostBack)
    //    {
    //        Int32 nMenuID = 0;
    //        m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_multi_pricing_plans.aspx");
    //        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
    //        //if (Request.QueryString["search_save"] != null)
    //        //Session["search_save"] = "1";
    //        //else
    //        //Session["search_save"] = null;

    //        if (Request.QueryString["subscription_id"] != null &&
    //            Request.QueryString["subscription_id"].ToString() != "")
    //        {
    //            Session["subscription_id"] = int.Parse(Request.QueryString["subscription_id"].ToString());
    //            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
    //            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
    //            if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
    //            {
    //                LoginManager.LogoutFromSite("login.html");
    //                return;
    //            }
    //        }
    //        else if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
    //        {
    //            LoginManager.LogoutFromSite("index.html");
    //            return;
    //        }
    //    }
    //}

    //public void GetHeader()
    //{
    //    Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("subscriptions", "NAME", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString() + " Product Codes ");
    //}

    //protected void GetMainMenu()
    //{
    //    Response.Write(m_sMenu);
    //}

    //protected void GetSubMenu()
    //{
    //    Response.Write(m_sSubMenu);
    //}

    //public string GetTableCSV()
    //{
    //    string sOldOrderBy = "";
    //    if (Session["order_by"] != null)
    //        sOldOrderBy = Session["order_by"].ToString();
    //    DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
    //    FillTheTableEditor(ref theTable, sOldOrderBy);

    //    string sCSVFile = theTable.OpenCSV();
    //    theTable.Finish();
    //    theTable = null;
    //    return sCSVFile;
    //}

    //protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    //{
    //    Int32 nGroupID = LoginManager.GetLoginGroupID();
    //    theTable.SetConnectionKey("pricing_connection");
    //    theTable += "SELECT spc.ID, spc.GROUP_ID, spc.SUBSCRIPTION_ID, spc.PRODUCT_CODE, spc.verification_payment_gateway_id, spc.IS_ACTIVE, spc.STATUS,  vpg.DESCRIPTION, vpg.ID as verification_pg_id";
    //    theTable += "  FROM Billing.dbo.verification_payment_gateway vpg   left  join  Pricing.dbo.subscriptions_product_codes spc ";
    //    theTable += "  on	vpg.id = spc.verification_payment_gateway_id and ";
    //    theTable += ODBCWrapper.Parameter.NEW_PARAM("spc.group_id", "=", nGroupID);
        
        

    //    if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&  LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
    //        theTable.AddActivationField("product_code");
               
    //    theTable.AddHiddenField("is_active");
    //    theTable.AddHiddenField("status");
    //    theTable.AddActivationField("product_code", "adm_subscription_product_codes.aspx");
    //    theTable.AddOnOffField("Ads Enabled", "product_code~~|~~ADS_ENABLED~~|~~id~~|~~Yes~~|~~No");
      
    //    //theTable.AddTechDetails("media_files");
    //    //theTable.AddEditorRemarks("media_files");
    //   // theTable.AddHiddenField("EDITOR_REMARKS");

    //    if (LoginManager.IsActionPermittedOnPage("adm_multi_pricing_plans.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
    //    {
    //        DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_product_codes_new.aspx", "Edit", "");
    //        linkColumn1.AddQueryStringValue("subscription_product_code_id", "field=id");
    //        linkColumn1.AddQueryStringValue("subscription_id", "field=SUBSCRIPTION_ID");
    //        linkColumn1.AddQueryStringValue("verification_payment_gateway_id", "field=verification_payment_gateway_id");
    //        theTable.AddLinkColumn(linkColumn1);
    //    }
    //    if (LoginManager.IsActionPermittedOnPage("adm_multi_pricing_plans.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
    //    {
    //        DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
    //        linkColumn.AddQueryStringValue("id", "field=id");
    //        linkColumn.AddQueryStringValue("table", "subscriptions_product_codes");
    //        linkColumn.AddQueryStringValue("db", "pricing_connection");
    //        linkColumn.AddQueryStringValue("confirm", "true");
    //        linkColumn.AddQueryStringValue("main_menu", "7");
    //        linkColumn.AddQueryStringValue("sub_menu", "1");
    //        linkColumn.AddQueryStringValue("rep_field", "NAME");
    //        linkColumn.AddQueryStringValue("rep_name", "ùí");
    //        theTable.AddLinkColumn(linkColumn);
    //    }

    //    if (LoginManager.IsActionPermittedOnPage("adm_multi_pricing_plans.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
    //    {
    //        DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
    //        linkColumn.AddQueryStringValue("id", "field=id");
    //        linkColumn.AddQueryStringValue("table", "subscriptions_product_codes");
    //        linkColumn.AddQueryStringValue("db", "pricing_connection");
    //        linkColumn.AddQueryStringValue("confirm", "true");
    //        linkColumn.AddQueryStringValue("main_menu", "7");
    //        linkColumn.AddQueryStringValue("sub_menu", "1");
    //        linkColumn.AddQueryStringValue("rep_field", "NAME");
    //        linkColumn.AddQueryStringValue("rep_name", "ùí");
    //        theTable.AddLinkColumn(linkColumn);
    //    }

    //    if (LoginManager.IsActionPermittedOnPage("adm_multi_pricing_plans.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
    //    {
    //        DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
    //        linkColumn.AddQueryStringValue("id", "field=id");
    //        linkColumn.AddQueryStringValue("table", "subscriptions_product_codes");
    //        linkColumn.AddQueryStringValue("db", "pricing_connection");
    //        linkColumn.AddQueryStringValue("confirm", "false");
    //        linkColumn.AddQueryStringValue("main_menu", "7");
    //        linkColumn.AddQueryStringValue("sub_menu", "1");
    //        linkColumn.AddQueryStringValue("rep_field", "NAME");
    //        linkColumn.AddQueryStringValue("rep_name", "ùí");
    //        theTable.AddLinkColumn(linkColumn);
    //    }
    //}

    //public string GetPageContent(string sOrderBy, string sPageNum)
    //{
    //    string sOldOrderBy = "";
    //    if (Session["order_by"] != null)
    //        sOldOrderBy = Session["order_by"].ToString();

    //    DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
    //    FillTheTableEditor(ref theTable, sOrderBy);

    //    string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
    //    Session["ContentPage"] = "adm_multi_pricing_plans.aspx";
    //    Session["LastContentPage"] = "adm_multi_pricing_plans.aspx?search_save=1";
    //    Session["order_by"] = sOldOrderBy;
    //    theTable.Finish();
    //    theTable = null;
    //    return sTable;
    //}

    //public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    //{
    //    Int32 nGroupID = LoginManager.GetLoginGroupID();        
    //    int mediaFileID;
    //    if (int.TryParse(sID, out mediaFileID))
    //    {
    //        Int32 nMediaID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", mediaFileID).ToString());
    //        if (nMediaID > 0)
    //        {
    //            if (!ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nGroupID, ApiObjects.eAction.Update))
    //            {
    //                log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", nMediaID, nGroupID));
    //            }
    //        }
    //    }
    //}
}