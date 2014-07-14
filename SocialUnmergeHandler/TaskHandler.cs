using RemoteTasksCommon;
using Social;
using SocialBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialUnmergeHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = "fail";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting social feeder request. data=", data), "SocialUnmergeHandler");
                ApiObjects.MediaIndexingObjects.SocialUnmergeRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjects.MediaIndexingObjects.SocialUnmergeRequest>(data);
                DoUnmerge(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        private void DoUnmerge(ApiObjects.MediaIndexingObjects.SocialUnmergeRequest request)
        {
            if (request == null || request.GroupId == 0 || string.IsNullOrEmpty(request.sSiteGuid))
            {
                throw new ArgumentNullException("DoUnmerge - Must provide a valud for group ID and site guid");
            }

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(request.GroupId);

            List<string> lDocIDs;
            bool bHasNoErrors = true;

            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = oSocialBL.GetFeedIDsByActorID(request.sSiteGuid, nNumOfDocs, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                    {
                        bHasNoErrors &= oSocialBL.DeleteActivityFromUserFeed(docID);
                    }
                }

            } while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
            {
                throw new Exception("Error occured during deletion of feed");
            }
        }
    }
}
