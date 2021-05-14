using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Users;
using DAL;
using KLogMonitor;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Users
{
    public interface IDomainLimitationModuleRepository
    {
        LimitationsManager Add(int groupId, int concurrentLimit, int deviceFrequency, int deviceLimit, string name, int userFrequency, int usersLimit, DeviceFamilyLimitations[] deviceFamilyLimitations, long updaterId);
        LimitationsManager Get(int groupId, int limitId);
        IEnumerable<int> GetDomainLimitationModuleIds(int groupId);
        bool Delete(int limitId, long updaterId);
    }

    public class DomainLimitationModuleRepository : IDomainLimitationModuleRepository
    {
        private readonly ILogger _logger;
        private readonly IDomainLimitationModuleDal _dlmDal;

        public DomainLimitationModuleRepository()
            : this(new DomainLimitationModuleDal(), new KLogger(nameof(DomainLimitationModuleRepository)))
        {
        }

        public DomainLimitationModuleRepository(IDomainLimitationModuleDal dlmDal, ILogger logger)
        {
            _dlmDal = dlmDal;
            _logger = logger;
        }

        public LimitationsManager Add(int groupId, int concurrentLimit, int deviceFrequency, int deviceLimit, string name, int userFrequency, int usersLimit, DeviceFamilyLimitations[] deviceFamilyLimitations, long updaterId)
        {
            try
            {
                var concurrentLimits = deviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.concurrency));
                var deviceLimits = deviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.quantity));
                var frequencyLimits = deviceFamilyLimitations.Select(x => new KeyValuePair<int, int>(x.deviceFamily, x.Frequency));

                var dataSet = _dlmDal.InsertGroupLimitsAndDeviceFamilies(groupId, concurrentLimit, deviceFrequency, deviceLimit, name, userFrequency, usersLimit, concurrentLimits, deviceLimits, frequencyLimits, updaterId);
                var limitationManager = CreateLimitationsManager(dataSet);

                return limitationManager;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Add)}: {e.Message}.");

                return null;
            }
        }

        public LimitationsManager Get(int groupId, int limitId)
        {
            try
            {
                var dataSet = _dlmDal.GetGroupLimitsAndDeviceFamilies(groupId, limitId);
                var oLimitationsManager = CreateLimitationsManager(dataSet);

                return oLimitationsManager;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Get)}: {nameof(groupId)}={groupId}, {nameof(limitId)}={limitId}, {e.Message}.");

                return null;
            }
        }

        public IEnumerable<int> GetDomainLimitationModuleIds(int groupId)
        {
            try
            {
                var dt = _dlmDal.GetGroupDeviceLimitationModules(groupId);
                var limitationsManagerIds = GetGroupsDeviceLimitationModuleIds(dt);

                return limitationsManagerIds;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(GetDomainLimitationModuleIds)}: {nameof(groupId)}={groupId}, {e.Message}.");

                return null;
            }
        }

        public bool Delete(int limitId, long updaterId)
        {
            try
            {
                var deletedRows = _dlmDal.DeleteGroupLimitsAndDeviceFamilies(limitId, updaterId: updaterId);

                return deletedRows > 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Delete)}: {nameof(limitId)}={limitId}, {e.Message}.");

                return false;
            }
        }

        private LimitationsManager CreateLimitationsManager(DataSet ds)
        {
            if (ds?.Tables == null || ds.Tables.Count <= 0)
            {
                return null;
            }

            LimitationsManager limitationsManager = null;

            #region GroupLevel + DLM Level

            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0 &&
                ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
            {
                limitationsManager = new LimitationsManager();

                var drGroup = ds.Tables[0].Rows[0];
                var drDLM = ds.Tables[1].Rows[0];
                if (drGroup != null && drDLM != null)
                {
                    limitationsManager.domianLimitID = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "ID");
                    limitationsManager.DomainLimitName = ODBCWrapper.Utils.GetSafeStr(drDLM, "NAME");
                    var nConcurrencyGroupLevel = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "GROUP_CONCURRENT_MAX_LIMIT");
                    limitationsManager.npvrQuotaInSecs = ODBCWrapper.Utils.GetIntSafeVal(drGroup, "npvr_quota_in_seconds");
                    var nConcurrencyDomainLevel = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "CONCURRENT_MAX_LIMIT");
                    limitationsManager.Frequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "freq_period_id");
                    limitationsManager.FrequencyDescription = Utils.GetMinPeriodDescription(limitationsManager.Frequency);
                    limitationsManager.Quantity = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "DEVICE_MAX_LIMIT");
                    limitationsManager.nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "USER_MAX_LIMIT");
                    limitationsManager.UserFrequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "user_freq_period_id");
                    limitationsManager.UserFrequencyDescrition = Utils.GetMinPeriodDescription(limitationsManager.UserFrequency);

                    limitationsManager.SetConcurrency(nConcurrencyDomainLevel, nConcurrencyGroupLevel);
                }
            }

            #endregion

            #region DeviceFamily

            if (limitationsManager != null && ds.Tables.Count >= 4)
            {
                DataTable dt = ds.Tables[2];
                DataTable dtSpecificLimits = ds.Tables[3];
                if (dt != null && dt.Rows.Count > 0)
                {
                    limitationsManager.lDeviceFamilyLimitations = new List<DeviceFamilyLimitations>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        var dfl = new DeviceFamilyLimitations
                        {
                            deviceFamily = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID"),
                            deviceFamilyName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                            concurrency = -1,
                            quantity = -1,
                            Frequency = -1
                        };

                        DataRow[] drSpecific = dtSpecificLimits.Select("device_family_id = " + dfl.deviceFamily);
                        foreach (DataRow drItem in drSpecific)
                        {
                            string sLimitationType = ODBCWrapper.Utils.GetSafeStr(drItem, "description");
                            int nLimitationValue = ODBCWrapper.Utils.GetIntSafeVal(drItem, "value", -1);

                            if (dfl.deviceFamily > 0 && nLimitationValue > -1 && sLimitationType.Length > 0)
                            {
                                if (sLimitationType.ToLower() == "concurrency")
                                {
                                    dfl.concurrency = nLimitationValue;
                                }
                                else if (sLimitationType.ToLower() == "quantity")
                                {
                                    dfl.quantity = nLimitationValue;
                                }
                                else if (sLimitationType.ToLower() == "frequency")
                                {
                                    dfl.Frequency = nLimitationValue;
                                }
                            }
                        }

                        // if concurrency / quantity is -1 take the value from the group itself.
                        if (dfl.concurrency == -1)
                        {
                            dfl.concurrency = limitationsManager.Concurrency;
                        }

                        if (dfl.quantity == -1)
                        {
                            dfl.quantity = limitationsManager.Quantity;
                        }

                        limitationsManager.lDeviceFamilyLimitations.Add(dfl);
                    }
                }
            }

            #endregion

            return limitationsManager;
        }

        private IEnumerable<int> GetGroupsDeviceLimitationModuleIds(DataTable dt)
        {
            List<int> response = null;
            if (dt?.Rows.Count > 0)
            {
                response = new List<int>();
                foreach (DataRow row in dt.Rows)
                {
                    response.Add(ODBCWrapper.Utils.GetIntSafeVal(row, "ID"));
                }
            }

            return response;
        }
    }
}
