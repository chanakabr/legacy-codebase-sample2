using System.Collections.Generic;
using Google.Protobuf;
using MoreLinq;

namespace APILogic
{
    internal static class Extensions
    {
        public static T[] Clone<T>(this T[] listToClone) where T: IDeepCloneable<T>
        {
            if (listToClone == null) return null;
            var newList = new T[listToClone.Length];
            listToClone.ForEach((item, i) =>
            {
                newList[i] = item.Clone();
            });
            return newList;
        }
        
        public static List<T> Clone<T>(this List<T> listToClone) where T: IDeepCloneable<T>
        {
            if (listToClone == null) return null;
            var newList = new List<T>(listToClone.Count);
            listToClone.ForEach(item =>
            {
                newList.Add(item.Clone());
            });
            return newList;
        }
        
        public static T Clone<T>(this T value) where T: IDeepCloneable<T>
        {
            return value == null ? default : value.Clone();
        }
    }
}