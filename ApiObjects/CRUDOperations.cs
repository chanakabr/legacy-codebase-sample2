using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class CRUDOperations<T>
    {
        public IList<T> ItemsToAdd { get; set; }
        public IList<T> ItemsToUpdate { get; set; }
        public IList<T> ItemsToDelete { get; set; }
    }
}
