using Jsonable;
using MessagePack;

namespace Sample.SampleData
{
    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_Unknown
    {
    }
}
