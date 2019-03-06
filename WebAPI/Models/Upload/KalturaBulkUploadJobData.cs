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
    /// instructions for upload data (the data file type, how to read the file, etc)
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadJobData : KalturaOTTObject
    {
    }

    /// <summary>
    /// instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadExcelJobData : KalturaBulkUploadJobData
    {
    }

    /// <summary>
    /// instructions for upload data type with xml
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadXmlJobData : KalturaBulkUploadJobData
    {
    }
}