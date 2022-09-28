using ApiObjects;
using Phx.Lib.Log;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace DAL
{
    public interface IPartnerDal
    {
        int AddPartner(int? partnerId, string partnerName, long updaterId);
        List<Partner> GetPartners();
        bool SetupPartnerInDb(long partnerId, string name, long updaterId, bool enableEpgV2 = false);
        bool DeletePartnerBasicDataDb(long partnerId, long updaterId);
        bool IsPartnerExists(int partnerId);
        bool DeletePartner(int partnerId, long updaterId);
    }

    public class PartnerDal : IPartnerDal
    {
        private static readonly Lazy<PartnerDal> LazyInstance = new Lazy<PartnerDal>(() => new PartnerDal(), LazyThreadSafetyMode.PublicationOnly);
        public static IPartnerDal Instance => LazyInstance.Value;

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int AddPartner(int? partnerId, string partnerName, long updaterId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_Groups");
            sp.AddParameter("@id", partnerId);
            sp.AddParameter("@name", partnerName);
            sp.AddParameter("@enableTemplates", 1);            
            sp.AddParameter("@updaterId", updaterId);

            var newPartnerId = sp.ExecuteReturnValue<int>();
            return newPartnerId;
        }

        public List<Partner> GetPartners()
        {
            List<Partner> returnList = new List<Partner>();

            DataTable dt = UtilsDal.Execute("Get_Groups");
            if (dt?.Rows?.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Partner p = new Partner();
                    p.Id = Utils.GetIntSafeVal(dr, "id");
                    p.Name = Utils.GetSafeStr(dr, "GROUP_NAME");
                    p.CreateDate = Utils.GetDateSafeVal(dr, "CREATE_DATE");
                    p.UpdateDate = Utils.GetDateSafeVal(dr, "UPDATE_DATE");
                    returnList.Add(p);
                }
            }

            return returnList;
        }

        public bool SetupPartnerInDb(long partnerId, string name, long updaterId, bool enableEpgV2 = false)
        {
            var sp = new StoredProcedure("Create_GroupBasicData");
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@name", name);
            sp.AddParameter("@updaterId", updaterId);
            sp.AddParameter("@isEpgIngestV2", enableEpgV2? 1:0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeletePartnerBasicDataDb(long partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_GroupBasicData");
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeletePartner(int partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_Group");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@Id", partnerId);
            sp.AddParameter("@updaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool IsPartnerExists(int partnerId)
        {
            var sp = new StoredProcedure("Is_GroupExists");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@Id", partnerId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}