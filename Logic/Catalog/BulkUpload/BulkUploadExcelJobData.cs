using ApiObjects.BulkUpload;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Core.Catalog
{
    /// <summary>
    /// Instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadExcelJobData : BulkUploadJobData
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        public override GenericListResponse<BulkUploadResult> Deserialize(int groupId, long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<BulkUploadResult>();

            var structureManager = objectData.GetStructureManager() as IExcelStructureManager;
            if (structureManager == null)
            {
                response.SetStatus(eResponseStatus.InvalidBulkUploadStructure);
                return response;
            }

            var excelStructure = structureManager.GetExcelStructure(objectData.GroupId, objectData.GetObjectType());
            if (excelStructure == null)
            {
                response.SetStatus(eResponseStatus.InvalidBulkUploadStructure);
                return response;
            }

            var propertyNameAndValueResponse = MapToObjectsPropertyNameAndValue(fileUrl, excelStructure);
            if (!propertyNameAndValueResponse.IsOkStatusCode())
            {
                response.SetStatus(propertyNameAndValueResponse.Status);
                return response;
            }

            var mandatoryColumns = excelStructure.ExcelColumns.Where(x => x.Value.IsMandatory).ToList();
            for (int i = 0; i < propertyNameAndValueResponse.Object.Count; i++)
            {
                var propertyNameToValue = propertyNameAndValueResponse.Object[i];
                var bulkUploadObjectResponse = MappedObjectToBulkObject(structureManager, excelStructure, propertyNameToValue, objectData, mandatoryColumns);
                var bulkUploadResult = objectData.GetNewBulkUploadResult(bulkUploadId, bulkUploadObjectResponse.Item1, i, bulkUploadObjectResponse.Item2);
                response.Objects.Add(bulkUploadResult);
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }
        
        private GenericResponse<List<Dictionary<string, object>>> MapToObjectsPropertyNameAndValue(string fileUrl, ExcelStructure excelStructure)
        {
            var deserializeResponse = new GenericResponse<List<Dictionary<string, object>>>();
            try
            {
                int columnNameRowIndex = excelStructure.OverviewInstructions.Count > 0 ? excelStructure.OverviewInstructions.Count + 2 : 1;
                using (var webClient = new WebClient())
                {
                    byte[] fileBytes = webClient.DownloadData(fileUrl);
                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        deserializeResponse.SetStatus(eResponseStatus.FileDoesNotExists, string.Format("Could not find file:{0}", fileUrl));
                        return deserializeResponse;
                    }
                    
                    using (var fileStream = new MemoryStream(fileBytes))
                    {
                        using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                        {
                            foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                            {
                                for (int row = columnNameRowIndex + 1; row <= worksheet.Dimension.End.Row; row++)
                                {
                                    var columnNamesToValues = new Dictionary<string, object>();
                                    for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                                    {
                                        var column = worksheet.Cells[columnNameRowIndex, col].Value;
                                        if (column == null) { continue; }
                                        var columnName = column.ToString();

                                        if (!string.IsNullOrEmpty(columnName) && !columnNamesToValues.ContainsKey(columnName) && worksheet.Cells[row, col].Value != null && excelStructure.ExcelColumns.ContainsKey(columnName))
                                        {
                                            //add the cell data to the List
                                            columnNamesToValues.Add(columnName, worksheet.Cells[row, col].Value);
                                        }
                                    }

                                    deserializeResponse.Object.Add(columnNamesToValues);
                                }
                            }
                        }
                    }

                    deserializeResponse.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                deserializeResponse.SetStatus(eResponseStatus.IllegalExcelFile);
                log.Error($"An Exception was occurred in Deserialize Excel File. fileUrl:{fileUrl}.", ex);
            }

            return deserializeResponse;
        }
        
        private Tuple<IExcelObject, List<Status>> MappedObjectToBulkObject(IExcelStructureManager structureManager, ExcelStructure excelStructure, Dictionary<string, object> propertyNameToValue, BulkUploadObjectData objectData, List<KeyValuePair<string, ApiObjects.BulkUpload.ExcelColumn>> mandatoryColumns)
        {
            IExcelObject excelObject = null;
            var errors = new List<Status>();

            try
            {
                foreach (var mandatoryColumn in mandatoryColumns)
                {
                    if (!propertyNameToValue.ContainsKey(mandatoryColumn.Key))
                    {
                        errors.Add(new Status(eResponseStatus.ExcelMandatoryValueIsMissing, $"Mandatory Value:{mandatoryColumn.Key} Is Missing"));
                    }
                }

                excelObject = objectData.CreateObjectInstance() as IExcelObject;
                if (excelObject != null)
                {
                    var isObjectValid = true;
                    foreach (var mandatoryPropertyAndValue in excelStructure.MandatoryPropertyAndValueMap)
                    {
                        var key = mandatoryPropertyAndValue.Key;
                        var value = mandatoryPropertyAndValue.Value;
                        if (!propertyNameToValue.ContainsKey(key) || !propertyNameToValue[key].Equals(value))
                        {
                            isObjectValid = false;
                            errors.Add(new Status(eResponseStatus.IllegalExcelFile, $"Excel columns do not match for current BulkObjectType data. Mandatory column name to value: {{{key}={value}}}."));
                        }
                    }

                    if (isObjectValid)
                    {
                        try
                        {
                            excelObject.SetExcelValues(objectData.GroupId, propertyNameToValue, excelStructure.ExcelColumns, structureManager);
                        }
                        catch (ExcelParserException ex)
                        {
                            log.Error($"An ExcelParserException was occurred in SetExcelValues. groupId:{objectData.GroupId}, Target method:{ex.MethodName}, Column:{ex.ColumnName}, Value:{ex.Value}.", ex);
                            var status = new Status(eResponseStatus.InvalidArgumentValue, $"Invalid Argument Value. Error:{ex.Message}");
                            status.AddArg("Target method", ex.MethodName);
                            status.AddArg("Column", ex.ColumnName);
                            status.AddArg("Value", ex.Value);
                            errors.Add(status);
                        }
                        catch (ArgumentException ex)
                        {
                            log.Error($"An ArgumentException was occurred in SetExcelValues. groupId:{objectData.GroupId}, Parameter name:{ex.ParamName}.", ex);
                            var status = new Status(eResponseStatus.InvalidArgumentValue, $"Invalid Argument Value, Error:{ex.Message}");
                            status.AddArg("Parameter name", ex.ParamName);
                            if (ex.TargetSite != null)
                            {
                                status.AddArg("Target method", ex.TargetSite.Name);
                            }
                            errors.Add(status);
                        }
                        catch (Exception ex)
                        {
                            log.Error($"An Exception was occurred in SetExcelValues. groupId:{objectData.GroupId}.", ex);
                            var status = new Status(eResponseStatus.InvalidArgumentValue, $"Could not set Excel Values for this object, Error:{ex.Message}");
                            if (ex.TargetSite != null)
                            {
                                status.AddArg("Target method", ex.TargetSite.Name);
                            }
                            errors.Add(status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(new Status(eResponseStatus.IllegalExcelFile));
                log.Error($"An Exception was occurred in Deserialize Excel File. groupId:{objectData.GroupId}.", ex);
            }

            return new Tuple<IExcelObject, List<Status>>(excelObject, errors);
        }
    }
}