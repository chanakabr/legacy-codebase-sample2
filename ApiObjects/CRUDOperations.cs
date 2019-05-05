using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class CRUDOperations<T>
    {
        public List<T> ItemsToAdd { get; set; }
        public List<T> ItemsToUpdate { get; set; }
        public List<T> ItemsToDelete { get; set; }

        public CRUDOperations()
        {
            ItemsToAdd = new List<T>();
            ItemsToUpdate = new List<T>();
            ItemsToDelete = new List<T>();
        }
    }
}
