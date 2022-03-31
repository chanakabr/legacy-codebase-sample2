using System.Threading.Tasks;
using ApiObjects.Pricing;

namespace Core.Pricing.Services
{
    public interface IProgramAssetGroupOfferCrudMessageService
    {
        Task PublishCreateEventAsync(int groupId, ProgramAssetGroupOffer programAssetGroupOffer);
        Task PublishUpdateEventAsync(int groupId, ProgramAssetGroupOffer programAssetGroupOffer);
        Task PublishDeleteEventAsync(int groupId, long pagoId);
    }
}