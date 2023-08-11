using System.Linq;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Recordings;
using Core.Recordings;
using DAL;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Extensions.RecordingFailed;
using TVinciShared;

namespace Phoenix.AsyncHandler.Recording
{
    public class RecordingFailedHandler : IKafkaMessageHandler<RecordingFailed>
    {
        private readonly ILogger<RecordingFailedHandler> _logger;

        public RecordingFailedHandler(ILogger<RecordingFailedHandler> logger)
        {
            _logger = logger;
        }

        public Task<HandleResult> Handle(ConsumeResult<string, RecordingFailed> consumeResult)
        {
            if (!consumeResult.Result.Message.Value.PartnerId.HasValue ||
                consumeResult.Result.Message.Value.ExternalRecordingId.IsNullOrEmpty())
            {
                _logger.LogError("Wrong event body - must have Partner id and External Recording Id");
                return Task.FromResult(Result.Ok);
            }

            var partnerId = (int)consumeResult.Result.Message.Value.PartnerId.Value;
            var externalRecordingId = consumeResult.Result.Message.Value.ExternalRecordingId;

            var recording = PaddedRecordingsManager.Instance.GetRecordingByExternalId(partnerId, externalRecordingId);
            if (recording == null || recording.Id == 0)
            {
                _logger.LogError($"Recording: {externalRecordingId} wasn't found");
                return Task.FromResult(Result.Ok);
            }

            if (recording.Status == RecordingInternalStatus.Failed.ToString())
            {
                _logger.LogWarning(
                    $"RecordingId: {recording.Id} has the correct status: {recording.Status}, no need to update");
                return Task.FromResult(Result.Ok);
            }

            UpdateFailedRecordingStatus(partnerId, recording);
            return Task.FromResult(Result.Ok);
        }

        private void UpdateFailedRecordingStatus(int partnerId, TimeBasedRecording recording)
        {
            if (PaddedRecordingsManager.Instance.UpdateRecordingStatus(partnerId, recording.Id, RecordingInternalStatus.Failed))
            {
                PaddedRecordingsManager.Instance.HandleFailedRecording(partnerId, recording);
            }
        }
    }
}
