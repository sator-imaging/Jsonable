using Jsonable;
using System;
using System.Collections.Generic;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CA1852  // Seal internal types

namespace Tests
{
    [FromJson]
    [ToJson]
    partial class JsonableObject { }


    [FromJson]
    [ToJson]
    partial class SwitchCases  // Covers all possible switch cases!
    {
        public enum MyEnum { Default, Value }

        // struct (same nullable generation code path is used)
        public bool Bool { get; set; }
        public int Primitive { get; set; }
        public int? NullableT { get; set; }
        public MyEnum EnumProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public TimeSpan TimeSpanProp { get; set; }
        public Guid GuidProp { get; set; }

        // ref types
        public string String { get; set; } = "";
        public string? NullableString { get; set; }
        public Uri UriProp { get; set; } = new("http://127.0.0.1/abc");
        public Uri? NullableUriProp { get; set; }
        public byte[] Base64 { get; set; } = Array.Empty<byte>();
        public byte[]? NullableBase64 { get; set; }
        public ulong[] ArrayProp { get; set; } = Array.Empty<ulong>();
        public ulong[]? NullableArrayProp { get; set; }
        public List<int> Collection { get; set; } = new();
        public List<int>? NullableCollection { get; set; }
        public Dictionary<string, int> Map { get; set; } = new();
        public Dictionary<string, int>? NullableMap { get; set; }
        public JsonableObject FromToJsonableProp { get; set; } = new();
        public JsonableObject? NullableFromToJsonableProp { get; set; }
    }
}
