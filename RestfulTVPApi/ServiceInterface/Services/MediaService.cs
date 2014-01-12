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
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApi;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/medias/{media_ids}", "GET", Notes = "This method returns all of the stored information regarding a media asset")]
    public class GetMediasInfoRequest : RequestBase, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/medias/{media_id}/comments", "GET", Notes = "This method returns an array of user's comments about the media specified")]
    public class GetMediaCommentsRequest : PagingRequest, IReturn<IEnumerable<Comment>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/comments", "POST", Notes = "This method adds a user comment to a media asset")]
    public class AddCommentRequest : RequestBase, IReturn<bool>
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

    [Route("/medias/{media_id}/media_mark", "GET", Notes = "This method returns the last play-position for this specific user and media asset")]
    public class GetMediaMarkRequest : RequestBase, IReturn<MediaMark>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/media_mark", "POST", Notes = "This method sends a status event (e.g., Play, Finish, …) for analytical purposes")]
    public class MediaMarkRequest : RequestBase, IReturn<string>
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

    [Route("/medias/{media_id}/media_hit", "POST", Notes = "This method marks the current position of the media asset being played and sends an event that the media asset is (still) being played. This is done every 30 seconds and is called the 'hit'")]
    public class MediaHitRequest : RequestBase, IReturn<string>
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

    [Route("/medias/{media_id}/related_medias", "GET", Notes = "This method returns an array of media assets related to a given media asset; it is limited to a specific media type. The related relationships are backend configurable; they are determined by tags")]
    public class GetRelatedMediasByTypesRequest : PagingRequest, IReturn<MediaMark>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_types { get; set; }
    }

    [Route("/medias/{media_id}/people_who_watched", "GET", Notes = "This method returns media assets that were watched by other users who have also watched this item")]
    public class GetPeopleWhoWatchedRequest : PagingRequest, IReturn<MediaMark>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/medias/files/{media_file_id}/license_link", "GET", Notes = "This method returns a playable link to the requested media")]
    public class GetMediaLicenseLinkRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "base_link", Description = "Base Link", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string base_link { get; set; }
    }

    [Route("/medias/files/{media_file_id}/is_purchased", "GET", Notes = "Checks if the given item was or was not purchased by the user")]
    public class IsItemPurchasedRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/medias/{media_ids}/are_favorites", "GET", Notes = "Send list of mediaIDs, returns is an array of key value pairs (the media asset ID, 'True' = Is user favorite; 'False' = Is not user favorite)")]
    public class AreMediasFavoriteRequest : RequestBase, IReturn<IEnumerable<KeyValuePair<long, bool>>>
    {
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/medias/files/{media_file_id}/charge", "POST", Notes = "This method buys pay-per-view (PPV) with prepaid. This processing is done by Tvinci not by a third party processor")]
    public class ChargeMediaWithPrepaidRequest : RequestBase, IReturn<PrePaidResponseStatus>
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

    [Route("/medias/files/{media_file_id}/dummy_charge", "POST", Notes = "This method performs a dummy purchase of an asset for a user account. Used to give the user an entitlement to an asset without charge")]
    public class DummyChargeUserForMediaFileRequest : RequestBase, IReturn<string>
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

    [Route("/medias/recommended", "GET", Notes = "This method returns an array of recommended media filtered by media type")]
    public class GetRecommendedMediasByTypesRequest : PagingRequest, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_types { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "with_dynamic", Description = "With Dynamic Data?", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool with_dynamic { get; set; }
    }

    [Route("/medias/{media_id}/actions/{action_type}", "POST", Notes = "Performs any of these following actions on the media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch). See also: AddUserSocialAction")]
    public class ActionDoneRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiAllowableValues("action", typeof(TVPApi.ActionType))]
        [ApiMember(Name = "action", Description = "Action", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public TVPApi.ActionType action_type { get; set; }
        [ApiMember(Name = "extra_val", Description = "Extra Variable", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int extra_val { get; set; }
    }

    [Route("/medias", "GET", Notes = "Search media asset by specific tags and metas")]
    public class SearchMediaByAndOrListRequest : PagingRequest, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "or_list", Description = "OR list", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> or_list { get; set; }
        [ApiMember(Name = "and_list", Description = "AND list", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> and_list { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "exact", Description = "Exact?", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool exact { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy))]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy order_by { get; set; }
        [ApiMember(Name = "order_dir", Description = "Order Direction", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_dir", typeof(Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir))]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir order_dir { get; set; }
        [ApiMember(Name = "order_meta_name", Description = "Order Meta Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string order_meta_name { get; set; }
    }

    [Route("/medias/{media_id}/send_to_friend", "POST", Notes = "This method shares a media asset with a friend via mail")]
    public class SendToFriendRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "sender_name", Description = "Sender Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string sender_name { get; set; }
        [ApiMember(Name = "sender_email", Description = "Sender Email", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string sender_email { get; set; }
        [ApiMember(Name = "to_email", Description = "To Email", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string to_email { get; set; }
    }

    [Route("/medias/auto_complete/{prefix_text}", "GET", Notes = "This method auto-completes the entered text and returns the resulting media title strings as an array")]
    public class GetAutoCompleteSearchListRequest : RequestBase, IReturn<IEnumerable<string>>
    {
        [ApiMember(Name = "prefix_text", Description = "Prefix Text", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string prefix_text { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_types { get; set; }
    }

    [Route("/medias/{media_id}/subscriptions", "GET", Notes = "This method returns all subscriptions ID's containing a posted media and file ID")]
    public class GetSubscriptionIDsContainingMediaFileRequest : RequestBase, IReturn<IEnumerable<int>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class MediasService : Service
    {
        public IMediasRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetMediasInfoRequest request)
        {
            var response = _repository.GetMediasInfo(request.InitObj, request.media_ids, request.pic_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaCommentsRequest request)
        {
            var response = _repository.GetMediaComments(request.InitObj, request.media_id, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(AddCommentRequest request)
        {
            var response = _repository.AddComment(request.InitObj, request.media_id, request.media_type, request.writer, request.header, request.sub_header, request.content, request.auto_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaMarkRequest request)
        {
            var response = _repository.GetMediaMark(request.InitObj, request.media_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(MediaMarkRequest request)
        {
            var response = _repository.MediaMark(request.InitObj, request.action, request.media_type, request.media_id, request.media_file_id, request.location);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(MediaHitRequest request)
        {
            var response = _repository.MediaHit(request.InitObj, request.media_type, request.media_id, request.media_file_id, request.location);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetRelatedMediasByTypesRequest request)
        {
            var response = _repository.GetRelatedMediasByTypes(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number, request.media_types);

            if (response == null)   
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPeopleWhoWatchedRequest request)
        {
            var response = _repository.GetPeopleWhoWatched(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaLicenseLinkRequest request)
        {
            var response = _repository.GetMediaLicenseLink(request.InitObj, request.media_file_id, request.base_link);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemPurchasedRequest request)
        {
            var response = _repository.IsItemPurchased(request.InitObj, request.media_file_id, request.site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(AreMediasFavoriteRequest request)
        {
            var response = _repository.AreMediasFavorite(request.InitObj, request.media_ids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(ChargeMediaWithPrepaidRequest request)
        {
            var response = _repository.ChargeMediaWithPrepaid(request.InitObj, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(DummyChargeUserForMediaFileRequest request)
        {
            var response = _repository.DummyChargeUserForMediaFile(request.InitObj, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.user_ip, request.coupon_code);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetRecommendedMediasByTypesRequest request)
        {
            var response = _repository.GetRecommendedMediasByTypes(request.InitObj, request.pic_size, request.page_size, request.page_number, request.media_types);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(ActionDoneRequest request)
        {
            var response = _repository.ActionDone(request.InitObj, request.action_type, request.media_id, request.media_type, request.extra_val);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(SearchMediaByAndOrListRequest request)
        {
            var response = _repository.SearchMediaByAndOrList(request.InitObj, request.or_list, request.and_list, request.media_type, request.page_size, request.page_number, request.pic_size, request.exact, request.order_by, request.order_dir, request.order_meta_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(SendToFriendRequest request)
        {
            var response = _repository.SendToFriend(request.InitObj, request.media_id, request.sender_name, request.sender_email, request.to_email);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetAutoCompleteSearchListRequest request)
        {
            var response = _repository.GetAutoCompleteSearchList(request.InitObj, request.prefix_text, request.media_types);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionIDsContainingMediaFileRequest request)
        {
            var response = _repository.GetSubscriptionIDsContainingMediaFile(request.InitObj, request.media_id, request.media_file_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
