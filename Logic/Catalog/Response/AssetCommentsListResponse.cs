using Core.Social.SocialFeed;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.Response
{
    /// <summary>
    /// asset comments
    /// </summary>
    [DataContract]
    public class AssetCommentsListResponse : BaseResponse, ISocialFeed
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        public List<SocialFeedItem> ToBaseSocialFeedObj()
        {
            List<SocialFeedItem> retVal = new List<SocialFeedItem>();

            if (this.Comments != null)
            {
                foreach (var item in this.Comments)
                {
                    retVal.Add(new SocialFeedItem()
                    {
                        Title = item.m_sHeader,
                        Body = item.m_sContentText,
                        CreatorName = item.m_sWriter,
                        CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(item.m_dCreateDate),
                        CreatorImageUrl = item.m_sUserPicURL
                    });
                }
            }

            log.Debug("ToBaseSocialFeedObj - " + string.Format("ToBaseSocialFeedObj: length = {0}", retVal.Count));

            return retVal;
        }
    }
}
