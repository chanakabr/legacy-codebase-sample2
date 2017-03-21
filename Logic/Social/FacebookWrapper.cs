using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using TVinciShared;
using System.Configuration;
using System.Data;
using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using Core.Billing;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects.Social;
using ApiObjects.Billing;

namespace Core.Social
{
    public class FacebookWrapper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public readonly string S_PRIVACY_SETINGS_JSON_EVERYONE = "{'value':'EVERYONE'}";
        static public readonly string S_PRIVACY_SETINGS_JSON_SELF = "{'value':'SELF'}";
        static public readonly string S_PRIVACY_SETINGS_JSON_ALL_FRIENDS = "{'value':'ALL_FRIENDS'}";
        static public readonly string S_PRIVACY_SETINGS_JSON_FRIENDS_OF_FRIENDS = "{'value':'FRIENDS_OF_FRIENDS'}";

        protected static readonly int STATUS_OK = 200;

        protected int m_nGroupID;
        protected BaseSocialBL m_oSocialBL;
        protected FacebookConfig m_oFBConfig;
        protected string m_sStaging;
        public FacebookWrapper(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;
            m_oFBConfig = FacebookManager.GetInstance.GetFacebookConfigInstance(m_nGroupID);
        }

        /*
         * Updates friends in Tvinci custom list on Facebook
         */
        public bool UpdateFBCustomFriendsList(int nSiteGuid, string sListName)
        {
            bool bRes = false;

            string sFbUserFriends = string.Empty;
            string sFbTvinciCustomList = string.Empty;
            UserResponseObject uObj = Utils.GetUserDataByID(nSiteGuid.ToString(), m_nGroupID);
            if (uObj == null || uObj.m_user == null || uObj.m_user.m_oBasicData == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookID)
                || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return bRes;
            }


            string sFriendListId = string.Empty;
            bool bFriendListEmpty = false;

            //Get all friend ids from Tvinci's facebook list
            GetFBListId(nSiteGuid.ToString(), sListName, ref sFriendListId);

            //if friends list id doesn't exist, create such a new custom Tvinci friend list on  Facebook
            if (string.IsNullOrEmpty(sFriendListId))
            {
                bFriendListEmpty = true;
                CreateTvinciFBFriendsList(uObj.m_user.m_oBasicData.m_sFacebookToken, sListName, ref sFriendListId);
            }

            if (!bFriendListEmpty || !string.IsNullOrEmpty(sFriendListId))
            {
                bRes = true;
                List<string> userFBFriends;
                int nFacebookFriends = GetUserFriendsFBId(nSiteGuid, out userFBFriends);
                if (nFacebookFriends > 0)
                {
                    sFbUserFriends = userFBFriends.Aggregate((current, next) => current + "," + next);
                    bRes = (AddUsersToFriendsList(uObj.m_user.m_oBasicData.m_sFacebookToken, sFriendListId, sFbUserFriends) == true) ? true : false;
                }
            }
            return bRes;
        }

        public bool UpdateFBCustomFriendsList(int nSiteGuid, string sListName, string sFriendsInList)
        {
            bool bRes = false;

            string sFbTvinciCustomList = string.Empty;
            UserResponseObject uObj = Utils.GetUserDataByID(nSiteGuid.ToString(), m_nGroupID);
            if (uObj == null || uObj.m_user == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookID)
                || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return bRes;
            }


            string sFriendListId = string.Empty;

            //Get friends list id
            GetFBListId(nSiteGuid.ToString(), sListName, ref sFriendListId);

            //if friends list id doesn't exist, create such a new custom Tvinci friend list on  Facebook
            if (string.IsNullOrEmpty(sFriendListId))
            {
                CreateTvinciFBFriendsList(uObj.m_user.m_oBasicData.m_sFacebookToken, sListName, ref sFriendListId);
            }

            if (!string.IsNullOrEmpty(sFriendListId))
            {
                bRes = (AddUsersToFriendsList(uObj.m_user.m_oBasicData.m_sFacebookToken, sFriendListId, sFriendsInList) == true) ? true : false;
            }
            return bRes;
        }

        public string FriendsInFriendsList(string sSiteGuid, string sListName)
        {
            string sFriendsList = string.Empty;
            string fbListID = string.Empty;

            UserResponseObject uObj = Utils.GetUserDataByID(sSiteGuid, m_nGroupID);
            if (uObj == null || uObj.m_user == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookID)
                || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return sFriendsList;
            }

            GetFBListId(sSiteGuid, sListName, ref fbListID);

            if (!string.IsNullOrEmpty(fbListID))
            {
                string sRetVal = string.Empty;
                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sDecryptToken = Utils.Decrypt(uObj.m_user.m_oBasicData.m_sFacebookToken, key);
                int nStatus = FBUtils.GetGraphApiAction(string.Format("{0}/members?fields=id,name&limit=500", fbListID), string.Empty, sDecryptToken, ref sRetVal);

                if (nStatus == STATUS_OK)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    FBFriendsContainer friends = serializer.Deserialize<FBFriendsContainer>(sRetVal);
                    if (friends != null && friends.data != null)
                    {
                        List<FBUser> friendsList = friends.data.ToList<FBUser>();
                        CommaDelimitedStringCollection friendCSV = new CommaDelimitedStringCollection();

                        foreach (var friend in friendsList)
                        {
                            friendCSV.Add(friend.id);
                        }

                        sFriendsList = friendCSV.ToString();
                    }
                }
            }

            return sFriendsList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sAccessToken">Token MUST be decrypted!</param>
        /// <param name="sFriendsListId"></param>
        /// <param name="sListMemebers"></param>
        /// <returns></returns>
        private bool AddUsersToFriendsList(string sAccessToken, string sFriendsListId, string sListMemebers)
        {
            string sRetVal = string.Empty;
            Int32 nStatus = 0;

            StringBuilder sb = new StringBuilder();
            sb.Append(Utils.GetValFromConfig("FB_GRAPH_URI"));
            sb.AppendFormat("/{0}/members?members={1}", sFriendsListId, sListMemebers);

            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            string sDecryptToken = Utils.Decrypt(sAccessToken, key);

            sb.AppendFormat("&access_token={0}", sDecryptToken);

            string sUrl = sb.ToString();

            sRetVal = Utils.SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, string.Empty);

            return (nStatus == STATUS_OK) ? true : false;
        }

        /// <summary>
        /// Get app default privacy settings chosen by user
        /// </summary>
        /// <param name="sAccessToken">Token should be encrypted</param>
        /// <returns></returns>
        public FBPrivacySetting GetFBAppDefaultPrivacySettings(string sSiteGuid, int nGroupID)
        {
            FBPrivacySetting oPrivacySettings = null;
            string sRetVal = string.Empty;

            UserResponseObject uObj = Utils.GetUserDataByID(sSiteGuid, nGroupID);

            if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return oPrivacySettings;
            }


            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            string token = uObj.m_user.m_oBasicData.m_sFacebookToken;
            string sDecryptToken = Utils.Decrypt(token, key);

            Int32 nStatus = FBUtils.GetGraphApiAction("fql?q=SELECT value FROM privacy_setting WHERE name='default_stream_privacy'", string.Empty, sDecryptToken, ref sRetVal);

            if (nStatus != STATUS_OK)
            {
                return oPrivacySettings;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FBPrivacySettingContainer oPrivacySettingContainer = serializer.Deserialize<FBPrivacySettingContainer>(sRetVal);
            if (oPrivacySettingContainer != null && oPrivacySettingContainer.data != null)
            {
                List<FBPrivacySetting> lPrivacySettings = oPrivacySettingContainer.data.ToList<FBPrivacySetting>();
                if (lPrivacySettings != null && lPrivacySettings.Count > 0)
                {
                    oPrivacySettings = lPrivacySettings[0];
                }
            }

            return oPrivacySettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sAccessToken">Token should be encrypted</param>
        /// <returns></returns>
        public eSocialPrivacy[] GetFBAvailablePrivacyGroups(string sSiteGuid, int nGroupID, string sCustomListName = "")
        {
            List<eSocialPrivacy> lSocialPrivacy = new List<eSocialPrivacy>() { eSocialPrivacy.SELF };

            FBPrivacySetting oPrivacySettings = GetFBAppDefaultPrivacySettings(sSiteGuid, nGroupID);

            if (oPrivacySettings != null)
            {
                if (oPrivacySettings.value == eSocialPrivacy.EVERYONE.ToString() || oPrivacySettings.value == eSocialPrivacy.CUSTOM.ToString())
                {
                    lSocialPrivacy.Add(eSocialPrivacy.EVERYONE);
                    lSocialPrivacy.Add(eSocialPrivacy.FRIENDS_OF_FRIENDS);
                    lSocialPrivacy.Add(eSocialPrivacy.ALL_FRIENDS);
                }
                else if (oPrivacySettings.value == eSocialPrivacy.FRIENDS_OF_FRIENDS.ToString())
                {
                    lSocialPrivacy.Add(eSocialPrivacy.FRIENDS_OF_FRIENDS);
                    lSocialPrivacy.Add(eSocialPrivacy.ALL_FRIENDS);
                }
                else if (oPrivacySettings.value == eSocialPrivacy.ALL_FRIENDS.ToString())
                {
                    lSocialPrivacy.Add(eSocialPrivacy.ALL_FRIENDS);
                }
            }
            if (!string.IsNullOrEmpty(sCustomListName))
            {
                string friendsListId = string.Empty;
                GetFBListId(sSiteGuid, sCustomListName, ref friendsListId);
                if (!string.IsNullOrEmpty(friendsListId))
                {
                    lSocialPrivacy.Add(eSocialPrivacy.CUSTOM);
                }
            }

            return lSocialPrivacy.ToArray();
        }

        /// <summary>
        /// Creates a new custom friends list on facebook (This list will contain only FB friends that have the appropriate Tvinci app)
        /// </summary>
        /// <param name="sAccessToken"></param>
        /// <param name="sListName"></param>
        /// <param name="sListId"></param>
        public void CreateTvinciFBFriendsList(string sAccessToken, string sListName, ref string sListId)
        {
            sListId = string.Empty;

            string sRetVal = string.Empty;
            Int32 nStatus = 0;

            StringBuilder sb = new StringBuilder();
            sb.Append(Utils.GetValFromConfig("FB_GRAPH_URI"));
            sb.AppendFormat("/me/FriendLists?name={0}", sListName);

            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            string sDecryptToken = Utils.Decrypt(sAccessToken, key);

            sb.AppendFormat("&access_token={0}", sDecryptToken);

            string sUrl = sb.ToString();


            sRetVal = Utils.SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, string.Empty);

            if (nStatus == STATUS_OK)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBFriendsList friendList = serializer.Deserialize<FBFriendsList>(sRetVal);

                if (friendList != null)
                {
                    sListId = friendList.flid;
                }
            }
        }

        public FBUser GetUserDetails(string sAccessToken)
        {
            FBUser oRes = null;
            string sRetVal = string.Empty;

            int nStatus = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, sAccessToken, ref sRetVal);

            if (nStatus == STATUS_OK)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                oRes = serializer.Deserialize<FBUser>(sRetVal);
            }

            return oRes;
        }


        public string[] GetUserFriends(int nUserGUID)
        {
            List<string> friends;

            GetUserFriendsGuid(nUserGUID, out friends);

            string[] result = (friends == null) ? new string[] { } : friends.ToArray();

            return friends.ToArray();
        }

        public bool GetUserFriendsGuid(int nUserGUID, out List<string> lFriendsList)
        {
            string sFBToken = string.Empty;
            int nTvincifriends = 0;
            lFriendsList = null;
            bool bFriendsResult = false;

            Core.Social.Cache.SocialCache cache = Core.Social.Cache.SocialCache.Instance;

            if (cache != null)
            {
                lFriendsList = cache.Get<List<string>>(string.Format("{0}_friendslist", nUserGUID));

                if (lFriendsList != null)
                    bFriendsResult = true;
            }

            if (lFriendsList == null)
            {
                lFriendsList = new List<string>();

                try
                {
                    UserResponseObject uObj = Utils.GetUserDataByID(nUserGUID.ToString(), m_nGroupID);
                    if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK || uObj.m_user == null || uObj.m_user.m_oBasicData == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
                    {
                        return false;
                    }

                    string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                    sFBToken = Utils.Decrypt(uObj.m_user.m_oBasicData.m_sFacebookToken, key);

                    List<FBUser> lTempList;
                    int nNumOfFriends;

                    bFriendsResult = GetFriendsList("me", sFBToken, out nNumOfFriends, out lTempList);

                    if (bFriendsResult == true && lTempList != null && lTempList.Count > 0)
                    {
                        List<long> fbIDList = new List<long>();
                        foreach (var item in lTempList)
                        {
                            long temp;
                            if (long.TryParse(item.id, out temp))
                            {
                                fbIDList.Add(temp);
                            }
                        }
                        List<FBUser> friendsList = m_oSocialBL.GetFBFriendsFromDB(fbIDList);

                        foreach (FBUser user in friendsList)
                        {
                            if (!string.IsNullOrEmpty(user.m_sSiteGuid))
                            {
                                lFriendsList.Add(user.m_sSiteGuid);
                                nTvincifriends++;
                            }
                        }

                        cache.Set(string.Format("{0}_friendslist", nUserGUID), lFriendsList);
                    }
                }
                catch
                {
                }
            }

            return bFriendsResult;
        }

        public bool GetFriendsList(string sID, string sFBToken, out int numOfFriends, out List<FBUser> lFriendsList)
        {
            lFriendsList = new List<FBUser>();

            bool bResult = false;
            string sRetVal = string.Empty;
            numOfFriends = 0;

            Int32 nStatus = FBUtils.GetGraphApiAction(sID, "friends", sFBToken, ref sRetVal);
            if (nStatus != STATUS_OK)
            {
                return bResult;
            }

            bResult = true;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FBFriendsContainer friends = serializer.Deserialize<FBFriendsContainer>(sRetVal);
            if (friends != null && friends.data != null)
            {
                lFriendsList.AddRange(friends.data);
                numOfFriends = lFriendsList.Count;
            }
            return bResult;
        }

        public bool DeleteFBObject(string sObjectID)
        {
            string sRetVal = string.Empty;
            string sAppToken = m_oFBConfig.AppSecret;
            int nStatus = FBUtils.DeleteObject(sAppToken, sObjectID, ref sRetVal);

            return (nStatus == STATUS_OK) ? true : false;
        }

        public int GetUserFriendsFBId(int nUserGUID, out List<string> lFriendsList)
        {
            string sFBToken = string.Empty;
            int nTvincifriends = 0;
            lFriendsList = new List<string>();

            UserResponseObject uObj = Utils.GetUserDataByID(nUserGUID.ToString(), m_nGroupID);
            if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK || uObj.m_user == null || uObj.m_user.m_oBasicData == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return 0;
            }

            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            sFBToken = Utils.Decrypt(uObj.m_user.m_oBasicData.m_sFacebookToken, key);

            List<FBUser> lTempList;
            int nNumOfFriends;

            bool bFriendsList = GetFriendsList("me", sFBToken, out nNumOfFriends, out lTempList);

            if (nNumOfFriends == 0)
            {
                return 0;
            }
            else
            {
                List<long> fbIDList = new List<long>();
                foreach (var item in lTempList)
                {
                    long temp;
                    if (long.TryParse(item.id, out temp))
                    {
                        fbIDList.Add(temp);
                    }
                }
                List<FBUser> friendsList = m_oSocialBL.GetFBFriendsFromDB(fbIDList);
                CommaDelimitedStringCollection csvFriendList = new CommaDelimitedStringCollection();

                foreach (FBUser user in friendsList)
                {
                    lFriendsList.Add(user.id);
                    nTvincifriends++;
                }
            }

            return nTvincifriends;
        }

        //Returns users GUID by using FQL and getting all app users who are the user's friend
        public int GetUserFriendsGuidFromFB(int nUserGUID, int nGroupID, ref string sFriendsList)
        {
            string sDecryptToken = string.Empty;
            int nTvincifriends = 0;

            UserResponseObject uObj = Utils.GetUserDataByID(nUserGUID.ToString(), nGroupID);
            if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK || uObj.m_user == null || uObj.m_user.m_oBasicData == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return 0;
            }

            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            sDecryptToken = Utils.Decrypt(uObj.m_user.m_oBasicData.m_sFacebookToken, key);

            string sRetVal = string.Empty;
            Int32 nStatus = FBUtils.GetGraphApiAction("fql?q=SELECT uid name first_name last_name sex email FROM user WHERE is_app_user AND uid IN (SELECT uid2 FROM friend WHERE uid1 = me())", string.Empty, sDecryptToken, ref sRetVal);

            if (nStatus == STATUS_OK)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBFriendsContainer lFBFriends = serializer.Deserialize<FBFriendsContainer>(sRetVal);
                if (lFBFriends != null)
                {
                    List<string> lFBFriendsID = new List<string>();

                    foreach (FBUser user in lFBFriends.data)
                    {
                        lFBFriendsID.Add(user.id);
                    }
                    nTvincifriends = lFBFriendsID.Count;
                    sFriendsList = lFBFriendsID.Aggregate((current, next) => current + "," + next);
                }
            }

            return nTvincifriends;
        }

        public string CreateFBProgramObject(int nProgID, string sUrl)
        {
            string sRes = string.Empty;
            string sAppAccessToken = m_oFBConfig.AppSecret;

            //call catalog service for details 
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = Utils.GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
            request.m_lProgramsIds = new List<int> { nProgID };
            int nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
            request.m_nGroupID = nGroupID;
            request.m_nPageIndex = 0;
            request.m_nPageSize = 0;
            request.m_sSignature = sSignature;
            request.m_sSignString = sSignString;

            EpgProgramResponse response = (EpgProgramResponse)request.GetProgramsByIDs(request);
            EPGChannelProgrammeObject progObject = null;
            if (response != null && response.m_nTotalItems > 0)
            {
                if (response.m_lObj[0] != null)
                {
                    ProgramObj programObj = response.m_lObj[0] as ProgramObj;
                    progObject = programObj.m_oProgram;
                }
            }

            //EPGChannelProgrammeObject progObject = m_oSocialBL.GetProgramInfo(nProgID);

            if (progObject != null)
            {
                FBMediaObject fbMediaObject = new FBMediaObject();
                FBObjectData objectData = new FBObjectData();

                if (progObject.EPG_TAGS != null)
                {
                    objectData.tag = new string[progObject.EPG_TAGS.Count()];

                    for (int i = 0; i < progObject.EPG_TAGS.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(progObject.EPG_TAGS[i].Value))
                        {
                            objectData.tag[i] = Uri.EscapeDataString(progObject.EPG_TAGS[i].Value);
                        }
                    }
                }

                string sMediaName = m_oSocialBL.GetMediaName(nProgID);
                objectData.release_date = Utils.ConvertToFBDate(Convert.ToDateTime(progObject.START_DATE));
                fbMediaObject.data = objectData;
                fbMediaObject.title = Uri.EscapeDataString(string.Format("{0}, {1}", progObject.NAME, sMediaName));
                fbMediaObject.description = Uri.EscapeDataString(progObject.DESCRIPTION);
                fbMediaObject.image = (string.IsNullOrEmpty(progObject.PIC_URL)) ? Uri.EscapeUriString(getDefaultPic()) : Uri.EscapeUriString(progObject.PIC_URL);
                fbMediaObject.type = "video.other";
                fbMediaObject.url = string.IsNullOrEmpty(sUrl) ? string.Empty : Uri.EscapeDataString(sUrl);

                sRes = FBUtils.CreateFBObject(fbMediaObject, sAppAccessToken);
            }

            return sRes;
        }

        private string getDefaultPic()
        {
            return "http://www.wallaw.co.il/image/users/57946/ftp/my_files/14-tvinci.gif?id=8296219";
        }

        public string CreateFBMediaObject(int nMediaID, string sUrl)
        {
            string sRes = string.Empty;
            string sAppAccessToken = string.Format("{0}|{1}", m_oFBConfig.sFBKey, m_oFBConfig.sFBSecret);

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = Utils.GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            Filter filter = new Filter();
            filter.m_bOnlyActiveMedia = false;
            filter.m_bUseFinalDate = false;
            filter.m_bUseStartDate = false;
            filter.m_nLanguage = 2;
            filter.m_sDeviceId = "";
            filter.m_sPlatform = "";


            MediasProtocolRequest mediaProtocolRequest = new MediasProtocolRequest();
            mediaProtocolRequest.m_oFilter = filter;
            mediaProtocolRequest.m_nGroupID = m_nGroupID;

            mediaProtocolRequest.m_sSignature = sSignature;
            mediaProtocolRequest.m_sSignString = sSignString;
            mediaProtocolRequest.m_sUserIP = "";
            mediaProtocolRequest.m_lMediasIds = new List<int> { nMediaID };
            mediaProtocolRequest.m_nPageIndex = 0;
            mediaProtocolRequest.m_nPageSize = 0;

            MediaResponse oMediaResponse = mediaProtocolRequest.GetMediasByIDs(mediaProtocolRequest);


            if (oMediaResponse == null || oMediaResponse.m_lObj == null || oMediaResponse.m_lObj.Count == 0)
            {
                return sRes;
            }

            MediaObj oMedia = oMediaResponse.m_lObj[0] as MediaObj;

            if (oMedia != null)
            {
                FBMediaObject fbMediaObject = new FBMediaObject();
                FBObjectData objectData = new FBObjectData();
                fbMediaObject.title = Uri.EscapeDataString(oMedia.m_sName);
                fbMediaObject.description = Uri.EscapeDataString(oMedia.m_sDescription);

                objectData.release_date = string.Format("{0}-{1}-{2}T{3}:{4}:{5}", oMedia.m_dStartDate.Year.ToString(), oMedia.m_dStartDate.Month.ToString(), oMedia.m_dStartDate.Date.Day.ToString(), oMedia.m_dStartDate.Hour.ToString(), oMedia.m_dStartDate.Minute.ToString(), oMedia.m_dStartDate.Second.ToString());

                fbMediaObject.type = string.Format("video.{0}", GetFBMediaType(oMedia.m_oMediaType).ToString());

                if (oMedia.m_lTags != null)
                {
                    List<string> lTags = new List<string>();
                    foreach (Tags tags in oMedia.m_lTags)
                    {
                        foreach (string tag in tags.m_lValues)
                        {
                            lTags.Add(Uri.EscapeDataString(tag));
                        }
                    }

                    objectData.tag = lTags.ToArray();
                }


                if (oMedia.m_lPicture != null && oMedia.m_lPicture.Count > 0)
                {
                    int nLargestPicIndex = 0;
                    int nLargestPicSize = 1;
                    for (int i = 1; i < oMedia.m_lPicture.Count; i++)
                    {
                        string sPicSize = oMedia.m_lPicture[i].m_sSize;
                        string[] lPicWidthLength = sPicSize.Split('X');
                        int nPicArea = 1;
                        foreach (string sLen in lPicWidthLength)
                        {
                            int nSizeLen;
                            bool bParse = int.TryParse(sLen, out nSizeLen);
                            if (bParse)
                                nPicArea *= nSizeLen;
                        }

                        if (nPicArea > nLargestPicSize)
                            nLargestPicIndex = i;

                    }

                    fbMediaObject.image = Uri.EscapeDataString(oMedia.m_lPicture[nLargestPicIndex].m_sURL);
                }

                fbMediaObject.data = objectData;
                fbMediaObject.url = string.IsNullOrEmpty(sUrl) ? string.Empty : Uri.EscapeDataString(sUrl);

                sRes = FBUtils.CreateFBObject(fbMediaObject, sAppAccessToken);
            }

            return sRes;
        }

        private FB_MediaType GetFBMediaType(MediaType oMediaType)
        {
            string sMediaType = oMediaType.m_sTypeName.ToLower();
            FB_MediaType eMediaType = FB_MediaType.OTHER;
            switch (sMediaType)
            {
                case "episode":
                    eMediaType = FB_MediaType.EPISODE;
                    break;
                case "movie":
                    eMediaType = FB_MediaType.MOVIE;
                    break;
                case "show":
                    eMediaType = FB_MediaType.TV_SHOW;
                    break;
                default:
                    break;

            }

            return eMediaType;
        }

        public FacebookResponse FBUserData(string token)
        {
            UserResponseObject uObj = null;
            return FBUserData(token, ref uObj);
        }

        public FacebookResponse FBUserDataByUserId(string userId)
        {
            FacebookResponse facebookResponse = new FacebookResponse();
            try
            {
                UserResponseObject uObj = Utils.GetUserDataByID(userId, m_nGroupID);

                //User Exists
                if (uObj.m_RespStatus == ResponseStatus.OK)
                {                    
                    if (string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
                    {
                        throw new FacebookException(FacebookResponseStatus.ERROR, "User token is empty");
                    }                    
                    string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                    string sDecryptToken = Utils.Decrypt(uObj.m_user.m_oBasicData.m_sFacebookToken, key);

                    string sRetVal = string.Empty;
                    int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, sDecryptToken, ref sRetVal);

                    if (status != STATUS_OK)
                    {
                        //Error with facebook response
                        throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
                    }

                    //Create FBUser
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);

                    facebookResponse.ResponseData.fbUser = fbUser;
                    facebookResponse.ResponseData.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbUser.id);
                    facebookResponse.ResponseData.facebookName = fbUser.name;

                    facebookResponse.ResponseData.status = FacebookResponseStatus.OK.ToString();
                    facebookResponse.Status.Code = (int)eResponseStatus.OK;

                    facebookResponse.ResponseData.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;
                    facebookResponse.ResponseData.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;

                    facebookResponse.ResponseData.data = Utils.GetEncryptPass(facebookResponse.ResponseData.siteGuid);
                }
                // User Does Not Exists
                else if (uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
                {
                    facebookResponse.ResponseData.status = FacebookResponseStatus.NOTEXIST.ToString();
                    facebookResponse.Status.Code = (int)eResponseStatus.UserDoesNotExist;
                    facebookResponse.ResponseData.siteGuid = string.Empty;
                }
            }
            catch (FacebookException ex)
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.Status.Message = ex.Message;
                facebookResponse.ResponseData.data = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error("Facebook error - error:" + ex.Message, ex);
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.ResponseData.data = ex.Message;
            }

            return facebookResponse;
        }

        public FacebookResponse FBUserData(string token, ref UserResponseObject uObj)
        {
            FacebookResponse facebookResponse = new FacebookResponse();
            try
            {

                if (string.IsNullOrEmpty(token))
                {
                    throw new FacebookException(FacebookResponseStatus.ERROR, "empty token");
                }

                string sRetVal = string.Empty;
                int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, token, ref sRetVal);

                if (status != STATUS_OK)
                {
                    //Error with facebook response
                    throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
                }

                //Create FBUser
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);

                //Search user with facebook id 
                uObj = Utils.GetUserDataByFacebookID(fbUser.id, m_nGroupID);

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sEncryptToken = Utils.Encrypt(token, key);

                facebookResponse.ResponseData.fbUser = fbUser;
                facebookResponse.ResponseData.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbUser.id);
                facebookResponse.ResponseData.facebookName = fbUser.name;

                //User Exists
                if (uObj.m_RespStatus == ResponseStatus.OK)
                {
                    string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;
                    facebookResponse.ResponseData.status = FacebookResponseStatus.OK.ToString();
                    facebookResponse.Status.Code = (int)eResponseStatus.OK;

                    //Update user FBToken
                    if (sFBToken != sEncryptToken)
                    {
                        uObj.m_user.m_oBasicData.m_sFacebookToken = sEncryptToken;
                        UserBasicData userBasicDataToUpdate = new UserBasicData() { m_sFacebookToken = sEncryptToken };
                        uObj = Utils.SetUserData(m_nGroupID, uObj.m_user.m_sSiteGUID, userBasicDataToUpdate, uObj.m_user.m_oDynamicData);
                        facebookResponse.ResponseData.status = uObj.m_RespStatus.ToString().ToUpper();
                        facebookResponse.Status.Code = (int)eResponseStatus.OK;
                    }

                    facebookResponse.ResponseData.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;
                    facebookResponse.ResponseData.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;

                    facebookResponse.ResponseData.data = Utils.GetEncryptPass(facebookResponse.ResponseData.siteGuid);
                }
                // User Does Not Exists
                if (uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
                {

                    if (string.IsNullOrEmpty(fbUser.email))
                    {
                        throw new FacebookException(FacebookResponseStatus.ERROR, "Missing user email");
                    }

                    //serach user by facebook email as username 
                    uObj = Utils.GetUserByUsername(fbUser.email, m_nGroupID);
                    if (uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.NOTEXIST.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.UserDoesNotExist;
                        facebookResponse.ResponseData.siteGuid = string.Empty;
                        facebookResponse.ResponseData.facebookName = fbUser.name;

                        List<FBUser> lFriendsList;
                        Int32 nNumOfFriends;

                        if (m_oFBConfig.nFBMinFriends > 0)
                        {
                            bool bFriendList = GetFriendsList("me", token, out nNumOfFriends, out lFriendsList);

                            if (nNumOfFriends < m_oFBConfig.nFBMinFriends)
                            {
                                facebookResponse.ResponseData.status = FacebookResponseStatus.MINFRIENDS.ToString();
                                facebookResponse.Status.Code = (int)eResponseStatus.OK;
                                facebookResponse.ResponseData.data = nNumOfFriends.ToString();
                                facebookResponse.ResponseData.minFriends = m_oFBConfig.nFBMinFriends.ToString();
                                facebookResponse.ResponseData.facebookName = fbUser.name;
                                facebookResponse.ResponseData.token = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        string sFacebookID = uObj.m_user.m_oBasicData.m_sFacebookID;
                        if (!string.IsNullOrEmpty(sFacebookID))
                        {
                            facebookResponse.ResponseData.status = FacebookResponseStatus.CONFLICT.ToString();
                            facebookResponse.Status.Code = (int)eResponseStatus.OK;
                            facebookResponse.ResponseData.tvinciName = uObj.m_user.m_oBasicData.m_sUserName;
                        }
                        else
                        {
                            facebookResponse.ResponseData.status = FacebookResponseStatus.MERGE.ToString();
                            facebookResponse.Status.Code = (int)eResponseStatus.OK;
                            facebookResponse.ResponseData.siteGuid = uObj.m_user.m_sSiteGUID;
                            facebookResponse.ResponseData.tvinciName = uObj.m_user.m_oBasicData.m_sUserName;
                        }
                    }
                }
            }
            catch (FacebookException ex)
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.ResponseData.data = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error("Facebook error - error:" + ex.Message, ex);
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.ResponseData.data = ex.Message;
            }

            return facebookResponse;
        }

        public FacebookResponse FBUserRegister(string token, List<ApiObjects.KeyValuePair> extra, string sUserIP)
        {
            FacebookResponse facebookResponse = new FacebookResponse();
            try
            {

                if (string.IsNullOrEmpty(token))
                {
                    throw new FacebookException(FacebookResponseStatus.ERROR, "empty token");
                }

                string sRetVal = string.Empty;
                int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, token, ref sRetVal);

                if (status != STATUS_OK)
                {
                    //Error with facebook response
                    throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
                }

                //Create FBUser
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);

                UserResponseObject uObj = new UserResponseObject();

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sEncryptToken = Utils.Encrypt(token, key);

                facebookResponse.ResponseData.fbUser = fbUser;
                facebookResponse.ResponseData.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbUser.id);
                facebookResponse.ResponseData.facebookName = fbUser.name;

                List<FBUser> lFBFriends;
                Int32 nNumOfFreinds;

                bool bFriendsList = GetFriendsList("me", token, out nNumOfFreinds, out lFBFriends);

                if (nNumOfFreinds < m_oFBConfig.nFBMinFriends)
                {
                    facebookResponse.ResponseData.status = FacebookResponseStatus.MINFRIENDS.ToString();
                    facebookResponse.Status.Code = (int)eResponseStatus.MinFriendsLimitation;
                    facebookResponse.ResponseData.data = nNumOfFreinds.ToString();
                    facebookResponse.ResponseData.minFriends = m_oFBConfig.nFBMinFriends.ToString();
                }
                else
                {
                    Core.Users.UserBasicData ubd = Utils.GetFBBasicData(fbUser, sEncryptToken, facebookResponse.ResponseData.pic);

                    if (string.IsNullOrEmpty(ubd.m_sUserName))
                    {
                        throw new FacebookException(FacebookResponseStatus.ERROR, "Missing user email.");
                    }

                    Core.Users.UserDynamicData udd = new Core.Users.UserDynamicData();
                    List<Core.Users.UserDynamicDataContainer> luddc = GetFBDynamicData(fbUser);

                    //NewsLetter
                    string news = Utils.GetValFromKVP(extra, "news");
                    if (news.Equals("1"))
                    {
                        Utils.AddToDynamicData("NewsLetter", "true", ref luddc);
                    }

                    //Mail
                    string mail = Utils.GetValFromKVP(extra, "mail");
                    if (!string.IsNullOrEmpty(mail))
                    {
                        Utils.AddToDynamicData("mailtemplate", mail, ref luddc);
                    }

                    udd.m_sUserData = luddc.ToArray();
                    uObj = Utils.AddNewUser(m_nGroupID, ubd, udd, Utils.GetPassword(), string.Empty);

                    if (uObj.m_RespStatus == ResponseStatus.OK || uObj.m_RespStatus == ResponseStatus.UserExists || uObj.m_RespStatus == ResponseStatus.UserWithNoDomain)
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.NEWUSER.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.OK;
                        facebookResponse.ResponseData.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;
                        facebookResponse.ResponseData.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;
                        facebookResponse.ResponseData.data = Utils.GetEncryptPass(facebookResponse.ResponseData.siteGuid);

                        //Subscription
                        string subID = Utils.GetValFromKVP(extra, "subid");
                        if (!string.IsNullOrEmpty(subID))
                        {
                            string sCouponCode = Utils.GetValFromKVP(extra, "coupon");

                            BillingResponse bObj = Utils.DummyChargeUserForSubscription(m_nGroupID, uObj.m_user.m_sSiteGUID, subID, sCouponCode, sUserIP);
                            if (bObj.m_oStatus != BillingResponseStatus.Success)
                            {
                                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                                m_oSocialBL.RemoveUser(m_nGroupID, uObj.m_user.m_sSiteGUID);
                                return facebookResponse;
                            }
                        }

                        //Domain
                        string domain = Utils.GetValFromKVP(extra, "domain");
                        if (domain.Equals("1"))
                        {
                            DomainResponseObject dObj = Utils.AddNewDomain(m_nGroupID, uObj.m_user);
                            if (dObj.m_oDomainResponseStatus != DomainResponseStatus.OK)
                            {
                                m_oSocialBL.RemoveUser(m_nGroupID, uObj.m_user.m_sSiteGUID);
                                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                                return facebookResponse;
                            }
                        }
                    }
                    else
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.Error;
                    }
                }
            }
            catch (FacebookException ex)
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.ResponseData.data = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error("Facebook error - error:" + ex.Message, ex);

                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
                facebookResponse.ResponseData.data = ex.Message;
            }

            return facebookResponse;
        }

        public bool RemoveUser(string sSiteGuid)
        {
            return m_oSocialBL.RemoveUser(m_nGroupID, sSiteGuid);
        }

        public FacebookResponse FBUserMerge(string token, string fbid, string sUserName, string sPass)
        {
            FacebookResponse facebookResponse = new FacebookResponse();

            UserResponseObject uObj = Utils.CheckUserPassword(m_nGroupID, sUserName, sPass, false);

            if (uObj != null && uObj.m_RespStatus == ResponseStatus.OK)
            {
                UserResponseObject tempUser = null;
                facebookResponse = FBUserData(token, ref tempUser);

                FacebookResponseStatus eRes = FacebookResponseStatus.ERROR;
                if (Enum.TryParse<FacebookResponseStatus>(facebookResponse.ResponseData.status, out eRes))
                {
                    switch (eRes)
                    {
                        case FacebookResponseStatus.OK:
                            {
                                facebookResponse.ResponseData.status = FacebookResponseStatus.CONFLICT.ToString();
                                facebookResponse.Status.Code = (int)eResponseStatus.Conflict;
                                facebookResponse.Status.Message = eResponseStatus.Conflict.ToString();
                                goto case FacebookResponseStatus.ERROR;
                            }
                        case FacebookResponseStatus.CONFLICT:
                        case FacebookResponseStatus.ERROR:
                            {
                                return facebookResponse;
                            }
                        default:
                            break;
                    }
                }

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sEncryptToken = Utils.Encrypt(token, key);

                facebookResponse.ResponseData.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbid);

                string sFacebookID = uObj.m_user.m_oBasicData.m_sFacebookID;
                string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;
                string sFacebookImage = uObj.m_user.m_oBasicData.m_sFacebookImage;

                if (sFBToken != sEncryptToken || sFacebookID != fbid || sFacebookImage != facebookResponse.ResponseData.pic)
                {
                    UserBasicData userBasicDataToUpdate = new UserBasicData() 
                    {
                        m_sFacebookID = fbid,
                        m_sFacebookToken = sEncryptToken,
                        m_sFacebookImage = facebookResponse.ResponseData.pic
                    };
                
                    uObj = Utils.SetUserData(m_nGroupID, uObj.m_user.m_sSiteGUID, userBasicDataToUpdate, uObj.m_user.m_oDynamicData);
                }

                Utils.TryWriteToUserLog("Facebook merged.", m_nGroupID, uObj.m_user.m_sSiteGUID);
                WriteMergeToQueue(uObj.m_user.m_sSiteGUID, "create");

                facebookResponse.ResponseData.status = FacebookResponseStatus.MERGEOK.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.OK;

                facebookResponse.ResponseData.siteGuid = uObj.m_user.m_sSiteGUID;
                facebookResponse.ResponseData.data = Utils.GetEncryptPass(facebookResponse.ResponseData.siteGuid);
                facebookResponse.ResponseData.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;


            }
            else if (uObj.m_RespStatus == ResponseStatus.WrongPasswordOrUserName || uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.WRONGPASSWORDORUSERNAME.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.WrongPasswordOrUserName;
            }
            else
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
            }

            return facebookResponse;
        }

        public FacebookResponse FBUserMergeByUserId(string userId, string token)
        {
            FacebookResponse facebookResponse = new FacebookResponse();

            UserResponseObject uObj = null;

            string sRetVal = string.Empty;
            int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, token, ref sRetVal);

            if (status != STATUS_OK)
            {
                //Error with facebook response
                throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
            }

            //Create FBUser
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);
            string fbid = fbUser.id;

            if(!string.IsNullOrEmpty(userId))
                uObj = Utils.GetUserDataByID(userId, m_nGroupID);

            if (uObj != null && uObj.m_RespStatus == ResponseStatus.OK)
            {
                UserResponseObject tempUser = null;
                facebookResponse = FBUserData(token, ref tempUser);

                FacebookResponseStatus eRes = FacebookResponseStatus.ERROR;
                if (Enum.TryParse<FacebookResponseStatus>(facebookResponse.ResponseData.status, out eRes))
                {
                    switch (eRes)
                    {
                        case FacebookResponseStatus.OK:
                            {
                                facebookResponse.ResponseData.status = FacebookResponseStatus.CONFLICT.ToString();
                                facebookResponse.Status.Code = (int)eResponseStatus.Conflict;
                                facebookResponse.Status.Message = eResponseStatus.Conflict.ToString();
                                goto case FacebookResponseStatus.ERROR;
                            }
                        case FacebookResponseStatus.CONFLICT:
                        case FacebookResponseStatus.ERROR:
                            {
                                return facebookResponse;
                            }
                        default:
                            break;
                    }
                }

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sEncryptToken = Utils.Encrypt(token, key);

                facebookResponse.ResponseData.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbid);

                string sFacebookID = uObj.m_user.m_oBasicData.m_sFacebookID;
                string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;
                string sFacebookImage = uObj.m_user.m_oBasicData.m_sFacebookImage;

                if (sFBToken != sEncryptToken || sFacebookID != fbid || sFacebookImage != facebookResponse.ResponseData.pic)
                {
                    UserBasicData userBasicDataToUpdate = new UserBasicData()
                    {
                        m_sFacebookID = fbid,
                        m_sFacebookToken = sEncryptToken,
                        m_sFacebookImage = facebookResponse.ResponseData.pic
                    };
                    uObj = Utils.SetUserData(m_nGroupID, uObj.m_user.m_sSiteGUID, userBasicDataToUpdate, uObj.m_user.m_oDynamicData);
                }

                Utils.TryWriteToUserLog("Facebook merged.", m_nGroupID, uObj.m_user.m_sSiteGUID);
                WriteMergeToQueue(uObj.m_user.m_sSiteGUID, "create");

                facebookResponse.ResponseData.status = FacebookResponseStatus.MERGEOK.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.OK;

                facebookResponse.ResponseData.siteGuid = uObj.m_user.m_sSiteGUID;
                facebookResponse.ResponseData.data = Utils.GetEncryptPass(facebookResponse.ResponseData.siteGuid);
                facebookResponse.ResponseData.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;


            }
            else if (uObj.m_RespStatus == ResponseStatus.WrongPasswordOrUserName || uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.WRONGPASSWORDORUSERNAME.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.WrongPasswordOrUserName;
            }
            else
            {
                facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
                facebookResponse.Status.Code = (int)eResponseStatus.Error;
            }

            return facebookResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sAccessToken">Token should be encrypted</param>
        /// <param name="sListName"></param>
        /// <param name="friendsListId"></param>
        public void GetFBListId(string sSiteGuid, string sListName, ref string friendsListId)
        {
            friendsListId = string.Empty;

            UserResponseObject uObj = Utils.GetUserDataByID(sSiteGuid, m_nGroupID);
            if (uObj == null || uObj.m_user == null || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookID)
                || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
            {
                return;
            }

            string sRetVal = string.Empty;
            string token = uObj.m_user.m_oBasicData.m_sFacebookToken;
            string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
            string sDecryptToken = Utils.Decrypt(token, key);


            //Int32 nStatus = Utils.GetGraphApiAction("me/FriendLists?fields=id,name", string.Empty, sDecryptToken, ref sRetVal);
            Int32 nStatus = FBUtils.GetGraphApiAction(string.Format("fql?q=SELECT flid,name FROM friendlist WHERE owner = me() and name = \"{0}\"", sListName), string.Empty, sDecryptToken, ref sRetVal);


            if (nStatus != STATUS_OK)
            {
                return;
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FBFriendsListContainer friendListContainer = serializer.Deserialize<FBFriendsListContainer>(sRetVal);
            if (friendListContainer != null)
            {
                List<FBFriendsList> friendLists = friendListContainer.data.ToList<FBFriendsList>();

                foreach (FBFriendsList list in friendLists)
                {
                    if (list.name == sListName)
                    {
                        friendsListId = list.flid;
                        break;
                    }
                }
            }
        }

        public void GetPrivacyGroupJSONString(string sSiteGuid, string sListName, eSocialPrivacy privacySettings, ref string jsonString)
        {
            switch (privacySettings)
            {
                case eSocialPrivacy.EVERYONE:
                    jsonString = S_PRIVACY_SETINGS_JSON_EVERYONE;
                    break;
                case eSocialPrivacy.ALL_FRIENDS:
                    jsonString = S_PRIVACY_SETINGS_JSON_ALL_FRIENDS;
                    break;
                case eSocialPrivacy.FRIENDS_OF_FRIENDS:
                    jsonString = S_PRIVACY_SETINGS_JSON_FRIENDS_OF_FRIENDS;
                    break;
                case eSocialPrivacy.SELF:
                    jsonString = S_PRIVACY_SETINGS_JSON_SELF;
                    break;
                case eSocialPrivacy.CUSTOM:
                    UserResponseObject uObj = Utils.GetUserDataByID(sSiteGuid, m_nGroupID);
                    if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookID) || string.IsNullOrEmpty(uObj.m_user.m_oBasicData.m_sFacebookToken))
                        break;

                    string sListId = string.Empty;

                    GetFBListId(sSiteGuid, sListName, ref sListId);
                    if (!string.IsNullOrEmpty(sListId))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{'value': 'CUSTOM', 'allow': '{");
                        sb.Append(sListId);
                        sb.Append("}'}");
                        jsonString = sb.ToString();
                    }
                    break;
                default:
                    jsonString = string.Empty;
                    break;
            }
        }

        public static List<Core.Users.UserDynamicDataContainer> GetFBDynamicData(FBUser fbUser)
        {
            List<Core.Users.UserDynamicDataContainer> lUserData = new List<Core.Users.UserDynamicDataContainer>();

            Utils.AddToDynamicData("NickName", fbUser.first_name, ref lUserData);
            Utils.AddToDynamicData("Gender", fbUser.gender, ref lUserData);
            Utils.AddToDynamicData("Birthday", fbUser.Birthday, ref lUserData);

            if (fbUser.Location != null)
            {
                Utils.AddToDynamicData("Loaction", fbUser.Location.name, ref lUserData);
            }
            if (fbUser.interests != null)
            {
                string sInterests = Utils.ConvertFBInterestsToJsonString(fbUser.interests.data);
                Utils.AddToDynamicData("Interests", sInterests, ref lUserData);
            }

            return lUserData;
        }

        public FacebookConfig FBConfig
        {
            get
            {
                return m_oFBConfig;
            }
        }

        public List<FriendWatchedObject> GetAllFriendsWatched(int nUserSiteGuid, int nMeidaID = 0)
        {
            List<string> lFriendsIDs;
            List<FriendWatchedObject> lRes = null;

            GetUserFriendsGuid(nUserSiteGuid, out lFriendsIDs);

            if (lFriendsIDs != null && lFriendsIDs.Count > 0)
            {
                lRes = m_oSocialBL.GetAllFriendsWatchedMedia(nMeidaID, lFriendsIDs);
            }

            return lRes;
        }

        public FacebookTokenResponse FBTokenValidation(string token)
        {
            FacebookTokenResponse ftr = new FacebookTokenResponse();
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new FacebookException(FacebookResponseStatus.ERROR, "Empty Token");
                }

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sDecryptToken = Utils.Decrypt(token, key);

                string sRetVal = string.Empty;
                int status = FBUtils.GetGraphApiAction(string.Format("debug_token?input_token={0}", sDecryptToken), string.Empty, m_oFBConfig.sFBToken, ref sRetVal);

                if (status != STATUS_OK)
                {
                    //Error with facebook response
                    throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
                }

                //Create FBUser
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBTokenRes fbToken = serializer.Deserialize<FBTokenRes>(sRetVal);

                if (fbToken != null)
                {
                    ftr.status = fbToken.data.is_valid ? "Valid" : "NotValid";
                }
            }
            catch (FacebookException ex)
            {
                ftr.status = FacebookResponseStatus.ERROR.ToString();
                ftr.message = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error("Facebook error - error:" + ex.Message, ex);

                ftr.status = FacebookResponseStatus.ERROR.ToString();
                ftr.message = ex.Message;
            }

            return ftr;
        }

        private bool HasOperationBeenPerformedBefore(string sSiteGuid, int nAssetID, eUserAction eFBAction, ApiObjects.eAssetType assetType)
        {
            SocialActivityDoc doc;

            if (m_oSocialBL.GetUserSocialAction(sSiteGuid, SocialPlatform.FACEBOOK, assetType, eFBAction, nAssetID, out doc) && doc != null)
                return true;
            else
                return false;
        }

        public FacebookResponse FBUserUnmerge(string sToken, string sUsername, string sPassword)
        {
            FacebookResponse facebookResponse = InitializeFacebookResponseObj();

            try
            {
                UserResponseObject user = null;

                if (!string.IsNullOrEmpty(sToken))
                {
                    facebookResponse = FBUserData(sToken, ref user);

                    if (facebookResponse == null || facebookResponse.Status == null || facebookResponse.ResponseData == null)
                    {
                        return InitializeFacebookResponseObj();
                    }

                    if (facebookResponse.Status.Code != (int)eResponseStatus.OK || !facebookResponse.ResponseData.status.Equals(FacebookResponseStatus.OK.ToString()))
                    {
                        return facebookResponse;
                    }

                    if (user == null || user.m_user == null || user.m_user.m_oBasicData == null)
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.WRONGPASSWORDORUSERNAME.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                        facebookResponse.Status.Message = eResponseStatus.WrongPasswordOrUserName.ToString();

                        return facebookResponse;
                    }
                }
                else
                {
                    user = Utils.CheckUserPassword(m_nGroupID, sUsername, sPassword, false);

                    if (user != null && (user.m_RespStatus == ResponseStatus.WrongPasswordOrUserName || user.m_RespStatus == ResponseStatus.UserDoesNotExist))
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.WRONGPASSWORDORUSERNAME.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                        facebookResponse.Status.Message = eResponseStatus.WrongPasswordOrUserName.ToString();
                        return facebookResponse;
                    }
                }

                if (user != null && user.m_user != null && user.m_user.m_oBasicData != null)
                {

                    /* Unmerging means deleting the following from users table:
                             * 1. Facebook Token. 2. Facebook ID. 3. Facebook Pic.
                             */
                    UserBasicData userBasicDataToUpdate = new UserBasicData()
                    {
                        m_sFacebookID = string.Empty,
                        m_sFacebookToken = string.Empty,
                        m_sFacebookImage = string.Empty
                    };
                    UserResponseObject resp = Utils.SetUserData(m_nGroupID, user.m_user.m_sSiteGUID, userBasicDataToUpdate, user.m_user.m_oDynamicData);
                    if (resp != null && resp.m_RespStatus == ResponseStatus.OK)
                    {
                        facebookResponse.ResponseData.status = FacebookResponseStatus.UNMERGEOK.ToString();
                        facebookResponse.Status.Code = (int)eResponseStatus.OK;
                        facebookResponse.ResponseData.siteGuid = user.m_user.m_sSiteGUID;
                        WriteMergeToQueue(user.m_user.m_sSiteGUID, "delete");
                        Utils.TryWriteToUserLog("FB Unmerged successfully.", m_nGroupID, user.m_user.m_sSiteGUID);
                    }
                    else
                    {
                        Utils.TryWriteToUserLog("FB Unmerge failed.", m_nGroupID, user.m_user.m_sSiteGUID);
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception occurred at FBUserUnmerge. ");
                sb.Append(String.Concat(" Username: ", sUsername));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return facebookResponse;
        }

        public FacebookResponse FBUserUnmergeByUserId(string sUserId)
        {
            FacebookResponse res = InitializeFacebookResponseObj();

            try
            {
                UserResponseObject uro = Utils.GetUserDataByID(sUserId, m_nGroupID);

                if (uro != null && (uro.m_RespStatus == ResponseStatus.WrongPasswordOrUserName || uro.m_RespStatus == ResponseStatus.UserDoesNotExist))
                {
                    res.ResponseData.status = FacebookResponseStatus.WRONGPASSWORDORUSERNAME.ToString();
                    res.Status.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                    return res;
                }

                if (uro != null && uro.m_user != null && uro.m_user.m_oBasicData != null)
                {

                    /* Unmerging means deleting the following from users table:
                             * 1. Facebook Token. 2. Facebook ID. 3. Facebook Pic.
                             */
                    UserBasicData userBasicDataToUpdate = new UserBasicData()
                    {
                        m_sFacebookID = string.Empty,
                        m_sFacebookToken = string.Empty,
                        m_sFacebookImage = string.Empty
                    };

                    UserResponseObject resp = Utils.SetUserData(m_nGroupID, uro.m_user.m_sSiteGUID, userBasicDataToUpdate, uro.m_user.m_oDynamicData);
                    if (resp != null && resp.m_RespStatus == ResponseStatus.OK)
                    {
                        res.ResponseData.status = FacebookResponseStatus.UNMERGEOK.ToString();
                        res.Status.Code = (int)eResponseStatus.OK;
                        res.ResponseData.siteGuid = uro.m_user.m_sSiteGUID;
                        WriteMergeToQueue(uro.m_user.m_sSiteGUID, "delete");
                        Utils.TryWriteToUserLog("FB Unmerged successfully.", m_nGroupID, uro.m_user.m_sSiteGUID);
                    }
                    else
                    {
                        Utils.TryWriteToUserLog("FB Unmerge failed.", m_nGroupID, uro.m_user.m_sSiteGUID);
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception occurred at FBUserUnmerge. ");
                sb.Append(String.Concat(" User ID: ", sUserId));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return res;
        }

        private void WriteMergeToQueue(string sSiteGuid, string sAction)
        {
            QueueWrapper.BaseQueue queue = new QueueWrapper.Queues.QueueObjects.SocialQueue();
            string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskSocialMerge");
            string routingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeySocialFeedMerge");
            Guid guid = Guid.NewGuid();


            ApiObjects.BaseCeleryData mergeData = new ApiObjects.BaseCeleryData(guid.ToString(), task, m_nGroupID.ToString(), sSiteGuid, sAction);
            bool bIsUpdateIndexSucceeded = queue.Enqueue(mergeData, string.Concat(routingKey, "\\", m_nGroupID));

            if (!bIsUpdateIndexSucceeded)
            {
                log.Error("Error - " + string.Format("Failed to enqueue merge request. SiteGuid={0}, GroupID={1}, Action={2}", sSiteGuid, m_nGroupID, sAction));
            }
        }

        private bool IsUnmergeCredentialsValid(string sInputUsername, string sInputPassword, FacebookResponse facebookResponse)
        {
            return (sInputUsername.Equals(facebookResponse.ResponseData.fbUser.m_sSiteGuid) || sInputUsername.Equals(facebookResponse.ResponseData.fbUser.email)) && sInputPassword.Equals(facebookResponse.ResponseData.data);
        }


        private bool IsUnmergeInputValid(string sToken, string sUsername, string sPassword)
        {
            return !string.IsNullOrEmpty(sToken) || (!string.IsNullOrEmpty(sUsername) && !string.IsNullOrEmpty(sPassword));
        }

        private FacebookResponse InitializeFacebookResponseObj()
        {
            FacebookResponse facebookResponse = new FacebookResponse();
            facebookResponse.ResponseData.status = FacebookResponseStatus.ERROR.ToString();
            facebookResponse.Status.Code = (int)eResponseStatus.Error;

            return facebookResponse;
        }

        public FBSignin FBUserSignin(string token, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            FBSignin fbs = new FBSignin();
            try
            {

                if (string.IsNullOrEmpty(token))
                {
                    throw new FacebookException(FacebookResponseStatus.ERROR, "empty token");
                }

                string sRetVal = string.Empty;
                int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location", string.Empty, token, ref sRetVal);

                if (status != STATUS_OK)
                {
                    //Error with facebook response
                    throw new FacebookException(FacebookResponseStatus.ERROR, sRetVal);
                }

                //Create FBUser
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);

                string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                string sEncryptToken = Utils.Encrypt(token, key);

                UserResponseObject uObj = new UserResponseObject();
                uObj = Utils.GetUserDataByFacebookID(fbUser.id, m_nGroupID);

                //User Exists
                if (uObj.m_RespStatus == ResponseStatus.OK)
                {
                    string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;

                    //Update user FBToken
                    if (sFBToken != sEncryptToken)
                    {
                        UserBasicData userBasicDataToUpdate = new UserBasicData() { m_sFacebookToken = sEncryptToken };
                        uObj = Utils.SetUserData(m_nGroupID, uObj.m_user.m_sSiteGUID, userBasicDataToUpdate, uObj.m_user.m_oDynamicData);
                    }
                    fbs.user = Utils.Signin(m_nGroupID, uObj.m_user.m_sSiteGUID, sIP, deviceID, bPreventDoubleLogins);
                    fbs.status.Code = (int)eResponseStatus.OK;
                }
                else
                {
                    fbs.status.Code = (int)eResponseStatus.UserDoesNotExist;
                    fbs.status.Message = "User not exists";
                }
            }
            catch (FacebookException ex)
            {
                log.Error("Facebook error - error:" + ex.Message, ex);

                fbs.status.Code = (int)eResponseStatus.Error;
                fbs.status.Message = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error("Exception - error:" + ex.Message, ex);

                fbs.status.Code = (int)eResponseStatus.Error;
                fbs.status.Message = ex.Message;
            }

            return fbs;
        }
    }
}
