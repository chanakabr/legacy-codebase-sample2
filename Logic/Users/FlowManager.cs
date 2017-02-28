using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.Response;

namespace Core.Users
{
    public class FlowManager
    {
        public static UserResponseObject SignIn(Int32 siteGuid, KalturaBaseUsers user, int maxFailCount,
                                                int lockMin, int groupId, string sessionId, string ip, string deviceId, bool preventDoubleLogin,
                                                List<KeyValuePair> keyValueList, string username = null, string password = null)
        {
            UserResponseObject response = new UserResponseObject();

            try
            {
                // pre
                response = user.PreSignIn(ref siteGuid, ref username, ref password, ref maxFailCount, ref lockMin, ref groupId,
                                          ref sessionId, ref ip, ref deviceId, ref preventDoubleLogin, ref keyValueList);

                if (response.m_RespStatus == ResponseStatus.OK)
                {
                    // mid
                    response = user.MidSignIn(siteGuid, username, password, maxFailCount, lockMin, groupId,
                                          sessionId, ip, deviceId, preventDoubleLogin);

                    // post
                    user.PostSignIn(ref response, ref keyValueList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }

        public static UserResponseObject SignOut(KalturaBaseUsers user, int siteGuid, int groupId, string sessionId, string ip, string deviceUdid, List<KeyValuePair> keyValueList)
        {
            UserResponseObject userResponse = new UserResponseObject();

            try
            {
                // pre
                userResponse = user.PreSignOut(ref siteGuid, ref groupId, ref sessionId, ref ip, ref deviceUdid, ref keyValueList);

                // mid
                userResponse = user.MidSignOut(siteGuid, groupId, sessionId, ip, deviceUdid);

                // post
                user.PostSignOut(ref userResponse, siteGuid, groupId, sessionId, ip, deviceUdid, ref keyValueList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userResponse;
        }

        public static UserResponseObject AddNewUser(KalturaBaseUsers user, UserBasicData basicData, UserDynamicData dynamicData, string password, List<KeyValuePair> keyValueList, DomainInfo domainInfo = null)
        {
            UserResponseObject response = new UserResponseObject();

            try
            {
                // pre
                response = user.PreAddNewUser(ref basicData, ref dynamicData, ref password, ref domainInfo, ref keyValueList);
                if (response.m_RespStatus == ResponseStatus.OK)
                {
                    // mid
                    response = user.MidAddNewUser(basicData, dynamicData, password, ref keyValueList, domainInfo);

                    // post
                    user.PostAddNewUser(ref response, ref keyValueList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public static int SaveUser(ref UserResponseObject userResponse, KalturaBaseUsers user, ref UserBasicData basicData, User userBo, Int32 groupId, bool bIsSetUserActive, List<KeyValuePair> keyValueList)
        {
            int userId = 0;

            try
            {
                // pre
                user.PreSaveUser(ref userResponse, ref basicData, userBo, groupId, bIsSetUserActive, ref keyValueList);

                // mid
                userId = user.MidSaveUser(ref userResponse, ref basicData, userBo, groupId, bIsSetUserActive);

                // post 
                user.PostSaveUser(ref userResponse, ref basicData, userBo, groupId, bIsSetUserActive, userId, ref keyValueList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userId;
        }

        public static bool SendWelcomeMailRequest(ref UserResponseObject userResponse, KalturaBaseUsers user, User newUser, string password, List<KeyValuePair> keyValueList)
        {
            bool mailSent = false;
            WelcomeMailRequest mailRequest = new WelcomeMailRequest();

            try
            {
                if (((KalturaUsers)user).ShouldSendWelcomeMail)
                {
                    // init
                    user.InitSendWelcomeMail(ref userResponse, ref mailRequest, newUser.m_oBasicData.m_sFirstName, newUser.m_oBasicData.m_sUserName, password, newUser.m_oBasicData.m_sEmail, newUser.m_oBasicData.m_sFacebookID);

                    // pre
                    user.PreSendWelcomeMail(ref userResponse, ref mailRequest, newUser.m_oBasicData.m_sFirstName, newUser.m_oBasicData.m_sUserName, password, newUser.m_oBasicData.m_sEmail, newUser.m_oBasicData.m_sFacebookID, ref keyValueList);

                    // mid
                    mailSent = user.MidSendWelcomeMail(ref userResponse, mailRequest);

                    // post
                    user.PostSendWelcomeMail(ref userResponse, mailSent, ref keyValueList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mailSent;
        }

        public static bool AddDomain(ref UserResponseObject userResponse, KalturaBaseUsers user, User userBo, string username, int userId, DomainInfo domainInfo, List<KeyValuePair> keyValueList)
        {
            bool passed = false;

            try
            {
                // pre
                user.PreAddDomain(ref userResponse, ref userBo, ref username, ref userId, ref domainInfo, ref keyValueList);

                // mid
                passed = user.MidAddDomain(ref userResponse, userBo, username, userId, domainInfo);

                // post 
                user.PostAddDomain(passed, ref userResponse, userBo, username, userId, domainInfo, ref keyValueList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return passed;
        }

        public static bool CreateDefaultRules(ref UserResponseObject userResponse, KalturaBaseUsers user, User userBo, string siteGuid, int groupId, List<KeyValuePair> keyValueList)
        {
            bool passed = false;

            try
            {
                if (((KalturaUsers)user).ShouldCreateDefaultRules)
                {
                    // pre
                    user.PreDefaultRules(ref userResponse, siteGuid, groupId, ref userBo, ref keyValueList);

                    // mid
                    passed = user.MidCreateDefaultRules(ref userResponse, siteGuid, groupId, ref userBo);

                    // post
                    user.PostDefaultRules(ref userResponse, passed, siteGuid, groupId, ref userBo, ref keyValueList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return passed;
        }

        public static bool SubscribeToNewsLetter(ref UserResponseObject userResponse, KalturaBaseUsers user, UserDynamicData dynamicData, User userBo, List<KeyValuePair> keyValueList)
        {
            bool passed = false;

            try
            {
                bool shouldSubscribe = ((KalturaUsers)user).ShouldSubscribeNewsLetter;
                if (((KalturaUsers)user).ShouldSubscribeNewsLetter)
                {
                    // init
                    user.InitSubscribeToNewsLetter(ref userResponse, ref dynamicData, ref userBo, ref shouldSubscribe);

                    // pre
                    user.PreSubscribeToNewsLetter(ref userResponse, ref dynamicData, ref userBo, ref shouldSubscribe, ref keyValueList);

                    // mid
                    passed = user.MidSubscribeToNewsLetter(ref userResponse, dynamicData, userBo, ref shouldSubscribe);

                    // post
                    user.PostSubscribeToNewsLetter(ref userResponse, passed, ref dynamicData, ref userBo, ref keyValueList);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return passed;
        }

        public static UserResponseObject GetUserData(KalturaBaseUsers user, string siteGuid, List<KeyValuePair> keyValueList, string userIP)
        {
            UserResponseObject userResponse = new UserResponseObject();

            try
            {
                // pre
                userResponse = user.PreGetUserData(siteGuid, ref keyValueList, userIP);

                // mid
                user.MidGetUserData(ref userResponse, siteGuid, userIP);

                // post
                user.PostGetUserData(ref userResponse, siteGuid, ref keyValueList, userIP);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userResponse;
        }

        public static List<UserResponseObject> GetUsersData(KalturaBaseUsers user, List<string> siteGuids, List<KeyValuePair> keyValueList, string userIP)
        {
            List<UserResponseObject> userResponses = new List<UserResponseObject>();

            try
            {
                // pre
                userResponses = user.PreGetUsersData(siteGuids, ref keyValueList, userIP);

                // mid
                user.MidGetUsersData(ref userResponses, siteGuids, ref keyValueList, userIP);

                // post
                user.PostGetUsersData(ref userResponses, siteGuids, ref keyValueList, userIP);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userResponses;
        }


        public static ApiObjects.Response.Status DeleteUser(Int32 siteGuid, KalturaBaseUsers user)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            try
            {
                // pre
                response = user.PreDeleteUser(siteGuid);

                if (response.Code == (int)eResponseStatus.OK)
                {
                    // mid
                    response = user.MidDeleteUser(siteGuid);

                    // post
                    user.PostDeleteUser(ref response);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }
    }
}
