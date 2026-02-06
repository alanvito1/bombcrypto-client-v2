# Architecture: Bombcrypto Game Client

## 1. Context Diagram (C4 Level 1)
High-level relationship between the user, the client, and external systems.

```mermaid
C4Context
    title System Context Diagram for Bombcrypto Game Client

    Person(Player, "Player", "A user playing the game via WebGL")
    System(GameClient, "Game Client", "Unity WebGL Application running in browser")
    System_Ext(GameServer, "Game Server", "Backend API and SmartFoxServer")
    System_Ext(Blockchain, "Blockchain Network", "Smart Contracts (BSC/Polygon/Solana)")

    Rel(Player, GameClient, "Plays using", "Browser")
    Rel(GameClient, GameServer, "REST API / SFS2X Socket", "HTTPS/WSS")
    Rel(GameClient, Blockchain, "Reads/Writes", "Web3 Provider")
```

## 2. Container Diagram (C4 Level 2)
Internal structure of the Game Client application.

```mermaid
C4Container
    title Container Diagram for Game Client

    Container_Boundary(Client, "Unity Game Client") {
        Component(UI, "UI Layer", "Unity UI/Canvas", "Menus, HUD, Inventory")
        Component(Engine, "Game Engine", "Custom ECS", "Entities, Physics, Game Loop")
        Component(Services, "Service Layer", "C# Managers", "API, Auth, Assets, Audio")
        Component(Data, "Data Layer", "ScriptableObjects / JSON", "Configs, Static Data")
    }

    System_Ext(Server, "Backend Server", "API & Multiplayer")

    Rel(UI, Services, "Requests Data/Actions")
    Rel(UI, Engine, "Renders State")
    Rel(Engine, Services, "Syncs Game State")
    Rel(Services, Data, "Reads Config")
    Rel(Services, Server, "HTTP / Socket", "JSON/Binary")
```

## 3. Sequence Diagram: Get Coin Balance
Detailed flow for retrieving wallet balance via `DefaultApiManager`.

```mermaid
sequenceDiagram
    participant UI as User Interface
    participant API as DefaultApiManager
    participant Utils as Utils Helper
    participant Server as Game Server

    UI->>API: GetCoinBalance(walletAddress)
    API->>Utils: GetHost(Domain, "coin_balance", address)
    API->>Utils: GetWebResponse(logManager, url)
    Utils->>Server: GET /coin_balance?address=...
    Server-->>Utils: HTTP 200 JSON Response
    Note over Utils: Logs request URL and Response Body
    Utils-->>API: Returns (code, responseBody)

    alt Response is Valid
        API->>API: JObject.Parse(responseBody)
        alt Code == 0 (Success)
            API->>API: Extract "message" as double
            API-->>UI: Returns balance (double)
        else Code != 0
            API-->>UI: Throws Exception(message)
        end
    else Response is Empty/Error
        API-->>UI: Throws Exception
    end
```

## 4. Class Diagram: Entity Component System (ECS) Lite
The core game engine uses a lightweight ECS pattern involving `Entity` and `ComponentContainer`.

```mermaid
classDiagram
    class IEntity {
        <<interface>>
        +IsAlive: bool
        +Type: EntityType
        +GetEntityComponent<T>()
        +AddEntityComponent<T>()
    }

    class Entity {
        +IsAlive: ObscuredBool
        +Immortal: ObscuredBool
        +Type: EntityType
        +EntityManager: IEntityManager
        -_componentContainer: ComponentContainer
        +Kill(trigger: bool)
        +Resurrect()
    }

    class EntityLocation {
        +HashLocation: int
    }

    class ComponentContainer {
        +AddComponent<T>(component)
        +GetComponent<T>()
    }

    class IEntityComponent {
        <<interface>>
    }

    IEntity <|.. Entity
    Entity <|-- EntityLocation
    Entity *-- ComponentContainer
    ComponentContainer o-- IEntityComponent
```

## 5. Folder Structure Analysis
The `Assets/Scripts` directory is organized by domain and layer.

```text
Assets/Scripts/
├── Engine/          # Core Game Logic (ECS, Physics, Managers)
│   ├── Entities/    # Base Entity classes (Entity.cs, Bomb.cs)
│   ├── Component/   # Logic Components attached to Entities
│   └── Manager/     # Game State Managers
├── Services/        # External Communication & Data Services
│   ├── DefaultApiManager.cs  # REST API Handler
│   ├── Utils.cs              # Helper Functions (Networking, Logging)
│   └── Server/               # Server Bridge Interfaces
├── Data/            # Data Containers & ScriptableObjects
├── Config/          # Configuration Classes (AppConfig.cs)
├── UI/              # User Interface Scripts
└── Utils/           # General Purpose Utilities
```
