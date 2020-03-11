using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BasePrePaidModuleDecorator : BasePrePaidModule
    {
        protected BasePrePaidModule originalBasePrePaidModule;

        public BasePrePaidModuleDecorator(BasePrePaidModule originalBasePrePaidModule)
        {
            this.originalBasePrePaidModule = originalBasePrePaidModule;
        }
    }
}
