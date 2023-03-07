using System.Linq;
using ApiObjects;
using ApiObjects.Recordings;
using Core.Recordings;
using DAL;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Extensions.RecordingFailed;
using TVinciShared;

namespace Phoenix.AsyncHandler.Recording
{
    public class RecordingFailedHandler : IHandler<RecordingFailed>
    {
        private readonly ILogger<RecordingFailedHandler> _logger;

        public RecordingFailedHandler(ILogger<RecordingFailedHandler> logger)
        {
            _logger = logger;
        }

        public HandleResult Handle(ConsumeResult<string, RecordingFailed> consumeResult)
        {
            if (!consumeResult.Result.Message.Value.PartnerId.HasValue ||
                consumeResult.Result.Message.Value.ExternalRecordingId.IsNullOrEmpty())
            {
                _logger.LogError("Wrong event body - must have Partner id and External Recording Id");
                return Result.Ok;
            }

            var partnerId = (int)consumeResult.Result.Message.Value.PartnerId.Value;
            var externalRecordingId = consumeResult.Result.Message.Value.ExternalRecordingId;

            var recording = PaddedRecordingsManager.Instance.GetRecordingByExternalId(partnerId, externalRecordingId);
            if (recording == null || recording.Id == 0)
            {
                _logger.LogError($"Recording: {externalRecordingId} wasn't found");
                return Result.Ok;
            }

            if (recording.Status == RecordingInternalStatus.Failed.ToString())
            {
                _logger.LogWarning(
                    $"RecordingId: {recording.Id} has the correct status: {recording.Status}, no need to update");
                return Result.Ok;
            }

            UpdateFailedRecordingStatus(partnerId, recording);
            return Result.Ok;
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