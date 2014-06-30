using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class EpgService : Service
    {
        public IEpgRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetEPGAutoCompleteRequest request)
        {
            return _repository.GetEPGAutoComplete(request);
        }

        public object Get(GetEPGChannelsRequest request)
        {
            return _repository.GetEPGChannels(request);
        }

        public object Get(GetEPGCommentsListRequest request)
        {
            return _repository.GetEPGCommentsList(request);
        }

        public object Get(GetEPGMultiChannelProgramRequest request)
        {
            return _repository.GetEPGMultiChannelProgram(request);
        }

        public object Get(SearchEPGProgramsRequest request)
        {
            return _repository.SearchEPGPrograms(request);
        }

        public object Get(GetEPGLicensedLinkRequest request)
        {
            return _repository.GetEPGLicensedLink(request);
        }

        public object Get(GetEPGProgramRulesRequest request)
        {
            return _repository.GetEPGProgramRules(request);
        }

        public object Get(SearchEPGByAndOrListRequest request)
        {
            return _repository.SearchEPGByAndOrList(request);
        }

        #endregion

        #region PUT
        #endregion

        #region POST
        #endregion

        #region DELETE
        #endregion

    }
}
