using System.Collections.Generic;
using System.Data;
using System.Linq;
using ODBCWrapper;
using Tvinci.Core.DAL;
using DAL.DTO;

namespace DAL
{
    public interface IDomainLimitationModuleDal
    {
        LimitationsManagerDTO InsertGroupLimitsAndDeviceFamilies(
            LimitationsManagerDTO limitationsManager,
            int groupId,
            long updaterId);

        LimitationsManagerDTO UpdateGroupLimitsAndDeviceFamilies(int groupId, long updaterId, LimitationsManagerDTO limitationsManagerDTO);
        IEnumerable<int> GetGroupDeviceLimitationModules(int groupId);
        LimitationsManagerDTO GetGroupLimitsAndDeviceFamilies(int groupId, int limitId);
        int DeleteGroupLimitsAndDeviceFamilies(int limitId, long updaterId);
    }

    public class DomainLimitationModuleDal : BaseDal, IDomainLimitationModuleDal
    {
        public LimitationsManagerDTO InsertGroupLimitsAndDeviceFamilies(
            LimitationsManagerDTO limitationsManager,
            int groupId,
            long updaterId)
        {
            var sp = new StoredProcedure("Insert_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@ConcurrentLimit", limitationsManager.Concurrency);
            sp.AddParameter("@DeviceFrequencyId", limitationsManager.Frequency);
            sp.AddParameter("@DeviceLimit", limitationsManager.Quantity);
            sp.AddParameter("@Name", limitationsManager.DomainLimitName);
            sp.AddParameter("@UserFrequencyId", limitationsManager.UserFrequency);
            sp.AddParameter("@UserLimit", limitationsManager.nUserLimit);
            sp.AddKeyValueListParameter("@DeviceFamiliesConcurrentLimits", limitationsManager.CreateConcurrencyLimitationsList().ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesDeviceLimits", limitationsManager.CreateDeviceLimitationsList().ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesFrequencyLimits", limitationsManager.CreateFrequencyLimitationsList().ToList(), "idKey", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@Description", limitationsManager.Description);

            var ds = sp.ExecuteDataSet();

            return CreateLimitationsManager(ds);
        }

        public IEnumerable<int> GetGroupDeviceLimitationModules(int groupId)
        {
            var sp = new StoredProcedure("Get_groupsDeviceLimitationModules");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);

            return GetGroupsDeviceLimitationModuleIds(sp.Execute());
        }

        public LimitationsManagerDTO GetGroupLimitsAndDeviceFamilies(int groupId, int limitId)
        {
            var sp = new StoredProcedure("Get_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@DomainLimitID", limitId);
            var ds = sp.ExecuteDataSet();

            return CreateLimitationsManager(ds);
        }

        public int DeleteGroupLimitsAndDeviceFamilies(int limitId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@LimitId", limitId);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteReturnValue<int>();
        }

        public LimitationsManagerDTO UpdateGroupLimitsAndDeviceFamilies(int groupId, long updaterId, LimitationsManagerDTO limitationsManagerDTO)
        {
            var sp = new StoredProcedure("Set_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@LimitId", limitationsManagerDTO.domianLimitID);
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@ConcurrentLimit", limitationsManagerDTO.Concurrency);
            sp.AddParameter("@DeviceFrequencyId", limitationsManagerDTO.Frequency);
            sp.AddParameter("@DeviceLimit", limitationsManagerDTO.Quantity);
            sp.AddParameter("@Name", limitationsManagerDTO.DomainLimitName);
            sp.AddParameter("@UserFrequencyId", limitationsManagerDTO.UserFrequency);
            sp.AddParameter("@UserLimit", limitationsManagerDTO.nUserLimit);
            sp.AddKeyValueListParameter("@DeviceFamiliesConcurrentLimits", limitationsManagerDTO.CreateConcurrencyLimitationsList().ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesDeviceLimits", limitationsManagerDTO.CreateDeviceLimitationsList().ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesFrequencyLimits", limitationsManagerDTO.CreateFrequencyLimitationsList().ToList(), "idKey", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@Description", limitationsManagerDTO.Description);

            var ds = sp.ExecuteDataSet();
            
            return CreateLimitationsManager(ds);
        }


        private LimitationsManagerDTO CreateLimitationsManager(DataSet ds)
        {
            if (ds?.Tables == null || ds.Tables.Count == 0)
            {
                return null;
            }

            LimitationsManagerDTO limitationsManager = null;

            #region GroupLevel + DLM Level

            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0 &&
                ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
            {
                limitationsManager = new LimitationsManagerDTO();

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
                    limitationsManager.Quantity = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "DEVICE_MAX_LIMIT");
                    limitationsManager.nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "USER_MAX_LIMIT");
                    limitationsManager.UserFrequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "user_freq_period_id");
                    limitationsManager.UserFrequency = ODBCWrapper.Utils.GetIntSafeVal(drDLM, "user_freq_period_id");
                    limitationsManager.Description = ODBCWrapper.Utils.GetSafeStr(drDLM, "DESCRIPTION");

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
                    limitationsManager.lDeviceFamilyLimitations = new List<DeviceFamilyLimitationsDTO>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        var dfl = new DeviceFamilyLimitationsDTO
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
