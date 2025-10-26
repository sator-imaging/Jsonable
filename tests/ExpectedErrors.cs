//#define __expected_errors
#if __expected_errors

using Jsonable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CA1852  // Seal internal types
#pragma warning disable IDE0250  // Struct can be made 'readonly'
#pragma warning disable CA1822  // Mark members as static
#pragma warning disable IDE0251  // Member can be made 'readonly'

namespace Tests
{
    //001
    [ToJson] partial class ToJson_C { public float Value { set { } } }
    [ToJson] partial struct ToJson_S { public float Value { set { } } }
    [ToJson] readonly partial struct ToJson_ROS { public float Value { set { } } }
    // no warn
    [ToJson] readonly partial record struct ToJson_RORS(float Value) { }
    [ToJson] partial record ToJson_R(float Value) { }
    [ToJson] partial record struct ToJson_RS(float Value) { }

    //002
    [FromJson] partial class FromJson_C { public float Value { get; } }
    [FromJson] partial struct FromJson_S { public float Value { get; } }
    [FromJson] readonly partial struct FromJson_ROS { public float Value { get; } }
    //003
    [FromJson] readonly partial record struct FromJson_RORS(float Value) { }
    [FromJson] partial record FromJson_R(float Value) { }
    // no warn
    [FromJson] partial record struct FromJson_RS(float Value) { }

    // IEnumerable is only valid for ToJson
    [ToJson] partial class ToJson_OK { public IReadOnlyList<int> NoError { get; set; } = Array.Empty<int>(); }
    [FromJson] partial class ToJson_NG { public IReadOnlyList<int> Error { get; set; } = Array.Empty<int>(); }

    //011
    [FromJson] class NoPartialFromJson { }
    [ToJson] class NoPartialToJson { }
    //012
    [ToJson(Property = "FOO")]
    //013
    [ToJson(Property = "FOO")]
    [ToJson]
    [ToJson]
    [FromJson]
    partial class ExpectedErrors
    {
        //021
        public DateTime DateTimeProp { get; private set; }
        public DateTime? DateTimeNull { get; private set; }
        public List<DateTime> DateTimeListProp { get; set; } = new();
        public IList<DateTime?> DateTimeNullableListProp { get; set; } = new List<DateTime?>();
        //022
        public Dictionary<int, float> NonStringKeyMapProp { get; private set; } = new();
        public IDictionary<int, float> NonStringKeyMapInterfaceProp { get; private set; } = new Dictionary<int, float>();
        //023
        public object UnsupportedProp { get; set; } = new();
        public object? UnsupportedNull { get; set; }
        public char CharProp { get; set; }
        public char? CharNull { get; set; }
        public decimal DecimalProp { get; set; }
        public decimal? DecimalNull { get; set; }
        public int[]?[] NullableJagArray { get; set; } = new int[]?[0];
        public int[]?[]? NullableJagArrayNull { get; set; }
        public int[][][] JagJagArray { get; set; } = new int[0][][];
        public int[][][]? JagJagArrayNull { get; set; }
        //024
        public KVPairCollection<string, float> KVPairCollectionProp { get; set; } = new();
        public KVPairCollection<string, float>? KVPairCollectionNull { get; set; }
        public ICollection<KeyValuePair<string, float>> IKVPairCollectionProp { get; set; } = new KVPairCollection<string, float>();
        public ICollection<KeyValuePair<string, float>>? IKVPairCollectionNull { get; set; }
        //025
        public NoParameterlessCtorJsonable NoParameterlessCtorJsonableProp { get; set; } = new(310);
        public NoParameterlessCtorJsonable? NoParameterlessCtorJsonableNull { get; set; }
        public NoParameterlessCtorCollection NoParameterlessCtorCollectionProp { get; set; } = new(310);
        public NoParameterlessCtorCollection? NoParameterlessCtorCollectionNull { get; set; }
        public NoParameterlessCtorDictionary NoParameterlessCtorDictionaryProp { get; set; } = new(310);
        public NoParameterlessCtorDictionary? NoParameterlessCtorDictionaryNull { get; set; }
        //026
        public MultipleICollections MultipleICollectionsProp { get; set; } = new();
        public MultipleICollections? MultipleICollectionsNull { get; set; }
        //027
        public ValueTypeCollection<int> ValueTypeCollectionProp { get; set; } = new();
        public ValueTypeCollection<int>? ValueTypeCollectionNull { get; set; }
        public ValueTypeDictionary<string, int> ValueTypeDictionaryProp { get; set; } = new();
        public ValueTypeDictionary<string, int>? ValueTypeDictionaryNull { get; set; }

        // nested jsonables
        public T_ReadOnlyStruct ReadOnlyStructJsonableProp { get; set; }
        public T_ReadOnlyStruct? ReadOnlyStructJsonableNull { get; set; }
        public T_ReadOnlyRecordStruct ReadOnlyRecordStructJsonableProp { get; set; }
        public T_ReadOnlyRecordStruct? ReadOnlyRecordStructJsonableNull { get; set; }
        public T_Record RecordJsonableProp { get; set; } = new();
        public T_Record? RecordJsonableNull { get; set; }
        public FromJsonOnly FromJsonOnlyObjectProp { get; set; } = new();
        public FromJsonOnly? FromJsonOnlyObjectNull { get; set; }
        public ToJsonOnly ToJsonOnlyObjectProp { get; set; } = new();
        public ToJsonOnly? ToJsonOnlyObjectNull { get; set; }
        // no error
        public T_Struct StructJsonableProp { get; set; }
        public T_Struct? StructJsonableNull { get; set; }
        public T_RecordStruct RecordStructJsonableProp { get; set; }
        public T_RecordStruct? RecordStructJsonableNull { get; set; }
        public ExpectedErrors SelfRefProp { get; set; } = new();
        public ExpectedErrors? SelfRefNull { get; set; }
    }

    [FromJson] partial class FromJsonOnly { }
    [ToJson] partial class ToJsonOnly { }

    class KVPairCollection<T, U> : ICollection<KeyValuePair<T, U>>
    {
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(KeyValuePair<T, U> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(KeyValuePair<T, U> item) => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<T, U>> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(KeyValuePair<T, U> item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class MultipleICollections : ICollection<int>, ICollection<float>
    {
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(int item) => throw new NotImplementedException();
        public void Add(float item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(int item) => throw new NotImplementedException();
        public bool Contains(float item) => throw new NotImplementedException();
        public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
        public void CopyTo(float[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(int item) => throw new NotImplementedException();
        public bool Remove(float item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<float> IEnumerable<float>.GetEnumerator() => throw new NotImplementedException();
    }

    [FromJson]
    [ToJson]
    partial class NoParameterlessCtorJsonable
    {
        private NoParameterlessCtorJsonable() { }
        public NoParameterlessCtorJsonable(int _) { }
    }

    partial class NoParameterlessCtorCollection : ICollection<int>
    {
        private NoParameterlessCtorCollection() { }
        public NoParameterlessCtorCollection(int _) { }

        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(int item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(int item) => throw new NotImplementedException();
        public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(int item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    partial class NoParameterlessCtorDictionary : IDictionary<string, string>
    {
        private NoParameterlessCtorDictionary() { }
        public NoParameterlessCtorDictionary(int _) { }

        public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> Keys => throw new NotImplementedException();
        public ICollection<string> Values => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(string key, string value) => throw new NotImplementedException();
        public void Add(KeyValuePair<string, string> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(KeyValuePair<string, string> item) => throw new NotImplementedException();
        public bool ContainsKey(string key) => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(string key) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, string> item) => throw new NotImplementedException();
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    struct ValueTypeCollection<T> : ICollection<T>
    {
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    struct ValueTypeDictionary<T, U> : IDictionary<T, U>
    {
        public U this[T key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<T> Keys => throw new NotImplementedException();
        public ICollection<U> Values => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(T key, U value) => throw new NotImplementedException();
        public void Add(KeyValuePair<T, U> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(KeyValuePair<T, U> item) => throw new NotImplementedException();
        public bool ContainsKey(T key) => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<T, U>> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(T key) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<T, U> item) => throw new NotImplementedException();
        public bool TryGetValue(T key, out U value) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

#endif
