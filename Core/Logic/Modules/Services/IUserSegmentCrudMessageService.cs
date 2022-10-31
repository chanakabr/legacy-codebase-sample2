using System.Threading.Tasks;
using ApiObjects.Segmentation;
using UserSegment = ApiObjects.Segmentation.UserSegment;

namespace ApiLogic.Modules.Services
{
    public interface IUserSegmentCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, UserSegment userSegment);
        Task PublishDeleteEventAsync(long groupId, UserSegment userSegment);
    }
}