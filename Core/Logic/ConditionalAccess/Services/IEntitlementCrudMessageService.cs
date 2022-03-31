using System.Collections.Generic;
using System.Threading.Tasks;
using Core.ConditionalAccess.Modules;

namespace ApiLogic.ConditionalAccess.Services
{
    public interface IEntitlementCrudMessageService
    {
        Task PublishCreateEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase);
        Task PublishUpdateEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase);
        Task PublishDeleteEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase);
    }
}