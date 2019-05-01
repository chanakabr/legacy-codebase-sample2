using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using ApiLogic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
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
                var epgPrograms = GetBulkUploadResults(bulkUploadId, parentGroupId, groupId, xmlTvEpgData);
                return epgPrograms;
            }
            catch (Exception ex)
            {
                _Logger.Error("DeserializeEpgChannel > error while trying to Deserialize.", ex);
                throw;
            }
        }

        private List<BulkUploadResult> GetBulkUploadResults(long bulkUploadId, int parentGroupId, int groupId, EpgChannels xmlTvEpgData)
        {
            //var fieldEntityMapping = EpgIngest.Utils.GetMappingFields(parentGroupId);
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();

            _Logger.Debug($"GetBulkUploadResults > Retriving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = GetLinearChannelSettings(groupId, channelExternalIds);

            var response = new List<BulkUploadResult>();
            var programIndex = 0;
            foreach (var prog in xmlTvEpgData.programme)
            {
                var channelExternalId = prog.channel;
                // Every channel external id can point to mulitple interbal channels that have to have the same EPG
                // like channel per region or HD channel vs SD channel etc..
                var channelsToIngestProgramInto = kalturaChannels.Where(c => c.ChannelExternalID.Equals(channelExternalId, StringComparison.OrdinalIgnoreCase));
                var programResults = new List<BulkUploadProgramAssetResult>();
                foreach (var innerChannel in channelsToIngestProgramInto)
                {
                    var result = new BulkUploadProgramAssetResult();
                    result.BulkUploadId = bulkUploadId;
                    result.Index = programIndex++;
                    result.ProgramExternalId = prog.external_id;
                    result.Status = BulkUploadResultStatus.InProgress;
                    result.LiveAssetId = innerChannel.LinearMediaId;

                    var progrStartDate = prog.ParseStartDate(result);
                    var progrEnDate = prog.ParseEndDate(result);

                    result.Object = new EpgProgramBulkUploadObject
                    {
                        ParsedProgramObject = prog,
                        ChannelExternalId = innerChannel.ChannelExternalID,
                        ChannelId = int.Parse(innerChannel.ChannelID),
                        LinearMediaId = innerChannel.LinearMediaId,
                        GroupId = groupId,
                        ParentGroupId = parentGroupId,
                        StartDate = progrStartDate,
                        EndDate = progrEnDate,
                    };
                    programResults.Add(result);
                }


                response.AddRange(programResults);
            }

            return response.ToList();
        }

        private static List<LinearChannelSettings> GetLinearChannelSettings(int groupId, List<string> channelExternalIds)
        {
            var kalturaChannels = EpgDal.GetAllEpgChannelObjectsList(groupId, channelExternalIds);
            var kalturaChannelIds = kalturaChannels.Select(k => k.ChannelId).ToList();
            var liveAsstes = CatalogDAL.GetLinearChannelSettings(groupId, kalturaChannelIds);
            return liveAsstes;
        }



    }
}
