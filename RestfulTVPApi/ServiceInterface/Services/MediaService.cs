using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPPro.SiteManager.TvinciPlatform.api;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects


    [Route("/medias/{media_ids}", "GET", Summary = "Get Medias Info", Notes = "Get Medias Info")]
    public class GetMediasInfo : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/medias/{media_id}/comments", "GET", Summary = "Get Media Comments", Notes = "Get Media Comments")]
    public class GetMediaComments : PagingRequest, IReturn<IEnumerable<CommentDTO>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/comments", "POST", Summary = "Add Media Comment", Notes = "Add Media Comment")]
    public class AddComment : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "writer", Description = "Writer", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string writer { get; set; }
        [ApiMember(Name = "header", Description = "Header", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string header { get; set; }
        [ApiMember(Name = "sub_header", Description = "Sub Header", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string sub_header { get; set; }
        [ApiMember(Name = "content", Description = "Content", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string content { get; set; }
        [ApiMember(Name = "auto_active", Description = "Auto Active?", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool auto_active { get; set; }
    }

    [Route("/medias/{media_id}/mediamark", "GET", Summary = "Get Media Mark", Notes = "Get Media Mark")]
    public class GetMediaMark : RequestBase, IReturn<MediaMarkDTO>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/mediamark", "POST", Summary = "Add Media Mark", Notes = "Add Media Mark")]
    public class MediaMark : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "location", Description = "Playback Position", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int location { get; set; }
        [ApiAllowableValues("action", typeof(action))]
        [ApiMember(Name = "action", Description = "Action", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public action action { get; set; }
    }

    [Route("/medias/{media_id}/mediahit", "POST", Summary = "Add Media Hit", Notes = "Add Media Hiet")]
    public class MediaHit : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "location", Description = "Playback Position", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int location { get; set; }
    }

    //[Route("/medias/{media_id}/related/{media_types}", "GET", Summary = "Get Related Medias", Notes = "Get Related Medias")]
    //[Route("/medias/{media_id}/related/type/{media_types}", "GET", Summary = "Get Related Medias", Notes = "Get Related Medias")]
    [Route("/medias/{media_id}/related", "GET", Summary = "Get Related Medias", Notes = "Get Related Medias")]
    public class GetRelatedMediasByTypes : PagingRequest, IReturn<MediaMarkDTO>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_types { get; set; }
    }

    [Route("/medias/{media_id}/peoplewhowatched", "GET", Summary = "Get People Who Watched", Notes = "Get People Who Watched")]
    public class GetPeopleWhoWatched : PagingRequest, IReturn<MediaMarkDTO>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/medias/files/{media_file_id}/licenselink", "GET", Summary = "Get Media License Link", Notes = "Get Media License Link")]
    public class GetMediaLicenseLink : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "base_link", Description = "Base Link", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string base_link { get; set; }
    }

    [Route("/medias/files/{media_file_id}/ispurchased", "GET", Summary = "Is Media Purchased", Notes = "Is Media Purchased")]
    public class IsItemPurchased : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "user_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string user_guid { get; set; }
    }

    [Route("/medias/favorites", "GET", Summary = "Are Medias Favorite", Notes = "Are Medias Favorite")]
    public class AreMediasFavorite : RequestBase, IReturn<IEnumerable<KeyValuePair<long, bool>>>
    {
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
        [ApiMember(Name = "user_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string user_guid { get; set; }
    }

    [Route("/medias/files/{media_file_id}/charge", "POST", Summary = "Charge Media With Prepaid", Notes = "Charge Media With Prepaid")]
    public class ChargeMediaWithPrepaid : RequestBase, IReturn<PrePaidResponseStatusDTO>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    [Route("/medias/files/{media_file_id}/dummycharge", "POST", Summary = "Dummy Charge User For Media File", Notes = "Dummy Charge User For Media File")]
    public class DummyChargeUserForMediaFile : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "user_ip", Description = "User IP Address", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_ip { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    //[Route("/medias/recommended/type/{media_types}", "GET", Summary = "Get Recommended Medias By Types", Notes = "Get Recommended Medias By Types")]
    [Route("/medias/recommended/{media_types}", "GET", Summary = "Get Recommended Medias By Types", Notes = "Get Recommended Medias By Types")]
    public class GetRecommendedMediasByTypes : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_types { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "with_dynamic", Description = "With Dynamic Data?", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool with_dynamic { get; set; }
    }

    //[Route("/medias/{media_id}/action/{action_type}", "POST", Notes = "Performs any of these following actions on the media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch). See also: AddUserSocialAction")]
    [Route("/medias/{media_id}/action", "POST", Notes = "Performs any of these following actions on the media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch). See also: AddUserSocialAction")]
    public class ActionDone : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiAllowableValues("action", typeof(TVPApi.ActionType))]
        [ApiMember(Name = "action", Description = "Action", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public TVPApi.ActionType action_type { get; set; }
        [ApiMember(Name = "extra_val", Description = "Extra Variable", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int extra_val { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class MediasService : Service
    {
        public IMediasRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetMediasInfo request)
        {
            var response = _repository.GetMediasInfo(request.InitObj, request.media_ids, request.pic_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaComments request)
        {
            var response = _repository.GetMediaComments(request.InitObj, request.media_id, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(AddComment request)
        {
            var response = _repository.AddComment(request.InitObj, request.media_id, request.media_type, request.writer, request.header, request.sub_header, request.content, request.auto_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaMark request)
        {
            var response = _repository.GetMediaMark(request.InitObj, request.media_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(MediaMark request)
        {
            var response = _repository.MediaMark(request.InitObj, request.action, request.media_type, request.media_id, request.media_file_id, request.location);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(MediaHit request)
        {
            var response = _repository.MediaHit(request.InitObj, request.media_type, request.media_id, request.media_file_id, request.location);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetRelatedMediasByTypes request)
        {
            var response = _repository.GetRelatedMediasByTypes(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number, request.media_types);

            if (response == null)   
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPeopleWhoWatched request)
        {
            var response = _repository.GetPeopleWhoWatched(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaLicenseLink request)
        {
            var response = _repository.GetMediaLicenseLink(request.InitObj, request.media_file_id, request.base_link);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemPurchased request)
        {
            var response = _repository.IsItemPurchased(request.InitObj, request.media_file_id, request.user_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(AreMediasFavorite request)
        {
            var response = _repository.AreMediasFavorite(request.InitObj, request.media_ids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(ChargeMediaWithPrepaid request)
        {
            var response = _repository.ChargeMediaWithPrepaid(request.InitObj, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(DummyChargeUserForMediaFile request)
        {
            var response = _repository.DummyChargeUserForMediaFile(request.InitObj, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.user_ip, request.coupon_code);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetRecommendedMediasByTypes request)
        {
            var response = _repository.GetRecommendedMediasByTypes(request.InitObj, request.pic_size, request.page_size, request.page_number, request.media_types);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(ActionDone request)
        {
            var response = _repository.ActionDone(request.InitObj, request.action_type, request.media_id, request.media_type, request.extra_val);

            return new HttpResult(response, HttpStatusCode.OK);
        }
    }
}
