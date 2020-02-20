using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class CommercePartnerConfig
    {
        public Dictionary<eTransactionType, int> BookmarkEventThresholds { get; set; }

        public bool SetUnchangedProperties(CommercePartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.BookmarkEventThresholds != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.BookmarkEventThresholds = oldConfig.BookmarkEventThresholds;
            }

            return needToUpdate;
        }
    }
}