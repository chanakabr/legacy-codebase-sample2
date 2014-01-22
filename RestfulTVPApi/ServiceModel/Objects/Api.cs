using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/countries", "GET", Notes = "This method returns a list of all countries, an ID and a symbol for each. Used to enable a user to select his/her country. Example: During user registration, the method returns an array of country codes, #ID, country name.")]
    public class GetCountriesListRequest : RequestBase, IReturn<IEnumerable<Country>> { }

    [Route("/coupons/{coupon_code}", "GET", Notes = "This method returns the status of a specific coupon")]
    public class GetCouponStatusRequest : RequestBase, IReturn<CouponData>
    {
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    [Route("/ppv_modules/{ppv_code}", "GET", Notes = "This method retrieves all information regarding a specific PPV module")]
    public class GetPPVModuleDataRequest : RequestBase, IReturn<PPVModule>
    {
        [ApiMember(Name = "ppv_code", Description = "PPV Code", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int ppv_code { get; set; }
    }

    [Route("/rpc/get_country_by_ip", "GET", Notes = "This method receives an IP address and returns the corresponding country.")]
    public class GetIPToCountryRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "ip", Description = "PPV Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string ip { get; set; }
    }

    [Route("/rpc/get_secured_site_guid_from", "GET", Notes = "This method returns the secured site Guid. PlayReadyTM encrypted media assets require a secured site Guid. The method receives the secured site Guid, attaches the GroupID and passes them to the player.")]
    public class GetSecuredSiteGuidRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    [Route("/rpc/get_site_guid_from_secured", "GET", Notes = "This method accepts the secured SiteGuid and returns the unsecured SiteGuid. It is the opposite of GetSecuredSiteGuid.")]
    public class GetSiteGuidFromSecuredRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "encrypted_site_guid", Description = "Encrypted Site Guid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string encrypted_site_guid { get; set; }
    }

    [Route("/rpc/get_user_data_by_co_guid", "GET", Notes = "This method get's a user's details from a 3rd party CoGuid.")]
    public class GetUserDataByCoGuidRequest : RequestBase, IReturn<UserResponseObject>
    {
        [ApiMember(Name = "co_guid", Description = "Co Guid", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
        [ApiMember(Name = "operator_id", Description = "Operator ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int operator_id { get; set; }
    }

    [Route("/rpc/get_client_merchant_signature", "GET", Notes = "This method returns a hashed string from Adyen for validation purposes")]
    public class GetClientMerchantSigRequest : RequestBase, IReturn<IEnumerable<AdyenBillingDetail>>
    {
        [ApiMember(Name = "paramaters", Description = "The set of parameters that comprise the hashed string", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string paramaters { get; set; }
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
        public CampaignActionResult status { get; set; }
        [ApiMember(Name = "voucher_receipents", Description = "Voucher Receipents", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public VoucherReceipentInfo[] voucher_receipents { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
    }

    #endregion

    #region POST
    #endregion

    #region DELETE
    #endregion
}