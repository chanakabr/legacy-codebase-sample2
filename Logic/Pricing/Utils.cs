using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    public class Utils
    {
        private static string PRICING_CONNECTION = "PRICING_CONNECTION";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static TimeSpan GetEndDateTimeSpan(Int32 nVal)
        {
            DateTime dEnd = DateTime.UtcNow;
            DateTime dStart = DateTime.UtcNow;
            if (nVal == 1111111)
                dEnd = dEnd.AddMonths(1);
            else if (nVal == 2222222)
                dEnd = dEnd.AddMonths(2);
            else if (nVal == 3333333)
                dEnd = dEnd.AddMonths(3);
            else if (nVal == 4444444)
                dEnd = dEnd.AddMonths(4);
            else if (nVal == 5555555)
                dEnd = dEnd.AddMonths(5);
            else if (nVal == 6666666)
                dEnd = dEnd.AddMonths(6);
            else if (nVal == 9999999)
                dEnd = dEnd.AddMonths(9);
            else if (nVal == 11111111)
                dEnd = dEnd.AddYears(1);
            else if (nVal == 22222222)
                dEnd = dEnd.AddYears(2);
            else if (nVal == 33333333)
                dEnd = dEnd.AddYears(3);
            else if (nVal == 44444444)
                dEnd = dEnd.AddYears(4);
            else if (nVal == 55555555)
                dEnd = dEnd.AddYears(5);
            else
                dEnd = dEnd.AddMinutes(nVal);
            return dEnd - dStart;
        }

        public static int GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.PRICING, wsc);

            return nGroupID;
        }

        internal static int GetModuleImplID(int nGroupID, ePricingModules ePricingModule)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.PRICING, nGroupID, (int)ePricingModule, PRICING_CONNECTION);

            return nImplID;
        }

        internal static void GetBaseImpl(ref BaseDiscount t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Discount);

            if (nImplID == 1)
                t = new TvinciDiscount(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCampaign t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Campaign);

            if (nImplID == 1)
                t = new TvinciCampaign(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseUsageModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.UsageModule);

            if (nImplID == 1)
                t = new TvinciUsageModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePPVModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.PPV);

            if (nImplID == 1)
                t = new TvinciPPVModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseSubscription t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Subscription);

            if (nImplID == 1)
                t = new TvinciSubscription(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCollection t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Collection);

            if (nImplID == 1)
                t = new TvinciCollection(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePricing t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Pricing);

            if (nImplID == 1)
                t = new TvinciPricing(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCoupons t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Coupons);

            if (nImplID == 1)
                t = new TvinciCoupons(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePrePaidModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.PrePaid);

            if (nImplID == 1)
                t = new TvinciPrePaidModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePreviewModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Preview);

            if (nImplID == 1)
                t = new TvinciPreviewModule(nGroupID);
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePricing t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static BasePricing GetBasePricing(int groupID, string sFunctionName)
        {
            BasePricing t = null;
            if (groupID != 0)
                GetBaseImpl(ref t, groupID);
            else
                log.Debug("WS ignored - " + string.Format("groupID:{0}, func:{1}", groupID, sFunctionName));

            return t;
        }

     
        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCampaign t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCoupons t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
            {
                Utils.GetBaseImpl(ref t, nGroupID);
            }
            else
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            }
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseDiscount t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseUsageModule t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));

            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePPVModule t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseSubscription t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCollection t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePrePaidModule t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePreviewModule t)
        {
            int nGroupID = 0;
            nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static void GetWSCredentials(int nGroupID, eWSModules eWSModule, string sFunctionName, ref string sUN, ref string sPass)
        {
            Credentials uc = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.PRICING, nGroupID, eWSModule);
            sUN = uc.m_sUsername;
            sPass = uc.m_sPassword;
        }
    }
}
