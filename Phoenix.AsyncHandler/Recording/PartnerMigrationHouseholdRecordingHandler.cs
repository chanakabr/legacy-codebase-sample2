using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Recordings;
using DAL;
using IngestHandler;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Crud.PartnerMigrationHouseholdRecording;
using Phoenix.Generated.Api.Events.Logical.HouseholdRecordingMigrationStatus;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects.Epg;
using ElasticSearch.Utilities;
using EpgBL;
using Tvinci.Core.DAL;
using TVinciShared;
using Mapper = ApiLogic.ConditionalAccess.Recordings.PartnerMigrationHouseholdRecordingCrudEventMapper;

namespace Phoenix.AsyncHandler.Recording
{
    public class PartnerMigrationHouseholdRecordingHandler : CrudHandler<PartnerMigrationHouseholdRecording>
    {
        private readonly ILogger<PartnerMigrationHouseholdRecordingHandler> _logger;
        private readonly IHouseholdRecordingMigrationPublisher _publisher;

        public PartnerMigrationHouseholdRecordingHandler(ILogger<PartnerMigrationHouseholdRecordingHandler> logger, IHouseholdRecordingMigrationPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        protected override long GetOperation(PartnerMigrationHouseholdRecording value) => value.Operation;

        protected override HandleResult Create(ConsumeResult<string, PartnerMigrationHouseholdRecording> consumeResult)
        {
            _logger.LogDebug($"**Handler:[{GetType().Name}], Event:[{consumeResult?.Value}], Action: Create");

            var householdRecording = consumeResult.GetValue();
            if (!ValidateAndGetProgramAsset(householdRecording, out long userId, out long domainId, out bool isPadded, out EpgAsset epgAsset))
            {
                return Result.Ok;
            }
            var groupId = (int)householdRecording.PartnerId;

            // add recording (record)
            if (isPadded)
            {
                AddPaddedRecording(groupId, householdRecording, userId, domainId, epgAsset);
            }
            else
            {
                AddImmediateRecording(groupId, householdRecording, userId, domainId, epgAsset);
            }

            return Result.Ok;
        }

        protected override HandleResult Update(ConsumeResult<string, PartnerMigrationHouseholdRecording> consumeResult) => Result.Ok;

        protected override HandleResult Delete(ConsumeResult<string, PartnerMigrationHouseholdRecording> consumeResult) => Result.Ok;

        private bool ValidateAndGetProgramAsset(PartnerMigrationHouseholdRecording householdRecording, out long userId, out long domainId, out bool isPadded, out EpgAsset epgAsset)
        {   
            userId = 0;
            domainId = 0;
            isPadded = true;
            epgAsset = null;

            if (householdRecording.PartnerId < 1 ||
                householdRecording.OttUserExternalId.IsNullOrEmpty() ||
                householdRecording.ProgramAssetExternalId.IsNullOrEmpty())
            {
                var allMandatoryField = "PartnerId, OttUserExternalId, ProgramAssetExternalId";
                LogWarning(householdRecording, $"Wrong event body - must have {allMandatoryField}");
                PublishError(householdRecording, eResponseStatus.MandatoryField, $"{allMandatoryField} are missing");
                return false;
            }

            var groupId = (int)householdRecording.PartnerId;

            CatalogGroupCache cache;
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out cache))
            {
                LogWarning(householdRecording, "Failed to get catalogGroupCache");
                PublishError(householdRecording, eResponseStatus.Error, "Failed while trying to get catalogGroupCache");
                return false;
            }
            
            var partnerSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (partnerSettings == null)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings does not exist");
                PublishError(householdRecording, eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, "Time Shifted Tv Partner Settings Not Found");
                return false;
            }

            if (partnerSettings.PersonalizedRecordingEnable == false)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings PersonalizedRecordingEnable is false - Import of recordings not allowed");
                PublishError(householdRecording, eResponseStatus.PersonalizedRecordingDisabled, "Import of recordings not allowed for this account");
                return false;
            }

            if (partnerSettings.IsPrivateCopyEnabled == true)
            {
                LogWarning(householdRecording, "TimeShiftedTvPartnerSettings IsPrivateCopyEnabled is true - Import of recordings not allowed");
                PublishError(householdRecording, eResponseStatus.PersonalizedRecordingDisabled, "Import of recordings not allowed for this account");
                return false;
            }

            userId = UsersDal.GetUserIDByExternalId(groupId, householdRecording.OttUserExternalId);
            if (userId < 1)
            {
                LogWarning(householdRecording, "User does not exist");
                PublishError(householdRecording, eResponseStatus.UserDoesNotExist, "User does not exist");
                return false;
            }

            domainId = UsersDal.GetUserDomainID(userId.ToString());
            if (domainId < 1)
            {
                LogWarning(householdRecording, "User does not have domain");
                PublishError(householdRecording, eResponseStatus.DomainNotExists, "Household does not exist");
                return false;
            }

            if (householdRecording.AbsoluteStartDateTime.HasValue && householdRecording.AbsoluteStartDateTime.Value > 0 &&
                householdRecording.AbsoluteEndDateTime.HasValue && householdRecording.AbsoluteEndDateTime.Value > 0)
            {
                isPadded = false;
            }

            // add program asset
            epgAsset = GetOrCreateProgramAsset(householdRecording, userId, partnerSettings, cache);
            if (epgAsset == null)
            {
                return false;
            }

            return true;
        }

        private EpgAsset GetOrCreateProgramAsset(PartnerMigrationHouseholdRecording householdRecording, long userId, TimeShiftedTvPartnerSettings partnerSettings, CatalogGroupCache cache)
        {
            var groupId = (int)householdRecording.PartnerId;

            // if epg exist so just return it - if not create new one - DO NOT UPDATE
            var epgAssetId = GetAssetIdByExternalId(groupId, householdRecording.ProgramAssetExternalId, "epg");
            if (epgAssetId > 0)
            {
                var epgAssetResponse = EpgAssetManager.Instance.GetEpgAssets(groupId, new long[] { epgAssetId }, null);
                if (epgAssetResponse.Any())
                {
                    var existEpg = epgAssetResponse.FirstOrDefault();
                    return ValidateExistEpg(householdRecording, groupId, existEpg);
                }
            }

            if (householdRecording.LiveAssetExternalId.IsNullOrEmpty() ||
                !householdRecording.ProgramAssetStartDateTime.HasValue || householdRecording.ProgramAssetStartDateTime.Value < 1 ||
                !householdRecording.ProgramAssetEndDateTime.HasValue || householdRecording.ProgramAssetEndDateTime.Value < 1 ||
                householdRecording.ProgramAssetCrid.IsNullOrEmpty())
            {
                var allMandatoryField = "LiveAssetExternalId, ProgramAssetStartDateTime, ProgramAssetEndDateTime and ProgramAssetCrid";
                LogWarning(householdRecording, $"Wrong event body - must have {allMandatoryField}");
                PublishError(householdRecording, eResponseStatus.MandatoryField, $"{allMandatoryField} are missing");
                return null;
            }

            var (name, nameTranslations) = Mapper.GetMultilingual(householdRecording.ProgramAssetmultilingualName, cache);
            if (name.IsNullOrEmpty())
            {
                LogWarning(householdRecording, "Wrong event body - must have ProgramAssetmultilingualName");
                PublishError(householdRecording, eResponseStatus.MandatoryField, "programAssetmultilingualName is missing");
                return null;
            }

            var linearAssetId = GetAssetIdByExternalId(groupId, householdRecording.LiveAssetExternalId, "media");
            if (linearAssetId == 0)
            {
                LogWarning(householdRecording, $"LiveAssetExternalId [{householdRecording.LiveAssetExternalId}] does not exist");
                PublishError(householdRecording, eResponseStatus.ChannelDoesNotExist, "Live channel does not exist");
                return null;
            }

            var epgAsset = Mapper.MapToEpgAsset(householdRecording, linearAssetId, cache, name, nameTranslations);

            var catchUpBufferTime = DateTime.UtcNow.AddMinutes(-partnerSettings.CatchUpBufferLength ?? 0);
            
            if (epgAsset.EndDate >= catchUpBufferTime)
            {
                LogWarning(householdRecording, $"Cannot import time-based recording EndDate [{epgAsset.EndDate}] within catch up buffer window date time [{catchUpBufferTime}].");
                PublishError(householdRecording, eResponseStatus.CannotImportRecordingWithinCatchUpBuffer, "Cannot import time-based recording within catch up buffer window");
                return null;
            }

            var addAssetResponse = AssetManager.Instance.AddAsset(groupId, epgAsset, userId, isFromIngest: true);
            if (!addAssetResponse.IsOkStatusCode())
            {
                LogWarning(householdRecording, $"Failed to add EpgAsset, error status: [{addAssetResponse.ToStringStatus()}]");
                PublishError(householdRecording, (eResponseStatus)addAssetResponse.Status.Code, addAssetResponse.Status.Message);
                return null;
            }

            epgAsset = addAssetResponse.Object as EpgAsset;

            AddEpgAssetImages(householdRecording, groupId, epgAsset);

            return epgAsset;
        }

        private void AddEpgAssetImages(PartnerMigrationHouseholdRecording householdRecording, int groupId, EpgAsset epgAsset)
        {
            if (householdRecording.ProgramAssetImages != null && householdRecording.ProgramAssetImages.Length > 0)
            {
                var epgPictures = Mapper.MapToEpgPictures(groupId, householdRecording.ProgramAssetImages, epgAsset);
                var uploadImageResult = EpgImageManager.UploadEPGPictures(groupId, epgPictures).ConfigureAwait(false).GetAwaiter().GetResult().ToList();
                if (uploadImageResult.Any(x => !x.IsOkStatusCode()))
                {
                    foreach (var image in uploadImageResult.Where(img => !img.IsOkStatusCode()))
                    {
                        LogWarning(householdRecording, $"Failed to add image, image: [{image.Object?.Id}]");
                    }
                }

                //Upsert epg cb
                var _bl = new TvinciEpgBL(groupId);
                var documentIds = _bl.GetEpgsCBKeys(groupId, new List<long> { epgAsset.Id }, null, false);
                var documentId = documentIds.FirstOrDefault();
                var epgCb = EpgDal.GetEpgCBList(documentIds).FirstOrDefault();
                if (epgCb != null)
                {
                    if (epgCb.pictures == null)
                        epgCb.pictures = new List<EpgPicture>();

                    epgCb.pictures.AddRange(uploadImageResult.Select(x => x.Object).ToList());
                    if (!EpgDal.SaveEpgCB(documentId, epgCb, cb => TtlService.Instance.GetEpgCouchbaseTtlSeconds(cb)))
                        LogWarning(householdRecording, $"Couldn't update epg's images, epgId: [{epgAsset.Id}]");
                    else
                        AssetManager.Instance.InvalidateAsset(eAssetTypes.EPG, groupId, epgAsset.Id);
                }
            }
        }

        private EpgAsset ValidateExistEpg(PartnerMigrationHouseholdRecording householdRecording, int groupId, EpgAsset existEpg)
        {
            if (householdRecording.ProgramAssetStartDateTime.HasValue && householdRecording.ProgramAssetStartDateTime.Value > 0)
            {
                var existStartDate = existEpg.StartDate.Value.ToUtcUnixTimestampSeconds();
                if (existStartDate != householdRecording.ProgramAssetStartDateTime.Value)
                {
                    LogWarning(householdRecording, $"Exist epg [{existEpg.Id}] StartDate [{existStartDate}] does not match event StartDate [{householdRecording.ProgramAssetStartDateTime}]");
                    PublishError(householdRecording, eResponseStatus.EpgStartDateToProgramAssetMismatch, "Supplied start date/time does not match existing Program Asset");
                    return null;
                }
            }

            if (householdRecording.ProgramAssetEndDateTime.HasValue && householdRecording.ProgramAssetEndDateTime.Value > 0)
            {
                var existEndDate = existEpg.EndDate.Value.ToUtcUnixTimestampSeconds();
                if (existEndDate != householdRecording.ProgramAssetEndDateTime.Value)
                {
                    LogWarning(householdRecording, $"Exist epg [{existEpg.Id}] EndDate [{existEndDate}] does not match event EndDate [{householdRecording.ProgramAssetEndDateTime}]");
                    PublishError(householdRecording, eResponseStatus.EpgEndDateToProgramAssetMismatch, "Supplied end date/time does not match existing Program Asset");
                    return null;
                }
            }

            if (!householdRecording.ProgramAssetCrid.IsNullOrEmpty() && !householdRecording.ProgramAssetCrid.Equals(existEpg.Crid))
            {
                LogWarning(householdRecording, $"Exist epg [{existEpg.Id}] Crid [{existEpg.Crid}] does not match event Crid [{householdRecording.ProgramAssetCrid}]");
                PublishError(householdRecording, eResponseStatus.CridToProgramAssetMismatch, "Supplied CRID does not match existing Program Asset");
                return null;
            }

            if (!householdRecording.LiveAssetExternalId.IsNullOrEmpty())
            {
                var linearAssetId = GetAssetIdByExternalId(groupId, householdRecording.LiveAssetExternalId, "media");
                if (linearAssetId == 0)
                {
                    LogWarning(householdRecording, $"LiveAssetExternalId [{householdRecording.LiveAssetExternalId}] does not exist");
                    PublishError(householdRecording, eResponseStatus.ChannelDoesNotExist, "Live channel does not exist");
                    return null;
                }

                if (!existEpg.LinearAssetId.HasValue || existEpg.LinearAssetId.Value != linearAssetId)
                {
                    LogWarning(householdRecording, $"Exist epg [{existEpg.Id}] LinearAssetId [{existEpg.LinearAssetId}] does not match event LinearAssetId [{linearAssetId}]");
                    PublishError(householdRecording, eResponseStatus.LiveAssetToProgramAssetMismatch, "Supplied channel ID does not match existing Program Asset");
                    return null;
                }
            }

            return existEpg;
        }

        /// <summary>
        /// Get Asset ID By ExternalId with ksql
        /// </summary>
        /// <returns></returns>
        private long GetAssetIdByExternalId(int groupId, string externalId, string assetType)
        {
            var ksql = $"(and externalId='{externalId}' asset_type='{assetType}')";
            var result = api.SearchAssets(groupId, ksql, 0, 0, true, 0, false, string.Empty, string.Empty, string.Empty, 0, 0, true, true);
            if (result.Length == 0)
            {
                return 0;
            }
            return long.Parse(result[0].AssetId);
        }

        private void AddPaddedRecording(int groupId, PartnerMigrationHouseholdRecording householdRecording, long userId, long domainId, EpgAsset epgAsset)
        {
            var accountSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            
            //Paddings
            var defaultBeforeMinutes = Core.ConditionalAccess.Utils.ConvertSecondsToMinutes((int)
                (householdRecording.PaddingStartSeconds ?? (accountSettings.PaddingBeforeProgramStarts ?? 0))); 
                //allow in migration to use given padding as default even if not configured

            var defaultAfterMinutes = Core.ConditionalAccess.Utils.ConvertSecondsToMinutes((int)
                (householdRecording.PaddingEndSeconds ?? (accountSettings.PaddingAfterProgramEnds ?? 0)));

            var eventBeforeMinutes =
                Core.ConditionalAccess.Utils.ConvertSecondsToMinutes(
                    Mapper.GetNullableValueIfExist(householdRecording.PaddingStartSeconds ?? 0));
            var eventAfterMinutes =
                Core.ConditionalAccess.Utils.ConvertSecondsToMinutes(
                    Mapper.GetNullableValueIfExist(householdRecording.PaddingEndSeconds ?? 0));
            
            var recording = PaddedRecordingsManager.Instance.Record(
                groupId: groupId,
                programId: epgAsset.Id,
                epgChannelID: epgAsset.EpgChannelId ?? 0,
                startDate: epgAsset.StartDate.Value,
                endDate: epgAsset.EndDate.Value,
                crid: householdRecording.ProgramAssetCrid,
                domainIds: new List<long>() { domainId },
                failedDomainIds: out _,
                paddingBefore: householdRecording.StartPaddingIsPersonal == true ? eventBeforeMinutes : defaultBeforeMinutes,
                paddingAfter: householdRecording.EndPaddingIsPersonal == true ? eventAfterMinutes : defaultAfterMinutes,
                recordingContext: RecordingContext.Regular);

            if (recording == null || recording.Id == 0)
            {
                LogWarning(householdRecording, "Failed to add recording");
                PublishError(householdRecording, eResponseStatus.RecordingFailed, "Failed to record padding recording");
                return;
            }
            
            var recordingKey = PaddedRecordingsManager.GetRecordingKey(epgAsset.Id, recording.StartPadding.Value, recording.EndPadding.Value);
            var insertResult = PaddedRecordingsManager.Instance.UpdateOrInsertHouseholdRecording(
                groupId: groupId,
                userId: userId,
                householdId: domainId,
                recording: recording,
                recordingKey: recordingKey,
                status: TstvRecordingStatus.OK,
                scheduledSaved: false,
                originalStartPadding: householdRecording.StartPaddingIsPersonal == true ? 
                    Core.ConditionalAccess.Utils.ConvertSecondsToMinutes(Mapper.GetNullableValueIfExist(householdRecording.PaddingStartSeconds)) : (int?)null,
                originalEndPadding: householdRecording.EndPaddingIsPersonal == true ? 
                    Core.ConditionalAccess.Utils.ConvertSecondsToMinutes(Mapper.GetNullableValueIfExist(householdRecording.PaddingEndSeconds)) : (int?)null);

            if (!insertResult.Success || insertResult.HouseholdRecordingId < 1)
            {
                LogWarning(householdRecording, "Failed to add recording");
                PublishError(householdRecording, eResponseStatus.RecordingFailed, $"Failed to record padding recording");
            }
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId, domainId));
                PublishSuccess(householdRecording, insertResult.HouseholdRecordingId);
            }
        }

        private void AddImmediateRecording(int groupId, PartnerMigrationHouseholdRecording householdRecording, long userId, long domainId, EpgAsset epgAsset)
        {
            var endPadding = Core.ConditionalAccess.Utils.ConvertSecondsToMinutes(Mapper.GetNullableValueIfExist(householdRecording.PaddingEndSeconds));
            var program = new ApiObjects.Recordings.Program(epgAsset.Id, epgAsset.StartDate.Value, epgAsset.EndDate.Value, epgAsset.EpgChannelId.Value, epgAsset.Crid);
            var result = PaddedRecordingsManager.Instance.ImmediateRecord(groupId, userId, domainId, epgAsset.Id, endPadding, program, householdRecording.AbsoluteStartDateTime, householdRecording.AbsoluteEndDateTime);

            if (result.HasObject() && result.Object.Id > 0)
            {
                PublishSuccess(householdRecording, result.Object.Id);
            }
            else
            {
                var statusCode = (eResponseStatus)result.Status.Code;
                if (statusCode == eResponseStatus.Error || (statusCode == eResponseStatus.OK && result.Object.Id < 1))
                {
                    LogWarning(householdRecording, "Failed to add recording");
                    PublishError(householdRecording, eResponseStatus.RecordingFailed, $"Failed to record immediate recording");
                }
                else
                {
                    LogWarning(householdRecording, result.ToStringStatus());
                    PublishError(householdRecording, statusCode, result.Status.Message);
                }
            }
        }

        private Code MapResponseStatusToCode(eResponseStatus responseStatus)
        {
            switch (responseStatus)
            {
                case eResponseStatus.OK: return Code.The0;
                case eResponseStatus.Error: return Code.The1;
                case eResponseStatus.DomainNotExists: return Code.The1006;
                case eResponseStatus.UserDoesNotExist: return Code.The2000;
                case eResponseStatus.TopicNotFound: return Code.The2038;
                case eResponseStatus.RecordingFailed: return Code.The3040;
                case eResponseStatus.RecordingStatusNotValid: return Code.The3043;
                case eResponseStatus.RecordingExceededConcurrency: return Code.The3094;
                case eResponseStatus.ExceedingAllowedImmediateRecordingAttempts: return Code.The3095;
                case eResponseStatus.AssetStructDoesNotExist: return Code.The4028;
                case eResponseStatus.MetaDoesNotExist: return Code.The4033;
                case eResponseStatus.AssetExternalIdMustBeUnique: return Code.The4038;
                case eResponseStatus.AssetDoesNotExist: return Code.The4039;
                case eResponseStatus.InvalidMetaType: return Code.The4040;
                case eResponseStatus.InvalidValueSentForMeta: return Code.The4041;
                case eResponseStatus.ChannelDoesNotExist: return Code.The4064;
                case eResponseStatus.GroupDoesNotContainLanguage: return Code.The4078;
                case eResponseStatus.StartDateShouldBeLessThanEndDate: return Code.The4111;
                case eResponseStatus.LiveAssetToProgramAssetMismatch: return Code.The4122;
                case eResponseStatus.EpgStartDateToProgramAssetMismatch: return Code.The4123;
                case eResponseStatus.EpgEndDateToProgramAssetMismatch: return Code.The4124;
                case eResponseStatus.CridToProgramAssetMismatch: return Code.The4125;
                case eResponseStatus.CannotImportRecordingWithinCatchUpBuffer: return Code.The4126;
                case eResponseStatus.NameRequired: return Code.The5005;
                case eResponseStatus.TimeShiftedTvPartnerSettingsNotFound: return Code.The5022;
                case eResponseStatus.AssetUserRulesOperationsDisable: return Code.The5033;
                case eResponseStatus.PersonalizedRecordingDisabled: return Code.The5097;
                case eResponseStatus.MandatoryField: return Code.The9011;
                case eResponseStatus.EPGSProgramDatesError: return Code.The11003;
            }

            throw new NotImplementedException($"Mapping for eResponseStatus {responseStatus} to Code was not implemented");
        }

        private void PublishError(PartnerMigrationHouseholdRecording householdRecording, eResponseStatus responseStatus, string message)
        {
            var status = new HouseholdRecordingMigrationStatus
            {
                PartnerId = householdRecording.PartnerId,
                Message = message,
                Code = MapResponseStatusToCode(responseStatus),
                OttUserExternalId = householdRecording.OttUserExternalId,
                ProgramAssetExternalId = householdRecording.ProgramAssetExternalId
            };
            _publisher.Publish(status);
        }

        private void PublishSuccess(PartnerMigrationHouseholdRecording householdRecording, long recordingId)
        {
            var status = new HouseholdRecordingMigrationStatus
            {
                PartnerId = householdRecording.PartnerId,
                Message = "Success",
                Code = Code.The0,
                OttUserExternalId = householdRecording.OttUserExternalId,
                ProgramAssetExternalId = householdRecording.ProgramAssetExternalId,
                RecordingId = recordingId
            };
            _publisher.Publish(status);
        }

        private void LogWarning(PartnerMigrationHouseholdRecording householdRecording, string message)
        {
            _logger.LogWarning($"{message} - PartnerId [{householdRecording.PartnerId}], OttUserExternalId [{householdRecording.OttUserExternalId}] and ProgramAssetExternalId [{householdRecording.ProgramAssetExternalId}].");
        }
    }
}
