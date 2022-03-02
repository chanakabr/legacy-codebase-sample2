using System;
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
}