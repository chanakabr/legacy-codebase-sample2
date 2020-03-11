using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DAL
{
    internal static class ConnectionsStringsHelper
    {                  
        
        private const string TVINCI_PROD_CONNECTION_STRING = "Server=72.26.211.98;Database=tvinci;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200";
        private const string USERS_PROD_CONNECTION_STRING = "Server=72.26.211.98;Database=users;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200";
        private const string MESSAGE_BOX_PROD_CONNECTION_STRING = "Server=72.26.211.98;Database=MessageBox;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200"; 
        private const string DMS_PROD_CONNECTION_STRING = "";
        private const string BILLING_PROD_CONNECTION_STRING = ""; 
        private const string PRICING_PROD_CONNECTION_STRING = "";
        private const string CONDITIONAL_ACCESS_PROD_CONNECTION_STRING = "";
        private const string DRM_PROD_CONNECTION_STRING = "";


        private const string TVINCI_STAGING_CONNECTION_STRING = "Server=72.26.211.99;Database=tvinci_staging;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200";
        private const string USERS_STAGING_CONNECTION_STRING = "Server=72.26.211.99;Database=users_staging;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200";
        private const string MESSAGE_BOX_STAGING_CONNECTION_STRING = "Server=72.26.211.98;Database=MessageBox_Staging;Uid=production;Pwd=lF6CZU9HIOIAGuzj;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200"; 
        private const string DMS_STAGING_CONNECTION_STRING = "";
        private const string BILLING_STAGING_CONNECTION_STRING = "";
        private const string PRICING_STAGING_CONNECTION_STRING = "";
        private const string CONDITIONAL_ACCESS_STAGING_CONNECTION_STRING = "";
        private const string DRM_STAGING_CONNECTION_STRING = "";

        private static bool m_IsStaging;


        public static bool IsStaging
        {
            get
            {
                if (ConfigurationManager.AppSettings["IsStaging"] != null) 
                {
                    bool result = bool.TryParse(ConfigurationManager.AppSettings["IsStaging"].ToString(), out m_IsStaging);
                    if (result == false) //Failed to parse config key
                    {
                        m_IsStaging = false;
                    }
                }
                else
                {

                    m_IsStaging = false;
                }          
                return  m_IsStaging; 
            }
        }
        
        public static string TvinciConnectionString
        {
            get
            {                              
                if (IsStaging == false)
                {
                    return TVINCI_PROD_CONNECTION_STRING; 
                }
                return TVINCI_STAGING_CONNECTION_STRING; 
            }
        }

        public static string UsersConnectionString
        {
            get
            {
                if (IsStaging == false)
                {
                    return USERS_PROD_CONNECTION_STRING ;
                }
                return USERS_STAGING_CONNECTION_STRING;
            }
        }

        public static string MessageBoxConnectionString
        {
            get
            {
                if (IsStaging == false)
                {
                    return MESSAGE_BOX_PROD_CONNECTION_STRING;
                }
                return MESSAGE_BOX_STAGING_CONNECTION_STRING;
            }
        }

        public static string DmsConnectionString
        {
            get
            {
                if (IsStaging == false)
                {
                    return DMS_PROD_CONNECTION_STRING;
                }
                return DMS_STAGING_CONNECTION_STRING;
            }
        }

        public static string BillingConnectionString
        {
            get
            {
                if (IsStaging == false)
                {
                    return BILLING_PROD_CONNECTION_STRING;
                }
                return BILLING_STAGING_CONNECTION_STRING;
            }
        }

        public static string PricingConnectionString
        {
            get
            {
                if (IsStaging == false)
                {
                    return PRICING_PROD_CONNECTION_STRING ;
                }
                return PRICING_STAGING_CONNECTION_STRING;
            }
        }
    }
}
