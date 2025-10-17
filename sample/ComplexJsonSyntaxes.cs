using System.Collections.Generic;
using Jsonable;

#pragma warning disable CA1861  // Avoid constant arrays as arguments
#pragma warning disable CA1852  // Seal internal types

namespace Sample
{
    [FromJson, ToJson] partial class Empty { }
    [FromJson, ToJson] partial class EmptyInEmpty { public Empty EmptyProp { get; set; } = new(); }

    // json ending condition tests: ex: ]}}, ]}], etc.
    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithBasicProperty
    {
        public string StringProp { get; set; } = "A";
        public string StringProp2 { get; set; } = "B";
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithObject
    {
        public EndsWithBasicProperty ObjectProp { get; set; } = new();
        public EndsWithBasicProperty ObjectProp2 { get; set; } = new();
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArray
    {
        public int[] ArrayProp { get; set; } = new int[] { 11, 22, 33 };
        public int[] ArrayProp2 { get; set; } = new int[] { 44, 55, 66 };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithMap
    {
        public Dictionary<string, int> MapProp { get; set; } = new() { { "A", 1 }, { "B", 2 } };
        public Dictionary<string, int> MapProp2 { get; set; } = new() { { "C", 3 }, { "D", 4 } };
    }


    // nest
    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class NestObjectEnd
    {
        public EndsWithObject ObjectProp { get; set; } = new();
        public EndsWithObject ObjectProp2 { get; set; } = new();
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class NestArrayEnd
    {
        public EndsWithArray ArrayProp { get; set; } = new();
        public EndsWithArray ArrayProp2 { get; set; } = new();
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class NestMapEnd
    {
        public EndsWithMap MapProp { get; set; } = new();
        public EndsWithMap MapProp2 { get; set; } = new();
    }


    // complex
    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithMapOfMap
    {
        public Dictionary<string, Dictionary<string, int>> MapOfMapProp { get; set; } = new()
        {
            { "A", new() { { "A1", 1 }, { "A2", 2 } } },
            { "B", new() { { "B1",-1 }, { "B2",-2 } } },
        };
        public Dictionary<string, Dictionary<string, int>> MapOfMapProp2 { get; set; } = new()
        {
            { "C", new() { { "C1", 3 }, { "C2", 4 } } },
            { "D", new() { { "D1",-3 }, { "D2",-4 } } },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithMapOfArray
    {
        public Dictionary<string, int[]> MapOfArrayProp { get; set; } = new Dictionary<string, int[]>
        {
            { "A1", new[] { 1, 2 } }, { "A2", new[] { 11, 22 } },
            { "B1", new[] { 3, 4 } }, { "B2", new[] { 33, 44 } },
        };
        public Dictionary<string, int[]> MapOfArrayProp2 { get; set; } = new Dictionary<string, int[]>
        {
            { "C1", new[] { 111, 222 } }, { "C2", new[] { 1111, 2222 } },
            { "D1", new[] { 333, 444 } }, { "D2", new[] { 3333, 4444 } },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithMapOfObject
    {
        public Dictionary<string, EndsWithBasicProperty> MapOfObjectProp { get; set; } = new()
        {
            { "A1", new() }, { "A2", new() },
            { "B1", new() }, { "B2", new() },
        };
        public Dictionary<string, EndsWithBasicProperty> MapOfObjectProp2 { get; set; } = new()
        {
            { "C1", new() }, { "C2", new() },
            { "D1", new() }, { "D2", new() },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithMapOfArrayOfObject
    {
        public Dictionary<string, EndsWithBasicProperty[]> MapOfArrayOfObjectProp { get; set; } = new()
        {
            { "A1", new EndsWithBasicProperty[] { new(), new() } }, { "A2", new EndsWithBasicProperty[] { new(), new() } },
            { "B1", new EndsWithBasicProperty[] { new(), new() } }, { "B2", new EndsWithBasicProperty[] { new(), new() } },
        };
        public Dictionary<string, EndsWithBasicProperty[]> MapOfArrayOfObjectProp2 { get; set; } = new()
        {
            { "C1", new EndsWithBasicProperty[] { new(), new() } }, { "C2", new EndsWithBasicProperty[] { new(), new() } },
            { "D1", new EndsWithBasicProperty[] { new(), new() } }, { "D2", new EndsWithBasicProperty[] { new(), new() } },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfMap
    {
        public Dictionary<string, int>[] ArrayOfMapProp { get; set; } = new Dictionary<string, int>[]
        {
            new () { { "A1", 1 }, { "A2", 2 } },
            new () { { "B1",-1 }, { "B2",-2 } },
        };
        public Dictionary<string, int>[] ArrayOfMapProp2 { get; set; } = new Dictionary<string, int>[]
        {
            new () { { "C1", 3 }, { "C2", 4 } },
            new () { { "D1",-3 }, { "D2",-4 } },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfObject
    {
        public EndsWithBasicProperty[] ArrayOfObjectProp { get; set; } = new EndsWithBasicProperty[]
        {
            new () { StringProp = "A1", StringProp2 = "A2" },
            new () { StringProp = "B1", StringProp2 = "B2" },
        };
        public EndsWithBasicProperty[] ArrayOfObjectProp2 { get; set; } = new EndsWithBasicProperty[]
        {
            new () { StringProp = "C1", StringProp2 = "C2" },
            new () { StringProp = "D1", StringProp2 = "D2" },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfArray
    {
        public int[][] ArrayOfArrayProp { get; set; } = new int[][]
        {
            new[]{ 1, 2 },
            new[]{ 3, 4 },
        };
        public int[][] ArrayOfArrayProp2 { get; set; } = new int[][]
        {
            new[]{ 5, 6 },
            new[]{ 7, 8 },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfMapOfMap
    {
        public Dictionary<string, Dictionary<string, int>>[] ArrayOfMapOfMapProp { get; set; } = new Dictionary<string, Dictionary<string, int>>[]
        {
            new () { { "A1", new() { { "A1a", -1 }, { "A1b", -2 } } }, { "A2", new() { { "A2a", -3 }, { "A2b", -4 } } } },
            new () { { "B1", new() { { "B1a", -5 }, { "B1b", -6 } } }, { "B2", new() { { "B2a", -7 }, { "B2b", -8 } } } },
        };
        public Dictionary<string, Dictionary<string, int>>[] ArrayOfMapOfMapProp2 { get; set; } = new Dictionary<string, Dictionary<string, int>>[]
        {
            new () { { "C1", new() { { "C1a", -11 }, { "C1b", -22 } } }, { "C2", new() { { "C2a", -33 }, { "C2b", -44 } } } },
            new () { { "D1", new() { { "D1a", -55 }, { "D1b", -66 } } }, { "D2", new() { { "D2a", -77 }, { "D2b", -88 } } } },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfMapOfArray
    {
        public Dictionary<string, int[]>[] ArrayOfMapOfArrayProp { get; set; } = new Dictionary<string, int[]>[]
        {
            new () { { "A1", new[] { 1, 2 } }, { "A2", new[] { 3, 4 } } },
            new () { { "B1", new[] { 5, 6 } }, { "B2", new[] { 7, 8 } } },
        };
        public Dictionary<string, int[]>[] ArrayOfMapOfArrayProp2 { get; set; } = new Dictionary<string, int[]>[]
        {
            new () { { "C1", new[] { -1, -2 } }, { "C2", new[] { -3, -4 } } },
            new () { { "D1", new[] { -5, -6 } }, { "D2", new[] { -7, -8 } } },
        };
    }


    // collection type variants
    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithArrayOfList
    {
        public List<int>[] ArrayOfListProp { get; set; } = new List<int>[]
        {
            new(){ 1, 2, 3, 4 },
            new(){ 3, 4, 5, 6 },
        };
        public List<int>[] ArrayOfListProp2 { get; set; } = new List<int>[]
        {
            new(){ 5, 6, 7, 8 },
            new(){ 7, 8, 9, 10 },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithListOfList
    {
        public List<List<int>> ListOfListProp { get; set; } = new List<List<int>>()
        {
            new(){ 1, 2, 3, 4 },
            new(){ 3, 4, 5, 6 },
            new(){ 5, 6, 7, 8 },
            new(){ 7, 8, 9, 10 },
        };
        public List<List<int>> ListOfListProp2 { get; set; } = new List<List<int>>()
        {
            new(){ 7, 8, 9, 10 },
            new(){ 5, 6, 7, 8 },
            new(){ 3, 4, 5, 6 },
            new(){ 1, 2, 3, 4 },
        };
    }

    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class EndsWithListOfArray
    {
        public List<int[]> ListOfArrayProp { get; set; } = new List<int[]>()
        {
            new[]{ 1, 2 },
            new[]{ 3, 4 },
            new[]{ 5, 6 },
            new[]{ 7, 8 },
        };
        public List<int[]> ListOfArrayProp2 { get; set; } = new List<int[]>()
        {
            new[]{ 7, 8 },
            new[]{ 5, 6 },
            new[]{ 3, 4 },
            new[]{ 1, 2 },
        };
    }
}
