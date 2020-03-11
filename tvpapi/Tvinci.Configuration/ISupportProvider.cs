using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Configuration
{
    public interface ISupportProvider
    {
        void SyncFromConfigurationFile(string virtualPath);
    }

}
