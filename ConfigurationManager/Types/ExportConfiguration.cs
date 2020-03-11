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
                DefaultValue = "c://export",
                OriginalKey = "export.base_path"
            };
            PathFormat = new StringConfigurationValue("path_format", this)
            {
                DefaultValue = "{0}/{1}/{2}",
                OriginalKey = "export.path_format"
            };
            FileNameFormat = new StringConfigurationValue("file_name_format", this)
            {
                DefaultValue = "{0}_{1}.xml",
                OriginalKey = "export.file_name_format"
            };
            FileNameDateFormat = new StringConfigurationValue("file_name_date_format", this)
            {
                DefaultValue = "yyyyMMddHHmmss",
                OriginalKey = "export.file_name_date_format"
            };
            MaxAssetsPerThread = new NumericConfigurationValue("max_assets_per_thread", this)
            {
                DefaultValue = 50,
                OriginalKey = "export.max_assets_per_thread"
            };
            MaxThreads = new NumericConfigurationValue("max_threads", this)
            {
                DefaultValue = 10,
                OriginalKey = "export.max_threads"
            };
            ThreadRetryLimit = new NumericConfigurationValue("thread_retry_limit", this)
            {
                DefaultValue = 2,
                OriginalKey = "export.thread_retry_limit"
            };
            FrequencyMinimumValue = new NumericConfigurationValue("frequency_min_value", this)
            {
                DefaultValue = 1,
                OriginalKey = "export.frequency_min_value"
            };
        }
    }
}
