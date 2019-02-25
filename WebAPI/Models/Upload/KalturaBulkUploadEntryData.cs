using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// instractions for upload data values
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadEntryData : KalturaOTTObject
    {
    }

    /// <summary>
    /// instractions for upload asset values
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadAssetEntryData : KalturaBulkUploadEntryData
    {
    }

    /// <summary>
    /// instractions for upload media asset values
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadMediaEntryData : KalturaBulkUploadAssetEntryData
    {
    }

    /// <summary>
    /// instractions for upload epg asset values
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadEpgEntryData : KalturaBulkUploadAssetEntryData
    {
    }
}