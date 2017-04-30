using ApiObjects.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TvinciImporter;
using TVinciShared;

public partial class AjaxManinEngagements : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";
        string sError = "";        

        string userId = "";
        int couponGroupId = 0;

        int engagementTypeId = 0;

        if (Request.Form["user_id"] != null)
            userId = Request.Form["user_id"].ToString();
        if (Request.Form["coupon_group"] != null && !string.IsNullOrEmpty(Request.Form["coupon_group"].ToString()))
        {
            couponGroupId = int.Parse(Request.Form["coupon_group"].ToString());
        }
        else
        {
            sRet = "FAIL";
            sError = "coupon_group is empty"; 
        }

        if (Request.Form["engagement_type"] != null && !string.IsNullOrEmpty(Request.Form["engagement_type"].ToString()))
        {
            engagementTypeId = int.Parse(Request.Form["engagement_type"].ToString());
        }
        else
        {
            sRet = "FAIL";
            sError = "engagement_type is empty"; 
        }

        if (string.IsNullOrEmpty(sError))
        {

            Engagement engagement = new Engagement();
            engagement.UserList = userId;
            engagement.SendTime = DateTime.UtcNow;
            engagement.CouponGroupId = couponGroupId;
            engagement.EngagementType = (ApiObjects.eEngagementType)engagementTypeId;

            if (engagement != null)
            {
                ApiObjects.Response.Status result = ImporterImpl.AddEngagement(LoginManager.GetLoginGroupID(), ref engagement);
                if (result == null)
                {
                    sError = "Error";
                    sRet = "FAIL";
                }
                else if (result.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    sError = result.Message;
                    sRet = "OK";
                }                
            }
        }


        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~" + sError);
    }

}