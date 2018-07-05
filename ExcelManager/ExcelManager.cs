using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace ExcelManager
{
    public class ExcelManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string excelTemplateDir = string.Format("{0}ExcelTemplates\\", AppDomain.CurrentDomain.BaseDirectory);
        private static object locker = new object();
        private static ExcelManager instance = null;
        private const int MAX_COLUMNS = 254; 
        private const int MAX_ROWS = 500;
        private const int EXCEL_DEFINITION_ROW = 1;
        private const int ACCOUNT_DEFINITION_ROW = 2;
        private const string WITH_FILES = "with_files";
        private const string WITHOUT_FILES = "without_files";
        private const string WITH_Images = "with_images";
        private const string WITHOUT_Images = "without_images";

        #region AssetFile Constants

        private const string URL = "Url";
        private const string DURATION = "Duration";
        private const string FILE_EXTERNAL_ID = "FileExternalId";
        private const string FILE_ALT_EXTERNAL_ID = "FileAltExternalId";
        private const string FILE_EXTERNAL_STORE_ID = "FileExternalStoreId";
        private const string CDN_ADAPTER_PROFILE_ID = "CdnAdapaterProfileId";
        private const string ALT_STREAMING_CODE = "AltStreamingCode";
        private const string ALT_CDN_ADAPTER_PROFILE_ID = "AlternativeCdnAdapaterProfileId";
        private const string ADDITIONAL_DATA = "AdditionalData";
        private const string BILLING_TYPE = "BillingType";
        private const string ORDER_NUM = "OrderNum";
        private const string LANGUAGE = "Language";
        private const string IS_DEFAULT_LANGUAGE = "IsDefaultLanguage";
        private const string OUTPUT_PROTECT_LVL = "OutputProtecationLevel";
        private const string FILE_START_DATE = "FileStartDate";
        private const string FILE_END_DATE = "FileEndDate";
        private const string FILE_SIZE = "FileSize";
        private const string FILE_IS_ACTIVE = "FileIsActive";

        public static readonly HashSet<string> ASSET_FILE_PROPERTIES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            URL, DURATION, FILE_EXTERNAL_ID, FILE_ALT_EXTERNAL_ID, FILE_EXTERNAL_STORE_ID, CDN_ADAPTER_PROFILE_ID, ALT_STREAMING_CODE, ALT_CDN_ADAPTER_PROFILE_ID, ADDITIONAL_DATA,
            BILLING_TYPE, ORDER_NUM, LANGUAGE, IS_DEFAULT_LANGUAGE, OUTPUT_PROTECT_LVL, FILE_START_DATE, FILE_END_DATE, FILE_SIZE,  FILE_IS_ACTIVE
        };

        #endregion

        private ExcelManager()
        {
            Directory.CreateDirectory(excelTemplateDir);
        }

        public static ExcelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new ExcelManager();
                        }
                    }
                }

                return instance;
            }
        }

        #region Public Methods

        public Dictionary<string, Status> InsertAssetsFromExcel(int groupId, string mediaType, string filePath, string fileName, long userId)
        {
            Dictionary<string, Status> externalIdToResultMap = null;
            try
            {
                //TODO: validate media type exists
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling InsertAssetsFromExcel", groupId);
                    return externalIdToResultMap;
                }

                if (!catalogGroupCache.AssetStructsMapById.Where(x => x.Value.Name ==(mediaType)).Any())
                {
                    log.ErrorFormat("mediaType {0} doesn't exist for groupId: {1} when calling InsertAssetsFromExcel", mediaType, groupId);
                    return externalIdToResultMap;
                }

                AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById.Where(x => x.Value.Name == (mediaType)).First().Value;
                long mediaTypeId = assetStruct.Id;
                DataTable dt = GetExcelWorkSheet(filePath, fileName);
                Dictionary<int, ExcelDefinitionColumn> noneBasicColumnDefinitions = null;
                Dictionary<string, int> basicColumnsSystemNameToindexMap = null;                
                Dictionary<string, MediaAsset> externalIdToMediaAssetMap = null;
                if (dt != null && dt.Rows != null && dt.Rows.Count > 1 && dt.Columns != null && dt.Columns.Count > 0)
                {                     
                    if (FillColumnDefinitionsAndIndexMapping(ref catalogGroupCache, new HashSet<long>(assetStruct.MetaIds), dt.Columns, ref noneBasicColumnDefinitions, ref basicColumnsSystemNameToindexMap))
                    if (basicColumnsSystemNameToindexMap == null || basicColumnsSystemNameToindexMap.Count == 0 || noneBasicColumnDefinitions == null)
                    {
                        // TODO: add log
                        return externalIdToResultMap;
                    }

                    int externalIdIndex = basicColumnsSystemNameToindexMap.ContainsKey(AssetManager.EXTERNAL_ID_META_SYSTEM_NAME) ? basicColumnsSystemNameToindexMap[AssetManager.EXTERNAL_ID_META_SYSTEM_NAME] : -1;
                    if (!FillExternalIdToMediaAssetMap(groupId, dt.Rows, externalIdIndex, ref externalIdToMediaAssetMap))
                    {
                        // TODO: add log
                        return externalIdToResultMap;
                    }

                    externalIdToResultMap = new Dictionary<string, Status>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        MediaAsset mediaAsset = new MediaAsset() { MediaType = new Core.Catalog.MediaType(mediaType, (int)mediaTypeId) };
                        string externalId = GetStringValueFromCell(dr, externalIdIndex);
                        if (string.IsNullOrEmpty(externalId))
                        {
                            externalIdToResultMap[externalId] = new Status((int)eResponseStatus.MissingBasicValueForAsset, string.Format("{0} for {1}",
                                                                            eResponseStatus.MissingBasicValueForAsset.ToString(), AssetManager.EXTERNAL_ID_META_SYSTEM_NAME));
                            continue;
                        }

                        mediaAsset.CoGuid = externalId;
                        bool doesMediaAlreadyExists = externalIdToMediaAssetMap[externalId].Id > 0;
                        #region Asset Properties

                        Status addMediaStatus = FillMediaAssetBasicColumns(dr, doesMediaAlreadyExists, ref basicColumnsSystemNameToindexMap, ref mediaAsset);
                        if (!IsValidStatus(addMediaStatus, externalId, ref externalIdToResultMap))
                        {
                            continue;
                        }
                                                
                        foreach (KeyValuePair<int, ExcelDefinitionColumn> pair in noneBasicColumnDefinitions.Where(x => x.Value.Type == ApiObjects.ExcelColumnType.Asset))
                        {
                            addMediaStatus = AddColumnValueToMediaAsset(dr[pair.Key].ToString(), pair.Value, ref mediaAsset);
                            if (!IsValidStatus(addMediaStatus, externalId, ref externalIdToResultMap))
                            {
                                break;
                            }
                        }

                        if (!IsValidStatus(addMediaStatus, externalId, ref externalIdToResultMap))
                        {
                            continue;
                        }

                        GenericResponse<Asset> assetGenResponse = new GenericResponse<Asset>();
                        if (doesMediaAlreadyExists)
                        {
                            assetGenResponse = AssetManager.UpdateAsset(groupId, externalIdToMediaAssetMap[externalId].Id, ApiObjects.eAssetTypes.MEDIA, mediaAsset, userId);
                        }
                        else
                        {
                            assetGenResponse = AssetManager.AddAsset(groupId, ApiObjects.eAssetTypes.MEDIA, mediaAsset, userId);
                        }

                        if (assetGenResponse != null && !IsValidStatus(assetGenResponse.Status, externalId, ref externalIdToResultMap))
                        {                            
                            continue;
                        }

                        #endregion

                        #region Files

                        List<AssetFile> files = CreateAssetFileFromExcelData(dr, assetGenResponse.Object.Id, noneBasicColumnDefinitions);
                        GenericResponse<AssetFile> assetFileGenResponse = new GenericResponse<AssetFile>();
                        foreach (AssetFile file in files)
                        {
                            if (file.Id > 0)
                            {
                                assetFileGenResponse = FileManager.UpdateMediaFile(groupId, file, userId);
                            }
                            else
                            {
                                assetFileGenResponse = FileManager.InsertMediaFile(groupId, userId, file);
                            }

                            if (assetFileGenResponse != null && !IsValidStatus(assetFileGenResponse.Status, externalId, ref externalIdToResultMap))
                            {
                                break;
                            }
                        }

                        if (assetFileGenResponse != null && !IsValidStatus(assetFileGenResponse.Status, externalId, ref externalIdToResultMap))
                        {
                            continue;
                        }

                        #endregion

                        #region Images

                        List<Image> images = CreateAssetImagesFromExcelData(dr, assetGenResponse.Object.Id, noneBasicColumnDefinitions);
                        GenericResponse<Image> assetImageGenResponse = new GenericResponse<Image>();
                        foreach (Image image in images)
                        {
                            if (image.Id == 0)
                            {
                                assetImageGenResponse = ImageManager.AddImage(groupId, image, userId);
                                if (assetImageGenResponse != null && !IsValidStatus(assetImageGenResponse.Status, externalId, ref externalIdToResultMap))
                                {
                                    break;
                                }
                            }

                            assetImageGenResponse.SetStatus(ImageManager.SetContent(groupId, userId, image.Id, image.Url));
                            if (!IsValidStatus(assetImageGenResponse.Status, externalId, ref externalIdToResultMap))
                            {
                                break;
                            }
                        }

                        if (assetImageGenResponse != null && !IsValidStatus(assetImageGenResponse.Status, externalId, ref externalIdToResultMap))
                        {
                            continue;
                        }

                        #endregion

                        externalIdToResultMap[externalId] = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InsertAssetsFromExcel, groupId: {0}, mediaType: {1}, filePath: {2}, fileName: {3}", groupId, mediaType, filePath, fileName), ex);
            }

            return externalIdToResultMap;
        }

        public byte[] GenerateExcelAsBytes(int groupId, string mediaType, bool shouldGenerateFiles, bool shouldGenerateImages)
        {
            byte[] res = null;
            try
            {
                byte[] tempRes = null;
                string key = LayeredCacheKeys.GetExcelTemplateKey(groupId, mediaType, shouldGenerateFiles, shouldGenerateImages);
                if (!LayeredCache.Instance.Get<byte[]>(key, ref tempRes, GetExcelFileBytes, new Dictionary<string, object>() { { "groupId", groupId }, { "mediaType", mediaType },
                                                    { "shouldGenerateFiles", shouldGenerateFiles }, { "shouldGenerateImages", shouldGenerateImages } }, groupId,
                                                    LayeredCacheConfigNames.GET_EXCEL_TEMPLATE, new List<string>() { LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting GetExcelFileBytes from LayeredCache, groupId: {0}, mediaType: {1}, shouldGenerateFiles: {2}, shouldGenerateImages: {3}",
                                        groupId, mediaType, shouldGenerateFiles, shouldGenerateImages);
                }
                else
                {
                    res = tempRes.ToArray();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GenerateExcelAsBytes for groupId: {0}, mediaType: {1}, shouldGenerateFiles: {2}, shouldGenerateImages: {3}",
                                        groupId, mediaType, shouldGenerateFiles, shouldGenerateImages), ex);
            }

            return res;
        }

        #endregion

        #region Private Methods

        private Tuple<byte[], bool> GetExcelFileBytes(Dictionary<string, object> funcParams)
        {
            bool res = false;
            byte[] fileData = null;
            try
            {                
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaType") 
                    && funcParams.ContainsKey("shouldGenerateFiles") && funcParams.ContainsKey("shouldGenerateImages"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    string mediaType = funcParams["mediaType"].ToString();
                    bool? shouldGenerateFiles = funcParams["shouldGenerateFiles"] as bool?;
                    bool? shouldGenerateImages = funcParams["shouldGenerateImages"] as bool?;
                    if (groupId.HasValue && !string.IsNullOrEmpty(mediaType) && shouldGenerateFiles.HasValue && shouldGenerateImages.HasValue)
                    {
                        string savedFileName;
                        if (GenerateExcelTemplate(groupId.Value, mediaType, shouldGenerateFiles.Value, shouldGenerateImages.Value, out savedFileName)
                            && string.IsNullOrEmpty(savedFileName))
                        {
                            fileData = File.ReadAllBytes(savedFileName);
                            res = fileData != null && fileData.Length > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetExcelFileBytes failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<byte[], bool>(fileData, res);
        }

        private bool GenerateExcelTemplate(int groupId, string mediaType, bool shouldGenerateFiles, bool shouldGenerateImages, out string savedFileName)
        {
            bool res = false;
            savedFileName = string.Empty;
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("Failed to get catalogGroupCache for groupId: {0} when calling GenerateExcelTemplate", groupId);
                    return res;
                }

                if (catalogGroupCache.AssetStructsMapById.Values.Count(x => x.Name == mediaType) != 1)
                {
                    log.ErrorFormat("MediaType {0} doesn't exist for groupId: {1}", mediaType, groupId);
                    return res;
                }

                GenericListResponse<MediaFileType> groupFileTypes = null;
                if (shouldGenerateFiles)
                {
                    groupFileTypes = FileManager.GetMediaFileTypes(groupId);
                    if (groupFileTypes == null || groupFileTypes.Status == null || groupFileTypes.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed to get group file types for groupId: {0}", groupId);
                        return res;
                    }
                }

                GenericListResponse<ImageType> groupImageTypes = null;
                if (shouldGenerateImages)
                {
                    groupImageTypes = ImageManager.GetImageTypes(groupId, true, null);
                    if (groupImageTypes == null || groupImageTypes.Status == null || groupImageTypes.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed to get group image types for groupId: {0}", groupId);
                        return res;
                    }
                }

                Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook workbook = app.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet sheet = workbook.ActiveSheet;
                Dictionary<int, string> indexToAccountDescMap = new Dictionary<int, string>();
                int columnIndex = 1;

                #region MediaAsset

                foreach (PropertyInfo property in typeof(MediaAsset).GetProperties().Where(x => !x.GetCustomAttribute<ExcelTemplateAttribute>().IgnoreWhenGeneratingTemplate))
                {
                    ExcelTemplateAttribute custAtt = property.GetCustomAttribute<ExcelTemplateAttribute>();
                    ExcelDefinitionColumn defColumn = new ExcelAssetDefinitionColumn()
                    {
                        IsKey = custAtt.IsKeyProperty,
                        IsValueRequired = custAtt.PropertyValueRequired,
                        LanguageId = catalogGroupCache.DefaultLanguage.ID,
                        SystemName = property.Name,
                        Propertytype = property.PropertyType,
                        Type = ApiObjects.ExcelColumnType.Asset
                    };

                    sheet.Cells[EXCEL_DEFINITION_ROW, columnIndex] = Newtonsoft.Json.JsonConvert.SerializeObject(defColumn);
                    sheet.Cells[ACCOUNT_DEFINITION_ROW, columnIndex] = custAtt.SystemName;
                    columnIndex++;
                }

                #endregion

                #region Topics

                AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById.Values.Where(x => x.Name == mediaType).First();
                foreach (long topicId in assetStruct.MetaIds)
                {
                    Topic topic = catalogGroupCache.TopicsMapById.ContainsKey(topicId) ? catalogGroupCache.TopicsMapById[topicId] : null;
                    if (topic == null)
                    {
                        continue;
                    }

                    ExcelDefinitionColumn defColumn = new ExcelTopicDefinitionColumn()
                    {
                        IsValueRequired = false,
                        LanguageId = catalogGroupCache.DefaultLanguage.ID,
                        SystemName = topic.SystemName,
                        metaType = topic.Type,
                        Type = ApiObjects.ExcelColumnType.Topic
                    };

                    sheet.Cells[EXCEL_DEFINITION_ROW, columnIndex] = Newtonsoft.Json.JsonConvert.SerializeObject(defColumn);
                    sheet.Cells[ACCOUNT_DEFINITION_ROW, columnIndex] = topic.Name;
                    columnIndex++;
                }

                #endregion

                #region Files

                if (shouldGenerateFiles)
                {
                    if (groupFileTypes.TotalItems > 0)
                    {
                        foreach (PropertyInfo property in typeof(AssetFile).GetProperties().Where(x => !x.GetCustomAttribute<ExcelTemplateAttribute>().IgnoreWhenGeneratingTemplate))
                        {
                            ExcelTemplateAttribute custAtt = property.GetCustomAttribute<ExcelTemplateAttribute>();
                            foreach (MediaFileType fileType in groupFileTypes.Objects)
                            {
                                ExcelDefinitionColumn defColumn = new ExcelFileDefinitionColumn()
                                {
                                    IsValueRequired = custAtt.PropertyValueRequired,
                                    SystemName = property.Name,
                                    Type = ApiObjects.ExcelColumnType.File,
                                    FileTypeId = fileType.Id
                                };

                                sheet.Cells[EXCEL_DEFINITION_ROW, columnIndex] = Newtonsoft.Json.JsonConvert.SerializeObject(defColumn);
                                sheet.Cells[ACCOUNT_DEFINITION_ROW, columnIndex] = string.Format("{0}_{1}", fileType.Name, property.Name);
                                columnIndex++;
                            }
                        }
                    }
                }

                #endregion

                #region Images

                if (shouldGenerateImages)
                {
                    if (groupImageTypes.TotalItems > 0)
                    {
                        foreach (PropertyInfo property in typeof(Image).GetProperties().Where(x => !x.GetCustomAttribute<ExcelTemplateAttribute>().IgnoreWhenGeneratingTemplate))
                        {
                            ExcelTemplateAttribute custAtt = property.GetCustomAttribute<ExcelTemplateAttribute>();
                            foreach (ImageType imageType in groupImageTypes.Objects)
                            {
                                ExcelDefinitionColumn defColumn = new ExcelImageDefinitionColumn()
                                {
                                    IsValueRequired = custAtt.PropertyValueRequired,
                                    SystemName = property.Name,
                                    Type = ApiObjects.ExcelColumnType.Image,
                                    ImageTypeId = imageType.Id
                                };

                                sheet.Cells[EXCEL_DEFINITION_ROW, columnIndex] = Newtonsoft.Json.JsonConvert.SerializeObject(defColumn);
                                sheet.Cells[ACCOUNT_DEFINITION_ROW, columnIndex] = string.Format("{0}_{1}", imageType.SystemName, property.Name);
                                columnIndex++;
                            }
                        }
                    }
                }

                #endregion

                string tempSavedFileName = string.Format("{0}_{1}_{2}_{3}.xls", excelTemplateDir, mediaType, shouldGenerateFiles ? WITH_FILES : WITHOUT_FILES, shouldGenerateImages ? WITH_Images : WITHOUT_Images);
                workbook.SaveAs(tempSavedFileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, System.Reflection.Missing.Value, System.Reflection.Missing.Value, false, false,
                        Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlShared, false, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                        System.Reflection.Missing.Value);
                workbook.Close(true, Type.Missing, Type.Missing);
                app.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(app);

                res = true;
                savedFileName = tempSavedFileName;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GenerateExcelTemplate for groupId: {0}, mediaType: {1}, shouldGenerateFiles: {2}, shouldGenerateImages: {3}",
                                        groupId, mediaType, shouldGenerateFiles, shouldGenerateImages), ex);
            }

            return res;
        }

        private string GetExcelColumnName(int columnNumber)
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

        private string CalacRangeColumns(int index)
        {
            string range = string.Empty;
            try
            {
                int columnnum = MAX_COLUMNS;
                string from = GetExcelColumnName(index * columnnum + 1);
                string to = GetExcelColumnName((index * columnnum) + columnnum);
                range = string.Format("{0}1:{1}{2}", from, to, MAX_ROWS);
            }
            catch (Exception)
            {
                range = string.Empty;
            }
            return range;
        }

        private DataTable MergeAll(IList<DataTable> tables, String primaryKeyColumn)
        {
            try
            {
                DataTable table = new DataTable();

                if (!tables.Any())
                {
                    throw new ArgumentException("Tables must not be empty", "tables");
                }

                if (primaryKeyColumn != null)
                {
                    foreach (DataTable t in tables)
                    {
                        if (!t.Columns.Contains(primaryKeyColumn))
                        {
                            throw new ArgumentException("All tables must have the specified primary-key column " + primaryKeyColumn, "primaryKeyColumn");
                        }
                    }
                }

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
                                {
                                    firstRow[c] = firstNotNullRow[c];
                                }
                            }
                        }

                        // remove all but first row
                        var rowsToRemove = grpDup.Skip(1);
                        foreach (DataRow rowToRemove in rowsToRemove)
                        {
                            table.Rows.Remove(rowToRemove);
                        }
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

        private DataTable GetExcelWorkSheet(string filePath, string fileName)
        {
            DataTable result = null;

            try
            {
                OleDbConnection excelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + @"\" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"");
                OleDbCommand excelCommand = new OleDbCommand();
                excelCommand.Connection = excelConnection;
                OleDbDataAdapter excelAdapter = new OleDbDataAdapter(excelCommand);

                excelConnection.Open();
                DataTable excelSheet = excelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                int i = 0;
                string rangeColumn = CalacRangeColumns(i);
                string spreadSheetName = "[" + excelSheet.Rows[0]["TABLE_NAME"].ToString() + rangeColumn + "]";
                List<DataTable> tables = new List<DataTable>();
                bool keepRead = true;

                while (keepRead)
                {
                    excelCommand.CommandText = @"SELECT * FROM " + spreadSheetName;
                    DataTable dt = new DataTable();
                    dt.TableName = i.ToString();
                    try
                    {
                        excelAdapter.Fill(dt);
                        dt.Columns.Add("primeryKey", typeof(string));
                        for (int rowIndex = 1; rowIndex < dt.Rows.Count; rowIndex++)
                        {
                            dt.Rows[rowIndex]["primeryKey"] = rowIndex.ToString();
                        }

                        tables.Add(dt);

                        if (dt == null || dt.Columns == null || dt.Columns.Count == 0 || dt.Columns.Count < MAX_COLUMNS)
                        {
                            keepRead = false;
                        }
                        else
                        {
                            // get ranges 
                            i++;
                            rangeColumn = CalacRangeColumns(i);
                            spreadSheetName = "[" + excelSheet.Rows[0]["TABLE_NAME"].ToString() + rangeColumn + "]";
                        }
                    }
                    catch (OleDbException oleException)
                    {
                        keepRead = false;
                        log.Error("Excel Feeder Error - stop reading excel file - no more columns  " + oleException.Message + " ExcelReader");
                    }
                }

                excelConnection.Close();
                result = MergeAll(tables, "primeryKey");                
            }
            catch (Exception ex)
            {
                log.Error("Excel Feeder Error - Error opening Excel file " + ex.Message, ex);                
            }

            return result;
        }

        private bool FillColumnDefinitionsAndIndexMapping(ref CatalogGroupCache catalogGroupCache, HashSet<long> assetStructMetaIds, DataColumnCollection columns,
                                                                ref Dictionary<int, ExcelDefinitionColumn> noneBasicColumnDefinitions, ref Dictionary<string, int> basicColumnsSystemNameToindexMap)
        {
            bool result = false;
            try
            {
                if (columns != null)
                {                    
                    for(int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                    {
                        noneBasicColumnDefinitions = new Dictionary<int, ExcelDefinitionColumn>();
                        basicColumnsSystemNameToindexMap = new Dictionary<string, int>();
                        // TODO: Create dictionary + consider what to do with invalid column \ systemNames that don't exist \ topics that don't exist on the media type (which I need to get in the call)
                        string columnData = columns[columnIndex].ColumnName;
                        ExcelDefinitionColumn defColumn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExcelDefinitionColumn>(columnData);
                        switch (defColumn.Type)
                        {
                            case ApiObjects.ExcelColumnType.Asset:
                                defColumn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExcelAssetDefinitionColumn>(columnData);
                                break;
                            case ApiObjects.ExcelColumnType.Topic:
                                defColumn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExcelTopicDefinitionColumn>(columnData);
                                break;
                            case ApiObjects.ExcelColumnType.Image:
                                defColumn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExcelImageDefinitionColumn>(columnData);
                                break;
                            case ApiObjects.ExcelColumnType.File:
                                defColumn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExcelFileDefinitionColumn>(columnData);
                                break;
                            default:
                                break;
                        }

                        if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(defColumn.SystemName) &&
                            assetStructMetaIds.Contains(catalogGroupCache.TopicsMapBySystemName[defColumn.SystemName].Id))
                        {
                            if (AssetManager.BasicMetasSystemNames.Contains(defColumn.SystemName))
                            {
                                basicColumnsSystemNameToindexMap.Add(defColumn.SystemName, columnIndex);
                            }
                            else
                            {
                                noneBasicColumnDefinitions.Add(columnIndex, defColumn);
                            }
                        }

                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }
        private Status AddColumnValueToMediaAsset(string columnValue, ExcelDefinitionColumn columnDefinition, ref MediaAsset mediaAsset)
        {
            Status result = new Status((int)eResponseStatus.Error, string.Format("{0} for column {1}, column value: {2}, externalId:{3}", eResponseStatus.Error.ToString(),
                                                                                columnDefinition.SystemName, columnValue, mediaAsset.CoGuid));
            try
            {
                //TODO: ...
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private Status FillMediaAssetBasicColumns(DataRow dr, bool doesMediaAlreadyExists, ref Dictionary<string, int> basicColumnsSystemNameToindexMap, ref MediaAsset mediaAsset)
        {
            Status result = new Status((int)eResponseStatus.MissingBasicValueForAsset, string.Format("{0} for externalId: {1}, doesMediaAlreadyExists: {2}",
                                                                                        eResponseStatus.MissingBasicValueForAsset.ToString(), mediaAsset.CoGuid, doesMediaAlreadyExists.ToString()));
            try
            {
                //TODO:..
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private string GetStringValueFromCell(DataRow dr, int columnIndex)
        {
            string result = string.Empty;      
            try
            {
                if (dr != null && dr[columnIndex] != DBNull.Value)
                {
                    result = dr[columnIndex].ToString();
                }
            }
            catch (Exception ex)
            {              
            }

            return result;
        }

        private long? GetNullableLongValueFromCell(DataRow dr, int columnIndex)
        {
            long? result = null;
            try
            {
                if (dr != null && dr[columnIndex] != DBNull.Value)
                {
                    long tempRes = 0;
                    if (long.TryParse(dr[columnIndex].ToString(), out tempRes))
                    {
                        result = tempRes;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private int? GetNullableIntValueFromCell(DataRow dr, int columnIndex)
        {
            int? result = null;
            try
            {
                if (dr != null && dr[columnIndex] != DBNull.Value)
                {
                    int tempRes = 0;
                    if (int.TryParse(dr[columnIndex].ToString(), out tempRes))
                    {
                        result = tempRes;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private bool? GetNullableBoolValueFromCell(DataRow dr, int columnIndex)
        {
            bool? result = null;
            try
            {
                if (dr != null && dr[columnIndex] != DBNull.Value)
                {
                    bool tempRes = false;
                    if (bool.TryParse(dr[columnIndex].ToString(), out tempRes))
                    {
                        result = tempRes;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private DateTime? GetNullableDateTimeValueFromCell(DataRow dr, int columnIndex)
        {
            DateTime? result = null;
            try
            {
                if (dr != null && dr[columnIndex] != DBNull.Value)
                {
                    DateTime tempRes = DateTime.MinValue;
                    if (DateTime.TryParse(dr[columnIndex].ToString(), out tempRes))
                    {
                        result = tempRes;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private bool FillExternalIdToMediaAssetMap(int groupId, DataRowCollection rows, int externalIdIndex, ref Dictionary<string, MediaAsset> externalIdToMediaAssetMap)
        {
            bool res = false;
            try
            {
                if (rows != null && rows.Count > 0 && externalIdIndex >= 0)
                {
                    HashSet<string> externalIds = new HashSet<string>();                    
                    foreach (DataRow dr in rows)
                    {
                        string externalId = GetStringValueFromCell(dr, externalIdIndex);
                        if (!string.IsNullOrEmpty(externalId) && !externalIds.Contains(externalId))
                        {
                            externalIds.Add(externalId);
                            externalIdToMediaAssetMap.Add(externalId, new MediaAsset() { CoGuid = externalId });
                        }
                    }

                    if (externalIds.Count > 0)
                    {
                        DataTable existingExternalIdsDt = CatalogDAL.ValidateExternalIdsExist(groupId, externalIds.ToList());
                        if (existingExternalIdsDt != null && existingExternalIdsDt.Rows != null && existingExternalIdsDt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in existingExternalIdsDt.Rows)
                            {
                                string externalId = ODBCWrapper.Utils.GetSafeStr(dr, "CO_GUID");
                                long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                                if (!string.IsNullOrEmpty(externalId) && id > 0 && externalIdToMediaAssetMap.ContainsKey(externalId))
                                {
                                    GenericResponse<Asset> genRespone = AssetManager.GetAsset(groupId, id, ApiObjects.eAssetTypes.MEDIA, true);
                                    if (genRespone != null && IsValidStatus(genRespone.Status))
                                    {
                                        externalIdToMediaAssetMap[externalId] = genRespone.Object as MediaAsset;
                                    }                                        
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
            }

            return res;
        }

        private bool IsValidStatus(Status status, string externalId, ref Dictionary<string, Status> externalIdToResultMap)
        {
            bool res = true;
            if (!IsValidStatus(status))
            {
                res = false;
                externalIdToResultMap[externalId] = status;
            }

            return res;
        }

        private bool IsValidStatus(Status status)
        {
            bool res = true;
            if (status != null && status.Code != (int)eResponseStatus.OK)
            {
                res = false;                
            }

            return res;
        }

        private List<Image> CreateAssetImagesFromExcelData(DataRow dr, long id, Dictionary<int, ExcelDefinitionColumn> noneBasicColumnDefinitions)
        {
            List<Image> images = new List<Image>();
            try
            {
                Dictionary<long, Image> imageTypeIdToAssetImageMap = new Dictionary<long, Image>();
                foreach (KeyValuePair<int, ExcelDefinitionColumn> pair in noneBasicColumnDefinitions.Where(x => x.Value.Type == ApiObjects.ExcelColumnType.Image))
                {
                    ExcelImageDefinitionColumn imageDefColumn = pair.Value as ExcelImageDefinitionColumn;
                    if (imageDefColumn.SystemName == URL)
                    {
                        if (!imageTypeIdToAssetImageMap.ContainsKey(imageDefColumn.ImageTypeId))
                        {
                            Image image = new Image() { ImageObjectId = id, ImageTypeId = imageDefColumn.ImageTypeId, ImageObjectType = ApiObjects.eAssetImageType.Media };
                            imageTypeIdToAssetImageMap.Add(imageDefColumn.ImageTypeId, image);
                        }

                        imageTypeIdToAssetImageMap[imageDefColumn.ImageTypeId].Url = GetStringValueFromCell(dr, pair.Key);
                    }
                }

                images = imageTypeIdToAssetImageMap.Values.ToList();
            }
            catch (Exception ex)
            {
            }

            return images;
        }

        private List<AssetFile> CreateAssetFileFromExcelData(DataRow dr, long id, Dictionary<int, ExcelDefinitionColumn> noneBasicColumnDefinitions)
        {
            List<AssetFile> files = new List<AssetFile>();
            try
            {
                Dictionary<long, AssetFile> fileTypeIdToAssetFileMap = new Dictionary<long, AssetFile>();
                foreach (KeyValuePair<int, ExcelDefinitionColumn> pair in noneBasicColumnDefinitions.Where(x => x.Value.Type == ApiObjects.ExcelColumnType.File))
                {
                    ExcelFileDefinitionColumn fileDefColumn = pair.Value as ExcelFileDefinitionColumn;
                    if (ASSET_FILE_PROPERTIES.Contains(fileDefColumn.SystemName))
                    {
                        if (!fileTypeIdToAssetFileMap.ContainsKey(fileDefColumn.FileTypeId))
                        {
                            AssetFile assetFile = new AssetFile() { AssetId = id, TypeId = (int)fileDefColumn.FileTypeId };
                            fileTypeIdToAssetFileMap.Add(fileDefColumn.FileTypeId, assetFile);
                        }

                        switch (fileDefColumn.SystemName)
                        {
                            case URL:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].Url = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case DURATION:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].Duration = GetNullableLongValueFromCell(dr, pair.Key);
                                break;
                            case FILE_EXTERNAL_ID:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].ExternalId = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case FILE_ALT_EXTERNAL_ID:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].AltExternalId = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case FILE_EXTERNAL_STORE_ID:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].ExternalStoreId = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case CDN_ADAPTER_PROFILE_ID:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].CdnAdapaterProfileId = GetNullableLongValueFromCell(dr, pair.Key);
                                break;
                            case ALT_STREAMING_CODE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].AltStreamingCode = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case ALT_CDN_ADAPTER_PROFILE_ID:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].AlternativeCdnAdapaterProfileId = GetNullableLongValueFromCell(dr, pair.Key);
                                break;
                            case ADDITIONAL_DATA:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].AdditionalData = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case BILLING_TYPE:
                                long? billingType = GetNullableLongValueFromCell(dr, pair.Key);
                                if (billingType.HasValue)
                                {
                                    fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].BillingType = billingType.Value;
                                }

                                break;
                            case ORDER_NUM:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].OrderNum = GetNullableIntValueFromCell(dr, pair.Key);
                                break;
                            case LANGUAGE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].Language = GetStringValueFromCell(dr, pair.Key);
                                break;
                            case IS_DEFAULT_LANGUAGE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].IsDefaultLanguage = GetNullableBoolValueFromCell(dr, pair.Key);
                                break;
                            case OUTPUT_PROTECT_LVL:
                                int? outProtlvl = GetNullableIntValueFromCell(dr, pair.Key);
                                if (outProtlvl.HasValue)
                                {
                                    fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].OutputProtecationLevel = outProtlvl.Value;
                                }

                                break;
                            case FILE_START_DATE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].StartDate = GetNullableDateTimeValueFromCell(dr, pair.Key);
                                break;
                            case FILE_END_DATE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].EndDate = GetNullableDateTimeValueFromCell(dr, pair.Key);
                                break;
                            case FILE_SIZE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].FileSize = GetNullableLongValueFromCell(dr, pair.Key);
                                break;
                            case FILE_IS_ACTIVE:
                                fileTypeIdToAssetFileMap[fileDefColumn.FileTypeId].IsActive = GetNullableBoolValueFromCell(dr, pair.Key);
                                break;
                            default:
                                break;
                        }
                    }
                }

                files = fileTypeIdToAssetFileMap.Values.ToList();
            }
            catch (Exception ex)
            {
            }

            return files;
        }

        #endregion

    }
}
