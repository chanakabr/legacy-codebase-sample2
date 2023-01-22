using System;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class RecordingValidator
    {
        public static void ValidateForAdd(this KalturaRecording model, int groupId)
        {
            switch (model)
            {
                case KalturaPaddedRecording c:
                    c.Validate(groupId);
                    break;
                case KalturaRecording c:
                    c.Validate(groupId);
                    break;
                default:
                    throw new NotImplementedException(
                        $"ValidateForAdd for {model.objectType} is not implemented"); //Throw if immediate recording is used
            }
        }


        public static void ValidateForUpdate(this KalturaRecording model, int groupId)
        {
            switch (model)
            {
                case KalturaPaddedRecording c:
                    c.Validate(groupId);
                    break;
                case KalturaImmediateRecording c:
                    c.Validate(groupId);
                    break;
                case KalturaRecording c:
                    c.Validate(groupId);
                    break;
                default:
                    throw new NotImplementedException(
                        $"ValidateForAdd for {model.objectType} is not implemented"); //Throw if immediate recording is used
            }
        }

        private static void Validate(this KalturaPaddedRecording model, int groupId)
        {
            model.ValidateConditions(groupId);
        }

        private static void Validate(this KalturaImmediateRecording model, int groupId)
        {
            model.ValidateConditions(groupId);
        }

        private static void Validate(this KalturaRecording model, int groupId)
        {
            model.ValidateConditions(groupId);
        }

        private static void ValidateConditions(this KalturaPaddedRecording model, int groupId)
        {
            //if padded recording not allowed, reject
            var accountSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (accountSettings?.PersonalizedRecordingEnable == null ||
                !accountSettings.PersonalizedRecordingEnable.Value)
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaPaddedRecording",
                    model.objectType);
            }
        }

        private static void ValidateConditions(this KalturaImmediateRecording model, int groupId)
        {
            //if padded recording not allowed, reject
            var accountSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (accountSettings?.PersonalizedRecordingEnable == null ||
                !accountSettings.PersonalizedRecordingEnable.Value)
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaImmediateRecording",
                    model.objectType);
            }
        }

        private static void ValidateConditions(this KalturaRecording model, int groupId)
        {
            //if padded recording allowed, reject
            var accountSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (accountSettings?.PersonalizedRecordingEnable != null &&
                accountSettings.PersonalizedRecordingEnable.Value)
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaRecording",
                    model.objectType);
            }
        }
    }
}