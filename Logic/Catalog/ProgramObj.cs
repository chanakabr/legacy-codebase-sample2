using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog
{
    [DataContract]
    public class ProgramObj : BaseObject
    {
        [DataMember]
        public ApiObjects.EPGChannelProgrammeObject m_oProgram;

        public ProgramObj()
            : base()
        {
            this.AssetType = ApiObjects.eAssetTypes.EPG;
            this.m_oProgram = new ApiObjects.EPGChannelProgrammeObject();
        }
    }
}
