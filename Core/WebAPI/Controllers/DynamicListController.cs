using ApiObjects;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [Service("dynamicList")]
    [AddAction]
    [UpdateAction]
    [DeleteAction]
    [ListAction(IsFilterOptional = false, IsPagerOptional = true)]
    public class DynamicListController : KalturaCrudController<KalturaDynamicList, KalturaDynamicListListResponse, DynamicList, long, KalturaDynamicListFilter>
    {
    }
}