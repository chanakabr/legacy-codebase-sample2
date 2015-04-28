using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebAPI.Managers.Models
{
    public class KS
    {
        private bool _isValid;

        public enum eUserType { USER = 0, ADMIN = 2 }
        public bool isValid
        {
            get { return _isValid; }
        }

        public KS(string adminSecret, string groupID, string userID, int expiration, eUserType userType)
        {
            string ksFormat = "_u={0}&_e={1}&_t={2}&Privileges={3}";

            string ks = string.Format(ksFormat, userID, expiration, (int)userType, "all:*");
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);
            byte[] randomBytes = createRandomByteArray(16);
            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = hashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            // byte[] encryptedFields = Utils.EncryptionUtils.Encrypt(adminSecret, input);
            //String prefix = "v2|" + groupID + "|";
        }

        public static KS CreateKSFromEncoded(string base64Value)
        {
            //TODO: decode, decrypt, populate
            return null;
        }

        private byte[] hashSHA1(byte[] payload)
        {
            byte[] result;

            SHA1 sha = new SHA1CryptoServiceProvider();
            result = sha.ComputeHash(payload);

            return result;
        }

        private byte[] createRandomByteArray(int size)
        {
            byte[] b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }
    }
}