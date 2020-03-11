using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Configuration;

namespace XMLAdapter
{
    public sealed class MCAdapter : BaseXMLAdapter
    {
        public override void Init()
        {
            base.Init(); // Init base
        }

        public override string ParseDateValue(string date)
        {
            DateTime dDate = DateTime.Parse(date);
            string sDate = dDate.AddHours(-8).ToString("dd/MM/yyyy HH:mm:ss");
            return sDate;
        }

        public override string ParseFinalDateValue(string date)
        {
            DateTime dDate = DateTime.Parse(date);
            if (string.IsNullOrEmpty(date))
            {
                string sFinalEndDate = DateTime.MaxValue.ToString("dd/MM/yyyy HH:mm:ss");
                return sFinalEndDate;
            }

            return ParseDateValue(date);
        }

        public override string ParseENumSNum(string eID)
        {
            string sEpisodeNum = string.Empty;
            string sSeasonNum = "0";

            string sPattern = "^s(?<season>[0-9]*):ep(?<episode>[0-9]*)$";

            Match m = Regex.Match(eID.ToLower(), sPattern);

            if (m.Success)
            {
                sSeasonNum = m.Groups["season"].Value;
                sEpisodeNum = m.Groups["episode"].Value;
            }
            else
            {
                sEpisodeNum = eID;
                sSeasonNum = "0";
            }

            return string.Format("{0}|{1}", sEpisodeNum, sSeasonNum);
        }

        public override string ParseFileType(string fileType)
        {
            string retVal = string.Empty;
            string targetPlatform = fileType.Substring(4, 1);
            if (!string.IsNullOrEmpty(targetPlatform))
            {
                switch (targetPlatform)
                {
                    case ("0"):
                    {
                        return "STB Main";
                    }
                    case ("1"):
                    {
                        return "Main";
                    }
                    case ("2"):
                    {
                        return "iPhone Main";
                    }
                    case ("3"):
                    {
                        return "iPad Main";
                    }
                    default:
                        break;
                }
            }

            return retVal;
        }

        public override string GetAdProvider()
        {
            string ret = ConfigurationManager.AppSettings["ADIFeederDefaultAdProvider"];
            if (string.IsNullOrEmpty(ret))
            {
                return "";
            }

            return ret;
        }

        public override string GetFileDuration(string sDuration)
        {
            string[] durationArr = sDuration.Split(':');
            TimeSpan ts = new TimeSpan(int.Parse(durationArr[0]), int.Parse(durationArr[1]), int.Parse(durationArr[2]));
            int durationSec = (int)ts.TotalSeconds;
            return durationSec.ToString();
        }
        
    }
}
