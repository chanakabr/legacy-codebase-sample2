using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoMapper;
using Core.Catalog;
using FeatureFlag;
using KalturaRequestContext;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Phx.Lib.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.App_Start
{
    public class ExcelFormatter : BaseFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly JsonManager _jsonManager;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;

        public ExcelFormatter() : base(KalturaResponseType.EXCEL, ExcelFormatterConsts.EXCEL_CONTENT_TYPE)
        {
            _jsonManager = JsonManager.GetInstance();
            _phoenixFeatureFlag = PhoenixFeatureFlagInstance.Get();
        }

        public override bool CanReadType(Type type)
        {
            if (type == (Type)null)
                throw new ArgumentNullException("type");
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        private static bool TryGetDataFromRequest(out int? groupId, out string fileName)
        {
            bool isResponseValid = false;
            groupId = null;
            fileName = null;
            groupId = Utils.Utils.GetGroupIdFromRequest();
            if (!groupId.HasValue || groupId.Value == 0)
            {
                log.ErrorFormat("no group id");
                throw new RequestParserException(RequestParserException.PARTNER_INVALID);
            }

            if (HttpContext.Current.Items[RequestContextConstants.REQUEST_METHOD_PARAMETERS] is IEnumerable)
            {
                List<object> requestMethodParameters = new List<object>(HttpContext.Current.Items[RequestContextConstants.REQUEST_METHOD_PARAMETERS] as IEnumerable<object>);
                var kalturaPersistedFilter = requestMethodParameters.FirstOrDefault(x => x is IKalturaPersistedFilter);
                if (kalturaPersistedFilter != null)
                {
                    fileName = (kalturaPersistedFilter as IKalturaPersistedFilter).Name;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "Filter.name");
                    }
                    if (!fileName.EndsWith(ExcelFormatterConsts.EXCEL_EXTENTION))
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "Filter.name");
                    }
                    isResponseValid = true;
                }
                else
                {
                    var id = requestMethodParameters.FirstOrDefault();
                    if (id != null)
                    {
                        fileName = id.ToString() + ExcelFormatterConsts.EXCEL_EXTENTION;
                        isResponseValid = true;
                    }
                }
            }
            return isResponseValid;
        }

        private static void CreateExcel(Stream writeStream, string fileName, DataTable dt, ExcelStructure excelStructure)
        {
            using (var pack = new ExcelPackage(new FileInfo(fileName)))
            {
                var excelWorksheet = pack.Workbook.Worksheets.Add(ExcelFormatterConsts.EXCEL_SHEET_NAME);
                int columnNameRowIndex = 1;
                // Set overview instructions
                if (excelStructure.OverviewInstructions.Count > 0)
                {
                    for (var i = 1; i <= excelStructure.OverviewInstructions.Count; i++)
                    {
                        excelWorksheet.Cells[i, 1].Value = excelStructure.OverviewInstructions[i - 1];
                    }
                    columnNameRowIndex = excelStructure.OverviewInstructions.Count + 2;
                }
                var columns = excelStructure.ExcelColumns.Values.ToList();
                for (var i = 1; i <= columns.Count; i++)
                {
                    // set the column name
                    excelWorksheet.Cells[columnNameRowIndex, i].Value = columns[i - 1].ToString();
                    if (!string.IsNullOrEmpty(columns[i - 1].HelpText))
                    {
                        excelWorksheet.Cells[columnNameRowIndex, i].AddComment(columns[i - 1].HelpText, "HelpText");
                    }
                    excelWorksheet.Cells[columnNameRowIndex, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorksheet.Cells[columnNameRowIndex, i].Style.Font.Bold = true;
                    if (excelStructure.ColumnsColors.ContainsKey(columns[i - 1].ColumnType))
                    {
                        excelWorksheet.Cells[columnNameRowIndex, i].Style.Fill.BackgroundColor.SetColor(excelStructure.ColumnsColors[columns[i - 1].ColumnType]);
                    }
                }
                if (dt != null)
                {
                    columnNameRowIndex++;
                    excelWorksheet.Cells[columnNameRowIndex, 1].LoadFromDataTable(dt, false);
                }
                // TODO - Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true instead.
                pack.SaveAs(writeStream);
            }
        }

        private static DataTable GetDataTableByObjects(int groupId, IEnumerable<object> objects, Dictionary<string, ApiObjects.BulkUpload.ExcelColumn> columns)
        {
            DataTable dataTable = new DataTable();
            if (columns != null && columns.Count > 0)
            {
                var defaultType = typeof(string);
                foreach (var col in columns)
                {
                    dataTable.Columns.Add(col.Key, defaultType);
                }

                if (objects != null)
                {
                    foreach (var excelObject in objects)
                    {
                        try
                        {
                            Dictionary<string, object> excelValues = null;
                            switch (excelObject)
                            {
                                case KalturaUdidDynamicList c:
                                    excelValues = GetExcelValuesFromUdidDynamicList(groupId, c);
                                    break;
                                case KalturaAsset c:
                                    excelValues = GetExcelValuesFromAsset(groupId, c);
                                    break;
                            }

                            if (excelValues != null && excelValues.Count > 0)
                            {
                                if (excelValues.Count == 1 && excelValues.First().Value is IEnumerable)
                                {
                                    var key = excelValues.First().Key;
                                    var values = (excelValues.First().Value as IEnumerable<object>)?.ToList();

                                    foreach (var _value in values)
                                    {
                                        AddRowByExcelValues(columns, dataTable, new Dictionary<string, object> { { key, _value} });
                                    }
                                }
                                else
                                {
                                    AddRowByExcelValues(columns, dataTable, excelValues);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Error in GetDataTableByObjects for object: {0}", excelObject), ex);
                        }
                    }
                }
            }
            return dataTable;
        }

        private static void AddRowByExcelValues(Dictionary<string, ApiObjects.BulkUpload.ExcelColumn> columns, DataTable dataTable, Dictionary<string, object> excelValues)
        {
            var row = dataTable.NewRow();
            foreach (var excelValue in excelValues)
            {
                if (dataTable.Columns.Contains(excelValue.Key))
                {
                    object value = null;
                    if (columns[excelValue.Key].Property.PropertyType.Equals(typeof(DateTime)) ||
                        columns[excelValue.Key].Property.PropertyType.Equals(typeof(DateTime?)))
                    {
                        value = (excelValue.Value as DateTime?).Value.ToString(DateUtils.MAIN_FORMAT);
                    }
                    else
                    {
                        value = excelValue.Value;
                    }
                    if (value != null)
                    {
                        row[excelValue.Key] = value;
                    }
                }
            }
            dataTable.Rows.Add(row);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, 
            HttpContent content, TransportContext transportContext)
        {
            if (value != null && value is StatusWrapper)
            {
                try
                {
                    // validate expected type was received
                    var restResultWrapper = value as StatusWrapper;
                    if (restResultWrapper != null && restResultWrapper.Result != null && !(restResultWrapper.Result is KalturaAPIExceptionWrapper))
                    {
                        int? groupId;
                        string fileName;
                        if (!TryGetDataFromRequest(out groupId, out fileName))
                        {
                            throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS);
                        }

                        ExcelStructure excelStructure = GetExcelStructureFromResult(value, restResultWrapper, groupId.Value);
                        DataTable fullDataTable = GetDataTableFromResult(groupId.Value, restResultWrapper.Result, excelStructure.ExcelColumns);
                        HttpContext.Current.Response.ContentType = ExcelFormatterConsts.EXCEL_CONTENT_TYPE;
                        HttpContext.Current.Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);
                        CreateExcel(writeStream, fileName, fullDataTable, excelStructure);
                        return Task.CompletedTask;
                    }
                }
                catch (ApiException ex)
                {
                    log.Error($"An ApiException was occurred in ExcelFormatter.WriteToStreamAsync. Details:{ex.ToString()}.");
                    throw ex;
                }
                catch (Exception ex)
                {
                    log.Error($"An Exception was occurred in ExcelFormatter.WriteToStreamAsync. Details:{ex.ToString()}.");
                    var apiException = new ApiException(ex, HttpStatusCode.InternalServerError);
                    throw apiException;
                }
            }

            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                if (_phoenixFeatureFlag.IsEfficientSerializationUsed())
                {
                    var jsonBuilder = _jsonManager.Serialize(value);
#if NETCOREAPP3_1
                    return streamWriter.WriteAsync(jsonBuilder);
#endif
#if NET48
                    return streamWriter.WriteAsync(jsonBuilder.ToString());
#endif
                }
                else
                {
                    string json = _jsonManager.ObsoleteSerialize(value);
                    return streamWriter.WriteAsync(json);
                }
            }
        }

        private ExcelStructure GetExcelStructureFromResult(object value, StatusWrapper restResultWrapper, int groupId)
        {
            ExcelStructure excelStructure = null;
            switch (restResultWrapper.Result)
            {
                case KalturaDynamicListListResponse c:
                    excelStructure = GetExcelStructureFromDynamicListListResponse(groupId, c);
                    break;
                case KalturaAssetListResponse c:
                    excelStructure = GetExcelStructureFromAssetListResponse(groupId, c);
                    break;
                case KalturaAssetStruct c:
                    excelStructure = GetExcelStructureFromAssetStruct(groupId, c);
                    break;
                default:
                    throw new BadRequestException(BadRequestException.FORMAT_NOT_SUPPORTED, KalturaResponseType.EXCEL, (int)KalturaResponseType.EXCEL);
            }

            if (excelStructure == null)
            {
                log.ErrorFormat("excelStructure is null for groupId:{0}, value:{1}", groupId, JsonConvert.SerializeObject(value));
                throw new ClientException((int)eResponseStatus.InvalidBulkUploadStructure, "Invalid BulkUpload Structure");
            }

            return excelStructure;
        }

        private ExcelStructure GetExcelStructureFromDynamicListListResponse(int groupId, KalturaDynamicListListResponse listResponse)
        {
            //Matan: Temp removal
            //var featureEnabled = FeatureFlag.PhoenixFeatureFlagInstance.Get().IsUdidDynamicListAsExcelEnabled(groupId);

            //if (!featureEnabled)
            //{
            //    throw new BadRequestException(BadRequestException.FORMAT_NOT_SUPPORTED, "Enable feature: [dynamicList.format]");
            //}

            var _objects = GetObjectsFromDynamicListListResponse(listResponse);
            if (_objects.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                                              $"KalturaDynamicList.id: {_objects[0].Id}",
                                              $"KalturaDynamicList.id: {_objects[1].Id}");
            }

            var _item = Mapper.Map<UdidDynamicList>(_objects.First());
            var excelStructure = _item.GetExcelStructure(groupId, _item.GetType());
            return excelStructure;
        }

        private ExcelStructure GetExcelStructureFromAssetListResponse(int groupId, KalturaAssetListResponse listResponse)
        {
            if (listResponse.Objects == null || listResponse.Objects.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaAssetListResponse.objects");
            }

            var duplicates = listResponse.Objects.GroupBy(x => x.getType()).Select(x => x.Key).ToList();
            if (duplicates.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                                              "KalturaAsset.type:" + duplicates[0].ToString(),
                                              "KalturaAsset.type:" + duplicates[1].ToString());
            }

            var kalturaAssetStruct = new KalturaAssetStruct()
            {
                Id = duplicates[0]
            };

            ExcelStructure excelStructer = GetExcelStructureFromAssetStruct(groupId, kalturaAssetStruct);
            return excelStructer;
        }

        private ExcelStructure GetExcelStructureFromAssetStruct(int groupId, KalturaAssetStruct assetStruct)
        {
            var mappedAssetStruct = Mapper.Map<AssetStruct>(assetStruct);

            return new AssetStructStructureManager(mappedAssetStruct).GetExcelStructure(groupId);
        }

        /// <summary>
        /// This method is used for the .net core version of phoenix and will serialize the object async
        /// </summary>
        public override Task<string> GetStringResponse(object obj)
        {
            // TODO: find a way to return and serialize 
            throw new NotImplementedException("Excel Format is not yet supported under .net core");
        }

        private DataTable GetDataTableFromResult(int groupId, object result, Dictionary<string, ApiObjects.BulkUpload.ExcelColumn> excelColumns)
        {
            DataTable dataTable = null;
            switch (result)
            {
                case KalturaDynamicListListResponse c: 
                    dataTable = GetDataTableByObjects(groupId, GetObjectsFromDynamicListListResponse(c), excelColumns);
                    break;
                case KalturaAssetListResponse c:
                    dataTable = GetDataTableByObjects(groupId, GetObjectsFromAssetListResponse(c), excelColumns);
                    break;
            }

            return dataTable;
        }

        private List<KalturaDynamicList> GetObjectsFromDynamicListListResponse(KalturaDynamicListListResponse listResponse)
        {
            if (listResponse.Objects == null || !listResponse.Objects.Any())
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaDynamicListListResponse.objects");
            }

            return listResponse.Objects;
        }

        private List<KalturaAsset> GetObjectsFromAssetListResponse(KalturaAssetListResponse listResponse)
        {
            return listResponse.Objects;
        }

        private static Dictionary<string, object> GetExcelValuesFromUdidDynamicList(int groupId, KalturaUdidDynamicList udidDynamicList)
        {
            //udid (column name): List<udid>
            var columnNameToUdids = DynamicListManager.Instance.GetUdidsFromDynamicListById(groupId, udidDynamicList.Id);
            return columnNameToUdids;
        }

        private static Dictionary<string, object> GetExcelValuesFromAsset(int groupId, KalturaAsset asset)
        {
            var excelableObject = Mapper.Map<Asset>(asset);
            Dictionary<string, object> excelValues  = excelableObject.GetExcelValues(groupId);
            return excelValues;
        }
    }
}