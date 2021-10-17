using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiLogic.Tests
{
    public class MultipleTypeRelay : ISpecimenBuilder
    {
        private readonly TypeRelay[] _relays;
        private readonly Random _random = new Random();

        public MultipleTypeRelay(Type from, params Type[] to) : this(from, (IEnumerable<Type>)to)
        {
        }

        public MultipleTypeRelay(Type from, IEnumerable<Type> to)
        {
            _relays = to.Select(_ => new TypeRelay(from, _)).ToArray();
        }

        public static MultipleTypeRelay NewHierarchyRelay<T>()
        {
            var tType = typeof(T);
            var notAbstractSubclasses = Assembly.GetAssembly(tType).GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && (tType.IsAssignableFrom(x) || x.IsSubclassOf(tType))).ToList();
            return new MultipleTypeRelay(tType, notAbstractSubclasses);
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return _relays[_random.Next(_relays.Length)].Create(request, context);
        }
    }
}
