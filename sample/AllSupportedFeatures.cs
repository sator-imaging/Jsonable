using Jsonable;
using System;
using System.Collections.Generic;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CA1852  // Seal internal types
#pragma warning disable CA1822  // Mark members as static
#pragma warning disable IDE0060  // Remove unused parameter
#pragma warning disable IDE2001  // Embedded statements must be on their own line

namespace Sample
{
    [ToJson, FromJson] partial struct T_Struct { public float FloatValue { get; set; } }
    [ToJson, /*FromJson*/] readonly partial struct T_ReadOnlyStruct { public float FloatValue { get; } }
    [ToJson, /*FromJson*/] readonly partial record struct T_ReadOnlyRecordStruct(float FloatValue) { }
    [ToJson, FromJson] partial record struct T_RecordStruct(float FloatValue) { }
    [ToJson, /*FromJson*/] partial record T_Record(float FloatValue) { public T_Record() : this(0) { } }

    //// partial type can omit 'readonly'
    //readonly partial struct ReadOnlyModifierTest { }
    //internal partial struct ReadOnlyModifierTest { public int Value; }
    //readonly partial record struct ReadOnlyModifierTestRecord { }
    //internal partial record struct ReadOnlyModifierTestRecord { public int Value; }

    // base64 size limit
    [FromJson, ToJson(PreservePropertyOrder = true)]
    partial class Base64Limit
    {
        public byte[] Base64Prop { get; set; } = Array.Empty<byte>();
    }

    [FromJson]
    [ToJson(PreservePropertyOrder = true)]
    partial class NestJsonable
    {
        public int IntProp { get; set; }
        public string? StringNull { get; set; }
        public Dictionary<string, float> StringToFloatMapProp { get; set; } = new();
    }

    [ToJson(Property = nameof(Base64Prop))]
    [ToJson(PreservePropertyOrder = true)]  // need to preserve order to be able to compare with Json.NET result
    [FromJson]
    partial class AllSupportedFeatures<T, U>
        where T : struct
        where U : notnull  // T2 suffix should be added
    {
        public readonly Dictionary<string, int> CallCounts = new()
        {
            { "OnWillSerialize", 0 },
            { "OnWillDeserialize", 0 },
            { "OnDidSerialize", 0 },
            { "OnDidDeserialize", 0 },
        };
        partial void OnWillSerialize() { CallCounts[nameof(OnWillSerialize)]++; Console.WriteLine(nameof(OnWillSerialize)); }
        partial void OnWillDeserialize() { CallCounts[nameof(OnWillDeserialize)]++; Console.WriteLine(nameof(OnWillDeserialize)); }
        partial void OnDidSerialize() { CallCounts[nameof(OnDidSerialize)]++; Console.WriteLine(nameof(OnDidSerialize)); }
        partial void OnDidDeserialize() { CallCounts[nameof(OnDidDeserialize)]++; Console.WriteLine(nameof(OnDidDeserialize)); }

        public byte[] Base64Prop { get; set; } = Array.Empty<byte>();
        public byte[]? Base64Null { get; set; }
        public int[] ArrayProp { get; set; } = Array.Empty<int>();
        public int[]? ArrayNull { get; set; }
        public float[][] JagArrayProp { get; set; } = Array.Empty<float[]>();
        public float[][]? JagArrayNull { get; set; }

        public EShort?[] NullableValueTypeElementArrayProp { get; set; } = Array.Empty<EShort?>();
        public EShort?[]? NullableValueTypeElementArrayNull { get; set; }
        public string?[] NullableRefTypeElementArrayProp { get; set; } = Array.Empty<string?>();
        public string?[]? NullableRefTypeElementArrayNull { get; set; }
        public int?[][] NullableValueElementJagArrayProp { get; set; } = Array.Empty<int?[]>();
        public int?[][]? NullableValueElementJagArrayNull { get; set; }
        public string?[][] NullableRefElementJagArrayProp { get; set; } = Array.Empty<string?[]>();
        public string?[][]? NullableRefElementJagArrayNull { get; set; }

        public List<int> ListProp { get; set; } = new();
        public List<int>? ListNull { get; set; }
        public ICollection<int> EnumerableProp { get; set; } = new List<int>();
        public ICollection<int>? EnumerableNull { get; set; }
        public Dictionary<string, float> DictProp { get; set; } = new();
        public Dictionary<string, float>? DictNull { get; set; }
        public IDictionary<string, float> MapProp { get; set; } = new Dictionary<string, float>();
        public IDictionary<string, float>? MapNull { get; set; }

        public List<int?> NullableItemListProp { get; set; } = new();
        public List<int?>? NullableItemListNull { get; set; }
        public ICollection<int?> NullableItemEnumerableProp { get; set; } = new List<int?>();
        public ICollection<int?>? NullableItemEnumerableNull { get; set; }
        public Dictionary<string, float?> NullableValueDictProp { get; set; } = new();
        public Dictionary<string, float?>? NullableValueDictNull { get; set; }
        public IDictionary<string, float?> NullableValueMapProp { get; set; } = new Dictionary<string, float?>();
        public IDictionary<string, float?>? NullableValueMapNull { get; set; }

        public string StringProp { get; set; } = "";
        public string? StringNull { get; set; }
        public Uri UriProp { get; set; } = new("http://127.0.0.1/abc");
        public Uri? UriNull { get; set; }
        public NestJsonable JsonableProp { get; set; } = new();
        public NestJsonable? JsonableNull { get; set; }
        public List<NestJsonable> JsonableListProp { get; set; } = new();
        public List<NestJsonable?>? JsonableListNull { get; set; }

        public T_Struct StructJsonableProp { get; set; }
        public T_Struct? StructJsonableNull { get; set; }
        //public T_ReadOnlyStruct ReadOnlyStructJsonableProp { get; set; }
        //public T_ReadOnlyStruct? ReadOnlyStructJsonableNull { get; set; }
        //public T_ReadOnlyRecordStruct ReadOnlyRecordStructJsonableProp { get; set; }
        //public T_ReadOnlyRecordStruct? ReadOnlyRecordStructJsonableNull { get; set; }
        public T_RecordStruct RecordStructJsonableProp { get; set; }
        public T_RecordStruct? RecordStructJsonableNull { get; set; }
        //public T_Record RecordJsonableProp { get; set; } = new();
        //public T_Record? RecordJsonableNull { get; set; }

        public bool BoolProp { get; set; }
        public bool? BoolNull { get; set; }
        public byte ByteProp { get; set; }
        public byte? ByteNull { get; set; }
        public sbyte SByteProp { get; set; }
        public sbyte? SByteNull { get; set; }
        public short ShortProp { get; set; }
        public short? ShortNull { get; set; }
        public ushort UShortProp { get; set; }
        public ushort? UShortNull { get; set; }
        public int IntProp { get; set; }
        public int? IntNull { get; set; }
        public uint UIntProp { get; set; }
        public uint? UIntNull { get; set; }
        public long LongProp { get; set; }
        public long? LongNull { get; set; }
        public ulong ULongProp { get; set; }
        public ulong? ULongNull { get; set; }

        public float FloatProp { get; set; }
        public float? FloatNull { get; set; }
        public double DoubleProp { get; set; }
        public double? DoubleNull { get; set; }

        public DateTimeOffset DateTimeOffsetProp { get; set; }
        public DateTimeOffset? DateTimeOffsetNull { get; set; }
        public TimeSpan TimeSpanProp { get; set; }
        public TimeSpan? TimeSpanNull { get; set; }
        public Guid GuidProp { get; set; }
        public Guid? GuidNull { get; set; }

        public enum EByte : byte { Default, Value = 111, Null = 3 }
        public enum ESByte : sbyte { Default, Value = -111, Null = -3 }
        public enum EShort : short { Default, Value = -22222, Null = -45 }
        public enum EUShort : ushort { Default, Value = 22222, Null = 45 }
        public enum EInt : int { Default, Value = -333333333, Null = -678 }
        public enum EUInt : uint { Default, Value = 3333333333, Null = 678 }
        public enum ELong : long { Default, Value = -4444444444444444444, Null = -9012 }
        public enum EULong : ulong { Default, Value = 4444444444444444444, Null = 9012 }
        public EByte EByteProp { get; set; }
        public EByte? EByteNull { get; set; }
        public ESByte ESByteProp { get; set; }
        public ESByte? ESByteNull { get; set; }
        public EShort EShortProp { get; set; }
        public EShort? ESShortNull { get; set; }
        public EUShort EUShortProp { get; set; }
        public EUShort? EUShortNull { get; set; }
        public EInt EIntProp { get; set; }
        public EInt? EIntNull { get; set; }
        public EUInt EUIntProp { get; set; }
        public EUInt? EUIntNull { get; set; }
        public ELong ELongProp { get; set; }
        public ELong? ELongNull { get; set; }
        public EULong EUlongProp { get; set; }
        public EULong? EULongNull { get; set; }


        public void FillValues(bool setNullToNullables)
        {
            Base64Prop = new byte[] { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };
            Base64Null = setNullToNullables ? null : new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            ArrayProp = new int[] { 11, 22, 33 };
            ArrayNull = setNullToNullables ? null : new int[] { 44, 55, 66 };
            JagArrayProp = new float[][] { new[] { 1.1f, 2.2f, 3.3f }, new[] { 11.11f, 22.22f, 33.33f } };
            JagArrayNull = setNullToNullables ? null : new float[][] { new[] { -1.1f, -2.2f }, new[] { -11.11f, -22.22f } };

            NullableValueTypeElementArrayProp = new EShort?[] { null, EShort.Value };
            NullableValueTypeElementArrayNull = setNullToNullables ? null : new EShort?[] { null, EShort.Null };
            NullableRefTypeElementArrayProp = new string?[] { null, "nullable ref type array (non-nullable)" };
            NullableRefTypeElementArrayNull = setNullToNullables ? null : new string?[] { null, "nullable ref type array (nullable)" };
            NullableValueElementJagArrayProp = new int?[][] { new int?[] { null, 1 }, new int?[] { null, 2 } };
            NullableValueElementJagArrayNull = setNullToNullables ? null : new int?[][] { new int?[] { null, -3 }, new int?[] { null, -4 } };
            NullableRefElementJagArrayProp = new string?[][] { new string?[] { null, "nullable ref element jag array (non-nullable)" }, new string?[] { null, "NREJA (non-nullable)" } };
            NullableRefElementJagArrayNull = setNullToNullables ? null : new string?[][] { new string?[] { null, "nullable ref element jag array (nullable)" }, new string?[] { null, "NREJA (nullable)" } };

            ListProp = new() { 77, 88, 99 };
            ListNull = setNullToNullables ? null : new() { 111, 222, 333 };
            EnumerableProp = new List<int>() { 444, 555, 666 };
            EnumerableNull = setNullToNullables ? null : new List<int>() { 777, 888, 999 };
            DictProp = new() { { "Alpha", 1111 }, { "Bravo", 2222 }, { "Charlie", 3333 } };
            DictNull = setNullToNullables ? null : new() { { "Delta", 4444 }, { "Echo", 5555 }, { "Foxtrott", 6666 } };
            MapProp = new Dictionary<string, float>() { { "Golf", -1111 }, { "Hotel", -2222 }, { "India", -3333 } };
            MapNull = setNullToNullables ? null : new Dictionary<string, float>() { { "Juliett", -4444 }, { "Kilo", -5555 }, { "Lima", -6666 } };

            NullableItemListProp = new List<int?>() { null, 111 };
            NullableItemListNull = setNullToNullables ? null : new List<int?>() { null, -222 };
            NullableItemEnumerableProp = new List<int?>() { null, 333 };
            NullableItemEnumerableNull = setNullToNullables ? null : new List<int?>() { null, -444 };
            NullableValueDictProp = new Dictionary<string, float?>() { { "A1", null }, { "A2", 11.11f } };
            NullableValueDictNull = setNullToNullables ? null : new Dictionary<string, float?>() { { "B1", null }, { "B2", -22.22f } };
            NullableValueMapProp = new Dictionary<string, float?>() { { "C1", null }, { "C2", 33.33f } };
            NullableValueMapNull = setNullToNullables ? null : new Dictionary<string, float?>() { { "D1", null }, { "D2", -44.44f } };

            StringProp = "\"string \\ \r\n\t \u004a\u0053\u004f\u004e \\ prop\" // /* ";
            StringNull = setNullToNullables ? null : " */ // nullable string prop";
            UriProp = new("test://uri.prop/abc");
            UriNull = setNullToNullables ? null : new("test://nullable.uri.prop/abc");
            JsonableProp = new() { IntProp = 123, StringNull = "nest object" };
            JsonableNull = setNullToNullables ? null : new() { IntProp = -321, StringNull = setNullToNullables ? null : "nullable nest object" };
            JsonableListProp = new() { new() { StringNull = "Jsonable list alpha" }, new() { StringNull = "Jsonable list bravo" } };
            JsonableListNull = setNullToNullables ? null : new() { new() { StringNull = "Jsonable list charlie" }, new() { StringNull = "Jsonable list delta" } };

            StructJsonableProp = new() { FloatValue = 123.456f };
            StructJsonableNull = setNullToNullables ? null : new() { FloatValue = -654.321f };
            //ReadOnlyStructJsonableProp = new T_ReadOnlyStruct() { };
            //ReadOnlyStructJsonableNull = setNullToNullables ? null : new T_ReadOnlyStruct() { };
            //ReadOnlyRecordStructJsonableProp = new T_ReadOnlyRecordStruct(310.310f);
            //ReadOnlyRecordStructJsonableNull = setNullToNullables ? null : new T_ReadOnlyRecordStruct(-310.310f);
            RecordStructJsonableProp = new T_RecordStruct(013.013f);
            RecordStructJsonableNull = setNullToNullables ? null : new T_RecordStruct(-013.013f);
            //RecordJsonableProp = new T_Record(111.222f);
            //RecordJsonableNull = setNullToNullables ? null : new T_Record(333.444f);

            BoolProp = true;
            BoolNull = setNullToNullables ? null : true;
            ByteProp = 1;
            ByteNull = setNullToNullables ? null : 9;
            SByteProp = -1;
            SByteNull = setNullToNullables ? null : -9;
            ShortProp = -22;
            ShortNull = setNullToNullables ? null : -2222;
            UShortProp = 22;
            UShortNull = setNullToNullables ? null : 2222;
            IntProp = -33;
            IntNull = setNullToNullables ? null : -3333;
            UIntProp = 33;
            UIntNull = setNullToNullables ? null : 3333;
            LongProp = -44;
            LongNull = setNullToNullables ? null : -4444;
            ULongProp = 44;
            ULongNull = setNullToNullables ? null : 4444;

            FloatProp = 11.11f;
            FloatNull = setNullToNullables ? null : -22.22f;
            DoubleProp = 33.33;
            DoubleNull = setNullToNullables ? null : -44.44;

            DateTimeOffsetProp = new(3210, 12, 3, 4, 5, 6, 789, TimeSpan.FromHours(10));
            DateTimeOffsetNull = setNullToNullables ? null : new(1234, 5, 6, 7, 8, 9, TimeSpan.FromHours(-10));
            TimeSpanProp = TimeSpan.FromMilliseconds(123456789);
            TimeSpanNull = setNullToNullables ? null : TimeSpan.FromMilliseconds(-987654321);
            GuidProp = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1);
            GuidNull = setNullToNullables ? null : new Guid(9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9);

            EByteProp = EByte.Value;
            EByteNull = setNullToNullables ? null : EByte.Null;
            ESByteProp = ESByte.Value;
            ESByteNull = setNullToNullables ? null : ESByte.Null;
            EShortProp = EShort.Value;
            ESShortNull = setNullToNullables ? null : EShort.Null;
            EUShortProp = EUShort.Value;
            EUShortNull = setNullToNullables ? null : EUShort.Null;
            EIntProp = EInt.Value;
            EIntNull = setNullToNullables ? null : EInt.Null;
            EUIntProp = EUInt.Value;
            EUIntNull = setNullToNullables ? null : EUInt.Null;
            ELongProp = ELong.Value;
            ELongNull = setNullToNullables ? null : ELong.Null;
            EUlongProp = EULong.Value;
            EULongNull = setNullToNullables ? null : EULong.Null;
        }


        // longer property name should not use stackalloc in generated ToJson method
        public string ______________________________________________________________64 { get; set; } = "long name use stackalloc";
        public string _______________________________________________________________65 { get; set; } = "longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc; longer name cannot use stackalloc";

        // conflict check
        public string ABCD__Conflict__WXYZ { get; set; } = "starting/ending 4 charsand 8 chars at middle of name are";
        public string ABCD___Conflict___WXYZ { get; set; } = "taken into account to compute property name hash";
        public string VXT { get; set; } = "first";
        public string VYT { get; set; } = "and";
        public string VZT { get; set; } = "last char is used to compute hash";

        // for preserve property order test
        public string StringAlpha { get; private set; } = "string alpha";
        public string StringBravo { get; protected set; } = "string bravo";
        public string StringCharlie { get; internal set; } = "string charlie";

        // type parameter less generic should be supported
        public NoTypeParameterCollection NoTypeParamList { get; set; } = new();
        public NoTypeParameterDictionary NoTypeParamDictionary { get; set; } = new();

        // empty map, list, and array
        public EShort[] EmptyEnumArray { get; set; } = Array.Empty<EShort>();
        public List<EByte> EmptyEnumList { get; set; } = new();
        public Dictionary<string, EByte> EmptyEnumDictionary { get; set; } = new();

        // special floating numbers are not supported by JSON spec (RFC 8259)
        //public float FloatNaN { get; set; } = float.NaN;
        public float FloatMaxValue { get; set; } = float.MaxValue;
        public float FloatMinValue { get; set; } = float.MinValue;
        //public float FloatPositiveInfinity { get; set; } = float.PositiveInfinity;
        //public float FloatNegativeInfinity { get; set; } = float.NegativeInfinity;
        //public double DoubleNaN { get; set; } = double.NaN;
        public double DoubleMaxValue { get; set; } = double.MaxValue;
        public double DoubleMinValue { get; set; } = double.MinValue;
        //public double DoublePositiveInfinity { get; set; } = double.PositiveInfinity;
        //public double DoubleNegativeInfinity { get; set; } = double.NegativeInfinity;
    }

    class NoTypeParameterCollection : List<float> { }
    class NoTypeParameterDictionary : Dictionary<string, float> { }
}
