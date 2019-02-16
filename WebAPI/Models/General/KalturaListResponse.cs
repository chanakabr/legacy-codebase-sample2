using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base list wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaListResponse : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }

        internal virtual List<string> GetExcelOverviewInstructions()
        {
            return null;
        }

        internal virtual Dictionary<ExcelColumnType, Color> GetExcelColumnsColors()
        {
            return null;
        }

        internal virtual Dictionary<string, KalturaExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null)
        {
            return null;
        }

        // TODO SHIR - IMPLEMENT WHEN OBJECTS WILL BE HERE
        //public bool HasObjects()
        //{
        //    return (Status != null && Status.Code == (int)eResponseStatus.OK && Objects != null && Objects.Count > 0);
        //}
    }
}