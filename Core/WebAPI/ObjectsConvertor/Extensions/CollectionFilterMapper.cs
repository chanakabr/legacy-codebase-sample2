using System;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class CollectionFilterMapper
    {
        public static string[] getCollectionIdIn(this KalturaCollectionFilter model)
        {
            if (string.IsNullOrEmpty(model.CollectionIdIn))
                return null;

            return model.CollectionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}