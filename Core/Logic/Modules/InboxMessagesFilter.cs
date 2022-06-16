using ApiObjects;
using ApiObjects.Notification;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ApiLogic.Modules
{
    public class InboxMessagesFilter
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<eMessageCategory> MessageCategorys { get; set; }
        public long CreatedAtGreaterThanOrEqual { get; set; }
        public long CreatedAtLessThanOrEqual { get; set; }

        public InboxMessageResponse ApplyOnInboxMessageResponse(InboxMessageResponse inboxMessageResponse)
        {
            if (this == null)
                return inboxMessageResponse;

            // in case messageCategorys  is null, no filter. get all.
            if (this.MessageCategorys == null)
            {
                this.MessageCategorys = new List<eMessageCategory>();
                this.MessageCategorys = Enum.GetValues(typeof(eMessageCategory)).Cast<eMessageCategory>().ToList();
            }

            // filter inboxMessageResponse according to category and CreatedAtLessThanOrEqual & CreatedAtGreaterThanOrEqual
            if (this.CreatedAtLessThanOrEqual > 0 || this.CreatedAtGreaterThanOrEqual > 0)
            {
                if (this.CreatedAtLessThanOrEqual > 0)
                    inboxMessageResponse.InboxMessages = inboxMessageResponse.InboxMessages
                        .Where(x => x.CreatedAtSec <= this.CreatedAtLessThanOrEqual && this.MessageCategorys.Contains(x.Category)).ToList();

                if (this.CreatedAtGreaterThanOrEqual > 0)
                    inboxMessageResponse.InboxMessages = inboxMessageResponse.InboxMessages
                        .Where(x => x.CreatedAtSec >= this.CreatedAtGreaterThanOrEqual && this.MessageCategorys.Contains(x.Category)).ToList();
            }
            else
                inboxMessageResponse.InboxMessages = inboxMessageResponse.InboxMessages
                    .Where(x => this.MessageCategorys.Contains(x.Category)).ToList();

            inboxMessageResponse.TotalCount = inboxMessageResponse.InboxMessages.Count;

            // paging
            inboxMessageResponse.InboxMessages = inboxMessageResponse.InboxMessages
                .Skip(this.PageSize * this.PageIndex)
                .Take(this.PageSize).ToList();

            return inboxMessageResponse;
        }
    }
}
