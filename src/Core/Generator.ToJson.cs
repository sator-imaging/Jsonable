using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonable.Core
{
    internal static partial class Generator
    {
        const int ToJsonBaseCapacity = JSONABLE.ObjectHeaderLength + 5;  // + BOM + {}
        const int ToJsonPerPropertyCapacity = JSONABLE.MaxPossibleValueLength;  // no *2
        const int ToJsonMinInitialCapacity = 64;
        const int ToJsonMaxInitialCapacity = 512;

        public static string GenerateToJson(
            GeneratorExecutionContext context,
            INamedTypeSymbol jsonableTypeSymbol,
            List<IPropertySymbol> properties,
            string targetPropertyName
        )
        {
            var propertySuffix = targetPropertyName.Length != 0 ? '_' + targetPropertyName : string.Empty;

            var sb = Utils.GetStringBuilder(properties.Count);

            // roughly allocates estimated capacity for resulting utf8
            int initialCapacity = Math.Max(ToJsonMinInitialCapacity, Math.Min(
                ToJsonMaxInitialCapacity,
                Math.Max(ToJsonBaseCapacity, properties.Count * ToJsonPerPropertyCapacity)
            ));

            // Source generation statistics header will be prepended later
            sb.Append(
$@"{Utils.GenerateCSharpDirectives()}
// To avoid accidentally referencing to non-shared resources, don't reference namespace.
using JSONABLE = Jsonable.JSONABLE;
using JsonableException = Jsonable.JsonableException;
using ToJsonHelpers = Jsonable.ToJsonHelpers;

{Utils.GetNamespaceAndContainingTypeDeclarations(jsonableTypeSymbol)}

    {Utils.GetPartialTypeDeclaration(jsonableTypeSymbol)}
    {{
        public void ToJsonUtf8{propertySuffix}<TWriter>(TWriter writer, bool emitMetadataComments = true, bool emitByteOrderMark = false)
            where TWriter : IBufferWriter<byte>
        {{
            {SR.OnWillSerialize}();

            _ = writer.GetSpan({initialCapacity});  // bulk allocation at startup

            //int bytesWritten;
            bool hasNoError = true;
            Span<byte> span;

            if (emitByteOrderMark)
            {{
                hasNoError &= ToJsonHelpers.TryCopyAndAdvance(writer, JSONABLE.Utf8Bom);
            }}

            if (emitMetadataComments)
            {{
                hasNoError &= ToJsonHelpers.TryWriteJsonableHeader(writer);
            }}

            hasNoError &= ToJsonHelpers.TryWriteChar(writer, '{{');
"
            );


            // main
            int generatedPropertyCount = 0;
            foreach (var property in properties)
            {
                if (generatedPropertyCount != 0)
                {
                    sb.Append(
@"            hasNoError &= ToJsonHelpers.TryWriteChar(writer, ',');
"
                    );
                }

                // Write property name
                {
                    sb.Append(
$@"
            //{property.Name}
            {{
"
                    );

                    sb.Append(
$@"                hasNoError &= ToJsonHelpers.TryWriteKey(writer, emitMetadataComments, {SR.Utf8NamesClass}.{property.Name});
            }}
"
                    );
                }


                // Write property value based on type
                var descriptor = PropertyExpression.GenerateJson(context, sb, property.Type,
                        isSerializer: true, property.Name, property.Type, indentLevel: 3, localFunctionDepth: 0);
                if (descriptor != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        property.Locations.FirstOrDefault(),
                        property.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)
#if DEBUG
                        + (
                            property.Type is INamedTypeSymbol named
                            ? $" ({named.ConstructedFrom.TypeKind}|{named.ConstructedFrom.SpecialType})"
                            : $" ({property.Type.TypeKind}|{property.Type.SpecialType})"
                        )
#endif
                    ));

                    continue;
                }

                generatedPropertyCount++;
            }

            sb.Append(
$@"
            hasNoError &= ToJsonHelpers.TryWriteChar(writer, '}}');

            if (!hasNoError)
            {{
                JsonableException.Throw(""Failed to write one or more values, non-nullable type has null, or maybe insufficient buffer space."");
            }}

            {SR.OnDidSerialize}();
        }}
"
            );


            // only when targeting all properties
            if (targetPropertyName.Length == 0)
            {
                sb.Append(
$@"
        public ReadOnlyMemory<byte> ToJsonable()
        {{
            var writer = new ArrayBufferWriter<byte>();
            ToJsonable(writer);

            return writer.WrittenMemory;
        }}

        public void ToJsonable<TWriter>(TWriter writer)
            where TWriter : IBufferWriter<byte>
        {{
            ToJsonUtf8(writer, emitMetadataComments: true, emitByteOrderMark: false);
        }}

        public string ToJson(bool prettyPrint = false, int indentSize = 2, char indentChar = ' ', string newLine = ""\n"", bool emitByteOrderMark = false)
        {{
            var writer = new ArrayBufferWriter<byte>();
            ToJsonUtf8(writer, emitMetadataComments: false, emitByteOrderMark);

            return JSONABLE.Stringify(writer.WrittenMemory, indentSize, indentChar, newLine, prettyPrint);
        }}
"
                );
            }


            // close namespace
            sb.Append(
$@"    }}

{Utils.GetNamespaceAndContainingTypeDeclarationsCloser(jsonableTypeSymbol)}
"
            );

            return Utils.GenerateSourceCodeWithHeader(sb, properties.Count, generatedPropertyCount);
        }
    }
}
