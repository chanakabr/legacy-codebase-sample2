using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using TVPPro.SiteManager.Objects;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IEpgRepository
    {
        List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        List<EPGChannel> GetEPGChannels(InitializationObject initObj, string sPicSize, OrderBy orderBy);

        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex);

        List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        List<EPGChannelProgrammeObject> SearchEPGPrograms(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        List<GroupRule> GetEPGProgramRules(InitializationObject initObj, string sSiteGUID, int MediaId, int programId);

        string GetEPGLicensedLink(InitializationObject initObj, string sSiteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType);
    }
}