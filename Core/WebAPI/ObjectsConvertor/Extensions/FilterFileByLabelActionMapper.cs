using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByLabelActionMapper
    {
        public static List<string> GetLabels(this KalturaFilterFileByLabelAction model)
        {
            return model.GetItemsIn<List<string>, string>(model.LabelIn, "labelIn", true);
        }
    }
}