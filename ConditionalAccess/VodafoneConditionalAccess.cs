using NPVR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class VodafoneConditionalAccess : TvinciConditionalAccess
    {
        private static readonly string VODAFONE_NPVR_LOG = "VodafoneNPVR";
        private static readonly DateTime UNIX_ZERO_TIME = new DateTime(1970, 1, 1, 0, 0, 0);

        public VodafoneConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        // here assetID will be the epg program id as appearing in epg_channels_schedule in CB.
        public override RecordResponse RecordNPVR(string siteGuid, string assetID, bool isSeries)
        {
            RecordResponse res = new RecordResponse();
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID) && domainID > 0)
                {
                    if (isSeries)
                    {
                        // series
                    }
                    else
                    {
                        // single asset
                        string epgChannelID = string.Empty;
                        DateTime programStartDate = DateTime.MinValue;
                        string assetIDToALU = GetEpgProgramCoGuid(assetID, ref epgChannelID, ref programStartDate);
                        if (!string.IsNullOrEmpty(assetIDToALU) && !string.IsNullOrEmpty(epgChannelID) && !programStartDate.Equals(UNIX_ZERO_TIME) && !programStartDate.Equals(DateTime.MinValue))
                        {
                            INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                            if (npvr != null)
                            {
                                NPVRRecordResponse response = npvr.RecordAsset(new NPVRParamsObj() { AssetID = assetIDToALU, StartDate = programStartDate, EpgChannelID = epgChannelID, EntityID = domainID.ToString() });
                                if (response != null)
                                {
                                    switch (response.status)
                                    {
                                        case RecordStatus.OK:
                                            res.status = NPVRStatus.OK.ToString();
                                            res.recordingID = response.recordingID;
                                            break;
                                        case RecordStatus.AlreadyRecorded:
                                            res.status = NPVRStatus.AssetAlreadyScheduled.ToString();
                                            res.recordingID = string.Empty;
                                            break;
                                        case RecordStatus.Error:
                                            res.status = NPVRStatus.Error.ToString();
                                            res.recordingID = string.Empty;
                                            break;
                                        default:
                                            Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg(String.Concat("Unidentified RecordStatus: ", response.status.ToString()), siteGuid, assetID, false, null), VODAFONE_NPVR_LOG);
                                            res.status = NPVRStatus.Unknown.ToString();
                                            res.recordingID = string.Empty;
                                            break;
                                    }
                                }
                                else
                                {
                                    Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg("Response returned from NPVR layer is null.", siteGuid, assetID, false, null), VODAFONE_NPVR_LOG);
                                    res.status = NPVRStatus.Error.ToString();
                                }
                            }
                            else
                            {
                                Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg("Failed to instantiate an INPVRProvider instance.", siteGuid, assetID, false, null), VODAFONE_NPVR_LOG);
                                res.status = NPVRStatus.Error.ToString();
                            }
                        }
                        else
                        {
                            // asset id or epg channel id or program start date is invalid
                            Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg(String.Concat("Either ALU Asset ID: ", assetIDToALU, " or Epg Channel ID: ", epgChannelID, " or StartDate: ", programStartDate.ToString(), " is invalid."), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                            res.status = NPVRStatus.InvalidAssetID.ToString();
                        }
                    }

                }
                else
                {
                    // either user or domain is invalid
                    Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", GetNPVRLogMsg("Exception at RecordNPVR", siteGuid, assetID, isSeries, ex), VODAFONE_NPVR_LOG);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }

        // here asset ID will be the recording ID in ALU.
        public override NPVRResponse CancelNPVR(string siteGuid, string assetID, bool isSeries)
        {
            NPVRResponse res = new NPVRResponse();
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID) && domainID > 0)
                {
                    if (!string.IsNullOrEmpty(assetID))
                    {

                    }
                    else
                    {
                        // asset id is invalid
                        Logger.Logger.Log("CancelNPVR", GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetID), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                        res.status = NPVRStatus.InvalidAssetID.ToString();
                    }
                }
                else
                {
                    // either user or domain is invalid
                    Logger.Logger.Log("CancelNPVR", GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", GetNPVRLogMsg("Exception at CancelNPVR", siteGuid, assetID, isSeries, ex), VODAFONE_NPVR_LOG);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;

        }

        // here assetID will be the recording ID in ALU
        public override NPVRResponse DeleteNPVR(string siteGuid, string assetID, bool isSeries)
        {
            NPVRResponse res = new NPVRResponse();
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID) && domainID > 0)
                {
                    string assetIDToALU = assetID;
                    if (!string.IsNullOrEmpty(assetIDToALU))
                    {

                    }
                    else
                    {
                        // asset id is invalid
                        Logger.Logger.Log("DeleteNPVR", GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetIDToALU), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                        res.status = NPVRStatus.InvalidAssetID.ToString();
                    }
                }
                else
                {
                    // either user or domain is invalid
                    Logger.Logger.Log("DeleteNPVR", GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", GetNPVRLogMsg("Exception at DeleteNPVR", siteGuid, assetID, isSeries, ex), VODAFONE_NPVR_LOG);
                res.status = NPVRStatus.Error.ToString();

            }
            return res;
        }

        public override QuotaResponse GetNPVRQuota(string siteGuid)
        {
            QuotaResponse res = new QuotaResponse();
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID) && domainID > 0)
                {
                    INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                    if (npvr != null)
                    {
                        NPVRQuotaResponse response = npvr.GetQuotaData(new NPVRParamsObj() { EntityID = domainID.ToString() });
                        if (response != null)
                        {
                            res.totalQuota = response.totalQuota;
                            res.occupiedQuota = response.usedQuota;
                            res.status = NPVRStatus.OK.ToString();
                        }
                        else
                        {
                            // log here response is null.
                            Logger.Logger.Log("Error", GetNPVRLogMsg(String.Concat("GetNPVRQuota. NPVR layer response is null. D ID: ", domainID), siteGuid, string.Empty, false, null), VODAFONE_NPVR_LOG);
                            res.status = NPVRStatus.Error.ToString();
                        }
                    }
                    else
                    {
                        Logger.Logger.Log("Error", GetNPVRLogMsg("GetNPVRQuota. Failed to instantiate INPVRProvider instance.", siteGuid, string.Empty, false, null), VODAFONE_NPVR_LOG);
                        res.status = NPVRStatus.Error.ToString();
                    }
                }
                else
                {
                    // log here user does not exist or no domain id.
                    Logger.Logger.Log("Error", GetNPVRLogMsg(String.Concat("GetNPVRQuota. Either user or domain is not valid. D ID: ", domainID), siteGuid, string.Empty, false, null), VODAFONE_NPVR_LOG);
                    res.status = NPVRStatus.Error.ToString();

                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", GetNPVRLogMsg("Exception at GetNPVRQuota.", siteGuid, string.Empty, false, ex), VODAFONE_NPVR_LOG);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }


        private string GetEpgProgramCoGuid(string assetID, ref string epgChannelID, ref DateTime startDate)
        {
            WS_Catalog.IserviceClient client = null;
            int progID = 0;
            string res = string.Empty;
            if (!Int32.TryParse(assetID, out progID) || progID < 1)
            {
                return string.Empty;
            }
            try
            {
                string catalogUrl = Utils.GetWSURL("WS_Catalog");
                if (string.IsNullOrEmpty(catalogUrl))
                {
                    throw new Exception("Catalog address is not configured. ");
                }
                client = new WS_Catalog.IserviceClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(catalogUrl);
                WS_Catalog.EpgProgramDetailsRequest epdr = new WS_Catalog.EpgProgramDetailsRequest();
                epdr.m_nGroupID =m_nGroupID;
                epdr.m_oFilter = new WS_Catalog.Filter();
                epdr.m_lProgramsIds = new int[1] { progID };
                Utils.FillCatalogSignature(epdr);
                WS_Catalog.EpgProgramResponse resp = client.GetProgramsByIDs(epdr) as WS_Catalog.EpgProgramResponse;
                if (resp != null && resp.m_lObj != null && resp.m_lObj.Length > 0 && resp.m_lObj[0] != null)
                {
                    WS_Catalog.ProgramObj prog = resp.m_lObj[0] as WS_Catalog.ProgramObj;
                    if (prog != null && prog.m_oProgram != null)
                    {
                        res = prog.m_oProgram.EPG_IDENTIFIER;
                        epgChannelID = prog.m_oProgram.EPG_CHANNEL_ID;
                        if (!DateTime.TryParseExact(prog.m_oProgram.START_DATE, "yyyyMMddHHmmss", new CultureInfo("de-DE"), DateTimeStyles.None, out startDate))
                        {
                            // failed to parse date.

                        }
                    }
                }

            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

            return res;
        }
    }
}
