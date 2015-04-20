using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using RestfulTVPApi.Objects.Responses;


namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/subscriptions/{subscription_ids}", "GET", Notes = "This method returns an array containing the data of subscription IDs (array) posted to the system")]
    public class GetSubscriptionDataRequest : RequestBase, IReturn<List<Subscription>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] subscription_ids { get; set; }
    }

    [Route("/subscriptions/{subscription_ids}/prices", "GET", Notes = "This method returns an array of the subscription data prices")]
    public class GetSubscriptionDataPricesRequest : RequestBase, IReturn<List<SubscriptionPrice>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] subscription_ids { get; set; }
    }

    [Route("/subscriptions/{subscription_id}/product_code", "GET", Notes = "This method returns the In_App product code corresponding to a specific Tvinci subscription ID number. This is because the Tvinci code for a particular product is different than the code used by different customer stores")]
    public class GetSubscriptionProductCodeRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_id { get; set; }
    }

    //Problematic routing - same as GetSubscriptionDataPricesRequest
    [Route("/subscriptions/{subscription_ids}/prices/{coupon_code}", "GET", Summary = "Get Subscription Prices With Coupon", Notes = "Get Subscription Prices With Coupon")]
    public class GetSubscriptionsPricesWithCouponRequest : RequestBase, IReturn<List<SubscriptionsPricesContainer>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] subscription_ids { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "site_guid", Description = "User ID", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }                
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    #endregion

    #region PUT
    #endregion

    #region POST

    [Route("/subscriptions/{subscription_id}/medias", "POST", Notes = "This method returns an array of media assets belonging to a specific package (subscription)")]
    public class GetMediasInPackageRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    #endregion

    #region DELETE
    #endregion
    
}