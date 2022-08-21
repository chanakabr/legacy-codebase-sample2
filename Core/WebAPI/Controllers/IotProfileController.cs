using System;
using WebAPI.Managers.Scheme;

namespace WebAPI.Controllers
{
    [Service("iotProfile")]
    public class IotProfileController : IKalturaController
    {
        /// <summary>
        /// Add new environment in aws
        /// </summary>
        /// <returns>boolean for processing the creation request</returns>
        /// <remarks>
        /// </remarks>
        [Action("add")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static bool Add()
        {
            throw new NotImplementedException("iotProfile.add should be used only by phoenix rest proxy");
        }

        /// <summary>
        /// Delete existing environment in aws
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>boolean for processing the deletion request</returns>
        /// <remarks>
        /// </remarks>
        [Action("delete", isInternal: true)]
        [ApiAuthorize]
        [SchemeArgument("groupId", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static bool Delete(long groupId)
        {
            throw new NotImplementedException("iotProfile.delete should be used only by phoenix rest proxy");
        }
    }
}
