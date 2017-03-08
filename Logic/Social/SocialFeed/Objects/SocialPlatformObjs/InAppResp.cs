using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.SocialFeed;
using KLogMonitor;
using System.Reflection;

namespace Core.Social
{
    public class CommentsListResponse : Core.Catalog.Response.CommentsListResponse, ISocialFeed
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<SocialFeedItem> ToBaseSocialFeedObj()
        {
            List<SocialFeedItem> retVal = new List<SocialFeedItem>();

            if (this.m_lComments != null)
            {
                foreach (var item in this.m_lComments)
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
    

