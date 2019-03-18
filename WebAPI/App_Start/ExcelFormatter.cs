using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using WebAPI.Models.General;
using System.Text;
using WebAPI.Managers.Models;
using WebAPI.Exceptions;
using System.Web.Http;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using KLogMonitor;
using System.Reflection;
using System.Data;
using System.ComponentModel;
using WebAPI.Models.Catalog;
using System.Linq;
using Core.Catalog.CatalogManagement;
using ApiObjects.Response;
using ApiObjects.Catalog;
using TVinciShared;
using ApiObjects;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using WebAPI.Utils;
using System.Drawing;
using WebAPI.Models.API;
using ApiObjects.BulkUpload;
using WebAPI.Managers;
using System.Net;

namespace WebAPI.App_Start
{
    public interface IKalturaBulkUploadObject
    {
    }

    public interface IKalturaExcelableObject : IKalturaBulkUploadObject
    {
        Dictionary<string, object> GetExcelValues(int groupId);
    }

    public interface IKalturaBulkUploadStructure
    {
    }

    public interface IKalturaExcelStructure : IKalturaBulkUploadStructure
    {
        ExcelStructure GetExcelStructure(int groupId);
    }

    public interface IKalturaExcelableListResponse : IKalturaExcelStructure
    {
        List<IKalturaExcelableObject> GetObjects();
    }

    public class ExcelFormatter : BaseFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string EXCEL_CONTENT_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string EXCEL_SHEET_NAME = "OPC Batch Template";
        public const string EXCEL_EXTENTION = ".xlsx";

        public ExcelFormatter() : base(KalturaResponseType.EXCEL, EXCEL_CONTENT_TYPE)
        {
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

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            if (value != null && value is StatusWrapper)
            {
                try
                {
                    // validate expected type was received
                    StatusWrapper restResultWrapper = value as StatusWrapper;
                    if (restResultWrapper != null && restResultWrapper.Result != null && restResultWrapper.Result is IKalturaExcelStructure)
                    {
                        int? groupId;
                        string fileName;

                        if (TryGetDataFromRequest(out groupId, out fileName))
                        {
                            var kalturaExcelStructure = restResultWrapper.Result as IKalturaExcelStructure;
                            if (kalturaExcelStructure != null)
                            {
                                var excelStructure = kalturaExcelStructure.GetExcelStructure(groupId.Value);
                                if (excelStructure != null)
                                {
                                    DataTable fullDataTable = null;
                                    var excelableListResponse = restResultWrapper.Result as IKalturaExcelableListResponse;
                                    if (excelableListResponse != null)
                                    {
                                        fullDataTable = GetDataTableByObjects(groupId.Value, excelableListResponse.GetObjects(), excelStructure.ExcelColumns);
                                    }

                                    HttpContext.Current.Response.ContentType = EXCEL_CONTENT_TYPE;
                                    HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                                    return CreateExcel(writeStream, fileName, fullDataTable, excelStructure);
                                }
                                else
                                {
                                    log.DebugFormat("excelStructure is null for groupId:{0}, value:{1}", groupId.Value, JsonConvert.SerializeObject(value));
                                }
                            }
                        }
                        else
                        {
                            throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS);
                        }
                    }
                }
                catch(ApiException ex)
                {
                    value = CreateStatusWrapper(ex, value);
                }
                catch (Exception ex)
                {
                    var apiException = new ApiException(ex, HttpStatusCode.InternalServerError);
                    value = CreateStatusWrapper(apiException, value);
                }
            }

            if (HttpContext.Current.Response.StatusCode == (int)HttpStatusCode.OK)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                HttpContext.Current.Response.ContentType = "application/json";
                streamWriter.Write(JsonConvert.SerializeObject(value));
                return Task.FromResult(writeStream);
            }
        }

        private StatusWrapper CreateStatusWrapper(ApiException ex, object value)
        {
            var subCode = ex.Code;
            var message = ex.Message;
            var exceptionWrapper = WrappingHandler.prepareExceptionResponse(ex.Code, ex.Message, ex.Args);

            var exceptionContentValue = (ex.Response.Content as System.Net.Http.ObjectContent).Value;
            if (exceptionContentValue is ApiException.ExceptionPayload && (exceptionContentValue as ApiException.ExceptionPayload).code != 0)
            {
                var payload = exceptionContentValue as WebAPI.Exceptions.ApiException.ExceptionPayload;
                subCode = payload.code;
                message = WrappingHandler.HandleError(payload.error.ExceptionMessage, payload.error.StackTrace);
                exceptionWrapper = WrappingHandler.prepareExceptionResponse(payload.code, message, payload.arguments);
                if (payload.failureHttpCode != System.Net.HttpStatusCode.OK && payload.failureHttpCode != 0)
                {
                    HttpContext.Current.Response.StatusCode = (int)payload.failureHttpCode;
                    HttpContext.Current.Response.Headers.Add("X-Kaltura-App", string.Format("exiting on error {0} - {1}", payload.code, message));
                    HttpContext.Current.Response.Headers.Add("X-Kaltura", string.Format("error-{0}", payload.code));
                }
            }

            var oldStatusWrapper = value as StatusWrapper;
            var statusWrapper = new StatusWrapper(subCode, Guid.Empty, oldStatusWrapper.ExecutionTime, exceptionWrapper, message);
            return statusWrapper;
        }

        private bool TryGetDataFromRequest(out int? groupId, out string fileName)
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

            if (HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS] is IEnumerable)
            {
                List<object> requestMethodParameters = new List<object>(HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS] as IEnumerable<object>);
                var kalturaPersistedFilter = requestMethodParameters.FirstOrDefault(x => x is IKalturaPersistedFilter);
                if (kalturaPersistedFilter != null)
                {
                    fileName = (kalturaPersistedFilter as IKalturaPersistedFilter).Name;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "Filter.name");
                    }

                    if (!fileName.EndsWith(EXCEL_EXTENTION))
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
                        fileName = id.ToString() + EXCEL_EXTENTION;
                        isResponseValid = true;
                    }
                }
            }
            
            return isResponseValid;
        }

        private Task CreateExcel(Stream writeStream, string fileName, DataTable dt, ExcelStructure excelStructure)
        {
            using (ExcelPackage pack = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet excelWorksheet = pack.Workbook.Worksheets.Add(EXCEL_SHEET_NAME);

                int columnNameRowIndex = 1;
                // Set overview instructions
                if (excelStructure.OverviewInstructions.Count > 0)
                {
                    for (int i = 1; i <= excelStructure.OverviewInstructions.Count; i++)
                    {
                        excelWorksheet.Cells[i, 1].Value = excelStructure.OverviewInstructions[i - 1];
                    }

                    columnNameRowIndex = excelStructure.OverviewInstructions.Count + 2;
                }

                var columns = excelStructure.ExcelColumns.Values.ToList();
                for (int i = 1; i <= columns.Count; i++)
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
                    excelWorksheet.Cells[columnNameRowIndex + 1, 1].LoadFromDataTable(dt, false, OfficeOpenXml.Table.TableStyles.Medium13);
                }
                
                pack.SaveAs(writeStream);

                return Task.FromResult(writeStream);
            }
        }

        private DataTable GetDataTableByObjects(int groupId, List<IKalturaExcelableObject> objects, Dictionary<string, ApiObjects.BulkUpload.ExcelColumn> columns)
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
                                            value = (excelValue.Value as DateTime?).Value.ToString(ExcelManager.DATE_FORMAT);
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
    }
}