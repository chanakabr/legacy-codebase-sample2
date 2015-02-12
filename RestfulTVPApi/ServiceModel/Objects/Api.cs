using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/countries", "GET", Notes = "This method returns a list of all countries, an ID and a symbol for each. Used to enable a user to select his/her country. Example: During user registration, the method returns an array of country codes, #ID, country name.")]
    public class GetCountriesListRequest : RequestBase, IReturn<List<Country>> { }

    [Route("/countries/{ip}", "GET", Notes = "This method receives an IP address and returns the corresponding country.")]
    public class GetIPToCountryRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "ip", Description = "IP Adress", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string ip { get; set; }
    }

    [Route("/coupons/{coupon_code}", "GET", Notes = "This method returns the status of a specific coupon")]
    public class GetCouponStatusRequest : RequestBase, IReturn<CouponData>
    {
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    [Route("/social_platforms/facebook/config", "GET", Notes = "This method returns a specific page from the site map.")]
    public class FBConfigRequest : RequestBase, IReturn<FBConnectConfig> { }

    [Route("/social_platforms/facebook/user", "POST", Notes = "This method verifies existence of user in Facebook and in Tvinci then returns user’s Facebook user-data. This follows receipt of a token from Facebook.")]
    public class GetFBUserDataRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    [Route("/ppv_modules/{ppv_code}", "GET", Notes = "This method retrieves all information regarding a specific PPV module")]
    public class GetPPVModuleDataRequest : RequestBase, IReturn<PPVModule>
    {
        [ApiMember(Name = "ppv_code", Description = "PPV Code", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int ppv_code { get; set; }
    }

    [Route("/rpc/secured_site_guid", "GET", Notes = "This method returns the secured site Guid. PlayReadyTM encrypted media assets require a secured site Guid. The method receives the secured site Guid, attaches the GroupID and passes them to the player.")]
    public class GetSecuredSiteGuidRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }
    
    [Route("/rpc/site_guid_from_secured", "GET", Notes = "This method accepts the secured SiteGuid and returns the unsecured SiteGuid. It is the opposite of GetSecuredSiteGuid.")]
    public class GetSiteGuidFromSecuredRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "encrypted_site_guid", Description = "Encrypted Site Guid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_site_guid { get; set; }
    }

    //Ofir - maybe combine with GetUserData?
    [Route("/rpc/user_data_by_co_guid", "GET", Notes = "This method get's a user's details from a 3rd party CoGuid.")]
    public class GetUserDataByCoGuidRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "co_guid", Description = "Co Guid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
        [ApiMember(Name = "operator_id", Description = "Operator ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int operator_id { get; set; }
    }

    [Route("/rpc/client_merchant_signature", "GET", Notes = "This method returns a hashed string from Adyen for validation purposes")]
    public class GetClientMerchantSigRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "paramaters", Description = "The set of parameters that comprise the hashed string", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string paramaters { get; set; }
    }

    [Route("/rpc/get_domain_by_co_guid", "GET", Summary = "Get a specific domain by a coguid", Notes = "Get a domain by coguid")]
    public class GetDomainByCoGuidRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "co_guid", Description = "CoGuid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
    }

    [Route("/rpc/get_domain_ids_by_operator_co_guid", "GET", Summary = "Returns all domain IDs belonging to a specific Operator by its co guid", Notes = "Returns all domain IDs belonging to a specific Operator by its co guid")]
    public class GetDomainIDsByOperatorCoGuidRequest : RequestBase, IReturn<List<int>>
    {
        [ApiMember(Name = "operator_co_guid", Description = "The operator coguid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string operator_co_guid { get; set; }
    }

    [Route("/rpc/get_domain_id_by_co_guid", "GET", Summary = "Returns a domain ID using a 3rd party Co-GUID", Notes = "used when a device has an existing 3rd party association and identification number outside of the Tvinci system")]
    public class GetDomainIDByCoGuidRequest : RequestBase, IReturn<int>
    {
        [ApiMember(Name = "co_guid", Description = "Domain Master Co-GUID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
    }

    #endregion

    #region PUT

    [Route("/campaigns/{campaign_id}/activate", "PUT", Notes = "This method initiates a campaign")]
    public class ActivateCampaignRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "campaign_id", Description = "Campaign ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int campaign_id { get; set; }
        [ApiMember(Name = "hash_code", Description = "Hash Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string hash_code { get; set; }
        [ApiMember(Name = "media_id", Description = "Tvinci's Media ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_id { get; set; }
        [ApiMember(Name = "media_link", Description = "Media link", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string media_link { get; set; }
        [ApiMember(Name = "sender_email", Description = "Sender Email", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string sender_email { get; set; }
        [ApiMember(Name = "sender_name", Description = "Sender Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string sender_name { get; set; }
        [ApiMember(Name = "status", Description = "Order Direction", ParameterType = "body", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("status", typeof(CampaignActionResult))]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status { get; set; }
        [ApiMember(Name = "voucher_receipents", Description = "Voucher Receipents", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucher_receipents { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/social_platforms/facebook/merge", "PUT", Notes = "This method merges a Facebook user with an existing regular Tvinci user. Used when Facebook user has an email address corresponding tothat of a registered Tvinci user.")]
    public class FBUserMergeRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
        [ApiMember(Name = "facebook_id", Description = "Facebook ID", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string facebook_id { get; set; }
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/social_platforms/facebook/unmerge", "PUT", Notes = "")]
    public class FBUserUnMergeRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }        
        [ApiMember(Name = "user_name", Description = "Username", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_name { get; set; }
        [ApiMember(Name = "password", Description = "Password", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string password { get; set; }
    }

    #endregion

    #region POST

    [Route("/social_platforms/facebook/register", "POST", Notes = "This method registers a user using his/her Facebook credentials (when a Facebook user does not exist in the Tvinci system).")]
    public class FBUserRegisterRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
        [ApiMember(Name = "create_new_domain", Description = "Create New Domain?", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool create_new_domain { get; set; }
        [ApiMember(Name = "get_newsletter", Description = "Get Newsletter?", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool get_newsletter { get; set; }
    }

    //Ofir - response object needs to be changed from struct
    [Route("/rpc/register_device_by_pin", "POST", Summary = "registers a new device to a domain from an input PIN code", Notes = "registers a new device to a domain from an input PIN code")]
    public class RegisterDeviceByPINRequest : RequestBase, IReturn<DeviceRegistration>
    {
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "pin", Description = "Pin code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
        [ApiMember(Name = "domain id", Description = "The user's domain", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    #endregion

    #region DELETE
    #endregion
}