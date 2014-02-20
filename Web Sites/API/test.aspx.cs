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
using System.Xml;
using TVinciShared;
using System.Data.OleDb;

public partial class test : System.Web.UI.Page
{
    static public string byteArrayToString(Byte[] baToConvert)
    {
        System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
        string StringMessage = System.Text.Encoding.UTF8.GetString(baToConvert);
        return StringMessage;
    }

    static public byte[] stringToByteArray(string sToConvert)
    {
        System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
        Byte[] CypherText = UTF8.GetBytes(sToConvert);
        return CypherText;
    }

    static public string stringToBase64(string sToEncode)
    {
        byte[] toEncode = stringToByteArray(sToEncode);
        return Convert.ToBase64String(toEncode);
    }

    static public string base64ToString(string sToDecode)
    {
        byte[] b = Convert.FromBase64String(sToDecode);
        return byteArrayToString(b);
    }

    protected Int32 GetIPToCountryID(string sFrom, string sTo, Int32 nCountryID, string sFromIP, string sToIP)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from ip_to_country_tmp where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "=", Int64.Parse(sFrom));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", "=", Int64.Parse(sTo));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FROM_IP", "=", sFromIP);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TO_IP", "=", sToIP);
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

    protected void IpToCountry()
    {
        string sFileName = "C:/Development/tmp/GeoIPCountryWhois.csv";
        string[] fields;
        CSVReader reader = new CSVReader(sFileName, System.Text.Encoding.UTF8);
        Int32 nCounter = 0;
        Int32 nFlushCounter = 0;
        while ((fields = reader.GetCSVLine()) != null)
        {
            try
            {
                string sFrom = fields[2];
                string sTo = fields[3];
                string sCountry2 = fields[4];
                string sCountryFull = fields[5];
                string sFromIP = fields[0];
                string sToIP = fields[1];
                Int32 nCountryID = GetCountryID(sCountry2, sCountryFull);
                Int32 nID = GetIPToCountryID(sFrom, sTo, nCountryID, sFromIP, sToIP);
                if (nID == 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ip_to_country_tmp");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "=", Int64.Parse(sFrom));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", "=", Int64.Parse(sTo));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FROM_IP", "=", sFromIP);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TO_IP", "=", sToIP);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
                nCounter++;
                nFlushCounter++;
                if (nFlushCounter == 100)
                {
                    nFlushCounter = 0;
                    Response.Write("Inserted: " + nCounter.ToString() + "<br/>");
                    Response.Flush();
                }
            }
            catch
            {
                continue;
            }
        }
        reader.Dispose();
    }



    protected Int32 GetCountryID(string sCountryCD2, string sFullName)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from countries where  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(COUNTRY_CD2)))", "=", sCountryCD2.Trim().ToLower());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(COUNTRY_NAME)))", "=", sFullName.Trim().ToLower());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nRet == 0)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("countries");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CD2", "=", sCountryCD2);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_NAME", "=", sFullName);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return GetCountryID(sCountryCD2, sFullName);
        }
        return nRet;
    }

    private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
    {
        OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + @"\" + fileName + ";Extended Properties=Excel 8.0;");
        OleDbCommand ExcelCommand = new OleDbCommand();
        ExcelCommand.Connection = ExcelConnection;
        OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);

        ExcelConnection.Open();
        DataTable ExcelSheets = ExcelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        string SpreadSheetName = "[" + ExcelSheets.Rows[workSheetNumber]["TABLE_NAME"].ToString() + "]";

        DataSet ExcelDataSet = new DataSet();
        ExcelCommand.CommandText = @"SELECT * FROM " + SpreadSheetName;
        ExcelAdapter.Fill(ExcelDataSet);

        ExcelConnection.Close();
        return ExcelDataSet;
    }

    protected string GetMetaSection(string sMetaName , string sLang , string sValues)
    {
        if (sValues.Trim() == "")
            return "";
        string sRet = "<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName , true) + "\" ml_handling=\"unique\">";
        
        string[] sToSplitWith = {";" , ","};
        string[] splited = sValues.Split(sToSplitWith , StringSplitOptions.RemoveEmptyEntries);
        for (int i =0; i < splited.Length; i++)
        {
            sRet += "<container>";
            sRet += "<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sLang , true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(splited[i].Trim() , true) + "</value>";
            sRet += "</container>";
        }
        sRet += "</meta>";
        return sRet;
    }

    public void ImportFile()
    {
        
    }
    protected void ImportNoveExcel()
    {
        string sRet = "<feed><export>";
        DataSet d = GetExcelWorkSheet("c:\\temp", "La lola new.xls", 0).Copy();
        Int32 nCount = d.Tables[0].DefaultView.Count;
        DataTable dd = d.Tables[0].Copy();
        for (int i = 2; i < nCount; i++)
        {
            
            //
            string sMSNGUID = dd.DefaultView[i].Row[0].ToString();
            string sName = dd.DefaultView[i].Row[1].ToString();
            string sDescription = dd.DefaultView[i].Row[2].ToString();
            string sMediaType = dd.DefaultView[i].Row[3].ToString();
            string sPoster = dd.DefaultView[i].Row[4].ToString();
            string sThumb = dd.DefaultView[i].Row[4].ToString();
            string sSynopsis = dd.DefaultView[i].Row[5].ToString();
            string sSpotlight = dd.DefaultView[i].Row[8].ToString();
            string sBuisinessModel = dd.DefaultView[i].Row[9].ToString();
            string sOfficialSite = dd.DefaultView[i].Row[10].ToString();
            string sAirDate = dd.DefaultView[i].Row[11].ToString();
            string sSeasonNumber = dd.DefaultView[i].Row[12].ToString();
            string sEpisodeNumber = dd.DefaultView[i].Row[13].ToString();
            string sProductionYear = dd.DefaultView[i].Row[14].ToString();
            string sNumberOfEpisodes = dd.DefaultView[i].Row[15].ToString();
            string sSubtitles = dd.DefaultView[i].Row[16].ToString();
            string sDubed = dd.DefaultView[i].Row[17].ToString();
            string sCategory = dd.DefaultView[i].Row[18].ToString();
            string sSeriesName = dd.DefaultView[i].Row[19].ToString();
            string sShow = dd.DefaultView[i].Row[20].ToString();
            string sGenre = dd.DefaultView[i].Row[21].ToString();
            string sCharacters = dd.DefaultView[i].Row[22].ToString();
            string sStaring = dd.DefaultView[i].Row[23].ToString();
            string sDirector = dd.DefaultView[i].Row[24].ToString();
            string sProducer = dd.DefaultView[i].Row[25].ToString();
            string sLanguage = dd.DefaultView[i].Row[26].ToString();
            string sCountry = dd.DefaultView[i].Row[27].ToString();
            string sContentProvider = dd.DefaultView[i].Row[28].ToString();
            string sProductioncompany = dd.DefaultView[i].Row[29].ToString();
            string sProject = dd.DefaultView[i].Row[30].ToString();
            string sAwards = dd.DefaultView[i].Row[31].ToString();
            string sHolidays = dd.DefaultView[i].Row[32].ToString();
            string sSpecialLocation = dd.DefaultView[i].Row[33].ToString();
            string sFree = dd.DefaultView[i].Row[34].ToString();
            string sChannel = dd.DefaultView[i].Row[35].ToString();
            
            string sFile = dd.DefaultView[i].Row[37].ToString();
            string sNumberOfSeasons = "0";
            if (sName != "")
            {                
                sRet += "<media co_guid=\"" + sMSNGUID + "\" action=\"insert\" is_active=\"true\">";
                sRet += "<basic>";
                sRet += "<name>";
                sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sName , true) + "</value>";
                sRet += "</name>";
                sRet += "<description>";
                sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sSynopsis , true) + "</value>";
                sRet += "</description>";

                sRet += "<media_type>" + TVinciShared.ProtocolsFuncs.XMLEncode(sMediaType , true) + "</media_type>";
                sRet += "<rules><watch_per_rule>Parent allowed</watch_per_rule></rules>";
                sRet += "<thumb url=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("http://tvinci.panthercustomer.com/" + sThumb , true) + "\"/>";
                sRet += "</basic>";

                sRet += "<structure>";
                
                sRet += "<strings>";
                sRet += "<meta name=\"Msn ID number\" ml_handling=\"duplicate\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sMSNGUID , true) + "</value>";
 			    sRet += "</meta>";
                sRet += "<meta name=\"Internal remarks\" ml_handling=\"unique\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription , true) + "</value>";
 			    sRet += "</meta>";
                sRet += "<meta name=\"Official site\" ml_handling=\"unique\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sOfficialSite , true) + "</value>";
 			    sRet += "</meta>";
                sRet += "<meta name=\"Business model\" ml_handling=\"unique\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sBuisinessModel , true) + "</value>";
 			    sRet += "</meta>";
                sRet += "<meta name=\"Air date\" ml_handling=\"unique\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sAirDate , true) + "</value>";
 			    sRet += "</meta>";
                
                               
                sRet += "<meta name=\"Spotlight\" ml_handling=\"unique\">";
 				sRet += "<value lang=\"spa\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sSpotlight , true) + "</value>";
 			    sRet += "</meta>";
                sRet += "</strings>";

                sRet += "<doubles>";
                
                if (sProductionYear != "")
                    sRet += "<meta name=\"Production Year\">" + sProductionYear + "</meta>";
                if (sSeasonNumber != "")
                    sRet += "<meta name=\"Season number\">" + sSeasonNumber + "</meta>";
                if (sEpisodeNumber != "")
                    sRet += "<meta name=\"Episode number\">" + sEpisodeNumber + "</meta>";
                if (sNumberOfEpisodes != "")
                    sRet += "<meta name=\"Number of episodes\">" + sNumberOfEpisodes + "</meta>";
                if (sNumberOfSeasons != "")
                    sRet += "<meta name=\"Number of seasons\">" + sNumberOfSeasons + "</meta>";
                sRet += "</doubles>";
                sRet += "<booleans>";
                if (sDubed == "yes")
                    sRet += "<meta name=\"Dubbed\">TRUE</meta>";
                else
                    sRet += "<meta name=\"Dubbed\">FALSE</meta>";
                if (sSubtitles == "yes")
                    sRet += "<meta name=\"Subtitles\">TRUE</meta>";
                else
                    sRet += "<meta name=\"Subtitles\">FALSE</meta>";
                sRet += "</booleans>";
                
                sRet += "<metas>";
                
                sRet += GetMetaSection("Genre", "spa", sGenre);
                sRet += GetMetaSection("starring", "spa", sStaring);
                sRet += GetMetaSection("Director", "spa", sDirector);
                sRet += GetMetaSection("Writer", "spa", "");
                sRet += GetMetaSection("Language", "spa", sLanguage);
                sRet += GetMetaSection("Category", "spa", sCategory);
                sRet += GetMetaSection("Series name", "spa", sSeriesName);
                sRet += GetMetaSection("Show", "spa", sShow);
                sRet += GetMetaSection("Characters name", "spa", sCharacters);
                sRet += GetMetaSection("Producer", "spa", sProducer);
                sRet += GetMetaSection("Country", "spa", sCountry);
                sRet += GetMetaSection("Content provider name", "spa", sContentProvider);
                sRet += GetMetaSection("Production Company", "spa", sProductioncompany);
                sRet += GetMetaSection("Project", "spa", sProject);
                sRet += GetMetaSection("Awards", "spa", sAwards);
                sRet += GetMetaSection("Holidays", "spa", sHolidays);
                sRet += GetMetaSection("Special location", "spa", sSpecialLocation);
                sRet += GetMetaSection("Channel", "spa", sChannel);
                 
                sRet += GetMetaSection("Free", "spa", sFree);
                sRet += "</metas>";

                sRet += "</structure>";
                sRet += "<files>";
                if (sPoster != "")
                    sRet += "<file handling_type=\"IMAGE\" type=\"POSTER\" quality=\"HIGH\" cdn_name=\"\" cdn_code=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("http://tvinci.panthercustomer.com/" + sPoster, true) + "\"/>";
                if (sMSNGUID != "")
                {
                    string sFileXML = "<set><file mk=\"es-xl\">" + sMSNGUID + "</file></set>";
                    sRet += "<file handling_type=\"CLIP\" type=\"Main MSN Player\" billing_type=\"None\" quality=\"HIGH\" player_type=\"MSN\" cdn_name=\"MSN\" cdn_code=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sFileXML, true) + "\"/>";
                }
                sRet += "</files>";
                sRet += "</media>";
            }
           // string sDescription = dd.DefaultView[i].Row[1].ToString();
        }
        sRet += "</export></feed>";
        if (System.IO.File.Exists("\\NY1WD231\\logs\\nove1.xml"))
            System.IO.File.Delete("\\NY1WD231\\logs\\nove1.xml");
        System.IO.StreamWriter w = System.IO.File.AppendText("\\NY1WD231\\logs\\nove1.xml");
        w.Write("{0}", sRet);
        w.Flush();
        w.Close();
    }

    protected void SetAllPicSize(Int32 nGroupID, bool bCrop)
    {
        object locker = new object();
        object oPicsFTP = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP", nGroupID);
        object oPicsFTPUN = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_USERNAME", nGroupID);
        object oPicsFTPPass = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_FTP_PASSWORD", nGroupID);
        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsFTP = "";
        string sPicsFTPUN = "";
        string sPicsFTPPass = "";
        string sPicsBasePath = "";
        if (oPicsFTP != DBNull.Value && oPicsFTP != null)
            sPicsFTP = oPicsFTP.ToString();
        if (oPicsFTPUN != DBNull.Value && oPicsFTPUN != null)
            sPicsFTPUN = oPicsFTPUN.ToString();
        if (oPicsFTPPass != DBNull.Value && oPicsFTPPass != null)
            sPicsFTPPass = oPicsFTPPass.ToString();

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (sPicsFTP.ToLower().Trim().StartsWith("ftp://") == true)
            sPicsFTP = sPicsFTP.Substring(6);
        FTPUploader.SetRunningProcesses(0);
        string sBasePath = Server.MapPath("");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //selectQuery += "select p.* from pics p,media m where m.MEDIA_PIC_ID=p.id and m.status=1 and p.status=1 and ";
        selectQuery += "select p.* from pics p where p.id = 72501 and p.status=1 and ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sUploadedFile = selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString();

                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                string sPicBaseName = "";
                if (nExtractPos > 0)
                {
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);
                    sPicBaseName = sUploadedFile.Substring(0, nExtractPos);
                }

                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select * from media_pics_sizes where status=1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                    for (int nI = 0; nI < nCount1; nI++)
                    {
                        string sWidth = selectQuery1.Table("query").DefaultView[nI].Row["WIDTH"].ToString();
                        string sHeight = selectQuery1.Table("query").DefaultView[nI].Row["HEIGHT"].ToString();

                        string sEndName = sWidth + "X" + sHeight.ToString();

                        string sTmpImage1 = sBasePath + "\\pics\\" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                        string sBasePic = sBasePath + "\\pics\\" + sPicBaseName + "_full" + sUploadedFileExt;
                        string sPicRemoteURL = sPicsBasePath + "/" + sPicBaseName + "_" + sEndName + sUploadedFileExt;
                        sTmpImage1 = @"C:\ode\TVM\Web Sites\TVM\pics\081010015159_784X441.jpg";
                        try
                        {
                            if (System.IO.File.Exists(sBasePic) == true && sEndName != "tn")
                            {
                                lock (locker)
                                {
                                    TVinciShared.ImageUtils.ResizeImageAndSave(sBasePic, sTmpImage1, int.Parse(sWidth), int.Parse(sHeight), bCrop);
                                    //TVinciShared.DBManipulator.UploadPicToGroup(sTmpImage1, sPicsFTP, sPicsFTPUN, sPicsFTPPass);
                                    Response.Write(sUploadedFile + " uploaded OK<br/>");
                                    Logger.Logger.Log("Pic Resize", "Pic " + sTmpImage1 + " resized and uploaded", "PicResize");
                                }
                            }
                            else
                            {
                                Response.Write(sUploadedFile + " full pic does not exist<br/>");
                                Logger.Logger.Log("Pic Resize", "Pic " + sTmpImage1 + " not uploaded", "PicResize");
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write(sUploadedFile + " uploaded Fail " + ex.Message + " || " + ex.StackTrace + "<br/>");
                            Logger.Logger.Log("Pic Resize", "Exception " + sTmpImage1 + " not uploaded " + ex.Message, "PicResize");
                        }

                    }

                }
                selectQuery1.Finish();
                selectQuery1 = null;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        while (FTPUploader.m_nNumberOfRuningUploads > 0)
        {
            System.Threading.Thread.Sleep(1000);
            Response.Write(FTPUploader.m_nNumberOfRuningUploads.ToString() + " Pics are still uploading<br/>");
            Response.Flush();
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        //ImportNoveExcel();
        //MSNFeeder.Feeder.LoadArticles();
        int groupID = int.Parse(Request.QueryString["GroupID"]);
        SetAllPicSize(groupID, true);
        return;
        //string sSrc = GetAkamaiURL("http://il.esperanto.mtvi.com/www/xml/flv/flvgen.jhtml?HiLoPref=lo&vid=223344");
        //Logger.Logger.SendSMS("This is a test"); 
        //return; 
        //string s = stringToBase64("\"alert(coocies)");
        //string g = base64ToString(s);
        //string sNewURL = "";
        //string sCallerIP = TVinciShared.PageUtils.GetCallerIP();
        //string sCheckURL = "http://www.s2o.tv/Is_User_VIP.aspx?ip=80.179.194.132";
        //Int32 nStatus = 200;
        //string sResp = TVinciShared.Notifier.SendGetHttpReq("http://intl.esperanto.mtvi.com/global/reporting/videos/video_rights_list.jhtml?regionCode=PL", ref nStatus);
        
        //return;
        //Int32 nCountryID=0;
        //string sLang = "";
        //Int32 nDeviceID = 0;
        //bool bAdmin = false;
        //bool bWithCache = true;
        //ProtocolsFuncs.GetAdminTokenValues("" , "192.118.32.80" , ref nCountryID , ref sLang , ref nDeviceID , 35 ,ref bAdmin , ref bWithCache);
        //Response.Write("192.118.32.80=" + TVinciShared.PageUtils.GetIPCountry2("192.118.32.80") + "<br/>");
        //Response.Write("82.102.135.22=" + TVinciShared.PageUtils.GetIPCountry2("82.102.135.22") + "<br/>");
        //IpToCountry();
        //Response.Write("92.236.44.237=" + TVinciShared.PageUtils.GetIPCountry2("92.236.44.237") + "<br/>");
        //Response.Write("89.229.20.230=" + TVinciShared.PageUtils.GetIPCountry2("89.229.20.230") + "<br/>");
        
        //UpdateHebrew();
        
        Response.Write(CookieUtils.GetCookie("tvinci_api"));
        return;
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from songs s where s.STREAMING_SUPLIER_ID=1 and is_active=1 and status=1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nSongID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                //string sPhonetic = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                string sStreamCode = selectQuery.Table("query").DefaultView[i].Row["STREAMING_CODE"].ToString();

                Int32 nYear = 0;
                if (selectQuery.Table("query").DefaultView[i].Row["CLIP_YEAR_ID"] != DBNull.Value)
                    nYear = int.Parse(selectQuery.Table("query").DefaultView[i].Row["CLIP_YEAR_ID"].ToString());
                Int32 nPicID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["VIDEO_PIC_ID"].ToString());
                nPicID = UpdatePics(nPicID);
                Int32 nMediaID = InsertMedia(sName, "", "", nYear, nPicID);
                UpdateMediaArtists(nSongID, nMediaID);
                UpdateMediaBands(nSongID, nMediaID);
                UpdateMediaJanners(nSongID, nMediaID);
                UpdateMediaMoods(nSongID, nMediaID);
                InsertMediaFile(nMediaID , sStreamCode , 2);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void UpdateHebrew()
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from media where is_active=1 and status=1 and group_id=6";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_tags");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID", "=", 1423);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 6);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void InsertMediaFile(Int32 nMediaID , string sStreamCode , Int32 nStreamSupID)
    {
        SetTvinciDB();
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_files");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", 3);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sStreamCode);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_SUPLIER_ID", "=", nStreamSupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected Int32 UpdatePics(Int32 nPicID)
    {
        SetMTVDB();
        Int32 nNewPicID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from pics where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPicID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                string sName = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                string sDesc = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                string sBaseURL = selectQuery.Table("query").DefaultView[0].Row["BASE_URL"].ToString();
                nNewPicID = InsertNewPic(sName, sDesc, "", sBaseURL);
                UpdatePicsTags(nPicID , nNewPicID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nNewPicID;
    }

    protected void UpdateMediaArtists(Int32 nMTVSongID, Int32 nTvinciMediaID)
    {
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ma.NAME_EN from mtv_artists ma,songs_artists sa where sa.MTV_ARTIST_ID=ma.id and sa.STATUS=1 and ma.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sa.SONG_ID", "=", nMTVSongID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME_EN"].ToString();
                Int32 nTagID = InsertNewTag(sName, 4);
                ConnectTagToMedia(nTvinciMediaID, nTagID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void UpdateMediaMoods(Int32 nMTVSongID, Int32 nTvinciMediaID)
    {
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ma.NAME_EN from moods ma,songs_moods sa where sa.MOOD_ID=ma.id and sa.STATUS=1 and ma.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sa.SONG_ID", "=", nMTVSongID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME_EN"].ToString();
                Int32 nTagID = InsertNewTag(sName, 11);
                ConnectTagToMedia(nTvinciMediaID, nTagID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }
    protected void UpdateMediaJanners(Int32 nMTVSongID, Int32 nTvinciMediaID)
    {
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ma.NAME_EN from janners ma,songs_janers sa where sa.JANER_ID=ma.id and sa.STATUS=1 and ma.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sa.SONG_ID", "=", nMTVSongID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME_EN"].ToString();
                Int32 nTagID = InsertNewTag(sName, 10);
                ConnectTagToMedia(nTvinciMediaID, nTagID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void UpdateMediaBands(Int32 nMTVSongID, Int32 nTvinciMediaID)
    {
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ma.NAME_EN from mtv_bands ma,songs_bands sa where sa.MTV_BAND_ID=ma.id and sa.STATUS=1 and ma.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sa.SONG_ID", "=", nMTVSongID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME_EN"].ToString();
                Int32 nTagID = InsertNewTag(sName, 5);
                ConnectTagToMedia(nTvinciMediaID, nTagID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void UpdatePicsTags(Int32 nMTVPicID, Int32 nTvinciPicID)
    {
        SetMTVDB();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select t.value from tags t,pics_tags pt where pt.TAG_ID=t.id and pt.STATUS=1 and t.status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pt.PIC_ID", "=", nMTVPicID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sName = selectQuery.Table("query").DefaultView[i].Row["value"].ToString();
                Int32 nTagID = InsertNewTag(sName, 0);
                ConnectTagToPic(nTvinciPicID, nTagID);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected Int32 ConnectTagToPic(Int32 nPicID, Int32 nTagID)
    {
        SetTvinciDB();
        Int32 nCount = 0;
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from pics_tags where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID" , "=" , nPicID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID" , "=" , nTagID);
        selectQuery += " and group_id=7";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nID > 0)
            return nID;

        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pics_tags");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID", "=", nTagID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return ConnectTagToPic(nPicID, nTagID);
    }

    protected Int32 ConnectTagToMedia(Int32 nMediaID, Int32 nTagID)
    {
        SetTvinciDB();
        Int32 nCount = 0;
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from media_tags where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID", "=", nTagID);
        selectQuery += " and group_id=7";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nID > 0)
            return nID;

        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_tags");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_ID", "=", nTagID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return ConnectTagToMedia(nMediaID, nTagID);
    }

    protected Int32 InsertNewTag(string sTagValue , Int32 nTagType)
    {
        SetTvinciDB();
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from tags where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(VALUE)))", "=", sTagValue.Trim().ToLower());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_TYPE_ID", "=", nTagType);
        selectQuery += "and group_id=7 and status=1 ";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nRet != 0)
            return nRet;

        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tags");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE" , "=" , sTagValue);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TAG_TYPE_ID" , "=" , nTagType);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS" , "=" , 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID" , "=" , 7);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return InsertNewTag(sTagValue , nTagType);
    }

    protected Int32 InsertNewPic(string sName , string sDesc , string sCred , string sBaseURL)
    {
        SetTvinciDB();
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pics");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT", "=", sCred);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from pics where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
        selectQuery += " and ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT", "=", sCred);
        //selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 7);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
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

    protected Int32 InsertMedia(string sName, string sDescription, string sPhonetic , 
        Int32 nYear , Int32 nPicID)
    {
        SetTvinciDB();
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from media where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 7);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("META1_STR", "=", sPhonetic);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("META2_STR", "=", "");
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("META1_DOUBLE", "=", nYear);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("META2_DOUBLE", "=", 5);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", 7);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", 5);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nRet > 0)
            return nRet;
        
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("META1_STR", "=", sPhonetic);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("META2_STR", "=", "");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("META1_DOUBLE", "=", nYear);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("META2_DOUBLE", "=", 5);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", 7);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_PIC_ID", "=", nPicID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", 5);
        
        
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return InsertMedia(sName, sDescription, sPhonetic, nYear, nPicID);
    }

    protected void SetTvinciDB()
    {
        Session["MSSQL_SERVER_NAME"] = "msd101.1host.co.il";
        Session["DB_NAME"] = "tvinci";
        Session["UN"] = "production";
        Session["PS"] = "lF6CZU9HIOIAGuzj";
        ODBCWrapper.Connection.ClearConnection();
    }

    protected void SetMTVDB()
    {
        Session["MSSQL_SERVER_NAME"] = "mssql17.ananeyservers.com";
        Session["DB_NAME"] = "mtvweb_db";
        Session["UN"] = "mtvweb_dbalogin";
        Session["PS"] = "jNBg65FAG";
        ODBCWrapper.Connection.ClearConnection();
    }
}
