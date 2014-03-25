using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder
{
    public class BuzzFeeder : ScheduledTasks.BaseTask
    {
        private int m_nGroupID;
        private List<BuzzActivity> m_lBuzzActivities;

        public BuzzFeeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            m_lBuzzActivities = new List<BuzzActivity>();
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new BuzzFeeder(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            return false;
        }

        protected bool InitParamter()
        {
            bool bRes = false;

            if (string.IsNullOrEmpty(m_sParameters))
            {
                Logger.Logger.Log("Error", "parameters passed to feeder are null or empty", "BuzzFeeder");
                return bRes;
            }
            DateTime dtDate;
            TimeSpan tsInterval;
            string sGroupID; 
            string sBuzzerType; // series/channels
            string actionType; //comments/likes/follows etc.
            string[] mediaTypes;
            string[] actions;

            string[] splitString = m_sParameters.Split('|');

            if (splitString.Length < 5)
            {
            }

            sGroupID = splitString[0];
            sBuzzerType = splitString[1];
            dtDate = DateTime.ParseExact(splitString[2], "yyyyMMddHHmmss", null);
            
            int nInterval;
            if(int.TryParse(splitString[3], out nInterval))
            {
                tsInterval = new TimeSpan(0, nInterval, 0);
            }
            else
            {
                Logger.Logger.Log("Error", string.Format("Could not parse time interval {0}", splitString[3]), "BuzzFeeder");
            }

            for (int i = 4; i < splitString.Length; i++)
            {
                BuzzActivity activity = GetBuzzActivityFromParam(splitString[i]);
            }



            return bRes;
        }

        private BuzzActivity GetBuzzActivityFromParam(string sParams)
        {
            BuzzActivity oRes = null;

            if (string.IsNullOrEmpty(sParams))
            {
                return oRes;
            }

            string[] splitParams = sParams.Split(';');

            if (splitParams.Length < 4)
            {
                return oRes;
            }

            eBuzzActivityTypes eBuzzActivityType = (eBuzzActivityTypes) Enum.Parse(typeof(eBuzzActivityTypes), splitParams[0]);

            if (!Enum.IsDefined(typeof(eBuzzActivityTypes), eBuzzActivityType))
            {
            }


            return oRes;
        }

    }
}
