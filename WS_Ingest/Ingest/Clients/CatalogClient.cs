using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;
using Ingest.Clients.ClientManager;

namespace Ingest.Clients
{
    public class CatalogClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string Signature { get; set; }
        public string SignString { get; set; }
        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }

        public int CacheDuration { get; set; }

        protected Ingest.Catalog.IserviceClient CatalogClientModule
        {
            get
            {
                return (Module as Ingest.Catalog.IserviceClient);
            }
        }

        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }
    }
}