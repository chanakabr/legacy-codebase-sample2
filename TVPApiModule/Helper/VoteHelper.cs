using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using ODBCWrapper;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApi;
using TVPApiModule.Services;

namespace TVPApiModule.Helper
{
    public static class VotesHelper
    {
        #region Fields
        /// <summary>
        /// Holds the logger
        /// </summary>
        public static ILog logger = LogManager.GetLogger("VotesHelper");
        #endregion

        public static string UserVote(string mediaId, string siteGuid, PlatformType platform, int groupID)
        {
            string res = "Failure";

            try
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    logger.InfoFormat("Adding new vote to media, ItemID:{0}.", mediaId);
                    if (!IsAlreadyVoted(mediaId, siteGuid))
                    {
                        InsertQuery query = new InsertQuery("tvp_elisa.dbo.UserVote");
                        query += ODBCWrapper.Parameter.NEW_PARAM("MEDIA", mediaId);
                        query += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", siteGuid);
                        query += ODBCWrapper.Parameter.NEW_PARAM("SCORE", GetVotingRatio(siteGuid, groupID, platform));
                        query += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "1");
                        query += ODBCWrapper.Parameter.NEW_PARAM("VOTE_DATE", DateTime.Now);
                        query += ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", platform);

                        if (!query.Execute())
                        {
                            logger.ErrorFormat("Failed voting on media, ItemID:{0}.", mediaId);
                            res = "Failure";
                        }

                        query.Finish();
                        query = null;

                        logger.InfoFormat("Vote was registered successfully, ItemID:{0}.", mediaId);
                        return "Success";
                    }
                    else
                    {
                        logger.InfoFormat("Double Vote was not registered, ItemID:{0}.", mediaId);
                        return "DoubleVote";
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

        public static bool IsAlreadyVoted(string mediaId, string siteGuid)
        {
            bool res = false;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select STATUS, VOTE_DATE from tvp_elisa.dbo.UserVote where";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("UserIdentifier", "=", UsersService.Instance.GetUserID());
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", siteGuid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA", "=", mediaId);
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
                    if (((oCurrentDate.Year - oVoteDate.Year) > 0) || ((oCurrentDate.Month - oVoteDate.Month) > 0))
                    {
                        res = false;
                    }
                    else
                    {
                        res = true;
                    }
                }
                else
                {
                    res = false;
                }
            }
            else
            {
                res = false;
            }

            return res;
        }

        public static Int32 GetVotingRatio(string sSiteGuid, int groupID, PlatformType platform)
        {
            UserCAStatus oUserCAStatus = new ApiConditionalAccessService(groupID, platform).GetUserCAStatus(sSiteGuid);

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
    }
}
