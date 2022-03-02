using System;

namespace WebAPI.Models.General
{
    public interface IKalturaSerializable
    {
        string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false);

        string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false);
    }
}
