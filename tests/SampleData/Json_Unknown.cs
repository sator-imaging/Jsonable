using Jsonable;
using MessagePack;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CS1591   // Missing XML comment for publicly visible type or member

namespace Tests.SampleData
{
    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_Unknown
    {
    }
}
