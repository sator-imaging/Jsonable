using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Jsonable.Core
{
    internal static partial class Generator
    {
        public static string GenerateStaticResources(
            INamedTypeSymbol typeSymbol,
            HashSet<IPropertySymbol> properties
        )
        {
            var sb = Utils.GetStringBuilder(properties.Count);

            // Source generation statistics header will be prepended later
            var nsDeclaration = Utils.GetNamespaceDeclaration(typeSymbol);
            sb.Append(
$@"{Utils.GenerateCSharpDirectives()}
{nsDeclaration}

    {Utils.GetPartialTypeDeclaration(typeSymbol)}
    {{"
            );


            if (typeSymbol.GetAttributes().Any(x => Utils.HasToJsonAttribute(x)))
            {
                sb.Append(
$@"
        partial void {SR.OnWillSerialize}();
        partial void {SR.OnDidSerialize}();
"
                );
            }


            if (typeSymbol.GetAttributes().Any(x => Utils.HasFromJsonAttribute(x)))
            {
                sb.Append(
$@"
        partial void {SR.OnWillDeserialize}();
        partial void {SR.OnDidDeserialize}();
"
                );
            }


            sb.Append(
$@"
        {SR.ResourceDataAccessibility} static class {SR.Utf8NamesClass}
        {{
"
            );


            // main
            int generatedPropertyCount = 0;
            foreach (var property in properties)
            {
                var declaration = Utils.IsEscapeRequired(property.Name)
                    ? $"{nameof(JSONABLE)}.{nameof(JSONABLE.Encoder)}.{nameof(JSONABLE.Encoder.GetBytes)}(\"{JSONABLE.EscapeStringIfRequired(property.Name)}\")"
                    : $"new byte[] {{ {string.Join(", ", property.Name.Select(x => $"(byte)'{x}'"))} }}"
                    ;

                // write property name bytes
                sb.Append(
$@"            public static readonly byte[] {property.Name} = {declaration};
"
                );

                generatedPropertyCount++;
            }


            // close
            sb.Append(
$@"        }}
    }}
{(nsDeclaration.Length > 0 ? "}" : string.Empty)}
"
            );

            return Utils.GenerateSourceCodeWithHeader(sb, properties.Count, generatedPropertyCount);
        }
    }
}
