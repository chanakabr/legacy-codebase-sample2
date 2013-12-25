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

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/users/{site_guid}", "GET", Summary = "Get User", Notes = "Get User")]
    public class GetUserData : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}", "PUT", Summary = "Update User", Notes = "Update User")]
    public class SetUserData : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "user_basic_data", Description = "User Basic Data", ParameterType = "body", DataType = "UserBasicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData user_basic_data { get; set; }
        [ApiMember(Name = "user_dynamic_data", Description = "User Dynamic Data", ParameterType = "body", DataType = "UserDynamicData", IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData user_dynamic_data { get; set; }
    }

    [Route("/users", "POST", Summary = "Add User", Notes = "Add User")]
    public class SignUp : RequestBase, IReturn<UserResponseObjectDTO>
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
    public class GetUserPermitedSubscriptions : PagingRequest, IReturn<IEnumerable<SubscriptionContainerDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/subscriptions/expired", "GET", Summary = "Get User Expired Subscriptions", Notes = "Get User Expired Subscriptions")]
    public class GetUserExpiredSubscriptions : PagingRequest, IReturn<IEnumerable<SubscriptionContainerDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/items/permitted", "GET", Summary = "Get User Permitted Items", Notes = "Get User Permitted Items")]
    public class GetUserPermittedItems : PagingRequest, IReturn<IEnumerable<MediaContainerDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/items/expired", "GET", Summary = "Get User Expired Items", Notes = "Get User Expired Items")]
    public class GetUserExpiredItems : PagingRequest, IReturn<IEnumerable<MediaContainerDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/favorites", "GET", Summary = "Get User Favorites", Notes = "Get User Favorites")]
    public class GetUserFavorites : PagingRequest, IReturn<IEnumerable<FavoriteDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/favorites", "POST", Summary = "Add User Favorites", Notes = "Add User Favorites")]
    public class AddUserFavorite : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "extra_val", Description = "Extra Value", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int extra_val { get; set; }
    }

    [Route("/users/{site_guid}/favorites/{media_id}", "DELETE", Summary = "Remove User Favorite", Notes = "Remove User Favorite")]
    public class RemoveUserFavorite : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "media_id", Description = "Media ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
    }

    [Route("/users/{site_guid}/rules", "GET", Summary = "Get User Group Rules", Notes = "Get User Group Rules")]
    public class GetUserGroupRules : PagingRequest, IReturn<IEnumerable<GroupRuleDTO>>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/users/{site_guid}/rules/{rule_id}", "PUT", Summary = "Update User Group Rule", Notes = "Update User Group Rules")]
    public class SetUserGroupRule : RequestBase, IReturn<bool>
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
    public class CheckGroupRule : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
        [ApiMember(Name = "pin", Description = "PIN", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
    }

    [Route("/users/{user_name}/password/change", "PUT", Summary = "Change user password", Notes = "Change user password")]
    public class ChangeUserPassword : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "old_password", Description = "Old password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string old_password { get; set; }
        [ApiMember(Name = "new_password", Description = "New password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string new_password { get; set; }
    }

    [Route("/users/{user_name}/password/renew", "PUT", Summary = "Renew user password", Notes = "Change user password")]
    public class RenewUserPassword : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/users/{user_name}/activate", "PUT", Notes = "This method activates a user account. User registers with email. An email is sent to the user‟s email address. It includes link and a token. The link sends users email an activation link. When user clicks the link a method is invoked. The method takes the token from the URI and sends event to Tvinci backend.")]
    public class ActivateAccount : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "token", Description = "Account activation token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    [Route("/users/{user_name}/activatebydomainmaster", "PUT", Notes = "This method approves adding a user to a domain by the domain master.")]
    public class ActivateAccountByDomainMaster : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "User Name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "master_user_name", Description = "Master User Name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string master_user_name { get; set; }
        [ApiMember(Name = "token", Description = "User Activation Token for Domain", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    [Route("/users/types", "GET", Summary = "Get Group user types", Notes = "Get Group user types")]
    public class GetGroupUserTypes : PagingRequest, IReturn<IEnumerable<UserTypeDTO>> { }

    [Route("/users/{site_guid}/rules/{rule_id}/renew", "GET", Notes = "This method sends the user a \"renew user PIN\" email with a temporary access token. Example: Adult content on TV, etc. Sends user an email with info.")]
    public class RenewUserPIN : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule identification", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
    }

    [Route("/users/{user_name}/password", "GET", Summary = "Get user password (mail)", Notes = "Get user password")]
    public class SendPasswordMail : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
    }

    //[Route("/users/{site_guid}/types", "PUT", Summary = "Set user type", Notes = "Set user type")]
    //public class SetUserTypeByUserID : RequestBase, IReturn<ResponseStatusDTO>
    //{
    //    [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
    //    public string site_guid { get; set; }
    //    [ApiMember(Name = "user_type", Description = "User Type", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
    //    public int user_type { get; set; }
    //}

    [Route("/users/{site_guid}/lists/{list_id}", "POST", Notes = "This method adds an item to a list.")]
    public class AddItemToList : RequestBase, IReturn<bool>
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
    public class GetItemFromList : PagingRequest, IReturn<IEnumerable<UserItemListDTO>>
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
    public class IsItemExistsInList : PagingRequest, IReturn<IEnumerable<KeyValuePair<string, string>>>
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
    public class RemoveItemFromList : RequestBase, IReturn<bool>
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
    public class UpdateItemInList : RequestBase, IReturn<bool>
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

    [Route("/users/{site_guid}/prepaidbalance", "GET", Summary = "Get User", Notes = "Get User")]
    public class GetPrepaidBalance : RequestBase, IReturn<string[]>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "currency_code", Description = "Currency Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string currency_code { get; set; }
    }

    //Ofir
    [Route("/users/coguid/{co_guid}", "GET", Notes = "This method get's a user's details from a 3rd party CoGuid.")]
    public class GetUserDataByCoGuid : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "co_guid", Description = "3rd party Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
        [ApiMember(Name = "operator_id", Description = "The SSO provider‟s identifier in the Tvinci system", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int operator_id { get; set; }
    }

    [Route("/users/{user_name}/activationmail", "GET", Notes = "This method resend activation mail to a user who has not activated his/her account within a specified amount of time")]
    public class ResendActivationMail : RequestBase, IReturn<UserResponseObjectDTO>
    {
        [ApiMember(Name = "user_name", Description = "User name", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }


    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class UsersService : Service
    {
        public IUsersRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetUserData request)
        {
            var response = _repository.GetUserData(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserData request)
        {
            var response = _repository.SetUserData(request.InitObj, request.site_guid, request.user_basic_data, request.user_dynamic_data);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(SignUp request)
        {
            var response = _repository.SignUp(request.InitObj, request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermitedSubscriptions request)
        {
            var response = _repository.GetUserPermitedSubscriptions(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredSubscriptions request)
        {
            var response = _repository.GetUserExpiredSubscriptions(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermittedItems request)
        {
            var response = _repository.GetUserPermittedItems(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredItems request)
        {
            var response = _repository.GetUserExpiredItems(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserFavorites request)
        {
            var response = _repository.GetUserFavorites(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(AddUserFavorite request)
        {
            var response = _repository.AddUserFavorite(request.InitObj, request.site_guid, request.media_id, request.media_type, request.extra_val);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(RemoveUserFavorite request)
        {
            var response = _repository.RemoveUserFavorite(request.InitObj, request.site_guid, request.media_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserGroupRules request)
        {
            var response = _repository.GetUserGroupRules(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserGroupRule request)
        {
            var response = _repository.SetUserGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin, request.is_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(CheckGroupRule request)
        {
            var response = _repository.CheckGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(ChangeUserPassword request)
        {
            var response = _repository.ChangeUserPassword(request.InitObj, request.user_name, request.old_password, request.new_password);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(RenewUserPassword request)
        {
            var response = _repository.RenewUserPassword(request.InitObj, request.user_name, request.password);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccount request)
        {
            var response = _repository.ActivateAccount(request.InitObj, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccountByDomainMaster request)
        {
            var response = _repository.ActivateAccountByDomainMaster(request.InitObj, request.master_user_name, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGroupUserTypes request)
        {
            var response = _repository.GetGroupUserTypes(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(RenewUserPIN request)
        {
            var response = _repository.RenewUserPIN(request.InitObj, request.site_guid, request.rule_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(SendPasswordMail request)
        {
            var response = _repository.SendPasswordMail(request.InitObj, request.user_name);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        //public HttpResult Put(SetUserTypeByUserID request)
        //{
        //    var response = _repository.SetUserTypeByUserID(request.InitObj, request.site_guid, request.user_type);

        //    if (response == null)
        //    {
        //        return new HttpResult(HttpStatusCode.InternalServerError);
        //    }

        //    var responseDTO = response.ToDto();

        //    return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        //}

        //Siteguid is irrelevant because it comes from initibj?

        public HttpResult Post(AddItemToList request)
        {
            var response = _repository.AddItemToList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetItemFromList request)
        {
            var response = _repository.GetItemFromList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemExistsInList request)
        {
            var response = _repository.IsItemExistsInList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Delete(RemoveItemFromList request)
        {
            var response = _repository.RemoveItemFromList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(UpdateItemInList request)
        {
            var response = _repository.UpdateItemInList(request.InitObj, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetPrepaidBalance request)
        {
            var response = _repository.GetPrepaidBalance(request.InitObj, request.currency_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserDataByCoGuid request)
        {
            var response = _repository.GetUserDataByCoGuid(request.InitObj, request.co_guid, request.operator_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(ResendActivationMail request)
        {
            var response = _repository.ResendActivationMail(request.InitObj, request.user_name, request.password);

            return new HttpResult(response, HttpStatusCode.OK);
        }
    }
}
