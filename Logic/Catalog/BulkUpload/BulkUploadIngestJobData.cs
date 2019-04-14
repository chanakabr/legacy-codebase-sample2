using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Epg;
using ApiObjects.Response;
using Core.GroupManagers;
using Core.Profiles;
using KLogMonitor;
using Newtonsoft.Json;
using Tvinci.Core.DAL;

namespace Core.Catalog
{
    /// <summary>
    /// Instructions for ingest of custom data file
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadIngestJobData : BulkUploadJobData
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string XML_TV_DATE_FORMAT = "yyyyMMddHHmmss";
        private static readonly XmlSerializer _XmltTVserilizer = new XmlSerializer(typeof(EpgChannels));
        public int IngestProfileId { get; set; }

        public override GenericListResponse<BulkUploadResult> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<BulkUploadResult>();
            var profile = IngestProfileManager.GetIngestProfileById(IngestProfileId)?.Object;
            var xmlTvString = GetXmlTv(fileUrl, profile);

            if (string.IsNullOrEmpty(xmlTvString))
            {
                response.SetStatus(eResponseStatus.FileDoesNotExists, $"Could not find file:[{fileUrl}]");
                return response;
            }

            var epgData = DeserializeXmlTvEpgData(bulkUploadId, xmlTvString);
            response.Objects = epgData;
            response.SetStatus(eResponseStatus.OK);
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

        private List<BulkUploadResult> DeserializeXmlTvEpgData(long bulkUploadId, string Data)
        {
            EpgChannels xmlTvEpgData = null;
            try
            {
                using (var textReader = new StringReader(Data))
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    xmlTvEpgData = (EpgChannels)_XmltTVserilizer.Deserialize(xmlReader);
                }

                _Logger.Debug($"DeserializeEpgChannel > Successfully  Deserialize xml. got epgchannels.programme.Length:[{xmlTvEpgData.programme.Length}]");
                // TODO: Arthur, Should we use this or the group id came with the builk request ?
                var groupId = xmlTvEpgData.groupid;
                var parentGroupId = xmlTvEpgData.parentgroupid;
                var epgPrograms = MapXmlTvProgramToCBEpgProgram(bulkUploadId, parentGroupId, groupId, xmlTvEpgData);
                return epgPrograms;
            }
            catch (Exception ex)
            {
                _Logger.Error("DeserializeEpgChannel > error while trying to Deserialize.", ex);
                throw;
            }
        }

        private List<BulkUploadResult> MapXmlTvProgramToCBEpgProgram(long bulkUploadId, int parentGroupId, int groupId, EpgChannels xmlTvEpgData)
        {

            //var fieldEntityMapping = EpgIngest.Utils.GetMappingFields(parentGroupId);
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();
            _Logger.Debug($"MapXmlTvProgramToCBEpgProgram > Retriving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = EpgDal.GetAllEpgChannelObjectsList(groupId, channelExternalIds);
            var languages = GroupLanguageManager.GetGroupLanguages(groupId);
            var defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            var itemIndex = 0;
            if (defaultLanguage == null)
            {
                throw new Exception($"No main language defined for group:[{groupId}], ingest failed");
            }

            var xmlTvDictionary = new Dictionary<string, BulkUploadXmlTvChannelResult>();

            foreach (var prog in xmlTvEpgData.programme)
            {
                // Every channel external id can point to mulitple interbal channels that have to have the same EPG
                // like channel per region or HD channel vs SD channel etc..
                var channelsToIngestProgramInto = kalturaChannels.Where(c => c.ChannelExternalId.Equals(prog.channel, StringComparison.OrdinalIgnoreCase));
                if (!xmlTvDictionary.ContainsKey(prog.channel))
                {
                    xmlTvDictionary[prog.channel] = new BulkUploadXmlTvChannelResult(bulkUploadId, prog.channel);
                    xmlTvDictionary[prog.channel].Status = BulkUploadResultStatus.InProgress;
                }

                var innerChannelList = ParseInnerChannels(parentGroupId, groupId, languages, defaultLanguage, prog, channelsToIngestProgramInto);
                xmlTvDictionary[prog.channel].InnerChannels = innerChannelList.ToArray();
            }

            var response = new List<BulkUploadResult>();
            foreach (var xmlTvChannelResult in xmlTvDictionary.Values)
            {
                response.Add(xmlTvChannelResult);
            }


            return response;
        }

        private List<BulkUploadChannelResult> ParseInnerChannels(int parentGroupId, int groupId, List<LanguageObj> languages, LanguageObj defaultLanguage, programme prog, IEnumerable<EpgChannelObj> channelsToIngestProgramInto)
        {
            var innerChannelList = new List<BulkUploadChannelResult>();
            foreach (var channel in channelsToIngestProgramInto)
            {
                var innerChannelResult = new BulkUploadChannelResult(channel.ChannelId);
                innerChannelResult.Status = BulkUploadResultStatus.InProgress;
                var multilengualPrgorams = new List<BulkUploadMultilingualProgramAssetResult>();
                foreach (var lang in languages)
                {
                    var newEpgAssetResult = ParseXmlTvProgramToEpgCBObj(parentGroupId, groupId, channel.ChannelId, prog, lang.Code, defaultLanguage.Code);
                    multilengualPrgorams.Add(new BulkUploadMultilingualProgramAssetResult(lang.Code, newEpgAssetResult));
                }

                innerChannelResult.Programs = multilengualPrgorams.ToArray();
                innerChannelList.Add(innerChannelResult);
            }

            return innerChannelList;
        }

        private BulkUploadProgramAssetResult ParseXmlTvProgramToEpgCBObj(int parentGroupId, int groupId, int channelId, programme prog, string langCode, string defaultLangCode)
        {
            // TODO: Arthur\ sunny make this code pretty, break into methods .. looks too long. :\
            var response = new BulkUploadProgramAssetResult();
            response.Status = BulkUploadResultStatus.InProgress;
            var epgItem = new EpgCB();

            epgItem.Language = langCode;
            epgItem.ChannelID = channelId;
            epgItem.GroupID = groupId;
            epgItem.ParentGroupID = parentGroupId;
            epgItem.EpgIdentifier = prog.external_id;

            if (ParseXmlTvDateString(prog.start, out var progStartDate)) { epgItem.StartDate = progStartDate; }
            else
            {
                response.AddError(eResponseStatus.EPGSProgramDatesError,
                    $"programExternalId:[{prog.external_id}], Start date:[{prog.start}] could not be parsed expected format:[{XML_TV_DATE_FORMAT}]");
            }

            if (ParseXmlTvDateString(prog.stop, out var progEndDate)) { epgItem.EndDate = progEndDate; }
            else
            {
                response.AddError(eResponseStatus.EPGSProgramDatesError,
                    $"programExternalId:[{prog.external_id}], End date:[{prog.start}] could not be parsed expected format:[{XML_TV_DATE_FORMAT}]");
            }

            epgItem.UpdateDate = DateTime.UtcNow;
            epgItem.CreateDate = DateTime.UtcNow;
            epgItem.IsActive = true;
            epgItem.Status = 1;
            epgItem.EnableCatchUp = ParseXmlTvEnableStatusValue(prog.enablecatchup);
            epgItem.EnableCDVR = ParseXmlTvEnableStatusValue(prog.enablecdvr);
            epgItem.EnableStartOver = ParseXmlTvEnableStatusValue(prog.enablestartover);
            epgItem.EnableTrickPlay = ParseXmlTvEnableStatusValue(prog.enabletrickplay);
            epgItem.Crid = prog.crid;
            epgItem.pictures = prog.icon.Select(p => new EpgPicture
            {
                Url = p.src,
                Ratio = p.ratio,
                PicWidth = p.width,
                PicHeight = p.height,
                ImageTypeId = 0, // TODO: Atthur\sunny look at the code in ws_ingest ingets.cs line#338
            }).ToList();

            epgItem.Name = GetTitleByLanguage(prog.title, langCode, defaultLangCode, out var nameParsingStatus);
            if (nameParsingStatus != eResponseStatus.OK)
            {
                response.AddError(nameParsingStatus, $"Error parsing title for programExternalId:[{prog.external_id}], langCode:[{langCode}], defaultLang:[{defaultLangCode}]");
            }

            epgItem.Description = GetDescriptionByLanguage(prog.desc, langCode, defaultLangCode, out var descriptionParsingStatus);
            if (descriptionParsingStatus != eResponseStatus.OK)
            {
                response.AddError(nameParsingStatus, $"Error parsing description for programExternalId:[{prog.external_id}], langCode:[{langCode}], defaultLang:[{defaultLangCode}]");
            }

            epgItem.Metas = ParseMetas(prog, langCode, defaultLangCode, response);
            epgItem.Tags = ParseTags(prog, langCode, defaultLangCode, response);
            response.ProgramExternalId = epgItem.EpgIdentifier;
            response.Object = epgItem;
            return response;
        }

        private Dictionary<string, List<string>> ParseTags(programme prog, string langCode, string defaultLangCode, BulkUploadProgramAssetResult response)
        {
            var tagsToSet = new Dictionary<string, List<string>>();
            foreach (var tag in prog.tags)
            {
                var tagValue = GetMetaByLanguage(tag, langCode, defaultLangCode, out var tagParsingStatus);
                if (tagParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(tagParsingStatus, $"Error parsing meta:[{tag.TagType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                tagsToSet[tag.TagType] = tagValue;
            }

            return tagsToSet;
        }

        private Dictionary<string, List<string>> ParseMetas(programme prog, string langCode, string defaultLangCode, BulkUploadProgramAssetResult response)
        {
            var metasToSet = new Dictionary<string, List<string>>();
            foreach (var meta in prog.metas)
            {
                var mataValue = GetMetaByLanguage(meta, langCode, defaultLangCode, out var metaParsingStatus);
                if (metaParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(metaParsingStatus, $"Error parsing meta:[{meta.MetaType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                metasToSet[meta.MetaType] = mataValue;
            }

            return metasToSet;
        }

        private List<string> GetMetaByLanguage(metas meta, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;

            var valuesByLang = meta.MetaValues.Where(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase));
            valuesByLang = valuesByLang ?? meta.MetaValues.Where(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (valuesByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
                return new List<string>();
            }

            var valuesStrByLang = valuesByLang.Select(v => v.Value).ToList();
            return valuesStrByLang;
        }

        private List<string> GetMetaByLanguage(tags tags, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;

            var valuesByLang = tags.TagValues.Where(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase));
            valuesByLang = valuesByLang ?? tags.TagValues.Where(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (valuesByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
                return new List<string>();
            }

            var valuesStrByLang = valuesByLang.Select(v => v.Value).ToList();
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
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), XML_TV_DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out theDate);
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
