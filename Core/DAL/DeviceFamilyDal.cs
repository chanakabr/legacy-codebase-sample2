using System;
using System.Data;
using System.Threading;
using ApiObjects;

namespace DAL
{
    public class DeviceFamilyDal : IDeviceFamilyDal
    {
        private static readonly Lazy<DeviceFamilyDal> LazyInstance = new Lazy<DeviceFamilyDal>(() => new DeviceFamilyDal(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceFamilyDal Instance => LazyInstance.Value;

        public DataSet Add(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("InsertDeviceFamily");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", deviceFamily.Id);
            sp.AddParameter("@Name", deviceFamily.Name);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet Update(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("UpdateDeviceFamily");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", deviceFamily.Id);
            sp.AddParameter("@Name", deviceFamily.Name);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet GetByDeviceBrandId(long groupId, long deviceBrandId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Get_DeviceFamilyIDAndName");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@DeviceBrandID", deviceBrandId);

            return sp.ExecuteDataSet();
        }

        public DataSet List(long groupId)
        {
            var sp = new ODBCWrapper.StoredProcedure("GetDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);

            return sp.ExecuteDataSet();
        }
    }
}