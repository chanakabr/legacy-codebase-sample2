using ApiObjects.MediaIndexingObjects;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialFeedHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting social feeder request. data=", data), "SocialFeedHandler");

                SocialFeedRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<SocialFeedRequest>(data);

                SocialFeeder feeder = new SocialFeeder(request.GroupId, request.ActorSiteGuid);
                bool bResult = feeder.UpdateFriendsFeed(request.DbActionId);

                if (bResult)
                {
                    res = "success";
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }
    }
}
