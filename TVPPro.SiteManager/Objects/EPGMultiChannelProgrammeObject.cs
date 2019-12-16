using ApiObjects;
using Core.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "EPGMultiChannelProgrammeObject", Namespace = "http://schemas.datacontract.org/2004/07/Catalog")]
    [System.SerializableAttribute()]
    public class EPGMultiChannelProgrammeObject : BaseObject 
    {
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string EPG_CHANNEL_ID { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public List<EPGChannelProgrammeObject> EPGChannelProgrammeObject { get; set; }

        public EPGMultiChannelProgrammeObject() : base()
        {
            this.AssetType = eAssetTypes.EPG;
        }
    }

    public enum EPGUnit
    {
        Days,
        
        Hours,
        
        Current
    }
}
