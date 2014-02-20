using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using TVinciShared;
using XSLT_transform_handlar;
using System.Configuration;
using System.Xml.Serialization;


namespace RoviFeeder
{
    public class RoviTasker : ScheduledTasks.BaseTask
    {    
        private const int GROUP_ID = 163;

        private string          m_lastDate  = string.Empty;     // m_lastDate - last invoke date time
        private string          m_url       = string.Empty;     // m_url - get request url
        private string          m_uniqueKey = string.Empty;     // m_uniqueKey - unique connection key
        private int             m_groupID   = 0;
        private int             m_fromID    = 0;
        private EFeeder_Impl    m_IngestID  = 0;
        private RoviBaseFeeder        m_feeder    = null;


        private RoviTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 6)
            {
                m_groupID   = int.Parse(splited[0]);
                m_lastDate  = splited[1];
                m_url       = splited[2];
                m_uniqueKey = splited[3];
                m_fromID    = int.Parse(splited[4]);
                m_IngestID = (EFeeder_Impl)Enum.Parse(typeof(EFeeder_Impl), splited[5]);
            }

            switch (m_IngestID)
            {
                case EFeeder_Impl.E_CMT_FEEDER:
                {
                    m_feeder = new Rovi_CMTFeeder(m_url, m_fromID, GROUP_ID);
                    break;
                }
                case EFeeder_Impl.E_MEDIA_FEEDER:
                {
                    m_feeder = new Rovi_VODFeeder(m_url, m_fromID, GROUP_ID);
                    break;
                }
            }

            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1/content/movies";
            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1/marketing/campaigns/";
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new RoviTasker(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            if (m_feeder == null)
            {
                return false;
            }

            return m_feeder.Ingest();
        }


        
    }

}
