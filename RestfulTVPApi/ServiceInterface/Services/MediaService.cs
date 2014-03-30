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
            return _repository.GetMediasInfo(request);
        }

        public object Get(GetMediaCommentsRequest request)
        {
            return _repository.GetMediaComments(request);
        }

        public object Get(GetMediaMarkRequest request)
        {
            return _repository.GetMediaMark(request);
        }

        public object Get(GetRelatedMediasByTypesRequest request)
        {
            return _repository.GetRelatedMediasByTypes(request);
        }

        public object Get(GetPeopleWhoWatchedRequest request)
        {
            return _repository.GetPeopleWhoWatched(request);
        }

        public object Get(SearchMediaByAndOrListRequest request)
        {
            return _repository.SearchMediaByAndOrList(request);
        }

        public object Get(GetAutoCompleteSearchListRequest request)
        {
            return _repository.GetAutoCompleteSearchList(request);
        }

        public object Get(GetSubscriptionIDsContainingMediaFileRequest request)
        {
            return _repository.GetSubscriptionIDsContainingMediaFile(request);
        }

        public object Get(GetItemsPricesWithCouponsRequest request)
        {
            return _repository.GetItemsPricesWithCoupons(request);
        }

        public object Get(IsItemPurchasedRequest request)
        {
            return _repository.IsItemPurchased(request);
        }

        public object Get(IsUserSocialActionPerformedRequest request)
        {
            return _repository.IsUserSocialActionPerformed(request);
        }

        public object Get(GetMediaLicenseLinkRequest request)
        {
            return _repository.GetMediaLicenseLink(request);
        }

        public object Get(GetUsersLikedMediaRequest request)
        {
            return _repository.GetUsersLikedMedia(request);
        }

        #endregion

        #region PUT
        #endregion

        #region POST

        public object Post(AddCommentRequest request)
        {
            return _repository.AddComment(request);
        }

        public object Post(MediaMarkRequest request)
        {
            return _repository.MediaMark(request);
        }

        public object Post(MediaHitRequest request)
        {
            return _repository.MediaHit(request);
        }

        public object Post(SendToFriendRequest request)
        {
            return _repository.SendToFriend(request);
        }

        public object Post(ChargeMediaWithPrepaidRequest request)
        {
            return _repository.ChargeMediaWithPrepaid(request);
        }

        public object Post(ActionDoneRequest request)
        {
            return _repository.ActionDone(request);
        }

        #endregion

        #region DELETE
        #endregion

    }
}
