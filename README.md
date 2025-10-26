[![Jsonable](https://img.shields.io/nuget/vpre/SatorImaging.Jsonable?label=Jsonable)](https://www.nuget.org/packages/SatorImaging.Jsonable)
[![Assertions](https://img.shields.io/nuget/vpre/SatorImaging.Jsonable.Assertions?label=Assertions)](https://www.nuget.org/packages/SatorImaging.Jsonable.Assertions)
&nbsp;
[![build](https://github.com/sator-imaging/Jsonable/actions/workflows/build.yml/badge.svg)](https://github.com/sator-imaging/Jsonable/actions/workflows/build.yml)
&nbsp;
[![DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/sator-imaging/Jsonable)





**Jsonable** is a high-performance, source-generated JSON serialization library for C#.

> Unity 2022 or later is supported



## Key Benefits

- **High Performance**: Achieves superior serialization and deserialization speeds by generating code at compile-time, avoiding runtime reflection overhead.
- **Compile-Time Safety**: Catches serialization errors during compilation rather than at runtime, leading to more robust applications.
- **Reduced Memory Allocations**: Optimized code generation minimizes memory footprint.
- **Easy to Use**: Integrates seamlessly with existing C# projects and types.


### Performance Comparison

| MessagePack | Json.NET |
|:-----------:|:--------:|
| ![](https://raw.githubusercontent.com/sator-imaging/Jsonable/refs/heads/main/docs/Benchmark_MessagePack.png) | ![](https://raw.githubusercontent.com/sator-imaging/Jsonable/refs/heads/main/docs/Benchmark_JsonNET.png) |


<details>
<summary>Benchmark Details</summary>

Sample data courtesy of simdjson  
https://github.com/simdjson/simdjson/tree/master/jsonexamples


Benchmark Action  
https://github.com/sator-imaging/Jsonable/actions/workflows/benchmark-dotnet.yml

```md
BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
AMD EPYC 7763 3.24GHz, 1 CPU, 2 logical cores and 1 physical core
.NET SDK 10.0.100-rc.2.25502.107
  [Host]   : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3
  ShortRun : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

| Method                              | Boost | Mean       | Ratio | Allocated | Alloc Ratio |
|------------------------------------ |------ |-----------:|------:|----------:|------------:|
| Catalog_Load_FromJsonable           | 1     |  1.1248 ms |  1.00 |   0.55 MB |        1.00 |
| Catalog_Load_FromJsonableReuseArray | 1     |  1.0579 ms |  0.94 |   0.09 MB |        0.16 |
| Catalog_Load_FromJsonableReuseList  | 1     |  1.1136 ms |  0.99 |   0.54 MB |        0.98 |
| Catalog_Load_MsgPack                | 1     |  0.9083 ms |  0.81 |   0.55 MB |        1.00 |
| Catalog_Load_SysTxtJson             | 1     |  6.0844 ms |  5.41 |   1.15 MB |        2.08 |
| Catalog_Load_SysTxtJsonUtf8         | 1     |  9.2315 ms |  8.21 |   1.15 MB |        2.08 |
| Catalog_Load_JsonNET                | 1     |  9.6652 ms |  8.59 |   1.68 MB |        3.05 |
|                                     |       |            |       |           |             |
| Catalog_Load_FromJsonable           | 10    | 13.3936 ms |  1.00 |   5.38 MB |        1.00 |
| Catalog_Load_FromJsonableReuseArray | 10    | 10.5651 ms |  0.79 |    0.8 MB |        0.15 |
| Catalog_Load_FromJsonableReuseList  | 10    | 11.1568 ms |  0.83 |   5.31 MB |        0.99 |
| Catalog_Load_MsgPack                | 10    | 10.2718 ms |  0.77 |   5.38 MB |        1.00 |
| Catalog_Load_SysTxtJson             | 10    | 42.3042 ms |  3.16 |  11.31 MB |        2.10 |
| Catalog_Load_SysTxtJsonUtf8         | 10    | 55.1850 ms |  4.12 |  11.31 MB |        2.10 |
| Catalog_Load_JsonNET                | 10    | 88.7237 ms |  6.63 |  16.59 MB |        3.08 |
|                                     |       |            |       |           |             |
| Twitter_Load_FromJsonable           | 1     |  1.0553 ms |  1.00 |   0.55 MB |        1.00 |
| Twitter_Load_FromJsonableArray      | 1     |  1.0244 ms |  0.97 |   0.43 MB |        0.79 |
| Twitter_Load_FromJsonableReuseList  | 1     |  1.0356 ms |  0.98 |   0.55 MB |        1.00 |
| Twitter_Load_MsgPack                | 1     |  1.0251 ms |  0.97 |   0.49 MB |        0.90 |
| Twitter_Load_SysTxtJson             | 1     |  2.2296 ms |  2.11 |   0.54 MB |        0.99 |
| Twitter_Load_SysTxtJsonUtf8         | 1     |  3.0119 ms |  2.85 |   0.54 MB |        0.99 |
| Twitter_Load_JsonNET                | 1     |  3.5951 ms |  3.41 |   0.61 MB |        1.11 |
|                                     |       |            |       |           |             |
| Twitter_Load_FromJsonable           | 10    | 12.2055 ms |  1.00 |   5.49 MB |        1.00 |
| Twitter_Load_FromJsonableArray      | 10    | 10.3395 ms |  0.85 |   4.34 MB |        0.79 |
| Twitter_Load_FromJsonableReuseList  | 10    | 10.7002 ms |  0.88 |   5.48 MB |        1.00 |
| Twitter_Load_MsgPack                | 10    | 10.6102 ms |  0.87 |   4.93 MB |        0.90 |
| Twitter_Load_SysTxtJson             | 10    | 18.3473 ms |  1.50 |    5.4 MB |        0.98 |
| Twitter_Load_SysTxtJsonUtf8         | 10    | 22.8803 ms |  1.88 |    5.4 MB |        0.98 |
| Twitter_Load_JsonNET                | 10    | 32.3182 ms |  2.65 |   5.94 MB |        1.08 |
|                                     |       |            |       |           |             |
| Catalog_Save_ToJsonUtf8Cache        | 1     |  0.8461 ms |  1.00 |      0 MB |        1.00 |
| Catalog_Save_MsgPack                | 1     |  0.6116 ms |  0.72 |   0.33 MB |      891.99 |
| Catalog_Save_SysTxtJson             | 1     |  1.0361 ms |  1.22 |   0.96 MB |    2,610.21 |
| Catalog_Save_SysTxtJsonUtf8         | 1     |  0.7017 ms |  0.83 |         - |        0.00 |
| Catalog_Save_JsonNET                | 1     |  4.2194 ms |  4.99 |   2.57 MB |    7,018.90 |
| Catalog_Save_ToJsonUtf8             | 1     |  1.2484 ms |  1.48 |   1.72 MB |    4,694.95 |
| Catalog_Save_ToJsonable             | 1     |  1.9402 ms |  2.29 |   1.72 MB |    4,694.60 |
| Catalog_Save_ToJson                 | 1     |  1.4458 ms |  1.71 |   2.67 MB |    7,300.57 |
|                                     |       |            |       |           |             |
| Catalog_Save_ToJsonUtf8Cache        | 10    |  7.8977 ms |  1.00 |      0 MB |        1.00 |
| Catalog_Save_MsgPack                | 10    |  5.5379 ms |  0.70 |   3.24 MB |      884.18 |
| Catalog_Save_SysTxtJson             | 10    |  8.9662 ms |  1.14 |   9.49 MB |    2,588.65 |
| Catalog_Save_SysTxtJsonUtf8         | 10    |  6.9008 ms |  0.87 |         - |        0.00 |
| Catalog_Save_JsonNET                | 10    | 40.9896 ms |  5.19 |   25.5 MB |    6,954.97 |
| Catalog_Save_ToJsonUtf8             | 10    | 10.2352 ms |  1.30 |  13.75 MB |    3,751.89 |
| Catalog_Save_ToJsonable             | 10    | 18.0759 ms |  2.29 |   27.5 MB |    7,502.62 |
| Catalog_Save_ToJson                 | 10    | 11.6558 ms |  1.48 |  23.24 MB |    6,338.47 |
|                                     |       |            |       |           |             |
| Twitter_Save_ToJsonUtf8Cache        | 1     |  0.6341 ms |  1.00 |   0.28 MB |        1.00 |
| Twitter_Save_MsgPack                | 1     |  0.6194 ms |  0.98 |   0.39 MB |        1.40 |
| Twitter_Save_SysTxtJson             | 1     |  1.2403 ms |  1.96 |   1.11 MB |        4.00 |
| Twitter_Save_SysTxtJsonUtf8         | 1     |  0.7665 ms |  1.21 |         - |        0.00 |
| Twitter_Save_JsonNET                | 1     |  2.2399 ms |  3.53 |   1.74 MB |        6.27 |
| Twitter_Save_ToJsonUtf8             | 1     |  1.1562 ms |  1.82 |   1.73 MB |        6.24 |
| Twitter_Save_ToJsonable             | 1     |  1.3231 ms |  2.09 |   1.73 MB |        6.25 |
| Twitter_Save_ToJson                 | 1     |  1.5037 ms |  2.37 |   2.55 MB |        9.23 |
|                                     |       |            |       |           |             |
| Twitter_Save_ToJsonUtf8Cache        | 10    |  6.4475 ms |  1.00 |   2.77 MB |        1.00 |
| Twitter_Save_MsgPack                | 10    |  5.7555 ms |  0.89 |   3.88 MB |        1.40 |
| Twitter_Save_SysTxtJson             | 10    | 10.0937 ms |  1.57 |  11.06 MB |        4.00 |
| Twitter_Save_SysTxtJsonUtf8         | 10    |  7.4574 ms |  1.16 |         - |        0.00 |
| Twitter_Save_JsonNET                | 10    | 18.9737 ms |  2.94 |  17.25 MB |        6.23 |
| Twitter_Save_ToJsonUtf8             | 10    |  9.1961 ms |  1.43 |  14.37 MB |        5.19 |
| Twitter_Save_ToJsonable             | 10    | 11.0734 ms |  1.72 |  14.37 MB |        5.19 |
| Twitter_Save_ToJson                 | 10    | 12.4818 ms |  1.94 |  22.63 MB |        8.18 |
```

</details>





# Quick Start

```csharp
using Jsonable;

[ToJson]
[FromJson]
partial class MyData
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Program
{
    public static void Main()
    {
        var data = new MyData { Id = 1, Name = "Example Item" };

        // Serialize to JSON
        string json = data.ToJson(prettyPrint: true);
        Console.WriteLine($"Serialized: {json}");

        // Deserialize from JSON (.jsonc format is only supported)
        var jsonWithComments = data.ToJsonable();
        var deserialized = new MyData();
        deserialized.FromJsonable(jsonWithComments, reuseInstance: true);

        Console.WriteLine($"Deserialized Id: {deserialized.Id}, Name: {deserialized.Name}");
    }
}
```


## Serialization and Deserialization Callbacks

There are predefined `partial void` methods that will be invoked *if* it is implemented.

```cs
partial void OnWillSerialize();
partial void OnDidSerialize();

partial void OnWillDeserialize();
partial void OnDidDeserialize();
```





# Supported Types

## Collection Type Handling

Here shows `reuseInstance` strategies for `FromJsonable` method.
- `T[]`, `Base64`: reuse (overwrite) *if* array length matches
- `ICollection<T>`: not reused at all
- `List<T>`: add items
- `Dictionary`, `IDictionary`: overwrite if key exists otherwise add item

> [!TIP]
> Existing items in list or dictionary are not cleared automatically.
> To change behaviour, implement `OnWillDeserialize` callback that clears items before deserialize.



## Struct Types

### Primitive Structs (Roslyn `IsPrimitive` is true)
- `bool`, ~~`char`~~
- `byte`, `sbyte`
- `short`, `ushort`
- `int`, `uint`
- `long`, `ulong`
- `float`, `double`

### Other Supported Structs
- ~~`decimal`~~
- ~~`DateTime`~~
- `DateTimeOffset` (serialized as 'O' (ISO 8601) string; parsed time is always UTC)
- `TimeSpan` (serialized as total milliseconds as double)
- `Guid` (serialized as string in 'D' format)
- `Enum` (serialized as underlying integer value)

### Nullable Structs
- `System.Nullable<T>` where `T` is any supported struct type.



## Reference Types
- `string`
- `Uri` (serialized as string)
- `T[]`, `ICollection<T>` (serialized as JSON array)
    - `T` can be any supported type.
    - `byte[]` is stored as base64 string.
- `ICollection<KeyValuePair<TKey, TValue>>` (aka. Dictionary)
    - `TKey` must be `string`.
    - `TValue` can be any supported type.
    - Serialized as JSON object.
- Types decorated with `[ToJson]` attribute (recursively serialized)
