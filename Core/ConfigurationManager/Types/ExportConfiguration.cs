using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class ExportConfiguration : BaseConfig<ExportConfiguration>
    {
        public BaseValue<string> BasePath = new BaseValue<string>("base_path", "c://export");
        public BaseValue<string> PathFormat = new BaseValue<string>("path_format", "{0}/{1}/{2}");
        public BaseValue<string> FileNameFormat = new BaseValue<string>("file_name_format", "{0}_{1}.xml");
        public BaseValue<string> FileNameDateFormat = new BaseValue<string>("file_name_date_format", "yyyyMMddHHmmss");

        public BaseValue<int> MaxAssetsPerThread = new BaseValue<int>("max_assets_per_thread", 50);
        public BaseValue<int> MaxThreads = new BaseValue<int>("max_threads", 10);
        public BaseValue<int> ThreadRetryLimit = new BaseValue<int>("thread_retry_limit", 2);
        public BaseValue<int> FrequencyMinimumValue = new BaseValue<int>("frequency_min_value", 1);

        public override string TcmKey => TcmObjectKeys.ExportConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
