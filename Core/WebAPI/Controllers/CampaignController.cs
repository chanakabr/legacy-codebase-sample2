using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    [Service("campaign")]
    [AddAction(ClientThrows = new eResponseStatus[] {  })]
    [UpdateAction(ClientThrows = new eResponseStatus[] {  })]
    [DeleteAction(ClientThrows = new eResponseStatus[] {  })]
    [ListAction(ClientThrows = new eResponseStatus[] {  })]
    public class CampaignController : KalturaCrudController<KalturaCampaign, KalturaCampaignListResponse, Campaign, long, KalturaCampaignFilter>
    {
    }
}