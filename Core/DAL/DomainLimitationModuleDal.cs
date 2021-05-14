using System.Collections.Generic;
using System.Data;
using System.Linq;
using ODBCWrapper;
using Tvinci.Core.DAL;

namespace DAL
{
    public interface IDomainLimitationModuleDal
    {
        DataSet InsertGroupLimitsAndDeviceFamilies(
            int groupId,
            int concurrentLimit,
            int deviceFrequency,
            int deviceLimit,
            string name,
            int userFrequency,
            int usersLimit,
            IEnumerable<KeyValuePair<int, int>> concurrentLimits,
            IEnumerable<KeyValuePair<int, int>> deviceLimits,
            IEnumerable<KeyValuePair<int, int>> frequencyLimits,
            long updaterId);
        DataTable GetGroupDeviceLimitationModules(int groupId);
        DataSet GetGroupLimitsAndDeviceFamilies(int groupId, int limitId);
        int DeleteGroupLimitsAndDeviceFamilies(int limitId, long updaterId);
    }

    public class DomainLimitationModuleDal : BaseDal, IDomainLimitationModuleDal
    {
        public DataSet InsertGroupLimitsAndDeviceFamilies(
            int groupId,
            int concurrentLimit,
            int deviceFrequency,
            int deviceLimit,
            string name,
            int userFrequency,
            int usersLimit,
            IEnumerable<KeyValuePair<int, int>> concurrentLimits,
            IEnumerable<KeyValuePair<int, int>> deviceLimits,
            IEnumerable<KeyValuePair<int, int>> frequencyLimits,
            long updaterId)
        {
            var sp = new StoredProcedure("Insert_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@ConcurrentLimit", concurrentLimit);
            sp.AddParameter("@DeviceFrequencyId", deviceFrequency);
            sp.AddParameter("@DeviceLimit", deviceLimit);
            sp.AddParameter("@Name", name);
            sp.AddParameter("@UserFrequencyId", userFrequency);
            sp.AddParameter("@UserLimit", usersLimit);
            sp.AddKeyValueListParameter("@DeviceFamiliesConcurrentLimits", concurrentLimits.ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesDeviceLimits", deviceLimits.ToList(), "idKey", "value");
            sp.AddKeyValueListParameter("@DeviceFamiliesFrequencyLimits", frequencyLimits.ToList(), "idKey", "value");
            sp.AddParameter("@UpdaterId", updaterId);

            var ds = sp.ExecuteDataSet();

            return ds;
        }

        public DataTable GetGroupDeviceLimitationModules(int groupId)
        {
            var sp = new StoredProcedure("Get_groupsDeviceLimitationModules");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);

            return sp.Execute();
        }

        public DataSet GetGroupLimitsAndDeviceFamilies(int groupId, int limitId)
        {
            var sp = new StoredProcedure("Get_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@DomainLimitID", limitId);
            var ds = sp.ExecuteDataSet();

            return ds;
        }

        public int DeleteGroupLimitsAndDeviceFamilies(int limitId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@LimitId", limitId);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteReturnValue<int>();
        }
    }
}
