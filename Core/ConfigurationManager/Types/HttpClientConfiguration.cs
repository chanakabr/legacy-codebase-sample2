using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationManager.Types
{
    public class HttpClientConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue MaxConnectionsPerServer;
        public BooleanConfigurationValue CheckCertificateRevocationList;
        public StringConfigurationValue EnabledSslProtocols;
        public StringConfigurationValue EnabledDecompressionMethods;
        public NumericConfigurationValue TimeOutInMiliSeconds;

        public HttpClientConfiguration(string key) : base(key)
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

            EnabledSslProtocols = new StringConfigurationValue("enabled_ssl_protocols", this)
            {
                DefaultValue = "Tls,Tls11,Tls12",
                ShouldAllowEmpty = true,
                Description = "the TLS/SSL protocols to be enabled by the HttpClient. Possible values Tls/Tls11/Tls12/Tls13/Ssl2/Ssl3/Default/None"
            };

            EnabledDecompressionMethods = new StringConfigurationValue("enabled_decompression_methods", this)
            {                
                DefaultValue = "Deflate,Gzip",
                ShouldAllowEmpty = true,
                Description = "Represents the file compression and decompression encoding format to be enabled by HttpClient to compress the data received in the response. Possible values Brotli/Deflate/Gzip/None/All"
            };

            TimeOutInMiliSeconds = new NumericConfigurationValue("timeout", this)
            {
                DefaultValue = 100000,
                ShouldAllowEmpty = true,
                Description = "The timeout in milliseconds for the HttpClient"
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

        public bool ValidateSslProtocols()
        {
            bool isValid = true;
            List<string> sslProtocols = new List<string>();
            if (!string.IsNullOrEmpty(EnabledSslProtocols.Value))
            {
                string splitValue = EnabledSslProtocols.Value.Contains(",") ? "," : EnabledSslProtocols.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedSslProtocols = EnabledSslProtocols.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
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
                    sslProtocols.Add(EnabledSslProtocols.Value);
                }
            }

            if (sslProtocols.Count > 0)
            {
                System.Security.Authentication.SslProtocols sslProtocol;
                isValid = sslProtocols.Any(x => !Enum.TryParse<System.Security.Authentication.SslProtocols>(x, out sslProtocol));
            }

            return isValid;
        }

        public List<System.Security.Authentication.SslProtocols> GetSslProtocols()
        {            
            List<System.Security.Authentication.SslProtocols> SslProtocols = new List<System.Security.Authentication.SslProtocols>();
            if (!string.IsNullOrEmpty(EnabledSslProtocols.Value))
            {
                System.Security.Authentication.SslProtocols tempSslProtocols;                
                string splitValue = EnabledSslProtocols.Value.Contains(",") ? "," : EnabledSslProtocols.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedSslProtocols = EnabledSslProtocols.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedSslProtocols != null && splitedSslProtocols.Length > 0)
                    {                        
                        foreach (string splitedSslProtocol in splitedSslProtocols)
                        {
                            if (!string.IsNullOrEmpty(splitedSslProtocol) && Enum.TryParse(splitedSslProtocol, out tempSslProtocols))
                            {
                                SslProtocols.Add(tempSslProtocols);
                            }
                        }
                    }
                }
                else if (Enum.TryParse(EnabledSslProtocols.Value, out tempSslProtocols))
                {
                    SslProtocols.Add(tempSslProtocols);
                }
            }
            else
            {
                SslProtocols.Add(System.Security.Authentication.SslProtocols.None);
            }

            return SslProtocols;            
        }

        public bool ValidateDecompressionMethods()
        {
            bool isValid = true;
            List<string> decompressionMethods = new List<string>();
            if (!string.IsNullOrEmpty(EnabledDecompressionMethods.Value))
            {
                string splitValue = EnabledDecompressionMethods.Value.Contains(",") ? "," : EnabledDecompressionMethods.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedDecompressionMethods = EnabledDecompressionMethods.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
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
                    decompressionMethods.Add(EnabledDecompressionMethods.Value);
                }
            }

            if (decompressionMethods.Count > 0)
            {
                System.Net.DecompressionMethods decompressionMethod;
                isValid = decompressionMethods.Any(x => !Enum.TryParse<System.Net.DecompressionMethods>(x, out decompressionMethod));
            }

            return isValid;
        }

        public List<System.Net.DecompressionMethods> GetDecompressionMethods()
        {
            List<System.Net.DecompressionMethods> DecompressionMethods = new List<System.Net.DecompressionMethods>();
            if (!string.IsNullOrEmpty(EnabledDecompressionMethods.Value))
            {
                System.Net.DecompressionMethods tempDecompressionMethods;
                string splitValue = EnabledDecompressionMethods.Value.Contains(",") ? "," : EnabledDecompressionMethods.Value.Contains(";") ? ";" : string.Empty;
                if (!string.IsNullOrEmpty(splitValue))
                {
                    string[] splitedDecompressionMethods = EnabledDecompressionMethods.Value.Split(new string[] { splitValue }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedDecompressionMethods != null && splitedDecompressionMethods.Length > 0)
                    {
                        foreach (string splitedDecompressionMethod in splitedDecompressionMethods)
                        {
                            if (!string.IsNullOrEmpty(splitedDecompressionMethod) && Enum.TryParse(splitedDecompressionMethod, out tempDecompressionMethods))
                            {
                                DecompressionMethods.Add(tempDecompressionMethods);
                            }
                        }
                    }
                }
                else if (Enum.TryParse(EnabledDecompressionMethods.Value, out tempDecompressionMethods))
                {
                    DecompressionMethods.Add(tempDecompressionMethods);
                }
            }
            else
            {
                DecompressionMethods.Add(System.Net.DecompressionMethods.None);
            }

            return DecompressionMethods;
        }

    }
}
