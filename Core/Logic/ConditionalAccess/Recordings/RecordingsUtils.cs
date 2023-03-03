using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Recordings;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using Core.Pricing;
using DAL;
using EpgBL;
using Phx.Lib.Log;
using QueueWrapper;
using TVinciShared;

namespace Core.Recordings
{
    public static class RecordingsUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string ROUTING_KEY_MODIFIED_RECORDING = "PROCESS_MODIFIED_RECORDING\\{0}";

        public static void UpdateCouchbase(int groupId, long programId, long recordingId, bool shouldDelete = false)
        {
            log.Info(
                $"UpdateCouchbase: recording UpdateCouchbase with epgId:{programId}, recordingId: {recordingId}, shouldDelete: {shouldDelete}");
            if (shouldDelete)
            {
                var recording = RecordingsDAL.GetRecordingByProgramId_CB(programId);
                RecordingsDAL.DeleteRecording_CB(recording);
            }
            else
            {
                var epgBLTvinci = new TvinciEpgBL(groupId);
                var epg = epgBLTvinci.GetEpgCB((ulong) programId);
                if (epg != null)
                {
                    var recording = new RecordingCB(epg)
                    {
                        RecordingId = (ulong) recordingId
                    };

                    RecordingsDAL.UpdateRecording_CB(recording);
                }
                else
                {
                    log.Error($"recording UpdateCouchbase failed epgId:{programId}");
                }
            }
        }

        public static void UpdateIndex(int groupId, long recordingId, eAction action)
        {
            Catalog.Module.UpdateRecordingsIndex(new List<long> {recordingId}, groupId, action);
        }

        public static void UpdateIndex(int groupId, long epgId, long recordingId)
        {
            var epgBL = new TvinciEpgBL(groupId);
            var epgDoc = epgBL.GetEpgCB((ulong) epgId);

            var recordingDoc = RecordingsDAL.GetRecordingByProgramId_CB(epgId);
            if (recordingDoc != null && epgDoc.StartDate.Date != recordingDoc.StartDate.Date)
                Catalog.Module.UpdateRecordingsIndex(new List<long> {recordingId}, groupId, eAction.Delete);

            UpdateIndex(groupId, recordingId, eAction.Update);
        }

        public static TstvRecordingStatus GetTstvRecordingStatus(DateTime epgStartDate, DateTime epgEndDate,
            TstvRecordingStatus recordingStatus)
        {
            var response = recordingStatus;
            if (recordingStatus == TstvRecordingStatus.Scheduled)
            {
                // If program already finished, we say it is recorded
                if (epgEndDate < DateTime.UtcNow)
                    response = TstvRecordingStatus.Recorded;
                // If program already started but didn't finish, we say it is recording
                else if (epgStartDate < DateTime.UtcNow) response = TstvRecordingStatus.Recording;
            }

            return response;
        }

        public static bool IsValidRecordingStatus(TstvRecordingStatus recordingStatus, bool isOkStatusValid = false)
        {
            var res = false;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.OK:
                    res = isOkStatusValid;
                    break;

                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                case TstvRecordingStatus.Scheduled:
                    res = true;
                    break;

                case TstvRecordingStatus.Deleted:
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Canceled:
                case TstvRecordingStatus.LifeTimePeriodExpired:
                default:
                    res = false;
                    break;
            }

            return res;
        }

        public static List<DomainRecordingStatus> ConvertToDomainRecordingStatus(
            List<TstvRecordingStatus> recordingStatus)
        {
            var result = new List<DomainRecordingStatus>();
            foreach (var status in recordingStatus)
                switch (status)
                {
                    case TstvRecordingStatus.Failed:
                    case TstvRecordingStatus.Scheduled:
                    case TstvRecordingStatus.Recording:
                    case TstvRecordingStatus.Recorded:
                        if (!result.Contains(DomainRecordingStatus.OK)) result.Add(DomainRecordingStatus.OK);
                        break;
                    case TstvRecordingStatus.Canceled:
                        if (!result.Contains(DomainRecordingStatus.Canceled))
                            result.Add(DomainRecordingStatus.Canceled);
                        break;
                    case TstvRecordingStatus.SeriesCancel:
                        if (!result.Contains(DomainRecordingStatus.SeriesCancel))
                            result.Add(DomainRecordingStatus.SeriesCancel);
                        break;
                    case TstvRecordingStatus.Deleted:
                    case TstvRecordingStatus.SeriesDelete:
                    default:
                        break;
                }

            return result;
        }

        public static DomainRecordingStatus? ConvertToDomainRecordingStatus(TstvRecordingStatus recordingStatus)
        {
            DomainRecordingStatus? result = null;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Scheduled:
                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                    result = DomainRecordingStatus.OK;
                    break;
                case TstvRecordingStatus.Canceled:
                    result = DomainRecordingStatus.Canceled;
                    break;
                case TstvRecordingStatus.Deleted:
                    result = DomainRecordingStatus.Deleted;
                    break;
                case TstvRecordingStatus.SeriesCancel:
                    result = DomainRecordingStatus.SeriesCancel;
                    break;
                case TstvRecordingStatus.SeriesDelete:
                    result = DomainRecordingStatus.SeriesDelete;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static RecordingInternalStatus? ConvertToRecordingInternalStatus(TstvRecordingStatus recordingStatus)
        {
            var recordingInternalStatus = RecordingInternalStatus.OK;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.Scheduled:
                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                case TstvRecordingStatus.OK:
                {
                    recordingInternalStatus = RecordingInternalStatus.OK;
                    break;
                }
                case TstvRecordingStatus.Failed:
                {
                    recordingInternalStatus = RecordingInternalStatus.Failed;
                    break;
                }
                case TstvRecordingStatus.Canceled:
                {
                    recordingInternalStatus = RecordingInternalStatus.Canceled;
                    break;
                }
                case TstvRecordingStatus.Deleted:
                {
                    recordingInternalStatus = RecordingInternalStatus.Deleted;
                    break;
                }
                default:
                    break;
            }

            return recordingInternalStatus;
        }


        public static bool IsServiceAllowed(int partnerId, int domainID, eService service)
        {
            var enforcedGroupServices = ConditionalAccess.Utils.GetGroupEnforcedServices(partnerId);
            //check if service is part of the group enforced services
            if (enforcedGroupServices == null || enforcedGroupServices.Count == 0 ||
                !enforcedGroupServices.Contains((int) service)) return true;

            // check if the service is allowed for the domain
            var allowedDomainServicesRes = GetDomainServices(partnerId, domainID);
            if (allowedDomainServicesRes != null && allowedDomainServicesRes.Status.Code == 0 &&
                allowedDomainServicesRes.Services != null && allowedDomainServicesRes.Services.Count > 0 &&
                allowedDomainServicesRes.Services.FirstOrDefault(s => s.ID == (int) service) != null)
                return true;

            return false;
        }

        public static DomainServicesResponse GetDomainServices(int partnerId, int domainID)
        {
            var domainServicesResponse = new DomainServicesResponse((int) eResponseStatus.OK);
            DomainEntitlements domainEntitlements = null;
            // Get all user entitlements
            if (!ConditionalAccess.Utils.TryGetDomainEntitlementsFromCache(partnerId, domainID, null,
                ref domainEntitlements))
            {
                log.ErrorFormat("Utils.GetUserEntitlements, groupId: {0}, domainId: {1}", partnerId, domainID);
                return domainServicesResponse;
            }

            if (domainEntitlements != null && domainEntitlements.DomainBundleEntitlements != null &&
                domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null
                && domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.Count > 0)
            {
                var subscriptionIDs = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions
                    .Select(x => x.Value.sBundleCode).ToList();
                string userName = string.Empty, pass = string.Empty;
                ConditionalAccess.Utils.GetWSCredentials(partnerId, eWSModules.PRICING, ref userName, ref pass);
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(pass))
                {
                    var subs = ConditionalAccess.Utils.GetSubscriptionsDataWithCaching(subscriptionIDs, partnerId);
                    if (subs != null && subs.Length > 0)
                    {
                        var services = subs.Where(x => x.m_lServices != null).SelectMany(x => x.m_lServices).Distinct()
                            .ToList();
                        if (services != null && services.Count > 0) domainServicesResponse.Services.AddRange(services);
                    }
                }
            }

            return domainServicesResponse;
        }

        public static Recording ValidateEntitlementForEpg(int partnerId, long userId, long domainId,
            EPGChannelProgrammeObject epg,
            int? paddingBefore = null, int? paddingAfter = null)
        {
            BaseConditionalAccess t = null;
            ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, partnerId);
            if (t != null)
                return t.ValidateEntitlementForEpg(userId.ToString(), domainId, epg, paddingBefore, paddingAfter);
            return null;
        }

        public static Recording CancelOrDeleteRecord(int partnerId, string userId, long domainId,
            long domainRecordingId, TstvRecordingStatus tstvRecordingStatus, bool shouldValidateUserAndDomain = true)
        {
            BaseConditionalAccess t = null;
            ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, partnerId);
            if (t != null)
                return t.CancelOrDeleteRecord(userId, domainId, domainRecordingId, tstvRecordingStatus,
                    shouldValidateUserAndDomain);
            return null;
        }

        public static void EnqueueRecordingModificationEvent(int groupId, Recording recording, int oldRecordingLength,
            int taskId = 0)
        {
            EnqueueRecordingModificationEvent(groupId, recording.Id, oldRecordingLength, taskId);
        }

        public static void EnqueueRecordingModificationEvent(int groupId, long recordingId, int oldRecordingLength,
            int taskId = 0)
        {
            var queue = new GenericCeleryQueue();
            var utcNow = DateTime.UtcNow;
            var data = new ApiObjects.QueueObjects.RecordingModificationData(groupId, taskId, recordingId, 0,
                oldRecordingLength) {ETA = utcNow};
            var queueExpiredRecordingResult =
                queue.Enqueue(data, string.Format(ROUTING_KEY_MODIFIED_RECORDING, groupId));
            if (!queueExpiredRecordingResult)
                log.ErrorFormat(
                    "Failed to queue ExpiredRecording task for RetryTaskAfterProgramEnded when recording FAILED, recordingId: {0}, groupId: {1}",
                    recordingId, groupId);
        }

        public static Recording BuildRecordingFromTBRecording(int groupId, TimeBasedRecording timeBasedRecording, 
            Program program, HouseholdRecording householdRecording = null)
        {
            var recording = new Recording()
            {
                Id = householdRecording?.Id ?? timeBasedRecording.Id,
                Status = new Status(eResponseStatus.OK),
                EpgId = timeBasedRecording.EpgId,
                ChannelId = timeBasedRecording.EpgChannelId,
                ExternalRecordingId = timeBasedRecording.ExternalId ?? string.Empty,
                EpgStartDate = program.StartDate,
                EpgEndDate = program.EndDate,
                GetStatusRetries = timeBasedRecording.RetriesStatus,
                ViewableUntilDate = timeBasedRecording.ViewableUntilEpoch,
                CreateDate = timeBasedRecording.CreateDate,
                UpdateDate = timeBasedRecording.UpdateDate,
                Crid = timeBasedRecording.Crid,
                StartPadding = timeBasedRecording.PaddingBeforeMins,
                EndPadding = timeBasedRecording.PaddingAfterMins,
                RecordedProgramId = timeBasedRecording.ProgramId,
                AbsoluteStartTime = timeBasedRecording.AbsoluteStartTime,
                AbsoluteEndTime = timeBasedRecording.AbsoluteEndTime
            };

            if (householdRecording != null)
            {
                recording.Type = (RecordingType)Enum.Parse(typeof(RecordingType), householdRecording.RecordingType);
                recording.ProtectedUntilDate = householdRecording.ProtectedUntilEpoch;
            }

            TstvRecordingStatus? recordingStatus;
            if (Enum.IsDefined(typeof(RecordingInternalStatus), timeBasedRecording.Status))
            {
                var recordingInternalStatus = (RecordingInternalStatus)Enum.Parse(typeof(RecordingInternalStatus), timeBasedRecording.Status);

                recordingStatus = ConditionalAccess.Utils.ConvertToTstvRecordingStatus(recordingInternalStatus,
                    recording.AbsoluteStartTime ?? program.StartDate.AddMinutes(-1 * timeBasedRecording.PaddingBeforeMins),
                    recording.AbsoluteEndTime ?? program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins), timeBasedRecording.CreateDate);
            }
            else
            {
                recordingStatus = (TstvRecordingStatus)Enum.Parse(typeof(TstvRecordingStatus), timeBasedRecording.Status);  //fix "Scheduled" value TODO: change the enum received 
            }
            
            //BEO-13622
            var isStopped = householdRecording?.IsStopped ?? false;
            if (isStopped && recordingStatus == TstvRecordingStatus.Recording)
            {
                recordingStatus = TstvRecordingStatus.Recorded;
            }
            
            if (recordingStatus.HasValue)
            {
                recording.RecordingStatus = recordingStatus.Value;
            }

            // if recording status is Recorded then set ViewableUntilDate
            if (recording.RecordingStatus != TstvRecordingStatus.Recorded)
            {
                recording.ViewableUntilDate = null;
            }

            recording.Duration = QuotaManager.GetRecordingDurationSeconds(groupId, recording, false);
            return recording;
        }

        public static long CalcTtl(DateTime nextCheck)
        {
            var newExpiration = nextCheck.AddHours(1);
            return DateUtils.ToUtcUnixTimestampSeconds(newExpiration);
        }

        public static (bool QuotaOverage, Status Status) QuotaOverageAndEntitlement(int groupId, long domainId, long userId, long epgId, EPGChannelProgrammeObject epg, long? absoluteStartEpoch, long? absoluteEndEpoch)
        {
            BaseConditionalAccess t = null;
            var quotaOverage = false;
            Status status = new Status(){Code = (int)eResponseStatus.OK};
            Recording _record;
            Core.ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, groupId);
            if (t != null)
            {
                long domainID = 0;
                _record = t.QueryRecords(userId.ToString(), epgId, ref domainID, ApiObjects.RecordingType.Single,
                    true,
                    true, epg, 0, 0, absoluteStartEpoch, absoluteEndEpoch);

                if (_record == null || _record.Status == null || _record.Status.Code != (int)eResponseStatus.OK)
                {
                    //check if it setting for quota_overage if so asyncronized action to delete oldest recordings 
                    //else return exceedeQuota
                    if (_record.Status.Code == (int)eResponseStatus.ExceededQuota)
                    {
                        var accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                        if (accountSettings != null &&
                            accountSettings.QuotaOveragePolicy == QuotaOveragePolicy.FIFOAutoDelete)
                        {
                            quotaOverage = true;
                        }
                    }

                    if (!quotaOverage)
                    {
                        log.Debug(
                            $"Recording status not valid, EpgID: {epgId}, DomainID: {domainID}, UserID: {userId}, Recording: {_record}");
                        status.Set(eResponseStatus.RecordingStatusNotValid);
                    }
                }

                if (_record == null || !_record.Status.IsOkStatusCode())
                {
                    status = _record?.Status ??
                                 new Status(eResponseStatus.NotEntitled, $"Can't record epg: {epgId}");
                    status.Set(status);
                }
            }

            return (quotaOverage, status);
        }
    }
}