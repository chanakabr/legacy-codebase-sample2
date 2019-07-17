using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADIFeeder
{
    public class ADIMethods
    {
        public static void HLicensingWindowStart(ADIFeeder thisParameter, string sValue, string key)
        {
            string sStart = GetSafeDate(sValue);

            thisParameter.m_basicsDict.Add("start", sStart);
            thisParameter.m_stringMetaDict.Add("Licensing window start", sStart);
        }

        public static void HShowType(ADIFeeder thisParameter, string sValue, string key)
        {
            if (string.IsNullOrEmpty(thisParameter.sMediaType))
            {
                thisParameter.sMediaType = sValue;
            }

            thisParameter.m_stringMetaDict.Add("Media type", sValue);
        }

        public static void HLicensingWindowEnd(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.sEndDate = GetSafeDate(sValue);

            thisParameter.m_basicsDict.Add("end", thisParameter.sEndDate);
            thisParameter.m_stringMetaDict.Add("Licensing window end", thisParameter.sEndDate);
        }

        public static void HPurgeDate(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.sFinalEndDate = GetSafeDate(sValue);
            thisParameter.m_basicsDict.Add("final", thisParameter.sFinalEndDate);
        }

        public static void HInsertBasic(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_basicsDict.Add(key, sValue);
        }

        public static void HInsertString(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_stringMetaDict.Add(key, sValue);
        }

        public static void HInsertNum(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_numMetaDict.Add(key, sValue);
        }

        public static void HInsertBool(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_boolMetaDict.Add(key, sValue.ToLower().Equals("false") ? "0" : "1");
        }

        public static void HInsertTags(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.AddTag(key, sValue);
        }

        public static void HRunTime(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.sDuration = sValue;
        }

        public static void HBillingID(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_stringMetaDict.Add("Billing ID", sValue);
            thisParameter.m_stringMetaDict.Add("PPVModule", string.Format("{0}{1}", thisParameter.assetProduct, sValue));
        }

        public static void HEpisodeId(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.m_numMetaDict.Add("episode id", sValue);
            thisParameter.m_numMetaDict.Add("Episode number", sValue);

            if (!thisParameter.m_numMetaDict.ContainsKey("Season number"))
            {
                HSeasonNumber(thisParameter, "0", string.Empty);
            }
        }

        public static void HSeasonNumber(ADIFeeder thisParameter, string sValue, string key)
        {
            if (thisParameter.m_numMetaDict.ContainsKey("Season number"))
            {
                thisParameter.m_numMetaDict["Season number"] = sValue;
                thisParameter.m_numMetaDict["Number of seasons"] = sValue;
            }
            else
            {
                thisParameter.m_numMetaDict.Add("Season number", sValue);
                thisParameter.m_numMetaDict.Add("Number of seasons", sValue);
            }
        }

        public static void HAdBreak(ADIFeeder thisParameter, string sValue, string key)
        {
            thisParameter.sBreakpoints = sValue;
        }

        public static void HGeoBlockRule(ADIFeeder thisParameter, string sValue, string key)
        {
            if (sValue.ToLower() == "singapore")
                thisParameter.m_basicsDict.Add("Geo Block Rule", "Singapore only");
            else
                thisParameter.m_basicsDict.Add("Geo Block Rule", string.Empty);
        }

        public static void HDeviceRule(ADIFeeder thisParameter, string sValue, string key)
        {
            if (sValue.EndsWith(";"))
            {
                thisParameter.m_basicsDict.Add("Device Rule", sValue.Substring(0, sValue.Length - 1));
            }
            else
            {
                thisParameter.m_basicsDict.Add("Device Rule", sValue);
            }
        }

        public static void HTXDate(ADIFeeder thisParameter, string sValue, string key)
        {
            string sTXDat = GetSafeDate(sValue);

            thisParameter.m_stringMetaDict.Add("TX Date", sTXDat);
        }

        private static string GetSafeDate(string sValue)
        {
            DateTime dDate;
            if (DateTime.TryParse(sValue, out dDate) == false)
            {
                return string.Empty;
            }

            return dDate.AddHours(-8).ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
