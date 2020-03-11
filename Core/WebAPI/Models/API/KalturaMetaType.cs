using System;

namespace WebAPI.Models.API
{
    [Serializable]
    public enum KalturaMetaType
    {
        STRING,
        NUMBER,
        BOOLEAN,
        STRING_ARRAY, // tag
        DATE,
        RELEATED_ENTITY
    }
}