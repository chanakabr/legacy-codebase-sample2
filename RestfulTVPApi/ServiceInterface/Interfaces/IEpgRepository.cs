using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IEpgRepository
    {
        List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        EPGChannel[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApi.OrderBy orderBy);

        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex);

        EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        List<EPGMultiChannelProgrammeObject> SearchEPGPrograms(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        GroupRule[] GetEPGProgramRules(InitializationObject initObj, string sSiteGUID, int MediaId, int programId);

        string GetEPGLicensedLink(InitializationObject initObj, string sSiteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType);
    }
}