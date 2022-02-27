using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("categoryItem")]
    public class CategoryItemController : IKalturaController
    {
        /// <summary>
        /// categoryItem add
        /// </summary>
        /// <param name="objectToAdd">categoryItem details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.ChildCategoryNotExist)]
        [Throws(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory)]
        [Throws(eResponseStatus.CategoryTypeNotExist)]
        [Throws(eResponseStatus.CategoryIsAlreadyAssociatedToVersion)]
        static public KalturaCategoryItem Add(KalturaCategoryItem objectToAdd)
        {
            var contextData = KS.GetContextData();
            objectToAdd.ValidateForAdd();

            Func<CategoryItem, GenericResponse<CategoryItem>> addFunc = (CategoryItem categoryItem) =>
                CategoryItemHandler.Instance.Add(contextData, categoryItem);

            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return response;
        }

        /// <summary>
        /// categoryItem update
        /// </summary>
        /// <param name="id">Category identifier</param>
        /// <param name="objectToUpdate">categoryItem details</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.CategoryNotExist)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.ChildCategoryNotExist)]
        [Throws(eResponseStatus.ParentIdShouldNotPointToItself)]
        [Throws(eResponseStatus.ChildCategoryCannotBeTheCategoryItself)]
        [Throws(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [Throws(eResponseStatus.CategoryIsAlreadyAssociatedToVersion)]
        [Throws(eResponseStatus.StartDateShouldBeLessThanEndDate)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.InvalidMetaType)]
        [Throws(eResponseStatus.InvalidValueSentForMeta)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.RelatedEntitiesExceedLimitation)]
        [Throws(eResponseStatus.DeviceRuleDoesNotExistForGroup)]
        [Throws(eResponseStatus.GeoBlockRuleDoesNotExistForGroup)]
        [Throws(eResponseStatus.AssetExternalIdMustBeUnique)]
        static public KalturaCategoryItem Update(long id, KalturaCategoryItem objectToUpdate)
        {
            objectToUpdate.ValidateForUpdate();
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            Func<CategoryItem, GenericResponse<CategoryItem>> addFunc = (CategoryItem categoryItem) =>
                CategoryItemHandler.Instance.Update(contextData, categoryItem);

            var response = ClientUtils.GetResponseFromWS(objectToUpdate, addFunc);
            return response;
        }

        /// <summary>
        /// Remove category
        /// </summary>
        /// <param name="id">Category identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.CategoryNotExist)]
        [Throws(eResponseStatus.ImageDoesNotExist)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [Throws(eResponseStatus.CategoryItemIsRoot)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => CategoryItemHandler.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Gets all categoryItem items
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CategoryTypeNotExist)]
        [Throws(eResponseStatus.InvalidValue)]
        [Throws(eResponseStatus.CategoryNotExist)]
        static public KalturaCategoryItemListResponse List(KalturaCategoryItemFilter filter = null, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            if (filter != null)
            {
                filter.Validate();
            }
            else
            {
                filter = new KalturaCategoryItemFilter();
            }

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaCategoryItem> result;
            switch (filter)
            {
                case KalturaCategoryItemAncestorsFilter f: result = ListByCategoryItemAncestorsFilter(contextData, corePager, f); break;
                case KalturaCategoryItemByIdInFilter f: result = ListByCategoryItemByIdInFilter(contextData, corePager, f); break;
                case KalturaCategoryItemSearchFilter f: result = ListByCategoryItemSearchFilter(contextData, corePager, f); break;
                case KalturaCategoryItemFilter f: result = ListByCategoryItemSearchFilter(contextData, corePager, new KalturaCategoryItemSearchFilter()); break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }

            var response = new KalturaCategoryItemListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaCategoryItem> ListByCategoryItemSearchFilter(ContextData contextData, CorePager pager, KalturaCategoryItemSearchFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemSearchFilter>(filter);

            Func<GenericListResponse<CategoryItem>> listFunc = () =>
                CategoryItemHandler.Instance.List(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCategoryItem> response =
               ClientUtils.GetResponseListFromWS<KalturaCategoryItem, CategoryItem>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaCategoryItem> ListByCategoryItemAncestorsFilter(ContextData contextData, CorePager pager, KalturaCategoryItemAncestorsFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemAncestorsFilter>(filter);

            Func<GenericListResponse<CategoryItem>> listFunc = () =>
                CategoryItemHandler.Instance.List(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCategoryItem> response =
               ClientUtils.GetResponseListFromWS<KalturaCategoryItem, CategoryItem>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaCategoryItem> ListByCategoryItemByIdInFilter(ContextData contextData, CorePager pager, KalturaCategoryItemByIdInFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemByIdInFilter>(filter);

            Func<GenericListResponse<CategoryItem>> listFunc = () =>
                CategoryItemHandler.Instance.List(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCategoryItem> response =
               ClientUtils.GetResponseListFromWS<KalturaCategoryItem, CategoryItem>(listFunc);

            return response;
        }
    }
}