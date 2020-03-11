using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public enum RelatedTo
    {
        PPV = 1,
        SUBSCRIPTION = 2,
        BOTH = 3,
        PPV_IN_SUBSCRIPTION = 4,
    }

    public enum CalculatedOn
    {
        CataloguePrice = 1,
        FinalPriceAfterDiscount = 2,
        FinalAfterTax = 3,
        FinalAfterProcessing = 4,
        FinalAfterTaxAndProcessing = 5,
        OnLevel = 6,
    }

    public enum ItemType
    {
        PPV = 1,
        SUBSCRIPTION = 2,
        COLLECTION = 3,
    }

    public enum RightHolderType
    {
        NONE = 1,
        RightHolder = 2,
        Account = 3
    }


    public enum StartCountSince
    {
        Default = 0,
        Contract = 1,
        Calculation = 2,
        Month = 3

    }

    public enum ValueRangeType
    {
        Default = 0,
        CountTransaction = 1,
        SumAmount = 2

    }
}
