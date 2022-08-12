using System;
using System.Text;

namespace WebAPI.Models.General
{
    public interface IKalturaSerializable
    {
        [Obsolete]
        string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false);
        void AppendAsJson(StringBuilder stringBuilder, Version currentVersion, bool omitObsolete, bool responseProfile = false);
        [Obsolete]
        string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false);
        void AppendAsXml(StringBuilder stringBuilder, Version currentVersion, bool omitObsolete, bool responseProfile = false);
    }
}
