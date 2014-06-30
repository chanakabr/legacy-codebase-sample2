using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using TVPPro.SiteManager.Objects;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IEpgRepository
    {
        List<string> GetEPGAutoComplete(GetEPGAutoCompleteRequest request);

        List<EPGChannel> GetEPGChannels(GetEPGChannelsRequest request);

        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(GetEPGCommentsListRequest request);

        List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(GetEPGMultiChannelProgramRequest request);

        List<EPGChannelProgrammeObject> SearchEPGPrograms(SearchEPGProgramsRequest request);

        List<GroupRule> GetEPGProgramRules(GetEPGProgramRulesRequest request);

        string GetEPGLicensedLink(GetEPGLicensedLinkRequest request);

        List<EPGChannelProgrammeObject> SearchEPGByAndOrList(SearchEPGByAndOrListRequest request);

        List<EPGChannelProgrammeObject> GetEPGChannelsPrograms(GetEPGChannelsProgramsRequest request);
    }
}