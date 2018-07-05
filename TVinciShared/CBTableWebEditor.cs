
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TVinciShared
{
    public class CBTableWebEditor<T>
    {
        protected System.Collections.Hashtable m_hiddenFields;
        protected System.Collections.Hashtable m_HTMLFields;
        protected System.Collections.Hashtable m_ImageFields;
        protected System.Collections.Hashtable m_OnOffFields;
        protected System.Collections.Hashtable m_AudioFields;
        protected System.Collections.Hashtable m_LinkColumns;
        protected System.Collections.Hashtable m_MultiValuesColumns;
        protected System.Collections.Specialized.NameValueCollection m_OrderByColumns;

        protected bool m_bPrintButton;
        protected bool m_bExcellButton;
        protected bool m_bNewButton;
        protected bool m_bWithTechDetails;
        protected bool m_bWithEditorRemarks;
        protected bool m_bWithActivation;
        protected bool m_bWithOrderNum;
        protected bool m_bWithOrderNum2;
        protected bool m_bWithVideo;
        protected bool m_bLinksBefore;

        protected string m_sHeaderCSS;
        protected string m_sNewParameters;
        protected string m_sCellCSS;
        protected string m_sAltCellCSS;
        protected string m_sPagerCSS;
        protected string m_sLinkCss;
        protected string m_sTableCss;
        protected string m_sTechDetailsTable;
        protected string m_sEditorRemarksTable;
        protected string m_sActivationTable;
        protected string m_sOrderNumTable;
        protected string m_sOrderNumTable2;
        protected string m_sOrderNumFieldName;
        protected string m_sOrderNumFieldName2;
        protected string m_sOrderNumFiledToChange;
        protected string m_sOrderNumFiledToChange2;
        protected string m_sOrderNumFiledHeader;
        protected string m_sOrderNumFiledHeader2;
        protected string m_sVideoTable;
        protected string m_sNewStr;

        protected Int32 m_nPageSize;

        List<T> m_theData;
        DataTable m_theDataTable;

        public CBTableWebEditor(bool bPrintButton, bool bExcellButton, bool bNewButton, string sNewParameters, string sHeaderCSS, string sCellCss, string sAltCellCss, string sLinkCss,
                                string sPagerCss, string sTableCss, string sOrderBy, Int32 nPageSize)
        {
            m_sNewStr = "";
            m_bLinksBefore = false;
            m_sNewParameters = sNewParameters;
            m_hiddenFields = new System.Collections.Hashtable();
            m_HTMLFields = new System.Collections.Hashtable();
            m_ImageFields = new System.Collections.Hashtable();
            m_OnOffFields = new System.Collections.Hashtable();
            m_AudioFields = new System.Collections.Hashtable();
            m_LinkColumns = new System.Collections.Hashtable();
            m_MultiValuesColumns = new System.Collections.Hashtable();
            m_bPrintButton = bPrintButton;
            m_bExcellButton = bExcellButton;
            m_bNewButton = bNewButton;
            m_sHeaderCSS = sHeaderCSS;
            m_sAltCellCSS = sAltCellCss;
            m_sPagerCSS = sPagerCss;
            m_sCellCSS = sCellCss;
            m_sLinkCss = sLinkCss;
            m_sTableCss = sTableCss;
            HttpContext.Current.Session["order_by"] = sOrderBy;
            m_nPageSize = nPageSize;
            m_OrderByColumns = new System.Collections.Specialized.NameValueCollection();
            m_bWithTechDetails = false;
            m_bWithEditorRemarks = false;
            m_bWithActivation = false;
            m_bWithVideo = false;
            m_theData = null;
        }


        public void AddLinkColumn(DataTableLinkColumn theColumn)
        {
            m_LinkColumns.Add(m_LinkColumns.Count.ToString(), theColumn);
        }

        public void AddHiddenField(string sFieldName)
        {
            m_hiddenFields.Add(sFieldName.ToUpper(), true);
        }

        public void AddImageField(string sFieldName)
        {
            m_ImageFields.Add(sFieldName.ToUpper(), true);
        }

        public void AddOnOffField(string sFieldName, string sIndexFieldName)
        {
            m_OnOffFields.Add(sFieldName.ToUpper(), sIndexFieldName);
        }


        public void AddField(string sFieldName)
        {
            m_HTMLFields.Add(sFieldName.ToUpper(), true);
        }

        public void FillDataTable(DataTable d)
        {
            m_theDataTable = d;
        }

        public virtual void SetData(List<T> theData)
        {
            m_theData = theData;
        }

        public virtual List<T> GetData()
        {
            return m_theData;
        }

        public virtual System.Collections.Hashtable GetHTMLFields()
        {
            return m_HTMLFields;
        }

        public virtual System.Collections.Hashtable GetHiddenFields()
        {
            return m_hiddenFields;
        }

        public virtual System.Collections.Hashtable GetOnOffFields()
        {
            return m_OnOffFields;
        }
        public virtual System.Collections.Hashtable GetImageFields()
        {
            return m_ImageFields;
        }

        protected void PageToSession(string sPageURL)
        {
            if (sPageURL != "")
                HttpContext.Current.Session["ContentPage"] = sPageURL;
            string sURL = HttpContext.Current.Request.FilePath.ToString();
            Int32 nStart = sURL.LastIndexOf('/'); ;
            Int32 nEnd = sURL.Length;
            string sPage = sURL.Substring(nStart + 1, nEnd - nStart - 1);
            if (sPageURL == "")
                HttpContext.Current.Session["ContentPage"] = sPage;
            HttpContext.Current.Session["LastContentPage"] = sPage;
            if (m_sNewParameters != "")
            {
                HttpContext.Current.Session["LastContentPage"] += "?";
                HttpContext.Current.Session["LastContentPage"] += m_sNewParameters;
            }
        }


        protected string GetTableHTML(Int32 nPageNum, string sOrderBy, string sPageURL)
        {
            return GetTableHTML(nPageNum, sOrderBy, sPageURL, true);
        }
        public string GetPageHTML(Int32 nPageNum, string sOrderBy)
        {
            return GetTableHTML(nPageNum, sOrderBy, "");
        }

        public string GetPageHTML(Int32 nPageNum, string sOrderBy, bool bEnterToSession)
        {
            return GetTableHTML(nPageNum, sOrderBy, "", bEnterToSession);
        }

        public string GetPageHTML(Int32 nPageNum, string sOrderBy, string sPageURL)
        {
            return GetTableHTML(nPageNum, sOrderBy, sPageURL);
        }

        protected string GetTableHTML(Int32 nPageNum, string sOrderBy, string sPageURL, bool bEnterToSession)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sBasePicsURL = PageUtils.GetBasePicURL(nGroupID);
            if (bEnterToSession == true)
                PageToSession(sPageURL);
            if (HttpContext.Current.Session["LastTablePage"] != null)
            {
                if (HttpContext.Current.Session["LastTablePage"].ToString() == LoginManager.GetCurrentPageURL() &&
                    nPageNum == 0 && HttpContext.Current.Session["LastTablePageNum"] != null)
                {
                    nPageNum = int.Parse(HttpContext.Current.Session["LastTablePageNum"].ToString());
                }
            }
            if (nPageNum == 0)
                nPageNum = 1;

            if (bEnterToSession == true)
            {
                HttpContext.Current.Session["LastTablePageNum"] = nPageNum;
                HttpContext.Current.Session["LastTablePage"] = LoginManager.GetCurrentPageURL();
            }

            Int32 nStartPage = (nPageNum - 1) * m_nPageSize + 1;
            Int32 nEndPage = nStartPage + m_nPageSize - 1;
            Int32 nPages = m_theDataTable.DefaultView.Count;
            //???????????????????????????????  
            //if (m_nQueryCount != 0)
            //    nPages = m_nQueryCount;
            //if (m_theDataTable.Rows.Count > 0)
            //{
            //    nPages = m_theDataTable.Rows.Count;
            //}

            if (nEndPage > nPages)
                nEndPage = nPages;

            double dPages = (double)nPages / m_nPageSize;
            nPages = (Int32)dPages + 1;
            if (dPages == (Int32)dPages && nPages > 1)
                nPages--;
            Int32 nRowsCount = m_theDataTable.Columns.Count;       

            //string sTable = "<table cellpadding=0 cellspacing=1 width=100%>";
            StringBuilder sTable = new StringBuilder();
            sTable.Append(GetTopLine(nPages, nStartPage, nEndPage, nPageNum, sOrderBy));

            sTable.Append("<table cellpadding=1 cellspacing=1 class='" + m_sTableCss + "'>");

            sTable.Append("<tr>");
            Int32 nCount = m_theDataTable.DefaultView.Count;
            for (int i = 0; i < nRowsCount; i++)
            {
                string sName = m_theDataTable.Columns[i].ColumnName.ToString();
                string sNewOrderBy = "";
                string sImage = "";
                if (m_OrderByColumns[sName] != null && m_OrderByColumns[sName].ToString() != "")
                {
                    sImage = "images/left_arrow.gif";
                    sNewOrderBy = m_OrderByColumns[sName].ToString();
                    if (sOrderBy == sNewOrderBy)
                    {
                        sImage = "images/up_arrow.gif";
                        sNewOrderBy = sOrderBy + " desc";
                    }
                    else if (sOrderBy == sNewOrderBy + " desc")
                    {
                        sImage = "images/down_arrow.gif";
                    }
                }

                if (m_hiddenFields.ContainsKey(sName.ToUpper()) && (bool)(m_hiddenFields[sName.ToUpper()]) == true)
                    continue;

                sTable.Append("<td class='" + m_sHeaderCSS + "' ");
                if (sImage != "")
                {
                    sTable.Append("style='cursor:pointer;' ");
                    sTable.Append("onclick='GetPageTable(\"" + sNewOrderBy + "\"," + nPageNum.ToString() + ");'");
                }
                sTable.Append("><table cellpadding=0 cellspacing=1 ><tr>");

                if (sImage != "")
                {
                    sTable.Append("<td>");
                    sTable.Append("<img src='" + sImage + "'/>");
                    sTable.Append("</td>");
                }
                sTable.Append("<td width=100% nowrap>");
                sTable.Append(sName);
                sTable.Append("</td>");
                sTable.Append("</tr>");
                sTable.Append("<tr height=0><td height=0><table style='visibility: hidden; display: none'><tr><td><a targe='_blank' id='download_csv' href='#'></a></td></tr></table></td></tr>");
                sTable.Append("</table></td>");
            }
            if (m_bWithOrderNum == true)
            {
                string sImage = "images/left_arrow.gif";
                string sNewOrderBy = m_sOrderNumFiledToChange;
                if (sOrderBy == m_sOrderNumFiledToChange)
                {
                    sImage = "images/up_arrow.gif";
                    sNewOrderBy = m_sOrderNumFiledToChange + " desc";
                }
                else if (sOrderBy == m_sOrderNumFiledToChange + " desc")
                {
                    sImage = "images/down_arrow.gif";
                }

                sTable.Append("<td class='" + m_sHeaderCSS + "' ");
                if (sImage != "")
                {
                    sTable.Append("style='cursor:pointer;' ");
                    sTable.Append("onclick='GetPageTable(\"" + sNewOrderBy + "\"," + nPageNum.ToString() + ");'");
                }
                sTable.Append("><table cellpadding=0 cellspacing=1 ><tr>");

                if (sImage != "")
                {
                    sTable.Append("<td>");
                    sTable.Append("<img src='" + sImage + "'/>");
                    sTable.Append("</td>");
                }
                sTable.Append("<td width=100% nowrap>");
                sTable.Append(m_sOrderNumFiledHeader);
                sTable.Append("</td>");
                sTable.Append("</tr>");
                sTable.Append("<tr height=0><td height=0><table style='visibility: hidden; display: none'><tr><td><a targe='_blank' id='download_csv' href='#'></a></td></tr></table></td></tr>");
                sTable.Append("</table></td>");
            }
            if (m_bWithOrderNum2 == true)
            {
                string sImage = "images/left_arrow.gif";
                string sNewOrderBy = m_sOrderNumFiledToChange2;
                if (sOrderBy == m_sOrderNumFiledToChange2)
                {
                    sImage = "images/up_arrow.gif";
                    sNewOrderBy = m_sOrderNumFiledToChange2 + " desc";
                }
                else if (sOrderBy == m_sOrderNumFiledToChange2 + " desc")
                {
                    sImage = "images/down_arrow.gif";
                }

                sTable.Append("<td class='" + m_sHeaderCSS + "' ");
                if (sImage != "")
                {
                    sTable.Append("style='cursor:pointer;' ");
                    sTable.Append("onclick='GetPageTable(\"").Append(sNewOrderBy).Append("\",").Append(nPageNum.ToString()).Append(");'");
                }
                sTable.Append("><table cellpadding=0 cellspacing=1 ><tr>");

                if (sImage != "")
                {
                    sTable.Append("<td>");
                    sTable.Append("<img src='").Append(sImage).Append("'/>");
                    sTable.Append("</td>");
                }
                sTable.Append("<td width=100% nowrap>");
                sTable.Append(m_sOrderNumFiledHeader2);
                sTable.Append("</td>");
                sTable.Append("</tr>");
                sTable.Append("<tr height=0><td height=0><table style='visibility: hidden; display: none'><tr><td><a targe='_blank' id='download_csv' href='#'></a></td></tr></table></td></tr>");
                sTable.Append("</table></td>");
            }
            if (m_bWithVideo == true)
            {
                sTable.Append("<td class='").Append(m_sHeaderCSS).Append("'>");
                sTable.Append("Video");
                sTable.Append("</td>");
            }
            for (int i = 0; i < m_MultiValuesColumns.Count; i++)
            {
                string sHeader = ((DataTableMultiValuesColumn)(m_MultiValuesColumns[i.ToString()])).GetColumnHeader();
                sTable.Append("<td class='").Append(m_sHeaderCSS).Append("'>");
                sTable.Append(sHeader);
                sTable.Append("</td>");
            }
            for (int i = 0; i < m_LinkColumns.Count; i++)
            {
                string sLinkColumnName = ((DataTableLinkColumn)m_LinkColumns[i.ToString()]).GetColumnText();
                if (sLinkColumnName.ToLower().Trim() == "edit")
                {
                    sLinkColumnName = "<img src='";
                    sLinkColumnName += "images/edit_icon.gif";
                    sLinkColumnName += "' ";
                    sLinkColumnName += " />";
                }
                else if (sLinkColumnName.ToLower().Trim() == "collection")
                {
                    sLinkColumnName = "<img src='";
                    sLinkColumnName += "images/gedit_icon_01.png";
                    sLinkColumnName += "' ";
                    sLinkColumnName += " />";
                }
                else if (sLinkColumnName.ToLower().Trim() == "delete")
                {
                    sLinkColumnName = "<img src='";
                    sLinkColumnName += "images/delete_icon.gif";
                    sLinkColumnName += "' ";
                    sLinkColumnName += " />";
                }
                else if (sLinkColumnName.ToLower().Trim() == "statistics")
                {
                    sLinkColumnName = "<img src='";
                    sLinkColumnName += "images/stat_icon.gif";
                    sLinkColumnName += "' ";
                    sLinkColumnName += " />";
                }
                else if (sLinkColumnName.ToLower().Trim() == "confirm")
                {
                    sLinkColumnName = "&nbsp;&nbsp;";
                }
                else if (sLinkColumnName.ToLower().Trim() == "cancel")
                {
                    sLinkColumnName = "&nbsp;&nbsp;";
                }
                sTable.Append("<td class='").Append(m_sHeaderCSS).Append("' nowrap>").Append(sLinkColumnName).Append("</td>");
            }
            if (m_bWithActivation == true)
            {
                sTable.Append("<td class='").Append(m_sHeaderCSS).Append("'>");
                sTable.Append("On / Off");
                sTable.Append("</td>");
            }
            if (m_bWithEditorRemarks == true)
            {
                sTable.Append("<td class='" + m_sHeaderCSS + "'>");
                sTable.Append("<img src='");
                sTable.Append("images/info_icon.gif");
                sTable.Append("' ");
                sTable.Append(" />");
                sTable.Append("</td>");
            }
            if (m_bWithTechDetails == true)
            {
                sTable.Append("<td class='" + m_sHeaderCSS + "'>");
                sTable.Append("<img src='");
                sTable.Append("images/history_icon.gif");
                sTable.Append("' ");
                sTable.Append(" />");
                sTable.Append("</td>");
            }
            sTable.Append("</tr>");
            if (nEndPage <= nStartPage - 1)
            {
                Int32 nColspan = nRowsCount + m_MultiValuesColumns.Count + m_LinkColumns.Count;
                if (m_bWithActivation == true)
                    nColspan++;
                if (m_bWithOrderNum == true)
                    nColspan++;
                if (m_bWithOrderNum2 == true)
                    nColspan++;
                if (m_bWithVideo == true)
                    nColspan++;
                if (m_bWithEditorRemarks == true)
                    nColspan++;
                if (m_bWithTechDetails == true)
                    nColspan++;
                sTable.Append("<tr class='" + m_sCellCSS + "' onmouseover='this.className=\"adm_table_mo_cell\";' onmouseout='this.className=\"" + m_sCellCSS + "\";' >");
                sTable.Append("<td colspan=" + nColspan.ToString() + ">&nbsp;&nbsp;&nbsp;&nbsp;No records</td>");
                sTable.Append("</tr>");
            }
            Int32 nCounter = 0;
            for (int pageIndx = nStartPage - 1; pageIndx < nEndPage; pageIndx++)
            {
                nCounter++;
                double d = (double)pageIndx / 2;
                string sCss = m_sCellCSS;
                if (d == (long)d)
                    sCss = m_sAltCellCSS;
                sTable.Append("<tr class='" + sCss + "' onmouseover='this.className=\"adm_table_mo_cell\";' onmouseout='this.className=\"" + sCss + "\";'>");
                for (int j = 0; j < nRowsCount; j++)
                {
                    string sName = m_theDataTable.Columns[j].ColumnName.ToString();
                    object sValue = m_theDataTable.DefaultView[pageIndx].Row[j];
                    if (sValue.ToString() == "")
                        sValue = "-";

                    if (m_hiddenFields.ContainsKey(sName.ToUpper()) && (bool)(m_hiddenFields[sName.ToUpper()]) == true)
                        continue;
                    
                    if (m_ImageFields.ContainsKey(sName.ToUpper()))
                    {
                        string sFileName = sValue.ToString();
                        
                        if (!string.IsNullOrEmpty(sFileName) && sFileName != "-")
                        {
                            if (ImageUtils.IsDownloadPicWithImageServer())
                            {
                                sTable.Append("<td>");

                                if (m_theDataTable.Columns.Contains("pic_id"))
                                {
                                    object picId = m_theDataTable.DefaultView[pageIndx].Row["pic_id"];
                                    if (picId != DBNull.Value && picId != null)
                                    {
                                        sTable.Append("<img src='" + PageUtils.GetEpgPicImageUrl(int.Parse(picId.ToString()), 90, 65));
                                        sTable.Append("'/>");
                                    }
                                }
                                sTable.Append("</td>");
                            }
                            else
                            {
                                int x = sFileName.LastIndexOf(".");
                                int y = sFileName.LastIndexOf("_");

                                if (x > 0 && y > 0 && x > y)
                                {
                                    string replace = sFileName.Substring(y, x - y);
                                    sFileName = sFileName.Replace(replace, "_tn");
                                }

                                sTable.AppendFormat("<td><img src='{0}'/></td>", sFileName);
                            }   
                        }
                        else
                        {
                            string sBP = sBasePicsURL;
                            if (m_theDataTable.Columns.Contains("group_id"))
                            {
                                object oGroupID = m_theDataTable.DefaultView[pageIndx].Row["group_id"];
                                if (oGroupID != DBNull.Value && oGroupID != null)
                                    sBP = PageUtils.GetBasePicURL(int.Parse(oGroupID.ToString()));
                            }

                            Int32 nPicID = PageUtils.GetDefaultPICID(nGroupID);
                            string sPicURL = "-";
                            if (nPicID != 0)
                                sPicURL = PageUtils.GetTableSingleVal("pics", "base_url", nPicID).ToString();
                            sTable.Append("<td ><img src='" + sBP);
                            if (sBP.EndsWith("=") == false)
                                sTable.Append("/");
                            string sP = ImageUtils.GetTNName(sPicURL, "tn");
                            if (sBP.EndsWith("=") == true)
                            {
                                string sTmp1 = "";
                                string[] s1 = sP.Split('.');
                                for (int j1 = 0; j1 < s1.Length - 1; j1++)
                                {
                                    if (j1 > 0)
                                        sTmp1 += ".";
                                    sTmp1 += s1[j1];
                                }
                                sP = sTmp1;
                            }
                            sTable.Append(sP + "'/></td>");
                        }
                    }
                    else if (m_OnOffFields.ContainsKey(sName.ToUpper()))
                    {
                        string sOnOffVal = m_OnOffFields[sName.ToUpper()].ToString();
                        string[] seperator = { "~~|~~" };
                        string[] splited = sOnOffVal.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                        string sOn = "On";
                        string sOff = "Off";
                        string sTableName = "Couchbase";
                        string sFieldName = "";
                        string sIndexField = "";
                        if (splited.Length == 5)
                        {
                            sTableName = splited[0].ToString();
                            sFieldName = splited[1].ToString();
                            sIndexField = splited[2].ToString();
                            sOn = splited[3].ToString();
                            sOff = splited[4].ToString();
                        }

                        Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row[sIndexField].ToString());
                        Int32 nActive = 0;
                        if (sValue != DBNull.Value && sValue != null)
                            nActive = int.Parse(sValue.ToString());

                        sTable.Append("<td nowrap id=\"activation_" + sFieldName + "_" + nID.ToString() + "\">");
                        if (nActive == 1)
                        {
                            sTable.Append("<b>" + sOn + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "','" + sIndexField + "'," + nID.ToString() + ",0 , '" + sOn + "','" + sOff + "' , '" + "" + "');\" ");
                            sTable.Append(" class='adm_table_link_div' >");
                            sTable.Append(sOff);
                            sTable.Append("</a>");
                        }
                        if (nActive == 0)
                        {
                            sTable.Append("<b>" + sOff + "</b> / <a href=\"javascript: ChangeOnOffStateRow('" + sTableName + "','" + sFieldName + "'," + nID.ToString() + "," + nID.ToString() + ",1 , '" + sOn + "','" + sOff + "' , '" + "" + "');\" ");
                            sTable.Append(" class='adm_table_link_div' >");
                            sTable.Append(sOn);
                            sTable.Append("</a>");
                        }
                        sTable.Append("</td>");
                    }
                    else if (m_HTMLFields.ContainsKey(sName.ToUpper()))
                    {
                        string sVal = sValue.ToString();
                        sTable.Append("<td >" + sVal + "</td>");
                    }
                    else if (m_AudioFields.ContainsKey(sName.ToUpper()))
                    {
                        string sFileName = "";
                        sFileName = "audio/" + ImageUtils.GetTNName(sValue.ToString(), "full");
                        if (sFileName != "")
                        {
                            sTable.Append("<td >");
                            sTable.Append("<IFRAME SRC=\"admin_player.aspx?player_type=audio&autoplay=false&audio_url=" + HttpContext.Current.Server.UrlEncode(sFileName.ToString()));
                            sTable.Append("\" WIDTH=\"50\" HEIGHT=\"22\" FRAMEBORDER=\"0\"></IFRAME>");
                            sTable.Append("</td>");
                        }
                        else
                            sTable.Append("<td >&nbsp;</td>");
                    }
                    else
                    {
                        string sVal = sValue.ToString();
                        sVal = ClearFromHTML(sVal);
                        sVal = sVal.Replace("\r\n", "<br/>");
                        if (sVal.Length > 50)
                        {
                            sVal = sVal.Substring(0, 50) + "...";
                        }
                        sVal = PageUtils.ReWriteTableValue(sVal);
                        sTable.Append("<td ");
                        if (sVal.Length < 22)
                            sTable.Append(" nowrap=\"nowrap\" ");
                        if (sVal.Length > 50)
                            sTable.Append(" alt=\"alt\" ");
                        sTable.Append(">" + sVal + "</td>");
                    }
                }
                if (m_bWithOrderNum == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row[m_sOrderNumFieldName].ToString());
                    Int32 nOrderNum = int.Parse(m_theDataTable.DefaultView[pageIndx].Row[m_sOrderNumFiledToChange].ToString());
                    sTable.Append("<td valign=\"top\" nowrap id=\"order_num_" + nID.ToString() + "\" align=\"center\" nowrap=\"nowrap\" style=\"margin: 0 0 0 0; padding: 0 0 0 0;\">");
                    sTable.Append("<table cellpadding=\"0\" cellspacing=\"0\" >");
                    sTable.Append("<tr>");
                    sTable.Append("<td align=\"right\" style=\"cursor: pointer;\"><img onmouseover=\"this.src='images/arrow_down-over.gif';\" onmouseout=\"this.src='images/arrow_down.gif';\"  src=\"images/arrow_down.gif\" onclick=\"javascript: ChangeOrderNumRow('" + m_sOrderNumTable + "'," + nID.ToString() + ",'" + m_sOrderNumFiledToChange.ToString() + "',-1 , '" + "" + "');\"/></td>");
                    sTable.Append("<td align=\"center\" nowrap=\"nowrap\">" + nOrderNum.ToString() + "</td>");
                    sTable.Append("<td align=\"left\" style=\"cursor: pointer;\"><img onmouseover=\"this.src='images/arrow_up-over.gif';\" onmouseout=\"this.src='images/arrow_up.gif';\" src=\"images/arrow_up.gif\" onclick=\"javascript: ChangeOrderNumRow('" + m_sOrderNumTable + "'," + nID.ToString() + ",'" + m_sOrderNumFiledToChange.ToString() + "',1 , '" + "" + "');\"/></td>");
                    sTable.Append("</tr>");
                    sTable.Append("</table>");
                    sTable.Append("</td>");
                }
                if (m_bWithOrderNum2 == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row[m_sOrderNumFieldName2].ToString());
                    Int32 nOrderNum = int.Parse(m_theDataTable.DefaultView[pageIndx].Row[m_sOrderNumFiledToChange2].ToString());
                    sTable.Append("<td valign=\"top\" nowrap id=\"" + m_sOrderNumFiledToChange2 + "_" + nID.ToString() + "\" align=\"center\" nowrap=\"nowrap\"  style=\"margin: 0 0 0 0; padding: 0 0 0 0;\">");
                    sTable.Append("<table cellpadding=\"0\" cellspacing=\"0\" >");
                    sTable.Append("<tr>");
                    sTable.Append("<td align=\"right\" style=\"cursor: pointer;\"><img onmouseover=\"this.src='images/arrow_down-over.gif';\" onmouseout=\"this.src='images/arrow_down.gif';\"  src=\"images/arrow_down.gif\" onclick=\"javascript: ChangeOrderNumRow('" + m_sOrderNumTable2 + "'," + nID.ToString() + ",'" + m_sOrderNumFiledToChange2.ToString() + "',-1 , '" + "" + "');\"/></td>");
                    sTable.Append("<td align=\"center\" nowrap=\"nowrap\">" + nOrderNum.ToString() + "</td>");
                    sTable.Append("<td align=\"left\" style=\"cursor: pointer;\"><img onmouseover=\"this.src='images/arrow_up-over.gif';\" onmouseout=\"this.src='images/arrow_up.gif';\" src=\"images/arrow_up.gif\" onclick=\"javascript: ChangeOrderNumRow('" + m_sOrderNumTable2 + "'," + nID.ToString() + ",'" + m_sOrderNumFiledToChange2.ToString() + "',1 , '" + "" + "');\"/></td>");
                    sTable.Append("</tr>");
                    sTable.Append("</table>");
                    sTable.Append("</td>");
                }
                if (m_bWithVideo == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["ID"].ToString());
                    DataRecordMediaViewerField dv = new DataRecordMediaViewerField("", nID);
                    dv.VideoTable(m_sVideoTable);
                    dv.Initialize("Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                    string sFLV = dv.GetPlayerSmallFrame();
                    sTable.Append("<td>");
                    sTable.Append(sFLV);
                    sTable.Append("</td>");
                }
                for (int i = 0; i < m_MultiValuesColumns.Count; i++)
                {
                    string sFieldRefName = ((DataTableMultiValuesColumn)(m_MultiValuesColumns[i.ToString()])).GetsParentRefColumnName();
                    object sRefVal = m_theDataTable.DefaultView[pageIndx].Row[sFieldRefName];
                    string sValue = ((DataTableMultiValuesColumn)(m_MultiValuesColumns[i.ToString()])).GetResultString(sRefVal, ",");
                    sTable.Append("<td>");
                    sTable.Append(sValue);
                    sTable.Append("</td>");
                }
                for (int i = 0; i < m_LinkColumns.Count; i++)
                {
                    string sURL = GetLinkURL(i, pageIndx);
                    string sTarget = GetLinkTarget(i, pageIndx);
                    string sInner = GetLinkInner(i, pageIndx);
                    string sCounterStr = GetLinkCountText(i, pageIndx);
                    sTable.Append("<td nowrap>");
                    if (sURL != "")
                    {
                        string sText = ((DataTableLinkColumn)m_LinkColumns[i.ToString()]).GetColumnText();
                        string sDynamicTextQuery = ((DataTableLinkColumn)m_LinkColumns[i.ToString()]).GetColumnTextDynamicQuery();
                        if (sDynamicTextQuery != "")
                        {
                            Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["ID"].ToString());
                            sDynamicTextQuery += nID.ToString();
                            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                            //if (m_sConnectionKey != "")
                            //    selectQuery.SetConnectionKey(m_sConnectionKey);
                            selectQuery += sDynamicTextQuery;
                            if (selectQuery.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                if (nCount1 > 0)
                                {
                                    sText = selectQuery.Table("query").DefaultView[0].Row["txt"].ToString();
                                }
                            }
                            selectQuery.Finish();
                            selectQuery = null;
                        }
                        if (sText != "")
                        {
                            if (sText.Trim().ToLower() != "editor remarks" &&
                                sText.Trim().ToLower() != "cancel" &&
                                sText.Trim().ToLower() != "delete" &&
                                sText.Trim().ToLower() != "confirm" &&
                                sText.Trim().ToLower() != "edit" &&
                                sText.Trim().ToLower() != "collection" &&
                                sText.Trim().ToLower() != "statistics")
                            {
                                sTable.Append("<a " + sInner + " href='");
                                sTable.Append(sURL);
                                sTable.Append("' class='");
                                sTable.Append(m_sLinkCss);
                                sTable.Append("' target='");
                                sTable.Append(sTarget);
                                sTable.Append("' >");
                                sTable.Append(sText);
                                sTable.Append("</a>" + sCounterStr);
                            }
                            else
                            {
                                string sImgeURL = "";
                                string sImgeURLOver = "";
                                if (sText.Trim().ToLower() == "editor remarks")
                                {
                                    sImgeURL = "images/info_btn.gif";
                                    sImgeURLOver = "images/info_btn-over.gif";
                                }
                                if (sText.Trim().ToLower() == "cancel")
                                {
                                    sImgeURL = "images/cancel_btn.gif";
                                    sImgeURLOver = "images/cancel_btn-over.gif";
                                }
                                if (sText.Trim().ToLower() == "delete")
                                {
                                    sImgeURL = "images/delete_btn.gif";
                                    sImgeURLOver = "images/delete_btn-over.gif";
                                }
                                if (sText.Trim().ToLower() == "confirm")
                                {
                                    sImgeURL = "images/confirm_btn.gif";
                                    sImgeURLOver = "images/confirm_btn-over.gif";
                                }
                                if (sText.Trim().ToLower() == "edit")
                                {
                                    sImgeURL = "images/edit_btn.gif";
                                    sImgeURLOver = "images/edit_btn-over.gif";
                                }
                                if (sText.Trim().ToLower() == "collection")
                                {
                                    sImgeURL = "images/gedit_icon_02.png";
                                    sImgeURLOver = "images/gedit_icon_02-over.png";
                                }
                                if (sText.Trim().ToLower() == "statistics")
                                {
                                    sImgeURL = "images/stat_btn.gif";
                                    sImgeURLOver = "images/stat_btn-over.gif";
                                }
                                sTable.Append("<img style='cursor: pointer;' src='");
                                sTable.Append(sImgeURL);
                                sTable.Append("' onmouseover='this.src=\"" + sImgeURLOver + "\"' onmouseout='this.src=\"" + sImgeURL + "\"' onclick='document.location.href=\"");
                                sTable.Append(sURL);
                                sTable.Append("\"'");
                                sTable.Append(" />");
                                sTable.Append(sCounterStr);
                            }
                        }
                        else
                            sTable.Append("&nbsp;&nbsp;&nbsp;");
                    }
                    sTable.Append("</td>");
                }
                if (m_bWithActivation == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["ID"].ToString());
                    Int32 nActive = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["IS_ACTIVE"].ToString());
                    sTable.Append("<td nowrap id=\"activation_" + nID.ToString() + "\">");
                    if (nActive == 1)
                    {
                        sTable.Append("<b>On</b> / <a href=\"javascript: ChangeActiveStateRow('" + m_sActivationTable + "'," + nID.ToString() + ",0,'" + "" + "');\" ");
                        sTable.Append(" class='adm_table_link_div' >");
                        sTable.Append("Off");
                        sTable.Append("</a>");
                    }
                    if (nActive == 0)
                    {
                        sTable.Append("<b>Off</b> / <a href=\"javascript: ChangeActiveStateRow('" + m_sActivationTable + "'," + nID.ToString() + ",1,'" + "" + "');\" ");
                        sTable.Append(" class='adm_table_link_div' >");
                        sTable.Append("On");
                        sTable.Append("</a>");
                    }
                    sTable.Append("</td>");
                }
                if (m_bWithEditorRemarks == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["ID"].ToString());
                    string sRemarks = m_theDataTable.DefaultView[pageIndx].Row["editor_remarks"].ToString();
                    sTable.Append("<td nowrap>");
                    sTable.Append("<img style='cursor: pointer;' src='");
                    sTable.Append("images/info_btn.gif");
                    sTable.Append("' onmouseover='javascript:openLocalWindow(\"" + HttpContext.Current.Server.HtmlEncode(sRemarks.Replace("\r\n", "<br\\>").Replace("\"", "").Replace("'", "").Trim()) + "\");' onclick='return false;' onmouseout='closeCollDiv(\"\");'");
                    sTable.Append(" />");
                    sTable.Append("</td>");
                }
                if (m_bWithTechDetails == true)
                {
                    Int32 nID = int.Parse(m_theDataTable.DefaultView[pageIndx].Row["ID"].ToString());
                    string sUpdaterName = "";
                    string sCreateSate = "";
                    string sUpdateSate = "";
                    string sPublishSate = "";
                    sTable.Append("<td nowrap>");

                    sTable.Append("<img style='cursor: pointer;' src='");
                    sTable.Append("images/history_btn.gif");
                    sTable.Append("' onmouseover='javascript:openTechDetails(\"" + sUpdaterName + "\",\"" + sCreateSate + "\",\"" + sUpdateSate + "\",\"" + sPublishSate + "\");' onmouseout='closeCollDiv(\"\");' onclick='return false;'");
                    sTable.Append(" />");
                    sTable.Append("</td>");
                }
                sTable.Append("</tr>");
            }
            sTable.Append("</table>");
            if (nCounter >= 20)
                sTable.Append(GetTopLine(nPages, nStartPage, nEndPage, nPageNum, sOrderBy));
            HttpContext.Current.Session["order_by"] = sOrderBy;
            return sTable.ToString();
        }

        protected string GetTopLine(Int32 nPages, Int32 nStartPage, Int32 nEndPage, Int32 nPageNum, string sOrderBy)
        {
            string sTable = "";
            sTable += "<table class=\"adm_table_pager_white\" cellspacing=\"1\" cellpadding=\"0\">";
            sTable += "<tr height=20px ><td class=\"pagerTd01\"><table ><tr>";
            if (m_bNewButton)
            {
                if (m_sNewStr == "")
                    sTable += "<td><a class=\"btn_new\" href=\"" + GetNewPageURL() + "\"></a></td>";
                else
                    sTable += "<td><a href=\"javascript: void(0);\" class=\"menuanchorclass\" rel=\"anylinkmenu1\" data-image=\"images/new01.png\" data-overimage=\"images/new02.png\"><img src=\"images/new01.png\" style=\"border-width:0\" /></a></td>";

            }
            if (m_bPrintButton)
            {
                sTable += "<td style=\"CURSOR: pointer\" onclick=\"window.print();\" valign=\"middle\"><img id=\"print_img\" alt=\"Print\" src=\"images/icon_print_normal.gif\" /></td>";
            }
            if (m_bExcellButton)
            {
                sTable += "<td style=\"CURSOR: pointer\" onclick=\"create_csv();\" valign=\"middle\"><img id=\"excell_img\" alt=\"Excell\" src=\"images/icon_export_normal.gif\" /></td>";
            }
            sTable += "</tr></table></td>";
            if (nPages > 1)
            {
                sTable += "<td class=\"pagerTd03\"  nowrap=\"nowrap\">";
                sTable += "<table>";
                sTable += "<tr>";
                sTable += "<td>Page:</td>";
                sTable += "<td><input size=\"4\" type=\"text\" dir=\"ltr\" class=\"FormInputSmall\" id=\"paging_goto\" value=\"" + (nPageNum).ToString() + "\" /></td>";
                sTable += "<td><img style='cursor:pointer;' onmouseover='this.src=\"images/button_next_over.gif\";' onmouseout='this.src=\"images/button_next_normal.gif\";' alt='Go' onclick='getPagingGoto(\"" + sOrderBy + "\");' src='images/button_next_normal.gif' /></td>";
                sTable += "</tr>";
                sTable += "</table>";
                sTable += "</td>";
            }
            sTable += "<td class=\"pagerTd02\" width=\"100%\" nowrap=\"nowrap\">";
            if (nPages > 1)
            {
                sTable += "<table align=\"center\" dir=\"ltr\"><tr>";
                Int32 nStart = nPageNum - 4;
                if (nStart < 0)
                    nStart = 0;
                Int32 nEnd = nStart + 5;
                if (nEnd > nPages)
                    nEnd = nPages;
                if (nStart > 0)
                    sTable += "<td class='pagelinks' onclick='javascript:GetPageTable(\"" + sOrderBy + "\",1);' onmouseover='this.className=\"pagelinks_over\";' onmouseout='this.className=\"pagelinks\";'>1</td>";
                if (nStart > 1)
                    sTable += "<td>...</td>";
                for (int i = nStart; i < nEnd; i++)
                {
                    if (i != nPageNum - 1)
                        sTable += "<td nowrap=\"nowrap\" width=12px class='pagelinks' onclick='javascript:GetPageTable(\"" + sOrderBy + "\"," + (i + 1).ToString() + ");' onmouseover='this.className=\"pagelinks_over\";' onmouseout='this.className=\"pagelinks\";'>" + (i + 1).ToString() + "</td>";
                    else
                        sTable += "<td nowrap=\"nowrap\" width=12px class=\"pagelinks_sel\" >" + (i + 1).ToString() + "</td>";
                }
                if (nEnd < nPages - 1)
                    sTable += "<td>...</td>";
                if (nEnd < nPages)
                    sTable += "<td nowrap=\"nowrap\" class='pagelinks' onclick='javascript:GetPageTable(\"" + sOrderBy + "\"," + nPages.ToString() + ");' onmouseover='this.className=\"pagelinks_over\";' onmouseout='this.className=\"pagelinks\";'>" + nPages.ToString() + "</td>";

                sTable += "</tr></table>";
            }
            sTable += "</td>";
            sTable += "<td class=\"pagerTd03\">";
            sTable += "<table>";
            sTable += "<tr>";
            sTable += "<td><img ";
            if (nPageNum != 1)
                sTable += "style='cursor:pointer;' onmouseover='this.src=\"images/button_prev_over.gif\";' onmouseout='this.src=\"images/button_prev_normal.gif\";' alt='לדף הקודם' onclick='GetPageTable(\"" + sOrderBy + "\"," + (nPageNum - 1).ToString() + ");' src='images/button_prev_normal.gif' /></td>";
            else
                sTable += "src=\"images/button_prev_dis.gif\" /></td>";
            sTable += "<td nowrap>&nbsp;&nbsp;";
            sTable += nStartPage.ToString() + "-" + (nEndPage).ToString() + " Out of " + /*m_nQueryCount.ToString() +*/ " Records";
            sTable += "&nbsp;&nbsp;</td>";
            sTable += "<td ><img ";
            if (nPageNum != nPages)
                sTable += " style=\"CURSOR: pointer\" onclick='GetPageTable(\"" + sOrderBy + "\"," + (nPageNum + 1).ToString() + ");' alt=\"Next page\" src=\"images/button_next_normal.gif\" /></td>";
            else
                sTable += " src='images/button_next_dis.gif' /></td>";
            sTable += "</tr>";
            sTable += "</table></td>";

            sTable += "</tr></table>";
            return sTable;
        }

        protected string GetNewPageURL()
        {
            string sURL = HttpContext.Current.Request.FilePath.ToString();
            Int32 nStart = 0;
            Int32 nEnd = sURL.LastIndexOf('.');
            string sPage = sURL.Substring(nStart, nEnd - nStart);
            sPage += "_new.aspx";
            if (m_sNewParameters != "")
            {
                sPage += "?";
                sPage += m_sNewParameters;
            }
            return sPage;
        }

        protected string GetLinkURL(Int32 nLinkColumnIndex, Int32 nRowNum)
        {
            string sURL = ((DataTableLinkColumn)m_LinkColumns[nLinkColumnIndex.ToString()]).GetLinkURL();
            string sIfStatement = ((DataTableLinkColumn)m_LinkColumns[nLinkColumnIndex.ToString()]).GetIfStatement();
            if (sIfStatement != "")
            {
                string[] sSplited = sIfStatement.Split(';');
                Int32 nCount = sSplited.Length;
                bool bCont = false;
                for (int i = 0; i < nCount; i++)
                {
                    string sSplitPart = sSplited[i].ToString();
                    string[] s2Parts = sSplitPart.Split('=');
                    string sColumnName = s2Parts[0].ToString();
                    string sColumnVal = s2Parts[1].ToString();
                    string sValToCompare = m_theDataTable.DefaultView[nRowNum].Row[sColumnName].ToString();
                    if (sValToCompare == sColumnVal)
                        bCont = true;
                }
                if (bCont == false)
                    return "";
            }
            System.Collections.Specialized.NameValueCollection nvc = ((DataTableLinkColumn)m_LinkColumns[nLinkColumnIndex.ToString()]).GetQueryString();
            if (nvc.Count > 0)
            {
                Int32 nJS = sURL.ToLower().IndexOf("javascript");
                if (nJS == -1)
                    sURL += "?";
                else
                    sURL += "(";
                for (int i = 0; i < nvc.Count; i++)
                {
                    if (i > 0)
                    {
                        if (nJS == -1)
                            sURL += "&";
                        else
                            sURL += ",";
                    }
                    if (nJS == -1)
                    {
                        sURL += nvc.GetKey(i);
                        sURL += "=";
                    }
                    string sVal = nvc.Get(i);
                    if (sVal.StartsWith("field="))
                    {
                        sVal = sVal.Substring(6);
                        Int32 nRowsCount = m_theDataTable.Columns.Count;
                        for (int j = 0; j < nRowsCount; j++)
                        {
                            string sName = "";
                            object sValue = null;
                            sName = m_theDataTable.Columns[j].ColumnName.ToString();
                            sValue = m_theDataTable.DefaultView[nRowNum].Row[sName].ToString();
                            if (sName.ToUpper() == sVal.ToUpper())
                            {
                                sURL += sValue;
                                break;
                            }
                        }
                    }
                    else
                        sURL += sVal;
                }
                if (nJS != -1)
                    sURL += ");";
            }
            return sURL;
        }

        protected string GetLinkTarget(Int32 nLinkColumnIndex, Int32 nRowNum)
        {
            string sTarget = ((DataTableLinkColumn)m_LinkColumns[nLinkColumnIndex.ToString()]).GetLinkTarget();

            return sTarget;
        }

        protected string GetLinkInner(Int32 nLinkColumnIndex, Int32 nRowNum)
        {
            string sTarget = ((DataTableLinkColumn)m_LinkColumns[nLinkColumnIndex.ToString()]).GetLinkInner();

            return sTarget;
        }

        protected string GetLinkCountText(Int32 nLinkColumnIndex, Int32 nRowNum)
        {
            //NOT IMPLEMENT
            string sRet = "";
            return sRet;
        }

        static public string ClearFromHTML(string sToClean)
        {
            string sCleaned = sToClean;
            bool bCont = true;
            while (bCont == true)
            {
                Int32 nLoc = sCleaned.IndexOf('<');
                Int32 nLocEnd = sCleaned.IndexOf('>');
                if (nLoc > -1 && nLocEnd > -1 && nLocEnd > nLoc)
                {
                    string sToRemove = sCleaned.Substring(nLoc, nLocEnd - nLoc + 1);
                    sCleaned = sCleaned.Replace(sToRemove, "");
                }
                else
                    bCont = false;
            }
            return sCleaned;
        }

        public void Finish()
        {
            if (m_theDataTable != null)
            {
                m_theDataTable.Dispose();
                m_theDataTable = null;
            }
        }

        public string OpenCSV()
        {
            if (m_theDataTable == null)
            {
                return "";
            }

            GridView gv = new GridView();

            gv.DataSource = m_theDataTable;
            gv.DataBind();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            gv.RenderControl(htmlWrite);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
            return "";
        }
    }
}
