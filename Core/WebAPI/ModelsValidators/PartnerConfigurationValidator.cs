using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Partner;

namespace WebAPI.ModelsValidators
{
    public static class PartnerConfigurationValidator
    {
        public static void ValidateForUpdate(this KalturaPartnerConfiguration model)
        {
            switch (model)
            {
                case KalturaPaymentPartnerConfig c: c.ValidateForUpdate(); break;
                case KalturaCatalogPartnerConfig c: c.ValidateForUpdate(); break;
                case KalturaOpcPartnerConfiguration c: c.ValidateForUpdate(); break;
            }
        }

        private static void ValidateForUpdate(this KalturaCatalogPartnerConfig model)
        {
            if (model.CategoryManagement != null)
            {
                model.CategoryManagement.ValidateForUpdate();
            }
        }

        private static void ValidateForUpdate(this KalturaOpcPartnerConfiguration model)
        {
            if (model.ResetPassword == null || model.ResetPassword.Templates == null)
            {
                return;
            }

            //REQ-03.3
            if (model.ResetPassword.Templates.Count() == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaResetPasswordPartnerConfigTemplate");
            }

            //REQ-03.2.1 + REQ-03.2.2
            if (model.CheckDuplications(model.ResetPassword.Templates))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED,
                    "KalturaResetPasswordPartnerConfigTemplate.Id OR KalturaResetPasswordPartnerConfigTemplate.Label");
            }

            //REQ-03.2.3
            if (model.ResetPassword.Templates.Where(t => t.IsDefault).Count() != 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaResetPasswordPartnerConfigTemplate.IsDefault", "Defaults");
            }
        }

        private static bool CheckDuplications(this KalturaOpcPartnerConfiguration model, List<KalturaResetPasswordPartnerConfigTemplate> templates)
        {
            var gb = templates.GroupBy(x => x.Id)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            if (gb.Count > 0)
                return true;

            gb = templates.GroupBy(x => x.Label)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            return gb.Count > 0;
        }

        private static void ValidateForUpdate(this KalturaPaymentPartnerConfig model)
        {
            if (model.UnifiedBillingCycles != null && model.UnifiedBillingCycles.Count > 0)
            {
                foreach (var unifiedBillingCycle in model.UnifiedBillingCycles)
                {
                    unifiedBillingCycle.Validate();
                }
            }
        }
    }
}
