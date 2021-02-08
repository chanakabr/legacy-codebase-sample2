using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Catalog
{
    public class CategoryItemDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ParentId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }
        public string Type { get; set; }
        public long? VersionId { get; set; }
        public bool HasDynamicData { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? VirtualAssetId { get; set; }
        public List<LanguageContainerDTO> NamesInOtherLanguages { get; set; }
        public List<UnifiedChannelInfoDTO> UnifiedChannels { get; set; }
    }
}
