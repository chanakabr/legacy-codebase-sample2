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
using ApiLogic;
using ApiObjects.BulkUpload;

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
            var deserializeResponse = new GenericListResponse<BulkUploadResult>();

            var structureManager = objectData.GetStructureManager() as IExcelStructureManager;
            if (structureManager == null)
            {
                deserializeResponse.SetStatus(eResponseStatus.InvalidBulkUploadStructure);
                return deserializeResponse;
            }

            var excelStructure = structureManager.GetExcelStructure(objectData.GroupId, objectData.GetObjectType());
            if (excelStructure == null)
            {
                deserializeResponse.SetStatus(eResponseStatus.InvalidBulkUploadStructure);
                return deserializeResponse;
            }

            var mappedObjectsResponse = DeserializeExcelFileToMapObjects(fileUrl, excelStructure, bulkUploadId);
            if (!mappedObjectsResponse.IsOkStatusCode())
            {
                deserializeResponse.SetStatus(mappedObjectsResponse.Status);
                return deserializeResponse;
            }

            var mandatoryColumns = excelStructure.ExcelColumns.Where(x => x.Value.IsMandatory).ToList();
            for (int i = 0; i < mappedObjectsResponse.Object.Count; i++)
            {
                var propertyNameToValue = mappedObjectsResponse.Object[i];
                var bulkUploadObjectResponse = MappedObjectToBulkObject(structureManager, excelStructure, propertyNameToValue, objectData, mandatoryColumns);
                var bulkUploadResult = objectData.GetNewBulkUploadResult(bulkUploadId, bulkUploadObjectResponse.Item1, i, bulkUploadObjectResponse.Item2);
                deserializeResponse.Objects.Add(bulkUploadResult);
            }

            deserializeResponse.SetStatus(eResponseStatus.OK);
            return deserializeResponse;
        }

        private GenericResponse<List<Dictionary<string, object>>> DeserializeExcelFileToMapObjects(string fileUrl, ExcelStructure excelStructure, long id)
        {
            var mappedObjectsResponse = new GenericResponse<List<Dictionary<string, object>>>();
            try
            {
                int columnNameRowIndex = excelStructure.OverviewInstructions.Count > 0 ? excelStructure.OverviewInstructions.Count + 2 : 1;
                log.Debug($"file url: {fileUrl}, id: {id}");
                var fileBytes = FileHandler.Instance.DownloadFile(id, fileUrl);
                if (fileBytes == null || fileBytes.Object == null || fileBytes.Object.Length == 0)
                {
                    mappedObjectsResponse.SetStatus(eResponseStatus.FileDoesNotExists, $"Could not find file:{fileUrl}");
                    return mappedObjectsResponse;
                }

                using (var fileStream = new MemoryStream(fileBytes.Object))
                {
                    log.Debug($"DeserializeExcelFileToMapObjects: parsing file: {fileUrl}, id: {id}");
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        var mappedObjects = new List<Dictionary<string, object>>();
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
                                    var propertyValue = worksheet.Cells[row, col].Value;

                                    if (!string.IsNullOrEmpty(columnName) && !columnNamesToValues.ContainsKey(columnName) && propertyValue != null && excelStructure.ExcelColumns.ContainsKey(columnName))
                                    {
                                        //add the cell data to the List
                                        columnNamesToValues.Add(columnName, propertyValue);
                                    }
                                }

                                mappedObjects.Add(columnNamesToValues);
                            }
                        }

                        mappedObjectsResponse.Object = mappedObjects;
                    }
                }

                mappedObjectsResponse.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                mappedObjectsResponse.SetStatus(eResponseStatus.IllegalExcelFile);
                log.Error($"An Exception was occurred in DeserializeExcelFileToMapObjects. fileUrl:{fileUrl}.", ex);
            }

            return mappedObjectsResponse;
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
                log.Error($"An Exception was occurred in MappedObjectToBulkObject.", ex);
            }

            return new Tuple<IExcelObject, List<Status>>(excelObject, errors);
        }
    }
}