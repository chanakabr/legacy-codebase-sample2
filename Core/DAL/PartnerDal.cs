using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using ApiObjects;
using KLogMonitor;
using ODBCWrapper;

namespace DAL
{
    public interface IPartnerDal
    {
        int AddPartner(int? partnerId, string partnerName, long updaterId);
        bool SetupPartnerInUsersDb(long partnerId, List<KeyValuePair<long, long>> moduleIds, long updaterId);
        List<Partner> GetPartners();
    }

    public class PartnerDal : IPartnerDal
    {
        private static readonly Lazy<PartnerDal> LazyInstance = new Lazy<PartnerDal>(() => new PartnerDal(), LazyThreadSafetyMode.PublicationOnly);
        public static IPartnerDal Instance => LazyInstance.Value;

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string USERS_CONNECTION_STRING = "USERS_CONNECTION_STRING";

        public int AddPartner(int? partnerId, string partnerName, long updaterId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@id", partnerId},
                {"@name", partnerName},
                {"@enableTemplates", 1},
                {"@updaterId", updaterId}
            };
            var newPartnerId = UtilsDal.ExecuteReturnValue<int>("Insert_Groups", parameters);
            return newPartnerId;
        }
        
        public bool SetupPartnerInUsersDb(long partnerId, List<KeyValuePair<long, long>> moduleIds, long updaterId)
        {
            var sp = new StoredProcedure("Create_GroupBasicData");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);
            //sp.AddIDListParameter("@moudleNames", moduleNames, "STR");
            // TODO IS_ACTIVATION_NEEDED
            // TODO ALLOW_DELETE_USER
            sp.AddKeyValueListParameter("@moduleIds", moduleIds, "idKey", "value");

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public List<Partner> GetPartners()
        {
            List<Partner> returnList = new List<Partner>();

            DataTable dt = UtilsDal.Execute("Get_Groups");
            if (dt?.Rows.Count > 0)
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
    }
}