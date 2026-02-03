using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ConfigurationManager;
using KLogMonitor;
using TVinciShared;


namespace ApiLogic.Authorization
{
    public class SessionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string APP_TOKEN_PRIVILEGE_SESSION_ID = "sessionid";
        private const string APP_TOKEN_PRIVILEGE_APP_TOKEN = "apptoken";
        private const string CB_SECTION_NAME = "tokens";

        private const string USERS_SESSIONS_KEY_FORMAT = "sessions_{0}";
        private const string REVOKED_KS_KEY_FORMAT = "r_ks_{0}";
        private const string REVOKED_SESSION_KEY_FORMAT = "r_session_{0}";


        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
        

       

        public static bool UpdateUsersSessionsRevocationTime(string groupUserSessionsKeyFormat,
            int groupAppTokenSessionMaxDurationSeconds,
            long groupKSExpirationSeconds,
            string userId, 
            string udid,
            int revocationTime, 
            int expiration,
            bool revokeAll = false)
        {
            if (!string.IsNullOrEmpty(userId) && userId != "0")
            {
                string userSessionsKeyFormat = GetUserSessionsKeyFormat(groupUserSessionsKeyFormat);

                // get user sessions from CB
                string userSessionsCbKey = string.Format(userSessionsKeyFormat, userId);

                ulong version;
                UserSessions usersSessions = cbManager.GetWithVersion<UserSessions>(userSessionsCbKey, out version, true);

                // if not found create one
                if (usersSessions == null)
                {
                    usersSessions = new UserSessions()
                    {
                        UserId = userId,
                    };
                }

                // calculate new expiration
                usersSessions.expiration = Math.Max(usersSessions.expiration, expiration);

                if (revokeAll)
                {
                    usersSessions.UserRevocation = revocationTime;

                    long now = DateUtils.GetUtcUnixTimestampNow();
                    usersSessions.expiration = Math.Max(Math.Max(usersSessions.expiration, (int)(now + groupKSExpirationSeconds)), 
                        (int)now + groupAppTokenSessionMaxDurationSeconds);
                }
                else
                {
                    if (!string.IsNullOrEmpty(udid))
                    {
                        if (usersSessions.UserWithUdidRevocations.ContainsKey(udid))
                        {
                            usersSessions.UserWithUdidRevocations[udid] = revocationTime;
                        }
                        else
                        {
                            usersSessions.UserWithUdidRevocations.Add(udid, revocationTime);
                        }
                    }
                }

                // store
                if (!cbManager.SetWithVersion(userSessionsCbKey, usersSessions, version, (uint)(usersSessions.expiration - DateUtils.GetUtcUnixTimestampNow()), true))
                {
                    log.ErrorFormat("LogOut: failed to set UserSessions in CB, key = {0}", userSessionsCbKey);
                    return false;
                }
            }
            return true;
        }

        public static string GetUserSessionsKeyFormat(string userSessionsKeyFormat)
        {
            
            if (string.IsNullOrEmpty(userSessionsKeyFormat))
            {
                userSessionsKeyFormat = ApplicationConfiguration.Current.AuthorizationManagerConfiguration.UsersSessionsKeyFormat.Value;

                if (string.IsNullOrEmpty(userSessionsKeyFormat))
                {
                    userSessionsKeyFormat = USERS_SESSIONS_KEY_FORMAT;
                }
            }

            return userSessionsKeyFormat;
        }

    }
}
