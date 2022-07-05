using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PurchaseSessionMapper
    {
        public static int getPreviewModuleId(this KalturaPurchaseSession model)
        {
            return model.PreviewModuleId.HasValue ? model.PreviewModuleId.Value : 0;
        }
    }
}