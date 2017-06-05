using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class MetaResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<Meta> MetaList { get; set; }

        public MetaResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            MetaList = new List<Meta>();
        }
    }

    public class Meta
    {
        public string Name { get; set; }
        public MetaFieldName FieldName { get; set; }
        public MetaType Type { get; set; }
        public eAssetTypes AssetType { get; set; }
        public bool SkipFeatures { get; set; }        
        public List<MetaFeatureType> Features { get; set; }
        public string ParentId { get; set; }
        public int PartnerId { get; set; }
        public string Id { get; set; }  // partnerId_AssetType_ColumnIndex  || partnerId_AssetType_TagId              
        public bool IsTag { get; set; }        
    }

    public enum MetaFeatureType
    {
        USER_INTEREST,
        ENABLED_NOTIFICATION
    }
}