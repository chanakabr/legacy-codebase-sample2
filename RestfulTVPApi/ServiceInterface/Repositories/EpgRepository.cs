using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Manager;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Users;

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

        public TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApi.OrderBy orderBy)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannels", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                sRet = new ApiApiService(groupId, initObj.Platform).GetEPGChannel(sPicSize);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }

        public List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex)
        {
            List<TVPPro.SiteManager.Objects.EPGComment> retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGCommentsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                retVal = CommentHelper.GetEPGCommentsList(groupId, initObj.Platform, initObj.Locale.LocaleLanguage, epgProgramID, pageSize, pageIndex);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGMultiChannelProgram", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                sRet = new ApiApiService(groupId, initObj.Platform).GetEPGMultiChannelProgram(sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }
    }
}