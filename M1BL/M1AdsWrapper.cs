using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using KLogMonitor;

namespace M1BL
{
    public class M1AdsWrapper
    {
        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        //const
        private const string WS_AUTH_SUCCESS_MSG = "Status_AuthSuccess";
        private const string WS_SESSION_VALID_MSG = "Session_Valid";

        //private const string WS_FAILURE_MSG = "Status_AuthFailure";
        
        private const string SESSION_TOKEN_VALIDATION_SUCCESS_MSG = "true"; 
        private const string SESSION_TOKEN_VALIDATION_FAILURE_MSG = "false";

        //private const string PROXIMITY_WS_FACADE_USER_NAME = "ToggleUser";
        //private const string PROXIMITY_WS_FACADE_PASSWORD = "wbX8OBRoedf73qnR"; // "jLyeNfdqnWdWUgZ6"; 
        //private const string PROXIMITY_WS_FACADE_ACCOUNT_TYPE = "SSOWebServices";

        //private const string PROXIMITY_WS_INTERFACE_CHANNEL_ID = "CM101";
        private const string PROXIMITY_WS_INTERFACE_USER_ID = "0";
        private const int DUMMY_VAS_ALREADY_EXIST_ERROR_CODE = -81100;


        private const string XML_GET_CUST_BLACK_LIST = @"<ServiceNoInput messageName=""GetCustClassBlackListInfo"">                                    
                                                                <MobileNo>{0}</MobileNo>
                                                        </ServiceNoInput>";

        private const string XML_CREATE_DUMMY_VAS = @"<ServiceMaintenanceInput messageName=""MaintainService"">
                                                            <MobileNo>{0}</MobileNo>
                                                            <Service ServiceId=""{1}"" Action=""ACADD""></Service>
                                                            <EmailOrSMSIndicator>0</EmailOrSMSIndicator>
                                                        </ServiceMaintenanceInput>";

        private const string XML_REMOVE_DUMMY_VAS = @"<ServiceMaintenanceInput messageName=""MaintainService"">
                                                            <MobileNo>{0}</MobileNo>
                                                            <Service ServiceId=""{1}"" Action=""ACRMV""></Service>
                                                            <EmailOrSMSIndicator>0</EmailOrSMSIndicator>
                                                    </ServiceMaintenanceInput>";


        // Added on Sep 2016 - align to M1 updated API
        private const string XML_CAN_ACCESS_VAS = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.onelogin.m1.nebulas.com/"">
                                                        <soapenv:Header/>
                                                            <soapenv:Body>
                                                                <ws:run>
                                                                    <userId>{0}</userId>
                                                                    <policyId>{1}</policyId>
                                                                    <appID>{2}</appID>
                                                                    <appSecret>{3}</appSecret>
                                                                </ws:run>
                                                            </soapenv:Body>
                                                    </soapenv:Envelope>";

        private const string XML_SESSION_VALIDATOR = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.onelogin.m1.nebulas.com/"">
                                                        <soapenv:Header/>
                                                            <soapenv:Body>
                                                                <ws:run>
                                                                    <userID>{0}</userID>
                                                                    <userType>{1}</userType>
                                                                    <sessionID>{2}</sessionID>
                                                                    <appID>{3}</appID>
                                                                    <appSecret>{4}</appSecret>
                                                                </ws:run>
                                                            </soapenv:Body>
                                                    </soapenv:Envelope>";

        //private const string    M1_ADSWRAPPER_LOG_FILE      = "M1AdsWrapper";
        //private const string    M1_ADSWRAPPER_LOG_HEADER    = "M1 AdsWrapper";

        //members
        private List<M1_ServiceParameters> m_lPrimaryActions = new List<M1_ServiceParameters>();
        private List<M1_ServiceParameters> m_lActions = new List<M1_ServiceParameters>();
        
        //private AdsService.AuthorizationWebServiceService m1Service = new AdsService.AuthorizationWebServiceService();

        private int m_nGroupID = 0;
        private string m_sAppID = string.Empty;
        private string m_sAppPassword = string.Empty;
        //private string m_sMacAddress = string.Empty;
        private string m_sWsAdsUrl = string.Empty;
        private string m_sWsServiceFacadeUrl = string.Empty; 
        private string m_sWsServiceInterfaceUrl = string.Empty;
        private string m_sSessionValidationUrl = string.Empty;
        private string m_sBaseRedirectUrl = string.Empty;
        private string m_sFixedMailAddress = string.Empty;

        private string m_sWsFacadeUsername = string.Empty;
        private string m_sWsFacadePassword = string.Empty;
        private string m_sWsFacadeAccountType = string.Empty;
        private string m_sWsInterfaceChannelId = string.Empty;

        //private string m_sM1ServiceUrl = string.Empty;
        private bool bIsInit = false;


        public M1AdsWrapper(int nGroupID, string sAppID)
        {
            InitObject(nGroupID, sAppID);
        }


        private bool InitObject(int nGroupID, string sAppID)
        {
            bool retValue = true;

            try
            {
                if (bIsInit)
                {
                    return false;
                }
                
                InitM1GroupParameters(nGroupID, sAppID);

                //if (!string.IsNullOrEmpty(m_sWsAdsUrl))
                //{
                //    m1Service.Url = m_sWsAdsUrl;
                //}

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                                          
                DataTable dt = BillingDAL.Get_M1CustomerServiceType(m_nGroupID);

                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        M1_ServiceParameters parameters = new M1_ServiceParameters
                        {
                            m_nServiceID = ODBCWrapper.Utils.GetIntSafeVal(dr["SERVICE_ID"]),
                            m_sCanAccessServiceID = ODBCWrapper.Utils.GetSafeStr(dr["CAN_ACCESS_SERVICE_ID"]),
                            m_sCanAccessVasServiceID = ODBCWrapper.Utils.GetSafeStr(dr["CAN_ACCESS_VAS_SERVICE_ID"])
                        };

                        bool bIs_primary = ODBCWrapper.Utils.GetIntSafeVal(dr["IS_PRIMARY"]) == 1;

                        if (bIs_primary)
                        {
                            m_lPrimaryActions.Add(parameters);
                        }
                        else
                        {
                            m_lActions.Add(parameters);
                        }

                    }
                }
            }
            catch
            {
                bIsInit = false;
            }
            finally
            {
                bIsInit = retValue;
            }

            return retValue;
        }

        private void InitM1GroupParameters(int nGroupID, string sAppID)
        {
            DataSet dsGroupParams = null;
            if (sAppID != null)
            {
                dsGroupParams = BillingDAL.Get_M1GroupParameters(null, sAppID);
            }
            else
            {
                dsGroupParams = BillingDAL.Get_M1GroupParameters(nGroupID, null);
            }
            
            if (dsGroupParams != null && dsGroupParams.Tables.Count > 0)
            {
                DataTable dtGroupParams = dsGroupParams.Tables[0];
                if (dtGroupParams != null && dtGroupParams.Rows.Count > 0)
                {
                    DataRow groupParameterRow = dtGroupParams.Rows[0];
                    m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(groupParameterRow["group_id"]);
                    m_sAppID = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["app_id"]);
                    m_sAppPassword  = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["app_password"]);
                    //m_sMacAddress = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["macAddress"]);
                    m_sWsAdsUrl =  ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_ads_url"]);
                    m_sWsServiceFacadeUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_service_facade_url"]);
                    m_sWsServiceInterfaceUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_service_interface_url"]);                      
                    m_sSessionValidationUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["sessionValidation_url"]);
                    m_sBaseRedirectUrl = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["base_redirect_url"]);
                    m_sFixedMailAddress = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["invoice_mail_address"]);
                    m_sWsFacadeUsername = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_facade_username"]);
                    m_sWsFacadePassword = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_facade_password"]);
                    m_sWsFacadeAccountType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_facade_account_type"]);
                    m_sWsInterfaceChannelId = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ws_interface_channel_id"]);
                }
            }
        }

        //private ADSResponse CheckSessionToken(string sSessionToken, string sMsisdn)
        //{
        //    ADSResponse retValue = new ADSResponse();
        //    WebResponse response = null;
        //    Stream receiveStream = null;
        //    StreamReader sr = null;


        //    retValue.reason = M1_API_ResponseReason.OK;
        //    retValue.is_succeeded = true;


        //    try
        //    {
        //        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        //        string url = string.Format(m_sSessionValidationUrl, sSessionToken, m_sAppID, sMsisdn);

        //        var request = (HttpWebRequest) WebRequest.Create(url);
        //        request.Method = "GET";

        //        response = request.GetResponse();
        //        receiveStream = response.GetResponseStream();

        //        if (receiveStream != null)
        //        {
        //            using (sr = new StreamReader(receiveStream))
        //            {
        //                string result = sr.ReadToEnd();

        //                if (result == SESSION_TOKEN_VALIDATION_SUCCESS_MSG)
        //                {
        //                    retValue.is_succeeded = true;

        //                }
        //                else if (result == SESSION_TOKEN_VALIDATION_FAILURE_MSG)
        //                {
        //                    retValue.is_succeeded = false;
        //                    retValue.reason = M1_API_ResponseReason.SESSION_TOKEN_INVALID;
        //                    retValue.description = "Invalid m1 session token:" + sSessionToken + ", url:" + url;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error(ex.Message, ex);
        //        return 
        //            (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
        //    }
        //    finally
        //    {
        //        if (sr != null)
        //        {
        //            sr.Dispose();
        //        }
        //        if (receiveStream != null)
        //        {
        //            receiveStream.Dispose();
        //        }
        //        if (response != null)
        //        {
        //            response.Close();
        //        }
        //    }

        //    return retValue;
        
        //}

        public ADSResponse CheckExistingPurchasePermissions(string sUserID, out string sFixedMailAddress)
        {
            ADSResponse retValue = new ADSResponse
            {
                reason = M1_API_ResponseReason.OK,
                is_succeeded = true
            };


            sFixedMailAddress = m_sFixedMailAddress;          

            if (bIsInit == false)
            {
                return (new ADSResponse( M1_API_ResponseReason.WRAPPER_NOT_INITIALIZED, false));
            }

            try
            {
                retValue = CanAccessVas(sUserID);
                if (!retValue.is_succeeded)
                {
                    return retValue; 
                }
                retValue = CheckBlackList(sUserID); 
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false,  ",exception:" + ex.Message + " || " + ex.StackTrace));
            }
                
            return retValue;
        }

        public ADSResponse CheckFirstPurchasePermissions(string sSessionId, string sUserId, string sUserType, string sMsisdn, out int nGroupId, out string sCustomerServiceID, out string sBaseRedirectUrl, out string sFixedMailAddress)
        {
            nGroupId = m_nGroupID;
            sBaseRedirectUrl = m_sBaseRedirectUrl;
            sFixedMailAddress = m_sFixedMailAddress;

            sCustomerServiceID = string.Empty;
            int nServiceID = 0;

            ADSResponse retValue;

            if (!bIsInit)
            {
                return (new ADSResponse(M1_API_ResponseReason.WRAPPER_NOT_INITIALIZED, false));
            }

            try
            {
                retValue = ValidateSession(sUserId, sUserType, sSessionId, m_sAppID, m_sAppPassword);  //retValue = CheckSessionToken(sSessionToken, sMsisdn);
                if (!retValue.is_succeeded)
                {
                    return retValue;
                }

                string result = WS_AUTH_SUCCESS_MSG;

                foreach (M1_ServiceParameters sp in m_lPrimaryActions)
                {
                    //CanAccessVas(sMsisdn, m_sAppID, sp.m_sCanAccessServiceID, m_sAppPassword);  //m1Service.canAccess(sMsisdn, m_sAppID , sp.m_sCanAccessServiceID , m_sAppPassword);

                    if (IsAuthorized(userID: sMsisdn, policyID: sp.m_sCanAccessServiceID, appID: m_sAppID, appSecret: m_sAppPassword))  //if (result == WS_AUTH_SUCCESS_MSG) { continue; }
                    {
                        continue;
                    }

                    string desc = string.Format("Authorization denied from M1 canAccessVas for: Msisdn:{0}, appID:{1}, serviceID:{2}", sMsisdn, m_sAppID, sp.m_sCanAccessServiceID);
                    var adsRes = (new ADSResponse(M1_API_ResponseReason.CAN_ACCESS_PRIMARY_REJECTION, false, desc));
                    return adsRes;
                }

                retValue = CheckBlackList(sMsisdn);
                _log.DebugFormat("CheckBlackList for MSISDN: {0} - Result: {1}", sMsisdn, retValue.is_succeeded);

                if (!retValue.is_succeeded)
                {
                    return retValue;
                }

              
                foreach (M1_ServiceParameters sp in m_lActions)
                {
                    if (!IsAuthorized(userID: sMsisdn, policyID: sp.m_sCanAccessServiceID, appID: m_sAppID, appSecret: m_sAppPassword)) 
                        //result = CanAccessVas(sMsisdn, m_sAppID, sp.m_sCanAccessServiceID, m_sAppPassword);  //m1Service.canAccess(sMsisdn, m_sAppID, sp.m_sCanAccessServiceID, m_sAppPassword);
                    {
                        continue;
                    }
                    
                    //if (result != WS_AUTH_SUCCESS_MSG) { continue; }

                    sCustomerServiceID = sp.m_sCanAccessServiceID;
                    nServiceID = sp.m_nServiceID;
                    break;
                }
              

                if (result != WS_AUTH_SUCCESS_MSG)
                {
                    string desc = string.Format("Failure result from canAccess() method: {0}, Msisdn:{1}, appID:{2}, canAccessServiceID:{3}", result, sMsisdn, m_sAppID, sCustomerServiceID);
                    return (new ADSResponse(M1_API_ResponseReason.CAN_ACCESS_EXECUTE_SERVICE_REJECTION, false, desc));
                }

                retValue = CreateDummyVas(sMsisdn, nServiceID);
                _log.DebugFormat("CreateDummyVas for MSISDN: {0}, Service ID: {1} - Result: {2}", sMsisdn, nServiceID, retValue.is_succeeded);

                if (!retValue.is_succeeded )
                {
                    string desc = string.Format("Failure result from create dummy vas: {0}, Msisdn:{1}, appID:{2}, ServiceID:{3}", result, sMsisdn, m_sAppID, nServiceID);
                    return (new ADSResponse(M1_API_ResponseReason.CREATE_DUMMY_VAS_ERROR, false, desc));
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }

            return retValue;
        }


        private ADSResponse ValidateSession(string sUserID, string sUserType, string sSessionId, string sAppID, string sAppSecret)
        {
            ADSResponse res = new ADSResponse()
            {
                is_succeeded = false,
                description = string.Format("ERROR> Failed to validate session, Group ID: {0}, SessionValidator URL: '{1}'", m_nGroupID, m_sSessionValidationUrl),
                reason = M1_API_ResponseReason.GENERAL_ERROR
            };

            try
            {
                string soapRequest = string.Format(XML_SESSION_VALIDATOR, sUserID, sUserType, sSessionId, sAppID, sAppSecret);

                string reqNoWhitespace = XElement.Parse(soapRequest).ToString(SaveOptions.DisableFormatting);

                _log.DebugFormat("Validating session for User ID: {0}, URL: {1}, Request: [{2}]", sUserID, m_sSessionValidationUrl, reqNoWhitespace);

                Stopwatch sw = Stopwatch.StartNew();
                string soapRes = Http.SendXMLHttpReq(m_sSessionValidationUrl, soapRequest, "", sAppID, sAppSecret);
                sw.Stop();

                _log.DebugFormat("ValidateSession response for : [{0}]; Time(ms) {1}", soapRes, sw.ElapsedMilliseconds);

                if (string.IsNullOrEmpty(soapRes) || string.IsNullOrEmpty(soapRes.Trim()))
                {
                    return res;
                    //Logger.Log("PreSignIn", error);
                }

                //string resCode = GetSubField(soapRes, "data");

                bool isSessionValid = soapRes.ToLower().Contains(WS_SESSION_VALID_MSG.ToLower());

                res.is_succeeded = isSessionValid; //resCode.ToLower().Equals(WS_SESSION_VALID_MSG.ToLower());
                res.reason = res.is_succeeded ? M1_API_ResponseReason.OK : M1_API_ResponseReason.SESSION_TOKEN_INVALID;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                res.description = "Exception: " + ex.Message + " || [" + ex.StackTrace + "]";
            }

            return res;
        }

        // CanAccessVAS authorization web service allows application to invoke a call to check if 
        // the customer is eligible to subscribe or is already a subscriber to a Value Added service.
        private bool IsAuthorized(string userID, string policyID, string appID, string appSecret)
        {
            try
            {
                string soapRequest = string.Format(XML_CAN_ACCESS_VAS, userID, policyID, appID, appSecret);

                string reqNoWhitespaces = XElement.Parse(soapRequest).ToString(SaveOptions.DisableFormatting);

                _log.DebugFormat("Checking CanAccessVas - URL: {0}, Request: [{1}]", m_sWsAdsUrl, reqNoWhitespaces);

                Stopwatch sw = Stopwatch.StartNew();
                string soapRes = Http.SendXMLHttpReq(m_sWsAdsUrl, soapRequest, "", appID, appSecret);
                sw.Stop();

                if (string.IsNullOrEmpty(soapRes) || string.IsNullOrEmpty(soapRes.Trim()))
                {
                    _log.ErrorFormat("ERROR> Empty response from CanAccessVas API, Group ID: {0}, Auth URL: '{1}'", m_nGroupID, m_sWsAdsUrl);
                    return false;
                }

                //soapRes = XElement.Parse(soapRes).ToString(SaveOptions.DisableFormatting);
                //string resCode = GetSubField(soapRes, "data");

                _log.DebugFormat("CanAccessVas response for User ID {0}: [{1}]; Time(ms) {2}", userID, soapRes, sw.ElapsedMilliseconds);

                bool canAccess = soapRes.ToLower().Contains(WS_AUTH_SUCCESS_MSG.ToLower());
                return canAccess;

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                _log.ErrorFormat("Exception: {0} || [{1}]", ex.Message, ex.StackTrace);
            }

            return false;
        } 

        public ADSResponse CanAccessVas(string sUserID)
        {
            ADSResponse retValue = new ADSResponse
            {
                is_succeeded = true,
                reason = M1_API_ResponseReason.OK
            };


            try
            {
                foreach (M1_ServiceParameters sp in m_lPrimaryActions)
                {
                    if (IsAuthorized(userID: sUserID, policyID: sp.m_sCanAccessVasServiceID, appID: m_sAppID, appSecret: m_sAppPassword)) { continue; }

                    string desc = string.Format("Authorization denied from M1 canAccessVas for: Msisdn:{0}, appID:{1}, serviceID:{2}", sUserID, m_sAppID, sp.m_sCanAccessServiceID);
                        //string desc = string.Format("Failure result from canAccessVas() method:{0}, UserID:{1}, appID:{2}, serviceID:{3}", result, sUserID, m_sAppID, sp.m_sCanAccessVasServiceID);
                    
                    return (new ADSResponse(M1_API_ResponseReason.CAN_ACCESS_VAS_SERVICE_FAILURE, false, desc));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }

            return retValue;
        }

        public ADSResponse CheckBlackList(string sMsisdn)
        {
            ADSResponse retValue = new ADSResponse
            {
                is_succeeded = true,
                reason = M1_API_ResponseReason.OK
            };

            try
            {
                string inputXML = string.Format(XML_GET_CUST_BLACK_LIST, sMsisdn);
                MobileOneBusinessFascadeService.StandardStringArrayResult proximityResult = CallProximityAction(inputXML);

                if (proximityResult != null && proximityResult.WebServiceResult != null && proximityResult.WebServiceResult.ErrorCode != 0)
                {
                    string desc = string.Format("Transaction failed result from MobileOneBusinessFascadeService.ServiceInterface.Execute(), error code:{0}, error message{1}",
                            proximityResult.WebServiceResult.ErrorCode, proximityResult.WebServiceResult.ErrorMessages);
                    
                    return (new ADSResponse(M1_API_ResponseReason.CHECK_BLACK_LIST_ERROR , false, desc));
                }

                if (proximityResult != null && !(proximityResult.Return != null && 
                    proximityResult.Return.Length > 0 && proximityResult.Return[0].Contains("<BLACK_LIST>false</BLACK_LIST>")))
                {
                    return (new ADSResponse(M1_API_ResponseReason.USER_BLACKLISTED, false));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }
            return retValue;
        }

        private ADSResponse CreateDummyVas(string sMsisdn, int nServiceID)
        {
            ADSResponse retValue = new ADSResponse
            {
                is_succeeded = true,
                reason = M1_API_ResponseReason.OK
            };

            try
            {

                string inputXML = string.Format(XML_CREATE_DUMMY_VAS, sMsisdn, nServiceID);
                MobileOneBusinessFascadeService.StandardStringArrayResult proximityResult = CallProximityAction(inputXML);

                if (proximityResult != null && proximityResult.WebServiceResult != null && proximityResult.WebServiceResult.ErrorCode != 0)
                {
                    if (proximityResult.WebServiceResult.ErrorCode != DUMMY_VAS_ALREADY_EXIST_ERROR_CODE) // if dummy vas already exist the purchase process pass
                    {
                        string desc = string.Format("Transaction failed result from MobileOneBusinessFascadeService.ServiceInterface.Execute(), error code:{0}, error message{1}",
                            proximityResult.WebServiceResult.ErrorCode, proximityResult.WebServiceResult.ErrorMessages);

                        return (new ADSResponse(M1_API_ResponseReason.CREATE_DUMMY_VAS_ERROR , false, desc));
                    }
                }              

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }
            return retValue;
        }

        private ADSResponse RemoveDummyVas(string sMsisdn, int nCustomerServiceID)
        {
            ADSResponse retValue = new ADSResponse
            {
                is_succeeded = true,
                reason = M1_API_ResponseReason.OK
            };

            try
            {
                string inputXML = string.Format(XML_REMOVE_DUMMY_VAS, sMsisdn, nCustomerServiceID);
                MobileOneBusinessFascadeService.StandardStringArrayResult proximityResult = CallProximityAction(inputXML);

                if (proximityResult != null && proximityResult.WebServiceResult != null && proximityResult.WebServiceResult.ErrorCode != 0)
                {
                    string desc = string.Format("Transaction failed result from MobileOneBusinessFascadeService.ServiceInterface.Execute(), error code:{0}, error message{1}",
                                            proximityResult.WebServiceResult.ErrorCode, proximityResult.WebServiceResult.ErrorMessages);

                    return (new ADSResponse(M1_API_ResponseReason.REMOVE_DUMMY_VAS_ERROR , false, desc));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }
            return retValue;
        }

        public ADSResponse RemoveDummyVas(string sMsisdn, string sCustomerServiceID)
        {
            ADSResponse retValue;

            try
            {
                int nServiceID = 0;
                if (m_lPrimaryActions != null)
                {
                    M1_ServiceParameters m1ServiceParam = m_lActions.Find(x => (x.m_sCanAccessServiceID == sCustomerServiceID));
                    nServiceID = m1ServiceParam.m_nServiceID;
                }

                retValue = RemoveDummyVas(sMsisdn, nServiceID);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return (new ADSResponse(M1_API_ResponseReason.GENERAL_ERROR, false, ",exception:" + ex.Message + " || " + ex.StackTrace));
            }

            return retValue;
        }

        private MobileOneBusinessFascadeService.StandardStringArrayResult CallProximityAction(string inputXml)
        {

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            SingleSignOnService.ServiceFacade oSSOService = new SingleSignOnService.ServiceFacade
            {
                Url = m_sWsServiceFacadeUrl
            };


            //    "GetAuthenticationTicketForClients: PROXIMITY_WS_FACADE_USER_NAME: " + PROXIMITY_WS_FACADE_USER_NAME +
            //                    " PROXIMITY_WS_FACADE_PASSWORD: " + PROXIMITY_WS_FACADE_PASSWORD + 
            //                    " PROXIMITY_WS_FACADE_ACCOUNT_TYPE: " + PROXIMITY_WS_FACADE_ACCOUNT_TYPE,
            //    M1_ADSWRAPPER_LOG_FILE);

            SingleSignOnService.GetAuthenticationTicketResult oTicket = oSSOService.GetAuthenticationTicketForClients(m_sWsFacadeUsername, m_sWsFacadePassword, m_sWsFacadeAccountType);

            //// to check return result
            if (oTicket != null && oTicket.ActionResult != null && oTicket.ActionResult.ErrorCode != 0)
            {
                //string exMsg = string.Format("Failure result from oSSOService.GetAuthenticationTicketForClients() error code:{0}", oTicket.ActionResult.ErrorCode);
                throw new Exception();
                //                return (new ADSResponse(M1_API_ResponseReason.SINGLESIGNON_SERVICE_TICKET_ERROR, false, string.Format("Failure result from oSSOService.GetAuthenticationTicketForClients() error code:{0}", oTicket.ActionResult.ErrorCode)));
            }

            //create the general service interface object
            MobileOneBusinessFascadeService.ServiceInterface myInterface = new MobileOneBusinessFascadeService.ServiceInterface {Url = m_sWsServiceInterfaceUrl};
            //point to correct back-end server
            //myInterface.Url = myInterface.Url.Replace("localhost", this.serverName);

            ////create authentication object
            MobileOneBusinessFascadeService.AuthenticationSoapHeader clTicket = new MobileOneBusinessFascadeService.AuthenticationSoapHeader();
            
            if (oTicket != null) clTicket.Credential = oTicket.AuthTicket;
            myInterface.AuthenticationSoapHeaderValue = clTicket;
            
            ////invoke        
            string channelId = m_sWsInterfaceChannelId;
            string userID = PROXIMITY_WS_INTERFACE_USER_ID;
            bool onErrorContinue = false;

            //    " userID= " + PROXIMITY_WS_INTERFACE_USER_ID
            //    , M1_ADSWRAPPER_LOG_FILE);
            MobileOneBusinessFascadeService.StandardStringArrayResult currResult = myInterface.Execute(inputXml, channelId, userID, onErrorContinue);


            return currResult;
        }

        //private bool CheckCanAccessVasResult(string[] result)
        //{
        //    if (!(result != null && result.Length > 0 && result[0] == WS_AUTH_SUCCESS_MSG))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
    }

    public struct M1_ServiceParameters
    {
        public int m_nServiceID;
        public string m_sCanAccessServiceID;
        public string m_sCanAccessVasServiceID;
    }

    public class ADSResponse
    {
        public ADSResponse()
        {
            description = string.Empty;
        }

        public ADSResponse(M1_API_ResponseReason r, bool se)
        {
            reason = r;
            is_succeeded = se;
            description = string.Empty;
        }

        public ADSResponse(M1_API_ResponseReason r, bool se, string desc)
        {
            reason = r;
            is_succeeded = se;
            description = desc;
        }

        public M1_API_ResponseReason reason { get; set; }

        public bool is_succeeded { get; set; }

        public string description { get; set; }
    }


}
