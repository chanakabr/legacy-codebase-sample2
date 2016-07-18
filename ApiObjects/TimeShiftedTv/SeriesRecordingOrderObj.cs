using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    [Serializable]
    [JsonObject(Id = "seriesRecordingOrderObj")]
    [DataContract]
    public class SeriesRecordingOrderObj
    {
        [DataMember]
        [JsonProperty()]
        public SeriesOrderBy OrderBy;
        [DataMember]
        [JsonProperty()]
        public OrderDir OrderDir;

        public SeriesRecordingOrderObj()
        {
            OrderBy = SeriesOrderBy.ID;
            OrderDir = OrderDir.DESC;
        }
    }

    [DataContract]
    public enum SeriesOrderBy
    {
        [EnumMember]
        ID = 0,
        [EnumMember]
        START_DATE = 1,
        [EnumMember]
        SERIES_ID = 2
    }
}
