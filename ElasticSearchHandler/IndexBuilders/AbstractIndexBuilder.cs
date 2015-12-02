using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public abstract class AbstractIndexBuilder
    {
        #region Data Members
        
        protected int groupId;
        protected ESSerializer serializer;
        protected ElasticSearchApi api;

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
        
        #endregion
        
        #region Ctor

        public AbstractIndexBuilder(int groupID)
        {
            this.groupId = groupID;
            api = new ElasticSearchApi();
            serializer = new ESSerializer();
        }

        #endregion

        #region Abstract Methods

        public abstract bool BuildIndex();

        #endregion

        protected static UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel, bool useMediaTypes)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

            if (useMediaTypes)
            {
                definitions.mediaTypes = new List<int>(channel.m_nMediaType);
            }

            if (channel.m_nMediaType != null)
            {
                if (channel.m_nMediaType.Contains(Channel.EPG_ASSET_TYPE))
                {
                    definitions.shouldSearchEpg = true;
                }

                if (channel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                {
                    definitions.shouldSearchMedia = true;
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDate = false;
            definitions.shouldUseFinalEndDate = false;

            BooleanPhraseNode filterTree = null;
            var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

            if (parseStatus.Code != (int)eResponseStatus.OK)
            {
                throw new KalturaException(parseStatus.Message, parseStatus.Code);
            }
            else
            {
                definitions.filterPhrase = filterTree;
            }

            return definitions;
        }

        protected static string GetPermittedWatchRules(int groupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(groupId, null);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }
    }
}
