#if NET5_0_OR_GREATER == false

// NOTE: to use record & init accessor in Unity 2021
//       https://stackoverflow.com/questions/62648189/testing-c-sharp-9-0-in-vs2019-cs0518-isexternalinit-is-not-defined-or-imported
#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices
{
    [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}

#endif


#if NETCOREAPP3_0_OR_GREATER == false

#pragma warning disable IDE0130
#pragma warning disable IDE0060

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
#pragma warning disable IDE0290
        public CallerArgumentExpressionAttribute(string parameterName) { }
#pragma warning restore IDE0290
    }
}

#endif
