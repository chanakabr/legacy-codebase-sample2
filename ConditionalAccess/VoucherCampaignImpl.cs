using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Web;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using Pricing;
using WS_Pricing;

namespace ConditionalAccess
{
    public class VoucherCampaignImpl : BaseCampaignActionImpl
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string m_mailServer;
        private string m_mailPass;
        private string m_mailUser;
        private string m_fromName;
        private string m_fromAdd;
        private string m_voucherMailSubject;

        public override bool ActivateCampaign(Campaign camp, CampaignActionInfo cai, int groupID)
        {
            bool retVal = false;
            int numOfUses = 0;
            if (IsCampaignValid(cai.m_siteGuid, camp.m_ID, ref numOfUses))
            {
                string sWSUserName = "";
                string sWSPass = "";
                mdoule m = new mdoule();
                Utils.GetWSCredentials(groupID, eWSModules.PRICING, ref sWSUserName, ref sWSPass);
                PasswordGenerator p = new PasswordGenerator();
                p.Maximum = 16;
                p.Minimum = 12;
                p.RepeatCharacters = false;
                string sPass = p.Generate();
                if (!string.IsNullOrEmpty(sPass))
                {
                    sPass = string.Format("V_{0}", sPass);
                }

                int mediaID = cai.m_mediaID;

                if (camp.m_oCouponsGroup != null)
                {
                    int couponGroupID = int.Parse(camp.m_oCouponsGroup.m_sGroupCode);
                    ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                    directQuery.SetConnectionKey("pricing_connection");
                    directQuery += string.Format("Insert into coupons(code, COUPON_GROUP_ID, GROUP_ID, Voucher, Voucher_Campaign_ID, owner_guid, rel_media) values('{0}',{1},{2}, {3}, {4}, {5}, {6});", sPass, couponGroupID, groupID, 1, camp.m_ID, cai.m_siteGuid, cai.m_mediaID);
                    directQuery.Execute();
                    directQuery.Finish();
                    directQuery = null;
                    retVal = true;
                    if (mediaID > 0)
                    {
                        string mailFrom = string.Empty;
                        string mailTemplate = string.Empty;
                        foreach (CampaignActionInfo.VoucherReceipentInfo vri in cai.m_voucherReceipents)
                        {
                            try
                            {
                                string voucherMailext = GetVoucherMailText(mediaID, ref mailFrom, ref mailTemplate, sPass, vri, cai, groupID);
                                SendMail(voucherMailext, vri.m_emailAdd, groupID);
                                log.Debug("Campaigns - Campaign ID " + camp.m_ID + " Voucher sent to email " + vri.m_emailAdd + " by user " + cai.m_siteGuid.ToString());
                            }
                            catch (Exception ex)
                            {
                                log.Error("Campaigns Error - Campaign Voucher not sent to email " + vri.m_emailAdd, ex);
                            }
                        }
                    }
                }
                if (retVal)
                {
                    base.ActivateCampaign(camp, cai, groupID);
                }
            }
            return retVal;
        }

        private string GetVoucherMailText(int mediaID, ref string mailFromAdd, ref string mailTemplate, string voucherCode, CampaignActionInfo.VoucherReceipentInfo vri, CampaignActionInfo cai, int groupID)
        {
            string retVal = string.Empty;
            string mediaName = string.Empty;
            string mediaType = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select * from groups_parameters where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    m_mailServer = selectQuery.Table("query").DefaultView[0].Row["MAIL_SERVER"].ToString();
                    m_mailUser = selectQuery.Table("query").DefaultView[0].Row["MAIL_USER_NAME"].ToString();
                    m_mailPass = selectQuery.Table("query").DefaultView[0].Row["MAIL_PASSWORD"].ToString();
                    mailTemplate = selectQuery.Table("query").DefaultView[0].Row["VOUCHER_EMAIL"].ToString();
                    m_fromAdd = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_ADD"].ToString();
                    m_voucherMailSubject = selectQuery.Table("query").DefaultView[0].Row["VOUCHER_SUBJECT"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("main_connection_string");
            selectQuery += "select mt.name as 'media_type', m.name from media_types mt, media m where mt.id = m.media_type_id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", mediaID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    mediaName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    mediaType = selectQuery.Table("query").DefaultView[0].Row["media_type"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            string sFirstName = string.Empty;

            m_fromName = cai.m_senderName;
            TVinciShared.Mailer t = new TVinciShared.Mailer(0);
            t.SetMailServer(m_mailServer, m_mailUser, m_mailPass, sFirstName, mailFromAdd);

            TVinciShared.MailTemplateEngine mt = new TVinciShared.MailTemplateEngine();
            string sFilePath = HttpContext.Current.Server.MapPath("");
            sFilePath += "/mailTemplates/" + mailTemplate;
            mt.Init(sFilePath);
            mt.Replace("NAME", vri.m_receipentName);
            mt.Replace("MEDIATYPE", mediaType);
            mt.Replace("MEDIANAME", mediaName);
            mt.Replace("VOUCHERCODE", voucherCode);
            mt.Replace("SENDERNAME", cai.m_senderName);
            mt.Replace("LINK", cai.m_mediaLink);
            string sMailData = mt.GetAsString();
            return sMailData;
        }

        protected void SendMail(string sText, string sEmail, Int32 nGroupID)
        {
            if (sText == "")
                return;
            string sMailData = sText;
            TVinciShared.Mailer t = new TVinciShared.Mailer(0);
            t.SetMailServer(m_mailServer, m_mailUser, m_mailPass, m_fromName, m_fromAdd);
            t.SendMail(sEmail, "", sMailData, m_voucherMailSubject);
            log.Debug("Voucher Email - Sent Voucher Mail to " + sEmail);
        }
    }
}