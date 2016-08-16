using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Response
{
    public class AssetCommentResponse : BaseResponse
    {
        [DataMember]
        public Comments AssetComment { get; set; }

        [DataMember]
        public Status Status;

        public AssetCommentResponse()
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.AssetComment = null;
        }

        public AssetCommentResponse(Comments assetComment, Status status)
        {
            this.Status = status;
            this.AssetComment = assetComment;
        }
    }
}
