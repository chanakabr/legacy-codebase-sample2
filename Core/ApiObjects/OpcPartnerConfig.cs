using System.Collections.Generic;

namespace ApiObjects
{
    public class OpcPartnerConfig
    {
        public ResetPasswordPartnerConfig ResetPassword { get; set; }
    }

    public class ResetPasswordPartnerConfig
    {
        public string TemplateListLabel { get; set; }
        public List<ResetPasswordPartnerConfigTemplate> Templates { get; set; }
    }

    public class ResetPasswordPartnerConfigTemplate
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool IsDefault { get; set; }
    }
}