using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;

namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/medias", "GET", Notes = "Search media asset by specific tags and metas")]
    public class SearchMediaByAndOrListRequest : PagingRequest, IReturn<List<Media>>
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

    [Route("/medias/auto_complete/{prefix_text}", "GET", Notes = "This method auto-completes the entered text and returns the resulting media title strings as an array")]
    public class GetAutoCompleteSearchListRequest : RequestBase, IReturn<List<string>>
    {
        [ApiMember(Name = "prefix_text", Description = "Prefix Text", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string prefix_text { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_types { get; set; }
    }

    [Route("/medias/{media_ids}", "GET", Notes = "This method returns all of the stored information regarding a media asset")]
    public class GetMediasInfoRequest : RequestBase, IReturn<List<Media>>
    {
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/medias/{media_id}/comments", "GET", Notes = "This method returns an array of user's comments about the media specified")]
    public class GetMediaCommentsRequest : PagingRequest, IReturn<List<Comment>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/media_mark", "GET", Notes = "This method returns the last play-position for this specific user and media asset")]
    public class GetMediaMarkRequest : RequestBase, IReturn<MediaMark>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/medias/{media_id}/related", "GET", Notes = "This method returns an array of media assets related to a given media asset; it is limited to a specific media type. The related relationships are backend configurable; they are determined by tags")]
    public class GetRelatedMediasByTypesRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_types { get; set; }
    }

    //Ofir - need to change routing
    [Route("/medias/{media_id}/people_who_watched", "GET", Notes = "This method returns media assets that were watched by other users who have also watched this item")]
    public class GetPeopleWhoWatchedRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    //Problematic routing
    //Ofir - move to subscriptions? /subscriptios/{media_id} - combine with GetSubscriptionData
    //[Route("/medias/{media_id}/containing_subscriptions", "GET", Notes = "This method returns all subscriptions ID's containing a posted media and file ID")]
    [Route("/medias/{media_id}/subscriptions", "GET", Notes = "This method returns all subscriptions ID's containing a posted media and file ID")]
    public class GetSubscriptionIDsContainingMediaFileRequest : RequestBase, IReturn<List<int>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
    }

    //Problematic routing - cant add media id bcoz it accepts multiple files
    [Route("/medias/files/{media_file_ids}/prices", "GET", Summary = "Get AutoComplete Search List", Notes = "Get AutoComplete Search List")]
    public class GetItemsPricesWithCouponsRequest : RequestBase, IReturn<List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer>>
    {
        [ApiMember(Name = "media_file_ids", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_file_ids { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "only_lowest", Description = "Coupon Code", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool only_lowest { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    [Route("/medias/{media_id}/files/{media_file_id}/is_purchased", "GET", Notes = "Checks if the given item was or was not purchased by the user")]
    public class IsItemPurchasedRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    //Maybe /medias/{media_id}/actions/{social_action} - 400 would tell the order is NOT performed yet, 200 would tell it is performed.
    [Route("/medias/{media_id}/is_social_action_performed", "GET", Notes = "This method checks whether the user has performed a specific social action on a specific media.")]
    public class IsUserSocialActionPerformedRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int social_platform { get; set; }
        [ApiMember(Name = "social_action", Description = "Social Action", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int social_action { get; set; }
    }

    [Route("/medias/{media_id}/files/{media_file_id}/license_link", "GET", Notes = "This method returns a playable link to the requested media")]
    public class GetMediaLicenseLinkRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "base_link", Description = "Base Link", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string base_link { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/medias/{media_id}/people_who_liked", "GET", Notes = "This method returns a list of a user’s friends that liked a specified media.")]
    public class GetUsersLikedMediaRequest : PagingRequest, IReturn<List<string>>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "only_friends", Description = "Only Friends?", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool only_friends { get; set; }
    }

    #endregion

    #region PUT
    #endregion

    #region POST

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

    //Ofir - change route? Not RESTful
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

    //Ofir - change route? Not RESTful
    [Route("/medias/{media_id}/files/{media_file_id}/charge_with_pre_paid", "POST", Notes = "This method buys pay-per-view (PPV) with prepaid. This processing is done by Tvinci not by a third party processor")]
    public class ChargeMediaWithPrepaidRequest : RequestBase, IReturn<TVPApiModule.Objects.Responses.PrePaidResponseStatus>
    {
        [ApiMember(Name = "media_file_id", Description = "Media File ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
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

    [Route("/medias/{media_id}/actions", "POST", Notes = "Performs any of these following actions on the media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch). See also: AddUserSocialAction")]
    public class ActionDoneRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiAllowableValues("action", typeof(ActionType))]
        [ApiMember(Name = "action", Description = "Action", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public ActionType action_type { get; set; }
        [ApiMember(Name = "extra_val", Description = "Extra Variable", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int extra_val { get; set; }
    }

    #endregion

    #region DELETE
    #endregion

}