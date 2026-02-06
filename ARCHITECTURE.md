# Architecture: Bombcrypto Game Client

## 1. Context Diagram (C4 Level 1)
This diagram illustrates the high-level relationship between the user, the client, and the external systems.

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

## 2. Sequence Diagram: Get Coin Balance
This sequence details the flow when the client requests the user's balance.

```mermaid
sequenceDiagram
    participant UI as User Interface
    participant API as DefaultApiManager
    participant Utils as Utils Helper
    participant Server as Game Server

    UI->>API: GetCoinBalance(walletAddress)
    API->>Utils: GetWebResponse(url)
    Utils->>Server: GET /coin_balance?address=...
    Server-->>Utils: HTTP 200 JSON Response
    Note over Utils: Logs response (Potential Security Risk)
    Utils-->>API: Returns (code, responseBody)
    loop Parse JSON
        API->>API: JObject.Parse(responseBody)
        API->>API: Extract "message" (balance)
    end
    API-->>UI: Returns double (balance)

    alt Error Case
        Server-->>Utils: HTTP 500 / Error
        API->>API: Throws Exception
        API-->>UI: Error Propagation
    end
```

## 3. Class Diagram: Entity Component System (ECS) Lite
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

## 4. Folder Structure Analysis
The `Assets/Scripts` directory is organized by domain and layer.

```text
Assets/Scripts/
├── Engine/          # Core Game Logic (ECS, Physics, Managers)
│   ├── Entities/    # Base Entity classes (Entity.cs, Bomb.cs)
│   ├── Component/   # Logic Components attached to Entities
│   └── Manager/     # Game State Managers
├── Services/        # External Communication & Data Services
│   ├── DefaultApiManager.cs  # REST API Handler
│   ├── Utils.cs              # Helper Functions
│   └── ...                   # Specific Feature Managers (Shop, Inventory)
├── Data/            # Data Containers & ScriptableObjects
├── UI/              # User Interface Scripts
└── Utils/           # General Purpose Utilities
```
