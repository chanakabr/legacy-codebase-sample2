using System;
using System.Data;
using System.Threading;
using ApiObjects;

namespace DAL
{
    public class DeviceBrandDal : IDeviceBrandDal
    {
        private static readonly Lazy<DeviceBrandDal> LazyInstance = new Lazy<DeviceBrandDal>(() => new DeviceBrandDal(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceBrandDal Instance => LazyInstance.Value;

        public DataSet Add(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("InsertDeviceBrand");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", deviceBrand.Id);
            sp.AddParameter("@DeviceFamilyId", deviceBrand.DeviceFamilyId);
            sp.AddParameter("@Name", deviceBrand.Name);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet Update(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("UpdateDeviceBrand");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", deviceBrand.Id);
            sp.AddParameter("@DeviceFamilyId", deviceBrand.DeviceFamilyId == 0 ? (long?)null : deviceBrand.DeviceFamilyId);
            sp.AddParameter("@Name", deviceBrand.Name);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet List(long groupId)
        {
            var sp = new ODBCWrapper.StoredProcedure("GetDeviceBrands");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);

            return sp.ExecuteDataSet();
        }
    }
}