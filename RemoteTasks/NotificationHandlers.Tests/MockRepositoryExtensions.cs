using System.Linq;
using System.Threading.Tasks;
using ApiObjects.Cloudfront;
using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Notification;
using IotGrpcClientWrapper;
using Moq;
using phoenix;

namespace NotificationHandlers.Tests
{
    public static class MockRepositoryExtensions
    {
        public static INotificationCache NotificationCache(this MockRepository mockRepository, int groupId, NotificationPartnerSettings settings)
        {
            var mock = mockRepository.Create<INotificationCache>();
            mock.Setup(m => m.GetPartnerNotificationSettings(groupId)).Returns(new NotificationPartnerSettingsResponse
                { settings = settings, Status = new Status(eResponseStatus.OK) });
            return mock.Object;
        }

        public static ICloudfrontInvalidator CloudfrontInvalidator(this MockRepository mockRepository, int groupId, string[] paths, bool returnSuccess = true)
        {
            var mock = mockRepository.Create<ICloudfrontInvalidator>();
            mock
                .Setup(x => x.InvalidateAndWaitAsync(groupId, paths, It.IsAny<WaitConfig>()))
                .ReturnsAsync( (returnSuccess, null) );
            return mock.Object;
        }

        public static IIotClient IotClient(this MockRepository mockRepository, int groupId, string message, EventNotificationType eventType, params long[] regionIds)
        {
            var regions = regionIds.Select(_ => (int)_).ToList();
            var mock = mockRepository.Create<IIotClient>();
            mock
                .Setup(x => x.SendNotificationAsync(groupId,
                    It.Is<string>(m => m.StartsWith(message)),
                    eventType,
                    regions))
                .Returns(Task.CompletedTask);

            return mock.Object;
        }
    }
}