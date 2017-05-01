using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Configuration;
using System.Web.Script.Serialization;
using KLogMonitor;
using System.Reflection;
using Newtonsoft.Json;

namespace Mailer
{
    public class MCMailer : IMailer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool SendMailTemplate(ApiObjects.MailRequestObj request)
        {
            try
            {
                log.DebugFormat("SendMailTemplate: SenderTo={0}, Subject={1}, TemplateName={2} ", request.m_sSenderTo, request.m_sSubject, request.m_sTemplateName);
                bool retVal = false;
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                MCObjByTemplate mcObj = request.parseRequestToTemplate();
                mcObj.key = Utils.GetTcmConfigValue("MCKey"); //default key
                if (!string.IsNullOrEmpty(request.m_emailKey))// specific key to group
                    mcObj.key = request.m_emailKey;

                //Patch until going live!!              
                if (mcObj.template_name.Contains("."))
                {
                    mcObj.template_name = mcObj.template_name.Remove(mcObj.template_name.IndexOf('.'));
                }
                string json = jsSer.Serialize(mcObj);
                log.DebugFormat("SendMailTemplate: mcObj={0} ", json);
                string sResp = Utils.SendXMLHttpReq(Utils.GetTcmConfigValue("MCURL"), json, null);
                log.DebugFormat("mailurl={0} response={1} ", Utils.GetTcmConfigValue("MCURL") + " key:" + Utils.GetTcmConfigValue("MCKey"), sResp);
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
                log.ErrorFormat("Error while sending mail template. request: {0}, ex: {1}", JsonConvert.SerializeObject(request), ex);
                return false;
            }
        }
    }
}
