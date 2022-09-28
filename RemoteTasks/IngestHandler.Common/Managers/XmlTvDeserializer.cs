using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using ApiLogic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using IngestHandler.Common;
using IngestHandler.Common.Repositories;
using Microsoft.Extensions.Logging;
using Tvinci.Core.DAL;

namespace IngestHandler.Common.Managers
{
    public interface IXmlTvDeserializer
    {
        GenericListResponse<BulkUploadResult> DeserializeXmlTv(int partnerId, long bulkUploadId, int? ingestProfileId, string fileUrl);
    }

    public class XmlTvDeserializer : IXmlTvDeserializer
    {
        private static readonly XmlSerializer _xmlTvSerializer = new XmlSerializer(typeof(EpgChannels));
        
        private readonly IIngestProfileRepository _ingestProfileRepository;
        private readonly IEpgDal _epgDal;
        private readonly IChannelRepository _channelRepository;
        private readonly ILogger<XmlTvDeserializer> _logger;

        public XmlTvDeserializer(
            IIngestProfileRepository ingestProfileRepository,
            IEpgDal epgDal,
            IChannelRepository channelRepository,
            ILogger<XmlTvDeserializer> logger)
        {
            _ingestProfileRepository = ingestProfileRepository;
            _epgDal = epgDal;
            _channelRepository = channelRepository;
            _logger = logger;
        
        }
        
        public GenericListResponse<BulkUploadResult> DeserializeXmlTv(int partnerId, long bulkUploadId, int? ingestProfileId, string fileUrl)
        {
            _ = BulkUploadMethods.GetGroupLanguages(partnerId, out var defaultLanguage);
            // before start set the results list to an empty list to avoid null ref errors
            var result = new GenericListResponse<BulkUploadResult>();
            result.SetStatus(Status.Ok);
            
            var ingestProfile = _ingestProfileRepository.GetIngestProfile(partnerId, ingestProfileId);
            var xmlTvString = GetXmlTv(fileUrl, ingestProfile);

            try
            {
                if (string.IsNullOrEmpty(xmlTvString))
                {
                    result.SetStatus(eResponseStatus.FileDoesNotExists, $"Could not find file:[{fileUrl}]");
                    return result;
                }

                var xmlTvEpgData = DeserializeXmlTvEpgData(bulkUploadId, xmlTvString);
                var epgBulkUploadResults = MapXmlTvEpgDataToBulkUploadResults(bulkUploadId, xmlTvEpgData);

                foreach (var r in epgBulkUploadResults)
                {
                    var epgObject = r.Object as EpgProgramBulkUploadObject;
                    
                    ProgramValidator.Validate(epgObject, r, defaultLanguage);
                }

                if (epgBulkUploadResults.Any())
                {
                    result.Objects = epgBulkUploadResults.Cast<BulkUploadResult>().ToList();
                }

                if (epgBulkUploadResults.Any(r => r.Errors?.Any() == true))
                {
                    result.SetStatus(eResponseStatus.Error, "Errors found during deserialization, review errors on result items.");
                    return result;
                }

                
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error during Epg ingest Deserialize");
                result.SetStatus(eResponseStatus.Error, $"Unexpected error during Epg ingest Deserialize, ex:[{e.Message}]");
                return result;
            }
        }
        
        private string GetXmlTv(string fileUrl, IngestProfile profile)
        {
            string xmlTvString;
            if (!string.IsNullOrEmpty(profile?.TransformationAdapterUrl))
            {
                _logger.LogDebug($"Found TransformationAdapterUrl:[{profile.TransformationAdapterUrl}] calling adapter to transform file");
                var transformationAdapter = new IngestTransformationAdapterClient(profile);
                xmlTvString = transformationAdapter.Transform(fileUrl);
            }
            else
            {
                _logger.LogDebug($"Transformation Adapter Url is not defined, assuming file is xmlTV format, downloading and parsing file.");
                xmlTvString = TryDownloadFileAsString(fileUrl);
            }

            return xmlTvString;
        }
        
        private string TryDownloadFileAsString(string fileUrl)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var xmlTvString = webClient.DownloadString(fileUrl);
                    return xmlTvString;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while downloading file to ingest, fileUrl:[{fileUrl}]");
                    return null;
                }
            }
        }
        
        private EpgChannels DeserializeXmlTvEpgData(long bulkUploadId, string data)
        {
            EpgChannels xmlTvEpgData;
            try
            {
                using (var textReader = new StringReader(data))
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    xmlTvEpgData = (EpgChannels)_xmlTvSerializer.Deserialize(xmlReader);
                }

                _logger.LogDebug($"DeserializeEpgChannel > Successfully  Deserialize xml. got epgchannels.programme.Length:[{xmlTvEpgData.programme.Length}]");
                return xmlTvEpgData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeserializeEpgChannel > error while trying to Deserialize.");
                throw;
            }
        }

        private List<BulkUploadProgramAssetResult> MapXmlTvEpgDataToBulkUploadResults(long bulkUploadId, EpgChannels xmlTvEpgData)
        {
            var groupId = xmlTvEpgData.groupid;
            var parentGroupId = xmlTvEpgData.parentgroupid;
            //var fieldEntityMapping = EpgIngest.Utils.GetMappingFields(parentGroupId);
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();

            _logger.LogDebug($"GetBulkUploadResults > Retrieving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = GetLinearChannelSettings(groupId, channelExternalIds);

            var response = new List<BulkUploadProgramAssetResult>();
            var programIndex = 0;
            foreach (var prog in xmlTvEpgData.programme)
            {
                var channelExternalId = prog.channel;
                // Every channel external id can point to mulitple interbal channels that have to have the same EPG
                // like channel per region or HD channel vs SD channel etc..
                var channelsToIngestProgramInto = kalturaChannels.Where(c => c.ChannelExternalID.Equals(channelExternalId, StringComparison.OrdinalIgnoreCase)).ToList();

                var programResults = new List<BulkUploadProgramAssetResult>();

                foreach (var innerChannel in channelsToIngestProgramInto)
                {
                    // TODO ARTHUR - WHY create results are here and not in BulkUploadEpgAssetData.GetNewBulkUploadResult like it should be?
                    var result = new BulkUploadProgramAssetResult
                    {
                        BulkUploadId = bulkUploadId,
                        Index = programIndex++,
                        ProgramExternalId = prog.external_id,
                        Status = BulkUploadResultStatus.InProgress,
                        LiveAssetId = innerChannel.LinearMediaId,
                        ChannelId = int.Parse(innerChannel.ChannelID),
                    };

                    result.StartDate = prog.ParseStartDate(result);
                    result.EndDate = prog.ParseEndDate(result);

                    result.Object = new EpgProgramBulkUploadObject
                    {
                        BulkUploadId = bulkUploadId,
                        ParsedProgramObject = prog,
                        ChannelExternalId = innerChannel.ChannelExternalID,
                        ChannelId = int.Parse(innerChannel.ChannelID),
                        LinearMediaId = innerChannel.LinearMediaId,
                        GroupId = groupId,
                        ParentGroupId = parentGroupId,
                        StartDate = result.StartDate,
                        EndDate = result.EndDate,
                        EpgExternalId = prog.external_id,
                    };
                    programResults.Add(result);
                }

                // If there are no inner channels found the previous loop did not fill any results, than we add error results;
                if (!channelsToIngestProgramInto.Any())
                {
                    var result = new BulkUploadProgramAssetResult
                    {
                        BulkUploadId = bulkUploadId,
                        Index = programIndex++,
                        ProgramExternalId = prog.external_id,
                        Status = BulkUploadResultStatus.Error,
                        LiveAssetId = -1
                    };
                    var progStartDate = prog.ParseStartDate(result);
                    var progEnDate = prog.ParseEndDate(result);

                    result.Object = new EpgProgramBulkUploadObject
                    {
                        ParsedProgramObject = prog,
                        ChannelExternalId = string.Empty,
                        ChannelId = -1,
                        LinearMediaId = -1,
                        GroupId = groupId,
                        ParentGroupId = parentGroupId,
                        StartDate = progStartDate,
                        EndDate = progEnDate,
                        EpgExternalId = prog.external_id,
                    };

                    var msg = $"no channel was found for channelExternalId:[{channelExternalId}]";
                    result.AddError(eResponseStatus.ChannelDoesNotExist, msg);
                    programResults.Add(result);
                }

                response.AddRange(programResults);
            }

            return response;
        }
        
        public List<LinearChannelSettings> GetLinearChannelSettings(int groupId, List<string> channelExternalIds)
        {
            var kalturaChannels = _epgDal.GetAllEpgChannelObjectsList(groupId, channelExternalIds);
            var kalturaChannelIds = kalturaChannels.Select(k => k.ChannelId).ToList();
            var liveAssets = _channelRepository.GetLinearChannelSettings(groupId, kalturaChannelIds);
            return liveAssets;
        }
    }
}