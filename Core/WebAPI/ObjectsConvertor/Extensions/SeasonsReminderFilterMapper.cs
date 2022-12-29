using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Models.Notification;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SeasonsReminderFilterMapper
    {
        public static List<long> GetSeasonNumberIn(this KalturaSeasonsReminderFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.SeasonNumberIn))
            {
                string[] stringValues = model.SeasonNumberIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    int value;
                    if (int.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSeasonsReminderFilter.seasonNumberIn");
                    }
                }
            }

            return list;
        }
    }
}
