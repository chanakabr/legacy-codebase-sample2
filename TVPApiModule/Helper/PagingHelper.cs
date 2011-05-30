using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PagingHelper
/// </summary>
public class PagingHelper
{
    public PagingHelper()
    {

    }

    //Get paged results (from start index to startIndex + page size
    public static IEnumerable<T> GetPagedData<T>(int startIndex, int pageSize, IEnumerable<T> origDT)
    {
        IEnumerable<T> retVal = null;
        int itemsToTake = 0;
        if (origDT.Count() > 0 && origDT.Count() > startIndex)
        {
            //If total items > index of last item to take with paging - take page size amount of items
            if (origDT.Count() >= (startIndex + pageSize))
            {
                itemsToTake = pageSize;
            }
            else
            {
                //Else - take whats left of total items from the start index of the paging
                itemsToTake = origDT.Count() - startIndex;
            }
        }

        if (itemsToTake > 0)
        {
            // take pages results
            retVal = origDT.Skip(startIndex).Take(itemsToTake);
        }

        return retVal;
    }
}
