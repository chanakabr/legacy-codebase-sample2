using System;
using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class RecordingFilterMapper
    {
        public static List<KalturaRecordingStatus> ConvertStatusIn(this KalturaRecordingFilter filter)
        {
            List<KalturaRecordingStatus> recordingStatuses = null;
            if (!string.IsNullOrEmpty(filter.StatusIn))
            {
                string[] recordingStatusInrecordingStatuses = filter.StatusIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                recordingStatuses = new List<KalturaRecordingStatus>();
                foreach (string sRecordingStatus in recordingStatusInrecordingStatuses)
                {
                    KalturaRecordingStatus recordingStatus;
                    if (Enum.TryParse<KalturaRecordingStatus>(sRecordingStatus.ToUpper(), out recordingStatus))
                    {
                        recordingStatuses.Add(recordingStatus);
                    }
                }
            }

            return recordingStatuses;
        }

        public static HashSet<string> GetExternalRecordingIds(this KalturaRecordingFilter filter)
        {
            HashSet<string> list = new HashSet<string>();
            if (!string.IsNullOrEmpty(filter.ExternalRecordingIdIn))
            {
                string[] stringValues = filter.ExternalRecordingIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    if (!list.Contains(stringValue))
                    {
                        list.Add(stringValue);
                    }
                }
            }

            return list;
        }
    }
}
