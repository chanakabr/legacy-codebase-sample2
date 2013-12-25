using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class RateMediaDTO
    {
        public int nSum { get; set; }
        public int nCount { get; set; }
        public double nAvg { get; set; }
        public GenericWriteResponseDTO oStatus { get; set; }
    }

    public class GenericWriteResponseDTO
    {

        public int m_nStatusCode { get; set; }
        public string m_sStatusDescription { get; set; }
    }
}