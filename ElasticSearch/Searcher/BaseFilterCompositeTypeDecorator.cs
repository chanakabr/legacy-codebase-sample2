using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public abstract class BaseFilterCompositeTypeDecorator : BaseFilterCompositeType
    {
        protected BaseFilterCompositeType originalFilterCompositeType;

        public BaseFilterCompositeType OriginalFilterCompositeType 
        {
            get
            {
                return originalFilterCompositeType;
            }
            set
            {
                originalFilterCompositeType = value;
            }
        }

        public BaseFilterCompositeTypeDecorator(BaseFilterCompositeType originalFilterCompositeType)
        {
            this.originalFilterCompositeType = originalFilterCompositeType;
        }

        public override bool IsEmpty()
        {
            return OriginalFilterCompositeType.IsEmpty();
        }

    }
}
