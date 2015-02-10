using ApiObjects;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMergeHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = "failure";
            Logger.Logger.Log("Info", string.Concat("starting social feeder request. data=", data), "SocialMergeHandler");
            ApiObjects.MediaIndexingObjects.SocialMergeRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjects.MediaIndexingObjects.SocialMergeRequest>(data);

            if (request == null || request.GroupId == 0 || string.IsNullOrEmpty(request.sSiteGuid))
                throw new ArgumentNullException("DoUnmerge - Must provide a value for group ID and site guid");

            switch (request.Action)
            {
                case "create":
                    DoMerge(request);
                    break;
                case "delete":
                    DoUnmerge(request);
                    break;
                default:
                    Logger.Logger.Log("Error", string.Concat("Invalid request action. request=", data), "SocialMergeHandler");
                    break;
            }

            res = "success";
            return res;
        }

        private void DoMerge(ApiObjects.MediaIndexingObjects.SocialMergeRequest request)
        {
            int siteGuid;

            if (int.TryParse(request.sSiteGuid, out siteGuid))
            {
                using (SocialReference.module socialRef = new SocialReference.module())
                {
                    try
                    {
                        string ip = "1.1.1.1";
                        string wsUserName = string.Empty;
                        string wsPassword = string.Empty;
                        TVinciShared.WS_Utils.GetWSUNPass(request.GroupId, "UpdateFriendsFeed", "social", ip, ref wsUserName, ref wsPassword);
                        string wsUrl = TVinciShared.WS_Utils.GetTcmConfigValue("social_ws");
                        if (wsUrl.Length > 0)
                            socialRef.Url = wsUrl;

                        socialRef.MergeFriendsActivityFeed(wsUserName, wsPassword, siteGuid);
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log("Info", string.Format("Error occurred while updating friends feed. groupId: {0}, siteguid: {1} exception: {2}", request.GroupId, siteGuid, ex.Message), "UpdateFriendsFeed");
                    }
                }
            }
            else
                throw new Exception(string.Concat("Invalid siteguid in DoMerge request. id=", request.sSiteGuid));
        }

        private void DoUnmerge(ApiObjects.MediaIndexingObjects.SocialMergeRequest request)
        {

            try
            {
                Task[] tasks = new Task[2]{Task.Factory.StartNew(() => DeleteActivitiesFromFriendsFeed(Convert.ToInt32(request.sSiteGuid),request.GroupId)),
                                           Task.Factory.StartNew(() => DeleteUserFeed(Convert.ToInt32(request.sSiteGuid),request.GroupId))};

                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                string sException = "";

                foreach (var e in ae.InnerExceptions)
                    string.Concat(sException, "ex=", e.Message, "; stack=", e.StackTrace, ";\n");

                throw new Exception(sException);
            }
        }

        private void DeleteActivitiesFromFriendsFeed(int siteGuid, int groupId)
        {
            using (SocialReference.module socialRef = new SocialReference.module())
            {
                try
                {
                    string ip = "1.1.1.1";
                    string wsUserName = string.Empty;
                    string wsPassword = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(groupId, "DeleteActivitiesFromFriendsFeed", "social", ip, ref wsUserName, ref wsPassword);
                    string wsUrl = TVinciShared.WS_Utils.GetTcmConfigValue("social_ws");
                    if (wsUrl.Length > 0)
                        socialRef.Url = wsUrl;

                    socialRef.DeleteFriendsFeed(wsUserName, wsPassword, siteGuid);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Info", string.Format("Error occurred while deleting friends feed. groupId: {0}, siteguid: {1} exception: {2}", groupId, siteGuid, ex.Message), "DeleteActivitiesFromFriendsFeed");
                }
            }
        }

        private void DeleteUserFeed(int siteGuid, int groupId)
        {
            using (SocialReference.module socialRef = new SocialReference.module())
            {
                try
                {
                    string ip = "1.1.1.1";
                    string wsUserName = string.Empty;
                    string wsPassword = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(groupId, "DeleteUserFeed", "social", ip, ref wsUserName, ref wsPassword);
                    string wsUrl = TVinciShared.WS_Utils.GetTcmConfigValue("social_ws");
                    if (wsUrl.Length > 0)
                        socialRef.Url = wsUrl;

                    socialRef.DeleteUserFeed(wsUserName, wsPassword, siteGuid);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Info", string.Format("Error occurred while deleting user feed. groupId: {0}, siteguid: {1} exception: {2}", groupId, siteGuid, ex.Message), "DeleteUserFeed");
                }
            }
        }
    }
}
