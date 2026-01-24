## 2026-01-18 - Unity UI Feedback
**Learning:** Unity's standard UI buttons lack built-in audio feedback, requiring manual implementation in a base class or via a dedicated component.
**Action:** Use a base class like `MMButton` to centralize UX behaviors like click sounds, ensuring consistency across the application.

## 2026-10-24 - Base Class UI Inheritance
**Learning:** Adding programmatic `EventTrigger` logic to a base UI class (`MMButton`) effectively centralizes interaction feedback (like hover scale) regardless of prefab structure, but requires strict adherence to `base.Awake()` calls in derived classes (`MainMenuButton`) to function.
**Action:** Always verify derived classes call `base.Awake()` when enhancing base class initialization logic.

## 2026-01-20 - Accessible Button Feedback
**Learning:** Custom UI buttons that implement visual feedback via `EventTrigger` (like scaling on hover) must also handle `Select` and `Deselect` events to ensure keyboard and gamepad users receive the same feedback.
**Action:** Always pair `OnPointerEnter` with `OnSelect`, and `OnPointerExit` with `OnDeselect` when creating custom interaction feedback.
