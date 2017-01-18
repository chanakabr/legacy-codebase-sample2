using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class MediaFilesResponse : BaseResponse
    {

        public MediaFilesResponse()
        {

        }
    }

    [DataContract]
    public class MediaFileObj : BaseObject
    {
        [DataMember]
        public FileMedia m_oFile;

        public MediaFileObj()
            : base()
        {
        }
    }
}
