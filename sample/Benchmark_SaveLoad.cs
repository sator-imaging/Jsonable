using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Jsonable.Assertions;
using MessagePack;
using Newtonsoft.Json;
using Sample.SampleData;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CA1822  // Mark members as static
#pragma warning disable IDE2001  // Embedded statements must be on their own line
#pragma warning disable CA1805  // Do not initialize unnecessarily
#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace Sample
{
    [HideColumns(Column.Gen0, Column.Gen1, Column.Gen2, Column.Error, Column.Median, Column.StdDev, Column.RatioSD)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [MemoryDiagnoser]
    public class Benchmark_SaveLoad
    {
        const string BDN_PathFix = "../../../../../../../../";
        const int MinJsonLength = 400_000;

        public const string SampleDataFolderSlash = "./sample/SampleData/";
        public const string OutputFilePath = "./z_TEST_";
        public const string TwitterFileNameNoExt = "twitter";
        public const string CatalogFileNameNoExt = "citm_catalog";
        public const string JsonNetPrefix = "JsonNET_";
        public const string SysTxtJsonPrefix = "SysTxtJson_";
        public const string JsonablePrefix = "Jsonable_";
        public const string JsonableWithCommentsPrefix = "WithComments_";

        private Json_Twitter Twitter = (((default)))!;
        private Json_CitmCatalog Catalog = (((default)))!;

        private Json_TwitterReusable TwitterReusable = (((default)))!;
        private Json_CitmCatalogReusable CatalogReusable = (((default)))!;

        private string TwitterJsonSource = (((default)))!;
        private string CatalogJsonSource = (((default)))!;

        private ReadOnlyMemory<byte> TwitterJsonUtf8;
        private ReadOnlyMemory<byte> CatalogJsonUtf8;

        private ReadOnlyMemory<byte> TwitterWithComments;
        private ReadOnlyMemory<byte> CatalogWithComments;

        private ReadOnlyMemory<byte> TwitterMessagePack;
        private ReadOnlyMemory<byte> CatalogMessagePack;

        [Params(1, 10)]
        public int Boost { get; set; }

        [GlobalSetup]
        public void Initialize()
        {
            if (Twitter != null &&
                Catalog != null &&
                TwitterJsonSource != null &&
                CatalogJsonSource != null &&
                TwitterWithComments.Length > 0 &&
                CatalogWithComments.Length > 0)
            {
                return;
            }

            TwitterJsonSource = File.ReadAllText(BDN_PathFix + SampleDataFolderSlash + TwitterFileNameNoExt + ".json");
            CatalogJsonSource = File.ReadAllText(BDN_PathFix + SampleDataFolderSlash + CatalogFileNameNoExt + ".json");

            Twitter = JsonConvert.DeserializeObject<Json_Twitter>(TwitterJsonSource)
                ?? throw new Exception("must not be reached");

            Catalog = JsonConvert.DeserializeObject<Json_CitmCatalog>(CatalogJsonSource)
                ?? throw new Exception("must not be reached");

            if (Twitter.ToJson().Length < MinJsonLength ||
                Catalog.ToJson().Length < MinJsonLength)
            {
                throw new Exception($"something went wrong: {Twitter.ToJson().Length:#,0} / {Catalog.ToJson().Length:#,0}");
            }

            if (Boost > 1)
            {
                if (Twitter.statuses != null)
                {
                    IEnumerable<Status> statuses = Twitter.statuses;
                    for (int i = 1; i < Boost; i++)
                    {
                        statuses = statuses.Concat(Twitter.statuses);
                    }
                    Twitter.statuses = statuses.ToArray();

                    TwitterJsonSource = JsonConvert.SerializeObject(Twitter);
                }

                if (Catalog.performances != null && Catalog.events != null)
                {
                    IEnumerable<Performance> performances = Catalog.performances;
                    IEnumerable<EventData> events = Catalog.events.Values;

                    for (int i = 1; i < Boost; i++)
                    {
                        performances = performances.Concat(Catalog.performances);
                        events = events.Concat(Catalog.events.Values);
                    }
                    Catalog.performances = performances.ToArray();
                    int key = 1234567890;
                    Catalog.events = events.ToDictionary(_ => (key++).ToString(), x => x);

                    CatalogJsonSource = JsonConvert.SerializeObject(Catalog);
                }
            }


            TwitterWithComments = Twitter.ToJsonable();
            CatalogWithComments = Catalog.ToJsonable();

            cache_utf8Writer = new Utf8JsonWriter(cache_memoryStream, new JsonWriterOptions { SkipValidation = true });
            TwitterJsonUtf8 = Encoding.UTF8.GetBytes(TwitterJsonSource);
            CatalogJsonUtf8 = Encoding.UTF8.GetBytes(CatalogJsonSource);

            TwitterMessagePack = MessagePackSerializer.Serialize(Twitter);
            CatalogMessagePack = MessagePackSerializer.Serialize(Catalog);


            // reusable
            TwitterReusable = JsonConvert.DeserializeObject<Json_TwitterReusable>(JsonConvert.SerializeObject(Twitter))
                ?? throw new Exception("must not be reached");

            CatalogReusable = JsonConvert.DeserializeObject<Json_CitmCatalogReusable>(JsonConvert.SerializeObject(Catalog))
                ?? throw new Exception("must not be reached");

            TwitterReusable.statuses?.Clear();
            TwitterReusable.FromJsonable(TwitterWithComments, reuseInstance: true);
            TwitterReusable.statuses?.Clear();
            TwitterReusable.FromJsonable(TwitterWithComments, reuseInstance: true);
            JsonableDebugger.AssertPropertiesEqual<IEnumerable<Status>>((((Twitter.statuses)))!, (((TwitterReusable.statuses)))!);
            JsonableDebugger.AssertPropertiesEqual<IEnumerable<Performance>>((((Catalog.performances)))!, (((CatalogReusable.performances)))!);


            // verify
            {
                var jsonableTwitter = new Json_Twitter();
                jsonableTwitter.FromJsonable(TwitterWithComments);
                Must.HaveEqualProperties(Twitter, jsonableTwitter);

                var jsonableCatalog = new Json_CitmCatalog();
                jsonableCatalog.FromJsonable(CatalogWithComments);
                Must.HaveEqualProperties(Catalog, jsonableCatalog);


                var msgpackTwitter = MessagePackSerializer.Deserialize<Json_Twitter>(TwitterMessagePack);
                Must.HaveEqualProperties(Twitter, msgpackTwitter);

                var msgpackCatalog = MessagePackSerializer.Deserialize<Json_CitmCatalog>(CatalogMessagePack);
                Must.HaveEqualProperties(Catalog, msgpackCatalog);


                // var simpleJsonTwitter = new Json_Twitter();
                // JSON.TryParse(simpleJsonTwitter, TwitterWithComments);
                // Must.HaveEqualProperties(Twitter, simpleJsonTwitter);

                // var simpleJsonCatalog = new Json_CitmCatalog();
                // JSON.TryParse(simpleJsonCatalog, CatalogWithComments);
                // Must.HaveEqualProperties(Catalog, simpleJsonCatalog);
            }

            //// write!
            //File.WriteAllText(OutputFilePath + JsonNetPrefix + TwitterFileNameNoExt + ".json", JsonConvert.SerializeObject(Twitter));
            //File.WriteAllText(OutputFilePath + JsonNetPrefix + CatalogFileNameNoExt + ".json", JsonConvert.SerializeObject(Catalog));
            //File.WriteAllText(OutputFilePath + JsonablePrefix + TwitterFileNameNoExt + ".json", Twitter.ToJson());
            //File.WriteAllText(OutputFilePath + JsonablePrefix + CatalogFileNameNoExt + ".json", Catalog.ToJson());
            //File.WriteAllBytes(OutputFilePath + JsonableWithCommentsPrefix + TwitterFileNameNoExt + ".jsonc", TwitterWithComments.ToArray());
            //File.WriteAllBytes(OutputFilePath + JsonableWithCommentsPrefix + CatalogFileNameNoExt + ".jsonc", CatalogWithComments.ToArray());
        }


        #region   Jsonable
        ///*

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CheckJsonSize(int length)
        {
            Must.BeTrue(length >= MinJsonLength * (Boost / 2));
            return 0;
        }

        readonly ArrayBufferWriter<byte> cache_buffer = new();
        readonly MemoryStream cache_memoryStream = new();
        Utf8JsonWriter cache_utf8Writer = (((default)))!;

        const string TWITTER = "Twitter";
        const string CATALOG = "Catalog";



        const string SAVE = "Serialize";
        [BenchmarkCategory(SAVE + TWITTER)]
        [Benchmark(Baseline = true)]
        public int Twitter_Save_ToJsonUtf8Cache()
        {
            var w = cache_buffer;
            Twitter.ToJsonUtf8(w, emitByteOrderMark: false, emitMetadataComments: false);
            var ret = CheckJsonSize(w.WrittenCount);
            w.Clear();
            return ret;
        }
        [BenchmarkCategory(SAVE + TWITTER)][Benchmark] public int Twitter_Save_MsgPack() => MessagePackSerializer.Serialize(Twitter).Length;
        [BenchmarkCategory(SAVE + TWITTER)][Benchmark] public int Twitter_Save_SysTxtJson() => CheckJsonSize(JsonSerializer.Serialize(Twitter, Json_Twitter_STJ.Default.Json_Twitter).Length);
        [BenchmarkCategory(SAVE + TWITTER)]
        [Benchmark]
        public int Twitter_Save_SysTxtJsonUtf8()
        {
            // don't close stream
            var w = cache_utf8Writer;
            JsonSerializer.Serialize(w, Twitter, Json_Twitter_STJ.Default.Json_Twitter);
            w.Flush();
            var ret = (int)cache_memoryStream.Position;
            cache_memoryStream.Position = 0;
            return CheckJsonSize(ret);
        }
        [BenchmarkCategory(SAVE + TWITTER)][Benchmark] public int Twitter_Save_JsonNET() => CheckJsonSize(JsonConvert.SerializeObject(Twitter).Length);
        [BenchmarkCategory(SAVE + TWITTER)]
        [Benchmark]
        public int Twitter_Save_ToJsonUtf8()
        {
            var w = new ArrayBufferWriter<byte>();
            Twitter.ToJsonUtf8(w, emitByteOrderMark: false, emitMetadataComments: false);
            return CheckJsonSize(w.WrittenCount);
        }
        [BenchmarkCategory(SAVE + TWITTER)][Benchmark] public int Twitter_Save_ToJsonable() => CheckJsonSize(Twitter.ToJsonable().Length);
        [BenchmarkCategory(SAVE + TWITTER)][Benchmark] public int Twitter_Save_ToJson() => CheckJsonSize(Twitter.ToJson().Length);



        [BenchmarkCategory(SAVE + CATALOG)]
        [Benchmark(Baseline = true)]
        public int Catalog_Save_ToJsonUtf8Cache()
        {
            var w = cache_buffer;
            Catalog.ToJsonUtf8(w, emitByteOrderMark: false, emitMetadataComments: false);
            var ret = CheckJsonSize(w.WrittenCount);
            w.Clear();
            return ret;
        }
        [BenchmarkCategory(SAVE + CATALOG)][Benchmark] public int Catalog_Save_MsgPack() => MessagePackSerializer.Serialize(Catalog).Length;
        [BenchmarkCategory(SAVE + CATALOG)][Benchmark] public int Catalog_Save_SysTxtJson() => CheckJsonSize(JsonSerializer.Serialize(Catalog, Json_CitmCatalog_STJ.Default.Json_CitmCatalog).Length);
        [BenchmarkCategory(SAVE + CATALOG)]
        [Benchmark]
        public int Catalog_Save_SysTxtJsonUtf8()
        {
            // don't close stream
            var w = cache_utf8Writer;
            JsonSerializer.Serialize(w, Catalog, Json_CitmCatalog_STJ.Default.Json_CitmCatalog);
            w.Flush();
            var ret = (int)cache_memoryStream.Position;
            cache_memoryStream.Position = 0;
            return CheckJsonSize(ret);
        }
        [BenchmarkCategory(SAVE + CATALOG)][Benchmark] public int Catalog_Save_JsonNET() => CheckJsonSize(JsonConvert.SerializeObject(Catalog).Length);
        [BenchmarkCategory(SAVE + CATALOG)]
        [Benchmark]
        public int Catalog_Save_ToJsonUtf8()
        {
            var w = new ArrayBufferWriter<byte>();
            Catalog.ToJsonUtf8(w, emitByteOrderMark: false, emitMetadataComments: false);
            return CheckJsonSize(w.WrittenCount);
        }
        [BenchmarkCategory(SAVE + CATALOG)][Benchmark] public int Catalog_Save_ToJsonable() => CheckJsonSize(Catalog.ToJsonable().Length);
        [BenchmarkCategory(SAVE + CATALOG)][Benchmark] public int Catalog_Save_ToJson() => CheckJsonSize(Catalog.ToJson().Length);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CheckResult(Json_Twitter? t)
        {
            Must.BeEqual(100 * Boost, t?.statuses?.Length ?? 0);
            return 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CheckResult(Json_TwitterReusable? t)
        {
            Must.BeEqual(100 * Boost, t?.statuses?.Count ?? 0);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CheckResult(Json_CitmCatalog? c)
        {
            Must.BeEqual(243 * Boost, c?.performances?.Length);
            Must.BeEqual(184 * Boost, c?.events?.Count);
            return 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CheckResult(Json_CitmCatalogReusable? c)
        {
            Must.BeEqual(243 * Boost, c?.performances?.Count);
            Must.BeEqual(184 * Boost, c?.events?.Count);
            return 0;
        }



        const string LOAD = "Deserialize";
        [BenchmarkCategory(LOAD + TWITTER)][Benchmark(Baseline = true)] public int Twitter_Load_FromJsonable() { Twitter.FromJsonable(TwitterWithComments); return CheckResult(Twitter); }
        [BenchmarkCategory(LOAD + TWITTER)][Benchmark] public int Twitter_Load_FromJsonableArray() { Twitter.FromJsonable(TwitterWithComments, reuseInstance: true); return CheckResult(Twitter); }
        [BenchmarkCategory(LOAD + TWITTER)]
        [Benchmark]
        public int Twitter_Load_FromJsonableReuseList()
        {
            TwitterReusable.statuses?.Clear();
            TwitterReusable.FromJsonable(TwitterWithComments, reuseInstance: true);
            return CheckResult(TwitterReusable);
        }
        [BenchmarkCategory(LOAD + TWITTER)][Benchmark] public int Twitter_Load_MsgPack() => CheckResult(MessagePackSerializer.Deserialize<Json_Twitter>(TwitterMessagePack));
        [BenchmarkCategory(LOAD + TWITTER)][Benchmark] public int Twitter_Load_SysTxtJson() => CheckResult(JsonSerializer.Deserialize(TwitterJsonSource, Json_Twitter_STJ.Default.Json_Twitter));
        [BenchmarkCategory(LOAD + TWITTER)]
        [Benchmark]
        public int Twitter_Load_SysTxtJsonUtf8()
        {
            var reader = new Utf8JsonReader(TwitterJsonUtf8.Span);
            return CheckResult(JsonSerializer.Deserialize(ref reader, Json_Twitter_STJ.Default.Json_Twitter));
        }
        [BenchmarkCategory(LOAD + TWITTER)][Benchmark] public int Twitter_Load_JsonNET() => CheckResult(JsonConvert.DeserializeObject<Json_Twitter>(TwitterJsonSource));
        // [BenchmarkCategory(LOAD + TWITTER)][Benchmark] public int Twitter_Load_SimpleJson() { JSON.TryParse(Twitter, TwitterWithComments); return CheckResult(Twitter); }



        [BenchmarkCategory(LOAD + CATALOG)][Benchmark(Baseline = true)] public int Catalog_Load_FromJsonable() { Catalog.FromJsonable(CatalogWithComments); return CheckResult(Catalog); }
        [BenchmarkCategory(LOAD + CATALOG)][Benchmark] public int Catalog_Load_FromJsonableReuseArray() { Catalog.FromJsonable(CatalogWithComments, reuseInstance: true); return CheckResult(Catalog); }
        [BenchmarkCategory(LOAD + CATALOG)]
        [Benchmark]
        public int Catalog_Load_FromJsonableReuseList()
        {
            var obj = CatalogReusable;
            {
                obj.areaNames?.Clear();
                obj.audienceSubCategoryNames?.Clear();
                //obj.blockNames?.Clear();
                obj.events?.Clear();
                obj.performances?.Clear();
                obj.seatCategoryNames?.Clear();
                obj.subTopicNames?.Clear();
                //obj.subjectNames?.Clear();
                obj.topicNames?.Clear();
                obj.topicSubTopics?.Clear();
                obj.venueNames?.Clear();
            }
            CatalogReusable.FromJsonable(CatalogWithComments, reuseInstance: true);
            return CheckResult(CatalogReusable);
        }
        [BenchmarkCategory(LOAD + CATALOG)][Benchmark] public int Catalog_Load_MsgPack() => CheckResult(MessagePackSerializer.Deserialize<Json_CitmCatalog>(CatalogMessagePack));
        [BenchmarkCategory(LOAD + CATALOG)][Benchmark] public int Catalog_Load_SysTxtJson() => CheckResult(JsonSerializer.Deserialize(CatalogJsonSource, Json_CitmCatalog_STJ.Default.Json_CitmCatalog));
        [BenchmarkCategory(LOAD + CATALOG)]
        [Benchmark]
        public int Catalog_Load_SysTxtJsonUtf8()
        {
            var reader = new Utf8JsonReader(CatalogJsonUtf8.Span);
            return CheckResult(JsonSerializer.Deserialize(ref reader, Json_CitmCatalog_STJ.Default.Json_CitmCatalog));
        }
        [BenchmarkCategory(LOAD + CATALOG)][Benchmark] public int Catalog_Load_JsonNET() => CheckResult(JsonConvert.DeserializeObject<Json_CitmCatalog>(CatalogJsonSource));
        // [BenchmarkCategory(LOAD + CATALOG)][Benchmark] public int Catalog_Load_SimpleJson() { JSON.TryParse(Catalog, CatalogWithComments); return CheckResult(Catalog); }

        //*/
        #endregion


        #region   Stackalloc vs Readonly Static
        /*

        static class SR
        {
            public static readonly byte[] StaticBuffer = new byte[]
            {
            0, 1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 58, 59, 60, 61, 62, 63,
            64, 65, 66, 67, 68, 69, 70, 71,
            72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87,
            88, 89, 90, 91, 92, 93, 94, 95,
            };
        }

        private static readonly byte[] staticBuffer = new byte[]
        {
            0, 1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 58, 59, 60, 61, 62, 63,
            64, 65, 66, 67, 68, 69, 70, 71,
            72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87,
            88, 89, 90, 91, 92, 93, 94, 95,
        };

        [Benchmark]
        public int UseStaticBuffer()
        {
            return Method(staticBuffer);
        }

        [Benchmark]
        public int UseStaticBuffer_SR()
        {
            return Method(SR.StaticBuffer);
        }

        [Benchmark]
        public int UseStackallocBuffer()
        {
            return Method((stackalloc byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7,
                8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23,
                24, 25, 26, 27, 28, 29, 30, 31,
                32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47,
                48, 49, 50, 51, 52, 53, 54, 55,
                56, 57, 58, 59, 60, 61, 62, 63,
                64, 65, 66, 67, 68, 69, 70, 71,
                72, 73, 74, 75, 76, 77, 78, 79,
                80, 81, 82, 83, 84, 85, 86, 87,
                88, 89, 90, 91, 92, 93, 94, 95,
            }));
        }

        private int Method(ReadOnlySpan<byte> buffer)
        {
            int sum = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i];
            }
            return sum;
        }

        */
        #endregion
    }
}
