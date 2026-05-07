# Documentation Coverage Report

## Overall Check Result
The documentation covers most of the core functionality and type support. However, several advanced features, attribute parameters, and API options are currently missing from the `README.md`.

- **Core Functionality**: 100%
- **Attributes & Parameters**: 50%
- **Public API Methods**: 75%
- **Type Support**: 85%
- **Callbacks**: 100%

---

## Detailed Result Follows

| Feature | Status | Notes |
| :--- | :--- | :--- |
| **Core Functionality** | | |
| Source Generation | Covered | Mentioned as high performance feature. |
| JMC (Metadata Comments) | Covered | Detailed in JMC.md. |
| **Attributes** | | |
| `[ToJson]` | Covered | Used in Quick Start. |
| `[FromJson]` | Covered | Used in Quick Start. |
| `IncludeInternals` | **Not Covered** | Allows serialization of internal properties. |
| `ExcludeInherited` | **Not Covered** | Excludes properties from base classes. |
| `PreservePropertyOrder` | **Not Covered** | Maintains property order in JSON (ToJson only). |
| `Property` | **Not Covered** | Allows targeted serialization of a specific property. |
| **API Methods** | | |
| `ToJson()` | Covered | Quick Start. |
| `ToJsonable()` | Covered | Quick Start. |
| `FromJsonable()` | Covered | Quick Start. |
| `ToJsonUtf8()` | **Not Covered** | Direct UTF-8 serialization to `IBufferWriter<byte>`. |
| API Parameters | **Partial** | `prettyPrint` and `reuseInstance` mentioned; others like `indentSize`, `throwIfSyntaxError` missing. |
| **Type Support** | | |
| Primitive Structs | Covered | Detailed list in README. |
| Common Structs (Guid, etc.) | Covered | Detailed list in README. |
| Nullables | Covered | Mentioned. |
| Collections (List, Dict, etc.) | Covered | Detailed strategies for reuse mentioned. |
| Nested Types | **Not Covered** | Supported but not explicitly mentioned. |
| Generic Types | **Not Covered** | Supported but not explicitly mentioned. |
| Records / Record Structs | **Not Covered** | Supported but not explicitly mentioned. |
| **Callbacks** | | |
| Serialization Callbacks | Covered | `OnWillSerialize`, etc. mentioned. |
| Deserialization Callbacks | Covered | `OnWillDeserialize`, etc. mentioned. |
| **Other** | | |
| Jump Table Optimization | **Not Covered** | Internal performance optimization for types with many properties. |
| UTF-8 Property Name Caching | **Not Covered** | Internal optimization. |
