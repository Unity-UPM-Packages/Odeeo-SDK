using System.Collections;
using System.Collections.Generic;
using Odeeo.Logging;
using Odeeo.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

namespace Odeeo
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class OdeeoIconAnchor : UIBehaviour
    {
        private RectTransform _rect;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        
        private float _maxPxSize;
        private float _minPxSize;

        private Vector3 _savedPosition;
        private Vector2 _savedAnchorMin;
        private Vector2 _savedAnchorMax;
        private Vector2 _savedSizeDelta;
        private Vector2 _savedCanvasDelta;

        private bool _isInitialized = false;
        private Coroutine _initCoroutine;

        private bool _isUnitFixed = false;
        private bool _isHierarchyDirty = true;
        private bool _isDimensionsDirty = true;

        private bool _isErrorMessageShown = false;
        private readonly IOdeeoLogging _logging = new OdeeoLogging();

        protected override void Awake()
        {
            if (Application.isPlaying)
            {
                GetComponent<Image>().enabled = false;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(false);
            }
        }
        
        protected override void OnEnable()
        {
            _isInitialized = false;

            if (_initCoroutine == null)
                _initCoroutine = StartCoroutine(Initialize());
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
#endif
        }

        protected override void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
#endif
        }

        private void OnHierarchyChanged()
        {
            if (!IsActive() || !_isInitialized || _isHierarchyDirty)
                return;

            _isHierarchyDirty = true;
        }

        protected override void OnBeforeTransformParentChanged()
        {
            if (!IsActive() || !_isInitialized || _isHierarchyDirty)
                return;

            if (IsProperHierarchy())
                _savedPosition = _rect.position;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (!IsActive() || !_isInitialized || _isDimensionsDirty)
                return;

            _isDimensionsDirty = true;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                if (_initCoroutine == null)
                    _initCoroutine = StartCoroutine(Initialize());
                return;
            }

            _isUnitFixed = false;

            CheckDimensions();
            CheckHierarchy();

            ResetRotationAndScale();
            StayInLines();

            if (!_isUnitFixed && IsProperHierarchy())
                _savedPosition = _rect.position;
        }

        private IEnumerator Initialize()
        {
            yield return new WaitForEndOfFrame();
            _initCoroutine = null;

            _rect = GetComponent<RectTransform>();
            if (!_rect || !_rect.parent)
            {
                if (!_isErrorMessageShown)
                {
                    _isErrorMessageShown = true;
                    ShowErrorInConsole("Parent is NULL");
                }

                yield break;
            }

            _canvas = _rect.parent.GetComponent<Canvas>();
            if (!_canvas)
            {
                if (!_isErrorMessageShown)
                {
                    _isErrorMessageShown = true;
                    ShowErrorInConsole("Wrong Parent");
                }

                yield break;
            }

            _canvasRect = _canvas.GetComponent<RectTransform>();
            if (!_canvasRect)
                yield break;

            _savedCanvasDelta = _canvasRect.sizeDelta;
            _savedPosition = _rect.position;
            _savedAnchorMin = _rect.anchorMin;
            _savedAnchorMax = _rect.anchorMax;

            _isInitialized = true;
            _isErrorMessageShown = false;

            _isHierarchyDirty = true;
            CheckHierarchy();

            CalculateScreenSize();
            SetUnitSize();

            _savedPosition = _rect.position;
        }

        private void CheckDimensions()
        {
            if (!_isDimensionsDirty)
                return;

            if (_savedCanvasDelta != _canvasRect.sizeDelta)
            {
                _isDimensionsDirty = false;
                _isInitialized = false;

                if (_initCoroutine == null)
                    _initCoroutine = StartCoroutine(Initialize());

                return;
            }

            if (IsAnchorsChanged())
            {
                FixAnchors();
                _isUnitFixed = true;
            }

            if (!_savedSizeDelta.Equals(_rect.sizeDelta))
            {
                SetUnitSize();

                _rect.position = _savedPosition;
                _isUnitFixed = true;
            }

            _isDimensionsDirty = false;
        }

        private void CheckHierarchy()
        {
            if (!_isHierarchyDirty)
                return;

            if (!_canvas)
            {
                if (!RectTransform.parent)
                {
                    ShowErrorInConsole("Parent is NULL");
                    return;
                }

                _canvas = RectTransform.parent.GetComponent<Canvas>();
                if (!_canvas)
                {
                    ShowErrorInConsole("Wrong Parent");
                    return;
                }
            }

            if (!_canvasRect)
                _canvasRect = _canvas.GetComponent<RectTransform>();

            FixHierarchy();
            _isHierarchyDirty = false;
        }

        private void FixHierarchy()
        {
            if (RectTransform.parent != _canvas.transform)
            {
                RectTransform.SetParent(_canvas.transform);
                _rect.position = _savedPosition;
                _isUnitFixed = true;
            }

            if (_canvasRect.childCount - 1 != RectTransform.GetSiblingIndex())
            {
                RectTransform.SetAsLastSibling();
                _rect.position = _savedPosition;
                _isUnitFixed = true;
            }
        }

        private bool IsProperHierarchy()
        {
            if (!_rect || !_rect.parent)
                return false;

            if (!_canvas)
                return false;

            if (!_canvasRect)
                return false;

            if (_rect.parent != _canvasRect)
                return false;

            return true;
        }

        private void ShowErrorInConsole(string message)
        {
            _logging.Error($"{message}Put IconAnchor in Canvas");
        }

        private OdeeoSdk.IconPosition GetClosest(Vector3 startPosition, Dictionary<OdeeoSdk.IconPosition, Vector3> pickups)
        {
            OdeeoSdk.IconPosition location = OdeeoSdk.IconPosition.Centered;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (KeyValuePair<OdeeoSdk.IconPosition, Vector3> potentialTarget in pickups)
            {
                Vector3 directionToTarget = potentialTarget.Value - startPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    location = potentialTarget.Key;
                }
            }

            return location;
        }

        private void StayInLines()
        {
            if (_canvas == null)
                return;

            Vector2 pivot = _rect.pivot;
            Vector2 sizeDelta = _rect.sizeDelta;
            Vector3 position = _rect.localPosition;
            Vector2 canvasSizeDelta = _canvasRect.sizeDelta;

            float leftBorder = canvasSizeDelta.x * -0.5f + pivot.x * sizeDelta.x;
            float rightBorder = canvasSizeDelta.x * 0.5f - sizeDelta.x + pivot.x * sizeDelta.x;
            float topBorder = canvasSizeDelta.y * 0.5f - sizeDelta.y + pivot.y * sizeDelta.y;
            float bottomBorder = canvasSizeDelta.y * -0.5f + pivot.y * sizeDelta.y;

            position.x = Mathf.Clamp(position.x, leftBorder, rightBorder);
            position.y = Mathf.Clamp(position.y, bottomBorder, topBorder);

            _rect.localPosition = position;
        }

        private void CalculateScreenSize()
        {
            if (!_canvas)
                return;
            
            OdeeoSdk.IsInitialized();
            
            OdeeoDpiResolution.SetOptimalDPI();

            float minScreenSize = 70f * OdeeoDpiResolution.GetDeviceScale();
            float maxScreenSize = 120f * OdeeoDpiResolution.GetDeviceScale();

            Vector2 sizeDelta = _canvasRect.sizeDelta;

            _minPxSize = minScreenSize / Screen.width * sizeDelta.x;
            _maxPxSize = maxScreenSize / Screen.width * sizeDelta.x;
        }

        private void SetUnitSize()
        {
            float unitSize = Mathf.Clamp(_rect.sizeDelta.x, _minPxSize, _maxPxSize);

            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, unitSize);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, unitSize);
            _rect.ForceUpdateRectTransforms();

            _savedSizeDelta = _rect.sizeDelta;
        }

        private bool IsAnchorsChanged()
        {
            if (_savedAnchorMax != _rect.anchorMax)
                return true;

            if (_savedAnchorMin != _rect.anchorMin)
                return true;

            return false;
        }

        private void FixAnchors()
        {
            bool isNeedToRestorePosition = false;
            if (_rect.anchorMax != _rect.anchorMin)
            {
                _savedPosition = _rect.position;
                isNeedToRestorePosition = true;
            }

            if (_savedAnchorMax != _rect.anchorMax)
                _rect.anchorMin = _rect.anchorMax;
            else if (_savedAnchorMin != _rect.anchorMin)
                _rect.anchorMax = _rect.anchorMin;

            _savedAnchorMin = _rect.anchorMin;
            _savedAnchorMax = _rect.anchorMax;

            _rect.sizeDelta = _savedSizeDelta;

            if (isNeedToRestorePosition)
                _rect.position = _savedPosition;
        }

        private void ResetRotationAndScale()
        {
            _rect.rotation = Quaternion.identity;
            _rect.localScale = Vector3.one;
        }

        public RectTransform RectTransform
        {
            get
            {
                if (!_rect) _rect = (RectTransform)transform;
                return _rect;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (!_canvas) _canvas = transform.parent.GetComponent<Canvas>();
                return _canvas;
            }
        }
    }
}