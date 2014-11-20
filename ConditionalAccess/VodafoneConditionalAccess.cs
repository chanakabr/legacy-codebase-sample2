using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class VodafoneConditionalAccess : TvinciConditionalAccess
    {
        private static readonly string VODAFONE_NPVR_LOG = "VodafoneNPVR";

        public VodafoneConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        // here assetID will be the epg program id as appearing in epg_channels_schedule in CB.
        public override NPVRResponse RecordNPVR(string siteGuid, string assetID, bool isSeries)
        {
            NPVRResponse res = new NPVRResponse();
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID) && domainID > 0)
                {
                    string assetIDToALU = isSeries ? assetID : GetEpgProgramCoGuid(m_nGroupID, assetID);
                    if (!string.IsNullOrEmpty(assetIDToALU))
                    {

                    }
                    else
                    {
                        // asset id is invalid
                        Logger.Logger.Log("RecordNPVR", GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetIDToALU), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
                        res.status = NPVRStatus.InvalidAssetID.ToString();
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
                    string assetIDToALU = isSeries ? assetID : GetEpgProgramCoGuid(m_nGroupID, assetID);
                    if (!string.IsNullOrEmpty(assetIDToALU))
                    {

                    }
                    else
                    {
                        // asset id is invalid
                        Logger.Logger.Log("CancelNPVR", GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetIDToALU), siteGuid, assetID, isSeries, null), VODAFONE_NPVR_LOG);
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
                    string assetIDToALU = isSeries ? assetID : GetEpgProgramCoGuid(m_nGroupID, assetID);
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


        private string GetEpgProgramCoGuid(int groupID, string assetID)
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
                epdr.m_nGroupID = groupID;
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
