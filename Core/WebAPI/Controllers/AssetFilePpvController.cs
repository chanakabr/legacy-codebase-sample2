using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

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
        static public KalturaAssetFilePpvListResponse List(KalturaAssetFilePpvFilter filter)
        {
            KalturaAssetFilePpvListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "filter");
            }

            filter.Validate();

            try
            {
                response = ClientsManager.PricingClient().GetAssetFilePPVList(groupId, filter.AssetIdEqual.GetValueOrDefault(), filter.AssetFileIdEqual.GetValueOrDefault());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add asset file ppv
        /// </summary>    
        /// <param name="assetFilePpv">asset file ppv</param>        
        [Action("add")]
        [ApiAuthorize]
        static public KalturaAssetFilePpv Add(KalturaAssetFilePpv assetFilePpv)
        {
            KalturaAssetFilePpv response = null;

            try
            {
                if (assetFilePpv.AssetFileId <0 )
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "assetFileId");
                }

                if (assetFilePpv.PpvModuleId < 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "ppvModuleId");
                }

                if (assetFilePpv.StartDate.HasValue && assetFilePpv.EndDate.HasValue && assetFilePpv.StartDate >= assetFilePpv.EndDate)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "endDate");
                }

                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.PricingClient().AddAssetFilePpv(groupId, assetFilePpv);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

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
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public bool Delete(long assetFileId, long ppvModuleId)
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
                // call client                
                response = ClientsManager.PricingClient().DeleteAssetFilePpv(groupId, assetFileId, ppvModuleId);

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
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaAssetFilePpv Update(long assetFileId, long ppvModuleId, KalturaAssetFilePpv assetFilePpv)
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
                // call client                
                response = ClientsManager.PricingClient().UpdateAssetFilePpv(groupId, assetFilePpv);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}