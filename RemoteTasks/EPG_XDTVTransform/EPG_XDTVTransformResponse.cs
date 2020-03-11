﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPG_XDTVTransform
{
    public class EPG_XDTVTransformResponse
    {

        public string host;
        public string account_id; //our identifier in alcaltel_lucent
        public string epg_data;// xdtv xml

        public EPG_XDTVTransformResponse(string sHost, string accountID, string sXml)
        {
            host = sHost;
            account_id = accountID;
            epg_data = sXml;
        }

        public override string ToString()
        {
            string[] strArray = new string[3];
            strArray[0] = host;
            strArray[1] = account_id;
            strArray[2] = epg_data;

            return string.Join("###", strArray); //needs to be changed to format
        }

    }
}
