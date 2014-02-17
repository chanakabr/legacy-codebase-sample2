using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog
{
    public interface IFactoryImp
    {
        IRequestImp GetTypeImp(BaseRequest oIRequest);
    }
}
