using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using EpgBL;

namespace EpgFeeder
{
    public static class Utils
    {
        public static string GetValueByKey(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey); 
        }


   
        public static bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyyMMddHHmmss";
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        public static Dictionary<string, List<string>> GetEpgProgramMetas(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dMetas = new Dictionary<string, List<string>>();

            var MetaFieldEntity = from item in FieldEntityMapping
                                  where item.FieldType == enums.FieldTypes.Meta && item.XmlReffName.Capacity > 0 && item.Value!= null && item.Value.Count > 0
                                  select item;

            foreach (var item in MetaFieldEntity)
            {
                foreach (var value in item.Value)
                {                    
                    if (dMetas.ContainsKey(item.Name))
                    {
                        dMetas[item.Name].AddRange(item.Value);
                    }
                    else
                    {
                        dMetas.Add(item.Name, item.Value);
                    }
                }
            }
            return dMetas;
        }

        public static Dictionary<string, List<string>> GetEpgProgramTags(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dTags = new Dictionary<string, List<string>>();
            var TagFieldEntity = from item in FieldEntityMapping
                                 where item.FieldType == enums.FieldTypes.Tag && item.XmlReffName.Capacity > 0 && item.Value != null && item.Value.Count > 0
                                 select item;


            foreach (var item in TagFieldEntity)
            {
                if (dTags.ContainsKey(item.Name))
                {
                    dTags[item.Name].AddRange(item.Value);
                }
                else
                {
                    dTags.Add(item.Name, item.Value);
                }

            }
            return dTags;
        }
    }
}
