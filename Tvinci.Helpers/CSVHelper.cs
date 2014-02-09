using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Tvinci.Helpers
{
    public abstract class CSVHelper<ReturnType> where ReturnType : new()
    {
        protected ReturnType ReadFile(string theFileName, char theDelimiter)
        {
            if (theFileName == null)
                throw new Exception("File given is null");

            if (!File.Exists(HttpContext.Current.Server.MapPath(theFileName)))
                throw new Exception("File given doesn't exist");

            string[] values;
            ReturnType result = new ReturnType();
            try
            {
                TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath(theFileName), Encoding.UTF7);

                string curLine = reader.ReadLine();
                int counter = 1;

                while (curLine != null)
                {
                    values = curLine.Split(theDelimiter);

                    if (!MapLine(values, result))
                        throw new Exception(string.Format("Failed reading line number {0}", counter));

                    counter++;

                    curLine = reader.ReadLine();
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed reading file");
            }

            return result;
        }

        protected abstract bool MapLine(string[] theValues, ReturnType theResult);
    }
}
