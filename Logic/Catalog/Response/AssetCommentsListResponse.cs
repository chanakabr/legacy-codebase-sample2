using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.Response
{
    /// <summary>
    /// asset comments
    /// </summary>
    [DataContract]
    public partial class AssetCommentsListResponse : BaseResponse
    {
        /// <summary>
        /// List of comments
        /// </summary>
        [DataMember]
        public List<Comments> Comments;

        [DataMember]
        public ApiObjects.Response.Status status;

        [DataMember]
        public string requestId;


        public AssetCommentsListResponse()
        {
            Comments = new List<Comments>();
            status = new ApiObjects.Response.Status();
        }
    }
}
