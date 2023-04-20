using System;
using System.Threading.Tasks;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;

namespace WebAPI.Controllers
{
    [Service("personalActivityCleanup")]
    public class PersonalActivityCleanupController : IKalturaController
    {
        /// <summary>
        /// PersonalActivityCleanupConfiguration get
        /// </summary>
        /// <remarks>
        /// </remarks>
        [Action("getPartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static KalturaPersonalActivityCleanupConfiguration GetPartnerConfiguration()
        {
            throw new NotImplementedException("personalActivityCleanup.getPartnerConfiguration " +
                                              "should be used only by phoenix rest proxy");
        }

        /// <summary>
        /// PersonalActivityCleanupConfiguration Update
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="personalActivityCleanupConfiguration">PersonalActivityCleanupConfiguration details</param>
        [Action("updatePartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static KalturaPersonalActivityCleanupConfiguration UpdatePartnerConfiguration(
            KalturaPersonalActivityCleanupConfiguration personalActivityCleanupConfiguration)
        {
            throw new NotImplementedException("personalActivityCleanup.updatePartnerConfiguration " +
                                              "should be used only by phoenix rest proxy");
        }
    }
}