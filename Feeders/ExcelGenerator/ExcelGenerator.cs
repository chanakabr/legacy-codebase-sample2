using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Collections;
using TVinciShared;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace ExcelGenerator
{
    public enum CellType
    {
        BASIC = 1,
        STRING = 2,
        DOUBLE = 3,
        BOOLEAN = 4,
        TAG = 5,
        FILE = 6
    }

    public class ExcelGenerator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private int nGroupID;
        private int nNumberOfFiles;
        private Dictionary<int, string> sLangs;
        private Dictionary<int, string> sRatios;

        private string sMainLang;

        private List<int> meta_str;
        private List<int> meta_double;
        private List<int> meta_bool;
        private List<int> meta_tags;

        private List<string> cells;

        private Hashtable hBasics;
        private Hashtable hFiles;
        private Hashtable hMediaTypes;

        private object objLockDataTable = new object();

        private int nTotalMedias;
        private int nMediaCounter;

        public ExcelGenerator(int nGUID, int nNOF)
        {
            nGroupID = nGUID;
            nNumberOfFiles = nNOF;
            sMainLang = string.Empty;
            GetLang();
            GetGroupPicRatios();
            meta_str = new List<int>();
            meta_double = new List<int>();
            meta_bool = new List<int>();
            meta_tags = new List<int>();

            cells = new List<string>();

            hBasics = new Hashtable();
            hFiles = new Hashtable();
            hMediaTypes = new Hashtable();

            nTotalMedias = 0;
            nMediaCounter = 0;
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

        private void GetLang()
        {
            sLangs = new Dictionary<int, string>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select code3 from lu_languages where id in (select LANGUAGE_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            selectQuery += ")";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "code3", 0);
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
                for (int i = 0; i < nCount; i++)
                {
                    int nLangID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    string sLang = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "code3", i);
                    sLangs.Add(nLangID, sLang);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }

        private void LoadMediaFileType()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select media_type_id, description from groups_media_type where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nMediaType = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_type_id", i);
                    string sDesc = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "description", i);

                    hMediaTypes.Add(nMediaType, sDesc);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private int GetMaxNumOfFiles(int[] medias)
        {
            int nMax = 0;
            if (medias == null || medias.Length == 0)
                return nMax;

            string sMedias = "(";
            for (int i = 0; i < medias.Length; i++)
            {
                if (i > 0)
                {
                    sMedias += ",";
                }
                sMedias += medias[i].ToString();
            }
            sMedias += ")";

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select TOP 1 gr.MEDIA_ID as media_id, gr.countMedia as count_files from ";
            selectQuery += "(select media_id, count(media_id) as countMedia from media_files where STATUS=1 and REF_ID=0 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and media_id in " + sMedias;
            selectQuery += "group by media_id) as gr order by gr.countMedia desc";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nMax = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "count_files", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return nMax;
        }


        public DataTable GetExcelTable(int nEmptyRows)
        {
            DataTable resultTable = new DataTable("resultTable");

            int nBasicCount = 0;
            int nFileCount = 0;

            //Basic

            //Get all basic metas from DB. 
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_media_basic_details where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 0);
            selectQuery += " order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                nBasicCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nBasicCount; i++)
                {

                    string sHeaderName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", i);
                    int nOrderNum = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "order_num", i);

                    hBasics.Add(sHeaderName, nOrderNum);

                    //Create "Basic" Column for every single value
                    DataColumn colBasic = new DataColumn();
                    colBasic.DataType = System.Type.GetType("System.String");
                    colBasic.ColumnName = GetHeaderName(CellType.BASIC, nOrderNum, 0, string.Empty);
                    resultTable.Columns.Add(colBasic);

                    //Get the Value and store it in list
                    cells.Add(sHeaderName);

                    if (sHeaderName.ToLower().Equals("name") || sHeaderName.ToLower().Equals("description"))
                    {
                        foreach (int key in sLangs.Keys)
                        {
                            DataColumn colBasicTranslate = new DataColumn();
                            colBasicTranslate.DataType = System.Type.GetType("System.String");
                            colBasicTranslate.ColumnName = GetHeaderName(CellType.BASIC, nOrderNum, key, "(T)");
                            resultTable.Columns.Add(colBasicTranslate);

                            cells.Add(sHeaderName + "_(" + sLangs[key] + ")");
                        }
                    }
                    else if (sHeaderName.ToLower().Equals("thumb"))
                    {
                        if (sRatios != null && sRatios.Count > 0)
                        {
                            foreach (KeyValuePair<int, string> ratioKP in sRatios)
                            {
                                DataColumn colBasicRatio = new DataColumn();
                                colBasicRatio.DataType = System.Type.GetType("System.String");
                                colBasicRatio.ColumnName = GetHeaderName(CellType.BASIC, nOrderNum, 0, ratioKP.Value + "(R)");
                                resultTable.Columns.Add(colBasicRatio);

                                cells.Add(sHeaderName + "_(" + ratioKP.Value + ")");
                            }
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //Strings, Booleans, Doubles
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    string res = "";

                    //Strings
                    for (int i = 1; i <= 20; i++)
                    {
                        //Get the Value
                        res = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_STR_NAME", 0);
                        if (!String.IsNullOrEmpty(res))
                        {
                            //Create "String" Column
                            DataColumn colString = new DataColumn();
                            colString.DataType = System.Type.GetType("System.String");
                            colString.ColumnName = GetHeaderName(CellType.STRING, i, 0, string.Empty);
                            resultTable.Columns.Add(colString);

                            meta_str.Add(i);

                            cells.Add(res);


                            foreach (int key in sLangs.Keys)
                            {
                                //Create "String" Column
                                DataColumn colStringTranslate = new DataColumn();
                                colStringTranslate.DataType = System.Type.GetType("System.String");
                                colStringTranslate.ColumnName = GetHeaderName(CellType.STRING, i, key, "(T)");
                                resultTable.Columns.Add(colStringTranslate);

                                cells.Add(res + "_(" + sLangs[key] + ")");

                            }
                        }
                    }

                    //Doubles
                    for (int i = 1; i <= 10; i++)
                    {
                        //Get the Value
                        res = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_DOUBLE_NAME", 0);
                        if (!String.IsNullOrEmpty(res))
                        {
                            //Create "Double" Column
                            DataColumn colDouble = new DataColumn();
                            colDouble.DataType = System.Type.GetType("System.String");
                            colDouble.ColumnName = GetHeaderName(CellType.DOUBLE, i, 0, string.Empty);
                            resultTable.Columns.Add(colDouble);

                            //Add Value to list
                            meta_double.Add(i);

                            cells.Add(res);
                        }
                    }

                    //Booleans
                    for (int i = 1; i <= 10; i++)
                    {

                        //Get the Value
                        res = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "META" + i.ToString() + "_BOOL_NAME", 0);
                        if (!String.IsNullOrEmpty(res))
                        {
                            //Create "Boolean" Column
                            DataColumn colBool = new DataColumn();
                            colBool.DataType = System.Type.GetType("System.String");
                            colBool.ColumnName = GetHeaderName(CellType.BOOLEAN, i, 0, string.Empty);
                            resultTable.Columns.Add(colBool);

                            //Add Value to list
                            meta_bool.Add(i);

                            cells.Add(res);
                        }
                    }

                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //Tags + TagsType by group_id = 0 and	TagFamilyID = 1
            string sTag = string.Empty;
            int nID;
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                        sTag = selectQuery.Table("query").DefaultView[i].Row["Description"].ToString();
                        nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                        meta_tags.Add(nID);

                        //Create "Tag" Column
                        DataColumn colTag = new DataColumn();
                        colTag.DataType = System.Type.GetType("System.String");
                        colTag.ColumnName = GetHeaderName(CellType.TAG, nID, 0, string.Empty);
                        resultTable.Columns.Add(colTag);

                        cells.Add(sTag);
                    }

                    //Create "Tag" Column for "Free"
                    DataColumn colTagFree = new DataColumn();
                    colTagFree.DataType = System.Type.GetType("System.String");
                    colTagFree.ColumnName = GetHeaderName(CellType.TAG, 0, 0, string.Empty);
                    resultTable.Columns.Add(colTagFree);

                    cells.Add("Free");
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //Files
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_media_basic_details where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
            selectQuery += "order by order_num";
            if (selectQuery.Execute("query", true) != null)
            {
                nFileCount = selectQuery.Table("query").DefaultView.Count;
                if (nFileCount > 0)
                {
                    for (int j = 1; j <= nNumberOfFiles; j++)
                    {
                        for (int i = 0; i < nFileCount; i++)
                        {
                            string sName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", i);
                            int nOrderNum = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "order_num", i);

                            if (j == 1)
                            {
                                hFiles.Add(sName, nOrderNum);
                            }

                            //Create "File" Column
                            DataColumn colFile = new DataColumn();
                            colFile.DataType = System.Type.GetType("System.String");
                            colFile.ColumnName = GetHeaderName(CellType.FILE, nOrderNum, 0, j.ToString());
                            resultTable.Columns.Add(colFile);

                            cells.Add(sName);
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //create first row with metas names
            DataRow row = resultTable.NewRow();

            int index = 0;

            foreach (string name in cells)
            {
                row[index++] = name;
            }

            resultTable.Rows.Add(row);


            for (int i = 0; i < nEmptyRows; i++)
            {

                DataRow emptyRow = resultTable.NewRow();

                for (int j = 0; j < index; j++)
                {
                    emptyRow[j] = string.Empty;
                }

                resultTable.Rows.Add(emptyRow);
            }

            return resultTable;

        }

        public DataTable GetExcelTableEdit(int[] medias)
        {
            LoadMediaFileType();
            nNumberOfFiles = GetMaxNumOfFiles(medias);

            DataTable resultTable = GetExcelTable(0);

            nMediaCounter = 0;
            nTotalMedias = medias.Length;
            int nNumberOfThreads = 5;



            string strMediaIDs = string.Join(",", medias.Select(x => x.ToString()).ToArray());


            ODBCWrapper.DataSetSelectQuery selectMediasQuery = new ODBCWrapper.DataSetSelectQuery();
            selectMediasQuery += "select * from media";
            selectMediasQuery += "where ID in";
            selectMediasQuery += "(";
            selectMediasQuery += strMediaIDs.ToString();
            selectMediasQuery += ")";
            selectMediasQuery += "and";
            selectMediasQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

            if (selectMediasQuery.Execute("query", true) != null)
            {
                nTotalMedias = selectMediasQuery.Table("query").DefaultView.Count;

                log.Debug("Update - total medias : " + nTotalMedias.ToString() + " ExcelGenerator");

                if (nTotalMedias >= nNumberOfThreads)
                {
                    int section = nTotalMedias / nNumberOfThreads;
                    int remnant = (nTotalMedias % nNumberOfThreads);


                    Thread[] arrThreads = new Thread[nNumberOfThreads];
                    bool[] arrResults = new bool[nNumberOfThreads];

                    for (int i = 1; i <= nNumberOfThreads; i++)
                    {
                        int startIndex = ((i - 1) * section);
                        int endIndex = ((i * section) - 1);
                        if (i == nNumberOfThreads && remnant != 0)
                        {
                            endIndex += remnant;
                        }
                        int currentIndex = i - 1;
                        ThreadStart start = delegate { arrResults[currentIndex] = ProcessScope(selectMediasQuery, startIndex, endIndex, resultTable); };
                        arrThreads[i - 1] = new Thread(start);
                        arrThreads[i - 1].Start();
                    }

                    foreach (Thread t in arrThreads)
                    {
                        t.Join();
                    }

                    if (arrResults.Any(threadResult => threadResult == false))
                    {
                        resultTable.Clear();
                    }
                }
                else
                {
                    for (int mediaIndex = 0; mediaIndex < nTotalMedias; mediaIndex++)
                    {
                        ProcessOneMediaRow(selectMediasQuery, mediaIndex, resultTable);
                    }
                }


            }

            selectMediasQuery.Finish();
            selectMediasQuery = null;

            return resultTable;
        }

        private bool ProcessScope(ODBCWrapper.DataSetSelectQuery selectMediasQuery, int startIndex, int endIndex, DataTable resultTable)
        {
            bool result = true;
            for (int mediaRowIndex = startIndex; mediaRowIndex <= endIndex; mediaRowIndex++)
            {
                try
                {
                    ProcessOneMediaRow(selectMediasQuery, mediaRowIndex, resultTable);
                }
                catch (Exception ex)
                {
                    log.Error("Update - Error occurred on ProcessScope(): " + ex.ToString() + " ExcelGenerator", ex);
                    result = false;
                    break;
                }
            }
            return result;
        }

        private void ProcessOneMediaRow(ODBCWrapper.DataSetSelectQuery selectMediasQuery, int mediaRowIndex, DataTable resultTable)
        {
            DataRow mediaRow = null;
            int nMediaID = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "id", mediaRowIndex);

            lock (objLockDataTable)
            {
                mediaRow = resultTable.NewRow();
            }

            for (int j = 0; j < cells.Count; j++)
            {
                mediaRow[j] = string.Empty;
            }
            Hashtable StringsMetas = new Hashtable();
            Hashtable TagsMetas = new Hashtable();


            string sName = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "name", mediaRowIndex);
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["Name"], 0, string.Empty)] = sName;

            string sDesc = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "description", mediaRowIndex);
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["Description"], 0, string.Empty)] = sDesc;

            string sCoGuid = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "co_guid", mediaRowIndex);
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["co_guid"], 0, string.Empty)] = sCoGuid;


            DateTime dCatalogStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectMediasQuery, "catalog_start_date", mediaRowIndex);
            DateTime dStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectMediasQuery, "start_date", mediaRowIndex);

            DateTime dEndDate = new DateTime(2099, 1, 1);
            DateTime dFinalDate = new DateTime(2099, 1, 1);

            if (selectMediasQuery.Table("query").DefaultView[mediaRowIndex].Row["end_date"] != null &&
                    selectMediasQuery.Table("query").DefaultView[mediaRowIndex].Row["end_date"] != DBNull.Value)
            {
                dEndDate = ODBCWrapper.Utils.GetDateSafeVal(selectMediasQuery, "end_date", mediaRowIndex);
            }

            if (selectMediasQuery.Table("query").DefaultView[mediaRowIndex].Row["final_end_date"] != null &&
                    selectMediasQuery.Table("query").DefaultView[mediaRowIndex].Row["final_end_date"] != DBNull.Value)
            {
                dFinalDate = ODBCWrapper.Utils.GetDateSafeVal(selectMediasQuery, "final_end_date", mediaRowIndex);
            }

            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["catalog_start(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty)] = dCatalogStartDate.ToString("dd/MM/yyyy HH:mm:ss");
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["start(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty)] = dStartDate.ToString("dd/MM/yyyy HH:mm:ss");
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["catalog_end(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty)] = dEndDate.ToString("dd/MM/yyyy HH:mm:ss");
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["final_end(dd/mm/yyyy hh:mm:ss)"], 0, string.Empty)] = dFinalDate.ToString("dd/MM/yyyy HH:mm:ss");

            int nIsActive = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "is_active", mediaRowIndex);
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["is_active"], 0, string.Empty)] = nIsActive;

            int nWatchPermissionTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "Watch_Permission_Type_ID", mediaRowIndex);
            string sWatchPermissionName = string.Empty;
            if (nWatchPermissionTypeID != 0)
            {
                sWatchPermissionName = ODBCWrapper.Utils.GetTableSingleVal("watch_permissions_types", "name", nWatchPermissionTypeID).ToString();
                mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["watch_per_rule"], 0, string.Empty)] = sWatchPermissionName;
            }

            int nBlockTemplateID = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "BLOCK_TEMPLATE_ID", mediaRowIndex);
            string sBlockTemplateName = string.Empty;
            if (nBlockTemplateID != 0)
            {
                sBlockTemplateName = ODBCWrapper.Utils.GetTableSingleVal("geo_block_types", "name", nBlockTemplateID).ToString();
                mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["geo_block_rule"], 0, string.Empty)] = sBlockTemplateName;
            }

            int nPlayersRulesID = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "PLAYERS_RULES", mediaRowIndex);
            string sPlayersRulesName = string.Empty;
            if (nPlayersRulesID != 0)
            {
                sPlayersRulesName = ODBCWrapper.Utils.GetTableSingleVal("players_groups_types", "name", nPlayersRulesID).ToString();
                mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["players_rule"], 0, string.Empty)] = sPlayersRulesName;
            }

            int nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectMediasQuery, "media_type_id", mediaRowIndex);
            string sMediaType = string.Empty;
            if (nMediaTypeID != 0)
            {
                sMediaType = ODBCWrapper.Utils.GetTableSingleVal("media_types", "name", nMediaTypeID).ToString();
                mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["media_type"], 0, string.Empty)] = sMediaType;
            }

            string sEPG = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "epg_identifier", mediaRowIndex);
            mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["epg_identifier"], 0, string.Empty)] = sEPG;

            foreach (int nIndex in meta_double)
            {
                string dVal = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "meta" + nIndex + "_double", mediaRowIndex);
                if (!string.IsNullOrEmpty(dVal))
                {
                    mediaRow[GetHeaderName(CellType.DOUBLE, nIndex, 0, string.Empty)] = dVal;
                }
            }

            foreach (int nIndex in meta_bool)
            {

                string bVal = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "meta" + nIndex + "_bool", mediaRowIndex);
                if (!string.IsNullOrEmpty(bVal))
                {
                    mediaRow[GetHeaderName(CellType.BOOLEAN, nIndex, 0, string.Empty)] = bVal;
                }
            }


            foreach (int nIndex in meta_str)
            {
                string val = ODBCWrapper.Utils.GetStrSafeVal(selectMediasQuery, "meta" + nIndex + "_str", mediaRowIndex);
                if (!string.IsNullOrEmpty(val))
                {
                    mediaRow[GetHeaderName(CellType.STRING, nIndex, 0, string.Empty)] = val;
                }
            }



            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from media_translate where status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += "order by LANGUAGE_ID";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nLangID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "LANGUAGE_ID", i);

                    //if sLangs

                    string sLangName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "name", i);
                    mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["Name"], nLangID, "(T)")] = sLangName;

                    string sLangDesc = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "description", i);
                    mediaRow[GetHeaderName(CellType.BASIC, (int)hBasics["Description"], nLangID, "(T)")] = sLangDesc;

                    foreach (int nIndex in meta_str)
                    {
                        string val = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "meta" + nIndex + "_str", i);
                        if (!string.IsNullOrEmpty(val))
                        {
                            mediaRow[GetHeaderName(CellType.STRING, nIndex, nLangID, "(T)")] = val;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select t.ID, t.VALUE, mtt.ID as tag_type_id from tags t full outer join media_tags_types mtt";
            selectQuery += "on t.TAG_TYPE_ID=mtt.ID join media_tags mt on mt.TAG_ID=t.ID where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.media_id", "=", nMediaID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.status", "=", 1);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mtt.status", "=", 1);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nTagID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    string sVal = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "value", i);
                    int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "tag_type_id", i);

                    if (!TagsMetas.Contains(nTagTypeID))
                    {
                        TagsMetas.Add(nTagTypeID, sVal);
                    }
                    else
                    {
                        TagsMetas[nTagTypeID] += ";" + sVal;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            foreach (DictionaryEntry de in TagsMetas)
            {
                int nKey = (int)de.Key;
                string sVal = (string)de.Value;

                mediaRow[GetHeaderName(CellType.TAG, nKey, 0, string.Empty)] = sVal;
            }

            selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select  media_files.* , sc.streaming_company_name , lu_player_descriptions.[DESCRIPTION], altsc.STREAMING_COMPANY_NAME as 'ASCN'"; //'ALT_STREAMING_COMPANY_NAME'";
            selectQuery += "from media_files left join streaming_companies sc";
            selectQuery += "on media_files.streaming_suplier_id = sc.id left join lu_player_descriptions";
            selectQuery += "on media_files.override_player_type_id = lu_player_descriptions.id";
            selectQuery += "left join streaming_companies altsc";
            selectQuery += "on media_files.ALT_STREAMING_SUPLIER_ID = altsc.id";
            selectQuery += "where media_files.status=1 and media_files.ref_id=0 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_files.group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_files.media_id", "=", nMediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);

                    string sHandlingType = "Clip";

                    int nFileMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_type_id", i);
                    string sFileMediaType = hMediaTypes[nFileMediaTypeID].ToString();

                    int nBillingTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "billing_type_id", i);
                    string sBillingType = (nBillingTypeID == 0) ? string.Empty : ODBCWrapper.Utils.GetTableSingleVal("lu_billing_type", "DESCRIPTION", nBillingTypeID).ToString();

                    int nQualityID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_quality_id", i);
                    string sQuality = (nQualityID == 0) ? string.Empty : ODBCWrapper.Utils.GetTableSingleVal("lu_media_quality", "DESCRIPTION", nQualityID).ToString();

                    string sCDNName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "STREAMING_COMPANY_NAME", i);
                    string sCDNCode = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "STREAMING_CODE", i);

                    string sPPVModule = GetPPVModuleName(nMediaFileID);

                    int nStreamingTypeID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "OVERRIDE_PLAYER_TYPE_ID", i);
                    string sStreamingType = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "DESCRIPTION", i);

                    string sDuration = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "duration", i);

                    int nPreRule = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "COMMERCIAL_TYPE_PRE_ID", i);
                    string sPreRule = GetAdCompName(nPreRule, nGroupID);

                    int nPostRule = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "COMMERCIAL_TYPE_POST_ID", i);
                    string sPostRule = GetAdCompName(nPostRule, nGroupID);

                    int nBreakRule = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "COMMERCIAL_TYPE_BREAK_ID", i);
                    string sBreakRule = GetAdCompName(nBreakRule, nGroupID);

                    int nOverlayRule = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "COMMERCIAL_TYPE_OVERLAY_ID", i);
                    string sOverlayRule = GetAdCompName(nOverlayRule, nGroupID);

                    string sOverlayPoints = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "COMMERCIAL_OVERLAY_POINTS", i);
                    string sBreakPoints = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "COMMERCIAL_BREAK_POINTS", i);

                    string sAdsEnabled = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ADS_ENABLED", i) == 1 ? "true" : "false";
                    string sPreSkip = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "OUTER_COMMERCIAL_SKIP_PRE", i) == 1 ? "true" : "false";
                    string sPostSkip = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "OUTER_COMMERCIAL_SKIP_POST", i) == 1 ? "true" : "false";

                    string sFileCoGuid = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "co_guid", i);

                    string sProductCode = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Product_Code", i);

                    string sLanguage = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "LANGUAGE", i);
                    string sIsDefaultLanguage = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "IS_DEFAULT_LANGUAGE", i);

                    DateTime dFileStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "START_DATE", i);
                    DateTime dFileEndDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "END_DATE", i);

                    string sAltCDNCode = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ALT_STREAMING_CODE", i);
                    string sAltCDNName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ASCN", i);
                    string sAltFileCoGuid = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "alt_co_guid", i);

                    string sExtra = (i + 1).ToString();
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["handling_type"], 0, sExtra)] = sHandlingType;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["Type"], 0, sExtra)] = sFileMediaType;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["billing_type"], 0, sExtra)] = sBillingType;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["quality"], 0, sExtra)] = sQuality;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["cdn_name"], 0, sExtra)] = sCDNName;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["cdn_code"], 0, sExtra)] = sCDNCode;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["player_type"], 0, sExtra)] = sStreamingType;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["assetDuration"], 0, sExtra)] = sDuration;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["PPV_Module"], 0, sExtra)] = sPPVModule;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["pre_rule"], 0, sExtra)] = sPreRule;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["post_rule"], 0, sExtra)] = sPostRule;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["break_rule"], 0, sExtra)] = sBreakRule;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["break_points"], 0, sExtra)] = sBreakPoints;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["overlay_rule"], 0, sExtra)] = sOverlayRule;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["overlay_points"], 0, sExtra)] = sOverlayPoints;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["ads_enabled"], 0, sExtra)] = sAdsEnabled;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["pre_skip_enabled"], 0, sExtra)] = sPreSkip;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["post_skip_enabled"], 0, sExtra)] = sPostSkip;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["co_guid"], 0, sExtra)] = sFileCoGuid;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["product_code"], 0, sExtra)] = sProductCode;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["language"], 0, sExtra)] = sLanguage;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["is_default_language"], 0, sExtra)] = sIsDefaultLanguage;

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["file_start_date(dd/mm/yyyy hh:mm:ss)"], 0, sExtra)] = dFileStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["file_end_date(dd/mm/yyyy hh:mm:ss)"], 0, sExtra)] = dFileEndDate.ToString("dd/MM/yyyy HH:mm:ss");

                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["alt_cdn_name"], 0, sExtra)] = sAltCDNName;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["alt_cdn_code"], 0, sExtra)] = sAltCDNCode;
                    mediaRow[GetHeaderName(CellType.FILE, (int)hFiles["alt_co_guid"], 0, sExtra)] = sAltFileCoGuid;
                }

                lock (objLockDataTable)
                {
                    resultTable.Rows.Add(mediaRow);
                    nMediaCounter++;
                    log.Debug("Add Media - " + nMediaCounter.ToString() + "/" + nTotalMedias.ToString() + " ExcelGenerator");
                }
            }
            selectQuery.Finish();
            selectQuery = null;



        }

        private string GetHeaderName(CellType eCellType, int nID, int nLang, string sExtra)
        {
            // <h> header <t> type <i> id <l> lang <e> extra
            StringBuilder sb = new StringBuilder();
            sb.Append("<h>");
            sb.Append("<t>" + TVinciShared.ProtocolsFuncs.XMLEncode(eCellType.ToString(), true) + "</t>");
            sb.Append("<i>" + nID.ToString() + "</i>");
            sb.Append("<l>" + nLang + "</l>");
            sb.Append("<e>" + TVinciShared.ProtocolsFuncs.XMLEncode(sExtra, true) + "</e>");
            sb.Append("</h>");

            return sb.ToString();
        }

        private string GetAdCompName(int nID, int nGroupID)
        {
            string res = string.Empty;

            if (nID == 0)
                return res;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ads_company_name from ads_companies where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += " order by id desc";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    res = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ads_company_name", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return res;
        }

        private string GetPPVModuleName(int nMediaFileID)
        {
            int nPPVID = 0;
            string sPPVName = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select ppv_module_id from ppv_modules_media_files where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", nMediaFileID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nPPVID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ppv_module_id", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select name from ppv_modules where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nPPVID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sPPVName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "name", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return sPPVName;
        }
    }
}
