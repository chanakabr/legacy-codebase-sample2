using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class IPNOFilterCompositeType : BaseFilterCompositeTypeDecorator
    {
        protected List<string> jsonizedIPNOChannelsDefinitions;
        protected List<string> jsonizedAllChannelsOfAllIPNOsDefinitions;
        protected CutWith cutWithOriginalQuery;
        protected CutWith cutWithBetweenIPNOChannelsDefinitions;
        protected CutWith cutWithBetweenAllChannelsOfAllIPNOsDefinitions;
        protected CutWith cutWithSpecificIPNOAndAllIPNOs;
        protected bool isNotBeforeEachIPNOChannelDef;
        protected bool isNotBeforeEachDefOfAllIPNOsChannelDef;


        public IPNOFilterCompositeType(BaseFilterCompositeType filterCompositeType, 
            List<string> jsonizedIPNOChannelsDefinitions, List<string> jsonizedAllChannelsOfAllIPNOsDefinitions,
            CutWith cutWithOriginalQuery, CutWith cutWithBetweenIPNOChannelsDefinitions,
            CutWith cutWithBetweenAllChannelsOfAllIPNOsDefinitions, CutWith cutWithSpecificIPNOAndAllIPNOs,
            bool isNotBeforeEachIPNOChannelDef, bool isNotBeforeEachDefOfAllIPNOsChannelDef) :  
            base(filterCompositeType)
        {
            if (cutWithBetweenAllChannelsOfAllIPNOsDefinitions == CutWith.WCF_ONLY_DEFAULT_VALUE ||
                cutWithBetweenIPNOChannelsDefinitions == CutWith.WCF_ONLY_DEFAULT_VALUE ||
                cutWithOriginalQuery == CutWith.WCF_ONLY_DEFAULT_VALUE ||
                cutWithSpecificIPNOAndAllIPNOs == CutWith.WCF_ONLY_DEFAULT_VALUE)
            {
                throw new Exception("IPNOFilterCompositeType constructor. At least one of the given CutWith is neither AND nor OR");
            }

            this.jsonizedIPNOChannelsDefinitions = jsonizedIPNOChannelsDefinitions;
            this.jsonizedAllChannelsOfAllIPNOsDefinitions = jsonizedAllChannelsOfAllIPNOsDefinitions;
            this.cutWithOriginalQuery = cutWithOriginalQuery;
            this.cutWithBetweenIPNOChannelsDefinitions = cutWithBetweenIPNOChannelsDefinitions;
            this.cutWithBetweenAllChannelsOfAllIPNOsDefinitions = cutWithBetweenAllChannelsOfAllIPNOsDefinitions;
            this.cutWithSpecificIPNOAndAllIPNOs = cutWithSpecificIPNOAndAllIPNOs;
            this.isNotBeforeEachIPNOChannelDef = isNotBeforeEachIPNOChannelDef;
            this.isNotBeforeEachDefOfAllIPNOsChannelDef = isNotBeforeEachDefOfAllIPNOsChannelDef;

        }

        public override string ToString()
        {
            bool addExtraBraces = false;

            if (jsonizedIPNOChannelsDefinitions != null)
            {
                StringBuilder sb = new StringBuilder(String.Concat("\"", cutWithOriginalQuery.ToString().ToLower(), "\":[{"));
                sb.Append(String.Concat(this.OriginalFilterCompositeType.ToString(), "}"));
                sb.Append(String.Concat(",{\"", cutWithSpecificIPNOAndAllIPNOs.ToString().ToLower(), "\":["));
                if (jsonizedIPNOChannelsDefinitions.Count > 0)
                {
                    TryOptimizeStringLength(cutWithBetweenIPNOChannelsDefinitions, isNotBeforeEachIPNOChannelDef,
                        ref addExtraBraces, ref sb);

                    ConcatChannelsDefinitions(jsonizedIPNOChannelsDefinitions, addExtraBraces, ref sb);
                    sb.Append(String.Concat("]}", addExtraBraces ? "}," : ","));
                }

                TryOptimizeStringLength(cutWithBetweenAllChannelsOfAllIPNOsDefinitions, isNotBeforeEachDefOfAllIPNOsChannelDef,
                    ref addExtraBraces, ref sb);
                ConcatChannelsDefinitions(jsonizedAllChannelsOfAllIPNOsDefinitions, addExtraBraces, ref sb);
                sb.Append(String.Concat("]}", addExtraBraces ? "}" : string.Empty));
                sb.Append("]}]");

                return sb.ToString();
            }

            return this.OriginalFilterCompositeType.ToString();
        }

        /*
         * This method uses De Morgan laws in order to determine whether it's possible to optimize the json string length
         */ 
        private void TryOptimizeStringLength(CutWith betweenEachChannelDef, bool isNotBeforeEachChannelDef, ref bool addExtraBraces, ref StringBuilder sb)
        {
            CutWith res = CutWith.AND;
            if (isNotBeforeEachChannelDef)
            {
                // switch and with or and vice versa. 1 ^ 3 = 2, 2 ^ 3 = 1.
                res = (CutWith)((int)betweenEachChannelDef ^ 3);
                addExtraBraces = true;
            }
            else
            {
                res = betweenEachChannelDef;
                addExtraBraces = false;
            }

            if (addExtraBraces)
            {
                sb.Append("{\"not\":{");
            }
            else
            {
                sb.Append("{");
            }
            sb.Append(String.Concat("\"", res.ToString().ToLower(), "\":["));
            
        }

        private void ConcatChannelsDefinitions(List<string> jsonizedChannelsDefinitions, bool addExtraBraces, ref StringBuilder sb)
        {
            int length = jsonizedChannelsDefinitions.Count;
            for (int i = 0; i < length; i++)
            {
                sb.Append(String.Concat(i == 0 ? string.Empty : ",", jsonizedChannelsDefinitions[i]));
            }

            
        }
    }
}
