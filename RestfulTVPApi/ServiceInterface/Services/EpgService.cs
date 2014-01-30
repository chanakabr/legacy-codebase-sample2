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
            var response = _repository.GetEPGAutoComplete(request.InitObj, request.search_text, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetEPGChannelsRequest request)
        {
            var response = _repository.GetEPGChannels(request.InitObj, request.pic_size, request.order_by);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetEPGCommentsListRequest request)
        {
            var response = _repository.GetEPGCommentsList(request.InitObj, request.program_id, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetEPGMultiChannelProgramRequest request)
        {
            var response = _repository.GetEPGMultiChannelProgram(request.InitObj, request.channel_ids, request.pic_size, request.unit, request.from_offset, request.to_offset, request.utc_offset);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(SearchEPGProgramsRequest request)
        {
            var response = _repository.SearchEPGPrograms(request.InitObj, request.search_text, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetEPGLicensedLinkRequest request)
        {
            var response = _repository.GetEPGLicensedLink(request.InitObj, request.site_guid, request.media_file_id, request.epg_item_id, request.start_time, request.base_link, request.refferer, request.country_code, request.language_code, request.device_name, request.format_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetEPGProgramRulesRequest request)
        {
            var response = _repository.GetEPGProgramRules(request.InitObj, request.site_guid, request.media_id, request.program_id);

            return new HttpResult(response, HttpStatusCode.OK);
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
