using Jsonable;
using System.Collections.Generic;

namespace Standalone
{
    [FromJson]
    [ToJson]
    [ToJson(Property = nameof(IntProp))]
    [ToJson(Property = nameof(StringProp))]
    public partial class MyData
    {
        public int IntProp { get; set; }
        public string? StringProp { get; set; }
        public byte[]? Base64Prop { get; set; }
        public ushort[]? ArrayProp { get; set; }
        public List<float>? ListProp { get; set; }
        public Dictionary<string, double>? MapProp { get; set; }
        public JsonableObject NestProp { get; set; }
    }

    [ToJson, FromJson]
    public partial struct JsonableObject
    {
        public int Id { get; set; }
    }
}
