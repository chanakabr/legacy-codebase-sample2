using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Models.Social;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SocialActionMapper
    {
        public static string ToString(this KalturaSocialAction model)
        {
            string res = $"actionType : {model.ActionType.ToString()}, Time :{model.Time}, AssetId : {model.AssetId}, AssetType : {model.AssetType}, Url : {model.Url}";
            return res;
        }

        public static string ToString(this KalturaSocialActionRate model)
        {
            string res = string.Format("{0}, Rate Value = {1} ", SocialActionMapper.ToString(model), model.Rate);
            return res;
        }
    }
}
