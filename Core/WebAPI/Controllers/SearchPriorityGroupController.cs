using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog.SearchPriorityGroup;
using WebAPI.Models.Catalog.SearchPriorityGroup.Validators;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Controllers
{
    [Service("searchPriorityGroup")]
    public class SearchPriorityGroupController : IKalturaController
    {
        /// <summary>
        /// Add a new priority group.
        /// </summary>
        /// <param name="searchPriorityGroup">Search priority group.</param>
        /// <returns>Created KalturaSearchPriorityGroup.</returns>
        /// <remarks>Possible status codes: ArgumentCannotBeEmpty = 50027, DuplicateLanguageSent = 500069, DefaultLanguageMustBeSent = 500071, GroupDoesNotContainLanguage = 500072, GlobalLanguageParameterMustBeAsterisk = 500073.</remarks>
        [Action("add")]
        [ApiAuthorize]
        public static KalturaSearchPriorityGroup Add(KalturaSearchPriorityGroup searchPriorityGroup)
        {
            var validator = new KalturaSearchPriorityGroupValidator();
            validator.ValidateToAdd(searchPriorityGroup, nameof(searchPriorityGroup));

            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();

            var response = ClientsManager.CatalogClient().AddSearchPriorityGroup(groupId, searchPriorityGroup, userId);

            return response;
        }

        /// <summary>
        /// Update an existing priority group.
        /// </summary>
        /// <param name="id">Identifier of search priority group.</param>
        /// <param name="searchPriorityGroup">Search priority group.</param>
        /// <returns>Updated KalturaSearchPriorityGroup.</returns>
        /// <remarks>Possible status codes: SearchPriorityGroupDoesNotExist = 4115, ArgumentCannotBeEmpty = 50027, DuplicateLanguageSent = 500069, DefaultLanguageMustBeSent = 500071, GroupDoesNotContainLanguage = 500072, GlobalLanguageParameterMustBeAsterisk = 500073.</remarks>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SearchPriorityGroupDoesNotExist)]
        public static KalturaSearchPriorityGroup Update(long id, KalturaSearchPriorityGroup searchPriorityGroup)
        {
            var validator = new KalturaSearchPriorityGroupValidator();
            validator.ValidateToUpdate(searchPriorityGroup, nameof(searchPriorityGroup));

            var groupId = KS.GetFromRequest().GroupId;
            searchPriorityGroup.Id = id;

            var response = ClientsManager.CatalogClient().UpdateSearchPriorityGroup(groupId, searchPriorityGroup);

            return response;
        }

        /// <summary>
        /// Delete the existing priority group by its identifier.
        /// </summary>
        /// <param name="id">The identifier of a search priority group.</param>
        /// <returns>true if the priority group has been successfully deleted, false otherwise.</returns>
        /// <remarks>Possible status codes: SearchPriorityGroupDoesNotExist = 4115.</remarks>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SearchPriorityGroupDoesNotExist)]
        public static bool Delete(int id)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();

            var response = ClientsManager.CatalogClient().DeleteSearchPriorityGroup(groupId, id, userId);

            return response;
        }

        /// <summary>
        /// Gets list of search priority groups which meet the filter criteria.
        /// </summary>
        /// <param name="filter">Filter.</param>
        /// <param name="pager">Page size and index.</param>
        /// <returns>List of KalturaSearchPriorityGroup items.</returns>
        /// <remarks>Possible status codes: ArgumentsConflictsEachOther = 500038.</remarks>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SearchPriorityGroupDoesNotExist)]
        public static KalturaSearchPriorityGroupListResponse List(KalturaSearchPriorityGroupFilter filter, KalturaFilterPager pager = null)
        {
            var filterValidator = new KalturaSearchPriorityGroupFilterValidator();
            filterValidator.Validate(filter, nameof(filter));

            if (pager == null)
            {
                pager = new KalturaFilterPager { PageSize = 10 };
            }

            var groupId = KS.GetFromRequest().GroupId;
            var language = Utils.Utils.GetLanguageFromRequest();
            var defaultLanguage = Utils.Utils.GetDefaultLanguage();
            var query = new SearchPriorityGroupQuery(filter.IdEqual, filter.ActiveOnly, (SearchPriorityGroupOrderBy)filter.OrderBy, language, defaultLanguage, pager.GetRealPageIndex(), pager.PageSize.Value);

            var response = ClientsManager.CatalogClient().ListSearchPriorityGroups(groupId, query);

            return response;
        }
    }
}