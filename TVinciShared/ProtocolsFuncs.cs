using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Data;
using com.llnw.mediavault;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.XPath;
using System.Configuration;
using KLogMonitor;
using System.Reflection;


namespace TVinciShared
{
    public class ProtocolsFuncs
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        const char FileFormatSeparater = ';';

        public struct MediaWatchedObj
        {
            public long m_mediaID;

            public string m_deviceName;

            public DateTime m_lastWatchedDate;

            public MediaWatchedObj(int mediaID, string deviceName, DateTime lastWatchedDate)
            {
                m_mediaID = mediaID;
                m_deviceName = deviceName;
                m_lastWatchedDate = lastWatchedDate;
            }

        }

        static public Int32 GetDeviceIdFromName(string sDevice, Int32 nGroupID)
        {
            if (String.IsNullOrEmpty(sDevice) == true || sDevice.Trim() == "")
                return 0;
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += " select id from groups_devices where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDevice);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }
        static public string GetMetaStrTranslation(Int32 nMediaID, Int32 nLangID, string sField)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select " + sField + " from media_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row[sField]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string XMLEncode(string sToEncode, bool bAttribute)
        {
            if (string.IsNullOrEmpty(sToEncode))
                return string.Empty;
            //XmlAttribute element = m_xmlDox.CreateAttribute("E");
            //element.InnerText = sToEncode;
            sToEncode = sToEncode.Replace("&", "&amp;");
            sToEncode = sToEncode.Replace((char)8232, '\r');
            sToEncode = sToEncode.Replace("<", "&lt;");
            sToEncode = sToEncode.Replace(">", "&gt;");
            if (bAttribute == true)
            {
                sToEncode = sToEncode.Replace("'", "&apos;");
                sToEncode = sToEncode.Replace("\"", "&quot;");
            }
            sToEncode = sToEncode.Replace("&amp;quot;", "&quot;");
            return sToEncode;
        }

        static protected string GetMetaFieldValuesXML(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, string sQuery, bool bOnlyRelated,
            ref ApiObjects.MediaInfoObject theInfo)
        {
            if (sQuery == "")
                return "";
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select " + sQuery + " from media m (nolock),groups g (nolock) ";
            if (bIsLangMain == false)
                selectQuery += ",media_translate mt (nolock) ";
            selectQuery += " where g.id=m.group_id and ";
            if (bIsLangMain == false)
                selectQuery += " mt.media_id=m.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
            if (theInfo != null)
            {
                selectQuery.SetCachedSec(0);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nColumnsCount = selectQuery.Table("query").Columns.Count;
                    for (int i = 0; i < nColumnsCount; i += 2)
                    {
                        string sColumnName = selectQuery.Table("query").Columns[i].ColumnName;
                        string sColumn1Name = selectQuery.Table("query").Columns[i + 1].ColumnName;
                        object oVal = DBNull.Value;
                        object oName = DBNull.Value;

                        oVal = selectQuery.Table("query").DefaultView[0].Row[sColumn1Name];
                        oName = selectQuery.Table("query").DefaultView[0].Row[sColumnName];

                        if (sColumnName.StartsWith("Column") == true && sColumn1Name == "name")
                        {
                            if (oVal != DBNull.Value && oVal != null && oVal.ToString() != "")
                            {
                                if (theInfo == null)
                                    sRet.Append("<name value=\"").Append(XMLEncode(oVal.ToString(), true)).Append("\"/>");
                                else
                                    theInfo.m_sTitle = oVal.ToString();
                            }
                        }

                        else if (sColumnName.StartsWith("Column") == true && sColumn1Name == "description")
                        {
                            if (oVal != DBNull.Value && oVal != null && oVal.ToString() != "")
                            {
                                if (theInfo == null)
                                    sRet.Append("<description value=\"").Append(XMLEncode(oVal.ToString(), true)).Append("\"/>");
                                else
                                    theInfo.m_sDescription = oVal.ToString();
                            }
                        }

                        else if (sColumnName.StartsWith("Column") == true && sColumn1Name == "MEDIA_TYPE_ID")
                        {
                            if (oVal != DBNull.Value && oVal != null && oVal.ToString() != "")
                            {
                                string sTypeID = oVal.ToString();
                                object oTypeName = ODBCWrapper.Utils.GetTableSingleVal("media_types", "NAME", GetSafeInt(sTypeID));
                                if (oTypeName != DBNull.Value && oTypeName != null && oTypeName.ToString() != "")
                                {
                                    if (theInfo == null)
                                        sRet.Append("<type id=\"").Append(sTypeID).Append("\" value=\"").Append(XMLEncode(oTypeName.ToString(), true)).Append("\"/>");
                                    else
                                    {
                                        theInfo.m_sTypeName = oTypeName.ToString();
                                        theInfo.m_nTypeID = int.Parse(sTypeID);
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (oVal != DBNull.Value && oVal != null && oVal.ToString() != "" &&
                                oName != DBNull.Value && oName != null && oName.ToString() != "")
                            {
                                if (theInfo == null)
                                {
                                    if (bOnlyRelated == true)
                                        sRet.Append("<meta name=\"").Append(XMLEncode(oName.ToString(), true)).Append("\" value=\"").Append(XMLEncode(oVal.ToString(), true)).Append("\"/>");
                                    else
                                        sRet.Append("<" + sColumnName + " name=\"").Append(XMLEncode(oName.ToString(), true)).Append("\" value=\"").Append(XMLEncode(oVal.ToString(), true)).Append("\"/>");
                                }
                                else
                                {
                                    if (sColumnName.EndsWith("_STR_NAME") == true)
                                    {
                                        ApiObjects.MetaStrObject m = new ApiObjects.MetaStrObject();
                                        m.Initialize(oName.ToString(), oVal.ToString());
                                        if (theInfo.m_oStrObjects == null)
                                        {
                                            theInfo.m_oStrObjects = new ApiObjects.MetaStrObject[1];
                                        }
                                        else
                                            theInfo.m_oStrObjects = (ApiObjects.MetaStrObject[])(ResizeArray(theInfo.m_oStrObjects, theInfo.m_oStrObjects.Length + 1));
                                        theInfo.m_oStrObjects[theInfo.m_oStrObjects.Length - 1] = m;
                                    }
                                    if (sColumnName.EndsWith("_BOOL_NAME") == true)
                                    {
                                        ApiObjects.MetaBoolObject m = new ApiObjects.MetaBoolObject();
                                        if (oVal.ToString() == "true")
                                            m.Initialize(oName.ToString(), true);
                                        else
                                            m.Initialize(oName.ToString(), false);
                                        if (theInfo.m_oBoolObjects == null)
                                        {
                                            theInfo.m_oBoolObjects = new ApiObjects.MetaBoolObject[1];
                                        }
                                        else
                                            theInfo.m_oBoolObjects = (ApiObjects.MetaBoolObject[])(ResizeArray(theInfo.m_oBoolObjects, theInfo.m_oBoolObjects.Length + 1));
                                        theInfo.m_oBoolObjects[theInfo.m_oBoolObjects.Length - 1] = m;
                                    }
                                    if (sColumnName.EndsWith("_DOUBLE_NAME") == true)
                                    {
                                        ApiObjects.MetaDoubleObject m = new ApiObjects.MetaDoubleObject();
                                        m.Initialize(oName.ToString(), double.Parse(oVal.ToString()), null);
                                        if (theInfo.m_oDoubleObjects == null)
                                        {
                                            theInfo.m_oDoubleObjects = new ApiObjects.MetaDoubleObject[1];
                                        }
                                        else
                                            theInfo.m_oDoubleObjects = (ApiObjects.MetaDoubleObject[])(ResizeArray(theInfo.m_oDoubleObjects, theInfo.m_oDoubleObjects.Length + 1));
                                        theInfo.m_oDoubleObjects[theInfo.m_oDoubleObjects.Length - 1] = m;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        static public string GetMetaFieldsvalues(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, ref XmlNode theInfoStruct, bool bOnlyRelated,
            ref ApiObjects.MediaInfoObject theInfo)
        {
            string sQueryParams = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select g.*,m.* from groups g (nolock),media m  WITH (nolock) where g.id=m.group_id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (bOnlyRelated == false)
                    {
                        if (IsNodeExists(ref theInfoStruct, "type", "", "") == true)
                        {
                            if (sQueryParams != "")
                                sQueryParams += ",";
                            sQueryParams += "'MEDIA_TYPE_ID',MEDIA_TYPE_ID";
                        }
                        bool bIsNameExists = IsNodeExists(ref theInfoStruct, "name", "", "");
                        bool bIsDescExists = IsNodeExists(ref theInfoStruct, "description", "", "");
                        bool bIsTypeExists = IsNodeExists(ref theInfoStruct, "type", "", "");
                        if (bIsNameExists == true)
                        {
                            if (sQueryParams != "")
                                sQueryParams += ",";
                            if (bIsLangMain == true)
                                sQueryParams += "'name',m.name";
                            else
                                sQueryParams += "'name',mt.name";
                        }
                        if (bIsDescExists == true)
                        {
                            if (sQueryParams != "")
                                sQueryParams += ",";
                            if (bIsLangMain == true)
                                sQueryParams += "'description',m.description";
                            else
                                sQueryParams += "'description',mt.description";
                        }
                    }
                    for (int i = 1; i < 21; i++)
                    {
                        string sFieldName = "META" + i.ToString() + "_STR_NAME";
                        string sFieldVal = "META" + i.ToString() + "_STR";
                        string sFieldRelated = "IS_META" + i.ToString() + "_STR_RELATED";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                        {
                            string sName = "";
                            sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                            if (sName != "")
                            {
                                bool bIsMetaExists = IsNodeExists(ref theInfoStruct, "meta", "name", sName);
                                if (bIsMetaExists)
                                {
                                    Int32 nIsRelated = 1;
                                    if (bOnlyRelated == true)
                                        nIsRelated = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row[sFieldRelated].ToString());
                                    if (nIsRelated == 1)
                                    {
                                        if (sQueryParams != "")
                                            sQueryParams += ",";
                                        if (bIsLangMain == true)
                                            sQueryParams += "g." + sFieldName + "," + "m." + sFieldVal;
                                        else
                                            sQueryParams += "g." + sFieldName + "," + "mt." + sFieldVal;
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 1; i < 11; i++)
                    {
                        string sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                        string sFieldVal = "META" + i.ToString() + "_DOUBLE";
                        string sFieldRelated = "IS_META" + i.ToString() + "_DOUBLE_RELATED";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                        {
                            string sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                            if (sName != "")
                            {
                                bool bIsMetaExists = IsNodeExists(ref theInfoStruct, "meta", "name", sName);
                                if (bIsMetaExists)
                                {
                                    Int32 nIsRelated = 1;
                                    if (bOnlyRelated == true)
                                        nIsRelated = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row[sFieldRelated].ToString());
                                    if (nIsRelated == 1)
                                    {
                                        if (sQueryParams != "")
                                            sQueryParams += ",";
                                        sQueryParams += "g." + sFieldName + "," + "m." + sFieldVal;
                                    }
                                }
                            }
                        }
                    }
                    if (bOnlyRelated == false)
                    {
                        for (int i = 1; i < 11; i++)
                        {
                            string sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                            string sFieldVal = "META" + i.ToString() + "_BOOL";
                            if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                            {
                                string sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                                if (sName != "")
                                {
                                    bool bIsMetaExists = IsNodeExists(ref theInfoStruct, "meta", "name", sName);
                                    if (bIsMetaExists)
                                    {
                                        if (sQueryParams != "")
                                            sQueryParams += ",";
                                        sQueryParams += "g." + sFieldName + "," + "m." + sFieldVal;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return GetMetaFieldValuesXML(nMediaID, nLangID, bIsLangMain, sQueryParams, bOnlyRelated, ref theInfo);
        }

        static public void GetMediaTranslation(Int32 nMediaID, Int32 nLangID, ref string sName, ref string sDescription)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select name,description from media_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    sDescription = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected bool IsNodeExists(ref XmlNode theItem, string sNodeType, string sWhereFieldName, string sWhereFieldVal)
        {
            if (theItem == null)
                return true;
            string sXpath = "//" + sNodeType;
            if (sWhereFieldName != "")
                sXpath += "[@" + sWhereFieldName + "='" + sWhereFieldVal + "']";
            XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
            if (theNodeVal != null)
                return true;
            return false;
        }

        static public string GetNodeParameter(ref XmlNode theItem, string sParameter)
        {
            if (theItem == null)
                return "true";
            string sXpath = "@" + sParameter;
            XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
            if (theNodeVal != null)
                return theNodeVal.Value.ToLower();
            return "true";
        }

        static public string GetMediaBasicData(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, ref XmlNode theInfoStruct)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media  WITH (nolock) where ";
            //selectQuery.SetCachedSec(3600);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    string sName = "";
                    string sDescription = "";
                    string sTypeID = "";
                    if (IsNodeExists(ref theInfoStruct, "type", "", "") == true)
                        sTypeID = selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"].ToString();
                    bool bIsNameExists = IsNodeExists(ref theInfoStruct, "name", "", "");
                    bool bIsDescExists = IsNodeExists(ref theInfoStruct, "description", "", "");
                    bool bIsTypeExists = IsNodeExists(ref theInfoStruct, "type", "", "");
                    if (bIsLangMain == true)
                    {
                        if (bIsNameExists == true)
                            sName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                        if (bIsDescExists == true)
                            sDescription = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
                    }
                    else
                    {
                        if (bIsNameExists == true || bIsDescExists == true)
                            GetMediaTranslation(nMediaID, nLangID, ref sName, ref sDescription);
                    }
                    //Optimization
                    if (sName.Trim() != "")
                        sRet.Append("<name value=\"").Append(XMLEncode(sName, true)).Append("\"/>");
                    if (sDescription.Trim() != "")
                        sRet.Append("<description value=\"").Append(XMLEncode(sDescription, true)).Append("\"/>");
                    //Optimization
                    object oName = DBNull.Value;
                    if (sTypeID != "")
                        oName = PageUtils.GetTableSingleVal("media_types", "NAME", GetSafeInt(sTypeID));
                    string sTypeName = "";
                    if (oName != DBNull.Value && oName != null)
                        sTypeName = oName.ToString();
                    if (sTypeName != "")
                        sRet.Append("<type id=\"").Append(sTypeID).Append("\" value=\"").Append(XMLEncode(sTypeName, true)).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetBasePicURL(Int32 nGroupID)
        {
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;
            return sBasePicsURL;
        }

        static public string GetPicURL(Int32 nPicID, string sPicSize)
        {
            if (nPicID == 0)
                return "";
            string sPicURL = "";
            Int32 nPicGroupID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from pics (nolock) where ";
            //selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPicID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sPicURL = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
                    nPicGroupID = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            string sBasePicsURL = GetBasePicURL(nPicGroupID);
            bool bWithEnding = true;
            if (sBasePicsURL.EndsWith("=") == false)
                sBasePicsURL += "/";
            else
                bWithEnding = false;
            sBasePicsURL += ImageUtils.GetTNName(sPicURL, sPicSize.Replace("x", "X"));
            if (bWithEnding == false)
            {
                string sTmp = "";
                string[] s = sBasePicsURL.Split('.');
                for (int i = 0; i < s.Length - 1; i++)
                {
                    if (i > 0)
                        sTmp += ".";
                    sTmp += s[i];
                }
                sBasePicsURL = sTmp;
            }
            return sBasePicsURL;
        }

        static public string GetTagTypesMediaValues(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, ref XmlNode theInfoStruct, bool bOnlyRelated,
            ref ApiObjects.MediaInfoObject theInfo)
        {
            StringBuilder sRet = new StringBuilder();
            string sTagsIDs = "";
            Int32 nGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", nMediaID).ToString());
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_tags_types (nolock) where status=1 and (group_id " + sGroups;
            selectQuery += " or group_id=0) ";
            if (bOnlyRelated == true)
                selectQuery += " and IS_RELATED=1 ";
            selectQuery += " order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    bool bIsMetaExists = IsNodeExists(ref theInfoStruct, "tags/tag_type", "name", sName);
                    if (bIsMetaExists)
                    {
                        if (sTagsIDs != "")
                            sTagsIDs += ",";
                        sTagsIDs += sID;
                    }
                }
                sRet.Append(GetMediaTags(nMediaID, nLangID, bIsLangMain, sTagsIDs, ref theInfo));
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetTagTypesMediaValuesForSearch(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, ref XmlNode theInfoStruct, bool bOnlyRelated)
        {
            ApiObjects.MediaInfoObject theInfo = null;
            StringBuilder sRet = new StringBuilder();
            string sTagsIDs = "";
            Int32 nGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", nMediaID).ToString());
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select * from media_tags_types (nolock) where status=1 and group_id " + sGroups;
            if (bOnlyRelated == true)
                selectQuery += " and IS_RELATED=1 ";
            selectQuery += " order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    bool bIsMetaExists = IsNodeExists(ref theInfoStruct, "tags/tag_type", "name", sName);
                    if (bIsMetaExists)
                    {
                        if (sTagsIDs != "")
                            sTagsIDs += ",";
                        sTagsIDs += sID;
                    }
                }
                GetMediaTags(nMediaID, nLangID, bIsLangMain, sTagsIDs, ref theInfo);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetMediaTags(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, string sTagIds,
            ref ApiObjects.MediaInfoObject theInfo)
        {
            if (sTagIds == "")
                return "";
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mtt.id as tag_type_id,mtt.NAME as tag_type_name,";
            if (bIsLangMain == true)
                selectQuery += " t.value as value,";
            else
                selectQuery += " tt.value as value,";

            selectQuery += "t.id as tag_id ";

            selectQuery += " from tags t (nolock),media_tags_types (nolock) mtt,media_tags mt  WITH (nolock) ";
            if (bIsLangMain == false)
                selectQuery += ",tags_translate tt ";
            selectQuery += " where mt.tag_id=t.id and t.status=1 and mt.status=1 and mtt.id=t.TAG_TYPE_ID and ";
            if (bIsLangMain == false)
            {
                selectQuery += "tt.tag_id=t.id and  ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.LANGUAGE_ID", "=", nLangID);
                selectQuery += " and ";
            }
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            selectQuery += "and t.TAG_TYPE_ID in (" + sTagIds + ") order by t.TAG_TYPE_ID";
            if (theInfo != null)
            {
                selectQuery.SetCachedSec(0);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                string sLastType = "";
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                bool bInside = false;
                if (theInfo == null)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        string sTagID = selectQuery.Table("query").DefaultView[i].Row["tag_id"].ToString();
                        string sTagType = selectQuery.Table("query").DefaultView[i].Row["tag_type_name"].ToString();
                        string sTagValue = selectQuery.Table("query").DefaultView[i].Row["value"].ToString();
                        if (sTagValue != "")
                        {
                            bInside = true;
                            if (sLastType != sTagType)
                            {
                                if (sLastType != "")
                                    sRet.Append("</tag_type>");
                                sRet.Append("<tag_type id=\"").Append(sTagID).Append("\" name=\"").Append(XMLEncode(sTagType, true)).Append("\">");
                                sLastType = sTagType;
                            }
                            sRet.Append("<tag id=\"").Append(sTagID).Append("\" name=\"").Append(XMLEncode(sTagValue, true)).Append("\"/>");
                        }
                    }
                    if (bInside == true)
                        sRet.Append("</tag_type>");
                }
                else
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        string sTagID = selectQuery.Table("query").DefaultView[i].Row["tag_id"].ToString();
                        string sTagType = selectQuery.Table("query").DefaultView[i].Row["tag_type_name"].ToString();
                        string sTagValue = selectQuery.Table("query").DefaultView[i].Row["value"].ToString();
                        if (sTagValue != "")
                        {
                            if (sLastType != sTagType)
                            {
                                ApiObjects.MetaM2MObject m = new ApiObjects.MetaM2MObject();
                                if (theInfo.m_oTagsObjects == null)
                                    theInfo.m_oTagsObjects = new ApiObjects.MetaM2MObject[1];
                                else
                                    theInfo.m_oTagsObjects = (ApiObjects.MetaM2MObject[])(ResizeArray(theInfo.m_oTagsObjects, theInfo.m_oTagsObjects.Length + 1));
                                theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1] = m;

                                //sRet.Append("<tag_type id=\"").Append(sTagID).Append("\" name=\"").Append(XMLEncode(sTagType, true)).Append("\">");
                                theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaName = sTagType;
                                theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_nMetaID = int.Parse(sTagID);
                                sLastType = sTagType;
                            }
                            if (theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues == null)
                                theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues = new string[1];
                            else
                                theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues = (string[])(ResizeArray(theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues, theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues.Length + 1));
                            theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues[theInfo.m_oTagsObjects[theInfo.m_oTagsObjects.Length - 1].m_sMetaValues.Length - 1] = sTagValue;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetMediaTagsForSearch(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, bool bOnlyRelated)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select t.id,t.value,mtt.NAME from tags t (nolock),media_tags mt (nolock),media_tags_types mtt  WITH (nolock) where mt.tag_id=t.id and t.status=1 and mt.status=1 and mtt.id=t.TAG_TYPE_ID and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            if (bOnlyRelated == true)
                selectQuery += " and mtt.IS_RELATED=1";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sTagID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                    string sTagType = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    string sTagValue = "";
                    if (bIsLangMain == true)
                        sTagValue = selectQuery.Table("query").DefaultView[i].Row["value"].ToString();
                    else
                        sTagValue = GetTagTranslation(int.Parse(sTagID), nLangID);
                    if (sTagValue != "")
                        sRet.Append("<tag id=\"").Append(sTagID).Append("\" name=\"").Append(XMLEncode(sTagType, true)).Append("\" value=\"").Append(XMLEncode(sTagValue, true)).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetTagTranslation(Int32 nTagID, Int32 nLangID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select VALUE from tags_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID", "=", nTagID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["value"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetMediaTagsRelatedIDs(Int32 nMediaID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("(");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct mt.tag_id from media_tags mt (nolock),tags t (nolock),media_tags_types mtt  WITH (nolock) where mtt.id=t.TAG_TYPE_ID and mt.tag_id=t.id and t.status=1 and mt.status=1 and mtt.is_related=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    string sTagID = selectQuery.Table("query").DefaultView[i].Row["tag_id"].ToString();
                    sRet.Append(sTagID);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            return sRet.ToString();
        }

        static public Int32 GetMediaFileID(Int32 nMediaID, string sFileFormat, string sFileQuality, bool bIsAdmin, Int32 nGroupID, bool bWithCache)
        {
            string sBilling = "";
            double dDuration = 0.0;
            return GetMediaFileID(nMediaID, sFileFormat, sFileQuality, ref sBilling, ref dDuration, bIsAdmin, nGroupID, bWithCache);
        }

        static public Int32 GetMediaFileID(Int32 nMediaID, string sFileFormat, string sFileQuality, ref string sBillingType, ref double dDuration, bool bIsAdmin, Int32 nGroupID, bool bWithCache)
        {
            string sBaseBilling = "";
            Int32 nViews = 0;
            return GetMediaFileID(nMediaID, sFileFormat, sFileQuality, ref sBillingType, ref sBaseBilling, ref dDuration, ref nViews, bIsAdmin, nGroupID, bWithCache);
        }

        static public Int32 GetMediaFileID(Int32 nMediaID, string sFileFormat, string sFileQuality, ref string sBillingType, bool bIsAdmin, Int32 nGroupID, bool bWithCache)
        {
            string sBaseBilling = "";
            double dDuration = 0.0;
            Int32 nViews = 0;
            return GetMediaFileID(nMediaID, sFileFormat, sFileQuality, ref sBillingType, ref sBaseBilling, ref dDuration, ref nViews, bIsAdmin, nGroupID, bWithCache);
        }

        static public Int32 GetFileQualityID(string sFileType)
        {
            Int32 nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select * from lu_media_quality (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sFileType);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static public Int32 GetFileTypeID(string sFileType, Int32 nGroupID)
        {
            sFileType = sFileType.Trim().ToLower();
            Int32 nID = GetFriendlyFileTypeID(sFileType, nGroupID);
            if (nID > 0)
                return nID;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "SELECT ID FROM lu_media_types (nolock) WHERE ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sFileType);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oID = selectQuery.Table("query").DefaultView[0].Row["id"];
                    if (oID != DBNull.Value)
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static public Int32 GetFriendlyFileTypeID(string sFileType, Int32 nGroupID)
        {

            sFileType = sFileType.Trim().ToLower();
            Int32 nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "SELECT MEDIA_TYPE_ID FROM groups_media_type (nolock) WHERE ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sFileType);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oF_ID = selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"];
                    if (oF_ID != DBNull.Value)
                        nID = int.Parse(oF_ID.ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static protected Int32 GetMediaFileID(Int32 nMediaID, string sFileFormat, string sFileQuality, ref string sBillingType, ref string sBaseBillingType, ref double dFileDuration, ref Int32 nViews, bool bIsAdmin, Int32 nGroupID, bool bWithCache)
        {
            Int32 nRet = 0;

            if (bWithCache == true && CachingManager.CachingManager.Exist("GetMediaFileID_" + nMediaID.ToString() + "_" + sFileFormat + "_" + sFileQuality + "_" + bIsAdmin.ToString() + "_" + nGroupID.ToString()) == true)
            {
                object[] arr = ((string[])(CachingManager.CachingManager.GetCachedData("GetMediaFileID_" + nMediaID.ToString() + "_" + sFileFormat + "_" + sFileQuality + "_" + bIsAdmin.ToString() + "_" + nGroupID.ToString())));
                nRet = int.Parse(arr[0].ToString());
                sBillingType = arr[1].ToString();
                sBaseBillingType = arr[2].ToString();
                dFileDuration = double.Parse(arr[3].ToString());
                nViews = int.Parse(arr[4].ToString());
            }

            Int32 nQualityID = GetFileQualityID(sFileQuality);
            Int32 nFormatID = GetFileTypeID(sFileFormat, nGroupID);


            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(7200);
            selectQuery += "select mf.id,mf.duration,mf.views,lbt.api_val as 'bill_type',lbt.description as 'base_bill_type' from lu_billing_type lbt (nolock) ,media_files mf WITH (nolock) where lbt.id=mf.billing_type_id and mf.STATUS=1 and ";
            if (bIsAdmin == false)
                selectQuery += " mf.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sBillingType = selectQuery.Table("query").DefaultView[0].Row["bill_type"].ToString();
                    sBaseBillingType = selectQuery.Table("query").DefaultView[0].Row["base_bill_type"].ToString();
                    dFileDuration = double.Parse(selectQuery.Table("query").DefaultView[0].Row["duration"].ToString());
                    nViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["views"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            object[] arrCache = new string[5];
            arrCache[0] = nRet.ToString();
            arrCache[1] = sBillingType;
            arrCache[2] = sBaseBillingType;
            arrCache[3] = dFileDuration.ToString();
            arrCache[4] = nViews.ToString();
            CachingManager.CachingManager.SetCachedData("GetMediaFileID_" + nMediaID.ToString() + "_" + sFileFormat + "_" + sFileQuality + "_" + bIsAdmin.ToString() + "_" + nGroupID.ToString(), arrCache, 5400, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return nRet;
        }

        static protected string GetMediaTypeForPlayer(string sFileFormat, Int32 nMediaFileID, Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select lpd.player_description from lu_player_descriptions lpd (nolock),media_files mf WITH (nolock) where lpd.id=mf.OVERRIDE_PLAYER_TYPE_ID and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["player_description"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sRet.ToString() != "")
                return sRet.ToString();
            return GetMediaTypeForPlayer(sFileFormat, nGroupID);
        }

        static protected string GetMediaTypeForPlayer(string sFileFormat, Int32 nGroupID)
        {
            Int32 nFormatID = GetFileTypeID(sFileFormat, nGroupID);
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_media_types (nolock) where ";
            selectQuery.SetCachedSec(86400);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nFormatID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["PLAYER_DESCRIPTION"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetFlashVarsValue(ref XmlDocument theDoc, string sAttrName)
        {
            string sRet = "";
            XmlNode theFlashVars = theDoc.SelectSingleNode("/root/flashvars");
            if (theFlashVars != null)
            {
                XmlAttributeCollection theAttr = theFlashVars.Attributes;
                if (theAttr != null)
                {
                    Int32 nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName == sAttrName.ToLower())
                        {
                            sRet = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            return sRet;
        }

        static public string GetFinalEndDateField(bool bUseFinalEndDate)
        {
            if (bUseFinalEndDate)
                return "FINAL_END_DATE";
            return "END_DATE";

        }

        static public string GetFinalEndDateField(ref XmlDocument theDoc)
        {
            string sFinalEndDate = GetFlashVarsValue(ref theDoc, "use_final_end_date");
            if (sFinalEndDate.Trim().ToLower() == "1" || sFinalEndDate.Trim().ToLower() == "true")
                return "FINAL_END_DATE";
            return "END_DATE";

        }

        static public string GetPicSizeForCache(ref XmlDocument theDoc)
        {
            if (theDoc == null)
                return "";
            StringBuilder sRet = new StringBuilder();
            bool bCont = true;
            Int32 nPicSizeNum = 1;
            while (bCont == true)
            {
                string sPicSize = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString());
                string sPicSizeFormat = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_format");
                string sPicSizeQuality = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_quality");
                if (sPicSize == "")
                {
                    bCont = false;
                    break;
                }
                else
                {
                    sRet.Append(sPicSize).Append("_").Append(sPicSizeFormat).Append("_").Append(sPicSizeQuality).Append("|");
                    nPicSizeNum++;
                }
            }
            return sRet.ToString();
        }

        static public string GetPicSizeForCache(ref ApiObjects.PicObject[] thePics)
        {
            if (thePics == null)
                return "";
            StringBuilder sRet = new StringBuilder();
            Int32 nCount = thePics.Length;
            for (int i = 0; i < nCount; i++)
            {
                string sPicSize = thePics[i].m_nPicWidth.ToString() + "X" + thePics[i].m_nPicHeight.ToString();
                string sPicSizeFormat = "";
                string sPicSizeQuality = "";
                if (thePics[i].m_oFileRequestObj != null)
                {
                    sPicSizeFormat = thePics[i].m_oFileRequestObj.m_sFileFormat;
                    sPicSizeQuality = thePics[i].m_oFileRequestObj.m_sFileQuality;
                }
                sRet.Append(sPicSize).Append("_").Append(sPicSizeFormat).Append("_").Append(sPicSizeQuality).Append("|");
            }
            return sRet.ToString();
        }

        static protected string GetSafeTextValue(ref ODBCWrapper.DataSetSelectQuery selectQuery, Int32 nIndex, string sVal)
        {
            object o = selectQuery.Table("query").DefaultView[nIndex].Row[sVal];
            if (o != null && o != DBNull.Value)
                return o.ToString();
            return "";
        }

        static protected void DeleteMediaTexts(Int32 nMediaID, Int32 nGroupID,
            Int32 nLangID, Int32 nMEDIA_TEXT_TYPE_ID, Int32 nMEDIA_TEXT_TYPE_NUM)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_values");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMEDIA_TEXT_TYPE_ID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nMEDIA_TEXT_TYPE_NUM);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static public void InsertMediaText(Int32 nMediaID, Int32 nMediaTextTypeID, Int32 nMediaTextTypeNum, Int32 nGroupID,
            Int32 nLangID, string sVal, bool bMultiValues)
        {
            if (sVal.Trim() == "")
                return;
            DateTime dUtcNow = ODBCWrapper.Utils.GetCurrentDBTime();
            Int32 nID = 0;
            if (sVal.Length > 300)
                sVal = sVal.Substring(0, 300);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += " select id from media_values (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nMediaTextTypeNum);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            if (bMultiValues == true)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_values");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nMediaTextTypeNum);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            else
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_values");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", dUtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        static public void SeperateMediaTexts(Int32 nMediaID)
        {
            SeperateMediaMainTexts(nMediaID);
            SeperateMediaTranslateTexts(nMediaID, 0);
            SeperateMediaMainTags(nMediaID);
            SeperateMediaTranslateTags(nMediaID);
        }

        static public void SeperateMediaTranslateTagsByTags(Int32 nTagID, Int32 nLanguageID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct mt.media_id from media_tags mt (nolock),tags t (nolock) where mt.tag_id=t.id and mt.status=1 and t.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.tag_id", "=", nTagID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_id"].ToString());
                    SeperateMediaTranslateTags(nMediaID);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void SeperateMediaTranslateTags(Int32 nMediaID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += " select t.TAG_TYPE_ID,tt.* from media_tags mt (nolock),tags t (nolock) ,tags_translate tt (nolock) where tt.tag_id=t.id and mt.tag_id=t.id and mt.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGroupID = int.Parse(GetSafeTextValue(ref selectQuery, i, "GROUP_ID"));
                    Int32 nLangID = int.Parse(GetSafeTextValue(ref selectQuery, i, "LANGUAGE_ID"));
                    Int32 nTagTypeID = int.Parse(GetSafeTextValue(ref selectQuery, i, "TAG_TYPE_ID"));
                    string sValue = GetSafeTextValue(ref selectQuery, i, "value");

                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 7, nTagTypeID);
                    InsertMediaText(nMediaID, 7, nTagTypeID, nGroupID, nLangID, sValue, true);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void SeperateMediaMainTags(Int32 nMediaID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += " select g.LANGUAGE_ID,t.* from media_tags mt (nolock),tags t (nolock) ,groups g (nolock) where mt.tag_id=t.id and t.group_id=g.id and mt.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            selectQuery += " order by TAG_TYPE_ID ";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                Int32 nLastTagTypeID = -1;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGroupID = int.Parse(GetSafeTextValue(ref selectQuery, i, "GROUP_ID"));
                    Int32 nLangID = int.Parse(GetSafeTextValue(ref selectQuery, i, "LANGUAGE_ID"));
                    Int32 nTagTypeID = int.Parse(GetSafeTextValue(ref selectQuery, i, "TAG_TYPE_ID"));
                    string sValue = GetSafeTextValue(ref selectQuery, i, "value");
                    if (nLastTagTypeID != nTagTypeID)
                        DeleteMediaTexts(nMediaID, nGroupID, nLangID, 7, nTagTypeID);
                    nLastTagTypeID = nTagTypeID;
                    InsertMediaText(nMediaID, 7, nTagTypeID, nGroupID, nLangID, sValue, true);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void SeperateMediaMainTexts(Int32 nMediaID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += " select g.LANGUAGE_ID,m.* from media m (nolock) ,groups g (nolock) where m.group_id=g.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nGroupID = int.Parse(GetSafeTextValue(ref selectQuery, 0, "GROUP_ID"));
                    Int32 nLangID = int.Parse(GetSafeTextValue(ref selectQuery, 0, "LANGUAGE_ID"));

                    string sName = GetSafeTextValue(ref selectQuery, 0, "NAME");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 1, 0);
                    InsertMediaText(nMediaID, 1, 0, nGroupID, nLangID, sName, false);

                    string sDescription = GetSafeTextValue(ref selectQuery, 0, "DESCRIPTION");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 2, 0);
                    InsertMediaText(nMediaID, 2, 0, nGroupID, nLangID, sDescription, false);

                    string sCO_GUID = GetSafeTextValue(ref selectQuery, 0, "CO_GUID");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 3, 0);
                    InsertMediaText(nMediaID, 3, 0, nGroupID, nLangID, sCO_GUID, false);

                    string sEPG_IDENTIFIER = GetSafeTextValue(ref selectQuery, 0, "EPG_IDENTIFIER");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 4, 0);
                    InsertMediaText(nMediaID, 4, 0, nGroupID, nLangID, sEPG_IDENTIFIER, false);

                    for (int i = 1; i <= 20; i++)
                    {
                        string sMetaStr = GetSafeTextValue(ref selectQuery, 0, "META" + i.ToString() + "_STR");
                        DeleteMediaTexts(nMediaID, nGroupID, nLangID, 5, i);
                        InsertMediaText(nMediaID, 5, i, nGroupID, nLangID, sMetaStr, false);
                    }
                    for (int i = 1; i <= 10; i++)
                    {
                        string sMetaStr = GetSafeTextValue(ref selectQuery, 0, "META" + i.ToString() + "_DOUBLE");
                        DeleteMediaTexts(nMediaID, nGroupID, nLangID, 6, i);
                        InsertMediaText(nMediaID, 6, i, nGroupID, nLangID, sMetaStr, false);
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public void SeperateMediaTranslateTexts(Int32 nMediaID, Int32 nLangusageID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += " select m.group_id as g_id,mt.* from media m (nolock) ,media_translate mt (nolock) where m.id=mt.media_id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.MEDIA_ID", "=", nMediaID);
            if (nLangusageID != 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.LANGUAGE_ID", "=", nLangusageID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGroupID = int.Parse(GetSafeTextValue(ref selectQuery, i, "G_ID"));
                    Int32 nLangID = int.Parse(GetSafeTextValue(ref selectQuery, i, "LANGUAGE_ID"));

                    string sName = GetSafeTextValue(ref selectQuery, i, "NAME");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 1, 0);
                    InsertMediaText(nMediaID, 1, 0, nGroupID, nLangID, sName, false);

                    string sDescription = GetSafeTextValue(ref selectQuery, i, "DESCRIPTION");
                    DeleteMediaTexts(nMediaID, nGroupID, nLangID, 2, 0);
                    InsertMediaText(nMediaID, 2, 0, nGroupID, nLangID, sDescription, false);

                    for (int j = 1; j <= 20; j++)
                    {
                        string sMetaStr = GetSafeTextValue(ref selectQuery, 0, "META" + j.ToString() + "_STR");
                        DeleteMediaTexts(nMediaID, nGroupID, nLangID, 5, j);
                        InsertMediaText(nMediaID, 5, j, nGroupID, nLangID, sMetaStr, false);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void GetMediaFilePicValues(Int32 nMediaFileID, ref object nPicID, ref object nHeight, ref object nRecurringType)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_files where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nPicID = selectQuery.Table("query").DefaultView[0].Row["ref_id"];
                    nHeight = selectQuery.Table("query").DefaultView[0].Row["BRAND_HEIGHT"];
                    nRecurringType = selectQuery.Table("query").DefaultView[0].Row["RECURRING_TYPE_ID"];
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string GetPicSizesXMLParts(ref XmlDocument theDoc, Int32 nPicID, Int32 nGroupID, Int32 nMediaID, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics, string sPicsForCache)
        {
            if (nPicID == 0)
                nPicID = PageUtils.GetDefaultPICID(nGroupID);
            Int32 nLocalPicID = nPicID;
            StringBuilder sRet = new StringBuilder();
            Int32 nBrandHeight = 0;
            Int32 nBrandRecurring = 0;
            if (thePics == null)
            {
                if (sPicsForCache != "" && CachingManager.CachingManager.Exist("GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString()) == true && bWithCache == true)
                    return CachingManager.CachingManager.GetCachedData("GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString()).ToString();

                bool bCont = true;
                Int32 nPicSizeNum = 1;
                while (bCont == true)
                {
                    string sPicSize = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString());
                    string sPicSizeFormat = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_format");
                    string sPicSizeQuality = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_quality");
                    if (sPicSize == "")
                    {
                        bCont = false;
                        break;
                    }
                    else
                    {
                        string sBasePicsURL = "";
                        if (sPicSizeFormat != "")
                        {
                            Int32 nMediaFileID = GetMediaFileID(nMediaID, sPicSizeFormat, sPicSizeQuality, bIsAdmin, nGroupID, bWithCache);
                            object oRefID = null;
                            object oBrandHeight = null;
                            object oBrandRecurring = null;
                            GetMediaFilePicValues(nMediaFileID, ref oRefID, ref oBrandHeight, ref oBrandRecurring);
                            //ODBCWrapper.Utils.GetTableSingleVal("media_files", "ref_id", nMediaFileID, 86400);
                            if (oRefID != null && oRefID != DBNull.Value)
                                nPicID = int.Parse(oRefID.ToString());
                            if (oBrandHeight != null && oBrandHeight != DBNull.Value)
                                nBrandHeight = int.Parse(oBrandHeight.ToString());
                            if (oBrandRecurring != null && oBrandRecurring != DBNull.Value)
                                nBrandRecurring = int.Parse(oBrandRecurring.ToString());
                        }
                        sBasePicsURL = XMLEncode(ProtocolsFuncs.GetPicURL(nPicID, sPicSize), true);
                        sRet.Append(" pic_size").Append(nPicSizeNum).Append("=\"").Append(sBasePicsURL).Append("\" ");
                        sRet.Append(" pic_size").Append(nPicSizeNum).Append("_bh=\"").Append(nBrandHeight.ToString()).Append("\" ");
                        sRet.Append(" pic_size").Append(nPicSizeNum).Append("_br=\"").Append(nBrandRecurring.ToString()).Append("\" ");
                        nPicSizeNum++;
                    }
                    nPicID = nLocalPicID;
                }
                if (sPicsForCache != "")
                    CachingManager.CachingManager.SetCachedData("GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            }
            else
            {
                if (sPicsForCache != "" && CachingManager.CachingManager.Exist("ws.GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString()) == true && bWithCache == true)
                {
                    thePics = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>((ApiObjects.PicObject[])(CachingManager.CachingManager.GetCachedData("ws.GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString())));
                    //thePics = (ApiObjects.PicObject[])(CachingManager.CachingManager.GetCachedData("ws.GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString()));
                    return "";
                }
                for (int i = 0; i < thePics.Length; i++)
                {
                    string sPicSize = thePics[i].m_nPicWidth.ToString() + "X" + thePics[i].m_nPicHeight.ToString();
                    string sPicSizeFormat = "";
                    string sPicSizeQuality = "";
                    if (thePics[i].m_oFileRequestObj != null)
                    {
                        sPicSizeFormat = thePics[i].m_oFileRequestObj.m_sFileFormat;
                        sPicSizeQuality = thePics[i].m_oFileRequestObj.m_sFileQuality;
                    }
                    string sBasePicsURL = "";
                    if (sPicSizeFormat != "")
                    {
                        Int32 nMediaFileID = GetMediaFileID(nMediaID, sPicSizeFormat, sPicSizeQuality, bIsAdmin, nGroupID, bWithCache);
                        //object oRefID = ODBCWrapper.Utils.GetTableSingleVal("media_files", "ref_id", nMediaFileID, 86400);
                        object oRefID = null;
                        object oBrandHeight = null;
                        object oBrandRecurring = null;
                        GetMediaFilePicValues(nMediaFileID, ref oRefID, ref oBrandHeight, ref oBrandRecurring);
                        if (oRefID != null && oRefID != DBNull.Value)
                            nPicID = int.Parse(oRefID.ToString());
                        if (oBrandHeight != null && oBrandHeight != DBNull.Value)
                            nBrandHeight = int.Parse(oBrandHeight.ToString());
                        if (oBrandRecurring != null && oBrandRecurring != DBNull.Value)
                            nBrandRecurring = int.Parse(oBrandRecurring.ToString());
                    }
                    sBasePicsURL = XMLEncode(ProtocolsFuncs.GetPicURL(nPicID, sPicSize), true);
                    thePics[i].m_sPicURL = sBasePicsURL;
                    thePics[i].m_nPicBrandHeight = nBrandHeight;
                    thePics[i].m_nPicBrandRecurringType = nBrandRecurring;
                    nPicID = nLocalPicID;
                }
                if (sPicsForCache != "")
                {
                    ApiObjects.PicObject[] thePicsClone = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                    CachingManager.CachingManager.SetCachedData("ws.GetPicSizesXMLParts_" + sPicsForCache + nMediaID.ToString() + "_" + nPicID.ToString(), thePicsClone, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                }
            }
            return sRet.ToString();
        }

        static public string GetPicSizesXMLPartsForChannel(ref XmlDocument theDoc, Int32 nPicID, Int32 nGroupID, Int32 nChannelID, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics, string sPicsForCache)
        {
            if (nPicID == 0)
                nPicID = PageUtils.GetDefaultPICID(nGroupID);
            Int32 nLocalPicID = nPicID;
            StringBuilder sRet = new StringBuilder();
            if (thePics == null && theDoc != null)
            {
                if (sPicsForCache != "" && CachingManager.CachingManager.Exist("GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString()) == true && bWithCache == true)
                    return CachingManager.CachingManager.GetCachedData("GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString()).ToString();

                bool bCont = true;
                Int32 nPicSizeNum = 1;
                while (bCont == true)
                {
                    string sPicSize = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString());
                    string sPicSizeFormat = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_format");
                    string sPicSizeQuality = GetFlashVarsValue(ref theDoc, "pic_size" + nPicSizeNum.ToString() + "_quality");
                    if (sPicSize == "")
                    {
                        bCont = false;
                        break;
                    }
                    else
                    {
                        string sBasePicsURL = "";
                        sBasePicsURL = XMLEncode(ProtocolsFuncs.GetPicURL(nPicID, sPicSize), true);
                        sRet.Append(" pic_size").Append(nPicSizeNum).Append("=\"").Append(sBasePicsURL).Append("\" ");
                        nPicSizeNum++;
                    }
                    nPicID = nLocalPicID;
                }
                if (sPicsForCache != "")
                    CachingManager.CachingManager.SetCachedData("GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            }
            else if (theDoc == null && thePics != null)
            {
                if (sPicsForCache != "" && CachingManager.CachingManager.Exist("ws.GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString()) == true && bWithCache == true)
                {
                    thePics = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>((ApiObjects.PicObject[])(CachingManager.CachingManager.GetCachedData("ws.GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString())));
                    //thePics = (ApiObjects.PicObject[])(CachingManager.CachingManager.GetCachedData("ws.GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString()));
                    return "";
                }
                for (int i = 0; i < thePics.Length; i++)
                {
                    string sPicSize = thePics[i].m_nPicWidth.ToString() + "X" + thePics[i].m_nPicHeight.ToString();
                    string sPicSizeFormat = "";
                    string sPicSizeQuality = "";
                    if (thePics[i].m_oFileRequestObj != null)
                    {
                        sPicSizeFormat = thePics[i].m_oFileRequestObj.m_sFileFormat;
                        sPicSizeQuality = thePics[i].m_oFileRequestObj.m_sFileQuality;
                    }
                    string sBasePicsURL = "";
                    sBasePicsURL = XMLEncode(ProtocolsFuncs.GetPicURL(nPicID, sPicSize), true);
                    thePics[i].m_sPicURL = sBasePicsURL;
                    nPicID = nLocalPicID;
                }
                if (sPicsForCache != "")
                {
                    ApiObjects.PicObject[] thePicsClone = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                    CachingManager.CachingManager.SetCachedData("ws.GetPicSizesXMLPartsForChannel_" + sPicsForCache + nChannelID.ToString(), thePicsClone, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                }
            }
            return sRet.ToString();
        }

        static protected bool DoWeNeddBackUp(Int32 nGroupID, Int32 nCDNMainID)
        {
            bool bRet = false;
            return bRet;
            //Int32 nActive = 0;
            //Int32 nDelta = 0;
            //Int32 nCalls = 0;
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select CDN_BACKUP_ACTIVE,CDN_BACKUP_VAL from groups (nolock) where ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //    {
            //        nActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CDN_BACKUP_ACTIVE"].ToString());
            //        nDelta = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CDN_BACKUP_VAL"].ToString());
            //    }
            //}
            //selectQuery.Finish();
            //selectQuery = null;
            //if (nActive == 0)
            //    return false;
            //if (nDelta == 0)
            //    return true;
            //ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery1 += "select count(*) as co from watchers_media_actions (nolock) where action_id=1 and create_date>DATEADD(minute, - 1, GETDATE()) and ";
            //selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            //selectQuery1 += "and";
            //selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNMainID);
            //if (selectQuery1.Execute("query", true) != null)
            //{
            //    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
            //    if (nCount1 > 0)
            //    {
            //        nCalls = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["co"].ToString());
            //    }
            //}
            //selectQuery1.Finish();
            //selectQuery1 = null;
            //if (nCalls > nDelta)
            //    return true;
            //return false;
        }

        static public void GetLangData(string sLang, Int32 nGroupID, ref Int32 nLangID, ref bool bIsMain)
        {
            if (sLang == "")
                return;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select g.LANGUAGE_ID,ll.id from groups g (nolock) ,lu_languages ll (nolock)  where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.NAME)))", "=", sLang.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    Int32 nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                    if (nLangID == nMainLangID)
                        bIsMain = true;
                    else
                        bIsMain = false;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public bool IsLangMain(Int32 nGroupID, Int32 nLangID)
        {
            bool bIsMain = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select g.LANGUAGE_ID from groups g (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                    if (nLangID == nMainLangID)
                        bIsMain = true;
                    else
                        bIsMain = false;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bIsMain;
        }

        static public XmlNode GetInfoStructNode(ref XmlDocument theDoc, Int32 nGroupID, bool bWithCache)
        {
            ApiObjects.MediaInfoStructObject theWSInfoStruct = null;
            return GetInfoStructNode(ref theDoc, nGroupID, bWithCache, ref theWSInfoStruct);
        }

        static protected XmlNode ConvertInfoStructToInfoXML(ref ApiObjects.MediaInfoStructObject theWSInfoStruct)
        {
            StringBuilder sXML = new StringBuilder();
            sXML.Append("<info_struct statistics=\"");
            if (theWSInfoStruct.m_bStatistics == true)
                sXML.Append("true");
            else
                sXML.Append("false");
            sXML.Append("\" personal=\"");
            if (theWSInfoStruct.m_bPersonal == true)
                sXML.Append("true");
            else
                sXML.Append("false");
            sXML.Append("\">");
            if (theWSInfoStruct.m_bTitle == true)
                sXML.Append("<name/>");
            if (theWSInfoStruct.m_bDescription == true)
                sXML.Append("<description/>");
            if (theWSInfoStruct.m_bType == true)
                sXML.Append("<type/>");
            if (theWSInfoStruct.m_sMetaStrings != null)
            {
                Int32 nCount = theWSInfoStruct.m_sMetaStrings.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(theWSInfoStruct.m_sMetaStrings[i]).Append("\"/>");
                }
            }
            if (theWSInfoStruct.m_sMetaDoubles != null)
            {
                Int32 nCount = theWSInfoStruct.m_sMetaDoubles.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(theWSInfoStruct.m_sMetaDoubles[i]).Append("\"/>");
                }
            }
            if (theWSInfoStruct.m_sMetaBools != null)
            {
                Int32 nCount = theWSInfoStruct.m_sMetaBools.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(theWSInfoStruct.m_sMetaBools[i]).Append("\"/>");
                }
            }
            if (theWSInfoStruct.m_sTags != null)
            {
                sXML.Append("<tags>");
                Int32 nCount = theWSInfoStruct.m_sTags.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<tag_type name=\"").Append(theWSInfoStruct.m_sTags[i]).Append("\"/>");
                }
                sXML.Append("</tags>");
            }
            sXML.Append("</info_struct>");

            XmlNode theInfoStruct = null;
            XmlDocument x = new XmlDocument();
            x.LoadXml(HttpContext.Current.Server.HtmlDecode(sXML.ToString()));
            theInfoStruct = x.DocumentElement.Clone();
            return theInfoStruct;
        }

        static protected XmlDocument ConvertSearchStructToInfoXML(ref ApiObjects.SearchDefinitionObject theWSInfoObj)
        {
            StringBuilder sXML = new StringBuilder();
            sXML.Append("<root><request><search_data cut_with=\"");
            if (theWSInfoObj.m_eAndOr == ApiObjects.AndOr.And)
                sXML.Append("and");
            else
                sXML.Append("or");
            sXML.Append("\">");
            sXML.Append("<channel start_index=\"").Append(theWSInfoObj.m_oPageDefinition.m_nStartIndex.ToString()).Append("\" media_count=\"").Append(theWSInfoObj.m_oPageDefinition.m_nNumberOfItems.ToString()).Append("\"/>");
            sXML.Append("<cut_values exact=\"");
            if (theWSInfoObj.m_bExact == true)
                sXML.Append("true");
            else
                sXML.Append("false");
            sXML.Append("\">");
            if ((theWSInfoObj.m_dMaxDate.Year < 2099 && theWSInfoObj.m_dMaxDate.Year > 1) ||
                (theWSInfoObj.m_dMinDate.Year < 2099 && theWSInfoObj.m_dMinDate.Year > 1))
            {
                sXML.Append("<date ");
                if ((theWSInfoObj.m_dMinDate.Year < 2099 && theWSInfoObj.m_dMinDate.Year > 1))
                    sXML.Append("min_value=\"").Append(DateUtils.GetStrFromDate(theWSInfoObj.m_dMinDate)).Append("\" ");
                if ((theWSInfoObj.m_dMaxDate.Year < 2099 && theWSInfoObj.m_dMaxDate.Year > 1))
                    sXML.Append("max_value=\"").Append(DateUtils.GetStrFromDate(theWSInfoObj.m_dMaxDate)).Append("\"/>");
            }
            if (String.IsNullOrEmpty(theWSInfoObj.m_sTitle) == false)
                sXML.Append("<name value =\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_sTitle, true)).Append("\"/>");
            if (String.IsNullOrEmpty(theWSInfoObj.m_sDescription) == false)
                sXML.Append("<description value =\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_sDescription, true)).Append("\"/>");
            if (String.IsNullOrEmpty(theWSInfoObj.m_sTypeName) == false)
                sXML.Append("<type value=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_sTypeName, true)).Append("\"/>");

            if (theWSInfoObj.m_oMetaDoubleObjects != null)
            {
                Int32 nCount = theWSInfoObj.m_oMetaDoubleObjects.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oMetaDoubleObjects[i].m_sMetaName, true)).Append("\" ");
                    if (theWSInfoObj.m_oMetaDoubleObjects[i].m_oDoubleRange == null)
                        sXML.Append(" value=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oMetaDoubleObjects[i].m_dMetaValue.ToString(), true)).Append("\" ");
                    else
                    {
                        if (theWSInfoObj.m_oMetaDoubleObjects[i].m_oDoubleRange.m_dMin != -999999.999)
                            sXML.Append(" min_value=\"").Append(theWSInfoObj.m_oMetaDoubleObjects[i].m_oDoubleRange.m_dMin.ToString()).Append("\" ");
                        if (theWSInfoObj.m_oMetaDoubleObjects[i].m_oDoubleRange.m_dMax != -999999.999)
                            sXML.Append(" max_value=\"").Append(theWSInfoObj.m_oMetaDoubleObjects[i].m_oDoubleRange.m_dMax.ToString()).Append("\" ");
                    }
                    sXML.Append("/>");
                }
            }
            if (theWSInfoObj.m_oMetaStrObjects != null)
            {
                Int32 nCount = theWSInfoObj.m_oMetaStrObjects.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oMetaStrObjects[i].m_sMetaName, true)).Append("\" ");
                    sXML.Append(" value=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oMetaStrObjects[i].m_sMetaValue.ToString(), true)).Append("\" ");
                    sXML.Append("/>");
                }
            }
            if (theWSInfoObj.m_oMetaBoolObjects != null)
            {
                Int32 nCount = theWSInfoObj.m_oMetaBoolObjects.Length;
                for (int i = 0; i < nCount; i++)
                {
                    sXML.Append("<meta name=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oMetaBoolObjects[i].m_sMetaName, true)).Append("\" ");
                    if (theWSInfoObj.m_oMetaBoolObjects[i].m_bMetaValue == true)
                        sXML.Append(" value=\"true\" ");
                    else
                        sXML.Append(" value=\"false\" ");
                    sXML.Append("/>");
                }
            }
            if (theWSInfoObj.m_oTagObjects != null && theWSInfoObj.m_oTagObjects.Length > 0)
            {
                sXML.Append("<tags>");
                Int32 nCount = theWSInfoObj.m_oTagObjects.Length;
                for (int i = 0; i < nCount; i++)
                {
                    for (int j = 0; j < theWSInfoObj.m_oTagObjects[i].m_sMetaValues.Length; j++)
                    {
                        if (String.IsNullOrEmpty(theWSInfoObj.m_oTagObjects[i].m_sMetaValues[j]) == false)
                            sXML.Append("<tag_type name=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oTagObjects[i].m_sMetaName, true)).Append("\" value=\"").Append(ProtocolsFuncs.XMLEncode(theWSInfoObj.m_oTagObjects[i].m_sMetaValues[j], true)).Append("\"/>");
                    }
                }
                sXML.Append("</tags>");
            }
            sXML.Append("</cut_values>");
            if (theWSInfoObj.m_sOrderByObjects != null && theWSInfoObj.m_sOrderByObjects.Length > 0)
            {
                sXML.Append("<order_values> ");
                for (int i = 0; i < theWSInfoObj.m_sOrderByObjects.Length; i++)
                {
                    if (theWSInfoObj.m_sOrderByObjects[i] == null)
                        continue;
                    string sField = theWSInfoObj.m_sOrderByObjects[i].m_sOrderField.Trim().ToLower();
                    if (sField == "name" || sField == "title")
                        sXML.Append("<name ");
                    else if (sField == "description")
                        sXML.Append("<description ");
                    else if (sField == "date")
                        sXML.Append("<date ");
                    else if (sField == "views")
                        sXML.Append("<views ");
                    else if (sField == "rate")
                        sXML.Append("<rate ");
                    else if (sField == "random")
                        sXML.Append("<random value=\"true\" ");
                    else
                        sXML.Append("<meta name=\"").Append(ProtocolsFuncs.XMLEncode(sField, true)).Append("\" ");

                    sXML.Append(" order_dir=\"");
                    if (theWSInfoObj.m_sOrderByObjects[i].m_eOrderBy == ApiObjects.OrderDiretion.Asc)
                        sXML.Append("asc");
                    else
                        sXML.Append("desc");
                    sXML.Append("\" order_num=\"").Append(theWSInfoObj.m_sOrderByObjects[i].m_nOrderNum.ToString()).Append("\"/>");
                }
                sXML.Append("</order_values>");
            }
            sXML.Append("</search_data></request></root>");



            XmlDocument x = new XmlDocument();
            x.LoadXml(HttpContext.Current.Server.HtmlDecode(sXML.ToString()));

            return (XmlDocument)(x.Clone());
        }

        static public XmlNode GetInfoStructNode(ref XmlDocument theDoc, Int32 nGroupID, bool bWithCache, ref ApiObjects.MediaInfoStructObject theWSInfoStruct)
        {
            try
            {
                if (theDoc != null)
                {
                    XmlNode theInfoStruct = theDoc.SelectSingleNode("/root/request/params/info_struct");
                    if (theInfoStruct == null)
                    {
                        if (int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "USE_DEFAULT_INFO_STRUCT", nGroupID, 60).ToString()) == 1)
                        {
                            if (CachingManager.CachingManager.Exist("infoStruct_" + nGroupID.ToString()) == true && bWithCache == true)
                                return (XmlNode)(CachingManager.CachingManager.GetCachedData("infoStruct_" + nGroupID.ToString()));
                            object oInfoStruct = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_INFO_STRUCT", nGroupID);
                            if (oInfoStruct != null && oInfoStruct != DBNull.Value && oInfoStruct.ToString().Trim() != "")
                            {
                                XmlDocument x = new XmlDocument();
                                x.LoadXml(HttpContext.Current.Server.HtmlDecode(oInfoStruct.ToString().Replace("''", "\"")));
                                theInfoStruct = x.DocumentElement.Clone();
                            }
                            CachingManager.CachingManager.SetCachedData("infoStruct_" + nGroupID.ToString(), theInfoStruct, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                        }
                    }
                    return theInfoStruct;
                }
                else
                {
                    XmlNode theInfoStruct = null;
                    if (theWSInfoStruct == null)
                    {
                        if (int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "USE_DEFAULT_INFO_STRUCT", nGroupID, 60).ToString()) == 1)
                        {
                            if (CachingManager.CachingManager.Exist("infoStruct_" + nGroupID.ToString()) == true && bWithCache == true)
                                return (XmlNode)(CachingManager.CachingManager.GetCachedData("infoStruct_" + nGroupID.ToString()));
                            object oInfoStruct = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_INFO_STRUCT", nGroupID);
                            if (oInfoStruct != null && oInfoStruct != DBNull.Value && oInfoStruct.ToString().Trim() != "")
                            {
                                XmlDocument x = new XmlDocument();
                                x.LoadXml(HttpContext.Current.Server.HtmlDecode(oInfoStruct.ToString().Replace("''", "\"")));
                                theInfoStruct = x.DocumentElement.Clone();
                            }
                            CachingManager.CachingManager.SetCachedData("infoStruct_" + nGroupID.ToString(), theInfoStruct, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                        }
                    }
                    else
                        theInfoStruct = ConvertInfoStructToInfoXML(ref theWSInfoStruct);
                    return theInfoStruct;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }
        }

        static public string GetSafeURL(string sURL)
        {
            if (sURL.Trim() == "")
                return "";
            if (sURL.Trim().ToLower().StartsWith("http://") ||
                sURL.Trim().ToLower().StartsWith("https://") ||
                sURL.Trim().ToLower().StartsWith("javascript:") ||
                sURL.Trim().ToLower().StartsWith("mms://") ||
                sURL.Trim().ToLower().StartsWith("rtmp://") ||
                sURL.Trim().ToLower().StartsWith("rtmpe://"))
                return sURL;
            else
                return "http://" + sURL;
        }

        static protected void GetPLIValues(Int32 nID, ref string sURL, ref string sActionCode)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select oct.ACTION_CODE,ac.COMMERCIAL_URL from lu_outer_comm_types oct (nolock),ads_companies ac (nolock) where oct.id=ac.COMMERCIAL_TYPE_ID and ac.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ac.id", "=", nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sURL = selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_URL"].ToString();
                    sActionCode = selectQuery.Table("query").DefaultView[0].Row["ACTION_CODE"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected Int32 GetSafeInt(object o)
        {
            int result = 0;
            try
            {
                if (o != null && o != DBNull.Value)
                    int.TryParse(o.ToString(), out result);
            }
            catch (Exception e)
            {
                log.Error("", e);
            }
            return result;
        }

        static public string GetPlayListSchema(ref XmlDocument theDoc, Int32 nChannelID, Int32 nGroupID, Int32 nLangID, bool bIsMainLang, Int32 nWatcherID, Int32 nPlayerID, bool bWithCache)
        {
            ApiObjects.PlayListSchema obj = null;
            return GetPlayListSchema(ref theDoc, nChannelID, nGroupID, nLangID, bIsMainLang, nWatcherID, nPlayerID, bWithCache, ref obj);
        }

        static public string GetPlayListSchema(ref XmlDocument theDoc, Int32 nChannelID, Int32 nGroupID, Int32 nLangID, bool bIsMainLang, Int32 nWatcherID, Int32 nPlayerID, bool bWithCache, ref ApiObjects.PlayListSchema oPlayListSchema)
        {
            if (oPlayListSchema == null && CachingManager.CachingManager.Exist("playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString()) == true && bWithCache == true)
                return CachingManager.CachingManager.GetCachedData("playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString()).ToString();
            if (oPlayListSchema != null && CachingManager.CachingManager.Exist("ws.playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString()) == true && bWithCache == true)
            {
                oPlayListSchema = (ApiObjects.PlayListSchema)(CachingManager.CachingManager.GetCachedData("ws.playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString()));
                return "";
            }
            Int32 nDefPT = 0;
            object oDefPT = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PLAYLIST_TEMPLATE_ID", nGroupID);
            if (oDefPT != null && oDefPT != DBNull.Value)
                nDefPT = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PLAYLIST_TEMPLATE_ID", nGroupID).ToString());
            StringBuilder sRet = new StringBuilder();
            if (oPlayListSchema == null)
                sRet.Append("<playlist_schema channel_id=\"").Append(nChannelID).Append("\">");
            else
                oPlayListSchema.m_nChannelID = nChannelID;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select plt.* from play_list_items_templates_types plt (nolock) ";
            if (nChannelID != 0)
                selectQuery += ",channels c (nolock) ";
            selectQuery += " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("plt.group_id", "=", nGroupID);
            selectQuery += " and ";
            if (nChannelID != 0)
                selectQuery += " c.PLAYLIST_TEMPLATE_ID=plt.id and ";
            selectQuery += " plt.status=1 and plt.is_active=1";
            if (nChannelID != 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.id", "=", nChannelID);
            }
            else if (nDefPT != 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("plt.id", "=", nDefPT);
            }
            selectQuery += " order by plt.order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {

                    Int32 nOuterCommTypePre = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_PRE_ID"].ToString());
                    Int32 nOuterCommTypePost = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_POST_ID"].ToString());
                    Int32 nOuterCommTypeOverlay = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_OVERLAY_ID"].ToString());
                    Int32 nOuterCommTypeBreak = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_BREAK_ID"].ToString());

                    string sOuterCommURLPre = "";
                    string sOuterCommURLOverlay = "";
                    string sOuterCommURLBreak = "";
                    string sOuterCommURLPost = "";
                    string sOuterCommTypePre = "";
                    string sOuterCommTypePost = "";
                    string sOuterCommTypeOverlay = "";
                    string sOuterCommTypeBreak = "";

                    if (nOuterCommTypePre != 0)
                        GetPLIValues(nOuterCommTypePre, ref sOuterCommURLPre, ref sOuterCommTypePre);
                    if (nOuterCommTypePost != 0)
                        GetPLIValues(nOuterCommTypePost, ref sOuterCommURLPost, ref sOuterCommTypePost);
                    if (nOuterCommTypeOverlay != 0)
                        GetPLIValues(nOuterCommTypeOverlay, ref sOuterCommURLOverlay, ref sOuterCommTypeOverlay);
                    if (nOuterCommTypeBreak != 0)
                        GetPLIValues(nOuterCommTypeBreak, ref sOuterCommURLBreak, ref sOuterCommTypeBreak);

                    Int32 nOuterCommDeltaPre = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_PRE"].ToString());
                    Int32 nOuterCommDeltaPost = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_POST"].ToString());
                    Int32 nOuterCommDeltaOverlay = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_OVERLAY"].ToString());
                    Int32 nOuterCommStartOverlay = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_OVERLAY_START"].ToString());
                    Int32 nOuterCommDeltaBreak = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_BREAK"].ToString());
                    Int32 nOuterCommStartBreak = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_BREAK_START"].ToString());

                    Int32 nOuterCommSkipPre = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCIAL_SKIP_PRE"].ToString());
                    Int32 nOuterCommSkipPost = GetSafeInt(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCIAL_SKIP_POST"].ToString());

                    if (sOuterCommTypePre != "" && nOuterCommDeltaPre != 0)
                    {
                        if (oPlayListSchema == null)
                        {
                            sRet.Append("<outer_comm type=\"pre\" skip=\"");
                            if (nOuterCommSkipPre == 0)
                                sRet.Append("false");
                            else
                                sRet.Append("true");
                            sRet.Append("\" delta=\"").Append(nOuterCommDeltaPre).Append("\" impl_type=\"").Append(sOuterCommTypePre).Append("\" outer_commercial_url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLPre), true)).Append("\" />");
                        }
                        else
                        {
                            ApiObjects.MediaAdObject o = new ApiObjects.MediaAdObject();
                            bool bSkip = true;
                            if (nOuterCommSkipPre == 0)
                                bSkip = false;
                            o.Initialize("pre", bSkip, nOuterCommDeltaPre, sOuterCommTypePre, GetSafeURL(sOuterCommURLPre), 0.0, "");
                            oPlayListSchema.m_oPre = o;
                        }
                    }
                    if (sOuterCommTypePost != "" && nOuterCommDeltaPost != 0)
                    {
                        if (oPlayListSchema == null)
                        {
                            sRet.Append("<outer_comm type=\"post\" skip=\"");
                            if (nOuterCommSkipPost == 0)
                                sRet.Append("false");
                            else
                                sRet.Append("true");
                            sRet.Append("\" delta=\"").Append(nOuterCommDeltaPost).Append("\" impl_type=\"").Append(sOuterCommTypePost).Append("\" outer_commercial_url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLPost), true)).Append("\" />");
                        }
                        else
                        {
                            ApiObjects.MediaAdObject o = new ApiObjects.MediaAdObject();
                            bool bSkip = true;
                            if (nOuterCommSkipPost == 0)
                                bSkip = false;
                            o.Initialize("post", bSkip, nOuterCommDeltaPost, sOuterCommTypePost, GetSafeURL(sOuterCommURLPost), 0.0, "");
                            oPlayListSchema.m_oPre = o;
                        }
                    }
                    if (sOuterCommTypeOverlay != "" && nOuterCommDeltaOverlay != 0)
                    {
                        if (oPlayListSchema == null)
                        {
                            sRet.Append("<outer_comm type=\"overlay\" skip=\"");
                            sRet.Append("true");
                            sRet.Append("\" start=\"" + nOuterCommStartOverlay.ToString() + "\" delta=\"").Append(nOuterCommDeltaOverlay).Append("\" impl_type=\"").Append(sOuterCommTypeOverlay).Append("\" outer_commercial_url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLOverlay), true)).Append("\" />");
                        }
                        else
                        {
                            ApiObjects.MediaAdObject o = new ApiObjects.MediaAdObject();
                            bool bSkip = true;
                            o.Initialize("overlay", bSkip, nOuterCommDeltaOverlay, sOuterCommTypeOverlay, GetSafeURL(sOuterCommURLOverlay), nOuterCommStartOverlay, "");
                            oPlayListSchema.m_oPre = o;
                        }
                    }
                    if (sOuterCommTypeBreak != "" && nOuterCommDeltaBreak != 0)
                    {
                        if (oPlayListSchema == null)
                        {
                            sRet.Append("<outer_comm type=\"break\" skip=\"");
                            sRet.Append("false");
                            sRet.Append("\" start=\"" + nOuterCommStartBreak.ToString() + "\" delta=\"").Append(nOuterCommDeltaBreak).Append("\" impl_type=\"").Append(sOuterCommTypeBreak).Append("\" outer_commercial_url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLBreak), true)).Append("\" />");
                        }
                        else
                        {
                            ApiObjects.MediaAdObject o = new ApiObjects.MediaAdObject();
                            bool bSkip = false;
                            o.Initialize("break", bSkip, nOuterCommDeltaBreak, sOuterCommTypeBreak, GetSafeURL(sOuterCommURLBreak), nOuterCommStartBreak, "");
                            oPlayListSchema.m_oPre = o;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (oPlayListSchema == null)
            {
                sRet.Append("</playlist_schema>");
                CachingManager.CachingManager.SetCachedData("playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                return sRet.ToString();
            }
            else
            {
                CachingManager.CachingManager.SetCachedData("ws.playlistschema" + nChannelID.ToString() + "_" + nGroupID.ToString(), oPlayListSchema, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                return "";
            }
        }

        static protected void GetMediaAdsData(Int32 nMediaID, Int32 nMediaFileID, ref Int32 nCOMMERCIAL_TYPE_PRE_ID,
            ref Int32 nCOMMERCIAL_TYPE_POST_ID, ref Int32 nCOMMERCIAL_TYPE_BREAK_ID, ref Int32 nCOMMERCIAL_TYPE_OVERLAY_ID,
            ref Int32 nOwnerGroupID, ref Int32 nAdsEnabled, ref Int32 nDoesPlayerControllAds, ref Int32 nOwnerDefaultPlaylistSchema,
            ref Int32 nPreSkip, ref Int32 nPostSkip,
            ref string sBreakPoints, ref string sOverlayPoints)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.COMMERCIAL_BREAK_POINTS,mf.COMMERCIAL_OVERLAY_POINTS,mf.OUTER_COMMERCIAL_SKIP_PRE,mf.OUTER_COMMERCIAL_SKIP_POST,mf.COMMERCIAL_TYPE_PRE_ID , mf.COMMERCIAL_TYPE_POST_ID , mf.COMMERCIAL_TYPE_BREAK_ID , mf.COMMERCIAL_TYPE_OVERLAY_ID ,m.group_id,mf.ADS_ENABLED,m.PLAYER_CONTROL_ADS,g.DEFAULT_PLAYLIST_TEMPLATE_ID from  media_files mf (nolock),media m (nolock),groups g (nolock) where g.id=m.group_id and mf.media_id=m.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nOwnerGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    nDoesPlayerControllAds = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PLAYER_CONTROL_ADS"].ToString());
                    nAdsEnabled = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ADS_ENABLED"].ToString());
                    if (selectQuery.Table("query").DefaultView[0].Row["DEFAULT_PLAYLIST_TEMPLATE_ID"] != DBNull.Value)
                        nOwnerDefaultPlaylistSchema = int.Parse(selectQuery.Table("query").DefaultView[0].Row["DEFAULT_PLAYLIST_TEMPLATE_ID"].ToString());
                    nCOMMERCIAL_TYPE_PRE_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_PRE_ID"].ToString());
                    nCOMMERCIAL_TYPE_POST_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_POST_ID"].ToString());
                    nCOMMERCIAL_TYPE_BREAK_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_BREAK_ID"].ToString());
                    nCOMMERCIAL_TYPE_OVERLAY_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_OVERLAY_ID"].ToString());
                    nPostSkip = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCIAL_SKIP_POST"].ToString());
                    nPreSkip = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCIAL_SKIP_PRE"].ToString());
                    object oBreakPoints = selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_BREAK_POINTS"];
                    object oOverlayPoints = selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_OVERLAY_POINTS"];
                    if (oBreakPoints != DBNull.Value && oBreakPoints != null)
                        sBreakPoints = oBreakPoints.ToString();
                    if (oOverlayPoints != DBNull.Value && oOverlayPoints != null)
                        sOverlayPoints = oOverlayPoints.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected string GetAdsPlaylistSchema(Int32 nOuterCommTypePre, Int32 nOuterCommTypePost,
            Int32 nOuterCommTypeOverlay, Int32 nOuterCommTypeBreak, Int32 nPreSkip, Int32 nPostSkip,
            string sBreakPoints, string sOverlayPoints,
            ref ApiObjects.MediaAdObject thePreAdObject, ref ApiObjects.MediaAdObject theBreakAdObj,
            ref ApiObjects.MediaAdObject theOverlayAdObj, ref ApiObjects.MediaAdObject thePostAdObj)
        {
            StringBuilder sRet = new StringBuilder();
            string sOuterCommURLPre = "";
            string sOuterCommURLOverlay = "";
            string sOuterCommURLBreak = "";
            string sOuterCommURLPost = "";
            string sOuterCommTypePre = "";
            string sOuterCommTypePost = "";
            string sOuterCommTypeOverlay = "";
            string sOuterCommTypeBreak = "";

            if (nOuterCommTypePre != 0)
                GetPLIValues(nOuterCommTypePre, ref sOuterCommURLPre, ref sOuterCommTypePre);
            if (nOuterCommTypePost != 0)
                GetPLIValues(nOuterCommTypePost, ref sOuterCommURLPost, ref sOuterCommTypePost);
            if (nOuterCommTypeOverlay != 0)
                GetPLIValues(nOuterCommTypeOverlay, ref sOuterCommURLOverlay, ref sOuterCommTypeOverlay);
            if (nOuterCommTypeBreak != 0)
                GetPLIValues(nOuterCommTypeBreak, ref sOuterCommURLBreak, ref sOuterCommTypeBreak);

            if (thePreAdObject == null && sOuterCommTypePre != "")
            {
                sRet.Append("<ad type=\"pre\" impl_type=\"").Append(sOuterCommTypePre).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLPre), true)).Append("\" skip=\"");
                if (nPreSkip == 1)
                    sRet.Append("true");
                else
                    sRet.Append("false");
                sRet.Append("\"/>");
            }
            else if (sOuterCommTypePre != "")
            {
                bool bSkip = false;
                if (nPreSkip == 1)
                    bSkip = true;
                thePreAdObject.Initialize("pre", bSkip, 0, sOuterCommTypePre, sOuterCommURLPre, 0, "");
            }
            else
                thePreAdObject = null;
            if (thePostAdObj == null && sOuterCommTypePost != "")
            {
                sRet.Append("<ad type=\"post\" impl_type=\"").Append(sOuterCommTypePost).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLPost), true)).Append("\" skip=\"");
                if (nPostSkip == 1)
                    sRet.Append("true");
                else
                    sRet.Append("false");
                sRet.Append("\"/>");
            }
            else if (sOuterCommTypePost != "")
            {
                bool bSkip = false;
                if (nPostSkip == 1)
                    bSkip = true;
                thePostAdObj.Initialize("post", bSkip, 0, sOuterCommTypePost, sOuterCommURLPost, 0, "");
            }
            else
                thePostAdObj = null;
            if (theOverlayAdObj == null && (sOuterCommTypeOverlay != "" || sOverlayPoints != ""))
                sRet.Append("<ad type=\"overlay\" impl_type=\"").Append(sOuterCommTypeOverlay).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLOverlay), true)).Append("\" points=\"" + sOverlayPoints + "\" />");
            else if (sOuterCommTypeOverlay != "" || sOverlayPoints != "")
            {
                theOverlayAdObj.Initialize("overlay", false, 0, sOuterCommTypeOverlay, sOuterCommURLOverlay, 0, sOverlayPoints);
            }
            else
                theOverlayAdObj = null;
            if (theBreakAdObj == null && (sOuterCommTypeBreak != "" || sBreakPoints != ""))
                sRet.Append("<ad type=\"break\" impl_type=\"").Append(sOuterCommTypeBreak).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(GetSafeURL(sOuterCommURLBreak), true)).Append("\" points=\"" + sBreakPoints + "\" />");
            else if (sOuterCommTypeBreak != "" || sBreakPoints != "")
            {
                theBreakAdObj.Initialize("break", false, 0, sOuterCommTypeBreak, sOuterCommURLBreak, 0, sBreakPoints);
            }
            else
                theBreakAdObj = null;
            return sRet.ToString();
        }

        static public void GetMediaAdsDelats(Int32 nDefaultOwnerPlaylistSchema, ref Int32 nBreakDelta, ref Int32 nBreakStart, ref Int32 nOverlayDelta, ref Int32 nOverlayStart)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from play_list_items_templates_types (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDefaultOwnerPlaylistSchema);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nBreakDelta = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_BREAK"].ToString());
                    nBreakStart = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_BREAK_START"].ToString());
                    nOverlayDelta = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_DELTA_OVERLAY"].ToString());
                    nOverlayStart = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OUTER_COMMERCILA_PLI_OVERLAY_START"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string GetMediaAdsSchema(Int32 nMediaID, Int32 nMediaFileID, Int32 nGroupID, bool bWithCache,
            ref ApiObjects.MediaAdObject thePreAdObject, ref ApiObjects.MediaAdObject theBreakAdObj,
            ref ApiObjects.MediaAdObject theOverlayAdObj, ref ApiObjects.MediaAdObject thePostAdObj, ref bool bPlaylistSchemaControlled)
        {
            try
            {
                if (thePreAdObject == null && CachingManager.CachingManager.Exist("mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString()) == true && bWithCache == true)
                    return CachingManager.CachingManager.GetCachedData("mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString()).ToString();

                if (thePreAdObject != null && CachingManager.CachingManager.Exist("ws.mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString()) == true && bWithCache == true)
                {
                    ApiObjects.MediaAdObject[] t = (ApiObjects.MediaAdObject[])(CachingManager.CachingManager.GetCachedData("ws.mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString()));
                    thePreAdObject = t[0];
                    theBreakAdObj = t[1];
                    theOverlayAdObj = t[2];
                    thePostAdObj = t[3];
                    return "";
                }


                Int32 nOwnerGroupID = 0;
                Int32 nDoesPlayerControllAds = 0;
                Int32 nAdsEnabled = 0;
                Int32 nDefaultOwnerPlaylistSchema = 0;

                Int32 nCOMMERCIAL_TYPE_PRE_ID = 0;
                Int32 nCOMMERCIAL_TYPE_POST_ID = 0;
                Int32 nCOMMERCIAL_TYPE_BREAK_ID = 0;
                Int32 nCOMMERCIAL_TYPE_OVERLAY_ID = 0;

                Int32 nPreSkip = 0;
                Int32 nPostSkip = 0;

                string sBreakPoints = "";
                string sOverlayPoints = "";

                GetMediaAdsData(nMediaID, nMediaFileID, ref nCOMMERCIAL_TYPE_PRE_ID, ref nCOMMERCIAL_TYPE_POST_ID,
                    ref nCOMMERCIAL_TYPE_BREAK_ID, ref nCOMMERCIAL_TYPE_OVERLAY_ID,
                    ref nOwnerGroupID, ref nAdsEnabled, ref nDoesPlayerControllAds, ref nDefaultOwnerPlaylistSchema,
                    ref nPreSkip, ref nPostSkip, ref sBreakPoints, ref sOverlayPoints);

                string sAdsEnabled = "true";
                if (nAdsEnabled == 0)
                    sAdsEnabled = "false";

                Int32 nOuterCommTypePre = 0;
                Int32 nOuterCommTypePost = 0;
                Int32 nOuterCommTypeOverlay = 0;
                Int32 nOuterCommTypeBreak = 0;

                if (nDoesPlayerControllAds == 1)
                {
                    object o = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PLAYLIST_TEMPLATE_ID", nGroupID);
                    if (o != DBNull.Value && o != null)
                        nDefaultOwnerPlaylistSchema = int.Parse(o.ToString());
                }
                Int32 nBreakDelta = 0;
                Int32 nBreakStart = 0;
                Int32 nOverlayDelta = 0;
                Int32 nOverlayStart = 0;

                GetMediaAdsDelats(nDefaultOwnerPlaylistSchema, ref nBreakDelta, ref nBreakStart, ref nOverlayDelta, ref nOverlayStart);

                StringBuilder sRet = new StringBuilder();
                bPlaylistSchemaControlled = true;


                string sAdsSchema = "";
                if (nAdsEnabled == 1)
                {
                    if ((nDoesPlayerControllAds == 1 && nGroupID == nOwnerGroupID) ||
                        (nDoesPlayerControllAds == 0 && nGroupID == nOwnerGroupID))
                    {
                        //ads_schema if exists
                        if (nCOMMERCIAL_TYPE_PRE_ID != 0 || nCOMMERCIAL_TYPE_POST_ID != 0
                            || nCOMMERCIAL_TYPE_BREAK_ID != 0 || nCOMMERCIAL_TYPE_OVERLAY_ID != 0 ||
                            sOverlayPoints != "" || sBreakPoints != "")
                        {
                            if (nCOMMERCIAL_TYPE_PRE_ID == 0 && nCOMMERCIAL_TYPE_POST_ID == 0
                                && nCOMMERCIAL_TYPE_BREAK_ID == 0 && nCOMMERCIAL_TYPE_OVERLAY_ID == 0)
                                bPlaylistSchemaControlled = true;
                            else
                                bPlaylistSchemaControlled = false;
                            sAdsSchema = GetAdsPlaylistSchema(nCOMMERCIAL_TYPE_PRE_ID, nCOMMERCIAL_TYPE_POST_ID,
                                nCOMMERCIAL_TYPE_OVERLAY_ID, nCOMMERCIAL_TYPE_BREAK_ID, nPreSkip, nPostSkip, sBreakPoints, sOverlayPoints,
                                ref thePreAdObject, ref theBreakAdObj, ref theOverlayAdObj, ref thePostAdObj);
                        }
                    }

                    if (nDoesPlayerControllAds == 0 && nGroupID != nOwnerGroupID)
                    {
                        bPlaylistSchemaControlled = false;
                        if (nCOMMERCIAL_TYPE_PRE_ID != 0 || nCOMMERCIAL_TYPE_POST_ID != 0
                            || nCOMMERCIAL_TYPE_BREAK_ID != 0 || nCOMMERCIAL_TYPE_OVERLAY_ID != 0)
                        {
                            sAdsSchema = GetAdsPlaylistSchema(nCOMMERCIAL_TYPE_PRE_ID, nCOMMERCIAL_TYPE_POST_ID,
                                nCOMMERCIAL_TYPE_OVERLAY_ID, nCOMMERCIAL_TYPE_BREAK_ID, nPreSkip, nPostSkip, sBreakPoints, sOverlayPoints
                                 , ref thePreAdObject, ref theBreakAdObj, ref theOverlayAdObj, ref thePostAdObj);
                        }
                        else
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery += "select plt.* from play_list_items_templates_types plt (nolock) ";
                            selectQuery += " where plt.status=1 and plt.is_active=1 and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("plt.id", "=", nDefaultOwnerPlaylistSchema);
                            if (selectQuery.Execute("query", true) != null)
                            {
                                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                if (nCount > 0)
                                {
                                    nOuterCommTypePre = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_PRE_ID"].ToString());
                                    nOuterCommTypePost = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_POST_ID"].ToString());
                                    nOuterCommTypeOverlay = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_OVERLAY_ID"].ToString());
                                    nOuterCommTypeBreak = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_TYPE_BREAK_ID"].ToString());

                                    sAdsSchema = GetAdsPlaylistSchema(nOuterCommTypePre, nOuterCommTypePost, nOuterCommTypeOverlay, nOuterCommTypeBreak, nPreSkip, nPostSkip, sBreakPoints, sOverlayPoints,
                                        ref thePreAdObject, ref theBreakAdObj, ref theOverlayAdObj, ref thePostAdObj);
                                }
                            }
                            selectQuery.Finish();
                            selectQuery = null;
                        }
                        //ads_schema and if not default owner Playlist schema - nDefaultOwnerPlaylistSchema
                    }
                }
                if (thePreAdObject == null)
                {
                    if (sAdsSchema != "")
                    {
                        sRet.Append("<ads_schema enable=\"").Append(sAdsEnabled).Append("\" break_delta=\"" + nBreakDelta.ToString() + "\" break_start=\"" + nBreakStart.ToString() + "\" overlay_delta=\"" + nOverlayDelta.ToString() + "\" overlay_start=\"" + nOverlayStart.ToString() + "\" playlist_schema_controlled=\"" + bPlaylistSchemaControlled.ToString().ToLower() + "\">");
                        sRet.Append(sAdsSchema);
                        sRet.Append("</ads_schema>");
                    }
                }
                else
                {
                    if (nAdsEnabled == 0)
                    {
                        thePreAdObject = null;
                        thePostAdObj = null;
                        theBreakAdObj = null;
                        theOverlayAdObj = null;
                    }
                    theBreakAdObj.m_nDelta = nBreakDelta;
                    theBreakAdObj.m_dStartSec = nBreakStart;
                    theOverlayAdObj.m_nDelta = nOverlayDelta;
                    theOverlayAdObj.m_dStartSec = nOverlayStart;
                }

                if (thePreAdObject == null)
                    CachingManager.CachingManager.SetCachedData("mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);

                if (thePreAdObject != null)
                {
                    ApiObjects.MediaAdObject[] t = new ApiObjects.MediaAdObject[4];
                    if (thePreAdObject.m_sUrl == "")
                        thePreAdObject = null;
                    t[0] = thePreAdObject;
                    if (theBreakAdObj.m_sUrl == "")
                        theBreakAdObj = null;
                    t[1] = theBreakAdObj;
                    if (theOverlayAdObj.m_sUrl == "")
                        theOverlayAdObj = null;
                    t[2] = theOverlayAdObj;
                    if (thePostAdObj.m_sUrl == "")
                        thePostAdObj = null;
                    t[3] = thePostAdObj;
                    CachingManager.CachingManager.SetCachedData("ws.mediaadsschema" + nMediaFileID.ToString() + "_" + nGroupID.ToString(), t, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                }
                return sRet.ToString();
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return "";
            }
        }

        static protected Int32 GetBillingTypeID(string sBillingType)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from lu_billing_type (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(api_val)))", "=", sBillingType.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 GetStreamCompByFileId(Int32 nMediaFileId)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select STREAMING_SUPLIER_ID from media_files (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileId);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (selectQuery.Table("query").DefaultView[0].Row["STREAMING_SUPLIER_ID"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["STREAMING_SUPLIER_ID"] != null)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STREAMING_SUPLIER_ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 GetOwnerMediaGroup(Int32 nMediaID)
        {
            try
            {
                object oOwner = ODBCWrapper.Utils.GetTableSingleVal("media", "GROUP_ID", nMediaID, 3600);
                if (oOwner != null && oOwner != DBNull.Value)
                {
                    return int.Parse(oOwner.ToString());
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public Int32 GetFriendlyFormatID(Int32 nGroupID, Int32 nFormatID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select id from groups_media_type (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nFormatID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetBrowserID(string sBrowser, bool bCreate)
        {
            Int32 nRet = -1;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bCreate == false)
                selectQuery.SetCachedSec(0);
            else
                selectQuery.SetCachedSec(86400);
            selectQuery += "select id from browsers (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(BROWSER)))", "=", sBrowser.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nRet == -1 && bCreate == true)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("browsers");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", sBrowser.Trim().ToLower());
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return GetBrowserID(sBrowser, false);
            }
            if (nRet == -1)
                nRet = 0;
            return nRet;
        }

        static protected Int32 GetPlatformID(string sPlatform, bool bCreate)
        {
            Int32 nRet = -1;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bCreate == false)
                selectQuery.SetCachedSec(0);
            else
                selectQuery.SetCachedSec(86400);
            selectQuery += "select id from platforms (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", sPlatform.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nRet == -1 && bCreate == true)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("platforms");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", sPlatform.Trim().ToLower());
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return GetPlatformID(sPlatform, false);
            }
            if (nRet == -1)
                nRet = 0;
            return nRet;
        }

        static protected string GetSessionID(ref Int32 nBrowser, ref Int32 nPlatform)
        {
            if (HttpContext.Current.Request.Browser != null)
            {
                string sBrowser = HttpContext.Current.Request.Browser.Type;
                string sPlatform = HttpContext.Current.Request.Browser.Platform;

                nBrowser = GetBrowserID(sBrowser, true);
                nPlatform = GetPlatformID(sPlatform, true);
            }

            return HttpContext.Current.Session.SessionID;
        }

        static protected Int32 GetActionValues(string sAction, ref bool bEOH)
        {
            Int32 nIsEOH = 0;
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select ID,IS_EOH from lu_media_action_type (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LOWER(API_VAL)", "=", sAction.ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nIsEOH = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_EOH"].ToString());
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nIsEOH == 0)
                bEOH = false;
            else
                bEOH = true;
            return nRet;
        }

        //static protected void InsertNewEOHStatistics(Int32 nGroupID, Int32 nOwnerGroupID,
        //    Int32 nMediaID, Int32 nMediaFileID, Int32 nBillingTypeID, Int32 nCDNID, Int32 nCountryID, Int32 nPlayerID,
        //    Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID,
        //    Int32 nDuration, Int32 nBrowser, Int32 nPlatform, int nCurrentLocation, Int32 nPlayCounter, Int32 nFirstPlayCounter, Int32 nLoadCounter, Int32 nPauseCounter, Int32 nStopCounter, Int32 nFullScreenCounter,
        //    Int32 nExitFullScreenCounter, Int32 nSendToFriendCounter, string sPlayCycleID, string sSiteGUID, string sUDID)
        //{

        //    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_eoh");
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_COUNTER", "=", nPlayCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_PLAY_COUNTER", "=", nFirstPlayCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOAD_COUNTER", "=", nLoadCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAUSE_COUNTER", "=", nPauseCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STOP_COUNTER", "=", nStopCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_SCREEN_COUNTER", "=", nFullScreenCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXIT_FULL_SCREEN_COUNTER", "=", nExitFullScreenCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SEND_TO_FRIEND_COUNTER", "=", nSendToFriendCounter);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", nCurrentLocation);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 0);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", nBrowser);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);

        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_CYCLE_ID", "=", sPlayCycleID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
        //    insertQuery.Execute();
        //    insertQuery.Finish();
        //    insertQuery = null;

        //}

        //static protected Int32 GetEOHStatistics(Int32 nGroupID, Int32 nOwnerGroupID,
        //    Int32 nMediaID, Int32 nMediaFileID, Int32 nBillingTypeID, Int32 nCDNID, Int32 nCountryID, Int32 nPlayerID,
        //    Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID, ref long playTime)
        //{
        //    Int32 nRet = 0;
        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery.SetWritable(true);
        //    selectQuery.SetCachedSec(0);
        //    selectQuery += "select id, PLAY_TIME_COUNTER from media_eoh WITH (nolock) where ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
        //    //selectQuery += " WITH (index(idx_name))  ";
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
        //        if (nCount > 0)
        //        {
        //            nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        //            playTime = long.Parse(selectQuery.Table("query").DefaultView[0].Row["PLAY_TIME_COUNTER"].ToString());
        //        }
        //    }
        //    selectQuery.Finish();
        //    selectQuery = null;
        //    return nRet;
        //}

        static public void AddVideoQualityStatistics(int groupID, int mediaID, int mediaFileID, int avgMaxBitRate, int currentBitRateInd,
            int totalBitRateNum, int watcherID, int siteGuid, int location, string sessionID, int browser, int platform, int countryID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_file_video_quality");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", watcherID);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", mediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", mediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", siteGuid);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", countryID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sessionID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", browser);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", platform);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("avg_max_bit_rate", "=", avgMaxBitRate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("bit_rate_index", "=", currentBitRateInd);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("total_bit_rates_num", "=", totalBitRateNum);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("location_sec", "=", location);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }

        static private void AddPlayCycleKey(Int32 nGroupID, Int32 nMediaID, Int32 nMediaFileID, string sSiteGuid, Int32 nPlatform, string sUDID, Int32 nCountryID, string sPlayCycleKey)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("play_cycle_keys");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", sSiteGuid);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", nPlatform);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", sUDID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", nCountryID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_CYCLE_KEY", sPlayCycleKey);

            insertQuery.Execute();
            insertQuery.Finish();
        }

        static private string GetLastPlayCycleKey(string sSiteGuid, Int32 nMediaID, Int32 nMediaFileID, string sUDID, Int32 nGroupID, Int32 nPlatform, Int32 nCountryID)
        {
            string retVal = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);

            selectQuery += "SELECT top 1 PLAY_CYCLE_KEY from play_cycle_keys where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGuid);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
            selectQuery += " ORDER BY ID DESC";
            selectQuery.SetWritable(true);

            DataTable dt = selectQuery.Execute("query", true);
            selectQuery.Finish();
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    retVal = dt.Rows[0][0].ToString();
                }
            }

            if (string.IsNullOrEmpty(retVal))
            {
                retVal = Guid.NewGuid().ToString();
                AddPlayCycleKey(nGroupID, nMediaID, nMediaFileID, sSiteGuid, nPlatform, sUDID, nCountryID, retVal);
            }

            return retVal;
        }

        static public void UpdateEOHStatistics(Int32 nGroupID, Int32 nOwnerGroupID,
            Int32 nMediaID, Int32 nMediaFileID, Int32 nBillingTypeID, Int32 nCDNID, Int32 nCountryID, Int32 nPlayerID,
            Int32 nFileQualityID, Int32 nFileFormatID, DateTime dCountDate, Int32 nWatcherID, string sSessionID, Int32 nDuration,
            Int32 nFirstPlayCounter, Int32 nPlayCounter, Int32 nLoadCounter, Int32 nPauseCounter, Int32 nStopCounter,
            Int32 nFullScreenCounter, Int32 nExitFullScreenCounter, Int32 nSendToFriendCounter, Int32 nCurrentLocation, Int32 nFinish, Int32 nBrowser, Int32 nPlatform, string sSiteGUID, string sUDID, string sPlayCycleID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_eoh");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DURATION", "=", nDuration);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nFileQualityID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFileFormatID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "=", dCountDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_COUNTER", "=", nPlayCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_PLAY_COUNTER", "=", nFirstPlayCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOAD_COUNTER", "=", nLoadCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAUSE_COUNTER", "=", nPauseCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STOP_COUNTER", "=", nStopCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FULL_SCREEN_COUNTER", "=", nFullScreenCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXIT_FULL_SCREEN_COUNTER", "=", nExitFullScreenCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SEND_TO_FRIEND_COUNTER", "=", nSendToFriendCounter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", nCurrentLocation);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", nBrowser);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_CYCLE_ID", "=", sPlayCycleID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;


        }

        static public string HitProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nCountryID, Int32 nPlayerID,
            ref ApiObjects.InitializationObject initObj, Int32 nWSLocationInVideoSec, ref ApiObjects.MediaFileObject theMediaFileObj)
        {
            StringBuilder sRet = new StringBuilder();
            DateTime dNow = DateTime.UtcNow;
            Int32 nMediaID = 0;
            string sFileFormat = "";
            string sFileQuality = "";
            Int32 nMediaFileID = 0;
            Int32 nBillingTypeID = 0;
            Int32 nCDNID = 0;
            Int32 nMediaDurationInSec = 0;
            int avgBitRate = 0;
            int totalBitRates = 0;
            int currentBitRate = 0;
            double nLoc = 0;

            string sUDID = string.Empty;
            string sAction = string.Empty;

            if (theDoc != null)
            {
                sRet.Append("<response type=\"hit\">");

                XmlNode theAction = theDoc.SelectSingleNode("/root/request/@action");
                sAction = theAction.Value.ToLower().Trim();

                if (theDoc.SelectSingleNode("/root/request/watching") != null)
                {
                    XmlNode theLocSec = theDoc.SelectSingleNode("/root/request/watching/@location_sec");
                    if (theLocSec != null && theLocSec.Value.Trim() != "")
                        nLoc = double.Parse(theLocSec.Value);

                    XmlNode theDeviceUDID = theDoc.SelectSingleNode("/root/request/watching/@device_udid");
                    if (theDeviceUDID != null && theDeviceUDID.Value.Trim() != "")
                        sUDID = theDeviceUDID.Value;
                }

                string sMediaID = "0";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/watching/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();
                nMediaID = int.Parse(sMediaID);

                string sAvgBitRate = "0";
                XmlNode theAvgBitRate = theDoc.SelectSingleNode("/root/request/watching/media/@avg_bit_rate_num");
                if (theAvgBitRate != null)
                    sAvgBitRate = theAvgBitRate.Value.ToUpper();
                avgBitRate = int.Parse(sAvgBitRate);

                string sCurrentBitRate = "0";
                XmlNode theCurrentBitRate = theDoc.SelectSingleNode("/root/request/watching/media/@current_bit_rate_num");
                if (theAvgBitRate != null)
                    sCurrentBitRate = theCurrentBitRate.Value.ToUpper();
                currentBitRate = int.Parse(sCurrentBitRate);

                string sTotalBitRate = "0";
                XmlNode theTotalBitRate = theDoc.SelectSingleNode("/root/request/watching/media/@total_bit_rate_num");
                if (theAvgBitRate != null)
                    sTotalBitRate = theTotalBitRate.Value.ToUpper();
                totalBitRates = int.Parse(sTotalBitRate);

                XmlNode theMediaFileFormat = theDoc.SelectSingleNode("/root/request/watching/media/@orig_file_format");
                if (theMediaFileFormat != null)
                    sFileFormat = theMediaFileFormat.Value.ToUpper();

                XmlNode theMediaFileQuality = theDoc.SelectSingleNode("/root/request/watching/media/@file_quality");
                if (theMediaFileQuality != null)
                    sFileQuality = theMediaFileQuality.Value.ToUpper();

                XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/watching/media/@file_id");
                string sMediaFileID = "";
                if (theMediaFileID != null)
                    sMediaFileID = theMediaFileID.Value.ToUpper();

                if (sMediaFileID != "")
                    nMediaFileID = int.Parse(sMediaFileID);


                XmlNode theBillingTypeID = theDoc.SelectSingleNode("/root/request/watching/media/@billing");
                string sBillingTypeID = "";
                if (theBillingTypeID != null)
                    sBillingTypeID = theBillingTypeID.Value.ToUpper();
                nBillingTypeID = GetBillingTypeID(sBillingTypeID);
                if (nMediaFileID != 0)
                    nCDNID = GetStreamCompByFileId(nMediaFileID);

                XmlNode theMediaDuration = theDoc.SelectSingleNode("/root/request/watching/media/@duration");
                string sMediaDuration = "";
                if (theMediaDuration != null)
                    sMediaDuration = theMediaDuration.Value.ToUpper();

                if (sMediaDuration.Trim() == "")
                    sMediaDuration = "0";
                nMediaDurationInSec = int.Parse(sMediaDuration);
                sRet.Append("</response>");
            }
            else
            {
                nLoc = nWSLocationInVideoSec;
                nMediaID = theMediaFileObj.m_nMediaID;
                sFileFormat = theMediaFileObj.m_sFileFormat;
                sFileQuality = theMediaFileObj.m_sFileQuality;
                nMediaFileID = theMediaFileObj.m_nFileID;
                if (nMediaFileID != 0)
                    nCDNID = GetStreamCompByFileId(nMediaFileID);
                nMediaDurationInSec = (int)(theMediaFileObj.m_dDuration);
                nLoc = nWSLocationInVideoSec;
                nBillingTypeID = GetBillingTypeID(theMediaFileObj.m_sBilling);
            }

            if (nMediaID != 0)
            {
                Int32 nOwnerGroupID = GetOwnerMediaGroup(nMediaID);
                Int32 nQualityID = ProtocolsFuncs.GetFileQualityID(sFileQuality);
                Int32 nFormatID = ProtocolsFuncs.GetFileTypeID(sFileFormat, nGroupID);
                Int32 nFriendlyFormatID = GetFriendlyFormatID(nGroupID, nFormatID);
                Int32 nPlay = 0;
                Int32 nFirstPlay = 0;
                Int32 nLoad = 0;
                Int32 nPause = 0;
                Int32 nStop = 0;
                Int32 nFull = 0;
                Int32 nExitFull = 0;
                Int32 nFinish = 0;
                Int32 nSendToFriend = 0;
                Int32 nPlayTime = 30;
                if (nLoc > 0)
                {
                    nPlayTime = (int)nLoc;
                }

                Int32 nBrowser = 0;
                Int32 nPlatform = 0;
                string sSessionID = GetSessionID(ref nBrowser, ref nPlatform);

                string sPlayCycleKey = GetLastPlayCycleKey(sSiteGUID, nMediaID, nMediaFileID, sUDID, nGroupID, nPlatform, nCountryID);

                UpdateEOHStatistics(nGroupID, nOwnerGroupID, nMediaID, nMediaFileID, nBillingTypeID, nCDNID, nCountryID
                        , nPlayerID, nQualityID, nFormatID, dNow, nWatcherID, sSessionID, nMediaDurationInSec,
                        nFirstPlay, nPlay, nLoad, nPause, nStop, nFull, nExitFull, nSendToFriend, nPlayTime, nFinish, nBrowser, nPlatform, sSiteGUID, sUDID, sPlayCycleKey);

                if (!sAction.ToLower().Equals("bitrate_change"))
                {
                    UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, nPlayTime, sUDID);
                }

                if (avgBitRate > 0)
                {
                    int siteGuid = 0;
                    if (!string.IsNullOrEmpty(sSiteGUID))
                    {
                        siteGuid = int.Parse(sSiteGUID);
                    }
                    AddVideoQualityStatistics(nGroupID, nMediaID, nMediaFileID, avgBitRate, currentBitRate, totalBitRates, nWatcherID, siteGuid, nPlayTime, sSessionID, nBrowser, nPlatform, nCountryID);
                }
            }
            return sRet.ToString();
        }

        //static public DateTime GetDateForMediaEOH()
        //{
        //    object t = null;
        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery += "select getdate() as t ";
        //    selectQuery.SetCachedSec(0);
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
        //        if (nCount > 0)
        //            t = selectQuery.Table("query").DefaultView[0].Row["t"];
        //    }
        //    selectQuery.Finish();
        //    selectQuery = null;
        //    DateTime theDate = DateTime.Now;
        //    if (t != null && t != DBNull.Value)
        //        theDate = (DateTime)t;
        //    return new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour, 0, 0);

        //}



        static public string MediaMark(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nCountryID, Int32 nPlayerID,
            ref ApiObjects.InitializationObject initObj, string sWSAction, Int32 nWSLocationSec,
            ref ApiObjects.MediaFileObject theMediaFileObj)
        {
            string sAction = "";
            Int32 nLoc = 0;
            Int32 nMediaFileID = 0;
            Int32 nMediaID = 0;
            Int32 nMediaDuration = 0;
            Int32 nOwnerGroupID = 0;
            Int32 nQualityID = 0;
            Int32 nFormatID = 0;
            Int32 nFriendlyFormatID = 0;
            Int32 nPlay = 0;
            Int32 nFirstPlay = 0;
            Int32 nLoad = 0;
            Int32 nPause = 0;
            Int32 nStop = 0;
            Int32 nFull = 0;
            Int32 nExitFull = 0;
            Int32 nFinish = 0;
            Int32 nSendToFriend = 0;
            Int32 nPlayTime = 0;
            DateTime dNow = DateTime.UtcNow;
            Int32 nBrowser = 0;
            Int32 nPlatform = 0;
            Int32 nActionID = 0;
            string sFileQuality = "";
            bool bEOH = true;
            string sFileFormat = "";
            Int32 nBillingTypeID = 0;
            Int32 nCDNID = 0;
            string sSessionID = "";
            int avgBitRate = 0;
            int totalBitRates = 0;
            int currentBitRate = 0;
            string sPlayCycleKey = "";

            StringBuilder sRet = new StringBuilder();

            string sUDID = string.Empty;

            if (theDoc != null)
            {
                if (theDoc.SelectSingleNode("/root/request/mark") != null)
                {
                    XmlNode theAction = theDoc.SelectSingleNode("/root/request/mark/@action");
                    if (theAction != null)
                        sAction = theAction.Value.ToUpper();
                }
                if (theDoc.SelectSingleNode("/root/request/mark") != null)
                {
                    XmlNode theLocSec = theDoc.SelectSingleNode("/root/request/mark/@location_sec");
                    if (theLocSec != null && theLocSec.Value.Trim() != "")
                        nLoc = int.Parse(theLocSec.Value);

                    XmlNode theDeviceUDID = theDoc.SelectSingleNode("/root/request/mark/@device_udid");
                    if (theDeviceUDID != null && theDeviceUDID.Value.Trim() != "")
                        sUDID = theDeviceUDID.Value;
                }
                sRet.Append("<response type=\"media_mark\">");
                sRet.Append("</response>");
            }
            else
            {
                sRet.Append("OK");
                sAction = sWSAction;
                nLoc = nWSLocationSec;
            }
            if (sAction != "")
            {
                if (theDoc != null)
                {
                    XmlNode theMedia = theDoc.SelectSingleNode("/root/request/mark/media/@id");
                    string sMedia = "";
                    if (theMedia != null)
                        sMedia = theMedia.Value.ToUpper();

                    XmlNode theMediaDuration = theDoc.SelectSingleNode("/root/request/mark/media/@duration");
                    string sMediaDuration = "";
                    if (theMediaDuration != null)
                        sMediaDuration = theMediaDuration.Value.ToUpper();

                    if (sMediaDuration.Trim() == "")
                        sMediaDuration = "0";
                    nMediaDuration = int.Parse(sMediaDuration);
                    XmlNode theMediaFile = theDoc.SelectSingleNode("/root/request/mark/media/@file_id");
                    string sMediaFile = "";
                    if (theMediaFile != null)
                        sMediaFile = theMediaFile.Value.ToUpper();

                    XmlNode theBillingType = theDoc.SelectSingleNode("/root/request/mark/media/@billing");
                    string sBillingType = "";
                    if (theBillingType != null)
                        sBillingType = theBillingType.Value.ToUpper();

                    nBillingTypeID = GetBillingTypeID(sBillingType);

                    if (sMediaFile != "")
                    {
                        nMediaFileID = int.Parse(sMediaFile);
                        nCDNID = GetStreamCompByFileId(int.Parse(sMediaFile));
                    }

                    XmlNode theMediaCDN = theDoc.SelectSingleNode("/root/request/mark/media/@cdn_id");
                    string sMediaCDN = "";
                    if (theMediaCDN != null)
                        sMediaCDN = theMediaCDN.Value.ToUpper();

                    XmlNode theMediaFileFormat = theDoc.SelectSingleNode("/root/request/mark/media/@orig_file_format");
                    if (theMediaFileFormat != null)
                        sFileFormat = theMediaFileFormat.Value.ToUpper();

                    XmlNode theMediaFileQuality = theDoc.SelectSingleNode("/root/request/mark/media/@file_quality");
                    if (theMediaFileQuality != null)
                        sFileQuality = theMediaFileQuality.Value.ToUpper();

                    //Bit rate stats
                    XmlNode theAvgBitRate = theDoc.SelectSingleNode("/root/request/mark/media/@avg_bit_rate_num");
                    if (theAvgBitRate != null)
                        avgBitRate = int.Parse(theAvgBitRate.Value);

                    XmlNode theTotalBitRate = theDoc.SelectSingleNode("/root/request/mark/media/@total_bit_rate_num");
                    if (theTotalBitRate != null)
                        totalBitRates = int.Parse(theTotalBitRate.Value);

                    XmlNode theCurrentBitRate = theDoc.SelectSingleNode("/root/request/mark/media/@current_bit_rate_num");
                    if (theCurrentBitRate != null)
                        currentBitRate = int.Parse(theCurrentBitRate.Value);

                    nMediaID = int.Parse(sMedia);
                }
                else
                {
                    nMediaDuration = (Int32)(theMediaFileObj.m_dDuration);
                    string sBillingType = theMediaFileObj.m_sBilling;
                    nBillingTypeID = GetBillingTypeID(sBillingType);
                    nMediaFileID = theMediaFileObj.m_nFileID;
                    nCDNID = GetStreamCompByFileId(nMediaFileID);
                    sFileFormat = theMediaFileObj.m_sFileFormat;
                    sFileQuality = theMediaFileObj.m_sFileQuality;
                    nMediaID = theMediaFileObj.m_nMediaID;
                }

                nOwnerGroupID = GetOwnerMediaGroup(nMediaID);
                nQualityID = ProtocolsFuncs.GetFileQualityID(sFileQuality);
                nFormatID = ProtocolsFuncs.GetFileTypeID(sFileFormat, nGroupID);
                nFriendlyFormatID = ProtocolsFuncs.GetFriendlyFormatID(nGroupID, nFormatID);

                sAction = sAction.ToLower().Trim();

                switch (sAction)
                {
                    case "error":
                        {
                            if (theDoc != null)
                            {
                                Int32 nErrorCode = 0;
                                string sErrorMessage = string.Empty;

                                XmlNode theErrorCode = theDoc.SelectSingleNode("/root/request/mark/@error_code");
                                if (theErrorCode != null && theErrorCode.Value.Trim() != "")
                                    nErrorCode = int.Parse(theErrorCode.Value);

                                XmlNode theErrorMessage = theDoc.SelectSingleNode("/root/request/mark/@error_message");
                                if (theErrorMessage != null && theErrorMessage.Value.Trim() != "")
                                    sErrorMessage = theErrorMessage.Value;

                                AddErrorMessage(nGroupID, nMediaID, nMediaFileID, sSiteGUID, nLoc, sUDID, nPlatform, nErrorCode, sErrorMessage);

                                return sRet.ToString();
                            }

                            break;
                        }
                    case "play":
                        {
                            nPlay = 1;
                            if (nMediaID != 0)
                            {
                                nActionID = 1;
                            }
                            if (IsConcurrent(sSiteGUID, sUDID, nGroupID))
                            {
                                if (theDoc != null)
                                    return "<response type=\"Concurrent\"></response>";
                            }
                            UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, nLoc, sUDID);
                            break;
                        }

                    case "stop":
                        {
                            nStop = 1;
                            if (nMediaID != 0)
                                nActionID = 2;
                            UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, nLoc, sUDID);

                            break;
                        }

                    case "pause":
                        {
                            nPause = 1;
                            if (nMediaID != 0)
                            {
                                nActionID = 3;
                                UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, nLoc, sUDID);
                            }

                            break;
                        }
                    case "finish":
                        {
                            nFinish = 1;
                            UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, 0, sUDID);

                            break;
                        }
                    case "full_screen":
                        {
                            nFull = 1;
                            if (nMediaID != 0)
                                nActionID = 6;

                            break;
                        }
                    case "full_screen_exit":
                        {
                            nExitFull = 1;
                            if (nMediaID != 0)
                                nActionID = 9;

                            break;
                        }
                    case "send_to_friend":
                        {
                            nSendToFriend = 1;
                            nActionID = 7;

                            break;
                        }
                    case "load":
                        {
                            nLoad = 1;

                            break;
                        }
                    case "first_play":
                        {
                            nFirstPlay = 1;

                            if (IsConcurrent(sSiteGUID, sUDID, nGroupID))
                            {
                                if (theDoc != null)
                                    return "<response type=\"Concurrent\"></response>";
                            }

                            sPlayCycleKey = Guid.NewGuid().ToString();
                            AddPlayCycleKey(nGroupID, nMediaID, nMediaFileID, sSiteGUID, nPlatform, sUDID, nCountryID, sPlayCycleKey);
                            UpdateFollowMe(nGroupID, nMediaID, sSiteGUID, nLoc, sUDID);

                            break;
                        }
                    case "bitrate_change":
                        {
                            nActionID = 40;

                            int siteGuid = 0;
                            if (!string.IsNullOrEmpty(sSiteGUID))
                            {
                                siteGuid = int.Parse(sSiteGUID);
                            }
                            AddVideoQualityStatistics(nGroupID, nMediaID, nMediaFileID, avgBitRate, currentBitRate, totalBitRates, nWatcherID, siteGuid, nPlayTime, sSessionID, nBrowser, nPlatform, nCountryID);

                            break;
                        }
                }

                if (nActionID == 0 && sAction != "")
                {
                    nActionID = GetActionValues(sAction, ref bEOH);
                }
                if (nMediaID != 0)
                {
                    if (nFirstPlay != 0 || nPlay != 0 || nLoad != 0 || nPause != 0 || nStop != 0 || nFull != 0 || nExitFull != 0 || nSendToFriend != 0 || nPlayTime != 0 || nFinish != 0)
                    {
                        if (string.IsNullOrEmpty(sPlayCycleKey))
                        {
                            sPlayCycleKey = GetLastPlayCycleKey(sSiteGUID, nMediaID, nMediaFileID, sUDID, nGroupID, nPlatform, nCountryID);
                        }


                        UpdateEOHStatistics(nGroupID, nOwnerGroupID, nMediaID, nMediaFileID, nBillingTypeID, nCDNID,
                            nCountryID, nPlayerID, nQualityID, nFormatID, dNow, nWatcherID, sSessionID, nMediaDuration,
                            nFirstPlay, nPlay, nLoad, nPause, nStop, nFull, nExitFull, nSendToFriend, nLoc,
                            nFinish, nBrowser, nPlatform, sSiteGUID, sUDID, sPlayCycleKey);
                    }
                }
                if (nActionID != 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_media_actions");
                    //insertQuery.SetLockTimeOut(10000);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                    if (nMediaID != 0)
                    {
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                    }
                    else
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", 0);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OWNER_GROUP_ID", "=", nOwnerGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_QUALITY_ID", "=", nQualityID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_FORMAT_ID", "=", nFormatID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TYPE_ID", "=", nBillingTypeID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SESSION_ID", "=", sSessionID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTION_ID", "=", nActionID);
                    if (nMediaID != 0)
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", nCDNID);
                    else
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_ID", "=", 0);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LOCATION_SEC", "=", nLoc);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", nBrowser);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    if (nActionID == 4) // update only when first_play
                    {
                        {
                            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                            //directQuery.SetLockTimeOut(10000);
                            directQuery += "update media set views=views+1 where ";
                            directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                            directQuery.Execute();
                            directQuery.Finish();
                            directQuery = null;

                            ODBCWrapper.DirectQuery directMediaFilesQuery = new ODBCWrapper.DirectQuery();
                            directMediaFilesQuery += "update media_files set views=views+1 where ";
                            directMediaFilesQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
                            directMediaFilesQuery.Execute();
                            directMediaFilesQuery.Finish();
                            directMediaFilesQuery = null;
                        }
                    }
                }
                else
                {
                    if (theDoc != null)
                        sRet.Append("<error>Action not recognized</error>");
                }
            }
            return sRet.ToString();
        }

        private static int GetLastWatcherAction(int watcherID, int sessionID, int mediaFileID)
        {
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 1 ACTION_ID from watchers_media_actions where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("watcher_id", "=", watcherID);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "=", sessionID);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", mediaFileID);
            selectQuery += "order by UPDATE_DATE desc";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ACTION_ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static protected void GetCommercialCampaignData(Int32 nCommercialID, ref Int32 nCampaignID, ref Int32 nViews, ref Int32 nMaxViews)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select cam.id,cam.views,cam.MAX_VIEWS from campaigns cam (nolock) ,commercial c (nolock) ,campaigns_commercials camc (nolock)  where camc.status=1 and c.status=1 and c.is_active=1 and cam.status=1 and cam.is_active=1 and cam.id=camc.campaign_id and camc.commercial_id=c.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.id", "=", nCommercialID);
            selectQuery += "order by cam.create_date";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCampaignID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VIEWS"].ToString());
                    nMaxViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MAX_VIEWS"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string SingleMediaProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            string sLang, Int32 nPlayerID, bool bWithCache, bool bIsAdmin,
            ref ApiObjects.InitializationObject oWSInitObj,
            Int32[] nWSMediaIDs, ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            ref ApiObjects.MediaObject[] theMediaObjs, ref ApiObjects.PlayListSchema thePlayListSchema,
            Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sMediaID = "";

            XmlNodeList theMediaIDs = null;
            XmlNode theInfoStruct = null;
            XmlNode theWithInfo = null;
            XmlNode theWithFileTypes = null;

            string sWithInfo = "";
            string sWithFileTypes = "";
            bool bWithInfo = false;
            bool bWithFileTypes = false;

            bool bUseStartDate = true;
            string sUseStartDate = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "use_start_date");
            if (sUseStartDate == "false")
            {
                bUseStartDate = false;
            }

            StringBuilder sRet = new StringBuilder();
            if (oWSInitObj == null)
            {
                theMediaIDs = theDoc.SelectNodes("/root/request/media");
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache);
                theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();
                theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();
                if (theMediaIDs == null)
                    sMediaID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "media_id");
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;

                sRet.Append("<response type=\"single_media\">");
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, bWithCache));
                if (theMediaIDs != null)
                {
                    Int32 nCount1 = theMediaIDs.Count;
                    for (int i = 0; i < nCount1; i++)
                    {
                        XmlNode theMediaID = theMediaIDs[i].SelectSingleNode("@id");
                        sMediaID = theMediaID.Value.ToUpper();
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, int.Parse(sMediaID), "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache,
                            nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID, false, string.Empty, DateTime.MaxValue, bUseStartDate));
                    }
                }
                else if (sMediaID != "")
                {
                    sMediaID = sMediaID.ToUpper();
                    sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, int.Parse(sMediaID), "media", nGroupID, nLangID, bIsLangMain,
                        nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID, false, string.Empty, DateTime.MaxValue, bUseStartDate));
                }
                sRet.Append("</response>");
            }
            else
            {
                ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID,
                    nPlayerID, bWithCache, ref thePlayListSchema);
                Int32 nCount1 = nWSMediaIDs.Length;
                if (nCount1 > 0)
                    theMediaObjs = new ApiObjects.MediaObject[nCount1];
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nMediaID = nWSMediaIDs[i];
                    bWithInfo = oWSInitObj.m_oExtraRequestObject.m_bWithInfo;
                    bWithCache = !oWSInitObj.m_oExtraRequestObject.m_bNoCache;
                    bWithFileTypes = oWSInitObj.m_oExtraRequestObject.m_bWithFileTypes;
                    if (theWSInfoStruct == null)
                        theInfoStruct = null;
                    else
                        theInfoStruct = ConvertInfoStructToInfoXML(ref theWSInfoStruct);


                    Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                    string sFileFormat = "";
                    string sSubFileFormat = "";
                    string sFileQuality = "";

                    if (oWSInitObj.m_oFileRequestObjects != null)
                    {
                        if (oWSInitObj.m_oFileRequestObjects[0] != null)
                        {
                            sFileFormat = oWSInitObj.m_oFileRequestObjects[0].m_sFileFormat;
                            sFileQuality = oWSInitObj.m_oFileRequestObjects[0].m_sFileQuality;
                        }
                        if (oWSInitObj.m_oFileRequestObjects.Length > 1 && oWSInitObj.m_oFileRequestObjects[1] != null)
                        {
                            sSubFileFormat = oWSInitObj.m_oFileRequestObjects[1].m_sFileFormat;
                            if (sFileQuality == "")
                                sFileQuality = oWSInitObj.m_oFileRequestObjects[1].m_sFileQuality;
                        }
                    }
                    theMediaObjs[i] = new ApiObjects.MediaObject();
                    bool bStatistics = false;
                    bool bPersonal = false;
                    if (theWSInfoStruct != null)
                    {
                        bPersonal = theWSInfoStruct.m_bPersonal;
                        bStatistics = theWSInfoStruct.m_bStatistics;
                    }
                    ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble,
                        sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, !oWSInitObj.m_oExtraRequestObject.m_bNoCache,
                        sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes,
                        ref oWSInitObj.m_oPicObjects, ref theMediaObjs[i], oWSInitObj.m_oExtraRequestObject.m_bUseFinalEndDate,
                        bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, false, string.Empty, bUseStartDate);
                }
            }


            return sRet.ToString();
        }

        static public string PicsProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            string sLang, Int32 nPlayerID, bool bWithCache, bool bIsAdmin)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            XmlNodeList thePicIDs = null;

            StringBuilder sRet = new StringBuilder();
            thePicIDs = theDoc.SelectNodes("/root/request/pic");

            sRet.Append("<response type=\"pics\">");
            if (thePicIDs != null)
            {
                Int32 nCount1 = thePicIDs.Count;
                string sPicSizeForCache = "";
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
                for (int i = 0; i < nCount1; i++)
                {
                    XmlNode thePicID = thePicIDs[i].SelectSingleNode("@id");
                    string sPicID = thePicID.Value.ToUpper();
                    Int32 nPicID = 0;
                    try
                    {
                        nPicID = int.Parse(sPicID);
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                    }

                    ApiObjects.PicObject[] thePics = null;
                    string sPicURLs = "";
                    if (nPicID != 0)
                        sPicURLs = GetPicSizesXMLParts(ref theDoc, int.Parse(sPicID), nGroupID, bIsAdmin, bWithCache, ref thePics, sPicSizeForCache + "_pic_id:" + nPicID.ToString());
                    sRet.Append("<pic id=\"" + sPicID + "\" " + sPicURLs + " />");
                }
            }
            sRet.Append("</response>");
            return sRet.ToString();
        }


        static public string GetMediaTag(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID, Int32 nLangID, bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin, bool bEnterToCache,
            bool bWithFileTypes, Int32 nCountryID, Int32 nDeviceID)
        {
            return GetMediaTag(ref theDoc, nMediaID, sTagName, nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach, nPlayerID, ref theInfoStruct, bIsAdmin, bEnterToCache, bWithFileTypes, nCountryID, nDeviceID, false, string.Empty, DateTime.MaxValue, true);
        }

        static public string GetMediaTag(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID, Int32 nLangID, bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin, bool bEnterToCache,
            bool bWithFileTypes, Int32 nCountryID, Int32 nDeviceID, bool bWithUDID, string sDeviceName, DateTime lastWatchedDate, bool bUseStartDate)
        {
            StringBuilder res = new StringBuilder();
            Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
            string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
            string[] sFileFormatArray = sFileFormat.Split(FileFormatSeparater).Distinct().ToArray();

            string sSubFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "sub_file_format").Trim();

            string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");
            bool bStatistics = false;
            bool bPersonal = false;
            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
            if (sStatistics == "true")
                bStatistics = true;
            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
            if (sPersonal == "true")
                bPersonal = true;
            ApiObjects.PicObject[] thePics = null;
            ApiObjects.MediaObject theMediaObj = null;
            ApiObjects.MediaInfoStructObject theWSInfoStruct = null;

            foreach (string strFF in sFileFormatArray)
            {

                res.Append(ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, strFF, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach,
                    sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, bEnterToCache, bWithFileTypes,
                    ref thePics, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, bWithUDID, sDeviceName, lastWatchedDate, bUseStartDate));
            }
            return res.ToString();
        }

        static public string GetMiniMediaTagInner(Int32 nMediaFileID)
        {
            Int32 nCDNID = 0;
            string sURL = "";
            string sCDNImpl = "";
            string sCDNNotifyURL = "";
            string sFileFormat = "";
            Int32 nGroupID = 0;
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.*,lmt.description as file_format from media_files mf (nolock) ,lu_media_types lmt WITH (nolock) where lmt.id=mf.MEDIA_TYPE_ID and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    string sDuration = selectQuery.Table("query").DefaultView[0].Row["duration"].ToString();
                    DataRecordMediaViewerField d = new DataRecordMediaViewerField("", nMediaFileID);
                    d.GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotifyURL);
                    sURL = d.GetFLVSrc();
                    sFileFormat = selectQuery.Table("query").DefaultView[0].Row["file_format"].ToString();
                    sRet.Append("<media ");
                    sRet.Append("media_file_id=\"").Append(nMediaFileID);
                    sRet.Append("\" ");
                    sRet.Append("duration=\"").Append(sDuration);
                    sRet.Append("\" ");
                    sRet.Append("orig_file_format=\"");
                    sRet.Append(sFileFormat);
                    sRet.Append("\" ");
                    sRet.Append("file_format=\"");
                    sRet.Append(GetMediaTypeForPlayer(sFileFormat, nMediaFileID, nGroupID));
                    sRet.Append("\" ");
                    sRet.Append("url=\"");
                    sRet.Append(XMLEncode(sURL, true));
                    sRet.Append("\" ");
                    sRet.Append("cdn_impl_type=\"");
                    sRet.Append(sCDNImpl);
                    sRet.Append("\"");
                    //sRet.Append(" outer_guid=\"");
                    //sRet.Append(sOuterGuid);
                    //sRet.Append("\" ");
                    sRet.Append("></media>");
                }
                else
                {
                    sRet.Append("<media ");
                    sRet.Append("media_file_id=\"").Append(nMediaFileID);
                    sRet.Append("\" ");
                    sRet.Append("duration=\"0\" ");
                    sRet.Append("file_format=\"\" ");
                    sRet.Append("url=\"\" ");
                    sRet.Append("cdn_impl_type=\"\"");
                    sRet.Append("></media>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected void RemoveUnDesiredElements(ref XmlDocument theDoc)
        {
            if (theDoc.SelectSingleNode("root/flashvars/@site_guid") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["site_guid"];
                attr.OwnerElement.RemoveAttribute("site_guid");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@zip") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["zip"];
                attr.OwnerElement.RemoveAttribute("zip");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@tvinci_guid") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["tvinci_guid"];
                attr.OwnerElement.RemoveAttribute("tvinci_guid");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@admin_token") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["admin_token"];
                attr.OwnerElement.RemoveAttribute("admin_token");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@no_cache") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["no_cache"];
                attr.OwnerElement.RemoveAttribute("no_cache");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@alt_tvm") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["alt_tvm"];
                attr.OwnerElement.RemoveAttribute("alt_tvm");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@debug") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["debug"];
                attr.OwnerElement.RemoveAttribute("debug");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@auto_init") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["auto_init"];
                attr.OwnerElement.RemoveAttribute("auto_init");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@auto_play") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["auto_play"];
                attr.OwnerElement.RemoveAttribute("auto_play");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@block") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["block"];
                attr.OwnerElement.RemoveAttribute("block");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@dir") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["dir"];
                attr.OwnerElement.RemoveAttribute("dir");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@error_clip") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["error_clip"];
                attr.OwnerElement.RemoveAttribute("error_clip");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@language_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["language_file"];
                attr.OwnerElement.RemoveAttribute("language_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@layout_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["layout_file"];
                attr.OwnerElement.RemoveAttribute("layout_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@skin_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["skin_file"];
                attr.OwnerElement.RemoveAttribute("skin_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@config_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["config_file"];
                attr.OwnerElement.RemoveAttribute("config_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_menu") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_menu"];
                attr.OwnerElement.RemoveAttribute("starting_menu");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_channel") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_channel"];
                attr.OwnerElement.RemoveAttribute("starting_channel");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_channel2") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_channel2"];
                attr.OwnerElement.RemoveAttribute("starting_channel2");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@object_id") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["object_id"];
                attr.OwnerElement.RemoveAttribute("object_id");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@object_key") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["object_key"];
                attr.OwnerElement.RemoveAttribute("object_key");
            }
        }

        static protected void RemoveUnDesiredElements(ref XmlNode theDoc)
        {
            if (theDoc.SelectSingleNode("root/flashvars/@site_guid") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["site_guid"];
                attr.OwnerElement.RemoveAttribute("site_guid");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@zip") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["zip"];
                attr.OwnerElement.RemoveAttribute("zip");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@tvinci_guid") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["tvinci_guid"];
                attr.OwnerElement.RemoveAttribute("tvinci_guid");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@no_cache") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["no_cache"];
                attr.OwnerElement.RemoveAttribute("no_cache");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@alt_tvm") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["alt_tvm"];
                attr.OwnerElement.RemoveAttribute("alt_tvm");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@debug") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["debug"];
                attr.OwnerElement.RemoveAttribute("debug");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@auto_init") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["auto_init"];
                attr.OwnerElement.RemoveAttribute("auto_init");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@auto_play") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["auto_play"];
                attr.OwnerElement.RemoveAttribute("auto_play");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@block") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["block"];
                attr.OwnerElement.RemoveAttribute("block");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@config_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["config_file"];
                attr.OwnerElement.RemoveAttribute("config_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@dir") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["dir"];
                attr.OwnerElement.RemoveAttribute("dir");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@error_clip") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["error_clip"];
                attr.OwnerElement.RemoveAttribute("error_clip");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@language_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["language_file"];
                attr.OwnerElement.RemoveAttribute("language_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@layout_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["layout_file"];
                attr.OwnerElement.RemoveAttribute("layout_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@skin_file") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["skin_file"];
                attr.OwnerElement.RemoveAttribute("skin_file");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_menu") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_menu"];
                attr.OwnerElement.RemoveAttribute("starting_menu");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_channel") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_channel"];
                attr.OwnerElement.RemoveAttribute("starting_channel");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@starting_channel2") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["starting_channel2"];
                attr.OwnerElement.RemoveAttribute("starting_channel2");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@object_id") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["object_id"];
                attr.OwnerElement.RemoveAttribute("object_id");
            }
            if (theDoc.SelectSingleNode("root/flashvars/@object_key") != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["object_key"];
                attr.OwnerElement.RemoveAttribute("object_key");
            }
        }

        static public string GetSig(ref XmlDocument theDoc, bool bRemove)
        {
            return ConvertXMLToString(ref theDoc, true);
            /*
            if (bRemove == true)
                RemoveUnDesiredElements(ref theDoc);
            
            theDoc.PreserveWhitespace = false;
            SignedXml signedXml = new SignedXml(theDoc);
            CspParameters CSPParam = new CspParameters();
            CSPParam.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider Key = new RSACryptoServiceProvider(CSPParam);

            string sKey = "<RSAKeyValue><Modulus>4NZOkPpDPdLOQhu8l44Q9z1zm8pIruAciKdOnd+AqYEN0ecd7sSxOJc+jNwQgoX4rx+1chRiqlUFKkXobc2KkWiJGIb3KrrZYQG3BtbvEiafcBpYx847OKPD49oo7LPrG9bbaX2LUHlTmzFi3RC96jkuAZIYWXNAMVoHt2EynB8=</Modulus><Exponent>AQAB</Exponent><P>+GJ1HQ/rR5qU6uBxIjLMygCkRk/VuGJCOqqHgNozRnmEUKpg7usWUxqzbt5u+zMneDh8/ekUrlC8QITAPalT+Q==</P><Q>57sHLuiyjlTdaqHkDaTMtERHRd74ImycQXqfnB27AixTUujEzM7mtawYAzC1M/1FivEAf+LyTsZGK9t/Vv9G1w==</Q><DP>Air50rMc3pcezZ3/3siKuQigZmyz9NQGt/RdEmbVI7xTx1B1YCB1JWyKPGF8nnynz4jOrnimUY8q1XVTKDaeIQ==</DP><DQ>MjdrdRTkLMfjO4pdmz9NRPtO3qtU5lDXA7GJCRHARtZIMD1U+HUVteTXR0m02tFV0qgrTYtmqv94kWR9keK7/Q==</DQ><InverseQ>lEdKDTgsQRnjWOFe+hTnGCeDdJ+qgFiyFGCJu4hsURDq6uH5uo++jox7mFItj2ktVUw8VZz4bByZZVl8xPjcaA==</InverseQ><D>CnFpzMoS/XghJGjtZYyvtQwhpobKwXSfXqmGuUZ8T2MzJSC6/zAbmylLOneuPKHEXz31y4qu2oKAbuo4VYFKgkgv113WHO01AGT3PZkpOe135thDktx/l+BnOxT+hZKt8PEA1kTWWC3OHIgid1/iloI/y3K16M6cMxXOxX22m0E=</D></RSAKeyValue>";
            Key.FromXmlString(sKey);
            signedXml.SigningKey = Key;
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            Reference reference = new Reference();
            reference.Uri = "";
            
            signedXml.AddReference(reference);

            signedXml.ComputeSignature();
            string sSigVAL = Convert.ToBase64String(signedXml.Signature.SignatureValue);
            return sSigVAL;
            */
        }

        static public string ConvertXMLToString(ref XmlDocument theDoc, bool bRemove)
        {
            if (bRemove == true)
                RemoveUnDesiredElements(ref theDoc);

            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            theDoc.WriteTo(xw);
            return sw.ToString();
        }

        static public string ConvertXMLToString(ref XmlNode theDoc, bool bRemove)
        {
            if (bRemove == true)
                RemoveUnDesiredElements(ref theDoc);
            if (theDoc == null)
                return "";
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            theDoc.WriteTo(xw);
            return sw.ToString();
        }

        private static bool CheckAdminToken(string token, int groupID, string sIP)
        {
            bool retVal = false;
            string groupStr = PageUtils.GetParentsGroupsStr(groupID);
            log.Debug(String.Format("{0} {1} {2} {3} ,{4}", "Token Request", sIP, token, groupStr, "Token Requests"));
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from admin_tokens where status=1 and end_date>getdate() and ";
            selectQuery += "group_id ";
            selectQuery += groupStr;
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;

                if (count > 0)
                {
                    log.Info(String.Format("{0} , {1}", "Token Request", "Token Found"));
                    retVal = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static public string GetMediaTagNeto(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID, Int32 nCountryID, Int32 nBlocakble, string sFileFormat, string sFileQuality, Int32 nLangID, bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, string sSubFileFormat, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin, bool bEnterToCache, bool bWithFileTypes, Int32 nDeviceID)
        {
            ApiObjects.PicObject[] thePics = null;
            ApiObjects.MediaObject theMediaObj = null;
            ApiObjects.MediaInfoStructObject theWSInfoStruct = null;
            bool bStatistics = false;
            bool bPersonal = false;
            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
            if (sStatistics == "true")
                bStatistics = true;
            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
            if (sPersonal == "true")
                bPersonal = true;
            return GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, bEnterToCache, bWithFileTypes, ref thePics, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
        }

        static public string GetMediaTagNeto(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID,
            Int32 nCountryID, Int32 nBlocakble, string sFileFormat, string sFileQuality, Int32 nLangID,
            bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, string sSubFileFormat, Int32 nPlayerID,
            bool bIsInner, ref XmlNode theInfoStruct, bool bAdmin, bool bEnterToCache, bool bWithFileTypes,
            ref ApiObjects.PicObject[] thePics, ref ApiObjects.MediaObject theMediaObj, bool bUseFinalEndDate,
            bool bStatistics, bool bPersonal, ref ApiObjects.MediaInfoStructObject theWSInfoStruct, Int32 nDeviceID)
        {
            return GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach, sSubFileFormat, nPlayerID, bIsInner,
                ref theInfoStruct, bAdmin, bEnterToCache, bWithFileTypes, ref thePics, ref theMediaObj, bUseFinalEndDate, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, false, string.Empty);
        }

        static public string GetMediaTagNeto(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID,
            Int32 nCountryID, Int32 nBlocakble, string sFileFormat, string sFileQuality, Int32 nLangID,
            bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, string sSubFileFormat, Int32 nPlayerID,
            bool bIsInner, ref XmlNode theInfoStruct, bool bAdmin, bool bEnterToCache, bool bWithFileTypes,
            ref ApiObjects.PicObject[] thePics, ref ApiObjects.MediaObject theMediaObj, bool bUseFinalEndDate,
            bool bStatistics, bool bPersonal, ref ApiObjects.MediaInfoStructObject theWSInfoStruct, Int32 nDeviceID, bool bWithUDID, string sDeviceName)
        {
            return GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach, sSubFileFormat, nPlayerID, bIsInner,
                ref theInfoStruct, bAdmin, bEnterToCache, bWithFileTypes, ref thePics, ref theMediaObj, bUseFinalEndDate, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, false, string.Empty, true);
        }

        static public string GetMediaTagNeto(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID,
            Int32 nCountryID, Int32 nBlocakble, string sFileFormat, string sFileQuality, Int32 nLangID,
            bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, string sSubFileFormat, Int32 nPlayerID,
            bool bIsInner, ref XmlNode theInfoStruct, bool bAdmin, bool bEnterToCache, bool bWithFileTypes,
            ref ApiObjects.PicObject[] thePics, ref ApiObjects.MediaObject theMediaObj, bool bUseFinalEndDate,
            bool bStatistics, bool bPersonal, ref ApiObjects.MediaInfoStructObject theWSInfoStruct, Int32 nDeviceID, bool bWithUDID, string sDeviceName, bool bUseStartDate)
        {
            return GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCach, sSubFileFormat, nPlayerID, bIsInner,
               ref theInfoStruct, bAdmin, bEnterToCache, bWithFileTypes, ref thePics, ref theMediaObj, bUseFinalEndDate, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, false, string.Empty, DateTime.MaxValue, bUseStartDate);
        }

        static public string GetMediaTagNeto(ref XmlDocument theDoc, Int32 nMediaID, string sTagName, Int32 nGroupID,
            Int32 nCountryID, Int32 nBlocakble, string sFileFormat, string sFileQuality, Int32 nLangID,
            bool bIsLangMain, Int32 nWatcherID, bool bWithInfo, bool bWithCach, string sSubFileFormat, Int32 nPlayerID,
            bool bIsInner, ref XmlNode theInfoStruct, bool bAdmin, bool bEnterToCache, bool bWithFileTypes,
            ref ApiObjects.PicObject[] thePics, ref ApiObjects.MediaObject theMediaObj, bool bUseFinalEndDate,
            bool bStatistics, bool bPersonal, ref ApiObjects.MediaInfoStructObject theWSInfoStruct, Int32 nDeviceID, bool bWithUDID, string sDeviceName, DateTime lastWatchDate, bool bUseStartDate)
        {
            if (theMediaObj != null)
                theMediaObj.m_nMediaID = nMediaID;
            Int32 nMediaFileID = 0;
            string sBillingType = "";
            double dFileDuration = 0;
            string sOuterGuid = "";
            int nLikeCounter = 0;
            Int32 nViews = 0;
            Int32 nTotalViews = 0;
            string sBaseBilling = "";
            string adminToken = string.Empty;
            string userIP = string.Empty;
            string sNoFileURL = string.Empty;
            if (theDoc != null)
            {
                adminToken = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "admin_token");
                userIP = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "user_ip");
                sNoFileURL = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "no_file_url");
            }
            ApiObjects.MediaFileObject oMediaFile = null;
            ApiObjects.MediaInfoObject oMediaInfo = null;
            ApiObjects.MediaStatistics oMediaStatistics = null;
            ApiObjects.MediaPersonalStatistics oMediaPersonalStatistics = null;
            ApiObjects.FileTypeContainer[] theFileTypes = null;
            ApiObjects.MediaAdObject thePreAdObj = null;
            ApiObjects.MediaAdObject theBreakAdObj = null;
            ApiObjects.MediaAdObject theOverlayAdObj = null;
            ApiObjects.MediaAdObject thePostAdObj = null;
            ApiObjects.MediaAdSchema theAdSchema = null;
            nMediaFileID = GetMediaFileID(nMediaID, sFileFormat, sFileQuality, ref sBillingType, ref sBaseBilling, ref dFileDuration, ref nViews, bAdmin, nGroupID, bWithCach);

            StringBuilder sRet = new StringBuilder();
            bool bExists = false;

            //string sCountryCD = PageUtils.GetIPCountry2().ToString();
            string sCountryCD = nCountryID.ToString();
            bool bIPAllowed = TVinciShared.ProtocolsFuncs.DoesCallerPermittedIP(nGroupID);
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            if (theMediaObj == null && CachingManager.CachingManager.Exist("GetMediaTagNeto_" + sPicSizeForCache + sCountryCD + nMediaFileID.ToString() + "_" + nMediaID.ToString() + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + adminToken + "_" + userIP + "_" + bUseStartDate.ToString()) == true && bWithCach == true && bWithUDID == false)
            {
                sRet.Append(CachingManager.CachingManager.GetCachedData("GetMediaTagNeto_" + sPicSizeForCache + sCountryCD + nMediaFileID.ToString() + "_" + nMediaID.ToString() + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + adminToken + "_" + userIP + "_" + bUseStartDate.ToString()).ToString());
                if (sRet.ToString() != "")
                    bExists = true;
            }
            else
            {
                Int32 nCDNID = 0;
                string sURL = "";
                string sConfigData = "";
                string sCDNImpl = "";
                string sCDNNotidyURL = "";

                bool bDoesMediaBelongsToGroup = true;
                bool bDoesPlayerPermitedToPlay = false;
                bool bDoesGroupPermitedToPlay = false;
                //If the media belongs to the group
                Int32 nOwnerGroup = 0;
                object oOG = ODBCWrapper.Utils.GetTableSingleVal("media", "group_id", nMediaID, 86400);
                if (oOG != null && oOG != DBNull.Value)
                    nOwnerGroup = int.Parse(oOG.ToString());
                if (nOwnerGroup != nGroupID)
                {
                    bDoesMediaBelongsToGroup = false;
                    //If the group is allowed to watch the other group media
                    object oWatchPermisionID = ODBCWrapper.Utils.GetTableSingleVal("media", "WATCH_PERMISSION_TYPE_ID", nMediaID, 86400);
                    if (oWatchPermisionID != null && oWatchPermisionID != DBNull.Value)
                    {
                        Int32 nWatchPermitID = int.Parse(oWatchPermisionID.ToString());
                        bDoesGroupPermitedToPlay = PageUtils.DoesWatchPermissionRuleOK(nWatchPermitID, nGroupID);
                    }
                }
                //If the player is allowed to watch the media
                object oPlayerPermisionID = ODBCWrapper.Utils.GetTableSingleVal("media", "PLAYERS_RULES", nMediaID, 86400);

                if (oPlayerPermisionID != null && oPlayerPermisionID != DBNull.Value)
                {
                    Int32 nPlayerPermitID = int.Parse(oPlayerPermisionID.ToString());
                    bDoesPlayerPermitedToPlay = PageUtils.DoesPlayerRuleOK(nPlayerPermitID, nPlayerID);
                }
                else
                    bDoesPlayerPermitedToPlay = true;
                bool bPermited = false;
                if (bDoesMediaBelongsToGroup == true && bDoesPlayerPermitedToPlay == true)
                    bPermited = true;
                else if (bDoesMediaBelongsToGroup == false && bDoesGroupPermitedToPlay == true && bDoesPlayerPermitedToPlay == true)
                    bPermited = true;

                if (bPermited == false)
                    return "";

                DataRecordMediaViewerField d = new DataRecordMediaViewerField("", nMediaFileID);
                d.GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotidyURL, 3600);
                if (sCDNImpl == "mtv_akamai_vip_ip_onetime")
                {
                    sCDNImpl = "akamai";
                }

                if (sNoFileURL.ToLower() == "true" && (!string.IsNullOrEmpty(sBillingType) && !sBillingType.Equals("none")))
                    sURL = string.Format("{0}||{1}", nMediaID, nMediaFileID);
                else
                    sURL = d.GetFLVSrc(nGroupID);

                sConfigData = d.GetConfigData();

                string sBlock = "none";
                Int32 nONLY_OR_BUT = 0;
                Int32 nGeoBlockID = 0;
                string sMediaName = "";
                string sMediaDesc = "";
                string sEPGIdentifier = string.Empty;

                Int32 nPicID = 0;

                DateTime dPublish = DateTime.Now;
                DateTime dStart = DateTime.Now;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                string sEndDateField = "m.";
                if (theDoc != null)
                    sEndDateField += GetFinalEndDateField(ref theDoc);
                else
                {
                    if (bUseFinalEndDate == false)
                        sEndDateField += "end_date";
                    else
                        sEndDateField += "final_end_date";
                }
                selectQuery += "select m.views as 'total_views' ";
                if (nMediaFileID != 0)
                    selectQuery += ", mf.VIEWS ";
                selectQuery += ", m.CO_GUID,m.MEDIA_PIC_ID,m.EPG_IDENTIFIER, m.start_date,m.create_date,m.publish_date,gbt.id as 'gbt_id',gbt.ONLY_OR_BUT,m.NAME,m.description, m.like_counter from ";
                if (nMediaFileID != 0)
                    selectQuery += " media_files mf (nolock), ";
                selectQuery += " media m (nolock) ";
                selectQuery += " LEFT JOIN geo_block_types gbt (nolock) ON ";
                selectQuery += " gbt.id=m.BLOCK_TEMPLATE_ID and gbt.status=1 and gbt.is_active=1 ";
                selectQuery += " where ";
                if (nMediaFileID != 0)
                    selectQuery += " mf.media_id=m.id and ";

                if (bAdmin == false)
                {
                    selectQuery += " (m.id not in (select id from media (nolock) where ";

                    selectQuery += GetDateRangeQuery(sEndDateField, bUseStartDate);

                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                    selectQuery += "))";
                    selectQuery += " and ";
                    selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where ";

                    selectQuery += GetDateRangeQuery(sEndDateField, bUseStartDate);

                    selectQuery += " (COUNTRY_ID=0 or ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                    selectQuery += ") and (LANGUAGE_ID=0 or ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    selectQuery += ") and (DEVICE_ID=0 or ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
                    selectQuery += ") and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                    selectQuery += "))";

                    //selectQuery += " (m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + ">getdate())) and m.is_active=1 and ";
                    if (string.IsNullOrEmpty(adminToken) || !CheckAdminToken(adminToken, nGroupID, userIP))
                        selectQuery += " and m.is_active=1 and ";
                    else
                    {
                        selectQuery += " and ";
                    }
                }
                selectQuery += " m.status=1 and ";
                if (nMediaFileID != 0)
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
                    selectQuery += " and ";
                }
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        bExists = true;
                        nPicID = 0;
                        object oGeoBlockType = selectQuery.Table("query").DefaultView[0].Row["gbt_id"];

                        if (oGeoBlockType != null && oGeoBlockType != DBNull.Value)
                        {
                            nONLY_OR_BUT = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ONLY_OR_BUT"].ToString());
                            if (bIPAllowed == false)
                                nGeoBlockID = int.Parse(oGeoBlockType.ToString());
                        }
                        if (bIsLangMain == true)
                        {
                            sMediaName = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                            if (selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"] != DBNull.Value &&
                                selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"] != null)
                                sMediaDesc = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                        }
                        else
                            TVinciShared.ProtocolsFuncs.GetMediaTranslation(nMediaID, nLangID, ref sMediaName, ref sMediaDesc);

                        if (selectQuery.Table("query").DefaultView[0].Row["publish_date"] != DBNull.Value)
                            dPublish = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["publish_date"]);
                        else if (selectQuery.Table("query").DefaultView[0].Row["create_date"] != DBNull.Value)
                            dPublish = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["create_date"]);
                        if (selectQuery.Table("query").DefaultView[0].Row["start_date"] != DBNull.Value)
                            dStart = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["start_date"]);
                        if (dStart > dPublish)
                            dPublish = dStart;
                        if (selectQuery.Table("query").DefaultView[0].Row["MEDIA_PIC_ID"] != DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["MEDIA_PIC_ID"] != null)
                            nPicID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MEDIA_PIC_ID"].ToString());
                        if (selectQuery.Table("query").DefaultView[0].Row["CO_GUID"] != DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["CO_GUID"] != null)
                            sOuterGuid = selectQuery.Table("query").DefaultView[0].Row["CO_GUID"].ToString();
                        if (selectQuery.Table("query").DefaultView[0].Row["like_counter"] != DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["like_counter"] != null)
                            nLikeCounter = int.Parse(selectQuery.Table("query").DefaultView[0].Row["like_counter"].ToString());
                        if (selectQuery.Table("query").DefaultView[0].Row["EPG_IDENTIFIER"] != DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["EPG_IDENTIFIER"] != null)
                            sEPGIdentifier = selectQuery.Table("query").DefaultView[0].Row["EPG_IDENTIFIER"].ToString();
                        if (nMediaFileID != 0)
                            nViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["views"].ToString());
                        nTotalViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["total_views"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                if (sMediaName == "")
                    return "";
                bool bAllowed = false;
                bool bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(nGeoBlockID, nCountryID);

                if (nONLY_OR_BUT == 0)
                    bAllowed = bExsitInRuleM2M;

                if (nONLY_OR_BUT == 1)
                    bAllowed = !bExsitInRuleM2M;

                if (bAllowed == false && nGeoBlockID != 0)
                {
                    if (PageUtils.GetIPCountry2() != 0 && bIPAllowed == false)
                        sBlock = "geo";
                }

                if (theMediaObj == null)
                {
                    sRet.Append("<").Append(sTagName).Append(" ");
                    sRet.Append("duration=\"").Append(dFileDuration);
                    sRet.Append("\" file_id=\"").Append(nMediaFileID).Append("\" id=\"").Append(nMediaID).Append("\" billing=\"");
                    sRet.Append(sBillingType);
                    sRet.Append("\" ");
                    sRet.Append(" file_format=\"");
                    sRet.Append(GetMediaTypeForPlayer(sFileFormat, nMediaFileID, nGroupID));
                    sRet.Append("\" ");
                    sRet.Append(" orig_file_format=\"");
                    sRet.Append(sFileFormat);
                    sRet.Append("\" ");
                    sRet.Append(" file_quality=\"");
                    sRet.Append(sFileQuality);
                    sRet.Append("\" ");
                    if (bIsInner == false)
                    {
                        string sPics = GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, nMediaID, bAdmin, bWithCach, ref thePics, sPicSizeForCache);
                        sRet.Append(sPics);
                    }

                    string sDateStr = string.Empty;
                    if (bUseStartDate)
                    {
                        sDateStr = DateUtils.GetStrFromDate(dPublish);
                    }
                    else
                    {
                        sDateStr = DateUtils.GetLongStrFromDate(dPublish);
                    }
                    sRet.Append(" title=\"").Append(XMLEncode(sMediaName, true)).Append("\" block=\"").Append(sBlock).Append("\" cdn_id=\"").Append(nCDNID).Append("\" cdn_impl_type=\"").Append(sCDNImpl).Append("\" notify_url=\"").Append(sCDNNotidyURL).Append("\" date=\"").Append(sDateStr).Append("\" url=\"").Append(XMLEncode(sURL, true)).Append("\" outer_guid=\"").Append(XMLEncode(sOuterGuid, true)).Append("\" ");
                    if (sConfigData.Trim() != "")
                        sRet.Append(" config_data=\"").Append(XMLEncode(sConfigData, true)).Append("\" ");

                    sRet.Append(" total_views=\"").Append(nTotalViews).Append("\" views=\"").Append(nViews).Append("\"");

                    sRet.Append(" last_watched_device_name=\"").Append(sDeviceName).Append("\"");

                    sRet.Append(" like_counter=\"").Append(nLikeCounter.ToString()).Append("\"");

                    string sLastWatchDate = string.Empty;
                    if (lastWatchDate != DateTime.MaxValue)
                    {
                        sLastWatchDate = lastWatchDate.ToString("dd/MM/yyyy hh:mm");
                    }

                    sRet.Append(" last_watched_date=\"").Append(sLastWatchDate).Append("\"");
                    sRet.Append(">");
                    if (!string.IsNullOrEmpty(sEPGIdentifier) && !bIsInner)
                    {
                        sRet.Append("<external_ids>");
                        sRet.Append(string.Format("<epg_id>{0}</epg_id>", sEPGIdentifier));
                        sRet.Append("</external_ids>");
                    }
                    if (bEnterToCache == true && nMediaFileID != 0 && bWithUDID == false)
                        CachingManager.CachingManager.SetCachedData("GetMediaTagNeto_" + sPicSizeForCache + sCountryCD + nMediaFileID.ToString() + "_" + nMediaID.ToString() + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + adminToken + "_" + userIP + "_" + bUseStartDate.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nMediaID, true);
                }
                else
                {
                    if (sSubFileFormat == "")
                        GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, nMediaID, bAdmin, bWithCach, ref thePics, sPicSizeForCache);
                    if (nMediaFileID != 0)
                    {
                        oMediaFile = new ApiObjects.MediaFileObject();
                        oMediaFile.m_dDuration = dFileDuration;
                        oMediaFile.m_nFileID = nMediaFileID;
                        oMediaFile.m_nMediaID = nMediaID;
                        oMediaFile.m_sBilling = sBillingType;
                        oMediaFile.m_sFileFormat = GetMediaTypeForPlayer(sFileFormat, nMediaFileID, nGroupID);
                        oMediaFile.m_sOrigFileFormat = sFileFormat;
                        oMediaFile.m_sFileQuality = sFileQuality;
                        oMediaFile.m_sCDNImplType = sCDNImpl;
                        oMediaFile.m_nCdnID = nCDNID;
                        oMediaFile.m_sNotifyUrl = sCDNNotidyURL;
                        oMediaFile.m_sUrl = sURL;
                        if (sConfigData.Trim() != "")
                            oMediaFile.m_sConfigData = sConfigData;
                        oMediaFile.m_nViews = nViews;
                    }

                    if (sSubFileFormat == "")
                    {
                        theMediaObj.m_sTitle = sMediaName;
                        theMediaObj.m_sBlockType = sBlock;
                        theMediaObj.m_dPublishDate = dPublish;
                        theMediaObj.m_sOwnerGUID = sOuterGuid;
                        if (bStatistics == true)
                        {
                            if (theMediaObj.m_oMediaStatistics == null)
                                theMediaObj.m_oMediaStatistics = new ApiObjects.MediaStatistics();
                            theMediaObj.m_oMediaStatistics.m_nTotalViews = nTotalViews;
                        }
                        if (thePics != null && bIsInner == false)
                        {
                            theMediaObj.m_oPicPbjects = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                        }
                    }
                }
            }
            if (bExists == true)
            {
                if (theMediaObj != null)
                {
                    thePreAdObj = new ApiObjects.MediaAdObject();
                    theBreakAdObj = new ApiObjects.MediaAdObject();
                    theOverlayAdObj = new ApiObjects.MediaAdObject();
                    thePostAdObj = new ApiObjects.MediaAdObject();
                }
                bool bPlaylistSchemaControlled = false;
                sRet.Append(GetMediaAdsSchema(nMediaID, nMediaFileID, nGroupID, bWithCach,
                    ref thePreAdObj, ref theBreakAdObj, ref theOverlayAdObj, ref thePostAdObj, ref bPlaylistSchemaControlled));

                if (theMediaObj != null && oMediaFile != null)
                {
                    theAdSchema = new ApiObjects.MediaAdSchema();
                    theAdSchema.Initialize(thePreAdObj, thePostAdObj, theBreakAdObj, theOverlayAdObj, bPlaylistSchemaControlled);
                    oMediaFile.m_oMediaAdSchema = theAdSchema;
                }

                //if (theMediaObj != null && theMediaObj.m_oMediaFiles == null)
                //{
                //if (sSubFileFormat == "")
                //theMediaObj.m_oMediaFiles = new ApiObjects.MediaFileObject[1];
                //else
                //theMediaObj.m_oMediaFiles = new ApiObjects.MediaFileObject[2];
                //}

                if (sSubFileFormat != "" && sTagName.ToLower().Trim() == "media")
                {
                    sRet.Append("<inner_medias>");

                    string[] strSFF = sSubFileFormat.Split(FileFormatSeparater).Distinct().ToArray();

                    foreach (string str in strSFF)
                    {
                        sRet.Append(GetMediaTagNeto(ref theDoc, nMediaID, sTagName, nGroupID, nCountryID, nBlocakble, str, sFileQuality,
                            nLangID, bIsLangMain, nWatcherID, false, bWithCach, "", nPlayerID, true, ref theInfoStruct, bAdmin,
                            bEnterToCache, bWithFileTypes, ref thePics, ref theMediaObj, bUseFinalEndDate,
                            bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, bWithUDID, sDeviceName, bUseStartDate));
                    }
                    sRet.Append("</inner_medias>");
                }



                if (bWithInfo == true)
                {
                    if (theMediaObj != null)
                    {
                        oMediaInfo = new ApiObjects.MediaInfoObject();
                        if (theWSInfoStruct != null)
                            theInfoStruct = ConvertInfoStructToInfoXML(ref theWSInfoStruct);
                        //else if(theInfoStruct != null)
                        //theInfoStruct = null;
                    }
                    sRet.Append(TVinciShared.ProtocolsFuncs.GetMediaInfoInner(nMediaID, nLangID, bIsLangMain, nWatcherID, bWithCach, ref theInfoStruct, bEnterToCache, bStatistics, bPersonal, ref oMediaInfo,
                        ref oMediaPersonalStatistics, ref oMediaStatistics));
                    if (theMediaObj != null)
                    {
                        theMediaObj.m_oMediaInfo = oMediaInfo;
                        theMediaObj.m_oMediaPersonalStatistics = oMediaPersonalStatistics;
                        theMediaObj.m_oMediaStatistics = oMediaStatistics;
                    }
                }
                if (bWithFileTypes == true && sSubFileFormat == "")
                {
                    if (theMediaObj == null)
                        sRet.Append(GetFileTypesXML(nMediaID, bWithCach));
                    else
                    {
                        GetFileTypesXML(nMediaID, bWithCach, ref theFileTypes);
                        theMediaObj.m_oAvailableFileTypes = theFileTypes;
                    }
                }
                if (theMediaObj == null)
                    sRet.Append("</").Append(sTagName).Append(">");

                if (theMediaObj != null && oMediaFile != null)
                {
                    if (theMediaObj.m_oMediaFiles == null)
                        theMediaObj.m_oMediaFiles = new ApiObjects.MediaFileObject[1];
                    else
                        theMediaObj.m_oMediaFiles = (ApiObjects.MediaFileObject[])(ResizeArray(theMediaObj.m_oMediaFiles, theMediaObj.m_oMediaFiles.Length + 1));
                    theMediaObj.m_oMediaFiles[theMediaObj.m_oMediaFiles.Length - 1] = oMediaFile;
                }

            }
            else
                return "";

            return sRet.ToString();
        }

        static protected void GetFileTypesXML(Int32 nMediaID, bool bWithCach, ref ApiObjects.FileTypeContainer[] theFileTypes)
        {
            if (CachingManager.CachingManager.Exist("ws.GetFileTypesXML" + nMediaID.ToString()) == true && bWithCach == true)
                theFileTypes = ((ApiObjects.FileTypeContainer[])(CachingManager.CachingManager.GetCachedData("ws.GetFileTypesXML" + nMediaID.ToString())));
            else
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select gmt.description,mf.MEDIA_TYPE_ID,mf.id FROM groups_media_type gmt (nolock),media_files mf WHERE mf.MEDIA_TYPE_ID=gmt.MEDIA_TYPE_ID and mf.is_active=1 and mf.status=1 and mf.group_id=gmt.group_id and gmt.is_active=1 and gmt.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_ID", "=", nMediaID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        theFileTypes = new ApiObjects.FileTypeContainer[nCount + 1];
                    for (int i = 0; i < nCount; i++)
                    {
                        string sID = selectQuery.Table("query").DefaultView[i].Row["MEDIA_TYPE_ID"].ToString();
                        string sMediaFileID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                        string sDesc = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        theFileTypes[i] = new ApiObjects.FileTypeContainer();
                        theFileTypes[i].Initialize(sDesc, int.Parse(sID));
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (bWithCach == true && theFileTypes != null)
                    CachingManager.CachingManager.SetCachedData("ws.GetFileTypesXML" + nMediaID.ToString(), theFileTypes, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nMediaID, true);
            }
        }

        static protected string GetFileTypesXML(Int32 nMediaID, bool bWithCach)
        {
            StringBuilder sRet = new StringBuilder();
            if (CachingManager.CachingManager.Exist("GetFileTypesXML" + nMediaID.ToString()) == true && bWithCach == true)
                sRet.Append(CachingManager.CachingManager.GetCachedData("GetFileTypesXML" + nMediaID.ToString()).ToString());
            else
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select gmt.description,gmt.MEDIA_TYPE_ID,mf.id FROM groups_media_type gmt (nolock),media_files mf WHERE mf.MEDIA_TYPE_ID=gmt.MEDIA_TYPE_ID and mf.is_active=1 and mf.status=1 and mf.group_id=gmt.group_id and gmt.is_active=1 and gmt.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_ID", "=", nMediaID);
                //selectQuery += "select gmt.description,gmt.MEDIA_TYPE_ID,mf.id FROM groups_media_type gmt (nolock),media_files mf (nolock) WHERE mf.MEDIA_TYPE_ID=gmt.id and mf.is_active=1 and mf.status=1 and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_ID", "=", nMediaID);
                if (selectQuery.Execute("query", true) != null)
                {
                    sRet.Append("<media_types media_id=\"").Append(nMediaID.ToString()).Append("\">");
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sID = selectQuery.Table("query").DefaultView[i].Row["MEDIA_TYPE_ID"].ToString();
                        string sMediaFileID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                        string sDesc = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        sRet.Append("<type id=\"").Append(sID).Append("\" description=\"").Append(XMLEncode(sDesc, true)).Append("\" media_file_id=\"" + sMediaFileID + "\" />");
                    }
                    sRet.Append("</media_types>");
                }
                selectQuery.Finish();
                selectQuery = null;
                if (bWithCach == true)
                    CachingManager.CachingManager.SetCachedData("GetFileTypesXML" + nMediaID.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nMediaID, true);
            }
            return sRet.ToString();
        }

        static public string GetMediaTagsIDs(Int32 nMediaID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("(");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct mt.tag_id from media_tags mt (nolock),tags t  WITH (nolock) where mt.tag_id=t.id and t.status=1 and mt.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    string sTagID = selectQuery.Table("query").DefaultView[i].Row["tag_id"].ToString();
                    sRet.Append(sTagID);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            return sRet.ToString();
        }


        static public string Get3ChoisesMediaValues(Int32 nMediaID, Int32 nWatcherID)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", nMediaID).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_3choise_types (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    string sChoise1 = selectQuery.Table("query").DefaultView[i].Row["CHOISE1"].ToString();
                    string sChoise2 = selectQuery.Table("query").DefaultView[i].Row["CHOISE2"].ToString();
                    Int32 nChoise1Cnt = 0;
                    Int32 nChoise2Cnt = 0;
                    Int32 nWatcheChoise = 0;
                    Get3ChoisesCounts(nWatcherID, nMediaID, int.Parse(sID), ref nChoise1Cnt, ref nChoise2Cnt, ref nWatcheChoise);
                    sRet.Append("<choise3 id=\"").Append(sID).Append("\" type=\"").Append(XMLEncode(sName, true)).Append("\" choise1=\"").Append(XMLEncode(sChoise1, true)).Append("\" choise2=\"").Append(XMLEncode(sChoise2, true)).Append("\" choise1_count=\"").Append(nChoise1Cnt).Append("\" choise2_count=\"").Append(nChoise2Cnt).Append("\" watcher_status=\"").Append(nWatcheChoise).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public void Get3ChoisesCounts(Int32 nWatcherID, Int32 nMediaID, Int32 nMedia_3choise_id, ref Int32 nChoise1Cnt, ref Int32 nChoise2Cnt, ref Int32 nUserChoise)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co,CHOISE_1_OR_2 from  watchers_media_3choise (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_3choise_id", "=", nMedia_3choise_id);
            selectQuery += " group by CHOISE_1_OR_2";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nCo = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                    Int32 nChoise1Ot2 = int.Parse(selectQuery.Table("query").DefaultView[i].Row["CHOISE_1_OR_2"].ToString());
                    if (nChoise1Ot2 == 1)
                        nChoise1Cnt = nCo;
                    if (nChoise1Ot2 == 2)
                        nChoise2Cnt = nCo;
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select CHOISE_1_OR_2 from  watchers_media_3choise (nolock) where ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("media_3choise_id", "=", nMedia_3choise_id);
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                if (nCount1 > 0)
                {
                    Int32 nChoise1Ot2 = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["CHOISE_1_OR_2"].ToString());
                    if (nChoise1Ot2 == 1)
                        nUserChoise = 1;
                    if (nChoise1Ot2 == 2)
                        nUserChoise = 2;
                }
            }
            selectQuery1.Finish();
            selectQuery1 = null;
        }

        static public Int32 UserVoteValueForMedia(Int32 nWatcherID, Int32 nMediaID, bool bWithCach, bool bWritable, int siteGuid)
        {
            Int32 nRet = -1;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //if (bWithCach == false)
            selectQuery.SetCachedSec(0);
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += "select RATE_VAL from watchers_media_rating (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            if (siteGuid > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", siteGuid);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["RATE_VAL"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public void GetCountersFromMedia(Int32 nMediaID, ref Int32 nViews, ref Int32 nVotesSum, ref Int32 nVotesCnt, ref double dAvg
            , ref Int32 nVotesLoCnt
            , ref Int32 nVotesUpCnt
            , ref Int32 nVotes1Cnt
            , ref Int32 nVotes2Cnt
            , ref Int32 nVotes3Cnt
            , ref Int32 nVotes4Cnt
            , ref Int32 nVotes5Cnt,
            bool bWritable)
        {
            nViews = 0;

            nVotesSum = 0;
            nVotesCnt = 0;
            dAvg = 0.0;
            nVotesLoCnt = 0;
            nVotesUpCnt = 0;
            nVotes1Cnt = 0;
            nVotes2Cnt = 0;
            nVotes3Cnt = 0;
            nVotes4Cnt = 0;
            nVotes5Cnt = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select * from media  WITH (nolock) where ";
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VIEWS"].ToString());
                    nVotesSum = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_SUM"].ToString());
                    nVotesCnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_COUNT"].ToString());
                    if (nVotesCnt > 0)
                    {
                        dAvg = (double)((double)nVotesSum / (double)nVotesCnt);
                    }

                    nVotesLoCnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_LO_COUNT"].ToString());
                    nVotesUpCnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_UP_COUNT"].ToString());
                    nVotes1Cnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_1_COUNT"].ToString());
                    nVotes2Cnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_2_COUNT"].ToString());
                    nVotes3Cnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_3_COUNT"].ToString());
                    nVotes4Cnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_4_COUNT"].ToString());
                    nVotes5Cnt = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VOTES_5_COUNT"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public bool DoesCallerPermittedIP(Int32 nGroupID)
        {
            if (HttpContext.Current.Session["caller_allowed" + nGroupID.ToString()] != null)
                return (bool)(HttpContext.Current.Session["caller_allowed" + nGroupID.ToString()]);
            bool bAllowedIP = false;
            string sIP = PageUtils.GetCallerIP();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups_ips (nolock) where ADMIN_OPEN=1 and status=1 and is_active=1 and (end_date is null or end_date>getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP.ToString().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    bAllowedIP = true;
            }
            selectQuery.Finish();
            selectQuery = null;
            HttpContext.Current.Session["caller_allowed" + nGroupID.ToString()] = bAllowedIP;
            return bAllowedIP;
        }

        static public string GetMediaInfoInner(Int32 nMediaID, Int32 nLangID, bool bIsLangMain, Int32 nWatcherID, bool bWithCach, ref XmlNode theInfoStruct, bool bEnterToCache, bool bStatistics, bool bPersonal,
            ref ApiObjects.MediaInfoObject theInfo, ref ApiObjects.MediaPersonalStatistics thePersonalStatistics,
            ref ApiObjects.MediaStatistics theMediaStatistics)
        {
            StringBuilder sRet = new StringBuilder();
            string sInfoSigStruct = ConvertXMLToString(ref theInfoStruct, false);
            if (theInfo == null && CachingManager.CachingManager.Exist("GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct) == true && bWithCach == true)
                sRet.Append(CachingManager.CachingManager.GetCachedData("GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct).ToString());
            else if (theInfo != null && CachingManager.CachingManager.Exist("ws.GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct) == true && bWithCach == true)
            {
                theInfo = (ApiObjects.MediaInfoObject)(CachingManager.CachingManager.GetCachedData("ws.GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct));
            }
            else
            {
                sRet.Append(GetMetaFieldsvalues(nMediaID, nLangID, bIsLangMain, ref theInfoStruct, false, ref theInfo));
                sRet.Append("<tags_collections>");
                sRet.Append(GetTagTypesMediaValues(nMediaID, nLangID, bIsLangMain, ref theInfoStruct, false, ref theInfo));
                sRet.Append("</tags_collections>");
                if (bEnterToCache == true && theInfo == null)
                    CachingManager.CachingManager.SetCachedData("GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct, sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nMediaID, true);
                if (bEnterToCache == true && theInfo != null)
                    CachingManager.CachingManager.SetCachedData("ws.GetMediaInfoInner" + nMediaID.ToString() + "_" + nLangID.ToString() + "_" + sInfoSigStruct, theInfo, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nMediaID, true);
            }

            Int32 nViews = 0;

            Int32 nVotesSum = 0;
            Int32 nVotesCnt = 0;
            double dAvg = 0.0;

            Int32 nVotesUpCnt = 0;
            Int32 nVotesLoCnt = 0;
            Int32 nVotes1Cnt = 0;
            Int32 nVotes2Cnt = 0;
            Int32 nVotes3Cnt = 0;
            Int32 nVotes4Cnt = 0;
            Int32 nVotes5Cnt = 0;

            Int32 nUserVote = -1;



            if (bPersonal == true)
                nUserVote = UserVoteValueForMedia(nWatcherID, nMediaID, bWithCach, false, 0);
            if (bStatistics == true)
            {
                GetCountersFromMedia(nMediaID, ref nViews, ref nVotesSum, ref nVotesCnt, ref dAvg
                    , ref nVotesLoCnt
                    , ref nVotesUpCnt
                    , ref nVotes1Cnt
                    , ref nVotes2Cnt
                    , ref nVotes3Cnt
                    , ref nVotes4Cnt
                    , ref nVotes5Cnt,
                    false);

                if (theInfo == null)
                {
                    sRet.Append("<views count=\"").Append(nViews).Append("\"/>");
                    sRet.Append("<rating sum=\"").Append(nVotesSum).Append("\" count=\"").Append(nVotesCnt).Append("\" avg=\"").Append(dAvg).Append("\" ");
                }
                else
                {
                    if (theMediaStatistics == null)
                        theMediaStatistics = new ApiObjects.MediaStatistics();
                    theMediaStatistics.Initialize(nViews, nVotesSum, nVotesCnt, nVotesLoCnt,
                        nVotesUpCnt, nVotes1Cnt, nVotes2Cnt, nVotes3Cnt, nVotes4Cnt, nVotes5Cnt);
                }

                if (bPersonal == true && theInfo == null)
                {
                    sRet.Append(" user_voted=\"");
                    if (nUserVote != -1)
                        sRet.Append("true");
                    else
                        sRet.Append("false");
                    sRet.Append("\" ");

                    sRet.Append("user_voted_val=\"");
                    if (nUserVote != -1)
                        sRet.Append(nUserVote);
                    sRet.Append("\"");
                }
                if (bPersonal == true && theInfo != null)
                {
                    if (thePersonalStatistics == null)
                        thePersonalStatistics = new ApiObjects.MediaPersonalStatistics();
                    bool bDidVoted = false;
                    if (nUserVote != -1)
                        bDidVoted = true;
                    thePersonalStatistics.Initialize(nWatcherID, nUserVote, bDidVoted);
                }
                if (theInfo == null)
                {
                    sRet.Append(">");
                    sRet.Append("<rate val=\"lo\" count=\"").Append(nVotesLoCnt).Append("\"/>");
                    sRet.Append("<rate val=\"up\" count=\"").Append(nVotesUpCnt).Append("\"/>");
                    sRet.Append("<rate val=\"1\" count=\"").Append(nVotes1Cnt).Append("\"/>");
                    sRet.Append("<rate val=\"2\" count=\"").Append(nVotes2Cnt).Append("\"/>");
                    sRet.Append("<rate val=\"3\" count=\"").Append(nVotes3Cnt).Append("\"/>");
                    sRet.Append("<rate val=\"4\" count=\"").Append(nVotes4Cnt).Append("\"/>");
                    sRet.Append("<rate val=\"5\" count=\"").Append(nVotes5Cnt).Append("\"/>");
                    sRet.Append("</rating>");
                }
            }
            return sRet.ToString();
        }

        static protected Int32 GetWatcherGroupFieldID(Int32 nWatcherID, Int32 nGroupID, ref string sGuid, bool bWritable)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += "select id,GROUP_GUID from watchers_groups_data WITH (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    object oGuid = selectQuery.Table("query").DefaultView[0].Row["GROUP_GUID"];
                    if (oGuid != null && oGuid != DBNull.Value)
                        sGuid = oGuid.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static public Int32 CreateNewWatchGroupField(Int32 nWatcherID, Int32 nGroupID, string sSiteGUID)
        {
            return CreateNewWatchGroupField(nWatcherID, nGroupID, sSiteGUID, true);
        }

        static public Int32 CreateNewWatchGroupField(Int32 nWatcherID, Int32 nGroupID, string sSiteGUID, bool bWithSession)
        {
            if (sSiteGUID.Trim() == "")
                return 0;
            string sCurrentSiteGUID = "";
            Int32 nRet = GetWatcherGroupFieldID(nWatcherID, nGroupID, ref sCurrentSiteGUID, false);
            if (nRet == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_groups_data");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_GUID", "=", sSiteGUID);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                nRet = GetWatcherGroupFieldID(nWatcherID, nGroupID, ref sCurrentSiteGUID, true);
            }
            else if (sCurrentSiteGUID != sSiteGUID && sSiteGUID.Trim() != "")
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("watchers_groups_data");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_GUID", "=", sSiteGUID);
                updateQuery += "where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRet);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            if (bWithSession == true)
                HttpContext.Current.Session["watchers_groups_data_" + nWatcherID.ToString() + "_" + nGroupID.ToString() + "_" + sSiteGUID] = nRet;
            return nRet;
        }

        static protected Int32 GetWatcherID(string sTVinciGUID, bool bWritable)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from watchers WITH (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TVINCI_GUID", "=", sTVinciGUID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 CreateNewWatcherField(string sTVinciGUID, string sUserAgent, string sCallerIP)
        {
            Int32 nRet = GetWatcherID(sTVinciGUID, false);
            if (nRet != 0)
                return nRet;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TVINCI_GUID", "=", sTVinciGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sCallerIP);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BROWSER", "=", sUserAgent);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            nRet = GetWatcherID(sTVinciGUID, true);

            return nRet;
        }

        static public bool GetAdminTokenValues(string sAdminToken, string sIP, ref Int32 nCountryID, ref string sLang, ref Int32 nDeviceID, Int32 nGroupID, ref bool bAdmin, ref bool bWithCache)
        {
            if (sAdminToken == "")
                return false;
            if (CachingManager.CachingManager.Exist(sAdminToken + "_" + sIP) == true)
            {
                string sCached = CachingManager.CachingManager.GetCachedData(sAdminToken + "_" + sIP).ToString();
                string[] sSep = { "|" };
                string[] sAll = sCached.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                if (sAll.Length == 3)
                {
                    bAdmin = true;
                    bWithCache = false;
                    HttpContext.Current.Session["ODBC_CACH_SEC"] = "0";
                    nCountryID = int.Parse(sAll[0].ToString());
                    sLang = sAll[1].ToString();
                    nDeviceID = int.Parse(sAll[2].ToString());
                    return true;
                }
            }
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from admin_tokens where status=1 and END_DATE>getdate() and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TOKEN", "=", sAdminToken);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    bAdmin = true;
                    bWithCache = false;
                    HttpContext.Current.Session["ODBC_CACH_SEC"] = "0";
                    nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUNTRY_ID"].ToString());
                    Int32 nLanguageID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                    object oLang = ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "NAME", nCountryID);
                    if (oLang != null && oLang != DBNull.Value)
                        sLang = oLang.ToString();
                    nDeviceID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["DEVICE_ID"].ToString());
                    string sToCache = nCountryID.ToString() + "|" + sLang + "|" + nDeviceID.ToString();
                    CachingManager.CachingManager.SetCachedData(sAdminToken + "_" + sIP, sToCache, 3600, System.Web.Caching.CacheItemPriority.Default, 0, false);
                    bRet = true;
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        static public Int32 GetStartValues(ref XmlDocument theDoc, ref Int32 nGroupID, ref string sTVinciGUID, ref string sLastTVinciDate, ref string sLastSiteDate, string sSiteGUID, ref Int32 nCountryID, ref Int32 nPlayerID, bool bCreate, ref Int32 nDeviceID, ref string sLang, ref bool bAdmin, ref bool bWithCache)
        {
            string sPlayerUN = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_un");
            string sPlayerPass = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_pass");
            string sProfile = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "profile");
            string sAdminToken = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "admin_token");
            string sIP = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "client_IP");

            if (string.IsNullOrEmpty(sIP))
                sIP = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "user_IP");


            //If the session has a tvinciguid
            if (HttpContext.Current.Session["tvinci_api"] != null &&
                HttpContext.Current.Session["tvinci_api"].ToString() != "" &&
                HttpContext.Current.Session["tvinci_api"].ToString() != "0")
                sTVinciGUID = HttpContext.Current.Session["tvinci_api"].ToString();
            //If the session does not haves a tvinciguid
            if (sTVinciGUID == "")
                sTVinciGUID = CookieUtils.GetCookie("tvinci_api");
            if (sTVinciGUID == "" || sTVinciGUID == "0")
                sTVinciGUID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "tvinci_guid");
            if (sTVinciGUID.Length > 36)
                sTVinciGUID = sTVinciGUID.Substring(0, 36);
            if (string.IsNullOrEmpty(sIP))
            {
                sIP = PageUtils.GetCallerIP();
            }
            Int32 nWatcherID = 0;
            string sSessionKey = "";
            if (sTVinciGUID != "")
                sSessionKey = sPlayerUN + "_" + sPlayerPass + "_" + sProfile + "_" + sAdminToken + "_" + sTVinciGUID + "_" + sIP + "_" + sSiteGUID;
            else
                sSessionKey = sPlayerUN + "_" + sPlayerPass + "_" + sProfile + "_" + sAdminToken + "_sTVinciGUID_" + sIP + "_" + sSiteGUID;
            if (sTVinciGUID != "" && sTVinciGUID != "0" && HttpContext.Current.Session[sSessionKey] != null && HttpContext.Current.Session[sSessionKey].ToString() != "")
            {
                string[] sSep = { "|" };
                string[] sSplited = HttpContext.Current.Session[sSessionKey].ToString().Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                if (sSplited.Length == 8)
                {
                    nWatcherID = int.Parse(sSplited[0].ToString());
                    nGroupID = int.Parse(sSplited[1].ToString());
                    sTVinciGUID = sSplited[2].ToString();
                    nCountryID = int.Parse(sSplited[3].ToString());
                    nPlayerID = int.Parse(sSplited[4].ToString());
                    nDeviceID = int.Parse(sSplited[5].ToString());
                    bAdmin = bool.Parse(sSplited[6].ToString());
                    bWithCache = bool.Parse(sSplited[7].ToString());
                    return nWatcherID;
                }
            }

            if (sAdminToken != "")
            {
                nCountryID = 0;
                GetAdminTokenValues(sAdminToken, sIP, ref nCountryID, ref sLang, ref nDeviceID, nGroupID, ref bAdmin, ref bWithCache);
            }
            if (nCountryID == 0)
            {
                nCountryID = PageUtils.GetIPCountry2(sIP);
            }

            nGroupID = PageUtils.GetGroupByUNPass(sPlayerUN, sPlayerPass, ref nPlayerID);
            if (nGroupID == 0)
                return 0;

            string sUserAgent = "";
            try
            {
                sUserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
                if (string.IsNullOrEmpty(sUserAgent) || (sUserAgent.Trim() == ""))
                {
                    return 0;
                }
                else
                {
                    sUserAgent = HttpContext.Current.Request.Browser.Type;// +Request.Browser.Platform;
                }
            }
            catch
            {
                return 0;
            }

            Int32 nLastWatcherID = 0;
            if (HttpContext.Current.Session["tvinci_watcher"] != null &&
                HttpContext.Current.Session["tvinci_watcher"].ToString() != "")
            {
                nLastWatcherID = int.Parse(HttpContext.Current.Session["tvinci_watcher"].ToString());
                nWatcherID = nLastWatcherID;
            }

            if (sSiteGUID.Trim() != "")
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetCachedSec(0);
                selectQuery += "select wgd.WATCHER_ID,w.TVINCI_GUID from watchers_groups_data wgd (nolock),watchers (nolock) w where wgd.WATCHER_ID=w.id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_GUID", "=", sSiteGUID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nWatcherID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["WATCHER_ID"].ToString());
                        sTVinciGUID = selectQuery.Table("query").DefaultView[0].Row["TVINCI_GUID"].ToString();
                        if (nLastWatcherID != nWatcherID && nLastWatcherID != 0)
                        {
                            //here we will move all old channels to the new account
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            updateQuery += "and";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nLastWatcherID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }

                    }
                    else
                    {
                        if (sTVinciGUID == "")
                            sTVinciGUID = System.Guid.NewGuid().ToString();
                        if (bCreate == true && nWatcherID == 0)
                            nWatcherID = AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                if (sTVinciGUID != "")
                {
                    //Add session based on tvinciguid
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select id from watchers (nolock) where ";
                    selectQuery.SetCachedSec(0);
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvinci_guid", "=", sTVinciGUID);
                    selectQuery += "order by id desc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nWatcherID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    if (nWatcherID == 0)
                    {
                        //Add to session
                        if (bCreate == true)
                            nWatcherID = AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID);
                    }
                }
                else
                {
                    if (bCreate == true)
                    {
                        //add to session
                        sTVinciGUID = System.Guid.NewGuid().ToString();
                        if (nWatcherID == 0)
                            nWatcherID = AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID);
                    }
                }
            }
            if (nWatcherID != 0)
                HttpContext.Current.Session["tvinci_watcher"] = nWatcherID;
            else
            {
                if (HttpContext.Current.Session["tvinci_watcher"] != null &&
                    HttpContext.Current.Session["tvinci_watcher"].ToString() != "" &&
                    HttpContext.Current.Session["tvinci_watcher"].ToString() != "0")
                {
                    nWatcherID = int.Parse(HttpContext.Current.Session["tvinci_watcher"].ToString());
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select tvinci_guid from watchers (nolock) where ";
                    selectQuery.SetCachedSec(0);
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nWatcherID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sTVinciGUID = selectQuery.Table("query").DefaultView[0].Row["tvinci_guid"].ToString();
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                //HttpContext.Current.Session["tvinci_watcher"] = null;
            }
            HttpContext.Current.Session["tvinci_api"] = sTVinciGUID;
            if (HttpContext.Current.Session["profile_check"] == null &&
                sProfile != "" &&
                nWatcherID != 0)
            {
                HttpContext.Current.Session["profile_check"] = "1";
                HandleProfile(nWatcherID, sProfile, nGroupID);

            }
            string sToSession = nWatcherID.ToString() + "|" + nGroupID.ToString() + "|" + sTVinciGUID.ToString() + "|" + nCountryID.ToString() + "|" + nPlayerID.ToString() + "|" + nDeviceID.ToString() + "|" + bAdmin.ToString() + "|" + bWithCache.ToString();
            sSessionKey = sSessionKey.Replace("sTVinciGUID", sTVinciGUID);
            HttpContext.Current.Session[sSessionKey] = sToSession;
            return nWatcherID;
        }

        static protected void ConnectWatcherToProfileTag(Int32 nWatcherID, Int32 nProfileTagID, Int32 nProfileTagTypeID, Int32 nGroupID, DateTime utcNow)
        {
            Int32 nID = 0;
            Int32 nCurrentProfileTagID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id,PROFILE_TAG_ID from watchers_profile_tags (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "<=", utcNow);
            selectQuery += " and (end_date is null or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", ">", utcNow);
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PROFILE_TAG_TYPE_ID", "=", nProfileTagTypeID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nCurrentProfileTagID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PROFILE_TAG_ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nID != 0)
            {
                if (nCurrentProfileTagID == nProfileTagID)
                    return;
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("watchers_profile_tags");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", utcNow);
                updateQuery += " where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_profile_tags");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PROFILE_TAG_ID", "=", nProfileTagID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PROFILE_TAG_TYPE_ID", "=", nProfileTagTypeID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", utcNow);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }

        static protected Int32 GetProfileTagID(string sVal, Int32 nProfileTagTypeID, Int32 nGroupID, bool bCreate, bool bWritable)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += "select id from profile_tags (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(VALUE)))", "=", sVal.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PROFILE_TAG_TYPE_ID", "=", nProfileTagTypeID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nRet != 0 || bCreate == false)
                return nRet;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("profile_tags");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sVal);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sVal);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PROFILE_TAG_TYPE_ID", "=", nProfileTagTypeID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return GetProfileTagID(sVal, nProfileTagTypeID, nGroupID, false, true);
        }

        static protected Int32 GetProfileTagTypeID(string sProfileTagName, Int32 nGroupID, bool bCreate, bool bWritable)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from profile_tags_types (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(name)))", "=", sProfileTagName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nRet != 0 || bCreate == false)
                return nRet;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("profile_tags_types");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sProfileTagName.Trim().ToLower());
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sProfileTagName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return GetProfileTagTypeID(sProfileTagName, nGroupID, false, true);
        }

        static protected void HandleProfile(Int32 nWatcherID, string sProfile, Int32 nGroupID)
        {
            DateTime utcNow = ODBCWrapper.Utils.GetCurrentDBTime();
            string sProfileTagsTypeHandled = "";
            string[] sep = { "||" };
            string[] splited = sProfile.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            Int32 nLength = splited.Length;
            for (int i = 0; i < nLength; i++)
            {
                string sStr = splited[i];
                string[] innerSep = { "~" };
                string[] innerSplited = sStr.Split(innerSep, StringSplitOptions.RemoveEmptyEntries);
                if (innerSplited.Length == 2)
                {
                    string sName = innerSplited[0];
                    string sVal = innerSplited[1];
                    Int32 nProfileTagTypeID = GetProfileTagTypeID(sName, nGroupID, true, false);
                    if (nProfileTagTypeID == 0)
                        continue;

                    if (sProfileTagsTypeHandled != "")
                        sProfileTagsTypeHandled += ",";
                    sProfileTagsTypeHandled += nProfileTagTypeID.ToString();

                    Int32 nProfileTagID = GetProfileTagID(sVal, nProfileTagTypeID, nGroupID, false, false);
                    if (nProfileTagID == 0)
                        continue;
                    ConnectWatcherToProfileTag(nWatcherID, nProfileTagID, nProfileTagTypeID, nGroupID, utcNow);
                }
            }

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("watchers_profile_tags");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", utcNow);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (sProfileTagsTypeHandled != "")
            {
                updateQuery += " and PROFILE_TAG_TYPE_ID not in (";
                updateQuery += sProfileTagsTypeHandled;
                updateQuery += ")";
            }
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }


        protected void GetDetailsByIP(ref string sTVinciGUID, ref Int32 nWatcherID)
        {
            string sCallerIP = PageUtils.GetCallerIP();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select TVINCI_GUID,ID from watchers (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sCallerIP);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sTVinciGUID = selectQuery.Table("query").DefaultView[0].Row["TVINCI_GUID"].ToString();
                    nWatcherID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public Int32 AddWatcherField(string sTVinciGUID, string sUserAgent, string sSiteGUID, Int32 nGroupID)
        {
            string sCallerIP = PageUtils.GetCallerIP();
            return AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, true);
        }

        static public Int32 AddWatcherField(string sTVinciGUID, string sUserAgent, string sSiteGUID, Int32 nGroupID, string sCallerIP, bool bSetSession)
        {
            Int32 nWatcherID = CreateNewWatcherField(sTVinciGUID, sUserAgent, sCallerIP);
            if (bSetSession == true)
                HttpContext.Current.Session["tvinci_watcher"] = nWatcherID;
            if (sSiteGUID != "")
                CreateNewWatchGroupField(nWatcherID, nGroupID, sSiteGUID, bSetSession);
            return nWatcherID;

        }

        static public string GetErrorMessage(string sMessage)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"error\">");
            sRet.Append("<error message=\"").Append(ProtocolsFuncs.XMLEncode(sMessage, true)).Append("\"/>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string VboxXmlProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"vbox_xml\">");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.ADDITIONAL_DATA,mf.id from media_files mf (nolock),media m WITH (nolock) where mf.additional_data is not null and mf.additional_data <>'' and mf.media_id=m.id and m.is_active=1 and m.status=1 and mf.is_active=1 and mf.status=1 and mf.group_id ";
            selectQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                sRet.Append("<tunerChannelList>");
                for (int i = 0; i < nCount; i++)
                {
                    try
                    {
                        string sAddData = HttpContext.Current.Server.HtmlDecode(selectQuery.Table("query").DefaultView[i].Row["ADDITIONAL_DATA"].ToString()).Replace("<br\\>", "");
                        string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                        XmlDocument theAddDataXML = new XmlDocument();
                        theAddDataXML.LoadXml(sAddData);
                        if (theAddDataXML.SelectSingleNode("Channel") != null)
                        {
                            XmlAttribute attr = theAddDataXML.CreateAttribute("uniqueChannelIndex");
                            attr.Value = sID;
                            theAddDataXML.SelectSingleNode("Channel").Attributes.Append(attr);
                        }
                        string sNewSigXML = ProtocolsFuncs.GetSig(ref theAddDataXML, false);
                        sRet.Append(sNewSigXML);
                    }
                    catch
                    { }
                }
                sRet.Append("</tunerChannelList>");
            }

            selectQuery.Finish();
            selectQuery = null;
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string StartingProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"starting\">");

            sRet.Append("<watcher tvinci_id=\"").Append(nWatcherID).Append("\" guid=\"").Append(ProtocolsFuncs.XMLEncode(sTVinciGUID, true)).Append("\" name=\"\" last_tvinci_watch_date=\"\" last_site_watch_date=\"\" site_guid=\"").Append(sSiteGUID).Append("\"/>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        protected Int32 GetCounterRecordID(Int32 nWatcherID, Int32 nGroupID, DateTime dNow)
        {
            Int32 nCounterID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from watchers_time_counters WITH (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNT_DATE", "=", dNow);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCounterID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nCounterID;
        }

        protected Int32 GetWatcherMediaCounterRecordID(Int32 nWatcherID, Int32 nMediaID, Int32 nMediaFileID, Int32 nGroupID, DateTime dNow, Int32 nCountryID, Int32 nPlayerID)
        {
            Int32 nCounterID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from watchers_media_play_counters with (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNT_DATE", "=", dNow);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAYER_ID", "=", nPlayerID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCounterID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nCounterID;
        }

        static protected string GetCategoryTranslation(Int32 nCategoryID, Int32 nLangID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select name from categories_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("category_id", "=", nCategoryID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["name"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected void GetChildCategories(ref XmlDocument theDoc, Int32 nCategoryID, ref StringBuilder sXML, Int32 nGroupID,
            Int32 nLangID, bool bIsLangMain, bool bWithChannels, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics,
            string sPicsForCache, ref ApiObjects.CategoryObject[] theChildCat, Int32 nCountryID, Int32 nDeviceID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from categories (nolock) where status=1 and IS_ACTIVE=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_CATEGORY_ID", "=", nCategoryID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nCatID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    string sTitle = "";
                    if (bIsLangMain == true)
                        sTitle = selectQuery.Table("query").DefaultView[i].Row["CATEGORY_NAME"].ToString();
                    else
                    {
                        sTitle = GetCategoryTranslation(nCatID, nLangID);
                        if (sTitle == "")
                            continue;
                    }
                    Int32 nPicID = 0;
                    if (selectQuery.Table("query").DefaultView[i].Row["PIC_ID"] != DBNull.Value && selectQuery.Table("query").DefaultView[i].Row["PIC_ID"] != null)
                        nPicID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["PIC_ID"].ToString());

                    if (theDoc != null)
                    {
                        sXML.Append("<category id=\"").Append(nCatID).Append("\" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\" ");
                        sXML.Append(GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, 0, bIsAdmin, bWithCache, ref thePics, sPicsForCache));
                        sXML.Append(" >");
                    }
                    else
                    {
                        if (theChildCat == null)
                            theChildCat = new ApiObjects.CategoryObject[nCount];
                        theChildCat[i] = new ApiObjects.CategoryObject();
                        theChildCat[i].m_sTitle = sTitle;
                        theChildCat[i].m_nID = nCatID;
                    }
                    ApiObjects.CategoryObject[] theCCat = null;
                    GetChildCategories(ref theDoc, nCatID, ref sXML, nGroupID, nLangID, bIsLangMain, bWithChannels, bIsAdmin, bWithCache,
                        ref thePics, sPicsForCache, ref theCCat, nCountryID, nDeviceID);
                    if (bWithChannels == true)
                    {
                        ApiObjects.ChannelObject[] theChannels = null;
                        sXML.Append(GetChannelsForCategory(ref theDoc, nCatID, nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref thePics, ref theChannels, nCountryID, nDeviceID));
                        if (theDoc == null)
                            theChildCat[i].m_oChannels = theChannels;
                    }
                    if (theDoc == null)
                        theChildCat[i].m_oCategories = theCCat;
                    else
                        sXML.Append("</category>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected string GetPicSizesXMLParts(ref XmlDocument theDoc, Int32 nPicID, Int32 nGroupID, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics, string sPicsForCache)
        {
            return ProtocolsFuncs.GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, 0, bIsAdmin, bWithCache, ref thePics, sPicsForCache);
        }

        static public void RemoveFlashVarsParameter(ref XmlDocument theDoc, string sPar)
        {
            if (theDoc.SelectSingleNode("root/flashvars/@" + sPar) != null)
            {
                XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes[sPar];
                attr.OwnerElement.RemoveAttribute(sPar);
            }
        }

        static public string CategoriesListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, bool bWithCache,
            ref ApiObjects.InitializationObject initObj, ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            Int32 nWSCategoryID, bool bWithChannels, ref ApiObjects.CategoryObject[] theCategories, Int32 nCountryID, Int32 nDeviceID, bool isTree)
        {
            string sPicSizeForCache = "";
            if (theDoc != null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else if (initObj.m_oPicObjects != null)
                sPicSizeForCache = GetPicSizeForCache(ref initObj.m_oPicObjects);
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            if (initObj == null)
            {
                string sCategoryID = "";
                XmlNode theCategoryID = theDoc.SelectSingleNode("/root/request/category/@id");
                if (theCategoryID != null)
                    sCategoryID = theCategoryID.Value.ToUpper();

                string sWithChannels = "";

                XmlNode theWithChannels = theDoc.SelectSingleNode("/root/request/params/@with_channels");
                if (theWithChannels != null)
                    sWithChannels = theWithChannels.Value.ToUpper();

                if (sCategoryID != "")
                    RemoveFlashVarsParameter(ref theDoc, "category_id");

                if (sWithChannels != "")
                    RemoveFlashVarsParameter(ref theDoc, "with_channels");

                string sTheDoc = ProtocolsFuncs.GetSig(ref theDoc, true);

                if (CachingManager.CachingManager.Exist(sTheDoc) == true)
                    return CachingManager.CachingManager.GetCachedData(sTheDoc).ToString();

                if (sCategoryID == "")
                    sCategoryID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "category_id");
                if (sWithChannels == "")
                    sWithChannels = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "with_channels");
                bool bOK = true;

                if (sCategoryID == "")
                    sCategoryID = "0";
                StringBuilder sRet = new StringBuilder();
                Int32 nCategoryID = int.Parse(sCategoryID.ToString());
                if (nCategoryID != 0)
                {
                    object oParentGroupID = PageUtils.GetTableSingleVal("categories", "GROUP_ID", nCategoryID);
                    if (oParentGroupID != DBNull.Value && oParentGroupID != null)
                    {
                        if (int.Parse(oParentGroupID.ToString()) != nGroupID)
                            bOK = false;
                    }
                }

                if (bOK == false)
                    return GetErrorMessage("Category Dont Belong To Site");

                if (!isTree)
                {
                    sRet.Append("<response type=\"categories_tree\">");
                }
                if (isTree)
                {
                    sRet.Append("<response type=\"mh_categories_tree\">");
                    if (nCategoryID != 0)
                    {
                        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery += "select * from categories (nolock) where status=1 and IS_ACTIVE=1 and status=1 and ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCategoryID);
                        selectQuery += " and ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                        if (selectQuery.Execute("query", true) != null)
                        {
                            int count = selectQuery.Table("query").DefaultView.Count;
                            if (count > 0)
                            {
                                string sTitle = "";
                                if (bIsLangMain == true)
                                    sTitle = selectQuery.Table("query").DefaultView[0].Row["CATEGORY_NAME"].ToString();
                                else
                                {
                                    sTitle = GetCategoryTranslation(nCategoryID, nLangID);
                                }
                                Int32 nPicID = 0;
                                if (selectQuery.Table("query").DefaultView[0].Row["PIC_ID"] != DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["PIC_ID"] != null)
                                    nPicID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PIC_ID"].ToString());

                                if (theDoc != null)
                                {
                                    ApiObjects.PicObject[] theBasePics = null;
                                    sRet.Append("<category id=\"").Append(nCategoryID).Append("\" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\" ");
                                    sRet.Append(GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, 0, bIsAdmin, bWithCache, ref theBasePics, sPicSizeForCache));
                                    sRet.Append(" >");
                                }
                            }
                        }
                        selectQuery.Finish();
                        selectQuery = null;
                    }
                    else
                    {
                        ApiObjects.PicObject[] theBasePics = null;
                        sRet.Append("<category id=\"").Append(nCategoryID).Append("\" title=\"").Append(string.Empty).Append("\" ");
                        sRet.Append(GetPicSizesXMLParts(ref theDoc, 0, nGroupID, 0, bIsAdmin, bWithCache, ref theBasePics, sPicSizeForCache));
                        sRet.Append(" >");
                    }
                }
                ApiObjects.PicObject[] thePics = null;
                ApiObjects.ChannelObject[] theChannels = null;
                ApiObjects.CategoryObject[] theCCat = null;
                if (sWithChannels.Trim().ToLower() == "true")
                {
                    sRet.Append(GetChannelsForCategory(ref theDoc, nCategoryID, nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref thePics, ref theChannels, nCountryID, nDeviceID));
                }
                //if (theDoc != null)
                //{
                //    sRet.Append("</base_category>");
                //}
                if (sWithChannels.Trim().ToLower() == "true")
                    GetChildCategories(ref theDoc, nCategoryID, ref sRet, nGroupID, nLangID, bIsLangMain, true, bIsAdmin, bWithCache, ref thePics,
                        sPicSizeForCache, ref theCCat, nCountryID, nDeviceID);
                else
                    GetChildCategories(ref theDoc, nCategoryID, ref sRet, nGroupID, nLangID, bIsLangMain, false, bIsAdmin, bWithCache, ref thePics,
                        sPicSizeForCache, ref theCCat, nCountryID, nDeviceID);
                if (isTree)
                {
                    sRet.Append("</category>");
                }
                sRet.Append("</response>");
                CachingManager.CachingManager.SetCachedData(sTheDoc, sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Default, 0, false);
                return sRet.ToString();
            }
            else
            {
                string sTheDoc = "ws.CategoriesListProtocol_" + nGroupID.ToString() + "_" + nWSCategoryID.ToString() + "_" + bWithCache.ToString();

                if (CachingManager.CachingManager.Exist(sTheDoc) == true && bWithCache == true)
                {
                    theCategories = (ApiObjects.CategoryObject[])(CachingManager.CachingManager.GetCachedData(sTheDoc));
                }

                bool bOK = true;
                Int32 nCategoryID = nWSCategoryID;

                if (nCategoryID != 0)
                {
                    object oParentGroupID = PageUtils.GetTableSingleVal("categories", "GROUP_ID", nCategoryID);
                    if (oParentGroupID != DBNull.Value && oParentGroupID != null)
                    {
                        if (int.Parse(oParentGroupID.ToString()) != nGroupID)
                            bOK = false;
                    }
                }
                if (bOK == false)
                    return GetErrorMessage("Category Dont Belong To Site");
                StringBuilder sRet = new StringBuilder();
                ApiObjects.ChannelObject[] theChannels = null;
                ApiObjects.CategoryObject cat = new ApiObjects.CategoryObject();
                ApiObjects.CategoryObject[] theChildCat = null;
                if (bWithChannels)
                {

                    GetChannelsForCategory(ref theDoc, nCategoryID, nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref initObj.m_oPicObjects, ref theChannels, nCountryID, nDeviceID);
                    cat.m_oChannels = theChannels;
                    GetChildCategories(ref theDoc, nCategoryID, ref sRet, nGroupID, nLangID, bIsLangMain, true, bIsAdmin, bWithCache, ref initObj.m_oPicObjects,
                        sPicSizeForCache, ref theChildCat, nCountryID, nDeviceID);
                }
                else
                {
                    GetChildCategories(ref theDoc, nCategoryID, ref sRet, nGroupID, nLangID, bIsLangMain, false, bIsAdmin, bWithCache, ref initObj.m_oPicObjects,
                        sPicSizeForCache, ref theChildCat, nCountryID, nDeviceID);
                }
                cat.m_oCategories = theChildCat;
                if (theCategories == null)
                    theCategories = new ApiObjects.CategoryObject[1];
                else
                    theCategories = (ApiObjects.CategoryObject[])(ResizeArray(theCategories, theCategories.Length + 1));
                theCategories[theCategories.Length - 1] = cat;
                if (theCategories != null)
                    CachingManager.CachingManager.SetCachedData(sTheDoc, theCategories, 10800, System.Web.Caching.CacheItemPriority.Default, 0, false);
                return "";
            }
        }

        static public string RSSChannelsProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, bool bWithCache,
            Int32 nCountryID, Int32 nDeviceID)
        {
            string sPicSizeForCache = "";
            sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sTheDoc = ProtocolsFuncs.GetSig(ref theDoc, true);
            if (CachingManager.CachingManager.Exist(sTheDoc) == true)
                return CachingManager.CachingManager.GetCachedData(sTheDoc).ToString();
            StringBuilder sRet = new StringBuilder();

            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 100;

            XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/params/@start_index");
            if (theStartIndex != null)
                nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

            XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/params/@number_of_items");
            if (theNumOfItems != null)
                nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

            sRet.Append("<response type=\"rss_channels_list\">");
            ApiObjects.PicObject[] thePics = null;
            ApiObjects.ChannelObject[] theChannels = null;
            sRet.Append(GetRSSChannels(ref theDoc, nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref thePics, ref theChannels, nCountryID, nDeviceID, nStartIndex, nNumOfItems));
            sRet.Append("</response>");
            CachingManager.CachingManager.SetCachedData(sTheDoc, sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Default, 0, false);
            return sRet.ToString();
        }

        protected string GetPlayListSave(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"playlist_save\">");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static protected string GetPeopleWatchedMedia(Int32 nMediaID, Int32 nMediaFileID, Int32 nWatcherID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("(");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct top 30 watcher_id,max(create_date) from watchers_media_actions WITH (nolock) where ";
            selectQuery.SetWritable(true);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            if (nMediaFileID != 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                selectQuery += " and ";
            }
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("watcher_id", "<>", nWatcherID);
            //selectQuery += " and PLAY_TIME_COUNTER>30 group by watcher_id order by max(create_date)";
            selectQuery += " group by watcher_id order by max(create_date)";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["watcher_id"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            return sRet.ToString();
        }


        static public string PWWAWProtocol(ref XmlDocument theDoc,
            Int32 nGroupID,
            string sTVinciGUID,
            string sLastOnTvinci,
            string sLastOnSite,
            string sSiteGUID,
            Int32 nWatcherID,
            string sLang,
            Int32 nPlayerID,
            bool bIsAdmin,
            Int32 nCountryID,
            ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            ref ApiObjects.ChannelObject theChannelObj,
            Int32 nWSMediaID,
            Int32 nWSMediaFileID,
            Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            bool bWithInfo = false;
            XmlNode theInfoStruct = null;
            bool bWithFileTypes = false;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nMediaID = 0;
            Int32 nMediaFileID = 0;
            string sEndDateField = "m.";

            if (initObj == null)
            {
                string sMediaID = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();

                string sMediaFileID = "";
                XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/media/@file_id");
                if (theMediaFileID != null)
                    sMediaFileID = theMediaFileID.Value.ToUpper();

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
                nMediaID = int.Parse(sMediaID);
                if (sMediaFileID != "")
                    nMediaFileID = int.Parse(sMediaFileID);
            }
            else
            {
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);

                nMediaID = nWSMediaID;
                nMediaFileID = nWSMediaFileID;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);

            }

            string sPeopleWatched = GetPeopleWatchedMedia(nMediaID, nMediaFileID, nWatcherID);
            StringBuilder sRet = new StringBuilder();
            if (initObj == null)
                sRet.Append("<response type=\"people_who_watched_also_watched\">");


            ODBCWrapper.StoredProcedure spGet_AlsoWatchedMedias = new ODBCWrapper.StoredProcedure("TVinci..PWWAWProtocol");
            spGet_AlsoWatchedMedias.SetConnectionKey("CONNECTION_STRING");
            spGet_AlsoWatchedMedias.SetWritable(true);
            spGet_AlsoWatchedMedias.AddParameter("@MediaID", nMediaID);
            //spGet_AlsoWatchedMedias.AddParameter("@MediaFileID",  nMediaFileID);
            spGet_AlsoWatchedMedias.AddParameter("@GroupID", nGroupID);
            spGet_AlsoWatchedMedias.AddParameter("@Language", nLangID);
            spGet_AlsoWatchedMedias.AddParameter("@CountryID", nCountryID);
            spGet_AlsoWatchedMedias.AddParameter("@EndDateField", sEndDateField);
            spGet_AlsoWatchedMedias.AddParameter("@DeviceID", nDeviceID);
            spGet_AlsoWatchedMedias.AddParameter("@SiteGuid", nWatcherID);

            #region Commented

            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            ////selectQuery += "select distinct top 8 wmpc.media_id from watchers_media_actions wmpc (nolock),media m WITH (nolock) where ";
            //selectQuery += "select top 8 q.m_id from (select umm.media_id as m_id,count(*) as co from users_media_mark umm (nolock), media m WITH (nolock) where ";
            //selectQuery += " m.id=umm.media_id and m.status=1 and m.is_active=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("umm.media_id", "<>", nMediaID);

            //if (nMediaFileID != 0)
            //{
            //    Int32 nTypeID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "MEDIA_TYPE_ID", nMediaFileID).ToString());
            //selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.FILE_FORMAT_ID", "=", nTypeID);
            //}
            //selectQuery += " and wmpc.ACTION_ID=4 and ";
            //selectQuery += " (m.start_date<getdate() and (";
            //selectQuery += sEndDateField + " is null or " + sEndDateField + ">getdate())) and ";

            //selectQuery += "and (m.id not in (select id from media (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            //selectQuery += "))";
            //selectQuery += " and ";
            //selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            //selectQuery += "( COUNTRY_ID=0 or ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            //selectQuery += ") and (LANGUAGE_ID=0 or ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            //selectQuery += ") and (DEVICE_ID=0 or ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            //selectQuery += ") and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            //selectQuery += "))";
            //selectQuery += " and ";
            //selectQuery += "(";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);

            //if (sWPGID != "")
            //{
            //    selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
            //    selectQuery += sWPGID;
            //    selectQuery += ")";
            //}
            //selectQuery += ") and umm.site_user_guid in " + sPeopleWatched;
            //selectQuery += " group by umm.media_id )q where q.co>0 order by q.co desc";

            #endregion

            if (string.IsNullOrEmpty(sPeopleWatched) || sPeopleWatched.Equals("()"))
            {
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                sRet.Append("<channel id=\"\" media_count=\"").Append(0).Append("\" >");
            }
            else
            {
                DataSet ds = spGet_AlsoWatchedMedias.ExecuteDataSet();  //("TVinci..Get_MediaDetails", Params, "MAIN_CONNECTION_STRING", false);

                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    DataTable dt = ds.Tables[0];

                    int nCount = dt.DefaultView.Count;

                    sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                    //int nCount = selectQuery.Table("query").DefaultView.Count;

                    if (initObj == null)
                    {
                        sRet.Append("<channel id=\"\" media_count=\"").Append(nCount).Append("\" >");
                    }

                    for (int i = 0; i < nCount; i++)
                    {
                        int nLocMediaID = int.Parse(dt.DefaultView[i].Row["m_ID"].ToString());

                        if (initObj == null)
                        {
                            sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nLocMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                        }
                        else
                        {
                            Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                            string sFileFormat = "";

                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            {
                                sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                            }

                            string sSubFileFormat = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            {
                                sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                            }

                            string sFileQuality = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            {
                                sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                            }

                            bool bStatistics = false;
                            bool bPersonal = false;

                            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();

                            if (sStatistics == "true")
                            {
                                bStatistics = true;
                            }
                            if (sPersonal == "true")
                            {
                                bPersonal = true;
                            }

                            ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                            ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nLocMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                            if (theChannelObj.m_oMediaObjects == null)
                            {
                                theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                            }
                            else
                            {
                                theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                            }

                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                            theChannelObj.m_nChannelTotalSize = nCount;
                        }
                    }
                }
            }

            //selectQuery.Finish();
            //selectQuery = null;

            if (initObj == null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }

            return sRet.ToString();
        }


        static protected DataTable GetGroupDT(Int32 nGroupID)
        {
            DataTable dRet = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                dRet = selectQuery.Table("query").Copy();
            }
            selectQuery.Finish();
            selectQuery = null;
            return dRet;
        }


        static public string SetDurationProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            string sLang, Int32 nPlayerID, ref ApiObjects.InitializationObject initObj,
            Int32 nWSMediaFileID, Int32 nWSDurationInSec, ref bool bRet)
        {
            Int32 nMediaFileID = 0;
            Int32 nDurationInSec = 0;
            StringBuilder sRet = new StringBuilder();

            if (theDoc != null)
            {
                string sMediaFileID = "";
                XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/media/@file_id");
                if (theMediaFileID != null)
                    sMediaFileID = theMediaFileID.Value.ToUpper();

                string sMediaDuration = "";
                XmlNode theMediaDuration = theDoc.SelectSingleNode("/root/request/duration/@secs");
                if (theMediaDuration != null)
                    sMediaDuration = theMediaDuration.Value.ToUpper();

                nDurationInSec = int.Parse(sMediaDuration);
                nMediaFileID = int.Parse(sMediaFileID);
                sRet.Append("<response type=\"set_duration\">");
                sRet.Append("</response>");
            }
            else
            {
                nMediaFileID = nWSMediaFileID;
                nDurationInSec = nWSDurationInSec;
            }
            if (nMediaFileID != 0 && nDurationInSec != 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_files");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("duration", "=", nDurationInSec);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaFileID);
                updateQuery += " and ";
                string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
                updateQuery += " GROUP_ID " + sGroups;
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                bRet = true;
            }
            return sRet.ToString();
        }

        static protected void GetMediaBasicData(Int32 nMediaID, ref Int32 nType, ref string sName,
            ref string sDescription, ref string sMediaStruct,
            Int32 nLangID, bool bIsLangMain, ref XmlNode theInfoStruct, ref ApiObjects.MediaInfoObject theInfo)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media WITH (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    //nType = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"].ToString());
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["MEDIA_TYPE_ID"], ref nType);
                    sName = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                    sDescription = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                    sMediaStruct = "<root>";
                    sMediaStruct += "<tags_meta>";
                    sMediaStruct += ProtocolsFuncs.GetMetaFieldsvalues(nMediaID, nLangID, bIsLangMain, ref theInfoStruct, true, ref theInfo);
                    sMediaStruct += "</tags_meta>";
                    sMediaStruct += "<tags_collections>";
                    sMediaStruct += ProtocolsFuncs.GetMediaTagsForSearch(nMediaID, nLangID, bIsLangMain, true);
                    sMediaStruct += "</tags_collections>";
                    sMediaStruct += "</root>";
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public string SearchRelatedProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID,
            bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj,
            ref ApiObjects.PageDefinition thePageDef, Int32 nWSMediaID, Int32 nDeviceID)
        {
            Int32 nStartIndex = 0;
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            Int32 nNumOfItems = 8;
            bool bWithCache = true;
            string sEndDateField = "m.";
            string sDocStruct = "";
            string sPlayListSchema = "";
            XmlNode theInfoStruct = null;
            Int32 nMediaID = 0;
            string sMediaTypes = string.Empty;
            ApiObjects.MediaInfoObject theInfo = null;
            ApiObjects.PlayListSchema thePlaylistSchema = null;
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            bool bUseStartDate = true;
            string sUseStartDate = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "use_start_date");
            if (sUseStartDate == "false")
            {
                bUseStartDate = false;
            }

            if (initObj == null)
            {
                string sMediaID = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();


                string sMediaTypeID = string.Empty;
                XmlNode theMediaTypeID = theDoc.SelectSingleNode("/root/request/media/@media_types");
                if (theMediaTypeID != null)
                {
                    sMediaTypes = theMediaTypeID.Value;
                }

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;


                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;

                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

                sPlayListSchema = ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true, ref thePlaylistSchema);

                if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null && HttpContext.Current.Session["ODBC_CACH_SEC"].ToString() == "0")
                    bWithCache = false;

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
                sDocStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theDoc, true);
                nMediaID = int.Parse(sMediaID);
            }
            else
            {

                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache, ref theWSInfoStruct);

                nStartIndex = thePageDef.m_nStartIndex;
                nNumOfItems = thePageDef.m_nNumberOfItems;
                sPlayListSchema = ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true, ref thePlaylistSchema);
                if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null && HttpContext.Current.Session["ODBC_CACH_SEC"].ToString() == "0")
                    bWithCache = false;

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);
                nMediaID = nWSMediaID;
            }

            Int32 nType = 0;
            string sName = "";
            string sDescription = "";
            string sMediaStruct = "";

            GetMediaBasicData(nMediaID, ref nType, ref sName, ref sDescription, ref sMediaStruct,
                nLangID, bIsLangMain, ref theInfoStruct, ref theInfo);

            if (string.IsNullOrEmpty(sMediaTypes))
            {
                sMediaTypes = nType.ToString();
            }

            string sDeviceID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "device_udid");
            int[] deviceRules = GetDeviceAllowedRuleIDs(sDeviceID, nGroupID).ToArray();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sMediaStruct);
            XmlNodeList theMetaList = doc.SelectNodes("/root/tags_meta/meta");
            XmlNodeList theTagsList = doc.SelectNodes("/root/tags_collections/tag");
            StringBuilder sRet = new StringBuilder();

            // call lucene search
            string sInner = GetSearchMediaWithLucene(nStartIndex, nNumOfItems, nMediaID, nGroupID, sMediaTypes, sName, false, true, string.Empty, ref  theMetaList, ref  theTagsList,
                    "", ref  theDoc, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref  theInfoStruct, bIsAdmin,
                    bWithFileTypes, nCountryID, "", "", sDocStruct, ref  theWSInfoStruct, nDeviceID, bUseStartDate, deviceRules);

            sRet.Append("<response type=\"search_related\">");
            sRet.Append(sInner);
            sRet.Append("</response>");
            return sRet.ToString();

        }

        static public string RatingProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID,
            ref ApiObjects.InitializationObject initObj, ref ApiObjects.RateResponseObject theStat, Int32 nWSMediaID, Int32 nWSRateVal)
        {
            Int32 nRateVal = 0;
            Int32 nMediaID = 0;

            StringBuilder sRet = new StringBuilder();

            if (theDoc != null)
            {
                sRet.Append("<response type=\"rating\">");
                string sMediaID = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();

                string sRateVal = "";
                XmlNode theRateVal = theDoc.SelectSingleNode("/root/request/rating/@value");
                if (theRateVal != null)
                    sRateVal = theRateVal.Value.ToUpper();


                nRateVal = int.Parse(sRateVal);
                nMediaID = int.Parse(sMediaID);
            }
            else
            {
                nRateVal = nWSRateVal;
                nMediaID = nWSMediaID;
            }
            string sVotesCountField = "";
            if (nRateVal > 0 && nRateVal < 6)
                sVotesCountField = "VOTES_" + nRateVal.ToString() + "_COUNT";
            if (nRateVal < 1)
                sVotesCountField = "VOTES_LO_COUNT";

            if (nRateVal > 5)
                sVotesCountField = "VOTES_UP_COUNT";

            Int32 nVoted = TVinciShared.ProtocolsFuncs.UserVoteValueForMedia(nWatcherID, nMediaID, false, true, 0);
            if (nVoted == -1)
            {
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update media set ";
                directQuery += sVotesCountField + "=" + sVotesCountField + "+1";
                directQuery += ",VOTES_SUM=VOTES_SUM+" + nRateVal.ToString() + ",VOTES_COUNT=VOTES_COUNT+1 where";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;

                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_media_rating");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RATE_VAL", "=", nRateVal);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            else
            {
                if (theStat == null)
                    theStat = new ApiObjects.RateResponseObject();
                ApiObjects.GenericWriteResponse theResp = new ApiObjects.GenericWriteResponse();
                theResp.Initialize("User already voted", -1);
                theStat.Initialize(null, theResp);
                return GetErrorMessage("User allready voted");
            }

            Int32 nVotesSum = 0;
            Int32 nViews = 0;
            Int32 nVotesCnt = 0;
            double dAvg = 0.0;

            Int32 nVotesUpCnt = 0;
            Int32 nVotesLoCnt = 0;
            Int32 nVotes1Cnt = 0;
            Int32 nVotes2Cnt = 0;
            Int32 nVotes3Cnt = 0;
            Int32 nVotes4Cnt = 0;
            Int32 nVotes5Cnt = 0;

            TVinciShared.ProtocolsFuncs.GetCountersFromMedia(nMediaID, ref nViews, ref nVotesSum, ref nVotesCnt, ref dAvg
                , ref nVotesLoCnt
                , ref nVotesUpCnt
                , ref nVotes1Cnt
                , ref nVotes2Cnt
                , ref nVotes3Cnt
                , ref nVotes4Cnt
                , ref nVotes5Cnt, true);
            if (theDoc != null)
            {
                sRet.Append("<rating sum=\"").Append(nVotesSum).Append("\" count=\"").Append(nVotesCnt).Append("\" avg=\"").Append(dAvg).Append("\" user_voted=\"true\" user_voted_val=\"").Append(nRateVal.ToString()).Append("\" >");
                sRet.Append("<rate val=\"lo\" count=\"").Append(nVotesLoCnt).Append("\"/>");
                sRet.Append("<rate val=\"up\" count=\"").Append(nVotesUpCnt).Append("\"/>");
                sRet.Append("<rate val=\"1\" count=\"").Append(nVotes1Cnt).Append("\"/>");
                sRet.Append("<rate val=\"2\" count=\"").Append(nVotes2Cnt).Append("\"/>");
                sRet.Append("<rate val=\"3\" count=\"").Append(nVotes3Cnt).Append("\"/>");
                sRet.Append("<rate val=\"4\" count=\"").Append(nVotes4Cnt).Append("\"/>");
                sRet.Append("<rate val=\"5\" count=\"").Append(nVotes5Cnt).Append("\"/>");
                sRet.Append("</rating>");
                sRet.Append("</response>");
            }
            else
            {
                if (theStat == null)
                    theStat = new ApiObjects.RateResponseObject();
                ApiObjects.GenericWriteResponse theResp = new ApiObjects.GenericWriteResponse();
                ApiObjects.MediaStatistics theS = new ApiObjects.MediaStatistics();
                theResp.Initialize("OK", 0);
                theS.Initialize(nViews, nVotesSum, nVotesCnt, nVotesLoCnt, nVotesUpCnt,
                    nVotes1Cnt, nVotes2Cnt, nVotes3Cnt, nVotes4Cnt, nVotes5Cnt);
                theStat.Initialize(theS, theResp);
            }
            return sRet.ToString();
        }

        static protected Int32 GetGroupActionID(string sActionName, Int32 nGroupID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from groups_actions (nolock) where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ACTION_NAME)))", "=", sActionName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected void SendMail(Int32 nGroupID, string sEmail, string sName, string sSender, string sLink, string sFromMail, string sContent)
        {
            string sMailData = GetSendMailText(nGroupID, sEmail, sName, sSender, sLink, sFromMail, sContent.Replace("\r", "<br/>"));
            if (sFromMail == "")
            {
                object oFromMail = ODBCWrapper.Utils.GetTableSingleVal("groups", "mail_ret_add", nGroupID);
                if (oFromMail != null && oFromMail != DBNull.Value)
                    sFromMail = oFromMail.ToString();
            }
            Mailer t = new Mailer(nGroupID);
            object oSubject = ODBCWrapper.Utils.GetTableSingleVal("groups", "MAIL_SUBJECT", nGroupID);

            string sSubject = "";
            if (oSubject == null || oSubject == DBNull.Value || oSubject.ToString() == "")
                sSubject = "Clip from: " + ODBCWrapper.Utils.GetTableSingleVal("groups", "group_name", nGroupID).ToString();
            else
                sSubject = oSubject.ToString();
            t.SendMail(sEmail, "", sMailData, sSubject, sSender, sFromMail);
        }

        static protected string SendMailText(Int32 nGroupID, string sEmail, string sName, string sSender, string sLink, string sFromMail, string sContent)
        {
            string sMailData = GetSendMailText(nGroupID, sEmail, sName, sSender, sLink, sFromMail, sContent);
            sMailData = StripHTML(sMailData);
            return sMailData;
        }

        static protected string StripHTML(string source)
        {
            try
            {

                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating speces becuase browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result, 
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty, 
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything thats enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testng
                //System.Text.RegularExpressions.Regex.Replace(result, 
                //       this.txtRegex.Text,string.Empty, 
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4. 
                // Prepare first to remove any whitespaces inbetween
                // the escaped characters and remove redundant tabs inbetween linebreaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multible tabs followind a linebreak with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for linebreaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // Thats it.
                return result;

            }
            catch
            {
                return source;
            }
        }

        static protected string GetSendMailText(Int32 nGroupID, string sEmail, string sName, string sSender, string sLink, string sFromMail, string sContent)
        {
            object oTemplate = PageUtils.GetTableSingleVal("groups", "MAIL_TEMPLATE", nGroupID);
            string sTemplate = "";
            if (oTemplate != null)
                sTemplate = oTemplate.ToString();
            else
                return "";

            MailTemplateEngine mt = new MailTemplateEngine();
            string sFilePath = HttpContext.Current.Server.MapPath("");
            sFilePath += "/mailTemplates/" + sTemplate;
            mt.Init(sFilePath);
            string sBaseURL = "http://" + HttpContext.Current.Request.Url.Host.ToString();
            mt.Replace("EMAIL_ADD", sEmail);
            mt.Replace("BASE_URL", sBaseURL);
            mt.Replace("NAME", sName);
            mt.Replace("SENDER_NAME", sSender);
            mt.Replace("LINK", sLink);
            if (sContent == "")
                sContent = "-";
            mt.Replace("CONTENT", sContent);
            string sMailData = mt.GetAsString();

            return sMailData;
        }

        static public string GetGroupMailBaseAddr(Int32 nGroupID)
        {
            object oRet = ODBCWrapper.Utils.GetTableSingleVal("groups", "MAIL_BASE_ADDR", nGroupID);
            if (oRet == null || oRet == DBNull.Value)
                return "";
            string sRet = oRet.ToString();
            return sRet;
        }

        protected string GetGroupDomain(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups_passwords (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["DOMAIN"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string SentToFriendProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, ref ApiObjects.InitializationObject initObj,
            Int32 nWSMediaID, string sWSFromEmail, string sWSToEmail, string sWSRecieverName, string sWSSenderName, string sWSContent,
            ref ApiObjects.GenericWriteResponse theWSResponse)
        {
            string sFrom = "";
            string sTo = "";
            string sRecieverName = "";
            string sSenderName = "";
            string sContent = "";
            string sMediaLink = "";
            Int32 nMediaID = 0;
            if (initObj == null)
            {
                object oFrom = ODBCWrapper.Utils.GetTableSingleVal("groups", "MAIL_RET_ADD", nGroupID);
                if (oFrom != null && oFrom != DBNull.Value)
                    sFrom = oFrom.ToString();
                if (sFrom == "")
                {
                    XmlNode theFrom = theDoc.SelectSingleNode("/root/request/mail/@from");
                    if (theFrom != null)
                        sFrom = theFrom.Value;
                }

                XmlNode theTo = theDoc.SelectSingleNode("/root/request/mail/@to");
                if (theTo != null)
                    sTo = theTo.Value;

                XmlNode theMediaLink = theDoc.SelectSingleNode("/root/request/mail/@media_link");
                if (theMediaLink != null)
                    sMediaLink = theMediaLink.Value;

                XmlNode theRecieverName = theDoc.SelectSingleNode("/root/request/mail/@receiver_name");
                if (theRecieverName != null)
                    sRecieverName = theRecieverName.Value;

                XmlNode theSenderName = theDoc.SelectSingleNode("/root/request/mail/@sender_name");
                if (theSenderName != null)
                    sSenderName = theSenderName.Value;

                XmlNode theContent = theDoc.SelectSingleNode("/root/request/mail").FirstChild;
                if (theContent != null)
                    sContent = theContent.Value;

                string sMediaID = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();

                nMediaID = int.Parse(sMediaID);
            }
            else
            {
                sFrom = sWSFromEmail;
                sTo = sWSToEmail;
                sRecieverName = sWSRecieverName;
                sSenderName = sWSSenderName;
                sContent = sWSContent;
                nMediaID = nWSMediaID;
            }
            string sMediaName = "";
            string sMediaType = "";
            try
            {
                sMediaType = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_TYPE_ID", nMediaID).ToString();
            }
            catch { }
            string sLink = GetGroupMailBaseAddr(nGroupID);
            if (!string.IsNullOrEmpty(sMediaLink))
            {
                sLink = sMediaLink;
            }
            sLink = sLink.Replace("<!--media_id-->", nMediaID.ToString());
            sLink = sLink.Replace("<!--media_type-->", sMediaType);
            try
            {
                sMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID).ToString();
                sContent = sMediaName;
            }
            catch
            {
                sMediaName = sLink;
            }
            if (sLink != "" || !string.IsNullOrEmpty(sMediaLink))
            {
                if (!string.IsNullOrEmpty(sMediaLink))
                {
                    sLink = "<a href=\"" + sMediaLink + "\">" + sMediaName;
                    sLink += "</a>";
                }
                else
                {
                    sLink = "<a href=\"http://" + sLink;
                    if (sLink.IndexOf("?") == -1)
                        sLink += "?";
                    else
                        sLink += "&";

                    sLink += "media_id=" + nMediaID.ToString() + "\">" + sMediaName + "</a>";
                }
                SendMail(nGroupID, sTo, sRecieverName, sSenderName, sLink, sFrom, sContent);
            }

            StringBuilder sRet = new StringBuilder();
            if (theDoc != null)
            {
                sRet.Append("<response type=\"send_to_friend\">");
                sRet.Append("</response>");
            }
            else
            {
                if (theWSResponse == null)
                    theWSResponse = new ApiObjects.GenericWriteResponse();
                theWSResponse.Initialize("OK", 0);
            }
            return sRet.ToString();
        }

        static public string SentToFriendProtocolText(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID)
        {
            string sFrom = "";
            XmlNode theFrom = theDoc.SelectSingleNode("/root/request/mail/@from");
            if (theFrom != null)
                sFrom = theFrom.Value.ToUpper();

            string sTo = "";
            XmlNode theTo = theDoc.SelectSingleNode("/root/request/mail/@to");
            if (theTo != null)
                sTo = theTo.Value.ToUpper();

            string sRecieverName = "";
            XmlNode theRecieverName = theDoc.SelectSingleNode("/root/request/mail/@receiver_name");
            if (theRecieverName != null)
                sRecieverName = theRecieverName.Value.ToUpper();

            string sSenderName = "";
            XmlNode theSenderName = theDoc.SelectSingleNode("/root/request/mail/@sender_name");
            if (theSenderName != null)
                sSenderName = theSenderName.Value.ToUpper();

            string sContent = "";
            XmlNode theContent = theDoc.SelectSingleNode("/root/request/mail").FirstChild;
            if (theContent != null)
                sContent = theContent.Value.ToUpper();

            string sMediaID = "";
            XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
            if (theMediaID != null)
                sMediaID = theMediaID.Value.ToUpper();

            string sQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");
            string sType = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");

            string sLink = GetGroupMailBaseAddr(nGroupID);

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"send_to_friend_text\"><mail>");
            if (sLink != "")
            {
                sLink = "<a href=\"http://" + sLink + "?media_id=" + sMediaID + "&quality=" + sQuality + "&type=" + sType + "\">" + sLink + "</a>";
                sRet.Append(ProtocolsFuncs.XMLEncode(SendMailText(nGroupID, sTo, sRecieverName, sSenderName, sLink, sFrom, sContent), false));
            }
            sRet.Append("</mail></response>");
            return sRet.ToString();
        }

        static protected string GetMediaTypes(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            selectQuery += "select * from media_types (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

            selectQuery += "order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    sRet.Append("<type id=\"").Append(sID).Append("\" name=\"").Append(sName).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetMetaFields(Int32 nGroupID, ref ApiObjects.MediaInfoStructObject theInfoStruct)
        {
            if (theInfoStruct != null)
            {
                theInfoStruct.m_bTitle = true;
                theInfoStruct.m_bDescription = true;
            }
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 1; i < 21; i++)
                    {
                        string sFieldName = "META" + i.ToString() + "_STR_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                        {
                            string sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                            if (sName != "" && theInfoStruct == null)
                                sRet.Append("<").Append(sFieldName).Append(" name=\"").Append(sName).Append("\"/>");
                            if (sName != "" && theInfoStruct != null)
                            {
                                if (theInfoStruct.m_sMetaStrings == null)
                                    theInfoStruct.m_sMetaStrings = new string[1];
                                else
                                    theInfoStruct.m_sMetaStrings = (string[])(ResizeArray(theInfoStruct.m_sMetaStrings, theInfoStruct.m_sMetaStrings.Length + 1));
                                theInfoStruct.m_sMetaStrings[theInfoStruct.m_sMetaStrings.Length - 1] = sName;
                            }
                        }
                    }
                    for (int i = 1; i < 11; i++)
                    {
                        string sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                        {
                            string sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                            if (sName != "" && theInfoStruct != null)
                                sRet.Append("<").Append(sFieldName).Append(" name=\"").Append(sName).Append("\"/>");
                            if (sName != "" && theInfoStruct != null)
                            {
                                if (theInfoStruct.m_sMetaDoubles == null)
                                    theInfoStruct.m_sMetaDoubles = new string[1];
                                else
                                    theInfoStruct.m_sMetaDoubles = (string[])(ResizeArray(theInfoStruct.m_sMetaDoubles, theInfoStruct.m_sMetaDoubles.Length + 1));
                                theInfoStruct.m_sMetaDoubles[theInfoStruct.m_sMetaDoubles.Length - 1] = sName;
                            }
                        }
                    }
                    for (int i = 1; i < 11; i++)
                    {
                        string sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != DBNull.Value)
                        {
                            string sName = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                            if (sName != "" && theInfoStruct != null)
                                sRet.Append("<").Append(sFieldName).Append(" name=\"").Append(sName).Append("\"/>");
                            if (sName != "" && theInfoStruct != null)
                            {
                                if (theInfoStruct.m_sMetaBools == null)
                                    theInfoStruct.m_sMetaBools = new string[1];
                                else
                                    theInfoStruct.m_sMetaBools = (string[])(ResizeArray(theInfoStruct.m_sMetaBools, theInfoStruct.m_sMetaBools.Length + 1));
                                theInfoStruct.m_sMetaBools[theInfoStruct.m_sMetaBools.Length - 1] = sName;
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetTagTypes(Int32 nGroupID, ref ApiObjects.MediaInfoStructObject theInfoStruct)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_tags_types (nolock) where status=1 and ";
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            selectQuery += " GROUP_ID " + sGroups;
            selectQuery += " order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    if (theInfoStruct == null)
                        sRet.Append("<tag_type id=\"").Append(sID).Append("\" name=\"").Append(sName).Append("\"/>");
                    else
                    {
                        if (theInfoStruct.m_sTags == null)
                            theInfoStruct.m_sTags = new string[1];
                        else
                            theInfoStruct.m_sTags = (string[])(ResizeArray(theInfoStruct.m_sTags, theInfoStruct.m_sTags.Length + 1));
                        theInfoStruct.m_sTags[theInfoStruct.m_sTags.Length - 1] = sName;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string Get3ChoisesTypes(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_3choise_types (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    string sChoise1 = selectQuery.Table("query").DefaultView[i].Row["CHOISE1"].ToString();
                    string sChoise2 = selectQuery.Table("query").DefaultView[i].Row["CHOISE2"].ToString();
                    sRet.Append("<choise3 id=\"").Append(sID).Append("\" type=\"").Append(sName).Append("\" choise1=\"").Append(sChoise1).Append("\" choise2=\"").Append(sChoise2).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string MediaStructureProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, ref ApiObjects.MediaInfoStructObject theInfoStruct)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"media_structure\">");
            sRet.Append("<media_structure>");
            sRet.Append("<types>");
            sRet.Append(GetMediaTypes(nGroupID));
            sRet.Append("</types>");
            sRet.Append(GetMetaFields(nGroupID, ref theInfoStruct));
            sRet.Append("<tags_collections>");
            sRet.Append(GetTagTypes(nGroupID, ref theInfoStruct));
            sRet.Append("</tags_collections>");
            sRet.Append("</media_structure>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static protected string GetLangNameOfGroup(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ll.NAME from lu_languages ll (nolock), groups g (nolock) where g.language_id=ll.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["NAME"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected void GetSafeDBStr(object o, ref string sRet)
        {
            if (o == DBNull.Value)
                return;
            else if (o == null)
                return;
            else
                sRet = o.ToString();
        }

        static protected void GetSafeDBInt(object o, ref Int32 nRet)
        {
            if (o == DBNull.Value)
                return;
            else if (o == null)
                return;
            else
            {
                string sRet = o.ToString();
                nRet = int.Parse(sRet);
            }
        }

        static protected void GetSafeDBBool(object o, ref bool bRet)
        {
            if (o == DBNull.Value)
                return;
            else if (o == null)
                return;
            else
            {
                string sRet = o.ToString();
                if (sRet == "1")
                    bRet = true;
                else
                    bRet = false;
            }
        }

        static protected string GetTVCFilterItems(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mtt.id,tm.site_name,mtt.name from tvc_meta tm (nolock),media_tags_types mtt (nolock) where mtt.id=tm.media_tags_types_id and mtt.status=1 and tm.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tm.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sTagName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    string sTagID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = selectQuery.Table("query").DefaultView[i].Row["site_name"].ToString();
                    sRet.Append("<item title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\" tag_name=\"").Append(ProtocolsFuncs.XMLEncode(sTagName, true)).Append("\" id=\"").Append(sTagID).Append("\" />");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetTVCMediaTypes(Int32 nGroupID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mtt.id,mtt.name from media_types mtt (nolock) where mtt.status=1 and ";
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            selectQuery += " mtt.GROUP_ID " + sGroups;
            selectQuery += " order by mtt.order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sTagName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    string sTagID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    sRet.Append("<type title=\"").Append(ProtocolsFuncs.XMLEncode(sTagName, true)).Append("\" id=\"").Append(sTagID).Append("\" />");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string TVCProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bWithCache)
        {
            Int32 nCATALOG_PAGE_SIZE = 0;
            Int32 nCOMPANY_LOGO_ID = 0;
            Int32 nSPONSER_LOGO_ID = 0;
            Int32 nCOMMERCIAL_PIC_ID = 0;
            string sSPONSER_LINK = "javascript:void(0);";
            string sCOMP_LINK = "javascript:void(0);";

            string sCOMMERCIAL_LINK = "javascript:void(0);";
            bool bCOMMENTS_ENABLED = true;
            bool bWITH_WMP = true;
            bool bWITH_TYPE = true;
            bool bCOMMENTS_AUTO_APPROVE = true;

            Int32 nPLAYER_MENU_CATEGORY_ID = 0;
            Int32 nPLAYER_TREE_CATEGORY_ID = 0;
            Int32 nPLAYER_START_CHANNEL_ID = 0;
            Int32 nFOOTER_CATEGORY_ID = 0;
            Int32 nSIDE_CHANNEL_ID = 0;
            Int32 nSIDE_CHANNEL_SIZE = 0;
            Int32 nFOOTER_CHANNEL_MAX_ITEMS = 0;
            Int32 nFOOTER_LINE_ITEMS_CNT = 0;
            Int32 nAUTO_PLAY = 1;

            string sPLAYER_SKIN_FILE = "";
            string sMINI_PLAYER_SKIN_FILE = "";
            string sXML_CONFIG_URL = "";
            string sLANG_XML_CONFIG_URL = "";
            string sTRNS_XML_CONFIG_URL = "";
            string sCOPYWRITE_LINE = "";
            string sPAGE_TITLE = "";
            string sVIDEO_PAGE_HEADER = "";
            string sCATALOG_PAGE_HEADER = "";
            string sPAGE_KEY_WORDS = "";
            string sPAGE_DESCRIPTION = "";
            string sCOPYWRITE_LINK = "javascript:void(0);";
            string sFOOTER_TEXT1 = "";
            string sFOOTER_TEXT2 = "";
            string sFOOTER_TEXT3 = "";
            string sFOOTER_LINK1 = "javascript:void(0);";
            string sFOOTER_LINK2 = "javascript:void(0);";
            string sFOOTER_LINK3 = "javascript:void(0);";
            string sBASE_SITE_ADD = "javascript:void(0);";
            string sSTAT_SCRIPT = "";
            string sMETA_SCRIPT = "";
            string sSTOP_AD = "";
            string sBOTTOM_AD = "";
            string sRIGHT_AD = "";
            string sPLAYER_BG_COLOR = "";
            string sEXTRAN_INFO_CODE = "";
            string sFOOTER_SEO_LINE = "";
            string sLINK_HEADER = "";
            string sALL_HEADER = "";
            string sTYPE_HEADER = "";
            string sADVERTISMENT_HEADER = "";
            string sIFRAME_URL = "";
            string sFILE_FORMAT = "";
            Int32 nIFRAME_WIDTH = 0;
            Int32 nIFRAME_HEIGHT = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from tvc (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["COMPANY_LOGO_ID"], ref nCOMPANY_LOGO_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["SPONSER_LOGO_ID"], ref nSPONSER_LOGO_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_PIC_ID"], ref nCOMMERCIAL_PIC_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["CATALOG_PAGE_SIZE"], ref nCATALOG_PAGE_SIZE);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["SIDE_CHANNEL_ID"], ref nSIDE_CHANNEL_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["SIDE_CHANNEL_SIZE"], ref nSIDE_CHANNEL_SIZE);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["FOOTER_CHANNEL_MAX_ITEMS"], ref nFOOTER_CHANNEL_MAX_ITEMS);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["FOOTER_LINE_ITEMS_CNT"], ref nFOOTER_LINE_ITEMS_CNT);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["AUTO_PLAY"], ref nAUTO_PLAY);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["COPYWRITE_LINE"], ref sCOPYWRITE_LINE);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["COPYWRITE_LINK"], ref sCOPYWRITE_LINK);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PAGE_TITLE"], ref sPAGE_TITLE);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PAGE_KEY_WORDS"], ref sPAGE_KEY_WORDS);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PAGE_DESCRIPTION"], ref sPAGE_DESCRIPTION);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["IFRAME_URL"], ref sIFRAME_URL);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FILE_FORMAT"], ref sFILE_FORMAT);



                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_TEXT1"], ref sFOOTER_TEXT1);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_TEXT2"], ref sFOOTER_TEXT2);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_TEXT3"], ref sFOOTER_TEXT3);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_LINK1"], ref sFOOTER_LINK1);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_LINK2"], ref sFOOTER_LINK2);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_LINK3"], ref sFOOTER_LINK3);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["SITE_BASE_ADD"], ref sBASE_SITE_ADD);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["STAT_SCRIPT"], ref sSTAT_SCRIPT);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["META_SCRIPT"], ref sMETA_SCRIPT);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["TOP_AD_SCRIPT"], ref sSTOP_AD);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["BOTTOM_AD_SCRIPT"], ref sBOTTOM_AD);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["RIGHT_AD_SCRIPT"], ref sRIGHT_AD);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PLAYER_BG_COLOR"], ref sPLAYER_BG_COLOR);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["EXTRAN_INFO_CODE"], ref sEXTRAN_INFO_CODE);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["SPONSER_LINK"], ref sSPONSER_LINK);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["COMP_LINK"], ref sCOMP_LINK);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["COMMERCIAL_LINK"], ref sCOMMERCIAL_LINK);
                    GetSafeDBBool(selectQuery.Table("query").DefaultView[0].Row["COMMENTS_ENABLED"], ref bCOMMENTS_ENABLED);
                    GetSafeDBBool(selectQuery.Table("query").DefaultView[0].Row["WITH_WMP"], ref bWITH_WMP);
                    GetSafeDBBool(selectQuery.Table("query").DefaultView[0].Row["WITH_TYPE"], ref bWITH_TYPE);


                    GetSafeDBBool(selectQuery.Table("query").DefaultView[0].Row["COMMENTS_AUTO_APPROVE"], ref bCOMMENTS_AUTO_APPROVE);

                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["PLAYER_MENU_CATEGORY_ID"], ref nPLAYER_MENU_CATEGORY_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["PLAYER_TREE_CATEGORY_ID"], ref nPLAYER_TREE_CATEGORY_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["PLAYER_START_CHANNEL_ID"], ref nPLAYER_START_CHANNEL_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["FOOTER_CATEGORY_ID"], ref nFOOTER_CATEGORY_ID);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["IFRAME_WIDTH"], ref nIFRAME_WIDTH);
                    GetSafeDBInt(selectQuery.Table("query").DefaultView[0].Row["IFRAME_HEIGHT"], ref nIFRAME_HEIGHT);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PLAYER_SKIN_FILE"], ref sPLAYER_SKIN_FILE);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["MINI_PLAYER_SKIN_FILE"], ref sMINI_PLAYER_SKIN_FILE);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PLAYER_CONFIG_FILE"], ref sXML_CONFIG_URL);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["PLAYER_LANG_CONFIG_FILE"], ref sLANG_XML_CONFIG_URL);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["TRANSLATION_CONFIG_FILE"], ref sTRNS_XML_CONFIG_URL);


                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["VIDEO_PAGE_HEADER"], ref sVIDEO_PAGE_HEADER);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["CATALOG_PAGE_HEADER"], ref sCATALOG_PAGE_HEADER);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["FOOTER_SEO_LINE"], ref sFOOTER_SEO_LINE);

                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["LINK_HEADER"], ref sLINK_HEADER);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["ALL_HEADER"], ref sALL_HEADER);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["TYPE_HEADER"], ref sTYPE_HEADER);
                    GetSafeDBStr(selectQuery.Table("query").DefaultView[0].Row["ADVERTISMENT_HEADER"], ref sADVERTISMENT_HEADER);

                }
            }
            selectQuery.Finish();
            selectQuery = null;
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"tvc\" >");
            string sLangOrig = GetLangNameOfGroup(nGroupID);
            sRet.Append("<definitions lang=\"").Append(ProtocolsFuncs.XMLEncode(sLangOrig, true)).Append("\">");
            sRet.Append("<general comments_enabled=\"");
            if (bCOMMENTS_ENABLED == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\" ");
            sRet.Append(" comments_auto_approve=\"");
            if (bCOMMENTS_AUTO_APPROVE == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\" ");
            sRet.Append("page_title=\"").Append(ProtocolsFuncs.XMLEncode(sPAGE_TITLE, true)).Append("\" page_key_words=\"").Append(ProtocolsFuncs.XMLEncode(sPAGE_KEY_WORDS, true)).Append("\" base_site_add=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sBASE_SITE_ADD), true)).Append("\" page_description=\"").Append(ProtocolsFuncs.XMLEncode(sPAGE_DESCRIPTION, true)).Append("\" >");
            sRet.Append("<comments enabled=\"");
            if (bCOMMENTS_ENABLED == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\" ");
            sRet.Append(" auto_show=\"");
            if (bCOMMENTS_AUTO_APPROVE == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\" />");
            sRet.Append("<headers video_page_header=\"").Append(ProtocolsFuncs.XMLEncode(sVIDEO_PAGE_HEADER, true)).Append("\" catalog_page_header=\"").Append(ProtocolsFuncs.XMLEncode(sCATALOG_PAGE_HEADER, true)).Append("\" >");
            sRet.Append("</headers>");
            sRet.Append("<layout player_bg_color=\"").Append(sPLAYER_BG_COLOR).Append("\"/>");
            sRet.Append("<header_html>");
            sRet.Append(sMETA_SCRIPT);
            sRet.Append("</header_html>");
            sRet.Append("<player ");
            sRet.Append(" with_wmp=\"");
            if (bWITH_WMP == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\" />");
            sRet.Append("</general>");
            sRet.Append("<translation>");
            sRet.Append("<words link=\"").Append(ProtocolsFuncs.XMLEncode(sLINK_HEADER, true)).Append("\" type=\"").Append(ProtocolsFuncs.XMLEncode(sTYPE_HEADER, true)).Append("\" advertisement=\"").Append(ProtocolsFuncs.XMLEncode(sADVERTISMENT_HEADER, true)).Append("\" all=\"").Append(ProtocolsFuncs.XMLEncode(sALL_HEADER, true)).Append("\" />");
            sRet.Append("</translation>");
            sRet.Append("<footer copyright=\"").Append(ProtocolsFuncs.XMLEncode(sCOPYWRITE_LINE, true)).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(sCOPYWRITE_LINK, true)).Append("\" category_id=\"").Append(nFOOTER_CATEGORY_ID).Append("\" channel_line_items=\"").Append(nFOOTER_LINE_ITEMS_CNT).Append("\" max_channel_line_items=\"").Append(nFOOTER_CHANNEL_MAX_ITEMS).Append("\" >");
            sRet.Append("<seo cotext=\"").Append(ProtocolsFuncs.XMLEncode(sFOOTER_SEO_LINE, true)).Append("\" />");
            if (sFOOTER_TEXT1 != "")
                sRet.Append("<powered_item name=\"").Append(ProtocolsFuncs.XMLEncode(sFOOTER_TEXT1, true)).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sFOOTER_LINK1), true)).Append("\" />");
            if (sFOOTER_TEXT2 != "")
                sRet.Append("<powered_item name=\"").Append(ProtocolsFuncs.XMLEncode(sFOOTER_TEXT2, true)).Append("\" url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sFOOTER_LINK2), true)).Append("\" />");
            if (sFOOTER_TEXT3 != "")
                sRet.Append("<powered_item name=\"").Append(ProtocolsFuncs.XMLEncode(sFOOTER_TEXT3, true)).Append("\" URL=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sFOOTER_LINK3), true)).Append("\" />");
            sRet.Append("</footer>");
            sRet.Append("<statistics>");
            sRet.Append(sSTAT_SCRIPT);
            sRet.Append("</statistics>");

            sRet.Append("<advertise>");
            sRet.Append("<header>");
            sRet.Append("<html_code>").Append(sSTOP_AD).Append("</html_code>");
            sRet.Append("</header>");
            sRet.Append("<side>");
            sRet.Append("<image link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sCOMMERCIAL_LINK), true)).Append("\" image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nCOMMERCIAL_PIC_ID, "full"), true)).Append("\" />");
            sRet.Append("<html_code>").Append(sRIGHT_AD).Append("</html_code>");
            sRet.Append("</side>");
            sRet.Append("<buttom>");
            sRet.Append("<html_code>").Append(sBOTTOM_AD).Append("</html_code>");
            sRet.Append("</buttom>");
            sRet.Append("</advertise>");

            sRet.Append("<main_page>");
            sRet.Append("<info extran_info_code=\"").Append(ProtocolsFuncs.XMLEncode(sEXTRAN_INFO_CODE, true)).Append("\" />");
            sRet.Append("<player auto_play=\"").Append(nAUTO_PLAY).Append("\" file_format=\"").Append(sFILE_FORMAT).Append("\" ");
            sRet.Append("xml_config_url=\"").Append(ProtocolsFuncs.XMLEncode("http://admin.tvinci.com/skins/" + sXML_CONFIG_URL, true)).Append("\" ");
            sRet.Append("language_xml_config_url=\"").Append(ProtocolsFuncs.XMLEncode("http://admin.tvinci.com/skins/" + sLANG_XML_CONFIG_URL, true)).Append("\" ");
            sRet.Append("translation_config_url=\"").Append(ProtocolsFuncs.XMLEncode("http://admin.tvinci.com/skins/" + sTRNS_XML_CONFIG_URL, true)).Append("\" ");

            sRet.Append("skin=\"").Append(ProtocolsFuncs.XMLEncode("http://admin.tvinci.com/skins/" + sPLAYER_SKIN_FILE, true)).Append("\" ");
            sRet.Append("mini_skin=\"").Append(ProtocolsFuncs.XMLEncode("http://admin.tvinci.com/skins/" + sMINI_PLAYER_SKIN_FILE, true)).Append("\" ");
            sRet.Append("menu_category_id=\"").Append(nPLAYER_MENU_CATEGORY_ID).Append("\" tree_category_id=\"").Append(nPLAYER_TREE_CATEGORY_ID).Append("\" start_channel_id=\"").Append(nPLAYER_START_CHANNEL_ID).Append("\"/>");
            sRet.Append("<channel_flooding channel_id=\"").Append(nSIDE_CHANNEL_ID).Append("\" max_items=\"5\" />");
            sRet.Append("</main_page>");
            //this should go down when all version are updated
            sRet.Append("<company_logo image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nCOMPANY_LOGO_ID, "full"), true)).Append("\" link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sCOMP_LINK), true)).Append("\" />");
            sRet.Append("<sponser_logo  image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nSPONSER_LOGO_ID, "full"), true)).Append("\" link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sSPONSER_LINK), true)).Append("\" />");
            //--------
            sRet.Append("<header>");
            sRet.Append("<iframe url=\"").Append(sIFRAME_URL).Append("\" height=\"").Append(nIFRAME_HEIGHT).Append("\" width=\"").Append(nIFRAME_WIDTH).Append("\" />");
            sRet.Append("<company_logo image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nCOMPANY_LOGO_ID, "full"), true)).Append("\" link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sCOMP_LINK), true)).Append("\" />");
            sRet.Append("<sponser_logo  image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nSPONSER_LOGO_ID, "full"), true)).Append("\" link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sSPONSER_LINK), true)).Append("\" />");
            sRet.Append("</header>");
            sRet.Append("<commercial image_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nCOMMERCIAL_PIC_ID, "full"), true)).Append("\" link_url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sCOMMERCIAL_LINK), true)).Append("\" />");
            sRet.Append("<catalog_page page_size=\"").Append(nCATALOG_PAGE_SIZE).Append("\" >");

            sRet.Append("<filter_by ");
            sRet.Append(" with_type=\"");
            if (bWITH_TYPE == true)
                sRet.Append("true");
            else
                sRet.Append("false");
            sRet.Append("\">");
            sRet.Append(GetTVCFilterItems(nGroupID));
            sRet.Append("</filter_by>");
            sRet.Append("<media>");
            sRet.Append(GetTVCMediaTypes(nGroupID));
            sRet.Append("</media>");
            sRet.Append("</catalog_page>");

            sRet.Append("</definitions>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string MediaInfoProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bWithCache, bool bIsAdmin, ref ApiObjects.PicObject[] thePics,
            ref ApiObjects.MediaInfoObject theInfo, ref ApiObjects.MediaPersonalStatistics thePersonalStatistics,
            ref ApiObjects.MediaStatistics theMediaStatistics, Int32 nMediaID)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            if (theInfo == null)
            {
                StringBuilder sRet = new StringBuilder();
                sRet.Append("<response type=\"media_info\" >");
                string sMediaID = "";
                XmlNodeList theMeidas = theDoc.SelectNodes("/root/request/media");
                Int32 nCount = theMeidas.Count;
                for (int i = 0; i < nCount; i++)
                {
                    XmlNode theMediaID = theMeidas[i].SelectSingleNode("@id");
                    if (theMediaID != null)
                        sMediaID = theMediaID.Value.ToUpper();
                    Int32 nPicID = 0;
                    object oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", int.Parse(sMediaID));
                    if (oPic != null && oPic != DBNull.Value)
                        nPicID = int.Parse(oPic.ToString());
                    sRet.Append("<media_info media_id=\"").Append(sMediaID).Append("\" ");
                    string sPicStr = "";
                    sPicStr = ProtocolsFuncs.GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, int.Parse(sMediaID), bIsAdmin, bWithCache, ref thePics, sPicSizeForCache);
                    sRet.Append(sPicStr);
                    sRet.Append(">");
                    //XmlNode tNode = null;
                    XmlNode theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache);
                    bool bStatistics = false;
                    bool bPersonal = false;
                    string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                    if (sStatistics == "true")
                        bStatistics = true;
                    string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                    if (sPersonal == "true")
                        bPersonal = true;
                    sRet.Append(TVinciShared.ProtocolsFuncs.GetMediaInfoInner(int.Parse(sMediaID), nLangID, bIsLangMain, nWatcherID, bWithCache, ref theInfoStruct, true, bStatistics, bPersonal, ref theInfo,
                        ref thePersonalStatistics, ref theMediaStatistics));
                    sRet.Append("</media_info >");
                }
                sRet.Append("</response>");
                return sRet.ToString();
            }
            else
            {
                XmlNode theInfoStruct = null;
                TVinciShared.ProtocolsFuncs.GetMediaInfoInner(nMediaID, nLangID, bIsLangMain, nWatcherID, bWithCache, ref theInfoStruct, true, false, false, ref theInfo,
                    ref thePersonalStatistics, ref theMediaStatistics);
                return "";
            }
        }

        static protected void GetEPGChannelScheduleTranslation(Int32 nEPGChannelScheduleID, Int32 nLangID, ref string sName, ref string sDesc)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select name,description from epg_channels_schedule_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNELS_SCHEDULE_ID", "=", nEPGChannelScheduleID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    sDesc = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void GetEPGChannelTranslation(Int32 nEPGChannelID, Int32 nLangID, ref string sName, ref string sDesc)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select name,description from epg_channels_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_channel_id", "=", nEPGChannelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    sDesc = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected string GetChannelTranslation(Int32 nChannelID, Int32 nLangID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select name from channel_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", nChannelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["name"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetChannelDescTranslation(Int32 nChannelID, Int32 nLangID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select description from channel_translate (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", nChannelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["description"]);
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetChannelOpenXMLObject(ref XmlDocument theDoc, Int32 nChannelID, Int32 nLangID, bool bIsLangMain, Int32 nGroupID, ref DataTable d, ref XmlNode theOrderBy, string sPicsForCache,
            ref ApiObjects.PicObject[] thePics, Int32 nCountryID, Int32 nDeviceID, bool bUseStartDate, int[] deviceRules)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description,ch.IS_RSS from channels ch (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ch.id", "=", nChannelID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = "";
                    string sDescription = "";
                    if (bIsLangMain == true)
                        sTitle += selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    else
                        sTitle = GetChannelTranslation(int.Parse(sID), nLangID);

                    if (bIsLangMain == true)
                        sDescription += selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                    else
                        sDescription = GetChannelDescTranslation(int.Parse(sID), nLangID);

                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());

                    Int32 nIsRss = 0;
                    object oIsRss = selectQuery.Table("query").DefaultView[i].Row["IS_RSS"];
                    if (oIsRss != DBNull.Value && oIsRss != null)
                        nIsRss = int.Parse(oIsRss.ToString());

                    // Get channel medias with lucene Search 
                    Channel c = new Channel(int.Parse(sID), nLangID, nGroupID, deviceRules);
                    d = c.GetChannelMediaDT();

                    Int32 nC = 0;
                    if (d != null)
                        nC = d.DefaultView.Count;
                    sRet.Append("<channel id=\"").Append(sID).Append("\" ");
                    sRet.Append(" rss=\"");
                    if (nIsRss == 0)
                        sRet.Append("false");
                    else
                        sRet.Append("true");
                    sRet.Append("\" ");
                    string sLinear = "";
                    DateTime dLinear = c.GetLinearDateTime();
                    if (dLinear.Hour < 10)
                        sLinear += "0";
                    sLinear += dLinear.Hour.ToString() + ":";
                    if (dLinear.Minute < 10)
                        sLinear += "0";
                    sLinear += dLinear.Minute.ToString();
                    sRet.Append(" linear_start_time=\"").Append(sLinear).Append("\" ").Append(" description=\"").Append(ProtocolsFuncs.XMLEncode(sDescription, true)).Append("\"  ");
                    sRet.Append(GetPicSizesXMLPartsForChannel(ref theDoc, nPicID, nGroupID, nChannelID, false, true, ref thePics, sPicsForCache));
                    sRet.Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  media_count=\"").Append(nC).Append("\">");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetChannelOpenXMLObject(ref XmlDocument theDoc, Int32 nChannelID, Int32 nLangID, bool bIsLangMain, Int32 nGroupID, ref DataTable d, string sOrderBy, string sOrderByAdd, string sPicsForCache, ref ApiObjects.PicObject[] thePics,
            ref string sTitle, ref string sDescription, ref string sEditorRemarks, Int32 nCountryID, Int32 nDeviceID)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description,ch.EDITOR_REMARKS from channels ch (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ch.id", "=", nChannelID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    //string sTitle = "";
                    if (bIsLangMain == true)
                        sTitle += selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    else
                        sTitle = GetChannelTranslation(int.Parse(sID), nLangID);

                    if (bIsLangMain == true)
                        sDescription += selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                    else
                        sDescription = GetChannelDescTranslation(int.Parse(sID), nLangID);

                    sEditorRemarks = selectQuery.Table("query").DefaultView[i].Row["EDITOR_REMARKS"].ToString();

                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());
                    bool bWithCache = true;
                    if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null && HttpContext.Current.Session["ODBC_CACH_SEC"].ToString() == "0")
                        bWithCache = false;
                    Channel c = new Channel(int.Parse(sID), bWithCache, sOrderBy, sOrderByAdd, nGroupID, nLangID, bIsLangMain, nCountryID, nDeviceID);

                    d = c.GetChannelMediaDT();
                    Int32 nC = 0;
                    if (d != null)
                        nC = d.DefaultView.Count;
                    sRet.Append("<channel id=\"").Append(sID).Append("\" ");
                    GetPicSizesXMLPartsForChannel(ref theDoc, nPicID, nGroupID, nChannelID, false, true, ref thePics, sPicsForCache);
                    string sLinear = "";
                    DateTime dLinear = c.GetLinearDateTime();
                    if (dLinear.Hour < 10)
                        sLinear += "0";
                    sLinear += dLinear.Hour.ToString() + ":";
                    if (dLinear.Minute < 10)
                        sLinear += "0";
                    sLinear += dLinear.Minute.ToString();


                    sRet.Append(" linear_start_time=\"").Append(sLinear).Append("\" ").Append(" description=\"").Append(ProtocolsFuncs.XMLEncode(sDescription, true)).Append("\" ");
                    sRet.Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  media_count=\"").Append(nC).Append("\">");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetChannelsForCategory(ref XmlDocument theDoc, Int32 nCategoryID, Int32 nGroupID,
            Int32 nLangID, bool bIsLangMain, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics,
            ref ApiObjects.ChannelObject[] theChannels, Int32 nCountryID, Int32 nDeviceID)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (nCategoryID != 0)
            {
                selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description,ch.EDITOR_REMARKS from categories_channels cc (nolock),categories c (nolock),channels ch (nolock) where cc.status=1 and ch.status=1 and ch.is_active=1 and c.status=1 and c.is_active=1 and cc.category_id=c.id and cc.channel_id=ch.id and ch.WATCHER_ID=0 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.id", "=", nCategoryID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
                selectQuery += " order by cc.ORDER_NUM";
            }
            else
            {
                selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description,ch.EDITOR_REMARKS from channels ch (nolock) where ch.status=1 and ch.is_active=1 and ch.WATCHER_ID=0 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ch.group_id", "=", nGroupID);
                selectQuery += "and ch.id not in(select cc.channel_id from categories_channels cc (nolock),categories c (nolock) where cc.category_id=c.id and cc.status=1 and c.status=1 and c.is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cc.group_id", "=", nGroupID);
                selectQuery += ")";

                selectQuery += " order by ch.ORDER_NUM";
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = "";
                    string sDescription = "";
                    string sEditorRemarks = "";
                    if (bIsLangMain == true)
                        sTitle += selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    else
                        sTitle = GetChannelTranslation(int.Parse(sID), nLangID);

                    if (bIsLangMain == true)
                        sDescription += selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                    else
                        sDescription = GetChannelDescTranslation(int.Parse(sID), nLangID);

                    sEditorRemarks = selectQuery.Table("query").DefaultView[i].Row["EDITOR_REMARKS"].ToString();
                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());

                    string sPicsStr = "";
                    //if (nPicID != 0)
                    sPicsStr = GetPicSizesXMLPartsForChannel(ref theDoc, nPicID, nGroupID, int.Parse(sID), bIsAdmin, bWithCache, ref thePics, sPicSizeForCache);

                    Channel c = new Channel(int.Parse(sID), bWithCache, nLangID, bIsLangMain, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);
                    //Int32 nC = c.GetChannelMediaDT().DefaultView.Count;
                    Int32 nC = 0;
                    if (sTitle != "")
                    {
                        if (theDoc != null)
                        {
                            sRet.Append("<channel id=\"").Append(sID).Append("\" ").Append(sPicsStr).Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  media_count=\"").Append(nC).Append("\" ").Append(" description=\"").Append(ProtocolsFuncs.XMLEncode(sDescription, true)).Append("\"  ");
                            string sLinear = "";
                            DateTime dLinear = c.GetLinearDateTime();
                            if (dLinear.Hour < 10)
                                sLinear += "0";
                            sLinear += dLinear.Hour.ToString() + ":";
                            if (dLinear.Minute < 10)
                                sLinear += "0";
                            sLinear += dLinear.Minute.ToString();
                            sRet.Append(" linear_start_time=\"").Append(sLinear).Append("\" ");
                            sRet.Append("/>");

                        }
                        else
                        {
                            if (theChannels == null)
                                theChannels = new ApiObjects.ChannelObject[nCount];
                            if (theChannels[i] == null)
                                theChannels[i] = new ApiObjects.ChannelObject();
                            theChannels[i].m_nChannelTotalSize = nC;
                            theChannels[i].m_oPicObjects = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                            theChannels[i].m_oMediaObjects = null;
                            theChannels[i].m_sTitle = sTitle;
                            theChannels[i].m_sDescription = sDescription;
                            theChannels[i].m_sEditorRemarks = sEditorRemarks;
                            theChannels[i].m_nID = int.Parse(sID);
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string GetRSSChannels(ref XmlDocument theDoc, Int32 nGroupID,
            Int32 nLangID, bool bIsLangMain, bool bIsAdmin, bool bWithCache, ref ApiObjects.PicObject[] thePics,
            ref ApiObjects.ChannelObject[] theChannels, Int32 nCountryID, Int32 nDeviceID, Int32 nStartIndex, Int32 nNumOfItems)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description,ch.EDITOR_REMARKS from channels ch (nolock) where ch.status=1 and ch.is_active=1 and ch.WATCHER_ID=0 and ch.is_rss=1 and ";
            selectQuery += "ch.group_id " + PageUtils.GetFullChildGroupsStr(nGroupID, string.Empty);
            selectQuery += " order by ch.ORDER_NUM";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                sRet.Append("<channels count=\"" + nCount.ToString() + "\">");
                Int32 nPageSize = nNumOfItems;
                if (nCount - nStartIndex < nPageSize)
                    nPageSize = nCount - nStartIndex;
                for (int i = nStartIndex; i < nStartIndex + nPageSize; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = "";
                    string sDescription = "";
                    string sEditorRemarks = "";
                    if (bIsLangMain == true)
                        sTitle += selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    else
                        sTitle = GetChannelTranslation(int.Parse(sID), nLangID);

                    if (bIsLangMain == true)
                        sDescription += selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                    else
                        sDescription = GetChannelDescTranslation(int.Parse(sID), nLangID);

                    sEditorRemarks = selectQuery.Table("query").DefaultView[i].Row["EDITOR_REMARKS"].ToString();
                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());

                    string sPicsStr = "";
                    //if (nPicID != 0)
                    sPicsStr = GetPicSizesXMLPartsForChannel(ref theDoc, nPicID, nGroupID, int.Parse(sID), bIsAdmin, bWithCache, ref thePics, sPicSizeForCache);

                    Channel c = new Channel(int.Parse(sID), bWithCache, nLangID, bIsLangMain, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);
                    Int32 nC = c.GetChannelMediaDT().DefaultView.Count;
                    if (sTitle != "")
                    {
                        if (theDoc != null)
                        {
                            sRet.Append("<channel id=\"").Append(sID).Append("\" ").Append(sPicsStr).Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  media_count=\"").Append(nC).Append("\" ").Append(" description=\"").Append(ProtocolsFuncs.XMLEncode(sDescription, true)).Append("\"  ");
                            string sLinear = "";
                            DateTime dLinear = c.GetLinearDateTime();
                            if (dLinear.Hour < 10)
                                sLinear += "0";
                            sLinear += dLinear.Hour.ToString() + ":";
                            if (dLinear.Minute < 10)
                                sLinear += "0";
                            sLinear += dLinear.Minute.ToString();
                            sRet.Append(" linear_start_time=\"").Append(sLinear).Append("\" ");
                            sRet.Append("/>");

                        }
                        else
                        {
                            if (theChannels == null)
                                theChannels = new ApiObjects.ChannelObject[nCount];
                            if (theChannels[i] == null)
                                theChannels[i] = new ApiObjects.ChannelObject();
                            theChannels[i].m_nChannelTotalSize = nC;
                            theChannels[i].m_oPicObjects = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                            theChannels[i].m_oMediaObjects = null;
                            theChannels[i].m_sTitle = sTitle;
                            theChannels[i].m_sDescription = sDescription;
                            theChannels[i].m_sEditorRemarks = sEditorRemarks;
                            theChannels[i].m_nID = int.Parse(sID);
                        }
                    }
                }
                sRet.Append("</channels>");
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string EPGChannelsScheduleProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID, ref ApiObjects.PicObject[] thePics, Int32 nDeviceID)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            bool bWithCache = true;
            if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null && HttpContext.Current.Session["ODBC_CACH_SEC"].ToString() == "0")
                bWithCache = false;

            string sTheSigDoc = ProtocolsFuncs.GetSig(ref theDoc, true);
            if (CachingManager.CachingManager.Exist(sTheSigDoc) == true && bWithCache == true)
                return CachingManager.CachingManager.GetCachedData(sTheSigDoc).ToString();

            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sChannelID = "";

            XmlNodeList theChannelsIDs = theDoc.SelectNodes("/root/request/channel");

            string sWithInfo = "";
            XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
            if (theWithInfo != null)
                sWithInfo = theWithInfo.Value.ToUpper();

            string sWithFileTypes = "";
            XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
            if (theWithFileTypes != null)
                sWithFileTypes = theWithFileTypes.Value.ToUpper();

            string sTimeZone = "0";
            XmlNode theTimeZone = theDoc.SelectSingleNode("/root/request/params/@time_zone");
            if (theTimeZone != null)
                sTimeZone = theTimeZone.Value.ToUpper();

            if (sTimeZone.Trim().StartsWith("+") == true)
                sTimeZone = sTimeZone.Substring(1);
            Int32 nTimeZone = int.Parse(sTimeZone);
            string sStartDate = "";
            XmlNode theStartDate = theDoc.SelectSingleNode("/root/request/period/@start_date");
            if (theStartDate != null)
                sStartDate = theStartDate.Value.ToUpper();

            string sEndDate = "";
            XmlNode theEndDate = theDoc.SelectSingleNode("/root/request/period/@end_date");
            if (theEndDate != null)
                sEndDate = theEndDate.Value.ToUpper();

            XmlNode theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache);

            bool bWithInfo = false;
            if (sWithInfo.Trim().ToLower() == "true")
                bWithInfo = true;

            bool bWithFileTypes = false;
            if (sWithFileTypes.Trim().ToLower() == "true")
                bWithFileTypes = true;

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"epg_channel_schedule\">");
            if (theChannelsIDs != null)
            {
                Int32 nCount1 = theChannelsIDs.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    XmlNode theChannelID = theChannelsIDs[i].SelectSingleNode("@id");
                    sChannelID = theChannelID.Value.ToUpper();
                    string sChannelName = "";
                    object oChannelName = ODBCWrapper.Utils.GetTableSingleVal("epg_channels", "name", int.Parse(sChannelID));
                    if (oChannelName != null && oChannelName != DBNull.Value)
                        sChannelName = oChannelName.ToString();
                    sRet.Append("<channel id=\"").Append(sChannelID).Append("\" start_date=\"").Append(sStartDate).Append("\" end_date=\"").Append(sEndDate).Append("\" time_zone=\"").Append(sTimeZone).Append("\" name=\"").Append(ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("\">");
                    DateTime dStart = DateUtils.GetDateFromStr(sStartDate).AddHours(nTimeZone * -1);
                    DateTime dEnd = DateUtils.GetDateFromStr(sEndDate).AddHours(nTimeZone * -1);
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select q.*,m.id as media_id from(select ecs.epg_identifier,ecs.start_date,ecs.end_date,ecs.id,ecs.PIC_ID,ecs.NAME,ecs.description from epg_channels_schedule ecs (nolock) where ecs.status=1 and ecs.is_active=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ecs.group_id", "=", nGroupID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ecs.end_date", ">=", dStart);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ecs.start_date", "<=", dEnd);
                    selectQuery += ")q left join media m on m.epg_identifier=q.epg_identifier ";
                    selectQuery += " order by start_date";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        for (int j = 0; j < nCount; j++)
                        {
                            string sID = selectQuery.Table("query").DefaultView[j].Row["id"].ToString();

                            string sTitle = "";
                            string sDesc = "";
                            if (bIsLangMain == true)
                            {
                                sTitle += selectQuery.Table("query").DefaultView[j].Row["name"].ToString();
                                sDesc += selectQuery.Table("query").DefaultView[j].Row["description"].ToString();
                            }
                            else
                            {
                                GetEPGChannelScheduleTranslation(int.Parse(sID), nLangID, ref sTitle, ref sDesc);
                            }
                            Int32 nPicID = 0;
                            object oPic = selectQuery.Table("query").DefaultView[j].Row["pic_id"];
                            if (oPic != DBNull.Value && oPic != null)
                                nPicID = int.Parse(oPic.ToString());

                            Int32 nMediaID = 0;
                            object oMedia = selectQuery.Table("query").DefaultView[j].Row["media_id"];
                            if (oMedia != DBNull.Value && oMedia != null)
                                nMediaID = int.Parse(oMedia.ToString());

                            string sStart = "";
                            object oSD = selectQuery.Table("query").DefaultView[j].Row["start_date"];
                            if (oSD != DBNull.Value && oSD != null)
                                sStart = DateUtils.GetLongStrFromDate(((DateTime)(selectQuery.Table("query").DefaultView[j].Row["start_date"])).AddHours(nTimeZone));

                            string sEnd = "";
                            object oED = selectQuery.Table("query").DefaultView[j].Row["end_date"];
                            if (oED != DBNull.Value && oED != null)
                                sEnd = DateUtils.GetLongStrFromDate(((DateTime)(selectQuery.Table("query").DefaultView[j].Row["end_date"])).AddHours(nTimeZone));

                            string sPicsStr = "";
                            if (nPicID != 0)
                                sPicsStr = GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, bIsAdmin, bWithCache, ref thePics, sPicSizeForCache);



                            if (sTitle != "")
                            {
                                sRet.Append("<schedule id=\"").Append(sID).Append("\" ").Append(sPicsStr).Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  description=\"").Append(ProtocolsFuncs.XMLEncode(sDesc, true)).Append("\" start_date=\"").Append(sStart).Append("\" end_date=\"").Append(sEnd).Append("\">");
                                if (nMediaID != 0)
                                {
                                    sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                                }
                                sRet.Append("</schedule>");
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    sRet.Append("</channel>");
                    //sRet.Append(GetMediaTag(ref theDoc, int.Parse(sMediaID), "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin));
                }
            }
            sRet.Append("</response>");
            CachingManager.CachingManager.SetCachedData(sTheSigDoc, sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Default, 0, false);
            return sRet.ToString();
        }



        static public string EPGChannelsListListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID,
            bool bIsAdmin, ref ApiObjects.PicObject[] thePics)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            bool bWithCache = true;
            if (HttpContext.Current.Session["ODBC_CACH_SEC"] != null && HttpContext.Current.Session["ODBC_CACH_SEC"].ToString() == "0")
                bWithCache = false;
            string sTheSigDoc = ProtocolsFuncs.GetSig(ref theDoc, true);
            if (CachingManager.CachingManager.Exist(sTheSigDoc) == true && bWithCache == true)
                return CachingManager.CachingManager.GetCachedData(sTheSigDoc).ToString();

            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"epg_channels_list\">");

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ec.id,ec.PIC_ID,ec.NAME,ec.description from epg_channels ec (nolock) where ec.status=1 and ec.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ec.group_id", "=", nGroupID);
            selectQuery += " order by ec.ORDER_NUM";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = "";
                    string sDesc = "";
                    if (bIsLangMain == true)
                        sTitle += selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    else
                    {
                        GetEPGChannelTranslation(int.Parse(sID), nLangID, ref sTitle, ref sDesc);
                    }
                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());
                    string sPicsStr = "";
                    if (nPicID != 0)
                        sPicsStr = GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, bIsAdmin, bWithCache, ref thePics, sPicSizeForCache);

                    if (sTitle != "")
                        sRet.Append("<epg_channel id=\"").Append(sID).Append("\" ").Append(sPicsStr).Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  description=\"").Append(ProtocolsFuncs.XMLEncode(sDesc, true)).Append("\"/>");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            sRet.Append("</response>");
            CachingManager.CachingManager.SetCachedData(sTheSigDoc, sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.Default, 0, false);
            return sRet.ToString();
        }

        static public string ChannelsListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin,
            bool bWithCache, ref ApiObjects.InitializationObject initObj, ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            Int32 nWSCategoryID, ref ApiObjects.ChannelObject[] theChannels, Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            if (initObj == null)
            {
                string sCategoryID = "";
                XmlNode theCategoryID = theDoc.SelectSingleNode("/root/request/category/@id");
                if (theCategoryID != null)
                    sCategoryID = theCategoryID.Value.ToUpper();

                if (sCategoryID.Trim() == "")
                    sCategoryID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "category_id");
                ApiObjects.PicObject[] thePics = null;
                StringBuilder sRet = new StringBuilder();
                sRet.Append("<response type=\"channels_list\">");
                sRet.Append("<category id=\"").Append(sCategoryID).Append("\">");
                sRet.Append(GetChannelsForCategory(ref theDoc, int.Parse(sCategoryID), nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref thePics, ref theChannels, nCountryID, nDeviceID));
                sRet.Append("</category>");
                sRet.Append("</response>");
                return sRet.ToString();
            }
            else
            {
                GetChannelsForCategory(ref theDoc, nWSCategoryID, nGroupID, nLangID, bIsLangMain, bIsAdmin, bWithCache, ref initObj.m_oPicObjects, ref theChannels, nCountryID, nDeviceID);
                return "";
            }
        }

        static protected string WatcherChannelsListProtocolInner(ref XmlDocument theDoc, Int32 nGroupID, Int32 nWatcherID, bool bWithCacheC, bool bIsAdmin,
            ref ApiObjects.PicObject[] thePics, ref ApiObjects.ChannelObject[] theChannels, Int32 nLangID,
            bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID, bool bWritable)
        {
            string sPicSizeForCache = "";
            if (thePics == null)
                sPicSizeForCache = GetPicSizeForCache(ref theDoc);
            else
                sPicSizeForCache = GetPicSizeForCache(ref thePics);

            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bWithCacheC == false)
                selectQuery.SetCachedSec(0);
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += "select ch.id,ch.PIC_ID,ch.NAME,ch.description from channels ch (nolock) where ch.status=1 and ch.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ch.WATCHER_ID", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ch.group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sTitle = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    string sDescription = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                    Int32 nPicID = 0;
                    object oPic = selectQuery.Table("query").DefaultView[i].Row["pic_id"];
                    if (oPic != DBNull.Value && oPic != null)
                        nPicID = int.Parse(oPic.ToString());
                    string sPicsStr = "";
                    if (nPicID != 0)
                        sPicsStr = GetPicSizesXMLParts(ref theDoc, nPicID, nGroupID, bIsAdmin, bWithCacheC, ref thePics, sPicSizeForCache);
                    Channel c = new Channel(int.Parse(sID), bWithCacheC, nLangID, bIsLangMain, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);
                    Int32 nC = c.GetChannelMediaDT().DefaultView.Count;
                    if (theDoc != null)
                        sRet.Append("<channel id=\"").Append(sID).Append("\" ").Append(sPicsStr).Append(" title=\"").Append(ProtocolsFuncs.XMLEncode(sTitle, true)).Append("\"  media_count=\"").Append(nC).Append("\" ").Append(" description=\"").Append(ProtocolsFuncs.XMLEncode(sDescription, true)).Append("\"  />");
                    else
                    {
                        if (theChannels == null)
                            theChannels = new ApiObjects.ChannelObject[nCount];
                        if (theChannels[i] == null)
                            theChannels[i] = new ApiObjects.ChannelObject();
                        theChannels[i].m_nChannelTotalSize = nC;
                        theChannels[i].m_oPicObjects = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(thePics);
                        theChannels[i].m_oMediaObjects = null;
                        theChannels[i].m_sTitle = sTitle;
                        theChannels[i].m_sDescription = sDescription;
                        theChannels[i].m_nID = int.Parse(sID);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected Int32 GetWatcherIDBySiteGUID(string sSiteGUID, Int32 nGroupID, ref string sTvinciGuid)
        {
            Int32 nWatcherID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select wgd.WATCHER_ID,w.TVINCI_GUID from watchers_groups_data wgd (nolock),watchers w WITH (nolock) where wgd.WATCHER_ID=w.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_GUID", "=", sSiteGUID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nWatcherID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["WATCHER_ID"].ToString());
                    sTvinciGuid = selectQuery.Table("query").DefaultView[0].Row["TVINCI_GUID"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nWatcherID;
        }

        static public string WatcherChannelsListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID,
            bool bIsAdmin, ref ApiObjects.InitializationObject initObj, ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            ref ApiObjects.ChannelObject[] theChannels, string sLang, Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            if (initObj == null)
            {
                StringBuilder sRet = new StringBuilder();
                sRet.Append("<response type=\"watcher_channels_list\">");
                XmlNode theSiteGUID = theDoc.SelectSingleNode("/root/request/watcher/@site_guid");
                if (theSiteGUID != null)
                    sSiteGUID = theSiteGUID.Value.Trim();
                Int32 nNewWatcherID = nWatcherID;
                string sNewTvinciGUID = sTVinciGUID;
                if (sSiteGUID != "")
                {
                    nNewWatcherID = GetWatcherIDBySiteGUID(sSiteGUID, nGroupID, ref sNewTvinciGUID);
                }
                sRet.Append("<watcher id=\"").Append(nNewWatcherID).Append("\" guid=\"").Append(sNewTvinciGUID).Append("\" site_guid=\"").Append(sSiteGUID).Append("\">");
                ApiObjects.PicObject[] thePics = null;
                if (nNewWatcherID != 0)
                    sRet.Append(WatcherChannelsListProtocolInner(ref theDoc, nGroupID, nNewWatcherID, false, bIsAdmin,
                        ref thePics, ref theChannels, nLangID, bIsLangMain, nCountryID, nDeviceID, false));
                sRet.Append("</watcher>");
                sRet.Append("</response>");
                return sRet.ToString();
            }
            else
            {
                StringBuilder sRet = new StringBuilder();
                Int32 nNewWatcherID = nWatcherID;
                string sNewTvinciGUID = sTVinciGUID;
                if (sSiteGUID != "")
                    nNewWatcherID = GetWatcherIDBySiteGUID(sSiteGUID, nGroupID, ref sNewTvinciGUID);
                if (nNewWatcherID != 0)
                    sRet.Append(WatcherChannelsListProtocolInner(ref theDoc, nGroupID, nNewWatcherID, false, bIsAdmin,
                        ref initObj.m_oPicObjects, ref theChannels, nLangID, bIsLangMain, nCountryID, nDeviceID, false));
                return "";
            }
        }

        static protected string GetCommercialTag(ref XmlDocument theDoc, Int32 nCommercialID, string sFileFormat, string sFileQuality, string sCommercialType)
        {
            Int32 nCDNID = 0;
            string sURL = "";
            string sCDNImpl = "";
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<commercial ");
            sRet.Append(" id=\"").Append(nCommercialID).Append("\" ");

            Int32 nMediaFileID = 0;
            string sClickURL = "";
            string sNot = "";
            nMediaFileID = GetCommercialFileID(nCommercialID, sFileFormat, sFileQuality, sCommercialType, ref sClickURL);
            DataRecordMediaViewerField d = new DataRecordMediaViewerField("", nMediaFileID);
            d.VideoTable("commercial_files");
            d.GetCDNData(ref sCDNImpl, ref nCDNID, ref sNot);
            sURL = d.GetFLVSrc();

            sRet.Append(" cdn_id=\"").Append(nCDNID).Append("\" cdn_impl_type=\"").Append(sCDNImpl).Append("\" notify_url=\"").Append(sNot).Append("\"  url=\"").Append(sURL).Append("\" ");
            sRet.Append(GetCommercialSettingsParameters(nCommercialID, "sa"));
            sRet.Append(" />");
            sRet.Append("<commercial_click url=\"").Append(sClickURL).Append("\" />");
            return sRet.ToString();
        }

        static protected string GetCommercialSettingsParameters(Int32 nCommercialID, string sCommType)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append(" ");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from commercial (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCommercialID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nMainStop = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MAIN_STOP"].ToString());
                    Int32 nControls = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CONTROLS_ENABLE"].ToString());
                    Int32 nCloseButton = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CLOSE_BUTTON"].ToString());
                    Int32 nVisibleSec = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VISIBLE_TIME_SEC"].ToString());
                    string sText = selectQuery.Table("query").DefaultView[0].Row["TEXT_COMM"].ToString();
                    string sClickURL = selectQuery.Table("query").DefaultView[0].Row["CLICK_URL"].ToString();
                    sRet.Append(" main_stop=\"");
                    if (nMainStop == 0)
                        sRet.Append("false");
                    else
                        sRet.Append("true");
                    sRet.Append("\" controls=\"");
                    if (nControls == 0)
                        sRet.Append("disable");
                    else
                        sRet.Append("enable");
                    sRet.Append("\" close_button=\"");
                    if (nCloseButton == 0)
                        sRet.Append("false");
                    else
                        sRet.Append("true");
                    sRet.Append("\" click_url=\"");
                    sRet.Append(sClickURL).Append("\" ");
                    sRet.Append(" type=\"").Append(sCommType).Append("\" appear_sec=\"");
                    sRet.Append(nVisibleSec);
                    sRet.Append("\" comm_text=\"");
                    sRet.Append(ProtocolsFuncs.XMLEncode(sText, true));
                    sRet.Append("\" ");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetTextCommercialTag(ref XmlDocument theDoc, Int32 nCommercialID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<commercial ");
            sRet.Append(" id=\"").Append(nCommercialID).Append("\" ");
            sRet.Append(GetCommercialSettingsParameters(nCommercialID, "ro_text"));
            sRet.Append(" />");
            return sRet.ToString();
        }

        static protected Int32 GetCommercialFileID(Int32 nCommercialID, string sFileFormat, string sFileQuality, string sCommercialType, ref string sClickURL)
        {
            Int32 nQualityID = 0;
            if (sFileQuality.ToLower().Trim() == "low")
                nQualityID = 1;
            if (sFileQuality.ToLower().Trim() == "medium")
                nQualityID = 2;
            if (sFileQuality.ToLower().Trim() == "high")
                nQualityID = 3;

            Int32 nFormatID = 0;
            if (sFileFormat.ToLower().Trim() == "flv")
                nFormatID = 1;
            if (sFileFormat.ToLower().Trim() == "mp3")
                nFormatID = 2;
            if (sFileFormat.ToLower().Trim() == "wmv")
                nFormatID = 3;
            if (sFileFormat.ToLower().Trim() == "mpeg")
                nFormatID = 4;
            if (sFileFormat.ToLower().Trim() == "wav")
                nFormatID = 5;
            if (sFileFormat.ToLower().Trim() == "mp4")
                nFormatID = 7;

            Int32 nCommercialType = 0;
            if (sCommercialType.ToLower().Trim() == "stand alone")
                nCommercialType = 1;
            if (sCommercialType.ToLower().Trim() == "over lay")
                nCommercialType = 2;
            Int32 nRet = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.id,c.click_url from commercial_files mf (nolock),commercial c (nolock) where c.id=mf.commercial_ID and mf.STATUS=1 and mf.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.commercial_ID", "=", nCommercialID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.commercial_TYPE_ID", "=", nCommercialType);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sClickURL = selectQuery.Table("query").DefaultView[0].Row["click_url"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected bool CanUserWatchCommercial(Int32 nCommID, Int32 nWatcherID, Int32 nUNIQUE_TIME_DIFF, Int32 nMAX_UNIQUE_VIEWS, Int32 nMAX_UNIQUE_VIEWS_DAY)
        {
            Int32 nTotal = 0;
            Int32 nDay = 0;
            Int32 nOnPeriod = 0;
            if (nUNIQUE_TIME_DIFF > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery.SetLockTimeOut(10000);
                selectQuery += "select count(*) as co from watchers_media_actions WITH (nolock) where action_id=4 and create_date>DATEADD(minute, " + (nUNIQUE_TIME_DIFF * -1).ToString() + ", GETDATE()) and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nCommID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nOnPeriod = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nOnPeriod > 0)
                    return false;
            }
            if (nMAX_UNIQUE_VIEWS_DAY > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                // selectQuery.SetLockTimeOut(10000);
                selectQuery += "select count(*) as co from watchers_media_actions WITH (nolock) where action_id=4 and create_date>getdate()-1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nCommID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nDay = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nDay >= nMAX_UNIQUE_VIEWS_DAY)
                    return false;
            }
            if (nMAX_UNIQUE_VIEWS > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                // selectQuery.SetLockTimeOut(10000);
                selectQuery += "select count(*) as co from watchers_media_actions WITH (nolock) where action_id=4  and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nCommID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nTotal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nDay >= nMAX_UNIQUE_VIEWS)
                    return false;
            }
            return true;
        }

        static public string TvinciROTextCommercialProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, Int32 nCountryID)
        {
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            string sCommercialTag = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select q.* from (select distinct c.UNIQUE_TIME_DIFF,c.MAX_UNIQUE_VIEWS,c.MAX_UNIQUE_VIEWS_DAY,c.id,c.BLOCK_TEMPLATE_ID,c.PLAYERS_RULES from commercial c (nolock) ,campaigns cam (nolock) ,campaigns_commercials camc (nolock) where camc.CAMPAIGN_ID=cam.id and camc.COMMERCIAL_ID=c.id and camc.status=1 and cam.status=1 and cam.is_active=1 and c.is_active=1 and c.status=1 and c.start_date<getdate() and (c.end_date is null or c.end_date>getdate()) and (cam.max_views is null or cam.max_views=0 or cam.max_views>cam.views) and c.TEXT_COMM is not null and c.TEXT_COMM<>'' and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or c.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")q order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                string sPlayerUN = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_un");
                string sPlayerPass = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_pass");
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGeoBlockID = 0;
                    bool bIPAllowed = TVinciShared.ProtocolsFuncs.DoesCallerPermittedIP(nGroupID);
                    if (bIPAllowed == false)
                    {
                        object oBTID = selectQuery.Table("query").DefaultView[0].Row["BLOCK_TEMPLATE_ID"];
                        if (oBTID != DBNull.Value && oBTID != null)
                            nGeoBlockID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BLOCK_TEMPLATE_ID"].ToString());
                    }
                    bool bAllowed = true;
                    bool bExsitInRuleM2M = false;
                    if (nGeoBlockID != 0)
                    {
                        Int32 nONLY_OR_BUT = 0;
                        nONLY_OR_BUT = int.Parse(PageUtils.GetTableSingleVal("geo_block_types", "ONLY_OR_BUT", nGeoBlockID).ToString());
                        bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(nGeoBlockID, nCountryID);
                        //No one except
                        if (nONLY_OR_BUT == 0)
                            bAllowed = bExsitInRuleM2M;
                        //All except
                        if (nONLY_OR_BUT == 1)
                            bAllowed = !bExsitInRuleM2M;
                    }
                    if (bAllowed == true)
                    {
                        Int32 nPlayerRuleID = 0;
                        object oPR = selectQuery.Table("query").DefaultView[0].Row["PLAYERS_RULES"];
                        if (oPR != DBNull.Value && oPR != null)
                            nPlayerRuleID = int.Parse(oPR.ToString());
                        bAllowed = true;
                        if (nPlayerRuleID != 0)
                        {
                            Int32 nONLY_OR_BUT = 0;
                            nONLY_OR_BUT = int.Parse(PageUtils.GetTableSingleVal("players_groups_types", "ONLY_OR_BUT", nPlayerRuleID).ToString());
                            bExsitInRuleM2M = PageUtils.DoesPlayerRuleTypeIncludePlayer(nPlayerRuleID, nPlayerID);
                            //No one except
                            if (nONLY_OR_BUT == 0)
                                bAllowed = bExsitInRuleM2M;
                            //All except
                            if (nONLY_OR_BUT == 1)
                                bAllowed = !bExsitInRuleM2M;
                        }
                        if (bAllowed == true)
                        {
                            //Check all user restrictions
                            Int32 nCommID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                            Int32 nUNIQUE_TIME_DIFF = int.Parse(selectQuery.Table("query").DefaultView[i].Row["UNIQUE_TIME_DIFF"].ToString());
                            Int32 nMAX_UNIQUE_VIEWS = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_UNIQUE_VIEWS"].ToString());
                            Int32 nMAX_UNIQUE_VIEWS_DAY = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_UNIQUE_VIEWS_DAY"].ToString());
                            if (CanUserWatchCommercial(nCommID, nWatcherID, nUNIQUE_TIME_DIFF, nMAX_UNIQUE_VIEWS, nMAX_UNIQUE_VIEWS_DAY) == true)
                            {
                                sCommercialTag = GetTextCommercialTag(ref theDoc, nCommID);
                                // media_mark for comm
                                break;
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"tvinci_ro_text_comm\">");
            sRet.Append(sCommercialTag);
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string TvinciSACommercialProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, Int32 nCountryID)
        {
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
            string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");
            string sCommercialTag = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select q.* from (select distinct c.UNIQUE_TIME_DIFF,c.MAX_UNIQUE_VIEWS,c.MAX_UNIQUE_VIEWS_DAY,c.id,c.BLOCK_TEMPLATE_ID,c.PLAYERS_RULES from commercial c (nolock),campaigns cam (nolock) ,campaigns_commercials camc (nolock) where camc.CAMPAIGN_ID=cam.id and camc.COMMERCIAL_ID=c.id and camc.status=1 and cam.status=1 and cam.is_active=1 and c.is_active=1 and c.status=1 and c.start_date<getdate() and (c.end_date is null or c.end_date>getdate()) and (cam.max_views is null or cam.max_views=0 or cam.max_views>cam.views) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or c.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")q order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                string sPlayerUN = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_un");
                string sPlayerPass = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_pass");
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nGeoBlockID = 0;
                    bool bIPAllowed = TVinciShared.ProtocolsFuncs.DoesCallerPermittedIP(nGroupID);
                    if (bIPAllowed == false)
                        nGeoBlockID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["BLOCK_TEMPLATE_ID"].ToString());
                    bool bAllowed = true;
                    bool bExsitInRuleM2M = false;
                    if (nGeoBlockID != 0)
                    {
                        Int32 nONLY_OR_BUT = 0;
                        nONLY_OR_BUT = int.Parse(PageUtils.GetTableSingleVal("geo_block_types", "ONLY_OR_BUT", nGeoBlockID).ToString());
                        bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(nGeoBlockID, nCountryID);
                        //No one except
                        if (nONLY_OR_BUT == 0)
                            bAllowed = bExsitInRuleM2M;
                        //All except
                        if (nONLY_OR_BUT == 1)
                            bAllowed = !bExsitInRuleM2M;
                    }
                    if (bAllowed == true)
                    {
                        Int32 nPlayerRuleID = 0;
                        nPlayerRuleID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PLAYERS_RULES"].ToString());
                        bAllowed = true;
                        if (nPlayerRuleID != 0)
                        {
                            Int32 nONLY_OR_BUT = 0;
                            nONLY_OR_BUT = int.Parse(PageUtils.GetTableSingleVal("players_groups_types", "ONLY_OR_BUT", nPlayerRuleID).ToString());
                            bExsitInRuleM2M = PageUtils.DoesPlayerRuleTypeIncludePlayer(nPlayerRuleID, nPlayerID);
                            //No one except
                            if (nONLY_OR_BUT == 0)
                                bAllowed = bExsitInRuleM2M;
                            //All except
                            if (nONLY_OR_BUT == 1)
                                bAllowed = !bExsitInRuleM2M;
                        }
                        if (bAllowed == true)
                        {
                            //Check all user restrictions
                            Int32 nCommID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                            Int32 nUNIQUE_TIME_DIFF = int.Parse(selectQuery.Table("query").DefaultView[i].Row["UNIQUE_TIME_DIFF"].ToString());
                            Int32 nMAX_UNIQUE_VIEWS = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_UNIQUE_VIEWS"].ToString());
                            Int32 nMAX_UNIQUE_VIEWS_DAY = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_UNIQUE_VIEWS_DAY"].ToString());
                            if (CanUserWatchCommercial(nCommID, nWatcherID, nUNIQUE_TIME_DIFF, nMAX_UNIQUE_VIEWS, nMAX_UNIQUE_VIEWS_DAY) == true)
                            {
                                sCommercialTag = GetCommercialTag(ref theDoc, nCommID, sFileFormat, sFileQuality, "stand alone");
                                break;
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"tvinci_sa_comm\">");
            sRet.Append(sCommercialTag);
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static protected string GetBillingBaseTypeByMediaFileID(Int32 nMediaFileID)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("none");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select lbt.description from lu_billing_type lbt (nolock) ,media_files mf WITH (nolock) where mf.billing_type_id=lbt.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRet.Append(selectQuery.Table("query").DefaultView[0].Row["description"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static protected string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            string sNodeVal = "";
            XmlNode theNodeVal = theItem.SelectSingleNode(sXpath);
            if (theNodeVal != null)
                sNodeVal = theNodeVal.FirstChild.Value;
            return sNodeVal;
        }

        static protected bool StrToBool(string sToConvert)
        {
            bool bRet = false;
            if (sToConvert.Trim().ToLower() == "true" || sToConvert.Trim().ToLower() == "1")
                bRet = true;
            if (sToConvert.Trim().ToLower() == "false" || sToConvert.Trim().ToLower() == "0" || sToConvert.Trim().ToLower() == "")
                bRet = false;
            return bRet;
        }

        static protected string GetInStr(ref XmlDocument theDoc, bool bAllPlayers, string sXpath)
        {
            string sPlayers = " in (";
            if (bAllPlayers == true)
                return "";
            else
            {
                XmlNodeList theGroups = theDoc.SelectNodes(sXpath);
                Int32 nListCount = theGroups.Count;
                for (int i = 0; i < nListCount; i++)
                {
                    string sPlayerID = theGroups[i].SelectSingleNode("@id").Value;

                    if (i > 0)
                        sPlayers += ",";
                    sPlayers += sPlayerID;
                }
            }
            sPlayers += " ) ";
            return sPlayers;
        }

        static protected string GetGroupsChildsInStr(ref XmlDocument theDoc, bool bAllGroups, Int32 nGroupID)
        {
            string sGroups = " in (";
            if (bAllGroups == true)
            {
                sGroups += nGroupID.ToString();
                PageUtils.GetAllGroupsStr(nGroupID, ref sGroups);
            }
            else
            {
                XmlNodeList theGroups = theDoc.SelectNodes("/root/request/report/params/groups/group");
                Int32 nListCount = theGroups.Count;
                Int32 nInserted = 0;
                for (int i = 0; i < nListCount; i++)
                {
                    string sGroupID = theGroups[i].SelectSingleNode("@id").Value;
                    bool bOK = false;
                    if (int.Parse(sGroupID) == nGroupID)
                        bOK = true;
                    else
                        PageUtils.DoesGroupIsParentOfGroup(nGroupID, int.Parse(sGroupID), ref bOK);
                    if (bOK == true)
                    {
                        if (nInserted > 0)
                            sGroups += ",";
                        sGroups += sGroupID;
                        PageUtils.GetAllGroupsStr(int.Parse(sGroupID), ref sGroups);
                        nInserted++;
                    }
                }
            }
            sGroups += " ) ";
            return sGroups;
        }

        static public string ReportProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID)
        {
            bool bisPeridStartExsits = false;
            bool bisPeridEndExsits = false;
            bool bisPeriodGroupByExists = false;
            bool bisPeriodGroupByUnitsExists = false;
            bool bisPeriodOrderByNumExists = false;

            bool bisGroupsGroupBy = false;
            bool bisAllGroups = false;
            bool bisGroupsOrderByNumExists = false;

            bool bisPlayersGroupBy = false;
            bool bisAllPlayers = false;
            bool bisPlayersOrderByNumExists = false;

            bool bisQualitiesGroupBy = false;
            bool bisAllQualities = false;
            bool bisQualitiesOrderByNumExists = false;

            bool bisFormatsGroupBy = false;
            bool bisAllFormats = false;
            bool bisFormatsOrderByNumExists = false;

            bool bisCountriesGroupBy = false;
            bool bisAllCountries = false;
            bool bisCountriesOrderByNumExists = false;

            bool bisMediasGroupBy = false;
            bool bisAllMedias = false;
            bool bisMediasOrderByNumExists = false;

            bool bisCDNsGroupBy = false;
            bool bisAllCDNs = false;
            bool bisCDNsOrderByNumExists = false;


            string sPeriodStart = GetXPathSafeValue(ref theDoc, ref bisPeridStartExsits, "/root/request/report/params/period/@start", "");
            string sPeriodEnd = GetXPathSafeValue(ref theDoc, ref bisPeridEndExsits, "/root/request/report/params/period/@end", "");
            string sPeriodGroupBy = GetXPathSafeValue(ref theDoc, ref bisPeriodGroupByExists, "/root/request/report/params/period/@group_by", "false");
            string sPeriodGroupByUnits = GetXPathSafeValue(ref theDoc, ref bisPeriodGroupByUnitsExists, "/root/request/report/params/period/@group_by_units", "dd");
            string sPeriodOrderByNum = GetXPathSafeValue(ref theDoc, ref bisPeriodOrderByNumExists, "/root/request/report/params/period/@order_by", "");

            string sisGroupsGroupBy = GetXPathSafeValue(ref theDoc, ref bisGroupsGroupBy, "/root/request/report/params/groups/@group_by", "false");
            string sisAllGroups = GetXPathSafeValue(ref theDoc, ref bisAllGroups, "/root/request/report/params/groups/@all", "true");
            string sGroupsOrderByNum = GetXPathSafeValue(ref theDoc, ref bisGroupsOrderByNumExists, "/root/request/report/params/groups/@order_by", "");

            string sisPlayersGroupBy = GetXPathSafeValue(ref theDoc, ref bisPlayersGroupBy, "/root/request/report/params/players/@group_by", "false");
            string sisAllPlayers = GetXPathSafeValue(ref theDoc, ref bisAllPlayers, "/root/request/report/params/players/@all", "true");
            string sPlayersOrderByNum = GetXPathSafeValue(ref theDoc, ref bisPlayersOrderByNumExists, "/root/request/report/players/@order_by", "");

            string sisQualitiesGroupBy = GetXPathSafeValue(ref theDoc, ref bisQualitiesGroupBy, "/root/request/report/params/file_qualities/@group_by", "false");
            string sisAllQualities = GetXPathSafeValue(ref theDoc, ref bisAllQualities, "/root/request/report/params/file_qualities/@all", "true");
            string sQualitiesOrderByNum = GetXPathSafeValue(ref theDoc, ref bisQualitiesOrderByNumExists, "/root/request/report/file_qualities/@order_by", "");

            string sisFormatsGroupBy = GetXPathSafeValue(ref theDoc, ref bisFormatsGroupBy, "/root/request/report/params/file_formats/@group_by", "false");
            string sisAllFormats = GetXPathSafeValue(ref theDoc, ref bisAllFormats, "/root/request/report/params/file_formats/@all", "true");
            string sFormatsOrderByNum = GetXPathSafeValue(ref theDoc, ref bisFormatsOrderByNumExists, "/root/request/report/file_formats/@order_by", "");

            string sisCountriesGroupBy = GetXPathSafeValue(ref theDoc, ref bisCountriesGroupBy, "/root/request/report/params/countries/@group_by", "false");
            string sisAllCountries = GetXPathSafeValue(ref theDoc, ref bisAllCountries, "/root/request/report/params/countries/@all", "true");
            string sCountriesOrderByNum = GetXPathSafeValue(ref theDoc, ref bisCountriesOrderByNumExists, "/root/request/report/countries/@order_by", "");

            string sisMediasGroupBy = GetXPathSafeValue(ref theDoc, ref bisMediasGroupBy, "/root/request/report/params/medias/@group_by", "false");
            string sisAllMedias = GetXPathSafeValue(ref theDoc, ref bisAllMedias, "/root/request/report/params/medias/@all", "true");
            string sMediasOrderByNum = GetXPathSafeValue(ref theDoc, ref bisMediasOrderByNumExists, "/root/request/report/medias/@order_by", "");

            string sisCDNsGroupBy = GetXPathSafeValue(ref theDoc, ref bisCDNsGroupBy, "/root/request/report/params/cdns/@group_by", "false");
            string sisAllCDNs = GetXPathSafeValue(ref theDoc, ref bisAllCDNs, "/root/request/report/params/cdns/@all", "true");
            string sCDNsOrderByNum = GetXPathSafeValue(ref theDoc, ref bisCDNsOrderByNumExists, "/root/request/report/cdns/@order_by", "");

            string sGroups = "";
            string sPlayers = "";
            string sCDNs = "";
            string sQualities = "";
            string sFormats = "";
            string sCountries = "";
            string sMedias = "";

            bool bAllGroups = StrToBool(sisAllGroups);
            bool bAllPlayers = StrToBool(sisAllPlayers);
            bool bAllCDNs = StrToBool(sisAllCDNs);
            bool bAllQualities = StrToBool(sisAllQualities);
            bool bAllFormats = StrToBool(sisAllFormats);
            bool bAllCountries = StrToBool(sisAllCountries);
            bool bAllMedias = StrToBool(sisAllMedias);

            sGroups = GetGroupsChildsInStr(ref theDoc, bAllGroups, nGroupID);
            sPlayers = GetInStr(ref theDoc, bAllPlayers, "/root/request/report/params/players/player");
            sCDNs = GetInStr(ref theDoc, bAllCDNs, "/root/request/report/params/cdns/cdn");
            sQualities = GetInStr(ref theDoc, bAllQualities, "/root/request/report/params/file_qualities/quality");
            sFormats = GetInStr(ref theDoc, bAllFormats, "/root/request/report/params/file_formats/format");
            sCountries = GetInStr(ref theDoc, bAllCountries, "/root/request/report/params/countries/country");
            sMedias = GetInStr(ref theDoc, bAllCountries, "/root/request/report/params/medias/media");

            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"report\">");
            sRet.Append("<results>");
            sRet.Append("<fix_params>");
            //if (bisPeriodGroupByExists == false ||
            sRet.Append("</fix_params>");
            sRet.Append("<rows>");
            sRet.Append("<row index=\"0\">");
            sRet.Append("</row>");
            sRet.Append("</rows>");
            sRet.Append("</results>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        static protected string GetXPathSafeValue(ref XmlDocument theDoc, ref bool bIsExists, string sXpath, string sDef)
        {
            string sRet = "";
            bIsExists = false;
            XmlNode theNode = theDoc.SelectSingleNode(sXpath);
            if (theNode != null)
            {
                bIsExists = true;
                sRet = theNode.Value;
            }
            else
                sRet = sDef;
            return sRet;
        }

        static public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }

        static public string GetCastUpToken(string sIP, Int32 nGroupID, ref DateTime ticketExp)
        {
            string sSecretCode = "";
            object oSecretCode = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID);
            if (oSecretCode != null && oSecretCode != DBNull.Value)
                sSecretCode = oSecretCode.ToString();

            string userData = "";
            int TicketExpMinutes = 24 * 60;
            DateTime start = DateTime.Now.AddHours(-1);
            ticketExp = DateTime.Now.AddMinutes(TicketExpMinutes);
            CUWMAuthTickets.TicketGenerator tgen = new CUWMAuthTickets.TicketGeneratorClass();
            string sAuthTicket = tgen.GenerateClientTicket(sSecretCode, start, ticketExp, sIP, userData);
            return sAuthTicket;
        }

        static protected string GetASXRefElement(string sRefLink)
        {
            string sRet = "";
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
                        sRet = s.Substring(nLoc + 6, nEnd - nLoc - 6);
                        nLoc = -1;
                    }
                }
                return sRet;
            }
            catch
            {
                return sRefLink;
            }
        }

        static public string MediaOneTimeLinkProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            string sLang, Int32 nPlayerID, string sWSCDNImpleType, Int32 nWSMediaID, Int32 nWSMediaFileID,
            string sWSBaseURL, string sWSPlayerUN, ref ApiObjects.OneTimeObject oOneTimeLink)
        {
            string sCDNImplType = "";
            string sMediaURL = "";
            string sMediaID = "";
            string sMediaFileID = "";
            string sPlayerUN = "";
            StringBuilder sRet = new StringBuilder();

            string sBlock = "";

            if (theDoc != null)
            {
                XmlNode theMediaURL = theDoc.SelectSingleNode("/root/request/media/@url");
                if (theMediaURL != null)
                    sMediaURL = theMediaURL.Value;

                XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/media/@file_id");
                if (theMediaFileID != null)
                    sMediaFileID = theMediaFileID.Value;

                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value;

                XmlNode theCDNImplType = theDoc.SelectSingleNode("/root/request/media/@cdn_impl_type");
                sCDNImplType = "";
                if (theCDNImplType != null)
                    sCDNImplType = theCDNImplType.Value.ToLower();
                sPlayerUN = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "player_un");
                sRet.Append("<response type=\"media_onetime_link\">");
            }
            else
            {
                sMediaURL = sWSBaseURL;
                sMediaFileID = nWSMediaFileID.ToString();
                sMediaID = nWSMediaID.ToString();
                sCDNImplType = sWSCDNImpleType;
                sPlayerUN = sWSPlayerUN;
            }
            if (sCDNImplType == "webicast_onetime")
            {
                string sNewURL = "";
                try
                {
                    //il.co.mediazone.web11.Service theServ = new il.co.mediazone.web11.Service();
                    //sNewURL = "https://platform.tvinci.com/proxy.aspx?url=" + HttpContext.Current.Server.UrlEncode(theServ.getHashCode(sMediaURL));
                    sNewURL = sMediaURL;
                    log.Info(String.Format("{0} {1} {2}", "URL", sNewURL, "Webbycasting"));
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\"/>");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }

            }
            else if (sCDNImplType == "castup_asx_flv_onetime")
            {
                string sNewURL = sMediaURL;
                try
                {
                    if (HttpContext.Current.Session["castup_ticket"] == null || (HttpContext.Current.Session["castup_ticket"] != null && ((DateTime)(HttpContext.Current.Session["castup_ticket_epiration"])) < DateTime.Now))
                    {
                        DateTime ticketExp = DateTime.UtcNow;
                        string sAuthTicket = GetCastUpToken(PageUtils.GetCallerIP(), nGroupID, ref ticketExp);
                        HttpContext.Current.Session["castup_ticket"] = sAuthTicket;
                        HttpContext.Current.Session["castup_ticket_epiration"] = ticketExp;
                    }

                    Uri u = new Uri(sMediaURL);
                    if (u.Query != "")
                        sMediaURL += "&";
                    else
                        sMediaURL += "?";
                    sMediaURL += "ticket=" + HttpContext.Current.Session["castup_ticket"].ToString();

                    sNewURL = GetASXRefElement(sMediaURL);

                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\"/>");
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
            }
            else if (sCDNImplType == "castup_onetime")
            {
                string sNewURL = "";
                try
                {
                    if (HttpContext.Current.Session["castup_ticket"] == null || (HttpContext.Current.Session["castup_ticket"] != null && ((DateTime)(HttpContext.Current.Session["castup_ticket_epiration"])) < DateTime.Now))
                    {
                        DateTime ticketExp = DateTime.UtcNow;
                        string sAuthTicket = GetCastUpToken(PageUtils.GetCallerIP(), nGroupID, ref ticketExp);
                        HttpContext.Current.Session["castup_ticket"] = sAuthTicket;
                        HttpContext.Current.Session["castup_ticket_epiration"] = ticketExp;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                sNewURL = sMediaURL;
                Uri u = new Uri(sMediaURL);
                if (u.Query != "")
                    sNewURL += "&";
                else
                    sNewURL += "?";
                sNewURL += "ticket=" + HttpContext.Current.Session["castup_ticket"].ToString();
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\" cdn_impl_type=\"asx\" />");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }
            }
            else if (sCDNImplType == "ll_vault_onetime")
            {
                string sNewURL = "";
                string sSecretCode = "";
                object oSecretCode = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID);
                if (oSecretCode != null && oSecretCode != DBNull.Value)
                    sSecretCode = oSecretCode.ToString();
                string sRefferer = "";
                if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
                    sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();
                string sIP = PageUtils.GetCallerIP();
                //if (sRefferer.ToLower().EndsWith(".swf") || sIP == "127.0.0.1")
                sNewURL = MediaVault.GetHashedURL(sSecretCode, sMediaURL, sIP, sRefferer);
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\"/>");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }
            }
            else if (sCDNImplType == "cdnetworks_onetime")
            {

                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sMediaURL), true)).Append("\"/>");
                /*
                string sNewURL = "";
                string sSecretCode = "";
                object oSecretCode = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID);
                if (oSecretCode != null && oSecretCode != DBNull.Value)
                    sSecretCode = oSecretCode.ToString();
                string sRefferer = "";
                if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
                    sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();
                sNewURL = MediaVault.GetHashedURL(sSecretCode, sMediaURL, sIP, sRefferer);
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\"/>");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }
                */
            }
            else if (sCDNImplType == "akamai_onetime")
            {
                string sNewURL = "";
                string sConfig = "";
                object oConfig = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_SECRET_CODE", nGroupID);
                if (oConfig != null && oConfig != DBNull.Value)
                    sConfig = oConfig.ToString();
                string[] sSep = { "|" };
                string[] sConfigs = sConfig.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                string sRefferer = "";
                string sProfile = "";
                string sAifp = "";
                string sSecretCode = "";
                string sToRemove = "";
                if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
                    sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();
                string sIP = PageUtils.GetCallerIP();
                if (sConfigs.Length == 4)
                {
                    sProfile = sConfigs[0];
                    sAifp = sConfigs[1];
                    sSecretCode = sConfigs[2];
                    sToRemove = sConfigs[3];
                }
                Uri u = new Uri(sMediaURL);
                string sURL = u.PathAndQuery;
                if (sURL.ToLower().EndsWith(".flv") == true)
                    sURL = sURL.Remove(sURL.Length - 4);
                sURL = sURL.Replace(sToRemove, "");
                sURL = sURL.Replace("mp4:", "");

                Akamai.Authentication.SecureStreaming.TypeDToken d = new Akamai.Authentication.SecureStreaming.TypeDToken(
                    sURL, sIP, sProfile, sSecretCode, Convert.ToInt64(0),
                    Convert.ToInt64(7200), Convert.ToInt64(0), null);

                string sToken = d.String;

                if (sMediaURL.ToLower().IndexOf("mp4:") == -1)
                    sMediaURL = sMediaURL.Replace(".flv", "");
                sNewURL = sMediaURL + "?auth=" + sToken + "&aifp=" + sAifp + "&slist=" + sURL;
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\"/>");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }
            }
            else if (sCDNImplType == "mtv_poland_onetime")
            {
                string sNewURL = "";
                try
                {
                    if (sMediaID != "")
                    {
                        Int32 nMediaID = int.Parse(sMediaID);
                        try
                        {
                            object oVIP = ODBCWrapper.Utils.GetTableSingleVal("media", "META2_BOOL", nMediaID);
                            if (oVIP != null && oVIP != DBNull.Value)
                            {
                                Int32 nVIP = int.Parse(oVIP.ToString());
                                if (nVIP == 1)
                                {
                                    string sCallerIP = PageUtils.GetCallerIP();
                                    string sCheckURL = "http://www.s2o.tv/Is_User_VIP.aspx?ip=" + sCallerIP;
                                    Int32 nStatus = 200;
                                    string sResp = Notifier.SendGetHttpReq(sCheckURL, ref nStatus);
                                    sResp = sResp.ToLower().Trim().Replace("\r\n", "");

                                    log.Info(string.Format("{0} : {1} {2} : {3} - {4}", "Call - call from", sCallerIP, "returned", sResp, "poland_vip_check"));
                                    if (sResp == "false" || nStatus != 200)
                                    {
                                        sBlock = "vip";
                                        sNewURL = "http://files.tvinci.com/flv/vip.flv";
                                    }
                                    else
                                        sNewURL = sMediaURL;

                                }
                                else
                                    sNewURL = sMediaURL;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("", ex);
                            sNewURL = "";
                            sBlock = "error";
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                log.Info(String.Format("{0}{1} : {2}, {3}", "Block and URL returned", sBlock, sNewURL, "poland_vip_check"));
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\" block=\"").Append(sBlock).Append("\" />");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }

            }
            else if (sCDNImplType == "mtv_akamai_vip_ip_onetime")
            {
                string sNewURL = "";
                try
                {
                    if (sMediaID != "")
                    {
                        Int32 nMediaID = int.Parse(sMediaID);
                        string sVIPCheck = GetVIPIPCheckURL(sPlayerUN);
                        try
                        {
                            object oVIP = ODBCWrapper.Utils.GetTableSingleVal("media", "META2_BOOL", nMediaID);
                            if (oVIP != null && oVIP != DBNull.Value)
                            {
                                Int32 nVIP = int.Parse(oVIP.ToString());
                                if (nVIP == 1)
                                {
                                    string sCallerIP = PageUtils.GetCallerIP();
                                    string sCheckURL = sVIPCheck + "?ip=" + sCallerIP;
                                    Int32 nStatus = 200;
                                    string sResp = Notifier.SendGetHttpReq(sCheckURL, ref nStatus);
                                    sResp = sResp.ToLower().Trim();
                                    if (sResp == "false" || nStatus != 200)
                                    {
                                        sNewURL = "http://files.tvinci.com/flv/vip.flv";
                                        sBlock = "vip";
                                    }
                                    else
                                    {
                                        sNewURL = GetAkamaiURL(sMediaURL);
                                    }

                                }
                                else
                                    sNewURL = GetAkamaiURL(sMediaURL);
                            }
                        }
                        catch (Exception ex)
                        {
                            sNewURL = "";
                            sBlock = "error";
                            log.Error("", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }

                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sNewURL), true)).Append("\" block=\"").Append(sBlock).Append("\" />");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sNewURL, sBlock);
                }

            }
            else
            {
                if (theDoc != null)
                    sRet.Append("<link url=\"").Append(ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetSafeURL(sMediaURL), true)).Append("\" block=\"").Append(sBlock).Append("\" />");
                else
                {
                    if (oOneTimeLink == null)
                        oOneTimeLink = new ApiObjects.OneTimeObject();
                    oOneTimeLink.Initialize(sMediaURL, sBlock);
                }
            }
            if (theDoc != null)
                sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string GetAkamaiURL(string sURL)
        {
            Int32 nStatus = 200;
            string sResp = Notifier.SendGetHttpReq(sURL, ref nStatus);
            XmlDocument theDoc = new XmlDocument();
            theDoc.Load(sURL);
            XmlNode n1 = theDoc.SelectSingleNode("/package/video/item");
            string sSrc = GetNodeValue(ref n1, "src");
            return sSrc;
        }

        static protected string GetVIPIPCheckURL(string sPlayerUN)
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select GROUP_IP_VIP_CHECK_URL from groups g (nolock),groups_passwords gp (nolock) where gp.group_id=g.id and gp.is_active=1 and gp.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(gp.USERNAME)))", "=", sPlayerUN.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oRet = selectQuery.Table("query").DefaultView[0].Row["GROUP_IP_VIP_CHECK_URL"];
                    if (oRet != null && oRet != DBNull.Value)
                    {
                        sRet = oRet.ToString();
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }

        static public string SmsBillingCodeCheckProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID)
        {
            string sMediaFileID = "0";
            XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/media/@file_id");
            if (theMediaFileID != null)
                sMediaFileID = theMediaFileID.Value.ToUpper();

            XmlNode theCellNum = theDoc.SelectSingleNode("/root/request/sms/@cell");
            string sCellNum = "";
            if (theCellNum != null)
                sCellNum = theCellNum.Value.ToUpper();

            XmlNode theCellCode = theDoc.SelectSingleNode("/root/request/sms/@code");
            string sCellCode = "";
            if (theCellCode != null)
                sCellCode = theCellCode.Value;

            XmlNode theExtra = theDoc.SelectSingleNode("/root/request/sms/@extra");
            string sExtra = "";
            if (theExtra != null)
                sExtra = theExtra.Value;

            string sBaseBillingType = GetBillingBaseTypeByMediaFileID(int.Parse(sMediaFileID));
            StringBuilder sRet = new StringBuilder();
            sRet.Append("<response type=\"sms_billing_code_check\">");
            if (sBaseBillingType == "SMS: NewSound")
            {
                try
                {
                    string sURL = "http://babalhara.newsound.net/authentication.asp?";
                    sURL += "password=" + HttpContext.Current.Server.UrlEncode(sCellCode);
                    sURL += "&number=" + HttpContext.Current.Server.UrlEncode(sCellNum);
                    sURL += "&episode=" + HttpContext.Current.Server.UrlEncode(sExtra);
                    string sCallerIP = PageUtils.GetCallerIP();
                    sURL += "&ip=" + HttpContext.Current.Server.UrlEncode(sCallerIP);
                    XmlDocument xmlConfirm = new XmlDocument();
                    xmlConfirm.Load(sURL);
                    XmlNode n1 = xmlConfirm.SelectSingleNode("/authentication");
                    string sResponseCode = GetNodeValue(ref n1, "responsecode");
                    string sResponseReason = GetNodeValue(ref n1, "reason");
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("sms_billing");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL", "=", sCellNum);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CODE", "=", sCellCode);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTRA", "=", sExtra);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CALLER_IP", "=", sCallerIP);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RET_CODE", "=", sResponseCode);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RET_REASON", "=", sResponseReason);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                    sRet.Append("<ret code=\"").Append(ProtocolsFuncs.XMLEncode(sResponseCode, true)).Append("\" reason=\"").Append(ProtocolsFuncs.XMLEncode(sResponseReason, true)).Append("\"/>");
                }
                catch (Exception ex)
                {
                    sRet.Append("<ret code=\"-1\" reason=\"").Append(ProtocolsFuncs.XMLEncode(ex.Message, true)).Append("\"/>");
                }
            }
            else
            {
                sRet.Append("<ret code=\"-1\" reason=\"Unknown SMS check type\"/>");
            }


            sRet.Append("</response>");
            return sRet.ToString();
        }

        static public string ChannelMediaProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bWithCache,
            bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelRequestObject[] nChannels,
            ref ApiObjects.ChannelObject[] theChannelObj, Int32 nDeviceID)
        {
            log.Info(String.Format("{0} : {1} ", "ChannelMediaProtocol Start At", DateTime.Now));

            Int32 nLangID = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            bool bUseStartDate = true;
            string sUseStartDate = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "use_start_date");
            if (sUseStartDate == "false")
            {
                bUseStartDate = false;
            }


            ApiObjects.PlayListSchema oPlayListSchema = null;
            if (initObj == null)
            {
                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache);

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;


                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
            }
            else
            {
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache, ref theWSInfoStruct);
                oPlayListSchema = new ApiObjects.PlayListSchema();
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
            }
            string sDeviceID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "device_udid");
            int[] deviceRules = GetDeviceAllowedRuleIDs(sDeviceID, nGroupID).ToArray();

            StringBuilder sRet = new StringBuilder();
            if (initObj == null)
            {
                sRet.Append("<response type=\"channels_media\">");
                XmlNodeList theChannelsList = theDoc.SelectNodes("/root/request/channel");
                Int32 nCount1 = theChannelsList.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    XmlNode theMediaID = theChannelsList[i].SelectSingleNode("@id");

                    string sChannelID = "";
                    XmlNode theChannelID = theChannelsList[i].SelectSingleNode("@id");
                    if (theChannelID != null)
                        sChannelID = theChannelID.Value.ToUpper();

                    string sChannelStart = "";
                    XmlNode theChannelStart = theChannelsList[i].SelectSingleNode("@start_index");
                    if (theChannelStart != null)
                        sChannelStart = theChannelStart.Value.ToUpper();

                    string sChannelNOI = "";
                    XmlNode theChannelNOI = theChannelsList[i].SelectSingleNode("@number_of_items");
                    if (theChannelNOI != null)
                        sChannelNOI = theChannelNOI.Value.ToUpper();

                    XmlNode theChannelOrderBy = theChannelsList[i].SelectSingleNode("order_values");
                    Int32 nNumOfItems = 20;
                    Int32 nStartIndex = 0;
                    if (sChannelNOI != "")
                        nNumOfItems = int.Parse(sChannelNOI);
                    if (sChannelStart != "")
                        nStartIndex = int.Parse(sChannelStart);
                    if (sChannelID == "0")
                        return GetErrorMessage("Channel id cant be 0");

                    Int32 nOwnerGroup = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("channels", "group_id", int.Parse(sChannelID)).ToString());
                    if (nOwnerGroup != nGroupID)
                    {
                        return GetErrorMessage("Channel does not belong to group");
                    }

                    sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, int.Parse(sChannelID), nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, bWithCache));
                    DataTable d = null;
                    string sPicSizeForCache = TVinciShared.ProtocolsFuncs.GetPicSizeForCache(ref theDoc);
                    ApiObjects.PicObject[] thePics = null;
                    sRet.Append(GetChannelOpenXMLObject(ref theDoc, int.Parse(sChannelID), nLangID, bIsLangMain, nGroupID, ref d, ref theChannelOrderBy,
                        sPicSizeForCache, ref thePics, nCountryID, nDeviceID, bUseStartDate, deviceRules));
                    if (d != null)
                    {
                        log.Info(String.Format("{0} : {1} ", "Complete all medias Data Start At ", DateTime.Now));
                        Int32 nCount = d.DefaultView.Count;
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
                        string sSubFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "sub_file_format");
                        string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");

                        if (nNumOfItems == 0)
                            nNumOfItems = nCount;

                        Int32 nPageSize = nNumOfItems;
                        if (nCount - nStartIndex < nPageSize)
                            nPageSize = nCount - nStartIndex;
                        for (int i1 = nStartIndex; i1 < nStartIndex + nPageSize; i1++)
                        {
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());

                            ApiObjects.MediaObject theMediaObj = null;
                            bool bStatistics = false;
                            bool bPersonal = false;
                            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                            if (sStatistics == "true")
                                bStatistics = true;
                            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                            if (sPersonal == "true")
                                bPersonal = true;

                            sRet.Append(ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID,
                                bIsLangMain, nWatcherID, bWithInfo, bWithCache, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref thePics,
                                ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID, false, string.Empty, bUseStartDate));
                        }
                    }
                    sRet.Append("</channel>");
                }
                sRet.Append("</response>");
                log.Info(String.Format("{0} : {1} ", "Complete all medias Data End At ", DateTime.Now));
            }
            else
            {
                Int32 nCount1 = nChannels.Length;
                for (int i = 0; i < nCount1; i++)
                {
                    if (nChannels[i] == null)
                        continue;
                    Int32 nNumOfItems = 20;
                    if (nChannels[i].m_oPageDef != null)
                        nNumOfItems = nChannels[i].m_oPageDef.m_nNumberOfItems;
                    Int32 nStartIndex = 0;
                    if (nChannels[i].m_oPageDef != null)
                        nStartIndex = nChannels[i].m_oPageDef.m_nStartIndex;
                    Int32 nChannelID = 0;
                    nChannelID = nChannels[i].m_nChannelID;

                    Int32 nOwnerGroup = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("channels", "group_id", nChannelID).ToString());
                    if (nOwnerGroup != nGroupID)
                    {
                        return GetErrorMessage("Channel does not belong to group");
                    }
                    ApiObjects.PlayListSchema oPlaylistSchema = new ApiObjects.PlayListSchema();
                    ProtocolsFuncs.GetPlayListSchema(ref theDoc, nChannelID, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, bWithCache, ref oPlayListSchema);
                    DataTable d = null;
                    string sMetaFieldQuery = "";
                    string sOrderByString = GetOrderByString(ref nChannels[i].m_oOrderBy, bIsLangMain, ref sMetaFieldQuery, nGroupID);
                    string sPicSizeForCache = TVinciShared.ProtocolsFuncs.GetPicSizeForCache(ref initObj.m_oPicObjects);
                    string sTitle = "";
                    string sDescription = "";
                    string sEditorRemarks = "";
                    sRet.Append(GetChannelOpenXMLObject(ref theDoc, nChannelID, nLangID, bIsLangMain, nGroupID, ref d, sOrderByString, sMetaFieldQuery, sPicSizeForCache, ref initObj.m_oPicObjects, ref sTitle, ref sDescription, ref sEditorRemarks, nCountryID, nDeviceID));
                    if (d != null)
                    {

                        bool bStatistics = false;
                        bool bPersonal = false;
                        string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                        string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;

                        Int32 nCount = d.DefaultView.Count;
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                        if (nNumOfItems == 0)
                            nNumOfItems = nCount;
                        if (nNumOfItems > 50)
                            nNumOfItems = 50;
                        Int32 nPageSize = nNumOfItems;
                        if (nCount - nStartIndex < nPageSize)
                            nPageSize = nCount - nStartIndex;
                        for (int i1 = nStartIndex; i1 < nStartIndex + nPageSize; i1++)
                        {
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            if (IsMediaAllowedForDevice(nMediaID, sDeviceID))
                            {
                                ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                                ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);

                                if (theChannelObj[i] == null)
                                    theChannelObj[i] = new ApiObjects.ChannelObject();
                                if (theChannelObj[i].m_oMediaObjects == null)
                                    theChannelObj[i].m_oMediaObjects = new ApiObjects.MediaObject[1];
                                else
                                    theChannelObj[i].m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj[i].m_oMediaObjects, theChannelObj[i].m_oMediaObjects.Length + 1));
                                theChannelObj[i].m_oMediaObjects[theChannelObj[i].m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                                theChannelObj[i].m_oMediaObjects[theChannelObj[i].m_oMediaObjects.Length - 1] = theMediaObj;
                                theChannelObj[i].m_nChannelTotalSize = nCount;
                                theChannelObj[i].m_oPicObjects = TVinciShared.ObjectCopier.Clone<ApiObjects.PicObject[]>(initObj.m_oPicObjects);
                                theChannelObj[i].m_sTitle = sTitle;
                                theChannelObj[i].m_sDescription = sDescription;
                                theChannelObj[i].m_sEditorRemarks = sEditorRemarks;
                                theChannelObj[i].m_nID = nChannelID;
                            }
                        }

                    }
                }
            }
            log.Info(String.Format("{0} : {1} ", "ChannelMediaProtocol End At", DateTime.Now));
            return sRet.ToString();
        }

        static public bool IsMediaAllowedForDevice(int mediaID, string deviceUDID)
        {
            // get media rule-id
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select device_rule_id from media where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", mediaID);

            bool bIsAllowed = true;
            if (selectQuery.Execute("query", true) != null)
            {
                int nDeviceRuleID;
                int.TryParse(selectQuery.Table("query").Rows[0]["device_rule_id"].ToString(), out nDeviceRuleID);
                selectQuery.Finish();
                if (nDeviceRuleID != 0)
                {
                    // get rule's device brands and the 'only or but' condition
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "SELECT dr.ONLY_OR_BUT, dr.STATUS, dr.IS_ACTIVE ,drb.BRAND_ID, drb.STATUS as Brand_STATUS FROM device_rules dr, device_rules_brands drb WHERE dr.ID=drb.RULE_ID AND";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", nDeviceRuleID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        DataTable dtBrandID = selectQuery.Table("query");
                        selectQuery.Finish();
                        int nOnlyOrBut = int.Parse(dtBrandID.Rows[0]["ONLY_OR_BUT"].ToString());

                        if (int.Parse(dtBrandID.Rows[0]["STATUS"].ToString()) == 1 && int.Parse(dtBrandID.Rows[0]["IS_ACTIVE"].ToString()) == 1)
                        {

                            // check device brand by udid
                            selectQuery = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery.SetConnectionKey("users_connection");
                            selectQuery += "SELECT device_brand_id FROM devices WHERE";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", deviceUDID);
                            if (selectQuery.Execute("query", true) != null)
                            {
                                if (selectQuery.Table("query").Rows.Count > 0)
                                {
                                    int nBrandId;
                                    int.TryParse(selectQuery.Table("query").Rows[0]["device_brand_id"].ToString(), out nBrandId);
                                    selectQuery.Finish();

                                    // if "No one except the below selections"
                                    if (nOnlyOrBut == 0)
                                    {
                                        bIsAllowed = false;
                                        foreach (DataRow brand_idRow in dtBrandID.Rows)
                                        {
                                            if (int.Parse(brand_idRow["BRAND_ID"].ToString()) == nBrandId && int.Parse(brand_idRow["Brand_STATUS"].ToString()) == 1)
                                            {
                                                bIsAllowed = true;
                                                break;
                                            }
                                        }
                                    }
                                    // if "Every body except the below selection"
                                    else if (nOnlyOrBut == 1)
                                    {
                                        foreach (DataRow brand_idRow in dtBrandID.Rows)
                                        {
                                            if (int.Parse(brand_idRow["BRAND_ID"].ToString()) == nBrandId && int.Parse(brand_idRow["Brand_STATUS"].ToString()) == 1)
                                            {
                                                bIsAllowed = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else selectQuery.Finish();
                                return bIsAllowed;
                            }
                        }
                        else selectQuery.Finish();
                        return bIsAllowed;
                    }
                    else selectQuery.Finish();
                    return bIsAllowed;
                }
                else selectQuery.Finish();
                return bIsAllowed;
            }
            else selectQuery.Finish();
            return bIsAllowed;
        }



        static public string GetMetaFieldByName(string sMetaName, Int32 nGroupID, ref Int32 nJ, ref Int32 nMediaTextTypeID)
        {
            nJ = 0;
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int j = 1; j < 21; j++)
                    {
                        string sFieldName = "META" + j.ToString() + "_STR_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != null &&
                            selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString().Trim().ToLower() == sMetaName.ToLower().Trim())
                        {
                            sRet.Append("META").Append(j).Append("_STR");
                            nJ = j;
                            nMediaTextTypeID = 5;
                            break;
                        }
                    }
                    for (int j = 1; j < 11; j++)
                    {
                        string sFieldName = "META" + j.ToString() + "_DOUBLE_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != null &&
                            selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString().Trim().ToLower() == sMetaName.ToLower().Trim())
                        {
                            sRet.Append("META").Append(j).Append("_DOUBLE");
                            nJ = j;
                            nMediaTextTypeID = 6;
                            break;
                        }
                    }
                    for (int j = 1; j < 11; j++)
                    {
                        string sFieldName = "META" + j.ToString() + "_BOOL_NAME";
                        if (selectQuery.Table("query").DefaultView[0].Row[sFieldName] != null &&
                            selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString().Trim().ToLower() == sMetaName.ToLower().Trim())
                        {
                            sRet.Append("META").Append(j).Append("_BOOL");
                            nJ = j;
                            nMediaTextTypeID = 7;
                            break;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }
        /*
        static public Int32 GetTagIDByName(string sTagName, Int32 nGroupID , ref Int32 nOrderNum)
        {
            if (sTagName.ToLower().Trim() == "free")
                return 0;
            Int32 nRet = 0;
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id,ORDER_NUM from media_tags_types (nolock) where status=1 and group_id " + sGroups;

            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sTagName.ToLower().Trim());
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nOrderNum = int.Parse(selectQuery.Table("query").DefaultView[0].Row["order_num"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }
        */

        static public string GetTagsIDsByName(string sTagName, Int32 nGroupID, ref Int32 nOrderNum)
        {
            if (sTagName.ToLower().Trim() == "free")
                return "in (0)";
            string sRet = "";
            string sGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id,ORDER_NUM from media_tags_types (nolock) where status=1 and group_id " + sGroups;

            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sTagName.ToLower().Trim());
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    if (i == 0)
                        sRet += "in (";
                    else
                        sRet += ",";
                    sRet += selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    Int32 nO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ORDER_NUM"].ToString());
                    if (nO > nOrderNum)
                        nOrderNum = nO;
                }
                if (nCount > 0)
                    sRet += ")";
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sRet == "")
                sRet = "in (-1)";
            return sRet;
        }

        static protected string GetLikeStr(string sLike)
        {
            StringBuilder sRet = new StringBuilder();
            bool bWith = true;
            if (sLike.IndexOf("%") == -1)
                bWith = false;
            if (bWith == false)
                sRet.Append("%");
            if (sLike.Length < 100)
                sRet.Append(sLike);
            else
                sRet.Append(sLike.Substring(0, 100));
            if (bWith == false)
                sRet.Append("%");
            return sRet.ToString();
        }

        static protected void GetMediaIDsByTagsToQuery(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sTagType, string sTagValue, Int32 nGroupID, bool bExact, string sGroups, bool bFirst, bool bLast, bool bAnd, ref string sRelevanceQuery, ref bool bFirstRelevance)
        {
            Int32 nOrderNum = 0;
            //Int32 nTagID = GetTagIDByName(sTagType, nGroupID , ref nOrderNum);
            string sTags = GetTagsIDsByName(sTagType, nGroupID, ref nOrderNum);
            if (bFirstRelevance == false)
                sRelevanceQuery += " UNION ALL ";
            bFirstRelevance = false;
            //sRelevanceQuery += "select mt.media_id,15 as relevance from tags t (nolock),media_tags mt (nolock) where t.tag_type_id=" + nTagID.ToString() + " and mt.tag_id=t.id and t.status=1 and mt.status=1 and t.value='" + sTagValue.Replace("'", "''") + "'";
            sRelevanceQuery += "select mt.media_id,15 as relevance from tags t (nolock),media_tags mt (nolock) where t.tag_type_id " + sTags + " and mt.tag_id=t.id and t.status=1 and mt.status=1 and t.value='" + sTagValue.Replace("'", "''") + "'";
            sRelevanceQuery += " UNION ALL ";
            //sRelevanceQuery += "select mt.media_id,3 as relevance from tags t (nolock),media_tags mt (nolock) where t.tag_type_id=" + nTagID.ToString() + " and mt.tag_id=t.id and t.status=1 and mt.status=1 and t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
            sRelevanceQuery += "select mt.media_id,3 as relevance from tags t (nolock),media_tags mt (nolock) where t.tag_type_id " + sTags + " and mt.tag_id=t.id and t.status=1 and mt.status=1 and t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
            if (bAnd == true)
            {
                if (bFirst == true)
                    selectQuery += "(";
                else
                    selectQuery += " and ";
                selectQuery += " m.id in (";
                selectQuery += "select m.id from media m (nolock),media_tags mt (nolock),tags t WITH (nolock) where  mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and (";
                selectQuery += "(";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
                selectQuery += "t.tag_type_id " + sTags;
                selectQuery += " and ";
                if (bExact == false)
                    selectQuery += " t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
                else
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagValue.Trim());
                selectQuery += ")";
                selectQuery += "))";
            }
            else
            {
                if (bFirst == true)
                {
                    selectQuery += " m.id in (";
                    selectQuery += "select m.id from media m (nolock),media_tags mt (nolock),tags t WITH (nolock) where  mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and (";
                }
                else
                    selectQuery += " or ";

                selectQuery += "(";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
                selectQuery += "t.tag_type_id " + sTags;
                selectQuery += " and ";
                if (bExact == false)
                    selectQuery += " t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
                else
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagValue.Trim());
                selectQuery += ")";
                //selectQuery += ")";
            }
        }

        protected string GetMediaIDsByTags(string sTagType, string sTagValue, Int32 nGroupID, bool bExact)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nOrderNum = 0;
            //Int32 nTagID = GetTagIDByName(sTagType, nGroupID , ref nOrderNum);
            string sTags = GetTagsIDsByName(sTagType, nGroupID, ref nOrderNum);
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ";
            if (bExact == false)
                selectQuery += " distinct ";
            selectQuery += " top 250 m.id from media m (nolock),media_tags mt (nolock),tags t WITH (nolock) where  mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
            selectQuery += "t.tag_type_id " + sTags;
            selectQuery += " and ";
            if (bExact == false)
                selectQuery += " LTRIM(RTRIM(LOWER(t.value))) like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "') ";
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagValue.Trim());
            sRet.Append("in (");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["ID"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            if (sRet.ToString() == "in ()")
                return "";
            return sRet.ToString();
        }

        static protected void GetMediaTranslateIDsByTagsToQuery(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sTagType, string sTagValue, Int32 nGroupID, bool bExact, string sGroups, bool bFirst, bool bLast, bool bAnd, ref string sRelevanceQuery, ref bool bFirstRelevance)
        {
            Int32 nOrderNum = 0;
            //Int32 nTagID = GetTagIDByName(sTagType, nGroupID , ref nOrderNum);
            string sTags = GetTagsIDsByName(sTagType, nGroupID, ref nOrderNum);
            if (bFirstRelevance == false)
                sRelevanceQuery += " UNION ALL ";
            bFirstRelevance = false;
            //sRelevanceQuery += "select mt.media_id,15 as relevance from media_tags mt (nolock),tags t (nolock) LEFT JOIN tags_translate tt (nolock) ON tt.tag_id=t.id where mt.tag_id=t.id and t.tag_type_id=" + nTagID.ToString() + " and t.status=1 and mt.status=1 and (tt.value='" + sTagValue.Replace("'", "''") + "' or t.value='" + sTagValue.Replace("'", "''") + "')";
            sRelevanceQuery += "select mt.media_id,15 as relevance from media_tags mt (nolock),tags t (nolock) LEFT JOIN tags_translate tt (nolock) ON tt.tag_id=t.id where mt.tag_id=t.id and t.tag_type_id " + sTags + " and t.status=1 and mt.status=1 and (tt.value='" + sTagValue.Replace("'", "''") + "' or t.value='" + sTagValue.Replace("'", "''") + "')";
            sRelevanceQuery += " UNION ALL ";
            //sRelevanceQuery += "select mt.media_id,3 as relevance from media_tags mt (nolock),tags t (nolock) LEFT JOIN tags_translate tt (nolock) ON tt.tag_id=t.id where mt.tag_id=t.id and mt.status=1 and t.tag_type_id=" + nTagID.ToString() + " and t.status=1 and (tt.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') or t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "')) ";
            sRelevanceQuery += "select mt.media_id,3 as relevance from media_tags mt (nolock),tags t (nolock) LEFT JOIN tags_translate tt (nolock) ON tt.tag_id=t.id where mt.tag_id=t.id and mt.status=1 and t.tag_type_id " + sTags + " and t.status=1 and (tt.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') or t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "')) ";
            //sRelevanceQuery += "select mt.media_id,3 as relevance from tags t (nolock),media_tags mt (nolock),tags_translate tt (nolock) where tt.tag_id=t.id and t.tag_type_id=" + nTagID.ToString() + " and mt.tag_id=t.id and t.status=1 and mt.status=1 and tt.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";

            if (bAnd == false)
            {
                if (bFirst == true)
                {
                    selectQuery += " m.id in (select ";
                    selectQuery += " m.id from media m (nolock),media_tags mt (nolock),tags t (nolock)  LEFT JOIN tags_translate tt (nolock) ON tt.tag_id=t.id where ";
                    if (bExact == false)
                    {
                        selectQuery += " (tt.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
                        selectQuery += " or t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "')) ";
                    }
                    else
                    {
                        selectQuery += " (";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.value", "=", sTagValue.Trim());
                        selectQuery += "or";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagValue.Trim());
                        selectQuery += ")";
                    }
                    selectQuery += " and mt.tag_id=t.id and t.status=1 and mt.media_id=m.id and mt.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and (";
                }
                else
                    selectQuery += " or ";
                selectQuery += "(";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
                selectQuery += "t.tag_type_id " + sTags;
                //selectQuery += " and ";

                selectQuery += ")";
            }
            else
            {
                if (bFirst == true)
                    selectQuery += "(";
                else
                    selectQuery += " and ";
                selectQuery += " m.id in (select ";
                selectQuery += " m.id from media m (nolock),media_tags mt (nolock),tags t (nolock) LEFT JOIN tags_translate tt WITH (nolock) ON tt.tag_id=t.id  where mt.tag_id=t.id and t.status=1 and  mt.media_id=m.id and mt.status=1  and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and (";
                selectQuery += "(";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
                selectQuery += "t.tag_type_id " + sTags;
                selectQuery += " and ";
                if (bExact == false)
                {
                    selectQuery += " (tt.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";
                    selectQuery += "  or t.value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "')) ";
                }
                else
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.value", "=", sTagValue.Trim());
                    selectQuery += "or";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagValue.Trim());
                }
                selectQuery += ")";
                selectQuery += "))";
            }
        }

        protected string GetMediaTranslateIDsByTags(string sTagType, string sTagValue, Int32 nGroupID, bool bExact)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nOrderNum = 0;
            //Int32 nTagID = GetTagIDByName(sTagType, nGroupID , ref nOrderNum);
            string sTags = GetTagsIDsByName(sTagType, nGroupID, ref nOrderNum);
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetTop(150);
            selectQuery += "select distinct m.id from media m (nolock),media_tags mt (nolock),tags t (nolock),tags_translate tt WITH (nolock) where  tt.tag_id=t.id and mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
            selectQuery += "t.tag_type_id " + sTags;
            selectQuery += " and ";
            if (bExact == false)
                selectQuery += " LTRIM(RTRIM(LOWER(tt.value))) like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "') ";
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.value", "=", sTagValue.Trim());
            //selectQuery += " order by newid()";
            sRet.Append("in (");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["ID"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            if (sRet.ToString() == "in ()")
                return "";
            return sRet.ToString();
        }

        protected void GetMediaTranslateIDsByTagsQuery(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sTagType, string sTagValue, Int32 nGroupID, bool bExact)
        {
            Int32 nOrderNum = 0;
            //Int32 nTagID = GetTagIDByName(sTagType, nGroupID , ref nOrderNum);
            string sTags = GetTagsIDsByName(sTagType, nGroupID, ref nOrderNum);
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            selectQuery += "select distinct m.id from media m (nolock),media_tags mt (nolock),tags t (nolock),tags_translate tt WITH (nolock) where  tt.tag_id=t.id and mt.media_id=m.id and mt.status=1 and mt.tag_id=t.id and t.status=1 and m.status=1 and m.is_active=1 and m.group_id " + sGroups + " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.tag_type_id", "=", nTagID);
            selectQuery += "t.tag_type_id " + sTags;
            selectQuery += " and ";
            if (bExact == false)
                selectQuery += " LTRIM(RTRIM(LOWER(tt.value))) like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "') ";
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.value", "=", sTagValue.Trim());
        }

        static public DataRow GetMediaRow(int mediaID)
        {
            DataRow retVal = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media ";
            selectQuery += " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = selectQuery.Table("query").DefaultView[0].Row;
                }
            }
            return retVal;
        }

        static public int GetSearchCountNew(int mediaID, int nGroupID, int mediaType, int country, int language, int device, string permission_rule, bool bWithCache)
        {

            int retVal = 0;
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();

            //if (CachingManager.CachingManager.Exist("GetSearchCountNew" + "_" + nGroupID + "_" + mediaID.ToString() + "_" + mediaType.ToString() + country.ToString() + "_" + language.ToString() + "_" + device.ToString()) == true && bWithCache == true)
            //{
            //    retVal = int.Parse(CachingManager.CachingManager.GetCachedData("GetSearchCountNew" + "_" + nGroupID + "_" + mediaID.ToString() + "_" + mediaType.ToString() + country.ToString() + "_" + language.ToString() + "_" + device.ToString()).ToString());
            //    return int.Parse(CachingManager.CachingManager.GetCachedData("GetSearchCountNew" + "_" + nGroupID + "_" + mediaID.ToString() + "_" + mediaType.ToString() + country.ToString() + "_" + language.ToString() + "_" + device.ToString()).ToString());
            //}

            string sGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select count(distinct q.id) as co";
            selectQuery += "from (select distinct m.id ";
            selectQuery += " from media m(nolock)";
            selectQuery += " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "<>", mediaID);
            selectQuery += " and (m.id not in ";
            selectQuery += " (select id ";
            selectQuery += "from media(nolock) ";
            selectQuery += "where (start_date > getdate() or m.END_DATE < getdate()) ";
            selectQuery += " and group_id ";
            selectQuery += sGroups;
            selectQuery += "))";
            selectQuery += " and (m.id not in ";
            selectQuery += " (select media_id ";
            selectQuery += "from media_locale_values(nolock) ";
            selectQuery += "where (start_date > getdate() or m.END_DATE < getdate()) ";
            selectQuery += " and (COUNTRY_ID = 0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", country);
            selectQuery += ") and (LANGUAGE_ID = 0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", language);
            selectQuery += ") and (DEVICE_ID = 0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", device);
            selectQuery += ") and group_id ";
            selectQuery += sGroups;
            selectQuery += "))";
            selectQuery += " and m.status = 1 and m.is_active = 1";
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", mediaType);
            selectQuery += " and (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (!string.IsNullOrEmpty(permission_rule))
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += permission_rule;
                selectQuery += "))";
            }
            else
            {
                selectQuery += ")";
            }
            selectQuery += " and m.id in (	select m2.MEDIA_ID ";
            selectQuery += " from media_values m1, ";
            selectQuery += " media_values m2 ";
            selectQuery += " where	";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m1.MEDIA_ID", "=", mediaID);
            selectQuery += " and ";
            selectQuery += " m1.MEDIA_TEXT_TYPE_ID = m2.MEDIA_TEXT_TYPE_ID and m1.MEDIA_TEXT_TYPE_NUM = m2.MEDIA_TEXT_TYPE_NUM and m1.VALUE = m2.VALUE and m1.GROUP_ID = m2.GROUP_ID and m2.status=1)) q";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                log.Info(String.Format("{0} - {1} : {2}, {3} : {4}, {5} : {6}", "GetSearchCountNew", "Select Query returned", retVal, "group id is", nGroupID, "media id is", mediaID));
            }
            else
            {
                log.Error("SearchCount - Select Query returned error : " + nGroupID + "_" + mediaID.ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            //if (retVal > 0)
            //{
            //    CachingManager.CachingManager.SetCachedData("GetSearchCountNew" + "_" + nGroupID + "_" + mediaID.ToString() + "_" + mediaType.ToString() + country.ToString() + "_" + language.ToString() + "_" + device.ToString(), retVal, 3600, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            //}
            //else
            //{
            //}

            return retVal;
        }

        static protected Int32 GetSearchCount(string sEndDateField, Int32 nType, Int32 nGroupID, string sWPGID,
    string sName, bool bAnd, bool bExact, string sDescription,
    ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, Int32 nMediaID, Int32 nLangID, bool bIsLangMain,
    string sMinDate, string sMaxDate, bool bWithCache, string sDocStructForCache, Int32 nCountryID,
    Int32 nDeviceID, bool bUseStartDate)
        {
            return GetSearchCount(sEndDateField, nType, nGroupID, sWPGID, sName, bAnd, bExact, sDescription, ref theMetaList, ref theTagsList, nMediaID, nLangID, bIsLangMain,
                                  sMinDate, sMaxDate, bWithCache, sDocStructForCache, nCountryID, nDeviceID, bUseStartDate, string.Empty);
        }

        static protected Int32 GetSearchCount(string sEndDateField, Int32 nType, Int32 nGroupID, string sWPGID,
            string sName, bool bAnd, bool bExact, string sDescription,
            ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, Int32 nMediaID, Int32 nLangID, bool bIsLangMain,
            string sMinDate, string sMaxDate, bool bWithCache, string sDocStructForCache, Int32 nCountryID,
            Int32 nDeviceID, bool bUseStartDate, string sUdid)
        {
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();

            //            string sDocStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theMetaList, true);
            //sDocStruct += TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theTagsList, true);

            if (CachingManager.CachingManager.Exist("GetSearchCount" + sDocStructForCache + sEndDateField + "_" + nType.ToString() + "_" + nGroupID.ToString() + "_" + sWPGID.ToString() + "_" + sName + "_" + bAnd.ToString() + "_" + bExact.ToString() + "_" + sDescription + "_" + sMinDate + "_" + sMaxDate + "_" + nCountryID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString()) == true && bWithCache == true)
            {
                return int.Parse(CachingManager.CachingManager.GetCachedData("GetSearchCount" + sDocStructForCache + sEndDateField + "_" + nType.ToString() + "_" + nGroupID.ToString() + "_" + sWPGID.ToString() + "_" + sName + "_" + bAnd.ToString() + "_" + bExact.ToString() + "_" + sDescription + "_" + sMinDate + "_" + sMaxDate + "_" + nCountryID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString()).ToString());
            }

            //string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            string sGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            selectQuery += "select count(distinct q.id) as co from ";
            //inner 1
            selectQuery += " (select distinct m.id from media m (nolock)";
            //if (bIsLangMain == false)
            //selectQuery += ",media_translate mt (nolock)";
            selectQuery += " where ";

            if (sUdid != string.Empty && sUdid != null)
            {
                List<int> BlockedRuleIDs = GetDeviceAllowedRuleIDs(sUdid, nGroupID);
                if (BlockedRuleIDs.Count > 0)
                {
                    selectQuery += "( m.device_rule_id in (";
                    for (int i = 0; i < BlockedRuleIDs.Count; i++)
                    {
                        selectQuery += BlockedRuleIDs[i];
                        if (i < BlockedRuleIDs.Count - 1)
                        {
                            selectQuery += ",";
                        }
                    }
                    selectQuery += ")  OR m.device_rule_id IS NULL)";
                    selectQuery += " and ";
                }
            }


            if (sMinDate.Trim() != "")
            {
                DateTime dMinDate = DateUtils.GetDateFromStr(sMinDate);
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.publish_date", ">=", dMinDate);
                selectQuery += " or (m.publish_date is null and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.create_date", ">=", dMinDate);
                selectQuery += "))";
                selectQuery += " and ";
            }

            if (sMaxDate.Trim() != "")
            {
                DateTime dMaxDate = DateUtils.GetDateFromStr(sMaxDate);
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.publish_date", "<=", dMaxDate);
                selectQuery += " or (m.publish_date is null and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.create_date", "<=", dMaxDate);
                selectQuery += "))";
                selectQuery += " and ";
            }

            //if (bIsLangMain == false)
            //{
            //selectQuery += "mt.media_id=m.id and mt.name is not null and mt.name<>'' and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.LANGUAGE_ID", "=", nLangID);
            //selectQuery += " and ";
            //}
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "<>", nMediaID);
            selectQuery += " and ";

            selectQuery += " (m.id not in (select id from media (nolock) where ";
            if (bUseStartDate)
            {
                selectQuery += "(start_date>getdate() or " + sEndDateField + "<getdate())";
            }
            else
            {
                selectQuery += sEndDateField + "<getdate()";
            }

            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and group_id " + sGroups;
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += "( COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            selectQuery += ") and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "group_id " + sGroups;
            selectQuery += "))";

            selectQuery += " and m.status=1 and m.is_active=1 and ";


            //selectQuery += "m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + "> getdate()) and m.status=1 and m.is_active=1 and ";
            if (nType != 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nType);
                selectQuery += " and ";
            }

            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")";
            bool bFirst = true;
            bool bInsideFirst = true;

            string[] sep = { "||" };
            string[] sNames = sName.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string sValues = " in (";
            string sValuesLike = "(";
            for (int j = 0; j < sNames.Length; j++)
            {
                if (j > 0)
                {
                    sValues += ",";
                    sValuesLike += " or ";
                }
                sValues += "N'" + sNames[j].Replace("'", "''") + "'";
                sValuesLike += " value like (N'" + GetLikeStr(sNames[j].Trim().Replace("'", "''")) + "') ";
            }
            sValues += ") ";
            sValuesLike += ") ";

            if (sName != "")
            {
                if (bFirst == true)
                {
                    selectQuery += " and (";
                    bFirst = false;
                }
                if (bInsideFirst == false)
                {
                    if (bAnd == false)
                        selectQuery += " or ";
                    else
                        selectQuery += " and ";
                }
                else
                    bInsideFirst = false;
                selectQuery += "(";
                if (bExact == false)
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and " + sValuesLike;
                    if (sGroups != "")
                        selectQuery += "and group_id " + sGroups;
                    selectQuery += ") ";
                }
                else
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and value " + sValues;
                    //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sName);
                    if (sGroups != "")
                        selectQuery += " and group_id " + sGroups;
                    selectQuery += ") ";
                }
                selectQuery += ")";
            }

            if (sDescription != "")
            {
                if (bFirst == true)
                {
                    selectQuery += " and (";
                    bFirst = false;
                }
                if (bInsideFirst == false)
                {
                    if (bAnd == false)
                        selectQuery += " or ";
                    else
                        selectQuery += " and ";
                }
                else
                    bInsideFirst = false;
                selectQuery += "(";
                if (bExact == false)
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and value like (N'" + GetLikeStr(sDescription.Trim().ToLower().Replace("'", "''")) + "')";
                    if (sGroups != "")
                        selectQuery += "and group_id " + sGroups;
                    selectQuery += ") ";
                }
                else
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sDescription);
                    if (sGroups != "")
                        selectQuery += " and group_id " + sGroups;
                    selectQuery += ") ";
                }
                selectQuery += " ) ";
            }

            for (int i = 0; i < theMetaList.Count; i++)
            {
                XmlNode theMetaName = theMetaList[i].SelectSingleNode("@name");
                XmlNode theMetaValue = theMetaList[i].SelectSingleNode("@value");
                XmlNode theMetaMinValue = theMetaList[i].SelectSingleNode("@min_value");
                XmlNode theMetaMaxValue = theMetaList[i].SelectSingleNode("@max_value");

                string sMetaName = "";
                string sMetaValue = "";
                string sMetaMinValue = "";
                string sMetaMaxValue = "";

                if (theMetaName != null)
                    sMetaName = theMetaName.Value;
                if (theMetaValue != null)
                    sMetaValue = theMetaValue.Value;
                if (theMetaMinValue != null)
                    sMetaMinValue = theMetaMinValue.Value;
                if (theMetaMaxValue != null)
                    sMetaMaxValue = theMetaMaxValue.Value;

                if (sMetaName != "" && (sMetaValue != "" || sMetaMinValue != "" || sMetaMaxValue != ""))
                {
                    Int32 nJ = 0;
                    Int32 nMediaTextTypeID = 0;
                    string sField = "m." + GetMetaFieldByName(sMetaName, nGroupID, ref nJ, ref nMediaTextTypeID);
                    if (sField.Trim() != "m.")
                    {
                        if (!(sField.EndsWith("_STR") == true && sMetaValue.Trim() == ""))
                        {
                            if (bFirst == true)
                            {
                                selectQuery += " and (";
                                bFirst = false;
                            }
                            if (bInsideFirst == false)
                            {
                                if (bAnd == false)
                                    selectQuery += " or ";
                                else
                                    selectQuery += " and ";
                            }
                            else
                                bInsideFirst = false;
                            selectQuery += " ( ";
                            if (sField.EndsWith("_STR") == true)// || sField.EndsWith("_DOUBLE"))
                            {
                                if (bExact == false && sField.EndsWith("_STR") == true)
                                {
                                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nJ);
                                    selectQuery += " and ";
                                    selectQuery += " value like (N'" + GetLikeStr(sMetaValue.Trim().Replace("'", "''")) + "')";
                                    if (sGroups != "")
                                        selectQuery += "and group_id " + sGroups;
                                    selectQuery += ") ";
                                }
                                else
                                {
                                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nJ);
                                    if (sMetaValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sMetaValue.Trim());
                                    }
                                    if (sMetaMinValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", ">=", int.Parse(sMetaMinValue));
                                    }
                                    if (sMetaMaxValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "<=", int.Parse(sMetaMaxValue));
                                    }
                                    if (sGroups != "")
                                        selectQuery += " and group_id " + sGroups;
                                    selectQuery += ") ";
                                }
                            }
                            else
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(" + sField + ")))", "=", double.Parse(sMetaValue.Trim().ToLower()));
                            selectQuery += " ) ";
                        }
                    }
                }
            }


            string sAndTagQuery = string.Empty;
            if (theTagsList.Count > 0)
            {
                StringBuilder sbAndTagQuery = new StringBuilder();
                StringBuilder sbOrTagQuery = new StringBuilder();
                bool bInFirstAndTag = true;

                for (int i = 0; i < theTagsList.Count; i++)
                {
                    XmlNode theTagName = theTagsList[i].SelectSingleNode("@name");
                    XmlNode theTagValue = theTagsList[i].SelectSingleNode("@value");
                    XmlNode theTagMust = theTagsList[i].SelectSingleNode("@cut_with");
                    string sTagName = "";
                    string sTagValue = "";
                    bool bTagMust = false;
                    if (theTagName != null)
                        sTagName = theTagName.Value;
                    if (theTagValue != null)
                        sTagValue = theTagValue.Value;

                    if (theTagMust != null && theTagMust.Value.ToLower().Equals("and"))
                        bTagMust = true;

                    if (sTagName != "" && sTagValue != "")
                    {
                        Int32 nOrderNum = 0;
                        //Int32 nTagTypeID = GetTagIDByName(sTagName, nGroupID , ref nOrderNum);
                        string sTags = GetTagsIDsByName(sTagName, nGroupID, ref nOrderNum);

                        if (bTagMust)
                        {
                            if (bInFirstAndTag)
                            {
                                bInFirstAndTag = false;
                            }
                            else
                            {
                                sbAndTagQuery.Append(" and ");
                            }
                        }
                        else
                        {
                            if (bFirst == true)
                            {
                                selectQuery += " and ( ";
                                bFirst = false;
                            }

                            if (bInsideFirst == false)
                            {
                                if (bAnd == false)
                                    sbOrTagQuery.Append(" or ");
                                else
                                    sbOrTagQuery.Append("and ");
                            }
                            else
                                bInsideFirst = false;
                        }

                        StringBuilder sbTagTemp = new StringBuilder(" (");
                        if (bExact == false)
                        {
                            //selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and value like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "')";
                            sbTagTemp.Append("m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM " + sTags + " and (value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') or value like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "'))");


                        }
                        else
                        {
                            //selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and ";
                            sbTagTemp.Append("m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM " + sTags + " and ");
                            sbTagTemp.Append("value like (N'" + sTagValue.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "')");
                        }

                        if (sGroups != "")
                            sbTagTemp.Append(" and group_id " + sGroups);
                        sbTagTemp.Append(")  ) ");



                        if (bTagMust)
                        {
                            sbAndTagQuery.Append(sbTagTemp.ToString());
                        }
                        else
                        {
                            sbOrTagQuery.Append(sbTagTemp.ToString());
                        }
                    }
                }


                string sOrTagQuery = sbOrTagQuery.ToString();
                sAndTagQuery = sbAndTagQuery.ToString();

                if (!string.IsNullOrEmpty(sOrTagQuery))
                {
                    selectQuery += sOrTagQuery;
                }
            }

            if (bFirst == false)
            {
                selectQuery += ")";
            }

            if (!string.IsNullOrEmpty(sAndTagQuery))
            {
                selectQuery += " and ( ";
                selectQuery += sAndTagQuery;
                selectQuery += " ) ";
            }

            SortedList theOrderBy = new SortedList();
            selectQuery += ")q ";
            Int32 nRet = 0;
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CO"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            CachingManager.CachingManager.SetCachedData("GetSearchCount" + sDocStructForCache + sEndDateField + "_" + nType.ToString() + "_" + nGroupID.ToString() + "_" + sWPGID.ToString() + "_" + sName + "_" + bAnd.ToString() + "_" + bExact.ToString() + "_" + sDescription + "_" + sMinDate + "_" + sMaxDate + "_" + nCountryID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + bUseStartDate.ToString(), nRet, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            return nRet;
        }


        static public List<int> GetDeviceAllowedRuleIDs(string deviceUdid, int nGroupID)
        {
            int nBrandID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            List<int> retVal = new List<int>();

            if (string.IsNullOrEmpty(deviceUdid))
            {
                //PC if empty
                nBrandID = 22;
            }
            else
            {
                // Check for brand-id            
                selectQuery += "SELECT device_brand_id FROM devices where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", deviceUdid);
                selectQuery.SetConnectionKey("users_connection");
                if (selectQuery.Execute("query", true) != null)
                {
                    if (selectQuery.Table("query").Rows.Count > 0)
                    {
                        nBrandID = int.Parse(selectQuery.Table("query").Rows[0]["device_brand_id"].ToString());
                        selectQuery.Finish();
                    }
                    else
                    {
                        selectQuery.Finish();
                        return retVal;
                    }
                }
                else
                {
                    selectQuery.Finish();
                    return retVal;
                }
            }

            // Get rules in which this device is allowed
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT DISTINCT m.DEVICE_RULE_ID FROM media m, device_rules dr, device_rules_brands drb where m.device_rule_id=dr.ID AND dr.ID=drb.RULE_ID  AND dr.STATUS=1 AND dr.IS_ACTIVE=1 AND drb.STATUS=1 AND ";
            selectQuery += "m.GROUP_ID " + TVinciShared.PageUtils.GetGroupsStrByParent(nGroupID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("drb.BRAND_ID", "=", nBrandID);

            DataTable dt = selectQuery.Execute("query", true);
            selectQuery.Finish();
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        retVal.Add(int.Parse(dr["DEVICE_RULE_ID"].ToString()));
                    }
                }
            }
            return retVal;
        }


        static protected Int32 GetTotalSearchSize(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, bool bAnd, bool bRelated, Int32 nLangID, bool bIsLangMain, bool bWithCache, string sDocStruct, ref ApiObjects.SearchDefinitionObject theSearchCriteria,
            ref ApiObjects.InitializationObject theIniObj, Int32 nCountryID, Int32 nDeviceID, bool bUseStartDate, string sUdid)
        {
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            string sName = "";
            string sMinDate = "";
            string sMaxDate = "";
            string sDescription = "";
            XmlNode theInfoStruct = null;
            bool bExact = false;
            Int32 nType = 0;
            XmlNodeList theTagsList = null;
            XmlNodeList theMetaList = null;
            string sEndDateField = "m.";

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            if (theSearchCriteria == null)
            {
                XmlNode theName = theDoc.SelectSingleNode("/root/request/search_data/cut_values/name/@value");
                if (theName != null)
                    sName = theName.Value;

                XmlNode theMinDate = theDoc.SelectSingleNode("/root/request/search_data/cut_values/date/@min_value");
                if (theMinDate != null)
                    sMinDate = theMinDate.Value;

                XmlNode theMaxDate = theDoc.SelectSingleNode("/root/request/search_data/cut_values/date/@max_value");
                if (theMaxDate != null)
                    sMaxDate = theMaxDate.Value;

                XmlNode theDesc = theDoc.SelectSingleNode("/root/request/search_data/cut_values/description/@value");
                if (theDesc != null)
                    sDescription = theDesc.Value;

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);

                string sExact = "";
                XmlNode theExact = theDoc.SelectSingleNode("/root/request/search_data/cut_values/@exact");
                if (theExact != null)
                    sExact = theExact.Value.ToLower().Trim();
                if (sExact == "true")
                    bExact = true;

                XmlNode theType = theDoc.SelectSingleNode("/root/request/search_data/cut_values/type/@value");
                if (theType != null)
                    nType = int.Parse(theType.Value);

                theTagsList = theDoc.SelectNodes("/root/request/search_data/cut_values/tags/tag_type");
                theMetaList = theDoc.SelectNodes("/root/request/search_data/cut_values/meta");

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
            }
            else
            {
                if (theSearchCriteria.m_eAndOr == ApiObjects.AndOr.And)
                    bAnd = true;
                else
                    bAnd = false;

                bExact = theSearchCriteria.m_bExact;
                sName = theSearchCriteria.m_sTitle;
                if (theSearchCriteria.m_dMinDate.Year < 2099 && theSearchCriteria.m_dMinDate.Year > 1)
                    sMinDate = DateUtils.GetStrFromDate(theSearchCriteria.m_dMinDate);

                if (theSearchCriteria.m_dMaxDate.Year < 2099 && theSearchCriteria.m_dMaxDate.Year > 1)
                    sMaxDate = DateUtils.GetStrFromDate(theSearchCriteria.m_dMaxDate);
                sDescription = theSearchCriteria.m_sDescription;
                if (theSearchCriteria.m_sTypeName != "")
                    nType = GetFileTypeID(theSearchCriteria.m_sTypeName, nGroupID);
                ApiObjects.MediaInfoStructObject theWSInfoStruct = null;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache, ref theWSInfoStruct);
                theDoc = ConvertSearchStructToInfoXML(ref theSearchCriteria);
                sDocStruct = ConvertXMLToString(ref theDoc, true);
                theTagsList = theDoc.SelectNodes("/root/request/search_data/cut_values/tags/tag_type");
                theMetaList = theDoc.SelectNodes("/root/request/search_data/cut_values/meta");
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(theIniObj.m_oExtraRequestObject.m_bUseFinalEndDate);
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
            }

            return GetSearchCount(sEndDateField, nType, nGroupID,
                sWPGID, sName, bAnd, bExact, sDescription, ref theMetaList, ref theTagsList, 0,
                nLangID, bIsLangMain, sMinDate, sMaxDate, bWithCache, sDocStruct, nCountryID, nDeviceID, bUseStartDate, sUdid);
        }

        static protected Int32 GetTotalSearchSize(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, bool bAnd, bool bRelated, Int32 nLangID, bool bIsLangMain, bool bWithCache, string sDocStruct, ref ApiObjects.SearchDefinitionObject theSearchCriteria,
    ref ApiObjects.InitializationObject theIniObj, Int32 nCountryID, Int32 nDeviceID, bool bUseStartDate)
        {
            return GetTotalSearchSize(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, bAnd, bRelated, nLangID, bIsLangMain, bWithCache, sDocStruct, ref theSearchCriteria,
        ref theIniObj, nCountryID, nDeviceID, bUseStartDate, string.Empty);
        }


        static public string TagValuesProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID,
            ref ApiObjects.InitializationObject initObj, ref ApiObjects.TagRequestObject[] sTagTypes,
            ref ApiObjects.TagResponseObject[] theTags, Int32 nCountryID, Int32 nDeviceID)
        {

            bool bIsLangMain = true;
            Int32 nLangID = 0;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            StringBuilder sRet = new StringBuilder();
            if (theDoc != null)
            {
                sRet.Append("<response type=\"tag_values\">");
                XmlNodeList theTagsList = theDoc.SelectNodes("/root/request/tags/tag_type");
                for (int i = 0; i < theTagsList.Count; i++)
                {
                    XmlNode theTagName = theTagsList[i].SelectSingleNode("@name");
                    XmlNode theOrderBy = theTagsList[i].SelectSingleNode("@order_by");
                    XmlNode theInMin = theTagsList[i].SelectSingleNode("@in_min");
                    XmlNode theInMax = theTagsList[i].SelectSingleNode("@in_max");

                    string sTagName = "";
                    if (theTagName != null)
                        sTagName = theTagName.Value;

                    string sOrderBy = "";
                    if (theOrderBy != null)
                        sOrderBy = theOrderBy.Value;

                    string sInMin = "";
                    if (theInMin != null)
                        sInMin = theInMin.Value;
                    string sInMax = "";
                    if (theInMax != null)
                        sInMax = theInMax.Value;

                    if (sTagName != "")
                    {
                        if (CachingManager.CachingManager.Exist("TagValuesProtocol_" + nGroupID.ToString() + "_" + sTagName + "_" + sOrderBy + "_" + sInMin + "_" + sInMax + "_" + nLangID.ToString() + "_" + nCountryID.ToString() + "_" + nDeviceID.ToString()) == true)
                            sRet.Append(CachingManager.CachingManager.GetCachedData("TagValuesProtocol_" + nGroupID.ToString() + "_" + sTagName + "_" + sOrderBy + "_" + sInMin + "_" + sInMax + "_" + nLangID.ToString() + "_" + nCountryID.ToString() + "_" + nDeviceID.ToString()).ToString());
                        else
                        {
                            StringBuilder sTmpRet = new StringBuilder();
                            sTmpRet.Append("<tag_type name=\"").Append(sTagName).Append("\">");
                            Int32 nOrderNum = 0;
                            //Int32 nTagID = GetTagIDByName(sTagName, nGroupID, ref nOrderNum);
                            string sTags = GetTagsIDsByName(sTagName, nGroupID, ref nOrderNum);
                            //if (nTagID != 0)
                            if (sTags != "")
                            {
                                #region Old Code
                                //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                //selectQuery.SetCachedSec(14400);
                                //selectQuery += "select q.id,q.value,count(q.m_id) as co from (select distinct m.id as m_id,t.id,t.value from tags t (nolock),media_tags mt (nolock),media m WITH (nolock) where mt.STATUS=1 and t.status=1 and mt.STATUS=1 and mt.tag_id=t.id and mt.MEDIA_ID=m.id and m.IS_ACTIVE=1 and m.status=1 and ";

                                //selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or m.end_date<getdate() or m.FINAL_END_DATE<getdate()) and ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                                //selectQuery += "))";
                                //selectQuery += " and ";
                                //selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or m.end_date<getdate() or m.FINAL_END_DATE<getdate()) and ";
                                //selectQuery += "( COUNTRY_ID=0 or ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                                //selectQuery += ") and (LANGUAGE_ID=0 or ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                                //selectQuery += ") and (DEVICE_ID=0 or ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
                                //selectQuery += ") and ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                                //selectQuery += "))";

                                ////selectQuery += " m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and (m.FINAL_END_DATE is null or m.FINAL_END_DATE>getdate()) and ";
                                //selectQuery += " and ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.Group_id", "=", nGroupID);
                                //selectQuery += "and";
                                //if (sInMin != "" && sInMax != "")
                                //    selectQuery += "t.value between '" + sInMin + "' and '" + sInMax + "' and ";
                                ////selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.TAG_TYPE_ID", "=", nTagID);
                                //selectQuery += "t.TAG_TYPE_ID " + sTags;
                                //selectQuery += ")q ";
                                //selectQuery += " group by q.id,q.value ";
                                #endregion
                                string sGroups = PageUtils.GetFullChildGroupsStr(nGroupID, "");
                                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery.SetCachedSec(14400);
                                selectQuery += "select q.id,q.value,count(q.m_id) as co from (select distinct m.id as m_id,t.id,t.value from tags t (nolock),media_tags mt (nolock),media m WITH (nolock) where mt.STATUS=1 and t.status=1 and mt.STATUS=1 and mt.tag_id=t.id and mt.MEDIA_ID=m.id and m.IS_ACTIVE=1 and m.status=1 and ";

                                selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or m.end_date<getdate() or m.FINAL_END_DATE<getdate()) and ";
                                selectQuery += " group_id " + sGroups;
                                selectQuery += "))";
                                selectQuery += " and ";
                                selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or m.end_date<getdate() or m.FINAL_END_DATE<getdate()) and ";
                                selectQuery += "( COUNTRY_ID=0 or ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                                selectQuery += ") and (LANGUAGE_ID=0 or ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                                selectQuery += ") and (DEVICE_ID=0 or ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
                                selectQuery += ") and ";
                                selectQuery += " group_id " + sGroups;
                                selectQuery += "))";

                                //selectQuery += " m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and (m.FINAL_END_DATE is null or m.FINAL_END_DATE>getdate()) and ";
                                selectQuery += " and ";
                                selectQuery += " m.Group_id " + sGroups;
                                selectQuery += "and";
                                if (sInMin != "" && sInMax != "")
                                    selectQuery += "t.value between '" + sInMin + "' and '" + sInMax + "' and ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.TAG_TYPE_ID", "=", nTagID);
                                selectQuery += "t.TAG_TYPE_ID " + sTags;
                                selectQuery += ")q ";
                                selectQuery += " group by q.id,q.value ";
                                if (sOrderBy == "count")
                                    selectQuery += " order by count(q.id) desc";
                                else
                                    selectQuery += " order by q.value ";
                                if (selectQuery.Execute("query", true) != null)
                                {
                                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                    for (int j = 0; j < nCount1; j++)
                                    {
                                        string sName = "";
                                        string sCO = "";
                                        Int32 nTag = int.Parse(selectQuery.Table("query").DefaultView[j].Row["ID"].ToString());
                                        if (bIsLangMain == true)
                                            sName = ProtocolsFuncs.XMLEncode(selectQuery.Table("query").DefaultView[j].Row["value"].ToString().Replace("&quot;", "\""), true);
                                        else
                                            sName = ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetTagTranslation(nTag, nLangID).Replace("&quot;", "\""), true);
                                        sCO = selectQuery.Table("query").DefaultView[j].Row["co"].ToString();
                                        if (sName.Trim() != "")
                                            sTmpRet.Append("<tag name=\"").Append(sName).Append("\" ");
                                        sTmpRet.Append("count=\"");
                                        sTmpRet.Append(sCO);
                                        sTmpRet.Append("\" ");
                                        sTmpRet.Append("/>");
                                    }
                                }
                                selectQuery.Finish();
                                selectQuery = null;
                            }
                            sTmpRet.Append("</tag_type>");
                            CachingManager.CachingManager.SetCachedData("TagValuesProtocol_" + nGroupID.ToString() + "_" + sTagName + "_" + sOrderBy + "_" + sInMin + "_" + sInMax + "_" + nLangID.ToString() + "_" + nCountryID.ToString() + "_" + nDeviceID.ToString(), sTmpRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                            sRet.Append(sTmpRet.ToString());
                        }
                    }
                }
                sRet.Append("</response>");
            }
            else
            {
                Int32 nCount = sTagTypes.Length;
                Int32 nLoc = 0;
                for (int i = 0; i < nCount; i++)
                {
                    string sTagName = sTagTypes[i].m_sTagName;
                    string sOrderBy = sTagTypes[i].m_sOrderBy;
                    string sInMin = "";
                    string sInMax = "";

                    if (sTagTypes[i].m_oRange != null)
                    {
                        sInMin = sTagTypes[i].m_oRange.m_sMin;
                        sInMax = sTagTypes[i].m_oRange.m_sMax;
                    }
                    if (sTagName != "")
                    {

                        Int32 nOrderNum = 0;
                        //Int32 nTagID = GetTagIDByName(sTagName, nGroupID, ref nOrderNum);
                        string sTags = GetTagsIDsByName(sTagName, nGroupID, ref nOrderNum);
                        //if (nTagID != 0)
                        if (sTags != "")
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery.SetCachedSec(14400);
                            selectQuery += "select q.id,q.value,count(q.m_id) as co from (select distinct m.id as m_id,t.id,t.value from tags t (nolock),media_tags mt (nolock),media m WITH (nolock) where mt.STATUS=1 and t.status=1 and mt.STATUS=1 and mt.tag_id=t.id and mt.MEDIA_ID=m.id and m.IS_ACTIVE=1 and m.status=1 and ";

                            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or end_date<getdate()  or m.FINAL_END_DATE<getdate()) and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            selectQuery += "))";
                            selectQuery += " and ";
                            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or end_date<getdate()  or m.FINAL_END_DATE<getdate()) and ";
                            selectQuery += " (COUNTRY_ID=0 or ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                            selectQuery += ") and (LANGUAGE_ID=0 or ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                            selectQuery += ") and (DEVICE_ID=0 or ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
                            selectQuery += ") and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            selectQuery += "))";
                            selectQuery += " and ";

                            //selectQuery += " m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and (m.FINAL_END_DATE is null or m.FINAL_END_DATE>getdate()) and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.Group_id", "=", nGroupID);
                            selectQuery += "and";
                            if (sInMin != "" && sInMax != "")
                                selectQuery += "t.value between '" + sInMin + "' and '" + sInMax + "' and ";
                            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.TAG_TYPE_ID", "=", nTagID);
                            selectQuery += "t.TAG_TYPE_ID" + sTags;
                            selectQuery += ")q ";
                            selectQuery += " group by q.id,q.value ";
                            if (sOrderBy == "count")
                                selectQuery += " order by count(q.id) desc";
                            else
                                selectQuery += " order by q.value ";
                            if (selectQuery.Execute("query", true) != null)
                            {
                                Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                if (theTags == null)
                                    theTags = new ApiObjects.TagResponseObject[nCount1];
                                else
                                    theTags = (ApiObjects.TagResponseObject[])(ResizeArray(theTags, theTags.Length + nCount1));
                                for (int j = 0; j < nCount1; j++)
                                {
                                    string sName = "";
                                    Int32 nCO = 0;
                                    Int32 nTag = int.Parse(selectQuery.Table("query").DefaultView[j].Row["ID"].ToString());
                                    if (bIsLangMain == true)
                                        sName = ProtocolsFuncs.XMLEncode(selectQuery.Table("query").DefaultView[j].Row["value"].ToString().Replace("&quot;", "\""), true);
                                    else
                                        sName = ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetTagTranslation(nTag, nLangID).Replace("&quot;", "\""), true);
                                    nCO = int.Parse(selectQuery.Table("query").DefaultView[j].Row["co"].ToString());
                                    theTags[nLoc] = new ApiObjects.TagResponseObject();
                                    theTags[nLoc].Initialize(sName, nCO);
                                    nLoc++;
                                }
                            }
                            selectQuery.Finish();
                            selectQuery = null;
                        }
                    }
                }
                if (nLoc > 0 && nLoc + 1 < theTags.Length)
                    theTags = (ApiObjects.TagResponseObject[])(ResizeArray(theTags, nLoc + 1));
            }
            return sRet.ToString();
        }

        static protected Int32 GetMostViewdCountNew(Int32 nGroupID, string sMediaTypes)
        {

            Int32 nRet = 0;
            string groupStr = PageUtils.GetGroupsStrByParent(nGroupID); ;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select COUNT(*) as co from media m(nolock) ";
            selectQuery += " WHERE m.is_active=1 and m.status=1 and m.start_date < getdate() and (m.end_date is null or m.end_date>getdate()) and ";
            if (!string.IsNullOrEmpty(sMediaTypes) && !sMediaTypes.Equals("0"))
            {
                selectQuery += "and m.media_type_id in (" + sMediaTypes.Replace(";", ",") + ")";
                selectQuery += "and";
            }
            selectQuery += "m.group_id ";
            selectQuery += groupStr;
            selectQuery += " and m.views <= (select max(views) from media where group_id ";
            selectQuery += groupStr;
            selectQuery += ") ";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static protected Int32 GetMostViewdCount(Int32 nGroupID, string sWPGID, Int32 nFormatID,
            Int32 nQualityID, string sHours, Int32 nActionID)
        {

            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetWritable(true);
            selectQuery += "select COUNT(*) as co from watchers_media_actions wma(nolock),media m(nolock) ";
            selectQuery.SetCachedSec(0);
            selectQuery += " WHERE m.id=wma.media_id and (m.is_active=1 and m.status=1 and m.start_date < getdate() and (m.end_date is null or m.end_date>getdate())) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.FILE_FORMAT_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.FILE_QUALITY_ID", "=", nQualityID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.ACTION_ID", "=", nActionID);
            selectQuery += " and wma.CREATE_DATE > DATEADD(hh, - " + sHours + ", GETDATE())";
            selectQuery += " and ";
            selectQuery += " (";
            selectQuery += "wma.group_id";
            selectQuery += PageUtils.GetGroupsStrByParent(nGroupID);
            if (sWPGID != "")
            {
            }
            selectQuery += ")";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static protected Int32 GetNowPlayingCount(Int32 nGroupID, string sWPGID)
        {
            DateTime t = DateTime.UtcNow.AddMinutes(-2);
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetWritable(true);
            selectQuery += "select COUNT(DISTINCT MEDIA_ID) as co from media_eoh m ";
            selectQuery.SetCachedSec(0);
            selectQuery += " WITH (nolock) where m.update_date>DATEADD(s,-30,getdate()) and ";
            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                //selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                //selectQuery += sWPGID;
                //selectQuery += ")";
            }
            selectQuery += ")";

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static protected Int32 GetPersonalLastWatchedCount(Int32 nGroupID, string sWPGID, Int32 nWatcherID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            // selectQuery.SetLockTimeOut(10000);
            selectQuery.SetWritable(true);
            selectQuery += "select COUNT(DISTINCT MEDIA_ID) as co from watchers_media_actions m WITH (nolock) where ";
            selectQuery.SetCachedSec(0);
            selectQuery += " (";
            selectQuery += "m.group_id ";
            selectQuery += PageUtils.GetGroupsStrByParent(nGroupID);
            if (sWPGID != "")
            {
                //selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                //selectQuery += sWPGID;
                //selectQuery += ")";
            }
            selectQuery += ")";
            selectQuery += " and ACTION_ID=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static protected Int32 GetPersonalLastWatchedCount(Int32 nGroupID, string sWPGID, string sWatcherID)
        {
            Int32 nRet = 0;
            if (!string.IsNullOrEmpty(sWatcherID))
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                // selectQuery.SetLockTimeOut(10000);
                selectQuery.SetWritable(true);
                selectQuery += "select COUNT(DISTINCT MEDIA_ID) as co from watchers_media_actions m WITH (nolock) where ";
                selectQuery.SetCachedSec(0);
                selectQuery += " (";
                selectQuery += "m.group_id ";
                selectQuery += PageUtils.GetGroupsStrByParent(nGroupID);
                if (sWPGID != "")
                {
                    //selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                    //selectQuery += sWPGID;
                    //selectQuery += ")";
                }
                selectQuery += ")";
                selectQuery += " and ACTION_ID=1 and ";
                selectQuery += " watcher_id in (";
                selectQuery += sWatcherID;
                selectQuery += ")";
                // selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            return nRet;
        }

        static protected Int32 GetPersonalRateCount(Int32 nGroupID, string sWPGID, Int32 nWatcherID, Int32 nMinRate,
            Int32 nMaxRate, string sEndDateField, Int32 nCountryID, Int32 nLangID, Int32 nDeviceID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select COUNT(distinct wmr.media_id) as co from watchers_media_rating wmr (nolock),media m (nolock) where m.id=wmr.media_id and m.status=1 and m.is_active=1 and ";
            selectQuery.SetWritable(true);
            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += " (COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";

            selectQuery += " and ";
            //selectQuery += " (m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + ">getdate())) and ";
            selectQuery.SetCachedSec(0);
            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.group_id", "=", nGroupID);
            //if (sWPGID != "")
            //{
            //selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
            //selectQuery += sWPGID;
            //selectQuery += ")";
            //}
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.rate_val", "<=", nMaxRate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.rate_val", ">=", nMinRate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.WATCHER_ID", "=", nWatcherID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

        static public string PersonalRatedProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID,
            ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj,
            ref ApiObjects.PageDefinition thePageDef, Int32 nWSMinRate, Int32 nWSMaxRate, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nNewWatcherID = nWatcherID;
            string sNewTvinciGUID = sTVinciGUID;

            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            Int32 nMinRate = 0;
            Int32 nMaxRate = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            ApiObjects.PlayListSchema oPlaylistSchema = null;
            string sEndDateField = "m.";

            StringBuilder sRet = new StringBuilder();

            if (theDoc != null)
            {
                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

                XmlNode theSiteGUID = theDoc.SelectSingleNode("/root/request/rate/@site_guid");
                if (theSiteGUID != null)
                    sSiteGUID = theSiteGUID.Value.Trim();

                if (sSiteGUID != "")
                    nNewWatcherID = GetWatcherIDBySiteGUID(sSiteGUID, nGroupID, ref sNewTvinciGUID);

                XmlNode theMinRate = theDoc.SelectSingleNode("/root/request/rate/@min");
                if (theMinRate != null)
                    nMinRate = int.Parse(theMinRate.Value.ToUpper());

                XmlNode theMaxRate = theDoc.SelectSingleNode("/root/request/rate/@max");
                if (theMaxRate != null)
                    nMaxRate = int.Parse(theMaxRate.Value.ToUpper());

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;

                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
            }
            else
            {
                oPlaylistSchema = new ApiObjects.PlayListSchema();
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);
                nStartIndex = thePageDef.m_nStartIndex;
                nNumOfItems = thePageDef.m_nNumberOfItems;
                if (sSiteGUID != "")
                    nNewWatcherID = GetWatcherIDBySiteGUID(sSiteGUID, nGroupID, ref sNewTvinciGUID);
                nMinRate = nWSMinRate;
                nMaxRate = nWSMaxRate;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);
            }

            Int32 nCo = 0;
            if (nNewWatcherID != 0)
                nCo = GetPersonalRateCount(nGroupID, sWPGID, nNewWatcherID, nMinRate, nMaxRate,
                    sEndDateField, nCountryID, nLangID, nDeviceID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery.SetWritable(true);
            selectQuery += "select top " + (nNumOfItems + nStartIndex).ToString() + " wmr.media_id,wmr.RATE_VAL from watchers_media_rating wmr (nolock),media m ";
            selectQuery += " WITH (nolock) where ";
            selectQuery += " m.id=wmr.media_id and m.status=1 and m.is_active=1 and ";

            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += " (COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";

            selectQuery += " and ";
            //selectQuery += " (m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + ">getdate())) and ";

            selectQuery += " (";
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);

            selectQuery += " m.GROUP_ID " + sGroups;
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.rate_val", "<=", nMaxRate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.rate_val", ">=", nMinRate);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmr.WATCHER_ID", "=", nNewWatcherID);
            selectQuery += " order by wmr.rate_val desc,wmr.create_date desc ";
            if (theDoc != null)
            {
                sRet.Append("<response type=\"personal_rated\">");
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nNewWatcherID, nPlayerID, true));
                sRet.Append("<channel id=\"\" media_count=\"").Append(nCo).Append("\" >");
            }
            if (nNewWatcherID != 0)
            {
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    Int32 nEnd = nStartIndex + nNumOfItems;
                    if (nEnd > nCount)
                        nEnd = nCount;
                    for (int i = nStartIndex; i < nEnd; i++)
                    {
                        Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_ID"].ToString());
                        if (theDoc != null)
                            sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nNewWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct,
                                bIsAdmin, false, bWithFileTypes, nCountryID, nDeviceID));
                        else
                        {
                            Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                            string sFileFormat = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                                sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                            string sSubFileFormat = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                                sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                            string sFileQuality = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                                sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                            bool bStatistics = false;
                            bool bPersonal = false;
                            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                            if (sStatistics == "true")
                                bStatistics = true;
                            if (sPersonal == "true")
                                bPersonal = true;
                            ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                            ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                            if (theChannelObj.m_oMediaObjects == null)
                                theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                            else
                                theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                            theChannelObj.m_nChannelTotalSize = nCo;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            if (theDoc != null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }
            return sRet.ToString();
        }

        static public string PersonalRecommendedProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID,
            bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj, ref ApiObjects.PageDefinition thePageDef,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj, Int32 nDeviceID, bool bWithCache)
        {
            StringBuilder retVal = new StringBuilder();
            Int32 nLangID = 0;
            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            string sEndDateField = "m.";
            ApiObjects.PlayListSchema oPlaylistSchema = null;

            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sPlayListSchema = ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true);
            string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
            string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");

            //string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);

            //StringBuilder sRet = new StringBuilder();
            string watcherIDStr = string.Empty;
            if (theDoc != null)
            {
                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();



                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;

                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
                string sParamsSiteGUID = "";
                XmlNode theParamsSiteGUID = theDoc.SelectSingleNode("/root/request/params/@site_guid");
                if (theParamsSiteGUID != null)
                    sParamsSiteGUID = theParamsSiteGUID.Value.ToUpper();

                string sMediaTypes = "";
                XmlNode theMediaTypes = theDoc.SelectSingleNode("/root/request/params/@media_types");
                if (theMediaTypes != null)
                {
                    sMediaTypes = theMediaTypes.Value;
                }

                string sDeviceID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "device_udid");
                int[] deviceRules = GetDeviceAllowedRuleIDs(sDeviceID, nGroupID).ToArray();

                int nLastMediaID = 0;

                if (!string.IsNullOrEmpty(sParamsSiteGUID))
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetCachedSec(0);
                    selectQuery += "select top 1 umm.media_id from users_media_mark umm, media m where umm.group_id " + PageUtils.GetGroupsStrByParent(nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("umm.site_user_guid", "=", sParamsSiteGUID);
                    selectQuery += "and umm.media_id=m.id and m.is_active=1 and m.status=1 and (m.start_date<getdate() and (m.end_date>getdate() or m.end_date is null))";
                    selectQuery += "order by umm.update_date desc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                        if (nCount > 0)
                        {
                            nLastMediaID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_id", 0);
                        }

                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }


                if (nLastMediaID > 0)
                {
                    if (theDoc != null)
                    {
                        Int32 nType = 0;
                        string sName = "";
                        string sDescription = "";
                        string sMediaStruct = "";

                        ApiObjects.MediaInfoObject theInfo = null;
                        GetMediaBasicData(nLastMediaID, ref nType, ref sName, ref sDescription, ref sMediaStruct,
                            nLangID, bIsLangMain, ref theInfoStruct, ref theInfo);

                        if (string.IsNullOrEmpty(sMediaTypes))
                        {
                            sMediaTypes = nType.ToString();
                        }

                        /*
                        int nCoNew = GetSearchCountNew(nLastMediaID, nGroupID, nType, nCountryID, nLangID, nDeviceID, sWPGID, bWithCache);
                        if (nCoNew > 0)
                            nCoNew--;
                        */

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(sMediaStruct);
                        XmlNodeList theMetaList = doc.SelectNodes("/root/tags_meta/meta");
                        XmlNodeList theTagsList = doc.SelectNodes("/root/tags_collections/tag");
                        string sDocStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theDoc, true);


                        /*
                        nStartIndex += 1;
                        string sInner = GetSearchMediaInner(nNumOfItems, nStartIndex, sEndDateField,
                            nGroupID, nType, sWPGID, sName, false, true, "", ref theMetaList,
                            ref theTagsList, " order by q1.co desc ", sPlayListSchema, nCoNew,
                            ref theDoc, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID,
                            ref theInfoStruct, bIsAdmin, "", bWithFileTypes, nCountryID, "", "", sDocStruct,
                            ref initObj, ref theWSInfoStruct, ref theChannelObj, nDeviceID, nLastMediaID);
                        */

                        string sInner = GetSearchMediaWithLucene(nStartIndex, nNumOfItems, nLastMediaID, nGroupID, sMediaTypes, sName, false, true, sDescription,
                            ref theMetaList, ref theTagsList, sPlayListSchema, ref theDoc, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref theInfoStruct,
                            bIsAdmin, bWithFileTypes, nCountryID, string.Empty, string.Empty, sDocStruct, ref theWSInfoStruct, nDeviceID, true, deviceRules);

                        if (initObj == null)
                        {
                            retVal.Append("<response type=\"personal_recommended\">");
                            retVal.Append(sInner);
                            retVal.Append("</response>");

                        }
                    }
                }
                else
                {
                    //Get most viewed items
                    int mostViewedCount = GetMostViewdCountNew(nGroupID, sMediaTypes);
                    string groupStr = PageUtils.GetGroupsStrByParent(nGroupID); ;


                    retVal.Append("<response type=\"personal_recommended\">");
                    retVal.Append("<channel id=\"\" media_count=\"").Append(mostViewedCount).Append("\" >");

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select top " + (nNumOfItems + nStartIndex).ToString() + " m.id, m.views from media m (nolock) ";
                    selectQuery += " WHERE m.is_active=1 and m.status=1 and m.start_date < getdate() and (m.end_date is null or m.end_date>getdate()) and ";
                    if (!string.IsNullOrEmpty(sMediaTypes) && !sMediaTypes.Equals("0"))
                    {
                        selectQuery += "and m.media_type_id in (" + sMediaTypes.Replace(";", ",") + ")";
                        selectQuery += "and";
                    }
                    selectQuery += "m.group_id ";
                    selectQuery += groupStr;
                    selectQuery += " and m.views <= (select max(views) from media where group_id ";
                    selectQuery += groupStr;
                    selectQuery += ") order by views desc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        oPlaylistSchema = null;
                        retVal.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true, ref oPlaylistSchema));
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        Int32 nEnd = nStartIndex + nNumOfItems;
                        if (nEnd > nCount)
                            nEnd = nCount;
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i = nStartIndex; i < nEnd; i++)
                        {
                            //Get media tag for each media
                            Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());

                            ApiObjects.PicObject[] thePics = null;
                            ApiObjects.MediaObject theMediaObj = null;

                            retVal.Append(ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, string.Empty, string.Empty, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, string.Empty, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref thePics, ref theMediaObj, false, false, false, ref theWSInfoStruct, nDeviceID));

                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    retVal.Append("</channel>");
                    retVal.Append("</response>");
                }
            }
            return retVal.ToString();
        }

        static public string PersonalLastWatchedProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID,
            bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj, ref ApiObjects.PageDefinition thePageDef,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            string sEndDateField = "m.";
            ApiObjects.PlayListSchema oPlaylistSchema = null;

            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nCo = 0;
            StringBuilder sRet = new StringBuilder();
            string watcherIDStr = string.Empty;

            string sParamsSiteGUID = "";

            if (theDoc != null)
            {
                XmlNode theParamsSiteGUID = theDoc.SelectSingleNode("/root/request/params/@site_guid");
                if (theParamsSiteGUID != null)
                    sParamsSiteGUID = theParamsSiteGUID.Value.ToUpper();
                if (sParamsSiteGUID != "")
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1.SetCachedSec(0);
                    selectQuery1 += "select wgd.WATCHER_ID,w.TVINCI_GUID from watchers_groups_data wgd (nolock),watchers (nolock) w where wgd.WATCHER_ID=w.id and wgd.GROUP_ID " + PageUtils.GetGroupsStrByParent(nGroupID);
                    //selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_ID", "=", nGroupID);
                    selectQuery1 += "and";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wgd.GROUP_GUID", "=", sParamsSiteGUID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < nCount; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(selectQuery1.Table("query").DefaultView[i].Row["WATCHER_ID"].ToString());
                        }
                        watcherIDStr = sb.ToString();
                        //if (nCount > 0)
                        //{
                        //    nWatcherID = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["WATCHER_ID"].ToString());
                        //    sTVinciGUID = selectQuery1.Table("query").DefaultView[0].Row["TVINCI_GUID"].ToString();

                        //}
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                }
                nCo = GetPersonalLastWatchedCount(nGroupID, sWPGID, watcherIDStr);
                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();



                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;

                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
                sRet.Append("<response type=\"personal_last_watched\">");
            }
            else
            {
                oPlaylistSchema = new ApiObjects.PlayListSchema();
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);
                nStartIndex = thePageDef.m_nStartIndex;
                nNumOfItems = thePageDef.m_nNumberOfItems;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);
            }


            List<KeyValuePair<Int32, MediaWatchedObj>> lMediaIDToDeviceUDID = GetPersonalLastWatched(nGroupID, sParamsSiteGUID);
            Int32 nCounter = lMediaIDToDeviceUDID.Count;

            if (nCounter > 0)
            {
                if (theDoc != null)
                {
                    sRet.Append("<channel id=\"\" media_count=\"").Append(nCounter).Append("\" >");

                }

                Int32 nEnd = nStartIndex + nNumOfItems;
                if (nEnd > nCounter)
                    nEnd = nCounter;
                for (int i = nStartIndex; i < nEnd; i++)
                {
                    KeyValuePair<Int32, MediaWatchedObj> kvp = lMediaIDToDeviceUDID[i];
                    Int32 nMediaID = kvp.Key;
                    MediaWatchedObj mwo = kvp.Value;
                    string sDeviceName = mwo.m_deviceName;
                    DateTime lastWatcedDate = mwo.m_lastWatchedDate;
                    if (theDoc != null)
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, false,
                            bWithFileTypes, nCountryID, nDeviceID, true, sDeviceName, lastWatcedDate, true));
                    else
                    {
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                        bool bStatistics = false;
                        bool bPersonal = false;
                        string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                        string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;
                        ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                        ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true,
                            sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct,
                            nDeviceID, true, sDeviceName);
                        if (theChannelObj.m_oMediaObjects == null)
                            theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                        else
                            theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                        theChannelObj.m_nChannelTotalSize = nCo;
                    }
                }

                if (theDoc != null)
                {
                    sRet.Append("</channel>");
                    sRet.Append("</response>");
                }
                return sRet.ToString();
            }




            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetLockTimeOut(10000);
            selectQuery.SetCachedSec(0);
            selectQuery.SetWritable(true);
            selectQuery += "select distinct top " + (nNumOfItems + nStartIndex).ToString() + " wma.media_id,max(wma.create_date) from watchers_media_actions wma (nolock),media m ";
            selectQuery += "  WITH (nolock) where ";
            selectQuery += " m.id=wma.media_id and m.status=1 and m.is_active=1 and ";

            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += " (COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id ", "=", nGroupID);
            selectQuery += "))";

            selectQuery += " and ";
            //selectQuery += " (m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + ">getdate())) and ";
            selectQuery += " (";
            selectQuery += "m.group_id " + PageUtils.GetGroupsStrByParent(nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")";
            selectQuery += " and wma.ACTION_ID=1 and ";
            selectQuery += "wma.WATCHER_ID in (" + watcherIDStr + ")";
            selectQuery += " group by wma.media_id order by max(wma.create_date) desc ";
            sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
            if (theDoc != null)
                sRet.Append("<channel id=\"\" media_count=\"").Append(nCo).Append("\" >");
            if (!string.IsNullOrEmpty(watcherIDStr))
            {
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    Int32 nEnd = nStartIndex + nNumOfItems;
                    if (nEnd > nCount)
                        nEnd = nCount;
                    for (int i = nStartIndex; i < nEnd; i++)
                    {
                        Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_ID"].ToString());
                        if (theDoc != null)
                            sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, false,
                                bWithFileTypes, nCountryID, nDeviceID));
                        else
                        {
                            Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                            string sFileFormat = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                                sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                            string sSubFileFormat = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                                sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                            string sFileQuality = "";
                            if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                                sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                            bool bStatistics = false;
                            bool bPersonal = false;
                            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                            if (sStatistics == "true")
                                bStatistics = true;
                            if (sPersonal == "true")
                                bPersonal = true;
                            ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                            ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                            if (theChannelObj.m_oMediaObjects == null)
                                theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                            else
                                theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                            theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                            theChannelObj.m_nChannelTotalSize = nCo;
                        }
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            if (theDoc != null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }
            return sRet.ToString();
        }

        static public string NowPlayingProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID,
            ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj,
            ref ApiObjects.PageDefinition thePageDef, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);

            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            StringBuilder sRet = new StringBuilder();
            ApiObjects.PlayListSchema oPlaylistSchema = null;
            if (initObj == null)
            {
                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());
                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());
                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();
                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
            }
            else
            {
                oPlaylistSchema = new ApiObjects.PlayListSchema();
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);
                nStartIndex = thePageDef.m_nStartIndex;
                nNumOfItems = thePageDef.m_nNumberOfItems;
                //theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
            }

            Int32 nCo = GetNowPlayingCount(nGroupID, sWPGID);



            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select distinct top " + (nNumOfItems + nStartIndex).ToString() + " media_id,count(*) as co from media_eoh m ";
            selectQuery.SetWritable(true);
            selectQuery += " WITH (nolock) where m.update_date>DATEADD(s,-30,getdate()) and ";
            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                //selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                //selectQuery += sWPGID;
                //selectQuery += ")";
            }
            selectQuery += ") group by media_id order by count(*) desc";

            if (initObj == null)
            {
                sRet.Append("<response type=\"now_playing\">");
                sRet.Append("<channel id=\"\" media_count=\"").Append(nCo).Append("\" >");
            }
            if (selectQuery.Execute("query", true) != null)
            {
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true, ref oPlaylistSchema));
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                Int32 nEnd = nStartIndex + nNumOfItems;
                if (nEnd > nCount)
                    nEnd = nCount;
                for (int i = nStartIndex; i < nEnd; i++)
                {
                    Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_ID"].ToString());
                    if (initObj == null)
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                    else
                    {
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                        bool bStatistics = false;
                        bool bPersonal = false;
                        string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                        string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;
                        ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                        ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                        if (theChannelObj.m_oMediaObjects == null)
                            theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                        else
                            theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                        theChannelObj.m_nChannelTotalSize = nCo;
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            if (initObj == null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }
            else
            {
                theChannelObj.m_nChannelTotalSize = nCo;
                theChannelObj.m_oPlayListSchema = oPlaylistSchema;
            }
            return sRet.ToString();
        }

        static public string MostViewdProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID,
            Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);

            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            StringBuilder sRet = new StringBuilder();

            string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
            string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");

            XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
            if (theStartIndex != null)
                nStartIndex = int.Parse(theStartIndex.Value.ToUpper());
            XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
            if (theNumOfItems != null)
                nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());
            string sWithInfo = "";
            XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
            if (theWithInfo != null)
                sWithInfo = theWithInfo.Value.ToUpper();

            string sHours = "";
            XmlNode theHours = theDoc.SelectSingleNode("/root/request/params/@hours");
            if (theHours != null)
                sHours = theHours.Value.ToUpper();

            string sActionType = "";
            XmlNode theActionType = theDoc.SelectSingleNode("/root/request/params/@action");
            if (theActionType != null)
                sActionType = theActionType.Value.ToUpper();

            string sWithFileTypes = "";
            XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
            if (theWithFileTypes != null)
                sWithFileTypes = theWithFileTypes.Value.ToUpper();
            theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
            if (sWithInfo.Trim().ToLower() == "true")
                bWithInfo = true;
            if (sWithFileTypes.Trim().ToLower() == "true")
                bWithFileTypes = true;

            Int32 nQualityID = GetFileQualityID(sFileQuality);
            Int32 nFormatID = GetFileTypeID(sFileFormat, nGroupID);
            bool bEOH = false;
            Int32 nActionID = GetActionValues(sActionType, ref bEOH);

            Int32 nCo = GetMostViewdCount(nGroupID, sWPGID, nFormatID, nQualityID, sHours, nActionID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top " + (nNumOfItems + nStartIndex).ToString() + " media_id,count(*) as co from watchers_media_actions wma (nolock),media m (nolock) ";
            selectQuery.SetWritable(true);
            selectQuery.SetCachedSec(0);
            selectQuery += " WHERE m.id=wma.media_id and (m.is_active=1 and m.status=1 and m.start_date < getdate() and (m.end_date is null or m.end_date>getdate())) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.FILE_FORMAT_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.FILE_QUALITY_ID", "=", nQualityID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.ACTION_ID", "=", nActionID);
            selectQuery += " and wma.CREATE_DATE > DATEADD(hh, - " + sHours + ", GETDATE())";
            selectQuery += " and ";
            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
            }
            selectQuery += ") group by media_id order by co desc";


            sRet.Append("<response type=\"most_viewd\">");
            sRet.Append("<channel id=\"\" media_count=\"").Append(nCo).Append("\" >");
            if (selectQuery.Execute("query", true) != null)
            {
                ApiObjects.PlayListSchema oPlaylistSchema = null;
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true, ref oPlaylistSchema));
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                Int32 nEnd = nStartIndex + nNumOfItems;
                if (nEnd > nCount)
                    nEnd = nCount;

                for (int i = nStartIndex; i < nEnd; i++)
                {
                    Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_ID"].ToString());
                    Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                    sRet.Append("<period_views count=\"" + nCO.ToString() + "\">");
                    sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                    sRet.Append("</period_views>");
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            sRet.Append("</channel>");
            sRet.Append("</response>");
            return sRet.ToString();
        }

        protected string GetMediaMetaTranslateLike(string sMetaFieldName, string sMetaFieldValue, Int32 nGroupID, bool bExact)
        {
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct media_id from media_translate m (nolock) where ";

            if (bExact == false)
            {
                selectQuery += " LTRIM(RTRIM(LOWER(" + sMetaFieldName + "))) ";
                selectQuery += " like (N'" + GetLikeStr(sMetaFieldValue.Trim().ToLower().Replace("'", "''")) + "')";
            }
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sMetaFieldName, "=", sMetaFieldValue.Trim());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sRet.Append("(");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["MEDIA_ID"]);
                }
                if (nCount > 0)
                    sRet.Append(")");
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        protected void GetMediaMetaTranslateLikeQuery(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sMetaFieldName, string sMetaFieldValue, Int32 nGroupID, bool bExact)
        {
            selectQuery += "select distinct media_id from media_translate m (nolock) where ";

            if (bExact == false)
            {
                selectQuery += " LTRIM(RTRIM(LOWER(" + sMetaFieldName + "))) ";
                selectQuery += " like (N'" + GetLikeStr(sMetaFieldValue.Trim().ToLower().Replace("'", "''")) + "')";
            }
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sMetaFieldName, "=", sMetaFieldValue.Trim());
        }
        static protected string GetSearchMediaInner(Int32 nNumOfItems, Int32 nStartIndex, string sEndDateField,
            Int32 nGroupID, Int32 nType, string sWPGID, string sName, bool bAnd, bool bExact,
            string sDescription, ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, string sOrderBy,
            string sPlaylistSchema, Int32 nFullCount, ref XmlDocument theDoc,
            Int32 nLangID, bool bIsLangMain, Int32 nWatcherID,
            bool bWithInfo, bool bWithCache, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin,
            string sMetaField, bool bWithFileTypes, Int32 nCountryID, string sMinDate, string sMaxDate,
            string sDocStruct, ref ApiObjects.InitializationObject theInitObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObject, Int32 nDeviceID)
        {

            return GetSearchMediaInner(nNumOfItems, nStartIndex, sEndDateField,
                nGroupID, nType, sWPGID, sName, bAnd, bExact,
                sDescription, ref theMetaList, ref theTagsList, sOrderBy,
                sPlaylistSchema, nFullCount, ref theDoc,
                nLangID, bIsLangMain, nWatcherID,
                bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin,
                sMetaField, bWithFileTypes, nCountryID, sMinDate, sMaxDate,
                sDocStruct, ref theInitObj,
                ref theWSInfoStruct, ref theChannelObject, nDeviceID, 0);
        }

        static protected string GetSearchMediaInner(Int32 nNumOfItems, Int32 nStartIndex, string sEndDateField,
            Int32 nGroupID, Int32 nType, string sWPGID, string sName, bool bAnd, bool bExact,
            string sDescription, ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, string sOrderBy,
            string sPlaylistSchema, Int32 nFullCount, ref XmlDocument theDoc,
            Int32 nLangID, bool bIsLangMain, Int32 nWatcherID,
            bool bWithInfo, bool bWithCache, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin,
            string sMetaField, bool bWithFileTypes, Int32 nCountryID, string sMinDate, string sMaxDate,
            string sDocStruct, ref ApiObjects.InitializationObject theInitObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObject, Int32 nDeviceID, Int32 nMediaID)
        {
            return GetSearchMediaInner(nNumOfItems, nStartIndex, sEndDateField,
               nGroupID, nType, sWPGID, sName, bAnd, bExact,
               sDescription, ref theMetaList, ref theTagsList, sOrderBy,
               sPlaylistSchema, nFullCount, ref theDoc,
               nLangID, bIsLangMain, nWatcherID,
               bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin,
               sMetaField, bWithFileTypes, nCountryID, sMinDate, sMaxDate,
               sDocStruct, ref theInitObj,
               ref theWSInfoStruct, ref theChannelObject, nDeviceID, nMediaID, true);
        }

        static protected string GetSearchMediaInner(Int32 nNumOfItems, Int32 nStartIndex, string sEndDateField,
    Int32 nGroupID, Int32 nType, string sWPGID, string sName, bool bAnd, bool bExact,
    string sDescription, ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, string sOrderBy,
    string sPlaylistSchema, Int32 nFullCount, ref XmlDocument theDoc,
    Int32 nLangID, bool bIsLangMain, Int32 nWatcherID,
    bool bWithInfo, bool bWithCache, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin,
    string sMetaField, bool bWithFileTypes, Int32 nCountryID, string sMinDate, string sMaxDate,
    string sDocStruct, ref ApiObjects.InitializationObject theInitObj,
    ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObject, Int32 nDeviceID, Int32 nMediaID, bool bUseStartDate)
        {
            return GetSearchMediaInner(nNumOfItems, nStartIndex, sEndDateField,
             nGroupID, nType, sWPGID, sName, bAnd, bExact,
             sDescription, ref theMetaList, ref theTagsList, sOrderBy,
             sPlaylistSchema, nFullCount, ref theDoc,
             nLangID, bIsLangMain, nWatcherID,
             bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin,
             sMetaField, bWithFileTypes, nCountryID, sMinDate, sMaxDate,
             sDocStruct, ref theInitObj,
             ref theWSInfoStruct, ref theChannelObject, nDeviceID, nMediaID, bUseStartDate, string.Empty);
        }

        static protected string GetSearchMediaInner(Int32 nNumOfItems, Int32 nStartIndex, string sEndDateField,
            Int32 nGroupID, Int32 nType, string sWPGID, string sName, bool bAnd, bool bExact,
            string sDescription, ref XmlNodeList theMetaList, ref XmlNodeList theTagsList, string sOrderBy,
            string sPlaylistSchema, Int32 nFullCount, ref XmlDocument theDoc,
            Int32 nLangID, bool bIsLangMain, Int32 nWatcherID,
            bool bWithInfo, bool bWithCache, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin,
            string sMetaField, bool bWithFileTypes, Int32 nCountryID, string sMinDate, string sMaxDate,
            string sDocStruct, ref ApiObjects.InitializationObject theInitObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObject, Int32 nDeviceID, Int32 nMediaID, bool bUseStartDate, string udid)
        {
            if (nGroupID == 0)
                nGroupID = LoginManager.GetLoginGroupID();

            bool bIPAllowed = TVinciShared.ProtocolsFuncs.DoesCallerPermittedIP(nGroupID);
            string sCountryCD = PageUtils.GetIPCountry2().ToString();
            string sPicSizeForCache = TVinciShared.ProtocolsFuncs.GetPicSizeForCache(ref theDoc);
            string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
            string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
            string sInfoSigStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theInfoStruct, false);


            if (sPersonal == "false" && CachingManager.CachingManager.Exist("GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString()) == true && bWithCache == true && theChannelObject == null)
                return CachingManager.CachingManager.GetCachedData("GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString()).ToString();
            else if (sPersonal == "false" && CachingManager.CachingManager.Exist("ws.GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString()) == true && bWithCache == true && theChannelObject != null)
            {
                theChannelObject = (ApiObjects.ChannelObject)(CachingManager.CachingManager.GetCachedData("ws.GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString()));
                return "";
            }


            string sRelevanceQuery = "select media_id,sum(relevance) as co into #tmp2 from (";
            bool bFirstRelevance = true;
            //string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            string sGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            StringBuilder sRet = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(86400);
            if (sOrderBy != " order by q1.co desc ")
                selectQuery += "select top " + (nNumOfItems + nStartIndex).ToString() + " q.id from (";
            else
                selectQuery += "select  q.id into #tmp1 from (";
            selectQuery += "select (CASE m.VOTES_COUNT WHEN 0 THEN 0 ELSE (m.VOTES_SUM/m.VOTES_COUNT) END) as rate,m.views,m.id,m.start_date,";
            //selectQuery += "select m.id,m.start_date,m.name,m.description ";
            if (bIsLangMain == true)
                selectQuery += "m.name,m.description ";
            else
                selectQuery += "mt.name,mt.description ";
            if (sMetaField != "")
                selectQuery += ", " + sMetaField;
            selectQuery += " from media m (nolock) ";
            if (bIsLangMain == false)
                selectQuery += ",media_translate mt (nolock) ";
            selectQuery += "  where ";

            if (udid != string.Empty && udid != null)
            {
                List<int> ruleIDs = GetDeviceAllowedRuleIDs(udid, nGroupID);
                if (ruleIDs.Count > 0)
                {
                    selectQuery += "( m.device_rule_id in (";
                    for (int i = 0; i < ruleIDs.Count; i++)
                    {
                        selectQuery += ruleIDs[i];
                        if (i < ruleIDs.Count - 1)
                        {
                            selectQuery += ",";
                        }
                    }
                    selectQuery += ") OR m.device_rule_id IS NULL)";
                    selectQuery += " and ";
                }
            }

            if (bIsLangMain == false)
            {
                selectQuery += "mt.media_id=m.id and mt.name is not null and mt.name<>'' and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.LANGUAGE_ID", "=", nLangID);
                selectQuery += " and ";
            }

            if (nMediaID != 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "<>", nMediaID);
                selectQuery += " and ";
            }

            selectQuery += " (m.id not in (select id from media (nolock) where ";
            if (bUseStartDate)
            {
                selectQuery += "(start_date>getdate() or " + sEndDateField + "<getdate())";
            }
            else
            {
                selectQuery += sEndDateField + "<getdate()";
            }

            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and group_id " + PageUtils.GetAllGroupTreeStr(nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += " (COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "group_id " + sGroups;
            selectQuery += "))";

            selectQuery += " and m.status=1 and m.is_active=1 and ";
            //selectQuery += "m.start_date<getdate() and (" + sEndDateField + " is null or " + sEndDateField + "> getdate()) and m.status=1 and m.is_active=1 and ";

            if (sMinDate.Trim() != "")
            {
                DateTime dMinDate = DateUtils.GetDateFromStr(sMinDate);
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.publish_date", ">=", dMinDate);
                selectQuery += " or (m.publish_date is null and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.create_date", ">=", dMinDate);
                selectQuery += "))";
                selectQuery += " and ";
            }

            if (sMaxDate.Trim() != "")
            {
                DateTime dMaxDate = DateUtils.GetDateFromStr(sMaxDate);
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.publish_date", "<=", dMaxDate);
                selectQuery += " or (m.publish_date is null and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.create_date", "<=", dMaxDate);
                selectQuery += "))";
                selectQuery += " and ";
            }

            if (nType != 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_TYPE_ID", "=", nType);
                selectQuery += " and ";
            }

            selectQuery += " (";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")";

            bool bFirst = true;
            bool bInsideFirst = true;

            if (sName != "")
            {
                if (bFirstRelevance == false)
                    sRelevanceQuery += "  ";
                bFirstRelevance = false;
                string[] sep = { "||" };
                string[] sNames = sName.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                string sValues = " in (";
                string sValuesLike = "(";
                for (int j = 0; j < sNames.Length; j++)
                {
                    if (j > 0)
                    {
                        sValues += ",";
                        sValuesLike += " or ";
                    }
                    sValues += "N'" + sNames[j].Replace("'", "''") + "'";
                    sValuesLike += " value like (N'" + GetLikeStr(sNames[j].Trim().Replace("'", "''")) + "') ";
                }
                sValues += ") ";
                sValuesLike += ") ";
                sRelevanceQuery += "select media_id,20 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and value " + sValues + " and group_id " + sGroups;
                sRelevanceQuery += " UNION ALL ";
                sRelevanceQuery += "select media_id,4 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and " + sValuesLike + " and group_id " + sGroups;
                if (bFirst == true)
                {
                    selectQuery += " and (";
                    bFirst = false;
                }
                if (bInsideFirst == false)
                {
                    if (bAnd == false)
                        selectQuery += " or ";
                    else
                        selectQuery += " and ";
                }
                else
                    bInsideFirst = false;
                selectQuery += "(";
                if (bExact == false)
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and " + sValuesLike;
                    if (sGroups != "")
                        selectQuery += "and group_id " + sGroups;
                    selectQuery += ") ";
                }
                else
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=1 and MEDIA_TEXT_TYPE_NUM=0 and value " + sValues;
                    //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sName);
                    if (sGroups != "")
                        selectQuery += " and group_id " + sGroups;
                    selectQuery += ") ";
                }
                selectQuery += ")";
            }

            if (sDescription != "")
            {
                if (bFirstRelevance == false)
                    sRelevanceQuery += " UNION ";
                bFirstRelevance = false;
                sRelevanceQuery += "select media_id,15 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and value=N'" + sDescription.Replace("'", "''") + "' and group_id " + sGroups;
                sRelevanceQuery += " UNION ALL ";
                sRelevanceQuery += "select media_id,3 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and value like (N'" + GetLikeStr(sDescription.Trim().Replace("'", "''")) + "') and group_id " + sGroups;
                if (bFirst == true)
                {
                    selectQuery += " and (";
                    bFirst = false;
                }
                if (bInsideFirst == false)
                {
                    if (bAnd == false)
                        selectQuery += " or ";
                    else
                        selectQuery += " and ";
                }
                else
                    bInsideFirst = false;
                selectQuery += "(";
                if (bExact == false)
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and value like (N'" + GetLikeStr(sDescription.Trim().ToLower().Replace("'", "''")) + "')";
                    if (sGroups != "")
                        selectQuery += "and group_id " + sGroups;
                    selectQuery += ") ";
                }
                else
                {
                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=2 and MEDIA_TEXT_TYPE_NUM=0 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sDescription);
                    if (sGroups != "")
                        selectQuery += " and group_id " + sGroups;
                    selectQuery += ") ";
                }
                selectQuery += " ) ";
            }

            for (int i = 0; i < theMetaList.Count; i++)
            {
                XmlNode theMetaName = theMetaList[i].SelectSingleNode("@name");
                XmlNode theMetaValue = theMetaList[i].SelectSingleNode("@value");
                XmlNode theMetaMinValue = theMetaList[i].SelectSingleNode("@min_value");
                XmlNode theMetaMaxValue = theMetaList[i].SelectSingleNode("@max_value");

                string sMetaName = "";
                string sMetaValue = "";
                string sMetaMinValue = "";
                string sMetaMaxValue = "";

                if (theMetaName != null)
                    sMetaName = theMetaName.Value;
                if (theMetaValue != null)
                    sMetaValue = theMetaValue.Value;
                if (theMetaMinValue != null)
                    sMetaMinValue = theMetaMinValue.Value;
                if (theMetaMaxValue != null)
                    sMetaMaxValue = theMetaMaxValue.Value;


                if (sMetaName != "" && (sMetaValue != "" || sMetaMinValue != "" || sMetaMaxValue != ""))
                {
                    Int32 nJ = 0;
                    Int32 nMediaTextTypeID = 0;
                    string sField = "m." + GetMetaFieldByName(sMetaName, nGroupID, ref nJ, ref nMediaTextTypeID);
                    if (sField.Trim() != "m.")
                    {
                        if (!(sField.EndsWith("_STR") == true && sMetaValue.Trim() == ""))
                        {
                            if (bFirst == true)
                            {
                                selectQuery += " and (";
                                bFirst = false;
                            }
                            if (bInsideFirst == false)
                            {
                                if (bAnd == false)
                                    selectQuery += " or ";
                                else
                                    selectQuery += " and ";
                            }
                            else
                                bInsideFirst = false;
                            selectQuery += " ( ";
                            if (sField.EndsWith("_STR") == true)// || sField.EndsWith("_DOUBLE"))
                            {
                                if (bFirstRelevance == false)
                                    sRelevanceQuery += " UNION ALL ";
                                bFirstRelevance = false;
                                if (sMetaValue.Trim() != "")
                                    sRelevanceQuery += "select media_id,15 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=" + nMediaTextTypeID.ToString() + " and MEDIA_TEXT_TYPE_NUM=" + nJ.ToString() + " and value=N'" + sMetaValue.Replace("'", "''") + "' and group_id " + sGroups;
                                else if (sField.EndsWith("_DOUBLE") == true)
                                {
                                    sRelevanceQuery += "select media_id,15 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=" + nMediaTextTypeID.ToString() + " and MEDIA_TEXT_TYPE_NUM=" + nJ.ToString() + "  and group_id " + sGroups;
                                    if (sMetaMaxValue.Trim() != "")
                                        sRelevanceQuery += " and value<= " + sMetaMaxValue;
                                    if (sMetaMinValue.Trim() != "")
                                        sRelevanceQuery += " and value>= " + sMetaMinValue;
                                }
                                if (sField.EndsWith("_STR") == true)
                                {
                                    sRelevanceQuery += " UNION ALL ";
                                    sRelevanceQuery += "select media_id,3 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=" + nMediaTextTypeID.ToString() + " and MEDIA_TEXT_TYPE_NUM=" + nJ.ToString() + " and value like (N'" + GetLikeStr(sMetaValue.Trim().Replace("'", "''")) + "')  and group_id " + sGroups;
                                }
                                if (bExact == false && sField.EndsWith("_STR") == true)
                                {
                                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nJ);
                                    selectQuery += " and ";
                                    selectQuery += " value like (N'" + GetLikeStr(sMetaValue.Trim().Replace("'", "''")) + "')";
                                    if (sGroups != "")
                                        selectQuery += "and group_id " + sGroups;
                                    selectQuery += ") ";
                                }
                                else
                                {
                                    selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_ID", "=", nMediaTextTypeID);
                                    selectQuery += " and ";
                                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TEXT_TYPE_NUM", "=", nJ);
                                    if (sMetaValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sMetaValue.Trim());
                                    }
                                    if (sMetaMinValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", ">=", int.Parse(sMetaMinValue));
                                    }
                                    if (sMetaMaxValue.Trim() != "")
                                    {
                                        selectQuery += " and ";
                                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "<=", int.Parse(sMetaMaxValue));
                                    }
                                    if (sGroups != "")
                                        selectQuery += " and group_id " + sGroups;
                                    selectQuery += ") ";
                                }
                            }
                            else
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(" + sField + ")))", "=", double.Parse(sMetaValue.Trim().ToLower()));
                            selectQuery += " ) ";
                        }
                    }
                }
            }

            string sAndTagQuery = string.Empty;
            if (theTagsList.Count > 0)
            {
                StringBuilder sbAndTagQuery = new StringBuilder();
                StringBuilder sbOrTagQuery = new StringBuilder();
                bool bInFirstAndTag = true;

                for (int i = 0; i < theTagsList.Count; i++)
                {
                    XmlNode theTagName = theTagsList[i].SelectSingleNode("@name");
                    XmlNode theTagValue = theTagsList[i].SelectSingleNode("@value");
                    XmlNode theTagMust = theTagsList[i].SelectSingleNode("@cut_with");
                    string sTagName = "";
                    string sTagValue = "";
                    bool bTagMust = false;
                    if (theTagName != null)
                        sTagName = theTagName.Value;
                    if (theTagValue != null)
                        sTagValue = theTagValue.Value;

                    if (theTagMust != null && theTagMust.Value.ToLower().Equals("and"))
                        bTagMust = true;

                    if (sTagName != "" && sTagValue != "")
                    {
                        Int32 nOrderNum = 0;
                        //Int32 nTagTypeID = GetTagIDByName(sTagName, nGroupID , ref nOrderNum);
                        string sTags = GetTagsIDsByName(sTagName, nGroupID, ref nOrderNum);

                        if (bFirstRelevance == false)
                            sRelevanceQuery += " UNION ";
                        bFirstRelevance = false;
                        //sRelevanceQuery += "select media_id," + nOrderNum.ToString() + " as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and value=N'" + sTagValue.Replace("'", "''") + "'  and group_id " + sGroups;
                        sRelevanceQuery += "select media_id," + nOrderNum.ToString() + " as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM " + sTags + " and value=N'" + sTagValue.Replace("'", "''") + "'  and group_id " + sGroups;
                        //sRelevanceQuery += " UNION ALL ";
                        //sRelevanceQuery += "select media_id,2 as relevance from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') ";


                        if (bTagMust)
                        {
                            if (bInFirstAndTag)
                            {
                                bInFirstAndTag = false;
                            }
                            else
                            {
                                sbAndTagQuery.Append(" and ");
                            }
                        }
                        else
                        {
                            if (bFirst == true)
                            {
                                selectQuery += " and ( ";
                                bFirst = false;
                            }

                            if (bInsideFirst == false)
                            {
                                if (bAnd == false)
                                    sbOrTagQuery.Append(" or ");
                                else
                                    sbOrTagQuery.Append("and ");
                            }
                            else
                                bInsideFirst = false;
                        }

                        StringBuilder sbTagTemp = new StringBuilder(" (");
                        if (bExact == false)
                        {
                            //selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and value like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "')";
                            sbTagTemp.Append("m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM " + sTags + " and (value like (N'" + GetLikeStr(sTagValue.Trim().Replace("'", "''")) + "') or value like (N'" + GetLikeStr(sTagValue.Trim().ToLower().Replace("'", "''")) + "'))");


                        }
                        else
                        {
                            //selectQuery += "m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM=" + nTagTypeID.ToString() + " and ";
                            sbTagTemp.Append("m.id in (select media_id from media_values (nolock) where status=1 and MEDIA_TEXT_TYPE_ID=7 and MEDIA_TEXT_TYPE_NUM " + sTags + " and ");
                            sbTagTemp.Append("value like (N'" + sTagValue.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "')");
                        }

                        if (sGroups != "")
                            sbTagTemp.Append(" and group_id " + sGroups);
                        sbTagTemp.Append(")  ) ");



                        if (bTagMust)
                        {
                            sbAndTagQuery.Append(sbTagTemp.ToString());
                        }
                        else
                        {
                            sbOrTagQuery.Append(sbTagTemp.ToString());
                        }
                    }
                }


                string sOrTagQuery = sbOrTagQuery.ToString();
                sAndTagQuery = sbAndTagQuery.ToString();

                if (!string.IsNullOrEmpty(sOrTagQuery))
                {
                    selectQuery += sOrTagQuery;
                }
            }

            if (bFirst == false)
            {
                selectQuery += ")";
            }

            if (!string.IsNullOrEmpty(sAndTagQuery))
            {
                selectQuery += " and ( ";
                selectQuery += sAndTagQuery;
                selectQuery += " ) ";
            }

            selectQuery += ")q ";
            sRelevanceQuery += ")q2 group by q2.media_id ";
            if (sRelevanceQuery.StartsWith("select media_id,sum(relevance) as co into #tmp2 from ()q2 group by q2.media_id ") == false && sOrderBy == " order by q1.co desc ")
            {
                //selectQuery += " left join (" + sRelevanceQuery + ")q1 on q1.media_id=q.id where q1.co is not null ";
                selectQuery += " ; " + sRelevanceQuery + "; select #tmp1.id from #tmp1 left join #tmp2 on #tmp2.media_id = #tmp1.id where #tmp2.co is not null order by #tmp2.co desc ";
                //selectQuery += " , (" + sRelevanceQuery + ")q1 where q1.media_id=q.id and q1.co is not null ";
                //selectQuery += sOrderBy;
            }
            else if (sOrderBy == " order by q1.co desc ")
            {
                selectQuery += " ; select * from #tmp1 ";
            }
            if (sOrderBy != " order by q1.co desc ")
                selectQuery += sOrderBy;
            if (theChannelObject == null)
                sRet.Append("<channel id=\"\" media_count=\"").Append(nFullCount).Append("\" >");
            if (selectQuery.Execute("query", true) != null)
            {
                if (theChannelObject == null)
                    sRet.Append(sPlaylistSchema);
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                Int32 nEnd = nStartIndex + nNumOfItems;
                if (nEnd > nCount)
                    nEnd = nCount;

                for (int i = nStartIndex; i < nEnd; i++)
                {
                    Int32 nMID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    if (theChannelObject == null)
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID, false, string.Empty, DateTime.MaxValue, bUseStartDate));
                    else
                    {
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (theInitObj.m_oFileRequestObjects != null && theInitObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = theInitObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (theInitObj.m_oFileRequestObjects != null && theInitObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = theInitObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (theInitObj.m_oFileRequestObjects != null && theInitObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = theInitObj.m_oFileRequestObjects[0].m_sFileQuality;
                        bool bStatistics = false;
                        bool bPersonal = false;
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;
                        ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                        ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref theInitObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                        if (theChannelObject.m_oMediaObjects == null)
                            theChannelObject.m_oMediaObjects = new ApiObjects.MediaObject[1];
                        else
                            theChannelObject.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObject.m_oMediaObjects, theChannelObject.m_oMediaObjects.Length + 1));
                        theChannelObject.m_oMediaObjects[theChannelObject.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                        theChannelObject.m_oMediaObjects[theChannelObject.m_oMediaObjects.Length - 1] = theMediaObj;

                    }

                }
            }

            selectQuery.Finish();
            selectQuery = null;
            if (theChannelObject == null)
                sRet.Append("</channel>");

            // Distinct tags

            if (sPersonal == "false" && theChannelObject == null)
                CachingManager.CachingManager.SetCachedData("GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString() + "_" + bUseStartDate.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            if (sPersonal == "false" && theChannelObject != null)
                CachingManager.CachingManager.SetCachedData("ws.GetSearchMediaInner_" + sCountryCD + "_" + sDocStruct + "_" + bIPAllowed.ToString() + "_" + nLangID.ToString() + "_" + sStatistics + "_" + sMinDate + "_" + sMaxDate + "_" + nDeviceID.ToString(), sRet.ToString() + "_" + bUseStartDate.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);

            return sRet.ToString();
        }




        static protected string GetOrderByString(ref XmlDocument theDoc, bool bIsLangMain, ref string sMetaFieldQuery, Int32 nGroupID)
        {
            SortedList theOrderBy = new SortedList();
            string sOrderBy = "";
            XmlNode theOrderRandomVal = theDoc.SelectSingleNode("/root/request/search_data/order_values/random/@value");
            string sOrderRandomVal = "";
            if (theOrderRandomVal != null)
                sOrderRandomVal = theOrderRandomVal.Value.ToLower().Trim();
            if (sOrderRandomVal == "")
            {
                XmlNode theOrderNameDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/name/@order_dir");
                XmlNode theOrderNameNum = theDoc.SelectSingleNode("/root/request/search_data/order_values/name/@order_num");
                Int32 nNameOrderNum = 0;
                string sOrderNameDir = "";
                if (theOrderNameDir != null)
                    sOrderNameDir = theOrderNameDir.Value;
                if (theOrderNameNum != null)
                    nNameOrderNum = int.Parse(theOrderNameNum.Value);
                if (theOrderNameDir != null)
                    theOrderBy[nNameOrderNum] = "q.name " + sOrderNameDir;

                XmlNode theOrderDescDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/description/@order_dir");
                XmlNode theOrderDescNum = theDoc.SelectSingleNode("/root/request/search_data/order_values/description/@order_num");
                Int32 nDescOrderNum = 0;
                string sOrderDescDir = "";
                if (theOrderDescDir != null)
                    sOrderDescDir = theOrderDescDir.Value;
                if (theOrderDescNum != null)
                    nDescOrderNum = int.Parse(theOrderDescNum.Value);
                if (theOrderDescDir != null)
                    theOrderBy[nDescOrderNum] = "q.description " + sOrderDescDir;

                XmlNode theOrderDateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/date/@order_dir");
                XmlNode theOrderDateNum = theDoc.SelectSingleNode("/root/request/search_data/order_values/date/@order_num");
                Int32 nDateOrderNum = 0;
                string sOrderDateDir = "";
                if (theOrderDateDir != null)
                    sOrderDateDir = theOrderDateDir.Value;
                if (theOrderDateNum != null)
                    nDateOrderNum = int.Parse(theOrderDateNum.Value);
                if (theOrderDateDir != null)
                    theOrderBy[nDateOrderNum] = "q.start_date " + sOrderDateDir;

                XmlNode theOrderViewsDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/views/@order_dir");
                XmlNode theOrderViewsNum = theDoc.SelectSingleNode("/root/request/search_data/order_values/views/@order_num");
                Int32 nViewsOrderNum = 0;
                string sOrderViewsDir = "";
                if (theOrderViewsDir != null)
                    sOrderViewsDir = theOrderViewsDir.Value;
                if (theOrderViewsNum != null)
                    nViewsOrderNum = int.Parse(theOrderViewsNum.Value);
                if (theOrderViewsDir != null)
                    theOrderBy[nViewsOrderNum] = "q.views " + sOrderViewsDir;

                XmlNode theOrderRateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/rate/@order_dir");
                XmlNode theOrderRateNum = theDoc.SelectSingleNode("/root/request/search_data/order_values/rate/@order_num");
                Int32 nRateOrderNum = 0;
                string sOrderRateDir = "";
                if (theOrderRateDir != null)
                    sOrderRateDir = theOrderRateDir.Value;
                if (theOrderRateNum != null)
                    nRateOrderNum = int.Parse(theOrderRateNum.Value);
                if (theOrderRateDir != null)
                    theOrderBy[nRateOrderNum] = "q.rate " + sOrderRateDir;

                XmlNodeList theOrderMetaList = theDoc.SelectNodes("/root/request/search_data/order_values/meta");
                IEnumerator iterMeta = theOrderMetaList.GetEnumerator();
                string sMetaField = "";
                while (iterMeta.MoveNext())
                {

                    XmlNode theMeta = (XmlNode)(iterMeta.Current);

                    XmlNode theOrderMetaDir = theMeta.SelectSingleNode("@order_dir");
                    XmlNode theOrderMetaNum = theMeta.SelectSingleNode("@order_num");
                    XmlNode theOrderMetaName = theMeta.SelectSingleNode("@name");
                    Int32 nMetaOrderNum = 0;
                    string sOrderMetaDir = "";
                    string sOrderMetaName = "";
                    if (theOrderMetaDir != null)
                        sOrderMetaDir = theOrderMetaDir.Value;
                    if (theOrderMetaNum != null)
                        nMetaOrderNum = int.Parse(theOrderMetaNum.Value);
                    if (theOrderMetaName != null)
                        sOrderMetaName = theOrderMetaName.Value;

                    if (sOrderMetaName != "")
                    {

                        Int32 nStrID = PageUtils.GetStringMetaIDByMetaName(nGroupID, sOrderMetaName);
                        if (nStrID != 0)
                            sMetaField = "META" + nStrID.ToString() + "_STR";
                        else
                        {
                            Int32 nDoubleID = PageUtils.GetDoubleMetaIDByMetaName(nGroupID, sOrderMetaName);
                            if (nDoubleID != 0)
                                sMetaField = "META" + nDoubleID.ToString() + "_DOUBLE";
                            else
                            {
                                Int32 nBoolID = PageUtils.GetBoolMetaIDByMetaName(nGroupID, sOrderMetaName);
                                if (nBoolID != 0)
                                    sMetaField = "META" + nBoolID.ToString() + "_BOOL";
                            }

                        }
                        theOrderBy[nMetaOrderNum] = "q." + sMetaField + " " + sOrderMetaDir;
                        if (sMetaFieldQuery != "")
                            sMetaFieldQuery += ",";
                        if (bIsLangMain == true || nStrID == 0)
                            sMetaFieldQuery += "m.";
                        else
                            sMetaFieldQuery += "mt.";
                        sMetaFieldQuery += sMetaField;
                    }
                }
                sOrderBy = GetOrderByFromSort(ref theOrderBy);
            }
            return sOrderBy;
        }

        static protected string GetOrderByString(ref ApiObjects.SearchOrderByObject[] theSearchObjects, bool bIsLangMain, ref string sMetaFieldQuery, Int32 nGroupID)
        {
            string sOrderBy = "";
            SortedList theOrderBy = new SortedList();

            Int32 nCount = 0;
            if (theSearchObjects != null)
                nCount = theSearchObjects.Length;
            for (int i = 0; i < nCount; i++)
            {
                if (theSearchObjects[i] == null)
                    continue;
                string sOrderNameDir = "";
                ApiObjects.OrderDiretion eOrderBy = theSearchObjects[i].m_eOrderBy;
                if (eOrderBy == ApiObjects.OrderDiretion.Asc)
                    sOrderNameDir = "asc";
                if (eOrderBy == ApiObjects.OrderDiretion.Desc)
                    sOrderNameDir = "desc";
                Int32 nOrderNum = theSearchObjects[i].m_nOrderNum;
                string sOrderField = theSearchObjects[i].m_sOrderField;
                if (sOrderField == "random")
                {
                    sMetaFieldQuery = "";
                    return "";
                }
                else if (sOrderField == "name")
                    theOrderBy[nOrderNum] = "q.name " + sOrderNameDir;
                else if (sOrderField == "description")
                    theOrderBy[nOrderNum] = "q.description " + sOrderNameDir;
                else if (sOrderField == "date")
                    theOrderBy[nOrderNum] = "q.start_date " + sOrderNameDir;
                else if (sOrderField == "views")
                    theOrderBy[nOrderNum] = "q.views " + sOrderNameDir;
                else if (sOrderField == "rate")
                    theOrderBy[nOrderNum] = "q.rate " + sOrderNameDir;
                else
                {
                    Int32 nJ = 0;
                    Int32 nMediaTextTypeID = 0;
                    string sMetaFieldName = GetMetaFieldByName(sOrderField, nGroupID, ref nJ, ref nMediaTextTypeID);
                    if (sMetaFieldName != "")
                    {
                        theOrderBy[nOrderNum] = "q." + sMetaFieldName + " " + sOrderNameDir;
                        if (sMetaFieldQuery != "")
                            sMetaFieldQuery += ",";
                        if (bIsLangMain == true)
                            sMetaFieldQuery += "m.";
                        else
                            sMetaFieldQuery += "mt.";
                        sMetaFieldQuery += sMetaFieldName;
                    }
                }
            }
            sOrderBy = GetOrderByFromSort(ref theOrderBy);
            return sOrderBy;
        }

        static protected string GetOrderByString(ref ApiObjects.SearchDefinitionObject theSearchObject, bool bIsLangMain, ref string sMetaFieldQuery, Int32 nGroupID)
        {
            return GetOrderByString(ref theSearchObject.m_sOrderByObjects, bIsLangMain, ref sMetaFieldQuery, nGroupID);
        }

        static public string GetOrderByFromSort(ref SortedList theOrderBy)
        {
            string sOrderBy = "";
            IDictionaryEnumerator iter = theOrderBy.GetEnumerator();
            bool bOrderFirst = true;
            while (iter.MoveNext())
            {
                if (bOrderFirst == true)
                {
                    sOrderBy += " order by ";
                    bOrderFirst = false;
                }
                else
                    sOrderBy += ",";
                sOrderBy += iter.Value.ToString();
            }
            return sOrderBy;
        }

        static public string SearchMediaProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bWithCache, bool bRelated,
            bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.SearchDefinitionObject theSearchCriteria, ref ApiObjects.MediaInfoStructObject theWSInfoStruct,
            ref ApiObjects.ChannelObject theChannelObj, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);

            bool bAnd = false;
            bool bExact = false;
            string sDocStruct = "";
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nStartIndex = 0;
            Int32 nNumOfItems = 0;
            string sName = "";
            string sMinDate = "";
            string sMaxDate = "";
            string sDescription = "";
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            string sMediaTypes = string.Empty;
            XmlNode theInfoStruct = null;
            XmlNodeList theTagsList = null;
            XmlNodeList theMetaList = null;
            string sEndDateField = "m.";
            string sOrderBy = "";
            string sMetaFieldQuery = "";


            bool bUseStartDate = true;
            string sUseStartDate = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "use_start_date");
            if (sUseStartDate == "false")
            {
                bUseStartDate = false;
            }

            ApiObjects.PlayListSchema thePlaylistSchema = null;
            if (theSearchCriteria == null)
            {
                string sAnd = "";
                XmlNode theAnd = theDoc.SelectSingleNode("/root/request/search_data/@cut_with");
                if (theAnd != null)
                    sAnd = theAnd.Value.ToUpper();
                if (sAnd.ToLower().Trim() == "and")
                    bAnd = true;
                else
                    bAnd = false;

                string sExact = "";
                XmlNode theExact = theDoc.SelectSingleNode("/root/request/search_data/cut_values/@exact");
                if (theExact != null)
                    sExact = theExact.Value.ToLower().Trim();

                if (sExact == "true")
                    bExact = true;
                sDocStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theDoc, true);

                XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/search_data/channel/@start_index");
                if (theStartIndex != null)
                    nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

                XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/search_data/channel/@media_count");
                if (theNumOfItems != null)
                    nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());

                XmlNode theName = theDoc.SelectSingleNode("/root/request/search_data/cut_values/name/@value");
                if (theName != null)
                    sName = theName.Value;

                XmlNode theMinDate = theDoc.SelectSingleNode("/root/request/search_data/cut_values/date/@min_value");
                if (theMinDate != null)
                    sMinDate = theMinDate.Value;

                XmlNode theMaxDate = theDoc.SelectSingleNode("/root/request/search_data/cut_values/date/@max_value");
                if (theMaxDate != null)
                    sMaxDate = theMaxDate.Value;

                XmlNode theDesc = theDoc.SelectSingleNode("/root/request/search_data/cut_values/description/@value");
                if (theDesc != null)
                    sDescription = theDesc.Value;

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;

                XmlNode theType = theDoc.SelectSingleNode("/root/request/search_data/cut_values/type/@value");
                if (theType != null)
                    sMediaTypes = theType.Value;


                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache);
                theTagsList = theDoc.SelectNodes("/root/request/search_data/cut_values/tags/tag_type");
                theMetaList = theDoc.SelectNodes("/root/request/search_data/cut_values/meta");

                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);

                //sOrderBy = GetOrderByString(ref theDoc, bIsLangMain, ref sMetaFieldQuery, nGroupID);
            }
            else
            {
                theChannelObj = new ApiObjects.ChannelObject();
                thePlaylistSchema = new ApiObjects.PlayListSchema();
                if (theSearchCriteria.m_eAndOr == ApiObjects.AndOr.And)
                    bAnd = true;
                else
                    bAnd = false;

                bExact = theSearchCriteria.m_bExact;
                nStartIndex = theSearchCriteria.m_oPageDefinition.m_nStartIndex;
                nNumOfItems = theSearchCriteria.m_oPageDefinition.m_nNumberOfItems;
                sName = theSearchCriteria.m_sTitle;
                if (theSearchCriteria.m_dMinDate.Year < 2099 && theSearchCriteria.m_dMinDate.Year > 1900)
                    sMinDate = DateUtils.GetStrFromDate(theSearchCriteria.m_dMinDate);

                if (theSearchCriteria.m_dMaxDate.Year < 2099 && theSearchCriteria.m_dMaxDate.Year > 1900)
                    sMaxDate = DateUtils.GetStrFromDate(theSearchCriteria.m_dMaxDate);
                sDescription = theSearchCriteria.m_sDescription;
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                //if (theSearchCriteria.m_sTypeName != "")
                //    int nType = GetFileTypeID(theSearchCriteria.m_sTypeName, nGroupID);
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, bWithCache, ref theWSInfoStruct);
                theDoc = ConvertSearchStructToInfoXML(ref theSearchCriteria);
                theTagsList = theDoc.SelectNodes("/root/request/search_data/cut_values/tags/tag_type");
                theMetaList = theDoc.SelectNodes("/root/request/search_data/cut_values/meta");
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);

                sOrderBy = GetOrderByString(ref theSearchCriteria, bIsLangMain, ref sMetaFieldQuery, nGroupID);
            }
            string sDeviceID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "device_udid");

            int[] deviceRules = GetDeviceAllowedRuleIDs(sDeviceID, nGroupID).ToArray();
            //Int32 nCo = GetTotalSearchSize(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, bAnd, bRelated, nLangID, bIsLangMain, bWithCache, sDocStruct,
            //    ref theSearchCriteria, ref initObj, nCountryID, nDeviceID, bUseStartDate, sDeviceID);

            StringBuilder sRet = new StringBuilder();
            if (theSearchCriteria == null)
                sRet.Append("<response type=\"search_media\">");

            string sPlaylistSchema = ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, bWithCache, ref thePlaylistSchema);
            if (sOrderBy == "")
                sOrderBy = " order by q1.co desc ";

            sRet.Append(GetSearchMediaWithLucene(nStartIndex, nNumOfItems, 0, nGroupID, sMediaTypes, sName, bAnd, bExact, sDescription, ref  theMetaList, ref  theTagsList,
                    sPlaylistSchema, ref  theDoc, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref  theInfoStruct, bIsAdmin,
                    bWithFileTypes, nCountryID, sMinDate, sMaxDate, sDocStruct, ref  theWSInfoStruct, nDeviceID, bUseStartDate, deviceRules));

            if (theSearchCriteria == null)
                sRet.Append("</response>");
            else
            {
                theChannelObj.m_oPlayListSchema = thePlaylistSchema;
                theChannelObj.m_nChannelTotalSize = 0; //TODO 
            }
            return sRet.ToString();
        }





        static protected string GetDateStr(DateTime dDate)
        {
            return dDate.ToString("HH:mm | dd.MM.yyyy");
            /*
            string sDate = "";
            if (dDate.Hour < 10)
                sDate = "0";
            sDate += dDate.Hour.ToString() + ":";
            if (dDate.Minute < 10)
                sDate += "0";
            sDate += dDate.Minute.ToString() + " | ";
            if (dDate.Day < 10)
                sDate += "0";
            sDate += dDate.Day.ToString() + "/";
            if (dDate.Month < 10)
                sDate += "0";
            sDate += dDate.Month.ToString() + "/0";
            sDate += (dDate.Year - 2000).ToString();
            return sDate;
            */
        }

        static protected string GetComments(Int32 nGroupID, Int32 nMediaID, Int32 nCommentType, Int32 nLangID, ref ApiObjects.UserComment[] theComments, bool bWithCache, bool bWritable)
        {
            StringBuilder sRet = new StringBuilder();

            Int32 nParentID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nGroupID).ToString());
            if (nParentID == 1)
            {
                nParentID = nGroupID;
            }

            string pattern = string.Empty;

            object oPattern = ODBCWrapper.Utils.GetTableSingleVal("group_language_filters", "Expression", "group_id", "=", nParentID);
            if (oPattern != null && oPattern != DBNull.Value)
            {
                pattern = oPattern.ToString();
            }

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (bWithCache == false)
                selectQuery.SetCachedSec(0);
            selectQuery += "select ";
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            if (nMediaID == 0)
                selectQuery += " top 20 ";
            selectQuery += " CASE ll.name when '---' then '' ELSE ll.name END as 'lang_name',mc.*,ct.NAME as 'type_name' from lu_languages ll(nolock),media_comments mc(nolock),comment_types ct(nolock),media m (nolock) where m.id=mc.media_id and mc.COMMENT_TYPE_ID=ct.id and mc.is_active=1 and mc.status=1 and ll.id=mc.language_id and ";
            if (nCommentType == -1)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.COMMENT_TYPE_ID", ">", 0);
                selectQuery += "and";
            }
            if (nCommentType >= 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.COMMENT_TYPE_ID", "=", nCommentType);
                selectQuery += " and ";
            }
            if (nLangID != 0)
            {
                selectQuery += "(";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.LANGUAGE_ID", "=", nLangID);
                selectQuery += " or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.LANGUAGE_ID", "=", 0);
                selectQuery += ") and ";
            }
            if (nMediaID != 0)
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.MEDIA_ID", "=", nMediaID);
                selectQuery += "and";
            }
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            selectQuery += "(";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.GROUP_ID", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ")";
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    string sMediaID = selectQuery.Table("query").DefaultView[i].Row["MEDIA_ID"].ToString();
                    string sWriter = selectQuery.Table("query").DefaultView[i].Row["WRITER"].ToString();
                    string sName = selectQuery.Table("query").DefaultView[i].Row["type_name"].ToString();
                    string sLang = selectQuery.Table("query").DefaultView[i].Row["lang_name"].ToString();
                    string sHeader = selectQuery.Table("query").DefaultView[i].Row["HEADER"].ToString();
                    string sSubHeader = selectQuery.Table("query").DefaultView[i].Row["SUB_HEADER"].ToString();
                    string sComment = selectQuery.Table("query").DefaultView[i].Row["CONTENT_TEXT"].ToString();

                    if (!string.IsNullOrEmpty(pattern))
                    {
                        sHeader = regex.Replace(sHeader, "****");
                        sComment = regex.Replace(sComment, "****");
                    }

                    DateTime dDate = (DateTime)(selectQuery.Table("query").DefaultView[i].Row["CREATE_DATE"]);
                    if (theComments == null)
                    {
                        sRet.Append("<comment id=\"").Append(sID).Append("\" type=\"").Append(sName).Append("\" language=\"").Append(sLang).Append("\">");
                        sRet.Append("<date>").Append(GetDateStr(dDate)).Append("</date>");
                        sRet.Append("<media id=\"").Append(sMediaID).Append("\"/>");
                        sRet.Append("<writer>").Append(sWriter).Append("</writer>");
                        sRet.Append("<header>").Append(sHeader).Append("</header>");
                        sRet.Append("<sub_header>").Append(sSubHeader).Append("</sub_header>");
                        sRet.Append("<content>").Append(XMLEncode(sComment, true)).Append("</content>");
                        sRet.Append("</comment>");
                    }
                    else
                    {
                        if (i == 0)
                            theComments = (ApiObjects.UserComment[])(ResizeArray(theComments, nCount));
                        theComments[i] = new ApiObjects.UserComment();
                        theComments[i].Initialize(nMediaID, int.Parse(sID), dDate, sWriter, sHeader, sSubHeader, sComment, sName, sLang);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet.ToString();
        }

        static public string SaveCommentsProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, ref ApiObjects.InitializationObject initObj,
            ApiObjects.UserComment theCommentToAdd, bool bWSAutoActive, string sWSCommentType, ref ApiObjects.UserComment[] theComments, string sLang)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            Int32 nMediaID = 0;
            Int32 nCommentType = 0;
            Int32 nAutoActive = 0;
            string sWriter = "";
            string sHeader = "";
            string sSubHeader = "";
            string sContent = "";
            string sCommentIP = "0.0.0.0";
            string sUDID = string.Empty;
            if (theDoc != null)
            {
                string sMediaID = "0";
                string sCommentType = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/comment/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();

                nMediaID = int.Parse(sMediaID);

                XmlNode theCommentType = theDoc.SelectSingleNode("/root/request/comment/@type");
                if (theCommentType != null)
                    sCommentType = theCommentType.Value.ToUpper();

                string sAutoActive = "";
                XmlNode theAutoActive = theDoc.SelectSingleNode("/root/request/comment/@auto_active");
                if (theAutoActive != null)
                    sAutoActive = theAutoActive.Value.ToLower();
                if (sAutoActive.Trim().ToLower() == "true")
                    nAutoActive = 1;
                else
                    nAutoActive = 0;
                XmlNode theWriter = theDoc.SelectSingleNode("/root/request/comment/writer").FirstChild;
                if (theWriter != null)
                    sWriter = theWriter.Value;

                XmlNode theHeader = theDoc.SelectSingleNode("/root/request/comment/header").FirstChild;
                if (theWriter != null)
                    sHeader = theHeader.Value;

                XmlNode theSubHeader = theDoc.SelectSingleNode("/root/request/comment/sub_header").FirstChild;
                if (theSubHeader != null)
                    sSubHeader = theSubHeader.Value;

                XmlNode theContent = theDoc.SelectSingleNode("/root/request/comment/content").FirstChild;
                if (theContent != null)
                    sContent = theContent.Value;

                sUDID = GetFlashVarsValue(ref theDoc, "device_udid");


                sCommentIP = PageUtils.GetCallerIP();
                nCommentType = GetCommentType(sCommentType, nGroupID);
            }
            else
            {
                if (theComments == null)
                    theComments = new ApiObjects.UserComment[0];
                nMediaID = theCommentToAdd.m_nMediaID;
                if (bWSAutoActive == true)
                    nAutoActive = 1;
                else
                    nAutoActive = 0;
                sWriter = theCommentToAdd.m_sWriter;
                sHeader = theCommentToAdd.m_sHeader;
                sSubHeader = theCommentToAdd.m_sSubHeader;
                sContent = theCommentToAdd.m_sContent;
                if (initObj.m_oUserIMRequestObject != null)
                    sCommentIP = initObj.m_oUserIMRequestObject.m_sUserIP;
                nCommentType = GetCommentType(sWSCommentType, nGroupID);
            }

            if (PageUtils.DoesStringSecurityValid(sWriter) == false)
                return GetErrorMessage("Security problem");
            if (PageUtils.DoesStringSecurityValid(sHeader) == false)
                return GetErrorMessage("Security problem");
            if (PageUtils.DoesStringSecurityValid(sSubHeader) == false)
                return GetErrorMessage("Security problem");
            if (PageUtils.DoesStringSecurityValid(sContent) == false)
                return GetErrorMessage("Security problem");

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_comments");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WRITER", "=", sWriter);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nAutoActive);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TVINCI_WATCHER_ID", "=", nWatcherID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMENT_IP", "=", sCommentIP);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("HEADER", "=", sHeader);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_HEADER", "=", sSubHeader);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTENT_TEXT", "=", sContent);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COMMENT_TYPE_ID", "=", nCommentType);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            StringBuilder sRet = new StringBuilder();
            if (theDoc != null)
                sRet.Append("<response type=\"save_comments\">");
            sRet.Append(GetComments(nGroupID, nMediaID, nCommentType, nLangID, ref theComments, false, true));
            if (theDoc != null)
                sRet.Append("</response>");
            return sRet.ToString();
        }


        static protected Int32 GetCommentType(string sCommentName, Int32 nGroupID)
        {
            if (sCommentName.ToLower() == "all")
                return -2;
            if (sCommentName.ToLower() == "all except users")
                return -1;
            Int32 nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from comment_types where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sCommentName);
            selectQuery += "order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static public string CommentsListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, Int32 nPlayerID, ref ApiObjects.InitializationObject initObj,
            Int32 nWSMediaID, string sWSCommentType, ref ApiObjects.UserComment[] theComments, string sLang)
        {
            Int32 nMediaID = 0;
            Int32 nCommentType = 0;
            StringBuilder sRet = new StringBuilder();
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            if (theDoc != null)
            {
                string sMediaID = "";
                string sCommentType = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();
                XmlNode theCommentType = theDoc.SelectSingleNode("/root/request/comments/@type");
                if (theCommentType != null)
                    sCommentType = theCommentType.Value;
                nMediaID = int.Parse(sMediaID);
                nCommentType = GetCommentType(sCommentType, nGroupID);
                sRet.Append("<response type=\"comments_list\">");
            }
            else
            {
                if (theComments == null)
                    theComments = new ApiObjects.UserComment[0];
                nMediaID = nWSMediaID;
                nCommentType = GetCommentType(sWSCommentType, nGroupID);
            }
            sRet.Append(GetComments(nGroupID, nMediaID, nCommentType, nLangID, ref theComments, true, false));
            if (theDoc != null)
                sRet.Append("</response>");
            return sRet.ToString();
        }

        static protected bool IsChannelBelongsToWatcher(Int32 nChannelID, Int32 nWatcherID, Int32 nGroupID)
        {
            bool bBelongs = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from channels (nolock) where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("watcher_id", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nChannelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    bBelongs = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bBelongs;
        }

        static protected Int32 GetPersonalChannelID(string sName, Int32 nWatcherID, Int32 nGroupID, Int32 nMediaID, ref Int32 nChannelsMediaID, ref Int32 nOrderNum)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select c.id,cm.id as 'cm_id',cm.order_num as 'o_n' from channels c (nolock),channels_media cm (nolock) where c.status=1 and c.is_active=1 and cm.channel_id=c.id and cm.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.watcher_id", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.name", "=", sName.Trim());
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cm.media_id", "=", nMediaID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nChannelsMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["cm_id"].ToString());
                    nOrderNum = int.Parse(selectQuery.Table("query").DefaultView[0].Row["o_n"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetPersonalChannelIDInner(string sName, Int32 nWatcherID, Int32 nGroupID, bool bWritable)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            if (bWritable == true)
                selectQuery.SetWritable(bWritable);
            selectQuery += "select c.id from channels c (nolock) where c.status=1 and c.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.watcher_id", "=", nWatcherID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.name", "=", sName.Trim());
            selectQuery += "and c.group_id " + PageUtils.GetAllGroupTreeStr(nGroupID);
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        static protected Int32 GetPersonalChannelID(string sName, Int32 nWatcherID, Int32 nGroupID, bool bRewrite)
        {
            Int32 nRet = GetPersonalChannelIDInner(sName, nWatcherID, nGroupID, true);
            if (nRet != 0)
            {
                if (bRewrite == false)
                    return 0;
                else
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels_media");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 2);
                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nRet);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                    return nRet;
                }
            }
            else
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channels");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_TYPE", "=", 2);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by platform API");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return GetPersonalChannelIDInner(sName, nWatcherID, nGroupID, false);
            }
        }

        static public string DeletePlayListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            Int32 nPlayerID, string sLang, bool bIsAdmin, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject[] theChannels,
            Int32 nWSChannelID, Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            Int32 nChannelID = 0;

            StringBuilder sRet = new StringBuilder();
            if (initObj == null)
            {
                sRet.Append("<response type=\"delete_playlist\">");
                XmlNode theChannelID = theDoc.SelectSingleNode("/root/request/channel/@id");
                if (theChannelID != null)
                {
                    string sChannelID = theChannelID.Value.Trim();
                    if (sChannelID != "")
                        nChannelID = int.Parse(sChannelID);
                }
            }
            else
                nChannelID = nWSChannelID;
            Int32 nNewWatcherID = nWatcherID;
            string sNewTvinciGUID = sTVinciGUID;
            if (sSiteGUID != "")
                nNewWatcherID = GetWatcherIDBySiteGUID(sSiteGUID, nGroupID, ref sNewTvinciGUID);
            bool bBelong = IsChannelBelongsToWatcher(nChannelID, nNewWatcherID, nGroupID);
            if (bBelong == false)
            {
                sRet.Append("<error>Channel does not belong to watcher</error>");
            }
            else
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 0);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nChannelID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            if (initObj == null)
            {
                ApiObjects.PicObject[] thePics = null;
                sRet.Append(WatcherChannelsListProtocolInner(ref theDoc, nGroupID, nNewWatcherID, false, bIsAdmin, ref thePics, ref theChannels, nLangID, bIsLangMain, nCountryID, nDeviceID, true));
                sRet.Append("</response>");
            }
            else
            {
                sRet.Append(WatcherChannelsListProtocolInner(ref theDoc, nGroupID, nNewWatcherID, false, bIsAdmin,
                    ref initObj.m_oPicObjects, ref theChannels, nLangID, bIsLangMain, nCountryID, nDeviceID, true));
            }
            return sRet.ToString();
        }

        static protected void InsertPlaylistItem(Int32 nChannelID, Int32 nMediaID, Int32 nGroupID,
            Int32 nOrderNum)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channels_media");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", nOrderNum);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }

        static protected bool DeleteMediaFromPlayList(string sChannelName, Int32 nWatcherID, Int32 nGroupID, Int32 nMediaID, Int32 nChannelID)
        {
            Int32 nChannelMediaID = 0;
            Int32 nOrderNum = 0;
            //Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID , nMediaID , ref nChannelMediaID , ref nOrderNum);
            //Int32 nChannelID = GetPersonalChannelIDInner(sChannelName, nWatcherID, nGroupID, true);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id,order_num from channels_media where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", nChannelID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += " and status=1";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nChannelMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nOrderNum = int.Parse(selectQuery.Table("query").DefaultView[0].Row["order_num"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nChannelMediaID == 0)
                return false;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("channels_media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nChannelMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;


            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update channels_media set order_num = order_num - 1 where status=1 and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
            directQuery += " and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", ">", nOrderNum);
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            return true;
        }

        static protected bool AddMediaToPlayList(string sChannelName, Int32 nWatcherID, Int32 nGroupID, Int32 nMediaID, Int32 nIndex, Int32 nChannelID)
        {
            //Int32 nChannelID = GetPersonalChannelIDInner(sChannelName, nWatcherID, nGroupID , true);
            if (nChannelID == 0)
                return false;
            Int32 nCurrentID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from channels_media where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCurrentID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCurrentID != 0)
                return false;
            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery += "update channels_media set order_num = order_num + 1 where status=1 and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
            directQuery += " and ";
            directQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", ">=", nIndex);
            directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            InsertPlaylistItem(nChannelID, nMediaID, nGroupID, nIndex);
            return true;
        }

        static public string SavePlayListProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            Int32 nPlayerID, string sLang, ref ApiObjects.InitializationObject initObj,
            Int32[] nMediaIDs, string sChannelTitle, bool bWSRewrite, ref ApiObjects.GenericWriteResponse theWSResponse)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sChannelName = "";
            if (nWatcherID != 0)
            {
                if (theDoc != null)
                {
                    sRet.Append("<response type=\"save_playlist\">");
                    XmlNode theChannelTitle = theDoc.SelectSingleNode("/root/request/channel/@title");
                    if (theChannelTitle != null)
                        sChannelName = theChannelTitle.Value.Replace("\r", "");
                    bool bRewrite = false;
                    string sChannelRewrite = "false";
                    XmlNode theChannelRewrite = theDoc.SelectSingleNode("/root/request/channel/@rewrite");
                    if (theChannelRewrite != null)
                        sChannelRewrite = theChannelRewrite.Value.ToUpper();
                    if (sChannelRewrite.Trim() == "TRUE")
                        bRewrite = true;

                    Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID, bRewrite);
                    if (nChannelID == 0)
                    {
                        sRet.Append("<error>Channel with this name allready exists</error>");
                    }
                    else
                    {
                        string sMediaID = "";
                        XmlNodeList theMeidas = theDoc.SelectNodes("/root/request/channel/media");
                        Int32 nCount = theMeidas.Count;
                        if (nCount > 0)
                        {
                            for (int i = 0; i < nCount; i++)
                            {
                                XmlNode theMediaID = theMeidas[i].SelectSingleNode("@id");
                                if (theMediaID != null)
                                    sMediaID = theMediaID.Value.ToUpper();
                                if (sMediaID == "")
                                    continue;
                                InsertPlaylistItem(nChannelID, int.Parse(sMediaID), nGroupID, i);
                            }
                        }
                        else
                        {
                            sRet.Append("<error>Channel with no media</error>");
                        }
                    }
                    sRet.Append("</response>");
                }
                else
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    sChannelName = sChannelTitle;
                    bool bRewrite = bWSRewrite;
                    Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID, bRewrite);
                    if (nChannelID == 0)
                    {
                        theWSResponse.Initialize("Such channel already exists - rewrite was set to false", -1);
                        return "";
                    }
                    else
                    {
                        Int32 nCount = nMediaIDs.Length;
                        if (nCount > 0)
                        {
                            for (int i = 0; i < nCount; i++)
                            {
                                Int32 nMediaID = nMediaIDs[i];
                                if (nMediaID == 0)
                                    continue;
                                InsertPlaylistItem(nChannelID, nMediaID, nGroupID, i);
                            }
                            theWSResponse.Initialize("OK", 0);
                        }
                        else
                        {
                            theWSResponse.Initialize("Mo media to save", -1);
                            return "";
                        }
                    }
                }
            }
            else
            {
                if (theDoc == null)
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    theWSResponse.Initialize("User unknown", -1);
                }
            }
            return sRet.ToString();
        }

        static public string DeletePlayListItemProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            Int32 nPlayerID, string sLang, ref ApiObjects.InitializationObject initObj,
            Int32 nMediaID, string sChannelTitle, ref ApiObjects.GenericWriteResponse theWSResponse)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sChannelName = "";
            if (nWatcherID != 0)
            {
                if (theDoc != null)
                {
                    sRet.Append("<response type=\"delete_playlist_item\">");
                    XmlNode theChannelTitle = theDoc.SelectSingleNode("/root/request/channel/@title");
                    if (theChannelTitle != null)
                        sChannelName = theChannelTitle.Value.Replace("\r", "");

                    Int32 nChannelID = GetPersonalChannelIDInner(sChannelName, nWatcherID, nGroupID, true);
                    if (nChannelID == 0)
                    {
                        sRet.Append("<error>Playlist does not exist</error>");
                    }
                    else
                    {
                        string sMediaID = "";
                        XmlNode theMeida = theDoc.SelectSingleNode("/root/request/channel/media");
                        if (theMeida != null)
                        {
                            XmlNode theMediaID = theMeida.SelectSingleNode("@id");
                            if (theMediaID != null)
                                sMediaID = theMediaID.Value.ToUpper();
                            if (sMediaID != "" && nChannelID != 0)
                                DeleteMediaFromPlayList(sChannelName, nWatcherID, nGroupID, int.Parse(sMediaID), nChannelID);
                        }
                        else
                        {
                            sRet.Append("<error>No media to delete</error>");
                        }
                    }
                    sRet.Append("</response>");
                }
                else
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    sChannelName = sChannelTitle;
                    Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID, true);
                    if (nChannelID == 0)
                    {
                        theWSResponse.Initialize("No channel exist", -1);
                        return "";
                    }
                    else
                    {
                        if (nMediaID != 0 && nChannelID != 0)
                        {
                            DeleteMediaFromPlayList(sChannelName, nWatcherID, nGroupID, nMediaID, nChannelID);
                            theWSResponse.Initialize("OK", 0);
                        }
                        else
                        {
                            if (nMediaID == 0)
                                theWSResponse.Initialize("No media to add", -1);
                            if (nChannelID == 0)
                                theWSResponse.Initialize("No channel exists", -1);
                            return "";
                        }
                    }
                }
            }
            else
            {
                if (theDoc == null)
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    theWSResponse.Initialize("User unknown", -1);
                }
            }
            return sRet.ToString();
        }

        static public string AddPlayListItemProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID,
            string sLastOnTvinci, string sLastOnSite, string sSiteGUID, Int32 nWatcherID,
            Int32 nPlayerID, string sLang, ref ApiObjects.InitializationObject initObj,
            Int32 nMediaID, Int32 nIndex, string sChannelTitle, ref ApiObjects.GenericWriteResponse theWSResponse)
        {
            StringBuilder sRet = new StringBuilder();
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sChannelName = "";
            if (nWatcherID != 0)
            {
                if (theDoc != null)
                {
                    string sIndex = "";
                    sRet.Append("<response type=\"add_playlist_item\">");
                    XmlNode theChannelTitle = theDoc.SelectSingleNode("/root/request/channel/@title");
                    if (theChannelTitle != null)
                        sChannelName = theChannelTitle.Value;

                    XmlNode theChannelIndex = theDoc.SelectSingleNode("/root/request/channel/@index");
                    if (theChannelIndex != null)
                        sIndex = theChannelIndex.Value;

                    if (sIndex == "")
                        sIndex = "0";

                    //Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID , true);
                    Int32 nChannelID = GetPersonalChannelIDInner(sChannelName, nWatcherID, nGroupID, true);
                    if (nChannelID == 0)
                    {
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("channels");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sChannelName);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_TYPE", "=", 2);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EDITOR_REMARKS", "=", "Created by platform API");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                        nChannelID = GetPersonalChannelIDInner(sChannelName, nWatcherID, nGroupID, false);
                        if (nChannelID == 0)
                            sRet.Append("<error>Playlist does not exist</error>");
                    }
                    else
                    {
                        string sMediaID = "";
                        XmlNode theMeida = theDoc.SelectSingleNode("/root/request/channel/media");
                        if (theMeida != null)
                        {
                            XmlNode theMediaID = theMeida.SelectSingleNode("@id");
                            if (theMediaID != null)
                                sMediaID = theMediaID.Value.ToUpper();
                            if (sMediaID != "")
                                if (AddMediaToPlayList(sChannelName, nWatcherID, nGroupID, int.Parse(sMediaID), int.Parse(sIndex), nChannelID) == false)
                                    sRet.Append("<error>Item already exist in playlist</error>");
                        }
                        else
                        {
                            sRet.Append("<error>No media to add</error>");
                        }
                    }
                    sRet.Append("</response>");
                }
                else
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    sChannelName = sChannelTitle;
                    Int32 nChannelID = GetPersonalChannelID(sChannelName, nWatcherID, nGroupID, true);
                    if (nChannelID == 0)
                    {
                        theWSResponse.Initialize("No channel exist", -1);
                        return "";
                    }
                    else
                    {
                        if (nMediaID != 0)
                        {
                            AddMediaToPlayList(sChannelName, nWatcherID, nGroupID, nMediaID, nIndex, nChannelID);
                            theWSResponse.Initialize("OK", 0);
                        }
                        else
                        {
                            theWSResponse.Initialize("Mo media to add", -1);
                            return "";
                        }
                    }
                }
            }
            else
            {
                if (theDoc == null)
                {
                    if (theWSResponse == null)
                        theWSResponse = new ApiObjects.GenericWriteResponse();
                    theWSResponse.Initialize("User unknown", -1);
                }
            }
            return sRet.ToString();
        }

        static private int GetChannelGroupID(int channelID)
        {
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from channels where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", channelID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static public string SubscriptionMediaProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang, Int32 nPlayerID, bool bWithCache,
            bool bIsAdmin, Int32 nCountryID, Int32 nDeviceID)
        {

            Int32 nLangID = 0;
            XmlNode theInfoStruct = null;
            bool bWithInfo = false;
            bool bWithFileTypes = false;
            bool bIsLangMain = true;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);


            int mediaCount = 0;
            StringBuilder sRet = new StringBuilder();


            StringBuilder sRetChannels = new StringBuilder();
            Int32 nCount = 0;

            string sSubscriptionID = "";
            Int32 nSubscriptionID = 0;

            XmlNode theSubscription = theDoc.SelectSingleNode("/root/request/subscription");

            if (theSubscription != null)
            {
                XmlNode theSubscriptionID = theSubscription.SelectSingleNode("@id");
                if (theSubscriptionID != null)
                {
                    sSubscriptionID = theSubscriptionID.Value.ToUpper();
                    nSubscriptionID = int.Parse(sSubscriptionID);
                }

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                XmlNode theMediaType = theDoc.SelectSingleNode("/root/request/subscription/cut_values/type/@value");
                string sMediaType = "";
                if (theMediaType != null)
                {
                    sMediaType = theMediaType.Value;
                }

                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;

                string sSubscriptionStart = "";
                XmlNode theSubscriptionStart = theSubscription.SelectSingleNode("@start_index");
                if (theSubscriptionStart != null)
                    sSubscriptionStart = theSubscriptionStart.Value.ToUpper();

                string sSubscriptionNOI = "";
                XmlNode theSubscriptionNOI = theSubscription.SelectSingleNode("@number_of_items");
                if (theSubscriptionNOI != null)
                    sSubscriptionNOI = theSubscriptionNOI.Value.ToUpper();

                XmlNode theSubscriptionOrderBy = theSubscription.SelectSingleNode("order_values");
                Int32 nNumOfItems = 20;
                Int32 nStartIndex = 0;
                if (sSubscriptionNOI != "")
                    nNumOfItems = int.Parse(sSubscriptionNOI);
                if (sSubscriptionStart != "")
                    nStartIndex = int.Parse(sSubscriptionStart);
                if (sSubscriptionID == "0")
                    return GetErrorMessage("Subscription id cant be 0");

                Int32 nOwnerGroup = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", nSubscriptionID, "pricing_connection").ToString());
                if (nOwnerGroup != nGroupID)
                {
                    return GetErrorMessage("Subscription does not belong to group");
                }


                if (CachingManager.CachingManager.Exist("SubscriptionMediaProtocol_" + sMediaType + "_" + sSubscriptionID + "_" + nGroupID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + nNumOfItems.ToString() + "_" + nStartIndex.ToString()) == true && bWithCache == true)
                {
                    sRet.Append(CachingManager.CachingManager.GetCachedData("SubscriptionMediaProtocol_" + sMediaType + "_" + sSubscriptionID + "_" + nGroupID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + nNumOfItems.ToString() + "_" + nStartIndex.ToString()));

                    return sRet.ToString();
                }
                else
                {
                    List<int> channelIds = new List<int>();

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("PRICING_CONNECTION_STRING");
                    selectQuery += " select channel_id from subscriptions_channels where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSubscriptionID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        nCount = selectQuery.Table("query").DefaultView.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            channelIds.Add(ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "channel_id", i));
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    if (channelIds.Count > 0)
                    {
                        Lucene_WCF.Service s = new Lucene_WCF.Service();

                        string sWSURL = GetWSURL("LUCENE_WCF");
                        if (!String.IsNullOrEmpty(sWSURL))
                            s.Url = sWSURL;

                        log.Info(String.Format("GetChannelsMedias:{0}, subID:{1}, channels:({2})", nGroupID, nSubscriptionID, string.Join(",", channelIds.Select(x => x.ToString()).ToArray())));

                        int[] mediaIds = null;
                        try
                        {
                            var res = s.GetChannelsMedias(nGroupID, channelIds.ToArray(), sMediaType, 0, GetOrderValues(theDoc, nGroupID), 0, 10000);
                            if (res != null && res.n_TotalItems > 0)
                            {
                                mediaIds = res.m_resultIDs;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("", ex);
                            mediaIds = new int[0];
                        }

                        mediaCount = mediaIds.Count(); //d.DefaultView.Count;

                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_format");
                        string sSubFileFormat = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "sub_file_format");
                        string sFileQuality = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "file_quality");
                        if (nNumOfItems == 0)
                            nNumOfItems = mediaCount;
                        Int32 nPageSize = nNumOfItems;
                        if (mediaCount - nStartIndex < nPageSize)
                            nPageSize = mediaCount - nStartIndex;

                        sRetChannels.Append("<channel>");
                        for (int i1 = nStartIndex; i1 < nStartIndex + nPageSize; i1++)
                        {
                            Int32 nMediaID = mediaIds.ElementAt(i1);    //int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            sRetChannels.Append(ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, sSubFileFormat, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nDeviceID));
                        }
                        sRetChannels.Append("</channel>");
                    }
                }

                string sSubName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubscriptionID, "pricing_connection").ToString();

                sRet.Append("<response type=\"subscription_media\">");
                sRet.Append("<subscription subscription_id=\"" + sSubscriptionID + "\" name=\"" + sSubName + "\" num_of_channels=\"1\" media_count=\"" + mediaCount.ToString() + "\">");
                sRet.Append(sRetChannels.ToString());
                sRet.Append("</subscription>");
                sRet.Append("</response>");

                if (bWithCache)
                {
                    CachingManager.CachingManager.SetCachedData("SubscriptionMediaProtocol_" + sMediaType + "_" + sSubscriptionID + "_" + nGroupID.ToString() + "_" + nLangID.ToString() + "_" + nDeviceID.ToString() + "_" + nNumOfItems.ToString() + "_" + nStartIndex.ToString(), sRet.ToString(), 10800, System.Web.Caching.CacheItemPriority.AboveNormal, nSubscriptionID, false);
                }

            }

            return sRet.ToString();
        }

        static private void AddErrorMessage(Int32 nGroupID, Int32 nMediaID, Int32 nMediaFileID, string sSiteGUID, Int32 nPlayTime, string sUDID, Int32 nPlatform, Int32 nErrorCode, string sErrorMessage)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("player_errors");

            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLAY_TIME_COUNTER", "=", nPlayTime);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", "=", nPlatform);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_UDID", "=", sUDID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ERROR_CODE", nErrorCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ERROR_MESSAGE", sErrorMessage);

            insertQuery.Execute();
            insertQuery.Finish();
        }

        static private void UpdateFollowMe(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, Int32 nPlayTime, string sUDID)
        {
            if (string.IsNullOrEmpty(sSiteGUID) || nMediaID == 0)
            {
                return;
            }

            Int32 nID = 0;
            DateTime dNow = DateTime.Now;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id, GETDATE() dNow from users_media_mark (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
            selectQuery += "and";

            if (sUDID.Contains("PC||"))
            {
                selectQuery += "device_udid like 'PC||%' order by update_date desc";
            }
            else
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_udid", "=", sUDID);
            }

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                    dNow = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "dNow", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;


            if (nID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_media_mark");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_udid", "=", sUDID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("location_sec", "=", nPlayTime);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            else
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_media_mark");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("location_sec", "=", nPlayTime);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_udid", "=", sUDID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        static private List<KeyValuePair<Int32, MediaWatchedObj>> GetPersonalLastWatched(Int32 nGroupID, string sSiteGUID)
        {
            List<KeyValuePair<Int32, MediaWatchedObj>> lMediaIDToDeviceName = new List<KeyValuePair<int, MediaWatchedObj>>();

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                return null;
            }

            string sGroupsStrByParent = PageUtils.GetGroupsStrByParent(nGroupID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select mr.media_id, mr.device_udid, mr.update_date from users_media_mark mr, ";
            selectQuery += "(select media_id,max(update_date) update_date from users_media_mark where ";
            selectQuery += "group_id " + sGroupsStrByParent;
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
            selectQuery += "group by media_id ) mm where mr.media_id=mm.media_id and mr.update_date=mm.update_date ";
            selectQuery += "order by mr.update_date desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_id"].ToString());
                    string sUDID = selectQuery.Table("query").DefaultView[i].Row["device_udid"].ToString();
                    DateTime lastWatchedDate = (DateTime)selectQuery.Table("query").DefaultView[i].Row["update_date"];
                    string sDeviceName = "PC";

                    if (!string.IsNullOrEmpty(sUDID))
                    {
                        if (sUDID.Contains("PC||"))
                        {
                            sDeviceName = "PC";
                        }
                        else
                        {
                            ODBCWrapper.DataSetSelectQuery selectDeviceQuery = new ODBCWrapper.DataSetSelectQuery();
                            selectDeviceQuery += "select name from devices where is_active=1 and status=1";
                            selectDeviceQuery += "and group_id " + sGroupsStrByParent;
                            selectDeviceQuery += "and";
                            selectDeviceQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sUDID);
                            selectDeviceQuery.SetConnectionKey("users_connection");
                            if (selectDeviceQuery.Execute("query", true) != null)
                            {
                                Int32 nCount2 = selectDeviceQuery.Table("query").DefaultView.Count;
                                if (nCount2 > 0)
                                {
                                    sDeviceName = selectDeviceQuery.Table("query").DefaultView[0].Row["name"].ToString();
                                }
                                else
                                {
                                    sDeviceName = "No Name";
                                }
                            }
                            selectDeviceQuery.Finish();
                            selectDeviceQuery = null;
                        }
                    }
                    MediaWatchedObj mwo = new MediaWatchedObj(nMediaID, sDeviceName, lastWatchedDate);
                    KeyValuePair<Int32, MediaWatchedObj> kvp = new KeyValuePair<int, MediaWatchedObj>(nMediaID, mwo);
                    lMediaIDToDeviceName.Add(kvp);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return lMediaIDToDeviceName;
        }

        private static int[] GetUserSocialMediaIDs(string siteGuid, int socialAction, int socialPlatform)
        {
            int[] retVal = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select usa.media_id from users_social_actions usa, media m where m.id = usa.media_id and m.is_active = 1 and m.status = 1 ";
            if (!string.IsNullOrEmpty(siteGuid))
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", siteGuid);
            }
            if (socialPlatform > 0)
            {
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", socialPlatform);
            }
            if (socialAction > 0)
            {
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", socialAction);
            }
            selectQuery += "and usa.is_active = 1 and usa.status = 1 order by usa.update_date desc";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        int mediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_id"].ToString());
                        retVal[i] = mediaID;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static public string GetUserSocialActions(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang,
            Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj,
            Int32 nWSMediaID, Int32 nWSMediaFileID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            bool bWithInfo = false;
            XmlNode theInfoStruct = null;
            bool bWithFileTypes = false;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nMediaID = 0;
            Int32 nMediaFileID = 0;
            string sEndDateField = "m.";
            int socialAction = 0;
            int socialPlatform = 0;
            int nStartIndex = 0;
            int nNumOfItems = 10;

            XmlNode theStartIndex = theDoc.SelectSingleNode("/root/request/channel/@start_index");
            if (theStartIndex != null)
                nStartIndex = int.Parse(theStartIndex.Value.ToUpper());

            XmlNode theNumOfItems = theDoc.SelectSingleNode("/root/request/channel/@number_of_items");
            if (theNumOfItems != null)
                nNumOfItems = int.Parse(theNumOfItems.Value.ToUpper());
            if (string.IsNullOrEmpty(sSiteGUID))
            {
                XmlNode theSiteGuid = theDoc.SelectSingleNode("/root/request/params/@site_guid");
                if (theSiteGuid != null)
                    sSiteGUID = theSiteGuid.Value.ToUpper();
            }
            if (initObj == null)
            {

                string sSocialAction = "";

                XmlNode theWithSA = theDoc.SelectSingleNode("/root/request/params/@social_action");
                if (theWithSA != null)
                    sSocialAction = theWithSA.Value.ToUpper();

                string sSocialPlatform = "";
                XmlNode theSP = theDoc.SelectSingleNode("/root/request/params/@social_platform");
                if (theSP != null)
                    sSocialPlatform = theSP.Value.ToUpper();

                if (!string.IsNullOrEmpty(sSocialAction))
                {
                    socialAction = int.Parse(sSocialAction);
                }

                if (!string.IsNullOrEmpty(sSocialPlatform))
                {
                    socialPlatform = int.Parse(sSocialPlatform);
                }

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);

            }
            else
            {
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);

                nMediaID = nWSMediaID;
                nMediaFileID = nWSMediaFileID;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);

            }

            int[] nUserMedias = GetUserSocialMediaIDs(sSiteGUID, socialAction, socialPlatform);



            StringBuilder sRet = new StringBuilder();
            if (initObj == null)
                sRet.Append("<response type=\"user_social_medias\">");

            if (nUserMedias == null || nUserMedias.Length == 0)
            {
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                sRet.Append("<channel id=\"\" media_count=\"").Append(0).Append("\" >");
            }
            else
            {

                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                Int32 nCount = nUserMedias.Length;
                if (initObj == null)
                    sRet.Append("<channel id=\"\" media_count=\"").Append(nCount).Append("\" >");
                int nPageSize = nNumOfItems;
                if (nCount - nStartIndex < nPageSize)
                    nPageSize = nCount - nStartIndex;
                for (int i = nStartIndex; i < nStartIndex + nPageSize; i++)
                {
                    Int32 nLocMediaID = nUserMedias[i];
                    if (initObj == null)
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nLocMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                    else
                    {
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                        bool bStatistics = false;
                        bool bPersonal = false;
                        string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                        string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;
                        ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                        ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nLocMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                        if (theChannelObj.m_oMediaObjects == null)
                            theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                        else
                            theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                        theChannelObj.m_nChannelTotalSize = nCount;
                    }
                }
            }

            if (initObj == null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }
            return sRet.ToString();
        }


        static public string PWLALProtocol(ref XmlDocument theDoc, Int32 nGroupID, string sTVinciGUID, string sLastOnTvinci,
            string sLastOnSite, string sSiteGUID, Int32 nWatcherID, string sLang,
            Int32 nPlayerID, bool bIsAdmin, Int32 nCountryID, ref ApiObjects.InitializationObject initObj,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, ref ApiObjects.ChannelObject theChannelObj,
            Int32 nWSMediaID, Int32 nWSMediaFileID, Int32 nDeviceID)
        {
            Int32 nLangID = 0;
            bool bIsLangMain = true;
            bool bWithInfo = false;
            XmlNode theInfoStruct = null;
            bool bWithFileTypes = false;
            ProtocolsFuncs.GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
            string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
            Int32 nMediaID = 0;
            Int32 nMediaFileID = 0;
            string sEndDateField = "m.";
            if (initObj == null)
            {
                string sMediaID = "";
                XmlNode theMediaID = theDoc.SelectSingleNode("/root/request/media/@id");
                if (theMediaID != null)
                    sMediaID = theMediaID.Value.ToUpper();

                string sMediaFileID = "";
                XmlNode theMediaFileID = theDoc.SelectSingleNode("/root/request/media/@file_id");
                if (theMediaFileID != null)
                    sMediaFileID = theMediaFileID.Value.ToUpper();

                string sWithInfo = "";
                XmlNode theWithInfo = theDoc.SelectSingleNode("/root/request/params/@with_info");
                if (theWithInfo != null)
                    sWithInfo = theWithInfo.Value.ToUpper();

                string sWithFileTypes = "";
                XmlNode theWithFileTypes = theDoc.SelectSingleNode("/root/request/params/@with_file_types");
                if (theWithFileTypes != null)
                    sWithFileTypes = theWithFileTypes.Value.ToUpper();

                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true);
                if (sWithInfo.Trim().ToLower() == "true")
                    bWithInfo = true;
                if (sWithFileTypes.Trim().ToLower() == "true")
                    bWithFileTypes = true;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(ref theDoc);
                nMediaID = int.Parse(sMediaID);
                if (sMediaFileID != "")
                    nMediaFileID = int.Parse(sMediaFileID);
            }
            else
            {
                bWithInfo = initObj.m_oExtraRequestObject.m_bWithInfo;
                bWithFileTypes = initObj.m_oExtraRequestObject.m_bWithFileTypes;
                theInfoStruct = ProtocolsFuncs.GetInfoStructNode(ref theDoc, nGroupID, true, ref theWSInfoStruct);

                nMediaID = nWSMediaID;
                nMediaFileID = nWSMediaFileID;
                sEndDateField += ProtocolsFuncs.GetFinalEndDateField(initObj.m_oExtraRequestObject.m_bUseFinalEndDate);

            }

            string sPeopleLiked = GetPeopleLikedMedia(nMediaID, sSiteGUID, 1, 1);



            StringBuilder sRet = new StringBuilder();
            if (initObj == null)
                sRet.Append("<response type=\"people_who_liked_also_liked\">");



            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select top 8 q.m_id from (select usa.media_id as m_id,count(*) as co from users_social_actions usa (nolock),media m WITH (nolock) where ";
            selectQuery += " m.id=usa.media_id and m.status=1 and m.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usa.media_id", "<>", nMediaID);
            if (nMediaFileID != 0)
            {
                Int32 nTypeID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "MEDIA_TYPE_ID", nMediaFileID).ToString());
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usa.FILE_FORMAT_ID", "=", nTypeID);
            }
            selectQuery += " and usa.is_active=1 and usa.social_acttion=1 and usa.social_platform=1 and "; /////////////////////////////////// Social_platform
            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            selectQuery += "( COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", nDeviceID);
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += "(";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            if (sWPGID != "")
            {
                selectQuery += " or m.WATCH_PERMISSION_TYPE_ID in (";
                selectQuery += sWPGID;
                selectQuery += ")";
            }
            selectQuery += ") and usa.user_site_guid in " + sPeopleLiked;
            selectQuery += " group by usa.media_id )q where q.co>0 order by q.co desc";
            if (string.IsNullOrEmpty(sPeopleLiked) || sPeopleLiked.Equals("()"))
            {
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                sRet.Append("<channel id=\"\" media_count=\"").Append(0).Append("\" >");
            }
            else if (selectQuery.Execute("query", true) != null)
            {
                sRet.Append(ProtocolsFuncs.GetPlayListSchema(ref theDoc, 0, nGroupID, nLangID, bIsLangMain, nWatcherID, nPlayerID, true));
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (initObj == null)
                    sRet.Append("<channel id=\"\" media_count=\"").Append(nCount).Append("\" >");
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nLocMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["m_ID"].ToString());
                    if (initObj == null)
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nLocMediaID, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, nPlayerID, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, nCountryID, nDeviceID));
                    else
                    {
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        string sFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileFormat = initObj.m_oFileRequestObjects[0].m_sFileFormat;
                        string sSubFileFormat = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 1)
                            sSubFileFormat = initObj.m_oFileRequestObjects[1].m_sFileFormat;
                        string sFileQuality = "";
                        if (initObj.m_oFileRequestObjects != null && initObj.m_oFileRequestObjects.Length > 0)
                            sFileQuality = initObj.m_oFileRequestObjects[0].m_sFileQuality;
                        bool bStatistics = false;
                        bool bPersonal = false;
                        string sStatistics = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "statistics").Trim().ToLower();
                        string sPersonal = TVinciShared.ProtocolsFuncs.GetNodeParameter(ref theInfoStruct, "personal").Trim().ToLower();
                        if (sStatistics == "true")
                            bStatistics = true;
                        if (sPersonal == "true")
                            bPersonal = true;
                        ApiObjects.MediaObject theMediaObj = new ApiObjects.MediaObject();
                        ProtocolsFuncs.GetMediaTagNeto(ref theDoc, nLocMediaID, "media", nGroupID, nCountryID, nBlocakble, sFileFormat, sFileQuality, nLangID, bIsLangMain, nWatcherID, bWithInfo, true, sSubFileFormat, nPlayerID, false, ref theInfoStruct, bIsAdmin, true, bWithFileTypes, ref initObj.m_oPicObjects, ref theMediaObj, false, bStatistics, bPersonal, ref theWSInfoStruct, nDeviceID);
                        if (theChannelObj.m_oMediaObjects == null)
                            theChannelObj.m_oMediaObjects = new ApiObjects.MediaObject[1];
                        else
                            theChannelObj.m_oMediaObjects = (ApiObjects.MediaObject[])(ResizeArray(theChannelObj.m_oMediaObjects, theChannelObj.m_oMediaObjects.Length + 1));
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = new ApiObjects.MediaObject();
                        theChannelObj.m_oMediaObjects[theChannelObj.m_oMediaObjects.Length - 1] = theMediaObj;
                        theChannelObj.m_nChannelTotalSize = nCount;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (initObj == null)
            {
                sRet.Append("</channel>");
                sRet.Append("</response>");
            }
            return sRet.ToString();
        }

        static protected string GetPeopleLikedMedia(Int32 nMediaID, string sSiteGUID, Int32 nSocialAction, Int32 nSocialPlatform)
        {
            StringBuilder sRet = new StringBuilder();
            sRet.Append("(");
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct top 30 user_site_guid, max(update_date) from users_social_actions WITH (nolock) where is_active=1 and status=1 and";
            selectQuery.SetWritable(true);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "<>", sSiteGUID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", nSocialAction);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", nSocialPlatform);
            selectQuery += " group by user_site_guid order by max(update_date)";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount == 0)
                    sRet.Append("0");
                for (int i = 0; i < nCount; i++)
                {
                    if (i > 0)
                        sRet.Append(",");
                    sRet.Append(selectQuery.Table("query").DefaultView[i].Row["user_site_guid"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sRet.Append(")");
            return sRet.ToString();
        }


        static public string GetPWLAL(Int32 nMediaID, string sSiteGUID)
        {
            Int32 nGroupID = 110;


            Int32 nSocialAction = 1;
            Int32 nSocialPlatform = 1;


            string sPeopleLiked = GetPeopleLikedMedia(nMediaID, sSiteGUID, 1, 1);

            StringBuilder sRet = new StringBuilder();

            sRet.Append("<response type=\"people_who_liked_also_liked\">");



            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select top 8 q.m_id from (select usa.media_id as m_id,count(*) as co from users_social_actions usa (nolock),media m WITH (nolock) where ";
            selectQuery += " m.id=usa.media_id and m.status=1 and m.is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usa.media_id", "<>", nMediaID);
            selectQuery += " and usa.is_active=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usa.social_action", "=", nSocialAction);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usa.social_platform", "=", nSocialPlatform);
            selectQuery += "and";
            selectQuery += " (m.id not in (select id from media (nolock) where (start_date>getdate() or FINAL_END_DATE<getdate()) and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += " (m.id not in (select media_id from media_locale_values (nolock) where (start_date>getdate() or FINAL_END_DATE<getdate()) and ";
            selectQuery += "( COUNTRY_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", "");
            selectQuery += ") and (LANGUAGE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", "");
            selectQuery += ") and (DEVICE_ID=0 or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", "");
            selectQuery += ") and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "))";
            selectQuery += " and ";
            selectQuery += "(";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);

            selectQuery += ") and usa.user_site_guid in " + sPeopleLiked;
            selectQuery += " group by usa.media_id )q where q.co>0 order by q.co desc";
            if (string.IsNullOrEmpty(sPeopleLiked) || sPeopleLiked.Equals("()"))
            {
                sRet.Append("<channel id=\"\" media_count=\"").Append(0).Append("\" >");
            }
            else if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                sRet.Append("<channel id=\"\" media_count=\"").Append(nCount).Append("\" >");
                for (int i = 0; i < nCount; i++)
                {
                    Int32 nLocMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["m_ID"].ToString());

                    sRet.Append("<media id=\"").Append(nLocMediaID).Append("\" />");

                }
            }
            selectQuery.Finish();
            selectQuery = null;

            sRet.Append("</channel>");
            sRet.Append("</response>");

            return sRet.ToString();
        }

        static public string GetDateRangeQuery(string sEndDateField, bool bWithStartDate)
        {
            string sQuery = string.Empty;

            if (bWithStartDate)
            {
                sQuery += " (start_date>getdate() or " + sEndDateField + "<getdate()) and ";
            }
            else
            {
                sQuery += sEndDateField + "<getdate() and";
            }

            return sQuery;
        }


        static public bool IsConcurrent(string sSiteGUID, string sUDID, int nGroupID)
        {
            bool bConcurrent = false;
            int nConcurrent = 0;

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                return bConcurrent;
            }

            object oConcurrent = ODBCWrapper.Utils.GetTableSingleVal("groups", "concurrent", nGroupID);
            if (oConcurrent != null && oConcurrent != DBNull.Value)
            {
                nConcurrent = int.Parse(oConcurrent.ToString());
            }

            if (nConcurrent == 0)
            {
                return bConcurrent;
            }


            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetWritable(true);
            selectQuery += "select top 1 GETDATE() as dNow, update_date, device_udid from users_media_mark (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
            selectQuery += "order by update_date desc";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    string sLastUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_udid", 0);
                    //string sLastSessionID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "session_id", 0);

                    if (sUDID != sLastUDID)
                    {
                        DateTime dLast = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "update_date", 0);
                        DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "dNow", 0);
                        TimeSpan ts = dNow.Subtract(dLast);

                        if (ts.TotalSeconds < 65)
                        {
                            bConcurrent = true;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return bConcurrent;
        }


        //Get Media with Lucene Search , then complite media data with ProtocolsFuncs.GetMediaTag
        static public string GetSearchMediaWithLucene(int nStartIndex, int nNumOfItems, int nMediaID, Int32 nGroupID, string sMediaTypeID, string sName, bool bAnd, bool bExact,
            string sDescription, ref XmlNodeList theMetaList, ref XmlNodeList theTagsList,
            string sPlaylistSchema, ref XmlDocument theDoc, Int32 nLangID, bool bIsLangMain, Int32 nWatcherID,
            bool bWithInfo, bool bWithCache, Int32 nPlayerID, ref XmlNode theInfoStruct, bool bIsAdmin,
             bool bWithFileTypes, Int32 nCountryID, string sMinDate, string sMaxDate, string sDocStruct,
            ref ApiObjects.MediaInfoStructObject theWSInfoStruct, Int32 nDeviceID, bool bUseStartDate, int[] nDeviceRules)
        {
            StringBuilder sRet = new StringBuilder("");
            string sWSURL = string.Empty;
            try
            {
                #region Search with Lucene
                //Build 2 CondList for search tags / metaStr / metaDobule .
                List<Lucene_WCF.SearchValue> m_dAnd = new List<Lucene_WCF.SearchValue>();
                List<Lucene_WCF.SearchValue> m_dOr = new List<Lucene_WCF.SearchValue>();

                if (theMetaList.Count > 0)
                    SearchObjectMeta(m_dAnd, m_dOr, theMetaList, nGroupID, bAnd); // add Metas values to m_dAnd / m_dOr .
                if (theTagsList.Count > 0)
                    SearchObjectTags(m_dAnd, m_dOr, theTagsList, nGroupID, bAnd); // add Tags values to m_dAnd / m_dOr .
                if (!string.IsNullOrEmpty(sName) || !string.IsNullOrEmpty(sDescription))
                {
                    SearchObjectString(m_dAnd, m_dOr, sName, sDescription, bAnd);// add name + description values to m_dAnd / m_dOr .
                }
                #endregion

                #region Build Lucene Search Object
                Lucene_WCF.SearchObj searchObj = new Lucene_WCF.SearchObj();
                searchObj.m_bUseStartDate = bUseStartDate;
                searchObj.m_nMediaID = nMediaID;
                searchObj.m_sMediaTypes = sMediaTypeID;
                searchObj.m_sName = sName;
                searchObj.m_sDescription = sDescription;
                searchObj.m_oOrder = GetOrderValues(theDoc, nGroupID);
                searchObj.m_nDeviceRuleId = nDeviceRules;

                if (m_dOr.Count > 0)
                {
                    searchObj.m_dOr = m_dOr.ToArray();
                }
                if (m_dAnd.Count > 0)
                {
                    searchObj.m_dAnd = m_dAnd.ToArray();
                }

                searchObj.m_bExact = bExact;
                if (bAnd)
                    searchObj.m_eCutWith = Lucene_WCF.CutWith.AND;
                else
                    searchObj.m_eCutWith = Lucene_WCF.CutWith.OR;
                #endregion

                #region call Lucene Search

                Lucene_WCF.Service s = new Lucene_WCF.Service();

                sWSURL = GetWSURL("LUCENE_WCF");
                if (!String.IsNullOrEmpty(sWSURL))
                    s.Url = sWSURL;

                int[] mediaIds = null;
                try
                {
                    mediaIds = s.SearchMedias(nGroupID, searchObj, nLangID, bUseStartDate);
                    log.Info(string.Format("GetSearchMediaWithLucene:{0}, lucene:{1}, res:{2}", nGroupID, s.Url, mediaIds.Length));
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                    mediaIds = new int[0];
                }

                #endregion

                #region GetMediaInfo

                if (mediaIds == null)
                {
                    return sRet.ToString();
                }

                if (mediaIds.Length > nNumOfItems + nStartIndex)
                {
                    nNumOfItems = nNumOfItems + nStartIndex;
                }
                else
                {
                    nNumOfItems = mediaIds.Length;
                }

                sRet.Append("<channel id=\"\" media_count=\"").Append(mediaIds.Length).Append("\" >");
                sRet.Append(sPlaylistSchema);
                if (mediaIds.Length > 0)
                {
                    for (int i = nStartIndex; i < nNumOfItems; i++)
                    {
                        int nMedia = mediaIds[i];
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMedia, "media", nGroupID, nLangID, bIsLangMain, nWatcherID, bWithInfo, bWithCache, nPlayerID, ref theInfoStruct, bIsAdmin,
                            true, bWithFileTypes, nCountryID, nDeviceID, false, string.Empty, DateTime.MaxValue, bUseStartDate));
                    }
                }
                sRet.Append("</channel>");
                #endregion
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return sRet.ToString();
        }

        //Fill Metas into list of And / Or conditions. - For Lucene Search
        private static void SearchObjectMeta(List<Lucene_WCF.SearchValue> m_dAnd, List<Lucene_WCF.SearchValue> m_dOr, XmlNodeList theMetaList, int nGroupID, bool bAnd)
        {
            Lucene_WCF.SearchValue searchValue;

            try
            {
                if (theMetaList != null)
                {
                    for (int i = 0; i < theMetaList.Count; i++)
                    {
                        XmlNode theMetaName = theMetaList[i].SelectSingleNode("@name");
                        XmlNode theMetaValue = theMetaList[i].SelectSingleNode("@value");

                        string sMetaName = "";
                        string sMetaValue = "";

                        if (theMetaName != null)
                            sMetaName = theMetaName.Value;

                        if (theMetaValue != null)
                            sMetaValue = theMetaValue.Value;

                        if (!String.IsNullOrEmpty(sMetaName) && !String.IsNullOrEmpty(sMetaValue))
                        {
                            searchValue = new Lucene_WCF.SearchValue();
                            searchValue.m_sKey = sMetaName;
                            searchValue.m_sValue = sMetaValue;

                            if (bAnd)
                            {
                                m_dAnd.Add(searchValue);
                            }
                            else
                            {
                                m_dOr.Add(searchValue);
                            }
                        }
                        searchValue = null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
        }

        //Fill tags into list of And / Or conditions. - For Lucene Search
        private static void SearchObjectTags(List<Lucene_WCF.SearchValue> m_dAnd, List<Lucene_WCF.SearchValue> m_dOr, XmlNodeList theTagsList, int nGroupID, bool bAnd)
        {
            Lucene_WCF.SearchValue searchValue;
            try
            {
                for (int i = 0; i < theTagsList.Count; i++)
                {
                    XmlNode theTagName = theTagsList[i].SelectSingleNode("@name");
                    XmlNode theTagValue = theTagsList[i].SelectSingleNode("@value");
                    XmlNode theTagMust = theTagsList[i].SelectSingleNode("@cut_with");

                    string sTagName = "";
                    string sTagValue = "";
                    bool bTagMust = bAnd;

                    if (theTagName != null)
                        sTagName = theTagName.Value;
                    if (theTagValue != null)
                        sTagValue = theTagValue.Value;

                    if (theTagMust != null && theTagMust.Value.ToLower().Equals("and"))
                        bTagMust = true;

                    if (!String.IsNullOrEmpty(sTagName) && !String.IsNullOrEmpty(sTagValue))
                    {
                        searchValue = new Lucene_WCF.SearchValue();
                        searchValue.m_sKey = sTagName;
                        searchValue.m_sValue = sTagValue;

                        if (bTagMust)  //Only on "and" parameter
                        {
                            m_dAnd.Add(searchValue);
                        }
                        else
                        {
                            m_dOr.Add(searchValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
        }

        private static void SearchObjectString(List<Lucene_WCF.SearchValue> m_dAnd, List<Lucene_WCF.SearchValue> m_dOr, string sName, string sDescription, bool bAnd)
        {
            Lucene_WCF.SearchValue searchValue;
            try
            {
                if (!String.IsNullOrEmpty(sName))
                {
                    searchValue = new Lucene_WCF.SearchValue();
                    searchValue.m_sKey = "name";
                    searchValue.m_sValue = sName;
                    if (bAnd)
                    {
                        m_dAnd.Add(searchValue);
                    }
                    else
                    {
                        m_dOr.Add(searchValue);
                    }
                }
                if (!String.IsNullOrEmpty(sDescription))
                {
                    searchValue = new Lucene_WCF.SearchValue();
                    searchValue.m_sKey = "description";
                    searchValue.m_sValue = sDescription;
                    if (bAnd)
                    {
                        m_dAnd.Add(searchValue);
                    }
                    else
                    {
                        m_dOr.Add(searchValue);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SearchObjectString", ex);
            }
        }
        private static string GetWSURL(string sKey)
        {
            return WS_Utils.GetTcmConfigValue(sKey);
        }

        /*
        private static void GetOrderValues(ref Lucene_WCF.OrderBy eOrderBy, ref Lucene_WCF.OrderDir eOrderDir, XmlDocument theDoc, int nGroupID)
        {
            try
            {
                string sOrderDir = String.Empty;

                #region name
                XmlNode theOrderNameDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/name/@order_dir");
                if (theOrderNameDir != null)
                {
                    sOrderDir = theOrderNameDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.NAME;
                    }
                }
                #endregion
                #region description
                XmlNode theOrderDescDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/description/@order_dir");
                if (theOrderDescDir != null)
                {
                    sOrderDir = theOrderDescDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        //eOrderBy = Lucene_WCF.OrderBy.; TODO !!!!!!!!!
                    }
                }
                #endregion
                #region date = startDate
                XmlNode theOrderDateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/date/@order_dir");
                if (theOrderDateDir != null)
                {
                    sOrderDir = theOrderDateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.START_DATE;
                    }
                }
                #endregion
                #region views
                XmlNode theOrderViewsDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/views/@order_dir");
                if (theOrderViewsDir != null)
                {
                    sOrderDir = theOrderViewsDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.VIEWS;
                    }
                }
                #endregion
                #region rate
                XmlNode theOrderRateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/rate/@order_dir");
                if (theOrderRateDir != null)
                {
                    sOrderDir = theOrderRateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.VOTES_COUNT;
                    }
                }
                #endregion
                #region id
                XmlNode theIdDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/id/@order_dir");
                if (theIdDir != null)
                {
                    sOrderDir = theIdDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.ID;
                    }
                }
                #endregion
                #region like_counter
                XmlNode theLikeCounterDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/like_counter/@order_dir");
                if (theLikeCounterDir != null)
                {
                    sOrderDir = theLikeCounterDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.LIKE_COUNTER;
                    }
                }
                #endregion
                #region votes_count
                XmlNode theVotesCountDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/votes_count/@order_dir");
                if (theVotesCountDir != null)
                {
                    sOrderDir = theVotesCountDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.VOTES_COUNT;
                    }
                }
                #endregion
                #region date = createDate
                XmlNode theCreateDateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/create_date/@order_dir");
                if (theCreateDateDir != null)
                {
                    sOrderDir = theCreateDateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        eOrderBy = Lucene_WCF.OrderBy.CREATE_DATE;
                    }
                }
                #endregion


                XmlNodeList theOrderMetaList = theDoc.SelectNodes("/root/request/search_data/order_values/meta");
                IEnumerator iterMeta = theOrderMetaList.GetEnumerator();

                while (iterMeta.MoveNext())
                {

                    XmlNode theMeta = (XmlNode)(iterMeta.Current);

                    XmlNode theOrderMetaDir = theMeta.SelectSingleNode("@order_dir");
                    XmlNode theOrderMetaName = theMeta.SelectSingleNode("@name");

                    string sOrderMetaDir = "";
                    string sOrderMetaName = "";
                    if (theOrderMetaDir != null)
                        sOrderMetaDir = theOrderMetaDir.Value;
                    if (theOrderMetaName != null)
                        sOrderMetaName = theOrderMetaName.Value;

                    if (!string.IsNullOrEmpty(sOrderMetaDir))
                    {
                        sOrderDir = sOrderMetaDir;
                    }

                    if (sOrderMetaName != "")
                    {
                        Int32 nStrID = PageUtils.GetStringMetaIDByMetaName(nGroupID, sOrderMetaName);
                        if (nStrID == 0)
                        {
                            nStrID = PageUtils.GetDoubleMetaIDByMetaName(nGroupID, sOrderMetaName);
                            if (nStrID != 0)
                                nStrID += 20; // Add 20 Becouse the values of Lucene_WCF.OrderBy Doubles values are 21-30
                        }

                        switch (nStrID)
                        {
                            case 0:
                                break;
                            case 1:
                                eOrderBy = Lucene_WCF.OrderBy.META1_STR;
                                break;
                            case 2:
                                eOrderBy = Lucene_WCF.OrderBy.META2_STR;
                                break;
                            case 3:
                                eOrderBy = Lucene_WCF.OrderBy.META3_STR;
                                break;
                            case 4:
                                eOrderBy = Lucene_WCF.OrderBy.META4_STR;
                                break;
                            case 5:
                                eOrderBy = Lucene_WCF.OrderBy.META5_STR;
                                break;
                            case 6:
                                eOrderBy = Lucene_WCF.OrderBy.META6_STR;
                                break;
                            case 7:
                                eOrderBy = Lucene_WCF.OrderBy.META7_STR;
                                break;
                            case 8:
                                eOrderBy = Lucene_WCF.OrderBy.META8_STR;
                                break;
                            case 9:
                                eOrderBy = Lucene_WCF.OrderBy.META9_STR;
                                break;
                            case 10:
                                eOrderBy = Lucene_WCF.OrderBy.META10_STR;
                                break;
                            case 11:
                                eOrderBy = Lucene_WCF.OrderBy.META11_STR;
                                break;
                            case 12:
                                eOrderBy = Lucene_WCF.OrderBy.META12_STR;
                                break;
                            case 13:
                                eOrderBy = Lucene_WCF.OrderBy.META13_STR;
                                break;
                            case 14:
                                eOrderBy = Lucene_WCF.OrderBy.META14_STR;
                                break;
                            case 15:
                                eOrderBy = Lucene_WCF.OrderBy.META15_STR;
                                break;
                            case 16:
                                eOrderBy = Lucene_WCF.OrderBy.META16_STR;
                                break;
                            case 17:
                                eOrderBy = Lucene_WCF.OrderBy.META17_STR;
                                break;
                            case 18:
                                eOrderBy = Lucene_WCF.OrderBy.META18_STR;
                                break;
                            case 19:
                                eOrderBy = Lucene_WCF.OrderBy.META19_STR;
                                break;
                            case 20:
                                eOrderBy = Lucene_WCF.OrderBy.META20_STR;
                                break;
                            case 21:
                                eOrderBy = Lucene_WCF.OrderBy.META1_DOUBLE;
                                break;
                            case 22:
                                eOrderBy = Lucene_WCF.OrderBy.META2_DOUBLE;
                                break;
                            case 23:
                                eOrderBy = Lucene_WCF.OrderBy.META3_DOUBLE;
                                break;
                            case 24:
                                eOrderBy = Lucene_WCF.OrderBy.META4_DOUBLE;
                                break;
                            case 25:
                                eOrderBy = Lucene_WCF.OrderBy.META5_DOUBLE;
                                break;
                            case 26:
                                eOrderBy = Lucene_WCF.OrderBy.META6_DOUBLE;
                                break;
                            case 27:
                                eOrderBy = Lucene_WCF.OrderBy.META7_DOUBLE;
                                break;
                            case 28:
                                eOrderBy = Lucene_WCF.OrderBy.META8_DOUBLE;
                                break;
                            case 29:
                                eOrderBy = Lucene_WCF.OrderBy.META9_DOUBLE;
                                break;
                            case 30:
                                eOrderBy = Lucene_WCF.OrderBy.META10_DOUBLE;
                                break;
                            default:
                                break;
                        }
                    }
                }


                //Set OrderDir with value
                if (!string.IsNullOrEmpty(sOrderDir))
                {
                    if (sOrderDir.ToLower() == "asc")
                    {
                        eOrderDir = Lucene_WCF.OrderDir.ASC;
                    }
                    else
                    {
                        eOrderDir = Lucene_WCF.OrderDir.DESC;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }
        */

        private static Lucene_WCF.OrderObj GetOrderValues(XmlDocument theDoc, int nGroupID)
        {
            Lucene_WCF.OrderObj oOrderObj = new Lucene_WCF.OrderObj();
            oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.CREATE_DATE;
            oOrderObj.m_eOrderDir = Lucene_WCF.OrderDir.DESC;
            try
            {

                string sOrderDir = String.Empty;

                #region name
                XmlNode theOrderNameDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/name/@order_dir");
                if (theOrderNameDir != null)
                {
                    sOrderDir = theOrderNameDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.NAME;
                    }
                }
                #endregion
                #region description
                XmlNode theOrderDescDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/description/@order_dir");
                if (theOrderDescDir != null)
                {
                    sOrderDir = theOrderDescDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        //eOrderBy = Lucene_WCF.OrderBy.; TODO !!!!!!!!!
                    }
                }
                #endregion
                #region date = startDate
                XmlNode theOrderDateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/date/@order_dir");
                if (theOrderDateDir != null)
                {
                    sOrderDir = theOrderDateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.START_DATE;
                    }
                }
                #endregion
                #region views
                XmlNode theOrderViewsDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/views/@order_dir");
                if (theOrderViewsDir != null)
                {
                    sOrderDir = theOrderViewsDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.VIEWS;
                    }
                }
                #endregion
                #region rate
                XmlNode theOrderRateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/rate/@order_dir");
                if (theOrderRateDir != null)
                {
                    sOrderDir = theOrderRateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.RATING;
                    }
                }
                #endregion
                #region id
                XmlNode theIdDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/id/@order_dir");
                if (theIdDir != null)
                {
                    sOrderDir = theIdDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.ID;
                    }
                }
                #endregion
                #region like_counter
                XmlNode theLikeCounterDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/like_counter/@order_dir");
                if (theLikeCounterDir != null)
                {
                    sOrderDir = theLikeCounterDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.LIKE_COUNTER;
                    }
                }
                #endregion
                #region votes_count
                XmlNode theVotesCountDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/votes_count/@order_dir");
                if (theVotesCountDir != null)
                {
                    sOrderDir = theVotesCountDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.VOTES_COUNT;
                        ;
                    }
                }
                #endregion
                #region date = createDate
                XmlNode theCreateDateDir = theDoc.SelectSingleNode("/root/request/search_data/order_values/create_date/@order_dir");
                if (theCreateDateDir != null)
                {
                    sOrderDir = theCreateDateDir.Value;

                    if (!String.IsNullOrEmpty(sOrderDir))
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.CREATE_DATE;
                    }
                }
                #endregion

                XmlNodeList theOrderMetaList = theDoc.SelectNodes("/root/request/search_data/order_values/meta");
                IEnumerator iterMeta = theOrderMetaList.GetEnumerator();

                while (iterMeta.MoveNext())
                {

                    XmlNode theMeta = (XmlNode)(iterMeta.Current);

                    XmlNode theOrderMetaDir = theMeta.SelectSingleNode("@order_dir");
                    XmlNode theOrderMetaName = theMeta.SelectSingleNode("@name");

                    string sOrderMetaDir = "";
                    string sOrderMetaName = "";
                    if (theOrderMetaDir != null)
                        sOrderMetaDir = theOrderMetaDir.Value;
                    if (theOrderMetaName != null)
                        sOrderMetaName = theOrderMetaName.Value;

                    if (!string.IsNullOrEmpty(sOrderMetaDir))
                    {
                        sOrderDir = sOrderMetaDir;
                    }

                    if (sOrderMetaName != "")
                    {
                        oOrderObj.m_eOrderBy = Lucene_WCF.OrderBy.META;
                        oOrderObj.m_sOrderVal = sOrderMetaName;
                    }
                }


                //Set OrderDir with value
                if (!string.IsNullOrEmpty(sOrderDir))
                {
                    if (sOrderDir.ToLower() == "asc")
                    {
                        oOrderObj.m_eOrderDir = Lucene_WCF.OrderDir.ASC;
                    }
                    else
                    {
                        oOrderObj.m_eOrderDir = Lucene_WCF.OrderDir.DESC;
                    }
                }


            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return oOrderObj;
        }
    }
}