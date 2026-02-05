using System;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Component {
    public class UpdateValueCounter : MonoBehaviour {
        [SerializeField]
        private Text valueText;

        // Optimization: Cached RectTransform to avoid GetComponent calls in UpdateDisplay
        private RectTransform _valueTextRect;
        private double _value;

        private void Awake() {
            if (valueText != null) {
                _valueTextRect = valueText.GetComponent<RectTransform>();
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
            var valueTextRect = _valueTextRect;
            if (valueTextRect == null) {
                valueTextRect = valueText.GetComponent<RectTransform>();
                _valueTextRect = valueTextRect;
            }

            var zoomIn = valueTextRect.DOScale(1.4f, 0.2f);
            var running = DOTween.To(() => valueFrom, x => valueText.text = App.Utils.FormatBcoinValue(x), valueTo, 1);
            var zoomOut = valueTextRect.DOScale(1, 0.2f);

            DOTween.Sequence()
                .Append(zoomIn)
                .Append(running)
                .Append(zoomOut)
                .AppendCallback(() => { callback?.Invoke(); });
        }
    }
}