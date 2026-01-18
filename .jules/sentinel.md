## 2024-05-22 - [Data Leak in Global Request Logging]
**Vulnerability:** Global network logging utility was dumping full headers and bodies, exposing plain-text passwords and tokens.
**Learning:** Centralized logging utilities are a high-risk point for data leaks because they often lack context of what they are logging (e.g., treating a login request same as a config fetch).
**Prevention:** Implement redaction logic at the lowest level of the networking stack or logging utility to sanitize standard sensitive fields (password, token, Authorization header) before any output.
