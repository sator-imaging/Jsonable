[![Jsonable](https://img.shields.io/nuget/vpre/SatorImaging.Jsonable?label=Jsonable)](https://www.nuget.org/packages/SatorImaging.Jsonable)
[![Assertions](https://img.shields.io/nuget/vpre/SatorImaging.Jsonable.Assertions?label=Assertions)](https://www.nuget.org/packages/SatorImaging.Jsonable.Assertions)
&nbsp;
[![build](https://github.com/sator-imaging/Jsonable/actions/workflows/build.yml/badge.svg)](https://github.com/sator-imaging/Jsonable/actions/workflows/build.yml)





# Jsonable

A high-performance, source-generated JSON serialization library for C#.

> Unity 2022 or later is supported



## Benefits

- **High Performance**: Achieves superior serialization and deserialization speeds by generating code at compile-time, avoiding runtime reflection overhead.
- **Compile-Time Safety**: Catches serialization errors during compilation rather than at runtime, leading to more robust applications.
- **Reduced Memory Allocations**: Optimized code generation minimizes memory footprint.
- **Easy to Use**: Integrates seamlessly with existing C# projects and types.



## Quick Start

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



## Sample Data

Sample data courtesy of simdjson  
https://github.com/simdjson/simdjson/tree/master/jsonexamples
