using ApiObjects.BulkUpload;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace WebAPI.App_Start
{
    public class ExcelFormatter : BaseFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        protected JsonManager jsonManager;

        public ExcelFormatter() : base(KalturaResponseType.EXCEL, ExcelFormatterConsts.EXCEL_CONTENT_TYPE)
        {
            jsonManager = JsonManager.GetInstance();
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

            if (HttpContext.Current.Items[RequestContextUtils.REQUEST_METHOD_PARAMETERS] is IEnumerable)
            {
                List<object> requestMethodParameters = new List<object>(HttpContext.Current.Items[RequestContextUtils.REQUEST_METHOD_PARAMETERS] as IEnumerable<object>);
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

                // TODO SHIR - Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true instead.
                pack.SaveAs(writeStream);
            }
        }

        private static DataTable GetDataTableByObjects(int groupId, List<IKalturaExcelableObject> objects, Dictionary<string, ApiObjects.BulkUpload.ExcelColumn> columns)
        {
            DataTable dataTable = new DataTable();
            if (columns != null && columns.Count > 0)
            {
                var defaultType = typeof(string);
                foreach (var col in columns)
                {
                    dataTable.Columns.Add(col.Key, defaultType);
                }

                if (objects != null && objects.Count > 0)
                {
                    foreach (var excelObject in objects)
                    {
                        try
                        {
                            var excelValues = excelObject.GetExcelValues(groupId);
                            if (excelValues != null && excelValues.Count > 0)
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

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            if (value != null && value is StatusWrapper)
            {
                try
                {
                    // validate expected type was received
                    var restResultWrapper = value as StatusWrapper;

                    if (restResultWrapper != null && restResultWrapper.Result != null && !(restResultWrapper.Result is KalturaAPIExceptionWrapper))
                    {
                        if (!(restResultWrapper.Result is IKalturaExcelStructureManager))
                        {
                            throw new BadRequestException(BadRequestException.FORMAT_NOT_SUPPORTED, KalturaResponseType.EXCEL, (int)KalturaResponseType.EXCEL);
                        }

                        int? groupId;
                        string fileName;

                        if (!TryGetDataFromRequest(out groupId, out fileName))
                        {
                            throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS);
                        }

                        var kalturaExcelStructure = restResultWrapper.Result as IKalturaExcelStructureManager;
                        if (kalturaExcelStructure == null)
                        {
                            throw new ClientException((int)eResponseStatus.InvalidBulkUploadStructure, "Invalid BulkUpload Structure");
                        }

                        var excelStructure = kalturaExcelStructure.GetExcelStructure(groupId.Value);
                        if (excelStructure == null)
                        {
                            log.ErrorFormat("excelStructure is null for groupId:{0}, value:{1}", groupId.Value, JsonConvert.SerializeObject(value));
                            throw new ClientException((int)eResponseStatus.InvalidBulkUploadStructure, "Invalid BulkUpload Structure");
                        }

                        DataTable fullDataTable = null;
                        var excelableListResponse = restResultWrapper.Result as IKalturaExcelableListResponse;
                        if (excelableListResponse != null)
                        {
                            fullDataTable = GetDataTableByObjects(groupId.Value, excelableListResponse.GetObjects(), excelStructure.ExcelColumns);
                        }

                        HttpContext.Current.Response.ContentType = ExcelFormatterConsts.EXCEL_CONTENT_TYPE;
                        HttpContext.Current.Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);

                        CreateExcel(writeStream, fileName, fullDataTable, excelStructure);
                        return Task.FromResult(writeStream);
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

            var serializedValue = JsonConvert.SerializeObject(value);
            log.Debug($"ExcelFormatter.WriteToStreamAsync invalid value is: {serializedValue}.");

            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                string json = jsonManager.Serialize(value);
                streamWriter.Write(json);
                return Task.FromResult(writeStream);
            }
        }

        /// <summary>
        /// This method is used for the .net core version of phoenix and will serialize the object async
        /// </summary>
        public override Task<string> GetStringResponse(object obj)
        {
            // TODO: find a way to return and serialize 
            throw new NotImplementedException("Excel Format is not yet supported under .net core");
        }
    }
}