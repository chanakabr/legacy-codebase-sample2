using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace ConditionalAccess
{
    class VodafoneConditionalAccess : TvinciConditionalAccess
    {
        private const int LEFT_MARGIN = 3;
        private const int RIGHT_MARGIN = 8;
        private static readonly string UNREACHABLE_ERROR = "Unable to connect to the billing server";

         public VodafoneConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

         public VodafoneConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

     /*    public override string GetEPGLink(int nProgramId, DateTime startTime, eEPGFormatType format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, 
             string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
         {
             // Validate inputs
             if ((nProgramId <= 0) || (string.IsNullOrEmpty(sBasicLink)) || (string.IsNullOrEmpty(sSiteGUID)))
             {
                 return string.Empty;
             }
            TvinciAPI.API api = null;
            string url = string.Empty;

            try
            {
                # region parameters
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;

                string formatDate = "yyyy-MM-ddThh:mm:ssZ";
                string sStartTime = string.Empty;
                string sEndTime = string.Empty;
                string sChannelName = string.Empty;
                string host = string.Empty;
                url = "http://{prefix}/rolling_buffer/{channel_name}/{Start}/{END}/{Device_Profile}";
                Dictionary<string, object> parametersToInjectInUrl = new Dictionary<string, object>();//dictinary  all parameters that need to be initialized in url
                #endregion

                host = (new Uri(sBasicLink)).Host;
                if (!string.IsNullOrEmpty(host))
                {
                    parametersToInjectInUrl.Add("prefix", host);

                    string sBaseLink = GetLicensedLink(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sCouponCode);
                    //GetLicensedLink return empty link no need to continue
                    if (string.IsNullOrEmpty(sBaseLink))
                    {
                        Logger.Logger.Log("LicensedLink",
                            string.Format("GetLicensedLink return empty basicLink siteGuid={0}, sBasicLink={1}, nMediaFileID={2}", sSiteGUID, sBasicLink, nMediaFileID), "LicensedLink");
                        return string.Empty;
                    }

                    api = new TvinciAPI.API();
                    string sApiWSUrl = Utils.GetWSURL("api_ws");
                    if (!string.IsNullOrEmpty(sApiWSUrl))
                    {
                        api.Url = sApiWSUrl;
                    }
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.API, "GetEPGLink", ref sWSUserName, ref sWSPass);

                    // get channelName from Api
                    sChannelName = api.GetCoGuidByMediaFileId(sWSUserName, sWSPass, nMediaFileID);

                    if (!string.IsNullOrEmpty(sChannelName))
                    {
                        parametersToInjectInUrl.Add("channel_name", sChannelName);

                        // get Device_Profile (=filetype) by filetid  - Api service                    
                        string fileTypeDescription = api.GetMediaFileTypeDescription(sWSUserName, sWSPass, nMediaFileID);
                        if (!string.IsNullOrEmpty(fileTypeDescription))
                        {
                            parametersToInjectInUrl.Add("Device_Profile", fileTypeDescription);
                        }

                        ConditionalAccess.TvinciAPI.Scheduling scheduling = api.GetProgramSchedule(sWSUserName, sWSPass, nProgramId);
                        if (scheduling != null)
                        {
                            switch (format)
                            {
                                case eEPGFormatType.Catchup:
                                case eEPGFormatType.StartOver:
                                    {
                                        if (scheduling != null)
                                        {
                                            //get date in yyyy-MM-ddThh:mm:ssZ format 
                                            sStartTime = GetDateFormat(scheduling.StartDate, formatDate);
                                            sEndTime = GetDateFormat(scheduling.EndTime, formatDate);

                                            parametersToInjectInUrl.Add("Start", sStartTime);
                                            parametersToInjectInUrl.Add("END", sEndTime);
                                        }
                                    }

                                    break;

                                case eEPGFormatType.LivePause:
                                    {
                                        sStartTime = GetDateFormat(startTime, formatDate);
                                        sEndTime = GetDateFormat(scheduling.EndTime, formatDate);

                                        parametersToInjectInUrl.Add("Start", sStartTime);
                                        parametersToInjectInUrl.Add("END", sEndTime);
                                    }

                                    break;

                                default:
                                    url = string.Empty;
                                    break;
                            }

                            // Injecting the parameters values to the url
                            if (!string.IsNullOrEmpty(url))
                            {
                                Utils.ReplaceSubStr(ref url, parametersToInjectInUrl);
                            }
                        }
                    }
                    else
                    {
                        url = string.Empty;
                    }
                }
                else
                {
                    url = string.Empty;
                }
                return url;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetEPGLink. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Program ID: ", nProgramId));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Start time: ", startTime.ToString()));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Coupon: ", sCouponCode));
                sb.Append(String.Concat(" Format: ", format.ToString()));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));
                Logger.Logger.Log("LicensedLink", sb.ToString(), "LicensedLink");
            }
             finally
             {
                 if (api != null)
                 {
                     api.Dispose();
                 }
             }
            return url;
         }*/

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
