using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdapaterCommon.Models;

namespace DRMAdapter
{
    public class DrmAdapterResponse
    {
        public string Data { get; set; }

        public string ProviderResponse { get; set; }

        public AdapterStatus Status { get; set; }
    }

    public enum AssetType
    {
        EPG,
        RECORDING,
        VOD
    }

    public enum ContextType
    {
        STREAMING,
        DOWNLOAD
    }
}