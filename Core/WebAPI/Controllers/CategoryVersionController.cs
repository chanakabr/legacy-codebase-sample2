using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using System;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("categoryVersion")]
    [AddAction(Summary = "categoryVersion add",
        ObjectToAddDescription = "categoryVersion details",
        ClientThrows = new [] {
            eResponseStatus.CategoryVersionDoesNotExist,
            eResponseStatus.CategoryNotExist
        }
     )]

    [UpdateAction(Summary = "categoryVersion update",
        IdDescription = "Category version identifier",
        ObjectToUpdateDescription = "categoryVersion details",
        ClientThrows = new [] {
            eResponseStatus.CategoryVersionDoesNotExist,
            eResponseStatus.CategoryVersionIsNotDraft
        }
    )]

    [DeleteAction(Summary = "Remove category version",
        IdDescription = "Category version identifier",
        ClientThrows = new [] {
            eResponseStatus.CategoryVersionIsNotDraft,
            eResponseStatus.CategoryVersionDoesNotExist,
            eResponseStatus.CategoryNotExist,
            eResponseStatus.CategoryItemIsRoot
        }
    )]

    [ListAction(Summary = "Gets all category versions", IsFilterOptional = false, IsPagerOptional = true)]
    public class  CategoryVersionController : KalturaCrudController<KalturaCategoryVersion, KalturaCategoryVersionListResponse, CategoryVersion, long, KalturaCategoryVersionFilter>
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
    }
}