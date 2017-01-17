using ApiObjects;
using ApiObjects.BulkExport;
using ApiObjects.CDNAdapter;
using ApiObjects.MediaMarks;
using ApiObjects.Roles;
using ApiObjects.Rules;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DAL
{
    public class ApiDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");
        private static readonly string CB_MESSAGE_QUEUE_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_queue_messages_design");

        public static DataTable Get_GeoBlockPerMedia(int nGroupID, int nMediaID)
        {
            ODBCWrapper.StoredProcedure spGeoBlockPerMedia = new ODBCWrapper.StoredProcedure("Get_GeoBlockPerMedia");
            spGeoBlockPerMedia.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGeoBlockPerMedia.AddParameter("@GroupID", nGroupID);
            spGeoBlockPerMedia.AddParameter("@MediaID", nMediaID);

            DataSet ds = spGeoBlockPerMedia.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_Operators_Info(int nGroupID, List<int> operatorIds)
        {
            ODBCWrapper.StoredProcedure spGet_Operators_Info = new ODBCWrapper.StoredProcedure("Get_Operators_Info");
            spGet_Operators_Info.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_Operators_Info.AddIDListParameter<int>("@IDs", operatorIds, "Id");
            spGet_Operators_Info.AddParameter("@ParentGroupID", nGroupID.ToString());

            DataSet ds = spGet_Operators_Info.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;


        }

        public static DataTable Get_GroupMediaRules(int nMediaID, string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spGroupMediaRules = new ODBCWrapper.StoredProcedure("Get_GroupMediaRules");
            spGroupMediaRules.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGroupMediaRules.AddParameter("@SiteGuid", sSiteGuid);
            spGroupMediaRules.AddParameter("@MediaID", nMediaID);

            DataSet ds = spGroupMediaRules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UserSocialActionID(int nMediaID, string sSiteGuid, int nGroupID, int nSocialPlatform, int nSocialAction)
        {
            ODBCWrapper.StoredProcedure spUserSocialActionID = new ODBCWrapper.StoredProcedure("Get_UserSocialActionID");
            spUserSocialActionID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUserSocialActionID.AddParameter("@SiteGuid", sSiteGuid);
            spUserSocialActionID.AddParameter("@MediaID", nMediaID);
            spUserSocialActionID.AddParameter("@GroupID", nGroupID);
            spUserSocialActionID.AddParameter("@SocialPlatform", nSocialPlatform);
            spUserSocialActionID.AddParameter("@SocialAction", nSocialAction);


            DataSet ds = spUserSocialActionID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_MediaDetailsForEmail(int nMediaID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spMediaDetailsForEmail = new ODBCWrapper.StoredProcedure("Get_MediaDetailsForEmail");
            spMediaDetailsForEmail.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMediaDetailsForEmail.AddParameter("@MediaID", nMediaID);
            spMediaDetailsForEmail.AddParameter("@GroupID", nGroupID);

            DataSet ds = spMediaDetailsForEmail.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;
        }

        public static DataTable Get_GroupRules(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGroupRules = new ODBCWrapper.StoredProcedure("Get_GroupRules");
            spGroupRules.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGroupRules.AddParameter("@GroupID", nGroupID);

            DataSet ds = spGroupRules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_LuceneUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spLuceneUr = new ODBCWrapper.StoredProcedure("Get_LuceneUrl");
            spLuceneUr.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLuceneUr.AddParameter("@GroupID", nGroupID);

            DataSet ds = spLuceneUr.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_StartValues(int nGroupID, string sSiteGuid, int caseOption)
        {
            ODBCWrapper.StoredProcedure spStartValues = new ODBCWrapper.StoredProcedure("Get_StartValues");
            spStartValues.SetConnectionKey("MAIN_CONNECTION_STRING");
            spStartValues.AddParameter("@GroupID", nGroupID);
            spStartValues.AddParameter("@SiteGuid", sSiteGuid);
            spStartValues.AddParameter("@CaseOption", caseOption);

            DataSet ds = spStartValues.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_LicenseKeyMaxMind(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spLicenseKeyMaxMind = new ODBCWrapper.StoredProcedure("Get_LicenseKeyMaxMind");
            spLicenseKeyMaxMind.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLicenseKeyMaxMind.AddParameter("@GroupID", nGroupID);

            DataSet ds = spLicenseKeyMaxMind.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AvailableDevices(int nMediaID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spAvailableDevices = new ODBCWrapper.StoredProcedure("Get_AvailableDevices");
            spAvailableDevices.SetConnectionKey("MAIN_CONNECTION_STRING");
            spAvailableDevices.AddParameter("@GroupID", nGroupID);
            spAvailableDevices.AddParameter("@MediaID", nMediaID);

            DataSet ds = spAvailableDevices.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_DefaultRules(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spSetDefaultRules = new ODBCWrapper.StoredProcedure("Get_DefaultRules");
            spSetDefaultRules.SetConnectionKey("MAIN_CONNECTION_STRING");
            spSetDefaultRules.AddParameter("@GroupID", nGroupID);

            DataSet ds = spSetDefaultRules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UserGroupRule(int nGroupID, string sSiteGuid, int RuleID)
        {
            ODBCWrapper.StoredProcedure spUserGroupRule = new ODBCWrapper.StoredProcedure("Get_UserGroupRule");
            spUserGroupRule.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUserGroupRule.AddParameter("@GroupID", nGroupID);
            spUserGroupRule.AddParameter("@SiteGuid", sSiteGuid);
            spUserGroupRule.AddParameter("@RuleID", RuleID);

            DataSet ds = spUserGroupRule.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool UpdateDomainGroupRule(int nDomainID, int nRuleID, int nIsActive, int nStatus, bool bWithPIN, string sPIN)
        {
            bool ret = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_group_rules");
                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                if (bWithPIN)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
                }
                updateQuery += "WHERE";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                updateQuery += "AND";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nRuleID);
                updateQuery += "AND";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

                ret = updateQuery.Execute();
                updateQuery.Finish();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static string[] GetDomainGroupRule(int nGroupID, int nDomainID, int nRuleID)
        {
            try
            {
                ODBCWrapper.StoredProcedure spUserGroupRule = new ODBCWrapper.StoredProcedure("Get_DomainGroupRule");
                spUserGroupRule.SetConnectionKey("MAIN_CONNECTION_STRING");
                spUserGroupRule.AddParameter("@GroupID", nGroupID);
                spUserGroupRule.AddParameter("@DomainID", nDomainID);
                spUserGroupRule.AddParameter("@RuleID", nRuleID);

                using (DataSet ds = spUserGroupRule.ExecuteDataSet())
                {
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].DefaultView.Count > 0)
                    {
                        using (DataTable dt = ds.Tables[0])
                        {
                            if (dt.DefaultView.Count > 0 && dt.Rows[0] != null)
                            {
                                DataRow dr = dt.Rows[0];

                                string sDomainRuleID = (dr["rule_id"] != DBNull.Value) ? (dr["rule_id"].ToString()) : string.Empty;
                                string sIsActive = (dr["is_active"] != DBNull.Value) ? (dr["is_active"].ToString()) : string.Empty;
                                string sPIN = dr["code"].ToString();

                                string[] dbDomainRule = new string[] { sDomainRuleID, sIsActive, sPIN };
                                return dbDomainRule;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static string GetDomainCodeForParentalPIN(int nDomainID, int nRuleID)
        {
            string sPIN = null;

            try
            {
                ODBCWrapper.StoredProcedure spCodeForParentalPIN = new ODBCWrapper.StoredProcedure("Get_DomainCodeForParentalPIN");
                spCodeForParentalPIN.SetConnectionKey("MAIN_CONNECTION_STRING");

                spCodeForParentalPIN.AddParameter("@DomainID", nDomainID);
                spCodeForParentalPIN.AddParameter("@RuleID", nRuleID);

                DataSet ds = spCodeForParentalPIN.ExecuteDataSet();

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt != null && dt.DefaultView.Count > 0)
                    {
                        sPIN = dt.Rows[0]["code"].ToString();
                        return sPIN;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sPIN;
        }

        public static DataTable Get_CodeForParentalPIN(string sSiteGuid, int RuleID)
        {
            ODBCWrapper.StoredProcedure spCodeForParentalPIN = new ODBCWrapper.StoredProcedure("Get_CodeForParentalPIN");
            spCodeForParentalPIN.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCodeForParentalPIN.AddParameter("@SiteGuid", sSiteGuid);
            spCodeForParentalPIN.AddParameter("@RuleID", RuleID);

            DataSet ds = spCodeForParentalPIN.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_GroupOperatorsDetails(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGroupOperatorsDetails = new ODBCWrapper.StoredProcedure("Get_GroupOperatorsDetails");
            spGroupOperatorsDetails.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGroupOperatorsDetails.AddParameter("@GroupID", nGroupID);

            DataSet ds = spGroupOperatorsDetails.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UserGroupRules(string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spUserGroupRules = new ODBCWrapper.StoredProcedure("Get_UserGroupRules");
            spUserGroupRules.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUserGroupRules.AddParameter("@SiteGuid", sSiteGuid);

            DataSet ds = spUserGroupRules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetDomainGroupRules(int nDomainID)
        {
            try
            {
                ODBCWrapper.StoredProcedure spDomainGroupRules = new ODBCWrapper.StoredProcedure("Get_DomainGroupRules");
                spDomainGroupRules.SetConnectionKey("MAIN_CONNECTION_STRING");

                spDomainGroupRules.AddParameter("@DomainID", nDomainID);

                DataSet ds = spDomainGroupRules.ExecuteDataSet();

                if (ds != null)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static DataTable Get_DetailsUsersDynamicData(int nUserID)
        {
            ODBCWrapper.StoredProcedure spDetailsUsersDynamicData = new ODBCWrapper.StoredProcedure("Get_DetailsUsersDynamicData");
            spDetailsUsersDynamicData.SetConnectionKey("USERS_MAIN_CONNECTION_STRING");
            spDetailsUsersDynamicData.AddParameter("@UserID", nUserID);

            DataSet ds = spDetailsUsersDynamicData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_EPGChannel(int groupID)
        {
            ODBCWrapper.StoredProcedure spEPGChannel = new ODBCWrapper.StoredProcedure("Get_EPGChannel");
            spEPGChannel.SetConnectionKey("MAIN_CONNECTION_STRING");
            spEPGChannel.AddParameter("@GroupID", groupID);

            DataSet ds = spEPGChannel.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AdminUserAccount(string user, string pass)
        {
            ODBCWrapper.StoredProcedure spAdminUserAccount = new ODBCWrapper.StoredProcedure("Get_AdminUserAccount");
            spAdminUserAccount.SetConnectionKey("MAIN_CONNECTION_STRING");
            spAdminUserAccount.AddParameter("@User", user);
            spAdminUserAccount.AddParameter("@Pass", pass);

            DataSet ds = spAdminUserAccount.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_MediaFileTypeID(int nMediaFileID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spMediaFileTypeID = new ODBCWrapper.StoredProcedure("Get_MediaFileTypeID");
            spMediaFileTypeID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMediaFileTypeID.AddParameter("@MediaFileID", nMediaFileID);
            spMediaFileTypeID.AddParameter("@GroupID", nGroupID);
            DataSet ds = spMediaFileTypeID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AdminTokenValues(string sIp, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spAdminTokenValues = new ODBCWrapper.StoredProcedure("Get_AdminTokenValues");
            spAdminTokenValues.SetConnectionKey("MAIN_CONNECTION_STRING");
            spAdminTokenValues.AddParameter("@IP", sIp);
            spAdminTokenValues.AddParameter("@GroupID", nGroupID);
            DataSet ds = spAdminTokenValues.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AvailableFileTypes(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spAvailableFileTypes = new ODBCWrapper.StoredProcedure("Get_AvailableFileTypes");
            spAvailableFileTypes.SetConnectionKey("MAIN_CONNECTION_STRING");
            spAvailableFileTypes.AddParameter("@GroupID", nGroupID);
            DataSet ds = spAvailableFileTypes.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        /*call Global stored procedure that select fileds from table by id .
         **need to get table name and filed/s name with , between them (if more then one filed required)
         **if @WhereFieldName is not ID - send the filed name   */
        public static DataTable Get_DataByTableID(string sWhereFieldValue, string sTableName, string sFieldsNames)
        {
            return Get_DataByTableID(sWhereFieldValue, sTableName, sFieldsNames, string.Empty);
        }

        public static DataTable Get_DataByTableID(string sWhereFieldValue, string sTableName, string sFieldsNames, string sWhereFieldName)
        {
            ODBCWrapper.StoredProcedure spDataByTableID = new ODBCWrapper.StoredProcedure("Get_DataByTableID");
            spDataByTableID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spDataByTableID.AddParameter("@WhereFieldValue", sWhereFieldValue);
            if (!string.IsNullOrEmpty(sWhereFieldName))
                spDataByTableID.AddParameter("@WhereFieldName", sWhereFieldName);
            spDataByTableID.AddParameter("@TableName", sTableName);
            spDataByTableID.AddParameter("@FieldsNames", sFieldsNames);

            DataSet ds = spDataByTableID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_MapMediaFiles(List<int> nMediaFileIDs)
        {
            ODBCWrapper.StoredProcedure spMapMediaFiles = new ODBCWrapper.StoredProcedure("Get_MapMediaFiles");
            spMapMediaFiles.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMapMediaFiles.AddIDListParameter("@MediaFileIdList", nMediaFileIDs, "Id");

            DataSet ds = spMapMediaFiles.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_SubGroupsTree()
        {
            ODBCWrapper.StoredProcedure spMapMediaFiles = new ODBCWrapper.StoredProcedure("Get_SubGroupsTree");
            spMapMediaFiles.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet ds = spMapMediaFiles.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }


        public static DataTable Get_GroupPlayers()
        {
            ODBCWrapper.StoredProcedure spMapMediaFiles = new ODBCWrapper.StoredProcedure("Get_GroupPlayers");
            spMapMediaFiles.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet ds = spMapMediaFiles.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_GroupMediaNames(string sGroupName)
        {
            ODBCWrapper.StoredProcedure spGroupMediaNames = new ODBCWrapper.StoredProcedure("Get_GroupMediaNames");
            spGroupMediaNames.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGroupMediaNames.AddParameter("@GroupName", sGroupName);

            DataSet ds = spGroupMediaNames.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;
        }


        public static MediaMarkObject Get_MediaMark(int nMediaID, string sSiteGUID, int nGroupID)
        {
            bool bGetDBData = TCMClient.Settings.Instance.GetValue<bool>("getDBData");

            int nUserID = 0;
            int.TryParse(sSiteGUID, out nUserID);

            MediaMarkObject ret = new MediaMarkObject();
            ret.nGroupID = nGroupID;
            ret.nMediaID = nMediaID;
            ret.sSiteGUID = sSiteGUID;

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);//CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.MEDIAMARK);
            string docKey = UtilsDal.getUserMediaMarkDocKey(nUserID, nMediaID);

            var data = cbManager.Get<string>(docKey);
            bool bContunueWithCB = (!string.IsNullOrEmpty(data)) ? true : false;

            if (bContunueWithCB)
            {
                MediaMarkLog mediaMarkLogObject = JsonConvert.DeserializeObject<MediaMarkLog>(data);
                ret.nLocationSec = mediaMarkLogObject.LastMark.Location;
                ret.sDeviceID = mediaMarkLogObject.LastMark.UDID;

                if (string.IsNullOrEmpty(mediaMarkLogObject.LastMark.UDID))
                {
                    ret.sDeviceName = "PC";
                }
                else
                {
                    DataTable dtDeviceInfo = DeviceDal.Get_DeviceInfo(mediaMarkLogObject.LastMark.UDID, true, nGroupID);
                    if (dtDeviceInfo != null && dtDeviceInfo.Rows.Count > 0)
                    {
                        ret.sDeviceName = ODBCWrapper.Utils.GetSafeStr(dtDeviceInfo.Rows[0]["name"]);
                    }
                    else
                    {
                        ret.sDeviceName = "N/A";
                        ret.eStatus = MediaMarkObject.MediaMarkObjectStatus.NA;
                    }
                }
            }
            else if (bGetDBData)
            {
                Get_MediaMark_DB(nMediaID, sSiteGUID, nGroupID, ret);
            }

            return ret;

        }

        private static void Get_MediaMark_DB(int nMediaID, string sSiteGUID, int nGroupID, MediaMarkObject ret)
        {
            try
            {
                ODBCWrapper.StoredProcedure spMediaMark = new ODBCWrapper.StoredProcedure("Get_MediaMark");
                spMediaMark.SetConnectionKey("MAIN_CONNECTION_STRING");
                spMediaMark.AddParameter("@MediaID", nMediaID);
                spMediaMark.AddParameter("@SiteGUID", sSiteGUID);
                spMediaMark.AddParameter("@GroupID", nGroupID);
                DataSet ds = spMediaMark.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        ret.nLocationSec = ODBCWrapper.Utils.GetIntSafeVal(dr, "location_sec");
                        ret.sDeviceID = ODBCWrapper.Utils.GetSafeStr(dr, "device_udid");

                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            ret.sDeviceName = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                        }
                        else
                        {
                            ret.sDeviceName = "N/A";
                            ret.eStatus = MediaMarkObject.MediaMarkObjectStatus.NA;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("", ex);
                ret = new MediaMarkObject();
            }
        }

        public static DataTable Get_GeoCommerceValue(int nGroupID, int SubscriptionGeoCommerceID)
        {
            ODBCWrapper.StoredProcedure spGeoCommerceValue = new ODBCWrapper.StoredProcedure("Get_GeoCommerceValue");
            spGeoCommerceValue.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGeoCommerceValue.AddParameter("@SubscriptionGeoCommerceID", SubscriptionGeoCommerceID);
            spGeoCommerceValue.AddParameter("@GroupID", nGroupID);

            DataSet ds = spGeoCommerceValue.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int Insert_NewUserGroupRule(int nGroupID, int nRuleID, string sSiteGuid, string sPIN, int nStatus)
        {
            ODBCWrapper.StoredProcedure spNewUserGroupRule = new ODBCWrapper.StoredProcedure("Insert_NewUserGroupRule");
            spNewUserGroupRule.SetConnectionKey("MAIN_CONNECTION_STRING");
            spNewUserGroupRule.AddParameter("@GroupID", nGroupID);
            spNewUserGroupRule.AddParameter("@RuleID", nRuleID);
            spNewUserGroupRule.AddParameter("@SiteGuid", sSiteGuid);
            spNewUserGroupRule.AddParameter("@PIN", sPIN);
            spNewUserGroupRule.AddParameter("@Status", nStatus);

            int retVal = spNewUserGroupRule.ExecuteReturnValue<int>();//.ExecuteDataSet();

            return retVal;
        }



        public static int Update_UserGroupRule(int nIsActive, string sPIN, string sSiteGuid, int nUserRuleID)
        {
            ODBCWrapper.StoredProcedure spUserGroupRul = new ODBCWrapper.StoredProcedure("Update_UserGroupRule");
            spUserGroupRul.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUserGroupRul.AddParameter("@RuleID", nUserRuleID);
            spUserGroupRul.AddParameter("@SiteGuid", sSiteGuid);
            spUserGroupRul.AddParameter("@PIN", sPIN);
            spUserGroupRul.AddParameter("@IsActive", nIsActive);

            int retVal = spUserGroupRul.ExecuteReturnValue<int>();//.ExecuteDataSet();

            return retVal;
        }

        public static void Update_UserGroupRule_Token(int nSiteGuid, int nUserRuleID, string sChangePinToken, DateTime dChangePinTokenLastDate)
        {
            ODBCWrapper.StoredProcedure spUpdateUserGroupRuleToken = new ODBCWrapper.StoredProcedure("Update_UserGroupRule_Token");
            spUpdateUserGroupRuleToken.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateUserGroupRuleToken.AddParameter("@SiteGuid", nSiteGuid);
            spUpdateUserGroupRuleToken.AddParameter("@RuleID", nUserRuleID);
            spUpdateUserGroupRuleToken.AddNullableParameter("@ChangePinToken", sChangePinToken); //can be update with null value
            spUpdateUserGroupRuleToken.AddParameter("@ChangePinTokenLastDate", dChangePinTokenLastDate);
            spUpdateUserGroupRuleToken.ExecuteNonQuery();
        }

        public static void Update_UserGroupRule_Code(int nSiteGuid, int nUserRuleID, string sChangePinToken, string sCode)
        {
            ODBCWrapper.StoredProcedure spUpdateUserGroupRuleCode = new ODBCWrapper.StoredProcedure("Update_UserGroupRule_Code");
            spUpdateUserGroupRuleCode.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateUserGroupRuleCode.AddParameter("@SiteGuid", nSiteGuid);
            spUpdateUserGroupRuleCode.AddParameter("@RuleID", nUserRuleID);
            spUpdateUserGroupRuleCode.AddParameter("@ChangePinToken", sChangePinToken);
            spUpdateUserGroupRuleCode.AddParameter("@Code", sCode);
            spUpdateUserGroupRuleCode.ExecuteNonQuery();
        }

        public static DataTable Check_UserGroupRule_Token(string sToken)
        {
            ODBCWrapper.StoredProcedure spCheckUserGroupRuleToken = new ODBCWrapper.StoredProcedure("Check_UserGroupRule_Token");
            spCheckUserGroupRuleToken.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCheckUserGroupRuleToken.AddParameter("@Token", sToken);

            DataSet ds = spCheckUserGroupRuleToken.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int UpdateOrInsert_UsersSocialActionsLike(int nID, int nGroupID, string sSiteGUID, int nMediaID, int nSocialAction, int nSocialPlatform)
        {
            ODBCWrapper.StoredProcedure spUsersSocialActions = new ODBCWrapper.StoredProcedure("UpdateOrInsert_UsersSocialActionsLike");
            spUsersSocialActions.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUsersSocialActions.AddParameter("@SocialActionID", nID);
            spUsersSocialActions.AddParameter("@GroupID", nGroupID);
            spUsersSocialActions.AddParameter("@SiteGUID", sSiteGUID);
            spUsersSocialActions.AddParameter("@MediaID", nMediaID);
            spUsersSocialActions.AddParameter("@SocialAction", nSocialAction);
            spUsersSocialActions.AddParameter("@SocialPlatform", nSocialPlatform);

            int retVal = spUsersSocialActions.ExecuteReturnValue<int>();//.ExecuteDataSet();

            return retVal;
        }

        public static int Update_MediaLikeCounter(int nMediaID, int nVal)
        {
            ODBCWrapper.StoredProcedure spMediaLikeCounter = new ODBCWrapper.StoredProcedure("Update_MediaLikeCounter");
            spMediaLikeCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMediaLikeCounter.AddParameter("@MediaID", nMediaID);
            spMediaLikeCounter.AddParameter("@AddVal", nVal);
            int retVal = spMediaLikeCounter.ExecuteReturnValue<int>();//.ExecuteDataSet();

            return retVal;
        }

        public static void Update_MediaViews(int nMediaID, int nMediaFileID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaViews = new ODBCWrapper.StoredProcedure("UpdateMediaViews");
            spUpdateMediaViews.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaViews.AddParameter("@MediaID", nMediaID);
            spUpdateMediaViews.AddParameter("@MediaFileID", nMediaFileID);
            spUpdateMediaViews.ExecuteNonQuery();
        }

        public static int UpdateOrInsert_UsersSocialActionsUnLike(int nID, int nGroupID, string sSiteGUID, int nMediaID, int nSocialAction, int nSocialPlatform, int nUpdateOrInsert)
        {
            ODBCWrapper.StoredProcedure spUsersSocialActionsUnLike = new ODBCWrapper.StoredProcedure("UpdateOrInsert_UsersSocialActionsUnLike");
            spUsersSocialActionsUnLike.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUsersSocialActionsUnLike.AddParameter("@SocialActionID", nID);
            spUsersSocialActionsUnLike.AddParameter("@GroupID", nGroupID);
            spUsersSocialActionsUnLike.AddParameter("@SiteGUID", sSiteGUID);
            spUsersSocialActionsUnLike.AddParameter("@MediaID", nMediaID);
            spUsersSocialActionsUnLike.AddParameter("@SocialAction", nSocialAction);
            spUsersSocialActionsUnLike.AddParameter("@SocialPlatform", nSocialPlatform);
            spUsersSocialActionsUnLike.AddParameter("@SQLOrInsert", nUpdateOrInsert);


            int retVal = spUsersSocialActionsUnLike.ExecuteReturnValue<int>();//.ExecuteDataSet();

            return retVal;
        }

        public static DataTable Get_AllSubAccounts(int nParendGroupID)
        {
            ODBCWrapper.StoredProcedure spSubAccounts = new ODBCWrapper.StoredProcedure("Get_AllSubAccounts");
            spSubAccounts.SetConnectionKey("MAIN_CONNECTION_STRING");
            spSubAccounts.AddParameter("@ParentGroupID", nParendGroupID);

            DataSet ds = spSubAccounts.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;

        }

        public static DataTable GetGroupRulesTagsValues(List<int> nGroupRuleIDs)
        {
            ODBCWrapper.StoredProcedure spSubAccounts = new ODBCWrapper.StoredProcedure("GetGroupRulesTagValues");
            spSubAccounts.SetConnectionKey("MAIN_CONNECTION_STRING");
            spSubAccounts.AddIDListParameter<int>("@GroupRuleIdList", nGroupRuleIDs, "Id");
            DataSet ds = spSubAccounts.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;

        }



        public static DataTable Get_DeviceMediaRules(int nMediaID, int nGroupID, string deviceUdid)
        {
            ODBCWrapper.StoredProcedure spGroupRules = new ODBCWrapper.StoredProcedure("Get_DeviceMediaRules");
            spGroupRules.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGroupRules.AddParameter("@MediaID", nMediaID);
            spGroupRules.AddParameter("@GroupID", nGroupID);
            spGroupRules.AddParameter("@DeviceID", deviceUdid);

            DataSet ds = spGroupRules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static object Get_LastTransactionFourDigits(string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spLastTransactionFourDigits = new ODBCWrapper.StoredProcedure("Get_LastTransactionFourDigits");
            spLastTransactionFourDigits.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLastTransactionFourDigits.AddParameter("@SiteGuid", sSiteGuid);

            object retLastFourDigits = spLastTransactionFourDigits.ExecuteReturnValue();

            return retLastFourDigits;
        }

        public static DataTable Get_LastBillingTransactionToUser(int nGroupID, string sSiteGUID, int? nBillingProvider)
        {
            ODBCWrapper.StoredProcedure spGetLastBillingTransactionToUser = new ODBCWrapper.StoredProcedure("Get_LastBillingTransactionToUser");
            spGetLastBillingTransactionToUser.SetConnectionKey("MAIN_CONNECTION_STRING");

            spGetLastBillingTransactionToUser.AddParameter("@GroupID", nGroupID);
            spGetLastBillingTransactionToUser.AddParameter("@SiteGuid", sSiteGUID);
            spGetLastBillingTransactionToUser.AddNullableParameter<int?>("@BillingProvider", nBillingProvider);

            DataSet ds = spGetLastBillingTransactionToUser.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }


        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }


        public static bool InsertNewDomainGroupRule(string sInGroups, int nDomainID, int nRuleID, string sPIN, int nIsActive, int nStatus)
        {
            bool ret = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT ID FROM groups_rules WITH (nolock) WHERE status=1 AND is_active=1 AND";
                selectQuery += "group_id " + sInGroups;
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRuleID);

                if ((selectQuery.Execute("query", true) != null) &&
                    (selectQuery.Table("query").DefaultView.Count > 0))
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_group_rules");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nRuleID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                    ret = insertQuery.Execute();
                    insertQuery.Finish();
                }
                selectQuery.Finish();

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        //public static DataTable GetCurrentDisplayedProgram(int nMediaID, DateTime now)
        //{
        //    DataTable returnedDisplayedProgram = null;
        //    ODBCWrapper.StoredProcedure spCurrentDisplayedProgram = new ODBCWrapper.StoredProcedure("TVinci..Get_CurrentDisplayedProgram");
        //    spCurrentDisplayedProgram.SetConnectionKey("MAIN_CONNECTION_STRING");
        //    spCurrentDisplayedProgram.AddParameter("@MediaID", nMediaID);
        //    spCurrentDisplayedProgram.AddParameter("@CurrentTime", now.ToString("yyyy-MM-dd HH:mm:ss"));

        //    DataSet returnedProgram = spCurrentDisplayedProgram.ExecuteDataSet();

        //    if (returnedProgram != null)
        //        returnedDisplayedProgram = returnedProgram.Tables[0];

        //    return returnedDisplayedProgram;
        //}

        public static DataTable Get_EPGProgramRules(int nProgramId, string sSiteGuid)
        {
            DataTable returnedDataTable = null;

            try
            {
                ODBCWrapper.StoredProcedure spEpgProgramRules = new ODBCWrapper.StoredProcedure("Get_EPGProgramRules");
                spEpgProgramRules.SetConnectionKey("MAIN_CONNECTION_STRING");
                spEpgProgramRules.AddParameter("@ProgramId", nProgramId);
                spEpgProgramRules.AddParameter("@SiteGuid", sSiteGuid);

                DataSet returnedRulesSet = spEpgProgramRules.ExecuteDataSet();

                if (returnedRulesSet != null)
                    returnedDataTable = returnedRulesSet.Tables[0];
            }
            catch
            {
                returnedDataTable = null;
            }


            return returnedDataTable;
        }


        public static DataTable Get_EPGRules(string sSiteGuid, int nGroupID)
        {
            DataTable returnedDataTable = null;
            try
            {
                ODBCWrapper.StoredProcedure spEpgProgramRules = new ODBCWrapper.StoredProcedure("Get_EPGRules");
                spEpgProgramRules.SetConnectionKey("MAIN_CONNECTION_STRING");
                spEpgProgramRules.AddParameter("@SiteGuid", sSiteGuid);
                spEpgProgramRules.AddParameter("@GroupID", nGroupID);

                DataSet returnedRulesSet = spEpgProgramRules.ExecuteDataSet();

                if (returnedRulesSet != null)
                    returnedDataTable = returnedRulesSet.Tables[0];
            }
            catch
            {
                returnedDataTable = null;
            }
            return returnedDataTable;
        }


        public static DataTable GetProgramSchedule(int nProgramId)
        {
            DataTable returnedDataTable = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ProgramStartAndEndDates");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ProgramID", nProgramId);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                returnedDataTable = ds.Tables[0];

            return returnedDataTable;
        }

        public static DataTable GetCoGuidByMediaFileId(int nMediaFileID)
        {
            DataTable returnedDataTable = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CoGuidByMediaFileId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaFileId", nMediaFileID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                returnedDataTable = ds.Tables[0];

            return returnedDataTable;
        }

        public static List<int> GetUserStartedWatchingMedias(string sSiteGuid, int nNumOfItems)
        {
            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);

            var mediaMarkManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            // this view gets us all media that user has watched
            var viewResult = mediaMarkManager.View<MediaMarkLog>(new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
            {
                startKey = new object[] { nSiteGuid, 0 },
                endKey = new object[] { nSiteGuid, string.Empty },
                asJson = true,
                shouldLookupById = true
            });

            List<int> resultList = new List<int>();

            if (viewResult != null)
            {
                //// Now we will get the media hits, because this is where the LOCATION is saved
                //List<string> keys = new List<string>();

                //foreach (var currentResult in viewResult)
                //{
                //    if (currentResult.LastMark != null)
                //    {
                //        string key = string.Format("u{0}_m{1}", currentResult.LastMark.UserID, currentResult.LastMark.MediaID);

                //        keys.Add(key);
                //    }
                //}

                //var mediaHitsDictionary = mediaHitsManager.GetValues<MediaMarkLog>(keys, true, false);

                List<MediaMarkLog> sortedMediaMarksList =
                    //mediaHitsDictionary.Values.OrderBy(x => x.LastMark.CreatedAt).ToList();
                    viewResult.OrderByDescending(x => x.LastMark.CreatedAt).ToList();

                if (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0)
                {
                    List<int> mediaIdsList = sortedMediaMarksList.Select(x => x.LastMark.AssetID).ToList();
                    DataTable dtMediasMaxDurations = ApiDAL.Get_MediasMaxDuration(mediaIdsList);

                    if (dtMediasMaxDurations != null && dtMediasMaxDurations.Rows.Count > 0)
                    {
                        Dictionary<int, int> dictMediasMaxDuration = new Dictionary<int, int>();
                        foreach (DataRow rowDuration in dtMediasMaxDurations.Rows)
                        {
                            int nMediaID = ODBCWrapper.Utils.GetIntSafeVal(rowDuration["media_id"]);
                            int nMaxDuration = ODBCWrapper.Utils.GetIntSafeVal(rowDuration["max_duration"]);
                            dictMediasMaxDuration.Add(nMediaID, nMaxDuration);
                        }

                        int i = 0;

                        foreach (MediaMarkLog mediaMarkLogObject in sortedMediaMarksList)
                        {
                            if (dictMediasMaxDuration.ContainsKey(mediaMarkLogObject.LastMark.AssetID))
                            {
                                double dMaxDuration = Math.Round((0.95 * dictMediasMaxDuration[mediaMarkLogObject.LastMark.AssetID]));

                                // If it started (not 0) and it is before 95%
                                if (mediaMarkLogObject.LastMark.Location > 1 && mediaMarkLogObject.LastMark.Location <= dMaxDuration)
                                {
                                    if (i >= nNumOfItems || nNumOfItems == 0)
                                    {
                                        break;
                                    }

                                    resultList.Add(mediaMarkLogObject.LastMark.AssetID);
                                    i++;
                                }
                            }
                        }

                    }
                }
            }

            return resultList;
        }

        public static bool CleanUserHistory(string siteGuid, List<int> lMediaIDs)
        {
            try
            {
                bool retVal = true;
                int nSiteGuid = 0;
                int.TryParse(siteGuid, out nSiteGuid);

                var mediaMarkManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
                var mediaHitManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);

                if (lMediaIDs.Count == 0)
                {
                    var view = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
                    {
                        startKey = new object[] { nSiteGuid, 0 },
                        endKey = new object[] { nSiteGuid, string.Empty },
                        shouldLookupById = true
                    };

                    var res = mediaMarkManager.View<string>(view);

                    // deserialize string to MediaMarkLog
                    List<MediaMarkLog> sortedMediaMarksList = res.Select(current => JsonConvert.DeserializeObject<MediaMarkLog>(current)).ToList();

                    if (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0)
                    {
                        lMediaIDs = sortedMediaMarksList.Select(x => x.LastMark.AssetID).ToList();
                    }
                }

                Random r = new Random();

                foreach (int nMediaID in lMediaIDs)
                {
                    string documentKey = UtilsDal.getUserMediaMarkDocKey(nSiteGuid, nMediaID);

                    // Irena - make sure doc type is right
                    bool markResult = mediaMarkManager.Remove(documentKey);
                    bool hitResult = mediaHitManager.Remove(documentKey);
                    Thread.Sleep(r.Next(50));
                
                    if (!markResult || !hitResult)
                    {
                        retVal = false;
                        return retVal;
                    }
                }

                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }


        public static bool Is_MediaExistsToUserType(int nMediaID, int nUserTypeID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsMediaExistsToUserType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaID", nMediaID);
            sp.AddParameter("@UserTypeID", nUserTypeID);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static string Get_PPVNameForPurchaseMail(string sPSPReference)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure spPPVNameForPurchaseMail = new ODBCWrapper.StoredProcedure("Get_PPVNameForPurchaseMail");
            spPPVNameForPurchaseMail.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPPVNameForPurchaseMail.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = spPPVNameForPurchaseMail.ExecuteDataSet();
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    if (dt.Rows[0]["name"] != DBNull.Value)
                        res = dt.Rows[0]["name"].ToString();
            }

            return res;
        }

        public static string Get_M1_PPVNameForPurchaseMail(int nM1TransactionID)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure spPPVNameForPurchaseMail = new ODBCWrapper.StoredProcedure("Get_M1_PPVNameForPurchaseMail");
            spPPVNameForPurchaseMail.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPPVNameForPurchaseMail.AddParameter("@M1TransactionID", nM1TransactionID);
            DataSet ds = spPPVNameForPurchaseMail.ExecuteDataSet();
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    if (dt.Rows[0]["name"] != DBNull.Value)
                        res = dt.Rows[0]["name"].ToString();
            }

            return res;
        }



        public static void Update_Last4Digits(long lID, string sLast4Digits)
        {
            ODBCWrapper.StoredProcedure spUpdateLast4Digits = new ODBCWrapper.StoredProcedure("Update_Last4Digits");
            spUpdateLast4Digits.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateLast4Digits.AddParameter("@ID", lID);
            spUpdateLast4Digits.AddParameter("@Last4Digits", sLast4Digits);
            spUpdateLast4Digits.AddParameter("@UpdateDate", DateTime.UtcNow);
            spUpdateLast4Digits.ExecuteNonQuery();
        }

        public static string Get_Last4DigitsByBillingTransctionID(long lIDInBillingTransactions)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_Last4DigitsByBillingTransctionID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@IDInBT", lIDInBillingTransactions);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["last_four_digits"] != DBNull.Value && dt.Rows[0]["last_four_digits"] != null)
                        res = dt.Rows[0]["last_four_digits"].ToString();
                }

            }

            return res;
        }

        public static bool Get_IsPurchasedWithPreviewModule(int nGroupID, string sSiteGuid, int nPurchaseID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IsPurchasedWithPreviewModule");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@PurchaseID", nPurchaseID);
            int res = sp.ExecuteReturnValue<int>();
            return res > 0;

        }

        public static bool Get_IsPurchasedWithPreviewModuleByBillingGuid(int nGroupID, string billingGuid, int nPurchaseID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IsPurchasedWithPreviewModuleByBillingGuid");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@BillingGuid", billingGuid);
            sp.AddParameter("@PurchaseID", nPurchaseID);
            int res = sp.ExecuteReturnValue<int>();
            return res > 0;
        }

        public static bool Update_PurchaseIDInBillingTransactions(long lBillingTransactionID, long lPurchaseID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PurchaseIDInBillingTransactions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);


            return sp.ExecuteReturnValue<bool>();
        }

        public static long Insert_NewBillingTransaction(string sSiteGuid, string sLastFourDigits, double dPrice,
           string sPriceCode, string sCurrencyCode, string sCustomData, int nBillingStatus, string sBillingReason,
           bool bIsRecurring, long lMediaFileID, long lMediaID, string sPPVModuleCode, string sSubscriptionCode,
           string sCellPhone, long lGroupID, long lBillingProvider, long lBillingProviderReference, double dPaymentMethodAddition,
           double dTotalPrice, int nPaymentNumber, int nNumberOfPayments, string sExtraParams, string sCountryCode,
           string sLanguageCode, string sDeviceName, int nBillingProcessor, int nBillingMethod, string sPrePaidCode,
           long lPreviewModuleID, string sCollectionCode, string billingGuid = null)
        {

            return Insert_NewBillingTransaction(sSiteGuid, sLastFourDigits, dPrice, sPriceCode, sCurrencyCode,
                sCustomData, nBillingStatus, sBillingReason, bIsRecurring, lMediaFileID, lMediaID, sPPVModuleCode,
                sSubscriptionCode, sCellPhone, lGroupID, lBillingProvider, lBillingProviderReference, dPaymentMethodAddition,
                dTotalPrice, nPaymentNumber, nNumberOfPayments, sExtraParams, sCountryCode, sLanguageCode, sDeviceName,
                nBillingProcessor, nBillingMethod, sPrePaidCode, lPreviewModuleID, 0, 0, 0, string.Empty, sCollectionCode, billingGuid);
        }

        public static long Insert_NewBillingTransaction(string sSiteGuid, string sLastFourDigits, double dPrice,
            string sPriceCode, string sCurrencyCode, string sCustomData, int nBillingStatus, string sBillingReason,
            bool bIsRecurring, long lMediaFileID, long lMediaID, string sPPVModuleCode, string sSubscriptionCode,
            string sCellPhone, long lGroupID, long lBillingProvider, long lBillingProviderReference, double dPaymentMethodAddition,
            double dTotalPrice, int nPaymentNumber, int nNumberOfPayments, string sExtraParams, string sCountryCode,
            string sLanguageCode, string sDeviceName, int nBillingProcessor, int nBillingMethod, string sPrePaidCode,
            long lPreviewModuleID, long lPurchaseID, int nFinancialProcessingStatus, int? nNewRenewableStatus, string sRemarks,
            string sCollectionCode, string billingGuid = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewBillingTransaction");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@LastFourDigits", sLastFourDigits);
            sp.AddParameter("@Price", dPrice);
            sp.AddParameter("@PaymentMethodAddition", dPaymentMethodAddition);
            sp.AddParameter("@TotalPrice", dTotalPrice);
            sp.AddParameter("@PriceCode", sPriceCode);
            sp.AddParameter("@CurrencyCode", sCurrencyCode);
            sp.AddParameter("@CustomData", sCustomData);
            sp.AddParameter("@BillingStatus", nBillingStatus);
            sp.AddParameter("@BillingReason", sBillingReason);
            sp.AddParameter("@IsRecurring", bIsRecurring ? 1 : 0);
            sp.AddParameter("@MediaFileID", lMediaFileID);
            sp.AddParameter("@MediaID", lMediaID);
            sp.AddParameter("@PPVModuleCode", sPPVModuleCode);
            sp.AddParameter("@SubscriptionCode", sSubscriptionCode);
            sp.AddParameter("@CellPhone", sCellPhone);
            sp.AddParameter("@BillingProvider", lBillingProvider);
            sp.AddParameter("@BillingProviderReference", lBillingProviderReference);
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@PaymentNumber", nPaymentNumber);
            sp.AddParameter("@NumberOfPayments", nNumberOfPayments);
            sp.AddParameter("@ExtraParams", sExtraParams);
            sp.AddParameter("@CountryCode", sCountryCode);
            sp.AddParameter("@LanguageCode", sLanguageCode);
            sp.AddParameter("@DeviceName", sDeviceName);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);
            sp.AddParameter("@GroupID", lGroupID);

            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@CreateDate", dtToWriteToDB);
            sp.AddParameter("@UpdaterID", DBNull.Value);
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@PublishDate", dtToWriteToDB);

            sp.AddParameter("@FinancialProcessingStatus", nFinancialProcessingStatus);
            if (nNewRenewableStatus.HasValue)
                sp.AddParameter("@NewRenewableStatus", nNewRenewableStatus.Value);
            else
                sp.AddParameter("@NewRenewableStatus", DBNull.Value);
            sp.AddParameter("@BillingProcessor", nBillingProcessor);
            sp.AddParameter("@BillingMethod", nBillingMethod);
            sp.AddParameter("@Remarks", sRemarks);
            sp.AddParameter("@PrePaidCode", sPrePaidCode);
            sp.AddParameter("@PreviewModuleID", lPreviewModuleID);
            if (string.IsNullOrEmpty(sCollectionCode))
            {
                sp.AddParameter("@CollectionCode", DBNull.Value);
            }
            else
            {
                sp.AddParameter("@CollectionCode", sCollectionCode);
            }

            sp.AddParameter("@BillingGuid", billingGuid);

            return sp.ExecuteReturnValue<long>();

        }

        public static bool Update_BillingTransactionStatus(long lBillingTransactionID, bool bIsActivate, ref long lBillingProviderTransactionID, ref long lPurchaseID)
        {
            bool res = false;
            int nNewStatusToWrite = bIsActivate ? 0 : 1;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_BillingTransactionStatus");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@NewStatus", nNewStatusToWrite);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                lBillingProviderTransactionID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0]["billing_provider_refference"]);
                lPurchaseID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0]["purchase_id"]);
                int nStatusInDB = -1;
                if (dt.Rows[0]["billing_status"] != DBNull.Value && dt.Rows[0]["billing_status"] != null && Int32.TryParse(dt.Rows[0]["billing_status"].ToString(), out nStatusInDB) && nStatusInDB > -1)
                    res = (nStatusInDB == nNewStatusToWrite);
            }

            return res;
        }

        public static DataTable Get_LastBillingTransactions(int nGroupID, List<long> nSubscriptionPurchasesIDs, List<int> mBillingProvidersList)
        {
            ODBCWrapper.StoredProcedure spLastBillingTransactions = new ODBCWrapper.StoredProcedure("Get_LastBillingTransactions");
            spLastBillingTransactions.SetConnectionKey("MAIN_CONNECTION_STRING");

            spLastBillingTransactions.AddParameter("@GroupID", nGroupID);
            spLastBillingTransactions.AddIDListParameter("@SubscriptionPurchasesIDs", nSubscriptionPurchasesIDs, "id");
            spLastBillingTransactions.AddIDListParameter("@BillingProvidersIDs", mBillingProvidersList, "id");

            DataSet ds = spLastBillingTransactions.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static long Get_LastNonGiftBillingMethod(string sSiteGuid, long lGroupID, long lBillingProvider)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_LastNonGiftBillingMethod");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@BillingProvider", lBillingProvider);

            return sp.ExecuteReturnValue<long>();
        }

        public static string Get_PPVNameByMediaID(long lMediaID)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVNameByMediaID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaID", lMediaID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["name"]);
                }
            }

            return res;
        }

        public static bool Update_BillingStatusAndReason(long lBillingTransactionID, bool bIsActivate, string sBillingReason)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_BillingStatusAndReason");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@BillingID", lBillingTransactionID);
            sp.AddParameter("@BillingStatus", bIsActivate ? 0 : 1);
            sp.AddParameter("@BillingReason", sBillingReason);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            return sp.ExecuteReturnValue<bool>();
        }


        public static bool Update_BillingStatusAndReason_ByBillingGuid(string billingGuid, int billingStatus, string billingReason)
        {
            bool result = false;

            try
            {
                long transactionId = 0;

                // Get Id by billing guid
                object id = ODBCWrapper.Utils.GetTableSingleVal("billing_transactions", "ID", "BILLING_GUID", "=", billingGuid, "MAIN_CONNECTION_STRING");

                transactionId = Convert.ToInt32(id);

                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Update_BillingStatusAndReason");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@BillingID", transactionId);
                storedProcedure.AddParameter("@BillingStatus", billingStatus);
                storedProcedure.AddParameter("@BillingReason", billingReason);
                storedProcedure.AddParameter("@UpdateDate", DateTime.UtcNow);

                result = storedProcedure.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed in Update_BillingStatusAndReason_ByBillingGuid. ex = {0}. Billing Guid = {1}", ex, billingGuid);
                result = false;
            }

            return result;
        }

        public static DateTime Get_PurchaseDateByBillingTransactionID(long lBillingTransactionID)
        {
            DateTime res = DateTime.UtcNow;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PurchaseDateByBillingTransactionID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["create_date"]);
                }
            }

            return res;
        }

        public static string Get_CustomDataFromBillingTransactions(long lBillingTransactionID)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CustomDataFromBillingTransactions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["customdata"]);
                }
            }

            return res;
        }

        public static DataTable Get_MediasMaxDuration(List<int> lMediaIDs)
        {
            ODBCWrapper.StoredProcedure spGetMediasMaxDuration = new ODBCWrapper.StoredProcedure("Get_MediasMaxDuration");
            spGetMediasMaxDuration.SetConnectionKey("MAIN_CONNECTION_STRING");

            spGetMediasMaxDuration.AddIDListParameter("@MediaIDs", lMediaIDs, "id");

            DataSet ds = spGetMediasMaxDuration.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }


        public static DataSet GetMCRules(int bmID, int groupID, int type)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMCRulesByBM");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@BMID", bmID);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Type", type);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
                return ds;
            return null;
        }

        public static DataTable GetMCRulesByID(int ruleID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMCRule");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@RuleID", ruleID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_MediaFileTypeDescription(int nMediaFileID, int nGroupID)
        {

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaFileTypeDescription");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaFileID", nMediaFileID);
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_Regions(int groupId, List<int> regionIds)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_Regions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@RegionIDs", regionIds, "id");
            sp.AddParameter("@GroupID", groupId);

            return sp.ExecuteDataSet();
        }

        /// <summary>
        /// Return Regions accroding to External Regions List
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="regionIds"></param>
        /// <returns></returns>
        public static DataSet Get_RegionsByExternalRegions(int groupId, List<string> externalRegionsList, RegionOrderBy orderBy)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ExternalRegions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@ExternalRegionIDs", externalRegionsList, "STR");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@orderBy", (int)orderBy);

            return sp.ExecuteDataSet();
        }

        /// <summary>
        /// Gets all parental rules active of a given group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<ParentalRule> Get_Group_ParentalRules(int groupId)
        {
            // Perform stored procedure

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Group_ParentalRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            return rules;
        }

        /// <summary>
        /// From a given dataset, returns the list of parental rules it (hopefully) contains
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        private static List<ParentalRule> CreateParentalRulesFromDataSet(DataSet dataSet)
        {
            List<ParentalRule> result = null;

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null)
            {
                if (dataSet.Tables.Count == 1)
                {
                    result = CreateParentalRulesFromSingleTable(dataSet);
                }
                else if (dataSet.Tables.Count == 2)
                {
                    Dictionary<long, ParentalRule> rules = new Dictionary<long, ParentalRule>();

                    DataTable rulesTable = dataSet.Tables[0];
                    DataTable tagsTable = dataSet.Tables[1];

                    // Run on first table and create initial list of parental rules, without tag values
                    if (rulesTable != null && rulesTable.Rows != null && rulesTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in rulesTable.Rows)
                        {
                            ParentalRule newRule = CreateParentalRuleFromRow(row);

                            // If rule is not positive, this means this is a dummy result: The user disabled the default
                            if (newRule.id > 0)
                            {
                                // Make sure we don't have an id like this already 
                                // (happens when user-rules table contains same rule id and user/domain in two different records)
                                if (!rules.ContainsKey(newRule.id))
                                {
                                    // Map in dictionary for easy access
                                    rules.Add(newRule.id, newRule);
                                }
                            }
                        }
                    }

                    if (tagsTable != null && tagsTable.Rows != null && tagsTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in tagsTable.Rows)
                        {
                            ParentalRule currentRule;

                            long ruleId = ODBCWrapper.Utils.ExtractValue<long>(row, "RULE_ID");

                            // Try to get the rule from the dictionary (should always succeed though)
                            if (rules.TryGetValue(ruleId, out currentRule))
                            {
                                // Asset type in database should match the enum!
                                eAssetTypes assetType = (eAssetTypes)ODBCWrapper.Utils.ExtractInteger(row, "ASSET_TYPE");
                                string value = ODBCWrapper.Utils.ExtractString(row, "VALUE");

                                // According to asset, update the relevant list
                                switch (assetType)
                                {
                                    case eAssetTypes.EPG:
                                        {
                                            currentRule.epgTagValues.Add(value);
                                            break;
                                        }
                                    case eAssetTypes.MEDIA:
                                        {
                                            currentRule.mediaTagValues.Add(value);
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }
                            }
                        }
                    }

                    result = rules.Values.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts data from fields and creates a parental rule object
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static ParentalRule CreateParentalRuleFromRow(DataRow row)
        {
            ParentalRule newRule = new ParentalRule();

            newRule.id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");
            newRule.name = ODBCWrapper.Utils.ExtractString(row, "NAME");
            newRule.description = ODBCWrapper.Utils.ExtractString(row, "DESCRIPTION");
            newRule.order = ODBCWrapper.Utils.ExtractInteger(row, "ORDER_NUM");
            newRule.mediaTagTypeId = ODBCWrapper.Utils.ExtractInteger(row, "MEDIA_TAG_TYPE_ID");
            newRule.epgTagTypeId = ODBCWrapper.Utils.ExtractInteger(row, "EPG_TAG_TYPE_ID");
            newRule.blockAnonymousAccess = ODBCWrapper.Utils.ExtractBoolean(row, "BLOCK_ANONYMOUS_ACCESS");
            newRule.ruleType = (eParentalRuleType)ODBCWrapper.Utils.ExtractInteger(row, "RULE_TYPE");

            int level = ODBCWrapper.Utils.ExtractInteger(row, "RULE_LEVEL");

            if (level != 0)
            {
                newRule.level = (eRuleLevel)level;
            }
            else
            {
                newRule.level = eRuleLevel.Group;
            }

            newRule.isDefault = ODBCWrapper.Utils.ExtractBoolean(row, "IS_DEFAULT");

            return newRule;
        }

        public static List<ParentalRule> Get_Domain_ParentalRules(int groupId, int domainId)
        {
            // Perform stored procedure

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Domain_ParentalRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@DomainID", domainId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            return rules;
        }

        public static List<ParentalRule> Get_User_ParentalRules(int groupId, string siteGuid)
        {
            // Perform stored procedure

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_User_ParentalRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            return rules;
        }

        public static int Set_UserParentalRule(int groupId, string siteGuid, long ruleId, int isActive)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_UserParentalRule");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@RuleID", ruleId);
            storedProcedure.AddParameter("@IsActive", isActive);
            storedProcedure.AddParameter("@GroupID", groupId);

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static int Set_DomainParentalRule(int groupId, int domainId, long ruleId, int isActive)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_DomainParentalRule");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@RuleID", ruleId);
            storedProcedure.AddParameter("@IsActive", isActive);
            storedProcedure.AddParameter("@GroupID", groupId);

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static string Get_ParentalPIN(int groupId, int domainId, string siteGuid, out eRuleLevel level, bool getUserDomain, int? ruleId)
        {
            string pin = null;
            level = eRuleLevel.User;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_UserDomainPIN");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", (int)eGroupRuleType.Parental);
            storedProcedure.AddParameter("@GetUserDomain", getUserDomain);
            storedProcedure.AddNullableParameter<int?>("@RuleId", ruleId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                DataTable table = dataSet.Tables[0];

                if (table != null && table.Rows != null && table.Rows.Count == 1)
                {
                    DataRow row = table.Rows[0];

                    pin = ODBCWrapper.Utils.ExtractString(row, "PIN");
                    level = (eRuleLevel)ODBCWrapper.Utils.ExtractInteger(row, "PIN_LEVEL");
                }
            }

            return pin;
        }

        public static int Set_ParentalPIN(int groupId, string siteGuid, int domainId, string pin, int? ruleId = null)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_UserDomainPin");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@Pin", pin);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", (int)eGroupRuleType.Parental);
            storedProcedure.AddNullableParameter<int?>("@RuleId", ruleId);

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static ePurchaeSettingsType Get_Group_PurchaseSetting(int groupId)
        {
            ePurchaeSettingsType purchaseSetting = ePurchaeSettingsType.Block;

            object value = ODBCWrapper.Utils.GetTableSingleVal("group_rule_settings", "DEFAULT_PURCHASE_SETTINGS", "GROUP_ID", "=", groupId);

            if (value != null && value != DBNull.Value)
            {
                purchaseSetting = (ePurchaeSettingsType)Convert.ToInt32(value);
            }

            return purchaseSetting;
        }

        public static string Get_Group_DefaultPIN(int groupId, eGroupRuleType ruleType)
        {
            string pin = string.Empty;
            string fieldName = string.Empty;

            switch (ruleType)
            {
                case eGroupRuleType.Unknown:
                    break;
                case eGroupRuleType.Parental:
                    {
                        fieldName = "DEFAULT_PARENTAL_PIN";
                        break;
                    }
                case eGroupRuleType.Purchase:
                    {
                        fieldName = "DEFAULT_PURCHASE_PIN";
                        break;
                    }
                case eGroupRuleType.Device:
                    break;
                case eGroupRuleType.EPG:
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                log.ErrorFormat("Get_Group_DefaultPIN - received bad rule type, cannot get. type = {0}", ruleType);
            }
            else
            {
                object value = ODBCWrapper.Utils.GetTableSingleVal("group_rule_settings", fieldName, "GROUP_ID", "=", groupId);

                if (value != null && value != DBNull.Value)
                {
                    pin = Convert.ToString(value);
                }
            }

            return pin;
        }

        public static bool Get_PurchaseSettings(int groupId, int domainId, string siteGuid, out eRuleLevel level, out ePurchaeSettingsType type)
        {
            bool success = false;
            level = eRuleLevel.User;
            type = ePurchaeSettingsType.Block;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_PurchaseSettings");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@GroupID", groupId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                DataTable table = dataSet.Tables[0];

                if (table != null && table.Rows != null && table.Rows.Count == 1)
                {
                    DataRow row = table.Rows[0];

                    type = (ePurchaeSettingsType)ODBCWrapper.Utils.ExtractInteger(row, "PURCHASE_SETTING");
                    level = (eRuleLevel)ODBCWrapper.Utils.ExtractInteger(row, "SETTING_LEVEL");

                    success = true;
                }
            }

            return success;
        }

        public static int Set_PurchaseSettings(int groupId, string siteGuid, int domainId, ePurchaeSettingsType ePurchaeSettingsType)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_PurchaseSettings");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@PurchaseSetting", (int)ePurchaeSettingsType);
            storedProcedure.AddParameter("@GroupID", groupId);

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static bool Get_PurchasePin(int groupId, int domainId, string siteGuid, out eRuleLevel level, out string pin, bool getUserDomain)
        {
            bool success = false;
            pin = null;
            level = eRuleLevel.User;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_UserDomainPIN");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", (int)eGroupRuleType.Purchase);
            storedProcedure.AddParameter("@GetUserDomain", getUserDomain);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                DataTable table = dataSet.Tables[0];

                if (table != null && table.Rows != null && table.Rows.Count == 1)
                {
                    DataRow row = table.Rows[0];

                    pin = ODBCWrapper.Utils.ExtractString(row, "PIN");
                    level = (eRuleLevel)ODBCWrapper.Utils.ExtractInteger(row, "PIN_LEVEL");

                    success = true;
                }
            }

            return success;
        }

        public static int Set_PurchasePIN(int groupId, string siteGuid, int domainId, string pin)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_UserDomainPin");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@Pin", pin);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", (int)eGroupRuleType.Purchase);

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static List<ParentalRule> Get_ParentalMediaRules(int groupId, string siteGuid, long mediaId, long domainId)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_ParentalMediaRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@MediaID", mediaId);
            storedProcedure.AddParameter("@DomainID", domainId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            return rules;
        }

        private static List<ParentalRule> CreateParentalRulesFromSingleTable(DataSet dataSet)
        {
            Dictionary<long, ParentalRule> rules = new Dictionary<long, ParentalRule>();

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                DataTable rulesAndTagsTable = dataSet.Tables[0];

                // Run on first table and create initial list of parental rules, without tag values
                if (rulesAndTagsTable != null && rulesAndTagsTable.Rows != null && rulesAndTagsTable.Rows.Count > 0)
                {
                    foreach (DataRow row in rulesAndTagsTable.Rows)
                    {
                        long id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");

                        // If rule is not positive, this means this is a dummy result: The user disabled the default
                        if (id > 0)
                        {
                            ParentalRule currentRule = null;

                            // First check if we have this rule already in dictionary
                            if (!rules.TryGetValue(id, out currentRule))
                            {
                                currentRule = CreateParentalRuleFromRow(row);

                                // Map in dictionary for easy access
                                rules[id] = currentRule;
                            }

                            eAssetTypes assetType = (eAssetTypes)ODBCWrapper.Utils.ExtractInteger(row, "ASSET_TYPE");

                            string value = ODBCWrapper.Utils.ExtractString(row, "VALUE");
                            // According to asset, update the relevant list
                            switch (assetType)
                            {
                                case eAssetTypes.EPG:
                                    {
                                        currentRule.epgTagValues.Add(value);
                                        break;
                                    }
                                case eAssetTypes.MEDIA:
                                    {
                                        currentRule.mediaTagValues.Add(value);
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                }
            }

            return rules.Values.ToList();
        }

        public static List<ParentalRule> Get_ParentalEPGRules(int groupId, string siteGuid, long epgId, long domainId)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_ParentalEPGRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@EpgID", epgId);
            storedProcedure.AddParameter("@DomainID", domainId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            return rules;
        }

        public static List<int> GetPermittedGeoBlockRules(int groupId, string ip)
        {
            List<int> result = new List<int>();


            // default = -1
            long ipValue = -1;

            if (string.IsNullOrEmpty(ip))
            {
                return result;
            }

            if (ip != "127.0.0.1")
            {
                string[] splitted = ip.Split('.');

                ipValue = Int64.Parse(splitted[3]) + Int64.Parse(splitted[2]) * 256 + Int64.Parse(splitted[1]) * 256 * 256 + Int64.Parse(splitted[0]) * 256 * 256 * 256;
            }

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Permitted_GeoBlockRules");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@IPValue", ipValue);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                DataTable table = dataSet.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");

                    result.Add(id);
                }
            }

            return result;
        }

        public static List<int> GetPermittedGeoBlockRulesByCountry(int groupId, int country)
        {
            List<int> result = new List<int>();

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Permitted_GeoBlockRules_ByCountry");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@Country", country);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                DataTable table = dataSet.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");

                    result.Add(id);
                }
            }

            return result;
        }

        public static void Get_User_ParentalRules_Tags(int groupId, string siteGuid,
            out List<TagPair> mediaTags, out List<TagPair> epgTags)
        {
            mediaTags = new List<TagPair>();
            epgTags = new List<TagPair>();

            // Perform stored procedure

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_User_ParentalRules_Tags");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count == 1)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    string name = ODBCWrapper.Utils.ExtractString(row, "NAME");
                    string value = ODBCWrapper.Utils.ExtractString(row, "VALUE");
                    // Asset type in database should match the enum!
                    eAssetTypes assetType = (eAssetTypes)ODBCWrapper.Utils.ExtractInteger(row, "ASSET_TYPE");

                    // According to asset, update the relevant list
                    switch (assetType)
                    {
                        case eAssetTypes.EPG:
                            {
                                epgTags.Add(new TagPair()
                                {
                                    key = name,
                                    value = value
                                });

                                break;
                            }
                        case eAssetTypes.MEDIA:
                            {
                                mediaTags.Add(new TagPair()
                                {
                                    key = name,
                                    value = value
                                });
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        public static long GetLinearMediaIdByEpgId(long epgId)
        {
            long mediaId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_LinearMediaIdByEpgId");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@epg_id", epgId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count > 0)
            {
                mediaId = ODBCWrapper.Utils.GetLongSafeVal(dataSet.Tables[0].Rows[0]["ID"]);
            }

            return mediaId;
        }

        #region OSSAdapter

        public static OSSAdapter GetOSSAdapterInternalID(int groupID, string externalIdentifier)
        {
            OSSAdapter ossAdapterRes = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OSSAdpterByExternalD");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@external_identifier", externalIdentifier);

                DataSet ds = sp.ExecuteDataSet();

                ossAdapterRes = CreateOSSAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return ossAdapterRes;
        }

        public static OSSAdapter InsertOSSAdapter(int groupID, OSSAdapter ossAdapter)
        {
            OSSAdapter ossAdapterRes = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_OSSAdapter");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@name", ossAdapter.Name);
                sp.AddParameter("@adapter_url", ossAdapter.AdapterUrl);
                sp.AddParameter("@external_identifier", ossAdapter.ExternalIdentifier);
                sp.AddParameter("@shared_secret", ossAdapter.SharedSecret);
                sp.AddParameter("@isActive", ossAdapter.IsActive);

                DataTable dt = CreateDataTable(ossAdapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                DataSet ds = sp.ExecuteDataSet();

                ossAdapterRes = CreateOSSAdapter(ds);
            }

            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ossAdapterRes;
        }

        private static OSSAdapter CreateOSSAdapter(DataSet ds)
        {
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow adapterRow = ds.Tables[0].Rows[0];
                DataTable settingsTable = null;

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    settingsTable = ds.Tables[1];
                }

                return CreateOSSAdapter(adapterRow, settingsTable);
            }

            return null;
        }

        private static OSSAdapter CreateOSSAdapter(DataRow adapterRow, DataTable settingsTable)
        {
            OSSAdapter ossAdapterRes = null;

            ossAdapterRes = new OSSAdapter();
            ossAdapterRes.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(adapterRow, "adapter_url");
            ossAdapterRes.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(adapterRow, "external_identifier");
            ossAdapterRes.ID = ODBCWrapper.Utils.GetIntSafeVal(adapterRow, "ID");
            int is_Active = ODBCWrapper.Utils.GetIntSafeVal(adapterRow, "is_active");
            ossAdapterRes.IsActive = is_Active == 1 ? true : false;
            ossAdapterRes.Name = ODBCWrapper.Utils.GetSafeStr(adapterRow, "name");
            ossAdapterRes.SharedSecret = ODBCWrapper.Utils.GetSafeStr(adapterRow, "shared_secret");

            if (settingsTable != null)
            {
                foreach (DataRow dr in settingsTable.Rows)
                {
                    int ossAdapterId = ODBCWrapper.Utils.GetIntSafeVal(dr, "OSS_Adapter_id", 0);
                    if (ossAdapterId > 0 && ossAdapterId != ossAdapterRes.ID)
                        continue;

                    string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                    string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                    if (ossAdapterRes.Settings == null)
                    {
                        ossAdapterRes.Settings = new List<OSSAdapterSettings>();
                    }
                    ossAdapterRes.Settings.Add(new OSSAdapterSettings(key, value));
                }
            }

            return ossAdapterRes;

        }

        private static DataTable CreateDataTable(List<OSSAdapterSettings> list)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            try
            {
                resultTable.Columns.Add("idkey", typeof(string));
                resultTable.Columns.Add("value", typeof(string));

                foreach (OSSAdapterSettings item in list)
                {
                    DataRow row = resultTable.NewRow();
                    row["idkey"] = item.key;
                    row["value"] = item.value;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }

            return resultTable;
        }

        public static bool DeleteOSSAdapter(int groupID, int ossAdapterId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_OSSAdapter");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", ossAdapterId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public static OSSAdapter SetOSSAdapter(int groupID, OSSAdapter ossAdapter)
        {
            OSSAdapter ossAdapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_OSSAdapter");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", ossAdapter.ID);
                sp.AddParameter("@name", ossAdapter.Name);
                sp.AddParameter("@external_identifier", ossAdapter.ExternalIdentifier);
                sp.AddParameter("@shared_secret", ossAdapter.SharedSecret);
                sp.AddParameter("@adapter_url", ossAdapter.AdapterUrl);
                sp.AddParameter("@isActive", ossAdapter.IsActive);

                DataSet ds = sp.ExecuteDataSet();

                ossAdapterRes = CreateOSSAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ossAdapterRes;
        }

        public static OSSAdapter SetOSSAdapterSharedSecret(int groupID, int ossAdapterId, string sharedSecret)
        {
            OSSAdapter ossAdapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_OSSAdapterSharedSecret");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@id", ossAdapterId);
                sp.AddParameter("@sharedSecret", sharedSecret);

                DataSet ds = sp.ExecuteDataSet();

                ossAdapterRes = CreateOSSAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ossAdapterRes;
        }


        public static List<OSSAdapter> GetOSSAdapterList(int groupID, int status = 1, int isActive = 1)
        {
            List<OSSAdapter> res = new List<OSSAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OSSAdapterList");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    DataTable settingsTable = null;

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        settingsTable = ds.Tables[1];
                    }

                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        OSSAdapter ossAdapter = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            ossAdapter = CreateOSSAdapter(dr, settingsTable);
                            res.Add(ossAdapter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<OSSAdapter>();
            }
            return res;
        }

        public static OSSAdapter GetOSSAdapter(int groupID, int ossAdapterId, int? isActive = null, int status = 1)
        {
            OSSAdapter ossAdapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OSSAdapter");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ossAdapterId", ossAdapterId);
                sp.AddParameter("@status", status);
                if (isActive.HasValue)
                {
                    sp.AddParameter("@isActive", isActive.Value);
                }

                DataSet ds = sp.ExecuteDataSet();

                ossAdapterRes = CreateOSSAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return ossAdapterRes;
        }

        public static bool InsertOSSAdapterSettings(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_OSSAdapterSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", ossAdapterId);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isInsert = sp.ExecuteReturnValue<bool>();
                return isInsert;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static bool SetOSSAdapterSettings(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_OSSAdapterSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", ossAdapterId);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static bool DeleteOSSAdapter(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_OSSAdapterSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", ossAdapterId);
                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static List<OSSAdapter> GetOSSAdapterSettingsList(int groupID, int ossAdapterId = 0, int status = 1, int isActive = 1)
        {
            List<OSSAdapter> res = new List<OSSAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OSSAdapterSettingsList");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ossAdapterId", ossAdapterId);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    DataTable dtConfig = ds.Tables[1];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        OSSAdapter ossAdapter = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            ossAdapter = new OSSAdapter();
                            ossAdapter.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            ossAdapter.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            ossAdapter.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                            ossAdapter.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            ossAdapter.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            int is_Active = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            ossAdapter.IsActive = is_Active == 1 ? true : false;

                            if (dtConfig != null)
                            {
                                DataRow[] drpc = dtConfig.Select("oss_adapter_id =" + ossAdapter.ID);

                                foreach (DataRow drp in drpc)
                                {
                                    string key = ODBCWrapper.Utils.GetSafeStr(drp, "key");
                                    string value = ODBCWrapper.Utils.GetSafeStr(drp, "value");
                                    if (ossAdapter.Settings == null)
                                    {
                                        ossAdapter.Settings = new List<OSSAdapterSettings>();
                                    }
                                    ossAdapter.Settings.Add(new OSSAdapterSettings(key, value));
                                }
                            }
                            res.Add(ossAdapter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<OSSAdapter>();
            }
            return res;
        }

        #endregion

        public static DataTable Get_IPToCountryTable()
        {
            DataTable table = null;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_IP_To_Country");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                table = dataSet.Tables[0];
            }

            return table;
        }

        public static List<int> GetAllCountries()
        {
            List<int> countries = new List<int>();

            DataTable table = ODBCWrapper.Utils.GetCompleteTable("countries");

            if (table != null)
            {
                // Convert table to list of IDs
                countries = table.Rows.Cast<DataRow>().Select(row => ODBCWrapper.Utils.ExtractInteger(row, "ID")).ToList();
            }

            return countries;
        }

        public static Dictionary<long, ParentalRule> Get_Group_ParentalRules_ByID(int groupId, List<long> ids)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Group_ParentalRules_ByID");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddIDListParameter("@IDs", ids, "ID");

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            List<ParentalRule> rules = CreateParentalRulesFromDataSet(dataSet);

            Dictionary<long, ParentalRule> dictionary = new Dictionary<long, ParentalRule>();

            for (int i = 0; i < ids.Count; i++)
            {
                dictionary.Add(ids[i], null);
            }

            foreach (ParentalRule rule in rules)
            {
                dictionary[rule.id] = rule;
            }

            return dictionary;
        }

        public static List<long> Get_User_ParentalRulesIDs(int groupId, string siteGuid)
        {
            List<long> ruleIds = new List<long>();
            // Perform stored procedure

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_User_ParentalRulesIDs");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            // Validate tables count
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                DataTable table = dataSet.Tables[0];

                // Run on first table and create initial list of parental rules, without tag values
                if (table != null && table.Rows != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        long id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");

                        if (id > 0)
                        {
                            ruleIds.Add(id);

                        }
                    }
                }
            }
            return ruleIds;
        }

        #region Bulk Export
        public static BulkExportTask InsertBulkExportTask(int groupId, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType, long frequency,
            string notificationUrl, List<int> vodTypes, string version, bool isActive)
        {
            BulkExportTask task = null;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Insert_BulkExportTask");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@name", name);
            storedProcedure.AddParameter("@external_key", externalKey);
            storedProcedure.AddParameter("@data_type", dataType);
            storedProcedure.AddParameter("@filter", filter);
            storedProcedure.AddParameter("@export_type", exportType);
            storedProcedure.AddParameter("@frequency", frequency);
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddParameter("@updater_id", null);
            storedProcedure.AddParameter("@version", version);
            storedProcedure.AddParameter("@notification_url", notificationUrl);
            storedProcedure.AddParameter("@is_active", isActive ? 1 : 0);
            storedProcedure.AddIDListParameter("@vod_types", vodTypes, "ID");

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            var tasks = BuildExportTasksFromDataSet(dataSet);

            if (tasks != null && tasks.Count > 0)
            {
                task = tasks[0];
            }

            return task;
        }

        public static BulkExportTask UpdateBulkExportTask(int groupId, long id, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType, long frequency,
             string notificationUrl, List<int> vodTypes, string version, bool? isActive)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Update_BulkExportTaskAndGet");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddParameter("@id", id);
            storedProcedure.AddParameter("@external_key", !string.IsNullOrEmpty(externalKey) ? externalKey : null);
            storedProcedure.AddParameter("@name", name);
            storedProcedure.AddParameter("@data_type", dataType);
            storedProcedure.AddParameter("@filter", filter);
            storedProcedure.AddParameter("@export_type", exportType);
            storedProcedure.AddParameter("@frequency", frequency);
            storedProcedure.AddParameter("@version", version);
            storedProcedure.AddParameter("@notification_url", notificationUrl);
            if (isActive != null)
            {
                storedProcedure.AddParameter("@is_active", isActive.Value ? 1 : 0);
            }
            else
            {
                storedProcedure.AddParameter("@is_active", null);
            }
            storedProcedure.AddIDListParameter("@vod_types", vodTypes, "ID");
            
            DataSet dataSet = storedProcedure.ExecuteDataSet();
            DataTable tasksTable = dataSet.Tables[0];

            BulkExportTask task = BuildExportTaskFromDataRow(tasksTable.Rows[0]);
            return task;
        }

        public static bool DeleteBulkExportTask(int groupId, long? id, string externalKey)
        {
            int rowCount = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Delete_BulkExportTask");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddParameter("@id", id != 0 ? id : null);
            storedProcedure.AddParameter("@external_key", !string.IsNullOrEmpty(externalKey) ? externalKey : null);

            rowCount = storedProcedure.ExecuteReturnValue<int>();

            return rowCount > 0;
        }

        public static List<BulkExportTask> GetBulkExportTasks(List<long> ids, List<string> externalKeys, int groupId, int orderBy = 0)
        {
            List<BulkExportTask> tasks = new List<BulkExportTask>();

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_BulkExportTasks");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddIDListParameter("@ids", ids != null && ids.Count > 0 ? ids : null, "ID");
            storedProcedure.AddIDListParameter("@external_keys", externalKeys != null && externalKeys.Count > 0 ? externalKeys : null, "STR");
            storedProcedure.AddParameter("@order_by", orderBy);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            tasks = BuildExportTasksFromDataSet(dataSet);
            return tasks;
        }

        private static BulkExportTask BuildExportTaskFromDataRow(DataRow row)
        {
            return new BulkExportTask()
            {
                DataType = (eBulkExportDataType)ODBCWrapper.Utils.GetIntSafeVal(row, "DATA_TYPE"),
                ExportType = (eBulkExportExportType)ODBCWrapper.Utils.GetIntSafeVal(row, "EXPORT_TYPE"),
                ExternalKey = ODBCWrapper.Utils.GetSafeStr(row, "EXTERNAL_KEY"),
                Filter = ODBCWrapper.Utils.GetSafeStr(row, "FILTER"),
                Frequency = ODBCWrapper.Utils.GetLongSafeVal(row, "FREQUENCY"),
                Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                Version = ODBCWrapper.Utils.GetSafeStr(row, "VERSION"),
                InProcess = ODBCWrapper.Utils.GetIntSafeVal(row, "IN_PROCESS") == 0 ? false : true,
                LastProcess = ODBCWrapper.Utils.GetNullableDateSafeVal(row, "LAST_PROCESS"),
                NotificationUrl = ODBCWrapper.Utils.GetSafeStr(row, "NOTIFICATION_URL"),
                IsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ACTIVE") == 0 ? false : true
            };
        }

        private static List<BulkExportTask> BuildExportTasksFromDataSet(DataSet dataSet)
        {
            List<BulkExportTask> tasks = new List<BulkExportTask>();

            if (dataSet != null && dataSet.Tables != null)
            {
                if (dataSet.Tables.Count >= 2)
                {
                    DataTable tasksTable = dataSet.Tables[0];
                    DataTable taskVodTypesTable = dataSet.Tables[1];

                    //build tasks
                    if (tasksTable != null && tasksTable.Rows != null && tasksTable.Rows.Count > 0)
                    {
                        tasks = new List<BulkExportTask>();

                        foreach (DataRow row in tasksTable.Rows)
                        {
                            tasks.Add(BuildExportTaskFromDataRow(row));
                        }
                    }

                    // build tasks' vod types lists
                    if (tasks != null && tasks.Count > 0 &&
                        taskVodTypesTable != null && taskVodTypesTable.Rows != null && taskVodTypesTable.Rows.Count > 0)
                    {
                        int typeId;
                        long taskId;
                        BulkExportTask task = null;

                        foreach (DataRow row in taskVodTypesTable.Rows)
                        {
                            taskId = ODBCWrapper.Utils.GetLongSafeVal(row, "TASK_ID");
                            typeId = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_TYPE_ID");
                            if (taskId != 0 && typeId != 0)
                            {
                                task = tasks.Where(t => t.Id == taskId).FirstOrDefault();
                                if (task != null)
                                {
                                    task.VodTypes.Add(typeId);
                                }
                            }
                        }
                    }
                }
            }
            return tasks;
        }

        public static bool SetBulkExportTaskProcess(long id, bool inProcess, DateTime? lastProcess = null)
        {
            int rowCount = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_BulkExportTaskProcess");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@id", id);
            storedProcedure.AddParameter("@in_process", inProcess ? 1 : 0);
            storedProcedure.AddParameter("@last_process", lastProcess);

            rowCount = storedProcedure.ExecuteReturnValue<int>();

            return rowCount > 0;
        }

        public static DataSet GetNotActiveMedia(int groupId, DateTime since)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_NotActiveMedia");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddParameter("@since", since);

            return storedProcedure.ExecuteDataSet();
        }

        public static DataSet GetNotActivePrograms(int groupId, DateTime since)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_NotActivePrograms");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@group_id", groupId);
            storedProcedure.AddParameter("@since", since);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            return dataSet;
        }
        #endregion

        public static List<MessageQueue> GetQueueMessages(int groupId, long baseDateSec, List<string> messageDataTypes)
        {
            List<MessageQueue> messageQueues = new List<MessageQueue>();

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SCHEDULED_TASKS);

            foreach (string messageDataType in messageDataTypes)
            {
                try
                {
                    // get views
                    messageQueues.AddRange(cbManager.View<MessageQueue>(new ViewManager(CB_MESSAGE_QUEUE_DESGIN, "queue_messages")
                    {
                        startKey = new object[] { messageDataType.ToLower(), baseDateSec },
                        endKey = new object[] { messageDataType.ToLower(), DateTimeToUnixTimestamp(DateTime.MaxValue) },
                        staleState = ViewStaleState.False
                    }));
                }
                catch (Exception ex)
                {
                    log.Error("Error while get Queue Messages", ex);
                }
            }

            return messageQueues;
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        public static List<Role> GetRoles(int groupId, List<long> roleIds)
        {
            List<Role> roles = new List<Role>();

            // roles permission dictionary holds a dictionary of permissions for each role - dictionary<role id, <permission id, permission>>
            Dictionary<long, Dictionary<long, Permission>> rolesPermissions = new Dictionary<long, Dictionary<long, Permission>>();

            // permissions permission items dictionary holds a dictionary of permission items for each permission - dictionary<permission id, <permission item id, permission item>>
            Dictionary<long, Dictionary<long, PermissionItem>> permissionPermissionItems = new Dictionary<long, Dictionary<long, PermissionItem>>();

            try
            {
                // get the roles, permissions, permission items tables 
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_Roles");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddIDListParameter<long>("@role_ids", roleIds, "ID");

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 3)
                {
                    DataTable rolesTable = ds.Tables[0];
                    DataTable permissionsTable = ds.Tables[1];
                    DataTable permissionItemsTable = ds.Tables[2];

                    Role role;

                    // build permission permission items dictionary
                    permissionPermissionItems = BuildPermissionItems(permissionItemsTable);

                    // build roles permissions dictionary
                    rolesPermissions = BuildPermissions(groupId, permissionPermissionItems, permissionsTable);

                    // build roles
                    if (rolesTable != null && rolesTable.Rows != null && rolesTable.Rows.Count > 0)
                    {
                        foreach (DataRow rolesRow in rolesTable.Rows)
                        {
                            // create new role
                            role = new Role()
                            {
                                Id = ODBCWrapper.Utils.GetLongSafeVal(rolesRow, "ID"),
                                Name = ODBCWrapper.Utils.GetSafeStr(rolesRow, "NAME"),
                            };

                            // add permissions for role if exists
                            if (rolesPermissions != null && rolesPermissions.ContainsKey(role.Id) && rolesPermissions[role.Id] != null)
                            {
                                role.Permissions = rolesPermissions[role.Id].Values.ToList();
                            }

                            // add role
                            roles.Add(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                roles = null;
                log.Error(string.Format("Error while group roles from DB, group id = {0}", groupId), ex);
            }

            return roles;
        }

        private static Dictionary<long, Dictionary<long, Permission>> BuildPermissions(int groupId, Dictionary<long, Dictionary<long, PermissionItem>> permissionPermissionItems, DataTable permissionsTable)
        {
            Dictionary<long, Dictionary<long, Permission>> rolesPermissions = new Dictionary<long, Dictionary<long, Permission>>();

            Permission permission;
            long roleId;
            int retrievedGroupId;
            bool isExcluded;
            ePermissionType permissionType;


            if (permissionsTable != null && permissionsTable.Rows != null && permissionsTable.Rows.Count > 0)
            {
                foreach (DataRow permissionItemsRow in permissionsTable.Rows)
                {
                    isExcluded = false;
                    permissionType = (ePermissionType)ODBCWrapper.Utils.GetIntSafeVal(permissionItemsRow, "TYPE");
                    retrievedGroupId = ODBCWrapper.Utils.GetIntSafeVal(permissionItemsRow, "GROUP_ID");

                    // if the role - permission connection is overridden by another group - get the exclusion status of the connection
                    if (groupId != 0)
                    {
                        isExcluded = ODBCWrapper.Utils.GetIntSafeVal(permissionItemsRow, "IS_EXCLUDED") == 1 ? true : false;
                    }

                    // build the permission object depending on the type
                    switch (permissionType)
                    {
                        case ePermissionType.Normal:
                            permission = new Permission();
                            break;
                        case ePermissionType.Group:
                            permission = new GroupPermission()
                            {
                                UsersGroup = ODBCWrapper.Utils.GetSafeStr(permissionItemsRow, "USERS_GROUP")
                            };
                            break;
                        default:
                            permission = null;
                            break;
                    }
                    if (permission != null)
                    {
                        permission.Id = ODBCWrapper.Utils.GetLongSafeVal(permissionItemsRow, "ID");
                        permission.Name = ODBCWrapper.Utils.GetSafeStr(permissionItemsRow, "NAME");
                        permission.GroupId = groupId;
                        if (permissionPermissionItems != null && permissionPermissionItems.ContainsKey(permission.Id) && permissionPermissionItems[permission.Id] != null)
                        {
                            permission.PermissionItems = permissionPermissionItems[permission.Id].Values.ToList();
                        }

                        roleId = ODBCWrapper.Utils.GetLongSafeVal(permissionItemsRow, "ROLE_ID");

                        // add the connection and the permission to the role - permission dictionary 
                        // if the role is not yet in the dictionary add it
                        if (!rolesPermissions.ContainsKey(roleId))
                        {
                            rolesPermissions.Add(roleId, new Dictionary<long, Permission>());
                        }
                        // if the connection should be excluded for this group - remove it from the dictionary
                        if (isExcluded && rolesPermissions[roleId].ContainsKey(permission.Id))
                        {
                            rolesPermissions[roleId].Remove(permission.Id);
                        }
                        // add it
                        else if (!rolesPermissions[roleId].ContainsKey(permission.Id))
                        {
                            rolesPermissions[roleId].Add(permission.Id, permission);
                        }
                    }
                }
            }

            return rolesPermissions;
        }

        private static Dictionary<long, Dictionary<long, PermissionItem>> BuildPermissionItems(DataTable permissionItemsTable)
        {
            Dictionary<long, Dictionary<long, PermissionItem>> permissionPermissionItems = new Dictionary<long, Dictionary<long, PermissionItem>>();

            if (permissionItemsTable != null && permissionItemsTable.Rows != null && permissionItemsTable.Rows.Count > 0)
            {
                PermissionItem permissionItem;
                long permissionId;
                int groupId;
                ePermissionItemType permissionItemType;
                bool isExcluded;

                foreach (DataRow permissionsRow in permissionItemsTable.Rows)
                {
                    isExcluded = false;

                    permissionItemType = (ePermissionItemType)ODBCWrapper.Utils.GetIntSafeVal(permissionsRow, "TYPE");
                    groupId = ODBCWrapper.Utils.GetIntSafeVal(permissionsRow, "GROUP_ID");

                    // if the permission - permission item connection is overridden by another group - get the exclusion status of the connection
                    if (groupId != 0)
                    {
                        isExcluded = ODBCWrapper.Utils.GetIntSafeVal(permissionsRow, "IS_EXCLUDED") == 1 ? true : false;
                    }

                    // build the permission item object depending on the type
                    switch (permissionItemType)
                    {
                        case ePermissionItemType.Action:
                            permissionItem = new ApiActionPermissionItem()
                            {
                                Action = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "ACTION"),
                                Service = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "SERVICE")
                            };
                            break;
                        case ePermissionItemType.Parameter:
                            permissionItem = new ApiParameterPermissionItem()
                            {
                                Object = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "OBJECT"),
                                Parameter = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "PARAMETER"),
                                Action = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "ACTION"),
                            };
                            break;
                        case ePermissionItemType.Argument:
                            permissionItem = new ApiArgumentPermissionItem()
                            {
                                Service = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "SERVICE"),
                                Action = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "ACTION"),
                                Parameter = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "PARAMETER"),
                            };
                            break;
                        default:
                            permissionItem = null;
                            break;
                    }

                    if (permissionItem != null)
                    {
                        permissionItem.Id = ODBCWrapper.Utils.GetLongSafeVal(permissionsRow, "ID");
                        permissionItem.Name = ODBCWrapper.Utils.GetSafeStr(permissionsRow, "NAME");


                        permissionId = ODBCWrapper.Utils.GetLongSafeVal(permissionsRow, "PERMISSION_ID");

                        // add the connection and the permission item to the permission - permission items dictionary 
                        // if the permission is not yet in the dictionary add it
                        if (!permissionPermissionItems.ContainsKey(permissionId))
                        {
                            permissionPermissionItems.Add(permissionId, new Dictionary<long, PermissionItem>());
                        }

                        // if the connection should be excluded for this group - remove it from the dictionary
                        if (isExcluded && permissionPermissionItems[permissionId].ContainsKey(permissionItem.Id))
                        {
                            permissionPermissionItems[permissionId].Remove(permissionItem.Id);
                        }
                        // add it
                        else if (!permissionPermissionItems[permissionId].ContainsKey(permissionItem.Id))
                        {
                            permissionPermissionItems[permissionId].Add(permissionItem.Id, permissionItem);
                        }
                    }
                }
            }

            return permissionPermissionItems;
        }

        public static List<Permission> GetPermissions(int groupId, List<long> permissionIds)
        {
            List<Permission> permissions = new List<Permission>();
            Dictionary<long, Dictionary<long, Permission>> rolesPermissions = new Dictionary<long, Dictionary<long, Permission>>();
            Dictionary<long, Dictionary<long, PermissionItem>> permissionPermissionItems = new Dictionary<long, Dictionary<long, PermissionItem>>();

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_Permissions");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddIDListParameter<long>("@permission_ids", permissionIds, "ID");

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    DataTable permissionsTable = ds.Tables[0];
                    DataTable permissionItemsTable = ds.Tables[1];

                    permissionPermissionItems = BuildPermissionItems(permissionItemsTable);

                    rolesPermissions = BuildPermissions(groupId, permissionPermissionItems, permissionsTable);

                    if (rolesPermissions != null && rolesPermissions.Count > 0 && rolesPermissions[0] != null)
                        permissions = rolesPermissions[0].Values.ToList();
                }
            }
            catch (Exception ex)
            {
                permissions = null;
                log.Error(string.Format("Error while getting permissions from DB, group id = {0}", groupId), ex);
            }

            return permissions;
        }

        public static Permission InsertPermission(int groupId, string name, List<long> permissionItemsIds, ePermissionType type, string usersGroup, long updaterId)
        {
            Permission permission = new Permission();
            Dictionary<long, Dictionary<long, Permission>> rolesPermissions = new Dictionary<long, Dictionary<long, Permission>>();
            Dictionary<long, Dictionary<long, PermissionItem>> permissionPermissionItems = new Dictionary<long, Dictionary<long, PermissionItem>>();

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_Permission");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@type", (int)type);
                sp.AddParameter("@users_group", usersGroup);
                sp.AddParameter("@updater_id", updaterId);
                sp.AddIDListParameter<long>("@permission_item_ids", permissionItemsIds, "ID");

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    DataTable permissionsTable = ds.Tables[0];
                    DataTable permissionItemsTable = ds.Tables[1];

                    permissionPermissionItems = BuildPermissionItems(permissionItemsTable);

                    rolesPermissions = BuildPermissions(groupId, permissionPermissionItems, permissionsTable);

                    if (rolesPermissions != null && rolesPermissions.Count > 0 && rolesPermissions[0] != null && rolesPermissions[0].Count > 0)
                        permission = rolesPermissions[0][0];
                }
            }
            catch (Exception ex)
            {
                permission = null;
                log.Error(string.Format("Error while inserting new permission to DB, group id = {0}", groupId), ex);
            }

            return permission;
        }

        public static int InsertRolePermission(int groupId, long roleId, long permissionId)
        {
            int rowCount = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_RolePermission");
                sp.AddParameter("@permission_id", permissionId);
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@role_id", roleId);
                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while adding permission to role in DB, group id = {0}", groupId), ex);
            }

            return rowCount;
        }

        public static int InsertPermissionPermissionItem(int groupId, long permissionId, long permissionItemId)
        {
            int rowCount = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_PermissionPermissionItem");
                sp.AddParameter("@permission_id", permissionId);
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@permission_item_id", permissionItemId);
                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while adding permission to role in DB, group id = {0}", groupId), ex);
            }

            return rowCount;
        }

        public static int UpdateImageState(int groupId, long rowId, int version, eTableStatus status, int? updaterId)
        {
            int result = -1;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("pics");
            try
            {
                if (status == eTableStatus.OK)
                {
                    // update image upload success only if current version is lower then updated version
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VERSION", "=", version);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)status);
                    if (updaterId.HasValue)
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", updaterId.Value);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", rowId);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VERSION", "<=", version);
                }

                if (status == eTableStatus.Failed)
                {
                    // update image upload failed only if current status is "Pending"
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)status);
                    if (updaterId.HasValue)
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", updaterId.Value);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", rowId);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)eTableStatus.Pending);
                }

                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                result = updateQuery.ExecuteAffectedRows();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to update image state. groupId: {0}, picId: {1}, version: {2}. ex: {3}", groupId, rowId, version, ex);
            }
            finally
            {
                updateQuery.Finish();
                updateQuery = null;
            }

            return result;
        }

        public static int UpdateEpgImageState(int groupId, long rowId, int version, eTableStatus status, int? updaterId)
        {
            int result = -1;
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("EPG_pics");

            try
            {
                if (status == eTableStatus.OK)
                {
                    // update image upload success only if current version is lower then updated version
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VERSION", "=", version);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)status);
                    if (updaterId.HasValue)
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", updaterId.Value);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", rowId);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VERSION", "<=", version);
                }

                if (status == eTableStatus.Failed)
                {
                    // update image upload failed only if current status is "Pending"
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)status);
                    if (updaterId.HasValue)
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", updaterId.Value);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", rowId);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", (int)eTableStatus.Pending);
                }

                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                result = updateQuery.ExecuteAffectedRows();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to update EPG image state. groupId: {0}, EPG picId: {1}, version: {2}. ex: {3}", groupId, rowId, version, ex);
            }
            finally
            {
                updateQuery.Finish();
                updateQuery = null;
            }

            return result;
        }

        public static List<RegistrySettings> GetAllRegistry(int groupID)
        {
            List<RegistrySettings> result = null;
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_AllRegistry");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<RegistrySettings>();
                    RegistrySettings registrySetting;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        registrySetting = new RegistrySettings();
                        registrySetting.key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                        registrySetting.value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                        result.Add(registrySetting);
                    }

                    return result;
                }
                return new List<RegistrySettings>();
            }
            catch
            {
                return new List<RegistrySettings>();
            }
        }

        public static List<KeyValuePair<int, DateTime>> GetFreeItemsToInitialize(int groupId, ref int mediaFileIDMin)
        {
            List<KeyValuePair<int, DateTime>> itemsToInitialize = null;
            ODBCWrapper.StoredProcedure spGetMediasByPPVModuleID = new ODBCWrapper.StoredProcedure("Get_FreeItemsToInitialize");
            spGetMediasByPPVModuleID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetMediasByPPVModuleID.AddParameter("@GroupID", groupId);
            spGetMediasByPPVModuleID.AddParameter("@MediaFileIDMin", mediaFileIDMin);

            DataTable dt = spGetMediasByPPVModuleID.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                itemsToInitialize = new List<KeyValuePair<int, DateTime>>();
                foreach (DataRow dr in dt.Rows)
                {
                    int mediaID = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id");
                    mediaFileIDMin = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                    if (mediaID > 0)
                    {
                        DateTime? mediaFileStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "media_file_start_date");
                        DateTime? mediaFileEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "media_file_end_date");
                        DateTime? ppvModuleMediaFileStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "ppv_module_media_file_start_date");
                        DateTime? ppvModuleMediaFileEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "ppv_module_media_file_end_date");
                        if (mediaFileStartDate.HasValue && mediaFileStartDate.Value > DateTime.UtcNow && mediaFileStartDate.Value <= DateTime.UtcNow.AddYears(2))
                        {
                            itemsToInitialize.Add(new KeyValuePair<int, DateTime>(mediaID, mediaFileStartDate.Value));
                        }
                        if (mediaFileEndDate.HasValue && mediaFileEndDate.Value > DateTime.UtcNow && mediaFileEndDate.Value <= DateTime.UtcNow.AddYears(2))
                        {
                            itemsToInitialize.Add(new KeyValuePair<int, DateTime>(mediaID, mediaFileEndDate.Value));
                        }
                        if (ppvModuleMediaFileStartDate.HasValue && ppvModuleMediaFileStartDate.Value > DateTime.UtcNow && ppvModuleMediaFileStartDate.Value <= DateTime.UtcNow.AddYears(2))
                        {
                            itemsToInitialize.Add(new KeyValuePair<int, DateTime>(mediaID, ppvModuleMediaFileStartDate.Value));
                        }
                        if (ppvModuleMediaFileEndDate.HasValue && ppvModuleMediaFileEndDate.Value > DateTime.UtcNow && ppvModuleMediaFileEndDate.Value <= DateTime.UtcNow.AddYears(2))
                        {
                            itemsToInitialize.Add(new KeyValuePair<int, DateTime>(mediaID, ppvModuleMediaFileEndDate.Value));
                        }
                    }
                }
            }
            return itemsToInitialize;
        }

        public static bool UpdateBillingTransactionGuid(long lBillingTransactionID, string billingGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_BillingTransactionGuid");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ID", lBillingTransactionID);
            sp.AddParameter("@BillingGuid", billingGuid);

            return sp.ExecuteReturnValue<bool>();
        }

        public static DataRow GetTimeShiftedTvPartnerSettings(int groupID)
        {
            DataRow dr = null;
            try
            {
                ODBCWrapper.StoredProcedure spGetTimeShiftedTvPartnerSettings = new ODBCWrapper.StoredProcedure("GetTimeShiftedTvPartnerSettings");
                spGetTimeShiftedTvPartnerSettings.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGetTimeShiftedTvPartnerSettings.AddParameter("@GroupID", groupID);

                DataTable dt = spGetTimeShiftedTvPartnerSettings.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    dr = dt.Rows[0];
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed getting TimeShiftedTvPartnerSettings when running the stored procedure: GetTimeShiftedTvPartnerSettings", ex);
            }

            return dr;
        }

        public static bool UpdateTimeShiftedTvPartnerSettings(int groupID, ApiObjects.TimeShiftedTv.TimeShiftedTvPartnerSettings settings)
        {
            bool isUpdated = false;
            try
            {
                ODBCWrapper.StoredProcedure spUpdateTimeShiftedTvPartnerSettings = new ODBCWrapper.StoredProcedure("UpdateTimeShiftedTvPartnerSettings");
                spUpdateTimeShiftedTvPartnerSettings.SetConnectionKey("MAIN_CONNECTION_STRING");
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@GroupID", groupID);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowCatchUp", settings.IsCatchUpEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowCdvr", settings.IsCdvrEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowStartOver", settings.IsStartOverEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowTrickPlay", settings.IsTrickPlayEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@CatchUpBuffer", settings.CatchUpBufferLength);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@TrickPlayBuffer", settings.TrickPlayBufferLength);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@RecordingScheduleWindowBuffer", settings.RecordingScheduleWindow);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@EnableRecordingScheduleWindow", settings.IsRecordingScheduleWindowEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@PaddingBeforeProgramStarts", settings.PaddingBeforeProgramStarts);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@PaddingAfterProgramEnds", settings.PaddingAfterProgramEnds);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowProtection", settings.IsProtectionEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@ProtectionPeriod", settings.ProtectionPeriod);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@ProtectionQuotaPercentage", settings.ProtectionQuotaPercentage);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@RecordingLifetimePeriod", settings.RecordingLifetimePeriod);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@CleanupNoticePeriod", settings.CleanupNoticePeriod);
                if (!settings.IsSeriesRecordingEnabled.HasValue) // Default = enabled
                {
                    settings.IsSeriesRecordingEnabled = true;
                }
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowSeriesRecording", settings.IsSeriesRecordingEnabled);

                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowRecordingPlaybackNonEntitled", settings.IsRecordingPlaybackNonEntitledChannelEnabled);
                spUpdateTimeShiftedTvPartnerSettings.AddParameter("@AllowRecordingPlaybackNonExisting", settings.IsRecordingPlaybackNonExistingChannelEnabled);
       
                isUpdated = spUpdateTimeShiftedTvPartnerSettings.ExecuteReturnValue<bool>();
            }

            catch (Exception ex)
            {
                log.Error("Failed updating TimeShiftedTvPartnerSettings when running the stored procedure: UpdateTimeShiftedTvPartnerSettings", ex);
            }

            return isUpdated;
        }

        public static List<CDNAdapter> GetCDNAdapters(int groupID)
        {
            List<CDNAdapter> res = new List<CDNAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDNAdapters");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        CDNAdapter adapter = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            adapter = new CDNAdapter()
                            {
                                ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id"),
                                Name = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                                BaseUrl = ODBCWrapper.Utils.GetSafeStr(dr, "base_url"),
                                AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url"),
                                SystemName = ODBCWrapper.Utils.GetSafeStr(dr, "alias"),
                                IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active") == 0 ? false : true,
                                SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret"),

                            };
                            res.Add(adapter);
                        }

                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            Dictionary<int, List<CDNAdapterSettings>> adapterToDynamicDataMap = new Dictionary<int, List<CDNAdapterSettings>>();
                            foreach (DataRow dr in ds.Tables[1].Rows)
                            {
                                string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                                string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                                int adapterId = ODBCWrapper.Utils.GetIntSafeVal(dr, "adapter_id");
                                if (!adapterToDynamicDataMap.ContainsKey(adapterId))
                                {
                                    adapterToDynamicDataMap.Add(adapterId, new List<CDNAdapterSettings>());
                                }
                                adapterToDynamicDataMap[adapterId].Add(new CDNAdapterSettings(key, value));
                            }

                            foreach (var adapterRes in res)
                            {
                                if (adapterToDynamicDataMap.ContainsKey(adapterRes.ID))
                                {
                                    adapterRes.Settings = adapterToDynamicDataMap[adapterRes.ID];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                res = new List<CDNAdapter>();
            }
            return res;
        }

        public static CDNAdapter GetCDNAdapter(int adapterId, bool shouldGetOnlyActive = true)
        {
            CDNAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDNAdapter");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@id", adapterId);
                if (!shouldGetOnlyActive)
                {
                    sp.AddParameter("@shouldGetOnlyActive", 0);
                }

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDNAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return adapterResponse;
        }

        private static CDNAdapter CreateCDNAdapter(DataSet ds)
        {
            CDNAdapter adapterResponse = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                adapterResponse = new CDNAdapter();
                adapterResponse.BaseUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "base_url");
                adapterResponse.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "adapter_url");
                adapterResponse.SystemName = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "alias");
                adapterResponse.ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                int is_Active = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_active");
                adapterResponse.IsActive = is_Active == 1 ? true : false;
                adapterResponse.Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "name");
                adapterResponse.SharedSecret = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "shared_secret");

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                        string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                        if (adapterResponse.Settings == null)
                        {
                            adapterResponse.Settings = new List<CDNAdapterSettings>();
                        }
                        adapterResponse.Settings.Add(new CDNAdapterSettings(key, value));
                    }
                }
            }

            return adapterResponse;
        }

        public static bool DeleteCDNAdapter(int groupID, int adapterId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_CDNAdapter");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@ID", adapterId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch
            {
                return false;
            }
        }

        public static CDNAdapter GetCDNAdapterByAlias(int groupID, string alias)
        {
            CDNAdapter adapterResponse = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDNAdpterByAlias");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@alias", alias);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDNAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return adapterResponse;
        }

        public static CDNAdapter InsertCDNAdapter(int groupID, CDNAdapter adapter)
        {
            CDNAdapter adapterResponse = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_CDNAdapter");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@name", adapter.Name);
                sp.AddParameter("@is_active", adapter.IsActive);
                sp.AddParameter("@adapter_url", adapter.AdapterUrl);
                sp.AddParameter("@base_url", adapter.BaseUrl);
                sp.AddParameter("@alias", adapter.SystemName);
                sp.AddParameter("@shared_secret", adapter.SharedSecret);

                DataTable dt = CreateDataTableFromCdnDynamicData(adapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDNAdapter(ds);
            }

            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        private static DataTable CreateDataTableFromCdnDynamicData(List<CDNAdapterSettings> dynamicData)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            resultTable.Columns.Add("idkey", typeof(string));
            resultTable.Columns.Add("value", typeof(string));

            foreach (CDNAdapterSettings item in dynamicData)
            {
                DataRow row = resultTable.NewRow();
                row["idkey"] = item.key;
                row["value"] = item.value;
                resultTable.Rows.Add(row);
            }

            return resultTable;
        }

        public static CDNAdapter SetCDNAdapterSharedSecret(int groupID, int adapterId, string sharedSecret)
        {
            CDNAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_CDNAdapterSharedSecret");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@id", adapterId);
                sp.AddParameter("@sharedSecret", sharedSecret);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDNAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        public static CDNAdapter SetCDNAdapter(int groupID, int adapterID, CDNAdapter adapter)
        {
            CDNAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_CDNAdapter");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@ID", adapterID);
                sp.AddParameter("@name", adapter.Name);
                sp.AddParameter("@alias", adapter.SystemName);
                sp.AddParameter("@shared_secret", adapter.SharedSecret);
                sp.AddParameter("@adapter_url", adapter.AdapterUrl);
                sp.AddParameter("@base_url", adapter.BaseUrl);
                sp.AddParameter("@isActive", adapter.IsActive);
                DataTable dt = CreateDataTableFromCdnDynamicData(adapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);
                if (adapter.Settings != null && adapter.Settings.Count > 0)
                {
                    sp.AddParameter("@keysValueListExists", 1);
                }

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDNAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        public static CDNPartnerSettings GetCdnSettings(int groupId)
        {
            CDNPartnerSettings response = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCdnSettings");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);

                DataSet ds = sp.ExecuteDataSet();

                response = CreateCDNPartnerSettings(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return response;
        }

        private static CDNPartnerSettings CreateCDNPartnerSettings(DataSet ds)
        {
            CDNPartnerSettings response = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                response = new CDNPartnerSettings();
                response.DefaultAdapter = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "adapter_id");
                response.DefaultRecordingAdapter = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "recording_adapter_id");
            }

            return response;
        }

        public static CDNPartnerSettings UpdateCdnSettings(int groupId, int? defaultVodAdapterId, int? defaultRecordingAdapterId)
        {
            CDNPartnerSettings response = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateCdnSettings");
                sp.SetConnectionKey("CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@AdapterId", defaultVodAdapterId);
                sp.AddParameter("@RecordingAdapterId", defaultRecordingAdapterId);

                DataSet ds = sp.ExecuteDataSet();

                response = CreateCDNPartnerSettings(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return response;
        }

        public static int GetCdnRegularGroupId(int parentGroupId)
        {
            int regularGroupId = 0;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format("SELECT top(1) max(GROUP_ID) group_id FROM dbo.media_files with (nolock) WHERE STATUS = 1 and IS_ACTIVE = 1 and GROUP_ID IN (SELECT cast(iSNULL(id,0) as bigint) as ID FROM dbo.F_Get_GroupsTree({0}))", parentGroupId);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("GetCdnRegularGroupIdQuery", true) != null)
                {
                    DataTable dt = selectQuery.Table("GetCdnRegularGroupIdQuery");
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        regularGroupId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "group_id", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                return regularGroupId;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetCdnRegularGroupId", ex);
            }

            return regularGroupId;        
        }

        public static List<int> GetEpgChannelIdsWithNoCatchUp(int groupID)
        {
            List<int> epgIds = null;
            DataTable dt = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetEpgChannelIdsWithNoCatchUp");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);

                dt = sp.Execute();

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    epgIds = new List<int>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        epgIds.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "id"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed getting GetEpgChannelIds", ex);
            }

            return epgIds;
        }

        public static DataTable GetLinearMediaInfoByEpgChannelIdAndFileType(int groupId, string epgChannelId, string fileType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetLinearMediaInfoByEpgChannelIdAndFileType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@epgChannelId", epgChannelId);
            sp.AddParameter("@fileType", fileType);
            DataTable dt = sp.Execute();

            return dt;
        }

        public static DataTable GetDeviceFamilies()
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDeviceFamilies = new ODBCWrapper.StoredProcedure("GetDeviceFamilies");
            spGetDeviceFamilies.SetConnectionKey("MAIN_CONNECTION_STRING");
            dt = spGetDeviceFamilies.Execute();

            return dt;

        }

        public static DataTable GetDeviceBrands()
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDeviceBrands = new ODBCWrapper.StoredProcedure("GetDeviceBrands");
            spGetDeviceBrands.SetConnectionKey("MAIN_CONNECTION_STRING");
            dt = spGetDeviceBrands.Execute();

            return dt;

        }

        public static DataTable GetCountries(List<int> countryIds)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCountries");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ind", countryIds.Count > 0 ? 1 : 0);
            sp.AddIDListParameter("@countryIds", countryIds, "ID");
            dt = sp.Execute();

            return dt;
        }

        public static List<MediaFile> GetMediaFiles(long mediaId)
        {
            List<MediaFile> files = null;
            DataTable dt = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMediaFiles");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@mediaId", mediaId);
            dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                files = new List<MediaFile>();
                MediaFile file;
                StreamerType streamerType;
                foreach (DataRow dr in dt.Rows)
                {
                    file = new MediaFile()
                    {
                        Duration = ODBCWrapper.Utils.GetLongSafeVal(dr, "duration"),
                        ExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "co_guid"),
                        Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id"),
                        Type = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION"), // TODO: get the type
                        IsTrailer = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_TRAILER") == 1 ? true : false,
                        CdnId = ODBCWrapper.Utils.GetIntSafeVal(dr, "STREAMING_SUPLIER_ID"),
                        MediaId = mediaId,
                    };
                    
                    if (Enum.TryParse(ODBCWrapper.Utils.GetSafeStr(dr, "streamer_type"), out streamerType))
                    {
                        file.StreamerType = streamerType;
                    }

                    files.Add(file);
                }
            }
            return files;
        }
    }
}
