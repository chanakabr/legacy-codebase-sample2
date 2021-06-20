using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using IngestHandler.Common;
using IngestTransformationHandler.Repositories;
using KLogMonitor;
using TVinciShared;

namespace IngestTransformationHandler.Managers
{
    public interface IEpgCRUDOperationsManager
    {
        CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperations(BulkUpload bulkUpload, eIngestProfileOverlapPolicy overlapPolicy, eIngestProfileAutofillPolicy autofillPolicy);
    }

    public class EpgCRUDOperationsManager : IEpgCRUDOperationsManager
    {
        private readonly IEpgRepository _epgRepository;

        public EpgCRUDOperationsManager(IEpgRepository epgRepository)
        {
            _epgRepository = epgRepository;
        }

        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperations(BulkUpload bulkUpload, eIngestProfileOverlapPolicy overlapPolicy, eIngestProfileAutofillPolicy autofillPolicy)
        {
            var resultsDictionary = bulkUpload.ConstructResultsDictionary();
            var crudOps = new CRUDOperations<EpgProgramBulkUploadObject>();
            foreach (var progChannelGrouping in resultsDictionary)
            {
                var channelId = progChannelGrouping.Key;
                var programsOfChannelToIngest = resultsDictionary[channelId].Values.Select(r => r.Object as EpgProgramBulkUploadObject).ToList();

                var overlapsInIngestSource = EpgBL.Utils.GetOverlappingPrograms(programsOfChannelToIngest);
                var isValid = ValidateSourceInputOverlaps(overlapsInIngestSource, resultsDictionary);
                if (!isValid)
                {
                    var msg = $"found overlapping programs ins source file, will not ingest";
                    _logger.Error($"CalculateCRUDOperations bulkUploadId:[{bulkUpload.Id}] > {msg}");
                    return null;
                }

                var channelCRUDOperations = CalculateCRUDOperationsForChannel(bulkUpload, channelId, overlapPolicy, autofillPolicy, programsOfChannelToIngest);

                crudOps.AddRange(channelCRUDOperations);
            }

            if (bulkUpload.Results.Any(r => r.Errors?.Any() == true))
            {
                return null;
            }

            return crudOps;
        }

        
        
        
        private CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperationsForChannel(BulkUpload bulkUpload, int channelId, eIngestProfileOverlapPolicy overlapPolicy, eIngestProfileAutofillPolicy autofillPolicy, List<EpgProgramBulkUploadObject> programsToIngest)
        {
            var channelCRUDOperations = GetBasicCRUDOperations(bulkUpload.GroupId, channelId, programsToIngest);
            
            var isOverlapValid = OverlapManager.GetOverlapManagerByPolicy(overlapPolicy, bulkUpload, channelCRUDOperations).HandleOverlaps();
            if (!isOverlapValid) { _logger.Error($"overlaps are not valid for channel:[{channelId}] bulkUploadId: [{bulkUpload.Id}]"); }
            
            var isGapsValid = EpgGapManager.GetGapManagerByPolicy(autofillPolicy, bulkUpload, channelCRUDOperations).HandleGaps();
            if (!isGapsValid) { _logger.Error($"gaps are not valid for channel:[{channelId}] bulkUploadId: [{bulkUpload.Id}]"); }

            return channelCRUDOperations;
        }

        /// <summary>
        /// DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!! DANGER !!!
        /// TO THE BRAVE SOULS WHO WALK THIS PATH OF MAINTAIN EPG V2 AND GOT THIS FAR - I SOLUTE YOU!
        /// This is the main logic method of EPG v2, countless hours, weekends and tears were spent of refactoring and re-writing it
        /// If you got this far please carefully consider your steps, every code line has a meaning here.
        /// I will try to explain to thee the treturous paths of BASIC CRUD calculation now:
        /// This method handles Basic CRUD Calculation without considering policy for overlaps or for gaps
        /// ============================================================================================================================
        /// 1. It will programsToIngest as a "chunk" of schedule to ve overwritten
        /// 2. Then it will set all existing data to be deleted in the date range of the "chunk"
        /// 3. Then it will loop over all programsToIngest and identify Updates vs Adds
        /// 4. NOTE! in case of update we set the old item to be deleted, and the new updated item to be updated
        /// 5. We also will add "remainingItems" with the +-1 day of existing programs because they will be used for Overlap\ Gap policy that cross days
        /// </summary>
        /// <returns>hopefully the basic CRUD operations to be done over the current EPG, without overlap\gap policy consideration</returns>
        private CRUDOperations<EpgProgramBulkUploadObject> GetBasicCRUDOperations(int groupId, int channelId, List<EpgProgramBulkUploadObject> programsToIngest)
        {
            var crudOps = new CRUDOperations<EpgProgramBulkUploadObject>();
            
            // NOTE! this method will no handle updates that happen more than +- 1 days from the ingest range
            // if an update was sent to a program that exists that far it will just calculate it as a program to add 
            var currentEpgStartRange = programsToIngest.Min(p => p.StartDate.Date).AddDays(-1).StartOfDay();
            var currentEpgEndRange = programsToIngest.Max(p => p.EndDate.Date).AddDays(1).EndOfDay();
            var currentEpgData = _epgRepository.GetCurrentProgramsByDate(groupId, channelId, currentEpgStartRange, currentEpgEndRange);

            var ingestStart = programsToIngest.Min(p => p.StartDate);
            var ingestEnd = programsToIngest.Max(p => p.EndDate);

            // All programs in ingest range are set to be deleted, yes even programs to update, we will decide if we want to update them later
            crudOps.ItemsToDelete = currentEpgData.Where(p => p.StartDate >= ingestStart && p.EndDate <= ingestEnd).ToList();
            
            // we also need to add all auto-fill programs to be deleted as they are not relevant for the calculation of CRUD..
            // later we will re-calculate auto-fill according to the relevant GAP policy 
            var allAutoFills = currentEpgData.Where(p => p.IsAutoFill);
            crudOps.ItemsToDelete.AddRange(allAutoFills);
            
            // here we have to exclude the autofill programs as they will later be re-calculated
            var currentEpgProgramsDict = currentEpgData.Where(p => !p.IsAutoFill).ToDictionary(p => p.EpgExternalId);
            foreach (var programToIngest in programsToIngest)
            {
                // if a program exists both on newly ingested epgs and in index - it's an update
                if (currentEpgProgramsDict.ContainsKey(programToIngest.EpgExternalId))
                {
                    // update the epg id of the ingested programs with their existing epg id from CB
                    var oldProgram = currentEpgProgramsDict[programToIngest.EpgExternalId];
                    programToIngest.EpgId = oldProgram.EpgId;
                    crudOps.ItemsToUpdate.Add(programToIngest);
                    if (!crudOps.ItemsToDelete.Contains(oldProgram))
                    {
                        crudOps.ItemsToDelete.Add(oldProgram);
                    }
                }
                else
                {
                    // if it exists only on newly ingested epgs and not in index, it's a program to add
                    crudOps.ItemsToAdd.Add(programToIngest);
                }

                // remove the program that we handled so at the end we keep all remaining items, this will allow us to calculate overlaps
                currentEpgProgramsDict.Remove(programToIngest.EpgExternalId);
            }

            crudOps.RemainingItems = currentEpgProgramsDict.Values.Except(crudOps.ItemsToDelete).ToList();
            _logger.Debug($"CalculateCRUDOperations > channel:[{channelId}], ingestStart:[{ingestStart}], ingestEnd:[{ingestEnd}] crud operations:[{crudOps}]");

            return crudOps;
        }

        private bool ValidateSourceInputOverlaps(List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlapsInIngestSource, Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> resultsDictionary)
        {
            if (overlapsInIngestSource.Any())
            {
                overlapsInIngestSource.ForEach(p =>
                {
                    var errorMessage = $"Program to ingets {p.Item1.EpgExternalId} is overlapping another programs to ingest {p.Item2.EpgExternalId}";
                    resultsDictionary[p.Item1.ChannelId][p.Item1.EpgExternalId].AddError(eResponseStatus.Error, errorMessage);
                    resultsDictionary[p.Item2.ChannelId][p.Item2.EpgExternalId].AddError(eResponseStatus.Error, errorMessage);
                });
                return false;
            }

            return true;
        }
    }
}