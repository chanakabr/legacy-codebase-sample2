using System;
using System.Threading;

namespace DAL.SearchPriorityGroups
{
    public class KeyGenerator : IKeyGenerator
    {
        private static readonly Lazy<KeyGenerator> Lazy = new Lazy<KeyGenerator>(() => new KeyGenerator(), LazyThreadSafetyMode.PublicationOnly);

        public static KeyGenerator Instance => Lazy.Value;

        public string GetGuidKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}