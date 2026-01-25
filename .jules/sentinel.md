## 2024-05-22 - [Data Leak in Global Request Logging]
**Vulnerability:** Global network logging utility was dumping full headers and bodies, exposing plain-text passwords and tokens.
**Learning:** Centralized logging utilities are a high-risk point for data leaks because they often lack context of what they are logging (e.g., treating a login request same as a config fetch).
**Prevention:** Implement redaction logic at the lowest level of the networking stack or logging utility to sanitize standard sensitive fields (password, token, Authorization header) before any output.

## 2024-10-14 - [URL Injection in Claim Manager]
**Vulnerability:** DefaultClaimManager constructed URLs by directly concatenating user account identifiers, allowing for potential path traversal or query parameter injection if the identifier contained special characters.
**Learning:** Even seemingly safe identifiers like wallet addresses or usernames should be treated as untrusted input when constructing URLs, as they might be manipulated or contain unexpected characters.
**Prevention:** Always use Uri.EscapeDataString() when inserting dynamic values into URL paths or query strings.

## 2024-11-19 - [Incomplete Sensitive Data Redaction]
**Vulnerability:** The centralized logging redaction list missed `newPassword`, causing it to be logged in plain text during password reset operations.
**Learning:** Allow-lists or Block-lists for sensitive keys are brittle and require constant maintenance as API contracts evolve (e.g., `newPassword`, `confirmPassword`).
**Prevention:** Periodically audit logging utilities against API definitions to ensure all sensitive fields are covered, or use a "safe by default" logging approach that only logs whitelisted non-sensitive fields.
