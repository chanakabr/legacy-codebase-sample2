using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BaseUsageModuleDecorator : BaseUsageModule
    {
        protected BaseUsageModule originalBaseUsageModule;

        public BaseUsageModuleDecorator(BaseUsageModule originalBaseUsageModule)
        {
            this.originalBaseUsageModule = originalBaseUsageModule;
        }
    }
}
