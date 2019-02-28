using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// instractions for upload data type
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadJobData : KalturaOTTObject
    {
    }

    /// <summary>
    /// instractions for upload data type with Excel
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadExcelJobData : KalturaBulkUploadJobData
    {
    }

    /// <summary>
    /// instractions for upload data type with xml
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadXmlJobData : KalturaBulkUploadJobData
    {
    }
}