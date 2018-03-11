using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class BaseCacheConfiguration : ConfigurationValue
    {
        public StringConfigurationValue CacheType;
        public StringConfigurationValue Version;
        public NumericConfigurationValue CacheDocumentTimeout;

        public BaseCacheConfiguration(string key) : base(key)
        {
            CacheType = new StringConfigurationValue("cache_type", this)
            {
                DefaultValue = "CouchBase"
            };
            Version = new StringConfigurationValue("version", this)
            {
                Description = "Version name or number, to allow backward compatibility for cache objects"
            }; CacheDocumentTimeout = new NumericConfigurationValue("cache_document_timeout", this)
            {
                DefaultValue = 86400
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= CacheType.Validate();
            result &= Version.Validate();
            result &= CacheDocumentTimeout.Validate();

            return result;
        }
    }
}