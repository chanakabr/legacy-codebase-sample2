using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace AdapterControllers
{
    public class Utils
    {
        public static string GetSignature(string SharedSecret, string Signature)
        {            
            return System.Convert.ToBase64String(EncryptUtils.AesEncrypt(SharedSecret, EncryptUtils.HashSHA1(Signature)));
        }
    }
}
