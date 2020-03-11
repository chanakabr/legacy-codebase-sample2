using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ApiObjects
{
    [DataContract(Namespace = "", Name = "EpgIngestReponse")]
    public class EpgIngestResponse : IngestResponse
    {

    }
}