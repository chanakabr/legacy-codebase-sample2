using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaManualChannel : KalturaChannel
    {
        /// <summary>
        /// A list of comma separated media ids associated with this channel, according to the order of the medias in the channel.
        /// </summary>
        [DataMember(Name = "mediaIds")]
        [JsonProperty("mediaIds")]
        [XmlElement(ElementName = "mediaIds", IsNullable = true)]
        public string MediaIds { get; set; }

        /// <summary>
        /// List of assets identifier
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public List<KalturaManualCollectionAsset> Assets { get; set; }

        public void ValidateMediaIds()
        {
            if (!string.IsNullOrEmpty(MediaIds))
            {
                string[] stringValues = MediaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaManualChannel.mediaIds");
                    }
                }
            }
        }

        public void ValidateAssets()
        {
            if (Assets != null)
            {
                List<string> ids = Assets.Where(x => x.Type == KalturaManualCollectionAssetType.media).Select(x => x.Id).ToList();
                var duplicates = ids.GroupBy(x => x).Count(t => t.Count() > 1);
                if (duplicates > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "assets");
                }

                ids = Assets.Where(x => x.Type == KalturaManualCollectionAssetType.epg).Select(x => x.Id).ToList();
                duplicates = ids.GroupBy(x => x).Count(t => t.Count() > 1);
                if (duplicates > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "assets");
                }
            }
        }

        internal override void ValidateForInsert()
        {
            base.ValidateForInsert();
            if(Assets != null && !string.IsNullOrEmpty(MediaIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaManualChannel.assets", "KalturaManualChannel.mediaIds");
            }

            ValidateMediaIds();
            ValidateAssets();
        }

        internal override void ValidateForUpdate()
        {
            base.ValidateForUpdate();
            if (Assets != null && !string.IsNullOrEmpty(MediaIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaManualChannel.assets", "KalturaManualChannel.mediaIds");
            }

            ValidateMediaIds();
            ValidateAssets();
        }
    }
}