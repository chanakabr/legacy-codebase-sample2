using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MandrilObj
    {
        public string key { get; set; }
        public MCMessage message { get; set; }

        public MandrilObj()
        {
            this.message = new MCMessage();
        }
    }

    public class MCObjByTemplate : MandrilObj
    {
        public string template_name { get; set; }
        public List<MCTemplateContent> template_content { get; set; }

        public MCObjByTemplate()
            : base()
        {
            this.template_content = new List<MCTemplateContent>();
        }
    }

    public class MCMessage
    {
        public string text { get; set; }
        public string subject { get; set; }
        public string from_email { get; set; }
        public string from_name { get; set; }
        public List<MCTo> to { get; set; }
        public Dictionary<object, object> headers { get; set; }
        public bool track_opens { get; set; }
        public bool track_clicks { get; set; }
        public bool auto_text { get; set; }
        public bool url_strip_qs { get; set; }
        public bool preserve_recipients { get; set; }
        public string bcc_address { get; set; }
        public bool merge { get; set; }
        public List<MCGlobalMergeVars> global_merge_vars { get; set; }
        public List<MCPerRecipientMergeVars> merge_vars { get; set; }
        public List<string> tags { get; set; }
        public List<string> google_analytics_domain { get; set; }
        public List<string> google_analytics_campaign { get; set; }

        //metadata
        //recipient_metadata


        public List<MCAttachment> attachments { get; set; }

        public MCMessage()
        {
            this.attachments = new List<MCAttachment>();
            this.to = new List<MCTo>();
            this.global_merge_vars = new List<MCGlobalMergeVars>();
            this.merge_vars = new List<MCPerRecipientMergeVars>();
            this.tags = new List<string>();
            this.google_analytics_domain = new List<string>();
            this.google_analytics_campaign = new List<string>();
            this.headers = new Dictionary<object, object>();
        }
    }

    public class MCTo
    {
        public string email { get; set; }
        public string name { get; set; }
    }

    // global merge variables to use for all recipients. You can override these per recipient.
    public class MCGlobalMergeVars
    {
        public string name { get; set; }
        public string content { get; set; }
    }

    // per-recipient merge variables, which override global merge variables with the same name.
    public class MCPerRecipientMergeVars
    {
        public string rcpt { get; set; }
        public List<MCGlobalMergeVars> vars { get; set; }

        public MCPerRecipientMergeVars()
        {
            this.vars = new List<MCGlobalMergeVars>();
        }
    }

    public class MCAttachment
    {
        public string type { get; set; }
        public string name { get; set; }

        //the content of the attachment as a base64-encoded string
        public string content { get; set; }
    }

    public class MCTemplateContent
    {
        public string name { get; set; }
        public string content { get; set; }
    }
}
