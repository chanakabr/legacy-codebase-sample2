using ApiObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    class EpgChannelUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string EPG = "epg";
        public static readonly int DAYS = 30;


        #region Data Members

        private int groupId;
        private ElasticSearch.Common.ESSerializerV1 esSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public EpgChannelUpdaterV2(int groupId)
        {
            this.groupId = groupId;
            esSerializer = new ElasticSearch.Common.ESSerializerV1();
            esApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        #endregion

        #region Interface methods

        public bool Start()
        {
            bool result = false;
            log.Debug("Info - Start EPG update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - EPG Id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId)))
            {
                log.Error("Error - " + string.Format("Index of type EPG for group {0} does not exist", groupId));
                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:                    
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    result = UpdateEpgChannel(IDs);
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        

        #endregion

        private bool UpdateEpg(List<int> epgIds)
        {
            bool result = false;

            try
            {

                EpgUpdaterV1 epgUpdater = new EpgUpdaterV1(this.groupId);
                epgUpdater.IDs = epgIds;
                epgUpdater.Action = eAction.Update;

                result = epgUpdater.Start();               
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return result;
        }
        
        private bool UpdateEpgChannel(List<int> epgChannelIDs)
        {
            bool result = false;

            try
            {
                // get all languages per group
                Group group = GroupsCache.Instance().GetGroup(this.groupId);

                if (group == null)
                {
                    log.ErrorFormat("Couldn't get group {0}", this.groupId);
                    return false;
                }
                if (epgChannelIDs == null || epgChannelIDs.Count == 0)
                {
                    log.ErrorFormat("No epgChannelIDs sent for group {0}", this.groupId);
                    return false;
                }

                // get all epg programs related to epg channel      
                int days = TCMClient.Settings.Instance.GetValue<int>("Channel_StartDate_Days");
                if (days == 0)
                    days = DAYS;
                
                DateTime fromUTCDay = DateTime.UtcNow.AddDays(-days);                 
                 DateTime toUTCDay = new DateTime(2100,12,01);

                 List<int> epgIds = Tvinci.Core.DAL.EpgDal.GetEpgProgramsByChannelIds(this.groupId, epgChannelIDs, fromUTCDay, toUTCDay);

                result = UpdateEpg(epgIds);
                
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return result;
        }
    }
}
