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

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/epg/autocomplete/{search_text}", "GET", Summary = "Logout", Notes = "Logout")]
    public class GetEPGAutoComplete : PagingRequest, IReturn<IEnumerable<string>>
    {
        [ApiMember(Name = "search_text", Description = "Search Text", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string search_text { get; set; }
    }

    [Route("/epg/channels", "GET", Summary = "Get Channel Multi Filter", Notes = "Get Channel Multi Filter")]
    public class GetEPGChannels : PagingRequest, IReturn<IEnumerable<EPGChannelObjectDTO>>
    {
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = "OrderBy", IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(OrderBy))]
        public OrderBy order_by { get; set; }
    }

    [Route("/epg/channels/programs/{program_id}/comments", "GET", Summary = "Get EPG Comments List", Notes = "Get EPG Comments List")]
    public class GetEPGCommentsList : PagingRequest, IReturn<IEnumerable<EPGCommentDTO>>
    {
        [ApiMember(Name = "program_id", Description = "Program ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int program_id { get; set; }
    }

    [Route("/epg/channels/{channel_ids}", "GET", Summary = "Get EPG Channels programs", Notes = "Get EPG Channels programs")]
    public class GetEPGMultiChannelProgram : PagingRequest, IReturn<IEnumerable<EPGMultiChannelProgrammeObjectDTO>>
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

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class EpgService : Service
    {
        public IEpgRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetEPGAutoComplete request)
        {
            var response = _repository.GetEPGAutoComplete(request.InitObj, request.search_text, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGChannels request)
        {
            var response = _repository.GetEPGChannels(request.InitObj, request.pic_size, request.order_by);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGCommentsList request)
        {
            var response = _repository.GetEPGCommentsList(request.InitObj, request.program_id, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetEPGMultiChannelProgram request)
        {
            var response = _repository.GetEPGMultiChannelProgram(request.InitObj, request.channel_ids, request.pic_size, request.unit, request.from_offset, request.to_offset, request.utc_offset);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }
    }
}
