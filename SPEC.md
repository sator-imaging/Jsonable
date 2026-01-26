# Json with Metadata Comments (JMC) Specification

This document outlines the specification for Json with Metadata Comments (JMC), a format that embeds metadata within a JSON structure using comments.

## 1. Formatting

The JMC format does not support "prettified" JSON. Newlines, indentation, and other whitespace used for formatting are not allowed. The examples in this document may include newlines and indentation for readability, but these are not part of the JMC specification. The entire JMC output must be a single line of text without any extraneous whitespace.

## 2. Object Header

A JMC object is identified by a specific header prepended to the JSON object.

- **Header:** `/*JMC1*/`
- **Purpose:** This header serves as a version identifier for the JMC format. The `1` in `JMC1` indicates version 1 of the specification.

### Example:

```jsonc
/*JMC1*/{
  "key": "value"
}
```

## 3. Length Metadata

Length metadata is prepended to arrays and strings (including property keys) to indicate the byte length of the element.

- **Format:** `/*<encoded_length>*/`
- **Structure:**
    - The metadata is a 6-byte block.
    - `/*` (2 bytes): The opening of the comment.
    - `<encoded_length>` (2 bytes): A 2-byte little-endian `ushort` representing the encoded length.
    - `*/` (2 bytes): The closing of the comment.
- **Encoding:**
    - The actual byte length of the string or array is encoded by adding a constant offset: `encoded_length = actual_length + 0x0130`.
    - This offset is used to prevent the encoded length from creating byte sequences that could be misinterpreted, such as `*/` (`0x2a2f`).

### Example:

- A string `"hello"` has a UTF-8 byte length of 5.
- The encoded length would be `5 + 0x0130 = 0x0135`.
- In little-endian format, this is `35 01`.
- The resulting metadata comment would be `/*\x35\x01*/`.

```jsonc
{
  /*\x2c\x01*/"my_key": /*\x35\x01*/"hello"
}
```
