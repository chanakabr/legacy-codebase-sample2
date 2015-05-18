using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class MediaAutoCompleteResponse : BaseResponse
    {
        [DataMember]
        public List<string> lResults { get; set; }
    }
}
