using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using KLogMonitor;
using System.Reflection;

namespace FilmoFeeder
{
    public class Feeder : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected string m_sVideosDirectory;
        protected string m_sPersonsDirectory;
        public Feeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            string[] seperator = { "||" };
            string[] splited = sParameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 2)
            {
                m_sVideosDirectory = splited[0].ToString();
                m_sPersonsDirectory = splited[1].ToString();
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new Feeder(nTaskID, nIntervalInSec, sParameters);
        }

        protected override bool DoTheTaskInner()
        {
            return ActualWork(m_sVideosDirectory, m_sPersonsDirectory);
        }

        static protected bool GetVideoFiles(string sPath, DateTime dLastCall)
        {
            bool bOK = true;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);
            System.IO.FileSystemInfo[] files = di.GetFileSystemInfos();
            Int32 nFilesCount = files.Length;
            for (int i = 0; i < nFilesCount; i++)
            {
                DateTime dLastAccess = files[i].LastAccessTimeUtc;
                if (dLastCall < dLastAccess && (files[i].FullName.EndsWith(".xls") == true || files[i].FullName.EndsWith(".csv") == true))
                {
                    bOK = ImportVideosExcel(files[i].FullName);
                }
            }
            return bOK;
        }

        static protected bool GetPersonFiles(string sPath, DateTime dLastCall)
        {
            bool bOK = true;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sPath);
            System.IO.FileSystemInfo[] files = di.GetFileSystemInfos();
            Int32 nFilesCount = files.Length;
            for (int i = 0; i < nFilesCount; i++)
            {
                DateTime dLastAccess = files[i].LastAccessTimeUtc;
                if (dLastCall < dLastAccess)
                {
                    bOK = ImportPersonExcel(files[i].FullName);
                }
            }
            return bOK;
        }

        static public bool ActualWork(string sVideosDirectory, string sPersonsDirectory)
        {
            bool bOK = true;
            DateTime dLastCall = DateTime.UtcNow;
            DateTime dNow = DateTime.UtcNow;
            string sXML = "";
            Int32 nStatus = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select LAST_SYNC_END_DATE from feed_dates where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", 7);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    dLastCall = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["LAST_SYNC_END_DATE"]);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            bOK = GetVideoFiles(sVideosDirectory, dLastCall);
            if (bOK == true)
            {
                bOK = GetPersonFiles(sPersonsDirectory, dLastCall);
            }
            if (bOK == true)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("feed_dates");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_SYNC_END_DATE", "=", dNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", 7);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            return true;
        }

        static private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
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

        static protected string GetMetaSection(string sMetaName, string sLang, string sValues)
        {
            if (sValues.Trim() == "")
                return "";
            string sRet = "<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName, true) + "\" ml_handling=\"unique\">";

            string[] sToSplitWith = { ";", "," };
            string[] splited = sValues.Split(sToSplitWith, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splited.Length; i++)
            {
                sRet += "<container>";
                sRet += "<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sLang, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(splited[i].Trim(), true) + "</value>";
                sRet += "</container>";
            }
            sRet += "</meta>";
            return sRet;
        }

        static protected bool ImportVideosExcel(string sFilePath)
        {
            try
            {
                Int32 nIndex = sFilePath.LastIndexOf("\\");
                string sPath = sFilePath.Substring(0, nIndex);
                string sFName = sFilePath.Substring(nIndex + 1);
                string sRet = "<feed><export>";
                DataSet d = GetExcelWorkSheet(sPath, sFName, 0).Copy();
                //DataSet d = GetExcelWorkSheet("c:\\temp", "La lola new.xls", 0).Copy();
                Int32 nCount = d.Tables[0].DefaultView.Count;
                DataTable dd = d.Tables[0].Copy();
                for (int i = 0; i < nCount; i++)
                {
                    //OK
                    string sBGInstituteID = dd.DefaultView[i].Row[0].ToString();
                    //OK
                    string sEyeInstituteID = dd.DefaultView[i].Row[1].ToString();
                    //OK
                    string sMediaType = dd.DefaultView[i].Row[2].ToString();
                    //OK
                    string sName = dd.DefaultView[i].Row[3].ToString();
                    //OK
                    string sDescription = dd.DefaultView[i].Row[4].ToString();
                    //OK
                    string sWorkDescription = dd.DefaultView[i].Row[5].ToString();
                    //OK
                    string sReleaseDate = dd.DefaultView[i].Row[6].ToString();
                    //OK
                    string sYearForFacet = dd.DefaultView[i].Row[7].ToString();
                    //OK
                    string sLinkForFilm = dd.DefaultView[i].Row[8].ToString();
                    //OK
                    string sMXFFileName = dd.DefaultView[i].Row[9].ToString();
                    //OK
                    string sDuration = dd.DefaultView[i].Row[10].ToString();
                    //OK
                    string sEpisodeNumber = dd.DefaultView[i].Row[11].ToString();
                    //OK
                    string sEpisodeTitle = dd.DefaultView[i].Row[12].ToString();
                    //OK
                    string sSeasonNumber = dd.DefaultView[i].Row[13].ToString();
                    //OK
                    string sSeasonTitle = dd.DefaultView[i].Row[14].ToString();
                    //OK
                    string sShow = dd.DefaultView[i].Row[15].ToString();
                    //OK
                    string sSubgenre = dd.DefaultView[i].Row[16].ToString();
                    //OK
                    string sGeneraTags = dd.DefaultView[i].Row[17].ToString();
                    //OK
                    string sProductionCountry = dd.DefaultView[i].Row[18].ToString();
                    //OK
                    string sSubtitleLanguage = dd.DefaultView[i].Row[19].ToString();
                    //OK
                    string sSpokenLanguage = dd.DefaultView[i].Row[20].ToString();
                    //OK
                    string sBlackWhite = dd.DefaultView[i].Row[21].ToString();
                    //OK
                    string sGeluidsfilmStille = dd.DefaultView[i].Row[22].ToString();
                    //OK
                    string sCast = dd.DefaultView[i].Row[23].ToString();
                    //OK
                    string sDirector = dd.DefaultView[i].Row[24].ToString();
                    //OK
                    string sDirectorOfPhotography = dd.DefaultView[i].Row[25].ToString();
                    //OK
                    string sWriter = dd.DefaultView[i].Row[26].ToString();
                    //OK
                    string sProducer = dd.DefaultView[i].Row[27].ToString();
                    //OK
                    string sMusic = dd.DefaultView[i].Row[28].ToString();
                    //OK
                    string sProductionCompany = dd.DefaultView[i].Row[29].ToString();
                    //OK
                    string sNetwork = dd.DefaultView[i].Row[30].ToString();
                    //OK
                    string sDistributorNetherlands = dd.DefaultView[i].Row[31].ToString();
                    //OK
                    string sOtherCrewMembers = dd.DefaultView[i].Row[32].ToString();
                    //OK
                    string sAspectRatio = dd.DefaultView[i].Row[33].ToString();
                    //OK
                    string sFilenamePicture = dd.DefaultView[i].Row[34].ToString();
                    //OK
                    string sCDNCodeMain = dd.DefaultView[i].Row[35].ToString();
                    //OK
                    string sCDNCodeTrailer = dd.DefaultView[i].Row[36].ToString();
                    //OK
                    string sAgeClassification = dd.DefaultView[i].Row[37].ToString();
                    //OK
                    string sWarnings = dd.DefaultView[i].Row[38].ToString();
                    if (sName != "")
                    {
                        sRet += "<media co_guid=\"" + sBGInstituteID + "_" + sEyeInstituteID + "\" action=\"insert\" is_active=\"false\">";
                        sRet += "<basic>";
                        sRet += "<name>";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sName, true) + "</value>";
                        sRet += "</name>";
                        sRet += "<description>";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "</value>";
                        sRet += "</description>";

                        sRet += "<media_type>" + TVinciShared.ProtocolsFuncs.XMLEncode(sMediaType, true) + "</media_type>";
                        sRet += "<rules><watch_per_rule>Parent allowed</watch_per_rule></rules>";
                        if (sFilenamePicture != "")
                            sRet += "<thumb url=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("http://tvinci.cdngc.net/filmo" + sFilenamePicture, true) + "\"/>";
                        sRet += "</basic>";

                        sRet += "<structure>";

                        sRet += "<strings>";
                        sRet += "<meta name=\"work description\" ml_handling=\"duplicate\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sWorkDescription, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Release date\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sReleaseDate, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Link to FilmInNederland.nl\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sLinkForFilm, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"MXF file name\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sMXFFileName, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Duration\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDuration, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"B&amp;G Institute ID\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sBGInstituteID, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Episode title\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sEpisodeTitle, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Aspect ratio encoded file\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sAspectRatio, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "</strings>";

                        sRet += "<doubles>";
                        if (sEyeInstituteID != "")
                            sRet += "<meta name=\"Eye Institute ID\">" + sEyeInstituteID + "</meta>";
                        if (sSeasonNumber != "")
                            sRet += "<meta name=\"Episode number\">" + sEpisodeNumber + "</meta>";
                        if (sSeasonNumber != "")
                            sRet += "<meta name=\"Season number\">" + sSeasonNumber + "</meta>";
                        if (sYearForFacet != "")
                            sRet += "<meta name=\"Year for facet\">" + sYearForFacet + "</meta>";
                        sRet += "</doubles>";
                        sRet += "<booleans>";
                        sRet += "</booleans>";
                        sRet += "<metas>";
                        sRet += GetMetaSection("Director", "dut", sDirector);
                        sRet += GetMetaSection("Music", "dut", sMusic);
                        sRet += GetMetaSection("Aspect ratio transcoded file", "dut", sAspectRatio);
                        sRet += GetMetaSection("Show", "dut", sShow);
                        sRet += GetMetaSection("Season title", "dut", sSeasonTitle);
                        //sRet += GetMetaSection("Genre", "dut", sSubgenre);
                        sRet += GetMetaSection("Subgenre", "dut", sSubgenre);
                        sRet += GetMetaSection("General tags", "dut", sGeneraTags);
                        sRet += GetMetaSection("Production country", "dut", sProductionCountry);
                        sRet += GetMetaSection("Subtitle language", "dut", sSubtitleLanguage);
                        sRet += GetMetaSection("Spoken language", "dut", sSpokenLanguage);
                        sRet += GetMetaSection("Color / black-white", "dut", sBlackWhite);
                        sRet += GetMetaSection("Geluidsfilm / stille film", "dut", sGeluidsfilmStille);
                        sRet += GetMetaSection("Age classification", "dut", sAgeClassification);
                        sRet += GetMetaSection("Warnings", "dut", sWarnings);
                        sRet += GetMetaSection("Cast", "dut", sCast);
                        //sRet += GetMetaSection("Main cast", "dut", sCast);
                        sRet += GetMetaSection("Director of Photography", "dut", sDirectorOfPhotography);
                        sRet += GetMetaSection("Writer", "dut", sWriter);
                        sRet += GetMetaSection("Producer", "dut", sProducer);
                        sRet += GetMetaSection("Production Company", "dut", sProductionCompany);
                        sRet += GetMetaSection("Network", "dut", sNetwork);
                        sRet += GetMetaSection("Distributor Netherlands", "dut", sDistributorNetherlands);
                        sRet += GetMetaSection("Other crew members", "dut", sOtherCrewMembers);
                        //sRet += GetMetaSection("Free", "dut", sGeneraTags);
                        sRet += "</metas>";

                        sRet += "</structure>";
                        sRet += "<files>";
                        /*
                        if (sPoster != "")
                            sRet += "<file handling_type=\"IMAGE\" type=\"POSTER\" quality=\"HIGH\" cdn_name=\"\" cdn_code=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("http://tvinci.panthercustomer.com/" + sPoster, true) + "\"/>";
                        */
                        if (sCDNCodeTrailer != "")
                        {
                            sRet += "<file handling_type=\"CLIP\" type=\"Trailer\" billing_type=\"None\" quality=\"HIGH\" cdn_name=\"CDNetworks-main2\" cdn_code=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sCDNCodeTrailer.Replace(".mxf", ""), true) + "\"/>";
                        }
                        if (sCDNCodeMain != "")
                        {
                            sRet += "<file handling_type=\"CLIP\" type=\"Main\" billing_type=\"None\" quality=\"HIGH\" cdn_name=\"CDNetworks-main2\" cdn_code=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sCDNCodeMain.Replace(".mxf", ""), true) + "\"/>";
                        }
                        sRet += "</files>";
                        sRet += "</media>";
                    }
                    // string sDescription = dd.DefaultView[i].Row[1].ToString();
                }
                sRet += "</export></feed>";
                string sNotifyXML = "";
                //TvinciImporter.ImporterImpl.DoTheWorkInner(sRet, 110, "", ref sNotifyXML);
                if (System.IO.File.Exists("C:\\temp\\FilmoReg\\filmo.xml"))
                    System.IO.File.Delete("C:\\temp\\FilmoReg\\filmo.xml");
                System.IO.StreamWriter w = System.IO.File.AppendText("C:\\temp\\FilmoReg\\filmo.xml");
                w.Write("{0}", sRet);
                w.Flush();
                w.Close();
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                return false;
            }
        }

        static protected bool ImportPersonExcel(string sFilePath)
        {
            try
            {
                Int32 nIndex = sFilePath.LastIndexOf("\\");
                string sPath = sFilePath.Substring(0, nIndex);
                string sFName = sFilePath.Substring(nIndex + 1);
                string sRet = "<feed><export>";
                DataSet d = GetExcelWorkSheet(sPath, sFName, 0).Copy();
                Int32 nCount = d.Tables[0].DefaultView.Count;
                DataTable dd = d.Tables[0].Copy();
                for (int i = 2; i < nCount; i++)
                {
                    //OK
                    string sMediaType = dd.DefaultView[i].Row[0].ToString();
                    //OK
                    string sName = dd.DefaultView[i].Row[1].ToString();
                    //OK
                    string sInternalRemark = dd.DefaultView[i].Row[2].ToString();
                    //OK
                    string sLinkForFilm = dd.DefaultView[i].Row[3].ToString();
                    //OK
                    string sDateOfBirth = dd.DefaultView[i].Row[4].ToString();
                    //OK
                    string sDateOfDeath = dd.DefaultView[i].Row[5].ToString();
                    //OK
                    string sYearOfBirth = dd.DefaultView[i].Row[6].ToString();
                    //OK
                    string sEyeInstituteID = dd.DefaultView[i].Row[7].ToString();
                    if (sName != "")
                    {
                        sRet += "<media co_guid=\"207_" + sName + "\" action=\"insert\" is_active=\"true\">";
                        sRet += "<basic>";
                        sRet += "<name>";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sName, true) + "</value>";
                        sRet += "</name>";
                        sRet += "<media_type>" + TVinciShared.ProtocolsFuncs.XMLEncode(sMediaType, true) + "</media_type>";
                        sRet += "<rules><watch_per_rule>Parent allowed</watch_per_rule></rules>";
                        sRet += "</basic>";
                        sRet += "<structure>";
                        sRet += "<strings>";
                        sRet += "<meta name=\"internal remarks\" ml_handling=\"duplicate\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sInternalRemark, true) + "</value>";
                        sRet += "</meta>";
                        sRet += "<meta name=\"Link to FilmInNederland.nl\" ml_handling=\"unique\">";
                        sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sLinkForFilm, true) + "</value>";
                        sRet += "</meta>";
                        if (sDateOfBirth != "Onbekend")
                        {
                            sRet += "<meta name=\"Date of birth\" ml_handling=\"unique\">";
                            sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDateOfBirth, true) + "</value>";
                            sRet += "</meta>";
                        }
                        if (sDateOfDeath != "Onbekend")
                        {
                            sRet += "<meta name=\"Date of death\" ml_handling=\"unique\">";
                            sRet += "<value lang=\"dut\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDateOfDeath, true) + "</value>";
                            sRet += "</meta>";
                        }
                        sRet += "</strings>";

                        sRet += "<doubles>";
                        if (sEyeInstituteID != "")
                            sRet += "<meta name=\"Eye Institute ID\">" + sEyeInstituteID + "</meta>";
                        if (sYearOfBirth != "")
                            sRet += "<meta name=\"Year of birth\">" + sYearOfBirth + "</meta>";
                        sRet += "</doubles>";
                        sRet += "<booleans>";
                        sRet += "</booleans>";
                        sRet += "<metas>";
                        sRet += "</metas>";
                        sRet += "</structure>";
                        sRet += "<files>";
                        sRet += "</files>";
                        sRet += "</media>";
                    }
                    // string sDescription = dd.DefaultView[i].Row[1].ToString();
                }
                sRet += "</export></feed>";
                string sNotifyXML = "";
                //TvinciImporter.ImporterImpl.DoTheWorkInner(sRet, 110, "", ref sNotifyXML);
                if (System.IO.File.Exists("C:\\temp\\FilmoPerson\\filmo.xml"))
                    System.IO.File.Delete("C:\\temp\\FilmoPerson\\filmo.xml");
                System.IO.StreamWriter w = System.IO.File.AppendText("C:\\temp\\FilmoPerson\\filmo.xml");
                w.Write("{0}", sRet);
                w.Flush();
                w.Close();
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                return false;
            }
        }

        static public bool UpdatePersonGUID(string sFilePath)
        {
            try
            {
                Int32 nIndex = sFilePath.LastIndexOf("\\");
                string sPath = sFilePath.Substring(0, nIndex);
                string sFName = sFilePath.Substring(nIndex + 1);
                DataSet d = GetExcelWorkSheet(sPath, sFName, 0).Copy();
                Int32 nCount = d.Tables[0].DefaultView.Count;
                DataTable dd = d.Tables[0].Copy();
                for (int i = 2; i < nCount; i++)
                {
                    //OK
                    string sMediaType = dd.DefaultView[i].Row[0].ToString();
                    //OK
                    string sName = dd.DefaultView[i].Row[1].ToString();
                    //OK
                    string sInternalRemark = dd.DefaultView[i].Row[2].ToString();
                    //OK
                    string sLinkForFilm = dd.DefaultView[i].Row[3].ToString();
                    //OK
                    string sDateOfBirth = dd.DefaultView[i].Row[4].ToString();
                    //OK
                    string sDateOfDeath = dd.DefaultView[i].Row[5].ToString();
                    //OK
                    string sYearOfBirth = dd.DefaultView[i].Row[6].ToString();
                    //OK
                    string sEyeInstituteID = dd.DefaultView[i].Row[7].ToString();
                    if (sMediaType == "People")
                    {
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery += " update media set CO_GUID='207_'+name where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sName);
                        directQuery += " and group_id=111 and MEDIA_TYPE_ID=207";
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Exception", ex);
                return false;
            }
        }
    }
}
