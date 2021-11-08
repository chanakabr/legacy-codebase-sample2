using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using System;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("categoryVersion")]
    public class CategoryVersionController : IKalturaController
    {
        /// <summary>
        /// Acreate new tree for this categoryItem
        /// </summary>
        /// <param name="categoryItemId">the categoryItemId to create the tree accordingly</param>
        /// <param name="name">Name of version</param>
        /// <param name="comment">Comment of version</param>
        /// <returns></returns>
        [Action("createTree")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CategoryIsAlreadyAssociatedToVersionTree)]
        [Throws(eResponseStatus.CategoryIsNotRoot)]
        [Throws(eResponseStatus.CategoryNotExist)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaCategoryVersion CreateTree(long categoryItemId, string name, string comment)
        {
            KalturaCategoryVersion result = null;

            var contextData = KS.GetContextData();

            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                Func<GenericResponse<CategoryVersion>> createTreeFunc = () =>
                    CategoryVersionHandler.Instance.CreateTree(contextData, categoryItemId, name, comment);

                result = ClientUtils.GetResponseFromWS<KalturaCategoryVersion, CategoryVersion>(createTreeFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Set new default category version
        /// </summary>
        /// <param name="id">category version id to set as default</param>
        /// <param name="force">force to set even if version is older then currenct version</param>
        /// <returns></returns>
        [Action("setDefault")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CategoryVersionDoesNotExist)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [Throws(eResponseStatus.CategoryVersionIsOlderThanDefault)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static void SetDefault(long id, bool force = false)
        {
            var contextData = KS.GetContextData();

            try
            {
                Func<Status> setDefaultFunc = () => CategoryVersionHandler.Instance.SetDefault(contextData, id, force);
                ClientUtils.GetResponseStatusFromWS(setDefaultFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// categoryVersion add
        /// </summary>
        /// <param name="objectToAdd">categoryVersion details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CategoryVersionDoesNotExist)]
        [Throws(eResponseStatus.CategoryNotExist)]
        static public KalturaCategoryVersion Add(KalturaCategoryVersion objectToAdd)
        {
            var contextData = KS.GetContextData();
            objectToAdd.ValidateForAdd();

            Func<CategoryVersion, GenericResponse<CategoryVersion>> addFunc = (CategoryVersion bolObject) =>
                CategoryVersionHandler.Instance.Add(contextData, bolObject);

            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return response;
        }

        /// <summary>
        /// categoryVersion update
        /// </summary>
        /// <param name="id">Category version identifier</param>
        /// <param name="objectToUpdate">categoryVersion details</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.CategoryVersionDoesNotExist)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        static public KalturaCategoryVersion Update(long id, KalturaCategoryVersion objectToUpdate)
        {
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            Func<CategoryVersion, GenericResponse<CategoryVersion>> addFunc = (CategoryVersion bolObject) =>
                CategoryVersionHandler.Instance.Update(contextData, bolObject);

            var response = ClientUtils.GetResponseFromWS(objectToUpdate, addFunc);
            return response;
        }

        /// <summary>
        /// Remove category version
        /// </summary>
        /// <param name="id">Category version identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [Throws(eResponseStatus.CategoryVersionDoesNotExist)]
        [Throws(eResponseStatus.CategoryNotExist)]
        [Throws(eResponseStatus.CategoryItemIsRoot)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => CategoryVersionHandler.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Gets all category versions
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CategoryTypeNotExist)]
        [Throws(eResponseStatus.InvalidValue)]
        [Throws(eResponseStatus.CategoryNotExist)]
        static public KalturaCategoryVersionListResponse List(KalturaCategoryVersionFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaCategoryVersion> result;
            switch (filter)
            {
                case KalturaCategoryVersionFilterByTree f: result = ListByCategoryVersionFilterByTree(contextData, corePager, f); break;
                case KalturaCategoryVersionFilter f: result = ListByCategoryVersionFilter(contextData, corePager, f); break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }

            var response = new KalturaCategoryVersionListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaCategoryVersion> ListByCategoryVersionFilter(ContextData contextData, CorePager pager, KalturaCategoryVersionFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryVersionFilter>(filter);

            Func<GenericListResponse<CategoryVersion>> listFunc = () =>
                CategoryCache.Instance.ListCategoryVersionDefaults(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCategoryVersion> response =
               ClientUtils.GetResponseListFromWS<KalturaCategoryVersion, CategoryVersion>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaCategoryVersion> ListByCategoryVersionFilterByTree(ContextData contextData, CorePager pager, KalturaCategoryVersionFilterByTree filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryVersionFilterByTree>(filter);

            Func<GenericListResponse<CategoryVersion>> listFunc = () =>
                CategoryCache.Instance.ListCategoryVersionByTree(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCategoryVersion> response =
               ClientUtils.GetResponseListFromWS<KalturaCategoryVersion, CategoryVersion>(listFunc);

            return response;
        }
    }
}