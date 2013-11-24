//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Services;
//using System.Web.Services.Protocols;
//using Newtonsoft.Json;

//namespace TVPApiTests
//{
//    public class TestHelper
//    {

//        SoapHttpClientProtocol[] webServices = new SoapHttpClientProtocol[]
//            {
//                new MediaService.MediaService(),
//                new BillingService.BillingService(),
//                new ConditionalAccessService.ConditionalAccessService(),
//                new DomainService.DomainService(),
//                new NotificationService.NotificationService(),
//                new PricingService.PricingService(),
//                new SiteService.SiteService(),
//                new SocialService.SocialService(),
//                new UsersService.UsersService()            
//            };

//        string sInitObj = @"{{
//                ""initObj"": {{
//                    ""Locale"": {{
//                        ""LocaleLanguage"": """",
//                        ""LocaleCountry"": """",
//                        ""LocaleDevice"": """",
//                        ""LocaleUserState"": ""Unknown""
//                    }},
//                    ""Platform"": ""iPad"",
//                    ""SiteGuid"": ""419912"",
//                    ""DomainID"": 42546,
//                    ""UDID"": ""3d609818aa3db97bdc02492f9dabc1b632dfd02d"",
//                    ""ApiUser"": ""tvpapi_147"",
//                    ""ApiPass"": ""11111""
//                }}
//                {0}
//            }}";

//        TVPApi.InitializationObject initObj = new TVPApi.InitializationObject()
//        {
//            Platform = TVPApi.PlatformType.iPad,
//            SiteGuid = "419912",
//            ApiUser = "tvpapi_147",
//            ApiPass = "11111",
//            Locale = new TVPApi.Locale()
//            {
//                LocaleLanguage = string.Empty,
//                LocaleCountry = string.Empty,
//                LocaleDevice = string.Empty,
//                LocaleUserState = TVPApi.LocaleUserState.Unknown
//            }
//        };

//        #region Public Methods

//        public void TestSanity(string methodName, List<KeyValuePair<string, object>> parameters, Type expectedResponseType)
//        {
//            try
//            {
//                testJson(methodName, parameters, expectedResponseType);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestSanity, Type: Json, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }

//            try
//            {
//                testSoap(methodName, parameters.Select(x => x.Value).ToList());
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestSanity, Type: Soap, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }
//        }

//        public void TestCount(string methodName, List<KeyValuePair<string, object>> parameters, Type expectedResponseType, int expectedItemCount)
//        {
//            try
//            {
//                object jsonResponse = testJson(methodName, parameters, expectedResponseType);

//                testLength(jsonResponse, expectedItemCount);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestCount, Type: Json, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }

//            try
//            {
//                object soapResponse = testSoap(methodName, parameters.Select(x => x.Value).ToList());

//                testLength(soapResponse, expectedItemCount);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestCount, Type: Soap, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }
//        }

//        public void TestValue(string methodName, List<KeyValuePair<string, object>> parameters, Type expectedResponseType, object expectedValue)
//        {
//            try
//            {
//                object jsonResponse = testJson(methodName, parameters, expectedResponseType);

//                testValue(jsonResponse, expectedValue);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestValue, Type: Json, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }

//            try
//            {
//                object soapResponse = testSoap(methodName, parameters.Select(x => x.Value).ToList());

//                testValue(soapResponse, expectedValue);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(string.Format("Test: TestValue, Type: Soap, Method Name: {0}, Error: {1}", methodName, ex.Message));
//            }
//        }

//        #endregion

//        #region Private Methods

//        private string getJsonResponse(string methodName, List<KeyValuePair<string, object>> requestParams)
//        {
//            HttpWebRequest webRequest = null;
//            WebResponse webResponse = null;

//            string uri = string.Format("{0}/gateways/jsonpostgw.aspx?m={1}", ConfigurationSettings.AppSettings["jsonGateWayURL"], methodName);

//            webRequest = (HttpWebRequest)WebRequest.Create(uri);
//            webRequest.Method = "POST";
//            webRequest.ContentType = "application/json; charset=utf-8;";

//            using (Stream stm = webRequest.GetRequestStream())
//            {
//                using (StreamWriter stmw = new StreamWriter(stm))
//                {
//                    string sRequestParams = string.Empty;

//                    foreach (var requestParam in requestParams)
//                    {
//                        string requestParamValue = JsonConvert.SerializeObject(requestParam.Value);

//                        sRequestParams = string.Format("{0},\"{1}\":{2}", sRequestParams, requestParam.Key, requestParamValue);
//                    }

//                    stmw.Write(string.Format(sInitObj, sRequestParams));
//                }
//            }

//            webResponse = webRequest.GetResponse();

//            StreamReader sr = new StreamReader(webResponse.GetResponseStream());

//            string sResponse = sr.ReadToEnd();

//            sr.Close();

//            webRequest.GetRequestStream().Close();
//            webResponse.GetResponseStream().Close();

//            return sResponse;
//        }

//        private object getSoapResponse(string methodName, List<object> requestParams)
//        {
//            object relevantInitObj = null;

//            MethodInfo methodInfo = null;
//            SoapHttpClientProtocol relevantWebService = null;

//            foreach (SoapHttpClientProtocol webService in webServices)
//            {
//                methodInfo = webService.GetType().GetMethod(methodName);

//                if (methodInfo != null)
//                {
//                    relevantWebService = webService;

//                    string webServiceType = webService.GetType().ToString();

//                    if (webServiceType.EndsWith("MediaService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(MediaService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("BillingService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(BillingService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("ConditionalAccessService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(ConditionalAccessService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("DomainService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(DomainService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("NotificationService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(NotificationService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("PricingService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(PricingService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("SiteService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(SiteService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("SocialService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(SocialService.InitializationObject));
//                    }
//                    else if (webServiceType.EndsWith("UsersService"))
//                    {
//                        relevantInitObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(initObj), typeof(UsersService.InitializationObject));
//                    }

//                    break;
//                }
//            }

//            requestParams.Insert(0, relevantInitObj);

//            if (relevantWebService == null)
//            {
//                throw new Exception("Method not found.");
//            }

//            return methodInfo.Invoke(relevantWebService, requestParams.ToArray());
//        }

//        private object testJson(string methodName, List<KeyValuePair<string, object>> parameters, Type expectedResponseType)
//        {
//            string response = getJsonResponse(methodName, parameters);

//            object jsonResponse = null;

//            try
//            {
//                jsonResponse = JsonConvert.DeserializeObject(response, expectedResponseType);
//            }
//            catch
//            {
//                throw new Exception("response type and expectedResponseType do not match.");
//            }
            
//            if (jsonResponse == null)
//            {
//                throw new Exception("response is null.");
//            }

//            return jsonResponse;
//        }

//        private object testSoap(string methodName, List<object> parameters)
//        {
//            object soapResponse = getSoapResponse(methodName, parameters);

//            if (soapResponse == null)
//            {
//                throw new Exception("response is null.");
//            }

//            return soapResponse;
//        }

//        private void testLength(object obj, int length)
//        {
//            Type type = obj.GetType();

//            if (type.IsArray)
//            {
//                int objLength = (int)type.GetProperty("Length").GetValue(obj);

//                if (length != objLength)
//                {
//                    throw new Exception("Different length.");
//                }
//            }
//        }

//        private void testValue(object obj, object value)
//        {
//            string soapResponseJson = JsonConvert.SerializeObject(obj);
//            string expectedValueJson = JsonConvert.SerializeObject(value);

//            if (soapResponseJson != expectedValueJson)
//            {
//                throw new Exception("Value dismatch.");
//            }
//        }

//        #endregion
//    }
//}
