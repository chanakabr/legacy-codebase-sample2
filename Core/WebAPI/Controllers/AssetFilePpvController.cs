using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
using ApiObjects.Base;
using ApiObjects.User;
using OfficeOpenXml.ConditionalFormatting;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.ModelsValidators;

namespace WebAPI.Controllers
{
    [Service("assetFilePpv")]
    public class AssetFilePpvController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return a list of asset files ppvs for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        public static KalturaAssetFilePpvListResponse List(KalturaAssetFilePpvFilter filter)
        {
            if (filter == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "filter");
            }

            filter.Validate();

            var contextData = KS.GetContextData();
            var response = ClientsManager.PricingClient().GetAssetFilePPVList(contextData,
                filter.AssetIdEqual.GetValueOrDefault(), filter.AssetFileIdEqual.GetValueOrDefault());

            return response;
        }

        /// <summary>
        /// Add asset file ppv
        /// </summary>    
        /// <param name="assetFilePpv">asset file ppv</param>        
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UnKnownPPVModule)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        public static KalturaAssetFilePpv Add(KalturaAssetFilePpv assetFilePpv)
        {
            if (assetFilePpv.AssetFileId < 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "assetFileId");
            }

            if (assetFilePpv.PpvModuleId < 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "ppvModuleId");
            }

            if (assetFilePpv.StartDate.HasValue && assetFilePpv.EndDate.HasValue &&
                assetFilePpv.StartDate >= assetFilePpv.EndDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate",
                    "endDate");
            }

            int groupId = KS.GetFromRequest().GroupId;
            long userId = KS.GetFromRequest().UserId.ParseUserId();
            var contextData = new ContextData(groupId) { UserId = userId };

            // call client                
            var response = ClientsManager.PricingClient().AddAssetFilePpv(contextData, assetFilePpv);
            return response;
        }

        /// <summary>
        /// Delete asset file ppv
        /// </summary>
        /// <param name="assetFileId">Asset file id</param>        
        /// <param name="ppvModuleId">Ppv module id</param>        
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFilePPVNotExist)]
        [Throws(eResponseStatus.UnKnownPPVModule)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static bool Delete(long assetFileId, long ppvModuleId)
        {
            bool response = false;

            try
            {
                if (assetFileId <= 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "assetFileId");
                }

                if (ppvModuleId <= 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "ppvModuleId");
                }

                int groupId = KS.GetFromRequest().GroupId;
                long userId = KS.GetFromRequest().UserId.ParseUserId();
                var contextData = new ContextData(groupId) { UserId = userId };
                
                // call client                
                response = ClientsManager.PricingClient().DeleteAssetFilePpv(contextData, assetFileId, ppvModuleId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update assetFilePpv
        /// </summary>    
        /// <param name="assetFileId">Asset file id</param>        
        /// <param name="ppvModuleId">Ppv module id</param>        
        /// <param name="assetFilePpv">assetFilePpv</param>        
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFilePPVNotExist)]
        [Throws(eResponseStatus.UnKnownPPVModule)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaAssetFilePpv Update(long assetFileId, long ppvModuleId, KalturaAssetFilePpv assetFilePpv)
        {
            KalturaAssetFilePpv response = null;

            try
            {
                if (assetFileId <= 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "assetFileId");
                }

                if (ppvModuleId <= 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "ppvModuleId");
                }

                assetFilePpv.AssetFileId = assetFileId;
                assetFilePpv.PpvModuleId = ppvModuleId;

                int groupId = KS.GetFromRequest().GroupId;
                long userId = KS.GetFromRequest().UserId.ParseUserId();
                var contextData = new ContextData(groupId) { UserId = userId };
                
                // call client                
                response = ClientsManager.PricingClient().UpdateAssetFilePpv(contextData, assetFilePpv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}