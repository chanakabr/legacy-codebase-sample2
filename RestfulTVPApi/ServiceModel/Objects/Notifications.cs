using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace RestfulTVPApi.ServiceModel
{

    #region GET


    #endregion

    #region PUT


    #endregion

    #region POST

    [Route("/notifications/subscribe", "POST", Notes = "This method sets notification view status.")]
    public class SubscribeByTagRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "tags", Description = "Tags", ParameterType = "body", DataType = SwaggerType.Array, IsRequired = true)]
        public List<TVPApi.TagMetaPairArray> tags { get; set; }
    }

    [Route("/notifications/unsubscribe", "POST", Notes = "This method in the opposite of FollowByTag. It unsubscibes the user from receiving notification using meta tags.")]
    public class UnSubscribeByTagRequest : SubscribeByTagRequest { }

    #endregion

    #region DELETE


    #endregion

}