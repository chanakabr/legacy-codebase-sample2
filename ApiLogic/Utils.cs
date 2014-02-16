using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;

namespace APILogic
{
    public class Utils
    {
        public static int GetIntSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return int.Parse(dr[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static string GetSafeStr(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return dr[sField].ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int GetIntSafeVal(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                    return int.Parse(val);
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        public static double GetDoubleSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return double.Parse(dr[sField].ToString());
                return -1.0;
            }
            catch
            {
                return -1.0;
            }
        }

        public static double GetDoubleSafeVal(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                    return double.Parse(val);
                return -1.0;
            }
            catch
            {
                return -1.0;
            }
        }

        //public static string GetSafeStr(object o)
        //{
        //    if (o == DBNull.Value)
        //        return string.Empty;
        //    else if (o == null)
        //        return string.Empty;
        //    else
        //        return o.ToString();
        //}
              
        internal static List<int> ConvertMediaResultObjectIDsToIntArray(ApiObjects.SearchObjects.SearchResult[] medias)
        {
            List<int> res = new List<int>();

            if (medias != null && medias.Length > 0)
            {
                IEnumerable<int> ids = from media in medias
                      select media.assetID;

                res = ids.ToList<int>();
            }

            return res;
        }

        public static string GetCatalogUrl()
        {
            string sCatalogURL = string.Empty;

            try
            {
                sCatalogURL = GetWSUrl("WS_Catalog");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Catalog Url", "Cannot read catalog url", "Catalog Url");
            }

            return sCatalogURL;
        }

        public static string GetWSUrl(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }
    }
}
