using ApiObjects;
using ApiObjects.Base;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using Core.ConditionalAccess;
using Core.GroupManagers;
using Core.Pricing;
using Core.Pricing.Services;
using DAL;
using DAL.Pricing;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    /// <summary>
    /// Program Asset Group Offer  (PAGO)
    /// </summary>
    public class PagoManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<PagoManager> lazy = new Lazy<PagoManager>(() =>
                            new PagoManager(ProgramAssetGroupOfferRepository.Instance,
                                                  GroupSettingsManager.Instance,
                                                  PriceDetailsManager.Instance,
                                                  Core.Catalog.CatalogManagement.FileManager.Instance,
                                                  Core.Pricing.Module.Instance,
                                                  api.Instance),
                            LazyThreadSafetyMode.PublicationOnly);

        private readonly IPagoRepository _repository;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IPriceDetailsManager _priceDetailsManager;
        private readonly IMediaFileTypeManager _fileManager;
        private readonly IPagoModule _pagoModule;
        private readonly IVirtualAssetManager _virtualAssetManager;
        private static IProgramAssetGroupOfferCrudMessageService _messageService;

        public static PagoManager Instance => lazy.Value;

        public PagoManager(IPagoRepository repository,
                          IGroupSettingsManager groupSettingsManager,
                          IPriceDetailsManager priceDetailsManager,
                          IMediaFileTypeManager fileManager,
                          IPagoModule pagoModule,
                          IVirtualAssetManager virtualAssetManager)
        {
            _repository = repository;
            _groupSettingsManager = groupSettingsManager;
            _priceDetailsManager = priceDetailsManager;
            _fileManager = fileManager;
            _pagoModule = pagoModule;
            _virtualAssetManager = virtualAssetManager;
        }

        public GenericResponse<ProgramAssetGroupOffer> Add(ContextData contextData, ProgramAssetGroupOffer pagoToInsert)
        {
            var response = ValidateAdd(contextData, pagoToInsert);
            if (!response.IsOkStatusCode())
            {
                return response;
            }

            // default active = true
            if (pagoToInsert.IsActive == null)
            {
                pagoToInsert.IsActive = true;
            }

            pagoToInsert.LastUpdaterId = contextData.UserId.Value;

            long id = _repository.AddPago(contextData.GroupId, pagoToInsert);
            if (id == 0)
            {
                log.Error($"Error while Insert ProgramAssetGroupOffer. contextData: {contextData.ToString()}.");
                return response;
            }

            _pagoModule.InvalidateProgramAssetGroupOffer(contextData.GroupId);

            // Add VirtualAssetInfo for new ProgramAssetGroupOffer
            var virtualAssetInfo = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.PAGO,
                Id = id,
                Name = pagoToInsert.Name.Values.First(),
                UserId = contextData.UserId.Value,
                StartDate = pagoToInsert.StartDate,
                EndDate = pagoToInsert.EndDate,
                IsActive = pagoToInsert.IsActive,
                Description = pagoToInsert.Description?.Count > 0 ? pagoToInsert.Description.Values.First() : string.Empty
            };

            var virtualAssetInfoResponse = _virtualAssetManager.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);
            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
            {
                log.Error($"Error while AddVirtualAsset - ProgramAssetGroupOfferId: {id} will delete ");
                Delete(contextData, id);
                return response;
            }

            pagoToInsert.Id = id;
            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.OK && virtualAssetInfoResponse.AssetId > 0)
            {
                _repository.UpdatePagoVirtualAssetId(contextData.GroupId, id, virtualAssetInfoResponse.AssetId);
            }

            response.Object = GetProgramAssetGroupOffer(contextData.GroupId, id, true); ;
            response.Status.Set(eResponseStatus.OK);
            _messageService?.PublishCreateEventAsync(contextData.GroupId, response.Object).GetAwaiter().GetResult();

            return response;
        }

        private GenericResponse<ProgramAssetGroupOffer> ValidateAdd(ContextData contextData, ProgramAssetGroupOffer pagoToInsert)
        {
            GenericResponse<ProgramAssetGroupOffer> response = new GenericResponse<ProgramAssetGroupOffer>();
            response.Status.Set(eResponseStatus.OK);
            Status status;

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            #region validate ExternalId - must be unique
            if (!string.IsNullOrEmpty(pagoToInsert.ExternalId))
            {
                status = ValidateExternalId(contextData.GroupId, pagoToInsert.ExternalId);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ExternalId

            #region validate ExternalOfferId - must be unique

            status = ValidateExternalOfferId(contextData.GroupId, pagoToInsert.ExternalOfferId);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }

            #endregion validate ExternalOfferId

            #region validate FileTypesIds
            if (pagoToInsert.FileTypeIds?.Count > 0)
            {
                status = ValidateFileTypesIds(contextData.GroupId, pagoToInsert.FileTypeIds);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate FileTypesIds

            #region validate PriceDetailsId
            if (pagoToInsert.PriceDetailsId.HasValue)
            {
                status = ValidatePriceDetails(contextData.GroupId, pagoToInsert.PriceDetailsId.Value);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate PriceDetailsId

            #region Validate Dates
            status = ValidateDates(pagoToInsert.StartDate, pagoToInsert.EndDate, pagoToInsert.ExpiryDate);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion

            return response;
        }

        public GenericResponse<ProgramAssetGroupOffer> Update(ContextData contextData, ProgramAssetGroupOffer pagoToUpdate, bool isAllowedToViewInactiveAssets)
        {
            ProgramAssetGroupOffer pago = GetProgramAssetGroupOffer(contextData.GroupId, pagoToUpdate.Id, isAllowedToViewInactiveAssets);
            GenericResponse<ProgramAssetGroupOffer> response = ValidateUpdate(pago, contextData, pagoToUpdate);
            if (!response.IsOkStatusCode())
            {
                return response;
            }

            VirtualAssetInfo virtualAssetInfo = null;
            if (pagoToUpdate.Name != null)
            {
                if (pagoToUpdate.Name != null && pagoToUpdate.Name.Values.First() != pago.Name.Values.First())
                {
                    virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.PAGO,
                        Id = pagoToUpdate.Id,
                        Name = pagoToUpdate.Name.Values.First(),
                        UserId = contextData.UserId.Value
                    };
                }
            }

            // Due to atomic action update virtual asset before pago update
            if (virtualAssetInfo != null)
            {
                var virtualAssetInfoResponse = _virtualAssetManager.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while update pago's virtualAsset. groupId: {contextData.GroupId}, programAssetGroupOfferId: {pagoToUpdate.Id}, Name: {pagoToUpdate.Name.Values.First()} ");
                    if (virtualAssetInfoResponse.ResponseStatus != null)
                    {
                        response.SetStatus(virtualAssetInfoResponse.ResponseStatus);
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.Error, "Error while updating pago.");
                    }

                    return response;
                }
                else
                {
                    if (virtualAssetInfoResponse.AssetId > 0 && virtualAssetInfoResponse.AssetId != pago.VirtualAssetId)
                    {
                        pagoToUpdate.VirtualAssetId = virtualAssetInfoResponse.AssetId;
                    }
                }
            }

            pagoToUpdate.IsActive = !pagoToUpdate.IsActive.HasValue ? pago.IsActive : pagoToUpdate.IsActive;
            pagoToUpdate.LastUpdaterId = contextData.UserId.Value;

            bool success = _repository.UpdatePago(contextData.GroupId, pagoToUpdate);
            if (success)
            {
                _pagoModule.InvalidateProgramAssetGroupOffer(contextData.GroupId, pagoToUpdate.Id);
                response.Object = GetProgramAssetGroupOffer(contextData.GroupId, pagoToUpdate.Id, true);
                response.Status.Set(eResponseStatus.OK);
                _messageService?.PublishUpdateEventAsync(contextData.GroupId, response.Object).GetAwaiter().GetResult();
            }

            return response;
        }

        private GenericResponse<ProgramAssetGroupOffer> ValidateUpdate(ProgramAssetGroupOffer pago, ContextData contextData, ProgramAssetGroupOffer pagoToUpdate)
        {
            GenericResponse<ProgramAssetGroupOffer> response = new GenericResponse<ProgramAssetGroupOffer>();
            response.Status.Set(eResponseStatus.OK);
            Status status;
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            if (pago == null)
            {
                response.SetStatus(eResponseStatus.ProgramAssetGroupOfferDoesNotExist, $"ProgramAssetGroupOffer {pagoToUpdate.Id} does not exist");
                return response;
            }

            #region validate ExternalId - must be unique
            if (!string.IsNullOrEmpty(pagoToUpdate.ExternalId) && pagoToUpdate.ExternalId != pago.ExternalId)
            {
                status = ValidateExternalId(contextData.GroupId, pagoToUpdate.ExternalId, pagoToUpdate.Id);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ExternalId

            #region validate ExternalOfferId - must be unique
            if (!string.IsNullOrEmpty(pagoToUpdate.ExternalOfferId) && pagoToUpdate.ExternalOfferId != pago.ExternalOfferId)
            {
                status = ValidateExternalOfferId(contextData.GroupId, pagoToUpdate.ExternalOfferId, pagoToUpdate.Id);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ExternalOfferId

            #region validate FileTypesIds
            if (pagoToUpdate.FileTypeIds != null)
            {
                status = ValidateFileTypesForUpdate(contextData.GroupId, pagoToUpdate.FileTypeIds, pago.FileTypeIds);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate FileTypesIds

            #region validate PriceDetailsId
            if (pagoToUpdate.PriceDetailsId.HasValue)
            {
                if (!pago.PriceDetailsId.HasValue || pago.PriceDetailsId != pagoToUpdate.PriceDetailsId)
                {
                    status = ValidatePriceDetails(contextData.GroupId, pagoToUpdate.PriceDetailsId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
            }
            #endregion validate PriceDetailsId

            if (pagoToUpdate.Name != null)
            {
                status = ValidateNamesForUpdate(pagoToUpdate.Name);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            var nullableEndDate = new DAL.NullableObj<DateTime?>(pagoToUpdate.EndDate, pagoToUpdate.IsNullablePropertyExists("EndDate"));
            var nullableStartDate = new DAL.NullableObj<DateTime?>(pagoToUpdate.StartDate, pagoToUpdate.IsNullablePropertyExists("StartDate"));
            var nullableExpiryDate = new DAL.NullableObj<DateTime?>(pagoToUpdate.ExpiryDate, pagoToUpdate.IsNullablePropertyExists("ExpiryDate"));

            #region validate dates
            status = ValidateDates(nullableEndDate, nullableStartDate, nullableExpiryDate, pago);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion


            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                result.Set(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return result;
            }

            if (!_repository.IsPagoExists(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.ProgramAssetGroupOfferDoesNotExist, $"ProgramAssetGroupOffer {id} does not exist");
                return result;
            }

            // Due to atomic action delete virtual asset before ProgramAssetGroupOffer delete
            // Delete the virtual asset
            var vai = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.PAGO,
                Id = id,
                UserId = contextData.UserId.Value
            };

            var response = _virtualAssetManager.DeleteVirtualAsset(contextData.GroupId, vai);
            if (response.Status == VirtualAssetInfoStatus.Error)
            {
                log.Warn($"Error while delete ProgramAssetGroupOffer virtual asset id {vai}");
                result.Set(eResponseStatus.Error, $"Failed to delete ProgramAssetGroupOffer {id}");
                return result;
            }

            if (_repository.DeletePago(contextData.GroupId, id))
            {
                _pagoModule.InvalidateProgramAssetGroupOffer(contextData.GroupId, (int)id);
                _messageService?.PublishDeleteEventAsync(contextData.GroupId, id).GetAwaiter().GetResult();
                result.Set(eResponseStatus.OK);
            }
            else
            {
                log.Warn($"Error while delete ProgramAssetGroupOffer {id}");
                result.Set(eResponseStatus.Error, $"Failed to delete ProgramAssetGroupOffer {id}");
            }

            return result;
        }

        private Status ValidateDates(DAL.NullableObj<DateTime?> endDateToUpdate, DAL.NullableObj<DateTime?> startDateToUpdate, DAL.NullableObj<DateTime?> expiryDateToUpdate,
            ProgramAssetGroupOffer pago)
        {
            DateTime? startDate = startDateToUpdate.Obj.HasValue ? startDateToUpdate.Obj.Value : pago.StartDate;
            DateTime? endDate = endDateToUpdate.Obj.HasValue ? endDateToUpdate.Obj.Value : pago.EndDate;
            DateTime? expiryDate = expiryDateToUpdate.Obj.HasValue ? expiryDateToUpdate.Obj.Value : pago.ExpiryDate;
            return ValidateDates(startDate, endDate, expiryDate);
        }

        private static Status ValidateDates(DateTime? startDate, DateTime? endDate, DateTime? expiryDate)
        {
            if (startDate >= endDate)
            {
                return new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, "startDate should be less than endDate");
            }

            if (startDate >= expiryDate)
            {
                return new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, "startDate should be less than expiryDate");
            }

            if (endDate > expiryDate)
            {
                return new Status(eResponseStatus.ConflictedParams, "endDate should be less than or equal to expiryDate");
            }

            return new Status(eResponseStatus.OK);
        }

        private Status ValidateExternalId(int partnerId, string externalId, long pagoId = 0)
        {
            Status status = new Status(eResponseStatus.OK);

            long id = GetPagoByExternalId(partnerId, externalId);


            if (!string.IsNullOrEmpty(externalId) && id > 0 && id != pagoId)
            {
                status.Set(eResponseStatus.ExternalIdAlreadyExists, $"External ID already exists {externalId}");
            }

            return status;
        }

        private long GetPagoByExternalId(int partnerId, string externalId)
        {
            return _repository.GetPagoByExternalId(partnerId, externalId);
        }

        private Status ValidateExternalOfferId(int partnerId, string externaOfferId, long pagoId = 0)
        {
            Status status = new Status(eResponseStatus.OK);

            long id = GetPagoByExternalOfferId(partnerId, externaOfferId);

            if (!string.IsNullOrEmpty(externaOfferId) && id > 0 && id != pagoId)
            {
                status.Set(eResponseStatus.ExternalOfferIdAlreadyExists, $"External offer Id already exists {externaOfferId}");
            }

            return status;
        }

        private long GetPagoByExternalOfferId(int partnerId, string externaOfferId)
        {
            return _repository.GetPagoByExternaOfferlId(partnerId, externaOfferId);
        }

        private Status ValidateFileTypesIds(int partnerId, List<long> fileTypesIds)
        {
            Status status = new Status(eResponseStatus.OK);

            var res = _fileManager.GetMediaFileTypes(partnerId);
            if (res.Objects == null || res.Objects.Count < 0)
            {
                status.Set(eResponseStatus.InvalidFileTypes, $"FileTypes are missing for group");
                return status;
            }

            List<long> groupFileTypeIds = res.Objects.Select(x => x.Id).ToList();

            foreach (var fileTypesId in fileTypesIds)
            {
                if (!groupFileTypeIds.Contains(fileTypesId))
                {
                    status.Set(eResponseStatus.InvalidFileType, $"FileType not valid {fileTypesId}");
                    return status;
                }
            }

            return status;
        }

        private Status ValidateFileTypesForUpdate(int partnerId, List<long> fileTypesIdsToUpdate, List<long> currFileTypeIds)
        {
            Status status = new Status(eResponseStatus.OK);

            if (fileTypesIdsToUpdate.Count == 0)
            {
                return status;
            }
            else
            {
                List<long> idsToValidate = fileTypesIdsToUpdate;
                // compare current fileTypes with updated list
                if (currFileTypeIds?.Count > 0)
                {
                    // check if both lists contain the same items in the same order. in case they ar not, need to update
                    if (!fileTypesIdsToUpdate.SequenceEqual(currFileTypeIds))
                    {
                        // need to validate new channels in the list
                        idsToValidate = fileTypesIdsToUpdate.Except(currFileTypeIds).ToList();
                    }
                }

                status = ValidateFileTypesIds(partnerId, idsToValidate);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }
            }

            return status;
        }

        private Status ValidatePriceDetails(int partnerId, long priceDetailsId)
        {
            Status status = new Status(eResponseStatus.OK);

            var priceDetailsRes = _priceDetailsManager.GetPriceDetailsById(partnerId, priceDetailsId);

            if (!priceDetailsRes.IsOkStatusCode() || priceDetailsRes.Object == null)
            {
                status.Set(eResponseStatus.PriceDetailsDoesNotExist, $"PriceDetails {priceDetailsId} does not exist");
            }

            return status;
        }

        private Status ValidateNamesForUpdate(Dictionary<string, string> names)
        {
            if (names.Count == 0 || string.IsNullOrEmpty(names.Values.First()))
            {
                return new Status(eResponseStatus.NameRequired, "ProgramAssetGroupOffer name cannot be empty");
            }

            return new Status(eResponseStatus.OK);
        }

        public GenericListResponse<ProgramAssetGroupOffer> List(ContextData contextData, List<long> programAssetGroupOfferIds, bool inactiveAssets, string nameContains,
            ProgramAssetGroupOfferOrderBy orderBy, CorePager corePager)
        {
            GenericListResponse<ProgramAssetGroupOffer> response = new GenericListResponse<ProgramAssetGroupOffer>();
            int totalResults = 0;

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            if (programAssetGroupOfferIds == null || programAssetGroupOfferIds.Count == 0)
            {
                programAssetGroupOfferIds = GetProgramAssetGroupOfferIds(contextData.GroupId, inactiveAssets);

                if (programAssetGroupOfferIds == null)
                {
                    return response;
                }
            }

            // getting all pagos
            response.Objects = GetProgramAssetGroupOffers(contextData.GroupId, programAssetGroupOfferIds, inactiveAssets, nameContains);
            if (response.Objects != null)
            {
                totalResults = response.Objects.Count;

                switch (orderBy)
                {
                    case ProgramAssetGroupOfferOrderBy.NameAsc:
                        response.Objects = response.Objects.OrderBy(col => col.Name.Values.First()).ToList();
                        break;
                    case ProgramAssetGroupOfferOrderBy.NameDesc:
                        response.Objects = response.Objects.OrderByDescending(col => col.Name.Values.First()).ToList();
                        break;
                    case ProgramAssetGroupOfferOrderBy.UpdateDateAsc:
                        response.Objects = response.Objects.OrderBy(col => col.UpdateDate).ToList();
                        break;
                    case ProgramAssetGroupOfferOrderBy.UpdateDateDesc:
                        response.Objects = response.Objects.OrderByDescending(col => col.UpdateDate).ToList();
                        break;
                }

                if (response.Objects.Count > 0)
                {
                    int startIndexOnList = corePager.PageIndex * corePager.PageSize;
                    int rangeToGetFromList = (startIndexOnList + corePager.PageSize) > response.Objects.Count ? (response.Objects.Count - startIndexOnList) > 0 ? (response.Objects.Count - startIndexOnList) : 0 : corePager.PageSize;
                    if (rangeToGetFromList > 0)
                    {
                        response.Objects = response.Objects.Skip(startIndexOnList).Take(rangeToGetFromList).ToList();
                    }
                    else
                    {
                        response.Objects.Clear();
                    }
                }

                response.TotalItems = totalResults > response.Objects.Count ? totalResults : response.Objects.Count;
            }


            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public List<long> GetProgramAssetGroupOfferIds(long partnerId, bool inactiveAssets)
        {
            var response = new List<long>();
            var result = new Dictionary<long, bool>();
            try
            {
                var key = GetPagoIdsCacheKey(partnerId);
                if (!LayeredCache.Instance.Get(key, ref result,
                    GetGroupPagoIds, new Dictionary<string, object>() { { "partnerId", partnerId } },
                (int)partnerId, LayeredCacheConfigNames.GET_GROUP_PROGRAM_ASSET_GROUP_OFFERS, new List<string>()
                { LayeredCacheKeys.GetPagoIdsInvalidationKey(partnerId) }))
                {
                    log.ErrorFormat($"GetProgramAssetGroupOfferIds - Failed get data from cache. partnerId: {partnerId}");
                    return response;
                }

                if (result?.Count > 0)
                {
                    response = result.Select(y => y.Key).ToList();
                    if (!inactiveAssets)
                    {
                        response = result.Where(x => x.Value == true).Select(y => y.Key).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetProgramAssetGroupOfferIds for partnerId: {0}, ex: {1}", partnerId, ex);
            }

            return response;
        }

        private string GetPagoIdsCacheKey(long partnerId)
        {
            return $"ProgramAssetGroupOfferIds_V1_{partnerId}";
        }

        private Tuple<Dictionary<long, bool>, bool> GetGroupPagoIds(Dictionary<string, object> funcParams)
        {
            long? partnerId = 0;
            if (funcParams != null && funcParams.Count == 1)
            {
                if (funcParams.ContainsKey("partnerId"))
                {
                    partnerId = funcParams["partnerId"] as long?;
                    if (partnerId == null)
                    {
                        return Tuple.Create(new Dictionary<long, bool>(), false);
                    }
                }
            }

            Dictionary<long, bool> res = _repository.GetAllPagoIds((int)partnerId.Value);
            return Tuple.Create(res, res?.Count > 0);
        }

        public List<ProgramAssetGroupOffer> GetProgramAssetGroupOffers(long partnerId, List<long> programAssetGroupOfferIds, bool getAlsoUnactive = false, string nameContains = null)
        {
            List<ProgramAssetGroupOffer> result = new List<ProgramAssetGroupOffer>();

            if (programAssetGroupOfferIds == null || programAssetGroupOfferIds.Count == 0)
            {
                return result;
            }

            Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

            foreach (long id in programAssetGroupOfferIds)
            {
                string key = LayeredCacheKeys.GetPagoKey(partnerId, id);
                keysToOriginalValueMap.Add(key, id.ToString());
                invalidationKeysMap.Add(key, new List<string>() { LayeredCacheKeys.GetPagoInvalidationKey(partnerId, id) });
            }

            Dictionary<string, ProgramAssetGroupOffer> pagosMap = null;

            if (!LayeredCache.Instance.GetValues(keysToOriginalValueMap,
                                                ref pagosMap,
                                                GetProgramAssetGroupOffers,
                                                new Dictionary<string, object>() {
                                                        { "partnerId", partnerId },
                                                        { "programAssetGroupOfferIds", keysToOriginalValueMap.Values.ToList() }
                                                   },
                                                (int)partnerId,
                                                LayeredCacheConfigNames.GET_PROGRAM_ASSET_GROUP_OFFERS,
                                                invalidationKeysMap))
            {
                log.Warn($"Failed getting ProgramAssetGroupOffers from LayeredCache, partnerId: {partnerId}, pagoIds: {string.Join(",", programAssetGroupOfferIds)}");
                return result;
            }


            if (pagosMap == null)
            {
                return result;
            }
            var tempPagos = pagosMap.Values.AsEnumerable();

            if (!getAlsoUnactive)
            {
                tempPagos = tempPagos.Where((item) => item.IsActive.HasValue && item.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(nameContains))
            {
                tempPagos = tempPagos.Where(item => item.Name.Any() && item.Name.Values.First().IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) > -1);
            }

            result = tempPagos.ToList();
            return result;
        }

        private Tuple<Dictionary<string, ProgramAssetGroupOffer>, bool> GetProgramAssetGroupOffers(Dictionary<string, object> funcParams)
        {
            Dictionary<string, ProgramAssetGroupOffer> programAssetGroupOffers = new Dictionary<string, ProgramAssetGroupOffer>();
            List<string> programAssetGroupOfferIds = null;

            long? partnerId = funcParams["partnerId"] as long?;

            if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
            {
                programAssetGroupOfferIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]);
            }
            else if (funcParams["programAssetGroupOfferIds"] != null)
            {
                programAssetGroupOfferIds = (List<string>)funcParams["programAssetGroupOfferIds"];
            }

            if (programAssetGroupOfferIds?.Count > 0)
            {
                List<ProgramAssetGroupOffer> programAssetGroupOfferList = _repository.GetProgramAssetGroupOffersData((int)partnerId.Value, programAssetGroupOfferIds.Select(s => long.Parse(s)).ToList());
                if (programAssetGroupOfferList?.Count > 0)
                {
                    programAssetGroupOffers = programAssetGroupOfferList.ToDictionary(x => LayeredCacheKeys.GetPagoKey(partnerId.Value, x.Id), y => y);
                }
            }

            return Tuple.Create(programAssetGroupOffers, true);
        }

        public ProgramAssetGroupOffer GetProgramAssetGroupOffer(long partnerId, long pagoId, bool getAlsoInactive = false)
        {
            ProgramAssetGroupOffer pago = null;
            var list = GetProgramAssetGroupOffers(partnerId, new List<long>() { pagoId }, getAlsoInactive);

            if (list?.Count > 0)
            {
                pago = list[0];
            }

            return pago;
        }

        public GenericListResponse<PagoPricesContainer> GetPagoPrices(int groupId, List<long> pagoIds, long userId, string currency)
        {
            GenericListResponse<PagoPricesContainer> response = new GenericListResponse<PagoPricesContainer>();
            response.SetStatus(eResponseStatus.OK);

            if (pagoIds?.Count > 0)
            {
                PriceReason priceReason = PriceReason.UnKnown;

                foreach (var pagoId in pagoIds)
                {
                    ProgramAssetGroupOffer pago = GetProgramAssetGroupOffer(groupId, pagoId);
                    if (pago == null)
                    {
                        response.SetStatus(eResponseStatus.ProgramAssetGroupOfferDoesNotExist, $"ProgramAssetGroupOffer {pagoId} does not exist");
                        return response;
                    }

                    if (!IsPagoAllowed(pago))
                    {
                        return response;
                    }

                    Price price = PriceManager.GetPagoFinalPrice(groupId, userId, ref priceReason, pago, string.Empty, string.Empty, currency);
                    if (price != null)
                    {
                        PagoPricesContainer pagoPricesContainer = new PagoPricesContainer() { PagoId = pagoId, m_oPrice = price, m_PriceReason = priceReason };
                        response.Objects.Add(pagoPricesContainer);
                    }
                    else
                    {
                        log.Warn($"Price not found for pagoId: {pagoId}, groupId: {groupId}");
                    }
                }
            }

            return response;
        }

        public bool IsPagoAllowed(ProgramAssetGroupOffer pago)
        {
            return pago.IsActive.Value && pago.StartDate < DateTime.UtcNow && pago.EndDate > DateTime.UtcNow;
        }

        public static void InitProgramAssetGroupOfferCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _messageService = new ProgramAssetGroupOfferCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                contextProvider);
        }
    }
}