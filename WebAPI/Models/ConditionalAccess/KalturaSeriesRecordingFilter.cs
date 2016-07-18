using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
     /// <summary>
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public class KalturaSeriesRecordingFilter : KalturaFilter<KalturaSeriesRecordingOrderBy>
    {  
       
        public override KalturaSeriesRecordingOrderBy GetDefaultOrderByValue()
        {
            return KalturaSeriesRecordingOrderBy.START_DATE_DESC;
        }
    }
}