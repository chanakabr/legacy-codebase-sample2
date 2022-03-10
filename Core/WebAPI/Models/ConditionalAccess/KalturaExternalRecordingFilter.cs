using ApiObjects.Base;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering external recordings
    /// </summary>
    [Serializable]
    public partial class KalturaExternalRecordingFilter : KalturaRecordingFilter
    {
        /// <summary>
        /// MetaData filtering 
        /// </summary>
        [DataMember(Name = "metaData")]
        [JsonProperty("metaData")]
        [XmlElement(ElementName = "metaData", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public SerializableDictionary<string, KalturaStringValue> MetaData { get; set; }

        new internal void Validate()
        {
            base.Validate();

            if (MetaData == null)
            {
                MetaData = new SerializableDictionary<string, KalturaStringValue>();
            }
        }

        internal override KalturaRecordingListResponse SearchRecordings(ContextData contextData, KalturaFilterPager pager)
        {
            this.Validate();

            var metaDataFilter = this.MetaData.ToDictionary(x => x.Key.ToLower(), x => x.Value.value.ToLowerOrNull());

            var response = ClientsManager.ConditionalAccessClient().SearchRecordings(contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value,
                this.ConvertStatusIn(), this.Ksql, this.GetExternalRecordingIds(), pager.GetRealPageIndex(), pager.PageSize, this.OrderBy, metaDataFilter);

            return response;
        }
    }
}