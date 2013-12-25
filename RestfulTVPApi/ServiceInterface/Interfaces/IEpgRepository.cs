using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IEpgRepository
    {
        List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApi.OrderBy orderBy);

        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex);

        TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);
    }
}