using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalog.Request;

namespace Catalog
{
    public interface IFactoryImp
    {
        IRequestImp GetTypeImp(BaseRequest oIRequest);
    }
}
