using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}