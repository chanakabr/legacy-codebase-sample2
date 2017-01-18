using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog.Response
{
    [DataContract]
    public class PersonalLastDeviceResponse : BaseResponse
    {
        [DataMember]
        public List<PersonalLastDevice> m_lPersonalLastWatched;       

        public PersonalLastDeviceResponse()
            : base()
        {
            m_lPersonalLastWatched = new List<PersonalLastDevice>();
        }
    }

    public class PersonalLastDevice
    {
        [DataMember]
        public int m_nID;
        [DataMember]
        public DateTime? m_dLastWatchedDate;
        [DataMember]
        public string m_sLastWatchedDevice;
        [DataMember]
        public string m_sSiteUserGuid;

        public PersonalLastDevice()
        {
        }
    }
}
