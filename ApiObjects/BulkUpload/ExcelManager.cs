using ApiObjects.Response;
using KLogMonitor;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace ApiObjects.BulkUpload
{
    public class ExcelManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // formats
        public const string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
        private const string SAPARATOR = ":";

        // col headers
        private const string COLUMN_TYPE = "t";
        private const string COLUMN_SYSTEM_NAME = "n";
        private const string COLUMN_LANGUAGE = "l";
        private const string ITEM_INDEX = "i";

        public static GenericListResponse<Tuple<Status, IBulkUploadObject>> Deserialize(int groupId, string fileUrl, BulkUploadObjectData objectData)
        {
            var excelObjects = new GenericListResponse<Tuple<Status, IBulkUploadObject>>();
            try
            {
                using (var webClient = new WebClient())
                {
                    byte[] fileBytes = webClient.DownloadData(fileUrl);
                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        excelObjects.SetStatus(eResponseStatus.FileDoesNotExists, string.Format("Could not find file:{0}", fileUrl));
                        return excelObjects;
                    }
                    
                    using (var fileStream = new MemoryStream(fileBytes))
                    {
                        using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                        {
                            foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                            {
                                IExcelStructure structure = objectData.GetStructure() as IExcelStructure;
                                if (structure != null)
                                {
                                    var excelStructure = structure.GetExcelStructure(groupId);
                                    if (excelStructure == null)
                                    {
                                        excelObjects.SetStatus(eResponseStatus.InvalidBulkUploadStructure);
                                        return excelObjects;
                                    }
                                    else
                                    {
                                        int columnNameRowIndex = excelStructure.OverviewInstructions.Count > 0 ? excelStructure.OverviewInstructions.Count + 2 : 1;
                                        
                                        for (int row = columnNameRowIndex + 1; row <= worksheet.Dimension.End.Row; row++)
                                        {
                                            var columnNamesToValues = new Dictionary<string, object>();
                                            var status = new Status((int)eResponseStatus.OK);
                                            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                                            {
                                                var column = worksheet.Cells[columnNameRowIndex, col].Value;
                                                if (column != null &&
                                                    !string.IsNullOrEmpty(column.ToString()) &&
                                                    !columnNamesToValues.ContainsKey(column.ToString()) &&
                                                    worksheet.Cells[row, col].Value != null)
                                                {
                                                    //add the cell data to the List
                                                    columnNamesToValues.Add(column.ToString(), worksheet.Cells[row, col].Value);
                                                }
                                                else if (excelStructure.ExcelColumns.ContainsKey(column.ToString()) &&
                                                         excelStructure.ExcelColumns[column.ToString()].IsMandatory)
                                                {
                                                    status.Set(eResponseStatus.ExcelMandatoryValueIsMissing, string.Format("Mandatory Value:{0} Is Missing", column));
                                                    break;
                                                }
                                            }

                                            var excelObject = objectData.CreateObjectInstance() as IExcelObject;
                                            if (excelObject != null)
                                            {
                                                if (status.IsOkStatusCode())
                                                {
                                                    if (objectData.Validate(columnNamesToValues))
                                                    {
                                                        try
                                                        {
                                                            excelObject.SetExcelValues(groupId, columnNamesToValues, excelStructure.ExcelColumns);
                                                        }
                                                        catch (ExcelParserException ex)
                                                        {
                                                            log.Error(string.Format("An ExcelParserException was occurred in SetExcelValues. groupId:{0}, columnNamesToValues:{1}.",
                                                                                    groupId, string.Join(",", columnNamesToValues), ex));
                                                            status.Set(eResponseStatus.InvalidArgumentValue, string.Format("Invalid Argument Value. Error:{0}", ex.Message));
                                                            status.AddArg("Target method", ex.MethodName);
                                                            status.AddArg("Column", ex.ColumnName);
                                                            status.AddArg("Value", ex.Value);
                                                        }
                                                        catch (ArgumentException ex)
                                                        {
                                                            log.Error(string.Format("An ArgumentException was occurred in SetExcelValues. groupId:{0}, columnNamesToValues:{1}.",
                                                                                    groupId, string.Join(",", columnNamesToValues), ex));
                                                            status.Set(eResponseStatus.InvalidArgumentValue, string.Format("Invalid Argument Value, Error:{0}", ex.Message));
                                                            status.AddArg("Parameter name", ex.ParamName);
                                                            if (ex.TargetSite != null)
                                                            {
                                                                status.AddArg("Target method", ex.TargetSite.Name);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error(string.Format("An Exception was occurred in SetExcelValues. groupId:{0}, columnNamesToValues:{1}.",
                                                                                    groupId, string.Join(",", columnNamesToValues), ex));
                                                            status.Set(eResponseStatus.InvalidArgumentValue, string.Format("Could not set Excel Values for this object, Error:{0}", ex.Message));
                                                            if (ex.TargetSite != null)
                                                            {
                                                                status.AddArg("Target method", ex.TargetSite.Name);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var mandatoryPropertyToValueMap = objectData.GetMandatoryPropertyToValueMap();
                                                        var mandatoryPropertyToValue =  "{" + string.Join(",", mandatoryPropertyToValueMap.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
                                                        status.Set(eResponseStatus.IllegalExcelFile, string.Format("Excel columns do not match for current BulkObjectType data. Mandatory column name to value: {0}", mandatoryPropertyToValue));
                                                    }
                                                }

                                                excelObjects.Objects.Add(new Tuple<Status, IBulkUploadObject>(status, excelObject));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    excelObjects.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                excelObjects.SetStatus(eResponseStatus.IllegalExcelFile);
                log.Error(string.Format("An Exception was occurred in Deserialize Excel File. groupId:{0}, fileUrl:{1}.",
                                        groupId, fileUrl), ex);
            }

            return excelObjects;
        }

        public static Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>> GetSystemNameToProperyData(Type type)
        {
            var systemNameToProperty = new Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>>();
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var customAttribut = prop.GetCustomAttributes(true).FirstOrDefault(x => x is ExcelColumnAttribute);
                var excelAttribute = customAttribut as ExcelColumnAttribute;
                if (excelAttribute != null && !systemNameToProperty.ContainsKey(excelAttribute.SystemName))
                {
                    systemNameToProperty.Add(excelAttribute.SystemName, new Tuple<ExcelColumnAttribute, PropertyInfo>(excelAttribute, prop));
                }
            }

            return systemNameToProperty;
        }

        public static ApiObjects.BulkUpload.ExcelColumn GetExcelColumnByAttribute(Tuple<ExcelColumnAttribute, PropertyInfo> excelProperty, string systemName, string language = null, string helpText = null, string innerSystemName = null)
        {
            var excelColumn = new ApiObjects.BulkUpload.ExcelColumn(excelProperty.Item1.ColumnType, systemName)
            {
                Property = excelProperty.Item2,
                IsMandatory = excelProperty.Item1.IsMandatory,
                IsUniqueMeta = excelProperty.Item1.IsUniqueMeta,
                Language = language,
                HelpText = helpText,
                InnerSystemName = innerSystemName
            };

            return excelColumn;
        }
    }
}