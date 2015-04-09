using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IEpgRepository
    {
        List<string> GetEPGAutoComplete(GetEPGAutoCompleteRequest request);

        List<EPGChannel> GetEPGChannels(GetEPGChannelsRequest request);

        List<EPGComment> GetEPGCommentsList(GetEPGCommentsListRequest request);

        List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(GetEPGMultiChannelProgramRequest request);

        List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> SearchEPGPrograms(SearchEPGProgramsRequest request);

        List<GroupRule> GetEPGProgramRules(GetEPGProgramRulesRequest request);

        string GetEPGLicensedLink(GetEPGLicensedLinkRequest request);

        List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> SearchEPGByAndOrList(SearchEPGByAndOrListRequest request);

        List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannelsPrograms(GetEPGChannelsProgramsRequest request);
    }
}