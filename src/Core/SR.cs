using Microsoft.CodeAnalysis;

#pragma warning disable RS2008  // Enable analyzer release tracking

namespace Jsonable.Core
{
    internal static class SR
    {
        // always be private.
        // it can be exposed by declaring property as readonly or read/write interface, if required.
        public const string ResourceDataAccessibility = "private";
        public const string ResourceDataPrefix = "Jsonable_";
        public const string Utf8NamesClass = ResourceDataPrefix + "Utf8PropertyNames";

        public const string OnWillSerialize = nameof(OnWillSerialize);
        public const string OnDidSerialize = nameof(OnDidSerialize);
        public const string OnWillDeserialize = nameof(OnWillDeserialize);
        public const string OnDidDeserialize = nameof(OnDidDeserialize);


        public const string DiagnosticCategory = nameof(Jsonable);
        public const string DiagnosticPrefix = "JABL";

        // traversal time diagnostics
        public static DiagnosticDescriptor SetOnlyPropertyNotSupportedDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "001",
            "Set-only property not supported",
            "Property '{0}' is set-only and cannot be serialized. A getter is required.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor GetOnlyPropertyNotSupportedDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "002",
            "Get-only property not supported",
            "Property '{0}' is get-only and cannot be deserialized. A setter is required.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor InitOnlyPropertyNotSupportedDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "003",
            "Init-only property not supported",
            "Property '{0}' is init-only and cannot be deserialized. A setter is required.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        // type-level diagnostics
        public static DiagnosticDescriptor MissingPartialKeywordDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "011",
            "Missing partial keyword",
            "Type '{0}' must be declared as partial to be processed by the Jsonable source generator",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor TargetPropertyNotFoundDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "012",
            "Target property not found",
            "Target property name '{0}' not found in type '{1}'",
            DiagnosticCategory,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ConflictingAttributesOnPropertyDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "013",
            "Conflicting attributes on property",
            "Cannot apply multiple attributes to the same property '{0}' in type '{1}'",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ContainingTypeNotPartialDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "014",
            "Containing type not partial",
            "Containing type '{0}' must be declared as partial to be processed by the Jsonable source generator",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        // generation-time diagnostics
        public static DiagnosticDescriptor DateTimeNotSupportedDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "021",
            "DateTime not supported",
            "'{0}' is not supported. Use 'DateTimeOffset' instead.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor NonStringKeyDictionaryDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "022",
            "Non-string key dictionary not supported",
            "'{0}' is an IDictionary<TKey, TValue> where TKey is not a string. Only string keys are supported for JSON object serialization/deserialization.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor UnsupportedTypeDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "023",
            "Unsupported Property Type",
            "'{0}' is unsupported type for JSON serialization/deserialization",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor AmbiguousCollectionDetectionDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "024",
            "Ambiguous collection detection",
            "Type '{0}' is an IEnumerable<KeyValuePair<TKey, TValue>>. Consider using IDictionary<TKey, TValue> for clearer JSON object serialization/deserialization.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ParameterlessConstructorRequiredDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "025",
            "Parameterless constructor required",
            "Type '{0}' requires a parameterless constructor to be deserialized from JSON",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor MultipleICollectionInterfacesDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "026",
            "Multiple ICollection interfaces detected",
            "Type '{0}' implements multiple ICollection interfaces. Only one ICollection implementation is supported for JSON serialization/deserialization.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ValueTypeCollectionContainerNotSupportedDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "027",
            "Value type collection container not supported",
            "Type '{0}' is a value type. Only reference type collection container is supported for JSON serialization/deserialization.",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        // DEBUG
        public static DiagnosticDescriptor DebugMessageDiagnostic = new DiagnosticDescriptor(
            DiagnosticPrefix + "xDBG",
            "Jsonable Debug Message",
            "{0}", // Generic message for debugging
            DiagnosticCategory,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);
    }
}
