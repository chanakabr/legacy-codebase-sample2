using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.MediaIndexingObjects;
using QueueWrapper;
using Tvinci.Core.DAL;

namespace EpgFeeder
{
    public class EpgFeederObj : ScheduledTasks.BaseTask
    {

        #region member
        const char spliter = '|';
        string sGroupID;
        string sEPGChannel;
        string sPathType;
        string sPath;
        Dictionary<string, string> sExtraParamter = new Dictionary<string, string>();
        EPGAbstract oEPGFeed;
        #endregion

        public EpgFeederObj(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParamter();
            oEPGFeed = new EPGAbstract();

        }
        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new EpgFeederObj(nTaskID, nIntervalInSec, sParameters);
        }

        protected override bool DoTheTaskInner()
        {
            try
            {
                switch (sEPGChannel)
                {
                    case "EPGxmlTv":
                        oEPGFeed.Implementer = new EPGGeneral(sGroupID, sPathType, sPath, sExtraParamter);  
                        break;
                    case "EPG_MediaCorp":
                        oEPGFeed.Implementer = new EPGMediaCorp(sGroupID, sPathType, sPath, sExtraParamter);
                        break;
                    case "EPG_Yes":
                        oEPGFeed.Implementer = new EPGYes(sGroupID, sPathType, sPath, sExtraParamter);
                        break;
                    case "EPG_KabelKiosk":
                        oEPGFeed.Implementer = new EPGEutelsat(sGroupID, sPathType, sPath, sExtraParamter);
                        break;

                }

                if (oEPGFeed != null)
                {
                    Dictionary<DateTime, List<int>> datesWithChannelIds = oEPGFeed.SaveChannel();

                    if (!string.IsNullOrEmpty(sGroupID))
                    {
                        DataTable dataParentGroupId = EpgDal.GetParentGroupIdByGroupId(int.Parse(sGroupID));
                        if (dataParentGroupId != null)
                        {
                            foreach (DataRow row in dataParentGroupId.Rows)
                            {
                                int nGroupIdFromDB = ODBCWrapper.Utils.GetIntSafeVal(row, "PARENT_GROUP_ID");
                                oEPGFeed.Implementer.m_ParentGroupId = EPGLogic.GetParentGroupId(nGroupIdFromDB, sGroupID).ToString();
                            }
                        }
                    }

                    #region Write EPG to Queue
                    //rabbit_queue_batch
                    string sParentGroupId = oEPGFeed.Implementer.m_ParentGroupId;
                    if (!string.IsNullOrEmpty(sParentGroupId) && datesWithChannelIds != null && datesWithChannelIds.Count > 0)
                    {
                        string sRouteKey = string.Format(@"{0}\{1}", sParentGroupId, eObjectType.EPG.ToString());

                        foreach (DateTime epgChangedDate in datesWithChannelIds.Keys)
                        {
                            IndexingData data = new IndexingData(datesWithChannelIds[epgChangedDate].Distinct().ToList<int>(), int.Parse(sParentGroupId), eObjectType.EPG, eAction.Update, TVinciShared.DateUtils.DateTimeToUnixTimestamp(epgChangedDate));
                            BaseQueue queue = new CatalogQueue();
                            bool bIsUpdateIndexSucceeded = queue.Enqueue(data, sRouteKey);
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("group:{0}, ex:{1}", sGroupID, ex.Message), "EpgFeeder");
            }
            return true;
        }

        protected void InitParamter()
        {
            try
            {
                string[] item = m_sParameters.Split(spliter);
                sGroupID = item[0];
                sEPGChannel = item[1];
                sPathType = item[2];
                sPath = item[3];

                for (int i = 4; i < item.Length; i++)
                {
                    string[] strspliter = { ";#" };
                    string[] extraitem = item[i].Split(strspliter, StringSplitOptions.RemoveEmptyEntries);
                    if (extraitem.Length > 1)
                    {
                        sExtraParamter.Add(extraitem[0], extraitem[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}
