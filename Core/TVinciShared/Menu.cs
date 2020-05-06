using System;
using System.Web;
using System.Text;
using KLogMonitor;
using System.Reflection;
using System.Xml;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for Menu
    /// </summary>
    public class Menu
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Menu()
        {
        }

        public static string GetFirstLink()
        {
            string sRet = "logout.aspx";
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select aap.view_permit,am.* from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and parent_menu_id=0 and aap.view_permit=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            if (nGroupID > 1)
                selectQuery += " and ONLY_TVINCI=0 ";
            else
                selectQuery += " and ONLY_CO=0 ";
            selectQuery += " order by menu_order_vis";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet = selectQuery.Table("query").DefaultView[0].Row["menu_href"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sRet != "")
                return sRet;
            else
            {
                if (nGroupID != 1)
                    return "adm_media.aspx";
                else
                    return "adm_groups.aspx";
            }
        }

        static protected Int32 GetOriginalOrderID(Int32 nOrder)
        {
            Int32 nRet = 0;
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select aap.view_permit,am.* from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and parent_menu_id=0 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            //if (nGroupID > 1)
            //selectQuery += " and am.ONLY_TVINCI=0 ";
            //else
            //selectQuery += " and am.ONLY_CO=0 ";
            selectQuery += " order by menu_order";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount >= nOrder)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[nOrder - 1].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected void GetMenuLevel(Int32 nMenuID, ref Int32 nLevel)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select parent_menu_id from admin_menu where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMenuID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nParentID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["parent_menu_id"].ToString());
                    if (nParentID != 0)
                    {
                        nLevel++;
                        GetMenuLevel(nParentID, ref nLevel);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected Int32 GetOriginalMenuIDByURL(Int32 nParentID, string sURL, bool bRemoveQuery)
        {
            if (sURL == "")
                sURL = HttpContext.Current.Request.GetUrl().PathAndQuery;
            Int32 nL = sURL.LastIndexOf("/");
            if (nL != -1)
                sURL = sURL.Substring(nL + 1);

            sURL = sURL.Replace("_new", "");
            sURL = sURL.Replace("_translate", "");
            if (bRemoveQuery == true)
            {
                string sQuery = HttpContext.Current.Request.GetUrl().Query;
                if (sQuery != "")
                    sURL = sURL.Replace(sQuery, "");
            }

            Int32 nRet = 0;
            Int32 nLevel = 0;
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select aap.view_permit,am.* from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            //string sLike = " (am.menu_href like ('" + sURL + "%') or am.menu_href like ('" + sOldURL + "%'))";
            selectQuery += " and ";

            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("am.menu_href", "=", sURL);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nCurrent = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    Int32 nCurrentLevel = 0;
                    GetMenuLevel(nCurrent, ref nCurrentLevel);
                    if (nCurrentLevel > nLevel && nCurrent != 0)
                    {
                        nRet = nCurrent;
                        nLevel = nCurrentLevel;
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetVisualOrderID(Int32 nOrder)
        {
            Int32 nRet = 0;
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select aap.view_permit,am.* from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and parent_menu_id=0 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            if (nGroupID > 1)
                selectQuery += " and am.ONLY_TVINCI=0 ";
            else
                selectQuery += " and am.ONLY_CO=0 ";
            selectQuery += " order by menu_order_vis";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount >= nOrder)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[nOrder - 1].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetOriginalSubOrderID(Int32 nParentID, Int32 nOrder)
        {
            Int32 nRet = 0;
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select am.*,aap.view_permit from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("am.parent_menu_id", "=", nParentID);
            //if (nGroupID > 1)
            //selectQuery += " and am.ONLY_TVINCI=0 ";
            //else
            //selectQuery += " and am.ONLY_CO=0 ";
            selectQuery += " order by menu_order";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount >= nOrder)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[nOrder - 1].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        public static string GetMainMenu(Int32 nMenuID, bool bAdmin, ref Int32 nSelID)
        {
            return GetNewMainMenu(nMenuID, bAdmin, ref nSelID, "");
        }

        public static string GetMainMenu(Int32 nMenuID, bool bAdmin, ref Int32 nSelID, string sPageURL)
        {
            string sXML = "<root>" + GetMainMenu(ref nMenuID, bAdmin, ref nSelID, 0, sPageURL) + "</root>";

            StringBuilder sTemp = new StringBuilder();

            sTemp.Append("<script type=\"text/javascript\" src=\"js/SWFObj.js\"></script><script  type=\"text/javascript\">");
            sTemp.Append("function menuXML()");
            sTemp.Append("{");
            sTemp.Append("return '").Append(sXML).Append("';");
            sTemp.Append("}");
            sTemp.Append("function changeMenuHeight(newHeight) ");
            sTemp.Append("{");
            sTemp.Append("e = document.getElementById(\"menu_holder\");");
            sTemp.Append("if(newHeight<200)newHeight=200;");
            sTemp.Append("e.style.height = newHeight + 'px';");
            sTemp.Append("}");
            sTemp.Append("var flashObj = new SWFObj");
            sTemp.Append("(");
            sTemp.Append("'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0',");
            sTemp.Append("'width', '100%',");
            sTemp.Append("'height', '100%',");
            sTemp.Append("'src', 'flash/amin_tree_menu',");
            sTemp.Append("'quality', 'high',");
            sTemp.Append("'pluginspage', 'http://www.macromedia.com/go/getflashplayer',");
            sTemp.Append("'align', 'left',");
            sTemp.Append("'scale', 'showall',");
            sTemp.Append("'devicefont', 'false',");
            sTemp.Append("'id', 'amin_tree_menu',");
            sTemp.Append("'bgcolor', '#ffffff',");
            sTemp.Append("'wmode', 'transparent',");
            sTemp.Append("'name', 'amin_tree_menu',");
            sTemp.Append("'menu', 'true',");
            sTemp.Append("'allowFullScreen', 'true',");
            sTemp.Append("'allowScriptAccess','sameDomain',");
            sTemp.Append("'movie', 'flash/amin_tree_menu',");
            sTemp.Append("'salign', '',");
            sTemp.Append("'flashVars', 'data_request_function=menuXML'");
            sTemp.Append("); //end AC code");
            sTemp.Append("</script>");
            sTemp.Append("<tr><td><div class=\"left_menu\" id=\"menu_holder\" name=\"menu_holder\"></div></td><tr>");
            sTemp.Append("<script  type=\"text/javascript\">");
            sTemp.Append("flashObj.write('menu_holder');");
            sTemp.Append("</script>");
            return sTemp.ToString();
        }

        public static string GetNewMainMenu(Int32 nMenuID, bool bAdmin, ref Int32 nSelID, string sPageURL)
        {
            string sXML = "<root>" + GetMainMenu(ref nMenuID, bAdmin, ref nSelID, 0, sPageURL) + "</root>";

            XmlDocument xmld = new XmlDocument();
            xmld.LoadXml(sXML);

            StringBuilder mainStr = new StringBuilder();
            mainStr.Append("<tr><td><div id=\"menu\"><ul>");

            foreach (XmlElement item in xmld.ChildNodes[0].ChildNodes)
            {
                AppendItemToMenu(item, mainStr);
            }

            mainStr.Append("</div></ul></td></tr>");

            var a = mainStr.ToString();

            return a;
        }

        public static string GetMainMenu(ref Int32 nMenuID, bool bAdmin, ref Int32 nSelID, Int32 nParentID)
        {
            return GetMainMenu(ref nMenuID, bAdmin, ref nSelID, nParentID, "");

        }

        public static bool IsLayoutManagerVisible(int groupID)
        {
            bool retVal = false;
            for (int i = 1; i < 7; i++)
            {
                string sConn = "tvp_connection_" + groupID.ToString() + "_" + i.ToString();
                if (!string.IsNullOrEmpty(WS_Utils.GetTcmConfigValue(sConn)))
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }

        public static string GetMainMenu(ref Int32 nMenuID, bool bAdmin, ref Int32 nSelID, Int32 nParentID, string sPageURL)
        {
            if (nParentID == 0)
            {
                nSelID = 0;
                nMenuID = GetOriginalMenuIDByURL(nParentID, sPageURL, false);
                if (nMenuID == 0)
                    nMenuID = GetOriginalMenuIDByURL(nParentID, sPageURL, true);
            }
            StringBuilder sXML = new StringBuilder();
            Int32 nCount = 0;
            Int32 nAcctID = LoginManager.GetLoginID();
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select aap.view_permit,am.* from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_menu_id", "=", nParentID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            selectQuery += " order by menu_order_vis";
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                for (Int32 i = 0; i < nCount; i++)
                {
                    Int32 nGroupHeader = int.Parse(selectQuery.Table("query").DefaultView[i].Row["IS_GROUP_HEADER"].ToString());
                    Int32 nBelongsToGroup = int.Parse(selectQuery.Table("query").DefaultView[i].Row["BELONG_TO_GROUP"].ToString());
                    Int32 nAMID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    Int32 nVis = int.Parse(selectQuery.Table("query").DefaultView[i].Row["menu_order_vis"].ToString());
                    Int32 nOnlyTV = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ONLY_TVINCI"].ToString());
                    Int32 nOnlyCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ONLY_CO"].ToString());
                    bool bVisible = true;
                    if (int.Parse(selectQuery.Table("query").DefaultView[i].Row["view_permit"].ToString()) == 0)
                        bVisible = false;
                    if (selectQuery.Table("query").DefaultView[i].Row["menu_text"].ToString() == "")
                        bVisible = false;
                    if (nGroupID > 1 && nOnlyTV == 1)
                        bVisible = false;
                    if (nGroupID == 1 && nOnlyCO == 1)
                        bVisible = false;
                    string menuText = selectQuery.Table("query").DefaultView[i].Row["menu_text"].ToString();
                    if (!string.IsNullOrEmpty(menuText) && menuText.ToLower().Contains("media store"))
                    {
                        bVisible = IsLayoutManagerVisible(nGroupID);
                    }
                    if (bVisible == true)
                    {
                        string sSelected = "false";
                        if (nMenuID == nAMID)
                        {
                            nSelID = nMenuID;
                            sSelected = "true";
                        }

                        sXML.Append("<node label=\"");
                        if (nParentID == 0)
                            sXML.Append(selectQuery.Table("query").DefaultView[i].Row["menu_text"].ToString().ToUpper());
                        else
                            sXML.Append(selectQuery.Table("query").DefaultView[i].Row["menu_text"].ToString());
                        string sUrl = selectQuery.Table("query").DefaultView[i].Row["menu_href"].ToString();
                        if (sUrl != "")
                        {                            
                            sUrl = sUrl.Replace("&", "&amp;");
                            sUrl = "[navigateURL::" + sUrl + "]";
                        }
                        sXML.Append("\" selected=\"").Append(sSelected).Append("\" actions=\"").Append(sUrl).Append("\" >");
                        sXML.Append(GetMainMenu(ref nMenuID, bAdmin, ref nSelID, nAMID, sPageURL));
                        sXML.Append("</node>");
                    }
                }
            }
            else
            {
            }
            selectQuery.Finish();
            selectQuery = null;
            return sXML.ToString();
        }

        public static string GetSubMenu(Int32 nTopOrder, Int32 nSubOrder, bool bAdmin)
        {
            return "";
            //try
            //{
            //    //nSubOrder = GetOriginalSubOrderID(nTopOrder, nSubOrder);
            //    Int32 nParent = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("admin_menu", "parent_menu_id", nTopOrder).ToString());
            //    StringBuilder sTemp = new StringBuilder();
            //    Int32 nCount = 0;
            //    Int32 nAcctID = LoginManager.GetLoginID();
            //    Int32 nGroupID = LoginManager.GetLoginGroupID();
            //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //    selectQuery += "select am.*,aap.view_permit from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and ";
            //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", nAcctID);
            //    selectQuery += "and ";
            //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("am.parent_menu_id", "=", nParent);
            //    selectQuery += " order by menu_order_vis";
            //    if (selectQuery.Execute("query", true) != null)
            //    {
            //        nCount = selectQuery.Table("query").DefaultView.Count;
            //        for (Int32 i = 0; i < nCount; i++)
            //        {
            //            if (int.Parse(selectQuery.Table("query").DefaultView[i].Row["view_permit"].ToString()) == 0)
            //                continue;
            //            Int32 nOnlyTV = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ONLY_TVINCI"].ToString());
            //            Int32 nOnlyCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ONLY_CO"].ToString());
            //            if (nGroupID > 1 && nOnlyTV == 1)
            //                continue;
            //            if (nGroupID == 1 && nOnlyCO == 1)
            //                continue;
            //            //if (nSubOrder == int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()))
            //            if (nTopOrder == int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()))
            //            {
            //                sTemp.Append("<li><a class=\"on\" href=\"");
            //            }
            //            else
            //            {
            //                sTemp.Append("<li><a href=\"");
            //            }
            //            sTemp.Append(selectQuery.Table("query").DefaultView[i].Row["menu_href"].ToString());
            //            sTemp.Append("\"><span>");
            //            sTemp.Append(selectQuery.Table("query").DefaultView[i].Row["menu_text"].ToString());
            //            sTemp.Append("</span></a></li>");
            //        }
            //        //sTemp += "<li class=\"red\">* Requiered fields</li>";
            //    }
            //    selectQuery.Finish();
            //    selectQuery = null;
            //    return sTemp.ToString();
            //}
            //catch
            //{
            //    HttpContext.Current.Response.Redirect("login.aspx");
            //    return "";
            //}
        }

        public static string GetSubMenu(System.Collections.SortedList sortedMenu, Int32 nSubOrder, bool bFixSize)
        {
            try
            {
                StringBuilder sTemp = new StringBuilder();
                Int32 nCount = 0;
                Int32 i = 0;
                System.Collections.IDictionaryEnumerator iter = sortedMenu.GetEnumerator();
                bool bFirst = true;
                while (iter.MoveNext())
                {
                    string[] splitted = iter.Value.ToString().Split('|');

                    if (bFirst == true)
                        sTemp.Append("<table width=100%><tr><td width=100% nowrap><table width=100% align=right><tr>");
                    bFirst = false;
                    if (i == 8)
                        sTemp.Append("</tr><tr>");
                    if (nSubOrder == i)
                    {
                        sTemp.Append("<td ");
                        if (bFixSize == true)
                            sTemp.Append("width=120px ");
                        sTemp.Append("nowrap valign=middle class=categorymenu_sel_outer>");
                        sTemp.Append("&nbsp;&nbsp;");
                        sTemp.Append("<a class=btn href='");
                    }
                    else
                    {
                        sTemp.Append("<td ");
                        if (bFixSize == true)
                            sTemp.Append("width=120px ");
                        sTemp.Append("nowrap valign=middle class=categorymenu_outer ");
                        //sTemp += "onmouseover=\"document.getElementById('submenu_";
                        //sTemp += iter.Key.ToString();
                        //sTemp += "').className='subcategorymenu_sel';this.className='categorymenu_sel_outer';\" onmouseout=\"document.getElementById('submenu_";
                        //sTemp += iter.Key.ToString();

                        //sTemp += "').className='subcategorymenu';this.className='categorymenu_outer';\"";
                        sTemp.Append(" onclick=\"location.href='");
                        sTemp.Append(splitted[1].ToString());
                        sTemp.Append("';\">");
                        sTemp.Append("&nbsp;&nbsp;");

                        sTemp.Append("<a id='submenu_");
                        sTemp.Append(iter.Key.ToString());

                        if (splitted[0] == "Add A.Channel")
                        {
                            sTemp.Append("' class=btn_a_channel href='");
                        }
                        else if (splitted[0] == "Add M.Channel")
                        {
                            sTemp.Append("' class=btn_m_channel href='");
                        }
                        else if (splitted[0] == "Add KSQL.Channel")
                        {
                            sTemp.Append("' class=btn_ksql_channel href='");
                        }
                        else if (splitted[0] == "Add A.UserList")
                        {
                            sTemp.Append("' class=btn_a_user_list href='");
                        }
                        else if (splitted[0] == "Add M.UserList")
                        {
                            sTemp.Append("' class=btn_m_user_list href='");
                        }
                        else
                        {
                            sTemp.Append("' class=btn href='");
                        }
                    }
                    sTemp.Append(splitted[1].ToString());

                    sTemp.Append("'>");

                    if (splitted[0] != "Add A.Channel" &&
                        splitted[0] != "Add M.Channel" &&
                        splitted[0] != "Add KSQL.Channel" &&
                        splitted[0] != "Add A.UserList" &&
                        splitted[0] != "Add M.UserList")
                    {
                        sTemp.Append(splitted[0]);
                    }

                    sTemp.Append("</a>&nbsp;&nbsp;</td>");
                    i++;
                }

                sTemp.Append("<td width=100% nowrap></td></tr></table></td></tr>");

                if (nCount > 0)
                    sTemp.Append("</table>");
                return sTemp.ToString();
            }
            catch
            {
                HttpContext.Current.Response.Redirect("login.aspx");
                return "";
            }
        }

        private static void AppendItemToMenu(XmlElement menuItem, StringBuilder mainStr)
        {
            SetMenuItemVariable(menuItem, out string name, out string path, out bool isActive, out bool isParentSelected);

            if (menuItem.HasChildNodes)
            {
                StringBuilder newNestedLi = new StringBuilder();
                if (isParentSelected)
                {
                    newNestedLi.Append("<li><span class ='caret caret-down' OnClick=\"javascript:expand(this)\"' ></span>");
                }
                else
                {
                    newNestedLi.Append("<li><span class ='caret' OnClick=\"javascript:expand(this)\"' ></span>");
                }

                newNestedLi.Append($"<span OnClick=\"javascript:expand(this)\">{name}</span>");

                StringBuilder nestedUl = new StringBuilder();
                if (isActive || isParentSelected)
                {
                    nestedUl.Append("<ul class='nested active'>");
                }
                else
                {
                    nestedUl.Append("<ul class='nested'>");
                }

                foreach (XmlElement child in menuItem.ChildNodes)
                {
                    AppendItemToMenu(child, nestedUl);
                }
                nestedUl.Append("</ul>");
                newNestedLi.Append(nestedUl).Append("</li>");
                mainStr.Append(newNestedLi);
            }
            else
            {
                string newLi = string.Empty;
                if (isActive)
                {
                    mainStr.Remove(mainStr.ToString().Length - 2, 2);
                    mainStr.Append(" active'>");
                    newLi = $"<li class='no-children selected' OnClick=\"javascript:window.location.href='{path}'\">{name }</li>";
                }
                else
                {
                    newLi = $"<li class='no-children' OnClick=\"javascript:window.location.href='{path}'\">{name }</li>";
                }

                mainStr.Append(newLi);
            }
        }

        private static void SetMenuItemVariable(XmlElement item, out string name, out string path, out bool isActive, out bool isParentSelected)
        {
            name = string.Empty;
            path = string.Empty;
            isActive = false;
            isParentSelected = false;

            if (item.Attributes["label"] != null)
            {
                name = item.Attributes["label"].Value;
            }

            if (item.Attributes["actions"] != null && !string.IsNullOrEmpty(item.Attributes["actions"].Value))
            {
                path = item.Attributes["actions"].Value;
                path = path.Remove(path.Length - 1, 1).Replace("[navigateURL::", "").Replace("&amp;", "&");
            }

            if (item.Attributes["selected"] != null)
            {
                isActive = Convert.ToBoolean(item.Attributes["selected"].Value);
            }

            if (item.InnerXml.Contains("selected=\"true\""))
            {
                isParentSelected = true;
            }
        }
    }
}