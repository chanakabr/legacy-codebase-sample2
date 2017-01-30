using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public abstract class MailRequestObj
    {
        public string m_sTemplateName;
        public string m_sFirstName;
        public string m_sLastName;
        public string m_sSubject;
        public string m_sSenderName;
        public string m_sSenderFrom;
        public string m_sSenderTo;
        public string m_sBCCAddress;

        public string m_emailKey;
        
        
        
        public eMailTemplateType m_eMailType;

        public virtual MCObjByTemplate parseRequestToTemplate()
        {
            MCObjByTemplate retVal = new MCObjByTemplate();
            retVal.template_name = this.m_sTemplateName;
            
            retVal.message = new MCMessage();
            retVal.message.subject = this.m_sSubject;
            retVal.message.from_email = this.m_sSenderFrom;
            retVal.message.from_name = this.m_sSenderName;
            retVal.message.bcc_address = this.m_sBCCAddress;
            retVal.message.to = new List<MCTo>();
            string[] senderToArr = this.m_sSenderTo.Split(';');
            for (int i = 0; i < senderToArr.Length; i++)
            {
                MCTo mcTo = new MCTo();
                mcTo.email = senderToArr[i]; 
                mcTo.name = this.m_sSenderTo;
                retVal.message.to.Add(mcTo);
            }
            retVal.message.global_merge_vars = getRequestMergeObj();


            return retVal;
        }

        public abstract List<MCGlobalMergeVars> getRequestMergeObj();


    }
}
