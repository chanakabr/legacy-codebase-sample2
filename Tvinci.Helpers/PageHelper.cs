using System;
using System.Web;

namespace Tvinci.Helpers
{
    //public static class ClientHelper
    //{
    //    public static void RegisterControl(Control control)
    //    {            
    //        ScriptManager.RegisterStartupScript(control,control.GetType(), control.ID, string.Format("var {0} = '{1}';", control.ID, control.ClientID), true);                                    
    //    }
    //}
    public static class PageHelper
    {
        #region Static Methods
        public static TValue GetValue<TValue>(string key, TValue defaultValue)
        {
            TValue result;
            if (TryGetValue<TValue>( key, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static TValue GetValue<TValue>(string key)
        {
            TValue result;
            if (TryGetValue<TValue>(key, out result))
            {
                return result;
            }
            else
            {
                throw new Exception(string.Format("Cannot find item with key '{0}' in page context", key));
            }
        }

        public static void SetValue(string key, object value)
        {
            key = generateUniqueKey(key);
            HttpContext.Current.Items[key] = value;
        }
        
        public static bool TryGetValue<TValue>(string key, out TValue value)
        {
            key = generateUniqueKey(key);

            value = default(TValue);

            try
            {
                if (HttpContext.Current.Items[key] == null)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            object result = HttpContext.Current.Items[key];

            if (result is TValue)
            {
                value = (TValue)result;
                return true;
            }
            else
            {
                return false;
            }
        }

        
        private static string generateUniqueKey(string key)
        {
            return string.Format("PageH_{0}",key);
        }

        public static void ClearValue(string key)
        {
            key = generateUniqueKey(key);

            try
            {
                HttpContext.Current.Items.Remove(key);
            }
            catch
            {
            }
        }
        
        #endregion
    }
}
