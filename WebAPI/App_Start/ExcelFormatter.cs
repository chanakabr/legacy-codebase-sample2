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
using Microsoft.Office.Interop.Excel;

namespace WebAPI.App_Start
{
    public class ExcelFormatter : MediaTypeFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ExcelFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            MediaTypeMappings.Add(new QueryStringMapping("format", "31", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
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
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (value != null)
                    {
                        // validate expected type was received
                        StatusWrapper restResultWrapper = (StatusWrapper)value;
                        
                        if (restResultWrapper != null && restResultWrapper.Result != null && (restResultWrapper.Result is KalturaListResponse))
                        {
                            // 1. Serialize value to xml
                            Version currentVersion = OldStandardAttribute.getCurrentRequestVersion();
                            var result = restResultWrapper.Result as Models.Catalog.KalturaAssetListResponse;
                            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + result.ToXml(currentVersion, true) + "</xml>";

                            // 2. Serialize xml to ds
                            System.Data.DataTable dt = CreateDataTableFromXml(xml);

                            // 3. Serialize ds to excel
                            var excel = ConvertDatatableToExcel(dt);

                            // 1. Serialize value to dt
                            //var genericList = KalturaOTTObject.buildList(type, result.Objects.ToArray());
                            //var dataTable = ConvertToDataTable(result.Objects);
                            //var dataTable = ConvertToDataTable(genericList);
                            // 2. Serialize dt to excel
                            //var excel1 = ConvertDatatableToExcel(dataTable);

                            //writeStream.Write(excel);

                            // TODO SHIR - ASK TANTAN ABOUT THIS
                            using (TextWriter streamWriter = new StreamWriter(writeStream))
                            {
                                streamWriter.Write(excel);
                            }
                        }
                        else
                        {
                            // TODO SHIR - ASK TANTAN ABOUT THIS
                            using (TextWriter streamWriter = new StreamWriter(writeStream))
                            {
                                streamWriter.Write(JsonConvert.SerializeObject(restResultWrapper));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error while formatting object to Excel. object type:{0}, value:{1}", type.Name, value), ex);
                }
            });
        }

        private System.Data.DataTable CreateDataTableFromXml(string xmlString)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {
                StringBuilder output  = new StringBuilder();
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlString)))
                {
                    XmlWriterSettings ws = new XmlWriterSettings();
                    ws.Indent = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create(output, ws))
                    {
                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                case System.Xml.XmlNodeType.Element:
                                    xmlWriter.WriteStartElement(xmlReader.Name);
                                    break;
                                case System.Xml.XmlNodeType.Text:
                                    xmlWriter.WriteString(xmlReader.Value);
                                    break;
                                case System.Xml.XmlNodeType.Comment:
                                    xmlWriter.WriteComment(xmlReader.Value);
                                    break;
                                case System.Xml.XmlNodeType.EndElement:
                                    xmlWriter.WriteFullEndElement();
                                    break;
                                case System.Xml.XmlNodeType.XmlDeclaration:
                                case System.Xml.XmlNodeType.ProcessingInstruction:
                                    xmlWriter.WriteProcessingInstruction(xmlReader.Name, xmlReader.Value);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlReader);
                    dt.Load(ds.CreateDataReader());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while CreateDataTableFromXml. XmlFile:{0}", xmlString), ex);
            }

            return dt;
        }

        private System.Data.DataTable ConvertToDataTable<T>(IList<T> data)
        {
            System.Data.DataTable table = new System.Data.DataTable();

            try
            {
                // TODO SHIR - GET FROM DATAMODEL
                Dictionary<Type, PropertyDescriptorCollection> allTypes = new Dictionary<Type, PropertyDescriptorCollection>();
                HashSet<PropertyDescriptor> allProperties = new HashSet<PropertyDescriptor>();
                foreach (var item in data)
                {
                    var currType = item.GetType();
                    if (!allTypes.ContainsKey(currType))
                    {
                        PropertyDescriptorCollection currProperties = TypeDescriptor.GetProperties(currType);
                        allTypes.Add(currType, currProperties);
                        foreach (PropertyDescriptor prop in currProperties)
                        {
                            if (!allProperties.Contains(prop))
                            {
                                // TODO SHIR - GET FROM DATAMODEL
                                // if nned more columns
                                if (prop.PropertyType.IsAutoLayout && 
                                    prop.PropertyType.IsClass && 
                                    !prop.PropertyType.IsLayoutSequential &&
                                    !prop.PropertyType.IsSealed &&
                                    !prop.PropertyType.IsValueType)
                                {

                                }
                                else
                                {
                                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                    allProperties.Add(prop);
                                }
                            }
                        }
                    }

                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in allTypes[currType])
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                    table.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while Convert objects list to datatable", ex);
            }

            return table;
        }

        public Application ConvertDatatableToExcel(System.Data.DataTable dataTable)
        {
            if (dataTable == null || dataTable.Columns.Count == 0)
                throw new Exception("ExportToExcel: Null or empty input table!\n");

            // Start Excel and get Application object.
            var excelApp = new Application();
            
            // for making Excel visible
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            excelApp.Worksheets.Add(dataTable, "shir name");

            // Creation a new Workbook
            var excelworkBook = excelApp.Workbooks.Add(Type.Missing);

            // single Worksheet
            var excelSheet = (Worksheet)excelworkBook.ActiveSheet;
            excelSheet.Name = "Test work sheet";
            
            // column headings
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                excelSheet.Cells[1, i + 1] = dataTable.Columns[i].ColumnName;
            }

            // rows
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                // TODO SHIR - format datetime values before printing
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    excelSheet.Cells[i + 2, j + 1] = dataTable.Rows[i][j];
                }
            }

            // Working with range and formatting Excel cells - resize the columns
            var excelCellrange = excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[dataTable.Rows.Count, dataTable.Columns.Count]];
            excelCellrange.EntireColumn.AutoFit();
            Borders border = excelCellrange.Borders;
            border.LineStyle = XlLineStyle.xlContinuous;
            border.Weight = 2d;

            return excelApp;
        }
    }
}