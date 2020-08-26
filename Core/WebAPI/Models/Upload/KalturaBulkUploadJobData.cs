using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// instructions for upload data (the data file type, how to read the file, etc)
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadJobData : KalturaOTTObject
    {
        /// <summary>
        /// Validate the specifics of the job data
        /// Will throw an exception if not valid
        /// </summary>
        internal abstract void Validate(KalturaOTTFile fileData);
    }

    /// <summary>
    /// instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadExcelJobData : KalturaBulkUploadJobData
    {
        internal override void Validate(KalturaOTTFile fileData)
        {
            if (!fileData.name.EndsWith(ExcelFormatterConsts.EXCEL_EXTENTION))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fileData.name");
            }
        }
    }

    /// <summary>
    /// instructions for upload data type with xml
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadIngestJobData : KalturaBulkUploadJobData
    {
        /// <summary>
        /// Identifies the ingest profile that will handle the ingest of programs
        /// Ingest profiles are created separately using the ingest profile service
        /// </summary>
        [DataMember(Name = "ingestProfileId")]
        [JsonProperty(PropertyName = "ingestProfileId")]
        [XmlElement(ElementName = "ingestProfileId")]
        [SchemeProperty(MinInteger = 1)]
        public int? IngestProfileId { get; set; }

        internal override void Validate(KalturaOTTFile fileData)
        {
            // TODO: Arthur Validate
        }
    }
}