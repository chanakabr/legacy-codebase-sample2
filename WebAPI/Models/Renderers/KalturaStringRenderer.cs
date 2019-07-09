using System;
using System.IO;
using System.Text;

namespace WebAPI.Models.Renderers
{
    public class KalturaStringRenderer : KalturaRenderer
    {
        private String _sContent;

        public KalturaStringRenderer(string content)
            : base()
        {
            _sContent = content;
        }

        public override string GetOutput()
        {
            return _sContent;
        }

        public override void Output(Stream writeStream) 
        {
            var buf = Encoding.UTF8.GetBytes(_sContent);
            writeStream.Write(buf, 0, buf.Length);
        }
    }
}