using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BaseSubscriptionDecorator : BaseSubscription
    {
        protected BaseSubscription originalBaseSubscription;

        public BaseSubscriptionDecorator(BaseSubscription originalBaseSubscription)
        {
            this.originalBaseSubscription = originalBaseSubscription;
        }

        public abstract Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID);

        public abstract Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked);

        public abstract string GetSubscriptionsContainingMediaSTR(int nMediaID, int nFileTypeID, bool isShrinked);

        public abstract Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked, int index);

        public abstract IdsResponse GetSubscriptionIDsContainingMediaFile(int nMediaID, int nMediaFileID);

        public abstract Subscription[] GetSubscriptionsContainingMediaFile(int nMediaID, int nMediaFileID);
    }
}
