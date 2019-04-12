using ApiObjects.BulkUpload;
using Core.Catalog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadAssetResult : BulkUploadResult
    {
        [JsonProperty("Type")]
        public int? Type { get; set; }

        [JsonProperty("ExternalId")]
        public string ExternalId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadMediaAssetResult : BulkUploadAssetResult
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadXmlTvChannelResult : BulkUploadResult
    {
        public BulkUploadXmlTvChannelResult() { }

        public BulkUploadXmlTvChannelResult(long bulkUploadId, string channelExternalId)
        {
            BulkUploadId = bulkUploadId;
            ChannelExternalId = channelExternalId;
        }

        public string ChannelExternalId { get; set; }
        public BulkUploadChannelResult[] InnerChannels { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadChannelResult : BulkUploadResult
    {
        public BulkUploadChannelResult() { }

        public BulkUploadChannelResult(int channelId)
        {
            ChannelId = channelId;
        }

        public int ChannelId { get; set; }

        /// <summary>
        /// key = language code, value = the program in that language
        /// </summary>
        public BulkUploadMultilingualProgramAssetResult[] Programs { get; set; }
    }

    public class BulkUploadMultilingualProgramAssetResult
    {
        public BulkUploadMultilingualProgramAssetResult() { }

        public BulkUploadMultilingualProgramAssetResult(string languageCode, BulkUploadProgramAssetResult program)
        {
            LanguageCode = languageCode;
            Program = program;
        }
        public string LanguageCode { get; set; }
        public BulkUploadProgramAssetResult Program { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadProgramAssetResult : BulkUploadResult
    {
        public int? ProgramId { get; set; }
        public string ProgramExternalId { get; set; }
    }

}
