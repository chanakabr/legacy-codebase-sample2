using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.Models.Social
{
    public enum KalturaSocialStatus
    {
        error,
        ok,
        user_does_not_exist,
        no_user_social_settings_found,
        asset_already_liked,
        not_allowed,
        invalid_parameters,
        no_facebook_action,
        asset_already_rated,
        asset_dose_not_exists,
        invalid_platform_request,
        invalid_access_token
    }
}
