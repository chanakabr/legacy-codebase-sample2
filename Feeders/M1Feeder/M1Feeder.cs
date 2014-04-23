using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScheduledTasks;
using M1BL;

namespace M1Feeder
{
    public class M1Feeder : BaseTask 
    {

        #region private Members
        private int m_nGroupID = 0;    
        #endregion


        #region Constructor
        public M1Feeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParams(sParameters);
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new M1Feeder(nTaskID, nIntervalInSec, sParameters);
        }
        #endregion

        #region private Methods
        private void InitParams(string sParameters)
        {
            Logger.Logger.Log("InitParams Begin", "sParameters=" + sParameters, "M1Feeder");                       

            string[] arrParams = sParameters.Split(new char[] { '|' });
            try
            {
                int.TryParse(arrParams[0], out m_nGroupID);   
               
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("InitParams Error", "GroupID=" + m_nGroupID.ToString() + ", Exception:" + ex.ToString(), "M1Feeder");
            }

            Logger.Logger.Log("InitParams Finish", "GroupID=" + m_nGroupID.ToString(), "M1Feeder");
        }



        #endregion

        #region protected Methods
        protected override bool DoTheTaskInner()
        {
            bool result = true;

            try
            {
                M1FilesManager m1FilesManager = new M1FilesManager(m_nGroupID);
                string sPPVFileName = m1FilesManager.ProcessCdrFile(M1ItemType.PPV);
                string sSubscriptionFileName = m1FilesManager.ProcessCdrFile(M1ItemType.Subscription); 
            }
            catch(Exception ex)
            {
                Logger.Logger.Log("DoTheTaskInner", "GroupID=" + m_nGroupID.ToString()+", Exception:"+ ex.ToString(), "M1Feeder");
                result = false;
            }
            return result;        
        } 
        #endregion
    }
}
