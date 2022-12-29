using System;
using System.Collections.Generic;
using WebAPI.Models.Notification;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SeriesReminderFilterMapper
    {
        public static List<string> GetSeriesIdIn(this KalturaSeriesReminderFilter model)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(model.SeriesIdIn))
            {
                string[] stringValues = model.SeriesIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    list.Add(stringValue);
                }
            }

            return list;
        }
    }
}
