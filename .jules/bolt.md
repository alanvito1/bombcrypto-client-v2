## 2024-05-22 - Avoid GetComponent when type is known
**Learning:** `GetComponent` is O(N) on components. When using `is` or `as` to check types (e.g., `if (otherEntity is Bomb)`), we already have the reference to the specific component instance (since `Entity` inherits `MonoBehaviour`).
**Action:** Cast the object directly (e.g., `((Bomb)otherEntity)`) instead of calling `GetComponent<Bomb>()`.
