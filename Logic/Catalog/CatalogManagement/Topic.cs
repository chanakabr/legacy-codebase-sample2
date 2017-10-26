using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class Topic
    {

        private const string MULTIPLE_VALUE = "multipleValue";
        private const string SEARCH_RELATED = "searchRelated";

        public long Id { get; set; }
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public string SystemName { get; set; }
        public MetaType Type { get; set; }
        public HashSet<string> Features { get; set; }
        public bool? IsPredefined { get; set; }
        public bool? MultipleValue { get; set; }
        public bool SearchRelated { get; set; }
        public string HelpText { get; set; }
        public long ParentId { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }

        public Topic()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.NamesInOtherLanguages = new List<LanguageContainer>();
            this.SystemName = string.Empty;
            this.Type = MetaType.All;
            this.Features = null;
            this.IsPredefined = null;
            this.MultipleValue = null;
            this.SearchRelated = false;
            this.HelpText = string.Empty;
            this.ParentId = 0;            
            this.CreateDate = 0;
            this.UpdateDate = 0;            
        }

        public Topic(long id, string name, List<LanguageContainer> namesInOtherLanguages, string systemName, MetaType type, HashSet<string> features, bool isPredefined, string helpText, long parentId,
                        long createDate, long updateDate)
        {
            this.Id = id;
            this.Name = name;
            this.NamesInOtherLanguages = new List<LanguageContainer>(namesInOtherLanguages);
            this.SystemName = systemName;
            this.Type = type;
            this.Features = features != null ? new HashSet<string>(features, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (this.Features.Contains(MULTIPLE_VALUE))
            {
                this.MultipleValue = true;
                this.Features.Remove(MULTIPLE_VALUE);
            }
            else
            {
                this.MultipleValue = false;
            }

            if (this.Features.Contains(SEARCH_RELATED))
            {
                this.SearchRelated = true;
                this.Features.Remove(SEARCH_RELATED);
            }
            else
            {
                this.SearchRelated = false;
            }

            this.IsPredefined = isPredefined;            
            this.HelpText = helpText;
            this.ParentId = parentId;
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;            
        }

        public Topic(Topic topicToCopy)
        {
            this.Id = topicToCopy.Id;
            this.Name = string.Copy(topicToCopy.SystemName);
            this.NamesInOtherLanguages = new List<LanguageContainer>(topicToCopy.NamesInOtherLanguages);
            this.SystemName = string.Copy(topicToCopy.SystemName);
            this.Type = topicToCopy.Type;
            this.Features = new HashSet<string>(topicToCopy.Features, StringComparer.OrdinalIgnoreCase);
            this.IsPredefined = topicToCopy.IsPredefined;
            this.MultipleValue = topicToCopy.MultipleValue;
            this.SearchRelated = topicToCopy.SearchRelated;
            this.HelpText = topicToCopy.HelpText;
            this.ParentId = topicToCopy.ParentId;
            this.CreateDate = topicToCopy.CreateDate;
            this.UpdateDate = topicToCopy.UpdateDate;            
        }

        public string GetCommaSeparatedFeatures()
        {
            if (this.Features != null && this.Features.Count > 0)
            {
                return string.Join(",", this.Features);
            }
            else
            {
                return string.Empty;
            }
        }

        internal string GetFeaturesForDB(HashSet<string> existingFeatures = null)
        {            
            StringBuilder regularFeatures = new StringBuilder(GetCommaSeparatedFeatures());
            HashSet<string> featuresWithLogic = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // if its for new topic --> existing features == null we take the value according to MultipleValue property            
            if ((existingFeatures == null && MultipleValue.HasValue && MultipleValue.Value) ||
                // if its for updating topic we take multipleValue according to existing features
                (existingFeatures != null && existingFeatures.Contains(MULTIPLE_VALUE)))
            {
                featuresWithLogic.Add(MULTIPLE_VALUE);
            }

            if (SearchRelated)
            {
                featuresWithLogic.Add(SEARCH_RELATED);
            }

            if (regularFeatures.Length > 0 && featuresWithLogic.Count > 0)
            {
                return string.Format("{0},{1}", regularFeatures.ToString(), string.Join(",", featuresWithLogic));
            }
            else if (regularFeatures.Length > 0)
            {
                return regularFeatures.ToString();
            }
            else
            {
                return string.Join(",", featuresWithLogic);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Name: {0},", Name);
            sb.AppendFormat("NamesInOtherLanguages: {0}, ", NamesInOtherLanguages != null && NamesInOtherLanguages.Count > 0 ?
                                                            string.Join(",", NamesInOtherLanguages.Select(x => string.Format("languageCode: {0}, value: {1}", x.LanguageCode, x.Value)).ToList()) : string.Empty);
            sb.AppendFormat("SystemName: {0}, ", SystemName);
            sb.AppendFormat("Type: {0}, ", Type);
            sb.AppendFormat("Features: {0}, ", Features != null ? string.Join(",", Features) : string.Empty);
            sb.AppendFormat("IsPredefined: {0}, ", IsPredefined.HasValue ? IsPredefined.ToString() : string.Empty);
            sb.AppendFormat("MultipleValue: {0}", MultipleValue.HasValue ? MultipleValue.ToString() : string.Empty);
            sb.AppendFormat("SearchRelated: {0}", SearchRelated);
            sb.AppendFormat("HelpText: {0}, ", HelpText);
            sb.AppendFormat("ParentId: {0}, ", ParentId);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);            
            return sb.ToString();
        }
    }
}
