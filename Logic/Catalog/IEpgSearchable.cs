using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Catalog
{
    public interface IEpgSearchable
    {
        EpgSearchObj BuildEPGSearchObject();
    }
}
