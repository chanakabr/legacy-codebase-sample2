using ApiObjects.Response;
using Core.Recordings;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.HouseholdRecordingMigrationStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.Recordings;
using CachingHelpers;
using Core.Api;
using Core.Catalog.Response;
using Core.GroupManagers;
using DAL;
using Phoenix.Generated.Api.Events.Logical.Recordings.Partnermigrations.HouseholdRecordingDelete;
using SchemaRegistryEvents.Catalog;

namespace Phoenix.AsyncHandler.Recording
{
    public class HouseholdRecordingDeleteHandler : CrudHandler<HouseholdRecordingDelete>
    {
        private readonly ILogger<HouseholdRecordingDeleteHandler> _logger;
        private readonly IHouseholdRecordingMigrationPublisher _publisher;
        private static readonly string NotAllowedError = "Import of recordings not allowed for this account";

        public HouseholdRecordingDeleteHandler(ILogger<HouseholdRecordingDeleteHandler> logger, IHouseholdRecordingMigrationPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        protected override long GetOperation(HouseholdRecordingDelete value) => CrudOperationType.DELETE_OPERATION;

        protected override HandleResult Delete(ConsumeResult<string, HouseholdRecordingDelete> consumeResult)
        {
            _logger.LogDebug($"**Handler:[{GetType().Name}], Event:[{consumeResult?.Value}], Action: Delete");

            var householdRecordingEvent = consumeResult.GetValue();
            
            if (!ValidateRecordingDeleteEvent(householdRecordingEvent, out var householdRecording))
            {
                return Result.Ok;
            }

            var groupId = (int)(householdRecordingEvent.PartnerId ?? 0);
            var recordingId = householdRecordingEvent.RecordingId;
            var externalUserId = UsersDal.GetExternalIdByUserId(groupId, householdRecording.UserId);
            var externalId = GetExternalIdByAssetId(groupId, householdRecording.EpgId); //must be before removing from index
            var deleteHhRecording = PaddedRecordingsManager.Instance.DeleteHouseholdRecordings(groupId, recordingId, householdRecording.UserId.ToString(), true);
            
            if (!deleteHhRecording.IsOkStatusCode())
            {
                LogWarning(householdRecordingEvent, $"Failed to delete household recording id: {recordingId}, error: {deleteHhRecording.Status.Message}");
                PublishError(householdRecordingEvent, (eResponseStatus)deleteHhRecording.Status.Code, deleteHhRecording.Status.Message, externalUserId, externalId);
            }
            else
            {
                _logger.LogDebug($"***Handler:[{GetType().Name}], Event:[{consumeResult?.Value}], successfully deleted recording: {recordingId}");
                PublishSuccess(householdRecordingEvent, recordingId, externalUserId, externalId);
            }

            return Result.Ok;
        }

        protected override HandleResult Update(ConsumeResult<string, HouseholdRecordingDelete> consumeResult) => Result.Ok;

        protected override HandleResult Create(ConsumeResult<string, HouseholdRecordingDelete> consumeResult) => Result.Ok;

        private bool ValidateRecordingDeleteEvent(HouseholdRecordingDelete householdRecording, out HouseholdRecording hhRecording)
        {
            hhRecording = null;
            
            if (!householdRecording.PartnerId.HasValue || 
                householdRecording.PartnerId.Value < 1 || 
                householdRecording.RecordingId < 1)
            {
                var allMandatoryField = "PartnerId, RecordingId";
                LogWarning(householdRecording, $"Wrong event body - must have {allMandatoryField}");
                PublishError(householdRecording, eResponseStatus.MandatoryField, $"{allMandatoryField} are missing");
                return false;
            }

            var groupId = (int)householdRecording.PartnerId.Value;
            var recordingId = householdRecording.RecordingId;
            
            if (!GroupSettingsManager.Instance.IsOpc(groupId))
            {
                LogWarning(householdRecording, "Account Is Not OPC Supported");
                PublishError(householdRecording, eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return false;
            }
            
            var partnerSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (partnerSettings == null || !partnerSettings.IsDefault.HasValue || partnerSettings.IsDefault.Value)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings does not exist");
                PublishError(householdRecording, eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, "Time Shifted Tv Partner Settings Not Found");
                return false;
            }
            
            if (partnerSettings.PersonalizedRecordingEnable == false)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings PersonalizedRecordingEnable is false - Import of recordings not allowed");
                PublishError(householdRecording, eResponseStatus.PersonalizedRecordingDisabled, NotAllowedError);
                return false;
            }
            
            if (partnerSettings.IsPrivateCopyEnabled == true)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings IsPrivateCopyEnabled is true - Import of recordings not allowed");
                PublishError(householdRecording, eResponseStatus.PersonalizedRecordingDisabled, NotAllowedError);
                return false;
            }
            
            if (partnerSettings.IsCdvrEnabled == null || !partnerSettings.IsCdvrEnabled.Value)
            {
                var msg = "Account Cdvr Not Enabled";
                LogWarning(householdRecording, msg);
                PublishError(householdRecording, eResponseStatus.AccountCdvrNotEnabled, msg);
                return false;
            }
            
            // var adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
            if (!partnerSettings.AdapterId.HasValue || partnerSettings.AdapterId < 1)
            {
                var msg = "Cdvr Adapter Identifier Required";
                LogWarning(householdRecording, msg);
                PublishError(householdRecording, eResponseStatus.AdapterIdentifierRequired, msg);
                return false;
            }
            
            var adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(groupId, partnerSettings.AdapterId.Value);
            if (adapter == null || string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                var msg = "Cdvr Adapter Not Exists";
                LogWarning(householdRecording, msg);
                PublishError(householdRecording, eResponseStatus.AdapterNotExists, msg);
                return false;
            }
            
            //returns only if status is OK
            hhRecording =
                PaddedRecordingsManager.Instance.GetHouseholdRecordingByRecordingId(groupId, recordingId, null);

            if (hhRecording == null || hhRecording.Id < 1)
            {
                LogWarning(householdRecording, "Failed to get recording");
                PublishError(householdRecording, eResponseStatus.RecordingNotFound, $"Recording {recordingId} not found");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get ExternalId By AssetId with ksql
        /// </summary>
        /// <returns></returns>
        private string GetExternalIdByAssetId(int groupId, long epgId)
        {
            if (epgId == 0) return "0";
            
            var ksql = $"(and asset_type='epg' epg_id = '{epgId}')";
            var extraFields = new List<string> { "externalid", "external_id" };
            var result = api.SearchAssets(groupId, ksql, 0, 0, true, 
                0, false, string.Empty, string.Empty, string.Empty, 
                0, 0, true, true, extraFields);
            
            if (result.Length == 0)
                return string.Empty;

            var externalId = (result.FirstOrDefault() as ExtendedSearchResult)?.ExtraFields?
                .FirstOrDefault(ef => ef.key == "externalid" || ef.key == "external_id")?.value ?? "";

            return externalId;
        }
        
        private void PublishError(HouseholdRecordingDelete householdRecording, eResponseStatus responseStatus, string message, 
            string externalUserId = "", string programAssetExternalId = "")
        {
            _logger.LogWarning($"Published at: {DateTime.UtcNow}, recordingId: {householdRecording.RecordingId}");

            var status = new HouseholdRecordingMigrationStatus
            {
                PartnerId = householdRecording.PartnerId,
                Message = message,
                Code = PaddedRecordingsManager.Instance.MapResponseStatusToCode(responseStatus),
                OttUserExternalId = externalUserId,
                ProgramAssetExternalId = programAssetExternalId,
                RecordingId = householdRecording.RecordingId,
                RequestType = RequestType.Delete
            };
            _publisher.Publish(status);
        }
        
        private void PublishSuccess(HouseholdRecordingDelete householdRecording, long recordingId, string externalUserId, string programAssetExternalId)
        {
            _logger.LogWarning($"Published at: {DateTime.UtcNow}, recordingId: {householdRecording.RecordingId}");

            var status = new HouseholdRecordingMigrationStatus
            {
                PartnerId = householdRecording.PartnerId,
                Message = "Success",
                Code = Code.The0,
                OttUserExternalId = externalUserId,
                ProgramAssetExternalId = programAssetExternalId,
                RecordingId = recordingId,
                RequestType = RequestType.Delete
            };
            _publisher.Publish(status);
        }

        private void LogWarning(HouseholdRecordingDelete householdRecording, string message)
        {
            _logger.LogWarning($"{message} - PartnerId [{householdRecording.PartnerId}], recordingId [{householdRecording.RecordingId}]].");
        }
    }
}
