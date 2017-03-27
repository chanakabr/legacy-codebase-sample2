using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CountryLocale : Country
    {
        public string MainLanguageCode { get; set; }
        public HashSet<string> LanguageCodes { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySign { get; set; }
        public double VatPercent { get; set; }

        public CountryLocale()
            : base()
        {
            this.MainLanguageCode = string.Empty;
            this.LanguageCodes = new HashSet<string>();
            this.CurrencyCode = string.Empty;
            this.CurrencySign = string.Empty;
            this.VatPercent = 0;
        }

        public CountryLocale(int countryId, string currencyCode, string currencySign, string mainLanguageCode, double vatPercent)
            : base()
        {
            this.Id = countryId;
            this.CurrencyCode = currencyCode;
            this.CurrencySign = currencySign;
            this.MainLanguageCode = mainLanguageCode;
            this.LanguageCodes = new HashSet<string>();
            this.LanguageCodes.Add(this.MainLanguageCode);
            this.VatPercent = vatPercent;
        }

    }
}
