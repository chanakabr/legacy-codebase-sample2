using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader.PredefinedAdapters
{
    public class CustomProvider : LoaderProvider<ICustomAdapter>
    {
        public override object GetDataFromSource(ICustomAdapter adapter)
        {
            return  adapter.ExtractResponse();
        }
    }
}
