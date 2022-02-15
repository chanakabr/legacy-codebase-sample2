using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SoapAdaptersCommon.GrpcAdapters
{
    public class SSLHelpers
    {
        #if NETFRAMEWORK
        public static X509Certificate2 NewX509Certificate2FromCrtAndKey(string certPath, string keyPath)
        {
            throw new NotImplementedException("You must run .Net core 3.1 or higher to use ssl with crt and pem.");
        }
        #endif
        #if !NETFRAMEWORK
        public static X509Certificate2 NewX509Certificate2FromCrtAndKey(string certPath, string keyPath)
        {
            using (var privateKey = RSA.Create())
            {
                var pemLines = File.ReadAllLines(keyPath);
                var base64PemDataLines = pemLines.Skip(1).Take(pemLines.Length - 2);
                var base64PemDataStr = string.Concat(base64PemDataLines);
                var binaryEncoding = Convert.FromBase64String(base64PemDataStr);
                privateKey.ImportPkcs8PrivateKey(binaryEncoding, out _);
                // do stuff with the key now
                
                using (var pubOnly = new X509Certificate2(certPath))
                using (var pubPrivateEphemeral = pubOnly.CopyWithPrivateKey(privateKey))
                {
                    // Export as PFX and re-import if you want "normal PFX private key lifetime"
                    // (this step is currently required for SslStream, but not for most other things
                    // using certificates)
                    return new X509Certificate2(pubPrivateEphemeral.Export(X509ContentType.Pfx));
                }
            }
        }
        #endif
    }
}