using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using IngestHandler.Common;
using Phx.Lib.Log;

namespace IngestHandler.Common.Managers
{
    public abstract class EpgGapManager
    {
        protected readonly BulkUpload _bulkUpload;
        protected readonly CRUDOperations<EpgProgramBulkUploadObject> _crudOperations;
        protected readonly Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> _resultsDictionary;
        protected static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected EpgGapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _bulkUpload = bulkUpload;
            _crudOperations = crudOperations;
            _resultsDictionary = bulkUpload.ConstructResultsDictionary();
        }

        public static EpgGapManager GetGapManagerByPolicy(eIngestProfileAutofillPolicy policy, BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            switch (policy)
            {
                case eIngestProfileAutofillPolicy.Autofill:
                    return new AutofillGapManager(bulkUpload, crudOperations);

                case eIngestProfileAutofillPolicy.KeepHoles:
                    return new IgnoreGapManager(bulkUpload, crudOperations);

                case eIngestProfileAutofillPolicy.Reject:
                default:
                    return new RejectGapManager(bulkUpload, crudOperations);
            }
        }

        public bool HandleGaps()
        {
            var gaps = new List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>>();
            var newEpgSchedule = _crudOperations.ItemsToAdd
                .Concat(_crudOperations.ItemsToUpdate)
                .Concat(_crudOperations.AffectedItems)
                .Concat(_crudOperations.RemainingItems)
                .Where(c => !c.IsAutoFill)
                .OrderBy(i => i.StartDate)
                .ToList();

            for (var i = 0; i < newEpgSchedule.Count - 1; i++)
            {
                var prog = newEpgSchedule[i];
                var nextProg = newEpgSchedule[i + 1];

                if (prog.EndDate < nextProg.StartDate)
                {
                    gaps.Add(Tuple.Create(prog, nextProg));
                }
            }

            var isValid = HandleGaps(gaps);
            return isValid;
        }

        protected abstract bool HandleGaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> gaps);


        protected bool TryAddError(int channelId, string epgExternalId, eResponseStatus status, string msg)
        {
            if (_resultsDictionary.TryGetValue(channelId, out var resultsOfChannel))
            {
                if (resultsOfChannel.TryGetValue(epgExternalId, out var epgResultObject))
                {
                    epgResultObject.AddError(status, msg);
                    return true;
                }
            }

            return false;
        }

        protected bool TryAddWarnning(int channelId, string epgExternalId, eResponseStatus status, string msg)
        {
            if (_resultsDictionary.TryGetValue(channelId, out var resultsOfChannel))
            {
                if (resultsOfChannel.TryGetValue(epgExternalId, out var epgResultObject))
                {
                    epgResultObject.AddWarning((int) status, msg);
                    return true;
                }
            }

            return false;
        }
    }

    public class RejectGapManager : EpgGapManager
    {
        public RejectGapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleGaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> gaps)
        {
            // if no gaps result is valid, should not reject
            if (!gaps.Any()) { return true; }

            foreach (var gap in gaps)
            {
                var gapStartTime = gap.Item1.EndDate;
                var gapEndTime = gap.Item2.StartDate;
                var errorMessage = $"Program {gap.Item1.EpgExternalId} end: {gapStartTime} creates a gap with another program {gap.Item2.EpgExternalId} start: {gapEndTime}";
                // Using try add error on both items because we are not sure which program is from source and which is existing
                var isUpdateSuccessItem1 = TryAddError(gap.Item1.ChannelId, gap.Item1.EpgExternalId, eResponseStatus.EPGSProgramDatesError, errorMessage);
                var isUpdateSuccessItem2 = TryAddError(gap.Item2.ChannelId, gap.Item2.EpgExternalId, eResponseStatus.EPGSProgramDatesError, errorMessage);

                // this means the gap was created between 2 programs in current epg schedule and maybe an update caused the gap
                // so we are gonna search for the culprit
                if (!isUpdateSuccessItem1 && !isUpdateSuccessItem2)
                {
                    var updatedProgramsCausingTheGap = _crudOperations.ItemsToUpdate.Where(p => p.StartDate >= gapStartTime && p.EndDate >= gapEndTime).ToList();
                    if (updatedProgramsCausingTheGap.Any())
                    {
                        updatedProgramsCausingTheGap.ForEach(p =>
                        {
                            errorMessage = $"Program {p.EpgExternalId} update, is causing a gap between program {gap.Item1.EpgExternalId} end: {gapStartTime} creates and program {gap.Item2.EpgExternalId} start: {gapEndTime}";

                            _resultsDictionary[p.ChannelId][p.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, errorMessage);
                        });
                    }
                    else
                    {
                        //This means we found an unxplainable gap and we cannot identify what was the root cause for it, so we put a general error;
                        _bulkUpload.AddError(eResponseStatus.EPGSProgramDatesError, errorMessage);
                    }
                }
            }

            // if we got to here, we looped over the gaps placed errors
            // now we can return a reject (isValid = false) to the caller
            return false;
        }
    }


    public class AutofillGapManager : EpgGapManager
    {
        public AutofillGapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleGaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> gaps)
        {
            var autofillPrograms = new List<EpgProgramBulkUploadObject>();
            foreach (var gap in gaps)
            {
                var warnMessage = $"Autofilling gap in between {gap.Item1.EpgExternalId} end: {gap.Item1.EndDate} and{gap.Item2.EpgExternalId} start: {gap.Item2.StartDate}";
                // Using try add warning on both items because we are not sure which program is from source and which is existing
                TryAddWarnning(gap.Item1.ChannelId, gap.Item1.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);
                TryAddWarnning(gap.Item2.ChannelId, gap.Item2.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);
                var autofillProgram = GetDefaultAutoFillProgram(gap.Item1.EndDate, gap.Item2.StartDate, gap.Item1.ChannelId, gap.Item1.ChannelExternalId, gap.Item1.LinearMediaId);
                autofillPrograms.Add(autofillProgram);
            }

            _crudOperations.ItemsToAdd.AddRange(autofillPrograms);
            return true;
        }

        private EpgProgramBulkUploadObject GetDefaultAutoFillProgram(DateTime start, DateTime end, int channelId, string channelExternalId, long leniarMediaId)
        {
            return new EpgProgramBulkUploadObject()
            {
                StartDate = start,
                EndDate = end,
                IsAutoFill = true,
                ChannelId = channelId,
                EpgExternalId = Guid.NewGuid().ToString(),
                ParentGroupId = _bulkUpload.GroupId,
                GroupId = _bulkUpload.GroupId,
                ChannelExternalId = channelExternalId,
                LinearMediaId = leniarMediaId
                //EpgCbObjects = new List<EpgCB>() { autoFillEpgCB }
            };
        }
    }

    public class IgnoreGapManager : EpgGapManager
    {
        public IgnoreGapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleGaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> gaps)
        {
            return true;
        }
    }
}