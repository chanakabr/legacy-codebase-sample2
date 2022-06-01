using System;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.AssetPersonalMarkup;
using WebAPI.Models.AssetSelection;
using WebAPI.Models.Catalog;

namespace WebAPI.Controllers
{
    // this is a workaround to generate documentation and client libs
    // should be removed, when we'll find solution to update KalturaClient.xml with non-Phoenix endpoints
    [Service("assetPersonalSelection")]
    public class AssetPersonalSelectionController : IKalturaController
    {
        /// <summary>
        /// Add or update asset selection in slot
        /// </summary>
        /// <param name="assetId">asset id</param>
        /// <param name="assetType">asset type: media/epg</param>
        /// <param name="slotNumber">slot number</param>
        /// <returns></returns>
        [Action("upsert")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(StatusCode.ArgumentMinValueCrossed)]
        [Throws(StatusCode.EnumValueNotSupported)]
        public static KalturaAssetPersonalSelection Upsert(long assetId, KalturaAssetType assetType, int slotNumber)
        {
            throw new NotImplementedException("assetPersonalSelection.upsert should be used only by phoenix rest proxy");
        }
        
        /// <summary>
        /// Remove asset selection in slot
        /// </summary>
        /// <param name="assetId">asset id</param>
        /// <param name="assetType">asset type: media/epg</param>
        /// <param name="slotNumber">slot number</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(StatusCode.ArgumentMinValueCrossed)]
        public static void Delete(long assetId, KalturaAssetType assetType, int slotNumber)
        {
            throw new NotImplementedException("assetPersonalSelection.delete should be used only by phoenix rest proxy");
        }
        
        /// <summary>
        /// Remove all asset selections for slot
        /// </summary>
        /// <param name="slotNumber">slot number</param>
        /// <returns></returns>
        [Action("deleteAll")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(StatusCode.ArgumentMinValueCrossed)]
        public static void Delete(int slotNumber)
        {
            throw new NotImplementedException("assetPersonalSelection.deleteAll should be used only by phoenix rest proxy");
        }
    }
}