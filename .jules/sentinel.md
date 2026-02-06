## 2024-05-22 - Partial JSON Redaction
**Vulnerability:** `Utils.RedactSensitiveData` regex only matched string values, leaving sensitive integers, booleans, and nulls unredacted in logs.
**Learning:** Naive regex matching for JSON redaction often overlooks primitive types. A robust regex must account for non-string values without consuming structural characters (like `{` or `[`) to avoid corrupting the JSON structure, which renders logs unreadable or misleading.
**Prevention:** Use a regex pattern that alternates between matching a full string and matching non-structural, non-whitespace characters: `(?:""(?:[^""\\]|\\.)*""|[^""\[\{,}\]\s]+)`.
