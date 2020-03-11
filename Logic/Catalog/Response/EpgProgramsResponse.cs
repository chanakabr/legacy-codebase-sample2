using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    public class EpgProgramsResponse : BaseResponse
    {
        [DataMember]
        public List<EPGChannelProgrammeObject> lEpgList;

        public EpgProgramsResponse()
        {
            lEpgList = new List<EPGChannelProgrammeObject>();
        }
    }
}
