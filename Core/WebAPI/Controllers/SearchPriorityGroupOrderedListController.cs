using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog.SearchPriorityGroup;
using WebAPI.Models.Catalog.SearchPriorityGroup.Validators;

namespace WebAPI.Controllers
{
    [Service("searchPriorityGroupOrderedIdsSet")]
    public class SearchPriorityGroupOrderedListController : IKalturaController
    {
        /// <summary>
        /// Return the current ordering of priority groups for the partner.
        /// </summary>
        /// <returns>Current ordering of priority groups.</returns>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaSearchPriorityGroupOrderedIdsSet Get()
        {
            var groupId = KS.GetFromRequest().GroupId;

            var response = ClientsManager.CatalogClient().GetKalturaSearchPriorityGroupOrderedList(groupId);

            return response;
        }

        /// <summary>
        /// Set the ordering of priority groups for the partner.
        /// </summary>
        /// <param name="orderedList">List with ordered search priority groups.</param>
        /// <returns>Updated ordering of priority groups.</returns>
        /// <remarks>Possible status codes: InvalidArgument = 50026, ArgumentsDuplicate = 500066, MaxArguments = 500088.</remarks>
        [Action("set")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaSearchPriorityGroupOrderedIdsSet Set(KalturaSearchPriorityGroupOrderedIdsSet orderedList)
        {
            var validator = new KalturaSearchPriorityGroupOrderedListValidator();
            validator.Validate(orderedList, nameof(orderedList));

            var groupId = KS.GetFromRequest().GroupId;

            var response = ClientsManager.CatalogClient().SetKalturaSearchPriorityGroupOrderedList(groupId, orderedList);

            return response;
        }
    }
}