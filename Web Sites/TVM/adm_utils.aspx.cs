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
using System.Threading;
using TVinciShared;
using TvinciImporter;
using System.Collections.Generic;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

public partial class adm_utils : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    //static string locker = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
    }

    public string ChangePic(string sObjectID, string sPicsIds)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sBasePicsURL = "";

        if (ImageUtils.IsDownloadPicWithImageServer())
        {
            if (Session["Pic_Image_Url"] != null)
            {
                sBasePicsURL = Session["Pic_Image_Url"].ToString();
                Session["Pic_Image_Url"] = null; 
            }
        }
        else
        {
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;
        }

        string sRet = sObjectID + "~|~";
        string[] sSplit = sPicsIds.Split(';');
        Int32 nRowCounter = 0;
        for (int i = 0; i < sSplit.Length; i++)
        {
            if (sSplit[i] != null && sSplit[i].ToString() != "")
            {
                object oPic = PageUtils.GetTableSingleVal("pics", "BASE_URL", int.Parse(sSplit[i]));
                if (oPic != null && oPic != DBNull.Value)
                {
                    nRowCounter++;
                    string sPicURL = ImageUtils.GetTNName(oPic.ToString(), "tn");
                    sRet += "<img src=\"";
                    sRet += sBasePicsURL;
                    if (sBasePicsURL.EndsWith("=") == false)
                        sRet += "/";
                    if (sBasePicsURL.EndsWith("=") == true)
                    {
                        string sTmp1 = "";
                        string[] s1 = sPicURL.Split('.');
                        for (int j = 0; j < s1.Length - 1; j++)
                        {
                            if (j > 0)
                                sTmp1 += ".";
                            sTmp1 += s1[j];
                        }
                        sPicURL = sTmp1;
                    }
                    sRet += sPicURL + "\" class=\"img_border\"/>";
                    if (nRowCounter == 6)
                    {
                        nRowCounter = 0;
                        sRet += "<br/>";
                    }
                }
            }
        }
        return sRet;
    }

    public string ChangeVid(string sObjectID, string sPicsIds, string sTable)
    {
        string sRet = sObjectID + "~|~";
        string[] sSplit = sPicsIds.Split(';');
        Int32 nRowCounter = 0;
        for (int i = 0; i < sSplit.Length; i++)
        {
            if (sSplit[i] != null && sSplit[i].ToString() != "")
            {
                nRowCounter++;

                Int32 nMediaVidID = 0;
                int groupId = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select mf.id, mf.group_id from media_files mf where mf.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", int.Parse(sSplit[i]));
                selectQuery += " order by mf.MEDIA_TYPE_ID";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nMediaVidID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        groupId = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField("", nMediaVidID);
                dr_player.Initialize("Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                string sName = PageUtils.GetTableSingleVal("media", "name", int.Parse(sSplit[i])).ToString();
                //string sPicURL = dr_player.GetTNImage();

                int picId = ODBCWrapper.Utils.GetIntSafeVal(PageUtils.GetTableSingleVal("media", "MEDIA_PIC_ID", int.Parse(sSplit[i])));
                
                string sPicURL = ImageUtils.GetImapgeSrc(picId, groupId);

                sRet += "<img style=\"cursor: pointer;\" onclick=\"ChangeVideoPlayer('" + sObjectID + "','" + dr_player.GetPlayerSrc() + "');\" src=\"";
                sRet += sPicURL + "\" class=\"img_border\"/>";
                sRet += "<div class=\"vid_name\">\r\n" + sName + "</div>\r\n";
                if (nRowCounter == 6)
                {
                    nRowCounter = 0;
                    sRet += "<br/>";
                }
            }
        }
        return sRet;
    }

    public string ChangeOnOffStateRow(string sTableName,
        string sFieldName, string sIndexField, string sIndexVal,
        string sRequestedStatus, string sOnStr, string sOffStr, string sConnKey)
    {
        string sRet = string.Empty;


        if (sTableName == "Epg")
        {
            Int32 nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());

            EpgBL.TvinciEpgBL oEpgBL = new EpgBL.TvinciEpgBL(nParentGroupID);
            EpgCB epgCB = oEpgBL.GetEpgCB(ulong.Parse(sIndexVal));
            if (epgCB != null)
            {
                epgCB.isActive = !(epgCB.isActive);
                bool res = oEpgBL.UpdateEpg(epgCB, null);

                //Update from ElasticSearch
                bool result = false;
                result = ImporterImpl.UpdateEpg(new List<ulong>() { epgCB.EpgID }, nParentGroupID, ApiObjects.eAction.Update);

                sRet = "activation_" + sFieldName.ToString() + "_" + sIndexVal + "~~|~~";
                if (res && epgCB.isActive)
                {
                    sRet += "<b>" + sOnStr + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "','" + sIndexField + "'," + sIndexVal.ToString() + ",0 , '" + sOnStr + "','" + sOffStr + "' , '" + sConnKey + "');\" ";
                    sRet += " class='adm_table_link_div' >";
                    sRet += sOffStr;
                    sRet += "</a>";
                }
                else // res = false or !epg.isActive
                {
                    sRet += "<b>" + sOffStr + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "','" + sIndexField + "'," + sIndexVal.ToString() + ",1 , '" + sOnStr + "','" + sOffStr + "' , '" + sConnKey + "');\" ";
                    sRet += " class='adm_table_link_div' >";
                    sRet += sOnStr;
                    sRet += "</a>";
                }
            }
            sFieldName = "is_active";
            sTableName = "epg_channels_schedule";
        }
        else
        {
            sRet = "activation_" + sFieldName.ToString() + "_" + sIndexVal + "~~|~~";
            if (int.Parse(sRequestedStatus) == 1)
            {
                sRet += "<b>" + sOnStr + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "','" + sIndexField + "'," + sIndexVal.ToString() + ",0 , '" + sOnStr + "','" + sOffStr + "' , '" + sConnKey + "');\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += sOffStr;
                sRet += "</a>";
            }
            else
            {
                sRet += "<b>" + sOffStr + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "','" + sIndexField + "'," + sIndexVal.ToString() + ",1 , '" + sOnStr + "','" + sOffStr + "' , '" + sConnKey + "');\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += sOnStr;
                sRet += "</a>";
            }
        }

        Int32 nGroupID = LoginManager.GetLoginGroupID();
        Int32 nRowGroupID = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select group_id from " + sTableName + " where ";
            if (sConnKey != "")
                selectQuery.SetConnectionKey(sConnKey);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sIndexVal));
            if (selectQuery.Execute("query", true) != null)
            {
                nRowGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch { }
        bool bBelongs = false;
        if (nGroupID == 0)
            bBelongs = false;
        if (nRowGroupID != 0 && nRowGroupID != 1 && nRowGroupID != nGroupID)
        {
            PageUtils.DoesGroupIsParentOfGroup(nGroupID, nRowGroupID, ref bBelongs);
            if (bBelongs == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return "";
            }
        }
        else
            bBelongs = true;
        if (bBelongs == true)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sTableName);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", int.Parse(sRequestedStatus));
            if (sTableName == "epg_channels_schedule")
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            if (sConnKey != "")
                updateQuery.SetConnectionKey(sConnKey);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(sIndexVal));
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            // Update Media / Channel in Lucene

            eAction eAction = eAction.Update;

            // Update Media / Channel in Lucene
            int nId = int.Parse(sIndexVal);
            List<int> idsToUpdate = new List<int>();
            if (nId != 0)
            {
                idsToUpdate.Add(nId);
            }            
            switch (sTableName.ToLower())
            {
                case "media":
                    if (!ImporterImpl.UpdateIndex(idsToUpdate, nGroupID, eAction))
                    {
                        log.Error(string.Format("Failed updating index for mediaIDs: {0}, groupID: {1}", idsToUpdate, nGroupID));
                    }
                    break;
                case "channels":
                    if (!ImporterImpl.UpdateChannelIndex(nGroupID, idsToUpdate, eAction))
                    {
                        log.Error(string.Format("Failed updating channel index for channelIDs: {0}, groupID: {1}", idsToUpdate, nGroupID));
                    }
                    break;
                default:
                    break;
            }
        }

        return sRet;
    }

    public string ChangeOrderNumRow(string sTable, string sID, string sFieldName, string sDelta, string sConnKey)
    {
        Int32 nDelta = 0;
        nDelta = int.Parse(sDelta);
        if (nDelta == 0)
            return "";
        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
        if (sConnKey != "")
            directQuery.SetConnectionKey(sConnKey);
        directQuery += "update " + sTable + " set " + sFieldName + "=" + sFieldName;
        directQuery += "+";
        directQuery += "(" + sDelta + ")";
        directQuery += "where";
        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
        directQuery.Execute();
        directQuery.Finish();
        directQuery = null;

        //Update Catalog group-cache
        if (sTable.Equals("channels_media"))
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            int nChannelID = 0;
            int nMediaID = 0;
            int nOrderNum = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select CHANNEL_ID, MEDIA_ID, ORDER_NUM from channels_media where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
            if (sConnKey != "")
                selectQuery.SetConnectionKey(sConnKey);
            if (selectQuery.Execute("query", true) != null)
            {
                nChannelID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "CHANNEL_ID", 0);
                nMediaID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "MEDIA_ID", 0);
                nOrderNum = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ORDER_NUM", 0);
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nChannelID > 0)
            {
                bool result = false;
                result = ImporterImpl.UpdateChannelIndex(nGroupID, new List<int>() { nChannelID }, eAction.Update);
                log.Debug("ChangeOrderNumRow - " + string.Format("cahnnel:{0}, media:{1}, orderNum:{2}, res:{3}", nChannelID, nMediaID, nOrderNum, result));
            }
        }

        string sCurrentVal = ODBCWrapper.Utils.GetTableSingleVal(sTable, sFieldName, int.Parse(sID), sConnKey).ToString();
        string sRet = sFieldName + "_" + sID.ToString() + "~~|~~";
        sRet += "<td valign=\"top\" nowrap id=\"" + sFieldName + "_" + sID.ToString() + "\" align=\"center\" nowrap=\"nowrap\" style=\"margin: 0 0 0 0; padding: 0 0 0 0;\">";
        sRet += "<table cellpadding=\"0\" cellspacing=\"0\">";
        sRet += "<tr>";
        sRet += "<td align=\"right\" style=\"cursor: pointer;\"><img  onmouseover=\"this.src='images/arrow_down-over.gif';\" onmouseout=\"this.src='images/arrow_down.gif';\" src=\"images/arrow_down.gif\" onclick=\"javascript: ChangeOrderNumRow('" + sTable + "'," + sID + ",'" + sFieldName + "',-1,'" + sConnKey + "');\"/></td>";
        sRet += "<td align=\"center\" nowrap=\"nowrap\">" + sCurrentVal + "</td>";
        sRet += "<td align=\"left\" style=\"cursor: pointer;\"><img  onmouseover=\"this.src='images/arrow_up-over.gif';\" onmouseout=\"this.src='images/arrow_up.gif';\" src=\"images/arrow_up.gif\" onclick=\"javascript: ChangeOrderNumRow('" + sTable + "'," + sID + ",'" + sFieldName + "',1,'" + sConnKey + "');\"/></td>";
        sRet += "</tr>";
        sRet += "</table>";
        sRet += "</td>";

        return sRet;
    }

    public string ChangeActiveStateRow(string sTable, string sID, string sStatus)
    {
        return ChangeActiveStateRow(sTable, sID, sStatus, "");
    }

    public string ChangeActiveStateRow(string sTable, string sID, string sStatus, string sConnectionKey, string sPage = "")
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        Int32 nRowGroupID = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (sConnectionKey != "")
                selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select group_id from " + sTable + " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
            if (selectQuery.Execute("query", true) != null)
            {
                nRowGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch { }
        bool bBelongs = false;
        if (nGroupID == 0)
            bBelongs = false;
        if (nRowGroupID != 0 && nRowGroupID != nGroupID)
        {
            PageUtils.DoesGroupIsParentOfGroup(nGroupID, nRowGroupID, ref bBelongs);
        }
        else
            bBelongs = true;
        if (bBelongs == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        else
        {
            string sRet = "activation_" + sID.ToString() + "~~|~~";

            string pageParam = string.IsNullOrEmpty(sPage) ? "" : ", '" + sPage + "'";

            if (sTable == "message_announcements")
            {
                Int32 groupID = LoginManager.GetLoginGroupID();
                bool result = false;

                bool bStatus = int.Parse(sStatus) == 1 ? true : false;
                result = ImporterImpl.UpdateMessageAnnouncementStatus(groupID, int.Parse(sID), bStatus);
                if (result)
                {
                    if (int.Parse(sStatus) == 1)
                    {
                        sRet += "<b>On</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",0,'" + sConnectionKey + "'" + pageParam + ");\" ";
                        sRet += " class='adm_table_link_div' >";
                        sRet += "Off";
                        sRet += "</a>";
                    }
                    else
                    {
                        sRet += "<b>Off</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",1,'" + sConnectionKey + "'" + pageParam + ");\" ";
                        sRet += " class='adm_table_link_div' >";
                        sRet += "On";
                        sRet += "</a>";
                    }
                }
                else // fail to change status
                {
                    if (int.Parse(sStatus) != 1)
                    {
                        sRet += "<b>On</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",0,'" + sConnectionKey + "'" + pageParam + ");\" ";
                        sRet += " class='adm_table_link_div' >";
                        sRet += "Off";
                        sRet += "</a>";
                    }
                    else
                    {
                        sRet += "<b>Off</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",1,'" + sConnectionKey + "'" + pageParam + ");\" ";
                        sRet += " class='adm_table_link_div' >";
                        sRet += "On";
                        sRet += "</a>";
                    }
                }
                return sRet;
            }
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sTable);
                if (sConnectionKey != "")
                    updateQuery.SetConnectionKey(sConnectionKey);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", int.Parse(sStatus));
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery += "where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
           
            // Update Media / Channel in Lucene
            log.Debug("ChangeActiveStateRow - table:" + sTable + ", ID:" + sID);

            

            if (int.Parse(sStatus) == 1)
            {
                sRet += "<b>On</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",0,'" + sConnectionKey + "'" + pageParam + ");\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += "Off";
                sRet += "</a>";
            }
            else
            {
                sRet += "<b>Off</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",1,'" + sConnectionKey + "'" + pageParam + ");\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += "On";
                sRet += "</a>";
            }
            return sRet;
        }
    }

    private string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    public string GetCollectionFill(string sMiddleTable, string sCollTable, string sTextField, string sStart, string sCollCss, string sExtraQuery, string sCurrentSelect, string sHeader, string sConnKey)
    {
        sTextField = sTextField.Trim();
        sTextField = sTextField.Replace(" ", "+' '+");
        string sTextFieldForQuery = "c." + sTextField;
        string sRet = "";
        sRet += sHeader + "|";
        if (sStart != "")
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (sConnKey != "")
                selectQuery.SetConnectionKey(sConnKey);
            string sLike = sTextFieldForQuery + " like(N'" + sStart.Replace("'", "''") + "%')";
            selectQuery += "select distinct top 80 " + sTextFieldForQuery + " as val,c.id  from " + sCollTable + " c ";
            if (sCollTable.ToLower().Trim() == "tags")
                selectQuery += "," + sMiddleTable + " mc ";
            if (sMiddleTable.ToLower().Trim() == "pics_tags")
                selectQuery += ",pics p";

            selectQuery += " where ";
            string sGroups = PageUtils.GetFullGroupsStr(nGroupID, sConnKey);
            if (sCollTable.ToLower().Trim() == "tags")
            {
                selectQuery += "mc.tag_id=c.id and mc.status=1 and ";
                selectQuery += " (mc.group_id " + sGroups + " ";
                selectQuery += " or mc.group_id is null or mc.group_id=0) and ";
            }
            if (sMiddleTable.ToLower().Trim() == "pics_tags")
            {
                selectQuery += "p.id=mc.pic_id and p.status=1 and  ";
                selectQuery += " (p.group_id " + sGroups + " ";
                selectQuery += " or p.group_id is null or p.group_id=0) and ";
            }
            selectQuery += sLike + " and ";

            selectQuery += " (c.group_id " + sGroups + " ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " or c.group_id is null or c.group_id=0) and ";
            if (sExtraQuery != "")
            {
                selectQuery += sExtraQuery;
                selectQuery += "and";
            }
            selectQuery += "c.status=1 order by " + sTextFieldForQuery;
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRet += "<table dir=rtl style=\"background-color: #FFFFFF;\"  cellspacing='1' cellpadding='0' border='1'>";
                    sRet += "<tr class=adm_table_header><td width=150 nowrap style='cursor:pointer;'>";
                    sRet += "<table class='adm_table_header' style='height:5px'><tr>";
                    sRet += "<td nowrap width='100%'>Choose from:</td>";
                    sRet += "<td class=adm_table_header align='right' onclick='";
                    sRet += "closeCollDiv(\"" + sHeader + "_coll\")";
                    sRet += "'>X</td>";
                    sRet += "</tr></table>";
                    sRet += "</td></tr>";
                    sRet += "<tr><td><table dir='rtl' border='0' cellpadding='6' cellspacing='0'><tr class='adm_table_header_nbg'>";
                    sRet += "<td nowrap valign=top>";
                    string sLastColl = "";
                    Int32 nC = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sCollTxt = selectQuery.Table("query").DefaultView[i].Row["val"].ToString();
                        if (sLastColl == sCollTxt)
                        {
                            continue;
                        }
                        nC++;
                        sLastColl = sCollTxt;
                        string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                        sRet += "<a class='";
                        if (sCurrentSelect.IndexOf(sCollTxt) != -1)
                            sRet += sCollCss + "_selected";
                        else
                            sRet += sCollCss;
                        string sEncodedCollTxt = sCollTxt.Replace("'", "~~apos~~").Replace("&quot;", "~~qoute~~").Replace("\"", "~~qoute~~");
                        sRet += "' onclick='tagSelect(\"" + sMiddleTable + sID + "\",\"" + sEncodedCollTxt + "\",\"" + sHeader + "\");return false;' href='#'>";
                        sRet += sCollTxt + "<br>";
                        sRet += "</a>";
                        if (nC == 40)
                            break;
                    }
                    sRet += "</td>";
                    sRet += "</tr></table></td></tr></table>";
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        return sRet;
    }

    public void GetCurrentDate()
    {
        HttpContext.Current.Response.Write(DateUtils.GetStrFromDate(DateTime.UtcNow));
    }
}
