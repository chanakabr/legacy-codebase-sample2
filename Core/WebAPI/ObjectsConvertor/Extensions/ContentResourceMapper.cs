using System;
using WebAPI.Managers;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.ModelsValidators;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ContentResourceMapper
    {
        public static string GetUrl(this KalturaUploadedFileTokenResource model, int groupId)
        {
            var ut = UploadTokenManager.GetUploadToken(model.Token, groupId);
            return ut.FileUrl;
        }
        
        public static string GetUrl(this KalturaUrlResource model, int groupId)
        {
            return model.Url;
        }
        
        public static string GetUrl(this KalturaContentResource model,int groupId)
        {
            switch (model)
            {
                case KalturaUploadedFileTokenResource c: return c.GetUrl(groupId); break;
                case KalturaUrlResource c: return c.GetUrl(groupId); break;
                default: throw new NotImplementedException($"GetUrl for {model.objectType} is not implemented");
            }
        }
    }
}