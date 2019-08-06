using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApiObjects.TimeShiftedTv
{

    public class ExternalSeriesRecording : SeriesRecording
    {
        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Dictionary<string, string> MetaData { get; set; }

        public ExternalSeriesRecording()
            : base()
        {
            this.isExternalRecording = true;
        }

        public ExternalSeriesRecording(ExternalSeriesRecording externalSeriesRecording)
            : base(externalSeriesRecording)
        {            
            if (externalSeriesRecording.MetaData != null)
                this.MetaData = new Dictionary<string, string>(externalSeriesRecording.MetaData);
        }

        public string MetaDataAsJson
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(this.MetaData, Formatting.None);
                }
                catch (Exception e)
                {
                    _log.Warn("error when trying to serialize MetaData to json");
                    return string.Empty;
                }
            }
        }
    }
}
