using System;
using Force.DeepCloner;

namespace Core.Tests
{
    public static class Patcher
    {
        public static T With<T>(this T value, Action<T> patch)
        {
            var e = value.DeepClone();
            patch(e);
            return e;
        }
    }
}