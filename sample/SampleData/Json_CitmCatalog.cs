using Jsonable;
using MessagePack;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CS1591   // Missing XML comment for publicly visible type or member

namespace Sample.SampleData
{
    [JsonSerializable(typeof(Json_CitmCatalog))]
    public partial class Json_CitmCatalog_STJ : JsonSerializerContext { }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_CitmCatalog
    {
        public Dictionary<string, string>? areaNames { get; set; }
        public Dictionary<string, string>? audienceSubCategoryNames { get; set; }
        public Blocknames? blockNames { get; set; }
        public Dictionary<string, EventData>? events { get; set; }
        public Performance[]? performances { get; set; }
        public Dictionary<string, string>? seatCategoryNames { get; set; }
        public Dictionary<string, string>? subTopicNames { get; set; }
        public Subjectnames? subjectNames { get; set; }
        public Dictionary<string, string>? topicNames { get; set; }
        public Dictionary<string, int[]>? topicSubTopics { get; set; }
        public Dictionary<string, string>? venueNames { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_CitmCatalogReusable
    {
        public Dictionary<string, string>? areaNames { get; set; }
        public Dictionary<string, string>? audienceSubCategoryNames { get; set; }
        public Blocknames? blockNames { get; set; }
        public Dictionary<string, EventData>? events { get; set; }
        public List<Performance>? performances { get; set; }
        public Dictionary<string, string>? seatCategoryNames { get; set; }
        public Dictionary<string, string>? subTopicNames { get; set; }
        public Subjectnames? subjectNames { get; set; }
        public Dictionary<string, string>? topicNames { get; set; }
        public Dictionary<string, int[]>? topicSubTopics { get; set; }
        public Dictionary<string, string>? venueNames { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)] public partial class Blocknames { }
    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)] public partial class Subjectnames { }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class EventData
    {
        public Json_Unknown? description { get; set; }
        public int id { get; set; }
        public string? logo { get; set; }
        public string? name { get; set; }
        public int[]? subTopicIds { get; set; }
        public Json_Unknown? subjectCode { get; set; }
        public Json_Unknown? subtitle { get; set; }
        public int[]? topicIds { get; set; }
    }


    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Performance
    {
        public int eventId { get; set; }
        public int id { get; set; }
        public string? logo { get; set; }
        public Json_Unknown? name { get; set; }
        public Performance__Prices[]? prices { get; set; }
        public Performance__Seatcategories[]? seatCategories { get; set; }
        public Json_Unknown? seatMapImage { get; set; }
        public long start { get; set; }
        public string? venueCode { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Performance__Prices
    {
        public int amount { get; set; }
        public int audienceSubCategoryId { get; set; }
        public int seatCategoryId { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Performance__Seatcategories
    {
        public Performance__Seatcategories__Areas[]? areas { get; set; }
        public int seatCategoryId { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Performance__Seatcategories__Areas
    {
        public int areaId { get; set; }
        public Json_Unknown[]? blockIds { get; set; }
    }
}
