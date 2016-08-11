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
using TvinciImporter;
using ApiObjects;
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;

public partial class adm_generic_confirm : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected Int32 m_nID;
    protected string m_sTable;
    protected bool m_bConfirm;
    protected Int32 m_nMainMenu;
    protected Int32 m_nSubMenu;
    protected string m_sBasePageURL;
    protected string m_sRepresentField;
    protected string m_sRepresentName;
    protected string m_sDB;
    protected string cacheKey;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (LoginManager.CheckLogin() == false)
            {
                Response.Redirect("login.html");
                return;
            }

            if (AMS.Web.RemoteScripting.InvokeMethod(this))
                return;
            if (Session["ContentPage"] != null)
                m_sBasePageURL = Session["ContentPage"].ToString();
            m_nSubMenu = int.Parse(Request.QueryString["sub_menu"].ToString());
            m_nMainMenu = int.Parse(Request.QueryString["main_menu"].ToString());
            if (Request.QueryString["confirm"].ToString() == "false")
                m_bConfirm = false;
            else
                m_bConfirm = true;
            m_sTable = Request.QueryString["table"].ToString();
            m_nID = int.Parse(Request.QueryString["id"].ToString());
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(m_nMainMenu, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, m_nSubMenu, true);
            m_sRepresentField = Request.QueryString["rep_field"].ToString();
            m_sRepresentName = Request.QueryString["rep_name"].ToString();
            cacheKey = Convert.ToString(Request.QueryString["cache_key"]);

            if (Request.QueryString["db"] != null)
                m_sDB = Request.QueryString["db"].ToString();
            else
                m_sDB = "";
            if (LoginManager.IsPagePermitted(m_sBasePageURL) == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            //if (LoginManager.IsActionPermittedOnPage(m_sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.PUBLISH) == false)
            //LoginManager.LogoutFromSite("login.html");

            if (m_sDB == "couchbase")
            {
                if (m_sTable == "Epg")
                {
                    //Delete from CouchBase
                    int nGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                    ulong uID = 0;
                    bool successParse = ulong.TryParse(m_nID.ToString(), out uID);
                    if (successParse)
                    {
                        EpgBL.TvinciEpgBL oEpgBL = new EpgBL.TvinciEpgBL(nGroupID);
                        EpgCB epgCB = oEpgBL.GetEpgCB(uID);
                        if (epgCB != null)
                        {
                            if (epgCB.Status == 4)
                            {
                                if (LoginManager.IsActionPermittedOnPage(m_sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.REMOVE) == false)
                                {
                                    LoginManager.LogoutFromSite("login.html");
                                    return;
                                }
                            }

                            if (m_bConfirm) // confirm button
                            {
                                if (epgCB.Status == 4) //remove permanent
                                {
                                    oEpgBL.RemoveEpg(uID);
                                    bool result = false;
                                    result = ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nGroupID, ApiObjects.eAction.Delete);
                                }
                                else if (epgCB.Status == 1)
                                {
                                    epgCB.Status = 4;
                                    bool res = oEpgBL.UpdateEpg(epgCB, null);
                                    bool result = false;
                                    result = ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nGroupID, ApiObjects.eAction.Update);
                                }
                            }
                            else // cancel button
                            {
                                epgCB.Status = 1;
                                bool res = oEpgBL.UpdateEpg(epgCB, null);
                                bool result = false;
                                result = ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nGroupID, ApiObjects.eAction.Update);
                            }

                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                            if (m_bConfirm == true)
                            {
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                            }
                            else
                            {
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                            }
                            updateQuery += "where";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        //Delete from ElasticSearch
                    }
                }
                if (Session["LastContentPage"].ToString().IndexOf("?") == -1)
                    Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "?search_save=1'</script>");
                else
                    Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "&search_save=1'</script>");


            }
            else
            {

                object oStatus = ODBCWrapper.Utils.GetTableSingleVal(m_sTable, "status", m_nID, m_sDB);
                if (oStatus != null)
                {
                    Int32 nCurrentStatus = int.Parse(oStatus.ToString());
                    if (nCurrentStatus == 3)
                    {
                        if (LoginManager.IsActionPermittedOnPage(m_sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.PUBLISH) == false)
                        {
                            LoginManager.LogoutFromSite("login.html");
                            return;
                        }
                    }
                    if (nCurrentStatus == 4)
                    {
                        if (LoginManager.IsActionPermittedOnPage(m_sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.REMOVE) == false)
                        {
                            LoginManager.LogoutFromSite("login.html");
                            return;
                        }
                    }
                }
                Remove();
            }
        }
        catch
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
    }

    protected void Remove()
    {
        bool bIsPublished = false;
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(m_sTable);
        updateQuery.SetConnectionKey(m_sDB);
        if (m_bConfirm == true)
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        else
        {
            bIsPublished = true;
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        }
        if (m_sTable.ToLower() == "media")
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
        }
        updateQuery += "where";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        updateQuery += "and";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery(m_sTable);
        updateQuery1.SetConnectionKey(m_sDB);
        if (m_bConfirm == true)
        {
            bIsPublished = true;
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        }
        else
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        // if media is deleted - update it's update_date too.
        if (m_sTable.ToLower() == "media")
        {
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
        }
        updateQuery1 += "where";
        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        updateQuery1 += "and";
        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 3);
        updateQuery1.Execute();
        updateQuery1.Finish();
        updateQuery1 = null;

        int nStatus = -1;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(m_sDB);
        selectQuery += "select status from " + m_sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oNURL = selectQuery.Table("query").DefaultView[0].Row["status"];
                if (oNURL != DBNull.Value && oNURL != null)
                    nStatus = int.Parse(oNURL.ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nStatus == 2) // Deleted
        {
            eAction action = eAction.Delete;
            List<int> lIds = new List<int>() { m_nID };
            int nGroupId = LoginManager.GetLoginGroupID();
            // delete from cache this DLM object                       
            DomainsWS.module domainWS;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            string sWSURL;

            log.Debug("Remove - Table: " + m_sTable + " Id:" + m_nID.ToString());

            //Remove Media / Channel from Lucene             
            switch (m_sTable.ToLower())
            {
                case "media":
                    if (!ImporterImpl.UpdateIndex(lIds, nGroupId, action))
                    {
                        log.Error(string.Format("Failed updating index for mediaIDs: {0}, groupID: {1}", lIds, nGroupId));
                    }
                    break;
                case "channels":
                    if (!ImporterImpl.UpdateChannelIndex(nGroupId, lIds, action))
                    {
                        log.Error(string.Format("Failed updating channel index for channelIDs: {0}, groupID: {1}", lIds, nGroupId));
                    }
                    break;
                case "channels_media":
                    object oChannelId = ODBCWrapper.Utils.GetTableSingleVal("channels_media", "CHANNEL_ID", m_nID);//get channel+media_id 
                    int nChannelId;
                    if (oChannelId != null && oChannelId != DBNull.Value)
                    {
                        nChannelId = int.Parse(oChannelId.ToString());
                        lIds.Clear();
                        lIds.Add(nChannelId);

                        if (!ImporterImpl.UpdateChannelIndex(nGroupId, lIds, action))
                        {
                            log.Error(string.Format("Failed updating channel index for channelIDs: {0}, groupID: {1}", lIds, nGroupId));
                        }
                    }
                    break;
                case "groups_device_families_limitation_modules":
                    //get parent_limit_module_id ==> than remove it                   
                    object oDlmID = ODBCWrapper.Utils.GetTableSingleVal("groups_device_families_limitation_modules", "PARENT_LIMIT_MODULE_ID", m_nID);
                    if (oDlmID != null)
                    {
                        int dlmID = int.Parse(oDlmID.ToString());
                        domainWS = new DomainsWS.module();
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
                        sWSURL = GetWSURL("domains_ws");
                        if (sWSURL != "")
                            domainWS.Url = sWSURL;
                        try
                        {
                            DomainsWS.Status resp = domainWS.RemoveDLM(sWSUserName, sWSPass, dlmID);
                            log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", dlmID, resp.Code));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", dlmID, ex.Message, ex.StackTrace), ex);
                        }
                    }
                    break;
                case "groups_device_limitation_modules":
                    domainWS = new DomainsWS.module();
                    TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
                    sWSURL = GetWSURL("domains_ws");
                    if (sWSURL != "")
                        domainWS.Url = sWSURL;
                    try
                    {
                        DomainsWS.Status resp = domainWS.RemoveDLM(sWSUserName, sWSPass, m_nID);
                        log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", m_nID, resp.Code));
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", m_nID, ex.Message, ex.StackTrace), ex);
                    }
                    break;
                case "payment_gateway_config":
                    // set adapter configuration
                    object oPaymentGatewayId = ODBCWrapper.Utils.GetTableSingleVal("payment_gateway_config", "payment_gateway_id", m_nID, "billing_connection");

                    int paymentGatewayId = 0;
                    if (oPaymentGatewayId != null && oPaymentGatewayId != DBNull.Value)
                    {
                        paymentGatewayId = int.Parse(oPaymentGatewayId.ToString());
                        Billing.module billing = new Billing.module();
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SetPaymentGatewayConfiguration", "billing", sIP, ref sWSUserName, ref sWSPass);
                        sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("billing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                            billing.Url = sWSURL;
                        try
                        {
                            Billing.Status status = billing.SetPaymentGatewayConfiguration(sWSUserName, sWSPass, paymentGatewayId);
                            log.Debug("delete PG configuration - " + string.Format("payment gateway ID:{0}, status:{1}", paymentGatewayId, status.Code));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - " + string.Format("payment gateway ID:{0}, ex msg:{1}, ex st: {2} ", paymentGatewayId, ex.Message, ex.StackTrace), ex);
                        }
                    }
                    break;
                case "oss_adapter_config":
                    // set adapter configuration
                    object oOssAdapterId = ODBCWrapper.Utils.GetTableSingleVal("oss_adapter_config", "oss_adapter_id", m_nID);

                    int ossAdapterId = 0;
                    if (oOssAdapterId != null && oOssAdapterId != DBNull.Value)
                    {
                        ossAdapterId = int.Parse(oOssAdapterId.ToString());

                        apiWS.API api = new apiWS.API();

                        sWSUserName = "";
                        sWSPass = "";
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SetOSSAdapterConfiguration", "api", sIP, ref sWSUserName, ref sWSPass);
                        sWSURL = GetWSURL("api_ws");
                        if (sWSURL != "")
                            api.Url = sWSURL;

                        try
                        {
                            apiWS.Status status = api.SetOSSAdapterConfiguration(sWSUserName, sWSPass, ossAdapterId);
                            log.Debug("SetOSSAdapterConfiguration - " + string.Format("oss adapter id:{0}, status:{1}", ossAdapterId, status.Code));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - " + string.Format("oss adapter id :{0}, ex msg:{1}, ex st: {2} ", ossAdapterId, ex.Message, ex.StackTrace), ex);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        // If user confirmed - let's remove the key of the object from the cache
        if (m_bConfirm && !string.IsNullOrEmpty(this.cacheKey))
        {
            string ip = "1.1.1.1";
            string userName = "";
            string password = "";

            int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
            TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
            string url = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                List<string> keys = new List<string>();
                keys.Add(cacheKey);

                apiWS.API client = new apiWS.API();
                client.Url = url;

                client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
            }
        }

        if (bIsPublished == true)
        {
            ODBCWrapper.UpdateQuery updateQuery2 = new ODBCWrapper.UpdateQuery(m_sTable);
            updateQuery2.SetConnectionKey(m_sDB);
            updateQuery2 += ODBCWrapper.Parameter.NEW_PARAM("publish_date", "=", DateTime.UtcNow);
            updateQuery2 += "where";
            updateQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
            updateQuery2.Execute();
            updateQuery2.Finish();
            updateQuery2 = null;
        }
        if (Session["LastContentPage"].ToString().IndexOf("?") == -1)
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "?search_save=1&confirmed_id=" + m_nID + "'</script>");
        else
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "&search_save=1&confirmed_id=" + m_nID + "'</script>");
    }

    private string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLoginName()
    {
        Response.Write(LoginManager.GetLoginName());
    }

    public void GetPageContext()
    {
        /*
        string sTmp = "<table width=100%><tr><td class=alert_text>";
        sTmp += "אנא אשר שינוי סטטוס הרשומה שזיהויה: <br>";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select " + m_sRepresentField + " from " + m_sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_nID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sTmp += m_sRepresentName + " : " + selectQuery.Table("query").DefaultView[0].Row[m_sRepresentField].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        sTmp += "</td></tr>";
        sTmp += "<tr><td width=100% nowrap><table width=100%><tr>";
        sTmp += "<td width=60px align=center onmouseover=\"this.className='FormButtonOver';\" onmouseout=\"this.className='FormButton';\" valign=top onclick='FinalRemove();' class=FormButton nowrap id=\"confirm_btn\"><strong>&nbsp;אשר&nbsp;</strong></td>";
        sTmp += "<td width=30px></td>";
        sTmp += "<td width=60px align=center onmouseover=\"this.className='FormButtonOver';\" onmouseout=\"this.className='FormButton';\" valign=top onclick='window.document.location.href=\"" + Session["LastContentPage"].ToString() + "\";' class=FormButton nowrap><strong>&nbsp;בטל&nbsp;</strong></td>";
        sTmp += "<td width=100%></td>";
        sTmp += "</tr></table></td></tr>";
        sTmp += "</table>";
        Response.Write(sTmp);
         */
    }

    protected void RemoveTheRecord(Object Sender, EventArgs e)
    {
        bool bIsPublished = false;
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(m_sTable);
        updateQuery.SetConnectionKey(m_sDB);
        if (m_bConfirm == true)
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        else
        {
            bIsPublished = true;
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        }
        updateQuery += "where";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        updateQuery += "and";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery(m_sTable);
        updateQuery1.SetConnectionKey(m_sDB);
        if (m_bConfirm == true)
        {
            bIsPublished = true;
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        }
        else
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery1 += "where";
        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        updateQuery1 += "and";
        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 3);
        updateQuery1.Execute();
        updateQuery1.Finish();
        updateQuery1 = null;

        if (bIsPublished == true)
        {
            ODBCWrapper.UpdateQuery updateQuery2 = new ODBCWrapper.UpdateQuery(m_sTable);
            updateQuery2.SetConnectionKey(m_sDB);
            updateQuery2 += ODBCWrapper.Parameter.NEW_PARAM("publish_date", "=", DateTime.UtcNow);
            updateQuery2 += "where";
            updateQuery2 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
            updateQuery2.Execute();
            updateQuery2.Finish();
            updateQuery2 = null;
        }
        if (Session["LastContentPage"].ToString().IndexOf("?") == -1)
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "?search_save=1'</script>");
        else
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "&search_save=1'</script>");
    }
}
