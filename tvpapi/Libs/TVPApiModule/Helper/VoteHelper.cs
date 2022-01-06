using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects;
using TVPApi.ODBCWrapper;
using TVPPro.SiteManager.Services;
using TVPApi;
using TVPApiModule.Services;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Helpers;
using System.Data;
using Phx.Lib.Log;
using System.Reflection;
using ApiObjects.ConditionalAccess;

namespace TVPApiModule.Helper
{
    public static class VotesHelper
    {
        /// <summary>
        /// Holds the logger
        /// </summary>
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public static string UserVote(string mediaId, string siteGuid, PlatformType platform, int groupID)
        {
            string res = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    logger.InfoFormat("Adding new vote to media, ItemID:{0}.", mediaId);
                    if (!IsAlreadyVoted(mediaId, siteGuid, groupID, platform))
                    {
                        ConnectionManager connMng = new ConnectionManager(groupID, platform, false);
                        TVPApi.ODBCWrapper.Connection.GetDefaultConnectionStringMethod = delegate() { return connMng.GetClientConnectionString(); };
                        InsertQuery query = new InsertQuery("tvp_elisa.dbo.UserVote");
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("MEDIA", mediaId);
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", siteGuid);
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("SCORE", GetVotingRatio(siteGuid, groupID, platform));
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("STATUS", "1");
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("VOTE_DATE", DateTime.Now);
                        query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("PLATFORM", platform);

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

            //return res;
        }

        public static bool IsAlreadyVoted(string mediaId, string siteGuid, int groupID, PlatformType platform)
        {
            bool res = false;

            ConnectionManager connMng = new ConnectionManager(groupID, platform, false);
            DataTable dt = new DataTable("UserVote");
            new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            {
                query.SetConnectionString(connMng.GetClientConnectionString());
                query += "select STATUS, VOTE_DATE from tvp_elisa.dbo.UserVote where";
                //selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("UserIdentifier", "=", UsersService.Instance.GetUserID());
                query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", siteGuid);
                //query += " and ";
                //query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("MEDIA", "=", mediaId);
                query += " and ";
                query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                query += " order by VOTE_DATE desc ";
            }, dt).Execute();



            if (dt.Rows.Count > 0)
            {
                string sVoteDate = dt.Rows[0]["VOTE_DATE"].ToString();
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

        public static List<TVPApiModule.Objects.UserVote> GetAllVotesByDates(long unixTimeStart, long unixTimeEnd, int groupID, PlatformType platform)
        {
            List<TVPApiModule.Objects.UserVote> retVal = new List<UserVote>();

            ConnectionManager connMng = new ConnectionManager(groupID, platform, false);

            DataTable dt;
            TVPApi.ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery(connMng.GetClientConnectionString());
            query += "select * from tvp_elisa.dbo.UserVote where";
            //selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("UserIdentifier", "=", UsersService.Instance.GetUserID());
            query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("VOTE_DATE", ">=", WSUtils.FromUnixTime(unixTimeStart));
            query += " and ";
            query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("VOTE_DATE", "<=", WSUtils.FromUnixTime(unixTimeEnd));
            query += " and ";
            query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            query += " order by VOTE_DATE desc ";
            dt = query.Execute("query", true);
            query.Finish();

            if (dt != null)
            {
                retVal.AddRange(
                    from DataRow row in dt.Rows
                    select new UserVote()
                                    {
                                        MediaID = row["MEDIA"].ToString(),
                                        Platform = row["PLATFORM"].ToString(),
                                        Score = int.Parse(row["SCORE"].ToString()),
                                        SiteGUID = row["SITE_GUID"].ToString(),
                                        Time = (DateTime)row["VOTE_DATE"]
                                    });
            }

            return retVal;
        }

        public static int GetVotesByMediaID(long mediaId, int groupID, PlatformType platform)
        {
            int retVal = 0;

            ConnectionManager connMng = new ConnectionManager(groupID, platform, false);

            DataTable dt;
            TVPApi.ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery(connMng.GetClientConnectionString());
            query += "select sum(score) from tvp_elisa.dbo.UserVote where";
            //selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("UserIdentifier", "=", UsersService.Instance.GetUserID());
            query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("Media", "=", mediaId.ToString());
            query += " and ";
            query += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            dt = query.Execute("query", true);
            query.Finish();

            if (dt != null)
            {
                int.TryParse(dt.Rows[0][0].ToString(), out retVal);
            }

            return retVal;
        }
    }
}
