using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class MediasService : Service
    {
        public IMediasRepository _repository { get; set; }  //Injected by IOC

        #region GET

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

        public HttpResult Get(GetMediaMarkRequest request)
        {
            var response = _repository.GetMediaMark(request.InitObj, request.media_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
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

        public HttpResult Get(SearchMediaByAndOrListRequest request)
        {
            var response = _repository.SearchMediaByAndOrList(request.InitObj, request.or_list, request.and_list, request.media_type, request.page_size, request.page_number, request.pic_size, request.exact, request.order_by, request.order_dir, request.order_meta_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
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

        public HttpResult Get(GetItemsPricesWithCouponsRequest request)
        {
            var response = _repository.GetItemsPricesWithCoupons(request.InitObj, request.site_guid, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemPurchasedRequest request)
        {
            var response = _repository.IsItemPurchased(request.InitObj, request.site_guid, request.media_file_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(IsUserSocialActionPerformedRequest request)
        {
            var response = _repository.IsUserSocialActionPerformed(request.InitObj, request.site_guid, request.media_id, request.social_platform, request.social_action);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediaLicenseLinkRequest request)
        {
            var response = _repository.GetMediaLicenseLink(request.InitObj, request.site_guid, request.media_file_id, request.base_link);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUsersLikedMediaRequest request)
        {
            var response = _repository.GetUsersLikedMedia(request.InitObj, request.site_guid, request.media_id, request.only_friends, request.page_number, request.page_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region PUT
        #endregion

        #region POST

        public HttpResult Post(AddCommentRequest request)
        {
            var response = _repository.AddComment(request.InitObj, request.media_id, request.media_type, request.writer, request.header, request.sub_header, request.content, request.auto_active);

            return new HttpResult(response, HttpStatusCode.OK);
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

        public HttpResult Post(SendToFriendRequest request)
        {
            var response = _repository.SendToFriend(request.InitObj, request.media_id, request.sender_name, request.sender_email, request.to_email);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(ChargeMediaWithPrepaidRequest request)
        {
            var response = _repository.ChargeMediaWithPrepaid(request.InitObj, request.site_guid, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(ActionDoneRequest request)
        {
            var response = _repository.ActionDone(request.InitObj, request.site_guid, request.action_type, request.media_id, request.media_type, request.extra_val);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(InApp_ChargeUserForMediaFileRequest request)
        {
            var response = _repository.InApp_ChargeUserForMediaFile(request.InitObj, request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.receipt);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region DELETE
        #endregion

    }
}
