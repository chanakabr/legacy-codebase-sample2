using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CRUDOperations<T> where T : IEquatable<T>
    {
        public List<T> ItemsToAdd { get; set; }
        public List<T> ItemsToUpdate { get; set; }
        public List<T> ItemsToDelete { get; set; }
        public List<T> RemainingItems { get; set; }

        /// <summary>
        /// Use only in special cases to distinguish between items to update that were explicitly requesyed
        /// and items that require updates implicitly
        /// </summary>
        public List<T> AffectedItems { get; set; }


        public CRUDOperations()
        {
            ItemsToAdd = new List<T>();
            ItemsToUpdate = new List<T>();
            ItemsToDelete = new List<T>();
            RemainingItems = new List<T>();
            AffectedItems = new List<T>();
        }

        public void AddRange(CRUDOperations<T> crudsToAdd)
        {
            ItemsToAdd.AddRange(crudsToAdd.ItemsToAdd);
            ItemsToUpdate.AddRange(crudsToAdd.ItemsToUpdate);
            ItemsToDelete.AddRange(crudsToAdd.ItemsToDelete);
            RemainingItems.AddRange(crudsToAdd.RemainingItems);
            AffectedItems.AddRange(crudsToAdd.AffectedItems);
        }


        ///// <summary>
        ///// Applies the CRUD operations onto a given collection of items
        ///// NOTE: T has to implement IEquatable<T>, Equals and GetHashCode 
        ///// </summary>
        ///// <param name="existingItems">Colection of items to apply CRUD operations onto</param>
        ///// <returns>The collection after CRUD operations were applied</returns>
        //public IEnumerable<T> ApplyCRUDOperations(IEnumerable<T> existingItems)
        //{
        //    var remainingItems = existingItems.Except(ItemsToDelete);
        //    var finalItemsState = ItemsToAdd.Concat(ItemsToUpdate).Concat(remainingItems);
        //    return remainingItems;
        //}
    }
}
