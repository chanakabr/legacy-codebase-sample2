using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class NullableObj<T>
    {
        public T Obj { get; set; }
        public bool IsNull { get; set; }

        public NullableObj()
        {
            IsNull = false;
        }

        public NullableObj(T obj)
        {
            Obj = obj;
            IsNull = false;
        }

        public NullableObj(T obj, bool isNull)
        {
            Obj = obj;
            IsNull = isNull;
        }
    }
}
