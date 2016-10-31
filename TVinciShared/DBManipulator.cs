using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Uploader;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using QueueWrapper;
using Tvinci.Core.DAL;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for DBManipulator
    /// </summary>
    public class DBManipulator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected const string ROUTING_KEY_PROCESS_IMAGE_UPLOAD = "PROCESS_IMAGE_UPLOAD\\{0}";

        public DBManipulator()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        static public string DBStrEncode(string sToEncode)
        {
            //sToEncode = HttpContext.Current.Server.HtmlEncode(sToEncode);
            sToEncode = sToEncode.Replace("\"", "&quot;");
            sToEncode = sToEncode.Replace("'", "\u0027");
            sToEncode = sToEncode.Replace("<", "&lt;");
            sToEncode = sToEncode.Replace(">", "&gt;");
            return sToEncode;
        }

        static protected bool CheckForbiddenChars(string sStr)
        {
            return true;
        }

        static protected bool validateParam(string sType, string sVal, double nMin, double nMax)
        {
            try
            {
                bool bOK = true;
                if (sType == "string")
                    return CheckForbiddenChars(sVal);
                if (sType == "int" && sVal != "")
                {
                    Int32 nVal = int.Parse(sVal);
                    if (nVal < nMin && nMin != -1)
                        bOK = false;
                    if (nVal > nMax && nMax != -1)
                        bOK = false;
                }
                if (sType == "double" && sVal != "")
                {
                    double nVal = double.Parse(sVal);
                    if (nVal < nMin && nMin != -1)
                        bOK = false;
                    if (nVal > nMax && nMax != -1)
                        bOK = false;
                }
                if (sType == "date")
                {
                    DateTime tTime = DateUtils.GetDateFromStr(sVal);
                }
                return bOK;
            }
            catch
            {
                return false;
            }
        }

        static public DateTime GetEODTime()
        {
            DateTime t = DateTime.UtcNow;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select DAY_START_TIME from site_configuration order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    DateTime db = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["DAY_START_TIME"]);
                    t = new DateTime(1999, 12, 31, db.Hour, db.Minute, 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return t;
        }

        static protected void UpdateTable(string sConnectionKey)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
            string sTableName = coll["table_name"].ToString();
            Int32 nCount = coll.Count;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sTableName);
            updateQuery.SetConnectionKey(sConnectionKey);
            bool bCont = true;
            Int32 nCounter = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select * from ";
            selectQuery += sTableName;
            selectQuery += " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(coll["id"].ToString()));
            bool bCollection = false;
            bool bValid = true;
            while (bCont)
            {
                //if (coll[nCounter.ToString() + "_val"] == null)
                if (coll[nCounter.ToString() + "_type"] == null || bValid == false)
                    break;
                string sVal = "";
                if (coll[nCounter.ToString() + "_val"] != null)
                    sVal = coll[nCounter.ToString() + "_val"].ToString();
                string sType = coll[nCounter.ToString() + "_type"].ToString();
                string sFieldName = coll[nCounter.ToString() + "_field"].ToString();
                if (sFieldName == "")
                {
                    nCounter++;
                    continue;
                }
                try
                {
                    try
                    {
                        if (sFieldName.Trim().ToLower() == "group_id")
                        {

                            bool bBelongs = false;
                            if (nGroupID == 0)
                                bBelongs = false;
                            Int32 nQueryGroupID = int.Parse(sVal.ToString());
                            if (nQueryGroupID != 0 && nQueryGroupID != nGroupID)
                            {
                                PageUtils.DoesGroupIsParentOfGroup(nGroupID, nQueryGroupID, ref bBelongs);
                            }
                            else
                                bBelongs = true;
                            if (bBelongs == false)
                            {
                                LoginManager.LogoutFromSite("login.html");
                                return;
                            }

                            /*
                            bool bOK = PageUtils.DoesGroupIsParentOfGroup(nQueryGroupID);
                            if (bOK == false)
                            {
                                LoginManager.LogoutFromSite("login.html");
                                return;
                            }
                            */
                        }
                    }
                    catch { LoginManager.LogoutFromSite("login.html"); return; }

                    if (sType == "string")
                    {
                        bValid = validateParam("string", sVal, -1, -1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBStrEncode(sVal.ToString()));
                    }
                    if (sType == "long_string")
                    {
                        bValid = validateParam("string", sVal, -1, -1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBStrEncode(sVal.ToString()));
                    }
                    if (sType == "int" && sVal != "")
                    {
                        bValid = validateParam("int", sVal, -1, -1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", int.Parse(sVal.ToString()));
                    }
                    if (sType == "int" && sVal == "")
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBNull.Value);
                    }
                    if (sType == "double" && sVal != "")
                    {
                        bValid = validateParam("double", sVal, -1, -1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", double.Parse(sVal.ToString()));
                    }
                    if (sType == "checkbox")
                    {
                        bValid = true;
                        if (sVal == "on")
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", 1);
                        else
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", 0);
                    }
                    if (sType == "time")
                    {
                        string sVal2 = coll[nCounter.ToString() + "_val2"].ToString();
                        bValid = validateParam("int", sVal, 0, 23);
                        if (bValid == true)
                            bValid = validateParam("int", sVal2, 0, 59);
                        DateTime tTime = new DateTime(1999, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        DateTime tEODTime = GetEODTime();

                        //if (int.Parse(sVal.ToString()) < 7)
                        if (tTime < tEODTime)
                            tTime = new DateTime(2000, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        else
                            tTime = new DateTime(1999, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                    }
                    if (sType == "date")
                    {
                        if (sVal != "")
                        {
                            bValid = validateParam("date", sVal, -1, -1);
                            DateTime tTime = DateUtils.GetDateFromStr(sVal);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                        }
                        else
                        {
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBNull.Value);
                        }
                    }
                    if (sType == "file")
                    {
                        string sPicBaseName = "";
                        string sBasePath = HttpContext.Current.Server.MapPath("");
                        string sPicUploaderPath = GetWSURL("pic_uploader_path");
                        if (!string.IsNullOrEmpty(sPicUploaderPath))
                        {
                            sBasePath = sPicUploaderPath;
                        }
                        string sUploadedFile = "";
                        string sFileObjName = nCounter.ToString() + "_val";
                        string sUploadedFileExt = "";
                        HttpPostedFile theFile = HttpContext.Current.Request.Files[sFileObjName];
                        string sIsImage = coll[nCounter.ToString() + "_isPic"].ToString();
                        string sDirectory = coll[nCounter.ToString() + "_directory"].ToString();
                        string ratioIndex = string.Empty;
                        if (coll[nCounter.ToString() + "_ratioIndex"] != null)
                        {
                            ratioIndex = coll[nCounter.ToString() + "_ratioIndex"].ToString();
                        }
                        log.Debug("Ratio index found - Ratio index is " + ratioIndex);

                        string selectedRatioVal = string.Empty;


                        if (coll[ratioIndex + "_val"] != null && coll[ratioIndex + "_val"].Trim().ToString() != "")
                        {
                            selectedRatioVal = coll[ratioIndex + "_val"].Trim().ToString();
                            log.Debug("Selected Ratio Found - Selected Ratio is :" + selectedRatioVal);
                        }
                        else
                        {
                            log.Debug("Selected Ratio Not Found - ratio index is :" + ratioIndex);
                        }
                        bool bIsImage = false;
                        if (sIsImage.Trim().ToUpper() == "TRUE")
                            bIsImage = true;
                        if (theFile != null && theFile.FileName != "")
                        {

                            bValid = false;
                            if (bIsImage == true)
                            {
                                if (theFile.ContentType.StartsWith("image"))
                                    bValid = true;
                            }
                            else
                            {
                                if (theFile.ContentType.StartsWith("audio") ||
                                    theFile.ContentType.StartsWith("text") ||
                                    theFile.ContentType.StartsWith("image") ||
                                    theFile.ContentType.StartsWith("video") ||
                                    theFile.ContentType == "application/vnd.ms-excel" ||
                                    theFile.ContentType == "application/msword" ||
                                    theFile.ContentType == "application/x-shockwave-flash"
                                    )
                                    bValid = true;
                            }
                            if (bValid == true)
                            {
                                string sUseQueue = TVinciShared.WS_Utils.GetTcmConfigValue("downloadPicWithQueue");
                                if (!string.IsNullOrEmpty(sUseQueue) && sUseQueue.ToLower().Equals("true"))
                                {
                                    #region useRabbitQueue

                                    int mediaID = 0;
                                    if (HttpContext.Current.Session["media_id"] != null)
                                    {
                                        string mediaIdStr = HttpContext.Current.Session["media_id"].ToString();
                                        if (!string.IsNullOrEmpty(mediaIdStr))
                                        {
                                            if (HttpContext.Current.Session["media_file_id"] == null)
                                            {
                                                mediaID = int.Parse(mediaIdStr);
                                            }
                                        }
                                    }

                                    sPicBaseName = ImageUtils.GetDateImageName(mediaID);  //Unique name (new or existing)                                                                                          

                                    sUploadedFile = theFile.FileName;

                                    sUploadedFileExt = ImageUtils.GetFileExt(sUploadedFile);

                                    if (!Directory.Exists(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString()))
                                    {
                                        Directory.CreateDirectory(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString());
                                    }

                                    if (bIsImage == false)
                                    {
                                        string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;

                                        bool bExists = System.IO.File.Exists(sTmpImage);
                                        Int32 nAdd = 0;
                                        while (bExists)
                                        {
                                            if (sPicBaseName.IndexOf("_") != -1)
                                                sPicBaseName = sPicBaseName.Substring(0, sPicBaseName.IndexOf("_"));
                                            sPicBaseName += "_" + nAdd.ToString();
                                            sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;
                                            bExists = System.IO.File.Exists(sTmpImage);
                                            nAdd++;
                                        }

                                        theFile.SaveAs(sTmpImage);

                                        UploadPicToGroup(nGroupID, sTmpImage);
                                    }
                                    else
                                    {
                                        List<string> lSizes = new List<string>();
                                        lSizes.Add("full");
                                        if (coll["6_val"] == "on")
                                        {
                                            lSizes.Add("tn");
                                        }

                                        Int32 nI = 0;
                                        bool bCont1 = true;
                                        //generate the picSizes list
                                        while (bCont1 && sPicBaseName != "")
                                        {
                                            if (coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].Trim().ToString() != "")
                                            {
                                                bool isResize = true;
                                                string sRatio = string.Empty;
                                                string sWidth = coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].ToString();
                                                string sHeight = coll[nCounter.ToString() + "_picDim_height_" + nI.ToString()].ToString();
                                                string sEndName = coll[nCounter.ToString() + "_picDim_endname_" + nI.ToString()].ToString();

                                                if (coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].Trim().ToString() != "")
                                                {
                                                    sRatio = coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].ToString();
                                                    log.Debug("Ratio found - Ratio is :" + sRatio);
                                                    if (!string.IsNullOrEmpty(selectedRatioVal) && sRatio != selectedRatioVal)
                                                    {
                                                        log.Debug("Ratio un-matched - " + selectedRatioVal);
                                                        isResize = false;
                                                    }
                                                    else
                                                    {
                                                        log.Debug("Ratio matched - for: " + sDirectory + "/" + sPicBaseName + sEndName);
                                                    }
                                                }
                                                else
                                                {
                                                    log.Debug("Ratio not found - for: " + sDirectory + "/" + sPicBaseName + sEndName);
                                                }
                                                if (isResize)
                                                {
                                                    lSizes.Add(sWidth + "X" + sHeight);
                                                }
                                                nI++;
                                            }
                                            else
                                                bCont1 = false;
                                        }

                                        string[] sPicSizes = lSizes.ToArray();
                                        bool succeed = ImageUtils.SendPictureDataToQueue(sUploadedFile, sPicBaseName, sBasePath, sPicSizes, nGroupID);//send to Rabbit
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region useUploader
                                    int mediaID = 0;
                                    if (HttpContext.Current.Session["media_id"] != null)
                                    {
                                        string mediaIdStr = HttpContext.Current.Session["media_id"].ToString();
                                        if (!string.IsNullOrEmpty(mediaIdStr))
                                        {
                                            if (HttpContext.Current.Session["media_file_id"] == null)
                                            {
                                                mediaID = int.Parse(mediaIdStr);
                                            }
                                        }
                                    }
                                    if (mediaID > 0)
                                    {
                                        sPicBaseName = ImageUtils.GetDateImageName(mediaID);
                                    }
                                    else
                                    {
                                        sPicBaseName = ImageUtils.GetDateImageName();
                                    }

                                    if (!Directory.Exists(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString()))
                                    {
                                        Directory.CreateDirectory(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString());
                                    }


                                    sUploadedFile = theFile.FileName;
                                    int nExtractPos = sUploadedFile.LastIndexOf(".");
                                    if (nExtractPos > 0)
                                        sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                                    if (bIsImage == false)
                                    {
                                        string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;

                                        bool bExists = System.IO.File.Exists(sTmpImage);
                                        Int32 nAdd = 0;
                                        while (bExists)
                                        {
                                            if (sPicBaseName.IndexOf("_") != -1)
                                                sPicBaseName = sPicBaseName.Substring(0, sPicBaseName.IndexOf("_"));
                                            sPicBaseName += "_" + nAdd.ToString();
                                            sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;
                                            bExists = System.IO.File.Exists(sTmpImage);
                                            nAdd++;
                                        }

                                        theFile.SaveAs(sTmpImage);

                                        UploadPicToGroup(nGroupID, sTmpImage);
                                    }
                                    else
                                    {
                                        string sFullImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_full" + sUploadedFileExt;
                                        bool bExists = System.IO.File.Exists(sFullImage);
                                        theFile.SaveAs(sFullImage);
                                        UploadPicToGroup(nGroupID, sFullImage);

                                        //add a "tn" size upload to pictre size at FTP 
                                        if (coll["6_val"] == "on" && !string.IsNullOrEmpty(sPicBaseName))
                                        {
                                            string sTNImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_tn" + sUploadedFileExt;
                                            ImageUtils.ResizeImageAndSave(sFullImage, sTNImage, 90, 65, true, true);
                                            UploadPicToGroup(nGroupID, sTNImage);
                                        }

                                        Int32 nI = 0;
                                        bool bCont1 = true;
                                        while (bCont1 && sPicBaseName != "")
                                        {
                                            if (coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].Trim().ToString() != "")
                                            {
                                                bool isResize = true;
                                                string sRatio = string.Empty;
                                                string sWidth = coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].ToString();
                                                string sHeight = coll[nCounter.ToString() + "_picDim_height_" + nI.ToString()].ToString();
                                                string sEndName = coll[nCounter.ToString() + "_picDim_endname_" + nI.ToString()].ToString();
                                                string sCropName = coll[nCounter.ToString() + "_crop_" + nI.ToString()].ToString();
                                                string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                                                if (coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].Trim().ToString() != "")
                                                {
                                                    sRatio = coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].ToString();
                                                    log.Debug("Ratio found - Ratio is :" + sRatio);
                                                    if (!string.IsNullOrEmpty(selectedRatioVal) && sRatio != selectedRatioVal)
                                                    {
                                                        log.Debug("Ratio un-matched - " + sTmpImage);
                                                        isResize = false;
                                                    }
                                                    else
                                                    {
                                                        log.Debug("Ratio matched - " + sTmpImage);
                                                    }
                                                }
                                                else
                                                {
                                                    log.Debug("Ratio not found - " + sTmpImage);
                                                }
                                                if (isResize)
                                                {
                                                    ImageUtils.ResizeImageAndSave(sFullImage, sTmpImage, int.Parse(sWidth), int.Parse(sHeight), bool.Parse(sCropName), true);

                                                    UploadPicToGroup(nGroupID, sTmpImage);
                                                }
                                                nI++;
                                            }
                                            else
                                                bCont1 = false;
                                        }



                                    }
                                    #endregion
                                }
                            }

                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", sPicBaseName + sUploadedFileExt);
                        }
                    }
                    if (sType == "datetime")
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
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                        }
                        else
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBNull.Value);
                    }
                }
                catch
                {
                    selectQuery.Finish();
                    selectQuery = null;
                    updateQuery.Finish();
                    updateQuery = null;
                    HttpContext.Current.Session["error_msg"] = "* הנתונים שהוSendו אינם חוקיים או מלאים";
                    return;
                }
                if (sType == "multi")
                    bCollection = true;
                nCounter++;
            }
            if (bValid == true)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(coll["id"].ToString()));
                updateQuery.Execute();

                if (bCollection == true)
                {
                    if (selectQuery.Execute("query", true) != null)
                        HandleMany2Many(ref selectQuery, sConnectionKey);
                }
            }
            else
            {
                HttpContext.Current.Session["error_msg"] = "* הנתונים שהוSendו אינם חוקיים או מלאים";
            }
            updateQuery.Finish();
            updateQuery = null;
            selectQuery.Finish();
            selectQuery = null;
        }

        static protected void DropOldMany2Many(string sMiddleTable, string sMiddleFieldRefToMain, object mainPointerValue, string sListIDs, Int32 nGroupID, string sConnectionKey)
        {
            //Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sMiddleTable);
            updateQuery.SetConnectionKey(sConnectionKey);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToMain, "=", mainPointerValue);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "<", 3);
            if (sMiddleFieldRefToMain.Trim().ToLower() != "group_id" && sMiddleTable.Trim().ToLower() != "watch_permissions_types_groups")
                updateQuery += "and group_id " + sGroups;
            //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (sListIDs != "")
            {
                updateQuery += sListIDs;
            }
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery(sMiddleTable);
            updateQuery1.SetConnectionKey(sConnectionKey);
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
            updateQuery1 += " where ";
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToMain, "=", mainPointerValue);
            updateQuery1 += "and";
            updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 3);
            if (sMiddleFieldRefToMain.Trim().ToLower() != "group_id" && sMiddleTable.Trim().ToLower() != "watch_permissions_types_groups")
                updateQuery1 += "and group_id " + sGroups;
            //updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (sListIDs != "")
            {
                updateQuery1 += sListIDs;
            }
            updateQuery1.Execute();
            updateQuery1.Finish();
            updateQuery1 = null;
        }

        static protected void DropOldMany2Many(string sMiddleTable, string sMiddleFieldRefToMain, object mainPointerValue, string sListIDs, string sConnectionKey)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            DropOldMany2Many(sMiddleTable, sMiddleFieldRefToMain, mainPointerValue, sListIDs, nGroupID, sConnectionKey);
        }

        static protected void InsertNewMany2Many(string sMiddleTable, string sMiddleFieldRefToMain, string sMiddleFieldRefToCollection, object mainPointerValue, object collectionPointreValue, string sConnectionKey)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            InsertNewMany2Many(sMiddleTable, sMiddleFieldRefToMain, sMiddleFieldRefToCollection, mainPointerValue, collectionPointreValue, nGroupID, sConnectionKey);
        }

        static protected void InsertNewMany2Many(string sMiddleTable, string sMiddleFieldRefToMain, string sMiddleFieldRefToCollection, object mainPointerValue, object collectionPointreValue, Int32 nGroupID, string sConnectionKey)
        {
            //Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
            Int32 nCo = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select id as co from " + sMiddleTable + " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToMain, "=", mainPointerValue);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToCollection, "=", collectionPointreValue);
            if (sMiddleFieldRefToCollection.Trim().ToLower() != "group_id" && sMiddleFieldRefToMain.Trim().ToLower() != "group_id")
                selectQuery += "and group_id " + sGroups;
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCo > 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sMiddleTable);
                updateQuery.SetConnectionKey(sConnectionKey);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 3);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCo);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            else
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery(sMiddleTable);
                insertQuery.SetConnectionKey(sConnectionKey);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToMain, "=", mainPointerValue);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sMiddleFieldRefToCollection, "=", collectionPointreValue);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 3);
                if (sMiddleFieldRefToCollection.ToLower().Trim() != "group_id" && sMiddleFieldRefToMain.ToLower().Trim() != "group_id")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
        }

        //sMainPointerField (the field in the main table that the middle table points to)
        //sExtraFieldName
        //sExtraFieldVal
        //sExtraFieldType
        //sCollectionPointerField
        //sCollectionTable
        //sMiddleTable
        //sMiddleFieldRefToMain
        //sMiddleFieldRefToCollection
        //sAddExtra

        static public void GetManyToManyContainer(ref System.Collections.Specialized.NameValueCollection coll,
            string sMainPointerField,
            string sExtraFieldName,
            string sExtraFieldVal,
            string sExtraFieldType,
            string sCollectionPointerField,
            string sCollectionTable,
            string sMiddleTable,
            string sMiddleFieldRefToMain,
            string sMiddleFieldRefToCollection,
            string sAddExtra,
            string sMainPointerID,
            string sVal,
            Int32 nGroupID)
        {
            Int32 nCounter = coll.Count;
            //System.Collections.Specialized.NameValueCollection coll = new System.Collections.Specialized.NameValueCollection();
            coll[nCounter.ToString() + "_type"] = "multi";
            coll[nCounter.ToString() + "_field"] = "value";
            coll[nCounter.ToString() + "_main_pointer_field"] = sMainPointerField;
            coll[nCounter.ToString() + "_extra_field_name"] = sExtraFieldName;
            coll[nCounter.ToString() + "_extra_field_val"] = sExtraFieldVal;
            coll[nCounter.ToString() + "_extra_field_type"] = sExtraFieldType;
            coll[nCounter.ToString() + "_collection_pointer_field"] = sCollectionPointerField;
            coll[nCounter.ToString() + "_collection_table"] = sCollectionTable;
            coll[nCounter.ToString() + "_middle_table"] = sMiddleTable;
            coll[nCounter.ToString() + "_middle_ref_main_field"] = sMiddleFieldRefToMain;
            coll[nCounter.ToString() + "_middle_ref_collection_field"] = sMiddleFieldRefToCollection;
            coll[nCounter.ToString() + "_collection_auto_add"] = sAddExtra;
            coll[nCounter.ToString() + "_value_to_enter"] = sMainPointerID;
            coll[nCounter.ToString() + "_val"] = sVal;
            coll[nCounter.ToString() + "_group"] = nGroupID.ToString();

        }

        static protected void HandleMany2Many(ref ODBCWrapper.DataSetSelectQuery selectQuery)
        {
            HandleMany2Many(ref selectQuery, "");
        }

        static protected void HandleMany2Many(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sConnectionKey)
        {
            System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
            HandleMany2Many(ref coll, ref selectQuery, sConnectionKey);
        }

        static public void HandleMany2Many(ref System.Collections.Specialized.NameValueCollection coll, ref ODBCWrapper.DataSetSelectQuery selectQuery, string sConnectionKey)
        {
            //string sTableName = coll["table_name"].ToString();
            Int32 nCount = coll.Count;
            bool bCont = true;
            Int32 nCounter = 0;
            while (bCont)
            {
                if (coll[nCounter.ToString() + "_type"] == null)
                    break;
                string sType = coll[nCounter.ToString() + "_type"].ToString();
                string sVal = "";
                if (coll[nCounter.ToString() + "_val"] != null)
                    sVal = coll[nCounter.ToString() + "_val"].ToString().Trim();
                string sCollectionFieldName = coll[nCounter.ToString() + "_field"].ToString();

                //            string sFieldName = m_sFieldName;
                if (sCollectionFieldName.IndexOf("+") != -1)
                {
                    string[] splited = sCollectionFieldName.Split('+');
                    sCollectionFieldName = "(";
                    for (int j = 0; j < splited.Length; j++)
                    {
                        if (j > 0)
                            sCollectionFieldName += "+' '+";
                        sCollectionFieldName += splited[j];
                    }
                    sCollectionFieldName += ")";
                }

                if (sType != "multi")
                {
                    nCounter++;
                    continue;
                }
                // Here start the handling of many to many
                string sMainPointerField = coll[nCounter.ToString() + "_main_pointer_field"].ToString();
                string sExtraFieldName = coll[nCounter.ToString() + "_extra_field_name"].ToString();
                string sExtraFieldVal = coll[nCounter.ToString() + "_extra_field_val"].ToString();
                string sExtraFieldType = coll[nCounter.ToString() + "_extra_field_type"].ToString();
                string sCollectionPointerField = coll[nCounter.ToString() + "_collection_pointer_field"].ToString();
                string sCollectionTable = coll[nCounter.ToString() + "_collection_table"].ToString();
                string sMiddleTable = coll[nCounter.ToString() + "_middle_table"].ToString();
                string sMiddleFieldRefToMain = coll[nCounter.ToString() + "_middle_ref_main_field"].ToString();
                string sMiddleFieldRefToCollection = coll[nCounter.ToString() + "_middle_ref_collection_field"].ToString();
                string sAddExtra = coll[nCounter.ToString() + "_collection_auto_add"].ToString();
                string sMiddleTableType = string.Empty;

                if (coll[nCounter.ToString() + "_middle_table_type"] != null)
                {
                    sMiddleTableType = coll[nCounter.ToString() + "_middle_table_type"].ToString();
                }


                object mainPointerValue = null;
                if (selectQuery != null)
                    mainPointerValue = selectQuery.Table("query").DefaultView[0].Row[sMainPointerField];
                else
                {
                    try
                    {
                        mainPointerValue = int.Parse(coll[nCounter.ToString() + "_value_to_enter"].ToString());
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception - On function: mainPointerValue = int.Parse(coll[nCounter.ToString() + '_value_to_enter'].ToString());", ex);
                    }
                }
                //char[] t = { ';', ',', ':' };
                //char[] t = { ';', ','};
                char[] t = { ';' };
                string[] splitedStr = sVal.Split(t);
                Int32 nGroupID = LoginManager.GetLoginGroupID();

                if (nGroupID == 0)
                {
                    if (coll[nCounter.ToString() + "_group"] != null)
                    {
                        try
                        {
                            nGroupID = int.Parse(coll[nCounter.ToString() + "_group"].ToString().Trim());
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - On function: nGroupID = int.Parse(coll[nCounter.ToString() + '_group'].ToString().Trim());", ex);
                            return;
                        }
                    }
                }

                int collectionTableGroupId = nGroupID;

                // This is a special case when defining tags on PARENT group.
                // The tag (collection) table is on CHILD group
                // Therefore we will look for the group of the TYPE of the tags
                // And this will be the CORRECT GROUP ID
                if (!string.IsNullOrEmpty(sMiddleTableType) &&
                    (sCollectionTable.ToLower() == "tags" || sCollectionTable.ToLower() == "epg_tags"))
                {
                    object correctGroup = ODBCWrapper.Utils.GetTableSingleVal(sMiddleTableType, "GROUP_ID", int.Parse(sExtraFieldVal));

                    if (correctGroup != null && correctGroup != DBNull.Value)
                    {
                        collectionTableGroupId = Convert.ToInt32(correctGroup);
                    }
                }

                for (int i = 0; i < splitedStr.Length; i++)
                {
                    bool bEntered = false;
                    Int32 nRound = 0;
                    while (bEntered == false)
                    {
                        string sText = splitedStr[i].ToString().Trim().Replace("\"", "&quot;");
                        if (sText == "")
                        {
                            break;
                        }
                        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                        selectQuery1.SetConnectionKey(sConnectionKey);
                        selectQuery1 += "select " + sCollectionPointerField + " from " + sCollectionTable + " where status<>2 and ";
                        if (sCollectionFieldName.ToLower() != "id")
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sCollectionFieldName, "=", sText);
                        else
                        {
                            try
                            {
                                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sCollectionFieldName, "=", int.Parse(sText));
                            }
                            catch (Exception ex)
                            {
                                log.Error("Exception - On function: selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sCollectionFieldName, ' = ', int.Parse(sText)); stext=" + sText, ex);
                                return;
                            }
                        }

                        if (sExtraFieldType != "")
                        {
                            if (sExtraFieldType == "int")
                            {
                                Int32 nToEnter = int.Parse(sExtraFieldVal);
                                selectQuery1 += "and";
                                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sExtraFieldName, "=", nToEnter);
                            }
                            if (sExtraFieldType == "string")
                            {
                                string sToEnter = sExtraFieldVal;
                                selectQuery1 += "and";
                                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sExtraFieldName, "=", sToEnter);
                            }
                        }

                        if (sCollectionTable != "groups" && sCollectionTable != "countries" && sCollectionTable != "lu_countries" &&
                            sCollectionTable != "lu_languages" && sCollectionTable != "lu_page_types" && sCollectionTable != "lu_pics_ratios" && sCollectionTable != "lu_pics_epg_ratios" && sCollectionTable.ToLower() != "lu_devicebrands")
                        {
                            selectQuery1 += " and ";

                            if (collectionTableGroupId != nGroupID)
                            {
                                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", collectionTableGroupId);
                            }
                            else
                            {
                                if (sCollectionTable != "channels" && sCollectionTable != "categories")
                                    selectQuery1 += " group_id " + PageUtils.GetParentsGroupsStr(nGroupID);
                                else if (sCollectionTable == "channels" && sMiddleTable == "categories_channels")
                                {
                                    selectQuery1 += " group_id " + PageUtils.GetAllGroupTreeStr(nGroupID);
                                }
                                else
                                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            }
                        }

                        if (selectQuery1.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {
                                object collectionPointerValue = selectQuery1.Table("query").DefaultView[0].Row[sCollectionPointerField];
                                InsertNewMany2Many(sMiddleTable, sMiddleFieldRefToMain, sMiddleFieldRefToCollection, mainPointerValue, collectionPointerValue, nGroupID, sConnectionKey);
                                bEntered = true;
                            }
                        }
                        selectQuery1.Finish();
                        selectQuery1 = null;

                        if (bEntered == false)
                        {
                            if (sAddExtra.ToLower() == "false" || nRound == 1)
                            {
                                bEntered = true;
                            }
                            else
                            {
                                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery(sCollectionTable);
                                insertQuery.SetConnectionKey(sConnectionKey);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sCollectionFieldName, "=", DBStrEncode(sText.Trim()));
                                if (sExtraFieldType != "")
                                {
                                    if (sExtraFieldType == "int")
                                    {
                                        Int32 nToEnter = int.Parse(sExtraFieldVal);
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sExtraFieldName, "=", nToEnter);
                                    }
                                    if (sExtraFieldType == "string")
                                    {
                                        string sToEnter = sExtraFieldVal;
                                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sExtraFieldName, "=", sToEnter);
                                    }
                                }

                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", collectionTableGroupId);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
                                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                                insertQuery.Execute();
                                insertQuery.Finish();
                                insertQuery = null;

                                if (sCollectionTable.Trim().ToLower() == "tags" && sExtraFieldVal != "")
                                {
                                    string sURL = "";
                                    string sTagType = "";
                                    string sXML = "";
                                    ODBCWrapper.DataSetSelectQuery selectQuery3 = new ODBCWrapper.DataSetSelectQuery();
                                    selectQuery3.SetConnectionKey(sConnectionKey);
                                    selectQuery3 += "select g.TAGS_NOTIFY_URL from groups g where ";
                                    selectQuery3 += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", collectionTableGroupId);
                                    if (sExtraFieldVal != "")
                                        sTagType = ODBCWrapper.Utils.GetTableSingleVal("media_tags_types", "name", int.Parse(sExtraFieldVal)).ToString();
                                    if (selectQuery3.Execute("query", true) != null)
                                    {
                                        Int32 nCount3 = selectQuery3.Table("query").DefaultView.Count;
                                        if (nCount3 > 0)
                                        {
                                            object oNURL = selectQuery3.Table("query").DefaultView[0].Row["TAGS_NOTIFY_URL"];
                                            if (oNURL != DBNull.Value && oNURL != null)
                                                sURL = oNURL.ToString();
                                        }
                                    }
                                    selectQuery3.Finish();
                                    selectQuery3 = null;
                                    if (sURL != "")
                                    {
                                        sXML = "<notification type=\"tag_new\">";
                                        sXML += "<tag type=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTagType, true) + "\" value=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sText, true) + "\">";
                                        sXML += "</tag>";
                                        sXML += "</notification>";
                                        log.Debug("Notification - " + sURL + " : " + sXML);
                                        //Notify here
                                        Notifier tt = new Notifier(sURL, sXML);
                                        ThreadStart job = new ThreadStart(tt.Notify);
                                        Thread thread = new Thread(job);
                                        thread.Start();
                                    }
                                    BuildFictivicMedia(sTagType, sText, 0, nGroupID);
                                }
                            }
                            nRound = 1;
                        }
                    }
                }
                string sListIDs = "";
                if (sExtraFieldType != "")
                {
                    sListIDs = "and " + sMiddleFieldRefToCollection + " in (";
                    sListIDs += "select " + sCollectionPointerField + " from " + sCollectionTable + " where status<>2 ";
                    if (sExtraFieldType == "int")
                    {
                        sListIDs += " and ";
                        sListIDs += sExtraFieldName + "=" + sExtraFieldVal;
                    }
                    if (sExtraFieldType == "string")
                    {
                        sListIDs += " and ";
                        sListIDs += sExtraFieldName + "='" + sExtraFieldVal + "'";
                    }
                    sListIDs += " and ";
                    string sGroups = PageUtils.GetParentsGroupsStr(collectionTableGroupId);
                    sListIDs += "group_id " + sGroups;
                    sListIDs += ")";
                }
                DropOldMany2Many(sMiddleTable, sMiddleFieldRefToMain, mainPointerValue, sListIDs, nGroupID, sConnectionKey);
                nCounter++;
            }
        }

        private static string GetFictivicDoubleMeta(int fictivicGroupID, string metaNameVal)
        {
            string retVal = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", fictivicGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        string metaName = string.Format("META{0}_DOUBLE_NAME", i.ToString());
                        if (selectQuery.Table("query").DefaultView[0].Row[metaName] != System.DBNull.Value && selectQuery.Table("query").DefaultView[0].Row[metaName] != null)
                        {
                            string metaVal = selectQuery.Table("query").DefaultView[0].Row[metaName].ToString();
                            if (!string.IsNullOrEmpty(metaVal) && metaVal.ToLower().Equals(metaNameVal.ToLower()))
                            {
                                retVal = string.Format("META{0}_DOUBLE", i.ToString());
                                break;
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        private static string GetFictivicStrMeta(int fictivicGroupID, string metaNameVal)
        {
            string retVal = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", fictivicGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 1; i < 21; i++)
                    {
                        string metaName = string.Format("META{0}_STR_NAME", i.ToString());
                        if (selectQuery.Table("query").DefaultView[0].Row[metaName] != System.DBNull.Value && selectQuery.Table("query").DefaultView[0].Row[metaName] != null)
                        {
                            string metaVal = selectQuery.Table("query").DefaultView[0].Row[metaName].ToString();
                            if (!string.IsNullOrEmpty(metaVal) && metaVal.ToLower().Equals(metaNameVal.ToLower()))
                            {
                                retVal = string.Format("META{0}_STR", i.ToString());
                                break;
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        public static int BuildFictivicMedia(string sTagType, string sText, Int32 nBaseID, Int32 nGroupID)
        {
            return BuildOrUpdateFictivicMedia(sTagType, sText, nBaseID, nGroupID, string.Empty);
        }

        /*
         * If you wish to update a media, send the old media name as parameter as sOldText.
         * If you wish to create a media, send string.Empty as sOldText
         * 
         * 
         */
        static public int BuildOrUpdateFictivicMedia(string sTagType, string sNewText, Int32 nBaseID, Int32 nGroupID, string sOldText)
        {
            int retVal = 0;
            string sCoGuid = string.Empty;
            try
            {
                string sTypeInParentGroup = "";
                Int32 nFictivicGroupID = 0;
                Int32 nTagTypeID = 0;
                if (CachingManager.CachingManager.Exist("BuildOrUpdateFictivicMedia" + nGroupID.ToString() + sTagType) == true)
                {
                    string[] theCached = (string[])(CachingManager.CachingManager.GetCachedData("BuildOrUpdateFictivicMedia" + nGroupID.ToString() + sTagType));
                    sTypeInParentGroup = theCached[0];
                    nTagTypeID = int.Parse(theCached[1]);
                    nFictivicGroupID = int.Parse(theCached[2]);
                }
                else
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += " select gfm.RELATED_TYPE,g.FICTIVIC_GROUP_ID from groups g,groups_fictivic_metas gfm where gfm.status=1 and gfm.is_active=1 and gfm.group_id=g.id and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gfm.group_id", "=", nGroupID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gfm.ORIGIN_META_NAME", "=", sTagType);
                    selectQuery += " order by gfm.id desc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sTypeInParentGroup = selectQuery.Table("query").DefaultView[0].Row["RELATED_TYPE"].ToString();
                            nFictivicGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["FICTIVIC_GROUP_ID"].ToString());
                            nTagTypeID = IngestionUtils.GetMediaTypeID(sTypeInParentGroup, nFictivicGroupID);
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    if (nFictivicGroupID == 0 || nTagTypeID == 0)
                        return retVal;

                    string[] theCached = new string[3];
                    theCached[0] = sTypeInParentGroup;
                    theCached[1] = nTagTypeID.ToString();
                    theCached[2] = nFictivicGroupID.ToString();
                    CachingManager.CachingManager.SetCachedData("BuildOrUpdateFictivicMedia" + nGroupID.ToString() + sTagType, theCached, 10800, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
                }

                Dictionary<string, string> sFictivicMetaRetVal = new Dictionary<string, string>();
                sFictivicMetaRetVal["Base Type"] = GetFictivicStrMeta(nFictivicGroupID, "Base Type");
                sFictivicMetaRetVal["Base Group ID"] = GetFictivicDoubleMeta(nFictivicGroupID, "Base Group ID");
                sFictivicMetaRetVal["Base ID"] = GetFictivicDoubleMeta(nFictivicGroupID, "Base ID");
                sFictivicMetaRetVal["Base Type"] = string.IsNullOrEmpty(sFictivicMetaRetVal["Base Type"]) ? "META1_STR" : sFictivicMetaRetVal["Base Type"];
                sFictivicMetaRetVal["Base Group ID"] = string.IsNullOrEmpty(sFictivicMetaRetVal["Base Group ID"]) ? "META1_DOUBLE" : sFictivicMetaRetVal["Base Group ID"];
                sFictivicMetaRetVal["Base ID"] = string.IsNullOrEmpty(sFictivicMetaRetVal["Base ID"]) ? "META2_DOUBLE" : sFictivicMetaRetVal["Base ID"];

                if (string.IsNullOrEmpty(sOldText))
                { // new fictivic media.
                    retVal = GetMediaIDByName(nFictivicGroupID, sNewText, nTagTypeID);
                    if (retVal == 0)
                    {
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sNewText);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nFictivicGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base Type"], "=", sTagType);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base Group ID"], "=", nGroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base ID"], "=", nBaseID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nTagTypeID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                    }
                }
                else
                { // update an existing fictivic media
                    retVal = GetMediaIDByName(nFictivicGroupID, sOldText, nTagTypeID);
                    if (retVal != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("media");
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sNewText);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nFictivicGroupID);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base Type"], "=", sTagType);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base Group ID"], "=", nGroupID);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM(sFictivicMetaRetVal["Base ID"], "=", nBaseID);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nTagTypeID);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                        updateQuery1 += " where ";
                        updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", retVal);
                        updateQuery1.Execute();
                        updateQuery1.Finish();
                        updateQuery1 = null;


                    }
                }
                GetCoGuid(sTagType, nFictivicGroupID, sNewText, nGroupID, nBaseID, nTagTypeID, 1, false, ref retVal, ref sCoGuid);
                if (retVal > 0 && string.IsNullOrEmpty(sCoGuid))
                    UpdateCoGuid(retVal);

            }
            catch (Exception ex)
            {
                log.Error("Exception - " + ex.Message + " || On function: BuildOrUpdateFictivicMedia(string sTagType, string sNewText, Int32 nGroupID, string sOldText):" + sTagType + " | " + sNewText + " | " + nGroupID.ToString() + "|" + sOldText, ex);
            }
            return retVal;
        }

        private static void GetCoGuid(string sTagType, int nFictivicGroupID, string sNewText, int nBaseGroupID, int nBaseID, int nTagTypeID, int nIsActiveZeroOrOne, bool bIsUseIsActiveInSelect, ref int retVal, ref string sCoGuid)
        {
            string res = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id, co_guid from media where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sNewText);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nFictivicGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(GetFictivicStrMeta(nFictivicGroupID, "Base Type"), "=", sTagType);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(GetFictivicDoubleMeta(nFictivicGroupID, "Base Group ID"), "=", nBaseGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(GetFictivicDoubleMeta(nFictivicGroupID, "Base ID"), "=", nBaseID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nTagTypeID);
            if (bIsUseIsActiveInSelect)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActiveZeroOrOne);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    object oCoGuid = selectQuery.Table("query").DefaultView[0].Row["co_guid"];
                    if (oCoGuid != null && oCoGuid != DBNull.Value)
                        sCoGuid = oCoGuid.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }

        private static void UpdateCoGuid(int retVal)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", retVal);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", retVal);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected Int32 GetMediaIDByName(Int32 nGroupID, string sName, Int32 nTagTypeID)
        {
            Int32 nMediaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from media (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", nTagTypeID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nMediaID;
        }

        static protected bool CheckUnique(string sTableName,
            string sUniqueField,
            string sUniqueVal,
            string sUniqueType)
        {
            bool bRet = true;
            if (sUniqueField == "")
                return true;
            if (sUniqueType != "int" && sUniqueType != "string" && sUniqueType != "double")
                return true;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from " + sTableName;
            if (sUniqueType == "int")
            {
                selectQuery += " where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sUniqueField, "=", int.Parse(sUniqueVal));
                selectQuery += " and status<>2";
            }
            if (sUniqueType == "string")
            {
                selectQuery += " where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sUniqueField, "=", sUniqueVal);
                selectQuery += " and status<>2";
            }
            if (sUniqueType == "double")
            {
                selectQuery += " where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sUniqueField, "=", double.Parse(sUniqueVal));
                selectQuery += " and status<>2";
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                    if (nCo > 0)
                        bRet = false;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        static public void UploadPicToGroup(int groupID, string sPicName)
        {
            UploadPicToGroup(groupID, sPicName, false);
        }

        static public void UploadPicToGroup(int groupID, string sPicName, bool isDelete)
        {
            BaseUploader uploader = UploaderFactory.GetUploader(groupID);

            if (uploader != null)
            {
                ThreadStart job = delegate { uploader.Upload(sPicName, isDelete); };
                Thread thread = new Thread(job);
                thread.Start();
            }
        }

        static public void UploadDirectoryToGroup(int groupID, string sDirectoryName)
        {
            BaseUploader uploader = UploaderFactory.GetUploader(groupID);

            if (uploader != null)
            {
                ThreadStart job = delegate { uploader.UploadDirectory(sDirectoryName); };
                Thread thread = new Thread(job);
                thread.Start();
            }
        }

        static protected Int32 InsertTable(string sConnectionKey)
        {

            Int32 nGroupID = LoginManager.GetLoginGroupID();

            Int32 nID = 0;
            System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
            string sUniquField = "";
            string sUniqueValue = "";
            string sUniqueType = "";
            if (coll["unique_field"] != null && coll["unique_field"].ToString() != "")
            {
                sUniquField = coll["unique_field"].ToString();
            }
            string sTableName = coll["table_name"].ToString();
            Int32 nCount = coll.Count;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select * from ";
            selectQuery += sTableName;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery(sTableName);
            insertQuery.SetConnectionKey(sConnectionKey);
            bool bCont = true;
            Int32 nCounter = 0;
            bool bCollection = false;
            bool bFirst = true;
            bool bValid = true;
            string selectedRatio = string.Empty;
            while (bCont)
            {
                if (coll[nCounter.ToString() + "_type"] == null || bValid == false)
                    break;
                string sVal = "";
                if (coll[nCounter.ToString() + "_val"] != null)
                    sVal = coll[nCounter.ToString() + "_val"].ToString().Trim();
                string sVal2 = "";
                string sType = coll[nCounter.ToString() + "_type"].ToString();
                string sFieldName = coll[nCounter.ToString() + "_field"].ToString();
                if (sFieldName == "")
                {
                    nCounter++;
                    continue;
                }
                if (sUniquField.ToUpper() == sFieldName.ToUpper())
                {
                    sUniqueValue = sVal;
                    sUniqueType = sType;
                }
                try
                {
                    try
                    {
                        if (sFieldName.Trim().ToLower() == "group_id")
                        {
                            /*
                            Int32 nQueryGroupID = int.Parse(sVal.ToString());
                            bool bOK = PageUtils.DoesGroupIsParentOfGroup(nQueryGroupID);
                            if (bOK == false)
                            {
                                LoginManager.LogoutFromSite("login.html");
                                return 0;
                            }
                            */
                            bool bBelongs = false;
                            if (nGroupID == 0)
                                bBelongs = false;
                            Int32 nQueryGroupID = int.Parse(sVal.ToString());
                            if (nQueryGroupID != 0 && nQueryGroupID != nGroupID)
                            {
                                PageUtils.DoesGroupIsParentOfGroup(nGroupID, nQueryGroupID, ref bBelongs);
                            }
                            else
                                bBelongs = true;
                            if (bBelongs == false)
                            {
                                LoginManager.LogoutFromSite("login.html");
                                return 0;
                            }
                        }
                    }
                    catch { LoginManager.LogoutFromSite("login.html"); return 0; }
                    if (sType == "string")
                    {
                        sVal = sVal.Replace("\r\n", "<br\\>");
                        bValid = validateParam("string", sVal, -1, -1);
                        if (bFirst == false)
                            selectQuery += "and";
                        else
                            selectQuery += "where";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBStrEncode(sVal.ToString()));
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBStrEncode(sVal.ToString()));
                        bFirst = false;
                    }
                    if (sType == "long_string")
                    {
                        sVal = sVal.Replace("\r\n", "&lt;br\\&gt;");
                        bValid = validateParam("string", sVal, -1, -1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", sVal.ToString());
                    }
                    if (sType == "int")
                    {
                        if (sVal != "")
                        {
                            bValid = validateParam("int", sVal, -1, -1);
                            if (bFirst == false)
                                selectQuery += "and";
                            else
                                selectQuery += "where";
                            bFirst = false;
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", int.Parse(sVal.ToString()));
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", int.Parse(sVal.ToString()));
                        }
                    }
                    if (sType == "double")
                    {
                        if (sVal != "")
                        {
                            bValid = validateParam("double", sVal, -1, -1);
                            if (bFirst == false)
                                selectQuery += "and";
                            else
                                selectQuery += "where";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", double.Parse(sVal.ToString()));
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", double.Parse(sVal.ToString()));
                            bFirst = false;
                        }
                    }
                    if (sType == "checkbox")
                    {
                        bValid = true;
                        if (bFirst == false)
                            selectQuery += "and";
                        else
                            selectQuery += "where";
                        Int32 nVal = 0;
                        if (sVal == "on")
                            nVal = 1;
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", nVal);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", nVal);
                        bFirst = false;
                    }
                    if (sType == "time")
                    {
                        sVal2 = coll[nCounter.ToString() + "_val2"].ToString();
                        bValid = validateParam("int", sVal, 0, 23);
                        if (bValid == true)
                            bValid = validateParam("int", sVal2, 0, 59);
                        if (sVal == "")
                            sVal = "0";
                        if (sVal2 == "")
                            sVal2 = "0";
                        DateTime tTime = new DateTime(1999, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        DateTime tEODTime = GetEODTime();
                        if (tTime < tEODTime)
                            tTime = new DateTime(2000, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        else
                            tTime = new DateTime(1999, 12, 31, int.Parse(sVal.ToString()), int.Parse(sVal2.ToString()), 0);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                        //bFirst = false;
                    }
                    if (sType == "date")
                    {
                        bValid = validateParam("date", sVal, -1, -1);
                        DateTime tTime = DateUtils.GetDateFromStr(sVal);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                        //bFirst = false;
                    }
                    //if (sType == "radio")
                    //{
                    //    if (sFieldName == "RATIO_ID" && !string.IsNullOrEmpty(sVal))
                    //    {
                    //        selectedRatio = GetRatioVal(sVal);
                    //    }
                    //}
                    if (sType == "file")
                    {


                        if (string.IsNullOrEmpty(selectedRatio))
                        {

                        }

                        string sPicBaseName = "";
                        string sBasePath = HttpContext.Current.Server.MapPath("");
                        string sPicUploaderPath = GetWSURL("pic_uploader_path");
                        if (!string.IsNullOrEmpty(sPicUploaderPath))
                        {
                            sBasePath = sPicUploaderPath;
                        }

                        string sUploadedFile = "";
                        string sFileObjName = nCounter.ToString() + "_val";
                        string sUploadedFileExt = "";
                        HttpPostedFile theFile = HttpContext.Current.Request.Files[sFileObjName];
                        string sIsImage = coll[nCounter.ToString() + "_isPic"].ToString();
                        string sDirectory = coll[nCounter.ToString() + "_directory"].ToString();
                        string ratioIndex = string.Empty;
                        if (coll[nCounter.ToString() + "_ratioIndex"] != null)
                        {
                            ratioIndex = coll[nCounter.ToString() + "_ratioIndex"].ToString();
                        }
                        string selectedRatioVal = string.Empty;
                        if (coll[ratioIndex + "_val"] != null && coll[ratioIndex + "_val"].Trim().ToString() != "")
                        {
                            selectedRatioVal = coll[ratioIndex + "_val"].Trim().ToString();
                        }
                        bool bIsImage = false;
                        if (sIsImage.Trim().ToUpper() == "TRUE")
                            bIsImage = true;
                        if (theFile != null && theFile.FileName != "")
                        {
                            bValid = false;
                            if (bIsImage == true)
                            {
                                if (theFile.ContentType.StartsWith("image"))
                                    bValid = true;
                            }
                            else
                            {
                                if (theFile.ContentType.StartsWith("audio") ||
                                    theFile.ContentType.StartsWith("text") ||
                                    theFile.ContentType.StartsWith("video") ||
                                    theFile.ContentType.StartsWith("image") ||
                                    theFile.ContentType == "application/vnd.ms-excel" ||
                                    theFile.ContentType == "application/msword" ||
                                    theFile.ContentType == "application/octet-stream")
                                    bValid = true;
                            }
                            if (bValid == true)
                            {
                                string sUseQueue = TVinciShared.WS_Utils.GetTcmConfigValue("downloadPicWithQueue");
                                sUseQueue = sUseQueue.ToLower();
                                if (sUseQueue.Equals("true"))
                                {
                                    log.DebugFormat("downloadPicWithQueue");

                                    int mediaID = 0;
                                    if (HttpContext.Current.Session["media_id"] != null)
                                    {
                                        string mediaIdStr = HttpContext.Current.Session["media_id"].ToString();
                                        if (!string.IsNullOrEmpty(mediaIdStr))
                                        {
                                            if (HttpContext.Current.Session["media_file_id"] == null)
                                            {
                                                mediaID = int.Parse(mediaIdStr);
                                            }
                                        }
                                    }

                                    #region useRabbitQueue

                                    sPicBaseName = ImageUtils.GetDateImageName(mediaID);

                                    sUploadedFile = theFile.FileName;

                                    sUploadedFileExt = ImageUtils.GetFileExt(sUploadedFile);

                                    if (!Directory.Exists(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString()))
                                    {
                                        Directory.CreateDirectory(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString());
                                    }

                                    if (bIsImage == false)
                                    {
                                        string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;
                                        theFile.SaveAs(sTmpImage);
                                    }
                                    else
                                    {
                                        List<string> lSizes = new List<string>();
                                        lSizes.Add("full");
                                        // add checkbox value if needed 
                                        if (coll["6_val"] == "on")
                                        {
                                            lSizes.Add("tn");
                                        }

                                        Int32 nI = 0;
                                        bool bCont1 = true;
                                        while (bCont1 && sPicBaseName != "")
                                        {
                                            if (coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].Trim().ToString() != "")
                                            {
                                                bool isResize = true;
                                                string sRatio = string.Empty;
                                                string sWidth = coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].ToString();
                                                string sHeight = coll[nCounter.ToString() + "_picDim_height_" + nI.ToString()].ToString();
                                                if (coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].Trim().ToString() != "")
                                                {
                                                    sRatio = coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].ToString();
                                                    if (!string.IsNullOrEmpty(selectedRatioVal) && sRatio != selectedRatioVal)
                                                    {
                                                        isResize = false;
                                                    }
                                                }
                                                if (isResize)
                                                {
                                                    lSizes.Add(sWidth + "X" + sHeight);
                                                }
                                                nI++;
                                            }
                                            else
                                                bCont1 = false;
                                        }

                                        string[] sPicSizes = lSizes.ToArray();
                                        bool succeed = ImageUtils.SendPictureDataToQueue(sUploadedFile, sPicBaseName, sBasePath, sPicSizes, nGroupID);//send to Rabbit
                                    }
                                    #endregion                                   
                                }
                                else
                                {
                                    #region useUploader
                                    int mediaID = 0;
                                    if (HttpContext.Current.Session["media_id"] != null)
                                    {
                                        string mediaIdStr = HttpContext.Current.Session["media_id"].ToString();
                                        if (!string.IsNullOrEmpty(mediaIdStr))
                                        {
                                            if (HttpContext.Current.Session["media_file_id"] == null)
                                            {
                                                mediaID = int.Parse(mediaIdStr);
                                            }
                                        }
                                    }
                                    if (mediaID > 0)
                                    {
                                        sPicBaseName = ImageUtils.GetDateImageName(mediaID);
                                    }
                                    else
                                    {
                                        sPicBaseName = ImageUtils.GetDateImageName();
                                    }
                                    sUploadedFile = theFile.FileName;
                                    int nExtractPos = sUploadedFile.LastIndexOf(".");
                                    if (nExtractPos > 0)
                                        sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                                    if (!Directory.Exists(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString()))
                                    {
                                        Directory.CreateDirectory(sBasePath + "/" + sDirectory + "/" + nGroupID.ToString());
                                    }

                                    if (bIsImage == false)
                                    {
                                        string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + sUploadedFileExt;
                                        theFile.SaveAs(sTmpImage);
                                    }
                                    else
                                    {
                                        //FULL 
                                        string sFullImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_full" + sUploadedFileExt; ;
                                        bool bExists = System.IO.File.Exists(sFullImage);
                                        int nAdd = 0;
                                        while (bExists)
                                        {
                                            if (sPicBaseName.IndexOf("_") != -1)
                                                sPicBaseName = sPicBaseName.Substring(0, sPicBaseName.IndexOf("_"));
                                            sPicBaseName += "_" + nAdd.ToString();
                                            sFullImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_full" + sUploadedFileExt;
                                            bExists = System.IO.File.Exists(sFullImage);
                                            nAdd++;
                                        }
                                        theFile.SaveAs(sFullImage);
                                        UploadPicToGroup(nGroupID, sFullImage);

                                        nAdd = 0;
                                        if (coll["6_val"] == "on" && !string.IsNullOrEmpty(sPicBaseName)) //tn size
                                        {
                                            string sTNImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_tn" + sUploadedFileExt;
                                            ImageUtils.ResizeImageAndSave(sFullImage, sTNImage, 90, 65, true);
                                            UploadPicToGroup(nGroupID, sTNImage);
                                        }

                                        Int32 nI = 0;
                                        bool bCont1 = true;
                                        while (bCont1 && sPicBaseName != "")
                                        {
                                            if (coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].Trim().ToString() != "")
                                            {
                                                bool isResize = true;
                                                string sRatio = string.Empty;
                                                string sWidth = coll[nCounter.ToString() + "_picDim_width_" + nI.ToString()].ToString();
                                                string sHeight = coll[nCounter.ToString() + "_picDim_height_" + nI.ToString()].ToString();
                                                string sEndName = coll[nCounter.ToString() + "_picDim_endname_" + nI.ToString()].ToString();
                                                string sCropName = coll[nCounter.ToString() + "_crop_" + nI.ToString()].ToString();
                                                string sTmpImage = sBasePath + "/" + sDirectory + "/" + nGroupID.ToString() + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                                                if (coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()] != null &&
                                                coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].Trim().ToString() != "")
                                                {
                                                    sRatio = coll[nCounter.ToString() + "_picDim_ratio_" + nI.ToString()].ToString();
                                                    if (!string.IsNullOrEmpty(selectedRatioVal) && sRatio != selectedRatioVal)
                                                    {
                                                        isResize = false;
                                                    }
                                                }
                                                if (isResize)
                                                {
                                                    ImageUtils.ResizeImageAndSave(sFullImage, sTmpImage, int.Parse(sWidth), int.Parse(sHeight), bool.Parse(sCropName));
                                                    UploadPicToGroup(nGroupID, sTmpImage);
                                                }
                                                nI++;
                                            }


                                            else
                                                bCont1 = false;
                                        }

                                    }
                                    #endregion
                                }
                            }
                            if (bFirst == false)
                                selectQuery += "and";
                            else
                                selectQuery += "where";
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", sPicBaseName + sUploadedFileExt);
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", sPicBaseName + sUploadedFileExt);
                            bFirst = false;
                        }
                    }
                    if (sType == "datetime")
                    {
                        if (sVal != "")
                        {
                            if (bFirst == false)
                                selectQuery += "and";
                            else
                                selectQuery += "where";
                            string sValMin = coll[nCounter.ToString() + "_valMin"].ToString();
                            string sValHour = coll[nCounter.ToString() + "_valHour"].ToString();
                            bValid = validateParam("int", sValHour, 0, 23);
                            if (bValid == true)
                                bValid = validateParam("int", sValMin, 0, 59);
                            if (bValid == true)
                                bValid = validateParam("date", sVal, 0, 59);
                            DateTime tTime = DateUtils.GetDateFromStr(sVal);
                            if (sValHour != "")
                                tTime = tTime.AddHours(int.Parse(sValHour.ToString()));
                            if (sValMin != "")
                                tTime = tTime.AddMinutes(int.Parse(sValMin.ToString()));
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                            bFirst = false;
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", tTime);
                        }
                        else
                        {
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM(sFieldName, "=", DBNull.Value);
                            //bFirst = false;
                        }
                    }
                }
                catch
                {
                    selectQuery.Finish();
                    selectQuery = null;
                    insertQuery.Finish();
                    insertQuery = null;
                    HttpContext.Current.Session["error_msg"] = "* הנתונים שהוSendו אינם חוקיים או מלאים";
                    return 0;
                }
                if (sType == "multi")
                    bCollection = true;
                nCounter++;
            }
            selectQuery += " order by id desc";
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.UtcNow);
            bool bNew = CheckUnique(sTableName, sUniquField, sUniqueValue, sUniqueType);
            if (bValid == true)
            {
                if (bNew == true)
                    insertQuery.Execute();
                else
                    HttpContext.Current.Session["error_msg"] = "* הנתונים שהוSendו מהווים כפילות לרשומה קיימת";
            }
            insertQuery.Finish();
            insertQuery = null;
            //Handling many to many tables
            if (bValid == true)
            {
                if (selectQuery.Execute("query", true) != null)
                {
                    try
                    {
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                    catch
                    {
                        nID = 0;
                    }
                }

                if (bCollection == true && bNew == true)
                {
                    HandleMany2Many(ref selectQuery, sConnectionKey);
                }
            }
            else
            {
                HttpContext.Current.Session["error_msg"] = "* הנתונים שהוSendו אינם חוקיים או מלאים";
            }

            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        private static bool SendImageDataToImageUploadQueue(string sourcePath, int groupId, int version, int picId, string picNewName, eMediaType mediaType)
        {
            bool enqueueSuccessful = false;

            try
            {
                // generate ImageUploadData and send to Queue 
                int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);

                // get image server URL
                string imageServerUrl = ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post);
                if (string.IsNullOrEmpty(imageServerUrl))
                    throw new Exception(string.Format("IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));

                if (sourcePath.ToLower().Trim().StartsWith("http://") == false && sourcePath.ToLower().Trim().StartsWith("https://") == false)
                {
                    sourcePath = ImageUtils.getRemotePicsURL(groupId) + sourcePath;
                }

                ImageUploadData data = new ImageUploadData(parentGroupId, picNewName, version, sourcePath, picId, imageServerUrl, mediaType);

                var queue = new ImageUploadQueue();

                enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_IMAGE_UPLOAD, parentGroupId));

                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of image upload {0}", data);
                }
                else
                {
                    log.DebugFormat("image upload: data: {0}", data);
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed image upload: Exception:{0} ", exc);
            }

            return enqueueSuccessful;
        }

        private static string GetWSURL(string key)
        {
            return WS_Utils.GetTcmConfigValue(key);
        }


        static private string GetRatioVal(string ratioID)
        {
            string retVal = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ratio from lu_pics_ratios where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(ratioID));
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = selectQuery.Table("query").DefaultView[0].Row["ratio"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        static public Int32 DoTheWork()
        {
            return DoTheWork("");
        }

        static public Int32 DoTheWork(string sConnectionKey)
        {
            Int32 nID = 0;
            System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
            if (coll["table_name"] == null)
            {
                HttpContext.Current.Session["error_msg"] = "חסרה טבלה לעידכון";
                EndOfAction();
            }
            if (coll["id"] != null)
            {
                nID = int.Parse(coll["id"].ToString());
                UpdateTable(sConnectionKey);
            }
            else
                nID = InsertTable(sConnectionKey);
            EndOfAction();

            CachingManager.CachingManager.RemoveFromCache("SetValue_" + coll["table_name"].ToString() + "_");
            //System.Web.HttpRuntime.Cache.

            return nID;
        }

        static protected void EndOfAction()
        {
            System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
            if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
            {
                // string sFailure = coll["failure_back_page"].ToString();
                if (coll["failure_back_page"] != null)
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
                else
                    HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
            else
            {
                if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
                {
                    HttpContext.Current.Session["last_page_html"] = null;
                    string s = HttpContext.Current.Session["back_n_next"].ToString();
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
                    HttpContext.Current.Session["back_n_next"] = null;
                }
                else
                {
                    if (coll["success_back_page"] != null)
                        HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
                    else
                        HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
                }
            }
        }
    }
}