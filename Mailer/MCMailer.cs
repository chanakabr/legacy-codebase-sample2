using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Configuration;
using System.Web.Script.Serialization;

namespace Mailer
{
    public class MCMailer : IMailer
    {
        public bool SendMailTemplate(ApiObjects.MailRequestObj request)
        {
            try
            {
                Logger.Logger.Log("Mail Responses", "Start Send " + request.m_sSenderTo, "Mailer");
                bool retVal = false;
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                MCObjByTemplate mcObj = request.parseRequestToTemplate();
                mcObj.key = Utils.GetTcmConfigValue("MCKey"); //default key
                if (!string.IsNullOrEmpty(request.m_emailKey))// specific key to group
                    mcObj.key = request.m_emailKey;

                //Patch until going live!!
                Logger.Logger.Log("Mail Responses", mcObj.template_name, "Mailer");
                if (mcObj.template_name.Contains("."))
                {
                    mcObj.template_name = mcObj.template_name.Remove(mcObj.template_name.IndexOf('.'));
                    Logger.Logger.Log("Mail Responses", mcObj.template_name, "Mailer");
                }
                Logger.Logger.Log("Mail Responses", mcObj.template_name, "Mailer");
                string json = jsSer.Serialize(mcObj);
                string sResp = Utils.SendXMLHttpReq(Utils.GetTcmConfigValue("MCURL"), json, null);
                Logger.Logger.Log("Mail Responses", "Start Send to url" + Utils.GetTcmConfigValue("MCURL") + " key:" +Utils.GetTcmConfigValue("MCKey") , "Mailer");
                Logger.Logger.Log("Mail Responses", sResp, "Mailer");
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
                Logger.Logger.Log("Mail Responses", "Exception " + request.m_sSenderTo + " : " + ex.Message, "Mailer");
                return false;
            }
        }

    }
}
