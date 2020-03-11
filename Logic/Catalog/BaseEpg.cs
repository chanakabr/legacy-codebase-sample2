using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using Core.Catalog.Request;

namespace Core.Catalog
{
    [KnownType(typeof(EPGProgramsByScidsRequest))]
    [KnownType(typeof(EPGProgramsByProgramsIdentefierRequest))]
    
    [DataContract]
    public abstract class BaseEpg : BaseRequest
    {
        [DataMember]
        public Language eLang { get; set; }
        [DataMember]
        public int duration { get; set; }

        public BaseEpg()
            : base()
        {
        }
        public BaseEpg(Language eLang, int duration, int nPageSize, int nPageIndex, string sUserIP, int nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            this.eLang = eLang;
            this.duration = duration;
        }

    }
}
