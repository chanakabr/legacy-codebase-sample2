using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class MediaAutoCompleteResponse : BaseResponse
    {
        [DataMember]
        public List<string> lResults { get; set; }
    }
}
