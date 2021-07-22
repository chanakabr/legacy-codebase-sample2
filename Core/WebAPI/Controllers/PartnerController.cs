using ApiObjects;
using ApiObjects.Response;
using ApiObjects.User;
using Core.GroupManagers;
using System;
using WebAPI.ClientManagers;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("partner")]
    public class PartnerController : IKalturaController
    {
        /// <summary>
        /// Returns a login session for external system (like OVP)
        /// </summary>
        /// <returns></returns>
        [Action("externalLogin")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        [Throws(StatusCode.MissingConfiguration)]
        public static KalturaLoginSession ExternalLogin()
        {
            int groupId = KS.GetFromRequest().GroupId;
            var response = AuthorizationManager.GenerateOvpSession(groupId);
            return response;
        }

        /// <summary>
        /// Add a partner with default user
        /// </summary>
        /// <param name="partner">partner</param>
        /// <param name="partnerSetup">mandatory parameters to create partner</param>
        /// <returns></returns>
        [Action("add")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ApiAuthorize]
        public static KalturaPartner Add(KalturaPartner partner, KalturaPartnerSetup partnerSetup)
        {
            partner.ValidateForAdd();
            partnerSetup.ValidateForAdd();

            var userId = KS.GetFromRequest().UserId.ParseUserId();
            var partnerBol = AutoMapper.Mapper.Map<Partner>(partner);
            var partnerSetupBol = AutoMapper.Mapper.Map<PartnerSetup>(partnerSetup);

            Func<GenericResponse<Partner>> addPartnerFunc = () =>
                PartnerManager.Instance.AddPartner(partnerBol, partnerSetupBol, userId);

            KalturaPartner result = null;
            try
            {
                result = ClientUtils.GetResponseFromWS<KalturaPartner, Partner>(addPartnerFunc);
                Func<Group, Status> addGroupFunc = (Group group) => GroupsManager.Instance.AddBaseConfiguration(result.Id.Value, group);
                ClientUtils.GetResponseStatusFromWS(addGroupFunc, partnerSetup.BasePartnerConfiguration);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return result;
        }

        /// <summary>
        /// Internal API !!! Returns the list of active Partners
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaPartnerListResponse List(KalturaPartnerFilter filter = null)
        {
            var result = new KalturaPartnerListResponse();
            try
            {
                var response = ClientUtils.GetResponseListFromWS<KalturaPartner, Partner>(() =>
                    PartnerManager.Instance.GetPartners(filter?.GetIdIn()));
                result.Partners = response.Objects;
                result.TotalCount = response.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            
            return result;
        }

        /// <summary>
        /// Internal API !!! Delete Partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Partner id</param>
        [Action("delete")]
        [Throws(eResponseStatus.PartnerDoesNotExist)]
        [ApiAuthorize]
        static public bool Delete(int id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteGroupFunc = () => GroupsManager.Instance.DeleteBaseConfiguration(id);
            Func<Status> delete = () => PartnerManager.Instance.Delete(contextData.UserId.Value, id);

            var result = false;
            try
            {
                result = ClientUtils.GetResponseStatusFromWS(deleteGroupFunc) && ClientUtils.GetResponseStatusFromWS(delete);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return result;
        }

        /// <summary>
        /// Internal API !!! create ElasticSearch indexes for partner
        /// </summary>
        /// <returns></returns>
        [Action("createIndexes")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        [Throws(eResponseStatus.PartnerDoesNotExist)]
        public static bool CreateIndexes()
        {
            int groupId = KS.GetFromRequest().GroupId;
            Func<Status> createIndexesFunc = () => PartnerManager.Instance.CreateIndexes(groupId);
            var result = false;
            try
            {
                result = ClientUtils.GetResponseStatusFromWS(createIndexesFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return result;
        }
    }
}
