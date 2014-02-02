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
            return _repository.GetEPGAutoComplete(request.InitObj, request.search_text, request.page_size, request.page_number);
        }

        public object Get(GetEPGChannelsRequest request)
        {
            return _repository.GetEPGChannels(request.InitObj, request.pic_size, request.order_by);
        }

        public object Get(GetEPGCommentsListRequest request)
        {
            return _repository.GetEPGCommentsList(request.InitObj, request.program_id, request.page_size, request.page_number);
        }

        public object Get(GetEPGMultiChannelProgramRequest request)
        {
            return _repository.GetEPGMultiChannelProgram(request.InitObj, request.channel_ids, request.pic_size, request.unit, request.from_offset, request.to_offset, request.utc_offset);
        }

        public object Get(SearchEPGProgramsRequest request)
        {
            return _repository.SearchEPGPrograms(request.InitObj, request.search_text, request.page_size, request.page_number);
        }

        public object Get(GetEPGLicensedLinkRequest request)
        {
            return _repository.GetEPGLicensedLink(request.InitObj, request.site_guid, request.media_file_id, request.epg_item_id, request.start_time, request.base_link, request.refferer, request.country_code, request.language_code, request.device_name, request.format_type);
        }

        public object Get(GetEPGProgramRulesRequest request)
        {
            return _repository.GetEPGProgramRules(request.InitObj, request.site_guid, request.media_id, request.program_id);
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
