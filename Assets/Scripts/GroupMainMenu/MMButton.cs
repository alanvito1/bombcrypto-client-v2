using System;
using System.Net.Http.Headers;

using App;
using DG.Tweening;
using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMButton : MonoBehaviour, ISelectHandler, IDeselectHandler {
        [SerializeField]
        protected Button button;

        [SerializeField]
        private TextMeshProUGUI counterText;

        [SerializeField]
        private Image[] images;

        public Button Button => button;

        private System.Action _onClickedCallback;
        private Vector3 _originalScale;
        private bool _isHovered;
        private bool _isSelected;

        public bool Interactable {
            get => button.interactable;
            set {
                InteractableColor(value);
                button.interactable = value;
                if (!value) {
                    transform.DOKill();
                    transform.localScale = _originalScale;
                }
            }
        }

        private void InteractableColor(bool value) {
            button.interactable = value;
            var color = value ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.9f);
            foreach (var iter in images) {
                iter.color = color;
            }
        }

        public void SetInteractable(bool value) {
            Interactable = value;
        }
        
        public void SetVisible(bool value) {
            button.gameObject.SetActive(value);
        }

        public void SetCounter(string text) {
            if (string.IsNullOrEmpty(text)) {
                counterText.gameObject.SetActive(false);
                return;
            }
            counterText.gameObject.SetActive(true);
            counterText.text = text;
        }
        
        protected virtual void Awake() {
            button.onClick.AddListener(OnClicked);

            _originalScale = transform.localScale;
            AddHoverEvents();
        }

        private void AddHoverEvents() {
            var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null) eventTrigger = button.gameObject.AddComponent<EventTrigger>();

            AddEventTrigger(eventTrigger, EventTriggerType.PointerEnter, OnPointerEnter);
            AddEventTrigger(eventTrigger, EventTriggerType.PointerExit, OnPointerExit);
            AddEventTrigger(eventTrigger, EventTriggerType.PointerDown, OnPointerDown);
            AddEventTrigger(eventTrigger, EventTriggerType.PointerUp, OnPointerUp);
        }

        private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action) {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        public void OnSelect(BaseEventData eventData) {
            _isSelected = true;
            UpdateScale();
        }

        public void OnDeselect(BaseEventData eventData) {
            _isSelected = false;
            UpdateScale();
        }

        private void UpdateScale(float duration = 0.2f) {
            if (Interactable) {
                var targetScale = (_isHovered || _isSelected) ? 1.05f : 1.0f;
                transform.DOScale(_originalScale * targetScale, duration).SetEase(Ease.OutQuad);
            }
        }

        private void OnPointerEnter(BaseEventData data) {
            _isHovered = true;
            UpdateScale();
        }

        private void OnPointerExit(BaseEventData data) {
            _isHovered = false;
            UpdateScale();
        }

        private void OnPointerDown(BaseEventData data) {
            if (Interactable) {
                transform.DOScale(_originalScale * 0.95f, 0.1f).SetEase(Ease.OutQuad);
            }
        }

        private void OnPointerUp(BaseEventData data) {
            UpdateScale(0.1f);
        }

        public void SetOnClickedCallback(System.Action callback) {
            _onClickedCallback = callback;
        }

        private void OnClicked() {
            try {
                ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            } catch (Exception) {
                // Ignore
            }
            _onClickedCallback?.Invoke();
        }
    }
}
