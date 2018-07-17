using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;
using System.Text;
using ApiObjects;
using ApiObjects.MediaMarks;
using System.Collections.Generic;
using Core.Users;
using ApiObjects.Catalog;

namespace Core.Catalog.Request
{
    public class MediaPlayRequestData
    {
        [DataMember]
        public string m_sAssetID;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public string m_sAction;
        [DataMember]
        public int m_nLoc;
        [DataMember]
        public string m_sUDID;
        [DataMember]
        public string m_sMediaDuration;
        [DataMember]
        public int m_nMediaFileID;
        [DataMember]
        public int m_nAvgBitRate;
        [DataMember]
        public int m_nTotalBitRate;
        [DataMember]
        public int m_nCurrentBitRate;
        [DataMember]
        public eAssetTypes m_eAssetType;
        [DataMember]
        public string m_sMediaTypeId;
        [DataMember]
        public long ProgramId;
        [DataMember]
        public bool IsReportingMode;

        public MediaPlayRequestData()
        {
            this.IsReportingMode = false;
        }

        public DevicePlayData GetOrCreateDevicePlayData(int mediaId, MediaPlayActions action, int groupId, bool isLinearChannel, ePlayType playType, int domainId,  
                                                        long recordingId, int platform, int countryId, eExpirationTTL ttl = eExpirationTTL.Short)
        {
            DevicePlayData currDevicePlayData = CatalogDAL.GetDevicePlayData(this.m_sUDID);
            string playCycleKey = string.Empty;

            int userId;
            int.TryParse(this.m_sSiteGuid, out userId);

            string npvrId = recordingId != 0 ? recordingId.ToString() : string.Empty;

            // create and save new DevicePlayData if not exist
            if (currDevicePlayData == null && userId > 0)
            {
                //get domain by user
                if (domainId == 0)
                {
                    domainId = UsersDal.GetUserDomainID(m_sSiteGuid);
                }

                List<int> mediaConcurrencyRuleIds = null;
                List<long> assetMediaRulesIds = ConditionalAccess.Utils.GetAssetMediaRuleIds(groupId, mediaId);
                List<long> assetEpgRulesIds = ConditionalAccess.Utils.GetAssetEpgRuleIds(groupId, mediaId, ref this.ProgramId);
                int deviceFamilyId = ConcurrencyManager.GetDeviceFamilyIdByUdid(domainId, groupId, this.m_sUDID);
               
                currDevicePlayData = CatalogDAL.InsertDevicePlayDataToCB(userId, this.m_sUDID, domainId, mediaConcurrencyRuleIds, assetMediaRulesIds, assetEpgRulesIds, 
                                                                         mediaId, this.ProgramId, deviceFamilyId, playType, npvrId, ttl, action);

                //FPNPC -  on First Play create New Play Cycle
                if (CatalogLogic.IsGroupUseFPNPC(groupId)) 
                {
                    // We still insert to DB incase needed by other process
                    if (currDevicePlayData != null && !string.IsNullOrEmpty(currDevicePlayData.PlayCycleKey))
                    {
                        CatalogDAL.InsertPlayCycleKey(currDevicePlayData.UserId.ToString(), currDevicePlayData.AssetId, this.m_nMediaFileID,
                                                      currDevicePlayData.UDID, platform, countryId, 0, groupId, currDevicePlayData.PlayCycleKey);
                    }
                    else
                    {
                        CatalogDAL.GetOrInsertPlayCycleKey(userId.ToString(), mediaId, this.m_nMediaFileID, this.m_sUDID, platform, countryId, 0, groupId, true);
                    }
                }
            }

            // update NpvrId
            if (string.IsNullOrEmpty(currDevicePlayData.NpvrId) && recordingId != 0)
            {
                currDevicePlayData.NpvrId = npvrId;
            }

            // update program assetEpgRules for linearChannel
            if (currDevicePlayData != null && playType == ePlayType.MEDIA && isLinearChannel && currDevicePlayData.ProgramId != this.ProgramId && this.ProgramId != 0)
            {
                // if not we need to update the devicePlayData with new assetrules according to the new programId
                List<long> assetEpgRulesIds = ConditionalAccess.Utils.GetAssetEpgRuleIds(groupId, mediaId, ref this.ProgramId);

                DevicePlayData newDevicePlayData = new DevicePlayData(currDevicePlayData)
                {
                    ProgramId = this.ProgramId,
                    AssetEpgConcurrencyRuleIds = assetEpgRulesIds
                };

                // save new devicePlayData
                CatalogDAL.UpdateOrInsertDevicePlayData(newDevicePlayData, false, ttl);
                currDevicePlayData = newDevicePlayData;
            }

            return currDevicePlayData;
        }

        public void ResetDevicePlayData(DevicePlayData devicePlayData)
        {
            if (devicePlayData.TimeStamp != 0)
            {
                devicePlayData.TimeStamp = 0;
                CatalogDAL.UpdateOrInsertDevicePlayData(devicePlayData, false, eExpirationTTL.Long);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaPlayRequestData obj: ");
            sb.Append(String.Concat(" Asset ID: ", m_sAssetID));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid ?? "null"));
            sb.Append(String.Concat(" Action: ", m_sAction ?? "null"));
            sb.Append(String.Concat(" Loc: ", m_nLoc));
            sb.Append(String.Concat(" UDID: ", m_sUDID ?? "null"));
            sb.Append(String.Concat(" Media Duration: ", m_sMediaDuration ?? "null"));
            sb.Append(String.Concat(" Media File ID: ", m_nMediaFileID));
            sb.Append(String.Concat(" Avg Bitrate: ", m_nAvgBitRate));
            sb.Append(String.Concat(" Total Bitrate: ", m_nTotalBitRate));
            sb.Append(String.Concat(" Current Bitrate: ", m_nCurrentBitRate));
            sb.Append(String.Concat(" Asset Type: ", m_eAssetType));
            sb.Append(String.Concat(" Program Id: ", ProgramId));
            sb.Append(String.Concat(" Is Reporting Mode: ", IsReportingMode));

            return sb.ToString();
        }
    }
}
