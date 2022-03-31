using System;
using WebAPI.Managers.Scheme;
using WebAPI.Models.AssetPersonalMarkup;

namespace WebAPI.Controllers
{
    // this is a workaround to generate documentation and client libs
    // should be removed, when we'll find solution to update KalturaClient.xml with non-Phoenix endpoints
    [Service("assetPersonalMarkup")]
    public class AssetPersonalMarkupController : IKalturaController
    {
        /// <summary>
        /// Response with list of assetPersonalMarkup.
        /// </summary>
        /// <param name="filter">Filter pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        static public KalturaAssetPersonalMarkupListResponse List(KalturaAssetPersonalMarkupSearchFilter filter)
        {
            throw new NotImplementedException("assetPersonalMarkup.list should be used only by phoenix rest proxy");
        }
    }
}