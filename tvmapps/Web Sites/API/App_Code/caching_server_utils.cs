using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for caching_server_utils
/// </summary>
public class caching_server_utils
{
    public caching_server_utils()
    {
        
    }

    static public string GetCallerIP()
    {
        string sIP = "";
        if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            sIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (sIP == "" || sIP.ToLower() == "unknown")
            sIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        return sIP;
    }

    static public string ConvertXMLToString(ref XmlNode theDoc)
    {
        if (theDoc == null)
            return "";
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlTextWriter xw = new XmlTextWriter(sw);
        theDoc.WriteTo(xw);
        return sw.ToString();
    }

    static public void SplitString(string sXMLRequest, ref string[] sXMLRequestPart)
    {
        Int32 nLoc = 0;
        Int32 nLength = sXMLRequest.Length;
        Int32 nIter = 0;
        while (nLoc < nLength)
        {
            Int32 nLengthToCut = 450;
            if (nLength - nLoc < 450)
                nLengthToCut = nLength - nLoc;
            string sPart = sXMLRequest.Substring(nLoc, nLengthToCut);
            nLoc += nLengthToCut;
            sXMLRequestPart[nIter] = sPart;
            nIter++;
        }
    }

    static public Int32 GetCacheID(ref string[] sXMLRequestPart, ref string sResponse)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,RESPONSE from tvm_caching_server where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST", "=", sXMLRequestPart[0]);
        if (sXMLRequestPart[1] != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST2", "=", sXMLRequestPart[1]);
        }
        if (sXMLRequestPart[2] != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST3", "=", sXMLRequestPart[2]);
        }
        if (sXMLRequestPart[3] != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST4", "=", sXMLRequestPart[3]);
        }
        if (sXMLRequestPart[4] != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST5", "=", sXMLRequestPart[4]);
        }
        if (sXMLRequestPart[5] != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST6", "=", sXMLRequestPart[5]);
        }
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                sResponse = selectQuery.Table("query").DefaultView[0].Row["RESPONSE"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static public string GetRequestXML(HttpRequest Request)
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }

    static public string EscapeDecoder(string sToDecode)
    {
        sToDecode = sToDecode.Replace("%20", " ");
        sToDecode = sToDecode.Replace("%3C", "<");
        sToDecode = sToDecode.Replace("%3E", ">");
        sToDecode = sToDecode.Replace("%23", "#");
        sToDecode = sToDecode.Replace("%25", "%");
        sToDecode = sToDecode.Replace("%7B", "{");
        sToDecode = sToDecode.Replace("%7D", "}");
        sToDecode = sToDecode.Replace("%7C", "|");
        sToDecode = sToDecode.Replace("%5C", "\\");
        sToDecode = sToDecode.Replace("%5E", "^");
        sToDecode = sToDecode.Replace("%7E", "~");
        sToDecode = sToDecode.Replace("%5B", "[");
        sToDecode = sToDecode.Replace("%60", "`");
        sToDecode = sToDecode.Replace("%27", "'");
        sToDecode = sToDecode.Replace("%3B", ";");
        sToDecode = sToDecode.Replace("%2F", "/");
        sToDecode = sToDecode.Replace("%3F", "?");
        sToDecode = sToDecode.Replace("%3A", ":");
        sToDecode = sToDecode.Replace("%40", "@");
        sToDecode = sToDecode.Replace("%3D", "=");
        sToDecode = sToDecode.Replace("%26", "&");
        sToDecode = sToDecode.Replace("%24", "$");
        sToDecode = sToDecode.Replace("%5F", "_");
        sToDecode = sToDecode.Replace("%22", "\"");

        sToDecode = sToDecode.Replace("%u05d0", "א");
        sToDecode = sToDecode.Replace("%u05d1", "ב");
        sToDecode = sToDecode.Replace("%u05d2", "ג");
        sToDecode = sToDecode.Replace("%u05d3", "ד");
        sToDecode = sToDecode.Replace("%u05d4", "ה");
        sToDecode = sToDecode.Replace("%u05d5", "ו");
        sToDecode = sToDecode.Replace("%u05d6", "ז");
        sToDecode = sToDecode.Replace("%u05d7", "ח");
        sToDecode = sToDecode.Replace("%u05d8", "ט");
        sToDecode = sToDecode.Replace("%u05d9", "י");
        sToDecode = sToDecode.Replace("%u05db", "כ");
        sToDecode = sToDecode.Replace("%u05dc", "ל");
        sToDecode = sToDecode.Replace("%u05de", "מ");
        sToDecode = sToDecode.Replace("%u05e0", "נ");
        sToDecode = sToDecode.Replace("%u05e1", "ס");
        sToDecode = sToDecode.Replace("%u05e2", "ע");
        sToDecode = sToDecode.Replace("%u05e4", "פ");
        sToDecode = sToDecode.Replace("%u05e6", "צ");
        sToDecode = sToDecode.Replace("%u05e7", "ק");
        sToDecode = sToDecode.Replace("%u05e8", "ר");
        sToDecode = sToDecode.Replace("%u05e9", "ש");
        sToDecode = sToDecode.Replace("%u05ea", "ת");
        sToDecode = sToDecode.Replace("%u05da", "ך");
        sToDecode = sToDecode.Replace("%u05dd", "ם");
        sToDecode = sToDecode.Replace("%u05df", "ן");
        sToDecode = sToDecode.Replace("%u05e3", "ף");
        sToDecode = sToDecode.Replace("%u05e5", "ץ");

        return sToDecode;
    }

    static public bool IsIPOK(string sAllowedIPs)
    {
        //get this IPs from config file
        //string sAllowedIPs = "127.0.0.1;213.8.115.108";
        string sCallerIP = GetCallerIP();
        string[] sep = { ";" };
        string[] allowedIPs = sAllowedIPs.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < allowedIPs.Length; i++)
        {
            if (allowedIPs[i].Trim() == sCallerIP)
                return true;
        }
        return false;
    }
}
