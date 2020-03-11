using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public abstract class BaseEncrypter
    {
        public abstract string Encrypt(string sPass, string key);

        protected string GetRand64String()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }

        public abstract void GenerateEncryptPassword(string clearPassword, ref string EncryptPassword, ref string salt);
    }
}
