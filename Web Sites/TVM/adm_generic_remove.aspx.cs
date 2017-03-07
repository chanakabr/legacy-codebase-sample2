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
using System.Collections.Generic;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

public partial class adm_generic_remove : System.Web.UI.Page
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
            if (Request.QueryString["db"] != null)
                m_sDB = Request.QueryString["db"].ToString();
            else
                m_sDB = "";
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(m_nMainMenu, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, m_nSubMenu, true);
            m_sRepresentField = Request.QueryString["rep_field"].ToString();
            m_sRepresentName = Request.QueryString["rep_name"].ToString();
            cacheKey = Convert.ToString(Request.QueryString["cache_key"]);

            if (m_sBasePageURL != "")
            {
                if (LoginManager.IsPagePermitted(m_sBasePageURL) == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                if (LoginManager.IsActionPermittedOnPage(m_sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.REMOVE) == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                if (LoginManager.IsPagePermitted() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE) == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            Remove();
        }
        catch
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
    }

    protected void Remove()
    {
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
                        if (epgCB.Status == 4) //remove permanent
                        {
                            oEpgBL.RemoveEpg(uID);
                            //Delete from ElasticSearch                            
                            if (!ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nGroupID, ApiObjects.eAction.Delete))
                            {
                                log.Error(string.Format("Failed updating index for epgID: {0}, groupID: {1}", epgCB.EpgID, nGroupID));
                            }
                        }
                        else if (epgCB.Status == 1)
                        {
                            epgCB.Status = 4;
                            bool res = oEpgBL.UpdateEpg(epgCB);
                            //Update from ElasticSearch                            
                            if (!ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nGroupID, ApiObjects.eAction.Update))
                            {
                                log.Error(string.Format("Failed updating index for epgID: {0}, groupID: {1}", epgCB.EpgID, nGroupID));
                            }
                        }
                    }
                }
            }
        }
        else
        {           
            DomainsWS.module domainWS;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            string sWSURL;

            int logedInGroupID = LoginManager.GetLoginGroupID();

            if (m_sTable.ToLower() == "domains")
            {
                // select  status  if = 4 then remove domain 
                if (RemoveDomain())
                    return;
            }
            //continue other cases that are not remove domain 
            
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(m_sTable);
            updateQuery.SetConnectionKey(m_sDB);
            // double confirm for remove
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
            int rowsChanged = updateQuery.ExecuteAffectedRows();
            updateQuery.Finish();
            updateQuery = null;

            //Remove Media / Channel from Lucene             
            List<int> lIds = new List<int>() { m_nID };

            // media changed from pending to deleted - delete index
            if (rowsChanged > 0 && m_sTable.ToLower() == "media")
            {
                if (!ImporterImpl.UpdateIndex(lIds, logedInGroupID, ApiObjects.eAction.Delete))
                {
                    log.Error(string.Format("Failed updating index for mediaIDs: {0}, groupID: {1}", lIds, logedInGroupID));
                }
            }

            ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery(m_sTable);
            updateQuery1.SetConnectionKey(m_sDB);
            // double confirm for remove
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
            updateQuery1 += "where";
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
            updateQuery1 += "and";
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            updateQuery1.Execute();
            updateQuery1.Finish();
            updateQuery1 = null;

            // media changed from active to pending - update index
            if (rowsChanged > 0 && m_sTable.ToLower() == "media")
            {
                if (!ImporterImpl.UpdateIndex(lIds, logedInGroupID, ApiObjects.eAction.Update))
                {
                    log.Error(string.Format("Failed updating index for mediaIDs: {0}, groupID: {1}", lIds, logedInGroupID));
                }
            }

            // if its not media
            if (m_sTable.ToLower() != "media")
            {
                switch (m_sTable.ToLower())
                {
                    case "ppv_modules":
                        if (!ImporterImpl.UpdateFreeFileTypeOfModule(logedInGroupID, m_nID))
                        {
                            log.Error(string.Format("Failed updating free file index for ppvModule: {0}, groupID: {1}", m_nID, logedInGroupID));
                        }
                        break;
                    case "media_files":
                        Int32 nMediaID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", m_nID).ToString());
                        if (nMediaID > 0)
                        {
                            if (!ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, logedInGroupID, ApiObjects.eAction.Update))
                            {
                                log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", nMediaID, logedInGroupID));
                            }
                        }
                        break;
                    case "channels":
                        if (!ImporterImpl.UpdateChannelIndex(logedInGroupID, lIds, ApiObjects.eAction.Delete))
                        {
                            log.Error(string.Format("Failed updating channel index for channelIDs: {0}, groupID: {1}", lIds, logedInGroupID));
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

                            if (!ImporterImpl.UpdateChannelIndex(logedInGroupID, lIds, ApiObjects.eAction.Delete))
                            {
                                log.Error(string.Format("Failed updating channel index for channelIDs: {0}, groupID: {1}", lIds, logedInGroupID));
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
                            TVinciShared.WS_Utils.GetWSUNPass(logedInGroupID, "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
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
                        // delete from cache this DLM object                       
                        domainWS = new DomainsWS.module();

                        TVinciShared.WS_Utils.GetWSUNPass(logedInGroupID, "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
                        sWSURL = GetWSURL("domains_ws");
                        if (sWSURL != "")
                            domainWS.Url = sWSURL;
                        try
                        {
                            DomainsWS.Status resp = domainWS.RemoveDLM(sWSUserName, sWSPass, m_nID);
                            log.Debug("RemoveDLM " + string.Format("Dlm:{0}, res:{1}", m_nID, resp.Code));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", m_nID, ex.Message, ex.StackTrace), ex);
                        }
                        break;                   
                    default:
                        break;
                }
            }
        }

        // Will be done in confirm
        //// If confirmed - remove object from cache according to its key
        //if (m_bConfirm && !string.IsNullOrEmpty(this.cacheKey))
        //{
        //    string ip = "1.1.1.1";
        //    string userName = "";
        //    string password = "";

        //    int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
        //    TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
        //    string url = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

        //    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
        //    {
        //        List<string> keys = new List<string>();
        //        keys.Add(cacheKey);

        //        apiWS.API client = new apiWS.API();
        //        client.Url = url;

        //        client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
        //    }
        //}

        if (Session["LastContentPage"].ToString().IndexOf("?") == -1)
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "?search_save=1'</script>");
        else
            Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "&search_save=1'</script>");
    }

    private bool RemoveDomain()
    {
        bool isRemoved = false;
        int domainStatus = ODBCWrapper.Utils.GetIntSafeVal(ODBCWrapper.Utils.GetTableSingleVal("domains", "status", m_nID, m_sDB));
        if (domainStatus == 4)
        {
            // change status back to 1 so  we can remove domain 

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(m_sTable);
            updateQuery.SetConnectionKey(m_sDB);
            // double confirm for remove
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
            int rowsChanged = updateQuery.ExecuteAffectedRows();
            updateQuery.Finish();
            updateQuery = null;

            if (rowsChanged == 0)
            {
                return false;
            }

            DomainsWS.module domainWS = new DomainsWS.module();           
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            string sWSURL;

            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "RemoveDomain", "domains", sIP, ref sWSUserName, ref sWSPass);
            sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("domains_ws");
            if (sWSURL != "")
                domainWS.Url = sWSURL;
            try
            {
                DomainsWS.DomainResponseStatus resp = domainWS.RemoveDomain(sWSUserName, sWSPass, m_nID);
                if (resp == DomainsWS.DomainResponseStatus.OK)
                {
                    isRemoved = true;
                }
                log.Debug("RemoveDomain - " + string.Format("DomainId:{0}, res:{1}", m_nID, resp.ToString()));
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("DomainId:{0}, msg:{1}, st:{2}", m_nID, ex.Message, ex.StackTrace), ex);
                isRemoved = false;
            }
        }
        return isRemoved;
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
        sTmp += "אנא אשר מחיקת הרשומה שזיהויה: <br>";
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
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(m_sTable);
        updateQuery.SetConnectionKey(m_sDB);
        // double confirm for remove
        //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 4);
        // Single confirm for remove
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += "where";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
        Response.Write("<script>document.location.href='" + Session["LastContentPage"].ToString() + "?search_save=1'</script>");
    }
}
