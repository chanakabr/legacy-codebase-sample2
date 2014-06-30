using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects.Responses;


namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/collections/{collection_id}", "GET", Summary = "Gets collection data", Notes = "Get domain info")]
    public class GetCollectionDataRequest : RequestBase, IReturn<Collection>
    {
        [ApiMember(Name = "collection_id", Description = "Collection Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string collection_id { get; set; }
        [ApiMember(Name = "get_also_inactive", Description = "Get also inactive collections data", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool get_also_inactive { get; set; }
    }

    [Route("/collections/{collection_ids}/prices/", "GET", Summary = "Gets collection prices", Notes = "")]
    public class GetCollectionsPricesRequest : RequestBase, IReturn<List<CollectionPricesContainer>>
    {
        [ApiMember(Name = "collection_ids", Description = "Collection Ids", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] collection_ids { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "country_code", Description = "Country Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string country_code { get; set; }
        [ApiMember(Name = "language_code", Description = "Language Code", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string language_code { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    [Route("/collections/{collection_ids}/prices/{coupon_code}", "GET", Summary = "Get collections Prices With Coupon", Notes = "")]
    public class GetCollectionsPricesWithCouponRequest : RequestBase, IReturn<List<CollectionPricesContainer>>
    {
        [ApiMember(Name = "collection_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] collection_ids { get; set; }
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
}