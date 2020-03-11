using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Logger.Model
{
    public class LogImpl
    {

        private const string sWriteLocation = @"c:\TempLog";
        string path = @"c:\TempLog\MyTest.txt";

        internal void WriteLog(string sLogToWrite)
        {
            if (!Directory.Exists(sWriteLocation))
            {
                Directory.CreateDirectory(sWriteLocation);
            }

            StreamWriter sw = null;

            if (!File.Exists(path))
            {
                // Create a file to write to. 
                sw = File.CreateText(path);
            }

            if (sw != null)
            {
                sw.Dispose();
            }

            //sw = File.AppendText(path);

            //sw.WriteLine(sLogToWrite);
            // This text is always added, making the file longer over time 
            // if it is not deleted. 
            using (StreamWriter sw1 = File.AppendText(path))
            {
                sw1.WriteLine(sLogToWrite);
                sw1.WriteLine("-------------------------------------------------------------------------------------------------");
            }
        }
    }
}
