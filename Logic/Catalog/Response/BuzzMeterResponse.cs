using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Statistics;

namespace Core.Catalog.Response
{
    [DataContract]
    public class BuzzMeterResponse : BaseResponse
    {
        [DataMember]
        public BuzzWeightedAverScore m_buzzAverScore;

        public BuzzMeterResponse()
        {
           
        }
    }
}
