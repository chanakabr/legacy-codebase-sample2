using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers
{
    public static class Extensions
    {      
        public static bool TryParse<T>(this Enum theEnum, string value, out T result)
        {               
            result = default(T);

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (Enum.IsDefined(typeof(T), value))
            {
                result = (T)Enum.Parse(typeof(T), value,true);
                return true;
            }

            foreach (string item in Enum.GetNames(typeof(T)))
            {
                if (item.ToLower().Equals(value.ToLower()))
                {
                    result = (T)Enum.Parse(typeof(T), value); 
                    return true;
                }
            }                       

            return false;
        }

        static string[] extensions = // 0 1 2 3 4 5 6 7 8 9  
            { "th", "st", "nd", "rd", "th", "th", "th", "tn", "th", "th",  
                // 10 11 12 13 14 15 16 17 18 19  
                "th", "th", "th", "th", "th", "th", "th", "tn", "th", "th",  
                // 20 21 22 23 24 25 26 27 28 29  
                "th", "st", "nd", "rd", "th", "th", "th", "tn", "th", "th",  
                // 30 31  
                "th", "st"  
            };
        public static string ToEndianFormat(this DateTime dt)
        {
            string s = dt.ToString(" MMMM, yyyy");
            string t = string.Format("{0:HH:mm} on {1}{2}", dt,dt.Day, extensions[dt.Day]);
            return t + s;
        }

        public static string ToCustomPairsString(this Dictionary<string, string> thisDic)
        {
            String retVal = String.Empty;
            int countAttached = 0;
            foreach (KeyValuePair<string, string> pair in thisDic)
            {
                retVal += pair.Key + "=" + pair.Value;
                if (++countAttached < thisDic.Count)
                {
                    retVal += "|";
                }
            }
            return retVal;
        }

        public static bool IsNullOrEmtpy<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string[] ToString<T>(this T[] array)
        {
            if (array == null) { return null; }

            string[] strArrays = new string[array.Length];
            for (int index = 0; index < array.Length; index++) { strArrays[index] = array[index].ToString(); }

            return strArrays;
        }

    }
}
