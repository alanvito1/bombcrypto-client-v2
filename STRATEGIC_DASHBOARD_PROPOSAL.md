# 📈 Pulse: Strategic Dashboard & KPIs Proposal

## 1. Executive Summary
This proposal outlines a **Strategic Dashboard** ("Pulse Tab") for the Game Client.
**Platform Decision**: After analyzing the architecture, we determined the dashboard must reside **inside the Unity Client**.
*   **Reason**: Critical data sources like `InventoryManager` (Heroes/Items) and `DailyMissionManager` operate via **SmartFoxServer (SFS)** sockets within the Unity instance. Replicating this in the React Web Wrapper would require complex SFS duplication or new Backend APIs (violating the Zero-Backend Policy).
*   **Goal**: Provide players with a "God-view" of their economy, roster, and engagement using existing client-side state.

## 2. 🔍 Data Mining: Mapping the Payload

We have identified rich data sources currently flowing into the client that can be aggregated for deeper insights.

| Data Source (Class) | Method / Property | Raw Data Fields | Strategic Insight |
| :--- | :--- | :--- | :--- |
| `DefaultApiManager` (REST) | `GetCoinBalance(wallet)` | `double` (Balance) | **Liquid Wealth**: Real-time spending power. |
| `InventoryManager` (SFS) | `GetHeroesAsync()` | `InventoryHeroData` (`Rarity`, `Level`, `Active`, `Stats`) | **Roster Valuation**: Total army strength and potential. |
| `InventoryManager` (SFS) | `GetChestAsync()` | `InventoryChestData` (`Type`, `RemainingTime`) | **Pipeline Value**: Pending rewards and unlock efficiency. |
| `DefaultDailyMissionManager` (SFS) | `GetDailyMission()` | `IDailyMission` (`RequestTimes`, `CompletedTimes`) | **Engagement Velocity**: How fast the user clears content. |
| `DefaultApiManager` (REST) | `GetMyMatches()` | `PvpMatchSchedule` (`Players`, `Score`, `Status`) | **Combat Performance**: Win rates and activity trends. |
| `InventoryManager` (SFS) | `GetItemsAsync()` | `InventoryItemData` (`Quantity`, `Type`) | **Resource Depth**: Stockpile of consumables/materials. |

## 3. 📝 The KPI Scorecards

These 5 High-Level Metrics will answer critical questions in < 5 seconds.

### 1. **Net Account Value (NAV)**
*   **Question**: "How rich am I?"
*   **Logic**: `CoinBalance` + `Sum(Hero Estimated Value)` + `Sum(Inventory Item Value)`.
*   **Data Source**: `DefaultApiManager.GetCoinBalance` + `InventoryManager.GetHeroesAsync`.

### 2. **Roster Efficiency Score**
*   **Question**: "Is my army optimized?"
*   **Logic**: `(Avg Hero Level / Max Level) * (Active Heroes / Total Heroes)`.
*   **Data Source**: `InventoryManager.GetHeroesAsync`.

### 3. **Mission Completion Rate**
*   **Question**: "Am I productive today?"
*   **Logic**: `Sum(Mission.CompletedTimes) / Sum(Mission.RequestTimes)` %.
*   **Data Source**: `DefaultDailyMissionManager.GetDailyMission()`.

### 4. **PvP Win Rate (Recent)**
*   **Question**: "Am I winning?"
*   **Logic**: Filter `GetMyMatches` for "Ended" status -> Calculate % where `MyScore > OpponentScore`.
*   **Data Source**: `DefaultApiManager.GetMyMatches`.

### 5. **Unlock Pipeline Efficiency**
*   **Question**: "Am I wasting time?"
*   **Logic**: % of Chest slots occupied vs % of Chests currently unlocking. Alert if slots are full but no timer is running.
*   **Data Source**: `InventoryManager.GetChestAsync`.

## 4. 📊 Interactive Charts (Visual Mockups)

### Chart A: Rarity Distribution (Pie Chart)
*   **Insight**: Diversity of the Hero Roster.
*   **Visualization**: Donut chart showing breakdown of Common vs Rare vs Legendary heroes.
*   **Interaction**: Click a slice to filter the "Hero List" below.
*   **Tech**: Unity UI `Image.fillAmount` or `RectMask2D` (Library Agnostic).

### Chart B: Resource Accumulation (Grouped Bar Chart)
*   **Insight**: Hoarding trends vs usage.
*   **Visualization**: Bars for Top 5 Resources (e.g., Upgrade Materials, Tickets).
*   **Tech**: Horizontal Bar using standard UI Layout Groups.

### Chart C: Daily Activity Heatmap (GitHub Style)
*   **Insight**: When does the player play?
*   **Visualization**: 7x5 Grid based on `PvpMatchSchedule.StartTimestamp`. Darker blocks = more matches.
*   **Tech**: Grid Layout Group with dynamically colored Images.

## 5. 🎛 Universal Filters & Logic

Slice the data entirely on the Front-end.

*   **Timeframe**: "Last 24 Hours", "Last 7 Days", "All Time"
    *   *Implementation*: `matches.Where(m => m.StartTimestamp > cutoff)`
*   **Hero Rarity**: "Common", "Rare", "Legendary"
    *   *Implementation*: `heroes.Where(h => h.Rarity == SelectedRarity)`
*   **Mode**: "PvP", "Story", "Tournament"
    *   *Implementation*: `matches.Where(m => m.Mode == SelectedMode)`

## 6. ✨ Technical Implementation Strategy

### The `DashboardViewModel`
We will introduce a non-MonoBehaviour View Model to aggregate data streams.

```csharp
public class DashboardViewModel {
    private readonly IInventoryManager _inventory;
    private readonly IApiManager _api;
    private readonly IDailyMissionManager _missions;

    // Reactive Properties (using UniRx or C# Events)
    public double NetWorth { get; private set; }
    public float WinRate { get; private set; }

    public async Task RefreshDashboard() {
        // 1. Parallel Fetch
        var tasks = new Task[] {
            _inventory.GetHeroesAsync(),
            _api.GetCoinBalance(wallet),
            _missions.SyncData()
        };
        await Task.WhenAll(tasks);

        // 2. Calculate KPIs (CPU bound, run on thread pool if heavy)
        CalculateNetWorth();
        CalculateWinRate();
    }
}
```

### Performance Considerations
*   **Memoization**: Cache results of `GetHeroesAsync` transformation. Only recalculate if `InventoryManager` signals an update.
*   **Lazy Loading**: Only fetch `GetMyMatches` when the "PvP" tab of the dashboard is active.
*   **UI Pooling**: Use object pooling for the "Match History" list to avoid GC spikes.

---
*Pulse 📈 - Turning Data into Decisions.*
