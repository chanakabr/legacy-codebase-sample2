using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using EpgBL;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public abstract class AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members
        
        protected int groupId;
        protected ElasticSearchApi api;
        protected BaseESSeralizer serializer;

        #endregion

        #region Properties

        public bool SwitchIndexAlias
        {
            get;
            set;
        }

        public bool DeleteOldIndices
        {
            get;
            set;
        }

        public DateTime? StartDate
        {
            get;
            set;
        }

        public DateTime? EndDate
        {
            get;
            set;
        }

        public string ElasticSearchUrl
        {
            get
            {
                if (api != null)
                {
                    return api.baseUrl;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (api != null)
                {
                    api.baseUrl = value;
                }
            }
        }

        #endregion
        
        #region Ctor

        public AbstractIndexBuilder(int groupID)
        {
            this.groupId = groupID;
            api = new ElasticSearchApi();
        }

        #endregion

        #region Abstract Methods

        public abstract bool BuildIndex();

        #endregion

        #region Protected Methods

        protected bool DualBuild(AbstractIndexBuilder firstBuilder, AbstractIndexBuilder secondBuilder)
        {
            bool success = false;

            // Copy definitions from current builder to the partial builders
            firstBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            secondBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            firstBuilder.DeleteOldIndices = this.DeleteOldIndices;
            secondBuilder.DeleteOldIndices = this.DeleteOldIndices;
            firstBuilder.StartDate = this.StartDate;
            secondBuilder.StartDate = this.StartDate;
            firstBuilder.EndDate = this.EndDate;
            secondBuilder.EndDate = this.EndDate;

            // Build the two indexes
            bool oldSuccess = false;
            bool newSuccess = false;

            try
            {
                oldSuccess = firstBuilder.BuildIndex();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ElasticSearchHandler - Dual build error in old builder. groupId = {0}, ex = {1}",
                    groupId, ex);
            }

            try
            {
                newSuccess = secondBuilder.BuildIndex();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ElasticSearchHandler - Dual build error in new builder. groupId = {0}, ex = {1}",
                    groupId, ex);
            }

            success = oldSuccess && newSuccess;

            return success;
        }

        protected Dictionary<ulong, Dictionary<string, EpgCB>> GetEpgPrograms(int groupId, DateTime? dateTime)
        {
            try
            {
                Dictionary<ulong, Dictionary<string, EpgCB>> epgs = new Dictionary<ulong, Dictionary<string, EpgCB>>();

                //Get All programs by group_id + date from CB
                TvinciEpgBL oEpgBL = new TvinciEpgBL(groupId);

                List<EpgCB> lEpgCB = oEpgBL.GetGroupEpgs(0, 0, dateTime, dateTime.Value.AddDays(1));

                if (lEpgCB != null && lEpgCB.Count > 0)
                {
                    epgs = BuildEpgsLanguageDictionary(lEpgCB);
                   
                }
                else
                {
                    log.DebugFormat("Got 0 or null EPG Programs. group = {0}, date = {1}", groupId, dateTime);
                }

                return epgs;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetEpgPrograms. group id = {2}, Date = {3}, Message = {0}, stack trace = {1}",
                    ex.Message, ex.StackTrace,
                    groupId, dateTime);
                throw ex;
            }
        }

        protected Dictionary<ulong, Dictionary<string, EpgCB>> BuildEpgsLanguageDictionary(List<EpgCB> epgs)
        {
            Dictionary<ulong, Dictionary<string, EpgCB>> epgDictionary = new Dictionary<ulong, Dictionary<string, EpgCB>>();

            foreach (var epg in epgs)
            {
                if (epg != null)
                {
                    if (!epgDictionary.ContainsKey(epg.EpgID))
                    {
                        epgDictionary.Add(epg.EpgID, new Dictionary<string, EpgCB>());
                    }
                    if (!epgDictionary[epg.EpgID].ContainsKey(epg.Language))
                    {
                        epgDictionary[epg.EpgID].Add(epg.Language, epg);
                    }
                }
                else
                {
                    log.ErrorFormat("Received null epg from TvinciEpgBL");
                }
            }

            return epgDictionary;
        }
        #endregion
    }
}
