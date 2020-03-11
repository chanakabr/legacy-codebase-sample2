using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using TVinciShared;

namespace Core.Catalog.Response
{
    [DataContract]
    public class MediaLastPositionResponse : BaseResponse
    {
        [DataMember]
        public string m_sStatus;

        [DataMember]
        public string m_sDescription;

        [DataMember]
        public int Location;

        public MediaLastPositionResponse()
        {
        }
    }
}
