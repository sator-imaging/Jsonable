using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jsonable.Assertions
{
    /// <summary>
    /// Provides debugging utilities for Jsonable, including roundtrip assertions and property comparisons.
    /// </summary>
    public static class JsonableDebugger
    {
        /// <summary>
        /// Runs only in DEBUG mode.
        /// </summary>
        /// <param name="instance">The instance to serialize and deserialize.</param>
        /// <param name="skipJsonDotNetTests">Whether to skip tests using Json.NET.</param>
        /// <param name="propertyNamesToSkip">(Optional) An array of property names to skip during comparison.</param>
        /// <param name="logger">(Optional) A logger action to output messages.</param>
        [Conditional("DEBUG")]
        public static void AssertRoundtrip<T>(
            T instance,
            bool skipJsonDotNetTests = false,
            string[]? propertyNamesToSkip = null,
            Action<string>? logger = null
        )
            where T : new()
        {
            VerifyRoundtrip(instance, skipJsonDotNetTests, propertyNamesToSkip, logger);
        }

        /// <summary>
        /// Runs only in DEBUG mode.
        /// </summary>
        /// <param name="expected">The expected instance.</param>
        /// <param name="actual">The actual instance.</param>
        /// <param name="propertyNamesToSkip">(Optional) An array of property names to skip during comparison.</param>
        /// <param name="logger">(Optional) A logger action to output messages.</param>
        [Conditional("DEBUG")]
        public static void AssertPropertiesEqual<T>(
            T expected,
            T actual,
            string[]? propertyNamesToSkip = null,
            Action<string>? logger = null
        )
        {
            Must.HaveEqualProperties(expected, actual, propertyNamesToSkip, logger);
        }


        internal static void VerifyRoundtrip<T>(
            T instance,
            bool skipJsonDotNetTests = false,
            string[]? propertyNamesToSkip = null,
            Action<string>? logger = null
        )
            where T : new()
        {
            var (ToJson, ToJsonable, FromJsonable) = GetJsonableMethods<T>();

            // export
            var json = ToJson(instance);
            var able = ToJsonable(instance);
            Must.BeTrue(json.Length != 0);
            Must.BeTrue(able.Length != 0);

            // readback
            var readback = new T();
            int consumed = FromJsonable(readback, able, false, false);
            Must.BeTrue(
                able.Length == consumed,
                GenerateErrorMessage($"{able.Length} == {consumed}", able.Span, consumed < 0 ? ~consumed : consumed)
            );

            // JSON & value comparison
            VerifySequenceEqual(json, ToJson(readback).AsSpan(), "ToJson readback", logger);
            VerifySequenceEqual(able.Span, ToJsonable(readback).Span, "ToJsonable readback", logger);
            Must.HaveEqualProperties(instance, readback, logger: logger);

            var jsonWithComments = Encoding.UTF8.GetString(able.Span);
            Must.BeTrue(!jsonWithComments.AsSpan().SequenceEqual(json));

            // Newtonsoft.Json
            if (!skipJsonDotNetTests)
            {
                var jobject = JObject.Parse(jsonWithComments, new JsonLoadSettings()
                {
                    //CommentHandling = CommentHandling.Ignore,
                });

                var jsonFromJsonDotNet = JsonConvert.SerializeObject(jobject, Formatting.None, new JsonSerializerSettings
                {
                    DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
                });

                // json comparison WITHOUT deserialize
                // (it's hard to serialize to json that complete matching, from deserialized object)
                VerifySequenceEqual(
                    json.AsSpan(),
                    jsonFromJsonDotNet.AsSpan(),
                    "Json.NET should produce same result",
                    logger
                    );

                // property comparison WITH deserialized object
                Must.HaveEqualProperties(
                    readback,
                    JsonSerializer.Create(new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter>
                        {
                            new MillisecondsTimeSpanConverter(),
                        }
                    })
                    .Deserialize<T>(new JsonTextReader(new StringReader(jsonWithComments))),
                    propertyNamesToSkip,
                    logger
                    );

                logger?.Invoke($"[{nameof(Jsonable)}] Assertion succeeded: {instance} (Json.NET)");
            }

            logger?.Invoke($"[{nameof(Jsonable)}] Assertion succeeded: {instance} (Roundtrip)");
        }

        class MillisecondsTimeSpanConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (reader.Value == null)
                {
                    return null;
                }

                var text = reader.Value.ToString();
                if (text.All(x => x is (>= '0' and <= '9') or '-' or '.'))
                {
                    return TimeSpan.FromMilliseconds(Convert.ToDouble(reader.Value));
                }
                else
                {
                    return TimeSpan.Parse(text);
                }
            }
        }

        static (Func<T, string> ToJson, Func<T, ReadOnlyMemory<byte>> ToJsonable, Func<T, ReadOnlyMemory<byte>, bool, bool, int> FromJsonable)
            GetJsonableMethods<T>()
        {
            var type = typeof(T);

            MethodInfo ToJson, ToJsonable, FromJsonable;

            ToJson = type.GetMethod(nameof(ToJson), 0, new Type[] { typeof(bool), typeof(int), typeof(char), typeof(string), typeof(bool) });
            ToJsonable = type.GetMethod(nameof(ToJsonable), 0, new Type[] { });
            FromJsonable = type.GetMethod(nameof(FromJsonable), 0, new Type[] { typeof(ReadOnlyMemory<byte>), typeof(bool), typeof(bool) });

            return (
                (x) => (string)ToJson.Invoke(x, new object[] { false, 2, ' ', "\n", false }),
                (x) => (ReadOnlyMemory<byte>)ToJsonable.Invoke(x, new object[] { }),
                (x, memory, reuseInstance, throwIfSyntaxError) => (int)FromJsonable.Invoke(x, new object[] { memory, reuseInstance, throwIfSyntaxError })
            );
        }


        static string GenerateErrorMessage<T>(string message, ReadOnlySpan<T> bytes, int failedIndex, int previewLength = 75)
        {
            if (failedIndex < 0)
            {
                return string.Empty;
            }

            int beforeStart = Math.Max(0, failedIndex - previewLength);
            int beforeLength = Math.Min(previewLength, failedIndex - beforeStart);

            int afterLength = Math.Min(previewLength, bytes.Length - failedIndex);

            return $"{message}\n"
                + $"  DONE --> {filter(bytes.Slice(beforeStart, beforeLength))}\n"
                + $"  FAIL --> {filter(bytes.Slice(failedIndex, afterLength))}\n"
                ;

            static string filter(ReadOnlySpan<T> bytes)
            {
                if (typeof(T) == typeof(byte))
                {
                    return Encoding.UTF8.GetString(bytes.ToArray().OfType<byte>().Where(x => x >= ' ').ToArray());
                }
                else if (typeof(T) == typeof(char))
                {
                    return string.Concat(bytes.ToArray().OfType<char>());
                }
                else
                {
                    throw new Exception("must not be reached");
                }
            }
        }

        static void VerifySequenceEqual<T>(
            ReadOnlySpan<T> expected,
            ReadOnlySpan<T> actual,
            string message,
            Action<string>? logger
            )
        {
            // to verify content itself, don't check length difference
            int minLength = Math.Min(expected.Length, actual.Length);
            expected = expected.Slice(0, minLength);
            actual = actual.Slice(0, minLength);

            int failedIndex = -1;
            for (int i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];
                if (!EqualityComparer<T>.Default.Equals(e, a))
                {
                    failedIndex = i;
                    break;
                }
            }

            // check length difference at last
            if (failedIndex < 0 && expected.Length != actual.Length)
            {
                failedIndex = minLength;
            }

            if (failedIndex >= 0)
            {
                (logger ?? Console.Error.WriteLine).Invoke(GenerateErrorMessage("> Expected", expected, failedIndex));
                (logger ?? Console.Error.WriteLine).Invoke(GenerateErrorMessage("> Actual", actual, failedIndex));
            }

            Must.BeEqual(-1, failedIndex, message);
        }
    }
}
