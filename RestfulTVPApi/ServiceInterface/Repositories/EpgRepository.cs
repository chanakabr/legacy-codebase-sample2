using System;
using System.Collections.Generic;
using System.Configuration;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPApiModule.Context;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class EpgRepository : IEpgRepository
    {
        public List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex)
        {
            List<string> retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "EPGAutoComplete", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                DateTime _startTime, _endTime;

                _startTime = DateTime.UtcNow.AddDays(-int.Parse(ConfigurationManager.AppSettings["EPGSearchOffsetDays"]));
                _endTime = DateTime.UtcNow.AddDays(int.Parse(ConfigurationManager.AppSettings["EPGSearchOffsetDays"])); ;

                retVal = new APIEPGAutoCompleteLoader(groupId, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, searchText, _startTime, _endTime)
                {
                    Culture = initObj.Locale.LocaleLanguage
                }.Execute() as List<string>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public List<EPGChannel> GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApiModule.Context.OrderBy orderBy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannels", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiApiService _service = new ApiApiService(groupId, initObj.Platform);

                return _service.GetEPGChannel(sPicSize);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGCommentsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return CommentHelper.GetEPGCommentsList(groupId, initObj.Platform, initObj.Locale.LocaleLanguage, epgProgramID, pageSize, pageIndex);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGMultiChannelProgram", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiApiService _service = new ApiApiService(groupId, initObj.Platform);

                return _service.GetEPGMultiChannelProgram(sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> SearchEPGPrograms(InitializationObject initObj, string searchText, int pageSize, int pageIndex)
        {
            List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SearchEPGPrograms", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                DateTime _startTime, _endTime;

                _startTime = DateTime.UtcNow.AddDays(-int.Parse(ConfigurationManager.AppSettings["EPGSearchOffsetDays"]));
                _endTime = DateTime.UtcNow.AddDays(int.Parse(ConfigurationManager.AppSettings["EPGSearchOffsetDays"]));

                retVal = new APIEPGSearchLoader(groupId, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, searchText, _startTime, _endTime)
                {
                    Culture = initObj.Locale.LocaleLanguage
                }.Execute() as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public List<GroupRule> GetEPGProgramRules(InitializationObject initObj, string sSiteGUID, int MediaId, int programId)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetEPGProgramRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetEPGProgramRules(MediaId, programId, sSiteGUID, SiteHelper.GetClientIP(), initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string GetEPGLicensedLink(InitializationObject initObj, string sSiteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGLicensedLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.GetEPGLicensedLink(sSiteGUID, mediaFileID, EPGItemID, startTime, basicLink, SiteHelper.GetClientIP(), refferer, countryCd2, languageCode3, deviceName, formatType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

    }
}