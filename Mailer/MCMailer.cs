using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Configuration;
using System.Web.Script.Serialization;
using KLogMonitor;
using System.Reflection;

namespace Mailer
{
    public class MCMailer : IMailer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool SendMailTemplate(ApiObjects.MailRequestObj request)
        {
            try
            {
                log.Debug("Mail Responses - Start Send " + request.m_sSenderTo);
                bool retVal = false;
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                MCObjByTemplate mcObj = request.parseRequestToTemplate();
                mcObj.key = Utils.GetTcmConfigValue("MCKey"); //default key
                if (!string.IsNullOrEmpty(request.m_emailKey))// specific key to group
                    mcObj.key = request.m_emailKey;

                //Patch until going live!!
                log.Debug("Mail Responses - " + mcObj.template_name);
                if (mcObj.template_name.Contains("."))
                {
                    mcObj.template_name = mcObj.template_name.Remove(mcObj.template_name.IndexOf('.'));
                    log.Debug("Mail Responses - " + mcObj.template_name);
                }
                log.Debug("Mail Responses - " + mcObj.template_name);
                string json = jsSer.Serialize(mcObj);
                string sResp = Utils.SendXMLHttpReq(Utils.GetTcmConfigValue("MCURL"), json, null);
                log.Debug("Mail Responses - Start Send to url" + Utils.GetTcmConfigValue("MCURL") + " key:" + Utils.GetTcmConfigValue("MCKey"));
                log.Debug("Mail Responses - " + sResp);
                if (sResp.Contains("sent"))
                {
                    retVal = true;
                }
                else
                {
                    if (mcObj.message != null && !string.IsNullOrEmpty(mcObj.message.bcc_address))
                    {
                        if (mcObj.message.to != null && mcObj.message.to.Count > 0)
                        {
                            mcObj.message.to[0].email = mcObj.message.bcc_address;
                            json = jsSer.Serialize(mcObj);
                            sResp = Utils.SendXMLHttpReq(Utils.GetTcmConfigValue("MCURL"), json, null);
                            if (sResp.Contains("sent"))
                            {
                                retVal = true;
                            }
                        }
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("Mail Responses - Exception " + request.m_sSenderTo + " : " + ex.Message);
                return false;
            }
        }

    }
}
