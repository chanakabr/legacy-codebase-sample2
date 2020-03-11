using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaImageStatus
    {
        PENDING,
        READY,
        FAILED
    }
}