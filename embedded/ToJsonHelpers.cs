using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;

#nullable enable

namespace Jsonable
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    internal sealed class ToJsonAttribute : Attribute
    {
        public bool IncludeInternals { get; set; }
        public bool ExcludeInherited { get; set; }

        public bool PreservePropertyOrder { get; set; }
        public string? Property { get; set; }
    }

    internal static class ToJsonHelpers
    {
        public static bool TryWriteJsonableHeader<TWriter>(TWriter writer)
            where TWriter : IBufferWriter<byte>
        {
            var header = JSONABLE.ObjectHeader;
            var span = writer.GetSpan(header.Length);

            if (header.AsSpan().TryCopyTo(span))
            {
                writer.Advance(header.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryWriteJsonableMetadata<TWriter>(TWriter writer, int length)
            where TWriter : IBufferWriter<byte>
        {
            unchecked
            {
                var span = writer.GetSpan(6);

                if ((uint)length > JSONABLE.MetadataLengthMaxInclusive ||
                    span.Length < 6)
                {
                    return false;
                }

                JSONABLE.CommentOpen.AsSpan().CopyTo(span);
                BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(2, 2), JSONABLE.EncodeLengthUnsafe(length));
                JSONABLE.CommentClose.AsSpan().CopyTo(span.Slice(4, 2));

                writer.Advance(6);
                return true;
            }
        }


        public static bool TryWriteNull<TWriter>(TWriter writer)
            where TWriter : IBufferWriter<byte>
        {
            var span = writer.GetSpan(4);

            if (JSONABLE.NULL.AsSpan().TryCopyTo(span))
            {
                writer.Advance(4);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryWriteBoolean<TWriter>(TWriter writer, bool value)
            where TWriter : IBufferWriter<byte>
        {
            var span = writer.GetSpan(5);

            var tf = value ? JSONABLE.TRUE : JSONABLE.FALSE;
            if (tf.AsSpan().TryCopyTo(span))
            {
                writer.Advance(tf.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryWriteChar<TWriter>(TWriter writer, char value)
            where TWriter : IBufferWriter<byte>
        {
            var span = writer.GetSpan(1);
            if (span.Length != 0)
            {
                span[0] = (byte)value;
                writer.Advance(1);
                return true;
            }
            else
            {
                return false;
            }
        }


        public static bool TryWriteString<TWriter>(TWriter writer, string? value, bool needEscape, bool writeMetadata)
            where TWriter : IBufferWriter<byte>
        {
            if (value == null)
            {
                return TryWriteNull(writer);
            }

            if (needEscape)
            {
                value = JSONABLE.EscapeStringIfRequired(value);
            }

            int metadataLength = writeMetadata ? 6 : 0;
            int estimatedLength = (value.Length * 4) + 2 + metadataLength;  // UTF8 max possible bytes + quotes + metadata

            var span = writer.GetSpan(estimatedLength);
            if (span.Length < estimatedLength)
            {
                return false;
            }

            // first, write string to get actual utf8 bytes length
            int bytesWritten = 0;
            if (value.Length > 0)
            {
                bytesWritten = JSONABLE.Encoder.GetBytes(value, span.Slice(metadataLength + 1));  // metadata + leading quote
                if (bytesWritten == 0)
                {
                    return false;
                }
            }

            // then, write metadata
            if (writeMetadata)
            {
                if (!TryWriteJsonableMetadata(writer, bytesWritten))
                {
                    return false;
                }
            }

            // finally add quotes.
            span[metadataLength] = (byte)'"';
            bytesWritten += metadataLength + 1;  // metadata + leading quote

            span[bytesWritten] = (byte)'"';
            bytesWritten++;

            // NOTE: writing metadata will advance!
            writer.Advance(bytesWritten - metadataLength);
            return true;
        }

        public static bool TryWriteKey<TWriter>(TWriter writer, string value, bool needEscape, bool writeMetadata)
            where TWriter : IBufferWriter<byte>
        {
            if (!TryWriteString(writer, value, needEscape, writeMetadata))
            {
                return false;
            }

            var span = writer.GetSpan(1);
            if (span.Length == 0)
            {
                return false;
            }

            span[0] = (byte)':';

            writer.Advance(1);
            return true;
        }

        public static bool TryWriteKey<TWriter>(TWriter writer, bool writeMetadata, byte[] value)  // take byte[] to avoid conversion at caller site
            where TWriter : IBufferWriter<byte>
        {
            var len = value.Length;

            if (writeMetadata)
            {
                if (!TryWriteJsonableMetadata(writer, len))
                {
                    return false;
                }
            }

            var requiredLength = len + 3;  // quotes + ':'

            var span = writer.GetSpan(requiredLength);
            if (span.Length < requiredLength)
            {
                return false;
            }

            span[0] = (byte)'"';
            value.AsSpan().CopyTo(span.Slice(1));
            span[1 + len] = (byte)'"';
            span[2 + len] = (byte)':';

            writer.Advance(requiredLength);
            return true;
        }


        public static bool TryWriteBase64<TWriter>(TWriter writer, byte[] bytes, bool writeMetadata)  // Takes byte[] to reduce caller site code size
            where TWriter : IBufferWriter<byte>
        {
            int encodedLength = (bytes.Length + 2) / 3 * 4;

            if (writeMetadata)
            {
                if (!TryWriteJsonableMetadata(writer, encodedLength))
                {
                    return false;
                }
            }

            int requiredLength = encodedLength + 2;

            var span = writer.GetSpan(requiredLength);
            if (span.Length < requiredLength)
            {
                return false;
            }

            span[0] = (byte)'"';

            if (encodedLength > 0)
            {
                var status = Base64.EncodeToUtf8(bytes, span.Slice(1), out var _, out var written, isFinalBlock: true);
                if (status != OperationStatus.Done ||
                    encodedLength != written)  // not output.Length --> may be larger than requested size
                {
                    return false;
                }
            }

            span[1 + encodedLength] = (byte)'"';

            writer.Advance(requiredLength);
            return true;
        }


        public static bool TryCopyAndAdvance<TWriter>(TWriter writer, byte[] value)  // Takes byte[] to reduce caller site code size
            where TWriter : IBufferWriter<byte>
        {
            if (value.Length == 0)
            {
                return true;
            }

            var span = writer.GetSpan(value.Length);

            if (value.AsSpan().TryCopyTo(span))
            {
                writer.Advance(value.Length);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
