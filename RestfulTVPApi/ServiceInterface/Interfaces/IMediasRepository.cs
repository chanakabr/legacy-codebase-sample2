using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Context;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IMediasRepository
    {
        List<TVPApiModule.Objects.Responses.Media> GetMediasInfo(GetMediasInfoRequest request);

        List<TVPApiModule.Objects.Responses.Comment> GetMediaComments(GetMediaCommentsRequest request);

        bool AddComment(AddCommentRequest request);

        TVPApiModule.Objects.Responses.MediaMarkObject GetMediaMark(GetMediaMarkRequest request);

        string MediaMark(RestfulTVPApi.ServiceModel.MediaMarkRequest request);

        string MediaHit(RestfulTVPApi.ServiceModel.MediaHitRequest request);

        List<TVPApiModule.Objects.Responses.Media> GetRelatedMediasByTypes(GetRelatedMediasByTypesRequest request);

        List<TVPApiModule.Objects.Responses.Media> GetPeopleWhoWatched(GetPeopleWhoWatchedRequest request);

        List<TVPApiModule.Objects.Responses.Media> SearchMediaByAndOrList(SearchMediaByAndOrListRequest request);

        bool SendToFriend(SendToFriendRequest request);

        List<string> GetAutoCompleteSearchList(GetAutoCompleteSearchListRequest request);

        List<int> GetSubscriptionIDsContainingMediaFile(GetSubscriptionIDsContainingMediaFileRequest request);

        List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(GetItemsPricesWithCouponsRequest request);

        bool IsItemPurchased(IsItemPurchasedRequest request);

        bool IsUserSocialActionPerformed(IsUserSocialActionPerformedRequest request);

        string GetMediaLicenseLink(GetMediaLicenseLinkRequest request);

        PrePaidResponseStatus ChargeMediaWithPrepaid(ChargeMediaWithPrepaidRequest request);

        bool ActionDone(ActionDoneRequest request);

        List<string> GetUsersLikedMedia(GetUsersLikedMediaRequest request);

        TVPApiModule.Objects.Responses.BuzzWeightedAverScore GetBuzzMeterData(GetBuzzMeterDataRequest request);

        List<TVPApiModule.Objects.Responses.AssetStatsResult> GetAssetsStats(GetAssetsStatsRequest request);

        List<TVPApiModule.Objects.Responses.AssetStatsResult> GetAssetsStatsForTimePeriod(GetAssetsStatsForTimePeriodRequest request);

        bool DoesBundleContainMedia(DoesBundleContainMediaRequest request);

        List<TVPApiModule.Objects.Responses.Media> GetBundleMedia(GetBundleMediaRequest request);

        SearchAssetsResponse SearchAssets(SearchAssetsRequest request);
    }
}