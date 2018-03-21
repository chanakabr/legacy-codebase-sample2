using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ExportConfiguration : ConfigurationValue
    {
        public StringConfigurationValue BasePath;
        public StringConfigurationValue PathFormat;
        public StringConfigurationValue FileNameFormat;
        public StringConfigurationValue FileNameDateFormat;
        public NumericConfigurationValue MaxAssetsPerThread;
        public NumericConfigurationValue MaxThreads;
        public NumericConfigurationValue ThreadRetryLimit;
        public NumericConfigurationValue FrequencyMinimumValue;

        public ExportConfiguration(string key) : base(key)
        {
            BasePath = new StringConfigurationValue("base_path", this)
            {
                DefaultValue = "c://export"
            };
            PathFormat = new StringConfigurationValue("path_format", this)
            {
                DefaultValue = "{0}/{1}/{2}"
            };
            FileNameFormat = new StringConfigurationValue("file_name_format", this)
            {
                DefaultValue = "{0}_{1}.xml"
            };
            FileNameDateFormat = new StringConfigurationValue("file_name_date_format", this)
            {
                DefaultValue = "yyyyMMddHHmmss"
            };
            MaxAssetsPerThread = new NumericConfigurationValue("max_assets_per_thread", this)
            {
                DefaultValue = 50
            };
            MaxThreads = new NumericConfigurationValue("max_threads", this)
            {
                DefaultValue = 10
            };
            ThreadRetryLimit = new NumericConfigurationValue("thread_retry_limit", this)
            {
                DefaultValue = 2
            };
            FrequencyMinimumValue = new NumericConfigurationValue("frequency_min_value", this)
            {
                DefaultValue = 1
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= BasePath.Validate();
            result &= PathFormat.Validate();
            result &= FileNameFormat.Validate();
            result &= FileNameDateFormat.Validate();
            result &= MaxAssetsPerThread.Validate();
            result &= MaxThreads.Validate();
            result &= ThreadRetryLimit.Validate();
            result &= FrequencyMinimumValue.Validate();

            return result;
        }
    }
}
