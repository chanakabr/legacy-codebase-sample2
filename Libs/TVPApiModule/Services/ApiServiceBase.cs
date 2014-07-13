using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Services
{
    public class ApiServiceBase 
    {
        private static object _locker = new object();
        private Dictionary<string, string> _SupportedPlatforms;
        public Dictionary<string, string> SupportedPlatforms
        {
            get
            {
                if (_SupportedPlatforms == null || _SupportedPlatforms.Count == 0)
                {
                    lock (_locker)
                    {
                        if (_SupportedPlatforms == null || _SupportedPlatforms.Count == 0)
                        {
                            _SupportedPlatforms = TVPApi.ConnectionHelper.GetSupportedPlatforms();
                        }
                    }
                }
                return _SupportedPlatforms;
            }
        }

        private char test() { return 'a'; }
    }
}