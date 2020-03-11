using ApiObjects.MediaIndexingObjects;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace SocialFeedHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.Info("Info - " + string.Concat("starting social feeder request. data=", data));

                SocialFeedRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<SocialFeedRequest>(data);

                SocialFeeder feeder = new SocialFeeder(request.GroupId, Convert.ToInt32(request.ActorSiteGuid));
                bool bResult = feeder.UpdateFriendsFeed(request.DbActionId);

                if (bResult)
                    res = "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }
    }
}
