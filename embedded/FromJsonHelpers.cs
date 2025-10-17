#if DEBUG
//#define __printf_debug__TryGetNext
//#define __printf_debug__TakeCollectionSizeOrNegative
//#define __printf_debug__TakeString
//#define __printf_debug__TakeStringBytes
//#define __printf_debug__Skip
#endif

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.InteropServices;

#nullable enable

namespace Jsonable
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    internal sealed class FromJsonAttribute : Attribute
    {
        public bool IncludeInternals { get; set; }
        public bool ExcludeInherited { get; set; }
    }

    internal static class FromJsonHelpers
    {
        public enum ItemType : short  // no Flags attribute
        {
            // NOTE: some logic is depending on the underlying value
            Error        /**/ = short.MinValue,  // 0b_1000...
            Unknown      /**/ = 0,
            EndOfStream  /**/ = 1,

            Key        /**/ = 1 << 1,
            String     /**/ = 1 << 2,
            Number     /**/ = 1 << 3,
            Object     /**/ = 1 << 4,
            Array      /**/ = 1 << 5,
            Boolean    /**/ = 1 << 6,
            Null       /**/ = 1 << 7,
            Map        /**/ = 1 << 8,
            Metadata   /**/ = 1 << 9,
            //EndOfObject = EndOfStream,
            EndOfArray /**/ = 1 << 10,
            EndOfMap   /**/ = 1 << 11,

            // combined flags
            AnyOfExitCondition = Error | EndOfStream,
            AnyOfString = Key | String,
            AnyOfCollection = Array | Map,
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Parser
        {
            readonly int initialLength;
            ReadOnlyMemory<byte> json;
            ItemType mode;

            public Parser(ReadOnlyMemory<byte> json)
            {
                if (json.Length < JSONABLE.ObjectHeaderLength ||
                    JSONABLE.ObjectHeader_LE != BinaryPrimitives.ReadUInt64LittleEndian(json.Span))
                {
                    throw new ArgumentException("Not a jsonable .jsonc stream");
                }

                this.initialLength = json.Length;  // BEFORE slice

                json = json.Slice(JSONABLE.ObjectHeaderLength);

                this.json = json;
                this.mode = default;

                // Empty
                if (json.Length >= 2 &&
                    json.Span[0] == '{' &&
                    json.Span[1] == '}')
                {
                    this.json = json.Slice(2);
                    this.mode = ItemType.EndOfStream;
                }
            }


            readonly
            public (int consumed, ItemType lastItem) GetStatus() => (initialLength - json.Length, mode);

            readonly public ReadOnlyMemory<byte> RawMemory => json;
            readonly public ReadOnlySpan<byte> RawSpan => json.Span;


            public bool TryGetNext(out ItemType foundType)
            {
                var mode = this.mode;
                var json = this.json;

                foundType = default;

#if __printf_debug__TryGetNext
                Console.Write($"  {mode,12}: ");
#endif

                var span = json.Span;
                if (span.Length == 0 || (mode & ItemType.AnyOfExitCondition) != 0)
                {
                    foundType = mode > ItemType.EndOfStream
                        ? ItemType.Error
                        : mode
                        ;
                    goto QUIT__DONT_FORGET_TO_SET_TYPE;
                }

                byte currentChar = span[0];

#if __printf_debug__TryGetNext
                Console.Write($"{(char)currentChar} --> ");
#endif

                // metadata switch
                // it must not advance stream!
                if (currentChar == (byte)'/')
                {
                    foundType = getItemType(span);
                    goto SUCCESS__DONT_FORGET_TO_SET_TYPE;

                    // not a hot path
                    static ItemType getItemType(ReadOnlySpan<byte> span)
                    {
                        // ok to skip length check

                        if (JSONABLE.ObjectHeader_LE == BinaryPrimitives.ReadUInt64LittleEndian(span))
                        {
                            return ItemType.Object;
                        }
                        else
                        {
                            var openChar = span[6];

                            if (openChar is (byte)'[')
                            {
                                return ItemType.Array;
                            }
                            else if (openChar is (byte)'{')
                            {
                                return ItemType.Map;
                            }

                            return ItemType.String;
                        }
                    }
                }

                // json switch
                // always advance!!
                json = json.Slice(1);
                span = span.Slice(1);

                switch (currentChar)
                {
                    case (byte)'{':
                        {
                            foundType = ItemType.Key;
                            goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                        }

                    case (byte)':':
                        {
                            // ok to skip length check

                            if (mode is ItemType.Key)
                            {
                                if (span[0] is (byte)'/')
                                {
                                    foundType = ItemType.Metadata;
                                    goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                                }
                                else
                                {
                                    var fourChars = BinaryPrimitives.ReadUInt32LittleEndian(span);

                                    if (fourChars is JSONABLE.NULL_LE)
                                    {
                                        foundType = ItemType.Null;
                                        goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                                    }
                                    else if (fourChars is JSONABLE.TRUE_LE or JSONABLE.FALSE_LE)
                                    {
                                        foundType = ItemType.Boolean;
                                        goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                                    }

                                    foundType = ItemType.Number;
                                    goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                                }
                            }
                        }
                        break;

                    case (byte)',':
                        {
                            foundType = (mode & ItemType.AnyOfCollection) != 0
                                ? mode
                                : ItemType.Key
                                ;
                            goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                        }

                    case (byte)']':
                        {
                            foundType = ItemType.EndOfArray;
                            goto SUCCESS__DONT_FORGET_TO_SET_TYPE;
                        }

                    case (byte)'}':
                        {
                            foundType = mode is ItemType.Map
                                ? ItemType.EndOfMap
                                : ItemType.EndOfStream
                                ;
                            goto QUIT__DONT_FORGET_TO_SET_TYPE;
                        }

                    default:
                        foundType = ItemType.Error;
                        goto QUIT__DONT_FORGET_TO_SET_TYPE;
                }

            SUCCESS__DONT_FORGET_TO_SET_TYPE:
#if __printf_debug__TryGetNext
                Console.WriteLine($"[SUCCESS] {foundType}");
#endif
                this.json = json;
                this.mode = foundType;
                return true;

            QUIT__DONT_FORGET_TO_SET_TYPE:
#if __printf_debug__TryGetNext
                Console.WriteLine($"[QUIT] {foundType}");
#endif
                this.json = json;
                this.mode = foundType;
                return false;
            }


            public int TakeCollectionSizeOrNegative()
            {
                var json = this.json;
                if (json.Span[0] is not (byte)'/')
                {
                    this.mode = ItemType.Error;
                    return -1;
                }

                // ummmm.....
                var mode = this.mode;
                if (mode is ItemType.Metadata)
                {
                    TryGetNext(out mode);
                }

                // ok to skip length check
                var length = JSONABLE.DecodeLengthUnsafe(json.Span.Slice(2, 2));

#if __printf_debug__TakeCollectionSizeOrNegative
                Console.WriteLine($"[{nameof(TakeCollectionSizeOrNegative)}] {length}");
#endif

                json = json.Slice(6);

                this.json = json;
                return length;
            }


            public ReadOnlySpan<byte> TakeStringBytes()
            {
                var length = TakeCollectionSizeOrNegative();
                if (length < 0)
                {
                    this.mode = ItemType.Error;
                    return ReadOnlySpan<byte>.Empty;
                }

                var result = json.Span.Slice(1, length);  // skip leading "
                this.json = json.Slice(length + 2);       // including quotes

#if __printf_debug__TakeStringBytes
                Console.WriteLine($"[{nameof(TakeStringBytes)}] {JSONABLE.GetVisibleString(result)}");
#endif
                return result;
            }

            public string TakeString()
            {
                var length = TakeCollectionSizeOrNegative();
                if (length < 0)
                {
                    this.mode = ItemType.Error;
                    return string.Empty;
                }

                var result = json.Span.Slice(1, length);  // skip leading "
                this.json = json.Slice(length + 2);       // including quotes

#if __printf_debug__TakeString
                Console.WriteLine($"[{nameof(TakeString)}] {JSONABLE.GetVisibleString(result)}");
#endif

                return JSONABLE.UnescapeStringIfRequired(result);
            }


            public ReadOnlySpan<byte> Skip(int size)
            {
                var result = json.Span.Slice(0, size);
                this.json = json.Slice(size);

#if __printf_debug__Skip
                Console.WriteLine($"[{nameof(Skip)}] {size}");
#endif
                return result;
            }


            public bool TryTakeNull()
            {
                if (json.Length >= 4)
                {
                    var value = BinaryPrimitives.ReadUInt32LittleEndian(json.Span);
                    if (value == JSONABLE.NULL_LE)
                    {
                        this.json = json.Slice(4);
                        return true;
                    }
                }

                return false;
            }


            public byte[] TakeBytesFromBase64(byte[]? current, bool reuseInstance)
            {
                var base64 = TakeStringBytes();

                var decodedLength = JSONABLE.GetBase64DecodedLength(base64);
                if (decodedLength < 0)
                {
                    goto FAILED;
                }

                var result = (reuseInstance && current?.Length == decodedLength) ? current : new byte[decodedLength];

                if (decodedLength > 0)
                {
                    var status = Base64.DecodeFromUtf8(base64, result, out _, out var written, isFinalBlock: true);
                    if (status != OperationStatus.Done ||
                        result.Length != written)
                    {
                        goto FAILED;
                    }
                }

                return result;

            FAILED:
                this.mode = ItemType.Error;
                return Array.Empty<byte>();
            }


            // ummmm.... this is required because it's hard to detect end of map of map/object
            // --> "map":{"key1":{...},"key2":{...}}  <-- ends with double '}'
            public void SetItemType(ItemType type)
            {
                this.mode = type;
            }
        }
    }
}
