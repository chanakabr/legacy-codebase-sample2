using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Models.Upload
{
    public enum KalturaBulkUploadJobStatus
    {
        Pending = 1,
        Uploaded = 2,
        Queued = 3,
        Parsing = 4,
        Processing = 5,
        Processed = 6,
        Success = 7,
        Partial = 8,
        Failed = 9,
        Fatal = 10
    }
}
