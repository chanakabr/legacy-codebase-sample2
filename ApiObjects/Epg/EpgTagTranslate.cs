using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    public class EpgTagTranslate
    {
        public EpgTagTranslate()
        {
        }

        public EpgTagTranslate(int language, string value , int id)
        {
            nLanguage = language;
            sValue = value;
            nID = id;
        }

        public EpgTagTranslate(int language, string value, string valueMain)
        {
            nLanguage = language;
            sValue = value;
            sValueMain = valueMain;
        }

        public int nLanguage { get; set; }
        public string sValue { get; set; }
        public int nID {get;set;}
        public string sValueMain { get; set; }
    }
}

