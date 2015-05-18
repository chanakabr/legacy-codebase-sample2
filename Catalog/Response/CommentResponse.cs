using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog.Response
{
    [DataContract]
    public class CommentResponse : BaseResponse 
    {
        [DataMember]
        public StatusComment eStatusComment { get; set; }
    }
}