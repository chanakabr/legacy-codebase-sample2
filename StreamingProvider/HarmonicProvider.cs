using ApiObjects;
using ApiObjects.Epg;
using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StreamingProvider
{
    public class HarmonicProvider : BaseLSProvider
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int LEFT_MARGIN = 3;
        private const int RIGHT_MARGIN = 8;

        public HarmonicProvider()
            : base()
        {
        }

        public override string GenerateVODLink(string vodUrl)
        {
            return string.Empty;
        }

        public override string GenerateEPGLink(Dictionary<string, object> dParams)
        {
            string url = string.Empty;
            try
            {
                Dictionary<string, object> parametersToInjectInUrl = new Dictionary<string, object>();//dictinary  all parameters that need to be initialized in url
                string host = string.Empty;

                bool bValid = ValidParameters(dParams);
                // sBasicLink

                host = (new Uri(dParams[EpgLinkConstants.BASIC_LINK].ToString())).Host;
                if (!string.IsNullOrEmpty(host))
                {
                    parametersToInjectInUrl.Add(EpgLinkConstants.HOST, host);
                    parametersToInjectInUrl.Add(EpgLinkConstants.CHANNEL_NAME, dParams[EpgLinkConstants.CHANNEL_NAME]);
                    eEPGFormatType format = (eEPGFormatType)dParams[EpgLinkConstants.EPG_FORMAT_TYPE];

                    eStreamType streamType = GetStreamType(dParams[EpgLinkConstants.BASIC_LINK].ToString());
                    url = GetStreamTypeAndFormatLink(streamType, format); // Getting the url which matches both the epg format and the stream type
                    #region calculate start + end date
                    DateTime dStart = (DateTime)dParams[EpgLinkConstants.PROGRAM_START];
                    DateTime dEnd = (DateTime)dParams[EpgLinkConstants.PROGRAM_END];

                    long nStartTime = 0;
                    long nEndTime = 0;
                    int nRightMargin = 0;
                    int nLeftMargin = 0;
                    // Time Factor for aligment with Harmonic server (e.g. convert millisec -> 10Xmicrosec)
                    int timeMultFactor = 10000;

                    timeMultFactor = (int)(dParams[EpgLinkConstants.TIME_MULT_FACTOR]);
                    nRightMargin = (int)dParams[EpgLinkConstants.RIGHT_MARGIN];
                    nLeftMargin = (int)dParams[EpgLinkConstants.LEFT_MARGIN];

                    switch (format)
                    {
                        case eEPGFormatType.Catchup:
                        case eEPGFormatType.StartOver:
                            {
                                nStartTime = (timeMultFactor * ConvertDateToEpochTimeInMilliseconds(dStart.ToUniversalTime().AddMinutes(nLeftMargin)));
                                nEndTime = (timeMultFactor * ConvertDateToEpochTimeInMilliseconds(dEnd.ToUniversalTime().AddMinutes(nRightMargin)));
                            }
                            break;

                        case eEPGFormatType.LivePause:
                            DateTime startTimeUTC = dStart.ToUniversalTime();
                            if (DateTime.Compare(startTimeUTC, DateTime.UtcNow) <= 0)
                            {
                                nStartTime = (timeMultFactor * ConvertDateToEpochTimeInMilliseconds(startTimeUTC.AddMinutes(nLeftMargin)));
                                nEndTime = (timeMultFactor * ConvertDateToEpochTimeInMilliseconds(dEnd.AddMinutes(nRightMargin)));
                            }
                            break;
                        default:
                            {
                                return string.Empty;
                            }
                    }

                    parametersToInjectInUrl.Add(EpgLinkConstants.PROGRAM_START, nStartTime);
                    parametersToInjectInUrl.Add(EpgLinkConstants.PROGRAM_END, nEndTime);


                    #endregion

                    if (!string.IsNullOrEmpty(url))
                    {
                        ReplaceSubStr(ref url, parametersToInjectInUrl);
                    }
                }

                return url;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at HarmonicProvider GenerateLink. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));
                log.Error("GetEPGLink - " + sb.ToString(), ex);
                return string.Empty;
            }
        }

        private string GetStreamTypeAndFormatLink(eStreamType streamType, eEPGFormatType format)
        {
            string url = string.Empty;
            switch (format)
            {
                case eEPGFormatType.Catchup:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.HLSCatchup.Value;                                
                                break;
                            case eStreamType.SS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.SmoothCatchup.Value; 
                                break;
                            case eStreamType.DASH:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.DashCatchup.Value; 
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case eEPGFormatType.StartOver:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.HLSStartOver.Value; 
                                break;
                            case eStreamType.SS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.SmoothStartOver.Value;
                                break;
                            case eStreamType.DASH:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.DashStartOver.Value; 
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case eEPGFormatType.LivePause:
                    {
                        switch (streamType)
                        {
                            case eStreamType.HLS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.HLSStartOver.Value; 
                                break;
                            case eStreamType.SS:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.SmoothStartOver.Value; 
                                break;
                            case eStreamType.DASH:
                                url = ApplicationConfiguration.HarmonicProviderConfiguration.DashStartOver.Value;
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }

            return url;
        }


        private eStreamType GetStreamType(string sBaseLink)
        {
            eStreamType streamType = eStreamType.HLS;

            if ((sBaseLink.ToLower().Contains("ism")) && (sBaseLink.ToLower().Contains("manifest")))
            {
                streamType = eStreamType.SS;
            }
            else if (sBaseLink.Contains(".m3u8"))
            {
                streamType = eStreamType.HLS;
            }
            else if (sBaseLink.Contains(".mpd"))
            {
                streamType = eStreamType.DASH;
            }

            return streamType;
        }

        private long ConvertDateToEpochTimeInMilliseconds(DateTime dateTime)
        {
            return long.Parse((Math.Floor(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString()));
        }


        protected override bool ValidParameters(Dictionary<string, object> dParams)
        {
            bool res = base.ValidParameters(dParams);
            if (res)
            {
                if (!dParams.ContainsKey(EpgLinkConstants.CHANNEL_NAME))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(dParams[EpgLinkConstants.CHANNEL_NAME].ToString()))
                {
                    return false;
                }

                if (!dParams.ContainsKey(EpgLinkConstants.TIME_MULT_FACTOR) || !dParams.ContainsKey(EpgLinkConstants.RIGHT_MARGIN) || !dParams.ContainsKey(EpgLinkConstants.LEFT_MARGIN))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
