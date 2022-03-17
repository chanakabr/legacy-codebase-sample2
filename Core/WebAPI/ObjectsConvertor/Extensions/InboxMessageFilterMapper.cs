using System;
using System.Collections.Generic;
using WebAPI.Models.Notification;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class InboxMessageFilterMapper
    {
        public static List<KalturaInboxMessageType> getTypeIn(this KalturaInboxMessageFilter model)
        {
            List<KalturaInboxMessageType> values = new List<KalturaInboxMessageType>();

            if (string.IsNullOrEmpty(model.TypeIn))
                return values;

            string[] stringValues = model.TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Type enumType = typeof(KalturaInboxMessageType);
            foreach (string value in stringValues)
            {
                KalturaInboxMessageType type = (KalturaInboxMessageType) Enum.Parse(enumType, value, true);
                values.Add(type);
            }

            return values;
        }
    }
}