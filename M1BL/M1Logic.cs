using System;
using KLogMonitor;
using System.Reflection;

namespace M1BL
{
    public static class M1Logic
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        //private const string M1_LOGIC_LOG_FILE = "M1Logic";
        private const string M1_LOGIC_LOG_HEADER = "M1 Logic";


        public static M1Response CheckFirstPurchasePermissions(string sAppID, string sSessionId, string sUserId, string sUserType, string sMsisdn, out int nGroupId, out string sCustomerServiceID, out string sBaseRedirectUrl, out string sFixedMailAddress)
        {
            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };

            nGroupId = 0;
            sCustomerServiceID = string.Empty;
            sBaseRedirectUrl = string.Empty;
            sFixedMailAddress = string.Empty;

            try
            {
                M1AdsWrapper adsWrapper = new M1AdsWrapper(0, sAppID);

                ADSResponse response = adsWrapper.CheckFirstPurchasePermissions(sSessionId, sUserId, sUserType: sUserType, sMsisdn: sMsisdn, 
                    nGroupId: out  nGroupId, sCustomerServiceID: out sCustomerServiceID, sBaseRedirectUrl: out sBaseRedirectUrl, sFixedMailAddress: out sFixedMailAddress);
                
                result.is_succeeded = response.is_succeeded;
                result.reason = response.reason.ToString();
                result.description = response.description;
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);

                result.is_succeeded = false;
                result.reason = ex.Message;
            }
            return result;
        }

        public static M1Response CheckSubsequencePurchasePermissions(int nGroupID, string sMsisdn, out string sFixedMailAddress)
        {

            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };
            sFixedMailAddress = string.Empty;

            try
            {
                M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
                ADSResponse response = adsWrapper.CheckExistingPurchasePermissions(sMsisdn, out sFixedMailAddress);
                result.is_succeeded = response.is_succeeded;
                result.reason = response.reason.ToString();
                result.description = response.description;
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result.is_succeeded = false;
                result.reason = ex.Message;
            }
            return result;
        }


        public static M1Response CheckCallBackLoginCode(string sCallBackLoginCode)
        {
            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };

            try
            {
                if (!string.IsNullOrEmpty(sCallBackLoginCode) && sCallBackLoginCode != "0")
                {
                    result.is_succeeded = false;
                    result.reason = "Invalid M1 CallBackLoginCode:" + sCallBackLoginCode;
                    result.description = "Invalid M1 CallBackLoginCode:" + sCallBackLoginCode;
                }
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + " CallBackLoginCode=" + sCallBackLoginCode + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result.is_succeeded = false;
                result.reason = ex.Message;
                result.description = "Exception on parsing CallBackLoginCode";
            }
            return result;
        }



        public static M1Response CanAccessVas(int nGroupID, string sMsisdn)
        {
            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };

            try
            {
                M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
                ADSResponse response = adsWrapper.CanAccessVas(sMsisdn);
                result.is_succeeded = response.is_succeeded;
                result.reason = response.reason.ToString();
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + ", GroupID=" + nGroupID.ToString() + ",Msisdn=" + sMsisdn + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result.is_succeeded = false;
                result.reason = ex.Message;
            }
            return result;
        }


        public static M1Response CheckBlackList(int nGroupID, string sMsisdn)
        {
            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };

            try
            {
                M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
                ADSResponse response = adsWrapper.CheckBlackList(sMsisdn);
                result.is_succeeded = response.is_succeeded;
                result.reason = response.reason.ToString();
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result.is_succeeded = false;
                result.reason = ex.Message;
            }
            return result;
        }


        //public static M1Response CreateDummyVas(int nGroupID, string sMsisdn, int nServiceID)
        //{
        //    M1Response result = new M1Response
        //    {
        //        is_succeeded = true,
        //        reason = string.Empty
        //    };

        //    try
        //    {
        //        M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
        //        ADSResponse response = adsWrapper.CreateDummyVas(sMsisdn, nServiceID);
        //        result.is_succeeded = response.is_succeeded;
        //        result.reason = response.reason.ToString();
        //        result.description = response.description;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",ServiceID=" + nServiceID + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
        //        result.is_succeeded = false;
        //        result.reason = ex.Message;
        //    }
        //    return result;
        //}

        //public static M1Response RemoveDummyVas(int nGroupID, string sMsisdn, int nServiceID)
        //{
        //    M1Response result = new M1Response
        //    {
        //        is_succeeded = true,
        //        reason = string.Empty
        //    };

        //    try
        //    {
        //        M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
        //        ADSResponse response = adsWrapper.RemoveDummyVas(sMsisdn, nServiceID);
        //        result.is_succeeded = response.is_succeeded;
        //        result.reason = response.reason.ToString();
        //        result.description = response.description;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",ServiceID=" + nServiceID + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
        //        result.is_succeeded = false;
        //        result.reason = ex.Message;
        //    }
        //    return result;
        //}


        public static M1Response RemoveDummyVas(int nGroupID, string sMsisdn, string sCustomerServiceID)
        {
            M1Response result = new M1Response
            {
                is_succeeded = true,
                reason = string.Empty
            };

            try
            {
                M1AdsWrapper adsWrapper = new M1AdsWrapper(nGroupID, null);
                ADSResponse response = adsWrapper.RemoveDummyVas(sMsisdn, sCustomerServiceID);

                result.is_succeeded = response.is_succeeded;
                result.reason = response.reason.ToString();
                result.description = response.description;
            }
            catch (Exception ex)
            {
                log.Error(M1_LOGIC_LOG_HEADER + " Msisdn=" + sMsisdn + ",CustomerServiceID=" + sCustomerServiceID + ",exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result.is_succeeded = false;
                result.reason = ex.Message;
            }
            return result;
        }
    }
}
