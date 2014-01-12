using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

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
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class ConditionalAccessService : Service
    {
        public IConditionalAccessRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Put(ActivateCampaignRequest request)
        {
            var response = _repository.ActivateCampaign(request.InitObj, request.campaign_id, request.hash_code, request.media_id, request.media_link, request.sender_email, request.sender_name,
                                                        request.status, request.voucher_receipents);

            return new HttpResult(response, HttpStatusCode.OK);
        }
    }
}
