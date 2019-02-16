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

namespace WebAPI.App_Start
{
    public class ExcelFormatter : MediaTypeFormatter
    {
        // TODO SHIR - NEED THAT??
        private static readonly string excelTemplateDir = string.Format("{0}ExcelTemplates\\", AppDomain.CurrentDomain.BaseDirectory);

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string EXCEL_CONTENT_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string OTHER_LANG_FORMAT = "{0} ({1})";
        internal const string DATE_FORMAT = "dd/MM/yyyy hh:mm:ss";

        // col headers
        private const string COLUMN_TYPE = "t";
        private const string COLUMN_SYSTEM_NAME = "n";
        private const string COLUMN_LANGUAGE = "l";
        private const string ITEM_INDEX = "i";
        
        public ExcelFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(EXCEL_CONTENT_TYPE));
            MediaTypeMappings.Add(new QueryStringMapping("format", "31", EXCEL_CONTENT_TYPE));
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

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content,
                                                System.Net.TransportContext transportContext)
        {
            try
            {
                if (value != null && value is StatusWrapper)
                {
                    // validate expected type was received
                    StatusWrapper restResultWrapper = value as StatusWrapper;

                    if (restResultWrapper != null && restResultWrapper.Result != null && (restResultWrapper.Result is KalturaListResponse))
                    {
                        var listResponse = restResultWrapper.Result as KalturaAssetListResponse;
                        int? groupId;
                        string fileName;

                        if (TryGetDataFromRequest(listResponse, out groupId, out fileName))
                        {
                            var excelColumns = listResponse.GetExcelColumns(groupId.Value);
                            if (excelColumns != null)
                            {
                                // TODO SHIR - TALK WITH TAN TAN ABOUT THE OBJECTS..
                                DataTable fullDataTable = GetDataTableByObjects(groupId.Value, listResponse.Objects, excelColumns);

                                if (fullDataTable != null)
                                {
                                    var sheetName = "shir Sheet";
                                    
                                    HttpContext.Current.Response.ContentType = EXCEL_CONTENT_TYPE;
                                    HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);

                                    return CreateExcel(writeStream, fileName, fullDataTable, sheetName, listResponse.GetExcelOverviewInstructions(), excelColumns.Values.ToList(), listResponse.GetExcelColumnsColors());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while formatting object to Excel. object type:{0}, value:{1}", type.Name, value), ex);
                ErrorUtils.HandleWSException(ex);
            }

            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                HttpContext.Current.Response.ContentType = "application/json";
                streamWriter.Write(JsonConvert.SerializeObject(value));
                return Task.FromResult(writeStream);
            }
        }

        public static string GetHiddenColumn(ExcelColumnType columnType, string systemName, string language = null, int? itemIndex = null)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { COLUMN_TYPE, columnType.ToString() },
                { COLUMN_SYSTEM_NAME, systemName }
            };

            if (!string.IsNullOrEmpty(language))
            {
                dic.Add(COLUMN_LANGUAGE, language);
            }

            if (itemIndex.HasValue)
            {
                dic.Add(ITEM_INDEX, itemIndex.Value.ToString());
            }

            return dic.ToJSON();
        }
        
        public static string GetNonDefaultFriendlyColumnName(string columnName, string langCode)
        {
            return String.Format(OTHER_LANG_FORMAT, columnName, langCode);
        }

        private bool TryGetDataFromRequest(KalturaListResponse assetListResponse, out int? groupId, out string fileName)
        {
            bool isResponseValid = false;
            groupId = null;
            fileName = null;

            if (assetListResponse != null)
            {
                groupId = Utils.Utils.GetGroupIdFromRequest();
                if (!groupId.HasValue || groupId.Value == 0)
                {
                    log.ErrorFormat("no group id");
                    throw new RequestParserException(RequestParserException.PARTNER_INVALID);
                }


                if(HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS] is IEnumerable)
                {
                    List<object> requestMethodParameters = new List<object>(HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS] as IEnumerable<object>);
                    var kalturaPersistedFilter = requestMethodParameters.FirstOrDefault(x => x is IKalturaPersistedFilter);
                    if (kalturaPersistedFilter != null)
                    {
                        fileName = (kalturaPersistedFilter as IKalturaPersistedFilter).Name;
                        // TODO SHIR - VALIDATE FILE NAME ENDS WITH .xlsx
                    }
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Guid.NewGuid().ToString();
                }

                isResponseValid = true;
            }

            return isResponseValid;
        }

        private Task CreateExcel(Stream writeStream, string fileName, DataTable dt, string sheetName,  List<string> overviewInstructions, List<KalturaExcelColumn> columns, Dictionary<ExcelColumnType, Color> columnsColors)
        {
            using (ExcelPackage pack = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet excelWorksheet = pack.Workbook.Worksheets.Add(sheetName);

                int hiddenRowIndex = 1;
                // Set overview instructions
                if (overviewInstructions != null && overviewInstructions.Count > 0)
                {
                    for (int i = 1; i <= overviewInstructions.Count; i++)
                    {
                        excelWorksheet.Cells[i, 1].Value = overviewInstructions[i - 1];
                    }

                    hiddenRowIndex = overviewInstructions.Count + 2;
                }
                
                excelWorksheet.Row(hiddenRowIndex).Hidden = true;
                for (int i = 1; i <= columns.Count; i++)
                {
                    excelWorksheet.Cells[hiddenRowIndex, i].Value = columns[i -1].HiddenName;
                    excelWorksheet.Cells[hiddenRowIndex + 1, i].Value = columns[i - 1].HelpText;
                    excelWorksheet.Cells[hiddenRowIndex + 2, i].Value = columns[i - 1].FriendlyName;
                    if (columnsColors != null && columnsColors.ContainsKey(columns[i - 1].ColumnType))
                    {
                        excelWorksheet.Cells[hiddenRowIndex + 2, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        excelWorksheet.Cells[hiddenRowIndex + 2, i].Style.Fill.BackgroundColor.SetColor(columnsColors[columns[i - 1].ColumnType]);
                    }
                    excelWorksheet.Cells[hiddenRowIndex + 2, i].Style.Font.Bold = true;
                }

                excelWorksheet.Cells[hiddenRowIndex + 3, 1].LoadFromDataTable(dt, false, OfficeOpenXml.Table.TableStyles.Medium13);
                
                pack.SaveAs(writeStream);

                return Task.FromResult(writeStream);
            }
        }

        private DataTable GetDataTableByObjects(int groupId, List<KalturaAsset> objects, Dictionary<string, KalturaExcelColumn> columns)
        {
            DataTable dataTable = new DataTable();
            if (columns != null && columns.Count > 0)
            {
                foreach (var col in columns)
                {
                    dataTable.Columns.Add(col.Key);
                }

                if (objects != null && objects.Count > 0)
                {
                    foreach (var OTTObject in objects)
                    {
                        try
                        {
                            var excelValues = OTTObject.GetExcelValues(groupId);
                            if (excelValues != null && excelValues.Count > 0)
                            {
                                var row = dataTable.NewRow();
                                foreach (var excelValue in excelValues)
                                {
                                    if (dataTable.Columns.Contains(excelValue.Key))
                                    {
                                        row[excelValue.Key] = excelValue.Value;
                                    }
                                }
                                dataTable.Rows.Add(row);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Error in GetDataTableByObjects for object: {0}", OTTObject), ex);
                        }
                    }
                }
            }

            return dataTable;
        } 
    }
}