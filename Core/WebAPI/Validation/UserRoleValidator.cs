using System;
using System.Linq;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public class UserRoleValidator : IUserRoleValidator
    {
        private static readonly Lazy<IUserRoleValidator> Lazy = new Lazy<IUserRoleValidator>(
            () => new UserRoleValidator(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IUserRoleValidator Instance => Lazy.Value;

        public void Validate(KalturaUserRole role)
        {
            switch (role.Profile)
            {
                case KalturaUserRoleProfile.SYSTEM:
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "profile", "KalturaUserRoleProfile.SYSTEM");
                case KalturaUserRoleProfile.USER:
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "profile", "KalturaUserRoleProfile.USER");
                case KalturaUserRoleProfile.PERMISSION_EMBEDDED:
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "profile", "KalturaUserRoleProfile.PERMISSION_EMBEDDED");
            }
        }
    }
}