using ApiObjects;
using CouchbaseManager;
using Phx.Lib.Log;
using System;
using System.Reflection;
using System.Threading;

namespace DAL
{
    public interface ICustomFieldsPartnerRepository
    {
        bool SaveCustomFieldsPartnerConfig(int groupId, CustomFieldsPartnerConfig partnerConfig);
        CustomFieldsPartnerConfig GetCustomFieldsPartnerConfig(int groupId);
    }

    public class CustomFieldsPartnerRepository : ICustomFieldsPartnerRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CustomFieldsPartnerRepository> lazy = new Lazy<CustomFieldsPartnerRepository>(() =>
                                new CustomFieldsPartnerRepository(),
                                LazyThreadSafetyMode.PublicationOnly);

        public static CustomFieldsPartnerRepository Instance { get { return lazy.Value; } }

        private CustomFieldsPartnerRepository()
        {
        }

        public CustomFieldsPartnerConfig GetCustomFieldsPartnerConfig(int groupId)
        {
            string key = GetCustomFieldsPartnerConfigKey(groupId);
            return UtilsDal.GetObjectFromCB<CustomFieldsPartnerConfig>(eCouchbaseBucket.OTT_APPS, key);
        }

        public bool SaveCustomFieldsPartnerConfig(int groupId, CustomFieldsPartnerConfig partnerConfig)
        {
            string key = GetCustomFieldsPartnerConfigKey(groupId);
            return UtilsDal.SaveObjectInCB<CustomFieldsPartnerConfig>(eCouchbaseBucket.OTT_APPS, key, partnerConfig);
        }

        private static string GetCustomFieldsPartnerConfigKey(int groupId)
        {
            return $"custom_fields_partner_config_{groupId}";
        }
    }
}
