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

    [Route("/site/countries", "GET", Notes = "This method returns a list of all countries, an ID and a symbol for each. Used to enable a user to select his/her country. Example: During user registration, the method returns an array of country codes, #ID, country name.")]
    public class GetCountriesList : PagingRequest, IReturn<IEnumerable<CountryDTO>> { }

    [Route("/site/autocomplete/{prefix_text}", "GET", Summary = "Get AutoComplete Search List", Notes = "Get AutoComplete Search List")]
    public class GetAutoCompleteSearchList : PagingRequest, IReturn<IEnumerable<string>>
    {
        [ApiMember(Name = "prefix_text", Description = "Prefix Text", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string prefix_text { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = false)]
        public int?[] media_types { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class SiteService : Service
    {
        public ISiteRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetCountriesList request)
        {
            var response = _repository.GetCountriesList(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetAutoCompleteSearchList request)
        {
            var response = _repository.GetAutoCompleteSearchList(request.InitObj, request.prefix_text, request.media_types);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
