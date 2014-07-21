using ApiObjects;
using RemoteTasksCommon;
using Social;
using SocialBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMergeHandler
{
    public class TaskHandler : ITaskHandler
    {
        private SocialBL.BaseSocialBL m_oSocialBL;

        public string HandleTask(string data)
        {
            string res = "failure";

            Logger.Logger.Log("Info", string.Concat("starting social feeder request. data=", data), "SocialMergeHandler");
            ApiObjects.MediaIndexingObjects.SocialMergeRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjects.MediaIndexingObjects.SocialMergeRequest>(data);
            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(request.GroupId);

            if (request == null || request.GroupId == 0 || string.IsNullOrEmpty(request.sSiteGuid))
            {
                throw new ArgumentNullException("DoUnmerge - Must provide a value for group ID and site guid");
            }

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
            FacebookWrapper oFBWrapper = new FacebookWrapper(request.GroupId);

            List<string> lFriendIDs;
            int nSiteGuid;

            if (int.TryParse(request.sSiteGuid, out nSiteGuid))
            {
                if (oFBWrapper.GetUserFriendsGuid(nSiteGuid, out lFriendIDs))
                {
                    List<SocialActivityDoc> lFriendsActions = new List<SocialActivityDoc>();
                    bool bSuccess = true;
                    foreach (string friendID in lFriendIDs)
                    {
                        List<SocialActivityDoc> lActions;
                        if (m_oSocialBL.GetUserSocialAction(friendID, 20, 0, out lActions))
                        {
                            lFriendsActions.AddRange(lActions);
                        }
                        else
                        {
                            bSuccess = false;
                            break;
                        }
                    }

                    if (bSuccess)
                    {
                        m_oSocialBL.InsertFriendsActivitiesToUserActivityFeed(request.sSiteGuid, lFriendsActions);
                    }
                    else
                    {
                        throw new Exception(string.Concat("Error retrieving friends activities. id=", request.sSiteGuid));
                    }
                }
                else
                {
                    throw new Exception(string.Concat("Unable to get user friends site guid. id=", request.sSiteGuid));
                }

            }
            else
            {
                throw new Exception(string.Concat("Invalid site guid in DoMerge request. id=", request.sSiteGuid));
            }

        }

        private void DoUnmerge(ApiObjects.MediaIndexingObjects.SocialMergeRequest request)
        {
         
            try
            {
                Task[] tasks = new Task[2]{Task.Factory.StartNew(() => DeleteActivitiesFromFriendsFeed(request.sSiteGuid)),
                                          Task.Factory.StartNew(() => DeleteUserFeed(request.sSiteGuid))};

                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                string sException = "";

                foreach (var e in ae.InnerExceptions)
                {
                    string.Concat(sException, "ex=", e.Message, "; stack=", e.StackTrace, ";\n");
                }

                throw new Exception(sException);
            }
        }

        private void DeleteActivitiesFromFriendsFeed(string sSiteGuid)
        {
            List<string> lDocIDs;
            bool bHasNoErrors = true;

            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = m_oSocialBL.GetFeedIDsByActorID(sSiteGuid, nNumOfDocs, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                    {
                        m_oSocialBL.DeleteActivityFromUserFeed(docID);
                    }
                }

            } while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
            {
                throw new Exception(string.Concat("Error occured during deletion of user friends feed. siteguid=", sSiteGuid));
            }
        }

        private void DeleteUserFeed(string sSiteGuid)
        {
            List<string> lDocIDs;
            bool bHasNoErrors = true;

            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = m_oSocialBL.GetUserActivityFeedIds(sSiteGuid, nNumOfDocs, 0, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                    {
                        m_oSocialBL.DeleteActivityFromUserFeed(docID);
                    }
                }

            } while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
            {
                throw new Exception(string.Concat("Error occured during deletion of user's feed. siteguid=", sSiteGuid));
            }
        }
    }
}
