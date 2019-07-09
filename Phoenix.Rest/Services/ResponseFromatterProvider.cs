using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebAPI.App_Start;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace Phoenix.Rest.Services
{
    public interface IResponseFromatterProvider
    {
        BaseFormatter GetFormatter(IEnumerable<string> acceptHeader, string responseFormat);
    }

    public class ResponseFromatterProvider : IResponseFromatterProvider
    {
        private readonly Dictionary<string, BaseFormatter> _FormattersByAcceptHeader = new Dictionary<string, BaseFormatter>();
        private readonly Dictionary<KalturaResponseType, BaseFormatter> _FormattersByResponseType = new Dictionary<KalturaResponseType, BaseFormatter>();
        private readonly BaseFormatter _DefaultFormatter;

        private static readonly Type KALTURA_RESPONSE_TYPE = typeof(KalturaResponseType);

        public ResponseFromatterProvider()
        {
            _DefaultFormatter = new JilFormatter();
            AddFormatter(_DefaultFormatter);
            AddFormatter(new CustomXmlFormatter());
            AddFormatter(new JsonPFormatter());
            AddFormatter(new AssetXmlFormatter());
            AddFormatter(new CustomResponseFormatter());
        }

        public BaseFormatter GetFormatter(IEnumerable<string> acceptHeader, string responseFormat)
        {
            var formatter = _DefaultFormatter;
            if (!string.IsNullOrEmpty(responseFormat))
            {
                if (Enum.TryParse(KALTURA_RESPONSE_TYPE, responseFormat, out var kalturaResponseType))
                {
                    formatter = _FormattersByResponseType[(KalturaResponseType)kalturaResponseType];
                }
            }
            else if (acceptHeader?.Any() == true)
            {
                foreach (var acceptHeaderValue in acceptHeader)
                {
                    if (_FormattersByAcceptHeader.TryGetValue(acceptHeaderValue, out var formatterByHeader))
                    {
                        formatter = formatterByHeader;
                        break;
                    };
                }
            }

            return formatter;
        }

        private void AddFormatter(BaseFormatter formatter)
        {
            foreach (var acceptHeader in formatter.AcceptContentTypes)
            {
                _FormattersByAcceptHeader.TryAdd(acceptHeader, formatter);
            }

            _FormattersByResponseType.TryAdd(formatter.Format, formatter);
        }
    }
}
