using ApiObjects.Response;
using KLogMonitor;
using System;
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
    [RoutePrefix("_service/meta/action")]
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
        public KalturaMetaListResponse ListOldStandard(KalturaMetaFilter filter = null)
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
        public KalturaMeta UpdateOldStandard(string id, KalturaMeta meta)
        {
            KalturaMeta response = null;
            meta.Id = id;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
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
        public KalturaMetaListResponse List(KalturaMetaFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaMetaFilter();
            }
            else
            {
                filter.Validate();
            }

            KalturaMetaListResponse response = new KalturaMetaListResponse();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (!string.IsNullOrEmpty(filter.AssetStructIdsContains))
                {
                    response = ClientsManager.CatalogClient().GetMetas(groupId, filter.GetAssetStructIdContains(), filter.TypeEqual, filter.OrderBy, true);
                }
                else
                {
                    response = ClientsManager.CatalogClient().GetMetas(groupId, filter.GetIdIn(), filter.TypeEqual, filter.OrderBy);
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
        [Throws(eResponseStatus.MetaNameAlreadyInUse)]
        [Throws(eResponseStatus.MetaSystemNameAlreadyInUse)]        
        public KalturaMeta Add(KalturaMeta meta)
        {
            KalturaMeta response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (meta.Name == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "name");
            }

            if (string.IsNullOrEmpty(meta.SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "systemName");
            }

            try
            {
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
        [Throws(eResponseStatus.MetaNameAlreadyInUse)]
        [Throws(eResponseStatus.MetaSystemNameAlreadyInUse)]        
        [Throws(eResponseStatus.CanNotChangePredefinedMetaSystemName)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaMeta Update(long id, KalturaMeta meta)
        {
            KalturaMeta response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

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
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
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