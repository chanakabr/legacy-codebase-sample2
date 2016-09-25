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

        public static ApiObjects.MetaType GetMetaTypeByDbName(string metaDbName)
        {
            if (metaDbName.EndsWith("DOUBLE_NAME"))
            {
                return ApiObjects.MetaType.Number;
            }

            if (metaDbName.EndsWith("BOOL_NAME"))
            {
                return ApiObjects.MetaType.Bool;
            }

            if (metaDbName.EndsWith("STR_NAME"))
            {
                return ApiObjects.MetaType.String;
            }

            return ApiObjects.MetaType.All;
        }
    }
}
