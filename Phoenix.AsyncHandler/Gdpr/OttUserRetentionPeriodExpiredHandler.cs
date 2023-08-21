using System.Threading.Tasks;
using ApiLogic.Catalog;
using ApiObjects;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.Generated.Api.Events.Logical.Gdpr.OttUserRetentionPeriodExpired;

namespace Phoenix.AsyncHandler.Gdpr
{
    public class OttUserRetentionPeriodExpiredHandler : IKafkaMessageHandler<OttUserRetentionPeriodExpired>
    {
        private readonly IUserWatchHistoryManager _userWatchHistoryManager;

        public OttUserRetentionPeriodExpiredHandler(IUserWatchHistoryManager userWatchHistoryManager)
        {
            _userWatchHistoryManager = userWatchHistoryManager;
        }

        public async Task<HandleResult> Handle(ConsumeResult<string, OttUserRetentionPeriodExpired> consumeResult)
        {
            await _userWatchHistoryManager.CleanByRetention(consumeResult.Result.Message.Value.UserId);

            return new HandleResult();
        }
    }
}