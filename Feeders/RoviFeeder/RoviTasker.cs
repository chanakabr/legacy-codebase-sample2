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
using KLogMonitor;
using System.Reflection;


namespace RoviFeeder
{
    public class RoviTasker : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int GROUP_ID = 163;

        private string m_lastDate = string.Empty;     // m_lastDate - last invoke date time
        private string m_url = string.Empty;     // m_url - get request url
        private string m_uniqueKey = string.Empty;     // m_uniqueKey - unique connection key
        private int m_groupID = 0;
        private int m_fromID = 0;
        private FeederImplEnum m_IngestID = 0;
        private RoviBaseFeeder m_feeder = null;


        private RoviTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 6)
            {
                m_groupID = int.Parse(splited[0]);
                m_lastDate = splited[1];
                m_url = splited[2];
                m_uniqueKey = splited[3];
                m_fromID = int.Parse(splited[4]);
                m_IngestID = (FeederImplEnum)Enum.Parse(typeof(FeederImplEnum), splited[5]);
            }

            switch (m_IngestID)
            {
                case FeederImplEnum.CMT:
                    {
                        m_feeder = new RoviCMTFeeder(m_url, m_fromID, m_groupID);
                        break;
                    }
                case FeederImplEnum.MOVIE:
                    {
                        m_feeder = new RoviMovieFeeder(m_url, m_fromID, m_groupID);
                        break;
                    }
                case FeederImplEnum.EPISODE:
                    {
                        m_feeder = new RoviEpisodeFeeder(m_url, m_fromID, m_groupID);
                        break;
                    }
                case FeederImplEnum.SERIES:
                    {
                        m_feeder = new RoviSeriesFeeder(m_url, m_fromID, m_groupID);
                        break;
                    }
            }

            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/movies";
            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/marketing/campaigns/";
            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/seasons"
            //m_url = "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/series"
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new RoviTasker(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            log.Debug("Start task - " + string.Format("Group:{0}, LastDate:{1}, Url:{2}, UniqueKey:{3}, m_IngestID:{4} ", m_groupID, m_lastDate, m_url, m_uniqueKey, m_IngestID.ToString()));

            DateTime d = DateTime.UtcNow;

            if (m_feeder == null)
            {
                return false;
            }

            bool ret = m_feeder.Ingest();

            // Update last time invoke parameter
            string parameters = string.Format("{0}||{1}||{2}||{3}||{4}||{5}", m_groupID, d.ToString("yyyy-MM-ddTHH:mm:ss"),
                                                                              m_url, m_uniqueKey, m_fromID, m_IngestID.ToString());

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", parameters);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            log.Debug("Ending task - Parameters: " + parameters);

            return ret;
        }
    }

}
