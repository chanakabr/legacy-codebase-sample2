using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TVinciShared;
using WebAPI;
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
            AddFormatter(_DefaultFormatter, new[] { "application/json" }, KalturaResponseType.JSON);
            AddFormatter(new CustomXmlFormatter(), new[] { "application/xml", "text/xml" }, KalturaResponseType.XML);
            AddFormatter(new JsonPFormatter(), null, KalturaResponseType.JSONP);
            AddFormatter(new AssetXmlFormatter(), new[] { "" }, KalturaResponseType.ASSET_XML);
            AddFormatter(new ExcelFormatter(), new[] { ExcelFormatterConsts.EXCEL_CONTENT_TYPE }, KalturaResponseType.EXCEL);
        }

        public BaseFormatter GetFormatter(IEnumerable<string> acceptHeader, string responseFormat)
        {
            var formatter = _DefaultFormatter;
            var formatterFound = false;

            // Sunny GEN-489 : ServeByDevice is a unique case that requires custom response formatter.
            // in legacy phoenix, there is a not very lovely bypass that determines the use of this formatter, using a special item in http context.
            // we are obliged to imitate this behavior here as well, although we really don't want to.
            // we should think of a proper solution to this case
            if (HttpContext.Current.Items[RequestContextUtils.REQUEST_SERVE_CONTENT_TYPE] != null)
            {
                return new CustomResponseFormatter();
            }

            // Try Get by response format if was sent
            if (!string.IsNullOrEmpty(responseFormat))
            {
                formatterFound = TryGetByResponseFormat(responseFormat, out var formatterByResponse);
                if (formatterFound) { formatter = formatterByResponse; }
            }
            // Try get by Accept header if was sent
            if (acceptHeader?.Any() == true && !formatterFound)
            {
                formatterFound = TryGetByAcceptHeader(acceptHeader, out var formatterByAcceptHeader);
                if (formatterFound) { formatter = formatterByAcceptHeader; }
            }

            return formatter;
        }

        private bool TryGetByAcceptHeader(IEnumerable<string> acceptHeader, out BaseFormatter formatter)
        {
            foreach (var acceptHeaderValue in acceptHeader)
            {
                if (_FormattersByAcceptHeader.TryGetValue(acceptHeaderValue, out var formatterByHeader))
                {
                    formatter = formatterByHeader;
                    return true;
                };
            }

            formatter = null;
            return false;
        }

        private bool TryGetByResponseFormat(string responseFormat, out BaseFormatter formatter)
        {
            if (Enum.TryParse(KALTURA_RESPONSE_TYPE, responseFormat, out var kalturaResponseType))
            {
                var responseFromatType = (KalturaResponseType)kalturaResponseType;
                if (_FormattersByResponseType.TryGetValue(responseFromatType, out var formatterByResponseType))
                {
                    formatter = formatterByResponseType;
                    return true;
                }
            }

            formatter = null;
            return false;
        }

        private void AddFormatter(BaseFormatter formatter, IEnumerable<string> acceptHeaderContentTypes, KalturaResponseType? responseType)
        {
            if (acceptHeaderContentTypes != null)
            {
                foreach (var acceptHeader in acceptHeaderContentTypes)
                {
                    _FormattersByAcceptHeader.TryAdd(acceptHeader, formatter);
                }
            }

            if (responseType.HasValue)
            {
                _FormattersByResponseType.TryAdd(responseType.Value, formatter);
            }
        }
    }
}
