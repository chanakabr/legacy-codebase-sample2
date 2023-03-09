using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WebAPI.Models.Catalog;
using WebAPI.Exceptions;

namespace WebAPI.ObjectsConvertor.Extensions
{
    internal static class ChannelFilterMapper
    {
        internal static List<int> GetIdIn(this KalturaChannelsFilter model)
        {
            List<int> list = null;

            if (!string.IsNullOrEmpty(model.IdIn))
            {
                list = new List<int>();
                string[] stringValues = model.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    if (int.TryParse(stringValue, out int value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaChannelsFilter.idIn");
                    }
                }
            }

            return list;
        }

        internal static List<long> GetAssetUserRuleIdIn(this KalturaChannelsFilter model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.AssetUserRuleIdIn, "KalturaChannelsFilter.assetUserRuleIdIn", true);
        }
    }
}
