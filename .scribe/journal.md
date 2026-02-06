# Scribe's Journal - Knowledge Gaps & Ambiguities

## ⚠️ Ambiguities
- **Backend Location**: The `Server/` directory mentioned in "Memory" is missing from the file listing. The repository appears to be Client-only (Unity).
- **Authentication**: `DefaultApiManager.cs` endpoints seem to rely on implicit auth or public access. Needs verification.
- **Service Locator**: The exact implementation of dependency injection is inferred but not fully visible (e.g., `Services` class).

## 📝 Discrepancies
- **Memory vs. Reality**: Memory mentions a .NET 8 backend solution in `Server/`, but `list_files` does not show it. Assuming this repo is strictly the Game Client.
- **Unity Version**: Memory stated Unity 6 (6000.3.6f1), but `ProjectSettings/ProjectVersion.txt` confirms **2022.3.62f3**.
- **Dev Script**: Memory mentioned `dev-start.ps1` for workflow orchestration, but it is missing from the repository.

## 🛡️ Missing Error Handling
- **API Responses**: Needs investigation on how non-200 OK responses are propagated to the UI.

## 🔍 Discoveries
- **Utils Location**: Found `Assets/Scripts/Services/Utils.cs` containing networking and logging helpers.
- **Server Bridge**: Found `Assets/Scripts/Services/Server/` which likely contains client-side interfaces for server bridges (SFS2X).
- **Configuration**: App is configured via `Assets/Resources/configs/AppConfig.json` (sample provided).
