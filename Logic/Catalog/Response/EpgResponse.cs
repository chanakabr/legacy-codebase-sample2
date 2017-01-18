using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    public class EpgResponse : BaseResponse 
    {
        [DataMember]
        public List<EpgResultsObj> programsPerChannel;        
        
        public EpgResponse()
            : base()
        {
            programsPerChannel = new List<EpgResultsObj>();
        }
    }

}
