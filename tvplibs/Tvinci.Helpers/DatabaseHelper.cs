using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers
{
    #region Enum
    public enum eExecuteLocation
    {
        Admin,
        Application
    }
    #endregion

    public static class DatabaseHelper
    {
        #region Fields
        public static IsUserEditorialDelegate m_isUserEditorialMethod;
        #endregion

        #region Properties
        public static IsUserEditorialDelegate IsUserEditorialMethod
        {            
            set
            {
                m_isUserEditorialMethod = value;
            }
        }
        #endregion

        #region Static Methods
        public static string AddCommonFields(string statusFieldName, string activeFieldName, eExecuteLocation executeLocation, bool postfixWithAnd)
        {
            string result = string.Format("{0} in {1}",statusFieldName, (executeLocation == eExecuteLocation.Admin) ? "(1,4)" : "(1)");

            if (!string.IsNullOrEmpty(activeFieldName) && executeLocation == eExecuteLocation.Application && !IsUserEditorial())
            {
                result = string.Format("{0} and {1} = 1", result, activeFieldName);
            }

            if (postfixWithAnd)
            {
                result += " and ";
            }

            return result;
        }

        private static bool IsUserEditorial()
        {
            if (m_isUserEditorialMethod != null)
            {
                return m_isUserEditorialMethod();
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Delegates
        public delegate bool IsUserEditorialDelegate();
        #endregion
    }
}
