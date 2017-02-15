using ApiObjects;
using DAL;
using NPVR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using WS_Pricing;
using Pricing;
using Users;


namespace ConditionalAccess
{
    public class VodafoneConditionalAccess : TvinciConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly DateTime UNIX_ZERO_TIME = new DateTime(1970, 1, 1, 0, 0, 0);

        public VodafoneConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override RecordResponse RecordSeriesByProgramID(string siteGuid, string epgProgramIdAssignedToSeries)
        {
            return RecordNPVR(siteGuid, epgProgramIdAssignedToSeries, true);
        }

        public override RecordResponse RecordSeriesByName(string siteGuid, string seriesName)
        {
            RecordResponse res = new RecordResponse();
            try
            {
                string epgProgramIDRelatedToSeries = GetEpgProgramIDRelatedToSeries(seriesName);
                if (!string.IsNullOrEmpty(epgProgramIDRelatedToSeries))
                {
                    res = RecordSeriesByProgramID(siteGuid, epgProgramIDRelatedToSeries);
                }
                else
                {
                    res.status = NPVRStatus.Error.ToString();
                    log.Debug("RecordSeriesByName - " + GetNPVRLogMsg("epg program id related to series is empty", siteGuid, seriesName, false, null));
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at RecordSeriesByName", siteGuid, seriesName, false, ex), ex);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;

        }

        private string GetEpgProgramIDRelatedToSeries(string seriesName)
        {
            WS_Catalog.IserviceClient client = null;
            string res = string.Empty;
            try
            {
                string catalogUrl = Utils.GetWSURL("WS_Catalog");
                if (string.IsNullOrEmpty(catalogUrl))
                {
                    throw new Exception("Catalog address is not configured. ");
                }
                client = new WS_Catalog.IserviceClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(catalogUrl);

                // instantiate here a Catalog request and parse the result.
                throw new NotImplementedException("To be completed after a proper representation of Series in the DB will be defined.");


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

        // here assetID will be the EPG program id as appearing in epg_channels_schedule in CB.
        public override RecordResponse RecordNPVR(string siteGuid, string assetID, bool isSeries)
        {
            RecordResponse res = new RecordResponse();
            DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0)
                {
                    // validate user is not suspended
                    if (suspendStatus != DomainSuspentionStatus.Suspended)
                    {
                        // validate that the service is allowed
                        if (IsServiceAllowed(domainID, eService.NPVR))
                        {
                            // get media files which corresponds to the given asset ID (program ID)
                            List<int> fileIds = DAL.ConditionalAccessDAL.GetFileIdsByEpgProgramId(Convert.ToInt32(assetID), m_nGroupID);

                            // validate that at least one of the file is free/purchased 
                            if (fileIds != null && fileIds.Count > 0)
                            {
                                bool priceValidationPassed = false;

                                //get only files that related to ppv module
                                List<int> files = GetFileList(fileIds, ref priceValidationPassed);

                                if (!priceValidationPassed)
                                {
                                    MediaFileItemPricesContainer[] prices = GetItemsPrices(files.ToArray(), siteGuid, string.Empty, true, string.Empty, string.Empty, string.Empty, string.Empty);
                                    if (prices != null && prices.Length > 0)
                                    {
                                        foreach (var price in prices)
                                        {
                                            if (Utils.IsFreeItem(price) || Utils.IsItemPurchased(price))
                                            {
                                                priceValidationPassed = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (priceValidationPassed)
                                {
                                    string epgChannelID = string.Empty;
                                    DateTime programStartDate = DateTime.MinValue;
                                    string assetIDToALU = GetEpgProgramCoGuid(assetID, ref epgChannelID, ref programStartDate);
                                    if (!string.IsNullOrEmpty(assetIDToALU) && !string.IsNullOrEmpty(epgChannelID) && !programStartDate.Equals(UNIX_ZERO_TIME) && !programStartDate.Equals(DateTime.MinValue))
                                    {
                                        INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                                        if (npvr != null)
                                        {
                                            NPVRRecordResponse response = null;
                                            if (isSeries)
                                                response = npvr.RecordSeries(new NPVRParamsObj() { AssetID = assetIDToALU, StartDate = programStartDate, EpgChannelID = epgChannelID, EntityID = domainID.ToString() });
                                            else
                                                response = npvr.RecordAsset(new NPVRParamsObj() { AssetID = assetIDToALU, StartDate = programStartDate, EpgChannelID = epgChannelID, EntityID = domainID.ToString() });
                                            if (response != null)
                                            {
                                                switch (response.status)
                                                {
                                                    case RecordStatus.OK:
                                                        res.status = NPVRStatus.OK.ToString();
                                                        res.recordingID = response.recordingID;
                                                        break;
                                                    case RecordStatus.ResourceAlreadyExists:
                                                        res.status = NPVRStatus.AssetAlreadyScheduled.ToString();
                                                        res.recordingID = string.Empty;
                                                        break;
                                                    case RecordStatus.Error:
                                                        res.status = NPVRStatus.Error.ToString();
                                                        res.recordingID = string.Empty;
                                                        break;
                                                    case RecordStatus.QuotaExceeded:
                                                        res.status = NPVRStatus.QuotaExceeded.ToString();
                                                        res.recordingID = string.Empty;
                                                        break;
                                                    default:
                                                        log.Debug("RecordNPVR - " + GetNPVRLogMsg(String.Concat("Unidentified RecordStatus: ", response.status.ToString()), siteGuid, assetID, false, null));
                                                        res.status = NPVRStatus.Unknown.ToString();
                                                        res.recordingID = string.Empty;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                log.Debug("RecordNPVR - " + GetNPVRLogMsg("Response returned from NPVR layer is null.", siteGuid, assetID, isSeries, null));
                                                res.status = NPVRStatus.Error.ToString();
                                            }
                                        }
                                        else
                                        {
                                            log.Debug("RecordNPVR - " + GetNPVRLogMsg("Failed to instantiate an INPVRProvider instance.", siteGuid, assetID, isSeries, null));
                                            res.status = NPVRStatus.Error.ToString();
                                        }
                                    }
                                    else
                                    {
                                        // asset id or EPG channel id or program start date is invalid
                                        log.Debug("RecordNPVR - " + GetNPVRLogMsg(String.Concat("Either ALU Asset ID: ", assetIDToALU, " or Epg Channel ID: ", epgChannelID, " or StartDate: ", programStartDate.ToString(), " is invalid."), siteGuid, assetID, false, null));
                                        res.status = NPVRStatus.InvalidAssetID.ToString();
                                    }
                                }
                                else
                                {
                                    // file is not purchased/free
                                    log.Debug("RecordNPVR - " + GetNPVRLogMsg("EPG program ID is not free/purchased.", siteGuid, assetID, false, null));
                                    res.status = NPVRStatus.NotPurchased.ToString();
                                }
                            }
                            else
                            {
                                // couldn't find media files which corresponds to the given asset ID (program ID)
                                log.Debug("RecordNPVR - " + GetNPVRLogMsg("Media file wasn't found", siteGuid, assetID, false, null));
                                res.status = NPVRStatus.Error.ToString();
                            }
                        }
                        else
                        {
                            // NPVR Service is not allowed for domain
                            log.Debug("RecordNPVR - " + GetNPVRLogMsg(String.Concat("NPVR Service is not allowed for Domain ID: ", domainID), siteGuid, assetID, isSeries, null));
                            res.status = NPVRStatus.ServiceNotAllowed.ToString();
                        }
                    }
                    else
                    {
                        // user is suspended
                        res.status = NPVRStatus.Suspended.ToString();
                        res.msg = "User is suspended";
                        log.Debug("RecordNPVR - " + string.Format("User {0} tried to record while in suspended state", siteGuid));
                    }
                }
                else
                {
                    // either user or domain is invalid
                    log.Debug("RecordNPVR - " + GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null));
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at RecordNPVR", siteGuid, assetID, isSeries, ex), ex);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }

        private List<int> GetFileList(List<int> fileIds, ref bool priceValidationPassed)
        {
            try
            {
                List<int> files = new List<int>();
                mdoule objPricingModule = null;
                string sPricingUsername = string.Empty;
                string sPricingPassword = string.Empty;
                MediaFilePPVContainer[] oModules = null;

                Utils.GetWSCredentials(m_nGroupID, eWSModules.PRICING, ref sPricingUsername, ref sPricingPassword);
                base.InitializePricingModule(ref objPricingModule);
                oModules = objPricingModule.GetPPVModuleListForMediaFilesWithExpiry(sPricingUsername, sPricingPassword, fileIds.ToArray(), string.Empty, string.Empty, string.Empty);
                //get only files that are related to any ppvModule 
                if (oModules == null || oModules.Count() == 0)
                {
                    priceValidationPassed = true;
                }
                // Build list of filed that related to ppv module 
                foreach (MediaFilePPVContainer module in oModules)
                {
                    if (module.m_oPPVModules != null)
                    {
                        files.Add(module.m_nMediaFileID);
                    }
                }
                if (files.Count == 0) // no file related to any ppvModule ==> can record media 
                {
                    priceValidationPassed = true;
                }

                return files;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                priceValidationPassed = false;
                return new List<int>();
            }
        }

        // here asset ID will be the recording ID in ALU.
        public override NPVRResponse CancelNPVR(string siteGuid, string assetID, bool isSeries)
        {
            NPVRResponse res = new NPVRResponse();
            DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0)
                {
                    if (!string.IsNullOrEmpty(assetID))
                    {
                        INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                        if (npvr != null)
                        {
                            NPVRCancelDeleteResponse response = null;
                            if (isSeries)
                            {
                                response = npvr.CancelSeries(new NPVRParamsObj() { EntityID = domainID.ToString(), AssetID = assetID });
                            }
                            else
                            {
                                // single asset
                                response = npvr.CancelAsset(new NPVRParamsObj() { EntityID = domainID.ToString(), AssetID = assetID });
                            }

                            if (response != null)
                            {
                                switch (response.status)
                                {
                                    case CancelDeleteStatus.OK:
                                        res.status = NPVRStatus.OK.ToString();
                                        break;
                                    case CancelDeleteStatus.AlreadyCanceled:
                                        res.status = NPVRStatus.AssetAlreadyCanceled.ToString();
                                        break;
                                    case CancelDeleteStatus.AssetDoesNotExist:
                                        res.status = NPVRStatus.AssetDoesNotExist.ToString();
                                        break;
                                    case CancelDeleteStatus.Error:
                                        res.status = NPVRStatus.Error.ToString();
                                        break;
                                    case CancelDeleteStatus.AssetAlreadyRecorded:
                                        res.status = NPVRStatus.AssetAlreadyRecorded.ToString();
                                        break;
                                    default:
                                        log.Debug("CancelNPVR - " + GetNPVRLogMsg(String.Concat("Unrecognized CancelDeleteStatus enum: ", response.status.ToString()), siteGuid, assetID, isSeries, null));
                                        res.status = NPVRStatus.Unknown.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                log.Debug("CancelNPVR - " + GetNPVRLogMsg("NPVR layer returned response null. ", siteGuid, assetID, isSeries, null));
                                res.status = NPVRStatus.Error.ToString();
                            }
                        }
                        else
                        {
                            log.Debug("CancelNPVR - " + GetNPVRLogMsg("Failed to instantiate INPVRProvider object.", siteGuid, assetID, isSeries, null));
                            res.status = NPVRStatus.Error.ToString();
                        }
                    }
                    else
                    {
                        // asset id is invalid
                        log.Debug("CancelNPVR - " + GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetID), siteGuid, assetID, isSeries, null));
                        res.status = NPVRStatus.InvalidAssetID.ToString();
                    }
                }
                else
                {
                    // either user or domain is invalid
                    log.Debug("CancelNPVR - " + GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null));
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at CancelNPVR", siteGuid, assetID, isSeries, ex), ex);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;

        }

        // here assetID will be the recording ID in ALU
        public override NPVRResponse DeleteNPVR(string siteGuid, string assetID, bool isSeries)
        {
            NPVRResponse res = new NPVRResponse();
            DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0)
                {
                    if (!string.IsNullOrEmpty(assetID))
                    {
                        INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                        if (npvr != null)
                        {
                            NPVRCancelDeleteResponse response = null;
                            if (isSeries)
                            {
                                response = npvr.DeleteSeries(new NPVRParamsObj() { EntityID = domainID.ToString(), AssetID = assetID });
                            }
                            else
                            {
                                // single asset
                                response = npvr.DeleteAsset(new NPVRParamsObj() { EntityID = domainID.ToString(), AssetID = assetID });
                            }

                            if (response != null)
                            {
                                switch (response.status)
                                {
                                    case CancelDeleteStatus.OK:
                                        res.status = NPVRStatus.OK.ToString();
                                        break;
                                    case CancelDeleteStatus.AssetDoesNotExist:
                                        res.status = NPVRStatus.InvalidAssetID.ToString();
                                        break;
                                    case CancelDeleteStatus.Error:
                                        res.status = NPVRStatus.Error.ToString();
                                        break;
                                    default:
                                        log.Debug("DeleteNPVR - " + GetNPVRLogMsg(String.Concat("Unrecognized CancelDeleteStatus enum: ", response.status.ToString()), siteGuid, assetID, isSeries, null));
                                        res.status = NPVRStatus.Unknown.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                // log here response is null
                                log.Debug("DeleteNPVR - " + GetNPVRLogMsg("NPVR layer response is null. ", siteGuid, assetID, isSeries, null));
                                res.status = NPVRStatus.Error.ToString();
                            }
                        }
                        else
                        {
                            // log here npvr layer instance is null
                            log.Debug("DeleteNPVR - " + GetNPVRLogMsg("INPVRProvider instance is null. ", siteGuid, assetID, isSeries, null));
                        }
                    }
                    else
                    {
                        // asset id is invalid
                        log.Debug("DeleteNPVR - " + GetNPVRLogMsg(String.Concat("Invalid Asset ID. ALU Asset ID: ", assetID), siteGuid, assetID, isSeries, null));
                        res.status = NPVRStatus.InvalidAssetID.ToString();
                    }
                }
                else
                {
                    // either user or domain is invalid
                    log.Debug("DeleteNPVR - " + GetNPVRLogMsg(String.Concat("Invalid user. SG: ", siteGuid, " D ID: ", domainID), siteGuid, assetID, isSeries, null));
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at DeleteNPVR", siteGuid, assetID, isSeries, ex), ex);
                res.status = NPVRStatus.Error.ToString();

            }
            return res;
        }

        public override QuotaResponse GetNPVRQuota(string siteGuid)
        {
            QuotaResponse res = new QuotaResponse();
            DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0)
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
                            log.Error("Error - " + GetNPVRLogMsg(String.Concat("GetNPVRQuota. NPVR layer response is null. D ID: ", domainID), siteGuid, string.Empty, false, null));
                            res.status = NPVRStatus.Error.ToString();
                        }
                    }
                    else
                    {
                        log.Error("Error - " + GetNPVRLogMsg("GetNPVRQuota. Failed to instantiate INPVRProvider instance.", siteGuid, string.Empty, false, null));
                        res.status = NPVRStatus.Error.ToString();
                    }
                }
                else
                {
                    // log here user does not exist or no domain id.
                    log.Error("Error - " + GetNPVRLogMsg(String.Concat("GetNPVRQuota. Either user or domain is not valid. D ID: ", domainID), siteGuid, string.Empty, false, null));
                    res.status = NPVRStatus.InvalidUser.ToString();

                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at GetNPVRQuota.", siteGuid, string.Empty, false, ex), ex);
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }

        public override NPVRResponse SetNPVRProtectionStatus(string siteGuid, string assetID, bool isSeries, bool isProtect)
        {
            NPVRResponse res = new NPVRResponse();
            DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
            try
            {
                int domainID = 0;
                if (Utils.IsUserValid(siteGuid, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0)
                {
                    INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);

                    if (npvr != null)
                    {
                        if (isSeries)
                        {
                            // cannot protect series recording.
                            res.status = NPVRStatus.BadRequest.ToString();
                            res.msg = "Cannot protect Series recording.";
                        }
                        else
                        {
                            // single asset
                            NPVRProtectResponse response = npvr.SetAssetProtectionStatus(new NPVRParamsObj() { EntityID = domainID.ToString(), AssetID = assetID, IsProtect = isProtect });
                            if (response != null)
                            {
                                switch (response.status)
                                {
                                    case ProtectStatus.Protected:
                                    // fall through
                                    case ProtectStatus.NotProtected:
                                        res.status = NPVRStatus.OK.ToString();
                                        break;
                                    case ProtectStatus.RecordingDoesNotExist:
                                        res.status = NPVRStatus.InvalidAssetID.ToString();
                                        break;
                                    case ProtectStatus.Error:
                                        res.status = NPVRStatus.Error.ToString();
                                        break;
                                    default:
                                        log.Error("Error - " + GetNPVRLogMsg(String.Concat("SetNPVRProtectionStatus. Unrecognized ProtectStatus enum: ", response.status.ToString()), siteGuid, assetID, isSeries, null));
                                        res.status = NPVRStatus.Unknown.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                // log here response is null.
                                log.Error("Error - " + GetNPVRLogMsg(String.Concat("SetNPVRProtectionStatus. NPVR layer response is null. D ID: ", domainID), siteGuid, string.Empty, false, null));
                                res.status = NPVRStatus.Error.ToString();
                            }
                        }
                    }
                    else
                    {
                        // INPVRProvider instance is null
                        log.Error("Error - " + GetNPVRLogMsg("SetNPVRProtectionStatus. Failed to instantiate INPVRProvider instance.", siteGuid, assetID, isSeries, null));
                        res.status = NPVRStatus.Error.ToString();
                    }
                }
                else
                {
                    // either user does not exist or domain is not valid
                    log.Error("Error - " + GetNPVRLogMsg(String.Concat("SetNPVRProtectionStatus. Either user or domain is not valid. D ID: ", domainID), siteGuid, assetID, isSeries, null));
                    res.status = NPVRStatus.InvalidUser.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + GetNPVRLogMsg("Exception at SetNPVRProtectionStatus.", siteGuid, assetID, isSeries, ex), ex);
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
                startDate = UNIX_ZERO_TIME;
                epgChannelID = string.Empty;
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
                epdr.m_nGroupID = m_nGroupID;
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

                        if (!DateTime.TryParseExact(prog.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", new CultureInfo("he-IL"), DateTimeStyles.None, out startDate))
                        {
                            // failed to parse date.
                            startDate = UNIX_ZERO_TIME;

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

        private bool IsCalcNPVRLicensedLinkInputValid(string programID, string siteGuid, string deviceUDID)
        {
            return !string.IsNullOrEmpty(programID) && !string.IsNullOrEmpty(siteGuid) && !string.IsNullOrEmpty(deviceUDID);
        }

        protected override string CalcNPVRLicensedLink(string sProgramId, DateTime dStartTime, int format, string sSiteGUID, int nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            // don't catch exceptions in this function!
            string res = string.Empty;
            if (IsCalcNPVRLicensedLinkInputValid(sProgramId, sSiteGUID, sDEVICE_NAME))
            {
                int domainID = 0;
                DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
                if (Utils.IsUserValid(sSiteGUID, m_nGroupID, ref domainID, ref suspendStatus) && domainID > 0
                                                                && suspendStatus == DomainSuspentionStatus.OK)
                {
                    INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                    if (npvr != null)
                    {
                        string streamType = string.Empty;
                        string profile = string.Empty;
                        if (GetDeviceStreamTypeAndProfile(sDEVICE_NAME, domainID, ref streamType, ref profile))
                        {
                            NPVRLicensedLinkResponse resp = npvr.GetNPVRLicensedLink(new NPVRParamsObj() { AssetID = sProgramId, EntityID = domainID.ToString(), HASFormat = streamType, StreamType = profile }); // it is not a bug we call ALU's HASFormat is Kaltura's StreamType.
                            if (resp != null)
                            {
                                if (resp.isOK)
                                {
                                    res = resp.licensedLink;
                                }
                                else
                                {
                                    log.Error("Error - " + GetNPVRLogMsg(String.Concat("CalcNPVRLicensedLink. Response is not OK. Msg: ", resp.msg), sSiteGUID, sProgramId, false, null));
                                }
                            }
                            else
                            {
                                log.Error("Error - " + GetNPVRLogMsg("CalcNPVRLicensedLink. Response from NPVR layer is null.", sSiteGUID, sProgramId, false, null));
                            }
                        }
                        else
                        {
                            // failed to retrieve data from WS_Domains
                            throw new Exception("Failed to retrieve stream type and profile from WS_Domains. Refer to GetDeviceStreamTypeAndProfile log file.");
                        }
                    }
                    else
                    {
                        // INPVRProvider instance is null
                        log.Error("Error - " + GetNPVRLogMsg("CalcNPVRLicensedLink. Failed to instantiate INPVRProvider instance.", sSiteGUID, sProgramId, false, null));
                    }
                }
                else
                {
                    // user not valid/ user is suspended.
                    log.Error("Error - " + GetNPVRLogMsg("CalcNPVRLicensedLink. User not valid or not associated to domain or suspended.", sSiteGUID, sProgramId, false, null));
                }
            }
            else
            {
                log.Error("Error - " + GetNPVRLogMsg("CalcNPVRLicensedLink. Input not valid. Either user id, device udid or program id not supplied.", sSiteGUID, sProgramId, false, null));
            }

            return res;
        }

        private bool GetDeviceStreamTypeAndProfile(string udid, int domainID, ref string streamType, ref string profile)
        {
            DeviceResponseObject resp = null;
            bool res = false;
            string wsUsername = string.Empty, wsPassword = string.Empty;
            Utils.GetWSCredentials(m_nGroupID, eWSModules.DOMAINS, ref wsUsername, ref wsPassword);
            if (string.IsNullOrEmpty(wsUsername) || string.IsNullOrEmpty(wsPassword))
            {
                log.Error("Error - " + string.Format("Failed to retrieve WS_Domains credentials. UDID: {0} , D ID: {1}", udid, domainID));
                return false;
            }
            using (WS_Domains.module domains = new WS_Domains.module())
            {
                resp = domains.GetDeviceInfo(wsUsername, wsPassword, udid, true);
                if (resp != null && resp.m_oDeviceResponseStatus == DeviceResponseStatus.OK && resp.m_oDevice != null && resp.m_oDevice.m_state == DeviceState.Activated && domainID == resp.m_oDevice.m_domainID)
                {
                    streamType = resp.m_oDevice.m_sStreamType;
                    profile = resp.m_oDevice.m_sProfile;
                    res = true;
                }
                else
                {
                    log.Error("Error - " + string.Format("Either WS_Domains response or device object is null or device status is not OK or device does not belong to domain. UDID: {0) , D ID: {1}", udid, domainID));
                    streamType = string.Empty;
                    profile = string.Empty;
                    res = false;
                }
            }

            return res;
        }
    }
}
