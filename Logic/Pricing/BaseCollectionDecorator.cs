using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BaseCollectionDecorator : BaseCollection
    {
        protected BaseCollection originalBaseCollection;

        public BaseCollectionDecorator(BaseCollection originalBaseCollection)
        {
            this.originalBaseCollection = originalBaseCollection;
        }
    }
}
