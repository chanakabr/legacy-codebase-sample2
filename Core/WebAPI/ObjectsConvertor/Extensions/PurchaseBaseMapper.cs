using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PurchaseBaseMapper
    {
        public static int getContentId(this KalturaPurchaseBase model)
        {
            return model.ContentId.HasValue ? model.ContentId.Value : 0;
        }
    }
}