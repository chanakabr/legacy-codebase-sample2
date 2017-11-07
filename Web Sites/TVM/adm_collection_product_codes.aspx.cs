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

public partial class adm_collection_product_codes : System.Web.UI.Page
{  
    private static readonly KLogger log = new KLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_collections.aspx") == false)
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
                int collectionId = 0;
                string xml = GetPageData(out collectionId);
                bool result = false;
                if (!string.IsNullOrEmpty(xml))
                {
                    result = SetCollectionProducts(xml, collectionId);
                }

                if (result)
                {
                    EndOfAction();
                    Session["collection_id"] = 0;
                    return;
                }
                else
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                    Session["collection_id"] = collectionId;
                    return;
                }                
            }
        }

        if (string.IsNullOrEmpty(Request.QueryString["collection_id"]))
        {
            log.Debug("Session key - collection_id_removes");
            Session["collection_id"] = null;
        }
        else
        {
            log.Debug("Session key - collection_id not null " + Request.QueryString["collection_id"]);
        }

        if (Request.QueryString["collection_id"] != null && Request.QueryString["collection_id"].ToString() != "")
        {
            Session["collection_id"] = int.Parse(Request.QueryString["collection_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("collections", "group_id", int.Parse(Session["collection_id"].ToString()), "pricing_connection").ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
            if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else if (Session["collection_id"] == null || Session["collection_id"].ToString() == "" || Session["collection_id"].ToString() == "0")
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

    private bool SetCollectionProducts(string xml, int collectionId)
    {
        bool res = false;
        try
        {
            res = DAL.PricingDAL.Update_ExternalProductCodes(LoginManager.GetLoginGroupID(), collectionId, ApiObjects.eTransactionType.Collection, xml);
        }
        catch (Exception ex)
        {

            return false;
        }
        return res;
    }

    private string GetPageData(out int collectionId)
    {
        collectionId = 0;
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
            int.TryParse(nvc["2_val"].ToString(), out collectionId);
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
        Response.Write(PageUtils.GetPreHeader() + ": Collection Product Codes");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }



    protected void AddFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "SELECT pc.ID, pc.GROUP_ID, pc.PRODUCT_ID, pc.PRODUCT_CODE, pc.verification_payment_gateway_id, pc.IS_ACTIVE, pc.STATUS,  vpg.DESCRIPTION,  vpg.ID as verification_pg_id  ";
        selectQuery += " FROM Billing.dbo.verification_payment_gateway vpg left  join  Pricing.dbo.products_codes pc on vpg.id = pc.verification_payment_gateway_id and pc.product_type = 2 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pc.group_id", "=", LoginManager.GetLoginGroupID());

        int collectionId = 0;
        if (Session["collection_id"] != null && !string.IsNullOrEmpty(Session["collection_id"].ToString()) && int.TryParse(Session["collection_id"].ToString(), out collectionId) && collectionId > 0)
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pc.PRODUCT_ID", "=", collectionId);
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

                    DataRecordShortIntField dr_collection_id = new DataRecordShortIntField(false, 9, 9);
                    dr_collection_id.Initialize("Collection", "adm_table_header_nbg", "FormInput", "COLLECTION_ID", false);
                    int.TryParse(Session["collection_id"].ToString(), out collectionId);
                    dr_collection_id.SetDefault(collectionId);
                    theRecord.AddRecord(dr_collection_id);

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
            string sBack = "adm_collections.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("collections_product_codes", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("pricing_connection");

            AddFields(ref theRecord);

            sTable = theRecord.GetTableHTML("adm_collection_product_codes.aspx?submited=1");
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
}