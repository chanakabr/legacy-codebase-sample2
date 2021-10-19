using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.Catalog.SearchPriority;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public static class SearchPriorityProfileProcessor
    {
        public static void ProcessSearchPriorityResponseProfile(KalturaBaseResponseProfile responseProfile, IEnumerable<KalturaAssetPriority> priorityAssets)
        {
            if (responseProfile == null)
            {
                return;
            }
            
            var profile = responseProfile as KalturaDetachedResponseProfile;
            var profiles = profile?.RelatedProfiles;
            if (profiles == null)
            {
                return;
            }

            if (profiles.Count == 0)
            {
                return;
            }

            var priorityGroupProfile = profiles.FirstOrDefault(x => x.Filter is KalturaPriorityGroupFilter);
            if (priorityGroupProfile == null)
            {
                return;
            }

            foreach (var priorityAsset in priorityAssets)
            {
                var response = GenerateResponse(priorityAsset);
                if (priorityAsset.Asset.relatedObjects == null)
                {
                    priorityAsset.Asset.relatedObjects = new SerializableDictionary<string, IKalturaListResponse>();
                }

                priorityAsset.Asset.relatedObjects.Add(priorityGroupProfile.Name, response);
            }
        }

        private static KalturaPriorityGroupListResponse GenerateResponse(KalturaAssetPriority priorityAsset)
        {
            return new KalturaPriorityGroupListResponse
            {
                Values = new List<KalturaIntegerValue>
                {
                    new KalturaIntegerValue
                    {
                        value = priorityAsset.PriorityGroupId ?? 0
                    }
                }
            };
        }
    }
}
