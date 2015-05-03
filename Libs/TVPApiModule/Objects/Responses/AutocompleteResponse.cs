using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class AutocompleteResponse
    {
        #region Properties

        [JsonProperty(PropertyName = "assets")]
        public List<SlimAssetInfo> Assets
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "total_items")]
        public int TotalItems
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status
        {
            get;
            set;
        }

        #endregion

        #region Ctors

        public AutocompleteResponse()
            : this(null)
        {

        }

        public AutocompleteResponse(UnifiedSearchResponse unifiedSearchResponse)
        {
            if (unifiedSearchResponse != null)
            {
                this.Status = unifiedSearchResponse.Status;
                this.TotalItems = unifiedSearchResponse.TotalItems;

                if (unifiedSearchResponse.Assets != null)
                {
                    this.Assets = unifiedSearchResponse.Assets.Select(asset =>
                            new SlimAssetInfo(asset.Id, asset.Type, asset.Name, asset.Description, asset.Images)).ToList();
                }
            }
        }

        #endregion
    }
}
