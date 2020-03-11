using System.Collections.Generic;

namespace Tvinci.Helpers
{
    public class CompareCaseInSensitive : IEqualityComparer<string>, IComparer<string>
    {

        #region IEqualityComparer<string> Members

        public bool Equals(string x, string y)
        {
            return x.ToLower().Equals(y.ToLower());
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLower().GetHashCode();
        }

        #endregion

        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            return x.ToLower().CompareTo(y.ToLower());
        }

        #endregion
    }
}