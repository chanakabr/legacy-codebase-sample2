using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLAdapter
{
    public abstract class BaseXMLAdapter
    {
        public BaseXMLAdapter()
        {
        }

        public virtual string HandleSubGenre(string filter, string language)
        {
            return "";
        }

        public virtual string HandleGenre(string filter, string language)
        {
            return "";
        }

        public virtual string HandleLanguage(string filter)
        {
            return "";
        }

        public virtual string HandleRating(string filter)
        {
            return "";
        }

        public virtual string HandleParental(string code)
        {
            return "";
        }

        public virtual string HandleMediaType(string filter)
        {
            return "";
        }

        public virtual string GetOfferNumber()
        {
            return "";
        }

        public virtual void Init()
        {
        }

        public virtual string HandleDiv(string number, string divider)
        {
            return "";
        }

        public virtual string GetRatio(string x, string y)
        {
            return "";
        }

        public virtual string ParseStartValue(string date)
        {
            return "";
        }

        public virtual string ParseDateValue(string date)
        {
            return "";
        }

        public virtual string ParseFinalDateValue(string date)
        {
            return "";
        }
        public virtual string ParseENumSNum(string eID)
        {
            return "";
        }
        public virtual string ParseFileType(string fileType)
        {
            return "";
        }
        public virtual string GetAdProvider()
        {
            return "";
        }
        public virtual string GetFileDuration(string sDuration)
        {
            return "";
        }
    }
}
