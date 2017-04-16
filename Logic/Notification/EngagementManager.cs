using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;

namespace Core.Notification
{
    public class EngagementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ENGAGEMENT_ADAPTER_ID_REQUIRED = "Engagement adapter identifier is required";
        private const string ENGAGEMENT_ADAPTER_NOT_EXIST = "Engagement adapter not exist";



        internal static EngagementAdapterResponseList GetEngagementAdapters(int groupId)
        {
            EngagementAdapterResponseList response = new EngagementAdapterResponseList();
            try
            {
                response.EngagementAdapters = NotificationDal.GetEngagementAdapterList(groupId);
                if (response.EngagementAdapters == null || response.EngagementAdapters.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no engagement adapter related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupId), ex);
            }

            return response;
        }

        internal static EngagementAdapterResponse GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();
            try
            {
                response.EngagementAdapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (response.EngagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, "Engagement adapter not exist");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
            }

            return response;
        }

        internal static Status DeleteEngagementAdapter(int groupId, int engagementAdapterId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {

                if (engagementAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check Engagement Adapter exist
                EngagementAdapter adapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (adapter == null || adapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }


                bool isSet = NotificationDal.DeleteEngagementAdapter(groupId, engagementAdapterId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
            }
            return response;
        }
    }
}
