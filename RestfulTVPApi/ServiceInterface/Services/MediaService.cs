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

        public object Get(GetMediasInfoRequest request)
        {
            return _repository.GetMediasInfo(request.InitObj, request.media_ids, request.pic_size);
        }

        public object Get(GetMediaCommentsRequest request)
        {
            return _repository.GetMediaComments(request.InitObj, request.media_id, request.page_size, request.page_number);
        }

        public object Get(GetMediaMarkRequest request)
        {
            return _repository.GetMediaMark(request.InitObj, request.media_id);
        }

        public object Get(GetRelatedMediasByTypesRequest request)
        {
            return _repository.GetRelatedMediasByTypes(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number, request.media_types);
        }

        public object Get(GetPeopleWhoWatchedRequest request)
        {
            return _repository.GetPeopleWhoWatched(request.InitObj, request.media_id, request.pic_size, request.page_size, request.page_number);
        }

        public object Get(SearchMediaByAndOrListRequest request)
        {
            return _repository.SearchMediaByAndOrList(request.InitObj, request.or_list, request.and_list, request.media_type, request.page_size, request.page_number, request.pic_size, request.exact, request.order_by, request.order_dir, request.order_meta_name);
        }

        public object Get(GetAutoCompleteSearchListRequest request)
        {
            return _repository.GetAutoCompleteSearchList(request.InitObj, request.prefix_text, request.media_types);
        }

        public object Get(GetSubscriptionIDsContainingMediaFileRequest request)
        {
            return _repository.GetSubscriptionIDsContainingMediaFile(request.InitObj, request.media_id, request.media_file_id);
        }

        public object Get(GetItemsPricesWithCouponsRequest request)
        {
            return _repository.GetItemsPricesWithCoupons(request.InitObj, request.site_guid, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);
        }

        public object Get(IsItemPurchasedRequest request)
        {
            return _repository.IsItemPurchased(request.InitObj, request.site_guid, request.media_file_id);
        }

        public object Get(IsUserSocialActionPerformedRequest request)
        {
            return _repository.IsUserSocialActionPerformed(request.InitObj, request.site_guid, request.media_id, request.social_platform, request.social_action);
        }

        public object Get(GetMediaLicenseLinkRequest request)
        {
            return _repository.GetMediaLicenseLink(request.InitObj, request.site_guid, request.media_file_id, request.base_link);
        }

        public object Get(GetUsersLikedMediaRequest request)
        {
            return _repository.GetUsersLikedMedia(request.InitObj, request.site_guid, request.media_id, request.only_friends, request.page_number, request.page_size);
        }

        #endregion

        #region PUT
        #endregion

        #region POST

        public object Post(AddCommentRequest request)
        {
            return _repository.AddComment(request.InitObj, request.media_id, request.media_type, request.writer, request.header, request.sub_header, request.content, request.auto_active);
        }

        public object Post(MediaMarkRequest request)
        {
            return _repository.MediaMark(request.InitObj, request.action, request.media_type, request.media_id, request.media_file_id, request.location);
        }

        public object Post(MediaHitRequest request)
        {
            return _repository.MediaHit(request.InitObj, request.media_type, request.media_id, request.media_file_id, request.location);
        }

        public object Post(SendToFriendRequest request)
        {
            return _repository.SendToFriend(request.InitObj, request.media_id, request.sender_name, request.sender_email, request.to_email);
        }

        public object Post(ChargeMediaWithPrepaidRequest request)
        {
            return _repository.ChargeMediaWithPrepaid(request.InitObj, request.InitObj.SiteGuid, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code);
        }

        public object Post(ActionDoneRequest request)
        {
            return _repository.ActionDone(request.InitObj, request.InitObj.SiteGuid, request.action_type, request.media_id, request.media_type, request.extra_val);
        }

        #endregion

        #region DELETE
        #endregion

    }
}
