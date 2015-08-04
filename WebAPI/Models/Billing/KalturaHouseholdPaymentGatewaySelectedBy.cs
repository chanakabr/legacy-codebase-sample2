using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// distinction payment gateway selected by account or household
    /// </summary>
    public enum KalturaHouseholdPaymentGatewaySelectedBy
    {
        account,        
        household
    }
}