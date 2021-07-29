using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurationManager.Types
{
    public class S3DataLakeConfiguration : S3Configuration
    {
        public override string[] TcmPath => new string[] { TcmObjectKeys.DataLake, TcmKey };
    }
}
