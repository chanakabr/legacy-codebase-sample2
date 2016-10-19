using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using MCGroupRules;
using ApiObjects;
using Pricing;
using WS_Pricing;

namespace ConditionalAccess
{
    public class SocialInviteCampaignImpl : BaseCampaignActionImpl
    {

        public override CampaignActionInfo ActivateCampaignWithInfo(Campaign camp, CampaignActionInfo cai, int groupID)
        {
            CampaignActionInfo retVal = null;
            int numOfUses = 0;

            string sWSUserName = "";
            string sWSPass = "";
            using (mdoule m = new mdoule())
            {
                Utils.GetWSCredentials(groupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                PasswordGenerator p = new PasswordGenerator();
                p.Maximum = 16;
                p.Minimum = 12;
                p.RepeatCharacters = false;
                string sPass = p.Generate();
                if (!string.IsNullOrEmpty(sPass))
                {
                    sPass = string.Format("S_{0}", sPass);
                }

                int mediaID = cai.m_mediaID;

                if (camp.m_oCouponsGroup != null)
                {
                    long ownerSiteGuid = 0;
                    if (!string.IsNullOrEmpty(cai.m_socialInviteInfo.m_hashCode))
                    {
                        CouponDataResponse cd = m.GetCouponStatus(sWSUserName, sWSPass, cai.m_socialInviteInfo.m_hashCode);
                        if (cd != null && cd.Coupon != null)
                        {
                            ownerSiteGuid = cd.Coupon.m_ownerGUID;
                        }
                    }
                    else
                    {
                        ownerSiteGuid = cai.m_siteGuid;
                    }
                    bool validCampaign = IsCampaignValid(ownerSiteGuid, camp.m_ID, ref numOfUses);
                    if (!validCampaign)
                    {
                        int couponGroupID = int.Parse(camp.m_oCouponsGroup.m_sGroupCode);
                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery.SetConnectionKey("pricing_connection");
                        directQuery += string.Format("Insert into coupons(code, COUPON_GROUP_ID, GROUP_ID, Voucher, Voucher_Campaign_ID, owner_guid, rel_media) values('{0}',{1},{2}, {3}, {4}, {5}, {6});", sPass, couponGroupID, groupID, 2, camp.m_ID, cai.m_siteGuid, cai.m_mediaID);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                        if (cai != null)
                        {
                            cai.m_socialInviteInfo = new CampaignActionInfo.SocialInviteInfo();
                            cai.m_socialInviteInfo.m_hashCode = sPass;
                            InitializeCampaignUses(camp, cai.m_siteGuid, 1000);
                            retVal = cai;
                        }
                    }
                    else
                    {
                        if (validCampaign && cai != null)
                        {
                            if (string.IsNullOrEmpty(cai.m_socialInviteInfo.m_hashCode))
                            {
                                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery.SetConnectionKey("pricing_connection");
                                selectQuery += "select code from coupons where ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("owner_guid", "=", cai.m_siteGuid);
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("voucher_campaign_id", "=", camp.m_ID);
                                if (selectQuery.Execute("query", true) != null)
                                {
                                    int count = selectQuery.Table("query").DefaultView.Count;
                                    if (count > 0)
                                    {
                                        string sExistingPass = selectQuery.Table("query").DefaultView[0].Row["code"].ToString();
                                        cai.m_socialInviteInfo = new CampaignActionInfo.SocialInviteInfo();
                                        cai.m_socialInviteInfo.m_hashCode = sExistingPass;
                                        retVal = cai;
                                    }
                                }
                                selectQuery.Finish();
                                selectQuery = null;
                            }
                            else
                            {
                                retVal = cai;

                                if (cai.m_siteGuid > 0)
                                {
                                    BaseConditionalAccess caImpl = null;
                                    Utils.GetBaseConditionalAccessImpl(ref caImpl, groupID);
                                    if (caImpl != null)
                                    {
                                        PermittedSubscriptionContainer[] validSubs = caImpl.GetUserPermittedSubscriptions(cai.m_siteGuid.ToString());
                                        if (validSubs != null && validSubs.Length > 0)
                                        {
                                            PermittedSubscriptionContainer relSub = (from vs in validSubs
                                                                                     select vs).OrderByDescending(vs => vs.m_nSubscriptionPurchaseID).FirstOrDefault();
                                            bool campRes = caImpl.UpdateSubscriptionDate(cai.m_siteGuid.ToString(), relSub.m_sSubscriptionCode, relSub.m_nSubscriptionPurchaseID, 30, true);
                                            if (campRes)
                                            {
                                                cai.m_status = CampaignActionResult.OK;
                                            }
                                            else
                                            {
                                                cai.m_status = CampaignActionResult.ERROR;
                                            }

                                        }
                                        else
                                        {
                                            cai.m_status = CampaignActionResult.ERROR;
                                        }
                                        if (ownerSiteGuid > 0)
                                        {
                                            PermittedSubscriptionContainer[] validOwnerSubs = caImpl.GetUserPermittedSubscriptions(ownerSiteGuid.ToString());
                                            if (validOwnerSubs != null && validOwnerSubs.Length > 0)
                                            {
                                                PermittedSubscriptionContainer relOwnerSub = (from vs in validOwnerSubs
                                                                                              select vs).OrderByDescending(vs => vs.m_nSubscriptionPurchaseID).FirstOrDefault();
                                                bool campRes = caImpl.UpdateSubscriptionDate(ownerSiteGuid.ToString(), relOwnerSub.m_sSubscriptionCode, relOwnerSub.m_nSubscriptionPurchaseID, 30, true);


                                            }
                                            else
                                            {
                                                int subCode = camp.m_subscriptionCode;
                                                if (subCode > 0)
                                                {
                                                    caImpl.CC_ChargeUserForBundle(ownerSiteGuid.ToString(), 0.0, string.Empty, subCode.ToString(), string.Empty, "1.1.1.1", string.Empty, "EUR", string.Empty, string.Empty, true, string.Empty, string.Empty, eBundleType.SUBSCRIPTION);
                                                }
                                            }
                                        }
                                        if (cai.m_status == CampaignActionResult.OK)
                                        {
                                            cai.m_siteGuid = (int)ownerSiteGuid;
                                            base.ActivateCampaign(camp, cai, groupID);
                                            SendMail(groupID, 26, 8, cai.m_siteGuid);
                                        }
                                    }
                                }
                            }
                        }


                    }
                }

            }
            return retVal;
        }

        private void InitializeCampaignUses(Campaign camp, int siteGuid, int maxUses)
        {
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                insertQuery = new ODBCWrapper.InsertQuery("campaigns_uses");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", camp.m_ID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", siteGuid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("num_of_uses", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("max_num_of_uses", "=", maxUses);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", camp.m_startDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", DateTime.MaxValue);
                insertQuery.Execute();
            }
            finally
            {
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }
        }

        private void SendMail(int groupID, int ruleID, int ruleTypeID, int siteGuid)
        {
            MCSocialInviteTriggered mcImp = new MCSocialInviteTriggered(groupID, ruleID, ruleTypeID, 0, 0, siteGuid);
            mcImp.InitMCObj();
            mcImp.Send();
        }
    }
}
