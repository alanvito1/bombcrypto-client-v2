# Bombcrypto Game Client

![Unity](https://img.shields.io/badge/Unity-2022.3-black?style=flat&logo=unity)
![Platform](https://img.shields.io/badge/Platform-WebGL-blue)
![Status](https://img.shields.io/badge/Status-Active-green)

> The official Unity WebGL client for the Bombcrypto blockchain game.

## 📚 Documentation
- **[System Atlas](SYSTEM_ATLAS.md)**: Full inventory of APIs, components, and data models.
- **[Architecture](ARCHITECTURE.md)**: Diagrams (Sequence, Class, C4) and structure analysis.

## 🏗️ Architecture Overview

```mermaid
C4Context
    title System Context Diagram for Bombcrypto Game Client

    Person(Player, "Player", "A user playing the game via WebGL")
    System(GameClient, "Game Client", "Unity WebGL Application running in browser")
    System_Ext(GameServer, "Game Server", "Backend API and WebSocket Server (.NET 8)")
    System_Ext(Blockchain, "Blockchain Network", "Smart Contracts (BSC/Polygon/Solana)")

    Rel(Player, GameClient, "Plays using", "Browser")
    Rel(GameClient, GameServer, "API Calls / Socket", "HTTPS/WSS")
    Rel(GameClient, Blockchain, "Reads/Writes", "Web3")
```

## 🚀 Quick Start

### Prerequisites
- Unity 2022.3.x
- `.NET 8` SDK (for backend compatibility checks)

### Setup
Run the following command to initialize the configuration:

```bash
cp Assets/Resources/configs/AppConfig.json.sample Assets/Resources/configs/AppConfig.json
```

## ⚠️ Important Note
This project **cannot operate as a standalone application**. A compatible backend server is required. Most sensitive credentials have been redacted.

---
*Maintained by Senspark*
