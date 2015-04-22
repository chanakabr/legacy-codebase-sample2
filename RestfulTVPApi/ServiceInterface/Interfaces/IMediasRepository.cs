using RestfulTVPApi.Objects.Models;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RestfulTVPApi.ServiceInterface
{
    public interface IMediasRepository
    {
        List<Media> GetMediasInfo(GetMediasInfoRequest request);

        List<Comment> GetMediaComments(GetMediaCommentsRequest request);

        bool AddComment(AddCommentRequest request);

        MediaMarkObject GetMediaMark(GetMediaMarkRequest request);

        string MediaMark(RestfulTVPApi.ServiceModel.MediaMarkRequest request);

        string MediaHit(RestfulTVPApi.ServiceModel.MediaHitRequest request);

        List<Media> GetRelatedMediasByTypes(GetRelatedMediasByTypesRequest request);

        List<Media> GetPeopleWhoWatched(GetPeopleWhoWatchedRequest request);

        List<Media> SearchMediaByAndOrList(SearchMediaByAndOrListRequest request);

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

        BuzzScore GetBuzzMeterData(GetBuzzMeterDataRequest request);

        List<AssetStats> GetAssetsStats(GetAssetsStatsRequest request);

        List<AssetStats> GetAssetsStatsForTimePeriod(GetAssetsStatsForTimePeriodRequest request);

        bool DoesBundleContainMedia(DoesBundleContainMediaRequest request);

        List<Media> GetBundleMedia(GetBundleMediaRequest request);

        SearchAssetsResponse SearchAssets(SearchAssetsRequest request);
    }
}