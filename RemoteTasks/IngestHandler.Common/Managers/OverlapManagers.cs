using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Force.DeepCloner;
using IngestHandler.Common;
using Phx.Lib.Log;
using TVinciShared;

namespace IngestHandler.Common.Managers
{
    public abstract class OverlapManager
    {
        protected static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected readonly BulkUpload _bulkUpload;
        protected readonly CRUDOperations<EpgProgramBulkUploadObject> _crudOperations;
        protected readonly BulkUploadResultsDictionary _resultsDictionary;

        protected OverlapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _bulkUpload = bulkUpload;
            _crudOperations = crudOperations;
            _resultsDictionary = bulkUpload.ConstructResultsDictionary();

        }
        public static OverlapManager GetOverlapManagerByPolicy(eIngestProfileOverlapPolicy policy, BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            switch (policy)
            {
                case eIngestProfileOverlapPolicy.CutSource: return new CutSourceOverlapManager(bulkUpload, crudOperations);
                case eIngestProfileOverlapPolicy.CutTarget: return new CurTargetOverlapManager(bulkUpload, crudOperations);
                case eIngestProfileOverlapPolicy.Reject:
                default:
                    return new RejectOverlapManager(bulkUpload, crudOperations);
            }
        }

        public bool HandleOverlaps()
        {
            var remainingItems = _crudOperations.RemainingItems.ToList();
            var allNew = _crudOperations.ItemsToAdd.Concat(_crudOperations.ItemsToUpdate).OrderBy(p => p.StartDate).ToList();
            var firstNewProgram = allNew.FirstOrDefault();
            var lastNewProgram = allNew.LastOrDefault();
            var firstAndLastProgram = new List<EpgProgramBulkUploadObject> {firstNewProgram, lastNewProgram};
            var overlappingEdgePrograms = EpgBL.Utils.GetOverlappingPrograms(firstAndLastProgram, remainingItems);
            
            _logger.Debug($"HandleOverlaps > before handling overlap: firstNewProgram:[{firstNewProgram}], lastNewProgram:[{lastNewProgram}], overlappingEdgePrograms:[{string.Join(",", overlappingEdgePrograms)}]");
            var isValid = HandleOverlaps(overlappingEdgePrograms);
            _logger.Debug($"HandleOverlaps > after handling overlap: isValid:[{isValid}]");
            return isValid;
        }
        protected abstract bool HandleOverlaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlaps);
    }

    public class RejectOverlapManager : OverlapManager
    {
        public RejectOverlapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleOverlaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlaps)
        {
            // if overlaps is null or empty this policy isValid=true
            if (overlaps?.Any() != true) { return true; }

            // otherwise add errors to results and return false
            foreach (var overlap in overlaps)
            {
                var programToIngest = overlap.Item1;
                var currentProgram = overlap.Item2;
                var msg = $"Program [{programToIngest.EpgExternalId}] overlapping [{currentProgram.EpgExternalId}, policy is set to Reject, rejecting input on overlapping items";
                _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddError(eResponseStatus.Error, msg);
            }

            return false;
        }
    }

    public class CutSourceOverlapManager : OverlapManager
    {
        public CutSourceOverlapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleOverlaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlaps)
        {
            foreach (var overlap in overlaps)
            {
                var programToIngest = overlap.Item1;
                var currentProgram = overlap.Item2;
                var msg = $"Program [{programToIngest.EpgExternalId}] overlapping [{currentProgram.EpgExternalId}, policy is set to CutSource, ";

                if (programToIngest.IsInMiddle(currentProgram))
                {
                    msg += $"program {programToIngest.EpgExternalId} will be removed as its end and start date are inside an existing program.";
                    _crudOperations.ItemsToAdd.Remove(programToIngest);
                    _crudOperations.ItemsToUpdate.Remove(programToIngest);
                    // setting this status tp play because ingets handler will not do that as it will not be sent to him at all
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].Status = BulkUploadResultStatus.Ok;
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);
                    continue;
                }

                if (programToIngest.StartsAfter(currentProgram))
                {
                    msg += $"changing new program to ingest start date from [{programToIngest.StartDate}], to [{currentProgram.EndDate}]";
                    programToIngest.StartDate = currentProgram.EndDate;
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);
                    continue;
                }

                if (programToIngest.StartsBefore(currentProgram))
                {
                    msg += $"changing new program to ingest end date from [{programToIngest.EndDate}], to [{currentProgram.StartDate}]";
                    programToIngest.EndDate = currentProgram.StartDate;
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);
                    continue;
                }

                msg = $"CutSourceOverlapHandler > programToIngest:[{programToIngest.EpgExternalId}], currentProgram:[{currentProgram.EpgExternalId}] are overlapping! and could not find a suitable fix for this situation";
                _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddError(eResponseStatus.Error, msg);
                _logger.Error(msg);
            }

            // always return true, should handle the cutting
            return true;
        }
    }

    public class CurTargetOverlapManager : OverlapManager
    {
        public CurTargetOverlapManager(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOperations) : base(bulkUpload, crudOperations)
        {
        }

        protected override bool HandleOverlaps(IList<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlaps)
        {
            foreach (var overlap in overlaps)
            {
                var programToIngest = overlap.Item1;
                var currentProgram = overlap.Item2;
                var msg = $"Program [{programToIngest.EpgExternalId}] overlapping [{currentProgram.EpgExternalId}, policy is set to CutTarget, ";

                if (programToIngest.IsInMiddle(currentProgram))
                {
                    msg += $"this program is in the middle of program {currentProgram.EpgExternalId} - the exsiting program will be removed and remaining gaps will be handled according to autofill policy.";
                    _crudOperations.ItemsToDelete.Add(currentProgram);
                    _crudOperations.RemainingItems.Remove(currentProgram);
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);
                    continue;
                }

                if (programToIngest.StartsAfter(currentProgram))
                {
                    msg += $"changing current program end date from [{currentProgram.EndDate}], to [{programToIngest.StartDate}]";
                    currentProgram.EndDate = programToIngest.StartDate;
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);

                    // we have to add same program to be deleted as in es it might move to a different index so we delete it agians the entire alias
                    _crudOperations.ItemsToDelete.Add(currentProgram);
                    _crudOperations.AffectedItems.Add(ObjectCopier.Clone(currentProgram));
                    _crudOperations.RemainingItems.Remove(currentProgram);
                    continue;
                }
                

                if (programToIngest.StartsBefore(currentProgram))
                {
                    msg += $"changing current program start date from [{currentProgram.StartDate}], to [{programToIngest.EndDate}]";
                    HandleMidnightCrossChanges(currentProgram, programToIngest);
                    
                    currentProgram.StartDate = programToIngest.EndDate;
                    _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int) eResponseStatus.EPGProgramOverlapFixed, msg);
                    
                    // we have to add same program to be deleted as in es it might move to a different index so we delete it agians the entire alias
                    _crudOperations.ItemsToDelete.Add(currentProgram);
                    _crudOperations.AffectedItems.Add(ObjectCopier.Clone(currentProgram));
                    _crudOperations.RemainingItems.Remove(currentProgram);
                    continue;
                }

                msg = $"CurTargetOverlapHandler > programToIngest:[{programToIngest.EpgExternalId}], currentProgram:[{currentProgram.EpgExternalId}] are overlapping! and could not find a suitable fix for this situation";
                _resultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddError(eResponseStatus.Error, msg);
            }

            // always return true, should handle the cutting
            return true;
        }

        /// <summary>
        /// Here we handle program that has been pushed to the next day, so we need to delete it on previous day.
        /// Cause we're mutating original object, copy of object should be put to save original StartDate.
        /// For example, current program dates 21 Jan 21:00 - 22 Jan 02:00, we cut it to 22 Jan 01:00 - 22 Jan 02:00.
        /// </summary>
        /// <param name="currentProgram"></param>
        /// <param name="programToIngest"></param>
        private void HandleMidnightCrossChanges(
            EpgProgramBulkUploadObject currentProgram,
            EpgProgramBulkUploadObject programToIngest)
        {
            if (currentProgram.StartDate.Date < programToIngest.EndDate.Date)
            {
                _crudOperations.ItemsToDelete.Add(currentProgram.ShallowClone());
            }
        }
    }
}