using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using KLogMonitor;
using WebAPI.Managers;
using WebAPI.Models.API;

namespace WebAPI.App_Start
{
    public class JilFormatter : BaseFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected JsonManager jsonManager;

        public JilFormatter(KalturaResponseType fortmat = KalturaResponseType.JSON) : base(fortmat, "application/json")
        {
            jsonManager = JsonManager.GetInstance();
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }
        
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                string json = jsonManager.Serialize(value);
                streamWriter.Write(json);
                return Task.FromResult(writeStream);
            }
        }
    }
}