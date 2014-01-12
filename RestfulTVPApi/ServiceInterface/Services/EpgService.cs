using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Objects;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/epg/auto_complete", "GET", Notes = "This method returns a string array of EPG program names that starts with the given search text")]
    public class GetEPGAutoCompleteRequest : PagingRequest, IReturn<IEnumerable<string>>
    {
        [ApiMember(Name = "search_text", Description = "Search Text", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string search_text { get; set; }
    }

    [Route("/epg/channels", "GET", Notes = "This method returns an array of EPG Channels for a specific account")]
    public class GetEPGChannelsRequest : PagingRequest, IReturn<IEnumerable<EPGChannelObject>>
    {
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = "OrderBy", IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(TVPApi.OrderBy))]
        public TVPApi.OrderBy order_by { get; set; }
    }

    [Route("/epg/channels/programs/{program_id}/comments", "GET", Notes = "This method returns a list of EPG comments created by users")]
    public class GetEPGCommentsListRequest : PagingRequest, IReturn<IEnumerable<EPGComment>>
    {
        [ApiMember(Name = "program_id", Description = "Program ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int program_id { get; set; }
    }

    [Route("/epg/channels/{channel_ids}", "GET", Notes = "This method returns an array of EPG channel programs, for each EPG channel entered, and which is available for the time range entered. This method is usually followed by GetEPGChannels")]
    public class GetEPGMultiChannelProgramRequest : PagingRequest, IReturn<IEnumerable<EPGMultiChannelProgrammeObjectDTO>>
    {
        [ApiMember(Name = "channel_ids", Description = "Channels IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] channel_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "unit", Description = "Program ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("unit", typeof(TVPPro.SiteManager.TvinciPlatform.api.EPGUnit))]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGUnit unit { get; set; }
        [ApiMember(Name = "from_offset", Description = "From Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int from_offset { get; set; }
        [ApiMember(Name = "to_offset", Description = "To Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int to_offset { get; set; }
        [ApiMember(Name = "utc_offset", Description = "UTC Offset", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int utc_offset { get; set; }
    }

    [Route("/epg/channels/programs", "GET", Notes = "This method searches the EPG programs by search text")]
    public class SearchEPGProgramsRequest : PagingRequest, IReturn<IEnumerable<EPGChannelProgrammeObjectDTO>>
    {
        [ApiMember(Name = "search_text", Description = "Search Text", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string search_text { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class EpgService : Service
    {
        public IEpgRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetEPGAutoCompleteRequest request)
        {
            var response = _repository.GetEPGAutoComplete(request.InitObj, request.search_text, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGChannelsRequest request)
        {
            var response = _repository.GetEPGChannels(request.InitObj, request.pic_size, request.order_by);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGCommentsListRequest request)
        {
            var response = _repository.GetEPGCommentsList(request.InitObj, request.program_id, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGMultiChannelProgramRequest request)
        {
            var response = _repository.GetEPGMultiChannelProgram(request.InitObj, request.channel_ids, request.pic_size, request.unit, request.from_offset, request.to_offset, request.utc_offset);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(SearchEPGProgramsRequest request)
        {
            var response = _repository.SearchEPGPrograms(request.InitObj, request.search_text, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
