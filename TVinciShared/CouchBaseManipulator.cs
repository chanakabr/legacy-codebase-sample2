using ApiObjects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace TVinciShared
{
    public class CouchBaseManipulator : DBManipulator
    {
        //updates the epg object and returns its ID 
        public static int DoTheWork(ref EpgCB epg, Dictionary<int, string> dMetaTyps, Dictionary<int, string> dTagTyps)
        {
            int nID = 0;
            NameValueCollection coll = HttpContext.Current.Request.Form;
            if (coll["table_name"] == null)
            {
                HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
                EndOfAction();
            }

            if (epg == null)
            {
                epg = new EpgCB();
                epg.CreateDate = DateTime.UtcNow;
            }

            setBasicEpgData(ref coll, ref epg);
            setEpgMeta(ref coll, ref epg, dMetaTyps);
            setEpgTags(ref coll, ref epg, dTagTyps);

            nID = (int)epg.EpgID;
            EndOfAction();
            return nID;
        }

        private static void setEpgTags(ref NameValueCollection coll, ref EpgCB epg, Dictionary<int, string> tagTypes)
        {
            int nCounter = 0;
            try
            {
                while (nCounter < coll.Count)
                {
                    if (coll[nCounter.ToString() + "_type"] == null)
                        break;
                    if (coll[nCounter.ToString() + "_type"].ToString() == "multi")
                    {
                        string sVal = "";
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                            if (sVal != "" && coll[nCounter.ToString() + "_extra_field_val"] != null)
                            {
                                string sExtraVal = coll[nCounter.ToString() + "_extra_field_val"].ToString();
                                if (sExtraVal != "")
                                {
                                    int nTagTypeID = int.Parse(sExtraVal);
                                    if (tagTypes.ContainsKey(nTagTypeID))//verify the tag type is in DB
                                    {
                                        char[] t = { ';' };
                                        string[] splitedStr = sVal.Split(t); //split between different tag values
                                        List<string> lValues = new List<string>();
                                        for (int i = 0; i < splitedStr.Length; i++)
                                        {
                                            if (splitedStr[i] != "")
                                            {
                                                lValues.Add(splitedStr[i]);
                                                if (!doesTagValueExist(nTagTypeID, splitedStr[i]))
                                                {
                                                    insertTagValue(nTagTypeID, splitedStr[i]);//enter new value to tags table
                                                }
                                            }
                                        }

                                        string sTagType = tagTypes[nTagTypeID].ToLower();
                                        if (!epg.Tags.ContainsKey(sTagType) && lValues.Count > 0)
                                        {
                                            epg.Tags.Add(sTagType, lValues); //add the new tagtype to the epg, with its values
                                        }
                                        else
                                        {
                                            if (lValues.Count > 0)
                                                epg.Tags[sTagType] = lValues;//add the values
                                            else
                                                epg.Tags.Remove(sTagType); //remove the entire tagType from the epg
                                        }
                                    }
                                }
                            }
                        }
                    }
                    nCounter++;
                }
            }

            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", ex.Message + " || On function: setEpgTags with epg ID :" + epg.EpgID, "Exceptions");
            }
        }

        private static bool doesTagValueExist(int nTagTypeId, string sVal)
        {
            bool exists = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select ID from EPG_tags where status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", nTagTypeId);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sVal);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    exists = true;
                }
            }
            return exists;
        }

        private static bool insertTagValue(int nTagTypeId, string sVal)
        {
            bool result = false;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("EPG_tags");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", nTagTypeId);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sVal);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", DateTime.UtcNow);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            result = insertQuery.Execute();
            insertQuery.Finish();
            return result;
        }

        private static void setEpgMeta(ref NameValueCollection coll, ref EpgCB epg, Dictionary<int, string> metaTypes)
        {
            int nCounter = 0;
            try
            {
                while (nCounter < coll.Count)
                {
                    if (coll[nCounter.ToString() + "_type"] == null)
                        break;
                    string sVal = "";
                    if (coll[nCounter.ToString() + "_val"] != null)
                        sVal = coll[nCounter.ToString() + "_val"].ToString();
                    if (coll[nCounter.ToString() + "_ext"] != null)
                    {
                        string sExtID = coll[nCounter.ToString() + "_ext"].ToString();
                        if (sExtID != "")
                        {
                            int id = int.Parse(sExtID);
                            bool wasFound = false;
                            if (metaTypes.ContainsKey(id)) //verify the meta type is in DB
                            {
                                string typeName = metaTypes[id].ToLower();
                                if (epg.Metas.ContainsKey(typeName))
                                {
                                    if (sVal == "") // if the value is "", remove the entire meta
                                        epg.Metas.Remove(typeName);
                                    else
                                        epg.Metas[typeName][0] = sVal;
                                    wasFound = true;
                                }

                                if (!wasFound && sVal != "")
                                {
                                    epg.Metas.Add(typeName, new List<string>() { sVal });
                                }
                            }
                        }
                    }
                    nCounter++;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", ex.Message + " || On function: setEpgMeta with epg ID :" + epg.EpgID, "Exceptions");
            }
        }

        private static void setBasicEpgData(ref NameValueCollection coll, ref EpgCB epg)
        {
            int nGroupID = LoginManager.GetLoginGroupID();
            int nCount = coll.Count;
            int nCounter = 0;
            bool bValid = true;

            epg.UpdateDate = DateTime.UtcNow;
            epg.isActive = true;
            epg.Status = 1;

            try
            {
                while (nCounter < nCount)
                {
                    if (coll[nCounter.ToString() + "_type"] == null || bValid == false)
                        break;

                    string sType = coll[nCounter.ToString() + "_type"].ToString();

                    string sVal = "";
                    if (coll[nCounter.ToString() + "_val"] != null)
                        sVal = coll[nCounter.ToString() + "_val"].ToString();

                    string sFieldName = "";
                    if (coll[nCounter.ToString() + "_field"] != null)
                        sFieldName = coll[nCounter.ToString() + "_field"].ToString();

                    if (sFieldName == "")
                    {
                        nCounter++;
                        continue;
                    }

                    if (sFieldName.Trim().ToLower() == "group_id")
                    {
                        bool bBelongs = false;
                        int nQueryGroupID = 0;
                        if (sVal != "")
                            nQueryGroupID = int.Parse(sVal);
                        if (nQueryGroupID != 0 && nQueryGroupID != nGroupID)
                        {
                            if (nGroupID != 0)
                                PageUtils.DoesGroupIsParentOfGroup(nGroupID, nQueryGroupID, ref bBelongs);
                        }
                        else
                            bBelongs = true;

                        if (bBelongs == false)
                        {
                            LoginManager.LogoutFromSite("login.html");
                            return;
                        }
                        epg.GroupID = nGroupID;
                        epg.ParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
                    }
                    if (sFieldName.Trim().ToLower() == "epg_channel_id" && sType == "int")
                    {
                        bValid = validateParam("int", sVal, -1, -1);
                        int nChannelID = 0;
                        if (int.TryParse(sVal, out nChannelID))
                            epg.ChannelID = nChannelID;
                    }

                    else if (sFieldName.Trim().ToLower() == "name" && (sType == "string" || sType == "long_string"))
                    {
                        epg.Name = DBStrEncode(sVal);
                    }
                    else if (sFieldName.Trim().ToLower() == "start_date" && sType == "datetime")
                    {
                        epg.StartDate = getDateTime(sVal, nCounter, ref  coll, ref bValid);
                    }
                    else if (sFieldName.Trim().ToLower() == "end_date" && sType == "datetime")
                    {
                        epg.EndDate = getDateTime(sVal, nCounter, ref  coll, ref bValid);
                    }
                    else if (sFieldName.Trim().ToLower() == "pic_id" && sType == "int")
                    {
                        bValid = validateParam("int", sVal, -1, -1);
                        int nPicID = 0;
                        int.TryParse(sVal, out nPicID);
                        epg.PicID = nPicID;
                        if (nPicID != 0)
                        {
                            epg.PicUrl = getEpgPicUrl(nPicID);
                        }

                    }
                    else if (sFieldName.Trim().ToLower() == "description" && (sType == "string" || sType == "long_string"))
                    {
                        epg.Description = DBStrEncode(sVal);
                    }
                    else if (sFieldName.Trim().ToLower() == "epg_identifier" && (sType == "string" || sType == "long_string"))
                    {
                        epg.EpgIdentifier = DBStrEncode(sVal);
                    }
                    else if (sFieldName.Trim().ToLower() == "media_id" && sType == "int")
                    {
                        bValid = validateParam("int", sVal, -1, -1);
                        int nMediaID = 0;
                        int.TryParse(sVal, out nMediaID);
                        epg.ExtraData.MediaID = nMediaID;
                    }
                    else if (sType == "file")
                    {
                        doTheWorkOnFile(nCounter, nGroupID, ref coll, ref bValid, ref epg);
                    }

                    nCounter++;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", ex.Message + " || On function: setEpgMeta with epg ID :" + epg.EpgID, "Exceptions");
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }

        private static void doTheWorkOnFile(int nCounter, int nGroupID, ref NameValueCollection coll, ref bool bValid, ref EpgCB epg)
        {

            #region intialize string values from 'coll'
            bool bIsNew = false;
            string sPicName = "";   
            string sFileObjName = nCounter.ToString() + "_val";
            HttpPostedFile theFile = HttpContext.Current.Request.Files[sFileObjName];
            string sUploadedFile = "";
            string sUploadedFileExt = "";

            string sIsImage = "";
            if (coll[nCounter.ToString() + "_isPic"] != null)
            {
                sIsImage = coll[nCounter.ToString() + "_isPic"].ToString();
            }

            string sDirectory = "";
            if (coll[nCounter.ToString() + "_directory"] != null)
                sDirectory = coll[nCounter.ToString() + "_directory"].ToString();

            #endregion

            //check if the file is an image file
            bool bIsImage = false;

            if (sIsImage.Trim().ToUpper() == "TRUE")
                bIsImage = true;

            if (theFile != null && theFile.FileName != "")
            {
                //check if this is a VALID image file               
                if (bIsImage == true && theFile.ContentType.StartsWith("image"))
                {
                    bValid = true;
              
                    //get the name of the file, or generate it if needed                   
                    sPicName = ImageUtils.GetDateImageNameEpg(epg.PicID, ref bIsNew);

                    List<string> lSizes = new List<string>();
                    lSizes.Add("full");

                    sUploadedFile = theFile.FileName;
                    sUploadedFileExt = ImageUtils.GetFileExt(sUploadedFile);     //get the file extension from the file                   

                    #region generate sizes list
                    int count = 0;
                    bool bCont = true;
                    while (bCont && sPicName != "")
                    {
                        if (coll[nCounter.ToString() + "_picDim_width_" + count.ToString()] != null &&
                            coll[nCounter.ToString() + "_picDim_width_" + count.ToString()].Trim().ToString() != "")
                        {
                            string sWidth = coll[nCounter.ToString() + "_picDim_width_" + count.ToString()].ToString();
                            string sHeight = coll[nCounter.ToString() + "_picDim_height_" + count.ToString()].ToString();
                            lSizes.Add(sWidth + "X" + sHeight);
                            count++;
                        }
                        else
                            bCont = false;
                    }
                    string[] sPicSizes = lSizes.ToArray();
                    #endregion
                                       
                    bool succeed = ImageUtils.SendPictureDataToQueue(sUploadedFile, sPicName, sPicSizes, nGroupID); //send to Rabbit

                    epg.PicUrl = sPicName + sUploadedFileExt;
                    updateEpgAndDB(ref epg, ref coll, epg.PicUrl, nGroupID, bIsNew, bValid);
                }
            }
        }

        //update the 'EPG_pics' table in DB and the picID in the epg object
        private static void updateEpgAndDB(ref EpgCB epg, ref NameValueCollection coll, string sUrl, int nGroupID, bool bIsNew, bool bValid)
        {
            //check if this is an existing epg_Pic and update it
            if (bIsNew && bValid && coll["id"] != null && coll["id"].ToString() != "")
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("EPG_pics");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.Now);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sUrl);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", epg.Name);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", epg.Description);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(coll["id"].ToString()));
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                epg.PicID = int.Parse(coll["id"].ToString());
            }
            else //insert a new Epg_pic
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("EPG_pics");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.Now);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sUrl);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", epg.Name);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", epg.Description);

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                //retreive the relevant ID and insert into epg
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select ID from EPG_pics where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sUrl);

                if (selectQuery.Execute("query", true) != null)
                {
                    int nRes = selectQuery.Table("query").DefaultView.Count;
                    if (nRes > 0)
                    {
                        epg.PicID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
        }

        public static string getEpgPicUrl(int nID)
        {
            string sResult = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select BASE_URL from EPG_pics (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    sResult = selectQuery.Table("query").DefaultView[0].Row["BASE_URL"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
            return sResult;


        }

        private static DateTime getDateTime(string sVal, int nCounter, ref NameValueCollection coll, ref bool bValid)
        {
            if (sVal != "")
            {
                string sValMin = coll[nCounter.ToString() + "_valMin"].ToString();
                string sValHour = coll[nCounter.ToString() + "_valHour"].ToString();
                bValid = validateParam("int", sValHour, 0, 23);
                if (bValid == true)
                    bValid = validateParam("int", sValMin, 0, 59);
                if (bValid == true)
                    bValid = validateParam("date", sVal, 0, 59);
                DateTime tTime = DateUtils.GetDateFromStr(sVal);
                if (sValHour == "")
                    sValHour = "0";
                if (sValMin == "")
                    sValMin = "0";
                tTime = tTime.AddHours(int.Parse(sValHour.ToString()));
                tTime = tTime.AddMinutes(int.Parse(sValMin.ToString()));
                bValid = true;
                return tTime;
            }
            else
            {
                bValid = false;
                return DateTime.MinValue;
            }
        }

        private static string GetWSURL(string key)
        {
            return WS_Utils.GetTcmConfigValue(key);
        }
    }
}
