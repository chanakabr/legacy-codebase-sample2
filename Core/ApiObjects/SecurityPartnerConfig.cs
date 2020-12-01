using System;

namespace ApiObjects
{
    public class SecurityPartnerConfig
    {
        public DataEncryption Encryption { get; set; }
    }

    public class DataEncryption
    {
        public Encryption Username { get; set; }
    }

    public class Encryption
    {
        public EncryptionType EncryptionType { get; set; }
        public DateTime ApplyAfter { get; set; }

        public bool IsApplicable() => DateTime.UtcNow > ApplyAfter;
    }

    public enum EncryptionType
    {
        aes256 = 1
    }
}
