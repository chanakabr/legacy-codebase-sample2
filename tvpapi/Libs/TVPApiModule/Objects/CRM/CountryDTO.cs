
using System;
using Core.Users;
namespace TVPApiModule.Objects.CRM
{
    public class CountryDTO
    {
        public int m_nObjecrtID;
        public int m_nObjecrtIDField { get { return m_nObjecrtID; } set { m_nObjecrtID = value; } }
        public string m_sCountryName;
        public string m_sCountryNameField { get { return m_sCountryName; } set { m_sCountryName = value; } }
        public string m_sCountryCode;
        public string m_sCountryCodeField { get { return m_sCountryCode; } set { m_sCountryCode = value; } }

        public static CountryDTO ConvertToDTO(Country country)
        {
            if(country == null)
            {
                return null;
            }
            CountryDTO res = new CountryDTO()
            {
                m_nObjecrtID = country.m_nObjecrtID,
                m_sCountryCode = country.m_sCountryCode,
                m_sCountryName = country.m_sCountryName
            };
            return res;
        }

        internal static Country ConvertToCore(CountryDTO country)
        {
            if(country == null)
            {
                return null;
            }
            Country res = new Country();
            res.m_nObjecrtID = country.m_nObjecrtID;
            res.m_sCountryCode = country.m_sCountryCode;
            res.m_sCountryName = country.m_sCountryName;
            return res;
        }
    }
}