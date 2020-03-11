using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ODBCWrapper;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Objects;
using KLogMonitor;
using System.Reflection;


namespace TVPPro.SiteManager.Helper
{
    public static class VotesHelper
    {
        #region Fields
        /// <summary>
        /// Holds the logger
        /// </summary>
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        #endregion

        [Flags]
        public enum VoteResponse : int
        {
            NONE = 0,
            VOTE_NOT_REGISTERED = 1,
            VOTE_REGISTERED_FOR_MEDIA = 2,
            VOTE_EXISTS = 4,
            FAILURE = 8
        }

        public static string UserVote(string mediaId, string siteGuid, TVPPro.SiteManager.Context.Enums.ePlatform platform)
        {
            string res = "Failure";

            try
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    logger.InfoFormat("Adding new vote to media, ItemID:{0}.", mediaId);
                    VoteResponse voteStatus = IaAlreadyVoted(mediaId, siteGuid);
                    switch (voteStatus)
                    {
                        case VoteResponse.VOTE_NOT_REGISTERED:
                            InsertQuery query = new InsertQuery("UserVote");
                            query += ODBCWrapper.Parameter.NEW_PARAM("MEDIA", mediaId);
                            query += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", siteGuid);
                            query += ODBCWrapper.Parameter.NEW_PARAM("SCORE", GetVotingRatio(siteGuid));
                            query += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "1");
                            query += ODBCWrapper.Parameter.NEW_PARAM("VOTE_DATE", DateTime.UtcNow);
                            query += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", platform);
                            if (!query.Execute())
                            {
                                logger.ErrorFormat("Failed voting on media, ItemID:{0}.", mediaId);
                                res = "Failure";
                                break;
                            }
                            query.Finish();
                            query = null;
                            logger.InfoFormat("Vote was registered successfully, ItemID:{0}.", mediaId);
                            return "Success";
                        case VoteResponse.VOTE_REGISTERED_FOR_MEDIA:
                            logger.InfoFormat("Double Vote was not registered, ItemID:{0}.", mediaId);
                            return "DoubleVote";
                        case VoteResponse.VOTE_EXISTS:
                            logger.InfoFormat("Already voted this month, ItemID:{0}.", mediaId);
                            return "AlreadyVoted";
                        case VoteResponse.FAILURE:
                            logger.ErrorFormat("Failed voting on media, ItemID:{0}.", mediaId);
                            return "Failure";
                        default:
                            break;
                    }
                }
                else
                {
                    logger.ErrorFormat("User didn't pass authentication");
                    return "Failure";
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(ex.Message);
                throw ex;
            }

            return res;
        }

        public static VoteResponse IaAlreadyVoted(string mediaId, string siteGuid)
        {
            VoteResponse res = VoteResponse.FAILURE;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 1 VOTE_DATE,MEDIA from UserVote where";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("UserIdentifier", "=", UsersService.Instance.GetUserID());
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", siteGuid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            selectQuery += " order by VOTE_DATE desc ";

            if (selectQuery.Execute("UserVote", true) != null)
            {
                Int32 nCount = selectQuery.Table("UserVote").DefaultView.Count;
                if (nCount > 0)
                {
                    string sVoteDate = selectQuery.Table("UserVote").DefaultView[0].Row["VOTE_DATE"].ToString();
                    DateTime oVoteDate = DateTime.Parse(sVoteDate);
                    DateTime oCurrentDate = DateTime.Now;
                    // Check if a month 
                    if ((oVoteDate.AddMonths(1) - oCurrentDate).Ticks > 0)
                    {
                        String mediaVoted = selectQuery.Table("UserVote").DefaultView[0].Row["MEDIA"].ToString();
                        if (mediaVoted == mediaId) res = VoteResponse.VOTE_REGISTERED_FOR_MEDIA;
                        else res = VoteResponse.VOTE_EXISTS;
                    }
                    else
                    {
                        res = VoteResponse.VOTE_NOT_REGISTERED;
                    }
                }
                else
                {
                    res = VoteResponse.VOTE_NOT_REGISTERED;
                }
            }
            else
            {
                res = VoteResponse.FAILURE;
            }

            return res;
        }

        public static Int32 GetVotingRatio(string sSiteGuid)
        {
            UserCAStatus oUserCAStatus = ConditionalAccessService.Instance.GetUserCAStatus(sSiteGuid);

            if (oUserCAStatus == UserCAStatus.CurrentSub)
            {
                return 3;
            }
            else if (oUserCAStatus == UserCAStatus.NeverPurchased)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static Dictionary<string, RatedMediaEntity> GetMediasByRating()
        {
            Dictionary<string, RatedMediaEntity> dic = new Dictionary<string, RatedMediaEntity>();
            DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select  MAX(ResetDate) as ResetDate from dbo.UserVoteCycle where ";
            selectQuery += Parameter.NEW_PARAM("ResetDate", "<", DateTime.UtcNow);
            selectQuery += " and [status]=1";
            if (selectQuery.Execute("UserVoteCycle", true) != null)
            {
                int nCount = selectQuery.Table("UserVoteCycle").DefaultView.Count;
                if (nCount > 0)
                {
                    string rDate = selectQuery.Table("UserVoteCycle").DefaultView[0].Row["ResetDate"].ToString();
                    try
                    {
                        DateTime fromWhen = DateTime.Parse(rDate);
                        dic = GetMediasByRating(fromWhen);
                    }
                    catch { }

                    selectQuery.Finish();

                }
            }

            return dic;



        }
        public static Dictionary<string, RatedMediaEntity> GetMediasByRating(DateTime fromWhen)
        {
            Dictionary<string, RatedMediaEntity> dic = new Dictionary<string, RatedMediaEntity>();
            int nCount = 0;
            string limitDate = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 1 ResetDate from dbo.UserVoteCycle where ";
            selectQuery += Parameter.NEW_PARAM("ResetDate", ">", fromWhen);
            selectQuery += " order by ResetDate asc";
            if (selectQuery.Execute("UserVoteCycle", true) != null)
            {
                nCount = selectQuery.Table("UserVoteCycle").DefaultView.Count;
                if (nCount > 0)
                {
                    limitDate = selectQuery.Table("UserVoteCycle").DefaultView[0].Row["ResetDate"].ToString();
                    nCount = 0;
                }
            }
            selectQuery.Finish();

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select MEDIA,sum(SCORE) as SCORE_SUM";
            selectQuery += "from dbo.UserVote where";
            selectQuery += Parameter.NEW_PARAM("VOTE_DATE ", " > ", fromWhen);
            if (!string.IsNullOrEmpty(limitDate))
                selectQuery += string.Format("and VOTE_DATE < '{0}' ", limitDate);
            selectQuery += " and [STATUS]=1";
            selectQuery += "group by MEDIA order by SCORE_SUM DESC";

            if (selectQuery.Execute("UserVote", true) != null)
            {
                nCount = selectQuery.Table("UserVote").DefaultView.Count;
                if (nCount > 0)
                {
                    RatedMediaEntity ent;

                    for (int i = 0; i < nCount; i++)
                    {
                        ent = new RatedMediaEntity();
                        ent.MediaID = selectQuery.Table("UserVote").DefaultView[i].Row["Media"].ToString();
                        ent.Score = (int)selectQuery.Table("UserVote").DefaultView[i].Row["SCORE_SUM"];
                        dic.Add(ent.MediaID, ent);
                    }

                }
                selectQuery.Finish();
            }
            return dic;

        }


        public static List<VoteCycleEntity> GetVoteCycles(bool IsInFuture)
        {
            List<VoteCycleEntity> list = new List<VoteCycleEntity>();
            string qOperator = "<";
            DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select  * from dbo.UserVoteCycle where ";

            if (IsInFuture)
                qOperator = "> ";

            selectQuery += Parameter.NEW_PARAM("ResetDate", qOperator, DateTime.UtcNow);
            selectQuery += " and [status] =1";
            selectQuery += "order by ResetDate DESC";
            if (selectQuery.Execute("UserVoteCycle", true) != null)
            {
                int nCount = selectQuery.Table("UserVoteCycle").DefaultView.Count;
                if (nCount > 0)
                {


                    for (int i = 0; i < nCount; i++)
                    {
                        list.Add(new VoteCycleEntity()
                        {
                            ResetDate = DateTime.Parse(selectQuery.Table("UserVoteCycle").DefaultView[i].Row["ResetDate"].ToString()),
                            CreateDate = DateTime.Parse(selectQuery.Table("UserVoteCycle").DefaultView[i].Row["CreateDate"].ToString()),
                            ID = selectQuery.Table("UserVoteCycle").DefaultView[i].Row["ID"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public static bool StartNewVoteCycle(DateTime when)
        {
            bool res = true;


            InsertQuery query = new InsertQuery("UserVoteCycle");
            query += ODBCWrapper.Parameter.NEW_PARAM("CreateDate", DateTime.UtcNow);
            query += ODBCWrapper.Parameter.NEW_PARAM("ResetDate", when);
            if (!query.Execute())
            {
                logger.Error("Failed to start new vote cycle");
                res = false;
            }
            query.Finish();
            query = null;


            return res;
        }
        public static void DeleteCycle(string id)
        {
            ODBCWrapper.DataSetSelectQuery delete = new ODBCWrapper.DataSetSelectQuery();
            delete += "update dbo.UserVoteCycle set [status]=0 where";
            delete += Parameter.NEW_PARAM("ID", "=", id);
            delete.Execute();
            delete.Finish();
            delete = null;
        }
    }

}
