using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    [ListResponse("IngestStatus")]
    public partial class KalturaIngestStatusEpgListResponse : KalturaListResponse<KalturaIngestEpg>
    {
    }
}
