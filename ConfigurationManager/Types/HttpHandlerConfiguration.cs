using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationManager.Types
{
    public class HttpHandlerConfiguration : ConfigurationValue
    {

        public NumericConfigurationValue MaxConnectionsPerServer;
        public BooleanConfigurationValue CheckCertificateRevocationList;
        public StringConfigurationValue SslProtocols;
        public StringConfigurationValue DecompressionMethods;

        public HttpHandlerConfiguration(string key) : base(key)
        {
            MaxConnectionsPerServer = new NumericConfigurationValue("max_connections_per_server", this)
            {
                DefaultValue = 5,
                ShouldAllowEmpty = true,
                Description = "The maximum number of concurrent connections (per server endpoint) allowed when making requests using HttpClient. Limit is per server endpoint"
            };

            CheckCertificateRevocationList = new BooleanConfigurationValue("check_certificate_revocation", this)
            {
                DefaultValue = false,
                ShouldAllowEmpty = true,
                Description = "Indicates whether the certificate is checked against the certificate authority revocation list"
            };

            SslProtocols = new StringConfigurationValue("ssl_protocols", this)
            {
                DefaultValue = "Tls,Tls11,Tls12",
                ShouldAllowEmpty = true,
                Description = "the TLS/SSL protocol used by the HttpClient objects managed by the HttpClientHandler object. Possible values Tls/Tls11/Tls12/Tls13/Ssl2/Ssl3/Default/None"
            };

            DecompressionMethods = new StringConfigurationValue("decompression_methods", this)
            {                
                DefaultValue = "Deflat,Gzip",
                ShouldAllowEmpty = true,
                Description = "Represents the file compression and decompression encoding format to be used to compress the data received in httpClient response. Possible values Brotli/Deflate/Gzip/None/All"
            };
        }

        internal override bool Validate()
        {
            bool isValid = base.Validate();
            if (isValid)
            {
                isValid = ValidateSslProtocols() && ValidateDecompressionMethods();
            }

            return isValid;
        }

        private bool ValidateSslProtocols()
        {
            bool isValid = true;
            List<string> sslProtocols = new List<string>();
            if (!string.IsNullOrEmpty(SslProtocols.Value))
            {
                string splitValue = SslProtocols.Value.Contains(",") ? "," : SslProtocols.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedSslProtocols = SslProtocols.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedSslProtocols != null && splitedSslProtocols.Length > 0)
                    {
                        if (splitedSslProtocols.Any(x => string.IsNullOrEmpty(x)))
                        {
                            isValid = false;
                        }
                        else
                        {
                            sslProtocols.AddRange(splitedSslProtocols);
                        }
                    }
                    else
                    {
                        isValid = false;
                    }
                }
                else
                {
                    sslProtocols.Add(SslProtocols.Value);
                }
            }

            if (sslProtocols.Count > 0)
            {
                System.Security.Authentication.SslProtocols sslProtocol;
                isValid = sslProtocols.Any(x => !Enum.TryParse<System.Security.Authentication.SslProtocols>(x, out sslProtocol));
            }

            return isValid;
        }

        private bool ValidateDecompressionMethods()
        {
            bool isValid = true;
            List<string> decompressionMethods = new List<string>();
            if (!string.IsNullOrEmpty(DecompressionMethods.Value))
            {
                string splitValue = DecompressionMethods.Value.Contains(",") ? "," : DecompressionMethods.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedDecompressionMethods = DecompressionMethods.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedDecompressionMethods != null && splitedDecompressionMethods.Length > 0)
                    {
                        if (splitedDecompressionMethods.Any(x => string.IsNullOrEmpty(x)))
                        {
                            isValid = false;
                        }
                        else
                        {
                            decompressionMethods.AddRange(splitedDecompressionMethods);
                        }
                    }
                    else
                    {
                        isValid = false;
                    }
                }
                else
                {
                    decompressionMethods.Add(DecompressionMethods.Value);
                }
            }

            if (decompressionMethods.Count > 0)
            {
                System.Net.DecompressionMethods decompressionMethod;
                isValid = decompressionMethods.Any(x => !Enum.TryParse<System.Net.DecompressionMethods>(x, out decompressionMethod));
            }

            return isValid;
        }

    }
}
