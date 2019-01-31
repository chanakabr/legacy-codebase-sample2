using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    // TODO SHIR - DELETE THIS WHEN FINISH TO CHECK..
    public partial class KalturaShirCheckListResponse : KalturaGenericListResponse<KalturaTvmGeoRule>
    {
    }
}