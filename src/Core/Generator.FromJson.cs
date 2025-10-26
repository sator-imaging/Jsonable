using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Jsonable.Core
{
    internal static partial class Generator
    {
        const int JumpTableThreshold = 13;  // be prime number
        const string LookupTableFieldName = SR.ResourceDataPrefix + "OrdinalNumberByPropertyNameHash";

        public static string GenerateFromJson(
            GeneratorExecutionContext context,
            INamedTypeSymbol jsonableTypeSymbol,
            List<IPropertySymbol> properties
        )
        {
            // hash table
            var propertiesByHash = new Dictionary<uint, List<IPropertySymbol>>(capacity: properties.Count);
            foreach (var property in properties)
            {
                var bytes = JSONABLE.Encoder.GetBytes(property.Name);
                var hash = JSONABLE.ComputeHash(bytes);

                if (!propertiesByHash.TryGetValue(hash, out var names))
                {
                    names = new();
                    propertiesByHash[hash] = names;
                }

                names.Add(property);
            }

            // lookup table
            string lookupTableLocalVarDeclaration = string.Empty;
            string lookupTableDeclaration = string.Empty;
            var useJumpTable = properties.Count >= JumpTableThreshold;
            if (useJumpTable)
            {
                lookupTableLocalVarDeclaration =
$@"
            var lookup = {LookupTableFieldName};";

                var lookup = Utils.GetStringBuilder(properties.Count);

                lookup.Append(
$@"
        {SR.ResourceDataAccessibility} static readonly Dictionary<uint, int> {LookupTableFieldName} = new Dictionary<uint, int>(capacity: {propertiesByHash.Count})
        {{
"
                );

                int i = -1;
                foreach (var p in propertiesByHash.OrderBy(x => x.Key))
                {
                    i++;

                    lookup.Append(
$@"            {{ {p.Key}, {i} }},  // {string.Join(", ", p.Value.Select(x => x.Name))}
"
                    );
                }

                lookup.Append(
$@"        }};
"
                );

                lookupTableDeclaration = lookup.ToString();
            }

            // main
            var sb = Utils.GetStringBuilder(properties.Count);

            // Source generation statistics header will be prepended later
            sb.Append(
$@"{Utils.GenerateCSharpDirectives()}
// To avoid accidentally referencing to non-shared resources, don't reference namespace.
using JSONABLE = Jsonable.JSONABLE;
using JsonableException = Jsonable.JsonableException;
using Parser = Jsonable.FromJsonHelpers.Parser;
using ItemType = Jsonable.FromJsonHelpers.ItemType;
using FromJsonHelpers = Jsonable.FromJsonHelpers;

{Utils.GetNamespaceAndContainingTypeDeclarations(jsonableTypeSymbol)}

    {Utils.GetPartialTypeDeclaration(jsonableTypeSymbol)}
    {{{lookupTableDeclaration}
        public int FromJsonable(ReadOnlyMemory<byte> jsonable, bool reuseInstance = false, bool throwIfSyntaxError = true)
        {{
            {SR.OnWillDeserialize}();

            var parser = new Parser(jsonable);

            var encoder = JSONABLE.Encoder;{lookupTableLocalVarDeclaration}

            //int consumed;
            ItemType itemType;

            while (parser.TryGetNext(out itemType))
            {{
                if (itemType is ItemType.Key)
                {{
                    var propName = parser.TakeStringBytes();
                    var propHash = JSONABLE.ComputeHash(propName);

                    parser.Skip(1);

                    switch ({(useJumpTable ? $"lookup[propHash]" : "propHash")})
                    {{
"
            );

            // Add property switch cases
            int generatedPropertyCount = 0;

            int ordinal = -1;
            foreach (var x in propertiesByHash.OrderBy(x => x.Key))
            {
                ordinal++;  // always increment!!

                var hash = x.Key;
                var hashCollisionProperties = x.Value;

                // case open
                sb.Append(
$@"                        case {(useJumpTable ? ordinal : hash)}:
                        {{
"
                );

                int collisionIndex = -1;
                foreach (var property in hashCollisionProperties)
                {
                    collisionIndex++;

                    var propName = property.Name;

                    sb.Append(
$@"                            //{propName}
"
                    );

                    if (hashCollisionProperties.Count > 1)
                    {
                        var elif = collisionIndex == 0 ? "if" : "else if";
                        sb.Append(
$@"                            {elif} ({SR.Utf8NamesClass}.{propName}.AsSpan().SequenceEqual(propName))
"
                        );
                    }

                    // property open
                    sb.Append(
$@"                            {{
"
                    );

                    // Write property value based on type
                    var descriptor = PropertyExpression.GenerateJson(context, sb, property.Type,
                            isSerializer: false, propName, property.Type, indentLevel: 8, localFunctionDepth: 0);
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

                    // property close
                    sb.Append(
$@"                                continue;
                            }}
"
                    );

                    generatedPropertyCount++;
                }

                // case close
                sb.Append(
$@"                            goto FAILED;
                        }}

"
                );
            }

            // switch close
            sb.Append(
$@"                        default:
                            goto FAILED;
                    }}
                }}

                break;
            }}


            if (itemType == ItemType.EndOfStream)
            {{
                var (totalConsumedLength, _) = parser.GetStatus();

                {SR.OnDidDeserialize}();

                return totalConsumedLength;
            }}

        FAILED:
            {{
                var (totalConsumedLength, _) = parser.GetStatus();

                if (throwIfSyntaxError)
                {{
                    JsonableException.ThrowWithBufferPreview($""Parse failed at index: {{totalConsumedLength}}"", parser.RawSpan);
                }}

                return (~totalConsumedLength);
            }}
        }}
    }}

{Utils.GetNamespaceAndContainingTypeDeclarationsCloser(jsonableTypeSymbol)}
"
            );

            return Utils.GenerateSourceCodeWithHeader(sb, properties.Count, generatedPropertyCount);
        }
    }
}
