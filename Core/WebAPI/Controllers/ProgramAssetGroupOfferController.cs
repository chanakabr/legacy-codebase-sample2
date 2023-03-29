using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("programAssetGroupOffer")]
    public class ProgramAssetGroupOfferController : IKalturaController
    {
        /// <summary>
        /// Insert new ProgramAssetGroupOffer for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="programAssetGroupOffer">programAssetGroupOffer object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.InvalidFileTypes)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.ExternalOfferIdAlreadyExists)]
        static public KalturaProgramAssetGroupOffer Add(KalturaProgramAssetGroupOffer programAssetGroupOffer)
        {
            KalturaProgramAssetGroupOffer result = null;
            ProgramAssetGroupOfferValidator.ValidateForAdd(programAssetGroupOffer);
            var contextData = KS.GetContextData();

            Func<ProgramAssetGroupOffer, GenericResponse<ProgramAssetGroupOffer>> insertPagoFunc = (ProgramAssetGroupOffer pagoToInsert) =>
                    PagoManager.Instance.Add(contextData, pagoToInsert);

            result = ClientUtils.GetResponseFromWS<KalturaProgramAssetGroupOffer, ProgramAssetGroupOffer>(programAssetGroupOffer, insertPagoFunc);

            return result;
        }

        /// <summary>
        /// Delete programAssetGroupOffer 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">ProgramAssetGroupOffer id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ProgramAssetGroupOfferDoesNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public bool Delete(long id)
        {
            bool result = false;
            var contextData = KS.GetContextData();

            Status delete() => PagoManager.Instance.Delete(contextData, id);
            result = ClientUtils.GetResponseStatusFromWS(delete);

            return result;
        }

        /// <summary>
        /// Update ProgramAssetGroupOffer
        /// </summary>
        /// <param name="id">ProgramAssetGroupOffer id</param>
        /// <param name="programAssetGroupOffer">ProgramAssetGroupOffer</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.InvalidFileTypes)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.StartDateShouldBeLessThanEndDate)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ProgramAssetGroupOfferDoesNotExist)]
        [Throws(eResponseStatus.ExternalOfferIdAlreadyExists)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaProgramAssetGroupOffer Update(long id, KalturaProgramAssetGroupOffer programAssetGroupOffer)
        {
            KalturaProgramAssetGroupOffer result = null;
            ProgramAssetGroupOfferValidator.ValidateForUpdate(programAssetGroupOffer);
            programAssetGroupOffer.Id = id;
            
            var contextData = KS.GetContextData();
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);

            Func<ProgramAssetGroupOffer, GenericResponse<ProgramAssetGroupOffer>> updateFunc = (ProgramAssetGroupOffer pagoToUpdate) =>
              PagoManager.Instance.Update(contextData, pagoToUpdate, isAllowedToViewInactiveAssets);

            result = ClientUtils.GetResponseFromWS<KalturaProgramAssetGroupOffer, ProgramAssetGroupOffer>(programAssetGroupOffer, updateFunc);

            return result;
        }

        /// <summary>
        /// Gets all Program asset group offer 
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaProgramAssetGroupOfferListResponse List(KalturaProgramAssetGroupOfferFilter filter = null, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            if (filter == null)
            {
                filter = new KalturaProgramAssetGroupOfferFilter();
            }

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaProgramAssetGroupOffer> result;
            switch (filter)
            {
                case KalturaProgramAssetGroupOfferIdInFilter f:
                    result = ListByProgramAssetGroupOfferIdInFilter(contextData, corePager, f);
                    break;
                case KalturaProgramAssetGroupOfferFilter f:
                    result = ListByProgramAssetGroupOfferFilter(contextData, corePager, f);
                    break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            };

            var response = new KalturaProgramAssetGroupOfferListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaProgramAssetGroupOffer> ListByProgramAssetGroupOfferFilter(ContextData contextData, CorePager corePager, KalturaProgramAssetGroupOfferFilter filter)
        {
            bool inactiveAssets = false;
            ProgramAssetGroupOfferOrderBy orderBy = AutoMapper.Mapper.Map<ProgramAssetGroupOfferOrderBy>(filter.OrderBy);

            if (filter.AlsoInactive.HasValue)
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);
                inactiveAssets = isAllowedToViewInactiveAssets && filter.AlsoInactive.Value;
            }

            GenericListResponse<ProgramAssetGroupOffer> listFunc() =>
                PagoManager.Instance.List(contextData, null, inactiveAssets, filter.NameContains, orderBy, corePager);

            KalturaGenericListResponse<KalturaProgramAssetGroupOffer> response =
               ClientUtils.GetResponseListFromWS<KalturaProgramAssetGroupOffer, ProgramAssetGroupOffer>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaProgramAssetGroupOffer> ListByProgramAssetGroupOfferIdInFilter(ContextData contextData, CorePager corePager, KalturaProgramAssetGroupOfferIdInFilter filter)
        {
            bool inactiveAssets = false;
            ProgramAssetGroupOfferOrderBy orderBy = AutoMapper.Mapper.Map<ProgramAssetGroupOfferOrderBy>(filter.OrderBy);

            if (filter.AlsoInactive.HasValue)
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);
                inactiveAssets = isAllowedToViewInactiveAssets && filter.AlsoInactive.Value;
            }

            GenericListResponse<ProgramAssetGroupOffer> listFunc() =>
                PagoManager.Instance.List(contextData, ProgramAssetGroupOfferMapper.GetProgramAssetGroupOfferIds(filter), inactiveAssets, filter.NameContains, orderBy, corePager);

            KalturaGenericListResponse<KalturaProgramAssetGroupOffer> response =
               ClientUtils.GetResponseListFromWS<KalturaProgramAssetGroupOffer, ProgramAssetGroupOffer>(listFunc);

            return response;
        }
    }
}