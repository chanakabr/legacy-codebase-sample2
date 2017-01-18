using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{



    public class EPGChannelObject
    {
        public string EPG_CHANNEL_ID;
        public string NAME;
        public string DESCRIPTION;
        public string ORDER_NUM;
        public string IS_ACTIVE;
        public string PIC_URL;
        public string GROUP_ID;
        public string EDITOR_REMARKS;
        public string STATUS;
        public string UPDATER_ID;
        public string CREATE_DATE;
        public string PUBLISH_DATE;
        public string CHANNEL_ID;
        public string MEDIA_ID;

        public void Initialize(string nEPG_CHANNEL_ID, string nNAME, string nDESCRIPTION, string nORDER_NUM, string nIS_ACTIVE, string nPIC_URL, string nGROUP_ID, string nEDITOR_REMARKS, string nSTATUS, string nUPDATER_ID, string nCREATE_DATE, string nPUBLISH_DATE, string nCHANNEL_ID, string nMEDIA_ID)
        {
            EPG_CHANNEL_ID = nEPG_CHANNEL_ID;
            NAME = nNAME;
            DESCRIPTION = nDESCRIPTION;
            ORDER_NUM = nORDER_NUM;
            IS_ACTIVE = nIS_ACTIVE;
            PIC_URL = nPIC_URL;
            GROUP_ID = nGROUP_ID;
            EDITOR_REMARKS = nEDITOR_REMARKS;
            STATUS = nSTATUS;
            UPDATER_ID = nUPDATER_ID;
            CREATE_DATE = nCREATE_DATE;
            PUBLISH_DATE = nPUBLISH_DATE;
            CHANNEL_ID = nCHANNEL_ID;
            MEDIA_ID = nMEDIA_ID;

        }
    }
      
}
