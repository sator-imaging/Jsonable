using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Jsonable;
using Jsonable.Assertions;
using Newtonsoft.Json;
using Perfolizer.Horology;
using Perfolizer.Metrology;
using Sample;
using Sample.SampleData;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CA1861 // Avoid constant arrays as arguments

var options = args.ToList();

bool pretty_print = options.Remove("--pretty-print");
bool benchmark = options.Remove("--benchmark");
bool dryRun = options.Remove("--dry-run");
bool shortRun = options.Remove("--short-run");

_ = options.Remove("--");
_ = options.Remove("-c");
_ = options.Remove("--configuration");
_ = options.Remove("Debug");
_ = options.Remove("Release");

bool TEST = options.Count == 0 || options.Remove("--TEST");

// don't do short circuit
bool test_stringify = options.Remove("--test-stringify") || TEST;
bool test_comprehensive = options.Remove("--test-comprehensive") || TEST;
bool test_basic = options.Remove("--test-basic") || TEST;

if (options.Count > 0)
{
    throw new ArgumentException($"Unknown command line options: {string.Join(" ", options)}");
}


if (benchmark)
{
    BenchmarkRunner.Run(new Type[]
        {
            typeof(Benchmark_SaveLoad),
        },
        ManualConfig.CreateEmpty()
            .AddJob(
                dryRun
                    ? Job.Dry.WithIterationCount(3)
                    : shortRun
                        ? Job.ShortRun
                        : Job.MediumRun
            )
            // .AddExporter(MarkdownExporter.Default)
            // .AddExporter(HtmlExporter.Default)
            .WithOptions(ConfigOptions.DisableLogFile)
            .AddColumnProvider(DefaultColumnProviders.Instance)
            .AddLogger(ConsoleLogger.Default)
            .WithSummaryStyle(
                SummaryStyle.Default
                .WithTimeUnit(TimeUnit.Millisecond)
                .WithSizeUnit(SizeUnit.MB)
                )
    );

    return;
}


// json output comparison
if (test_stringify)
{
    var test = new AllSupportedFeatures<int, string>();

    Console.WriteLine();
    Console.WriteLine("= Default ===============================================================");
    Console.WriteLine(test.ToJson(pretty_print));
    Console.WriteLine("================================================================");

    test.FillValues(setNullToNullables: true);
    Console.WriteLine();
    Console.WriteLine("= Fill w/null ===============================================================");
    Console.WriteLine(test.ToJson(pretty_print));
    Console.WriteLine("================================================================");

    test.FillValues(setNullToNullables: false);
    Console.WriteLine();
    Console.WriteLine("= Fill w/o null ===============================================================");
    Console.WriteLine(test.ToJson(pretty_print));
    Console.WriteLine("================================================================");

    test.FillValues(setNullToNullables: true);
    Console.WriteLine();
    Console.WriteLine("= JMT Format w/null ===============================================================");
    Console.WriteLine(JSONABLE.Stringify(test.ToJsonable(), 2, ' ', "\n", pretty_print));
    Console.WriteLine("================================================================");

    test.FillValues(setNullToNullables: false);
    Console.WriteLine();
    Console.WriteLine("= JMT Format w/o null ===============================================================");
    Console.WriteLine(JSONABLE.Stringify(test.ToJsonable(), 2, ' ', "\n", pretty_print));
    Console.WriteLine("================================================================");
}


// sample data from simdjson
if (test_comprehensive)
{
    foreach (var (type, fileName) in new (Type, string)[]
    {
        (typeof(Json_Twitter), Benchmark_SaveLoad.TwitterFileNameNoExt + ".json"),
        (typeof(Json_CitmCatalog), Benchmark_SaveLoad.CatalogFileNameNoExt + ".json"),
    })
    {
        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
        var fileContent = File.ReadAllText(Benchmark_SaveLoad.SampleDataFolderSlash + fileName);

        var jobject = JsonConvert.DeserializeObject(fileContent, type);
        Must.BeTrue(jobject != null);

        var jsonFromJsonDotNet = JsonConvert.SerializeObject(jobject, pretty_print ? Formatting.Indented : Formatting.None);
        File.WriteAllText(Benchmark_SaveLoad.OutputFilePath + Benchmark_SaveLoad.JsonNetPrefix + fileName, jsonFromJsonDotNet);

        var jsonFromSysTxtJson = jobject switch
        {
            Json_Twitter x => JsonSerializer.Serialize(x, Json_Twitter_STJ.Default.Json_Twitter),
            Json_CitmCatalog x => JsonSerializer.Serialize(x, Json_CitmCatalog_STJ.Default.Json_CitmCatalog),
            _ => throw new Exception("must not be reached"),
        };
        File.WriteAllText(Benchmark_SaveLoad.OutputFilePath + Benchmark_SaveLoad.SysTxtJsonPrefix + fileName, jsonFromSysTxtJson);

        var jsonable = jobject switch
        {
            // Json.NET uses \r\n by default!!
            Json_Twitter x => x.ToJson(pretty_print, 2, ' ', "\r\n", false),
            Json_CitmCatalog x => x.ToJson(pretty_print, 2, ' ', "\r\n", false),
            _ => throw new Exception("must not be reached"),
        };
        var jsonableWithComments = jobject switch
        {
            Json_Twitter x => x.ToJsonable(),
            Json_CitmCatalog x => x.ToJsonable(),
            _ => throw new Exception("must not be reached"),
        };
        File.WriteAllText(Benchmark_SaveLoad.OutputFilePath + Benchmark_SaveLoad.JsonablePrefix + fileName, jsonable);
        File.WriteAllBytes(
            Benchmark_SaveLoad.OutputFilePath + Benchmark_SaveLoad.JsonableWithCommentsPrefix + fileNameNoExt + ".jsonc",
            jsonableWithComments.Span.ToArray());

        // roundtrip
        {
            if (jobject is Json_Twitter T)
            {
                var invalid = new Json_Twitter();
                invalid.FromJsonable(T.ToJsonable());
                invalid.statuses = Array.Empty<Status>();
                Must.Throw("FUnitImpl.FUnitException", null, () => Must.HaveEqualProperties(T, invalid));

                JsonableDebugger.VerifyRoundtrip(T);
            }
            else if (jobject is Json_CitmCatalog C)
            {
                var invalid = new Json_CitmCatalog();
                invalid.FromJsonable(C.ToJsonable());
                invalid.performances = Array.Empty<Performance>();
                Must.Throw("FUnitImpl.FUnitException", null, () => Must.HaveEqualProperties(C, invalid));

                JsonableDebugger.VerifyRoundtrip(C);
            }
            else
            {
                throw new Exception("must not be reached");
            }
        }

        // load failure check
        if (jobject is Json_Twitter invalidT)
        {
            var invalid = invalidT.ToJsonable();
            Must.BeTrue(new Json_CitmCatalog().FromJsonable(invalid) is < 0);
        }
        else if (jobject is Json_CitmCatalog invalidC)
        {
            var invalid = invalidC.ToJsonable();
            Must.BeTrue(new Json_Twitter().FromJsonable(invalid) is < 0);
        }
        else
        {
            throw new Exception("must not be reached");
        }
    }

    Console.WriteLine($"✔️ [{nameof(Jsonable)}] All comprehensive tests successfully completed");
}


// roundtrip of all supported features
if (test_basic)
{
    var test = new AllSupportedFeatures<int, string>();
    Must.BeEqual(0, test.CallCounts["OnWillSerialize"]);
    Must.BeEqual(0, test.CallCounts["OnDidSerialize"]);
    Must.BeEqual(0, test.CallCounts["OnWillDeserialize"]);
    Must.BeEqual(0, test.CallCounts["OnDidDeserialize"]);
    test.FromJsonable(test.ToJsonable());
    Must.BeEqual(1, test.CallCounts["OnWillSerialize"]);
    Must.BeEqual(1, test.CallCounts["OnDidSerialize"]);
    Must.BeEqual(1, test.CallCounts["OnWillDeserialize"]);
    Must.BeEqual(1, test.CallCounts["OnDidDeserialize"]);
    test.ToJson();
    Must.BeEqual(2, test.CallCounts["OnWillSerialize"]);
    Must.BeEqual(2, test.CallCounts["OnDidSerialize"]);
    test.FromJsonable(test.ToJsonable());
    Must.BeEqual(3, test.CallCounts["OnWillSerialize"]);
    Must.BeEqual(3, test.CallCounts["OnDidSerialize"]);
    Must.BeEqual(2, test.CallCounts["OnWillDeserialize"]);
    Must.BeEqual(2, test.CallCounts["OnDidDeserialize"]);

    JsonableDebugger.VerifyRoundtrip(test, logger: Console.WriteLine);

    test.FillValues(setNullToNullables: true);
    JsonableDebugger.VerifyRoundtrip(test, logger: Console.WriteLine);

    test.FillValues(setNullToNullables: false);
    JsonableDebugger.VerifyRoundtrip(test, logger: Console.WriteLine);

    // nullable element array checks
    var readback = new AllSupportedFeatures<int, string>();
    readback.FromJsonable(test.ToJsonable());
    Must.BeTrue(test.NullableRefTypeElementArrayProp.Length > 0);
    Must.HaveSameSequence(test.NullableRefTypeElementArrayProp, readback.NullableRefTypeElementArrayProp);
    Must.BeTrue(test.NullableRefTypeElementArrayNull?.Length > 0);
    Must.HaveSameSequence(test.NullableRefTypeElementArrayNull!, readback.NullableRefTypeElementArrayNull!);
    Must.BeTrue(test.NullableValueTypeElementArrayProp.Length > 0);
    Must.HaveSameSequence(test.NullableValueTypeElementArrayProp, readback.NullableValueTypeElementArrayProp);
    Must.BeTrue(test.NullableValueTypeElementArrayNull?.Length > 0);
    Must.HaveSameSequence(test.NullableValueTypeElementArrayNull!, readback.NullableValueTypeElementArrayNull!);

    // caching
    {
        var cache = new ArrayBufferWriter<byte>();

        test.ToJsonable(cache);
        var first = cache.WrittenSpan.ToArray();

        cache.Clear();

        test.ToJsonable(cache);
        var second = cache.WrittenSpan.ToArray();

        Must.HaveSameSequence(first, second);

        Console.WriteLine($"✔️ [{nameof(Jsonable)}] Caching test successfully completed");
    }

    // reuseInstance
    {
        var expected = new AllSupportedFeatures<int, float>();
        expected.FillValues(setNullToNullables: true);

        var reuse = new AllSupportedFeatures<int, float>();
        reuse.FillValues(setNullToNullables: true);


        // base64
        var old_base64 = reuse.Base64Prop;
        old_base64.AsSpan().Fill(byte.MaxValue);
        reuse.FromJsonable(expected.ToJsonable());
        Must.NotBeSameReference(old_base64, reuse.Base64Prop);
        Must.HaveSameSequence(expected.Base64Prop, reuse.Base64Prop);

        old_base64 = reuse.Base64Prop;
        old_base64.AsSpan().Fill(byte.MaxValue);
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_base64, reuse.Base64Prop);
        Must.HaveSameSequence(expected.Base64Prop, reuse.Base64Prop);


        // T[]
        var old_array = reuse.ArrayProp;
        old_array.AsSpan().Fill(int.MaxValue);
        reuse.FromJsonable(expected.ToJsonable());
        Must.NotBeSameReference(old_array, reuse.ArrayProp);
        Must.HaveSameSequence(expected.ArrayProp, reuse.ArrayProp);

        old_array = reuse.ArrayProp;
        old_array.AsSpan().Fill(int.MaxValue);
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_array, reuse.ArrayProp);
        Must.HaveSameSequence(expected.ArrayProp, reuse.ArrayProp);


        // List<T>
        var old_list = reuse.ListProp;
        old_list.Clear();
        reuse.FromJsonable(expected.ToJsonable());
        Must.NotBeSameReference(old_list, reuse.ListProp);
        Must.HaveSameSequence(expected.ListProp, reuse.ListProp);

        old_list = reuse.ListProp;
        old_list.Clear();
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_list, reuse.ListProp);
        Must.HaveSameSequence(expected.ListProp, reuse.ListProp);
        // add
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_list, reuse.ListProp);
        Must.HaveSameSequence(expected.ListProp.Concat(expected.ListProp), reuse.ListProp);
        // refresh
        reuse.FromJsonable(expected.ToJsonable());
        Must.NotBeSameReference(old_list, reuse.ListProp);
        Must.HaveSameSequence(expected.ListProp, reuse.ListProp);


        // Dictionary
        var old_dict = reuse.DictProp;
        old_dict.Clear();
        reuse.FromJsonable(expected.ToJsonable());
        Must.NotBeSameReference(old_dict, reuse.DictProp);
        Must.HaveSameSequence(expected.DictProp, reuse.DictProp);

        old_dict = reuse.DictProp;
        old_dict.Clear();
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_dict, reuse.DictProp);
        Must.HaveSameSequence(expected.DictProp, reuse.DictProp);
        // add (no change)
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.BeSameReference(old_dict, reuse.DictProp);
        Must.HaveSameSequence(expected.DictProp, reuse.DictProp);
        // add
        const string KEY = "=== ABC XYZ ===";
        const float VALUE = 805684.091585f;
        expected.DictProp.Add(KEY, VALUE);
        {
            Must.NotHaveSameSequence(expected.DictProp, reuse.DictProp);
            reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
            Must.HaveSameSequence(expected.DictProp, reuse.DictProp);
            Must.BeSameReference(old_dict, reuse.DictProp);
        }
        expected.DictProp.Remove(KEY);
        // refresh
        reuse.FromJsonable(expected.ToJsonable());
        Must.HaveSameSequence(expected.DictProp, reuse.DictProp);
        Must.NotBeSameReference(old_dict, reuse.DictProp);
        // update
        old_dict = reuse.DictProp;
        foreach (var key in expected.DictProp.Keys.ToList())
        {
            expected.DictProp[key] = VALUE;
        }
        Must.NotHaveSameSequence(expected.DictProp, reuse.DictProp);
        reuse.FromJsonable(expected.ToJsonable(), reuseInstance: true);
        Must.HaveSameSequence(expected.DictProp, reuse.DictProp);
        Must.BeSameReference(old_dict, reuse.DictProp);
    }

    // other
    JsonableDebugger.VerifyRoundtrip(new SwitchCases());

    JsonableDebugger.VerifyRoundtrip(new Empty());
    JsonableDebugger.VerifyRoundtrip(new EmptyInEmpty());
    JsonableDebugger.VerifyRoundtrip(new EndsWithBasicProperty());
    JsonableDebugger.VerifyRoundtrip(new EndsWithObject());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArray());
    JsonableDebugger.VerifyRoundtrip(new EndsWithMap());
    JsonableDebugger.VerifyRoundtrip(new NestObjectEnd());
    JsonableDebugger.VerifyRoundtrip(new NestArrayEnd());
    JsonableDebugger.VerifyRoundtrip(new NestMapEnd());
    JsonableDebugger.VerifyRoundtrip(new EndsWithMapOfMap());
    JsonableDebugger.VerifyRoundtrip(new EndsWithMapOfArray());
    JsonableDebugger.VerifyRoundtrip(new EndsWithMapOfObject());
    JsonableDebugger.VerifyRoundtrip(new EndsWithMapOfArrayOfObject());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfMap());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfObject());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfArray());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfMapOfMap());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfMapOfArray());
    JsonableDebugger.VerifyRoundtrip(new EndsWithArrayOfList());
    // Json.NET doesn't correctly deserialize complex list
    JsonableDebugger.VerifyRoundtrip(new EndsWithListOfList(), skipJsonDotNetTests: true);
    {
        var expected = new EndsWithListOfList();
        var actual = new EndsWithListOfList();
        actual.ListOfListProp = new();
        actual.ListOfListProp2 = new();
        Must.Throw("FUnitImpl.FUnitException", null, () => Must.HaveEqualProperties(expected, actual));

        actual.FromJsonable(expected.ToJsonable());
        Must.HaveEqualProperties(expected, actual);
        Must.BeEqual(expected.ListOfListProp.Count, actual.ListOfListProp.Count);
        Must.HaveSameSequence(expected.ListOfListProp[0], actual.ListOfListProp[0]);
        Must.HaveSameSequence(expected.ListOfListProp[1], actual.ListOfListProp[1]);
        Must.HaveSameSequence(expected.ListOfListProp[2], actual.ListOfListProp[2]);
        Must.HaveSameSequence(expected.ListOfListProp[3], actual.ListOfListProp[3]);
        Must.BeEqual(expected.ListOfListProp2.Count, actual.ListOfListProp2.Count);
        Must.HaveSameSequence(expected.ListOfListProp2[0], actual.ListOfListProp2[0]);
        Must.HaveSameSequence(expected.ListOfListProp2[1], actual.ListOfListProp2[1]);
        Must.HaveSameSequence(expected.ListOfListProp2[2], actual.ListOfListProp2[2]);
        Must.HaveSameSequence(expected.ListOfListProp2[3], actual.ListOfListProp2[3]);
    }
    // Json.NET doesn't correctly deserialize complex list
    JsonableDebugger.VerifyRoundtrip(new EndsWithListOfArray(), skipJsonDotNetTests: true);
    {
        var expected = new EndsWithListOfArray();
        var actual = new EndsWithListOfArray();
        actual.ListOfArrayProp = new();
        actual.ListOfArrayProp2 = new();
        Must.Throw("FUnitImpl.FUnitException", null, () => Must.HaveEqualProperties(expected, actual));

        actual.FromJsonable(expected.ToJsonable());
        Must.HaveEqualProperties(expected, actual);
        Must.BeEqual(expected.ListOfArrayProp.Count, actual.ListOfArrayProp.Count);
        Must.HaveSameSequence(expected.ListOfArrayProp[0], actual.ListOfArrayProp[0]);
        Must.HaveSameSequence(expected.ListOfArrayProp[1], actual.ListOfArrayProp[1]);
        Must.HaveSameSequence(expected.ListOfArrayProp[2], actual.ListOfArrayProp[2]);
        Must.HaveSameSequence(expected.ListOfArrayProp[3], actual.ListOfArrayProp[3]);
        Must.BeEqual(expected.ListOfArrayProp2.Count, actual.ListOfArrayProp2.Count);
        Must.HaveSameSequence(expected.ListOfArrayProp2[0], actual.ListOfArrayProp2[0]);
        Must.HaveSameSequence(expected.ListOfArrayProp2[1], actual.ListOfArrayProp2[1]);
        Must.HaveSameSequence(expected.ListOfArrayProp2[2], actual.ListOfArrayProp2[2]);
        Must.HaveSameSequence(expected.ListOfArrayProp2[3], actual.ListOfArrayProp2[3]);
    }

    var base64Limit = new Base64Limit();
    base64Limit.Base64Prop = new byte[JSONABLE.Base64MaxLength];
    _ = base64Limit.ToJsonable();
    base64Limit.Base64Prop = new byte[JSONABLE.Base64MaxLength + 1];
    Must.Throw<JsonableException>(
        "Failed to write one or more values, non-nullable type has null, or maybe insufficient buffer space.",
        () => base64Limit.ToJsonable());


    Console.WriteLine();
    Console.WriteLine($"✔️ [{nameof(Jsonable)}] All basic compatibility tests successfully completed");
}


// partial read/write
{
    // plan A
    {
        var writer = new ArrayBufferWriter<byte>();
        var exporter = new Composite();

        var comp = new Composite();
        Must.BeTrue(null == comp.Position);
        Must.BeTrue(null == comp.Rotation);
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        Console.WriteLine();
        Console.WriteLine(comp);

        exporter.Position = new float[] { 1.1f, 2.2f, 3.3f };
        exporter.ToJsonUtf8_Position(writer);
        Must.BeTrue(0 <= comp.FromJsonable(writer.WrittenMemory));
        writer.Clear();
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.BeTrue(null == comp.Rotation);
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        exporter.Rotation = new float[] { 11.11f, 22.22f, 33.33f };
        exporter.ToJsonUtf8_Rotation(writer);
        Must.BeTrue(0 <= comp.FromJsonable(writer.WrittenMemory));
        writer.Clear();
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        exporter.Scale = new float[] { 111.111f, 222.222f, 333.333f };
        exporter.ToJsonUtf8_Scale(writer);
        Must.BeTrue(0 <= comp.FromJsonable(writer.WrittenMemory));
        writer.Clear();
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeTrue(null == comp.Payload);

        exporter.Payload = new(-1, "cleared");
        exporter.ToJsonUtf8_Payload(writer);
        Must.BeTrue(0 <= comp.FromJsonable(writer.WrittenMemory));
        writer.Clear();
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeEqual(-1, comp.Payload?.Id);
        Must.BeEqual("cleared", comp.Payload?.Name);

        Must.BeTrue(0 <= comp.FromJsonable(comp.ToJsonable()));
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeEqual(-1, comp.Payload?.Id);
        Must.BeEqual("cleared", comp.Payload?.Name);

        Console.WriteLine(comp);
    }

    // plan B
    {
        var comp = new Composite();
        Must.BeTrue(null == comp.Position);
        Must.BeTrue(null == comp.Rotation);
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        Console.WriteLine();
        Console.WriteLine(comp);

        comp.FromJsonable(new PositionOnly(new float[] { 1.1f, 2.2f, 3.3f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.BeTrue(null == comp.Rotation);
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        comp.FromJsonable(new RotationOnly(new float[] { 11.11f, 22.22f, 33.33f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.BeTrue(null == comp.Scale);
        Must.BeTrue(null == comp.Payload);

        comp.FromJsonable(new ScaleOnly(new float[] { 111.111f, 222.222f, 333.333f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeTrue(null == comp.Payload);

        comp.FromJsonable(new PayloadOnly(new(-1, "cleared")).ToJsonable());
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeEqual(-1, comp.Payload?.Id);
        Must.BeEqual("cleared", comp.Payload?.Name);

        comp.FromJsonable(comp.ToJsonable());
        Must.HaveSameSequence(new float[] { 1.1f, 2.2f, 3.3f }, (((comp.Position!))));
        Must.HaveSameSequence(new float[] { 11.11f, 22.22f, 33.33f }, (((comp.Rotation!))));
        Must.HaveSameSequence(new float[] { 111.111f, 222.222f, 333.333f }, (((comp.Scale!))));
        Must.BeEqual(-1, comp.Payload?.Id);
        Must.BeEqual("cleared", comp.Payload?.Name);

        var posOnly = new PositionOnly();
        posOnly.FromJsonable(new PositionOnly(new float[] { 1f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 1f }, posOnly.Position);

        var rotOnly = new RotationOnly();
        rotOnly.FromJsonable(new RotationOnly(new float[] { 2f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 2f }, rotOnly.Rotation);

        var scaleOnly = new ScaleOnly();
        scaleOnly.FromJsonable(new ScaleOnly(new float[] { 3f }).ToJsonable());
        Must.HaveSameSequence(new float[] { 3f }, scaleOnly.Scale);

        var payloadOnly = new PayloadOnly();
        payloadOnly.FromJsonable(new PayloadOnly(new(1, "test")).ToJsonable());
        Must.BeEqual(1, payloadOnly.Payload.Id);
        Must.BeEqual("test", payloadOnly.Payload.Name);

        Console.WriteLine(comp);
    }

    Console.WriteLine();
    Console.WriteLine($"✔️ [{nameof(Jsonable)}] All partial read/write tests successfully completed");
}


Console.WriteLine();
Console.WriteLine($"Base64 max length: {JSONABLE.Base64MaxLength:#,0}");

Console.WriteLine();
Console.WriteLine($"✅ [{nameof(Jsonable)}] All tests successfully completed");
