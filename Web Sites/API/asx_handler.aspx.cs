using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Collections;

public partial class asx_handler : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string sUserAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            string sMid = Request.QueryString["mid"];
            if (String.IsNullOrEmpty(sMid) == true)
                sMid = "";
            string sPre = Request.QueryString["pre"];
            if (String.IsNullOrEmpty(sPre) == true)
                sPre = "";

            string sPost = Request.QueryString["post"];
            if (String.IsNullOrEmpty(sPost) == true)
                sPost = "";
            string sMain = Request.QueryString["main"];
            if (String.IsNullOrEmpty(sMain) == true)
                sMain = "";
            string sMidPoints = Request.QueryString["mid_points"];
            if (String.IsNullOrEmpty(sMidPoints) == true)
                sMidPoints = "";
            string sAsx = "";
            
            //string sTicket = "";
            //if (sMain.LastIndexOf("ticket=") != -1)
            //{
                //Int32 nTicketLoc = sMain.LastIndexOf("ticket=") + 7;
                //Int32 nTicketLocEnd = sMain.IndexOf("&", nTicketLoc);
                //if (nTicketLocEnd != -1)
                    //sTicket = sMain.Substring(nTicketLoc, nTicketLocEnd - nTicketLoc);
                //else
                    //sTicket = sMain.Substring(nTicketLoc);
                //sMain = sMain.Replace(sTicket, "TICKET_HOLDER_PLACE");
            //}
            /*
            if (CachingManager.CachingManager.Exist("asx_" + sMid + "_" + sPre + "_" + sPost + "_" + sMain + "_" + sMidPoints) == true)
            {
                sAsx = (CachingManager.CachingManager.GetCachedData("asx_" + sMid + "_" + sPre + "_" + sPost + "_" + sMain + "_" + sMidPoints)).ToString();
                //replace the ticket in ticket
            }
            else
            {
            */
                Int32 nCounter = 0;
                sAsx = "<asx version=\"3.0\">\r\n";
                Int32 nTotalEntries = 0;
                string sAsxMap = GetAsxMap(sPre, sMidPoints, sMid, sPost, ref nTotalEntries);
                sAsx += "<entry>\r\n";
                sAsx += "<PARAM NAME=\"seek_map\" VALUE=\"" + sAsxMap + "\"/>\r\n";
                sAsx += "<PARAM NAME=\"total_entries\" VALUE=\"" + nTotalEntries.ToString() + "\"/>\r\n";
                sAsx += "<PARAM NAME=\"item_index\" VALUE=\"" + nCounter.ToString() + "\"/>\r\n";
                sAsx += "<PARAM NAME=\"entry_type\" VALUE=\"dummy\"/>\r\n";
                sAsx += "<ref href=\"dummy.wmv\" />";
                sAsx += "</entry>\r\n";
                nCounter++;
                if (String.IsNullOrEmpty(sPre) == false)
                {
                    string[] spliter = { ";" };
                    string[] sPres = sPre.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < sPres.Length; i++)
                    {
                        sAsx += "<entry>\r\n";
                        //sAsx += "<PARAM NAME=\"seek_map\" VALUE=\"" + sAsxMap + "\"/>\r\n";
                        //sAsx += "<PARAM NAME=\"total_entries\" VALUE=\"" + nTotalEntries.ToString() + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"item_index\" VALUE=\"" + nCounter.ToString() + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"entry_type\" VALUE=\"pre\"/>\r\n";
                        double dS = 0.0;
                        double dDuration = 0.0;
                        sAsx += GetRefElement(sPres[i], 0, ref dS, ref dDuration);
                        //sAsx += "<ref href=\"" + sPres[i] + "\" />\r\n";
                        sAsx += "</entry>\r\n";
                        nCounter++;
                    }
                }

                sAsx += GetAsxMain(sPre, sMidPoints, sPost, sMain, sMid, sAsxMap, ref nCounter, nTotalEntries);

                if (String.IsNullOrEmpty(sPost) == false)
                {
                    string[] spliter = { ";" };
                    string[] sProsts = sPost.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < sProsts.Length; i++)
                    {
                        sAsx += "<entry>\r\n";
                        //sAsx += "<PARAM NAME=\"seek_map\" VALUE=\"" + sAsxMap + "\"/>\r\n";
                        //sAsx += "<PARAM NAME=\"total_entries\" VALUE=\"" + nTotalEntries.ToString() + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"item_index\" VALUE=\"" + nCounter.ToString() + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"entry_type\" VALUE=\"post\"/>\r\n";
                        double dS = 0.0;
                        double dDuration = 0.0;
                        sAsx += GetRefElement(sProsts[i], 0, ref dS, ref dDuration);
                        //sAsx += "<ref href=\"" + sProsts[i] + "\" />\r\n";
                        sAsx += "</entry>\r\n";
                        nCounter++;
                    }
                }

                sAsx += "</asx>\r\n";
                /*CachingManager.CachingManager.SetCachedData("asx_" + sMid + "_" + sPre + "_" + sPost + "_" + sMain + "_" + sMidPoints, sAsx, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            }*/
            Response.ClearHeaders();
            Response.Clear();
            Response.ContentType = "video/x-ms-asf";
            Response.Expires = -1;
            //if (sTicket != "")
                //sAsx = sAsx.Replace("TICKET_HOLDER_PLACE", sTicket);
            Response.Write(sAsx.Replace("&amp;" , "&"));
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("exception", ex.Message, "proxy");
        }
    }

    protected string GetAsxMap(string sPre , string sMidPoints , string sMid , string sPost , ref Int32 nTotalEntries)
    {
        Int32 nCounter = 1;
        string sRet = "";
        double dCurrentStart = 0;
        if (String.IsNullOrEmpty(sPre) == false)
        {
            string[] spliter = { ";" };
            string[] sPres = sPre.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            nCounter += sPres.Length;
        }

        string[] sep = { ";" };
        string[] sMids = { };

        sMids = sMidPoints.Split(sep, StringSplitOptions.RemoveEmptyEntries);

        Int32 nMidCount = sMids.Length;

        for (int i = 0; i <= nMidCount; i++)
        {
            if (sRet != "")
                sRet += ";";
            if (i < nMidCount)
            {
                string sPoint = sMids[i];
                double dMidLocation = double.Parse(sPoint);
                sRet += String.Format("{0:0.###}", dCurrentStart) + ":" + nCounter.ToString();
                dCurrentStart = dMidLocation;
                string[] sMidSplitChar = {";"};
                string[] sMidSplit = sMid.Split(sMidSplitChar, StringSplitOptions.RemoveEmptyEntries);
                nCounter += sMidSplit.Length;
                nCounter ++;
            }
            else
            {
                sRet += String.Format("{0:0.###}", dCurrentStart) + ":" + nCounter.ToString();
            }
        }
        if (String.IsNullOrEmpty(sPost) == false)
        {
            string[] spliter = { ";" };
            string[] sPosts = sPost.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            nCounter += sPosts.Length;
        }
        nTotalEntries = nCounter;
        return sRet;
    }

    protected string GetAsxTime(double dTime)
    {
        Int32 nHours = (int)((double)(dTime) / 3600);
        double dLeft = dTime - nHours * 3600;
        Int32 nMin = (int)((double)(dLeft) / 60);
        dLeft = dLeft - nMin * 60;

        string sRet = "";
        if (nHours < 10)
            sRet += "0";
        sRet += nHours.ToString();
        sRet += ":";
        if (nMin < 10)
            sRet += "0";
        sRet += nMin.ToString();
        sRet += ":";
        if (dLeft < 10.00)
            sRet += "0";
        sRet += String.Format("{0:0.###}", dLeft);

        return sRet;
    }

    static protected double GetTimeStrInSecs(string sTimeSecStr)
    {
        if (String.IsNullOrEmpty(sTimeSecStr) == true)
            return 0;
        double dRet = 0.0;
        string[] sToSplitWith = { ":" };
        string[] sTimeSplited = sTimeSecStr.Split(sToSplitWith, StringSplitOptions.RemoveEmptyEntries);
        Int32 nTimeParts = sTimeSplited.Length;
        if (nTimeParts == 3)
            dRet = double.Parse(sTimeSplited[0]) * 3600 + double.Parse(sTimeSplited[1]) * 60 + double.Parse(sTimeSplited[2]);
        return dRet;
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

    static protected double GetSafeDouble(string sDuration)
    {
        try
        {
            return double.Parse(sDuration);
        }
        catch
        {
            return 0.0;
        }
    }

    protected string GetRefElement(string sRefLink , double dStartPoint , ref double dPoint , ref double dDuration)
    {
        string sRet = "";
        if (sRefLink.IndexOf("a.total-media.net") != -1)
            return "<ref href=\"" + sRefLink + "\"/>\r\n";
        try
        {
            Int32 nStatus = 404;
            string s = TVinciShared.Notifier.SendGetHttpReq(sRefLink, ref nStatus);
            s = s.ToLower();
            Int32 nLoc = 0;
            while (nLoc != -1)
            {
                nLoc = s.IndexOf("href=\"", nLoc);
                if (nLoc != -1)
                {
                    Int32 nEnd = s.IndexOf("\"", nLoc + 6);
                    string sURL = s.Substring(nLoc + 6, nEnd - nLoc - 6);
                    string sNewURL = TVinciShared.ProtocolsFuncs.XMLEncode(sURL, true);
                    s = s.Remove(nLoc + 6, sURL.Length);
                    s = s.Insert(nLoc + 6, sNewURL);
                    nLoc += 6 + sNewURL.Length + 1;
                }
            }
            XmlDocument theAsx = new XmlDocument();
            theAsx.LoadXml(s);
            XmlNodeList theEntries = theAsx.GetElementsByTagName("entry");
            IEnumerator entryIter = theEntries.GetEnumerator();
            bool bOK = false;
            while (entryIter.MoveNext())
            {
                bOK = true;
                XmlNode theEntry = (XmlNode)(entryIter.Current);
                string sEntryTitle = GetNodeParameterVal(ref theEntry, "param[@name='title']", "value");
                if (sEntryTitle != "postroll" && sEntryTitle != "preroll" && sEntryTitle != "midroll")
                {
                    string sStart = GetNodeParameterVal(ref theEntry, "starttime", "value");
                    string sDuration = GetNodeParameterVal(ref theEntry, "duration", "value");
                    dPoint = GetTimeStrInSecs(sStart) + dStartPoint;
                    dDuration = GetTimeStrInSecs(sDuration);
                    XmlNodeList theRefs = ((XmlElement)(theEntry)).GetElementsByTagName("ref");
                    IEnumerator refIter = theRefs.GetEnumerator();
                    while (refIter.MoveNext())
                    {
                        XmlNode theRef = (XmlNode)(refIter.Current);
                        sRet += TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theRef, false) + "\r\n";
                    }
                }
            }
            if (bOK == false)
                return "<ref href=\"" + sRefLink + "\"/>\r\n";
            else
                return sRet;
        }
        catch
        {
            return "<ref href=\"" + sRefLink + "\"/>\r\n";
        }
    }

    protected string GetAsxMain(string sPre, string sMidPoints, string sPost , string sMain , string sMid , string sAsxMap, ref Int32 nCounter , Int32 nTotalEntries)
    {
        string sAsx = "";
        double nCurrentStart = 0;

        string[] sep = { ";" };
        string[] sMids = sMidPoints.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        
        Int32 nMidCount = sMids.Length;

        for (int i = 0; i <= nMidCount; i++)
        {
            string sPoint = "";
            double nMidLocation = 0;
            if (i < nMidCount)
            {
                sPoint = sMids[i];
                nMidLocation = double.Parse(sPoint);
            }

            sAsx += "<entry>\r\n";
            //sAsx += "<PARAM NAME=\"seek_map\" VALUE=\"" + sAsxMap + "\"/>\r\n";
            sAsx += "<PARAM NAME=\"item_index\" VALUE=\"" + nCounter.ToString() + "\"/>\r\n";
            //sAsx += "<PARAM NAME=\"total_entries\" VALUE=\"" + nTotalEntries.ToString() + "\"/>\r\n";
            sAsx += "<PARAM NAME=\"entry_type\" VALUE=\"main\"/>\r\n";
            double dS = 0;
            double dDuration = 0.0;
            sAsx += GetRefElement(sMain, nCurrentStart, ref dS , ref dDuration);
            //sAsx += "<ref href=\"" + sMain + "\" />\r\n";
            sAsx += "<starttime value=\"" + GetAsxTime(dS) + "\" />\r\n";
            if (i < nMidCount)
                sAsx += "<duration value=\"" + GetAsxTime(nMidLocation - nCurrentStart) + "\" />\r\n";
            else if (dDuration > nCurrentStart)
                sAsx += "<duration value=\"" + GetAsxTime(dDuration - nCurrentStart) + "\" />\r\n";
            sAsx += "</entry>\r\n";
            nCounter++;

            if (i < nMidCount)
            {
                if (String.IsNullOrEmpty(sMid) == false)
                {
                    string[] spliter = { ";" };
                    string[] sMidParts = sMid.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                    Int32 nCount = sMidParts.Length;
                    for (int j =0; j < nCount; j++)
                    {
                        sAsx += "<entry>\r\n";
                        //sAsx += "<PARAM NAME=\"seek_map\" VALUE=\"" + sAsxMap + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"item_index\" VALUE=\"" + nCounter.ToString() + "\"/>\r\n";
                        sAsx += "<PARAM NAME=\"entry_type\" VALUE=\"mid\"/>\r\n";
                        sAsx += GetRefElement(sMidParts[j], 0, ref dS , ref dDuration);
                        //sAsx += "<ref href=\"" + sMidParts[j] + "\" />\r\n";
                        sAsx += "</entry>\r\n";
                        nCounter++;
                    }
                }
            }
            nCurrentStart = nMidLocation;
        }
        return sAsx;
    }
}
