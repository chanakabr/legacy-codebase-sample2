using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum  KalturaMetaDataType
    {
        STRING,
        MULTILINGUAL_STRING,
        NUMBER,
        BOOLEAN,        
        DATE,
        RELEATED_ENTITY
    }
}