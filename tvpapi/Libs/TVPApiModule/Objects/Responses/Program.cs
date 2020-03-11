using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [DataContract]
    public class Program
    {
        [DataMember]
        public string AssetId;

        [DataMember]
        public DateTime m_dUpdateDate;

        [DataMember]
        public eAssetTypes AssetType { get; set; }

        [DataMember]
        public EPGChannelProgrammeObject m_oProgram;

        public Program()
        {
            this.m_oProgram = new EPGChannelProgrammeObject();
            m_dUpdateDate = DateTime.MinValue;
            AssetType = eAssetTypes.MEDIA;
        }
    }
}
