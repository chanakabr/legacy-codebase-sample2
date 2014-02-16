using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Data;
using ApiObjects;
using System.Xml;
using System.Collections;
using System.Web;
using System.Configuration;
using System.Net;
using System.IO;
using APILogic;
using APILogic.Catalog;
using ApiObjects.SearchObjects;

namespace APIWS
{
    public class api
    {
        protected api() { }

        protected api(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        static public UserIMRequestObject GetTvinciGUID(InitializationObject oInitObj, Int32 nGroupID)
        {
            Int32 nWatcherID = 0;
            GetStartValues(oInitObj.m_oUserIMRequestObject.m_sSiteGuid, ref oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                ref nWatcherID, nGroupID, true, oInitObj.m_oUserIMRequestObject.m_sUserAgent, oInitObj.m_oUserIMRequestObject.m_sUserIP);
            return oInitObj.m_oUserIMRequestObject;

        }

        static public List<GroupOperator> GetOperators(int nGroupID, List<int> operatorIds)
        {
            try
            {
                // Return all group operators by ID
                List<GroupOperator> ret = new List<GroupOperator>();


                DataSet ds = DAL.ApiDAL.Get_Operators_Info(nGroupID, operatorIds);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        GroupOperator go;

                        foreach (DataRow dataRow in ds.Tables[0].Rows)
                        {
                            go                  = new GroupOperator();
                            go.ID               = ODBCWrapper.Utils.GetIntSafeVal(dataRow["ID"]);
                            go.Name             = ODBCWrapper.Utils.GetSafeStr(dataRow["Name"]);
                            go.SubGroupID       = ODBCWrapper.Utils.GetIntSafeVal(dataRow["Sub_Group_ID"]);
                            go.Type             = (eOperatorType)ODBCWrapper.Utils.GetIntSafeVal(dataRow["type"]);
                            go.CoGuid           = ODBCWrapper.Utils.GetSafeStr(dataRow["Client_Id"]);

                            go.UIData           = new UIData();
                            go.UIData.picURL    = ODBCWrapper.Utils.GetSafeStr(dataRow["Pic_URL"]);
                            go.UIData.ColorCode = ODBCWrapper.Utils.GetSafeStr(dataRow["Color_Code"]);
                            go.GroupUserName    = ODBCWrapper.Utils.GetSafeStr(dataRow["USERNAME"]);
                            go.GroupPassword    = ODBCWrapper.Utils.GetSafeStr(dataRow["PASSWORD"]);
                            go.AboutUs          = ODBCWrapper.Utils.GetSafeStr(dataRow["About_Us"]);
                            go.ContactUs        = ODBCWrapper.Utils.GetSafeStr(dataRow["Contact_Us"]);

                            go.Groups_operators_menus = new List<KeyValuePair<string, string>>();
                            foreach (DataRow groupsOperatorsMenusDRdataRow in ds.Tables[1].Rows)
                            {
                                string PID = ODBCWrapper.Utils.GetSafeStr(groupsOperatorsMenusDRdataRow["PlatformID"]);
                                string MID = ODBCWrapper.Utils.GetSafeStr(groupsOperatorsMenusDRdataRow["TVPMenuID"]);
                                go.Groups_operators_menus.Add(new KeyValuePair<string, string>(PID, MID));
                            }

                            ret.Add(go);
                        }
                    }
                }
                return ret;
            }
            catch
            {
                // TODO print to logger
                return null;
            }
        }


        static public bool InitializeGroupNPlayer(ref ApiObjects.InitializationObject initObj)
        {
            try
            {
                if (initObj.m_oPlayerIMRequestObject != null)
                    initObj.m_nGroupID = PageUtils.GetGroupByUNPass(initObj.m_oPlayerIMRequestObject.m_sPalyerID, initObj.m_oPlayerIMRequestObject.m_sPlayerKey, ref initObj.m_nPlayerID);
                else
                    return false;

                if (initObj.m_nGroupID == 0)
                    return false;

                api.GetStartValues(initObj.m_oUserIMRequestObject.m_sSiteGuid, ref initObj.m_oUserIMRequestObject.m_sTvinciGuid,
                    ref initObj.m_nWatcherID, initObj.m_nGroupID, false, initObj.m_oUserIMRequestObject.m_sUserAgent, initObj.m_oUserIMRequestObject.m_sUserIP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public ApiObjects.MediaInfoObject TVAPI_GetMediaInfo(InitializationObject oInitObj, Int32 nGroupID, Int32 nMediaID)
        {
            XmlDocument doc = null;
            ApiObjects.MediaInfoObject theInfo = new MediaInfoObject();
            ApiObjects.MediaStatistics theStat = null;
            ApiObjects.MediaPersonalStatistics thePersonalStat = null;
            ApiObjects.PicObject[] thePics = null;

            TVinciShared.ProtocolsFuncs.MediaInfoProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, ref thePics, ref theInfo, ref thePersonalStat, ref theStat, nMediaID);
            return theInfo;
        }

        static public ApiObjects.MediaInfoStructObject GetMediaStructure(InitializationObject oInitObj, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.MediaInfoStructObject theInfoStruct = new MediaInfoStructObject();
            TVinciShared.ProtocolsFuncs.MediaStructureProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref theInfoStruct);
            return theInfoStruct;
        }

        static public ApiObjects.ChannelObject SearchMedia(InitializationObject oInitObj, SearchDefinitionObject theSearchCriteria, MediaInfoStructObject theInfoStruct, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = 0;
            if (oInitObj == null)
            {
                nDeviceID = 0;
            }
            else
            {
                nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            }
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SearchMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, false, nCountryID, ref oInitObj,
                ref theSearchCriteria, ref theInfoStruct, ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.RateResponseObject RateMedia(InitializationObject oInitObj, Int32 nMediaID, Int32 nRateVal, Int32 nGroupID)
        {
            XmlDocument doc = null;
            RateResponseObject ret = new RateResponseObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.RatingProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, ref ret, nMediaID, nRateVal);

            return ret;
        }

        static public ApiObjects.ChannelObject SearchRelated(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMediaID, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SearchRelatedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nMediaID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject NowPlaying(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.NowPlayingProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject PersonalRateList(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID, Int32 nMinRate, Int32 nMaxRate)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PersonalRatedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nMinRate, nMaxRate, nDeviceID);

            return ret;
        }

        static public ApiObjects.TagResponseObject[] TagValues(InitializationObject oInitObj, ApiObjects.TagRequestObject[] sTagTypes, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.TagResponseObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.TagValuesProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                ref oInitObj, ref sTagTypes, ref ret, nCountryID, nDeviceID);
            return ret;
        }

        static public ApiObjects.OneTimeObject OneTimeLink(InitializationObject oInitObj, Int32 nMediaID, Int32 nMediaFileID,
            string sBaseLink, string sCDNImpleType, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.OneTimeObject ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.MediaOneTimeLinkProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                sCDNImpleType, nMediaID, nMediaFileID, sBaseLink, oInitObj.m_oPlayerIMRequestObject.m_sPalyerID, ref ret);
            return ret;
        }

        /*Get the Lucene URL from DB , else from config file*/
        private static string GetLuceneUrl(int nGroupID)
        {
            string sLuceneURL = GetWSURL("LUCENE_WCF");
            return sLuceneURL;
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public List<string> GetAutoCompleteList(RequestObj request, int groupID)
        {
            Logger.Logger.Log("AutoComplete", "Start AutoComplete", "AutoComplete");
            List<string> retVal = new List<string>();

            APILogic.Catalog.IserviceClient client = new APILogic.Catalog.IserviceClient();
            string sCatalogUrl = APILogic.Utils.GetWSUrl("WS_Catalog");

            if (!string.IsNullOrEmpty(sCatalogUrl))
            {
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sCatalogUrl);
            }

            Logger.Logger.Log("AutoComplete", "Start AutoComplete : Catalog URL is :" + client.Endpoint.Address.Uri.AbsolutePath, "AutoComplete");

            APILogic.Catalog.MediaAutoCompleteRequest autoCompleteRequest = new APILogic.Catalog.MediaAutoCompleteRequest();
            autoCompleteRequest.m_nGroupID = groupID;

            autoCompleteRequest.m_sPrefix = request.m_InfoStruct.m_sPrefix;
            autoCompleteRequest.m_lMetas = request.m_InfoStruct.m_Metas.ToArray();
            autoCompleteRequest.m_lTags = request.m_InfoStruct.m_Tags.ToArray();

            APILogic.Catalog.MediaAutoCompleteResponse response = client.GetResponse(autoCompleteRequest) as APILogic.Catalog.MediaAutoCompleteResponse;

            if (response != null && response.lResults != null)
            {
                retVal = response.lResults.ToList();
            }

            return retVal;
        }

        static public ApiObjects.UserComment[] MediaComments(InitializationObject oInitObj, Int32 nMediaID, Int32 nGroupID, string sCommentType)
        {
            XmlDocument doc = null;
            ApiObjects.UserComment[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.CommentsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaID, sCommentType, ref ret, oInitObj.m_oLanguageRequestObject.m_sFullName);
            return ret;
        }

        static public ApiObjects.UserComment[] UserSaveComment(InitializationObject oInitObj, ApiObjects.UserComment theUserComment,
            bool bAutoPublish, Int32 nGroupID, string sCommentType)
        {
            XmlDocument doc = null;
            ApiObjects.UserComment[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SaveCommentsProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, theUserComment, bAutoPublish, sCommentType, ref ret, oInitObj.m_oLanguageRequestObject.m_sFullName);
            return ret;
        }


        static public bool SetMediaDuration(InitializationObject oInitObj, Int32 nMediaFileID, Int32 nDurationInSec, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = false;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SetDurationProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID,
                oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaFileID, nDurationInSec, ref ret);
            return ret;
        }

        static public bool MediaMark(InitializationObject oInitObj, string sAction, Int32 nLocationSec,
            ref MediaFileObject theMediaFile, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = true;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.MediaMark(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, nCountryID,
                oInitObj.m_nPlayerID, ref oInitObj, sAction, nLocationSec, ref theMediaFile);
            return ret;
        }

        static public bool Hit(InitializationObject oInitObj, Int32 nLocationInVideoSec,
            ref MediaFileObject theMediaFile, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = true;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.HitProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, nCountryID,
                oInitObj.m_nPlayerID, ref oInitObj, nLocationInVideoSec, ref theMediaFile);
            return ret;
        }

        static public ApiObjects.ChannelObject UserLastWatched(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PersonalLastWatchedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref thePageDef, ref theInfoStruct, ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject PeopleWhoWatchedAlsoWatched(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nMediaID, Int32 nMediaFileID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PWWAWProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, nMediaID, nMediaFileID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] ChannelsMedia(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = new ChannelObject[theChannelsRequestObj.Length];
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.ChannelMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, nCountryID,
                ref oInitObj, ref theInfoStruct, ref theChannelsRequestObj,
                ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.CategoryObject[] CategoriesTree(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nCategoryID, bool bWithChannels)
        {
            XmlDocument doc = null;
            CategoryObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.CategoriesListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, !oInitObj.m_oExtraRequestObject.m_bNoCache, ref oInitObj, ref theInfoStruct, nCategoryID, bWithChannels,
                ref ret, nCountryID, nDeviceID, true);


            return ret;
        }

        static public ApiObjects.ChannelObject[] CategoryChannels(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nCategoryID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.ChannelsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, !oInitObj.m_oExtraRequestObject.m_bNoCache, ref oInitObj, ref theInfoStruct, nCategoryID, ref ret, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] UserChannels(InitializationObject oInitObj,
            MediaInfoStructObject theInfoStruct, Int32 nGroupID, string sLang)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.WatcherChannelsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                false, ref oInitObj, ref theInfoStruct, ref ret, sLang, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] DeleteUserChannel(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct,
            Int32 nGroupID, Int32 nChannelID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.DeletePlayListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                false, ref oInitObj, ref theInfoStruct, ref ret, nChannelID, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.GenericWriteResponse saveUserPlaylist(InitializationObject oInitObj, Int32 nGroupID, Int32[] nMediaIDs, string sChannelTitle, bool bRewrite)
        {
            XmlDocument doc = null;
            GenericWriteResponse ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SavePlayListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                ref oInitObj, nMediaIDs, sChannelTitle, bRewrite, ref ret);
            return ret;
        }

        static public ApiObjects.GenericWriteResponse SendMediaByEmail(InitializationObject oInitObj, Int32 nGroupID, Int32 nMediaID,
            string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent)
        {
            XmlDocument doc = null;
            GenericWriteResponse ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SentToFriendProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaID, sFromEmail, sToEmail, sRecieverName, sSenderName, sContent, ref ret);
            return ret;
        }

        static public bool SetTokenData(ref InitializationObject oInitObj, ref Int32 nCountryID, ref Int32 nDeviceID, Int32 nGroupID)
        {
            string sIP = PageUtils.GetCallerIP();
            Int32 nCountryTokenID = 0;
            string sLang = "";
            Int32 nDeviceTokenID = 0;
            bool bAdmin = false;
            bool bWithCache = false;
            bool bAdminToken = false;
            if (String.IsNullOrEmpty(oInitObj.m_sAdminToken) == false)
                bAdminToken = TVinciShared.ProtocolsFuncs.GetAdminTokenValues(oInitObj.m_sAdminToken, sIP,
                    ref nCountryTokenID, ref sLang, ref nDeviceTokenID, nGroupID, ref bAdmin, ref bWithCache);
            if (bAdminToken == true)
            {
                nCountryID = nCountryTokenID;
                oInitObj.m_oLanguageRequestObject = new LanguageRequestObject();
                oInitObj.m_oLanguageRequestObject.Initialize(sLang);
                nDeviceID = nDeviceTokenID;
            }
            return bAdminToken;
        }

        static public ChannelObject SingleMedia(InitializationObject oInitObj, Int32 nGroupID, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            ChannelObject ret = new ChannelObject();
            XmlDocument doc = null;
            ApiObjects.MediaObject[] theMediaObjs = null;
            ApiObjects.PlayListSchema thePlayListSchema = new PlayListSchema(); ;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SingleMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid, "", "",
                oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                oInitObj.m_nPlayerID, !oInitObj.m_oExtraRequestObject.m_bNoCache, false, ref oInitObj, nMediaIDs,
                ref theInfoStruct, ref theMediaObjs, ref thePlayListSchema, nCountryID, nDeviceID);
            ret.Initialize(thePlayListSchema, theMediaObjs, nMediaIDs.Length, null, "Container", "", "", 0);
            return ret;
        }

        static public void GetStartValues(string sSiteGUID, ref string sTVinciGUID, ref Int32 nWatcherID, Int32 nGroupID, bool bCreate, string sUserAgent, string sCallerIP)
        {
            if (sSiteGUID.Trim() != "")
            {
                DataTable dt = DAL.ApiDAL.Get_StartValues(nGroupID, sSiteGUID, 1);// 1 = by sSiteGUID
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        nWatcherID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "WATCHER_ID");
                        sTVinciGUID = APILogic.Utils.GetSafeStr(dt.Rows[0], "TVINCI_GUID");
                    }
                    else
                    {
                        if (sTVinciGUID == "")
                            sTVinciGUID = System.Guid.NewGuid().ToString();
                        if (bCreate == true)
                            nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
            }
            else
            {
                if (sTVinciGUID != "")
                {
                    DataTable dt = DAL.ApiDAL.Get_StartValues(0, sTVinciGUID, 2);// 1 = by sTVinciGUID
                    if (dt != null)
                    {
                        if (dt.Rows != null && dt.Rows.Count > 0)
                        {
                            nWatcherID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "ID");
                        }
                    }

                    if (nWatcherID == 0)
                    {
                        if (bCreate == true)
                            nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
                else
                {
                    if (bCreate == true)
                    {
                        sTVinciGUID = System.Guid.NewGuid().ToString();
                        nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
            }
        }

        static public Int32 GetMediaFileTypeID(Int32 nMediaFileID, Int32 nGroupID)
        {
            Int32 nRet = 0;
            DataTable dt = DAL.ApiDAL.Get_MediaFileTypeID(nMediaFileID, nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    nRet = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "virtual_name");
                }
            }


            return nRet;
        }

        static public bool GetAdminTokenValues(string sIP, string sToken, Int32 nGroupID, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref ApiObjects.UserStatus theUserStatus)
        {
            bool bRet = false;
            DataTable dt = DAL.ApiDAL.Get_AdminTokenValues(sIP, nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {

                    sDeviceName = APILogic.Utils.GetSafeStr(dt.Rows[0], "DEVICE_NAME");
                    sLanguageFullName = APILogic.Utils.GetSafeStr(dt.Rows[0], "LANGUAGE_NAME");
                    sCountryCd2 = APILogic.Utils.GetSafeStr(dt.Rows[0], "COUNTRY_NAME");
                    Int32 nUserStatus = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "USER_STATUS_ID");
                    theUserStatus = (UserStatus)nUserStatus;
                    bRet = true;
                }
            }
            return bRet;
        }

        //static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, int[] deviceRulesIds)
        static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, string sDevice)
        {
            List<int> nMedias = new List<int>();
            APILogic.Catalog.IserviceClient client = new APILogic.Catalog.IserviceClient();

            try
            {
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");

                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                string sCatalogUrl = APILogic.Utils.GetWSUrl("WS_Catalog");
                if (!string.IsNullOrEmpty(sCatalogUrl))
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sCatalogUrl);

                    for (int i = 0; i < nChannels.Length; i++)
                    {
                        try
                        {
                            APILogic.Catalog.ChannelRequestMultiFiltering channelRequest = new APILogic.Catalog.ChannelRequestMultiFiltering();
                            channelRequest.m_nChannelID = nChannels[i];
                            channelRequest.m_nGroupID = nGroupID;
                            channelRequest.m_oFilter = new APILogic.Catalog.Filter();
                            channelRequest.m_oFilter.m_bOnlyActiveMedia = true;
                            channelRequest.m_oFilter.m_bUseStartDate = true;
                            channelRequest.m_nPageSize = 0;
                            channelRequest.m_nPageIndex = 0;
                            channelRequest.m_oFilter.m_sDeviceId = sDevice;
                            channelRequest.m_sSignString = sSignString;
                            channelRequest.m_sSignature = sSignature;
                            APILogic.Catalog.BaseResponse response = client.GetResponse(channelRequest);
                            SearchResult[] medias = ((APILogic.Catalog.ChannelResponse)response).m_nMedias.ToArray();
                            int[] mediaIdsReturned = null;
                           
                            List<int> lIdsReturned = APILogic.Utils.ConvertMediaResultObjectIDsToIntArray(medias);
                            if (lIdsReturned != null)
                            {
                                mediaIdsReturned = lIdsReturned.ToArray();
                            }

                            if (mediaIdsReturned != null && mediaIdsReturned.Length > 0)
                            {
                                nMedias.AddRange(mediaIdsReturned);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Error", string.Format("channel:{0}, msg:{1}", nChannels[i], ex.Message), "Channel Medias");
                        }
                    }
                }
            }
            catch
            {
                Logger.Logger.Log("Configuration Reading", "Couldn't read values from configuration ", "Configuration");
            }
            finally
            {
                client.Close();
            }


            return nMedias.Distinct().ToArray();

        }


        //static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, int[] deviceRulesIds)
        //{
        //    try
        //    {
        //        List<int> nMedias = new List<int>();
        //        APILogic.Lucene.Service client = new APILogic.Lucene.Service();

        //        string sWSURL = GetLuceneUrl(nGroupID);
        //        if (!string.IsNullOrEmpty(sWSURL))
        //        {
        //            client.Url = sWSURL;
        //        }

        //        for (int i = 0; i < nChannels.Length; i++)
        //        {
        //            var medias = client.GetChannelMedias(nGroupID, nChannels[i], 0, nFileTypeIDs, true, true, deviceRulesIds, 0, 0);

        //            if (medias == null || medias.n_TotalItems == 0)
        //                continue;

        //            nMedias.AddRange(medias.m_resultIDs);
        //        }

        //        if (nMedias == null || nMedias.Count == 0)
        //            return null;

        //        return nMedias.Distinct().ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Logger.Log("Error", string.Format("group:{0}, channels:({1}), ex:{3}", nGroupID, string.Join(",", nChannels.Select(x => x.ToString()).ToArray()), ex.Message), "GetChannelsMediaIDs");
        //        return new int[0];
        //    }
        //}

        static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, Int32 nDeviceID)
        {

            // TO DO add call to catalog !!!!! SubsContainMedia

            List<int> nMedias = new List<int>();

            //APILogic.Lucene.Service client = new APILogic.Lucene.Service();

            //string sWSURL = GetLuceneUrl(nGroupID);
            //if (!String.IsNullOrEmpty(sWSURL))
            //{
            //    client.Url = sWSURL; // add parent group id to url
            //}

            //for (int i = 0; i < nChannels.Length; i++)
            //{
            //    APILogic.Lucene.SearchResultsObj medias = client.GetChannelMedias(nGroupID, nChannels[i], 0, null, true, true, null, 0, 0, 0);

            //    if (medias == null || medias.m_resultIDs == null || medias.m_resultIDs.Length == 0)
            //        continue;

            //    nMedias.AddRange(medias.m_resultIDs);
            //}

            //if (nMedias == null || nMedias.Count == 0)
            //    return null;

            return nMedias.Distinct().ToArray();
        }

        static public FileTypeContainer[] GetAvailableFileTypes(Int32 nGroupID)
        {
            FileTypeContainer[] ret = null;
            DataTable dt = DAL.ApiDAL.Get_AvailableFileTypes(nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    ret = new FileTypeContainer[dt.Rows.Count];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                       
                        int nID = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "MEDIA_TYPE_ID");
                        if (nID > 0)
                        {
                            FileTypeContainer f = new FileTypeContainer();
                     
                            f.Initialize(dt.Rows[i]["description"].ToString(), APILogic.Utils.GetIntSafeVal(dt.Rows[i], "ID"));
                            ret[i] = f;
                        }
                    }
                }
            }

            return ret;
        }

        static public Int32[] GetChannelMediaIDs(Int32 nChannelID, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID,
            Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nCount = 0;
            TVinciShared.Channel c = new TVinciShared.Channel(nChannelID, bWithCache, nLangID, bIsLangMain, nCountryID, nDeviceID);
            c.SetGroupID(nGroupID);
            System.Data.DataTable d = c.GetChannelMediaDT(nFileTypeIDs);
            if (d == null)
                return null;
            nCount += d.DefaultView.Count;
            if (nCount > 0)
            {
                Int32[] ret = new Int32[nCount];
                Int32 nIndex = 0;
                if (d == null)
                    return null;
                for (int j = 0; j < nCount; j++)
                {
                    Int32 nMediaID = int.Parse(d.DefaultView[j].Row["id"].ToString());
                    if (Array.IndexOf(ret, nMediaID) == -1)
                    {
                        ret[nIndex] = nMediaID;
                        nIndex++;
                    }
                }
                if (nIndex < nCount)
                {
                    Int32[] tmp1 = new Int32[nIndex];
                    Array.Copy(ret, tmp1, nIndex);
                    ret = tmp1;
                }
                return ret;
            }
            return null;
        }

        static public bool DoesMediaBelongToChannels(Int32[] nChannels, Int32[] nFileTypeIDs, Int32 nMediaID, bool bWithCache, Int32 nGroupID, Int32 nDeviceID)
        {
            try
            {
                ChannelsContainingMediaRequest ccm = new ChannelsContainingMediaRequest();  
                ccm.m_nGroupID = nGroupID;
                ccm.m_nMediaID = nMediaID;
                ccm.m_lChannles = nChannels;
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                ccm.m_sSignString = sSignString;
                ccm.m_sSignature = sSignature;
                ccm.m_oFilter.m_sDeviceId = nDeviceID.ToString();

                string sURL = GetWSURL("WS_Catalog");
                APILogic.Catalog.IserviceClient client = new IserviceClient();
                if (!string.IsNullOrEmpty(sURL))
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sURL);

                ChannelsContainingMediaResponse response = (ChannelsContainingMediaResponse)client.GetResponse(ccm);

                if (response == null || response.m_lChannellList == null || response.m_lChannellList.Count() == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("group:{0}, msg:{1}", nGroupID, ex.Message), "DoesMediaBelongToChannels");
                return false;
            }           
        }

        static public bool DoesMediaBelongToChannels(Int32[] nChannels, Int32[] nFileTypeIDs, Int32 nMediaID, bool bWithCache, Int32 nGroupID, string sDevice)
        {
            try
            {
                ChannelsContainingMediaRequest ccm = new ChannelsContainingMediaRequest();
                ccm.m_nGroupID = nGroupID;
                ccm.m_nMediaID = nMediaID;
                ccm.m_lChannles = nChannels;
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                ccm.m_sSignString = sSignString;
                ccm.m_sSignature = sSignature;

                ccm.m_oFilter = new Filter();
                ccm.m_oFilter.m_sDeviceId = sDevice;

                string sURL = GetWSURL("WS_Catalog");
                APILogic.Catalog.IserviceClient client = new IserviceClient();
                if (!string.IsNullOrEmpty(sURL))
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(sURL);

                ChannelsContainingMediaResponse response = (ChannelsContainingMediaResponse)client.GetResponse(ccm);

                if (response == null)
                    return false;
                else if (response.m_lChannellList == null || response.m_lChannellList.Count() == 0)
                    return false;
                else
                    return true;

            }
            catch (Exception ex)
            {
                return false;
            }
           
        }

        static public bool ValidateBaseLink(Int32 nMediaFileID, string sBaseLink, Int32 nGroupID)
        {
            bool bRet = false;
            string sDBBaseLink = "";
            DataTable dt = DAL.ApiDAL.Get_DataByTableID(nMediaFileID.ToString(), "media_files", "STREAMING_CODE");
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    sDBBaseLink = APILogic.Utils.GetSafeStr(dt.Rows[0], "STREAMING_CODE");
                    if (sDBBaseLink != string.Empty)
                        sDBBaseLink = sDBBaseLink.ToLower().Trim();

                    if (sBaseLink.Trim().ToLower().EndsWith(sDBBaseLink) == true && sDBBaseLink != "")
                        bRet = true;
                    else
                    {
                        if (System.Web.HttpUtility.HtmlDecode(sBaseLink.Trim().ToLower()).EndsWith(System.Web.HttpUtility.HtmlDecode(sDBBaseLink)) == true && sDBBaseLink != "")
                            bRet = true;
                        else
                        {
                            if (System.Web.HttpUtility.UrlDecode(sBaseLink.Trim().ToLower()).Trim().EndsWith(System.Web.HttpUtility.UrlDecode(sDBBaseLink).Trim()) == true && sDBBaseLink != "")
                                bRet = true;
                        }
                    }
                }
            }
            return bRet;
        }

        static public MeidaMaper[] MapMediaFiles(Int32[] nMediaFileIDs, Int32 nGroupID)
        {
            MeidaMaper[] ret = null;
            if (nMediaFileIDs.Length == 0)
                return null;

            DataTable dt = DAL.ApiDAL.Get_MapMediaFiles(nMediaFileIDs.ToList<int>());
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    ret = new MeidaMaper[dt.Rows.Count];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Int32 nMediaFileID = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "id");
                        Int32 nMediaID = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "media_id");
                        MeidaMaper m = new MeidaMaper();
                        m.Initialize(nMediaFileID, nMediaID);
                        ret[i] = m;
                    }
                }
            }
            return ret;
        }

        static public MeidaMaper[] MapMediaFilesST(string sSeperatedMediaFileIDs, Int32 nGroupID)
        {

            string[] sToSep = { ";" };
            string[] sSep = sSeperatedMediaFileIDs.Split(sToSep, StringSplitOptions.RemoveEmptyEntries);
            if (sSep.Length == 0)
                return null;

            List<int> nMediaFileIDs = new List<int>(sSeperatedMediaFileIDs.Split(';').Select(int.Parse));
            return MapMediaFiles(nMediaFileIDs.ToArray<int>(), nGroupID);
        }

        static public GroupInfo[] GetSubGroupsTree(string sGroupName, Int32 nGroupID)
        {
            if (CachingManager.CachingManager.Exist("WSAPI.GetSubGroupsTree" + "|" + sGroupName))
            {
                object cData = CachingManager.CachingManager.GetCachedData("WSAPI.GetSubGroupsTree" + "|" + sGroupName);
                return ((List<GroupInfo>)cData).ToArray();
            }

            // Get groups table from database

            DataTable dt = DAL.ApiDAL.Get_SubGroupsTree();
            List<GroupInfo> ret = new List<GroupInfo>();
            if (dt != null)
            {
                if (dt.Rows != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GroupInfo info = new GroupInfo();

                        info.Name = APILogic.Utils.GetSafeStr(row, "Group_Name");
                        info.ID = (long)row["ID"];
                        info.ParentID = (long)row["PARENT_GROUP_ID"];

                        ret.Add(info);
                    }
                }
            }
            else
            {
                throw new Exception("Failed getting groups table from database");
            }
            CachingManager.CachingManager.SetCachedData("WSAPI.GetSubGroupsTree" + "|" + sGroupName, ret, 86400000, System.Web.Caching.CacheItemPriority.Normal, 0, false);

            return ret.ToArray();
        }

        static public string[] GetGroupPlayers(string sGroupName, bool sIncludeChildGroups, Int32 nGroupID)
        {
            if (CachingManager.CachingManager.Exist("WSAPI.GetGroupPlayers" + "|" + sGroupName))
            {
                object cData = CachingManager.CachingManager.GetCachedData("WSAPI.GetGroupPlayers" + "|" + sGroupName);
                return ((List<string>)cData).ToArray();
            }

            DataTable dt = DAL.ApiDAL.Get_GroupPlayers();
            List<string> ret = new List<string>();
            if (dt != null)
            {
                EnumerableRowCollection<DataRow> source = dt.AsEnumerable();

                EnumerableRowCollection<DataRow> mainGroupRows = source.Where(item => item["GROUP_NAME"].ToString() == sGroupName);

                if (mainGroupRows.Count() == 0)
                {
                    // Main group not found
                    throw new Exception("Main group given was not found in groups table");
                }

                // Add group's players
                ret.AddRange(mainGroupRows.Select<DataRow, string>(item => item["USERNAME"].ToString()));

                long mainGroupID = (long)mainGroupRows.ElementAt(0)["ID"];

                if (sIncludeChildGroups)
                {
                    // Add child group's players
                    GetChildGroupPlayers(mainGroupID, ret, source);
                }
            }
            else
            {
                throw new Exception("Failed getting groups table from database");
            }

            // Sort return array
            ret.Sort();

            CachingManager.CachingManager.SetCachedData("WSAPI.GetGroupPlayers" + "|" + sGroupName, ret, 86400000, System.Web.Caching.CacheItemPriority.Normal, 0, false);

            return ret.ToArray();
        }

        static private void GetChildGroupPlayers(long lParentGroupID, List<string> addToList, EnumerableRowCollection<DataRow> eSourceData)
        {
            EnumerableRowCollection<DataRow> children = eSourceData.Where(item => (long)item["PARENT_GROUP_ID"] == lParentGroupID);

            foreach (DataRow child in children)
            {
                addToList.Add(child["USERNAME"].ToString());

                GetChildGroupPlayers((long)child["ID"], addToList, eSourceData);
            }
        }

        static public string[] GetGroupMediaNames(string sGroupName, Int32 nGroupID)
        {
            if (CachingManager.CachingManager.Exist("WSAPI.GetGroupMediaNames" + "|" + sGroupName))
            {
                object cData = CachingManager.CachingManager.GetCachedData("WSAPI.GetGroupMediaNames" + "|" + sGroupName);
                return (string[])cData;
            }

            DataSet ds = DAL.ApiDAL.Get_GroupMediaNames(sGroupName);
            string[] ret = null;
            int groupID = 0;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    groupID = APILogic.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");

                    if (groupID != 0 && ds.Tables[1] != null)
                    {
                        EnumerableRowCollection<DataRow> source = ds.Tables[1].AsEnumerable();
                        ret = source.Select<DataRow, string>(item => item["NAME"].ToString()).ToArray();
                    }
                    else
                    {
                        throw new Exception("Failed getting group's media names from database");
                    }
                }
                else
                {
                    throw new Exception("Failed getting group's id from database");
                }
            }
            else
            {
                throw new Exception("Failed getting group's id from database");
            }

            CachingManager.CachingManager.SetCachedData("WSAPI.GetGroupMediaNames" + "|" + sGroupName, ret, 86400000, System.Web.Caching.CacheItemPriority.Normal, 0, false);

            return ret;
        }


        static public ApiObjects.MediaMarkObject GetMediaMark(Int32 nGroupID, Int32 nMediaID, string sSiteGUID)
        {
            MediaMarkObject mmo = new MediaMarkObject();

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.MISSING_USER_SITE_GUID;
                return mmo;
            }

            if (nMediaID == 0)
            {
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.MISIING_MEDIA_ID;
                return mmo;
            }


            mmo.Initialize(nGroupID, nMediaID, sSiteGUID);

            Int32 nMediaOwnerGroupID = 0;


            object oMediaOwnerGroupID = ODBCWrapper.Utils.GetTableSingleVal("media", "group_id", nMediaID);

            if (oMediaOwnerGroupID != null && oMediaOwnerGroupID != DBNull.Value)
                nMediaOwnerGroupID = int.Parse(oMediaOwnerGroupID.ToString());

            if (nMediaOwnerGroupID == 0)
            {
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.FAILED;
                return mmo;
            }

            string groupStr = PageUtils.GetFullGroupsStr(nGroupID, string.Empty);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            //23.10.2012: set Writeble parameter to true 
            selectQuery.SetWritable(true);
            selectQuery += "SELECT TOP(1) LOCATION_SEC, DEVICE_UDID FROM USERS_MEDIA_MARK WITH (NOLOCK) WHERE ";
            selectQuery += " GROUP_ID ";
            selectQuery += groupStr;
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sSiteGUID);
            selectQuery += "ORDER BY UPDATE_DATE DESC";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    mmo.nLocationSec = int.Parse(selectQuery.Table("query").DefaultView[0].Row["location_sec"].ToString());
                    mmo.sDeviceID = selectQuery.Table("query").DefaultView[0].Row["device_udid"].ToString();

                    if (string.IsNullOrEmpty(mmo.sDeviceID))
                    {
                        mmo.sDeviceName = "PC";
                    }
                    else
                    {
                        ODBCWrapper.DataSetSelectQuery selectDeviceQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectDeviceQuery += "SELECT NAME FROM DEVICES WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND";
                        selectDeviceQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                        selectDeviceQuery += " AND ";
                        selectDeviceQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_ID", "=", mmo.sDeviceID);
                        selectDeviceQuery.SetConnectionKey("USERS_CONNECTION");
                        if (selectDeviceQuery.Execute("query", true) != null)
                        {
                            Int32 nCount1 = selectDeviceQuery.Table("query").DefaultView.Count;
                            if (nCount1 > 0)
                            {
                                mmo.sDeviceName = selectDeviceQuery.Table("query").DefaultView[0].Row["name"].ToString();
                            }
                            else
                            {
                                mmo.sDeviceName = "N/A";
                                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.NA;
                            }
                        }
                        selectDeviceQuery.Finish();
                        selectDeviceQuery = null;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return mmo;
        }

        static public ApiObjects.RateMediaObject RateMedia(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, Int32 nRateVal)
        {
            RateMediaObject rmo = new RateMediaObject();
            rmo.oStatus.Initialize("OK", 0);

            Int32 nWatcherID = 0;
            int siteGuid = 0;
            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                siteGuid = int.Parse(sSiteGUID);
                object oWatcherID = ODBCWrapper.Utils.GetTableSingleVal("watchers_groups_data", "watcher_id", "group_guid", "=", sSiteGUID);
                if (oWatcherID != null && oWatcherID != DBNull.Value)
                    nWatcherID = int.Parse(oWatcherID.ToString());
            }

            string sVotesCountField = "";
            if (nRateVal > 0 && nRateVal < 6)
                sVotesCountField = "VOTES_" + nRateVal.ToString() + "_COUNT";
            if (nRateVal < 1)
                sVotesCountField = "VOTES_LO_COUNT";
            if (nRateVal > 5)
                sVotesCountField = "VOTES_UP_COUNT";


            Int32 nVoted = TVinciShared.ProtocolsFuncs.UserVoteValueForMedia(nWatcherID, nMediaID, false, true, siteGuid);
            if (nVoted == -1)
            {
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update media set ";
                directQuery += sVotesCountField + "=" + sVotesCountField + "+1";
                directQuery += ",VOTES_SUM=VOTES_SUM+" + nRateVal.ToString() + ",VOTES_COUNT=VOTES_COUNT+1 where";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;

                if (nWatcherID > 0 || siteGuid > 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_media_rating");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RATE_VAL", "=", nRateVal);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", siteGuid);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
            }
            else
            {
                rmo.oStatus.Initialize("User already voted", -1);
                return rmo;
            }

            Int32 nVotesSum = 0;
            Int32 nViews = 0;
            Int32 nVotesCnt = 0;
            double dAvg = 0.0;

            Int32 nVotesUpCnt = 0;
            Int32 nVotesLoCnt = 0;
            Int32 nVotes1Cnt = 0;
            Int32 nVotes2Cnt = 0;
            Int32 nVotes3Cnt = 0;
            Int32 nVotes4Cnt = 0;
            Int32 nVotes5Cnt = 0;

            TVinciShared.ProtocolsFuncs.GetCountersFromMedia(nMediaID, ref nViews, ref nVotesSum, ref nVotesCnt, ref dAvg
                , ref nVotesLoCnt
                , ref nVotesUpCnt
                , ref nVotes1Cnt
                , ref nVotes2Cnt
                , ref nVotes3Cnt
                , ref nVotes4Cnt
                , ref nVotes5Cnt, true);

            rmo.Initialize(nVotesSum, nVotesCnt, dAvg);


            return rmo;
        }

        /*
        public static string InsertAssets(string sXML, int groupID)
        {
            string retVal = string.Empty;
            TvinciImporter.ImporterImpl.DoTheWorkInner(sXML, groupID, string.Empty, ref retVal);
            return retVal;
        }
        */

        private static void AddSubAccountsToAccount(ref AdminAccountObj accountObj)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id, group_name from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", accountObj.m_id);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_archived", "=", 0);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string name = selectQuery.Table("query").DefaultView[i].Row["group_name"].ToString();
                        int id = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                        AdminAccountObj subAccount = new AdminAccountObj(id, name, string.Empty, accountObj.m_id);
                        accountObj.m_subGroups.Add(subAccount);
                        AddSubAccountsToAccount(ref subAccount);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public ApiObjects.AdminAccountUserResponse GetAdminUserAccount(string user, string pass)
        {
            ApiObjects.AdminAccountUserResponse retVal = new AdminAccountUserResponse();

            DataTable dt = DAL.ApiDAL.Get_AdminUserAccount(user, pass);
            if (dt != null)
            {
                int count = dt.DefaultView.Count;
                if (count > 0)
                {
                    int id = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "id");
                    string name = APILogic.Utils.GetSafeStr(dt.Rows[0], "username");
                    string email = APILogic.Utils.GetSafeStr(dt.Rows[0], "email_add");
                    int groupID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "group_id");
                    int parentGroupID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "parent_group_id");
                    string groupName = APILogic.Utils.GetSafeStr(dt.Rows[0], "group_name");
                    AdminAccountUserObj userObj = new AdminAccountUserObj();
                    AdminAccountObj accountObj = new AdminAccountObj(groupID, groupName, string.Empty, parentGroupID);
                    AddSubAccountsToAccount(ref accountObj);
                    userObj.Initialize(id, name, email, groupID, accountObj);
                    retVal.Initialize(AdminUserStatus.OK, userObj);
                }
                else
                {
                    retVal.Initialize(AdminUserStatus.UserDoesNotExist, null);
                }
            }
            return retVal;
        }


        static public int InserEPGScheduleToChannel(int groupID, int channelID, string xml, bool deleteOld)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNodeList programList = xmlDoc.SelectNodes("/tv/programme");
            int count = programList.Count;
            if (deleteOld)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "0");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "2");
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);
                updateQuery.Execute();
                updateQuery = null;
            }
            for (int i = 0; i < count; i++)
            {
                XmlNode programNode = programList[i];
                string startDateStr = XmlUtils.GetItemParameterVal(ref programNode, "start");
                string endDateStr = XmlUtils.GetItemParameterVal(ref programNode, "stop");
                string name = XmlUtils.GetSafeValueFromXML(ref programNode, "title");
                string desc = XmlUtils.GetSafeValueFromXML(ref programNode, "desc");
                DateTime startDate = ParseEPGStrToDate(startDateStr);
                DateTime endDate = ParseEPGStrToDate(endDateStr);
                string epgIdentifier = string.Format("{0}_{1}", channelID.ToString(), i.ToString());

                ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("epg_channels_schedule");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", channelID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", epgIdentifier);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", name);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Description", desc);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Start_Date", startDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("End_Date", endDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", groupID);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            return count;
        }

        static public List<EPGChannelObject> GetEPGChannel(int groupID, string sPicSize)
        {
            List<EPGChannelObject> res = new List<EPGChannelObject>();
            DataTable dt = DAL.ApiDAL.Get_EPGChannel(groupID);
            if (dt != null)
            {
                int count = dt.DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        EPGChannelObject item = new EPGChannelObject();
                        string EPG_CHANNEL_ID = APILogic.Utils.GetSafeStr(dr, "ID");
                        string NAME = APILogic.Utils.GetSafeStr(dr, "NAME");
                        string DESCRIPTION = APILogic.Utils.GetSafeStr(dr, "DESCRIPTION");
                        string ORDER_NUM = APILogic.Utils.GetSafeStr(dr, "ORDER_NUM");
                        string IS_ACTIVE = APILogic.Utils.GetSafeStr(dr, "IS_ACTIVE");
                        string PIC_ID = PageUtils.GetPicURL(APILogic.Utils.GetIntSafeVal(dr, "PIC_ID"), sPicSize);
                        string GROUP_ID = APILogic.Utils.GetSafeStr(dr, "GROUP_ID");
                        string EDITOR_REMARKS = APILogic.Utils.GetSafeStr(dr, "EDITOR_REMARKS");
                        string STATUS = APILogic.Utils.GetSafeStr(dr, "STATUS");
                        string UPDATER_ID = APILogic.Utils.GetSafeStr(dr, "UPDATER_ID");
                        string CREATE_DATE = APILogic.Utils.GetSafeStr(dr, "CREATE_DATE");
                        string PUBLISH_DATE = APILogic.Utils.GetSafeStr(dr, "PUBLISH_DATE");
                        string CHANNEL_ID = APILogic.Utils.GetSafeStr(dr, "CHANNEL_ID");
                        string MEDIA_ID = APILogic.Utils.GetSafeStr(dr, "MEDIA_ID");
                        item.Initialize(EPG_CHANNEL_ID, NAME, DESCRIPTION, ORDER_NUM, IS_ACTIVE, PIC_ID, GROUP_ID, EDITOR_REMARKS, STATUS, UPDATER_ID, CREATE_DATE, PUBLISH_DATE, CHANNEL_ID, MEDIA_ID);
                        res.Add(item);
                        i++;
                    }
                }
            }
            return res;
        }
        static private Dictionary<string, List<EPGDictionary>> GetAllEPGMetaProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseMeta = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment

                ODBCWrapper.DataSetSelectQuery selectMetaQuery = new ODBCWrapper.DataSetSelectQuery();
                selectMetaQuery += " select mt.name as Name, mp.value as Value, mp.program_id as program_id  from epg_program_metas as mp inner join epg_metas_types as mt";
                selectMetaQuery += "on mt.id = mp.epg_meta_id ";
                selectMetaQuery += "where mp.group_id ";
                selectMetaQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectMetaQuery += " and ";
                selectMetaQuery += string.Format("mp.program_id {0}", programIDSQL);
                selectMetaQuery += " and ";
                selectMetaQuery += "mp.status=1 and mt.status=1 and mt.is_active=1";

                if (selectMetaQuery.Execute("query", true) != null)
                {
                    int countMeta = selectMetaQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drMeta in selectMetaQuery.Table("query").DefaultView)
                        {
                            EPGDictionary metaItem = new EPGDictionary();
                            metaItem.Key = ODBCWrapper.Utils.GetSafeStr(drMeta["Name"]);
                            metaItem.Value = ODBCWrapper.Utils.GetSafeStr(drMeta["Value"]);

                            if (EPG_ResponseMeta.ContainsKey(drMeta["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseMeta[drMeta["program_id"].ToString()];
                                temp.Add(metaItem);
                                EPG_ResponseMeta[drMeta["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(metaItem);
                                EPG_ResponseMeta.Add(drMeta["program_id"].ToString(), temp);
                            }
                        }
                    }
                }
                selectMetaQuery.Finish();
                selectMetaQuery = null;
            }
            return EPG_ResponseMeta;
        }
        static private List<EPGDictionary> GetEPGMetasData(int nGroupID, string ProgramID)
        {

            List<EPGDictionary> EPG_ResponseMeta = new List<EPGDictionary>();



            ODBCWrapper.DataSetSelectQuery selectMetaQuery = new ODBCWrapper.DataSetSelectQuery();
            selectMetaQuery += " select mt.name as Name , mp.value as Value  from epg_program_metas as mp inner join epg_metas_types as mt";
            selectMetaQuery += "on mt.id = mp.epg_meta_id ";
            selectMetaQuery += "where mp.group_id ";
            selectMetaQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
            selectMetaQuery += " and ";
            selectMetaQuery += ODBCWrapper.Parameter.NEW_PARAM("mp.program_id", "=", ProgramID);
            selectMetaQuery += " and ";
            selectMetaQuery += "mp.status=1 and mt.status=1 and mt.is_active=1";

            if (selectMetaQuery.Execute("query", true) != null)
            {
                int countMeta = selectMetaQuery.Table("query").DefaultView.Count;
                if (countMeta > 0)
                {
                    foreach (DataRowView drMeta in selectMetaQuery.Table("query").DefaultView)
                    {
                        EPGDictionary metaItem = new EPGDictionary();
                        metaItem.Key = ODBCWrapper.Utils.GetSafeStr(drMeta["Name"]);
                        metaItem.Value = ODBCWrapper.Utils.GetSafeStr(drMeta["Value"]);
                        EPG_ResponseMeta.Add(metaItem);
                    }
                }
            }
            selectMetaQuery.Finish();
            selectMetaQuery = null;
            return EPG_ResponseMeta;
        }
        static private Dictionary<string, List<EPGDictionary>> GetAllEPGTagProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseTag = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment               

                ODBCWrapper.DataSetSelectQuery selectTagsQuery = new ODBCWrapper.DataSetSelectQuery();
                selectTagsQuery += " select tt.name as Name, tv.value as Value, tp.program_id as program_id from epg_program_tags as tp inner join epg_tags as tv on tp.epg_tag_id = tv.id";
                selectTagsQuery += "inner join epg_tags_types as tt on tv.epg_tag_type_id = tt.id ";
                selectTagsQuery += "where tp.group_id ";
                selectTagsQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectTagsQuery += " and ";
                selectTagsQuery += string.Format("tp.program_id {0}", programIDSQL);
                selectTagsQuery += " and ";
                selectTagsQuery += "tp.status=1 and tv.status=1 and tt.status=1 and tt.is_active=1";

                if (selectTagsQuery.Execute("query", true) != null)
                {
                    int countMeta = selectTagsQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drTag in selectTagsQuery.Table("query").DefaultView)
                        {
                            EPGDictionary tagItem = new EPGDictionary();
                            tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag["Name"]);
                            tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag["Value"]);
                            if (EPG_ResponseTag.ContainsKey(drTag["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseTag[drTag["program_id"].ToString()];
                                temp.Add(tagItem);
                                EPG_ResponseTag[drTag["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(tagItem);
                                EPG_ResponseTag.Add(drTag["program_id"].ToString(), temp);
                            }

                        }
                    }
                }
                selectTagsQuery.Finish();
                selectTagsQuery = null;
            }
            return EPG_ResponseTag;
        }
        static private List<EPGDictionary> GetEPGTagsData(int nGroupID, string ProgramID)
        {
            List<EPGDictionary> EPG_ResponseTag = new List<EPGDictionary>();

            ODBCWrapper.DataSetSelectQuery selectTagsQuery = new ODBCWrapper.DataSetSelectQuery();
            selectTagsQuery += " select tt.name as Name, tv.value as Value from epg_program_tags as tp inner join epg_tags as tv on tp.epg_tag_id = tv.id";
            selectTagsQuery += "inner join epg_tags_types as tt on tv.epg_tag_type_id = tt.id ";
            selectTagsQuery += "where tp.group_id ";
            selectTagsQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
            selectTagsQuery += " and ";
            selectTagsQuery += ODBCWrapper.Parameter.NEW_PARAM("tp.program_id", "=", ProgramID);
            selectTagsQuery += " and ";
            selectTagsQuery += "tp.status=1 and tv.status=1 and tt.status=1 and tt.is_active=1";

            if (selectTagsQuery.Execute("query", true) != null)
            {
                int countMeta = selectTagsQuery.Table("query").DefaultView.Count;
                if (countMeta > 0)
                {
                    foreach (DataRowView drTag in selectTagsQuery.Table("query").DefaultView)
                    {
                        EPGDictionary tagItem = new EPGDictionary();
                        tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag["Name"]);
                        tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag["Value"]);
                        EPG_ResponseTag.Add(tagItem);
                    }
                }
            }
            selectTagsQuery.Finish();
            selectTagsQuery = null;
            return EPG_ResponseTag;

        }

        static public List<EPGChannelProgrammeObject> GetEPGChannelPrograms_Old(Int32 groupID, string sEPGChannelID, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();


            DateTime fromOffsetDay;
            DateTime toOffsetDay;

            DateTime UTCOffsetFromTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            DateTime UTCOffsetToTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            switch (unit)
            {
                case EPGUnit.Days:
                    DateTime fromDay = UTCOffsetFromTimeZone.AddDays(nFromOffsetUnit);
                    DateTime toDay = UTCOffsetToTimeZone.AddDays(nToOffsetUnit);
                    fromOffsetDay = new DateTime(fromDay.Year, fromDay.Month, fromDay.Day, 00, 00, 00);
                    toOffsetDay = new DateTime(toDay.Year, toDay.Month, toDay.Day, 23, 59, 59);

                    #region Get EPG Program Schedule
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += " select * from epg_channels_schedule where group_id ";
                    selectQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectQuery += " and status = 1 and is_active = 1 ";
                    selectQuery += " order by START_DATE asc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            int i = 0;
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectQuery.Table("query"));
                            foreach (DataRowView dr in selectQuery.Table("query").DefaultView)
                            {

                                long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "PIC_ID", i), sPicSize, "epg_pics");
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id.ToString()
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id.ToString()
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();



                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    #endregion

                    break;
                case EPGUnit.Hours:

                    fromOffsetDay = UTCOffsetFromTimeZone.AddHours(nFromOffsetUnit);
                    toOffsetDay = UTCOffsetToTimeZone.AddHours(nToOffsetUnit);

                    #region Get EPG Program Schedule
                    ODBCWrapper.DataSetSelectQuery selectHourQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectHourQuery += " select * from epg_channels_schedule where group_id ";
                    selectHourQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectHourQuery += " and status = 1 and is_active = 1 ";
                    selectHourQuery += " order by START_DATE asc";
                    if (selectHourQuery.Execute("query", true) != null)
                    {
                        int count = selectHourQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectHourQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectHourQuery.Table("query"));

                            int i = 0;
                            foreach (DataRowView dr in selectHourQuery.Table("query").DefaultView)
                            {
                                string program_id = ODBCWrapper.Utils.GetSafeStr(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectHourQuery, "PIC_ID", i), sPicSize);
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(0, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectHourQuery.Finish();
                    selectHourQuery = null;
                    #endregion
                    break;

                case EPGUnit.Current:
                    fromOffsetDay = UTCOffsetFromTimeZone;
                    toOffsetDay = UTCOffsetToTimeZone;

                    #region Get EPG Program Schedule
                    if (nFromOffsetUnit > 0)
                    {
                        #region Get Befor Programmes
                        ODBCWrapper.DataSetSelectQuery selectBeforQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectBeforQuery += string.Format(" select TOP({0}) * from epg_channels_schedule where group_id ", nFromOffsetUnit);
                        selectBeforQuery += PageUtils.GetAllGroupTreeStr(groupID);
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "<", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                        selectBeforQuery += " and status = 1 and is_active = 1 ";
                        selectBeforQuery += " order by END_DATE desc";
                        if (selectBeforQuery.Execute("query", true) != null)
                        {
                            int count = selectBeforQuery.Table("query").DefaultView.Count;
                            if (count > 0)
                            {
                                int i = 0;

                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectBeforQuery.Table("query"));
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectBeforQuery.Table("query"));

                                foreach (DataRowView dr in selectBeforQuery.Table("query").DefaultView)
                                {
                                    long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                    string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                    string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                    string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                    string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                    DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "START_DATE", i);
                                    string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "END_DATE", i);
                                    string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectBeforQuery, "PIC_ID", i), sPicSize);
                                    string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                    string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                    string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                    string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                    string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                    string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                    string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                    string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                    int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                                    List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                           where t.Key == program_id.ToString()
                                                                           select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                    List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                            where t.Key == program_id.ToString()
                                                                            select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                    EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                    item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                    res.Add(item);
                                    i++;
                                }
                            }

                        }
                        selectBeforQuery.Finish();
                        selectBeforQuery = null;
                        #endregion
                    }
                    #region Get Current Program
                    ODBCWrapper.DataSetSelectQuery selectUnitQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectUnitQuery += " select * from epg_channels_schedule where group_id ";
                    selectUnitQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectUnitQuery += " and status = 1 and is_active = 1 ";
                    selectUnitQuery += " order by START_DATE asc";
                    if (selectUnitQuery.Execute("query", true) != null)
                    {
                        int count = selectUnitQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            int i = 0;
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectUnitQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectUnitQuery.Table("query"));

                            foreach (DataRowView dr in selectUnitQuery.Table("query").DefaultView)
                            {
                                long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectUnitQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectUnitQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");


                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectUnitQuery, "PIC_ID", i), sPicSize);
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id.ToString()
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id.ToString()
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectUnitQuery.Finish();
                    selectUnitQuery = null;
                    #endregion

                    if (nToOffsetUnit > 0)
                    {
                        #region Get after Programmes
                        ODBCWrapper.DataSetSelectQuery selectBeforQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectBeforQuery += string.Format(" select TOP({0}) * from epg_channels_schedule where group_id ", nToOffsetUnit);
                        selectBeforQuery += PageUtils.GetAllGroupTreeStr(groupID);
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", ">", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                        selectBeforQuery += " and status = 1 and is_active = 1 ";
                        selectBeforQuery += " order by START_DATE asc";

                        if (selectBeforQuery.Execute("query", true) != null)
                        {
                            int count = selectBeforQuery.Table("query").DefaultView.Count;
                            if (count > 0)
                            {
                                int i = 0;
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectBeforQuery.Table("query"));
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectBeforQuery.Table("query"));
                                foreach (DataRowView dr in selectBeforQuery.Table("query").DefaultView)
                                {
                                    long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                    string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                    string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                    string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                    string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                    DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "START_DATE", i);
                                    string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "END_DATE", i);
                                    string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectBeforQuery, "PIC_ID", i), sPicSize);
                                    string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                    string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                    string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                    string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                    string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                    string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                    string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                    string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                    int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                    List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                           where t.Key == program_id.ToString()
                                                                           select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                    List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                            where t.Key == program_id.ToString()
                                                                            select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                    EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                    item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                    res.Add(item);
                                    i++;
                                }
                            }

                        }
                        selectBeforQuery.Finish();
                        selectBeforQuery = null;
                        #endregion
                    }
                    #endregion
                    break;

            }



            return res;
        }

        static public List<EPGChannelProgrammeObject> GetEPGChannelProgramsByDates_Old(Int32 groupID, string sEPGChannelID, string sPicSize, DateTime fromDay, DateTime toDay, double nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            DateTime fromUTCDay = fromDay.AddHours(nUTCOffset);
            DateTime toUTCDay = toDay.AddHours(nUTCOffset);

            ODBCWrapper.DataSetSelectQuery selectHourQuery = new ODBCWrapper.DataSetSelectQuery();
            selectHourQuery += " select * from epg_channels_schedule where group_id ";
            selectHourQuery += PageUtils.GetAllGroupTreeStr(groupID);
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromUTCDay.ToString("yyyy-MM-dd HH:mm:ss"));
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toUTCDay.ToString("yyyy-MM-dd HH:mm:ss"));
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
            selectHourQuery += " and status = 1 and is_active = 1 ";
            selectHourQuery += " order by START_DATE asc";
            if (selectHourQuery.Execute("query", true) != null)
            {
                int count = selectHourQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectHourQuery.Table("query"));
                    Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectHourQuery.Table("query"));

                    int i = 0;
                    foreach (DataRowView dr in selectHourQuery.Table("query").DefaultView)
                    {
                        long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                        string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                        string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                        string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                        string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                        DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "START_DATE", i);
                        string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                        DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "END_DATE", i);
                        string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                        string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectHourQuery, "PIC_ID", i), sPicSize);
                        string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                        string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                        string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                        string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                        string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                        string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                        string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                        string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                        int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                        List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                               where t.Key == program_id.ToString()
                                                               select t.Value).FirstOrDefault<List<EPGDictionary>>();

                        List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                where t.Key == program_id.ToString()
                                                                select t.Value).FirstOrDefault<List<EPGDictionary>>();


                        EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                        item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                        res.Add(item);
                        i++;
                    }
                }

            }
            selectHourQuery.Finish();
            selectHourQuery = null;
            return res;
        }

        static private DateTime ParseEPGStrToDate(string dateStr)
        {
            DateTime dt = new DateTime();
            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));
            int hour = int.Parse(dateStr.Substring(8, 2));
            int min = int.Parse(dateStr.Substring(10, 2));
            int sec = int.Parse(dateStr.Substring(12, 2));
            dt = new DateTime(year, month, day, hour, min, sec);
            return dt;
        }

        static public bool AddUserSocialAction(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, Int32 nSocialAction, Int32 nSocialPlatform)
        {
            bool res = true;

            SocialAction eSocialAction = (SocialAction)nSocialAction;
            SocialPlatform eSocialPlatform = (SocialPlatform)nSocialPlatform;

            switch (eSocialAction)
            {
                case SocialAction.LIKE:
                    {
                        Int32 nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, eSocialAction, eSocialPlatform);
                        if (nID > 0)
                        {
                            //Update Like - set is_active=1
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;

                            //Update UnLike - set is_active=0
                            updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)SocialAction.UNLIKE);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_social_actions");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)eSocialAction);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }

                        UpdateMediaLikeCounter(nMediaID, 1);

                        break;
                    }
                case SocialAction.UNLIKE:
                    {
                        //Update Like - set is_active=0
                        Int32 nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, SocialAction.LIKE, eSocialPlatform);
                        if (nID > 0)
                        {
                            //Update Like - set is_active=1
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }

                        //Update UnLike
                        nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, eSocialAction, eSocialPlatform);
                        if (nID > 0)
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);

                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_social_actions");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)eSocialAction);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }

                        UpdateMediaLikeCounter(nMediaID, -1);

                        break;
                    }
                default:
                    {
                        res = false;
                        break;
                    }
            }

            return res;
        }

        static private Int32 GetUserSocialActionID(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, SocialAction eSocialAction, SocialPlatform eSocialPlatform)
        {
            Int32 nID = 0;

            DataTable dt = DAL.ApiDAL.Get_UserSocialActionID(nMediaID, sSiteGUID, nGroupID, (int)eSocialPlatform, (int)eSocialAction);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    nID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "id");
                }
            }
            return nID;
        }

        static private Int32 UpdateMediaLikeCounter(Int32 nMediaID, int nVal)
        {
            Int32 nlikeCounter = 0;

            object oLikeCounter = ODBCWrapper.Utils.GetTableSingleVal("media", "like_counter", nMediaID);
            if (oLikeCounter != null && oLikeCounter != DBNull.Value)
                nlikeCounter = int.Parse(oLikeCounter.ToString());


            nlikeCounter += nVal;

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("like_counter", "=", nlikeCounter);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            return nlikeCounter;
        }

        static public bool RunImporter(Int32 nGroupID, string extraParams)
        {
            return false;
            //ScheduledTasks.BaseTask t = null;
            //if (nGroupID == 134 || nGroupID == 135 || nGroupID == 136)
            //{
            //    t = ElisaFeeder.ElisaFeeder.GetInstance(0, 0, extraParams);
            //}

            ////Unknown
            //else
            //{
            //    return false;
            //}

            //return t.DoTheTask();
        }

        protected Int32 m_nGroupID;

        static private Int32 GetIPCountryCode(string sIP)
        {
            Int32 nCountry = 0;
            if (sIP == "127.0.0.1")
                nCountry = 18;
            else if (sIP != "")
            {
                string[] splited = sIP.Split('.');

                Int64 nIPVal = Int64.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
                Int32 nID = 0;

                DataTable dt = DAL.ApiDAL.Get_IPCountryCode(nIPVal);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        nCountry = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "COUNTRY_ID");
                        nID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "ID");
                    }
                }
            }
            return nCountry;
        }

        static public string CheckGeoBlockMedia(Int32 nGroupID, Int32 nMediaID, string sIP)
        {
            string res = "Geo";
            Int32 nGeoBlockID = 0;
            int nProxyRule = 0;
            double dProxyLevel = 0.0;

            Int32 nCountryID = GetIPCountryCode(sIP);

            //call Dal layer 
            DataTable dt = DAL.ApiDAL.Get_GeoBlockPerMedia(nGroupID, nMediaID);

            if (dt != null)
            {
                //       comment.m_nMediaID = Utils.GetIntSafeVal(ds.Tables[1].Rows[i], "MEDIA_ID");
                bool bAllowed = false;
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    nGeoBlockID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "BLOCK_TEMPLATE_ID");
                    bAllowed = true;
                }

                bool bExsitInRuleM2M = false;
                Logger.Logger.Log("Geo Blocks", "Geo Block ID " + nGeoBlockID + " Country ID " + nCountryID, "Geo Block");
                if (nGeoBlockID != 0)
                {
                    Int32 nONLY_OR_BUT = 0;
                    nONLY_OR_BUT = int.Parse(PageUtils.GetTableSingleVal("geo_block_types", "ONLY_OR_BUT", nGeoBlockID).ToString());
                    bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(nGeoBlockID, nCountryID);

                    //No one except
                    if (nONLY_OR_BUT == 0)
                        bAllowed = bExsitInRuleM2M;
                    //All except
                    if (nONLY_OR_BUT == 1)
                        bAllowed = !bExsitInRuleM2M;


                    if (bAllowed) // then check what about the proxy - is it reliable 
                    {
                        nProxyRule = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "PROXY_RULE");
                        dProxyLevel = APILogic.Utils.GetDoubleSafeVal(dt.Rows[0], "PROXY_LEVEL");
                        bAllowed = MaxMind.IsProxyAllowed(nProxyRule, dProxyLevel, sIP, nGroupID);
                    }


                }
                if (bAllowed)
                {
                    res = "OK";
                }
            }
            return res;
        }


        static public bool CheckMediaUserType(Int32 nMediaID, int nSiteGuid)
        {
            bool result = true;
            int nUserTypeID = 0;

            DataTable dtUserType = DAL.UsersDal.GetUserBasicData(nSiteGuid);
            if (dtUserType != null && dtUserType.Rows.Count > 0)
            {
                nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(dtUserType.Rows[0]["user_type_id"]);
            }
            result = DAL.ApiDAL.Is_MediaExistsToUserType(nMediaID, nUserTypeID);

            return result;
        }

        public static bool SendToFriend(int nGroupID, string sSenderName, string sSendaerMail, string sMailTo, string sNameTo, int nMediaID)
        {
            bool retVal = false;
            string sMediaName = string.Empty;
            string sMediaType = string.Empty;
            string sMailSubject = string.Empty;
            string sMailTemplate = string.Empty;

            DataSet ds = DAL.ApiDAL.Get_MediaDetailsForEmail(nMediaID, nGroupID);
            if (ds != null)
            {
                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        sMediaName = ds.Tables[0].Rows[0]["Name"].ToString();
                        sMediaType = ds.Tables[0].Rows[0]["MediaType"].ToString();
                    }
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            sMailTemplate = ds.Tables[1].Rows[0]["mail_template"].ToString();
                            sMailSubject = ds.Tables[1].Rows[0]["mail_subject"].ToString();
                        }
                    }
                }
            }

            SendToFriendMailRequest request = new SendToFriendMailRequest();
            request.m_eMailType = eMailTemplateType.SendToFriend;
            request.m_sContentName = sMediaName;
            request.m_sFirstName = sNameTo;
            request.m_sMediaID = nMediaID.ToString();
            request.m_sMediaType = HttpUtility.UrlEncode(sMediaType);
            request.m_sSenderFrom = sSendaerMail;
            request.m_sSenderName = sSenderName;
            request.m_sSenderTo = sMailTo;
            request.m_sSubject = sMailSubject;
            request.m_sTemplateName = sMailTemplate;
            retVal = SendMailTemplate(request);
            return retVal;
        }

        static public List<GroupRule> GetGroupMediaRules(int nMediaID, string sIP, int nSiteGuid, int nGroupID, string deviceUdid)
        {
            List<GroupRule> tempRules = new List<GroupRule>();
            List<GroupRule> retRules = new List<GroupRule>();


            DataTable dt = DAL.ApiDAL.Get_GroupMediaRules(nMediaID, nSiteGuid.ToString());
            GetRules(dt, eGroupRuleType.Parental, nSiteGuid, nMediaID, nGroupID, sIP, ref retRules);

            return retRules;
        }

        private static bool CheckAgeValidation(int ageLimit, int userID)
        {
            bool retVal = false;
            DataTable dt = DAL.ApiDAL.Get_DetailsUsersDynamicData(userID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    int count = dt.DefaultView.Count;
                    int birthDay = 0;
                    int birthMonth = 0;
                    int birthYear = 0;
                    DateTime birthDate = DateTime.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        string dataType = dt.Rows[i]["data_type"].ToString();
                        string dataValue = dt.Rows[i]["data_value"].ToString();
                        if (dataType.ToLower().Equals("birthday"))
                        {
                            if (dataValue.ToLower().Contains("/"))
                            {
                                if (DateTime.TryParse(dataValue.ToLower(), out birthDate))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                birthDay = int.Parse(dataValue);
                            }
                        }
                        else if (dataType.ToLower().Equals("birthmonth"))
                        {
                            birthMonth = int.Parse(dataValue);
                        }
                        else if (dataType.ToLower().Equals("birthyear"))
                        {
                            birthYear = int.Parse(dataValue);
                        }

                    }
                    if (birthDate == DateTime.MaxValue)
                    {
                        if (birthYear != 0 && birthYear != 0)
                        {
                            birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        }
                    }
                    if (birthDate != DateTime.MaxValue)
                    {
                        if (birthDate.CompareTo(DateTime.Now.AddYears(-ageLimit)) < 0)
                        {
                            retVal = true;
                        }
                    }
                }
            }

            return retVal;
        }

        static public bool SendMailTemplate(MailRequestObj request)
        {
            bool retVal = false;
            Mailer.IMailer mailer = Mailer.MailFactory.GetMailer(Mailer.MailImplementors.MCMailer);
            retVal = mailer.SendMailTemplate(request);
            return retVal;
        }


        static public List<GroupRule> GetGroupRules(int nGroupID)
        {
            List<GroupRule> retRules = new List<GroupRule>();

            DataTable dt = DAL.ApiDAL.Get_GroupRules(nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    DataTable ruleTypeDt = dt.DefaultView.ToTable(true, "ID", "Name", "TAG_TYPE_ID", "dynamic_data_key", "Order_Num", "group_rule_type_id");
                    GroupRule gr;
                    foreach (DataRow dr in ruleTypeDt.Rows)
                    {
                        gr = new GroupRule();
                        int nGroupRuleType = APILogic.Utils.GetIntSafeVal(dr, "group_rule_type_id");
                        gr.RuleID = APILogic.Utils.GetIntSafeVal(dr, "ID");
                        gr.Name = APILogic.Utils.GetSafeStr(dr, "Name");
                        gr.TagTypeID = APILogic.Utils.GetIntSafeVal(dr, "TAG_TYPE_ID");
                        gr.DynamicDataKey = APILogic.Utils.GetSafeStr(dr, "dynamic_data_key");
                        gr.OrderNum = APILogic.Utils.GetIntSafeVal(dr, "ORDER_NUM");
                        gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
                        gr.AllTagValues = new List<string>();
                        gr.IsActive = true;
                        DataRow[] drArr = dt.Select("ID=" + gr.RuleID);
                        foreach (DataRow tagIdDr in drArr)
                        {
                            gr.AllTagValues.Add(tagIdDr["Value"].ToString());
                        }
                        retRules.Add(gr);
                    }
                }
            }

            return retRules.OrderBy(gr => gr.OrderNum).ToList();

        }

        #region User Group Rules

        static public List<GroupRule> GetUserGroupRules(int nGroupID, string sSiteGuid)
        {
            Dictionary<int, GroupRule> groupRulesDict = new Dictionary<int, GroupRule>();

            DataTable dtGroupRules = DAL.ApiDAL.Get_UserGroupRules(sSiteGuid);

            if (dtGroupRules != null)
            {
                foreach (DataRow drGroupRule in dtGroupRules.Rows)
                {
                    GroupRule gr = new GroupRule();
                    int nGroupRuleType = APILogic.Utils.GetIntSafeVal(drGroupRule, "group_rule_type_id");
                    gr.RuleID = APILogic.Utils.GetIntSafeVal(drGroupRule, "rule_id");
                    gr.IsActive = (APILogic.Utils.GetIntSafeVal(drGroupRule, "is_active") == 1) ? true : false;
                    gr.Name = APILogic.Utils.GetSafeStr(drGroupRule, "name");
                    gr.TagTypeID = APILogic.Utils.GetIntSafeVal(drGroupRule, "tag_type_id");
                    gr.DynamicDataKey = APILogic.Utils.GetSafeStr(drGroupRule, "dynamic_data_key");
                    gr.OrderNum = APILogic.Utils.GetIntSafeVal(drGroupRule, "order_num");
                    gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
                    gr.AllTagValues = new List<string>();
                    groupRulesDict.Add(gr.RuleID, gr);
                }
            }

            if (groupRulesDict.Count > 0)
            {
                List<GroupRule> parentalGroupRules = groupRulesDict.Values.ToList().Where(x => x.GroupRuleType == eGroupRuleType.Parental).ToList();
                List<int> ruleIDsList = parentalGroupRules.Select(x => x.RuleID).ToList();
                DataTable dtGroupRulesTagsValues = DAL.ApiDAL.GetGroupRulesTagsValues(ruleIDsList);
                if (dtGroupRulesTagsValues != null)
                {
                    foreach (DataRow drTagValue in dtGroupRulesTagsValues.Rows)
                    {
                        int ruleID = APILogic.Utils.GetIntSafeVal(drTagValue, "ruleID");
                        string tagValue = APILogic.Utils.GetSafeStr(drTagValue, "Value");

                        if (groupRulesDict.ContainsKey(ruleID) == true && groupRulesDict[ruleID] != null)
                        {
                            groupRulesDict[ruleID].AllTagValues.Add(tagValue);
                        }
                    }
                }
            }
            return groupRulesDict.Values.ToList();
        }

        static public bool SetUserGroupRule(string sSiteGUID, int nRuleID, int nStatus, string sPIN, int nGroupID)
        {
            DataTable dt = DAL.ApiDAL.Get_UserGroupRule(nGroupID, sSiteGUID, nRuleID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    int m_nUserRuleID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "rule_id");
                    int m_nIsActive = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "is_active");
                    string m_sPIN = dt.Rows[0]["code"].ToString();

                    return UpdateUserGroupRule(sSiteGUID, m_nUserRuleID, nStatus, true, sPIN);
                }
                else
                {
                    return InsertNewUserGroupRule(sSiteGUID, nRuleID, nStatus, sPIN, nGroupID);
                }

            }
            return false;           
        }

        private static bool UpdateUserGroupRule(string sSiteGuid, int nUserRuleID, int nIsActive, bool bWithPIN, string sPIN)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_group_rules");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
            if (bWithPIN)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
            }
            updateQuery += "WHERE";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGuid);
            updateQuery += "AND";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nUserRuleID);
            updateQuery += "AND";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

            bool ret = updateQuery.Execute();
            updateQuery.Finish();
            return ret;
        }

        private static bool InsertNewUserGroupRule(string sSiteGuid, int nRuleID, int nStatus, string sPIN, int nGroupID)
        {
            bool ret = false;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT id FROM groups_rules WHERE status=1 AND is_active=1 AND";
            selectQuery += "group_id " + PageUtils.GetAllGroupTreeStr(nGroupID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRuleID);

            if (selectQuery.Execute("query", true) != null)
            {
                if (selectQuery.Table("query").DefaultView.Count > 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_group_rules");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGuid);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nRuleID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nStatus);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                    ret = insertQuery.Execute();
                    insertQuery.Finish();
                }
            }
            selectQuery.Finish();
            return ret;
        }

        #endregion

        #region Domain Group Rules

        public static List<GroupRule> GetUserDomainGroupRules(int nGroupID, string sSiteGuid, int nDomainID)
        {
            Dictionary<int, GroupRule> groupRulesDict = new Dictionary<int, GroupRule>();

            DataTable dtGroupRules = string.IsNullOrEmpty(sSiteGuid) ? DAL.ApiDAL.GetDomainGroupRules(nDomainID) : DAL.ApiDAL.Get_UserGroupRules(sSiteGuid);

            if (dtGroupRules != null && dtGroupRules.Rows.Count > 0)
            {
                foreach (DataRow drGroupRule in dtGroupRules.Rows)
                {
                    GroupRule gr = new GroupRule();

                    int nGroupRuleType = APILogic.Utils.GetIntSafeVal(drGroupRule, "group_rule_type_id");
                    gr.RuleID = APILogic.Utils.GetIntSafeVal(drGroupRule, "rule_id");
                    gr.IsActive = (APILogic.Utils.GetIntSafeVal(drGroupRule, "is_active") == 1);
                    gr.Name = APILogic.Utils.GetSafeStr(drGroupRule, "name");
                    gr.TagTypeID = APILogic.Utils.GetIntSafeVal(drGroupRule, "tag_type_id");
                    gr.DynamicDataKey = APILogic.Utils.GetSafeStr(drGroupRule, "dynamic_data_key");
                    gr.OrderNum = APILogic.Utils.GetIntSafeVal(drGroupRule, "order_num");
                    gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
                    gr.AllTagValues = new List<string>();

                    groupRulesDict.Add(gr.RuleID, gr);
                }
            }

            if (groupRulesDict.Count > 0)
            {
                List<GroupRule> parentalGroupRules = groupRulesDict.Values.ToList().Where(x => x.GroupRuleType == eGroupRuleType.Parental).ToList();
                List<int> ruleIDsList = parentalGroupRules.Select(x => x.RuleID).ToList();
                DataTable dtGroupRulesTagsValues = DAL.ApiDAL.GetGroupRulesTagsValues(ruleIDsList);

                if (dtGroupRulesTagsValues != null)
                {
                    foreach (DataRow drTagValue in dtGroupRulesTagsValues.Rows)
                    {
                        int ruleID = APILogic.Utils.GetIntSafeVal(drTagValue, "ruleID");
                        string tagValue = APILogic.Utils.GetSafeStr(drTagValue, "Value");

                        if (groupRulesDict.ContainsKey(ruleID) == true && groupRulesDict[ruleID] != null)
                        {
                            groupRulesDict[ruleID].AllTagValues.Add(tagValue);
                        }
                    }
                }
            }

            return groupRulesDict.Values.ToList();
        }

        public static bool SetDomainGroupRule(int nDomainID, int nRuleID, int nStatus, string sPIN, int nGroupID)
        {
            bool ret = false;
            string[] dbRule = DAL.ApiDAL.GetDomainGroupRule(nGroupID, nDomainID, nRuleID);

            if (dbRule != null && dbRule.Length > 0)
            {
                int ruleID = APILogic.Utils.GetIntSafeVal(dbRule[0]);
                int isActive = APILogic.Utils.GetIntSafeVal(dbRule[1]);
                string pin = dbRule[2];

                ret = UpdateDomainGroupRule(nDomainID, ruleID, nStatus, true, sPIN);
            }
            else
            {
                ret = InsertNewDomainGroupRule(nDomainID, nRuleID, nStatus, sPIN, nGroupID);
            }

            return ret;
        }

        private static bool InsertNewDomainGroupRule(int nDomainID, int nRuleID, int nIsActive, string sPIN, int nGroupID)
        {
            string sInGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            bool ret = DAL.ApiDAL.InsertNewDomainGroupRule(sInGroups, nDomainID, nRuleID, sPIN, nIsActive, 1);
            return ret;
        }

        private static bool UpdateDomainGroupRule(int nDomainID, int nRuleID, int nIsActive, bool bWithPIN, string sPIN)
        {
            bool bUpdated = DAL.ApiDAL.UpdateDomainGroupRule(nDomainID, nRuleID, nIsActive, 1, bWithPIN, sPIN);
            return bUpdated;
        }

        #endregion

        static public GroupOperator[] GetGroupOperators(int nGroupID, string sScope = "")
        {
            DataTable dt = DAL.ApiDAL.Get_GroupOperatorsDetails(nGroupID);
                     
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {

                    return dt.Rows.OfType<DataRow>()
                        // GroupBy to select only operators     
                        .GroupBy(
                                 dr =>
                                 new
                                     {
                                         ID = dr.Field<int>("ID"),
                                         Name = dr.Field<string>("Name"),
                                         Type = dr.Field<int>("Type"),
                                         CoGuid = dr.Field<string>("Client_Id"),
                                         LoginUrl = dr.Field<string>("Operator_Login"),
                                         LogoutUrl = dr.Field<string>("Operator_Login")
                                     })

                             .Select(dr => new GroupOperator()
                             {
                                 ID = dr.Key.ID,
                                 Name = dr.Key.Name,
                                 Type = (eOperatorType)dr.Key.Type,
                                 CoGuid = dr.Key.CoGuid,
                                 LoginUrl =

                                     string.IsNullOrEmpty(sScope) ?

                                     dr.Key.LoginUrl
                                     :

                                     // if provider has scopes:
                                     dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Any() ?

                                     // True: Set the LoginUrl to the scope's LoginUrl
                                         dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Select(r => r["Scope_Login"].ToString()).FirstOrDefault()
                                         :

                                     // False: set the LoginUrl to the provider's LoginUrl
                                         dr.Key.LoginUrl,
                                 LogoutURL = string.IsNullOrEmpty(sScope) ?

                                     dr.Key.LogoutUrl
                                     :

                                     // if provider has scopes:
                                     dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Any() ?

                                     // True: Set the LoginUrl to the scope's LoginUrl
                                         dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Select(r => r["Scope_Logout"].ToString()).FirstOrDefault()
                                         :

                                     // False: set the LoginUrl to the provider's LoginUrl
                                         dr.Key.LogoutUrl,
                                 Scopes =
                                           string.IsNullOrEmpty(sScope) ?

                                               dt.Select("ID=" + dr.Key.ID).Select(r => new Scope
                                               {
                                                   Name = r["Scope"].ToString(),
                                                   LoginUrl = r["Scope_Login"].ToString()
                                               }
                                                                                   ).ToArray()
                                               :

                                               null

                             })
                        .ToArray();

                }
                else return new GroupOperator[0];
            }
            else
            {
                return null;
            }
        }

        public static bool CheckParentalPIN(string sSiteGUID, int nRuleID, string sParentalPIN)
        {
            bool retVal = false;

            //string dbPIN = DAL.ApiDAL.GetCodeForParentalPIN(sSiteGUID, nRuleID);

            DataTable dt = DAL.ApiDAL.Get_CodeForParentalPIN(sSiteGUID, nRuleID);
         
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    if (dt.Rows[0]["code"].ToString() == sParentalPIN)
                    {
                        retVal = true;
                    }
                }
            }
            return retVal;
        }

        public static bool CheckDomainParentalPIN(int nDomainID, int nRuleID, string sParentalPIN)
        {
            return CheckParentalPIN(null, nDomainID, nRuleID, sParentalPIN);
        }

        public static bool CheckParentalPIN(string sSiteGUID, int nDomainID, int nRuleID, string sParentalPIN)
        {
            bool retVal = false;

            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                retVal = CheckParentalPIN(sSiteGUID, nRuleID, sParentalPIN);
            }
            else
            {
                string dbPIN = DAL.ApiDAL.GetDomainCodeForParentalPIN(nDomainID, nRuleID);
                retVal = (string.Compare(dbPIN, sParentalPIN, false) == 0);
            }

            return retVal;
        }


        public static bool SetDefaultRules(string sSiteGuid, int nGroupID)
        {           
            DataTable dt = DAL.ApiDAL.Get_DefaultRules(nGroupID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        bool bSuccsess = SetUserGroupRule(sSiteGuid, APILogic.Utils.GetIntSafeVal(dr, "ID"), APILogic.Utils.GetIntSafeVal(dr, "default_enabled"), dr["default_val"].ToString(), nGroupID);
                        if (!bSuccsess)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else return true;
            }
            else return false;
        }

        public static bool SetRuleState(string sSiteGuid, int nDomainID, int nRuleID, int nStatus)
        {
            if (!string.IsNullOrEmpty(sSiteGuid))
            {
                return UpdateUserGroupRule(sSiteGuid, nRuleID, nStatus, false, string.Empty);
            }
            else
            {
                return UpdateDomainGroupRule(nDomainID, nRuleID, nStatus, false, string.Empty);
            }
        }

        public static DeviceAvailabiltyRule GetAvailableDevices(int nMediaID, int nGroupID)
        {
            DeviceAvailabiltyRule retVal = new DeviceAvailabiltyRule();

            DataTable dt = DAL.ApiDAL.Get_AvailableDevices(nMediaID, nGroupID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    retVal.ID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "Rule_ID");
                    retVal.Name = APILogic.Utils.GetSafeStr(dt.Rows[0], "Rule_Name");

                    retVal.AvailableDevices = dt.AsEnumerable()
                             .Select(dr => new DeviceObject()
                             {
                                 ID = APILogic.Utils.GetIntSafeVal(dr, "ID"),
                                 Name = APILogic.Utils.GetSafeStr(dr, "Name"),
                                 FamilyID = APILogic.Utils.GetIntSafeVal(dr, "Family_ID"),
                                 FamilyName = APILogic.Utils.GetSafeStr(dr, "Family_Name")
                             }
                                     ).ToList();
                }
            }
            return retVal;
        }
        /// <summary>
        /// Check Geo Commerce subscription
        /// </summary>
        /// <param name="nGroupID">set group ID</param>
        /// <param name="SubscriptionRuleID">set subscriiption geo commerce rule ID</param>
        /// <param name="sIP">set client IP</param>
        /// <returns>return true if the enable purchase subscription in spasfic country by IP</returns>
        public static bool IsGeoCommerceBlock(int nGroupID, int SubscriptionGeoCommerceID, string sIP)
        {
            bool GeoCommersRes = false;
            //convert IP to country ID
            Int32 nCountryID = GetIPCountryCode(sIP);
            //
            string sGroupID = PageUtils.GetAllGroupTreeStr(nGroupID);


            Logger.Logger.Log("Geo Commerce", "Geo Commerce ID " + SubscriptionGeoCommerceID + " Country ID " + nCountryID, "Geo Commerce");
            if (SubscriptionGeoCommerceID != 0)
            {
                //define the logic roul only spasfic country or no one except.
                // true : No one except the below selections
                // false : Every body except the below selection
                bool rule_ONLY_OR_BUT = true;

                //get the ONLY_OR_BUT value from geo_block_types table by Subscription Geo Commerce ID
                //GEO_RULE_TYPE = 3 only for geo commerce.

                //"select ONLY_OR_BUT from geo_block_types where isActiv=1 and GEO_RULE_TYPE=3 and ID = SubscriptionGeoCommerceID"

                #region Get geo commerce ONLY_OR_BUT value
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "Select ONLY_OR_BUT from geo_block_types where IS_ACTIVE=1 and STATUS=1 and GEO_RULE_TYPE=3 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", SubscriptionGeoCommerceID);
                selectQuery += " and group_id " + sGroupID;

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        object oBTID = selectQuery.Table("query").DefaultView[0].Row["ONLY_OR_BUT"];
                        if (oBTID != null && !string.IsNullOrEmpty(oBTID.ToString()))
                        {
                            rule_ONLY_OR_BUT = oBTID.ToString() == "1" ? true : false;
                        }
                    }
                }
                #endregion

                //object temp = PageUtils.GetTableSingleVal("geo_block_types", "ONLY_OR_BUT", SubscriptionGeoCommerceID);
                //if(temp != null && !string.IsNullOrEmpty(temp.ToString()))
                //{
                //    rule_ONLY_OR_BUT = temp.ToString() == "1" ? true : false;
                //}

                //declare member exist country in rule
                bool bExsitInRuleM2M = false;
                //is geo block type include country
                bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(SubscriptionGeoCommerceID, nCountryID);

                //retun true if: 
                //No one except except this country (true && true)
                //every body except this country (false && false)
                if ((rule_ONLY_OR_BUT && bExsitInRuleM2M) || (!rule_ONLY_OR_BUT && !bExsitInRuleM2M))
                {
                    GeoCommersRes = true;
                }
                else
                {
                    GeoCommersRes = false;
                }

            }
            return GeoCommersRes;
        }

        public static bool CleanUserHistory(int nGroupID, string siteGuid, List<int> lMediaIDs)
        {
            try
            {
                if (string.IsNullOrEmpty(siteGuid))
                {
                    Logger.Logger.Log("CleanUserHistory", " siteGuid = " + siteGuid, "API");
                    return false;
                }

                if (lMediaIDs == null || lMediaIDs.Count == 0)
                {
                    lMediaIDs = new List<int>();
                }

                //Logger.Logger.Log("CleanUserHistory", "Start CleanUserHistory with " + lMediaIDs.Count() + " mediaIds ", "API");

                bool result = DAL.ApiDAL.CleanUserHistory(nGroupID, siteGuid, lMediaIDs);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("CleanUserHistory", "Error = " + ex.Message, "API");
                return false;
            }
        }

        public static Scheduling GetProgramSchedule(int nProgramId, int nGroupID)
        {
            //call catalog service for details 
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
           
            APILogic.Catalog.IserviceClient service = new IserviceClient();
            string sCatalogUrl = GetWSURL("WS_Catalog");

            APILogic.Catalog.EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
            request.m_lProgramsIds = new int[1] { nProgramId };
            request.m_nGroupID = nGroupID;
            request.m_nPageIndex = 0;
            request.m_nPageSize = 0;
            request.m_sSignature = sSignature;
            request.m_sSignString = sSignatureString;

            EpgProgramResponse response = (EpgProgramResponse)service.GetProgramsByIDs(request);
            Scheduling scheduling = null;
            if (response != null && response.m_nTotalItems > 0)
            {
                ProgramObj programObj = response.m_lObj[0] as ProgramObj;
                DateTime startTime = ODBCWrapper.Utils.GetDateSafeVal(programObj.m_oProgram.START_DATE);
                DateTime endTime = ODBCWrapper.Utils.GetDateSafeVal(programObj.m_oProgram.END_DATE);
                scheduling = new Scheduling(startTime, endTime);
            }
          
            return scheduling;
        }

        public static string GetCoGuidByMediaFileId(int nMediaFileID)
        {
            string sCoGuid = null;

            DataTable dt = DAL.ApiDAL.GetCoGuidByMediaFileId(nMediaFileID);

            if (dt != null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0] != null)
                {
                    sCoGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["CO_GUID"]);
                }
            }

            return sCoGuid;
        }

        public static List<string> GetUserStartedWatchingMedias(string sSiteGuid, int nNumOfItems)
        {
            List<string> medias = new List<string>();
            if (!string.IsNullOrEmpty(sSiteGuid))
            {

                DataTable idsTable = DAL.ApiDAL.GetUserStartedWatchingMedias(sSiteGuid, nNumOfItems);
                if (idsTable != null)
                {
                    if (idsTable.Rows.Count > 0)
                    {
                        medias.AddRange(idsTable.AsEnumerable().Select(dr => ODBCWrapper.Utils.GetIntSafeVal(dr["media_id"]).ToString()));
                    }
                }
            }

            return medias;
        }

        public static bool DoesMediaBelongToSubscription(int nSubscriptionCode, int[] nFileTypeIDs, int nMediaID, string sDevice, int nGroupID)
        {
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            APILogic.Catalog.IserviceClient client = new APILogic.Catalog.IserviceClient();

            string catalogUrl = string.Empty;
            catalogUrl = APILogic.Utils.GetCatalogUrl();

            if (!string.IsNullOrEmpty(catalogUrl))
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(catalogUrl);

            try
            {
                APILogic.Catalog.SubscriptionContainingMediaRequest request = new APILogic.Catalog.SubscriptionContainingMediaRequest();
                request.m_nPageIndex = 0;
                request.m_nPageSize = 0;
                request.m_nGroupID = nGroupID;
                request.m_nSubscriptionID = nSubscriptionCode;
                request.m_nMediaID = nMediaID;
                request.m_sSignString = sSignString;
                request.m_sSignature = sSignature;
                request.m_oFilter = new APILogic.Catalog.Filter();
                request.m_oFilter.m_bOnlyActiveMedia = true;
                request.m_oFilter.m_bUseStartDate = true;
                request.m_oFilter.m_sDeviceId = sDevice;
                request.m_sMediaType = "0";

                APILogic.Catalog.ContainingMediaResponse response = (APILogic.Catalog.ContainingMediaResponse)client.GetResponse(request);

                bool isMediaInSubscription = false;

                if (response != null)
                {
                    isMediaInSubscription = response.m_bContainsMedia;
                }
                return isMediaInSubscription;
            }
            catch
            {
                Logger.Logger.Log("Get Subscription Media Ids", "Failed to get subscription " + nSubscriptionCode + " from group " + nGroupID, "Get Subscription Media Ids");
                return false;
            }
        }

        public static List<int> GetSubscriptionMediaIds(int nSubscriptionCode, int[] nFileTypeIDs, string sDevice, int nGroupID)
        {
            List<int> nMedias = new List<int>();

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            APILogic.Catalog.IserviceClient client = new APILogic.Catalog.IserviceClient();

            string catalogUrl = string.Empty;
            catalogUrl = APILogic.Utils.GetCatalogUrl();

            if (!string.IsNullOrEmpty(catalogUrl))
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(catalogUrl);

            try
            {
                APILogic.Catalog.SubscriptionMediaRequest subscriptionMediaRequest = new APILogic.Catalog.SubscriptionMediaRequest();
                subscriptionMediaRequest.m_nPageIndex = 0;
                subscriptionMediaRequest.m_nPageSize = 0;
                subscriptionMediaRequest.m_nGroupID = nGroupID;
                subscriptionMediaRequest.m_nSubscriptionID = nSubscriptionCode;
                subscriptionMediaRequest.m_sSignString = sSignString;
                subscriptionMediaRequest.m_sSignature = sSignature;
                subscriptionMediaRequest.m_oFilter = new APILogic.Catalog.Filter();
                subscriptionMediaRequest.m_oFilter.m_bOnlyActiveMedia = true;
                subscriptionMediaRequest.m_oFilter.m_bUseStartDate = true;
                subscriptionMediaRequest.m_oFilter.m_sDeviceId = sDevice;
                subscriptionMediaRequest.m_sMediaType = "0";

                APILogic.Catalog.MediaIdsResponse response = (APILogic.Catalog.MediaIdsResponse)client.GetResponse(subscriptionMediaRequest);
                ApiObjects.SearchObjects.SearchResult[] medias = response.m_nMediaIds.ToArray();


                nMedias = APILogic.Utils.ConvertMediaResultObjectIDsToIntArray(medias);
                if (nMedias != null && nMedias.Count > 0)
                {
                    nMedias = nMedias.Distinct().ToList();
                }
            }
            catch
            {
                Logger.Logger.Log("Get Subscription Media Ids", "Failed to get subscription " + nSubscriptionCode + " from group " + nGroupID, "Get Subscription Media Ids");
            }
            finally
            {
                client.Close();
            }

            return nMedias;
        }

        public static List<GroupRule> GetEPGProgramRules(int nProgramId, int nMediaId, int nSiteGuid, string sIP, int nGroupId, string deviceUdid)
        {
            List<GroupRule> tempRules = new List<GroupRule>();
            List<GroupRule> retRules = new List<GroupRule>();

            DataTable rulesDt = DAL.ApiDAL.Get_EPGProgramRules(nProgramId, nSiteGuid.ToString());
            GetRules(rulesDt, eGroupRuleType.EPG, nSiteGuid, nMediaId, nGroupId, sIP, ref retRules);


            return retRules;
        }

        private static void GetRules(DataTable rulesTable, eGroupRuleType eRuleType, int nSiteGuid, int nMediaId, int nGroupId, string sIP, ref List<GroupRule> rules)
        {
            List<GroupRule> tempRules = new List<GroupRule>();

            //Check if geo-block applies
            if (CheckGeoBlockMedia(nGroupId, nMediaId, sIP) != "OK")
            {
                rules.Add(new GroupRule() { Name = "GeoBlock", BlockType = eBlockType.Geo });
            }

            //Check if user type match media user types
            if (CheckMediaUserType(nMediaId, nSiteGuid) == false)
            {
                rules.Add(new GroupRule() { Name = "UserTypeBlock", BlockType = eBlockType.UserType });
            }


            if (rulesTable != null)
            {
                if (rulesTable.DefaultView.Count > 0)
                {
                    tempRules.AddRange(rulesTable.AsEnumerable()
                                        .Select(dr => new GroupRule(int.Parse(dr["rule_id"].ToString()), int.Parse(dr["TAG_TYPE_ID"].ToString()), dr["VALUE"].ToString(), dr["Key"].ToString(), dr["Name"].ToString(), dr["age_restriction"], int.Parse(dr["is_active"].ToString()), (eGroupRuleType)(dr["group_rule_type_id"]))
                                       ).ToList()
                                      );
                }
            }

            if (tempRules != null && tempRules.Count > 0)
            {
                foreach (GroupRule rule in tempRules)
                {
                    if (rule.GroupRuleType == eRuleType && rule.BlockType != eBlockType.Geo)
                    {
                        if (rule.AgeRestriction > 0 && !CheckAgeValidation(rule.AgeRestriction, nSiteGuid))
                        {
                            rule.BlockType = eBlockType.AgeBlock;
                            rules.Add(rule);
                        }
                        else
                        {
                            if (rule.IsActive)
                            {
                                rule.BlockType = eBlockType.Validation;
                                rules.Add(rule);
                            }
                        }
                    }
                }
            }
        }

        public static List<int> ChannelsContainingMedia(List<int> lChannels, int nMediaID, int nGroupID, int nFileTypeID)
        {
            APILogic.Catalog.Iservice service = new APILogic.Catalog.IserviceClient();

            APILogic.Catalog.Filter oFilter = new APILogic.Catalog.Filter();
            oFilter.m_bOnlyActiveMedia = false;
            oFilter.m_bUseFinalDate = true;
            oFilter.m_bUseStartDate = true;
            oFilter.m_nLanguage = DAL.UtilsDal.GetLangGroupID(nGroupID);
            oFilter.m_sDeviceId = "";
            oFilter.m_sPlatform = "";

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);


            //Call Catalog to create search object 
            APILogic.Catalog.ChannelsContainingMediaRequest subRrequest = new APILogic.Catalog.ChannelsContainingMediaRequest();
            subRrequest.m_oFilter = oFilter;
            subRrequest.m_sSignString = sSignString;
            subRrequest.m_sSignature = sSignature;
            subRrequest.m_nGroupID = nGroupID;
            subRrequest.m_nMediaID = nMediaID;
            subRrequest.m_lChannles = lChannels.ToArray<int>();

            try
            {
                APILogic.Catalog.ChannelsContainingMediaResponse response = (APILogic.Catalog.ChannelsContainingMediaResponse)service.GetResponse(subRrequest);
                return response.m_lChannellList.ToList<int>();
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("ChannelsContainingMedia", "Failed  due ex = " + ex.Message, "ChannelsContainingMedia");
            }

            return null;
        }

        public static List<int> GetMediaChannels(int nGroupID, int nMediaID)
        {
            List<int> lChannels = new List<int>();
            APILogic.Catalog.IserviceClient catalogClient = new APILogic.Catalog.IserviceClient();

            try
            {
                string sCatalogUrl = APILogic.Utils.GetWSUrl("CATALOG_WCF");
                if (!string.IsNullOrEmpty(sCatalogUrl))
                {
                    catalogClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(sCatalogUrl);
                }

                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = APILogic.Utils.GetWSUrl("CatalogSignatureKey");

                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

                APILogic.Catalog.MediaChannelsRequest oMediaChannelReq = new APILogic.Catalog.MediaChannelsRequest()
                {
                    m_nGroupID = nGroupID,
                    m_nMediaID = nMediaID,
                    m_sSignString = sSignString,
                    m_sSignature = sSignature
                };

                APILogic.Catalog.MediaChannelsResponse response = catalogClient.GetResponse(oMediaChannelReq) as APILogic.Catalog.MediaChannelsResponse;

                if (response != null && response.m_nChannelIDs != null && response.m_nChannelIDs.Length > 0)
                {
                    lChannels.AddRange(response.m_nChannelIDs);
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Exception thrown in GetMediaChannels. Ex={0}", ex.Message), "SubscriptionMediaFile");
            }
            finally
            {
                catalogClient.Close();
            }

            return lChannels;
        }


        public static EPGChannelProgrammeObject GetProgramDetails(int nProgramId, int nGroupID)
        {
            //call catalog service for details 
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            APILogic.Catalog.IserviceClient service = new IserviceClient();
            string sCatalogUrl = GetWSURL("WS_Catalog");

            APILogic.Catalog.EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
            request.m_lProgramsIds = new int[1] { nProgramId };
            request.m_nGroupID = nGroupID;
            request.m_nPageIndex = 0;
            request.m_nPageSize = 0;
            request.m_sSignature = sSignature;
            request.m_sSignString = sSignString;

            EpgProgramResponse response = (EpgProgramResponse)service.GetProgramsByIDs(request);
            EPGChannelProgrammeObject epg = null;
            if (response != null && response.m_nTotalItems > 0)
            {
                if (response.m_lObj[0] != null)
                {
                    ProgramObj programObj = response.m_lObj[0] as ProgramObj;
                    epg = programObj.m_oProgram;
                }
            }
            return epg;

        }
    }

}