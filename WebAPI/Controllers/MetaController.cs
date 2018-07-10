using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("meta")]
    public class MetaController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get the list of meta mappings for the partner
        /// </summary>
        /// <param name="filter">Meta filter</param>
        /// <remarks></remarks>
        [Route("listOldStandard"), HttpPost]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaMetaListResponse ListOldStandard(KalturaMetaFilter filter = null)
        {
            KalturaMetaListResponse response = null;

            if (filter == null)
            {
                filter = new KalturaMetaFilter();
            }

            filter.OldValidate();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetGroupMeta(groupId, filter.AssetTypeEqual, filter.TypeEqual, filter.FieldNameEqual, filter.FieldNameNotEqual, filter.GetFeaturesIn());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update meta's user interest
        /// </summary>
        /// <param name="id">Meta identifier</param>           
        /// <param name="meta">Meta</param>           
        /// <returns></returns>
        /// <remarks>
        /// Possible status codes: 
        /// NoMetaToUpdate, NameRequired, NotaTopicInterestMeta, ParentDuplicateAssociation, MetaNotAUserinterest, ParentIdShouldNotPointToItself, ParentIdNotAUserInterest,
        /// ParentAssetTypeDiffrentFromMeta, MetaNotFound, MetaNotBelongtoPartner, WrongMetaName, ParentParnerDiffrentFromMetaPartner
        /// </remarks>
        [Route("updateOldStandard"), HttpPost]
        [OldStandardAction("update")]
        [ApiAuthorize]
        [Obsolete]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.NoMetaToUpdate)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.NotaTopicInterestMeta)]
        [Throws(eResponseStatus.ParentDuplicateAssociation)]
        [Throws(eResponseStatus.MetaNotAUserinterest)]
        [Throws(eResponseStatus.ParentIdShouldNotPointToItself)]
        [Throws(eResponseStatus.ParentIdNotAUserInterest)]
        [Throws(eResponseStatus.ParentAssetTypeDiffrentFromMeta)]
        [Throws(eResponseStatus.MetaNotFound)]
        [Throws(eResponseStatus.MetaNotBelongtoPartner)]
        [Throws(eResponseStatus.ParentAssetTypeDiffrentFromMeta)]
        [Throws(eResponseStatus.ParentParnerDiffrentFromMetaPartner)]
        static public KalturaMeta UpdateOldStandard(string id, KalturaMeta meta)
        {
            KalturaMeta response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                meta.Id = id;
                if (meta.Name != null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMeta.Name");
                }

                // call client
                response = ClientsManager.CatalogClient().UpdateGroupMeta(groupId, meta);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return a list of metas for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        static public KalturaMetaListResponse List(KalturaMetaFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaMetaFilter();
            }

            KalturaMetaListResponse response = new KalturaMetaListResponse();
            int groupId = KS.GetFromRequest().GroupId;
            try
            {
                filter.Validate();
                if (filter.AssetStructIdEqual.HasValue && filter.AssetStructIdEqual.Value > 0)
                {
                    response = ClientsManager.CatalogClient().GetMetas(groupId, new List<long>(), filter.DataTypeEqual, filter.OrderBy, filter.MultipleValueEqual, filter.AssetStructIdEqual.Value);
                }
                else
                {
                    response = ClientsManager.CatalogClient().GetMetas(groupId, filter.GetIdIn(), filter.DataTypeEqual, filter.OrderBy, filter.MultipleValueEqual);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add a new meta
        /// </summary>
        /// <param name="meta">Meta Object</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.MetaSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.InvalidMutlipleValueForMetaType)]
        static public KalturaMeta Add(KalturaMeta meta)
        {
            KalturaMeta response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            meta.ValidateFeatures();
            if (meta.Name == null || meta.Name.Values == null || meta.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            meta.Name.Validate("multilingualName");
            if (string.IsNullOrEmpty(meta.SystemName.Trim()))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (!meta.DataType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "dataType");
            }

            try
            {
                if (meta.MultipleValue.HasValue && meta.MultipleValue.Value && meta.DataType != Models.Catalog.KalturaMetaDataType.STRING)
                {
                    throw new ClientException((int)eResponseStatus.InvalidMutlipleValueForMetaType, string.Format("{0} - MultipleValue can only be set to true for KalturaMeta.DataType with value STRING",
                                                                                                                eResponseStatus.InvalidMutlipleValueForMetaType.ToString()));
                }

                response = ClientsManager.CatalogClient().AddMeta(groupId, meta, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update an existing meta
        /// </summary>
        /// <param name="id">Meta Identifier</param>
        /// <param name="meta">Meta Object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.MetaSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.CanNotChangePredefinedMetaSystemName)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaMeta Update(long id, KalturaMeta meta)
        {
            KalturaMeta response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            meta.ValidateFeatures();

            if (meta.Name != null)
            {
                if ((meta.Name.Values == null || meta.Name.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    meta.Name.Validate("multilingualName");
                }
            }

            try
            {
                response = ClientsManager.CatalogClient().UpdateMeta(groupId, id, meta, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing meta
        /// </summary>
        /// <param name="id">Meta Identifier</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.CanNotDeletePredefinedMeta)]
        [Throws(eResponseStatus.CanNotDeleteConnectingAssetStructMeta)]
        [SchemeArgument("id", MinLong = 1)]
        static public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteMeta(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}