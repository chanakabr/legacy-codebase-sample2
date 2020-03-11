using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Core.Pricing
{
    public class PriceManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string ASSET_FILE_PPV_NOT_EXIST = "Asset file ppv doesn't exist";


        public static GenericResponse<AssetFilePpv> AddAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId, DateTime? startDate, DateTime? endDate)
        {
            GenericResponse<AssetFilePpv> response = new GenericResponse<AssetFilePpv>();
            try
            {
                // Validate mediaFileId && ppvModuleId
                Status status = ValidateAssetFilePPV(groupId, mediaFileId, ppvModuleId);
                if (status != null && status.Code != (int)eResponseStatus.OK)
                {
                    response.SetStatus(status.Code, status.Message);
                    return response;
                }

                // validate ppvModuleId && mediaFileId not already exist
                bool isExist = IsAssetFilePpvExist(groupId, mediaFileId, ppvModuleId);
                if (isExist)
                {
                    log.ErrorFormat("Error. mediaFileId {0} && ppvModuleId {1} already exist for groupId: {2}", mediaFileId, ppvModuleId, groupId);
                    response.SetStatus(eResponseStatus.Error, "AssetFilePpv already exist");
                    return response;
                }

                DataTable dt = PricingDAL.AddAssetFilePPV(groupId, mediaFileId, ppvModuleId, startDate, endDate);

                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while AddAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}", groupId, mediaFileId, ppvModuleId);
                    return response;
                }

                response.Object = new AssetFilePpv()
                {
                    AssetFileId = mediaFileId,
                    PpvModuleId = ppvModuleId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed AddAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}", groupId, mediaFileId, ppvModuleId, ex);
            }

            return response;
        }

        public static GenericResponse<AssetFilePpv> UpdateAssetFilePPV(int groupId, AssetFilePpv request)
        {
            GenericResponse<AssetFilePpv> response = new GenericResponse<AssetFilePpv>();
            try
            {
                // Validate mediaFileId && ppvModuleId
                Status status = ValidateAssetFilePPV(groupId, request.AssetFileId, request.PpvModuleId);
                if (status != null && status.Code != (int)eResponseStatus.OK)
                {
                    response.SetStatus(status.Code, status.Message);
                    return response;
                }

                // validate ppvModuleId && mediaFileId not already exist
                bool isExist = IsAssetFilePpvExist(groupId, request.AssetFileId, request.PpvModuleId);
                if (!isExist)
                {
                    log.ErrorFormat("Error. mediaFileId {0} && ppvModuleId {1} already exist for groupId: {2}", request.AssetFileId, request.PpvModuleId, groupId);
                    response.SetStatus(eResponseStatus.AssetFilePPVNotExist, ASSET_FILE_PPV_NOT_EXIST);
                    return response;
                }

                var nullableStartDate = new NullableObj<DateTime?>(request.StartDate, request.IsNullablePropertyExists("StartDate"));
                var nullableEndDate = new NullableObj<DateTime?>(request.EndDate, request.IsNullablePropertyExists("EndDate"));

                DataTable dt = PricingDAL.UpdateAssetFilePPV(groupId, request.AssetFileId, request.PpvModuleId, nullableStartDate, nullableEndDate);

                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while UpdateAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}", groupId, request.AssetFileId, request.PpvModuleId);
                    return response;
                }

                response.Object = CreateAssetFilePPV(dt.Rows[0]);
                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed UpdateAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}", groupId, request.AssetFileId, request.PpvModuleId, ex);
            }

            return response;
        }

        public static Status DeleteAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId)
        {
            Status status = new Status();
            try
            {
                // Validate mediaFileId && ppvModuleId
                status = ValidateAssetFilePPV(groupId, mediaFileId, ppvModuleId);
                if (status != null && status.Code != (int)eResponseStatus.OK)
                {
                    return status;
                }

                int res = PricingDAL.DeleteAssetFilePPV(groupId, mediaFileId, ppvModuleId);
                if (res == 0)
                {
                    return new Status((int)eResponseStatus.Error, "failed to DeleteAssetFilePPV");
                }
                else if (res == -1)
                {
                    return new Status((int)eResponseStatus.AssetFilePPVNotExist, ASSET_FILE_PPV_NOT_EXIST);
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed DeleteAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}", groupId, mediaFileId, ppvModuleId, ex);
            }

            return status;
        }

        public static GenericListResponse<AssetFilePpv> GetAssetFilePPVList(int groupId, long assetId, long assetFileId)
        {
            GenericListResponse<AssetFilePpv> response = new GenericListResponse<AssetFilePpv>();
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                List<AssetFilePpv> assetFilePPVList = new List<AssetFilePpv>();
                List<AssetFile> assetFiles = null;
                List<int> fileIds = new List<int>();
                if (assetId > 0)
                {
                    // check if assetId exist
                    GenericResponse<Asset> assetResponse = AssetManager.GetAsset(groupId, assetId, eAssetTypes.MEDIA, true);
                    if (assetResponse != null && assetResponse.Status != null && assetResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        response.SetStatus(assetResponse.Status);
                        return response;
                    }

                    assetFiles = FileManager.GetAssetFilesByAssetId(groupId, assetId);
                    // Get Asset Files
                    if (assetFiles == null)
                    {
                        log.ErrorFormat("Error while getting assetFiles. groupId: {0}, assetId {1}", groupId, assetId);
                        response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }
                }
                else if (assetFileId > 0)
                {
                    // check assetFileId  exist
                    assetFiles = FileManager.GetAssetFilesById(groupId, assetFileId);

                    // Get Asset Files
                    if (assetFiles == null)
                    {
                        log.ErrorFormat("Error while getting assetFiles. groupId: {0}, assetFileId {1}", groupId, assetFileId);
                        response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }
                }

                if (assetFiles.Count > 0)
                {
                    fileIds = assetFiles.Select(x => (int)x.Id).ToList();
                    if (fileIds.Count > 0)
                    {
                        assetFilePPVList = GetAssetFilePPVByFileIds(groupId, fileIds);
                        response.Objects.AddRange(assetFilePPVList);
                        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetAssetFilePPVList with groupId: {0}. assetId: {1}, assetFileId: {2}. ex: {3}",
                    groupId, assetId, assetFileId, ex);
            }

            return response;
        }

        private static List<AssetFilePpv> GetAssetFilePPVByFileIds(int groupId, List<int> fileIds)
        {
            List<AssetFilePpv> assetFilePPVList = new List<AssetFilePpv>();
            DataTable dt = PricingDAL.Get_PPVModuleForMediaFiles(groupId, fileIds);
            if (dt != null && dt.Rows.Count > 0)
            {
                AssetFilePpv assetFilePPV = null;
                foreach (DataRow dr in dt.Rows)
                {
                    assetFilePPV = new AssetFilePpv()
                    {
                        AssetFileId = ODBCWrapper.Utils.GetLongSafeVal(dr, "mfid"),
                        PpvModuleId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ppmid"),
                        StartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "PPMF_START_DATE"),
                        EndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "PPMF_END_DATE")
                    };

                    assetFilePPVList.Add(assetFilePPV);
                }
            }

            return assetFilePPVList;
        }

        public static GenericListResponse<PPVModule> GetPPVList(int groupId, int pageIndex, int pageSize)
        {
            GenericListResponse<PPVModule> response = new GenericListResponse<PPVModule>();

            try
            {
                List<PPVModule> allPpvs = new List<PPVModule>();
                string allPpvsKey = LayeredCacheKeys.GetAllPpvsKey(groupId);

                if (!LayeredCache.Instance.Get<List<PPVModule>>(allPpvsKey,
                                                                ref allPpvs,
                                                                GetAllPpvs,
                                                                new Dictionary<string, object>()
                                                                {
                                                                    { "groupId", groupId }
                                                                },
                                                                groupId,
                                                                LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME,
                                                                new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId) }))
                {
                    return response;
                }

                if (pageSize > 0)
                {
                    int skip = pageIndex * pageSize;

                    if (allPpvs.Count > skip)
                    {
                        response.Objects = (allPpvs.Count) > (skip + pageSize) ? allPpvs.Skip(skip).Take(pageSize).ToList() : allPpvs.Skip(skip).ToList();
                    }
                }
                else
                {
                    response.Objects = allPpvs;
                }

            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;

        }

        private static Tuple<List<PPVModule>, bool> GetAllPpvs(Dictionary<string, object> funcParams)
        {
            List<PPVModule> allPpvs = new List<PPVModule>();

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            DataTable dtPPVModuleData = PricingDAL.Get_PPVModuleData(groupId.Value, null);

                            if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
                            {
                                for (int i = 0; i < dtPPVModuleData.Rows.Count; i++)
                                {
                                    DataRow ppvModuleDataRow = dtPPVModuleData.Rows[i];
                                    int nPPVModuleID = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["ID"]);
                                    string sPriceCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["PRICE_CODE"]);
                                    string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["USAGE_MODULE_CODE"]);
                                    string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["DISCOUNT_MODULE_CODE"]);
                                    string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["COUPON_GROUP_CODE"]);
                                    string sName = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["NAME"]);
                                    bool bSubOnly = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["SUBSCRIPTION_ONLY"]));
                                    bool bIsFirstDeviceLimitation = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["FIRSTDEVICELIMITATION"]));
                                    string productCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["Product_Code"]);
                                    string adsParam = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["ADS_PARAM"]);

                                    int adsPolicyInt = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["ADS_POLICY"]);
                                    AdsPolicy? adsPolicy = null;
                                    if (adsPolicyInt > 0)
                                    {
                                        adsPolicy = (AdsPolicy)adsPolicyInt;
                                    }

                                    PPVModule t = new PPVModule();
                                    t.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, null, groupId.Value, nPPVModuleID.ToString(),
                                        bSubOnly, sName, string.Empty, string.Empty, string.Empty, null, bIsFirstDeviceLimitation, productCode, 0, adsPolicy, adsParam);

                                    allPpvs.Add(t);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allPpvs = null;
            }

            return new Tuple<List<PPVModule>, bool>(allPpvs, allPpvs != null);
        }

        private static AssetFilePpv CreateAssetFilePPV(DataRow dataRow)
        {
            AssetFilePpv assetFilePPV = new AssetFilePpv()
            {
                AssetFileId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "MEDIA_FILE_ID"),
                PpvModuleId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PPV_MODULE_ID"),
                StartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "START_DATE"),
                EndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "END_DATE")
            };

            return assetFilePPV;
        }

        private static Status ValidateAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId)
        {
            // validate ppvModuleId Exists               
            bool isExist = IsPPVModuleExist(groupId, ppvModuleId);
            if (!isExist)
            {
                log.ErrorFormat("Error. Unknown PPVModule: {0} for groupId: {1}", ppvModuleId, groupId);
                return new Status((int)eResponseStatus.UnKnownPPVModule, "The ppv module is unknown");
            }

            // validate mediaFileId Exists
            isExist = IsMediaFileIdExist(groupId, mediaFileId);
            if (!isExist)
            {
                log.ErrorFormat("Error. Unknown mediaFileId: {0} for groupId: {1}", mediaFileId, groupId);
                return new Status((int)eResponseStatus.MediaFileDoesNotExist, "Media file does not exist");
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        private static bool IsPPVModuleExist(int groupId, long ppvModuleId)
        {
            // check ppvModuleId  exist
            DataTable dt = PricingDAL.Get_PPVModuleData(groupId, (int)ppvModuleId);
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return false;
            }

            return true;
        }

        private static bool IsMediaFileIdExist(int groupId, long mediaFileId)
        {
            // check mediaFileId  exist
            List<AssetFile> assetFiles = FileManager.GetAssetFilesById(groupId, mediaFileId);

            // Get Asset Files
            if (assetFiles == null || assetFiles.Count == 0)
            {
                return false;
            }

            return true;
        }

        private static bool IsAssetFilePpvExist(int groupId, long mediaFileId, long ppvModuleId)
        {
            // check ppvModuleId  exist
            DataRow dr = PricingDAL.Get_PPVModuleForMediaFile((int)mediaFileId, ppvModuleId, groupId);
            if (dr == null)
            {
                return false;
            }

            return true;
        }
    }
}
