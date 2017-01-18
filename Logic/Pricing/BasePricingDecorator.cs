using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
     * 1. Decorator for BasePricing.
     * 2. Google "Decorator Design Pattern" before you apply any changes.
     */ 
    public abstract class BasePricingDecorator : BasePricing
    {
        protected BasePricing originalBasePricing;

        public BasePricingDecorator(BasePricing originalBasePricing)
        {
            this.originalBasePricing = originalBasePricing;
        }
    }
}
