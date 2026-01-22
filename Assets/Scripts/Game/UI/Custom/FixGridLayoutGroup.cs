using System.Threading.Tasks;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Custom {
    public class FixGridLayoutGroup : MonoBehaviour, ILayoutGroup {
        public int minColInput = 2;
        public int minRowInput = 2;
        public int minSpaceInput = 4;

        public int ColMin => _colMin;
        public int RowMin => _rowMin;
        public bool IsLoad => _isLoad;

        public Task<bool> WaitLayoutDone {
            get {
                if (_isLoad) {
                    return Task.FromResult(true);
                }
                _waitLayoutDone ??= new TaskCompletionSource<bool>();
                return _waitLayoutDone.Task;
            }
        }

        private int _colMin = 0;
        private int _rowMin = 0;
        private bool _isLoad = false;
        private TaskCompletionSource<bool> _waitLayoutDone;

        // Cache components
        private GridLayoutGroup _gridLayoutGroup;
        private RectTransform _parentRect;

        private GridLayoutGroup CachedGridLayoutGroup {
            get {
                if (_gridLayoutGroup == null) {
                    _gridLayoutGroup = GetComponent<GridLayoutGroup>();
                }
                return _gridLayoutGroup;
            }
        }

        private RectTransform CachedParentRect {
            get {
                if (_parentRect == null && transform.parent != null) {
                    _parentRect = transform.parent.GetComponent<RectTransform>();
                }
                return _parentRect;
            }
        }

        // Invalidate parent cache if hierarchy changes
        private void OnTransformParentChanged() {
            _parentRect = null;
        }

        public async Task<int> GetColumn() {
            await WaitLayoutDone;
            // Use cached component
            return CachedGridLayoutGroup.constraintCount;
        }
        
        public void SetLayoutHorizontal() {
            if(!Application.isPlaying) {
                return;
            }
            // Use cached component
            var gridLayoutGroup = CachedGridLayoutGroup;
            if (!gridLayoutGroup) {
                return;
            }

            if (!AppConfig.IsTon() && ScreenUtils.IsIPadScreen()) {
                gridLayoutGroup.padding.left = 0;
            }
            
            // Use cached parent rect
            var parent = CachedParentRect;
            if (parent == null) {
                return;
            }

            var size = parent.rect.size;
            size.x -= gridLayoutGroup.padding.left;
            if (size.x <= 0) {
                return;
            }
            _colMin = Mathf.Max(Mathf.FloorToInt(size.x / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x)), 1);
            if (_colMin < minColInput) {
                _colMin = minColInput;
                var wRefer = (size.x / _colMin) - minSpaceInput;
                var hRefer = wRefer / gridLayoutGroup.cellSize.x * gridLayoutGroup.cellSize.y;
                gridLayoutGroup.spacing = new Vector2(minSpaceInput, minSpaceInput);
                gridLayoutGroup.cellSize = new Vector2(wRefer, hRefer);
            }
            _rowMin = Mathf.Max(Mathf.FloorToInt(size.y / (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y)), minRowInput);
            gridLayoutGroup.constraintCount = _colMin;
            _isLoad = true;
        }

        public void SetLayoutVertical() {
        }

        private void LateUpdate() {
            if (_waitLayoutDone == null || !_isLoad) {
                return;
            }
            _waitLayoutDone.SetResult(true);
            _waitLayoutDone = null;
        }
    }
}