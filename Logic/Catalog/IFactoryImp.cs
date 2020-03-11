using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Catalog.Request;

namespace Core.Catalog
{
    public interface IFactoryImp
    {
        IRequestImp GetTypeImp(BaseRequest oIRequest);
    }
}
