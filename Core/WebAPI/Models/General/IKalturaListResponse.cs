using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Models.General
{
    public interface IKalturaListResponse
    {
        string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false);
        string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false);
    }
}
