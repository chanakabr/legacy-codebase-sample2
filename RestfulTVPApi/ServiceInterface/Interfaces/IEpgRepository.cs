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

        List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(GetEPGMultiChannelProgramRequest request);

        List<EPGChannelProgrammeObject> SearchEPGPrograms(SearchEPGProgramsRequest request);

        List<GroupRule> GetEPGProgramRules(GetEPGProgramRulesRequest request);

        string GetEPGLicensedLink(GetEPGLicensedLinkRequest request);

        List<EPGChannelProgrammeObject> SearchEPGByAndOrList(SearchEPGByAndOrListRequest request);

        List<EPGChannelProgrammeObject> GetEPGChannelsPrograms(GetEPGChannelsProgramsRequest request);
    }
}