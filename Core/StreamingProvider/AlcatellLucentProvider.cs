using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Xml;
using ApiObjects;
using ApiObjects.Epg;
using ConfigurationManager;
using KLogMonitor;
using TVinciShared;

namespace StreamingProvider
{
    public class AlcatellLucentProvider : BaseLSProvider
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(ApplicationConfiguration.NPVRHttpClientConfiguration);

        public AlcatellLucentProvider()
            : base()
        {
        }

        /*return dynamic VOD link*/
        public override string GenerateVODLink(string vodUrl)
        {
            string url = string.Empty;
            try
            {
                url = GetDynamicURL(vodUrl);
                return url;
            }
            catch (Exception ex)
            {
                log.Error("get VOD Url link - " + string.Format("fail getting the vod dynamic url - return empty url originalUrl ={0},  ex ={1}", vodUrl, ex.Message), ex);
                return string.Empty;
            }
        }

        /**/
        public override string GenerateEPGLink(Dictionary<string, object> dParams)
        {
            string url = string.Empty;
            try
            {
                // Validate inputs
                if (dParams == null || dParams.Count == 0)
                {
                    log.Debug("get epg url link - " + string.Format("get no parameters - return empty url"));
                    return string.Empty;
                }

                # region parameters
                string formatDate = "yyyy-MM-ddTHH:mm:ssZ";
                string host = string.Empty;
                string sStartTime = string.Empty;
                string sEndTime = string.Empty;
                bool isDynamic = dParams.ContainsKey(EpgLinkConstants.IS_DYNAMIC) ? (bool)dParams[EpgLinkConstants.IS_DYNAMIC] : false;

                #endregion

                bool bValid = ValidParameters(dParams); // validate that all needed parameters exsits with values 
                if (bValid)
                {
                    url = dParams[EpgLinkConstants.BASIC_LINK].ToString();

                    DateTime dStart = (DateTime)dParams[EpgLinkConstants.PROGRAM_START];
                    DateTime dEnd = (DateTime)dParams[EpgLinkConstants.PROGRAM_END];
                    sStartTime = GetDateFormat(dStart, formatDate);
                    sEndTime = GetDateFormat(dEnd, formatDate);

                    // replace the LIVE in start date 
                    if (isDynamic)
                    {
                        url = url.Replace("/LIVE/", string.Format("/{0}/", sStartTime));
                    }
                    else
                    {
                        url = url.Replace("=LIVE", string.Format("={0}", sStartTime));
                    }
                    eEPGFormatType format = (eEPGFormatType)dParams[EpgLinkConstants.EPG_FORMAT_TYPE];
                    switch (format)
                    {
                        case eEPGFormatType.Catchup: // replace END with end_date only in Catchup
                            if (isDynamic)
                            {
                                url = url.Replace("/END/", string.Format("/{0}/", sEndTime));
                            }
                            else
                            {
                                url = url.Replace("=END", string.Format("={0}", sEndTime));
                            }
                            break;
                        case eEPGFormatType.StartOver:
                        case eEPGFormatType.LivePause:
                            break;
                        default:
                            {
                                return string.Empty;
                            }
                    }

                    if (isDynamic)
                    {
                        url = GetDynamicURL(url);
                    }
                }
                else
                {
                    // to do write to log
                    log.Debug("get epg url link - " + string.Format("nmissing some parameters can't generate url  - return empty url"));
                    return string.Empty;
                }

                return url;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at AlcatellLucentProvider GenerateLink. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));
                log.Error("GenerateLink - " + sb.ToString(), ex);
                return string.Empty;
            }
        }

        //private bool ValidParameters(Dictionary<string, object> dParams)
        //{  
        //    if (!dParams.ContainsKey(EpgLinkConstants.BASIC_LINK)) 
        //    {
        //        return false;
        //    }
        //    if (string.IsNullOrEmpty(dParams[EpgLinkConstants.BASIC_LINK].ToString()))
        //    {
        //        return false;
        //    }


        //    if (!dParams.ContainsKey(EpgLinkConstants.PROGRAM_START) ||  !dParams.ContainsKey(EpgLinkConstants.PROGRAM_END))
        //    {
        //        return false;
        //    }
        //    if (dParams[EpgLinkConstants.PROGRAM_START] == null || dParams[EpgLinkConstants.PROGRAM_END] == null)
        //    {
        //        return false;
        //    }
        //    if (!dParams.ContainsKey(EpgLinkConstants.EPG_FORMAT_TYPE))
        //    {
        //        return false;
        //    }
        //    if (dParams[EpgLinkConstants.EPG_FORMAT_TYPE] == null)
        //    {
        //        return false;
        //    }

        //   return true;
        //}

        private string GetDynamicURL(string url)
        {
            string dynamicURL = string.Empty;
            try
            {
                // request AlcatellLucentProvider for response (get XML to parse the url from it)
                string sXml = SendGetXMLHttpReq(url);
                if (!string.IsNullOrEmpty(sXml))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(sXml);

                    XmlNodeList node = doc.SelectNodes("/rolling_buffer_allocation_result");
                    if (node != null && node.Count > 0)
                    {
                        XmlNode urlNode = node[0];
                        string sUrlNode = TVinciShared.XmlUtils.GetNodeValue(ref urlNode, "url");

                        if (string.IsNullOrEmpty(url))
                        {
                            //to do write to log
                            log.Debug("get dynamic url link - " + string.Format("the url return from httpRequest sendUrl ={0},  to AlcatellLucent is empty", url));
                            return string.Empty;
                        }
                        dynamicURL = sUrlNode;
                    }
                }
                else
                {
                    //to do write to log
                    log.Debug("get dynamic url link - " + string.Format("AlcatellLucent response to the url ={0} is null or empty", url));
                    dynamicURL = string.Empty;
                }

                return dynamicURL;
            }
            catch (Exception ex)
            {
                //to do write to log
                log.Error("GetDynamicURL - " + string.Format("AlcatellLucent fail to get response from XMLHTTPREQUEST ex ={0}", ex.Message), ex);
                return string.Empty;
            }
        }

        public static string SendGetXMLHttpReq(string url)
        {
            HttpMethod method = HttpMethod.Get;

            HttpRequestMessage request = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "text/xml; charset=utf-8")
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            try
            {
                using (var response = httpClient.SendAsync(request).ExecuteAndWait())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        log.Error($"XML Http request not successful. url = {url}, status = {response.StatusCode}");
                    }

                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error when sending XML http request. url = {url} ex={ex}");
            }

            return string.Empty;
        }

        private string GetDateFormat(DateTime dateTime, string formatDate)
        {
            if (dateTime != null)
            {
                return dateTime.ToString(formatDate);
            }
            return string.Empty;
        }
    }
}
