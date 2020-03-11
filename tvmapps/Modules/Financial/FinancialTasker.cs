using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public class FinancialTasker : ScheduledTasks.BaseTask 
    {
        
        
        private Int32 m_nGroupID;
        private Int32 m_nMonth;
        private Int32 m_nYear;
        private Int32 m_nNum;
        private Int32 m_nBillingTransactionID;
        private DateTime m_dStartDate;
        private DateTime m_dEndDate;

      

        public FinancialTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {

            m_nGroupID = 0;
            
            m_nMonth = 0;
            m_nYear = 0;
            m_nNum = 0;
            m_nBillingTransactionID = 0;
            m_dStartDate =  DateTime.MaxValue;
            m_dEndDate = DateTime.MinValue;

            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
           

            if (splited.Length > 0)
            {
                m_nGroupID = int.Parse(splited[0].ToString());

                if (m_nGroupID == 125 || m_nGroupID == 147) //MC
                {              
                    m_nBillingTransactionID = int.Parse(splited[1].ToString()); //Last Billing Transaction that was calculat                  
                    if (splited.Length == 4) //incluse startDate and endDate
                    {
                        m_dStartDate = DateTime.ParseExact(splited[2].ToString(), "yyyyMMdd", null);
                        m_dEndDate = DateTime.ParseExact(splited[3].ToString(), "yyyyMMdd", null);
                    }
                }
                else if (m_nGroupID == 109) //Filmo 
                {
                    if (splited.Length > 1)
                    {
                        m_nMonth = int.Parse(splited[1].ToString());
                        m_nYear = int.Parse(splited[2].ToString());
                        m_nNum = int.Parse(splited[3].ToString());
                    }
                }
            }
            else
            {
                m_nGroupID = int.Parse(engrameters);
            }
        }


        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new FinancialTasker(nTaskID, nIntervalInSec, engrameters);
        }


        protected override bool DoTheTaskInner()
        {
            switch (m_nGroupID)
            {
                case 125:
                case 147:
                     bool bNew = false;
                    if (m_dStartDate == DateTime.MaxValue && m_dEndDate == DateTime.MinValue)
                    {
                        bNew = true;
                        m_dStartDate = Utils.GetDateForBillingTransaction(m_nGroupID, m_nBillingTransactionID);
                        m_dEndDate = DateTime.Now;
                    }
                    TvinciFinancialCalculatorBase calculatorMC = new MCFinancialCalculator(m_nGroupID, m_nTaskID, m_dStartDate, m_dEndDate, bNew); 
                    calculatorMC.Calculate();       
                    break;
                case 109:
                    if (m_nMonth == 0)
                    {
                        int month = -1;
                        DateTime dt = DateTime.Now.AddMonths(month);

                        m_dStartDate = new DateTime(dt.Year, dt.Month, 1);
                        m_dEndDate = m_dStartDate.AddMonths(1);
                        TvinciFinancialCalculatorBase calculatorFl = new TvinciFinancialCalculator(m_nGroupID, m_dStartDate, m_dEndDate);
                        calculatorFl.Calculate();
                    }
                    // run from m_nMonth/m_nYear -  m_nNum months
                    else
                    {
                        m_dStartDate = new DateTime(m_nYear, m_nMonth, 1);
                        for (int i = 0; i < m_nNum; i++)
                        {
                            m_dStartDate = m_dStartDate.AddMonths(i);
                            m_dEndDate = m_dStartDate.AddMonths(1);

                            TvinciFinancialCalculatorBase calculator = new TvinciFinancialCalculator(m_nGroupID, m_dStartDate, m_dEndDate);
                            calculator.Calculate();
                        }
                    }
                    break;
                default:
                    break;
            }

            return true;
        }             

    }
}
