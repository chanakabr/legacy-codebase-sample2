using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using APILogic.Catalog.BulkUpload;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Epg;
using ApiObjects.Response;
using Core.Api;
using KLogMonitor;
using Newtonsoft.Json;
using Tvinci.Core.DAL;

namespace APILogic.BulkUpload
{
    /// <summary>
    /// Instructions for ingest of custom data file
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadIngestJobData : BulkUploadJobData
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public int IngestProfileId { get; set; }

        public override GenericListResponse<BulkUploadResult> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<BulkUploadResult>();
            var profile = api.GetIngestProfileById(IngestProfileId)?.Object;
            var xmlTvString = GetXmlTv(fileUrl, profile);

            if (string.IsNullOrEmpty(xmlTvString))
            {
                response.SetStatus(eResponseStatus.FileDoesNotExists, $"Could not find file:[{fileUrl}]");
                return response;
            }

            var epgData = DeserializeXmlTvEpgData(xmlTvString);
            response.Objects = epgData;

            return response;
        }

        private static string GetXmlTv(string fileUrl, IngestProfile profile)
        {
            string xmlTvString = null;
            if (!string.IsNullOrEmpty(profile?.TransformationAdapterUrl))
            {
                _Logger.Debug($"Found TransformationAdapterUrl:[{profile?.TransformationAdapterUrl}] calling adapter to transform file");
                var transformationAdptr = new IngestTransformationAdapterClient(profile);
                xmlTvString = transformationAdptr.Transform(fileUrl);
            }
            else
            {
                _Logger.Debug($"Transformation Adapter Url is not defined, assuming file is xmlTV format, downloading and parsing file.");
                xmlTvString = TryDownloadFileAsString(fileUrl, xmlTvString);
            }

            return xmlTvString;
        }

        private static string TryDownloadFileAsString(string fileUrl, string xmlTvString)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    xmlTvString = webClient.DownloadString(fileUrl);
                }
                catch (Exception e)
                {
                    _Logger.Error($"Error while downloading file to ingets, fileUrl:[{fileUrl}]", e);

                }
            }

            return xmlTvString;
        }

        private List<BulkUploadResult> DeserializeXmlTvEpgData(string Data)
        {
            EpgChannels xmlTvEpgData = null;
            try
            {
                var ser = new XmlSerializer(typeof(EpgChannels));
                var settings = new XmlReaderSettings();
                using (var textReader = new StringReader(Data))
                {
                    using (var xmlReader = XmlReader.Create(textReader, settings))
                    {
                        xmlTvEpgData = (EpgChannels)ser.Deserialize(xmlReader);
                    }
                }

                _Logger.Debug($"DeserializeEpgChannel > Successfully  Deserialize xml. got epgchannels.programme.Length:[{xmlTvEpgData.programme.Length}]");
                // TODO: Arthur, Should we use this or the group id came with the builk request ?
                var groupId = xmlTvEpgData.groupid;
                var parentGroupId = xmlTvEpgData.parentgroupid;
                var epgPrograms = MapXmlTvProgramToCBEpgProgram(parentGroupId, groupId, xmlTvEpgData);
                return epgPrograms;
            }
            catch (Exception ex)
            {
                _Logger.Error("DeserializeEpgChannel > error while trying to Deserialize.", ex);
                throw;
            }
        }

        private List<BulkUploadResult> MapXmlTvProgramToCBEpgProgram(int parentGroupId, int groupId, EpgChannels xmlTvEpgData)
        {
            var response = new List<BulkUploadResult>();

            var fieldEntityMapping = EpgIngest.Utils.GetMappingFields(parentGroupId);
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();
            _Logger.Debug($"MapXmlTvProgramToCBEpgProgram > Retriving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = EpgDal.GetAllEpgChannelObjectsList(groupId, channelExternalIds);
            var languages = GroupsCacheManager.GroupsCache.Instance().GetGroup(groupId).GetLangauges();
            var defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (defaultLanguage == null)
            {
                // TODO: Arthur, Check with ophir if we should fail the ingest in this case or just use first language or something else
                throw new Exception($"No main language defined for group:[{groupId}], ingest failed");
            }

            foreach (var prog in xmlTvEpgData.programme)
            {
                // Every channel external id can point to mulitple interbal channels that have to have the same EPG
                // like channel per region or HD channel vs SD channel etc..
                var channelsToIngestProgramInto = kalturaChannels.Where(c => c.ChannelExternalId == prog.channel);

                foreach (var channel in channelsToIngestProgramInto)
                {
                    foreach (var lang in languages)
                    {
                        var newEpgAssetResult = ParseXmlTvProgramToEpgCBObj(parentGroupId, groupId, channel.ChannelId, prog, lang.Code, defaultLanguage.Code, fieldEntityMapping, out var parsingStatus);
                        newEpgItem.SetStatus(parsingStatus);
                        response.Add(newEpgItem);
                    }

                }
            }

            return response;
        }

        private BulkUploadEpgAssetResult ParseXmlTvProgramToEpgCBObj(int parentGroupId, int groupId, int channelId, programme prog, string langCode, string defaultLangCode, List<FieldTypeEntity> fieldMappings, out eResponseStatus parsingStatus)
        {
            var response = new BulkUploadEpgAssetResult();
            var epgItem = new EpgCB();
            
            parsingStatus = eResponseStatus.OK;
            epgItem.Language = langCode;
            epgItem.ChannelID = channelId;
            epgItem.GroupID = groupId;
            epgItem.ParentGroupID = parentGroupId;
            epgItem.EpgIdentifier = prog.external_id;

            if (ParseXmlTvDateString(prog.start, out var progStartDate)) { epgItem.StartDate = progStartDate; }
            else { parsingStatus = eResponseStatus.EPGSProgramDatesError; }

            if (ParseXmlTvDateString(prog.stop, out var progEndDate)) { epgItem.EndDate = progEndDate; }
            else { parsingStatus = eResponseStatus.EPGSProgramDatesError; }

            epgItem.UpdateDate = DateTime.UtcNow;
            epgItem.CreateDate = DateTime.UtcNow;
            epgItem.IsActive = true;
            epgItem.Status = 1;
            epgItem.EnableCatchUp = ParseXmlTvEnableStatusValue(prog.enablecatchup);
            epgItem.EnableCDVR = ParseXmlTvEnableStatusValue(prog.enablecdvr);
            epgItem.EnableStartOver = ParseXmlTvEnableStatusValue(prog.enablestartover);
            epgItem.EnableTrickPlay = ParseXmlTvEnableStatusValue(prog.enabletrickplay);
            epgItem.Crid = prog.crid;

            epgItem.Name = GetTitleByLanguage(prog.title, langCode, defaultLangCode, out var nameParsingStatus);
            if (nameParsingStatus != eResponseStatus.OK) { _Logger.Error($"GetTitleByLanguage > could not find a match for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]"); }

            epgItem.Description = GetDescriptionByLanguage(prog.desc, langCode, defaultLangCode, out var descriptionParsingStatus);
            if (descriptionParsingStatus != eResponseStatus.OK) { _Logger.Error($"GetDescriptionByLanguage > could not find a match for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]"); }


            epgItem.Metas = new Dictionary<string, List<string>>();
            foreach (var meta in prog.metas)
            {
                // TODO: Cehck with ira, shouldnt meat be a single value and tags be multiple value per type key ? 
                var mataValues = GetMetaByLanguage(meta, langCode, defaultLangCode, fieldMappings, out var metasParsingStatus);
                if (metasParsingStatus != eResponseStatus.OK) { _Logger.Error($"GetMetasByLanguage > could not find a match for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]"); }
                epgItem.Metas[meta.MetaType] = mataValues;

            }

            response.Object = epgItem;
            return response;
        }

        private List<string> GetMetaByLanguage(metas meta, string language, string defaultLanguage, List<FieldTypeEntity> fieldMappings, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;
            var response = new Dictionary<string, List<string>>();
            var metaMapping = fieldMappings.FirstOrDefault(x => x.FieldType == FieldTypes.Meta && x.Name.ToLower() == meta.MetaType);

            var valuesByLang = meta.MetaValues.Where(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase));
            valuesByLang = valuesByLang ?? meta.MetaValues.Where(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (valuesByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
            }

            var valuesStrByLang = valuesByLang.Select(v => v.Value).ToList();
            if (!string.IsNullOrEmpty(metaMapping?.RegexExpression))
            {
                var metaRgxValidator = new Regex(metaMapping.RegexExpression);
                var metaValidationResult = valuesStrByLang
                    .GroupBy(metaRgxValidator.IsMatch)
                    .ToDictionary(k=>k.Key, v=>v.AsEnumerable());
                if (metaValidationResult.TryGetValue(false, out var invalidMetaValues))
                {
                    // TODO: add parsing errors regarding some metas that could not be parsed
                    _Logger.Warn($"GetMetaByLanguage > following metas failed parsing:[{string.Join(",",invalidMetaValues)}] ");
                }

            }

            return valuesStrByLang;
        }

        private string GetDescriptionByLanguage(desc[] desc, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;
            var valueByLang = desc.FirstOrDefault(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase))?.Value;
            valueByLang = valueByLang ?? desc.FirstOrDefault(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))?.Value;
            if (valueByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
            }

            return valueByLang;
        }

        private string GetTitleByLanguage(title[] title, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;
            var valueByLang = title.FirstOrDefault(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase))?.Value;
            valueByLang = valueByLang ?? title.FirstOrDefault(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))?.Value;
            if (valueByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
            }

            return valueByLang;
        }

        private static bool ParseXmlTvDateString(string dateStr, out DateTime theDate)
        {
            theDate = default(DateTime);
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14) { return false; }
            string format = "yyyyMMddHHmmss";
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        private int ParseXmlTvEnableStatusValue(string val)
        {
            try
            {
                if (string.IsNullOrEmpty(val))
                    return 0; // 0 == none
                if (val == "false" || val == "2")
                    return 2;
                if (val == "true" || val == "1")
                    return 1;
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
