using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using ServiceStack;
using TVPPro.SiteManager.TvinciPlatform.Users;
using System.Collections.Generic;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using System;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.api;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/users/{site_guids}", "GET", Summary = "Get User", Notes = "Get User")]
    public class GetUsersDataRequest : RequestBase, IReturn<UserResponseObject[]>
    {
        [ApiMember(Name = "site_guids", Description = "Users Identifiers", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guids { get; set; }
    }

    [Route("/users/{site_guid}", "PUT", Summary = "Update User", Notes = "Update User")]
    public class SetUserDataRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "user_basic_data", Description = "User Basic Data", ParameterType = "body", DataType = "UserBasicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData user_basic_data { get; set; }
        [ApiMember(Name = "user_dynamic_data", Description = "User Dynamic Data", ParameterType = "body", DataType = "UserDynamicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData user_dynamic_data { get; set; }
    }

    [Route("/users", "POST", Summary = "Add User", Notes = "Add User")]
    public class SignUpRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_basic_data", Description = "User Basic Data", ParameterType = "body", DataType = "UserBasicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData user_basic_data { get; set; }
        [ApiMember(Name = "user_dynamic_data", Description = "User Dynamic Data", ParameterType = "body", DataType = "UserDynamicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData user_dynamic_data { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
        [ApiMember(Name = "affiliate_code", Description = "Affiliate Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string affiliate_code { get; set; }
    }

    [Route("/users/{site_guid}/subscriptions/permitted", "GET", Summary = "Get User Permitted Subscriptions", Notes = "Get User Permitted Subscriptions")]
    public class GetUserPermitedSubscriptionsRequest : RequestBase, IReturn<IEnumerable<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/subscriptions/expired", "GET", Summary = "Get User Expired Subscriptions", Notes = "Get User Expired Subscriptions")]
    public class GetUserExpiredSubscriptionsRequest : PagingRequest, IReturn<IEnumerable<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/items/{item_type}", "GET", Notes = "This method returns an array of user items (favorites, rentals etc.,). The media type returns within the media object. Use this method to obtain all personal information for user‟s personal zone")]
    public class GetUserItemsRequest : PagingRequest, IReturn<IEnumerable<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "item_type", Description = "Item type", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("item_type", typeof(UserItemType))]
        public UserItemType item_type { get; set; }
    }

    [Route("/users/{site_guid}/medias/permitted", "GET", Summary = "Get User Permitted Items", Notes = "Get User Permitted Items")]
    public class GetUserPermittedItemsRequest : RequestBase, IReturn<IEnumerable<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/medias/expired", "GET", Summary = "Get User Expired Items", Notes = "Get User Expired Items")]
    public class GetUserExpiredItemsRequest : PagingRequest, IReturn<IEnumerable<PermittedMediaContainer>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/favorites", "GET", Summary = "Get User Favorites", Notes = "Get User Favorites")]
    public class GetUserFavoritesRequest : RequestBase, IReturn<IEnumerable<FavoritObject>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/rules", "GET", Summary = "Get User Group Rules", Notes = "Get User Group Rules")]
    public class GetUserGroupRulesRequest : RequestBase, IReturn<IEnumerable<GroupRule>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
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

    [Route("/users/{user_name}/password/change", "PUT", Summary = "Change user password", Notes = "Change user password")]
    public class ChangeUserPasswordRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "old_password", Description = "Old password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string old_password { get; set; }
        [ApiMember(Name = "new_password", Description = "New password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string new_password { get; set; }
    }

    [Route("/users/{user_name}/password/renew", "PUT", Summary = "Renew user password", Notes = "Change user password")]
    public class RenewUserPasswordRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/users/{user_name}/activate", "PUT", Notes = "This method activates a user account. User registers with email. An email is sent to the user‟s email address. It includes link and a token. The link sends users email an activation link. When user clicks the link a method is invoked. The method takes the token from the URI and sends event to Tvinci backend.")]
    public class ActivateAccountRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "token", Description = "Account activation token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

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

    [Route("/users/types", "GET", Summary = "Get Group user types", Notes = "Get Group user types")]
    public class GetGroupUserTypesRequest : RequestBase, IReturn<IEnumerable<TVPPro.SiteManager.TvinciPlatform.Users.UserType>> { }

    [Route("/users/{site_guid}/rules/{rule_id}/renew", "GET", Notes = "This method sends the user a \"renew user PIN\" email with a temporary access token. Example: Adult content on TV, etc. Sends user an email with info.")]
    public class RenewUserPINRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule identification", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
    }

    [Route("/users/{user_name}/password", "GET", Summary = "Get user password (mail)", Notes = "Get user password")]
    public class SendPasswordMailRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_id}", "POST", Notes = "This method adds an item to a list.")]
    public class AddItemToListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each object contains item id and item order in the list", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_id}", "GET", Summary = "Get Item From List", Notes = "Get Item From List")]
    public class GetItemFromListRequest : RequestBase, IReturn<IEnumerable<UserItemList>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "query", DataType = "ItemType", IsRequired = true)]
        public ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Objects", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_id}/exists", "GET", Notes = "This method checks whether an item exists or does not exist in a list. It queries the list for an array of specific item. Return is an array of Booleans for each item.")]
    public class IsItemExistsInListRequest : RequestBase, IReturn<IEnumerable<KeyValuePair<string, string>>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "query", DataType = "ItemType", IsRequired = true)]
        public ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each objec contains item id and item order in the list.", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_id}", "DELETE", Notes = "This method removes an item from a list. It is the opposite of AddItemToList.")]
    public class RemoveItemFromListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Item objects, each objec contains item id and item order in the list.", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/lists/{list_id}", "PUT", Summary = "Update Item In List", Notes = "Update Item In List")]
    public class UpdateItemInListRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "list_type", Description = "List Type", ParameterType = "path", DataType = "ListType", IsRequired = true)]
        public ListType list_type { get; set; }
        [ApiMember(Name = "item_type", Description = "Item Type", ParameterType = "body", DataType = "ItemType", IsRequired = true)]
        public ItemType item_type { get; set; }
        [ApiMember(Name = "item_objects", Description = "Objects", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public ItemObj[] item_objects { get; set; }
    }

    [Route("/users/{site_guid}/pre_paid_balance", "GET", Summary = "Get User", Notes = "Get User")]
    public class GetPrepaidBalanceRequest : RequestBase, IReturn<string[]>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "currency_code", Description = "Currency Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string currency_code { get; set; }
    }

    //Ofir
    [Route("/users/coguid/{co_guid}", "GET", Notes = "This method get's a user's details from a 3rd party CoGuid.")]
    public class GetUserDataByCoGuidRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "co_guid", Description = "3rd party Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
        [ApiMember(Name = "operator_id", Description = "The SSO provider‟s identifier in the Tvinci system", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int operator_id { get; set; }
    }

    [Route("/users/{user_name}/activation_mail", "GET", Notes = "This method resend activation mail to a user who has not activated his/her account within a specified amount of time")]
    public class ResendActivationMailRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    //Ofir
    [Route("/users/{site_guid}/last_watched", "GET", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
    public class GetLastWatchedMediasRequest : PagingRequest, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    //Ofir
    [Route("/users/{site_guid}/social_medias", "GET", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
    public class GetUserSocialMediasRequest : PagingRequest, IReturn<IEnumerable<Media>>
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
    public class GetUserTransactionHistoryRequest : PagingRequest, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/countries", "GET", Notes = "This method returns a list of all countries, an ID and a symbol for each. Used to enable a user to select his/her country. Example: During user registration, the method returns an array of country codes, #ID, country name.")]
    public class GetCountriesListRequest : RequestBase, IReturn<IEnumerable<Country>> { }

    [Route("/users/{site_guid}/charge/pre_paid/cc", "POST", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
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

    [Route("/users/{site_guid}/google_signature", "GET", Summary = "Get User Transaction History", Notes = "Get User Transaction History")]
    public class GetGoogleSignatureRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "customer_id", Description = "Customer ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int customer_id { get; set; }
    }

    //Ofir
    [Route("/users/{site_guid}/medias_files/{media_file_ids}/prices", "GET", Summary = "Get AutoComplete Search List", Notes = "Get AutoComplete Search List")]
    public class GetItemsPricesWithCouponsRequest : RequestBase, IReturn<IEnumerable<MediaFileItemPricesContainer>>
    {
        [ApiMember(Name = "media_file_ids", Description = "Prefix Text", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] media_file_ids { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "only_lowest", Description = "Coupon Code", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool only_lowest { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    //Ofir
    [Route("/users/{site_guid}/subscriptions/{subscription_ids}/prices", "GET", Summary = "Get Subscription Prices With Coupon", Notes = "Get Subscription Prices With Coupon")]
    public class GetSubscriptionsPricesWithCouponRequest : RequestBase, IReturn<IEnumerable<SubscriptionsPricesContainer>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] subscription_ids { get; set; }
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    [Route("/users/{site_guids}/billing_history", "GET", Notes = "This method returns a user‟s billing history for a given time range.")]
    public class GetUsersBillingHistoryRequest : RequestBase, IReturn<IEnumerable<UserBillingTransactionsResponse[]>>
    {
        [ApiMember(Name = "site_guids", Description = "User's Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] site_guids { get; set; }
        [ApiMember(Name = "start_date", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime start_date { get; set; }
        [ApiMember(Name = "end_date", Description = "User's Identifiers", ParameterType = "query", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime end_date { get; set; }
    }

    [Route("/users/{site_guid}/charge/media/in_app", "POST", Summary = "Get Last Watched Medias", Notes = "Get Last Watched Medias")]
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

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class UsersService : Service
    {
        public IUsersRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetUsersDataRequest request)
        {
            var response = _repository.GetUsersData(request.InitObj, request.site_guids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserDataRequest request)
        {
            var response = _repository.SetUserData(request.InitObj, request.site_guid, request.user_basic_data, request.user_dynamic_data);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(SignUpRequest request)
        {
            var response = _repository.SignUp(request.InitObj, request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermitedSubscriptionsRequest request)
        {
            var response = _repository.GetUserPermitedSubscriptions(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredSubscriptionsRequest request)
        {
            var response = _repository.GetUserExpiredSubscriptions(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermittedItemsRequest request)
        {
            var response = _repository.GetUserPermittedItems(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredItemsRequest request)
        {
            var response = _repository.GetUserExpiredItems(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserFavoritesRequest request)
        {
            var response = _repository.GetUserFavorites(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserGroupRulesRequest request)
        {
            var response = _repository.GetUserGroupRules(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserGroupRuleRequest request)
        {
            var response = _repository.SetUserGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin, request.is_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(CheckGroupRuleRequest request)
        {
            var response = _repository.CheckGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(ChangeUserPasswordRequest request)
        {
            var response = _repository.ChangeUserPassword(request.InitObj, request.user_name, request.old_password, request.new_password);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(RenewUserPasswordRequest request)
        {
            var response = _repository.RenewUserPassword(request.InitObj, request.user_name, request.password);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccountRequest request)
        {
            var response = _repository.ActivateAccount(request.InitObj, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccountByDomainMasterRequest request)
        {
            var response = _repository.ActivateAccountByDomainMaster(request.InitObj, request.master_user_name, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGroupUserTypesRequest request)
        {
            var response = _repository.GetGroupUserTypes(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(RenewUserPINRequest request)
        {
            var response = _repository.RenewUserPIN(request.InitObj, request.site_guid, request.rule_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(SendPasswordMailRequest request)
        {
            var response = _repository.SendPasswordMail(request.InitObj, request.user_name);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(AddItemToListRequest request)
        {
            var response = _repository.AddItemToList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetItemFromListRequest request)
        {
            var response = _repository.GetItemFromList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemExistsInListRequest request)
        {
            var response = _repository.IsItemExistsInList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Delete(RemoveItemFromListRequest request)
        {
            var response = _repository.RemoveItemFromList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(UpdateItemInListRequest request)
        {
            var response = _repository.UpdateItemInList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetPrepaidBalanceRequest request)
        {
            var response = _repository.GetPrepaidBalance(request.InitObj, request.currency_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserDataByCoGuidRequest request)
        {
            var response = _repository.GetUserDataByCoGuid(request.InitObj, request.co_guid, request.operator_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(ResendActivationMailRequest request)
        {
            var response = _repository.ResendActivationMail(request.InitObj, request.user_name, request.password);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetLastWatchedMediasRequest request)
        {
            var response = _repository.GetLastWatchedMedias(request.InitObj, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserSocialMediasRequest request)
        {
            var response = _repository.GetUserSocialMedias(request.InitObj, request.social_platform, request.social_action, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserTransactionHistoryRequest request)
        {
            var response = _repository.GetUserTransactionHistory(request.InitObj, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetCountriesListRequest request)
        {
            var response = _repository.GetCountriesList(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(CC_ChargeUserForPrePaidRequest request)
        {
            var response = _repository.CC_ChargeUserForPrePaid(request.InitObj, request.price, request.currency, request.product_code, request.ppv_module_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGoogleSignatureRequest request)
        {
            var response = _repository.GetGoogleSignature(request.InitObj, request.customer_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionsPricesWithCouponRequest request)
        {
            var response = _repository.GetSubscriptionsPricesWithCoupon(request.InitObj, request.subscription_ids, request.site_guid, request.coupon_code, request.country_code, request.language_code, request.device_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetItemsPricesWithCouponsRequest request)
        {
            var response = _repository.GetItemsPricesWithCoupons(request.InitObj, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUsersBillingHistoryRequest request)
        {
            var response = _repository.GetUsersBillingHistory(request.InitObj, request.site_guids, request.start_date, request.end_date);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(InApp_ChargeUserForMediaFileRequest request)
        {
            var response = _repository.InApp_ChargeUserForMediaFile(request.InitObj, request.price, request.currency, request.product_code, request.ppv_module_code, request.receipt);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserItemsRequest request)
        {
            var response = _repository.GetUserItems(request.InitObj, request.item_type, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
