using ApiObjects;
using ApiObjects.Response;
using ApiObjects.User;
using Core.GroupManagers;
using System;
using WebAPI.ClientManagers;
using WebAPI.Clients;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;

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

            var result = ClientUtils.GetResponseFromWS<KalturaPartner, Partner>(addPartnerFunc);
            Func<Group, Status> addGroupFunc = (Group group) => GroupsManager.Instance.AddBaseConfiguration(result.Id.Value, group);
            ClientUtils.GetResponseStatusFromWS(addGroupFunc, partnerSetup.BasePartnerConfiguration);

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
            var response = ClientUtils.GetResponseListFromWS<KalturaPartner, Partner>(() =>
                PartnerManager.Instance.GetPartners(filter?.GetIdIn()));

            var result = new KalturaPartnerListResponse {Partners = response.Objects, TotalCount = response.TotalCount};
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

            return ClientUtils.GetResponseStatusFromWS(deleteGroupFunc) && ClientUtils.GetResponseStatusFromWS(delete);
        }
    }
}
