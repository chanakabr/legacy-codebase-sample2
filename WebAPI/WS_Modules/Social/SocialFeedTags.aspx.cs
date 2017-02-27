using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core.Social;
using TVinciShared;

namespace WS_Social
{
    public partial class SocialFeedTags : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int mediaID;
            if (!int.TryParse(Request.QueryString["mediaID"], out mediaID)) return;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Response.Write(serializer.Serialize(SocialFeedUtils.GetSocialFeedTags(mediaID)));
            Response.Cache.SetMaxAge(TimeSpan.FromMinutes(WS_Utils.GetTcmIntValue(string.Format("SocialFeed_Tags_TTL"))));
        }
    }
}