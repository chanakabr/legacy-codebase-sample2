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
            string res = "fail";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting social feeder request. data=", data), "ESUpdateHandler");

                SocialData request = Newtonsoft.Json.JsonConvert.DeserializeObject<SocialData>(data);

                SocialFeeder feeder = new SocialFeeder(request.GroupId, request.ActorSiteGuid, request.DbActionId);
                bool bResult = feeder.UpdateFriendsFeed();

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
