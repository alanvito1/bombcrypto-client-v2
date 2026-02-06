using System;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Component {
    public class UpdateValueCounter : MonoBehaviour {
        [SerializeField]
        private Text valueText;

        private double _value;
        private RectTransform _rectTransform;
        private Sequence _sequence;

        private void Awake() {
            if (valueText != null) {
                _rectTransform = valueText.GetComponent<RectTransform>();
            }
        }

        public void SetValue(double value, bool isCounter = false) {
            if (Math.Abs(_value - value) < Mathf.Epsilon) {
                return;
            }

            if (isCounter) {
                UpdateDisplay((int) _value, (int) value, () => {
                    _value = value;
                    valueText.text = App.Utils.FormatBcoinValue(value);
                });
            } else {
                _value = value;
                valueText.text = App.Utils.FormatBcoinValue(value);
            }
        }

        public string GetText() {
            return valueText.text;
        }
        
        private void UpdateDisplay(int valueFrom, int valueTo, System.Action callback) {
            // OPTIMIZATION: Kill previous sequence to prevent overlapping animations and CPU waste
            _sequence?.Kill();

            var valueTextRect = _rectTransform;
            if (valueTextRect == null) {
                // Fallback: Cache the RectTransform if it wasn't captured in Awake (e.g., dynamically assigned)
                valueTextRect = valueText.GetComponent<RectTransform>();
                _rectTransform = valueTextRect;
            }

            var zoomIn = valueTextRect.DOScale(1.4f, 0.2f);
            var running = DOTween.To(() => valueFrom, x => valueText.text = App.Utils.FormatBcoinValue(x), valueTo, 1);
            var zoomOut = valueTextRect.DOScale(1, 0.2f);

            _sequence = DOTween.Sequence();
            _sequence
                .Append(zoomIn)
                .Append(running)
                .Append(zoomOut)
                .AppendCallback(() => { callback?.Invoke(); });
        }
    }
}
