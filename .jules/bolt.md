## 2024-05-22 - Avoid GetComponent when type is known
**Learning:** `GetComponent` is O(N) on components. When using `is` or `as` to check types (e.g., `if (otherEntity is Bomb)`), we already have the reference to the specific component instance (since `Entity` inherits `MonoBehaviour`).
**Action:** Cast the object directly (e.g., `((Bomb)otherEntity)`) instead of calling `GetComponent<Bomb>()`.

## 2024-10-25 - Collision Handling Optimization
**Learning:** `CollisionDetector` caches and passes the specific `Entity` subclass (e.g., `Wall`, `Bomb`) to collision listeners. This means `GetComponent<T>` is redundant in collision callbacks (like `WalkThrough.HitObstacle`) because the passed `Entity` reference *is* the component.
**Action:** Use pattern matching (`is T variable`) directly on the `Entity` parameter instead of `GetComponent<T>()` in collision logic.

## 2024-10-26 - StringBuilder for Frequent String Updates
**Learning:** `TimeUtil` methods used string concatenation (`+=`) in conditionals, which creates excessive garbage in frequent updates (like UI timers).
**Action:** Use `StringBuilder` for helper methods that construct strings dynamically, especially if likely to be called every frame or second.
