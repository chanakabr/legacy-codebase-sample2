using System.Collections.Generic;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStringComparerService
    {
        IComparer<string> GetComparer(string languageCode);
    }
}