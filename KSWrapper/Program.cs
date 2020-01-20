using System;
using System.Text.RegularExpressions;

namespace KSWrapper
{
    public class shir_ks
    {
        private const string shirksformat = "int: (?<intval>[^,]+); string:(?<stringval>[^,]+);";
        //"int: 123; string: shir123;"
        public int MyPropertyint { get; set; }
        public string MyPropertystring { get; set; }
        
        public shir_ks(int intval, string stringval)
        {
            this.MyPropertyint = intval;
            this.MyPropertystring = stringval;
        }

        public shir_ks(string shitks)
        {
            var expression = new Regex(shirksformat);

            var match = expression.Match(shitks);
            int.TryParse(match.Groups["intval"].ToString(), out int myint);
            this.MyPropertyint = myint;
            this.MyPropertystring = match.Groups["stringval"].ToString();
        }
    }
}
