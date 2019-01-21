using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Types
{
    public class FileUploadConfiguration : ConfigurationValue
    {
        public S3Configuration S3;
        public FileSystemConfiguration FileSystem;

        public BooleanConfigurationValue ShouldDeleteSourceFile;
        
        internal StringConfigurationValue Type;
        public eFileUploadType UploadType
        {
            get
            {
                if (Enum.TryParse(Type.Value, true, out eFileUploadType ret))
                    return ret;

                return eFileUploadType.None;
            }
        }
        
        public enum eFileUploadType
        {
            None,
            S3,
            FileSystem
        }

        public FileUploadConfiguration(string key) 
            : base(key)
        {
            S3 = new S3Configuration("S3", this);
            FileSystem = new FileSystemConfiguration("FileSystem", this);

            Type = new StringConfigurationValue("type", this)
            {
                DefaultValue = "s3"
            };
            ShouldDeleteSourceFile = new BooleanConfigurationValue("shouldDeleteSourceFile", this)
            {
                DefaultValue = false,
            };
        }

        internal override bool Validate()
        {
            bool res = base.Validate();

            if (UploadType.Equals(eFileUploadType.None))
            {
                res = false;
                LogError("invalid value for property Type", ConfigurationValidationErrorLevel.Failure);
            }

            return res;
        }
    }
}