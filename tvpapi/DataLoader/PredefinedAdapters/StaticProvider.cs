using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public class StaticProvider : LoaderProvider<IStaticAdapter>
    {
        public static StaticProvider Instance { get; private set; }

        static StaticProvider()
        {
            Instance = new StaticProvider();
        }

        private StaticProvider ()
	    {

	    }

        public override object GetDataFromSource(IStaticAdapter adapter)
        {
            object result = adapter.Data;

            if (result == null)
            {
               // throw new Exception("When using 'StaticAdapter', you must always set the data even during postback'");
            }
            return result ;
        }
    }
}
