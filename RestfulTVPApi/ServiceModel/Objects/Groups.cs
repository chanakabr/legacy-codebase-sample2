using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace RestfulTVPApi.ServiceModel
{

    #region GET

    [Route("/groups/operators", "GET", Notes = "This method returns the list of group operators.")]
    public class GetGroupOperatorsRequest : RequestBase, IReturn<IEnumerable<GroupOperator>>
    {
        [ApiMember(Name = "scope", Description = "Scope", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string scope { get; set; }
    }

    [Route("/groups/rules", "GET", Notes = "This method returns all of the group’s rules. For example: Geo block, parental & purchase.")]
    public class GetGroupRulesRequest : RequestBase, IReturn<IEnumerable<GroupRule>> { }

    [Route("/groups/facebook/config", "GET", Notes = "This method returns a specific page from the site map.")]
    public class FBConfigRequest : RequestBase, IReturn<FBConnectConfig> { }

    [Route("/groups/facebook/user/{token}", "GET", Notes = "This method verifies existence of user in Facebook and in Tvinci then returns user’s Facebook user-data. This follows receipt of a token from Facebook.")]
    public class GetFBUserDataRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
    }

    #endregion

    #region PUT

    [Route("/groups/facebook/merge", "PUT", Notes = "This method merges a Facebook user with an existing regular Tvinci user. Used when Facebook user has an email address corresponding tothat of a registered Tvinci user.")]
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

    [Route("/groups/facebook/register", "POST", Notes = "This method registers a user using his/her Facebook credentials (when a Facebook user does not exist in the Tvinci system).")]
    public class FBUserRegisterRequest : RequestBase, IReturn<FacebookResponseObject>
    {
        [ApiMember(Name = "token", Description = "Token", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string token { get; set; }
        [ApiMember(Name = "create_new_domain", Description = "Create New Domain?", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool create_new_domain { get; set; }
        [ApiMember(Name = "get_newsletter", Description = "Get Newsletter?", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool get_newsletter { get; set; }
    }

    #endregion

    #region POST

    

    #endregion

    #region DELETE
    #endregion

}