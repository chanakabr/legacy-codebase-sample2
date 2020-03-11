using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using TVinciShared;
using TvinciImporter;
using System.Xml;
using System.Collections;
using KLogMonitor;
using System.Reflection;

namespace ExcelFeeder
{
    public class Feeder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const int COLUMN_MAX = 254;
        public const int ROW_MAX = 500;

        private int nGroupID;
        private string sPath;
        private string sFileName;
        private string sMainLang;
        private KeyValuePair<int, string>[] sLangs;
        private Dictionary<int, string> sRatios;
        private string errMesage;
        Hashtable hStr;
        Hashtable hDouble;
        Hashtable hBool;
        Hashtable hTags;
        Hashtable hBasicsIndex;
        Hashtable hFile;

        public Feeder(int nGID, string path, string fileName)
        {
            nGroupID = nGID;
            sPath = path;
            sFileName = fileName;
            GetLang();
            GetGroupPicRatios();
            errMesage = "Error while parse excel";

            hStr = new Hashtable();
            hDouble = new Hashtable();
            hBool = new Hashtable();
            hBasicsIndex = new Hashtable();
            hTags = new Hashtable();
            hFile = new Hashtable();
        }

        private void GetLang()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select code3 from lu_languages where id in (select LANGUAGE_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            selectQuery += ")";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, code3 from lu_languages where id in (select LANGUAGE_ID from group_extra_languages where status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += ") order by id";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                sLangs = new KeyValuePair<int, string>[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    int nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    string sLang = selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                    sLangs[i] = new KeyValuePair<int, string>(nLangID, sLang);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }

        private void GetGroupPicRatios()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select lpr.ratio, lpr.id from lu_pics_ratios lpr, groups g where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += " lpr.id = g.ratio_id UNION ";
            selectQuery += " select lpr.ratio, lpr.id from lu_pics_ratios lpr, group_ratios gr where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gr.group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += "lpr.id = gr.ratio_id and gr.status = 1 ";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        int ratioID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                        string ratio = selectQuery.Table("query").DefaultView[i].Row["ratio"].ToString();
                        if (sRatios == null)
                        {
                            sRatios = new Dictionary<int, string>();
                        }
                        if (!sRatios.ContainsKey(ratioID))
                        {
                            sRatios.Add(ratioID, ratio);
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }

        public bool ActualWork(ref DataTable resultTable)
        {
            bool bOK = true;

            //Parse Excel 
            string sXml = ParseExcel();

            //Error While Parse Excel
            if (string.IsNullOrEmpty(sXml))
            {
                DataRow row = resultTable.NewRow();
                row["co_guid"] = errMesage;
                resultTable.Rows.Add(row);

                return true;
            }

            //Importer
            string sImporterNotifyXML = "";
            bOK = TvinciImporter.ImporterImpl.DoTheWorkInner(sXml, nGroupID, "", ref sImporterNotifyXML, false);

            string sImporterResponse = "<importer>" + sImporterNotifyXML + "</importer>";
            ImporterResponseToDataTable(ref resultTable, sImporterResponse);

            log.Debug("Excel Feeder - Excel=" + sXml + " Importer= " + sImporterResponse + " ExcelFeeder");

            if (bOK == true)
            {
                DateTime dNow = DateTime.UtcNow;

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("batch_upload_dates");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_BATCH_UPLOAD", "=", dNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            return true;
        }

        protected string ParseExcel()
        {
            try
            {
                StringBuilder sMFeedXML = new StringBuilder();

                sMFeedXML.Append("<feed><export>");

                DataSet ds = GetExcelWorkSheet(sPath, sFileName, 0).Copy();

                int nCount = ds.Tables[0].DefaultView.Count;
                DataTable dd = ds.Tables[0].Copy();

                int nColCount = dd.Columns.Count;

                int nNumOfBasicCols = 0;

                //load all metas
                LoadGroupMetas();
                LoadGroupTags();
                LoadFileParams();

                //array of all cols names
                string[] sCols = new string[nColCount];

                //fill the cols array
                for (int i = 0; i < nColCount; i++)
                {
                    if (!string.IsNullOrEmpty(dd.Columns[i].ColumnName) && dd.Columns[i].ColumnName.ToString().StartsWith("<h>"))
                    {
                        string sType = string.Empty;
                        int nIndex = 0;
                        string sExtra = string.Empty;

                        XmlDocument theDoc = new XmlDocument();
                        theDoc.LoadXml(dd.Columns[i].ColumnName);

                        XmlNode typeNode = theDoc.SelectSingleNode("h/t");
                        if (typeNode != null && typeNode.FirstChild != null)
                        {
                            sType = typeNode.FirstChild.Value;
                        }

                        XmlNode nameNode = theDoc.SelectSingleNode("h/i");
                        if (nameNode != null && nameNode.FirstChild != null)
                        {
                            string sIndex = nameNode.FirstChild.Value;
                            nIndex = int.Parse(sIndex);
                        }

                        XmlNode extraNode = theDoc.SelectSingleNode("h/e");
                        if (extraNode != null && extraNode.FirstChild != null)
                        {
                            sExtra = extraNode.FirstChild.Value;
                        }


                        //Count the number of basic cols
                        if (sType.Equals("BASIC") && string.IsNullOrEmpty(sExtra))
                        {
                            nNumOfBasicCols++;
                        }

                        //Get the cols names
                        sCols[i] = dd.DefaultView[0].Row[i].ToString();

                        if (sType.Equals("STRING") && string.IsNullOrEmpty(sExtra))
                        {
                            if (!hStr.Contains(sCols[i]))
                            {
                                log.Error("Excel Feeder Error - Error, no such String_meta found (" + sCols[i] + ") - ExcelFeeder");
                                errMesage = "Error, no such String_meta found (" + sCols[i] + ")";
                                return string.Empty;
                            }
                        }

                        if (sType.Equals("DOUBLE"))
                        {
                            if (!hDouble.Contains(sCols[i]))
                            {
                                log.Error("Excel Feeder Error - Error, no such Double_meta found (" + sCols[i] + ") ExcelFeeder");
                                errMesage = "Error, no such Double_meta found (" + sCols[i] + ")";
                                return string.Empty;
                            }
                        }

                        if (sType.Equals("Boolean"))
                        {
                            if (!hBool.Contains(sCols[i]))
                            {
                                log.Error("Excel Feeder Error - Error, no such Boolean_meta found (" + sCols[i] + ") ExcelFeeder");
                                errMesage = "Error, no such Boolean_meta found (" + sCols[i] + ")";
                                return string.Empty;
                            }
                        }
                    }
                    else
                    {
                        log.Error("Excel Format Error - Col :(" + (i + 1) + ") " + dd.Columns[i].ColumnName.ToString() + " ExcelFeeder");
                        break;
                    }
                }

                //Check if number of basic cols in excel file equals the number of basic cols in table
                if (!IsNumOfBasicColsIsEqual(nNumOfBasicCols))
                {
                    log.Error("Excel Feeder Error - Error while parsing basics cols. ExcelFeeder");
                    errMesage = "Error while parsing basics cols";
                    return string.Empty;
                }

                //parse all rows in the excel file
                for (int i = 1; i < nCount; i++)
                {

                    string sCoGuid = GetColValue("BASIC", (int)hBasicsIndex["co_guid"], 0, string.Empty, dd, i);   //dd.DefaultView[i].Row[index++].ToString();

                    if (string.IsNullOrEmpty(sCoGuid))
                    {
                        continue;
                    }

                    sMFeedXML.Append("<media ");

                    string sIsActive = GetColValue("BASIC", (int)hBasicsIndex["is_active"], 0, string.Empty, dd, i); //dd.DefaultView[i].Row[index++].ToString().Trim().ToLower();

                    if (sIsActive.Equals("true") || sIsActive.Equals("yes") || sIsActive.Equals("1"))
                    {
                        sIsActive = "true";
                    }
                    else
                    {
                        sIsActive = "false";
                    }

                    sMFeedXML.Append("co_guid=\"" + sCoGuid + "\" action=\"insert\" is_active=\"" + sIsActive + "\" erase=\"false\">");

                    //basic metas
                    sMFeedXML.Append("<basic>");
                    string sMediaType = GetColValue("BASIC", (int)hBasicsIndex["media_type"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<media_type>" + TVinciShared.ProtocolsFuncs.XMLEncode(sMediaType, true) + "</media_type>");

                    //EPG
                    string sEPG = GetColValue("BASIC", (int)hBasicsIndex["epg_identifier"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<epg_identifier>" + TVinciShared.ProtocolsFuncs.XMLEncode(sEPG, true) + "</epg_identifier>");

                    //name
                    sMFeedXML.Append("<name>");
                    string sName = GetColValue("BASIC", (int)hBasicsIndex["Name"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<value lang=\"" + sMainLang + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sName, true) + "</value>");
                    for (int j = 0; j < sLangs.Length; j++)
                    {
                        string val = GetColValue("BASIC", (int)hBasicsIndex["Name"], sLangs[j].Key, "(T)", dd, i);
                        if (!String.IsNullOrEmpty(val))
                            sMFeedXML.Append("<value lang=\"" + sLangs[j].Value + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(val, true) + "</value>");
                    }
                    sMFeedXML.Append("</name>");

                    //description
                    sMFeedXML.Append("<description>");
                    string sDesc = GetColValue("BASIC", (int)hBasicsIndex["Description"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<value lang=\"" + sMainLang + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sDesc, true) + "</value>");
                    for (int j = 0; j < sLangs.Length; j++)
                    {
                        string val = GetColValue("BASIC", (int)hBasicsIndex["Description"], sLangs[j].Key, "(T)", dd, i);
                        if (!String.IsNullOrEmpty(val))
                            sMFeedXML.Append("<value lang=\"" + sLangs[j].Value + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(val, true) + "</value>");
                    }
                    sMFeedXML.Append("</description>");

                    //thumb
                    string sThumb = GetColValue("BASIC", (int)hBasicsIndex["thumb"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<thumb url=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sThumb, true) + "\"/>");

                    if (sRatios != null && sRatios.Count > 0)
                    {
                        sMFeedXML.Append("<pic_ratios>");
                        foreach (KeyValuePair<int, string> ratioKP in sRatios)
                        {
                            try
                            {
                                string val = GetColValue("BASIC", (int)hBasicsIndex["thumb"], 0, ratioKP.Value + "(R)", dd, i);

                                if (!string.IsNullOrEmpty(val))
                                {
                                    sMFeedXML.AppendFormat("<ratio thumb=\"{0}\" ratio=\"{1}\" />", TVinciShared.ProtocolsFuncs.XMLEncode(val, true), ratioKP.Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(string.Empty, ex);
                                //Column does not belong to table - do nothing
                            }
                        }
                        sMFeedXML.Append("</pic_ratios>");
                    }


                    //rules
                    string sWPR = GetColValue("BASIC", (int)hBasicsIndex["watch_per_rule"], 0, string.Empty, dd, i);
                    string sGBR = GetColValue("BASIC", (int)hBasicsIndex["geo_block_rule"], 0, string.Empty, dd, i);
                    string sPR = GetColValue("BASIC", (int)hBasicsIndex["players_rule"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<rules>");
                    sMFeedXML.Append("<watch_per_rule>" + TVinciShared.ProtocolsFuncs.XMLEncode(sWPR, true) + "</watch_per_rule>");
                    sMFeedXML.Append("<geo_block_rule>" + TVinciShared.ProtocolsFuncs.XMLEncode(sGBR, true) + "</geo_block_rule>");
                    sMFeedXML.Append("<players_rule>" + TVinciShared.ProtocolsFuncs.XMLEncode(sPR, true) + "</players_rule>");
                    sMFeedXML.Append("</rules>");

                    //dates
                    string sCSD = GetColValue("BASIC", (int)hBasicsIndex["catalog_start(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty, dd, i);
                    string sSD = GetColValue("BASIC", (int)hBasicsIndex["start(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty, dd, i);
                    string sCE = GetColValue("BASIC", (int)hBasicsIndex["catalog_end(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty, dd, i);
                    string sFE = GetColValue("BASIC", (int)hBasicsIndex["final_end(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty, dd, i);
                    sMFeedXML.Append("<dates>");
                    sMFeedXML.Append("<catalog_start>" + TVinciShared.ProtocolsFuncs.XMLEncode(sCSD, true) + "</catalog_start>");
                    sMFeedXML.Append("<start>" + TVinciShared.ProtocolsFuncs.XMLEncode(sSD, true) + "</start>");
                    sMFeedXML.Append("<catalog_end>" + TVinciShared.ProtocolsFuncs.XMLEncode(sCE, true) + "</catalog_end>");
                    sMFeedXML.Append("<final_end>" + TVinciShared.ProtocolsFuncs.XMLEncode(sFE, true) + "</final_end>");
                    sMFeedXML.Append("</dates>");

                    sMFeedXML.Append("</basic>");
                    sMFeedXML.Append("<structure>");

                    //strings
                    sMFeedXML.Append("<strings>");
                    foreach (DictionaryEntry de in hStr)
                    {
                        string sMetaName = de.Key.ToString();
                        int nID = (int)de.Value;

                        List<KeyValuePair<string, string>> lVals = new List<KeyValuePair<string, string>>();
                        string val = GetColValue("STRING", nID, 0, string.Empty, dd, i);

                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(sMainLang, val);
                        lVals.Add(kvp);

                        for (int j = 0; j < sLangs.Length; j++)
                        {
                            val = GetColValue("STRING", nID, sLangs[j].Key, "(T)", dd, i);
                            kvp = new KeyValuePair<string, string>(sLangs[j].Value, val);
                            lVals.Add(kvp);
                        }

                        sMFeedXML.Append(GetMetaSectionStrings(sMetaName, lVals));
                    }
                    sMFeedXML.Append("</strings>");

                    //doubles
                    sMFeedXML.Append("<doubles>");
                    foreach (DictionaryEntry de in hDouble)
                    {
                        string sMetaName = de.Key.ToString();
                        int nID = (int)de.Value;

                        string val = GetColValue("DOUBLE", nID, 0, string.Empty, dd, i);
                        sMFeedXML.Append(GetMetaSection(sMetaName, val));
                    }
                    sMFeedXML.Append("</doubles>");


                    //booleans
                    sMFeedXML.Append("<booleans>");
                    foreach (DictionaryEntry de in hBool)
                    {
                        string sMetaName = de.Key.ToString();
                        int nID = (int)de.Value;

                        string val = GetColValue("BOOLEAN", nID, 0, string.Empty, dd, i);
                        sMFeedXML.Append(GetMetaSection(sMetaName, val));
                    }
                    sMFeedXML.Append("</booleans>");


                    //Tags
                    sMFeedXML.Append("<metas>");
                    foreach (DictionaryEntry de in hTags)
                    {
                        int nID = (int)de.Key;
                        string sTag = (string)de.Value;

                        string sVal = GetColValue("TAG", nID, 0, string.Empty, dd, i);
                        sMFeedXML.Append(GetMetaSectionTags(sTag, sVal));
                    }
                    sMFeedXML.Append("</metas>");
                    sMFeedXML.Append("</structure>");

                    //Files
                    sMFeedXML.Append("<files>");
                    int nFilesCounter = 1;

                    while (IsColExsits("FILE", (int)hFile["handling_type"], 0, nFilesCounter.ToString(), dd))
                    //while (!string.IsNullOrEmpty(GetColValue("FILE", (int)hFile["handling_type"], 0, nFilesCounter.ToString(), dd, i)))
                    {
                        if (!string.IsNullOrEmpty(GetColValue("FILE", (int)hFile["handling_type"], 0, nFilesCounter.ToString(), dd, i)))
                        {
                            List<KeyValuePair<string, string>> filesVals = new List<KeyValuePair<string, string>>();
                            string sHandlingType = string.Empty;
                            foreach (DictionaryEntry de in hFile)
                            {
                                int nID = (int)de.Value;
                                string sParam = (string)de.Key;

                                string val = GetColValue("FILE", nID, 0, nFilesCounter.ToString(), dd, i);
                                if (sParam == "handling_type")
                                {
                                    sHandlingType = val;
                                }
                                else if (sParam == "file_start_date(dd/mm/yyyy hh:mm:ss)" || sParam == "file_end_date(dd/mm/yyyy hh:mm:ss)")
                                {
                                    int index = sParam.IndexOf('(');
                                    sParam = sParam.Substring(0, index);
                                    if (val != "")
                                    {
                                        DateTime date;
                                        bool parsed = DateTime.TryParseExact(val, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out date);
                                        if (!parsed)
                                        {
                                            val = "";
                                        }
                                    }
                                }
                                else if (sParam == "language")
                                {
                                    sParam = "lang";
                                }
                                else if (sParam == "is_default_language")
                                {
                                    sParam = "default";
                                }

                                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(sParam, val);
                                filesVals.Add(kvp);
                            }
                            sMFeedXML.Append(GetMediaFile(filesVals, sHandlingType));
                        }
                        nFilesCounter++;
                    }

                    sMFeedXML.Append("</files>");
                    sMFeedXML.Append("</media>");
                }

                sMFeedXML.Append("</export></feed>");

                return sMFeedXML.ToString();
            }
            catch (Exception ex)
            {
                log.Error("Excel Feeder Error " + ex.Message + " ExcelFeeder");
                errMesage = ex.Message;
                return string.Empty;
            }
        }

        private bool IsNumOfBasicColsIsEqual(int nNumOfBasic)
        {
            int nBasicCount = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_media_basic_details where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 0);
            selectQuery += "order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                nBasicCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nBasicCount; i++)
                {
                    string sName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", i);
                    int index = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "order_num", i);

                    hBasicsIndex.Add(sName, index);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return (nNumOfBasic > 0 && nNumOfBasic == nBasicCount);

        }

        private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
        {
            try
            {
                int rowNum = TVinciShared.WS_Utils.GetTcmIntValue("EXCEL_MAX_ROW");
                if (rowNum == 0)
                {
                    rowNum = ROW_MAX;
                }

                OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + @"\" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"");
                OleDbCommand ExcelCommand = new OleDbCommand();
                ExcelCommand.Connection = ExcelConnection;
                OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);

                ExcelConnection.Open();
                DataTable ExcelSheets = ExcelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                int i = 0;
                string rangeColumn = CalacRrangeColumns(i, rowNum);
                string SpreadSheetName = "[" + ExcelSheets.Rows[workSheetNumber]["TABLE_NAME"].ToString() + rangeColumn + "]";
                DataSet ExcelDataSet = new DataSet();
                bool keepRead = true;

                while (keepRead)
                {
                    ExcelCommand.CommandText = @"SELECT * FROM " + SpreadSheetName;
                    DataTable dt = new DataTable();
                    dt.TableName = i.ToString();
                    try
                    {
                        ExcelAdapter.Fill(dt);
                        dt.Columns.Add("primeryKey", typeof(string));
                        for (int rowIndex = 1; rowIndex < dt.Rows.Count; rowIndex++)
                        {
                            dt.Rows[rowIndex]["primeryKey"] = rowIndex.ToString();
                        }
                        if (!ExcelDataSet.Tables.Contains(dt.TableName))
                            ExcelDataSet.Tables.Add(dt);
                        if (dt == null || dt.Columns == null || dt.Columns.Count == 0 || dt.Columns.Count < COLUMN_MAX)
                        {
                            keepRead = false;
                        }
                        else
                        {
                            // get ranges 
                            i++;
                            rangeColumn = CalacRrangeColumns(i, rowNum);
                            SpreadSheetName = "[" + ExcelSheets.Rows[workSheetNumber]["TABLE_NAME"].ToString() + rangeColumn + "]";
                        }
                    }
                    catch (OleDbException oleException)
                    {
                        keepRead = false;
                        log.Error("Excel Feeder Error - stop reading excel file - no more columns  " + oleException.Message + " ExcelReader");
                    }
                }
                ExcelConnection.Close();

                // merge all dt to one table in dateset
                DataTable mergeDT = new DataTable();
                List<DataTable> tables = new List<DataTable>();
                foreach (DataTable table in ExcelDataSet.Tables)
                {
                    tables.Add(table);
                }
                mergeDT = MergeAll(tables, "primeryKey");

                ExcelDataSet = new DataSet();
                ExcelDataSet.Tables.Add(mergeDT);

                return ExcelDataSet;
            }
            catch (Exception ex)
            {
                log.Error("Excel Feeder Error - Error opening Excel file " + ex.Message, ex);
                return null;
            }
        }
        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private string CalacRrangeColumns(int index, int rowNum)
        {
            string range = string.Empty;
            try
            {
                int columnnum = COLUMN_MAX;
                string from = GetExcelColumnName(index * columnnum + 1);
                string to = GetExcelColumnName((index * columnnum) + columnnum);
                range = string.Format("{0}1:{1}{2}", from, to, rowNum);
            }
            catch (Exception)
            {
                range = string.Empty;
            }
            return range;
        }

        public static DataTable MergeAll(IList<DataTable> tables, String primaryKeyColumn)
        {
            try
            {
                DataTable table = new DataTable("TblUnion");

                if (!tables.Any())
                    throw new ArgumentException("Tables must not be empty", "tables");
                if (primaryKeyColumn != null)
                    foreach (DataTable t in tables)
                        if (!t.Columns.Contains(primaryKeyColumn))
                            throw new ArgumentException("All tables must have the specified primarykey column " + primaryKeyColumn, "primaryKeyColumn");

                table.BeginLoadData(); // Turns off notifications, index maintenance, and constraints while loading data
                foreach (DataTable t in tables)
                {
                    table.Merge(t); // same as table.Merge(t, false, MissingSchemaAction.Add);
                }
                table.EndLoadData();

                if (primaryKeyColumn != null)
                {
                    // since we might have no real primary keys defined, the rows now might have repeating fields
                    // so now we're going to "join" these rows ...
                    var pkGroups = table.AsEnumerable()
                        .GroupBy(r => r[primaryKeyColumn]);
                    var dupGroups = pkGroups.Where(g => g.Count() > 1);
                    foreach (var grpDup in dupGroups)
                    {
                        // use first row and modify it
                        DataRow firstRow = grpDup.First();
                        foreach (DataColumn c in table.Columns)
                        {
                            if (firstRow.IsNull(c))
                            {
                                DataRow firstNotNullRow = grpDup.Skip(1).FirstOrDefault(r => !r.IsNull(c));
                                if (firstNotNullRow != null)
                                    firstRow[c] = firstNotNullRow[c];
                            }
                        }
                        // remove all but first row
                        var rowsToRemove = grpDup.Skip(1);
                        foreach (DataRow rowToRemove in rowsToRemove)
                            table.Rows.Remove(rowToRemove);
                    }
                    // remove primaryKeyColumn
                    table.Columns.Remove(primaryKeyColumn);
                }
                return table;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }


        static protected string GetMetaSection(string sMetaName, string sValue)
        {
            if (String.IsNullOrEmpty(sValue))
                return "";

            return "<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(sValue.Trim(), true) + "</meta>";
        }


        private string GetMetaSectionStrings(string sMetaName, List<KeyValuePair<string, string>> lVals)
        {
            if (lVals.Count == 0)
                return string.Empty;



            StringBuilder sRet = new StringBuilder();

            sRet.Append("<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName, true) + "\" ml_handling=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("unique", true) + "\">");

            for (int i = 0; i < lVals.Count; i++)
            {
                KeyValuePair<string, string> kvp = lVals[i];

                if (!string.IsNullOrEmpty(kvp.Key))
                    sRet.Append("<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Key, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Value, true) + "</value>");
            }

            sRet.Append("</meta>");

            return sRet.ToString();
        }


        private string GetMetaSectionTags(string sMetaName, string sVal)
        {
            if (String.IsNullOrEmpty(sVal))
                return "";


            string[] seperator = { ";" };

            string[] splited = sVal.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sRet = new StringBuilder();

            sRet.Append("<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName, true) + "\" ml_handling=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("unique", true) + "\">");

            for (int i = 0; i < splited.Length; i++)
            {
                sRet.Append("<container>");
                sRet.Append("<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMainLang, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(splited[i].Trim(), true) + "</value>");
                sRet.Append("</container>");
            }

            sRet.Append("</meta>");

            return sRet.ToString();
        }


        private string GetMetaSectionTags(string sMetaName, List<KeyValuePair<string, string>> lVals)
        {
            if (lVals.Count == 0 || string.IsNullOrEmpty(lVals[0].Key))
                return "";


            string[] seperator = { ";" };

            KeyValuePair<string, string> kvp = lVals[0];

            string[] splited = kvp.Value.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sRet = new StringBuilder();

            sRet.Append("<meta name=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMetaName, true) + "\" ml_handling=\"" + TVinciShared.ProtocolsFuncs.XMLEncode("unique", true) + "\">");

            for (int i = 0; i < splited.Length; i++)
            {
                sRet.Append("<container>");
                sRet.Append("<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sMainLang, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(splited[i].Trim(), true) + "</value>");


                for (int j = 1; j < lVals.Count; j++)
                {
                    kvp = lVals[j];

                    string[] spl = kvp.Value.Split(seperator, StringSplitOptions.RemoveEmptyEntries);


                    if (spl.Length > i)
                        sRet.Append("<value lang=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Key, true) + "\">" + TVinciShared.ProtocolsFuncs.XMLEncode(spl[i].Trim(), true) + "</value>");
                }
                sRet.Append("</container>");
            }

            sRet.Append("</meta>");

            return sRet.ToString();
        }

        private string GetMediaFile(List<KeyValuePair<string, string>> filesVals, string sHandlingType)
        {
            StringBuilder sRet = new StringBuilder();

            if (!string.IsNullOrEmpty(sHandlingType) && (sHandlingType.ToLower().Equals("image") || sHandlingType.ToLower().Equals("clip")))
            {
                sRet.Append("<file");
                foreach (KeyValuePair<string, string> kvp in filesVals)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                    {
                        sRet.Append(" " + kvp.Key + "=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Value, true) + "\"");
                    }
                }
                sRet.Append(" />");
            }

            return sRet.ToString();
        }

        private void ImporterResponseToDataTable(ref DataTable resultTable, string sImporterResponse)
        {

            if (string.IsNullOrEmpty(sImporterResponse))
            {
                DataRow row = resultTable.NewRow();
                row["co_guid"] = "No records";
                resultTable.Rows.Add(row);

                return;
            }

            XmlDocument theDoc = new XmlDocument();

            try
            {
                theDoc.LoadXml(sImporterResponse);

                XmlNode importerNode = theDoc.SelectSingleNode("/importer");

                XmlNodeList theItems = importerNode.ChildNodes;

                int nCount1 = theItems.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    XmlNode node = theItems[i];

                    string co_guid = GetItemParameterVal(ref node, "co_guid");
                    string status = GetItemParameterVal(ref node, "status");
                    string message = GetItemParameterVal(ref node, "message");
                    string tvm_id = GetItemParameterVal(ref node, "tvm_id");

                    DataRow row = resultTable.NewRow();
                    row["co_guid"] = co_guid;
                    row["status"] = status;
                    row["message"] = message;
                    row["media_id"] = tvm_id;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error("Excel Feeder Error - Error parsing importer response " + ex.Message, ex);
            }
        }

        private string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            string sVal = "";
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
                if (theAttr != null)
                {
                    int nCount = theAttr.Count;
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

        private void LoadGroupMetas()
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                string col = string.Empty;
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    //Strings
                    for (int i = 1; i <= 20; i++)
                    {
                        //Get the Value
                        col = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_STR_NAME", 0);
                        if (string.IsNullOrEmpty(col) == false)
                        {
                            hStr.Add(col, i);
                        }
                    }

                    //Doubles
                    for (int i = 1; i <= 10; i++)
                    {
                        //Get the Value
                        col = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_DOUBLE_NAME", 0);
                        if (string.IsNullOrEmpty(col) == false)
                        {
                            hDouble.Add(col, i);
                        }
                    }

                    //Booleans
                    for (int i = 1; i <= 10; i++)
                    {

                        //Get the Value
                        col = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_BOOL_NAME", 0);
                        if (string.IsNullOrEmpty(col) == false)
                        {
                            hBool.Add(col, i);
                        }
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private void LoadGroupTags()
        {
            //Tags
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select id, description from media_tags_types where status = 1 and  (   ";
            selectQuery += " ( TagFamilyID = 1	  and group_id = 0) or (  group_id ";
            string groupsStr = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            selectQuery += groupsStr;
            selectQuery += "  and TagFamilyID IS NULL ) )";
            selectQuery += "order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        //Get the Value and store in list
                        string sTag = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Description", i);
                        int nID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                        hTags.Add(nID, sTag);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            hTags.Add(0, "free");
        }

        private void LoadFileParams()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_media_basic_details where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
            selectQuery += "order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                int nFileCount = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0; i < nFileCount; i++)
                {
                    string sName = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
                    int nOrderNum = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "order_num", i);
                    hFile.Add(sName, nOrderNum);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private string GetColValue(string sType, int nID, int nLang, string sExtra, DataTable dt, int nRowIndex)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<h>");
            sb.Append("<t>" + TVinciShared.ProtocolsFuncs.XMLEncode(sType, true) + "</t>");
            sb.Append("<i>" + nID + "</i>");
            sb.Append("<l>" + nLang + "</l>");
            sb.Append("<e>" + TVinciShared.ProtocolsFuncs.XMLEncode(sExtra, true) + "</e>");
            sb.Append("</h>");

            string val = string.Empty;

            if (dt.Columns.Contains(sb.ToString()) && dt.DefaultView[nRowIndex].Row[sb.ToString()] != null)
            {
                val = dt.DefaultView[nRowIndex].Row[sb.ToString()].ToString();
            }

            return val;
        }

        private bool IsColExsits(string sType, int nID, int nLang, string sExtra, DataTable dt)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<h>");
            sb.Append("<t>" + TVinciShared.ProtocolsFuncs.XMLEncode(sType, true) + "</t>");
            sb.Append("<i>" + nID + "</i>");
            sb.Append("<l>" + nLang + "</l>");
            sb.Append("<e>" + TVinciShared.ProtocolsFuncs.XMLEncode(sExtra, true) + "</e>");
            sb.Append("</h>");

            return dt.Columns.Contains(sb.ToString());
        }
    }
}
