using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaCategoryVersionState
    {
        DRAFT = 0,
        DEFAULT = 1,
        RELEASED = 2
    }
}