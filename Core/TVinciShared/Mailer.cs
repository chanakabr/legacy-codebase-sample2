using System;
using System.Web;
using System.Xml;
using System.Net.Mail;
using Phx.Lib.Log;
using System.Reflection;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for Mailer
    /// </summary>
    public class Mailer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public static string m_sBasePath = "";
        protected string m_sMailServer;
        protected string m_sMailUserName;
        protected string m_sMailPassword;
        protected string m_sMailFromName;
        protected string m_sMailFromAdd;
        protected Int32 m_nGroupID;
        protected System.Collections.ArrayList m_Attachments;

        protected int m_sMailSSL;
        protected int m_sMailPort;


        public Mailer(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
            Initialize();
        }
        /*
        public Mailer(string sFromName , string sFromAdd)
        {
            m_nGroupID = 0;
            m_Attachments = new System.Collections.ArrayList();
            SetMailServer("localhost", "", "", sFromName, "sender@tvinci.com");
        }
        */
        public void SetMailServer(string sMS, string sUN, string sPass, string sFromName, string sFromAdd)
        {
            SetMailServer(sMS, sUN, sPass, sFromName, sFromAdd, 0, 0);
        }

        public void SetMailServer(string sMS, string sUN, string sPass, string sFromName, string sFromAdd, int sMailSSL, int sMailPort)
        {
            m_sMailServer = sMS;
            m_sMailUserName = sUN;
            m_sMailPassword = sPass;
            m_sMailFromName = sFromName;
            m_sMailFromAdd = sFromAdd;
            m_sMailSSL = sMailSSL;
            m_sMailPort = sMailPort;
        }

        protected void Initialize()
        {
            m_Attachments = new System.Collections.ArrayList();
            if (m_nGroupID == 0)
                return;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    m_sMailServer = selectQuery.Table("query").DefaultView[0].Row["MAIL_SERVER"].ToString();
                    m_sMailUserName = selectQuery.Table("query").DefaultView[0].Row["MAIL_USER_NAME"].ToString();
                    m_sMailPassword = selectQuery.Table("query").DefaultView[0].Row["MAIL_PASSWORD"].ToString();
                    m_sMailFromName = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_NAME"].ToString();
                    m_sMailFromAdd = selectQuery.Table("query").DefaultView[0].Row["MAIL_RET_ADD"].ToString();

                    m_sMailSSL = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MAIL_SSL"].ToString());
                    m_sMailPort = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MAIL_PORT"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public void AddAttachment(string sAttPath)
        {
            string sAttach = "";
            if (HttpContext.Current != null)
                sAttach = HttpContext.Current.ServerMapPath("") + "\\";
            else
                //get from DB
                sAttach = m_sBasePath;

            sAttach += sAttPath;
            if (System.IO.File.Exists(sAttach) == true)
                m_Attachments.Add(sAttach);

        }

        public bool SendMail(string sTo, string sBCC, string sMail, string sSubject)
        {
            return SendMail(sTo, sBCC, sMail, sSubject, m_sMailFromName, m_sMailFromAdd);
        }

        public bool SendMail(string sTo, string sBCC, string sMail, string sSubject, string sMailFromName, string sMailFromAdd)
        {
            //string sFrom = "\"" + sMailFromName + "\"<" + sMailFromAdd + ">";
            //string sBody = sMail;

            System.Net.Mail.MailMessage MyMail = new System.Net.Mail.MailMessage();

            System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(sMail, null, "text/html");
            System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(sMail, @"<(.|\n)*?>", string.Empty), null, "text/plain");

            MyMail.AlternateViews.Add(plainView);
            MyMail.AlternateViews.Add(htmlView);


            MyMail.IsBodyHtml = true;
            System.Collections.IEnumerator iter = m_Attachments.GetEnumerator();
            while (iter.MoveNext())
            {
                //MyMail.Attachments.Add(new MailAttachment(iter.Current.ToString()));
                MyMail.Attachments.Add(new System.Net.Mail.Attachment(iter.Current.ToString()));
            }

            System.Net.Mail.MailAddress mFrom = new System.Net.Mail.MailAddress(sMailFromAdd, sMailFromName, System.Text.Encoding.UTF8);
            MyMail.From = mFrom;
            System.Net.Mail.MailAddressCollection mBcc = new System.Net.Mail.MailAddressCollection();
            System.Net.Mail.MailAddressCollection mTo = new System.Net.Mail.MailAddressCollection();
            {
                string[] sBccSplited = sBCC.Split(';');
                for (int i = 0; i < sBccSplited.Length; i++)
                    if (sBccSplited[i].Trim() != "")
                        MyMail.Bcc.Add(sBccSplited[i]);
            }
            //MyMail.Bcc = mBcc;
            {
                string[] sToSplited = sTo.Split(';');
                for (int i = 0; i < sToSplited.Length; i++)
                    if (sToSplited[i].Trim() != "")
                        MyMail.To.Add(sToSplited[i]);
            }
            //MyMail.Bcc.Add = mBcc;
            //MyMail.To = mTo;
            MyMail.Subject = sSubject;
            MyMail.SubjectEncoding = System.Text.Encoding.UTF8;
            //MyMail.Body = sBody;
            MyMail.BodyEncoding = System.Text.Encoding.UTF8;
            /*
            if (m_sMailUserName != "" && m_sMailPassword != "")
            {
                MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 1;
                MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusername"] = m_sMailUserName;
                MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"] = m_sMailPassword;
            }
            else
                MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 0;
            */
            System.Net.Mail.SmtpClient theClient = new System.Net.Mail.SmtpClient(m_sMailServer);

            if (m_sMailUserName != "" && m_sMailPassword != "")
            {
                theClient.Credentials = new System.Net.NetworkCredential(m_sMailUserName, m_sMailPassword);
                //MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 1;
                //MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusername"] = m_sMailUserName;
                //MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"] = m_sMailPassword;
            }

            if (m_sMailSSL == 1)
            {
                theClient.EnableSsl = true;
            }

            if (m_sMailPort > 0)
            {
                theClient.Port = m_sMailPort;
            }



            //else
            //MyMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 0;
            //SmtpMail.SmtpServer = m_sMailServer;
            try
            {
                //SmtpMail.Send(MyMail);
                theClient.Send(MyMail);
                log.Debug("Mail Sent - mail sent to: " + sTo);
                return true;
            }
            catch (Exception e)
            {
                log.Error("Mail Fail - mail sent to: " + sTo + " failed with exception: " + e.Message, e);
                return false;
            }
        }

        public static bool IsEmailAddressValid(string sEmail)
        {
            bool res = true;
            if (string.IsNullOrEmpty(sEmail))
                return false;
            try
            {
                MailAddress ma = new MailAddress(sEmail);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                res = false;
            }

            return res;
        }
    }

    public class OneLineFileReader
    {
        public OneLineFileReader()
        {
            m_bInit = false;
        }

        public OneLineFileReader(string sFileName)
        {
            Init(sFileName);
        }

        public bool Init(string sFileName)
        {
            try
            {
                m_Reader = System.IO.File.OpenText(sFileName);
                m_bInit = true;
                return true;
            }
            catch
            {
                m_bInit = false;
                return false;
            }
        }

        public bool Next(ref string sLine)
        {
            if (m_Reader == null)
                return false;
            string s = m_Reader.ReadLine();
            if (s == null)
            {
                sLine = "";
                return false;
            }
            sLine = s;
            return true;
        }

        public void Finish()
        {
            if (m_bInit == true)
            {
                m_Reader.Close();
                m_Reader = null;
            }
        }

        protected System.IO.StreamReader m_Reader;
        protected bool m_bInit;

    }

    public class MailTemplateEngine
    {
        public MailTemplateEngine()
        {
            m_bInit = false;
        }

        public bool Init(string sFileName)
        {
            try
            {
                System.IO.StreamReader r = System.IO.File.OpenText(sFileName);
                m_sTemplate = r.ReadToEnd();
                r.Close();
                r = null;
                m_bInit = true;
                return true;
            }
            catch
            {
                m_bInit = false;
                return false;
            }
        }

        public bool Replace(string sToReplace, string sWith)
        {
            string sOld = "<!--";
            sOld += sToReplace.ToUpper();
            sOld += "-->";
            m_sTemplate = m_sTemplate.Replace(sOld, sWith);
            return true;
        }

        public string GetAsString()
        {
            if (m_bInit == true)
                return m_sTemplate;
            else
                return "";
        }

        public XmlDocument GetAsXML()
        {
            XmlDocument oXmlDoc = new XmlDocument();
            oXmlDoc.LoadXml(m_sTemplate);
            return oXmlDoc;
        }

        protected string m_sTemplate;
        protected bool m_bInit;

    }
}