using System;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for Parameter.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Minimum date time that is supported in SQL Server
        /// </summary>
        public static readonly DateTime MINIMUM_DATE_TIME = new DateTime(1753, 1, 1);

        private Parameter(string name, string type, object value)
        {
            m_sParName = name;
            m_sType = type;

            if (value is DateTime)
            {
                if ((DateTime)value < MINIMUM_DATE_TIME)
                {
                    value = MINIMUM_DATE_TIME;
                }
            }

            m_sParVal = value;
        }

        ~Parameter()
        {
        }

        public string m_sParName = "";
        public string m_sType = "";
        public object m_sParVal = null;

        public static Parameter NEW_PARAM(string sParName, string sType, object sParVal)
        {
            return new Parameter(sParName, sType, sParVal);
        }

        public static Parameter NEW_PARAM(string sParName, object sParVal)
        {
            return NEW_PARAM(sParName, "", sParVal);
        }
    }
}