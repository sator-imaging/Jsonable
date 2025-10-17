using Jsonable.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Jsonable
{
    [Generator]
    public sealed class JsonableSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new JsonableSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource($"- Jsonable - {nameof(Embedded.JSONABLE)}.g.cs", Embedded.JSONABLE);
            // context.AddSource($"- Jsonable - {nameof(Embedded.JSON)}.g.cs", Embedded.JSON);
            context.AddSource($"- Jsonable - {nameof(Embedded.ToJsonHelpers)}.g.cs", Embedded.ToJsonHelpers);
            context.AddSource($"- Jsonable - {nameof(Embedded.FromJsonHelpers)}.g.cs", Embedded.FromJsonHelpers);

            if (context.SyntaxReceiver is not JsonableSyntaxReceiver receiver)
            {
                return;
            }


            var generatedTargetProperties = new Dictionary<INamedTypeSymbol, HashSet<string>>(SymbolEqualityComparer.Default);
            var propertiesForSR = new Dictionary<INamedTypeSymbol, HashSet<IPropertySymbol>>(SymbolEqualityComparer.Default);

            static void addProperties(
                Dictionary<INamedTypeSymbol, HashSet<IPropertySymbol>> propertiesForSR,
                INamedTypeSymbol typeSymbol,
                IEnumerable<IPropertySymbol> jsonableProperties)
            {
                if (!propertiesForSR.TryGetValue(typeSymbol, out var set))
                {
                    set = new(SymbolEqualityComparer.Default);
                    propertiesForSR.Add(typeSymbol, set);
                }

                foreach (var p in jsonableProperties)
                {
                    set.Add(p);
                }
            }


            foreach (var candidateType in receiver.CandidateTypes)
            {
                // Check if the type is declared as partial
                if (!candidateType.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        SR.MissingPartialKeywordDiagnostic,
                        candidateType.Identifier.GetLocation(),
                        candidateType.Identifier.Text));

                    continue; // Skip this type if it's not partial
                }

#if DEBUG
                context.ReportDiagnostic(Diagnostic.Create(SR.DebugMessageDiagnostic, candidateType.Identifier.GetLocation(), "Candidate type found (1/3)"));
#endif

                SemanticModel model = context.Compilation.GetSemanticModel(candidateType.SyntaxTree);

                if (model.GetDeclaredSymbol(candidateType) is not INamedTypeSymbol typeSymbol)
                {
                    continue;
                }

#if DEBUG
                context.ReportDiagnostic(Diagnostic.Create(SR.DebugMessageDiagnostic, typeSymbol.Locations.First(), "Type symbol found (2/3)"));
#endif

                // NOTE: DO NOT perform any type check here!
                //       it must be done in source generation process to detect nested, generic type argument,
                //       or other violation correctly.
                //       again, DO NOT PERFORM ANY TYPE CHECK HERE!!

                // Check if the type has the Jsonable attribute
                foreach (var attributeSyntax in typeSymbol.DeclaringSyntaxReferences
                    .Select(x => x.GetSyntax())
                    .OfType<TypeDeclarationSyntax>()
                    .SelectMany(x => x.AttributeLists)
                    .SelectMany(x => x.Attributes)
                )
                {
                    [Conditional("DEBUG")]
                    static void DEBUG_ReportConfig(
                        GeneratorExecutionContext context,
                        string attrName,
                        Location location,
                        (bool, bool, ImmutableDictionary<string, object>) config
                    )
                    {
                        context.ReportDiagnostic(Diagnostic.Create(SR.DebugMessageDiagnostic, location,
                            $"Attribute '{attrName}' found (3/3)\n" +
                            $"> IncludeInternals: {config.Item1}\n" +
                            $"> ExcludeInherited: {config.Item2}\n" +
                            $"> {string.Join("\n> ", config.Item3)}"));
                    }

                    var attrName = attributeSyntax.Name.ToString();

                    switch (attrName)
                    {
                        case "ToJson":
                        case "ToJsonAttribute":
                            {
                                const string PreservePropertyOrder = nameof(PreservePropertyOrder);
                                const string TargetProperty = "Property";

                                var config = Utils.GetAttributeConfiguration(model, attributeSyntax, PreservePropertyOrder, TargetProperty);
                                var targetPropertyName
                                        = ((config.extraData.TryGetValue(TargetProperty, out var x) ? x as string : null) ?? string.Empty).Trim();

                                DEBUG_ReportConfig(context, attrName, typeSymbol.Locations.First(), config);

                                if (!generatedTargetProperties.TryGetValue(typeSymbol, out var processedPropNameSet))
                                {
                                    processedPropNameSet = new();
                                    generatedTargetProperties.Add(typeSymbol, processedPropNameSet);
                                }
                                if (!processedPropNameSet.Add(targetPropertyName))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        SR.ConflictingAttributesOnPropertyDiagnostic,
                                        attributeSyntax.GetLocation(),
                                        (targetPropertyName.Length != 0 ? targetPropertyName : "*"),
                                        candidateType.Identifier.Text));
                                    continue;
                                }

                                var jsonableProperties = GetJsonableProperties(
                                    context, typeSymbol,
                                    config.includeInternals, config.excludeInherited, requireGetter: true, requireSetter: false,
                                    targetPropertyName);

                                if (targetPropertyName.Length != 0 &&
                                    !jsonableProperties.Any())
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        SR.TargetPropertyNotFoundDiagnostic,
                                        attributeSyntax.GetLocation(),
                                        targetPropertyName,
                                        candidateType.Identifier.Text));
                                    continue;
                                }

                                if (!config.extraData.TryGetValue(PreservePropertyOrder, out x) ||
                                    x is not bool preserveOrder ||
                                    !preserveOrder)
                                {
                                    // to aggregate same helper method calls
                                    jsonableProperties = jsonableProperties
                                        .OrderByDescending(x => x.Type.SpecialType is SpecialType.System_String)
                                        .ThenByDescending(x => x.Type.IsValueType)
                                        .ThenBy(x => (x.Type as INamedTypeSymbol)?.TypeParameters.Length ?? 0)
                                        .ThenBy(x => x.Type.SpecialType)
                                        .ThenBy(x => x.Type.TypeKind)
                                        .ThenBy(x => x.Type.Name)
                                        ;
                                }

                                // Generate serialization code
                                var sourceCode = Generator.GenerateToJson(context, typeSymbol, jsonableProperties.ToList(), targetPropertyName);
                                var fileNameSuffix = targetPropertyName.Length != 0 ? "+" + targetPropertyName : string.Empty;
                                context.AddSource($"{Utils.GetGenericAwareName(typeSymbol)}.{attrName}{fileNameSuffix}.g.cs", sourceCode);

                                // static resource generation request
                                addProperties(propertiesForSR, typeSymbol, jsonableProperties);
                            }
                            break;

                        case "FromJson":
                        case "FromJsonAttribute":
                            {
                                var config = Utils.GetAttributeConfiguration(model, attributeSyntax);//, "no extra property at this moment");

                                DEBUG_ReportConfig(context, attrName, typeSymbol.Locations.First(), config);

                                var jsonableProperties = GetJsonableProperties(
                                    context, typeSymbol,
                                    config.includeInternals, config.excludeInherited, requireGetter: false, requireSetter: true,
                                    targetPropertyName: string.Empty);

                                // Generate serialization code
                                var sourceCode = Generator.GenerateFromJson(context, typeSymbol, jsonableProperties.ToList());
                                context.AddSource($"{Utils.GetGenericAwareName(typeSymbol)}.{attrName}.g.cs", sourceCode);

                                // static resource generation request
                                addProperties(propertiesForSR, typeSymbol, jsonableProperties);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            // Generate static resources for the type
            foreach (var x in propertiesForSR)
            {
                context.AddSource(
                    $"{Utils.GetGenericAwareName(x.Key)}.SR.g.cs",
                    Generator.GenerateStaticResources(x.Key, x.Value)
                );
            }
        }

        private IEnumerable<IPropertySymbol> GetJsonableProperties(
            GeneratorExecutionContext context,
            INamedTypeSymbol typeSymbol,
            bool includeInternals,
            bool excludeInherited,
            bool requireGetter,
            bool requireSetter,
            string targetPropertyName
        )
        {
            var currentSymbol = typeSymbol;

        LOOP:
            foreach (var propertySymbol in currentSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (targetPropertyName.Length != 0 &&
                    propertySymbol.Name != targetPropertyName)
                {
                    continue;
                }

                if (excludeInherited)
                {
                    if (!propertySymbol.ContainingType.Equals(typeSymbol, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }
                }

                if (includeInternals)
                {
                    if (propertySymbol.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
                    {
                        continue;
                    }
                }
                else
                {
                    if (propertySymbol.DeclaredAccessibility is not Accessibility.Public)
                    {
                        continue;
                    }
                }

                if (requireGetter)
                {
                    // Property must have a getter
                    if (propertySymbol.GetMethod == null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            SR.SetOnlyPropertyNotSupportedDiagnostic,
                            propertySymbol.Locations.FirstOrDefault(),
                            propertySymbol.Name));
                        continue;
                    }
                }

                if (requireSetter)
                {
                    // Check for get-only properties (has getter, but no setter)
                    if (propertySymbol.SetMethod == null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            SR.GetOnlyPropertyNotSupportedDiagnostic,
                            propertySymbol.Locations.FirstOrDefault(),
                            propertySymbol.Name));
                        continue;
                    }

                    // Check for init-only properties (has setter, but it's init-only)
                    if (propertySymbol.SetMethod.IsInitOnly)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            SR.InitOnlyPropertyNotSupportedDiagnostic,
                            propertySymbol.Locations.FirstOrDefault(),
                            propertySymbol.Name));
                        continue;
                    }
                }

                yield return propertySymbol;
            }

            if (currentSymbol.BaseType is INamedTypeSymbol baseTypeSymbol)
            {
                currentSymbol = baseTypeSymbol;
                goto LOOP;
            }
        }

        sealed class JsonableSyntaxReceiver : ISyntaxReceiver
        {
            public HashSet<TypeDeclarationSyntax> CandidateTypes { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is not TypeDeclarationSyntax typeDeclarationSyntax)
                {
                    return;
                }

                foreach (var attribute in typeDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var attributeName = attribute.Name.ToString();
                    if (attributeName is "ToJson"
                                      or "ToJsonAttribute"
                                      or "FromJson"
                                      or "FromJsonAttribute")
                    {
                        CandidateTypes.Add(typeDeclarationSyntax);
                    }
                }
            }
        }
    }
}
