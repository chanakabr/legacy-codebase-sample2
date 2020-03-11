using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ApiLogic.Api.Managers
{
    public class PartnerConfigurationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region internal methods 
        internal static GenericListResponse<GeneralPartnerConfig> GetGeneralPartnerConfiguration(int groupId)
        {
            GenericListResponse<GeneralPartnerConfig> response = new GenericListResponse<GeneralPartnerConfig>();
            var generalPartnerConfig = GetGeneralPartnerConfig(groupId);
            if (generalPartnerConfig != null)
            {
                response.Objects.Add(generalPartnerConfig);
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }
        #endregion

        #region private methods
        private static GeneralPartnerConfig GetGeneralPartnerConfig(int groupId)
        {
            GeneralPartnerConfig generalPartnerConfig = null;

            try
            {
                string key = LayeredCacheKeys.GetGeneralPartnerConfig(groupId);
                List<string> configInvalidationKey = new List<string>() { LayeredCacheKeys.GetGeneralPartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<GeneralPartnerConfig>(key,
                                                          ref generalPartnerConfig,
                                                          GetGeneralPartnerConfigDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_GENERAL_PARTNER_CONFIG,
                                                          configInvalidationKey))
                {
                    log.ErrorFormat("Failed getting GetGeneralPartnerConfig from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGeneralPartnerConfig for groupId: {0}", groupId), ex);
            }

            return generalPartnerConfig;
        }

        private static Tuple<GeneralPartnerConfig, bool> GetGeneralPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            GeneralPartnerConfig generalPartnerConfig = null;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    DataSet ds = ApiDAL.GetGeneralPartnerConfig(groupId.Value);
                    if (ds != null && ds.Tables != null && ds.Tables.Count == 3)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig = new GeneralPartnerConfig()
                            {
                                DateFormat = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "date_email_format"),
                                HouseholdLimitationModule = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "max_device_limit"),
                                MailSettings = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "mail_settings"),
                                MainCurrency = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "CURRENCY_ID"),
                                PartnerName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "GROUP_NAME"),
                                MainLanguage = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "LANGUAGE_ID")
                            };

                            int? deleteMediaPolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DELETE_MEDIA_POLICY");
                            int? downgradePolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DOWNGRADE_POLICY");
                            int? defaultRegion = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DEFAULT_REGION");
                            int? enableRegionFiltering = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "IS_REGIONALIZATION_ENABLED");

                            if (deleteMediaPolicy.HasValue)
                            {
                                generalPartnerConfig.DeleteMediaPolicy = (DeleteMediaPolicy)deleteMediaPolicy.Value;
                            }

                            if (downgradePolicy.HasValue)
                            {
                                generalPartnerConfig.DowngradePolicy = (DowngradePolicy)downgradePolicy.Value;
                            }

                            if (enableRegionFiltering.HasValue)
                            {
                                generalPartnerConfig.EnableRegionFiltering = enableRegionFiltering.Value == 1;
                            }

                            if (defaultRegion.HasValue && defaultRegion.Value > 0)
                            {
                                generalPartnerConfig.DefaultRegion = defaultRegion.Value;
                            }
                        }

                        dt = ds.Tables[1];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig.SecondaryLanguages = new List<int>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                generalPartnerConfig.SecondaryLanguages.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "LANGUAGE_ID"));
                            }
                        }

                        dt = ds.Tables[2];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig.SecondaryCurrencies = new List<int>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                generalPartnerConfig.SecondaryCurrencies.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "CURRENCY_ID"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGeneralPartnerConfig failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<GeneralPartnerConfig, bool>(generalPartnerConfig, generalPartnerConfig != null);
        }

        #endregion
    }
}