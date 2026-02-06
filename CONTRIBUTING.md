# Contributing to Bombcrypto Game Client

Thank you for your interest in contributing to the Bombcrypto Game Client! This document provides guidelines for setting up your environment and submitting contributions.

## 🤝 Code of Conduct
We expect all contributors to adhere to our Code of Conduct. Please be respectful, inclusive, and professional in all interactions.

## 🛠️ Setup & Build

### Prerequisites
- **Unity Version**: `2022.3.62f3` (Strict requirement).
- **IDE**: JetBrains Rider (Recommended) or Visual Studio 2022.
- **Git LFS**: Ensure Git Large File Storage is installed.

### Initial Setup
1. **Clone the Repository**:
   ```bash
   git clone <repo-url>
   ```
2. **Setup Configuration**:
   The project requires a valid `AppConfig.json` to run.
   ```bash
   cp Assets/Resources/configs/AppConfig.json.sample Assets/Resources/configs/AppConfig.json
   ```
3. **Open in Unity**:
   - Add the project to Unity Hub.
   - Open the project.
   - Wait for the package manager to resolve dependencies.

### Building
- **Platform**: WebGL.
- **Build Settings**: Ensure "Development Build" is checked for debugging.
- **Output**: Builds are typically generated in `Builds/WebGL/`.

## 🎨 Code Style

### C# Conventions
We follow standard C# coding conventions with slight modifications for Unity:

- **Classes/Methods/Properties**: `PascalCase`.
- **Private Fields**: `_camelCase` (e.g., `_isProduction`).
- **Local Variables**: `camelCase`.
- **Constants**: `UPPER_CASE_SNAKE` (e.g., `GET_COIN_BALANCE`).

### Unity Specifics
- **Serialization**: Use `[SerializeField]` for private fields exposed to the Inspector.
- **GetComponent**: Cache `GetComponent` results in `Awake` or `Start`. Avoid calling in `Update`.
- **Tags/Layers**: Use `CompareTag()` instead of string comparison.

## 🔄 Pull Request Process

1. **Fork & Branch**: Create a new branch for your feature or fix.
   - Feature: `feature/name-of-feature`
   - Fix: `fix/name-of-bug`
2. **Commit Messages**: Use clear, descriptive commit messages.
   - Example: `feat: add new inventory slot UI`
   - Example: `fix: resolve null reference in ApiManager`
3. **Tests**: Ensure your changes do not break existing functionality. Run any available PlayMode/EditMode tests.
4. **Submit PR**: Open a Pull Request against the `main` or `develop` branch.
5. **Review**: Wait for code review feedback and address any comments.

## 🐛 Reporting Issues
If you encounter a bug, please open an issue with:
- Steps to reproduce.
- Expected vs. Actual behavior.
- Screenshots or Logs (redact sensitive info).

---
*Happy Coding!*
