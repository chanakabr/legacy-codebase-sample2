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
using WebAPI.Models.Catalog;
using System.Linq;
using Core.Catalog.CatalogManagement;
using ApiObjects.Response;
using ApiObjects.Catalog;

namespace WebAPI.App_Start
{
    public class ExcelFormatter : MediaTypeFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ID = "id";
        private const string TYPE = "type";
        private const string NAME = "name";
        private const string DESCRIPTION = "description";
        private const string IMAGE = "image";
        private const string MEDIA_FILE = "mediaFile";
        private const string META = "meta";
        private const string TAG = "tag";
        private const string START_DATE = "startDate";
        private const string END_DATE = "endDate";
        private const string CREATE_DATE = "createDate";
        private const string UPDATE_DATE = "updateDate";
        private const string EXTERNAL_ID = "externalId";

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
            try
            {
                if (value != null)
                {
                    // validate expected type was received
                    StatusWrapper restResultWrapper = (StatusWrapper)value;

                    if (restResultWrapper != null && restResultWrapper.Result != null && (restResultWrapper.Result is KalturaAssetListResponse))
                    {
                        var result = restResultWrapper.Result as Models.Catalog.KalturaAssetListResponse;
                        // 1. Serialize value to xml
                        //Version currentVersion = OldStandardAttribute.getCurrentRequestVersion();

                        //string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + result.ToXml(currentVersion, true) + "</xml>";

                        // 2. Serialize xml to ds
                        //var ds = CreateDataSetFromXml(xml);
                        // System.Data.DataTable dt = CreateDataTableForAsset(xml);

                        // 3. Serialize ds to excel
                        //var excel = ConvertDatatableToExcel(dt);
                        //--------------
                        // 1. Serialize value to dt
                        //var genericList = KalturaOTTObject.buildList(type, result.Objects.ToArray());
                        //var dataTable = ConvertToDataTable(result.Objects);
                        //var dataTable = ConvertToDataTable(genericList);
                        // 2. Serialize dt to excel
                        //var excel1 = ConvertDatatableToExcel(dataTable);
                        //writeStream.Write(excel);

                        // 1.
                        var dt = ConvertToTabel(result.Objects, content);
                        var excel = ConvertDatatableToExcel(dt);

                        if (excel != null)
                        {
                            // TODO SHIR - ASK TANTAN ABOUT THIS
                            using (TextWriter streamWriter = new StreamWriter(writeStream))
                            {
                                streamWriter.Write(excel);
                                return Task.FromResult(writeStream);
                            }
                        }
                    }
                    else
                    {
                        // TODO SHIR - ASK TANTAN ABOUT THIS
                        using (TextWriter streamWriter = new StreamWriter(writeStream))
                        {
                            streamWriter.Write(JsonConvert.SerializeObject(restResultWrapper));
                            return Task.FromResult(writeStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while formatting object to Excel. object type:{0}, value:{1}", type.Name, value), ex);
            }

            return null;
        }

        private DataSet CreateDataSetFromXml(string xmlString)
        {
            DataSet ds = new DataSet();
            try
            {
                StringBuilder output = new StringBuilder();
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlString)))
                {
                    while (xmlReader.Read())
                    {
                        string colName;
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            colName = xmlReader.Name;
                        }


                        switch (xmlReader.NodeType)
                        {
                            case System.Xml.XmlNodeType.Element:
                                System.Data.DataTable dt;
                                if (ds.Tables.Contains(xmlReader.Name))
                                {
                                    dt = ds.Tables[xmlReader.Name];
                                }
                                else
                                {
                                    dt = new System.Data.DataTable(xmlReader.Name);
                                    ds.Tables.Add(dt);
                                }
                                
                                while (xmlReader.MoveToNextAttribute())
                                {
                                    dt.Columns.Add(xmlReader.Value);
                                }

                                if (xmlReader.Read())
                                {
                                    DataRow row = dt.NewRow();
                                    int i = 0;
                                    while (xmlReader.NodeType != XmlNodeType.EndElement)
                                    {
                                        //row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                                    }
                                    dt.Rows.Add();
                                }
                                
                                break;
                            case System.Xml.XmlNodeType.Text:
                                //xmlWriter.WriteString(xmlReader.Value);
                                break;
                            case System.Xml.XmlNodeType.Comment:
                                //xmlWriter.WriteComment(xmlReader.Value);
                                break;
                            case System.Xml.XmlNodeType.EndElement:
                                //xmlWriter.WriteFullEndElement();
                                break;
                            case System.Xml.XmlNodeType.XmlDeclaration:
                            case System.Xml.XmlNodeType.ProcessingInstruction:
                                //xmlWriter.WriteProcessingInstruction(xmlReader.Name, xmlReader.Value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while CreateDataTableFromXml. XmlFile:{0}", xmlString), ex);
            }

            return ds;
        }

        private System.Data.DataTable CreateDataTableForAsset(string xmlString)
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

        private System.Data.DataTable ConvertToDataTable(List<KalturaAsset> data)
        {
            System.Data.DataTable table = new System.Data.DataTable();

            if (data == null || data.Count == 0)
            {
                return table;
            }

            try
            {
                var types = new Dictionary<Type, Dictionary<ExcelPropertyAttribute, PropertyInfo>>();
                foreach (var asset in data)
                {
                    // add type attributes
                    var currType = asset.GetType();
                    if (!types.ContainsKey(currType))
                    {
                        types.Add(currType, new Dictionary<ExcelPropertyAttribute, PropertyInfo>());
                        var properties = currType.GetProperties();
                        foreach (var prop in properties)
                        {
                            var excelObjectPropertyAttribute = prop.GetCustomAttribute<ExcelObjectPropertyAttribute>();
                            if (excelObjectPropertyAttribute != null)
                            {
                                var innerProperties = prop.PropertyType.GetProperties();
                                ExcelKeyPropertyAttribute innerKey = null;
                                ExcelValuePropertyAttribute innerValue = null;
                                foreach (var innerProp in innerProperties)
                                {
                                    if (innerKey == null)
                                    {
                                        innerKey = innerProp.GetCustomAttribute<ExcelKeyPropertyAttribute>();
                                        if (innerKey != null)
                                        {
                                            continue;
                                        }
                                    }

                                    if (innerValue == null)
                                    {
                                        innerValue = innerProp.GetCustomAttribute<ExcelValuePropertyAttribute>();
                                    }

                                    if (innerKey != null && innerValue != null)
                                    {
                                        break;
                                    }
                                }

                                if (innerKey != null && innerValue != null)
                                {
                                    break;
                                }
                            }

                            var excelPropertyAttribute = prop.GetCustomAttribute<ExcelPropertyAttribute>();
                            if (excelPropertyAttribute != null)
                            {
                                if (types[currType].ContainsKey(excelPropertyAttribute))
                                {
                                    // TODO SHIR - THROW EXCEPTION FOR DUPLICATE COLUMN NAME
                                    //throw new BadRequestException(BadRequestException.DUPLICATE_PIN, name, DynamicMaxInt);
                                }

                                if (string.IsNullOrEmpty(excelPropertyAttribute.Name))
                                {
                                    // TODO SHIR - THROW EXCEPTION FOR empty COLUMN NAME
                                    //throw new BadRequestException(BadRequestException.DUPLICATE_PIN, name, DynamicMaxInt);
                                }

                                types[currType].Add(excelPropertyAttribute, prop);
                                table.Columns.Add(excelPropertyAttribute.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                            }

                            // names
                            if (prop.PropertyType.IsAutoLayout &&
                                prop.PropertyType.IsClass &&
                                !prop.PropertyType.IsLayoutSequential &&
                                !prop.PropertyType.IsSealed &&
                                !prop.PropertyType.IsValueType)
                            {
                                // excelAttribute.SystemName + "_";
                            }
                        }
                    }
                }

                // add asset to table
                DataRow row = table.NewRow();
                //foreach (var prop in types[currType])
                //{
                //    if (!prop.Key.IsKey)
                //    {
                //        row[prop.Key.SystemName] = prop.Value.GetValue(asset) ?? DBNull.Value;
                //    }

                //}
                table.Rows.Add(row);


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
                                // if need more columns
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

                    //DataRow row = table.NewRow();
                    //foreach (PropertyDescriptor prop in allTypes[currType])
                    //{
                    //    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    //}
                    //table.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while Convert objects list to datatable", ex);
            }

            return table;
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
                                // if need more columns
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
       
        private System.Data.DataTable ConvertToTabel(List<KalturaAsset> assets, System.Net.Http.HttpContent content)
        {
            int groupId = 0;
            IEnumerable<string> values = null;
            if (content.Headers.TryGetValues(KLogMonitor.Constants.GROUP_ID, out values) && values != null)
            {
                groupId = int.Parse(values.First());
            }

            if (groupId == 0)
            {
                return null;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling ConvertToMediaAssets", groupId);
                return null;
            }

            var dt = GenerateEmptyDataTable(groupId, "0");
            if (dt == null)
            {
                return null;
            }

            foreach (var asset in assets)
            {
                var row = dt.NewRow();
                row[ID] = asset.Id;
                row[TYPE] = asset.Type;
                row[START_DATE] = asset.StartDate;
                row[END_DATE] = asset.EndDate;
                row[CREATE_DATE] = asset.CreateDate;
                row[UPDATE_DATE] = asset.UpdateDate;
                row[EXTERNAL_ID] = asset.ExternalId;

                if (asset.Name != null && asset.Name.Values != null && asset.Name.Values.Count > 0)
                {
                    foreach (var name in asset.Name.Values)
                    {
                        row[NAME + "_" + name.Language] = name.Value;
                    }
                }

                if (asset.Description != null && asset.Description.Values != null && asset.Description.Values.Count > 0)
                {
                    foreach (var description in asset.Description.Values)
                    {
                        row[DESCRIPTION + "_" + description.Language] = description.Value;
                    }
                }

                if (asset.Images != null && asset.Images.Count > 0)
                {
                    foreach (var image in asset.Images)
                    {
                        row[IMAGE + "_" + image.Ratio] = image.Url;
                    }
                }

                //row[MEDIA_FILE ] = asset.MediaFiles;

                if (asset.Metas != null && asset.Metas.Count > 0)
                {
                    foreach (var meta in asset.Metas)
                    {
                        // TODO SHIR - CHECK THAT!!
                        row[META + "_" + meta.Key] = meta.Value;
                    }
                }

                if (asset.Tags != null && asset.Tags.Count > 0)
                {
                    foreach (var tag in asset.Tags)
                    {
                        if (tag.Value != null && tag.Value.Objects != null && tag.Value.Objects.Count > 0)
                        {
                            foreach (var tagValue in tag.Value.Objects)
                            {
                                if (tagValue.value != null && tagValue.value.Values != null && tagValue.value.Values.Count > 0)
                                {
                                    var defaultValue = tagValue.value.Values.FirstOrDefault(x => catalogGroupCache.DefaultLanguage.Code.Equals(x.Language));
                                    if (defaultValue != null)
                                    {
                                        foreach (var tagValueValue in tagValue.value.Values)
                                        {
                                            // TODO SHIR - CHECK THAT!!
                                            row[TAG + "_" + tag.Key + "_" + defaultValue.Value + "_" + tagValueValue.Language] = tagValueValue.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private System.Data.DataTable GenerateEmptyDataTable(int groupId, string mediaType)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("Failed to get catalogGroupCache for groupId: {0} when calling GenerateExcelTemplate", groupId);
                    return null;
                }

                if (catalogGroupCache.AssetStructsMapById.Values.Count(x => x.Name == mediaType) != 1)
                {
                    log.ErrorFormat("MediaType {0} doesn't exist for groupId: {1}", mediaType, groupId);
                    return null;
                }

                GenericListResponse<MediaFileType> groupFileTypes = FileManager.GetMediaFileTypes(groupId);
                if (!groupFileTypes.HasObjects())
                {
                    log.ErrorFormat("Failed to get group file types for groupId: {0}", groupId);
                    return null;
                }

                GenericListResponse<ImageType> groupImageTypes = ImageManager.GetImageTypes(groupId, true, null);
                if (!groupImageTypes.HasObjects())
                {
                    log.ErrorFormat("Failed to get group image types for groupId: {0}", groupId);
                    return null;
                }
                
                Dictionary<string, PropertyInfo> indexToAccountDescMap = new Dictionary<string, PropertyInfo>();
                
                dt.Columns.Add(ID);
                dt.Columns.Add(TYPE);
                dt.Columns.Add(START_DATE);
                dt.Columns.Add(END_DATE);
                dt.Columns.Add(CREATE_DATE);
                dt.Columns.Add(UPDATE_DATE);
                dt.Columns.Add(EXTERNAL_ID);

                foreach (var lang in catalogGroupCache.LanguageMapByCode)
                {
                    dt.Columns.Add(NAME + "_" + lang.Key);
                    dt.Columns.Add(DESCRIPTION + "_" + lang.Key);
                }

                foreach (var imageType in groupImageTypes.Objects)
                {
                    // TODO SHIR - CHECK THIS
                    dt.Columns.Add(IMAGE + "_" + imageType.SystemName);
                }

                // TODO SHIR - ADD MediaFiles TO EMPTY DT
                //row[MEDIA_FILE ] = asset.MediaFiles;
                

                AssetStruct assetStruct = catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(x => x.Name == mediaType);
                foreach (long topicId in assetStruct.MetaIds)
                {
                    Topic topic = catalogGroupCache.TopicsMapById.ContainsKey(topicId) ? catalogGroupCache.TopicsMapById[topicId] : null;
                    if (topic == null)
                    {
                        continue;
                    }

                    if (topic.Type == ApiObjects.MetaType.Tag)
                    {
                        //if (asset.Tags != null && asset.Tags.Count > 0)
                        //{
                        //    foreach (var tag in asset.Tags)
                        //    {
                        //        if (tag.Value != null && tag.Value.Objects != null && tag.Value.Objects.Count > 0)
                        //        {
                        //            foreach (var tagValue in tag.Value.Objects)
                        //            {
                        //                if (tagValue.value != null && tagValue.value.Values != null && tagValue.value.Values.Count > 0)
                        //                {
                        //                    var defaultValue = tagValue.value.Values.FirstOrDefault(x => catalogGroupCache.DefaultLanguage.Code.Equals(x.Language));
                        //                    if (defaultValue != null)
                        //                    {
                        //                        foreach (var tagValueValue in tagValue.value.Values)
                        //                        {
                        //                            // TODO SHIR - CHECK THAT!!
                        //                            row[TAG + "_" + tag.Key + "_" + defaultValue.Value + "_" + tagValueValue.Language] = tagValueValue.Value;
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        foreach (var lang in catalogGroupCache.LanguageMapByCode)
                        {
                            dt.Columns.Add(META + "_" + topic.SystemName + "_" + lang.Key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                log.Error(string.Format("Failed GenerateEmptyDataTable for groupId: {0}, mediaType: {1}", groupId, mediaType), ex);
            }

            return dt;
        }

        private Dictionary<string, string> GetImages(List<KalturaMediaImage> mediaImages)
        {
            Dictionary<string, string> images = new Dictionary<string, string>();
            if (mediaImages == null || mediaImages.Count == 0)
            {
                return images;
            }

            foreach (var image in mediaImages)
            {
                images.Add(image.Ratio, image.Url);
            }

            return images;
        }

        private Dictionary<string, string> GetLanguagePairs(KalturaMultilingualString multilingualString, ref HashSet<string> allLanguages)
        {
            Dictionary<string, string> languagePairs = new Dictionary<string, string>();
            if (multilingualString == null || multilingualString.Values == null || multilingualString.Values.Count == 0)
            {
                return languagePairs;
            }

            foreach (var pair in multilingualString.Values)
            {
                languagePairs.Add(pair.Language, pair.Value);
                if (!allLanguages.Contains(pair.Language))
                {
                    allLanguages.Add(pair.Language);
                }
            }

            return languagePairs;
        }
    }

    public class ExcelAsset
    {
        [ExcelProperty(Name = "id")]
        public long? Id { get; set; }

        [ExcelProperty(Name = "type")]
        public int? Type { get; set; }
        
        [ExcelProperty(Name = "name")]
        public Dictionary<string, string> Names { get; set; }

        [ExcelProperty(Name = "description")]
        public Dictionary<string, string> Descriptions { get; set; }
        
        [ExcelProperty(Name = "image")]
        public Dictionary<string, string> Images { get; set; }

        [ExcelProperty(Name = "mediaFile")]
        public Dictionary<string, string> MediaFiles { get; set; }

        [ExcelProperty(Name = "meta")]
        public Dictionary<string, Dictionary<string, KalturaValue>> Metas { get; set; }

        [ExcelProperty(Name = "tag")]
        public Dictionary<string, Dictionary<string, Dictionary<string, KalturaValue>>> Tags { get; set; }
        
        [ExcelProperty(Name = "startDate")]
        public long? StartDate { get; set; }

        [ExcelProperty(Name = "endDate")]
        public long? EndDate { get; set; }

        [ExcelProperty(Name = "createDate")]
        public long CreateDate { get; set; }

        [ExcelProperty(Name = "updateDate")]
        public long UpdateDate { get; set; }
        
        [ExcelProperty(Name = "externalId")]
        public string ExternalId { get; set; }
    }
}