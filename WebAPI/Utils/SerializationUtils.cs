using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebAPI.Utils
{
    public class SerializationUtils
    {
        //TODO: Change. but keep hardcoded, as TCM config may damage this.
        private const string passPhrase = "OdedIsOded";

        public static string MaskSensitiveObject(string originalVal)
        {
            return EncryptionUtils.Encrypt(originalVal, passPhrase);
        }

        public static string UnmaskSensitiveObject(string maskedVal)
        {
            return EncryptionUtils.Decrypt(maskedVal, passPhrase);
        }
    }
}