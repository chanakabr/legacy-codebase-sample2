using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("label")]
    public class LabelController : IKalturaController
    {
        /// <summary>
        /// Create a new label associated with a predefined entity attribute. Currently supports only labels on KalturaMediaFile.
        /// </summary>
        /// <param name="label">KalturaLabel object with defined Value.</param>
        /// <returns>Created KalturaLabel.</returns>
        /// <remarks>Possible status codes: ArgumentCannotBeEmpty = 50027, ArgumentMaxLengthCrossed = 500045, InvalidActionParameter = 500054, LabelAlreadyInUse = 4112.</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.LabelAlreadyInUse)]
        public static KalturaLabel Add(KalturaLabel label)
        {
            MediaFileLabelValidator.Instance.ValidateToAdd(label, nameof(label));

            KalturaLabel response = null;
            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();

                response = ClientsManager.CatalogClient().AddLabel(groupId, label, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Gets list of labels which meet the filter criteria.
        /// </summary>
        /// <param name="filter">Filter.</param>
        /// <param name="pager">Page size and index.</param>
        /// <returns>List of KalturaLabel items.</returns>
        /// <remarks>Possible status codes: InvalidArgument = 50026, ArgumentsConflictsEachOther = 500038, InvalidActionParameter = 500054.</remarks>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaLabelListResponse List(KalturaLabelFilter filter, KalturaFilterPager pager = null)
        {
            var filterValidator = new KalturaLabelFilterValidator();
            filterValidator.Validate(filter, nameof(filter));

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            KalturaLabelListResponse response = null;
            try
            {
                var groupId = KS.GetFromRequest().GroupId;

                var idIn = Utils.Utils.ParseCommaSeparatedValues<long>(filter.IdIn, $"{nameof(filter)}.idIn", checkDuplicate: true, ignoreDefaultValueValidation: true);
                response = ClientsManager.CatalogClient().SearchLabels(groupId, idIn, filter.LabelEqual, filter.LabelStartsWith, filter.EntityAttributeEqual, pager.GetRealPageIndex(), pager.PageSize.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates the existing label with a new value.
        /// </summary>
        /// <param name="id">The identifier of label.</param>
        /// <param name="label">KalturaLabel object with new Value.</param>
        /// <returns>Updated KalturaLabel.</returns>
        /// <remarks>Possible status codes: ArgumentCannotBeEmpty = 50027, ArgumentMaxLengthCrossed = 500045, LabelAlreadyInUse = 4112, LabelDoesNotExist = 4113.</remarks>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.LabelAlreadyInUse)]
        [Throws(eResponseStatus.LabelDoesNotExist)]
        public static KalturaLabel Update(long id, KalturaLabel label)
        {
            label.Id = id;

            MediaFileLabelValidator.Instance.ValidateToUpdate(label, nameof(label));

            KalturaLabel response = null;
            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();

                response = ClientsManager.CatalogClient().UpdateLabel(groupId, label, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Deletes the existing label by its identifier.
        /// </summary>
        /// <param name="id">The identifier of label.</param>
        /// <returns>true if the label has been successfully deleted, false otherwise.</returns>
        /// <remarks>Possible status codes: LabelDoesNotExist = 4113.</remarks>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.LabelDoesNotExist)]
        public static bool Delete(long id)
        {
            var response = false;
            try
            {
                var groupId = KS.GetFromRequest().GroupId;
                var userId = Utils.Utils.GetUserIdFromKs();

                response = ClientsManager.CatalogClient().DeleteLabel(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
