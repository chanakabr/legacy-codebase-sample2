using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PurchaseWithPreviewModuleRequest : PurchaseMailRequest
    {
        public string m_sPreviewModuleEndDate;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            retVal.Add(nameMergeVar);
            MCGlobalMergeVars lastNameMergeVar = new MCGlobalMergeVars();
            lastNameMergeVar.name = "LASTNAME";
            lastNameMergeVar.content = this.m_sLastName;
            retVal.Add(lastNameMergeVar);
            MCGlobalMergeVars priceMergeVar = new MCGlobalMergeVars();
            priceMergeVar.name = "CATPRICE";
            priceMergeVar.content = this.m_sPrice;
            retVal.Add(priceMergeVar);
            MCGlobalMergeVars itemMergeVar = new MCGlobalMergeVars();
            itemMergeVar.name = "ITEMNAME";
            itemMergeVar.content = this.m_sItemName;
            retVal.Add(itemMergeVar);
            MCGlobalMergeVars recMergeVar = new MCGlobalMergeVars();
            recMergeVar.name = "RECNUMBER";
            recMergeVar.content = this.m_sTransactionNumber;
            retVal.Add(recMergeVar);
            MCGlobalMergeVars payMethodMergeVar = new MCGlobalMergeVars();
            payMethodMergeVar.name = "PAYMENTMETHOD";
            payMethodMergeVar.content = this.m_sPaymentMethod;
            retVal.Add(payMethodMergeVar);
            MCGlobalMergeVars dateMergeVar = new MCGlobalMergeVars();
            dateMergeVar.name = "DATEOFPURCHASE";
            dateMergeVar.content = this.m_sPurchaseDate;
            retVal.Add(dateMergeVar);
            MCGlobalMergeVars taxMergeVar = new MCGlobalMergeVars();
            taxMergeVar.name = "TAXVAL";
            taxMergeVar.content = this.m_sTaxVal;
            retVal.Add(taxMergeVar);
            MCGlobalMergeVars taxDiscMergeVar = new MCGlobalMergeVars();
            taxDiscMergeVar.name = "TAXPRICE";
            taxDiscMergeVar.content = this.m_sTaxSubtotal;
            retVal.Add(taxDiscMergeVar);
            MCGlobalMergeVars taxTotalMergeVar = new MCGlobalMergeVars();
            taxTotalMergeVar.name = "TAXDISCOUNT";
            taxTotalMergeVar.content = this.m_sTaxAmount;
            retVal.Add(taxTotalMergeVar);
            MCGlobalMergeVars invoiceNumMergeVar = new MCGlobalMergeVars();
            invoiceNumMergeVar.name = "INVOICENUM";
            invoiceNumMergeVar.content = this.m_sInvoiceNum;
            retVal.Add(invoiceNumMergeVar);
            MCGlobalMergeVars externalNumMergeVar = new MCGlobalMergeVars();
            externalNumMergeVar.name = "EXTERNALNUM";
            externalNumMergeVar.content = this.m_sExternalTransationNum;
            retVal.Add(externalNumMergeVar);
            MCGlobalMergeVars addressNumMergeVar = new MCGlobalMergeVars();
            addressNumMergeVar.name = "ADDRESS";
            addressNumMergeVar.content = this.m_sAddress;
            retVal.Add(addressNumMergeVar);
            MCGlobalMergeVars previewEndVar = new MCGlobalMergeVars();
            addressNumMergeVar.name = "PREVIEWEND";
            addressNumMergeVar.content = this.m_sPreviewModuleEndDate;
            retVal.Add(previewEndVar);

            return retVal;
        }
    }
}
