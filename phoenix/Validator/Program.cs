using System;
using Validator.Managers.Scheme;

namespace Validator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool valid = new SchemeValidator(false).Validate();
            
            // We can no longer validate tcm in new jenkins its an Single Version tcm
            //valid &= ApplicationConfiguration.Validate();

            if (valid)
                Environment.Exit(0);

            Environment.Exit(-1);
        }
    }
}
