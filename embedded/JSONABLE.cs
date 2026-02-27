// Licensed under the MIT License
// https://github.com/sator-imaging/Jsonable

// NOTE: this script file is included in analyzer build AND embedded in target assembly.
//       * some methods are not called in ANALYZER so it can be excluded from ANALYZER build.
//       * the following directive is resolved in target assembly because it is EMBEDDED.
//       * source generator in Unity runs on unknown environment so directive for Unity is required.
#if UNITY_2021_3_OR_NEWER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#define __supported
#endif

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

#nullable enable

namespace Jsonable
{
    internal sealed class JsonableException : Exception
    {
        JsonableException(string message, Exception? inner) : base(message, inner) { }

        [DoesNotReturn]
        public static void Throw(string message, Exception? inner = null) => throw new JsonableException(message, inner);

        [DoesNotReturn]
        public static void ThrowWithBufferPreview(string message, ReadOnlySpan<byte> bufferToPreview, Exception? inner = null)
        {
            var preview = JSONABLE.GetVisibleString(bufferToPreview.Slice(0, Math.Min(40, bufferToPreview.Length)));
            throw new JsonableException($"{message} (```{preview}```)", inner);
        }
    }

    internal static class JSONABLE
    {
        // NOTE: in little endian
        //       - 1st char must be greater than '/' (can have UTF-8 lead byte)
        //       --> when emit .jsonc comment with random ushort value in /*{r}*/ format,
        //           need to avoid generating '/**/*/' and '/*/**/' unexpectedly.
        //       - 2nd char must be less than '0x7F' as 1st char may be UTF-8 lead byte
        //       --> if MSB is starting '10', it can be valid utf8 string.
        //       --> also, avoid null char (\0) in json by adding +1 always.
        public const int MetadataLengthOffset = 0x_01_30;  // 0x2A = '*', 0x2F = '/'
        public const int MetadataLengthMaxInclusive = 0x_7E_FF - MetadataLengthOffset;  // no 0x7F (DEL) at 2nd char

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort EncodeLengthUnsafe(int length)
        {
            // unsafe means no range check. not unchecked arithmetic.
            checked
            {
                return (ushort)(length + MetadataLengthOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort DecodeLengthUnsafe(ReadOnlySpan<byte> bytes)
        {
            // unsafe means no range check. not unchecked arithmetic.
            checked
            {
                return (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(bytes) - MetadataLengthOffset);
            }
        }


        public const int Base64MaxLength = (MetadataLengthMaxInclusive / 4 * 3);
        public const int MaxPossibleValueLength = 40;  // for value types without length metadata

        public const char DateTimeFormat = 'O';
        public const char GuidFormat = 'D';

        public static readonly UTF8Encoding Encoder = (UTF8Encoding)Encoding.UTF8;
        public static readonly byte[] NULL = new byte[] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static readonly byte[] TRUE = new byte[] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static readonly byte[] FALSE = new byte[] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };
        public static readonly byte[] CommentOpen = new byte[] { (byte)'/', (byte)'*' };
        public static readonly byte[] CommentClose = new byte[] { (byte)'*', (byte)'/' };
        public static readonly byte[] ValueEndChars = new byte[] { (byte)',', (byte)'}', (byte)']' };
        public static readonly char[] EscapeTargets = new char[] { '"', '\\', '\n', '\t', '\r', /*'\b', '\f'*/ };

        public static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

        public static readonly byte[] ObjectHeader = new byte[] { (byte)'/', (byte)'*', (byte)'J', (byte)'M', (byte)'C', (byte)'1', (byte)'*', (byte)'/' };
        public const ulong ObjectHeader_LE
            = ((ulong)'/' << 0)
            | ((ulong)'*' << 8)
            | ((ulong)'J' << 16)
            | ((ulong)'M' << 24)
            | ((ulong)'C' << 32)
            | ((ulong)'1' << 40)
            | ((ulong)'*' << 48)
            | ((ulong)'/' << 56)
            ;
        public const int ObjectHeaderLength = 8;

        // literal detectors
        public const uint NULL_LE
            = ((uint)'n' << 0)
            | ((uint)'u' << 8)
            | ((uint)'l' << 16)
            | ((uint)'l' << 24)
            ;
        public const uint TRUE_LE
            = ((uint)'t' << 0)
            | ((uint)'r' << 8)
            | ((uint)'u' << 16)
            | ((uint)'e' << 24)
            ;
        public const uint FALSE_LE
            = ((uint)'f' << 0)
            | ((uint)'a' << 8)
            | ((uint)'l' << 16)
            | ((uint)'s' << 24)
            ;


        public static string GetVisibleString(ReadOnlySpan<byte> bytes)
        {
            return Encoder.GetString(bytes.ToArray().Where(x => x != '\0').ToArray());
        }


        /// <returns>The length of the decoded byte array, or -1 if the input length is invalid.</returns>
        public static int GetBase64DecodedLength(ReadOnlySpan<byte> base64Bytes)
        {
            int encodedLength = base64Bytes.Length;
            if (encodedLength == 0)
            {
                return 0;
            }
            else if (encodedLength < 4)
            {
                return -1;
            }

            int paddingCharCount = 0;
            if (base64Bytes[encodedLength - 1] == (byte)'=')
            {
                paddingCharCount++;
            }
            if (base64Bytes[encodedLength - 2] == (byte)'=')
            {
                paddingCharCount++;
            }

            // Each 4 chars represent 3 bytes, except for padding.
            // We assume valid Base64 input, so no need to check for invalid lengths.
            return (encodedLength / 4 * 3) - paddingCharCount;
        }


        // TODO: use SIMD function for string operation while keeping Unity compatibility

        #region   Escape/Unescape string

        volatile static WeakReference<StringBuilder>? interlock_sb;

        public static string EscapeStringIfRequired(string value)
        {
            // TODO: SIMD --> detect char code lower than or equal to 0x2F (control chars) and also \ and "
            int firstIndex = value.IndexOfAny(EscapeTargets);
            if (firstIndex < 0)
            {
                return value;
            }

#if __supported
            if (value.Length <= 128)
            {
                return ShortSlowPath(value.AsSpan(), firstIndex);

                static string ShortSlowPath(ReadOnlySpan<char> value, int firstIndex)
                {
                    Span<char> buffer = stackalloc char[value.Length << 1];
                    value.Slice(0, firstIndex).CopyTo(buffer);
                    int written = firstIndex;
                    for (int i = firstIndex; i < value.Length; i++)
                    {
                        switch (value[i])
                        {
                            case '\\': buffer[written++] = '\\'; buffer[written++] = '\\'; break;
                            case '"': buffer[written++] = '\\'; buffer[written++] = '"'; break;
                            case '\n': buffer[written++] = '\\'; buffer[written++] = 'n'; break;
                            case '\t': buffer[written++] = '\\'; buffer[written++] = 't'; break;
                            case '\r': buffer[written++] = '\\'; buffer[written++] = 'r'; break;
                            default: buffer[written++] = value[i]; break;
                        }
                    }
                    return new string(buffer.Slice(0, written));
                }
            }
#endif

            return SlowPath(value, firstIndex);

            static string SlowPath(string value, int firstIndex)
            {
                var weakRef = Interlocked.Exchange(ref interlock_sb, null);
                if (weakRef == null || !weakRef.TryGetTarget(out var sb))  // don't simplify by using weakRef?.TryGetTarget (fix for Unity)
                {
                    sb = new(capacity: value.Length * 2);
                }

                int lastIndex = 0;
                int currentIndex = firstIndex;
                while (currentIndex >= 0)
                {
                    sb.Append(value, lastIndex, currentIndex - lastIndex);
                    switch (value[currentIndex])
                    {
                        case '\\': sb.Append(@"\\"); break;
                        case '"': sb.Append(@"\"""); break;
                        case '\n': sb.Append(@"\n"); break;
                        case '\t': sb.Append(@"\t"); break;
                        case '\r': sb.Append(@"\r"); break;
                    }
                    lastIndex = currentIndex + 1;
                    currentIndex = (lastIndex < value.Length) ? value.IndexOfAny(EscapeTargets, lastIndex) : -1;
                }

                if (lastIndex < value.Length)
                {
                    sb.Append(value, lastIndex, value.Length - lastIndex);
                }

                var result = sb.ToString();
                sb.Length = 0;

                weakRef ??= new(sb);
                weakRef.SetTarget(sb);

                Interlocked.Exchange(ref interlock_sb, weakRef);

                return result;
            }
        }

#if __supported
        public static string EscapeStringIfRequired(ReadOnlySpan<char> value)
        {
            int firstIndex = value.IndexOfAny(EscapeTargets);
            if (firstIndex < 0)
            {
                return value.ToString();
            }

            if (value.Length <= 128)
            {
                return ShortSlowPath(value, firstIndex);

                static string ShortSlowPath(ReadOnlySpan<char> value, int firstIndex)
                {
                    Span<char> buffer = stackalloc char[value.Length << 1];
                    value.Slice(0, firstIndex).CopyTo(buffer);
                    int written = firstIndex;
                    for (int i = firstIndex; i < value.Length; i++)
                    {
                        switch (value[i])
                        {
                            case '\\': buffer[written++] = '\\'; buffer[written++] = '\\'; break;
                            case '"': buffer[written++] = '\\'; buffer[written++] = '"'; break;
                            case '\n': buffer[written++] = '\\'; buffer[written++] = 'n'; break;
                            case '\t': buffer[written++] = '\\'; buffer[written++] = 't'; break;
                            case '\r': buffer[written++] = '\\'; buffer[written++] = 'r'; break;
                            default: buffer[written++] = value[i]; break;
                        }
                    }
                    return new string(buffer.Slice(0, written));
                }
            }

            return EscapeStringIfRequired(value.ToString());
        }

        public static string UnescapeStringIfRequired(ReadOnlySpan<byte> utf8)  // for .NET Standard 2.0; Encoding doesn't have span overload
        {
            if (utf8.Length == 0)
            {
                return string.Empty;
            }

            // checking utf8 bytes is faster
            int firstIndex = utf8.IndexOf((byte)'\\');
            if (firstIndex < 0)
            {
                return Encoder.GetString(utf8);
            }

            if (utf8.Length <= 256)
            {
                return ShortSlowPath(utf8, firstIndex);

                static string ShortSlowPath(ReadOnlySpan<byte> utf8, int firstIndex)
                {
                    Span<byte> buffer = stackalloc byte[utf8.Length];
                    utf8.Slice(0, firstIndex).CopyTo(buffer);
                    int written = firstIndex;
                    for (int i = firstIndex; i < utf8.Length; i++)
                    {
                        if (utf8[i] == (byte)'\\' && i + 1 < utf8.Length)
                        {
                            switch (utf8[i + 1])
                            {
                                case (byte)'\\': buffer[written++] = (byte)'\\'; i++; break;
                                case (byte)'"': buffer[written++] = (byte)'"'; i++; break;
                                case (byte)'n': buffer[written++] = (byte)'\n'; i++; break;
                                case (byte)'t': buffer[written++] = (byte)'\t'; i++; break;
                                case (byte)'r': buffer[written++] = (byte)'\r'; i++; break;
                                default: buffer[written++] = (byte)'\\'; break;
                            }
                        }
                        else
                        {
                            buffer[written++] = utf8[i];
                        }
                    }
                    return Encoder.GetString(buffer.Slice(0, written));
                }
            }

            return SlowPath(utf8);

            static string SlowPath(ReadOnlySpan<byte> utf8)
            {
                var value = Encoder.GetString(utf8);

                var weakRef = Interlocked.Exchange(ref interlock_sb, null);
                if (weakRef == null || !weakRef.TryGetTarget(out var sb))  // don't simplify by using weakRef?.TryGetTarget (fix for Unity)
                {
                    sb = new(capacity: value.Length);
                }

                int lastIndex = 0;
                int currentIndex = value.IndexOf('\\');
                while (currentIndex >= 0)
                {
                    sb.Append(value, lastIndex, currentIndex - lastIndex);
                    if (currentIndex + 1 < value.Length)
                    {
                        char next = value[currentIndex + 1];
                        switch (next)
                        {
                            case '\\': sb.Append('\\'); lastIndex = currentIndex + 2; break;
                            case '"': sb.Append('"'); lastIndex = currentIndex + 2; break;
                            case 'n': sb.Append('\n'); lastIndex = currentIndex + 2; break;
                            case 't': sb.Append('\t'); lastIndex = currentIndex + 2; break;
                            case 'r': sb.Append('\r'); lastIndex = currentIndex + 2; break;
                            default:
                                sb.Append('\\');
                                lastIndex = currentIndex + 1;
                                break;
                        }
                    }
                    else
                    {
                        sb.Append('\\');
                        lastIndex = currentIndex + 1;
                    }
                    currentIndex = (lastIndex < value.Length) ? value.IndexOf('\\', lastIndex) : -1;
                }

                if (lastIndex < value.Length)
                {
                    sb.Append(value, lastIndex, value.Length - lastIndex);
                }

                var result = sb.ToString();
                sb.Length = 0;

                weakRef ??= new(sb);
                weakRef.SetTarget(sb);

                Interlocked.Exchange(ref interlock_sb, weakRef);

                return result;
            }
        }
#endif

        #endregion


        #region   Stringify & Prettify
#if __supported

        public static Span<byte> CopySliceUnsafe(Span<byte> span, ReadOnlySpan<byte> value)
        {
            value.CopyTo(span);
            return span.Slice(value.Length);
        }

        public static Span<byte> CopySliceUnsafe(Span<byte> span, byte value, int count)
        {
            if (count == 1)
            {
                span[0] = value;
            }
            else
            {
                span.Slice(0, count).Fill(value);
            }

            return span.Slice(count);
        }


        public static string Stringify(ReadOnlyMemory<byte> utf8, int indentSize, char indentChar, ReadOnlySpan<char> newLine, bool prettyPrint)
        {
            if (prettyPrint)
            {
                var nl = (stackalloc byte[newLine.Length]);
                for (int i = 0; i < newLine.Length; i++)
                {
                    nl[i] = (byte)(newLine[i] & 0x7F);
                }

                utf8 = Prettify(writer: null, utf8, indentSize, (byte)indentChar, nl);
            }

            return Encoder.GetString(utf8.Span);
        }

        public static ReadOnlyMemory<byte> Prettify(
            ArrayBufferWriter<byte>? writer,
            ReadOnlyMemory<byte> utf8,
            int indentSize,
            byte indentChar,
            ReadOnlySpan<byte> newLine)
        {
            var span = utf8.Span;
            if (span.Length == 0)
            {
                return ReadOnlyMemory<byte>.Empty;
            }

            writer ??= new ArrayBufferWriter<byte>(initialCapacity: span.Length << 1);
            int written;

            int indentLevel = 0;
            int currentIndex = 0;

            do
            {
                switch (span[currentIndex])
                {
                    case (byte)'{':
                    case (byte)'[':
                        {
                            // for comparing with Json.NET, write empty in single line.
                            if (currentIndex + 1 < span.Length && span[currentIndex + 1] is (byte)']' or (byte)'}')
                            {
                                written = currentIndex + 1 + 1;
                                var buffer = writer.GetSpan(written);

                                buffer = CopySliceUnsafe(buffer, span.Slice(0, currentIndex));

                                // write both open and close
                                buffer = CopySliceUnsafe(buffer, span[currentIndex], 1);
                                currentIndex++;
                                /*buffer =*/
                                CopySliceUnsafe(buffer, span[currentIndex], 1);
                                currentIndex++;
                            }
                            else
                            {
                                indentLevel++;

                                written = currentIndex + 1 + newLine.Length + (indentSize * indentLevel);
                                var buffer = writer.GetSpan(written);

                                buffer = CopySliceUnsafe(buffer, span.Slice(0, currentIndex));
                                buffer = CopySliceUnsafe(buffer, span[currentIndex], 1);
                                currentIndex++;
                                buffer = CopySliceUnsafe(buffer, newLine);
                                /*buffer =*/
                                CopySliceUnsafe(buffer, indentChar, indentSize * indentLevel);
                            }
                        }
                        goto ADVANCE;

                    case (byte)':':
                        {
                            written = currentIndex + 1 + 1;
                            var buffer = writer.GetSpan(written);

                            currentIndex++;
                            buffer = CopySliceUnsafe(buffer, span.Slice(0, currentIndex));
                            /*buffer =*/
                            CopySliceUnsafe(buffer, (byte)' ', 1);
                        }
                        goto ADVANCE;

                    case (byte)',':
                        {
                            written = currentIndex + 1 + newLine.Length + (indentSize * indentLevel);
                            var buffer = writer.GetSpan(written);

                            currentIndex++;
                            buffer = CopySliceUnsafe(buffer, span.Slice(0, currentIndex));
                            buffer = CopySliceUnsafe(buffer, newLine);
                            /*buffer =*/
                            CopySliceUnsafe(buffer, indentChar, indentSize * indentLevel);
                        }
                        goto ADVANCE;

                    case (byte)'}':
                    case (byte)']':
                        {
                            indentLevel--;

                            written = currentIndex + newLine.Length + (indentSize * indentLevel) + 1;
                            var buffer = writer.GetSpan(written);

                            buffer = CopySliceUnsafe(buffer, span.Slice(0, currentIndex));
                            buffer = CopySliceUnsafe(buffer, newLine);
                            buffer = CopySliceUnsafe(buffer, indentChar, indentSize * indentLevel);
                            /*buffer =*/
                            CopySliceUnsafe(buffer, span[currentIndex], 1);
                            currentIndex++;
                        }
                        goto ADVANCE;

                    case (byte)'"':
                        {
                            currentIndex++;

                            // update both
                            currentIndex =
                            written = impl(currentIndex, span);

                            static int impl(int currentIndex, ReadOnlySpan<byte> span)
                            {
                                int originalIndex = currentIndex;

                                int found;
                                while ((found = span.Slice(currentIndex).IndexOf((byte)'"')) >= 0)
                                {
                                    found += currentIndex;
                                    currentIndex = found + 1;

                                    if (span[found - 1] == (byte)'\\')
                                    {
                                        continue;
                                    }
                                    return currentIndex;  // currentIndex is next char of '"'. return as is.
                                }
                                return originalIndex;
                            }
                        }
                        goto WRITE_AND_ADVANCE;

                    case (byte)'/':
                        {
                            currentIndex++;

                            // update both
                            currentIndex =
                            written = impl(currentIndex, span);

                            static int impl(int currentIndex, ReadOnlySpan<byte> span)
                            {
                                int originalIndex = currentIndex;

                                if (currentIndex >= span.Length)  // .Slice is allowed but indexer
                                {
                                    goto NOT_FOUND;
                                }

                                // block
                                if (span[currentIndex] == (byte)'*')
                                {
                                    currentIndex++;

                                    int found;
                                    while ((found = span.Slice(currentIndex).IndexOf((byte)'*')) >= 0)
                                    {
                                        found += currentIndex;
                                        currentIndex = found + 1;

                                        if (currentIndex == span.Length || span[currentIndex] != '/')
                                        {
                                            continue;
                                        }

                                        return currentIndex + 1;  // <-- currentIndex is '/'. +1 for written
                                    }

                                    goto NOT_FOUND;
                                }

                            // won't support single line comments

                            NOT_FOUND:
                                return originalIndex;
                            }
                        }
                        goto WRITE_AND_ADVANCE;

                    default:
                        {
                            currentIndex++;
                        }
                        goto CONTINUE;
                }

            WRITE_AND_ADVANCE:
                if (written <= 0)
                {
                    written = span.Length;
                }
                span.Slice(0, written).CopyTo(writer.GetSpan(written));

            ADVANCE:
                writer.Advance(written);
                span = span.Slice(currentIndex);
                currentIndex = 0;

            CONTINUE:
                ;
            }
            while (currentIndex < span.Length);

            written = span.Length;
            span.CopyTo(writer.GetSpan(written));
            writer.Advance(written);

            return writer.WrittenMemory;
        }

#endif
        #endregion


        /* NOTE: loop unrolling doesn't make sense on small data set

        ## x32 times on various data length (48 or more longer property names)

        | Method  | Mean     | Error   | StdDev  |
        |-------- |---------:|--------:|--------:|
        | Default | 262.3 ns | 2.25 ns | 1.88 ns |
        | Unroll4 | 266.8 ns | 3.82 ns | 3.39 ns |
        | Smart   | 264.5 ns | 4.13 ns | 3.86 ns |

        > Smart: use Default if data.Length < 12 otherwise use Unroll4

        */

        public static uint ComputeHash(ReadOnlySpan<byte> data)
        {
            // NOTE: for the reasoning, it's not required to compute complete hash for property names.
            //       instead just takes first N bytes. if collide, perform SequenceEqual to complete check.
            int len = data.Length;
            if (len >= 8)
            {
                // take middle of data
                // --> suffix style: MetadataFoo/Bar --> etadataF/etadataB
                // --> prefix style: Foo/BarMetadata --> ooMetada/arMetada
                var value = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice((len - 8) >> 1));

                var hash = unchecked((uint)(value >> 32) ^ (uint)value);

                if (len >= 9)
                {
                    hash ^= BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(len - 4));
                }

                if (len >= 10)
                {
                    hash ^= BinaryPrimitives.ReadUInt32LittleEndian(data);
                }

                return hash;
            }
            else
            {
                return len switch
                {
                    // 99% are suffix style: V0, Value1, TX, RY, SZ or etc.
                    >= 4 => BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(len - 4)) ^ ((uint)data[0] << 24),  // XOR with last char (little endian)
                    >= 1 => data[len - 1],
                    _ => 0,
                };
            }
        }
    }
}
