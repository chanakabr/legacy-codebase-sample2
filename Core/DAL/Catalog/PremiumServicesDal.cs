using ApiObjects.Pricing;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace DAL.Catalog
{
    public interface IPremiumServiceRepository
    {
        List<long> GetGroupPremiumServiceIds(int groupId);
        List<ServiceObject> GetAllPremiumServices();
        bool UpdateGroupPremiumServices(int groupId, long userId, PartnerPremiumServices partnerPremiumServices);
    }
    public class PremiumServicesDal : IPremiumServiceRepository
    {
        private static readonly Lazy<PremiumServicesDal> lazy = new Lazy<PremiumServicesDal>(() => new PremiumServicesDal(), LazyThreadSafetyMode.PublicationOnly);

        public static PremiumServicesDal Instance { get { return lazy.Value; } }


        public List<ServiceObject> GetAllPremiumServices()
        {
            List<ServiceObject> services = new List<ServiceObject>();

            var dt = UtilsDal.Execute("Get_Services", null, "MAIN_CONNECTION_STRING");

            if (dt?.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    services.Add(new ServiceObject() { ID = Utils.GetIntSafeVal(row, "ID"), Name = Utils.GetSafeStr(row, "DESCRIPTION") });
                }
            }

            return services;
        }

        public List<long> GetGroupPremiumServiceIds(int groupId)
        {
            List<long> serviceIds = new List<long>();
            var parameters = new Dictionary<string, object>() { { "@groupId", groupId } };
            var dt = UtilsDal.Execute("GetGroupServices", parameters, "MAIN_CONNECTION_STRING");

            if (dt?.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    serviceIds.Add(Utils.GetLongSafeVal(row, "SERVICE_ID"));
                }
            }

            return serviceIds;
        }


        public bool UpdateGroupPremiumServices(int groupId, long userId, PartnerPremiumServices partnerPremiumServices)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_GroupsServices");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@updaterId", userId);
            sp.AddIDListParameter<int>("@services", partnerPremiumServices.Services.Where(x => x.IsApplied).Select(x => (int)x.Id).ToList(), "Id");

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}