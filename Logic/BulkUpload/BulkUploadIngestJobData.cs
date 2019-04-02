using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using ApiObjects;
using ApiObjects.BulkUpload;
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

        public override GenericListResponse<GenericResponse<IBulkUploadObject>> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<GenericResponse<IBulkUploadObject>>();
            var profile = api.GetIngestProfileById(IngestProfileId)?.Object;
            var xmlTvString = GetXmlTv(fileUrl, profile);

            if (string.IsNullOrEmpty(xmlTvString))
            {
                response.SetStatus(eResponseStatus.FileDoesNotExists, $"Could not find file:[{fileUrl}]");
                return response;
            }

            var epgData = DeserializeXmlTvEpgData(xmlTvString);

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

        private EpgChannels DeserializeXmlTvEpgData(string Data)
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
                var epgPrograms = MapXmlTvProgramToCBEpgProgram(groupId, xmlTvEpgData);
            }
            catch (Exception ex)
            {
                _Logger.Error("DeserializeEpgChannel > error while trying to Deserialize.", ex);
                throw;
            }
        }

        private IList<EpgCB> MapXmlTvProgramToCBEpgProgram(int groupId, EpgChannels xmlTvEpgData)
        {
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();
            _Logger.Debug($"MapXmlTvProgramToCBEpgProgram > Retriving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = EpgDal.GetAllEpgChannelsDic(groupId, channelExternalIds);
            EpgCB newEpgItem = new EpgCB();
            foreach (var prog in xmlTvEpgData.programme)
            {
                newEpgItem.ChannelID = kalturaChannelID;
                newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(m_Channels.groupid);
                newEpgItem.ParentGroupID = m_Channels.parentgroupid;
                newEpgItem.EpgIdentifier = prog.external_id;
                newEpgItem.StartDate = dProgStartDate;
                newEpgItem.EndDate = dProgEndDate;
                newEpgItem.UpdateDate = DateTime.UtcNow;
                newEpgItem.CreateDate = DateTime.UtcNow;
                newEpgItem.IsActive = true;
                newEpgItem.Status = 1;
                newEpgItem.EnableCatchUp = EnableLinearSetting(prog.enablecatchup);
                newEpgItem.EnableCDVR = EnableLinearSetting(prog.enablecdvr);
                newEpgItem.EnableStartOver = EnableLinearSetting(prog.enablestartover);
                newEpgItem.EnableTrickPlay = EnableLinearSetting(prog.enabletrickplay);
                newEpgItem.Crid = prog.crid;
            }
        }
    }
}
