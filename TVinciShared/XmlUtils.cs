using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TVinciShared
{
    public class XmlUtils
    {
        static public bool IsNodeExists(ref XmlNode theItem, string sXpath)
        {
            try
            {
                XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
                if (theNodeVal != null)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

       
        /// <summary>
        /// GetSafeValue
        /// </summary>
        /// <param name="sQueryKey"></param>
        /// <param name="theRoot"></param>
        /// <returns></returns>
        public static string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) == null ||
                    theRoot.SelectSingleNode(sQueryKey).FirstChild == null)
                {
                    return "";
                }

                return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// Get Safe Par Value
        /// </summary>
        /// <param name="sQueryKey"></param>
        /// <param name="sParName"></param>
        /// <param name="theRoot"></param>
        /// <returns></returns>
        public static string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
            }
            catch
            {
                return "";
            }
        }

        static public string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            string sNodeVal = "";

            try
            {
                XmlNode theNodeVal = null;
                if (sXpath != "")
                    theNodeVal = theItem.SelectSingleNode(sXpath);
                else
                    theNodeVal = theItem;
                if (theNodeVal != null && theNodeVal.FirstChild != null)
                    sNodeVal = theNodeVal.FirstChild.Value;
            }
            catch { }
            return sNodeVal;
        }

        static public string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            string sVal = "";
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
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

        static public string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
        {
            string sVal = "";
            try
            {
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
            }
            catch { }
            return sVal;
        }

        static public string GetSafeValueFromXML(ref XmlNode theRef, string sNodeXpath)
        {
            try
            {
                if (theRef.SelectSingleNode(sNodeXpath) != null)
                {
                    string sRet = theRef.SelectSingleNode(sNodeXpath).FirstChild.Value;
                    return sRet;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        static public string[] GetSafeValuesFromXML(ref XmlNode theRef, string sNodeXpath)
        {
            string[] s = null;
            try
            {
                if (theRef.SelectNodes(sNodeXpath) != null)
                {
                    XmlNodeList l = theRef.SelectNodes(sNodeXpath);
                    s = new string[l.Count];
                    for (int i = 0; i < l.Count; i++)
                    {
                        s[i] = l[i].FirstChild.Value;
                    }
                    return s;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
