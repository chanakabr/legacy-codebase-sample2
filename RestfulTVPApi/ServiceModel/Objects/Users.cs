using System;
using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using RestfulTVPApi.Notification;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;

namespace RestfulTVPApi.ServiceModel
{

    #region GET

    [Route("/users/data", "POST", Notes = "This method returns the user details, as an array, for each user ID entered. When entering user IDs, enter them in a single string, separated by a semicolon.")]
    public class GetUsersDataRequest : RequestBase, IReturn<List<UserResponseObject>>
    {
        [ApiMember(Name = "site_guids", Description = "Users Identifiers", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guids { get; set; }
    }

    [Route("/users/{site_guid}/subscriptions/permitted", "GET", Notes = "This method returns an array of media subscriptions that were purchased by the user including time, date, viewing and purchase details. ")]
    public class GetUserPermitedSubscriptionsRequest : RequestBase, IReturn<List<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/subscriptions/expired", "GET", Notes = "This method returns an array of subscriptions that the user has purchased and which are now expired. Example: Can display the expired subscriptions items in the user’s personal zone.")]
    public class GetUserExpiredSubscriptionsRequest : PagingRequest, IReturn<List<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/collections/expired", "GET", Notes = "This method returns an array of collections that the user has purchased and which are now expired. Example: Can display the expired collections items in the user’s personal zone.")]
    public class GetUserExpiredCollectionsRequest : RequestBase, IReturn<List<PermittedCollectionContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "num_of_items", Description = "", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int num_of_items { get; set; }
    }

    [Route("/users/{site_guid}/items", "GET", Notes = "This method returns an array of user items (favorites, rentals etc.,). The media type returns within the media object. Use this method to obtain all personal information for user‟s personal zone")]
    public class GetUserItemsRequest : PagingRequest, IReturn<List<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "item_type", Description = "Item type", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("item_type", typeof(RestfulTVPApi.Objects.Enums.UserItemType))]
        public RestfulTVPApi.Objects.Enums.UserItemType item_type { get; set; }
    }    

    [Route("/users/{site_guid}/medias/permitted", "GET", Notes = "This method returns an array of media file items that were purchased by the user including time, date, viewing and purchase details. ")]
    public class GetUserPermittedItemsRequest : RequestBase, IReturn<List<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/medias/expired", "GET", Notes = "This method returns an array of media items that the user has purchased and which are now expired. Example: Can display the expired items in the user’s personal zone.")]
    public class GetUserExpiredItemsRequest : PagingRequest, IReturn<List<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/medias/favorites", "GET", Summary = "Get User Favorites", Notes = "Get User Favorites")]
    public class GetUserFavoritesRequest : RequestBase, IReturn<List<FavoriteObject>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/rules", "GET", Summary = "Get User Group Rules", Notes = "Get User Group Rules")]
    public class GetUserGroupRulesRequest : RequestBase, IReturn<List<GroupRule>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/rules/check", "GET", Summary = "Check User Group Rule", Notes = "Check User Group Rules")]
    public class CheckGroupRuleRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
        [ApiMember(Name = "pin", Description = "PIN", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
    }
    
    //Ofir - change route - PUT?
    [Route("/users/{site_guid}/rules/{rule_id}/renew", "GET", Notes = "This method sends the user a \"renew user PIN\" email with a temporary access token. Example: Adult content on TV, etc. Sends user an email with info.")]
    public class RenewUserPINRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule identification", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_type}", "GET", Summary = "Get Item From List", Notes = "Get Item From List")]
    public class GetItemFromListRequest : RequestBase, IReturn<List<UserItemList>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public RestfulTVPApi.Users.ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "query", DataType = "ItemType", IsRequired = true)]
        public RestfulTVPApi.Users.ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Objects", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Users.ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_type}/exists", "GET", Notes = "This method checks whether an item exists or does not exist in a list. It queries the list for an array of specific item. Return is an array of Booleans for each item.")]
    public class IsItemExistsInListRequest : RequestBase, IReturn<List<KeyValuePair<string, string>>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public RestfulTVPApi.Users.ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "query", DataType = "ItemType", IsRequired = true)]
        public RestfulTVPApi.Users.ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each objec contains item id and item order in the list.", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Users.ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/pre_paid_balance", "GET", Summary = "Get User", Notes = "Get User")]
    public class GetPrepaidBalanceRequest : RequestBase, IReturn<List<string>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "currency_code", Description = "Currency Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string currency_code { get; set; }
    }

    //Ofir - Should it be POST? - creates new email..
    //Change route, not in users...
    [Route("/users/{user_name}/activation_mail", "GET", Notes = "This method resend activation mail to a user who has not activated his/her account within a specified amount of time")]
    public class ResendActivationMailRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/users/{site_guid}/medias/last_watched", "GET", Notes = "This method returns an array listing the last watched media (without reference to time period).")]
    public class GetLastWatchedMediasByPeriodRequest : RequestBase, IReturn<List<Media>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Picture size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "period_before", Description = "The number of periods preceeding the current period", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int period_before { get; set; }
        [ApiMember(Name = "by_period", Description = "By time period", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("by_period", typeof(RestfulTVPApi.Objects.Enums.ePeriod))]
        public RestfulTVPApi.Objects.Enums.ePeriod by_period { get; set; }
    }

    [Route("/users/{site_guid}/medias/social", "GET", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
    public class GetUserSocialMediasRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int social_platform { get; set; }
        [ApiMember(Name = "social_action", Description = "Social Action", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int social_action { get; set; }
    }

    [Route("/users/{site_guid}/transaction_history", "GET", Summary = "Get User Transaction History", Notes = "Get User Transaction History")]
    public class GetUserTransactionHistoryRequest : PagingRequest, IReturn<BillingTransactionsResponse>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    //Ofir - Change route, not in users?
    [Route("/users/{site_guid}/google_signature", "GET", Summary = "Get User Transaction History", Notes = "Get User Transaction History")]
    public class GetGoogleSignatureRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "customer_id", Description = "Customer ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int customer_id { get; set; }
    }

    [Route("/users/{site_guids}/billing_history", "GET", Notes = "This method returns a user‟s billing history for a given time range.")]
    public class GetUsersBillingHistoryRequest : RequestBase, IReturn<List<UserBillingTransactionsResponse>>
    {
        [ApiMember(Name = "site_guids", Description = "User's Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] site_guids { get; set; }
        [ApiMember(Name = "start_date", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime start_date { get; set; }
        [ApiMember(Name = "end_date", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime end_date { get; set; }
    }

    [Route("/users/{site_guids}/last_billing_info", "GET", Notes = "Returns last billing information of the user. This is used when sending previous billing information to the payment server (e.g., VISA). Presented to user for OK. Goal is for user to verify and approve")]
    public class GetLastBillingUserInfoRequest : RequestBase, IReturn<List<AdyenBillingDetail>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "billing_method", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public int billing_method { get; set; }
    }

    [Route("/users/{site_guid}/medias/{media_ids}/are_favorites", "GET", Notes = "Send list of mediaIDs, returns is an array of key value pairs (the media asset ID, 'True' = Is user favorite; 'False' = Is not user favorite)")]
    public class AreMediasFavoriteRequest : RequestBase, IReturn<List<KeyValuePair<long, bool>>>
    {
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public List<int> media_ids { get; set; }
    }

    //Ofir - expose with_dynamic? Should be in Media?
    [Route("/users/{site_guid}/medias/recommended", "GET", Notes = "This method returns an array of recommended media filtered by media type")]
    public class GetRecommendedMediasByTypesRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_types", Description = "Media Types", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_types { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "with_dynamic", Description = "With Dynamic Data?", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool with_dynamic { get; set; }
    }

    [Route("/users/{site_guid}/notifications", "GET", Notes = "This method gets device notifications.")]
    public class GetDeviceNotificationsRequest : PagingRequest, IReturn<List<RestfulTVPApi.Objects.Responses.Notification>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("notification_type", typeof(NotificationMessageType))]
        [ApiMember(Name = "notification_type", Description = "Notification Type", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public NotificationMessageType notification_type { get; set; }
        [ApiAllowableValues("view_status", typeof(NotificationMessageViewStatus))]
        [ApiMember(Name = "view_status", Description = "View Status", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public NotificationMessageViewStatus view_status { get; set; }
    }

    [Route("/users/{site_guid}/notifications/tags", "GET", Notes = "This method returns the tags the user is currently subscribed to for followup notifications. Related to FollowUpByTag.")]
    public class GetUserStatusSubscriptionsRequest : RequestBase, IReturn<List<RestfulTVPApi.Objects.Responses.Notification>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    //Ofir - Change route?
    [Route("/users/{site_guid}/medias/unfinished", "GET", Notes = "This method returns an array containing the media object IDs of media that the user started watching, but did not finish watching.")]
    public class GetUserStartedWatchingMediasRequest : PagingRequest, IReturn<List<string>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/session", "GET", Notes = "This method discovers whether the user is or is not signed in.")]
    public class IsUserSignedInRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/friends/medias/watched", "GET", Notes = "This method returns records listing user friend, media watched, and last date media was last watched by the user’s friends.")]
    public class GetAllFriendsWatchedRequest : PagingRequest, IReturn<List<FriendWatchedObject>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/friends/social_platforms/{social_platform}/actions", "GET", Notes = "This method returns all social actions carried out by friends. Note: The result is filtered according to the input parameters.")]
    public class GetFriendsActionsRequest : PagingRequest, IReturn<List<UserSocialActionObject>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "user_actions", Description = "User Actions", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] user_actions { get; set; }
        [ApiAllowableValues("asset_type", typeof(RestfulTVPApi.Social.eAssetType))]
        [ApiMember(Name = "asset_type", Description = "Asset Type", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eAssetType asset_type { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
    }

    [Route("/users/{site_guid}/social_platforms/{social_platform}/actions", "GET", Notes = "This method returns all the social actions carried out by the user. Note: the results are filtered according to the specified parameters.")]
    public class GetUserActionsRequest : PagingRequest, IReturn<List<UserSocialActionObject>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("user_action", typeof(RestfulTVPApi.Social.eUserAction))]
        [ApiMember(Name = "user_action", Description = "User Action", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eUserAction user_action { get; set; }
        [ApiAllowableValues("asset_type", typeof(RestfulTVPApi.Social.eAssetType))]
        [ApiMember(Name = "asset_type", Description = "Asset Type", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eAssetType asset_type { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
    }

    [Route("/users/{site_guid}/social_platforms/facebook/privacy_settings", "GET", Notes = "This method returns the user’s social privacy-level options as listed/configured on the based upon the privacy configured at his the user’s social network. Note: These are the social network privacy settings, not the Tvinci privacy settings.")]
    public class GetUserAllowedSocialPrivacyListRequest : PagingRequest, IReturn<List<eSocialPrivacy>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    //Ofir -  ask avi
    [Route("/users/{site_guid}/social_platforms/{social_platform}/actions/{user_action}/privacy_settings", "GET", Notes = "This method returns the user’s external privacy level settings for a specific social action. Note: These settings determine whether the action can be viewed by the user’s social-network friends on the social network.")]
    public class GetUserExternalActionShareRequest : RequestBase, IReturn<eSocialActionPrivacy>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("user_action", typeof(RestfulTVPApi.Social.eUserAction))]
        [ApiMember(Name = "user_action", Description = "User Action", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eUserAction user_action { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
    }

    //??? ask avi
    [Route("/users/{site_guid}/actions/{user_action}/privacy_settings", "GET", Notes = "This method returns the user’s internal privacy level settings for a specific social action. Note: These settings determine whether the action can be viewed by the user’s social-network friends on the internal site.")]
    public class GetUserInternalActionPrivacyRequest : GetUserExternalActionShareRequest { }

    //??? ask avi
    [Route("/users/{site_guid}/social_platforms/facebook/friends", "GET", Notes = "This method returns a list containing all of the user's friends.")]
    public class GetUserFriendsRequest : PagingRequest, IReturn<List<string>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    //??? ask avi
    [Route("/users/{site_guid}/social_platforms/{social_platform}/privacy_settings", "GET", Notes = "This method returns the user’s social privacy level (authorizations) for a specified social platform and action.")]
    public class GetUserSocialPrivacyRequest : RequestBase, IReturn<eSocialPrivacy>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("user_action", typeof(RestfulTVPApi.Social.eUserAction))]
        [ApiMember(Name = "user_action", Description = "User Action", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eUserAction user_action { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
    }

    //Maybe POST?
    [Route("/users/{site_guid}/ad_custom_data_id", "GET", Notes = "This method returns customer data. This is the first step in the purchase flow. Insert the method’s parameters (price, payment method, etc.,). Returns an integer.")]
    public class AD_GetCustomDataIDRequest : RequestBase, IReturn<int>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency_code", Description = "Currency Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency_code { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "campaign_code", Description = "Campaign Code", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string campaign_code { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "payment_method", Description = "Payment Method", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "asset_type", Description = "Asset Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_type { get; set; }
    }

    //Maybe POST?
    [Route("/users/{site_guid}/custom_data_id", "GET", Notes = "This method is used as part of purchase flow process. The site indicates the item to purchase and passes required information (item ID, user information, purchase method, etc.) to the Tvinci system.")]
    public class GetCustomDataIDRequest : AD_GetCustomDataIDRequest
    {
        [ApiMember(Name = "override_end_date", Description = "Override end date?", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string override_end_date { get; set; }
    }

    [Route("/users/password", "POST", Notes = "This method is used when the user has forgotten his/her password. A new password is emailed to the user")]
    public class SendNewPasswordRequest : RequestBase, IReturn<RestfulTVPApi.Objects.Response.Status>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
    }

    [Route("/users/{site_guid}/billingTypeUserInfo", "GET", Summary = "Gets a hashed string from Adyen for validation purposes", Notes = "")]
    public class GetLastBillingTypeUserInfoRequest : RequestBase, IReturn<AdyenBillingDetail>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/collections/permitted", "GET", Summary = "Get the user's permitted colections", Notes = "")]
    public class GetUserPermittedCollectionsRequest : RequestBase, IReturn<PermittedCollectionContainer>
    {
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/token/{token}", "GET", Summary = "Returns the user username by checking the token", Notes = "")]
    public class CheckTemporaryTokenRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    [Route("/users/{site_guid}/subscription/{sub_id}/permitted", "GET", Summary = "Returns if the subscription is permitted to the user", Notes = "")]
    public class IsPermittedSubscriptionRequest : RequestBase, IReturn<RestfulTVPApi.Objects.Response.Status>
    {
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "sub_id", Description = "Subscription id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int sub_id { get; set; }
    }



    #endregion

    #region PUT

    [Route("/users", "PUT", Summary = "Update User", Notes = "Update User")]
    public class SetUserDataRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "user_basic_data", Description = "User Basic Data", ParameterType = "body", DataType = "UserBasicData", IsRequired = true)]
        public RestfulTVPApi.Objects.Responses.UserBasicData user_basic_data { get; set; }
        [ApiMember(Name = "user_dynamic_data", Description = "User Dynamic Data", ParameterType = "body", DataType = "UserDynamicData", IsRequired = true)]
        public RestfulTVPApi.Objects.Responses.UserDynamicData user_dynamic_data { get; set; }
    }

    [Route("/users/{site_guid}/dynamic_data/{key}", "PUT", Notes = "This method expects dynamic data key and data value; it sets the value to the existing key. Example: Key = birthday, Value = 03/03/2014.")]
    public class SetUserDynamicDataRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "key", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string key { get; set; }
        [ApiMember(Name = "value", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string value { get; set; }
    }

    [Route("/users/{site_guid}/rules/{rule_id}", "PUT", Summary = "Update User Group Rule", Notes = "Update User Group Rules")]
    public class SetUserGroupRuleRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
        [ApiMember(Name = "pin", Description = "PIN", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
        [ApiMember(Name = "is_active", Description = "Is Active", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int is_active { get; set; }
    }

    [Route("/users/password", "PUT", Notes = "User wants to change password. Must enter Old and new passwords.")]
    public class ChangeUserPasswordRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "old_password", Description = "Old password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string old_password { get; set; }
        [ApiMember(Name = "new_password", Description = "New password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string new_password { get; set; }
    }

    [Route("/users/{user_name}/activate_status", "PUT", Notes = "This method activates a user account. User registers with email. An email is sent to the user‟s email address. It includes link and a token. The link sends users email an activation link. When user clicks the link a method is invoked. The method takes the token from the URI and sends event to Tvinci backend.")]
    public class ActivateAccountRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "token", Description = "Account activation token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    //Ofir - problematic route, combine with ActivateAccount?
    [Route("/users/{user_name}/activate_by_domain_master", "PUT", Notes = "This method approves adding a user to a domain by the domain master.")]
    public class ActivateAccountByDomainMasterRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User Name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "master_user_name", Description = "Master User Name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string master_user_name { get; set; }
        [ApiMember(Name = "token", Description = "User Activation Token for Domain", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_type}", "PUT", Summary = "Update Item In List", Notes = "Update Item In List")]
    public class UpdateItemInListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public RestfulTVPApi.Users.ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public RestfulTVPApi.Users.ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Objects", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Users.ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/pre_paid_balance", "PUT", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
    public class CC_ChargeUserForPrePaidRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "product_code", Description = "Product Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string product_code { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
    }

    [Route("/users/{site_guid}/notifications/view_status", "PUT", Notes = "This method sets notification view status.")]
    public class SetNotificationMessageViewStatusRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "notification_request_id", Description = "notification_request_id", ParameterType = "body", DataType = SwaggerType.Long, IsRequired = false)]
        public long? notification_request_id { get; set; }
        [ApiMember(Name = "notification_message_id", Description = "notification_message_id", ParameterType = "body", DataType = SwaggerType.Long, IsRequired = false)]
        public long? notification_message_id { get; set; }
        [ApiAllowableValues("view_status", typeof(NotificationMessageViewStatus))]
        [ApiMember(Name = "view_status", Description = "View Status", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public NotificationMessageViewStatus view_status { get; set; }
    }

    [Route("/users/{site_guid}/social_platforms/{social_platform}/actions/{user_action}/privacy_settings/external", "PUT", Notes = "This method sets the user’s external privacy level settings for a specific social action. Note: These settings determine whether the action can be viewed by the user’s social-network friends on the external social network.")]
    public class SetUserExternalActionShareRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("user_action", typeof(RestfulTVPApi.Social.eUserAction))]
        [ApiMember(Name = "user_action", Description = "User Action", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eUserAction user_action { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
        [ApiAllowableValues("social_action_privacy", typeof(RestfulTVPApi.Social.eSocialActionPrivacy))]
        [ApiMember(Name = "social_action_privacy", Description = "Social Action Privacy", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eSocialActionPrivacy social_action_privacy { get; set; }
    }

    [Route("/users/{site_guid}/social_platforms/{social_platform}/actions/{user_action}/privacy_settings/internal", "PUT", Notes = "This method sets the user’s internal privacy level settings for a specific social action. Note: These settings determine whether the action can be viewed by the user’s social-network friends on the internal site.")]
    public class SetUserInternalActionPrivacyRequest : SetUserExternalActionShareRequest { }

    [Route("/users/{site_guid}/subscription/{old_subscription}/change/{new_subscription}", "PUT", Summary = "ChangeSubscription enable customer care representative migrating user from an existing subscription to a new one, while maintaining the same renewal date and credit card but adapting to the subscription's new content and pricing. After a user has migrated from subscription A to B, he will not be charged until the end of the A's current billing cycle. Nevertheless, he will instantly receive subscription B's content entitlements. At the beginning of next billing cycle, he will be charged subscription B's price", Notes = "")]
    public class ChangeSubscriptionRequest : RequestBase, IReturn<ChangeSubscriptionStatus>
    {
        [ApiMember(Name = "site_guid", Description = "User identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "old_subscription", Description = "Current subscription Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int old_subscription { get; set; }
        [ApiMember(Name = "new_subscription", Description = "New subscription Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int new_subscription { get; set; }
    }

    [Route("/users/transaction/cancel", "PUT", Summary = "", Notes = "")]
    public class CancelTransactionRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
        [ApiMember(Name = "transaction_type", Description = "Transaction type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public RestfulTVPApi.ConditionalAccess.eTransactionType transaction_type { get; set; }
        [ApiMember(Name = "is_force", Description = "Cancel now or in end of period", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool is_force { get; set; }
    }

    [Route("/users/{site_guid}/transaction_waiver/{asset_id}", "PUT", Summary = "", Notes = "")]
    public class WaiverTransactionRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
        [ApiMember(Name = "transaction_type", Description = "Transaction type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public RestfulTVPApi.ConditionalAccess.eTransactionType transaction_type { get; set; }
    }

    #endregion

    #region POST

    [Route("/users", "POST", Summary = "Add User", Notes = "Add User")]
    public class SignUpRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_basic_data", Description = "User Basic Data", ParameterType = "body", DataType = "UserBasicData", IsRequired = true)]
        public RestfulTVPApi.Users.UserBasicData user_basic_data { get; set; }
        [ApiMember(Name = "user_dynamic_data", Description = "User Dynamic Data", ParameterType = "body", DataType = "UserDynamicData", IsRequired = true)]
        public RestfulTVPApi.Users.UserDynamicData user_dynamic_data { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
        [ApiMember(Name = "affiliate_code", Description = "Affiliate Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = false)]
        public string affiliate_code { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_type}", "POST", Notes = "This method adds an item to a list.")]
    public class AddItemToListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public RestfulTVPApi.Users.ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public RestfulTVPApi.Users.ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each object contains item id and item order in the list", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Users.ItemObj[] item_objects { get; set; }
    }
    
    [Route("/users/sign_in", "POST", Notes = "This method signs-in a user.")]
    public class SignInRequest : RequestBase, IReturn<RestfulTVPApi.Clients.UsersClient.LogInResponseData>
    {
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/users/fb/sign_in", "POST", Notes = "This method signs-in a user via FB.")]
    public class FBUserSigninRequest : RequestBase, IReturn<FBSignIn>
    {
        [ApiMember(Name = "token", Description = "token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
        [ApiMember(Name = "ip", Description = "IP", ParameterType = "body", DataType = SwaggerType.String, IsRequired = false)]
        public string ip { get; set; }
        [ApiMember(Name = "device_id", Description = "Device id", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_id { get; set; }
        [ApiMember(Name = "prevent_double_logins", Description = "prevent double logins", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = false)]
        public bool prevent_double_logins { get; set; }
    }

    [Route("/users/{token}/token_sign_in", "POST", Notes = "This method signs-in a user with a token")]
    public class SignInWithTokenRequest : RequestBase, IReturn<RestfulTVPApi.Clients.UsersClient.LogInResponseData>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
    }

    [Route("/users/{site_guid}/actions", "POST", Notes = "This method does a requested user social action")]
    public class DoUserActionRequest : RequestBase, IReturn<SocialActionResponseStatus>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiAllowableValues("user_action", typeof(RestfulTVPApi.Social.eUserAction))]
        [ApiMember(Name = "user_action", Description = "User Action", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eUserAction user_action { get; set; }
        [ApiMember(Name = "extra_params", Description = "Extra Params", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Social.KeyValuePair[] extra_params { get; set; }
        [ApiAllowableValues("social_platform", typeof(RestfulTVPApi.Social.SocialPlatform))]
        [ApiMember(Name = "social_platform", Description = "Social Platform", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.SocialPlatform social_platform { get; set; }
        [ApiAllowableValues("asset_type", typeof(RestfulTVPApi.Social.eAssetType))]
        [ApiMember(Name = "asset_type", Description = "Asset Type", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public RestfulTVPApi.Social.eAssetType asset_type { get; set; }
        [ApiMember(Name = "asset_id", Description = "Asset ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int asset_id { get; set; }
    }

    //Ofir - Need to understand the method to decide on routing
    [Route("/users/{site_guid}/charge_in_app", "POST", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
    public class InApp_ChargeUserForMediaFileRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "product_code", Description = "Product Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string product_code { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "receipt", Description = "Receipt", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string receipt { get; set; }
    }

    [Route("/users/reset_password", "POST", Notes = "This method sets a new password when user has forgotten password. Admin uses to set a new password.")]
    public class RenewUserPasswordRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/users/{site_guid}/purchase_token", "POST", Notes = "This method returns customer data. This is the first step in the purchase flow. Insert the method’s parameters (price, payment method, etc.,). Returns an integer. It’s the same method as GetCustomDataID only it also receives an extra parameter called 'previewModuleID' for the 'free trial' feature.")]
    public class CreatePurchaseTokenRequest : GetCustomDataIDRequest, IReturn<int>
    {
        [ApiMember(Name = "preview_module_id", Description = "The Preview model Identifier that was predefined in the TVM", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string preview_module_id { get; set; }
    }

    [Route("/users/{site_guid}/dummy_charge_collection/{collection_id}", "POST", Notes = "")]
    public class DummyChargeUserForCollectionRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "collection_id", Description = "Collection Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string collection_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }        
    }

    [Route("/users/subscriptions/charge/dummy", "POST", Notes = "")]
    public class DummyChargeUserForSubscriptionRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Id", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }        
    }

    [Route("/users/{site_guid}/charge_collection/{collection_id}", "POST", Notes = "")]
    public class ChargeUserForCollectionRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "collection_code", Description = "Collection Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string collection_code { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }        
        [ApiMember(Name = "payment_method_id", Description = "Payment method identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method_id { get; set; }
        [ApiMember(Name = "encrypted_cvv", Description = "3 digits on card's back", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_cvv { get; set; }
    }

    [Route("/users/{site_guid}/cellular_charge_subscription/{subscription_id}", "POST", Notes = "")]
    public class CellularChargeUserForSubscriptionRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_code { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }        
    }

    [Route("/users/subscriptions/charge/payment_method", "POST", Notes = "")]
    public class ChargeUserForSubscriptionByPaymentMethodRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "subscription_id", Description = "Subscription Id", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_code { get; set; }
        [ApiMember(Name = "payment_method_id", Description = "Payment method Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method_id { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "encrypted_cvv", Description = "3 digits on card's back", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_cvv { get; set; }
    }

    [Route("/users/{site_guid}/charge_media_file/{media_file_id}/payment_method/{payment_method_id}", "POST", Notes = "")]
    public class ChargeUserForMediaFileByPaymentMethodRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_file_id", Description = "Media file identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "payment_method_id", Description = "Payment method Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method_id { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }        
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }        
        [ApiMember(Name = "encrypted_cvv", Description = "3 digits on card's back", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_cvv { get; set; }
    }

    [Route("/users/{site_guid}/cellular_charge_media_file/{media_file_id}", "POST", Notes = "")]
    public class CellularChargeUserForMediaFileRequest : RequestBase, IReturn<BillingResponse>
    {
        [ApiMember(Name = "media_file_id", Description = "Media file identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    [Route("/users/{site_guid}/charge_media_file_CC/{media_file_id}", "POST", Notes = "")]
    public class ChargeUserForMediaFileUsingCCRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "media_file_id", Description = "Media file identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_file_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "payment_method_id", Description = "Payment method Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method_id { get; set; }
        [ApiMember(Name = "encrypted_cvv", Description = "3 digits on card's back", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_cvv { get; set; }
    }

    [Route("/users/{site_guid}/charge_media_subscription_CC/{subscription_id}", "POST", Notes = "Perform a user purchase for subscription using credit card")]
    public class ChargeUserForMediaSubscriptionUsingCCRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "subscription_id", Description = "Media file identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "price", Description = "Price to charge", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }        
        [ApiMember(Name = "extra_parameters", Description = "Extra parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "country_code", Description = "Country code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "udid", Description = "Device identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "payment_method_id", Description = "Payment method Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string payment_method_id { get; set; }
        [ApiMember(Name = "encrypted_cvv", Description = "3 digits on card's back", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_cvv { get; set; }
    }

    #endregion

    #region DELETE

    [Route("/users/{site_guid}/lists/{list_id}", "DELETE", Notes = "This method removes an item from a list. It is the opposite of AddItemToList.")]
    public class RemoveItemFromListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public RestfulTVPApi.Users.ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public RestfulTVPApi.Users.ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each objec contains item id and item order in the list.", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public RestfulTVPApi.Users.ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/medias/watch_history", "DELETE", Notes = "This method clears the user watch history (in the user’s personal zone) by individual media IDs. Note: The parameter “0” erases all entries.")]
    public class ClearUserHistoryRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_ids", Description = "Medias IDs", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_ids { get; set; }
    }

    [Route("/users/subscriptions/cancel", "DELETE", Notes = "This method cancels a subscription previously purchased by the user. Requires the subscription identifier and the subscription purchase identifier. Returns boolean success/fail")]
    public class CancelSubscriptionRequest : RequestBase, IReturn<RestfulTVPApi.Objects.Response.Status>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "subscription_purchase_id", Description = "Subscription Purchase ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_purchase_id { get; set; }
    }

    [Route("/users/subscriptions/renewal/cancel", "DELETE", Notes = "Cancel a household service subscription at the next renewal. The subscription stays valid untill the next renewal")]
    public class CancelSubscriptionRenewalRequest : RequestBase, IReturn<RestfulTVPApi.Objects.Response.Status>
    {
        [ApiMember(Name = "domain_id", Description = "Domain id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
    }

    [Route("/users/{user_name}/session", "DELETE", Notes = "This method signs-in a user.")]
    public class SignOutRequest : RequestBase, IReturn<RestfulTVPApi.Clients.UsersClient.LogInResponseData>
    {
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
    }

    #endregion

}