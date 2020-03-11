using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public class ProcessorContract : BaseContract
    {
 
        public double m_nValidForMonthlyTransaction;
        public double m_nValidForMonthlyTransactionLower;

        public Int32 m_nPaymentMethodEntityID;

        public ProcessorContract()
            : base()
        {  
            m_nValidForMonthlyTransaction = 0;
            m_nValidForMonthlyTransactionLower = 0;
            m_nPaymentMethodEntityID = 0;
        }

        public void Initialize(Int32 nGID, Int32 nCID, Int32 nFEID, string sName, string sDescription, 
            Int32 nCurID, Int32 nRID, DateTime dStart, DateTime dEnd, 
            double nFixAmount, double nPercentageAmount, double nMinAmount, double nMaxAmount,
            RelatedTo eRT, Int32 nCOL, Int32 nLevel, CalculatedOn eCO,  
            double nValidForMonthlyTransaction, double nValidForMonthlyTransactionLower)
        {
            base.Initialize(nGID, nCID, nFEID, sName, sDescription,
            nCurID, nRID, dStart, dEnd,
            nFixAmount, nPercentageAmount, nMinAmount, nMaxAmount,
            eRT, nCOL, nLevel, eCO);

            m_nValidForMonthlyTransaction = nValidForMonthlyTransaction;
            m_nValidForMonthlyTransactionLower = nValidForMonthlyTransactionLower;
        }

        public void Initialize(Int32 nGID, Int32 nCID, Int32 nFEID, string sName, string sDescription,
            Int32 nCurID, Int32 nRID, DateTime dStart, DateTime dEnd,
            double nFixAmount, double nPercentageAmount, double nMinAmount, double nMaxAmount,
            RelatedTo eRT, Int32 nCOL, Int32 nLevel, CalculatedOn eCO,
            double nValidForMonthlyTransaction, double nValidForMonthlyTransactionLower, Int32 nContractRangeId, Int32 nPaymentMethodEntityID)
        {
            base.Initialize(nGID, nCID, nFEID, sName, sDescription,
            nCurID, nRID, dStart, dEnd,
            nFixAmount, nPercentageAmount, nMinAmount, nMaxAmount,
            eRT, nCOL, nLevel, eCO, nContractRangeId);

            m_nValidForMonthlyTransaction = nValidForMonthlyTransaction;
            m_nValidForMonthlyTransactionLower = nValidForMonthlyTransactionLower;

            m_nPaymentMethodEntityID = nPaymentMethodEntityID;
        }

        public override bool IsContracatValid(double nPrice, Int32 nCurrenyCD, string sCountryName, DateTime dDate, RelatedTo eRT)
        {
            bool res = base.IsContracatValid(nPrice, nCurrenyCD, sCountryName, dDate, eRT);

            if (m_nMinAmount == 0.0 && m_nMaxAmount == 0.0) // no range per transaction to check 
                return res;

            return (res && ((nPrice >= m_nMinAmount) && ( (nPrice < m_nMaxAmount) || (m_nMaxAmount == 0.0) )));
        }


        public override double Calculate(double dPrice)
        {

            double nCal = m_nFixAmount + (dPrice * (m_nPercentageAmount / 100));

            
            //if (nCal < m_nMinAmount)
            //{
            //    return m_nMinAmount;
            //}

            //if (m_nMaxAmount > 0 && nCal > m_nMaxAmount)
            //{
            //    return m_nMaxAmount;
            //}

            return nCal;
        }

        
    }

}
