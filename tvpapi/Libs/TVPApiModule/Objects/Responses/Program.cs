using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

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
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject m_oProgram;

        public Program()
        {
            this.m_oProgram = new Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject();
            m_dUpdateDate = DateTime.MinValue;
            AssetType = eAssetTypes.MEDIA;
        }
    }
}
