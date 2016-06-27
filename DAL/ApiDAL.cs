using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using ApiObjects;
using ApiObjects.MediaMarks;
using CouchbaseManager;
using Newtonsoft.Json;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace DAL
{
    public class ApiDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");

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

        public static DataTable Get_IPCountryCode(Int64 nIP)
        {
            ODBCWrapper.StoredProcedure spIPCountryCode = new ODBCWrapper.StoredProcedure("Get_IPCountryCode");
            spIPCountryCode.SetConnectionKey("MAIN_CONNECTION_STRING");
            spIPCountryCode.AddParameter("@IPVal", nIP);

            DataSet ds = spIPCountryCode.ExecuteDataSet();

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

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            var res = cbManager.View<MediaMarkLog>(new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
            {
                startKey = new object[] { nSiteGuid, 0 },
                endKey = new object[] { nSiteGuid, string.Empty},
                asJson = true,
                shouldLookupById = true
            });

            List<MediaMarkLog> sortedMediaMarksList = res.ToList().OrderByDescending(x => x.LastMark.CreatedAt).ToList();

            List<int> retList = new List<int>();

            if (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0)
            {
                List<int> mediaIdsList = sortedMediaMarksList.Select(x => x.LastMark.MediaID).ToList();
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
                        if (dictMediasMaxDuration.ContainsKey(mediaMarkLogObject.LastMark.MediaID))
                        {
                            double dMaxDuration = Math.Round((0.95 * dictMediasMaxDuration[mediaMarkLogObject.LastMark.MediaID]));
                            if (mediaMarkLogObject.LastMark.Location > 1 && mediaMarkLogObject.LastMark.Location <= dMaxDuration)
                            {
                                if (i >= nNumOfItems || nNumOfItems == 0)
                                {
                                    break;
                                }
                                retList.Add(mediaMarkLogObject.LastMark.MediaID);
                                i++;
                            }
                        }
                    }

                }
            }
            return retList;
        }

        public static bool CleanUserHistory(string siteGuid, List<int> lMediaIDs)
        {
            try
            {
                bool retVal = true;
                int nSiteGuid = 0;
                int.TryParse(siteGuid, out nSiteGuid);

                var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

                if (lMediaIDs.Count == 0)
                {
                    var view = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
                    {
                        startKey = new object[] { nSiteGuid, 0 },
                        endKey = new object[] { nSiteGuid, string.Empty },
                        shouldLookupById = true
                    };

                    var res = cbManager.View<string>(view);

                    // deserialize string to MediaMarkLog
                    List<MediaMarkLog> sortedMediaMarksList = res.Select(current => JsonConvert.DeserializeObject<MediaMarkLog>(current)).ToList();

                    if (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0)
                    {
                        lMediaIDs = sortedMediaMarksList.Select(x => x.LastMark.MediaID).ToList();
                    }
                }

                Random r = new Random();
                foreach (int nMediaID in lMediaIDs)
                {
                    string sDcoKey = UtilsDal.getUserMediaMarkDocKey(nSiteGuid, nMediaID);

                    // Irena - make sure doc type is right
                    retVal = cbManager.Remove(sDcoKey);
                    Thread.Sleep(r.Next(50));
                    if (!retVal)
                    {
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
        public static DataSet Get_RegionsByExternalRegions(int groupId, List<string> externalRegionsList)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ExternalRegions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@ExternalRegionIDs", externalRegionsList, "STR");
            sp.AddParameter("@GroupID", groupId);

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

            newId = storedProcedure.ExecuteReturnValue<int>();

            return newId;
        }

        public static string Get_ParentalPIN(int groupId, int domainId, string siteGuid, out eRuleLevel level, bool getUserDomain)
        {
            string pin = null;
            level = eRuleLevel.User;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_UserDomainPIN");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", 1);
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
                }
            }

            return pin;
        }

        public static int Set_ParentalPIN(int groupId, string siteGuid, int domainId, string pin)
        {
            int newId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Set_UserDomainPin");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@DomainID", domainId);
            storedProcedure.AddParameter("@SiteGuid", siteGuid);
            storedProcedure.AddParameter("@Pin", pin);
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.AddParameter("@RuleType", 1);

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

        public static string Get_Group_DefaultPIN(int groupId)
        {
            string pin = string.Empty;

            object value = ODBCWrapper.Utils.GetTableSingleVal("group_rule_settings", "DEFAULT_PARENTAL_PIN", "GROUP_ID", "=", groupId);

            if (value != null && value != DBNull.Value)
            {
                pin = Convert.ToString(value);
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
            storedProcedure.AddParameter("@RuleType", 2);
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
            storedProcedure.AddParameter("@RuleType", 2);

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

        public static long GetLinearMediaIdByEpgId(long epgId)
        {
            long mediaId = 0;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_LinearMediaIdByEpgId");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@epg_id", epgId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows != null && dataSet.Tables[0].Rows.Count > 0){
                mediaId = ODBCWrapper.Utils.GetLongSafeVal(dataSet.Tables[0].Rows[0]["ID"]);
            }
            
            return mediaId;
        }
    }
}
