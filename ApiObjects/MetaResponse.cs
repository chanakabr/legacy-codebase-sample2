using ApiObjects.Response;
using Newtonsoft.Json;
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
        public List<MetaFeatureType> Features { get; set; }
        public List<string> DefaultValues { get; set; }
    }

    public enum MetaFeatureType
    {
        USER_INTEREST
    }
}