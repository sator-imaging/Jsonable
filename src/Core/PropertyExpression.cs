using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;

namespace Jsonable.Core
{
    internal static class PropertyExpression
    {
        // TODO: define context object
        public static DiagnosticDescriptor? GenerateJson(
            GeneratorExecutionContext context,
            StringBuilder sb,
            ITypeSymbol propertyTypeSymbol,
            bool isSerializer,
            string variableName,
            ITypeSymbol resolvingTypeSymbol,
            int indentLevel,
            int localFunctionDepth)
        {
            // NOTE: to find local function declaration, search the following word.
            //       ---> localFuncName
            //       ---> localFuncParamName (write only)
            //       * local function need to take parameter parser/writer as `ref` because it may be struct.
            //       * assign collection to parameter BEFORE calling local func because local func of struct may not
            //         be allowed to access `this`.
            //       * recursive local function calls doesn't need to access `this`.
            //         just pass collection to local function.
            //       * `var` in local function is safe.

            string indent = new string(' ', indentLevel * 4);
            bool isNullable = resolvingTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            INamedTypeSymbol? namedTypeSymbol = resolvingTypeSymbol as INamedTypeSymbol;
            SpecialType specialType = (namedTypeSymbol?.ConstructedFrom ?? resolvingTypeSymbol).SpecialType;

            var resolvingTypeDisplayName = resolvingTypeSymbol.ToDisplayString();
            var resolvingTypeDisplayNameNonNullable = resolvingTypeDisplayName.TrimEnd('?');

            var goToFailedOrReturnFalse = localFunctionDepth > 0 ? "return false;" : "goto FAILED;";

#pragma warning disable IDE0010  // Add missing cases to switch statement
            switch (specialType)
#pragma warning restore IDE0010
            {
                case SpecialType.System_String:
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteString(writer, {variableName}, needEscape: true, emitMetadataComments);
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    {variableName} = parser.TakeString();
{indent}}}
"
                            );

                            return null;
                        }
                    }

                case SpecialType.System_Boolean:
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            sb.Append(
$@"{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteBoolean(writer, {variableName});
{indent}}}
"
);

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            sb.Append(
$@"{indent}if (Utf8Parser.TryParse(parser.RawSpan, out bool value, out consumed))
{indent}{{
{indent}    {variableName} = value;
{indent}    _ = parser.Skip(consumed);
{indent}}}
"
                            );

                            return null;
                        }
                    }

                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                //case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            sb.Append(
$@"{indent}{{
{indent}    hasNoError &= Utf8Formatter.TryFormat({variableName}, writer.GetSpan(40), out bytesWritten);  // Sufficient for all numbers
{indent}    writer.Advance(bytesWritten);
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            sb.Append(
$@"{indent}if (Utf8Parser.TryParse(parser.RawSpan, out {resolvingTypeDisplayNameNonNullable} value, out consumed))
{indent}{{
{indent}    {variableName} = value;
{indent}    _ = parser.Skip(consumed);
{indent}}}
"
                            );

                            return null;
                        }
                    }

                case SpecialType.System_Nullable_T:
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            if (namedTypeSymbol?.TypeArguments[0] is ITypeSymbol underlyingType)
                            {
                                sb.Append(
$@"{indent}if ({variableName}.HasValue)
"
                                );

                                var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                    $"{variableName}.Value", underlyingType, indentLevel /*+ 1*/, localFunctionDepth);
                                // return AFTER append

                                sb.Append(
$@"{indent}else
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
"
                                );

                                if (descriptor != null)
                                {
                                    return descriptor;
                                }
                                return null;
                            }

                            return SR.UnsupportedTypeDiagnostic;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            if (namedTypeSymbol?.TypeArguments[0] is ITypeSymbol underlyingType)
                            {
                                sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
{indent}{{
"
                                );

                                var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                    variableName, underlyingType, indentLevel + 1, localFunctionDepth);
                                // return AFTER append

                                sb.Append(
$@"{indent}}}
"
                                );

                                if (descriptor != null)
                                {
                                    return descriptor;
                                }
                                return null;
                            }

                            return SR.UnsupportedTypeDiagnostic;
                        }
                    }

                case SpecialType.System_DateTime:
                    {
                        return SR.DateTimeNotSupportedDiagnostic;
                    }

                // NOTE: these cases may not match when the property type is interface, etc.
                //       but may match if concrete type. it's not stable so do it in default case.
                case SpecialType.System_Array:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_ICollection_T:
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                default:

                    #region   STRUCT

                    // ENUM
                    if (resolvingTypeSymbol.TypeKind == TypeKind.Enum)
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            if (namedTypeSymbol?.EnumUnderlyingType is INamedTypeSymbol enumUnderlyingType)
                            {
                                sb.Append(
$@"{indent}{{
{indent}    hasNoError &= Utf8Formatter.TryFormat(({enumUnderlyingType.ToDisplayString()}){variableName}, writer.GetSpan(40), out bytesWritten);  // Sufficient for all numbers
{indent}    writer.Advance(bytesWritten);
{indent}}}
"
                                );

                                return null;
                            }

                            return SR.UnsupportedTypeDiagnostic;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            if (namedTypeSymbol?.EnumUnderlyingType is INamedTypeSymbol enumUnderlyingType)
                            {
                                sb.Append(
$@"{indent}if (Utf8Parser.TryParse(parser.RawSpan, out {enumUnderlyingType.ToDisplayString()} value, out consumed))
{indent}{{
{indent}    {variableName} = ({resolvingTypeDisplayName})value;
{indent}    _ = parser.Skip(consumed);
{indent}}}
"
                                );

                                return null;
                            }

                            return SR.UnsupportedTypeDiagnostic;
                        }
                    }

                    // struct
                    else if (resolvingTypeDisplayNameNonNullable == "System.DateTimeOffset")
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            // DO NOT use DateTimeOffset overload!
                            // it must be converted to DateTimeKind.Utc thru .UtcDateTime to append 'Z' suffix instead of +00:00
                            // --> 28 is hardcoded
                            // https://github.com/dotnet/runtime/blob/v5.0.0/src/libraries/System.Private.CoreLib/src/System/Globalization/DateTimeFormat.cs#L1197-L1200
                            const int UtcDateTimeLength = 28;

                            sb.Append(
$@"{indent}{{
{indent}    if (emitMetadataComments) {{ hasNoError &= ToJsonHelpers.TryWriteJsonableMetadata(writer, {UtcDateTimeLength}); }}
{indent}    hasNoError &= ToJsonHelpers.TryWriteChar(writer, '""');
{indent}    {{    
{indent}        hasNoError &= Utf8Formatter.TryFormat({variableName}.UtcDateTime, writer.GetSpan({UtcDateTimeLength}), out bytesWritten, '{JSONABLE.DateTimeFormat}');  // Sufficient for ""YYYY-MM-DDTHH:MM:SS.FFFFFFFZ""
{indent}        writer.Advance(bytesWritten);
{indent}    }}
{indent}    hasNoError &= ToJsonHelpers.TryWriteChar(writer, '""');
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            sb.Append(
$@"{indent}{{
{indent}    var date = parser.TakeStringBytes();
{indent}    if (Utf8Parser.TryParse(date, out DateTimeOffset value, out consumed, '{JSONABLE.DateTimeFormat}'))
{indent}    {{
{indent}        {variableName} = value;
{indent}    }}
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    // struct
                    else if (resolvingTypeDisplayNameNonNullable == "System.TimeSpan")
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            sb.Append(
$@"{indent}{{
{indent}    hasNoError &= Utf8Formatter.TryFormat({variableName}.TotalMilliseconds, writer.GetSpan(25), out bytesWritten);  // Sufficient for TimeSpan.TotalMilliseconds as double
{indent}    writer.Advance(bytesWritten);
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            sb.Append(
$@"{indent}if (Utf8Parser.TryParse(parser.RawSpan, out double value, out consumed))
{indent}{{
{indent}    {variableName} = TimeSpan.FromMilliseconds(value);
{indent}    _ = parser.Skip(consumed);
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    // struct
                    else if (resolvingTypeDisplayNameNonNullable == "System.Guid")
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            // https://github.com/dotnet/runtime/blob/v5.0.0/src/libraries/System.Private.CoreLib/src/System/Buffers/Text/Utf8Formatter/Utf8Formatter.Guid.cs#L34-L52
                            const int GuidLength = 36;

                            sb.Append(
$@"{indent}{{
{indent}    if (emitMetadataComments) {{ hasNoError &= ToJsonHelpers.TryWriteJsonableMetadata(writer, {GuidLength}); }}
{indent}    hasNoError &= ToJsonHelpers.TryWriteChar(writer, '""');
{indent}    {{
{indent}        hasNoError &= Utf8Formatter.TryFormat({variableName}, writer.GetSpan({GuidLength}), out bytesWritten, '{JSONABLE.GuidFormat}');  // Sufficient for GUID in D format
{indent}        writer.Advance(bytesWritten);
{indent}    }}
{indent}    hasNoError &= ToJsonHelpers.TryWriteChar(writer, '""');
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            sb.Append(
$@"{indent}{{
{indent}    var guid = parser.TakeStringBytes();
{indent}    if (Utf8Parser.TryParse(guid, out Guid value, out consumed, '{JSONABLE.GuidFormat}'))
{indent}    {{
{indent}        {variableName} = value;
{indent}    }}
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    #endregion

                    #region   REFERENCE TYPES

                    // class: Uri
                    else if (resolvingTypeDisplayNameNonNullable is "System.Uri")
                    {
                        return isSerializer ? WRITE() : READ();
                        DiagnosticDescriptor? WRITE()
                        {
                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    var uri = {variableName}.ToString();
{indent}    hasNoError &= ToJsonHelpers.TryWriteString(writer, uri, needEscape: true, emitMetadataComments);
{indent}}}
"
                            );

                            return null;
                        }
                        DiagnosticDescriptor? READ()
                        {
                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    {variableName} = new Uri(parser.TakeString());
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    // class: TO Jsonable
                    else if (
                        isSerializer &&
                        namedTypeSymbol?.GetAttributes().Any(ad => Utils.HasToJsonAttribute(ad)) == true)
                    {
                        return to();
                        DiagnosticDescriptor? to()
                        {
                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    {variableName}.ToJsonUtf8(writer, emitMetadataComments, emitByteOrderMark: false);
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    // class: FROM Jsonable
                    else if (
                        !isSerializer &&
                        namedTypeSymbol?.GetAttributes().Any(ad => Utils.HasFromJsonAttribute(ad)) == true)
                    {
                        if (!Utils.HasParameterlessConstructor(namedTypeSymbol))
                        {
                            return SR.ParameterlessConstructorRequiredDiagnostic;
                        }

                        return from();
                        DiagnosticDescriptor? from()
                        {
                            // NOTE: to make things simple, always set 'nest' to property after method call.
                            //       * property always returns copy of struct.
                            //         so, ref type can reduce instructions but don't try it. don't make things complicated.
                            //       * always check reference type existence even if it is not nullable.
                            //         it's required for array item reuse.
                            var reuseInstance = !propertyTypeSymbol.IsValueType
                                ? $"(reuseInstance && {variableName} != null) ? {variableName} : new()"
                                : $"new {resolvingTypeDisplayNameNonNullable}()"
                                ;

                            if (isNullable)
                            {
                                sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                );
                            }

                            sb.Append(
$@"{indent}{{
{indent}    var nest = {reuseInstance};

{indent}    var skip = nest.FromJsonable(parser.RawMemory, reuseInstance, throwIfSyntaxError);
{indent}    if (skip < 0)
{indent}    {{
{indent}        {goToFailedOrReturnFalse}
{indent}    }}
{indent}    _ = parser.Skip(skip);

{indent}    {variableName} = nest;
{indent}}}
"
                            );

                            return null;
                        }
                    }

                    // class: ICollection
                    else if (
                        specialType is SpecialType.System_Array or SpecialType.System_Collections_Generic_ICollection_T ||
                        resolvingTypeSymbol.AllInterfaces.Any(i => i.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_ICollection_T))
                    {
                        // Check for value type collection container
                        if (resolvingTypeSymbol.IsValueType)
                        {
                            return SR.ValueTypeCollectionContainerNotSupportedDiagnostic;
                        }

                        // Check for multiple ICollection implementations
                        if (resolvingTypeSymbol.AllInterfaces.Count(iface =>
                        {
                            return iface.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_ICollection_T;
                        }) > 1)
                        {
                            return SR.MultipleICollectionInterfacesDiagnostic;
                        }

                        // Collection: Dictionary<string, T>
                        // --> Check if it's ICollection<KeyValuePair<TKey, TValue>>
                        if (Utils.IsKeyValuePairCollection(resolvingTypeSymbol, context, out ITypeSymbol? keyType, out ITypeSymbol? valueType))
                        {
                            // Check for ambiguous collection type
                            const string GenericCollectionFullPrefix = "System.Collections.Generic.";
                            if (!(
                                    resolvingTypeDisplayName.StartsWith(GenericCollectionFullPrefix + "Dictionary<", StringComparison.Ordinal) ||
                                    resolvingTypeDisplayName.StartsWith(GenericCollectionFullPrefix + "IDictionary<", StringComparison.Ordinal) ||
                                    resolvingTypeSymbol.AllInterfaces.Any(i =>
                                        i.IsGenericType &&
                                        i.ConstructedFrom.ToDisplayString().StartsWith(GenericCollectionFullPrefix + "IDictionary<", StringComparison.Ordinal)
                                    )
                                )
                            )
                            {
                                return SR.AmbiguousCollectionDetectionDiagnostic;
                            }

                            // Serialize Key
                            if (keyType.SpecialType != SpecialType.System_String)
                            {
                                return SR.NonStringKeyDictionaryDiagnostic;
                            }

                            sb.Append(
$@"{indent}// Collection: Dictionary or IDictionary<TKey, TValue>
"
                            );

                            return isSerializer ? WRITE() : READ();
                            DiagnosticDescriptor? WRITE()
                            {
                                var localFuncName = $"write_map_{localFunctionDepth}";
                                var localFuncParamName = $"map_{variableName}_{localFunctionDepth}";

                                if (isNullable)
                                {
                                    sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                    );
                                }

                                sb.Append(
$@"{indent}{{
{indent}    hasNoError = {localFuncName}(ref writer, {variableName});
{indent}    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
{indent}    bool {localFuncName}(ref TWriter writer, {resolvingTypeDisplayNameNonNullable} {localFuncParamName})
{indent}    {{
{indent}        if (emitMetadataComments) {{ hasNoError &= ToJsonHelpers.TryWriteJsonableMetadata(writer, {localFuncParamName}.Count()); }}
{indent}        hasNoError &= ToJsonHelpers.TryWriteChar(writer, '{{');

{indent}        bool firstElement = true;
{indent}        foreach (var item in {localFuncParamName})
{indent}        {{
{indent}            var itemValue = item.Value;
{indent}            if (!firstElement)
{indent}            {{
{indent}                hasNoError &= ToJsonHelpers.TryWriteChar(writer, ',');
{indent}            }}
{indent}            firstElement = false;
"
                                );

                                sb.Append(
$@"
{indent}            hasNoError &= ToJsonHelpers.TryWriteKey(writer, item.Key, needEscape: true, emitMetadataComments);
"
                                );

                                // Serialize Value
                                var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                    "itemValue", valueType, indentLevel + 3, localFunctionDepth + 1);
                                // return AFTER append

                                sb.Append(
$@"{indent}        }}
{indent}        hasNoError &= ToJsonHelpers.TryWriteChar(writer, '}}');
{indent}        return hasNoError;
{indent}    }}
{indent}}}
"
                                );

                                if (descriptor != null)
                                {
                                    return descriptor;
                                }

                                return null;
                            }
                            DiagnosticDescriptor? READ()
                            {
                                var localFuncName = $"read_map_{localFunctionDepth}";
                                var sizeVariableName = $"size_{localFunctionDepth}";
                                var collectionVariableName = $"map_{localFunctionDepth}";

                                var concreteMapTypeName = resolvingTypeDisplayNameNonNullable;
                                if (resolvingTypeSymbol.IsAbstract)  // IsAbstract includes interface types
                                {
                                    int pos = concreteMapTypeName.IndexOf('<');
                                    if (pos < 0)
                                    {
                                        pos = concreteMapTypeName.Length;
                                    }

                                    concreteMapTypeName = $"System.Collections.Generic.Dictionary{concreteMapTypeName.Substring(pos)}";
                                }
                                else
                                {
                                    if (!Utils.HasParameterlessConstructor(resolvingTypeSymbol))
                                    {
                                        return SR.ParameterlessConstructorRequiredDiagnostic;
                                    }
                                }

                                var canHaveCapacityParameter = concreteMapTypeName.StartsWith("System.Collections.Generic.Dictionary<", StringComparison.Ordinal);

                                if (isNullable)
                                {
                                    sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                    );
                                }

                                sb.Append(
$@"{indent}{{
{indent}    int {sizeVariableName} = parser.TakeCollectionSizeOrNegative();
{indent}    _ = parser.Skip(1);  // '{{'
{indent}    if ({sizeVariableName} < 0)
{indent}    {{
{indent}        {goToFailedOrReturnFalse}
{indent}    }}

{indent}    var {collectionVariableName} = {variableName};
{indent}    if (!reuseInstance || {collectionVariableName} == null)
{indent}    {{
{indent}        {collectionVariableName} = new {concreteMapTypeName}({(canHaveCapacityParameter ? $"capacity: {sizeVariableName}" : string.Empty)});
{indent}    }}

{indent}    if ({sizeVariableName} == 0)
{indent}    {{
{indent}        _ = parser.Skip(1);  // '}}'
{indent}    }}
{indent}    else if (!{localFuncName}(ref parser, {collectionVariableName}))
{indent}    {{
{indent}        {goToFailedOrReturnFalse}
{indent}    }}

{indent}    {variableName} = {collectionVariableName};

{indent}    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
{indent}    bool {localFuncName}(ref Parser parser, {resolvingTypeDisplayNameNonNullable} {collectionVariableName})
{indent}    {{
{indent}        while ({sizeVariableName} > 0)
{indent}        {{
{indent}            {sizeVariableName}--;
{indent}            var mapKey = parser.TakeString();
{indent}            _ = parser.Skip(1);  // ':'
{indent}            {valueType.ToDisplayString()} mapValue = (((default)))!;  // ok
"
                                );

                                // Serialize Value
                                var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                    "mapValue", valueType, indentLevel + 3, localFunctionDepth + 1);
                                // return AFTER append

                                sb.Append(
$@"{indent}            {collectionVariableName}[mapKey] = mapValue;
{indent}            _ = parser.Skip(1);  // ',' or '}}'
{indent}        }}
{indent}        parser.SetItemType(ItemType.EndOfMap);
{indent}        return true;
{indent}    }}
{indent}}}
"
                                );

                                if (descriptor != null)
                                {
                                    return descriptor;
                                }

                                return null;
                            }
                        }

                        // Handle Base64, T[] and ICollection<T> as a JSON array
                        else
                        {
                            // IArrayTypeSymbol doesn't inherit from INamedTypeSymbol...!!
                            (bool isArray, ITypeSymbol? elementType)
                                = resolvingTypeSymbol is IArrayTypeSymbol arrayTypeSymbol
                                ? (true, arrayTypeSymbol.ElementType)
                                : (false, namedTypeSymbol?.TypeArguments.FirstOrDefault())
                                ;

                            if (elementType == null)
                            {
                                // Support non-generic type collection (eg., class FooCollection : ICollection<Foo>)
                                if (resolvingTypeSymbol.AllInterfaces.FirstOrDefault(i =>
                                        i.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_ICollection_T
                                    ) is INamedTypeSymbol iCollectionT)
                                {
                                    elementType = iCollectionT.TypeArguments[0];
                                }
                                else
                                {
                                    return SR.UnsupportedTypeDiagnostic;
                                }
                            }

                            // Collection: Base64
                            if (isArray && elementType.SpecialType == SpecialType.System_Byte)
                            {
                                sb.Append(
$@"{indent}// Collection: Base64
"
                                );

                                return isSerializer ? WRITE() : READ();
                                DiagnosticDescriptor? WRITE()
                                {
                                    if (isNullable)
                                    {
                                        sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                        );
                                    }

                                    sb.Append(
$@"{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteBase64(writer, {variableName}, emitMetadataComments);
{indent}}}
"
                                    );

                                    return null;
                                }
                                DiagnosticDescriptor? READ()
                                {
                                    if (isNullable)
                                    {
                                        sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                        );
                                    }

                                    sb.Append(
$@"{indent}{{
{indent}    var bin = parser.TakeBytesFromBase64({variableName}, reuseInstance);
{indent}    {variableName} = bin;
{indent}}}
"
                                    );

                                    return null;
                                }
                            }

                            // Collection: T[] or ICollection
                            else
                            {
                                sb.Append(
$@"{indent}// Collection: T[] or ICollection<T>
"
                                );

                                return isSerializer ? WRITE() : READ();
                                DiagnosticDescriptor? WRITE()
                                {
                                    var localFuncName = $"write_coll_{localFunctionDepth}";
                                    var localFuncParamName = $"coll_{variableName}_{localFunctionDepth}";

                                    if (isNullable)
                                    {
                                        sb.Append(
$@"{indent}if ({variableName} == null)
{indent}{{
{indent}    hasNoError &= ToJsonHelpers.TryWriteNull(writer);
{indent}}}
{indent}else
"
                                        );
                                    }

                                    sb.Append(
$@"{indent}{{
{indent}    hasNoError = {localFuncName}(ref writer, {variableName});
{indent}    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
{indent}    bool {localFuncName}(ref TWriter writer, {resolvingTypeDisplayNameNonNullable} {localFuncParamName})
{indent}    {{
{indent}        if (emitMetadataComments) {{ hasNoError &= ToJsonHelpers.TryWriteJsonableMetadata(writer, {localFuncParamName}.Count()); }}
{indent}        hasNoError &= ToJsonHelpers.TryWriteChar(writer, '[');

{indent}        bool firstElement = true;
{indent}        foreach (var item in {localFuncParamName})
{indent}        {{
{indent}            if (!firstElement)
{indent}            {{
{indent}                hasNoError &= ToJsonHelpers.TryWriteChar(writer, ',');
{indent}            }}
{indent}            firstElement = false;
"
                                    );

                                    var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                        "item", elementType, indentLevel + 3, localFunctionDepth + 1);
                                    // return AFTER append

                                    sb.Append(
$@"{indent}        }}
{indent}        hasNoError &= ToJsonHelpers.TryWriteChar(writer, ']');
{indent}        return hasNoError;
{indent}    }}
{indent}}}
"
                                    );

                                    if (descriptor != null)
                                    {
                                        return descriptor;
                                    }

                                    return null;
                                }
                                DiagnosticDescriptor? READ()
                                {
                                    var localFuncName = $"read_coll_{localFunctionDepth}";
                                    var sizeVariableName = $"size_{localFunctionDepth}";
                                    var collectionVariableName = $"collection_{localFunctionDepth}";
                                    var collectionIndexVariableName = $"collectionIndex_{localFunctionDepth}";
                                    var isArrayReusableVariableName = $"isArrayReusable_{localFunctionDepth}";

                                    string collectionVariableDeclaration;
                                    string assign;
                                    string funcParamDeclaration;
                                    string funcParams;
                                    if (isArray || resolvingTypeSymbol.IsAbstract)  // IsAbstract includes interface types
                                    {
                                        var elementTypeDisplayName = elementType.ToDisplayString();  // don't change element's nullability (int?[] cannot be int[])

                                        // support jag array
                                        string arrayCtor;
                                        if (elementTypeDisplayName.TrimEnd('?').EndsWith("[]", StringComparison.Ordinal))
                                        {
                                            // but don't support complex array (eg., int[][][] or int[]?[])
                                            if (elementTypeDisplayName.EndsWith("]?", StringComparison.Ordinal) ||
                                                elementTypeDisplayName.TrimEnd('?').EndsWith("][]", StringComparison.Ordinal))
                                            {
                                                return SR.UnsupportedTypeDiagnostic;
                                            }

                                            arrayCtor = elementTypeDisplayName.Substring(0, elementTypeDisplayName.Length - 2) + $"[{sizeVariableName}][]";
                                        }
                                        else
                                        {
                                            arrayCtor = $"{elementTypeDisplayName}[{sizeVariableName}]";
                                        }


                                        // bake #if directive into generated method.
                                        // instead using aggressive inlining helper method.
                                        collectionVariableDeclaration = $@"
{indent}    bool {isArrayReusableVariableName} = {(isArray ? $"(reuseInstance && {variableName}?.Length == {sizeVariableName})" : "false")};
{indent}    var {collectionVariableName} = {sizeVariableName} == 0
{indent}        ? Array.Empty<{elementTypeDisplayName}>()
{indent}        : {isArrayReusableVariableName}
{indent}            ? ((({(isArray ? variableName : "default")})))!  // ok
{indent}            :
#if NET7_0_OR_GREATER  // actual supported version is NET *5* but NET *6* target is used for older environment compatibility tests
{indent}                GC.AllocateUninitializedArray<{elementTypeDisplayName}>({sizeVariableName}, pinned: false);
#else
{indent}                new {arrayCtor};
#endif
{indent}    int {collectionIndexVariableName} = 0;
"
                                        ;

                                        assign = $@"{collectionVariableName}[{collectionIndexVariableName}] = collectionValue;
{indent}            {collectionIndexVariableName}++";

                                        // perf: captured variable by local function will be class field.
                                        //       to avoid unnecessary class field access as possible, take necessary variables as func param.
                                        //       ---> increment captured index variable leads 2 class field access. eg., field = field + 1;  // read and assign
                                        funcParamDeclaration = $", {elementTypeDisplayName}[] {collectionVariableName}, int {collectionIndexVariableName}, bool {isArrayReusableVariableName}";
                                        funcParams = $", {collectionVariableName}, {collectionIndexVariableName}, {isArrayReusableVariableName}";
                                    }
                                    else
                                    {
                                        if (!Utils.HasParameterlessConstructor(resolvingTypeSymbol))
                                        {
                                            return SR.ParameterlessConstructorRequiredDiagnostic;
                                        }

                                        var canHaveCapacityParameter = resolvingTypeDisplayName.StartsWith("System.Collections.Generic.List<", StringComparison.Ordinal);

                                        collectionVariableDeclaration = $@"
{indent}    var {collectionVariableName} = (reuseInstance && {variableName} != null) ? {variableName} : new {resolvingTypeDisplayNameNonNullable}({(canHaveCapacityParameter ? $"capacity: {sizeVariableName}" : string.Empty)});
"
                                        ;

                                        assign = $"{collectionVariableName}.Add(collectionValue)";

                                        funcParamDeclaration = $", {resolvingTypeDisplayNameNonNullable} {collectionVariableName}";
                                        funcParams = $", {collectionVariableName}";
                                    }

                                    if (isNullable)
                                    {
                                        sb.Append(
$@"{indent}if (parser.TryTakeNull())
{indent}{{
{indent}    {variableName} = null;
{indent}}}
{indent}else
"
                                        );
                                    }

                                    sb.Append(
$@"{indent}{{
{indent}    int {sizeVariableName} = parser.TakeCollectionSizeOrNegative();
{indent}    _ = parser.Skip(1);  // '['
{indent}    if ({sizeVariableName} < 0)
{indent}    {{
{indent}        {goToFailedOrReturnFalse}
{indent}    }}
{collectionVariableDeclaration}
{indent}    if ({sizeVariableName} == 0)
{indent}    {{
{indent}        _ = parser.Skip(1);  // ']'
{indent}    }}
{indent}    else if (!{localFuncName}(ref parser{funcParams}))
{indent}    {{
{indent}        {goToFailedOrReturnFalse}
{indent}    }}

{indent}    {variableName} = {collectionVariableName};

{indent}    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
{indent}    bool {localFuncName}(ref Parser parser{funcParamDeclaration})
{indent}    {{
{indent}        while ({sizeVariableName} > 0)
{indent}        {{
{indent}            {sizeVariableName}--;
{indent}            {elementType.ToDisplayString()} collectionValue = {(isArray ? $"{isArrayReusableVariableName} ? {collectionVariableName}[{collectionIndexVariableName}] : " : string.Empty)}(((default)))!;  // ok
"
                                    );

                                    var descriptor = GenerateJson(context, sb, propertyTypeSymbol, isSerializer,
                                        "collectionValue", elementType, indentLevel + 3, localFunctionDepth + 1);
                                    // return AFTER append

                                    sb.Append(
$@"{indent}            {assign};
{indent}            _ = parser.Skip(1);  // ',' or ']'
{indent}        }}
{indent}        parser.SetItemType(ItemType.EndOfArray);
{indent}        return true;
{indent}    }}
{indent}}}
"
                                    );

                                    if (descriptor != null)
                                    {
                                        return descriptor;
                                    }

                                    return null;
                                }
                            }
                        }
                    }

                    #endregion

                    break;
            }

            return SR.UnsupportedTypeDiagnostic;
        }
    }
}
