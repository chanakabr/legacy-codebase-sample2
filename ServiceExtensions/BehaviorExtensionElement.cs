#if NETCOREAPP3_0
namespace System.ServiceModel.Configuration
{
    /// <summary>
    /// This is a shim to support compatability with .net standard and net core
    /// This calss has no value in netstandard and is here only to support compiling
    /// agains both net452 and netstandard2.0
    /// </summary>
    public abstract class BehaviorExtensionElement
    {

        public abstract Type BehaviorType { get; }

        protected abstract object CreateBehavior();
    }
}
#endif
