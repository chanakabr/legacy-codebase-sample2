using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrlRecordingRequest : KalturaLicensedUrlBaseRequest
    {
        /// <summary>
        /// The start date of the recording (epoch)
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }

        /// <summary>
        /// The file type for the URL
        /// </summary>
        [DataMember(Name = "fileType")]
        [JsonProperty("fileType")]
        [XmlElement(ElementName = "fileType")]
        public string FileType { get; set; }

        private int recordingId { get; set; }

        public int GetRecordingId()
        {
            if (recordingId == 0)
            {
                int parsed = 0;
                if (!int.TryParse(AssetId, out parsed))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetId must be a number");
                }
                recordingId = parsed;
            }
            return recordingId;
        }

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(FileType))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "fileType cannot be empty");
            }
            int parsed = 0;
            if (!int.TryParse(AssetId, out parsed))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetId must be a number");
            }
            recordingId = parsed;
        }
    }
}