using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/16/2019 10:05:17 PM
Modified On:    Modified On: 11/16/2019 10:05:17 PM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.Utilties {
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSnapUtil : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler,
                                  IPointerUpHandler {
        #region Fields.

        private int _nearestPanel, _targetPanel, _currentPanel, _numberOfToggles;
        private bool _dragging, _pressing, _selected;
        private float _releaseSpeed, _planeDistance;
        private Vector2 _contentSize, _previousPosition;
        private ReleaseDirection _releaseDirection;
        private CanvasGroup _canvasGroup;
        private GameObject[] _panels;
        private Toggle[] _toggles;
        private Graphic[] _graphics;
        private Canvas _canvas;
        private CanvasScaler _canvasScaler;

        [SerializeField] private MovementType _movementType = MovementType.Fixed;
        [SerializeField] private MovementAxis _movementAxis = MovementAxis.Horizontal;
        [SerializeField] private bool _automaticallyLayout = true;
        [SerializeField] private SizeControl _sizeControl = SizeControl.Fit;
        [SerializeField] private Vector2 _size = new Vector2(400, 200);
        [SerializeField] private float automaticLayoutSpacing = 0.25f;
        [SerializeField] private float _leftMargin, _rightMargin, _bottomMargin, _topMargin;
        [SerializeField] private bool _infinitelyScroll = false;
        [SerializeField] private float infiniteScrollingEndSpacing = 0f;
        [SerializeField] private int _startingPanel = 0;
        [SerializeField] private bool _swipeGestures = true;
        [SerializeField] private float _minimumSwipeSpeed;
        [SerializeField] private Button _previousButton = null, _nextButton = null;
        [SerializeField] private GameObject _pagination = null;
        [SerializeField] private bool _toggleNavigation = true;
        [SerializeField] private SnapTarget _snapTarget = SnapTarget.Next;
        [SerializeField] private float _snappingSpeed = 10f;
        [SerializeField] private float _thresholdSnappingSpeed = 1f;
        [SerializeField] private bool _hardSnap = true;
        [SerializeField] private UnityEvent _onPanelChanged, _onPanelSelecting, onPanelSelected, _onPanelChanging;

        public List<TransitionEffect> transitionEffects = new List<TransitionEffect>();

        #endregion

        #region Properties.

        public int CurrentPanel => _currentPanel;

        public int TargetPanel => _targetPanel;

        public int NearestPanel => _nearestPanel;

        public int NumberOfPanels => Content.childCount;

        public ScrollRect ScrollRect => GetComponent<ScrollRect>();

        public RectTransform Content => GetComponent<ScrollRect>().content;

        public RectTransform Viewport => GetComponent<ScrollRect>().viewport;

        public GameObject[] Panels => _panels;

        public Toggle[] Toggles => _toggles;

        #endregion

        #region Enumerators

        public enum MovementType {
            Fixed,
            Free
        }

        public enum MovementAxis {
            Horizontal,
            Vertical
        }

        public enum ReleaseDirection {
            Up,
            Down,
            Left,
            Right
        }

        public enum SnapTarget {
            Nearest,
            Previous,
            Next
        }

        public enum SizeControl {
            Manual,
            Fit
        }

        #endregion

        #region Unity Methods.

        private void Start() {
            if(Validate())
                Setup(true);
            else
                throw new Exception("Invalid Configuration!");
        }

        private void Update() {
            if(NumberOfPanels == 0)
                return;

            OnSelectingAndSnapping();
            OnInfiniteScrolling();
            OnTransitionEffects();
            OnSwipeGestures();
        }

        #endregion

        #region Custom Methods.

        private bool Validate() {
            var valid = true;

            if(_pagination == null)
                return valid;

            _numberOfToggles = _pagination.transform.childCount;

            if(_numberOfToggles == NumberOfPanels)
                return valid;

            Debug.LogError($"<b>[ScrollSnapUtil]</b> The number of toggles should be equivalent to the number of panels. There are currently {_numberOfToggles} toggles and {NumberOfPanels} panels. If you are adding panels dynamically during runtime, please update your pagination to reflect the number of panels you will have before adding.",
                           gameObject);
            valid = false;

            return valid;
        }

        private void Setup(bool updatePosition) {
            if(NumberOfPanels == 0)
                return;

            // Canvas & Camera
            _canvas = GetComponentInParent<Canvas>();
            if(_canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                _canvas.planeDistance = _canvas.GetComponent<RectTransform>().rect.height / 2f /
                                        Mathf.Tan(_canvas.worldCamera.fieldOfView / 2f * Mathf.Deg2Rad);
                if(_canvas.worldCamera.farClipPlane < _canvas.planeDistance) {
                    _canvas.worldCamera.farClipPlane = Mathf.Ceil(_canvas.planeDistance);
                }
            }

            // ScrollRect
            if(_movementType == MovementType.Fixed) {
                ScrollRect.horizontal = _movementAxis == MovementAxis.Horizontal;
                ScrollRect.vertical = _movementAxis == MovementAxis.Vertical;
            }
            else {
                ScrollRect.horizontal = ScrollRect.vertical = true;
            }

            // Panels
            _size = _sizeControl == SizeControl.Manual
                        ? _size
                        : new Vector2(GetComponent<RectTransform>().rect.width,
                                      GetComponent<RectTransform>().rect.height);
            _panels = new GameObject[NumberOfPanels];
            for(var i = 0; i < NumberOfPanels; i++) {
                _panels[i] = ((RectTransform) Content.GetChild(i)).gameObject;

                if(_movementType != MovementType.Fixed || !_automaticallyLayout)
                    continue;

                _panels[i].GetComponent<RectTransform>().anchorMin =
                    new Vector2(_movementAxis == MovementAxis.Horizontal ? 0f : 0.5f,
                                _movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                ;
                _panels[i].GetComponent<RectTransform>().anchorMax =
                    new Vector2(_movementAxis == MovementAxis.Horizontal ? 0f : 0.5f,
                                _movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                ;

                var x = (_rightMargin + _leftMargin) / 2f - _leftMargin;
                var y = (_topMargin + _bottomMargin) / 2f - _bottomMargin;
                var marginOffset = new Vector2(x / _size.x, y / _size.y);
                _panels[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f) + marginOffset;
                _panels[i].GetComponent<RectTransform>().sizeDelta =
                    _size - new Vector2(_leftMargin + _rightMargin, _topMargin + _bottomMargin);

                var panelPosX = _movementAxis == MovementAxis.Horizontal
                                    ? i * (automaticLayoutSpacing + 1f) * _size.x + _size.x / 2f
                                    : 0f;
                var panelPosY = _movementAxis == MovementAxis.Vertical
                                    ? i * (automaticLayoutSpacing + 1f) * _size.y + _size.y / 2f
                                    : 0f;
                _panels[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(panelPosX, panelPosY, 0f);
            }

            // Content
            if(_movementType == MovementType.Fixed) {
                // Automatic Layout
                if(_automaticallyLayout) {
                    Content.anchorMin = new Vector2(_movementAxis == MovementAxis.Horizontal ? 0f : 0.5f,
                                                    _movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                    Content.anchorMax = new Vector2(_movementAxis == MovementAxis.Horizontal ? 0f : 0.5f,
                                                    _movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                    Content.pivot = new Vector2(_movementAxis == MovementAxis.Horizontal ? 0f : 0.5f,
                                                _movementAxis == MovementAxis.Vertical ? 0f : 0.5f);

                    var min = _panels[0].transform.position;
                    var max = _panels[NumberOfPanels - 1].transform.position;

                    var contentWidth = _movementAxis == MovementAxis.Horizontal
                                           ? (NumberOfPanels * (automaticLayoutSpacing + 1f) * _size.x) -
                                             (_size.x * automaticLayoutSpacing)
                                           : _size.x;
                    var contentHeight = (_movementAxis == MovementAxis.Vertical)
                                            ? (NumberOfPanels * (automaticLayoutSpacing + 1f) * _size.y) -
                                              (_size.y * automaticLayoutSpacing)
                                            : _size.y;
                    Content.sizeDelta = new Vector2(contentWidth, contentHeight);
                }

                // Infinite Scrolling
                if(_infinitelyScroll) {
                    ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;

                    _contentSize =
                        ((Vector2) _panels[NumberOfPanels - 1].transform.localPosition -
                         (Vector2) _panels[0].transform.localPosition) +
                        (_panels[NumberOfPanels - 1].GetComponent<RectTransform>().sizeDelta / 2f +
                         _panels[0].GetComponent<RectTransform>().sizeDelta / 2f) +
                        (new
                                Vector2(_movementAxis == MovementAxis.Horizontal ? infiniteScrollingEndSpacing * _size.x : 0f,
                                        _movementAxis == MovementAxis.Vertical
                                            ? infiniteScrollingEndSpacing * _size.y
                                            : 0f));

                    if(_movementAxis == MovementAxis.Horizontal) {
                        _contentSize += new Vector2(_leftMargin + _rightMargin, 0);
                    }
                    else {
                        _contentSize += new Vector2(0, _topMargin + _bottomMargin);
                    }

                    _canvasScaler = _canvas.GetComponent<CanvasScaler>();
                    if(_canvasScaler != null) {
                        _contentSize *= new Vector2(Screen.width / _canvasScaler.referenceResolution.x,
                                                    Screen.height / _canvasScaler.referenceResolution.y);
                    }
                }
            }

            // Starting Panel
            if(updatePosition) {
                var xOffset = (_movementAxis == MovementAxis.Horizontal || _movementType == MovementType.Free)
                                  ? Viewport.GetComponent<RectTransform>().rect.width / 2f
                                  : 0f;
                var yOffset = (_movementAxis == MovementAxis.Vertical || _movementType == MovementType.Free)
                                  ? Viewport.GetComponent<RectTransform>().rect.height / 2f
                                  : 0f;
                var offset = new Vector2(xOffset, yOffset);
                Content.anchoredPosition = -(Vector2) _panels[_startingPanel].transform.localPosition + offset;
                _currentPanel = _targetPanel = _nearestPanel = _startingPanel;
            }

            // Previous Button
            if(_previousButton != null) {
                _previousButton.onClick.AddListener(GoToPreviousPanel);
            }

            // Next Button
            if(_nextButton != null) {
                _nextButton.onClick.AddListener(GoToNextPanel);
            }

            // Pagination
            if(_pagination == null)
                return;

            _toggles = new Toggle[_numberOfToggles];
            for(var i = 0; i < _numberOfToggles; i++) {
                _toggles[i] = _pagination.transform.GetChild(i).GetComponent<Toggle>();
                if(_toggles[i] == null)
                    continue;

                _toggles[i].isOn = (i == _startingPanel);
                _toggles[i].interactable = (i != _targetPanel);
                var panelNum = i;
                _toggles[i].onValueChanged.AddListener(delegate {
                    if(_toggles[panelNum].isOn && _toggleNavigation) {
                        GoToPanel(panelNum);
                    }
                });
            }
        }

        private Vector2 DisplacementFromCenter(Vector2 position) {
            return position - (Vector2) Viewport.position;
        }

        private int DetermineNearestPanel() {
            var panelNumber = _nearestPanel;
            var distances = new float[NumberOfPanels];
            for(var i = 0; i < _panels.Length; i++) {
                distances[i] = DisplacementFromCenter(_panels[i].transform.position).magnitude;
            }

            var minDistance = Mathf.Min(distances);
            for(var i = 0; i < _panels.Length; i++) {
                if(minDistance == distances[i]) {
                    panelNumber = i;
                }
            }

            return panelNumber;
        }

        private void SelectTargetPanel() {
            _nearestPanel = DetermineNearestPanel();
            if(_snapTarget == SnapTarget.Nearest) {
                GoToPanel(_nearestPanel);
            } else if(_snapTarget == SnapTarget.Previous) {
                if(_releaseDirection == ReleaseDirection.Right) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).x < 0f) {
                        GoToNextPanel();
                    }  else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Left) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).x > 0f) {
                        GoToPreviousPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Up) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).y < 0f) {
                        GoToNextPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Down) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).y > 0f) {
                        GoToPreviousPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                }
            } else if(_snapTarget == SnapTarget.Next) {
                if(_releaseDirection == ReleaseDirection.Right) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).x > 0f) {
                        GoToPreviousPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Left) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).x < 0f) {
                        GoToNextPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Up) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).y > 0f) {
                        GoToPreviousPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                } else if(_releaseDirection == ReleaseDirection.Down) {
                    if(DisplacementFromCenter(_panels[_nearestPanel].transform.position).y < 0f) {
                        GoToNextPanel();
                    } else {
                        GoToPanel(_nearestPanel);
                    }
                }
            }
        }

        private void SnapToTargetPanel() {
            var xOffset = (_movementAxis == MovementAxis.Horizontal || _movementType == MovementType.Free)
                                ? Viewport.GetComponent<RectTransform>().rect.width / 2f
                                : 0f;
            var yOffset = (_movementAxis == MovementAxis.Vertical || _movementType == MovementType.Free)
                                ? Viewport.GetComponent<RectTransform>().rect.height / 2f
                                : 0f;
            var offset = new Vector2(xOffset, yOffset);

            var targetPosition = (-(Vector2) _panels[_targetPanel].transform.localPosition + offset);
            Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, targetPosition,
                                                    Time.unscaledDeltaTime * _snappingSpeed);

            if(DisplacementFromCenter(_panels[_targetPanel].transform.position).magnitude <
               (_panels[_targetPanel].GetComponent<RectTransform>().rect.width / 10f) && _targetPanel != _currentPanel) {
                _onPanelChanged.Invoke();
                _currentPanel = _targetPanel;
            } else if(ScrollRect.velocity != Vector2.zero) {
               _onPanelChanging.Invoke();
            }
        }

        private void OnSelectingAndSnapping() {
            if(!_dragging && !_pressing) {
                // Snap/Select after Swiping
                if(_releaseSpeed >= _minimumSwipeSpeed || _currentPanel != DetermineNearestPanel()) {
                    if(ScrollRect.velocity.magnitude <= _thresholdSnappingSpeed || _thresholdSnappingSpeed == -1f) {
                        if(_selected) {
                            SnapToTargetPanel();
                        } else {
                            SelectTargetPanel();
                        }
                    } else {
                        _onPanelSelecting.Invoke();
                    }
                } else {    // Snap/Select after Pressing Button/Pagination Toggle
                    if(_selected) {
                        SnapToTargetPanel();
                    } else {
                        GoToPanel(_currentPanel);
                    }
                }
            }
        }

        private void OnInfiniteScrolling() {
            if(_infinitelyScroll) {
                if(_movementAxis == MovementAxis.Horizontal) {
                    for(var i = 0; i < NumberOfPanels; i++) {
                        var width = _contentSize.x;
                        if(_canvasScaler != null)
                            width *= (_canvas.GetComponent<RectTransform>().localScale.x /
                                      (Screen.width / _canvasScaler.referenceResolution.x));

                        if(DisplacementFromCenter(_panels[i].transform.position).x > width / 2f) {
                            _panels[i].transform.position += width * Vector3.left;
                        } else if(DisplacementFromCenter(_panels[i].transform.position).x < -1f * width / 2f) {
                            _panels[i].transform.position += width * Vector3.right;
                        }
                    }
                } else if(_movementAxis == MovementAxis.Vertical) {
                    var height = _contentSize.y;
                    if(_canvasScaler != null)
                        height *= (_canvas.GetComponent<RectTransform>().localScale.y /
                                   (Screen.height / _canvasScaler.referenceResolution.y));

                    for(var i = 0; i < NumberOfPanels; i++) {
                        if(DisplacementFromCenter(_panels[i].transform.position).y > height / 2f) {
                            _panels[i].transform.position += height * Vector3.down;
                        } else if(DisplacementFromCenter(_panels[i].transform.position).y < -1f * height / 2f) {
                            _panels[i].transform.position += height * Vector3.up;
                        }
                    }
                }
            }
        }

        private void OnTransitionEffects() {
            foreach(var panel in _panels) {
                foreach(var transitionEffect in transitionEffects) {
                    // Displacement
                    var displacement = 0f;
                    if(_movementType == MovementType.Fixed) {
                        if(_movementAxis == MovementAxis.Horizontal) {
                            displacement = DisplacementFromCenter(panel.transform.position).x;
                        } else if(_movementAxis == MovementAxis.Vertical) {
                            displacement = DisplacementFromCenter(panel.transform.position).y;
                        }
                    } else {
                        displacement = DisplacementFromCenter(panel.transform.position).magnitude;
                    }

                    // Value
                    switch(transitionEffect.Label) {
                        case "localPosition.z":
                            panel.transform.localPosition = new Vector3(panel.transform.localPosition.x,
                                                                        panel.transform.localPosition.y,
                                                                        transitionEffect.GetValue(displacement));
                            break;
                        case "localScale.x":
                            panel.transform.localScale =
                                new Vector2(transitionEffect.GetValue(displacement), panel.transform.localScale.y);
                            break;
                        case "localScale.y":
                            panel.transform.localScale =
                                new Vector2(panel.transform.localScale.x, transitionEffect.GetValue(displacement));
                            break;
                        case "localRotation.x":
                            panel.transform.localRotation =
                                Quaternion.Euler(new Vector3(transitionEffect.GetValue(displacement),
                                                             panel.transform.localEulerAngles.y,
                                                             panel.transform.localEulerAngles.z));
                            break;
                        case "localRotation.y":
                            panel.transform.localRotation =
                                Quaternion.Euler(new Vector3(panel.transform.localEulerAngles.x,
                                                             transitionEffect.GetValue(displacement),
                                                             panel.transform.localEulerAngles.z));
                            break;
                        case "localRotation.z":
                            panel.transform.localRotation =
                                Quaternion.Euler(new Vector3(panel.transform.localEulerAngles.x,
                                                             panel.transform.localEulerAngles.y,
                                                             transitionEffect.GetValue(displacement)));
                            break;
                        case "color.r":
                            _graphics = panel.GetComponentsInChildren<Graphic>();
                            foreach(var graphic in _graphics) {
                                graphic.color = new Color(transitionEffect.GetValue(displacement), graphic.color.g,
                                                          graphic.color.b, graphic.color.a);
                            }

                            break;
                        case "color.g":
                            _graphics = panel.GetComponentsInChildren<Graphic>();
                            foreach(var graphic in _graphics) {
                                graphic.color = new Color(graphic.color.r, transitionEffect.GetValue(displacement),
                                                          graphic.color.b, graphic.color.a);
                            }

                            break;
                        case "color.b":
                            _graphics = panel.GetComponentsInChildren<Graphic>();
                            foreach(var graphic in _graphics) {
                                graphic.color = new Color(graphic.color.r, graphic.color.g,
                                                          transitionEffect.GetValue(displacement), graphic.color.a);
                            }

                            break;
                        case "color.a":
                            _graphics = panel.GetComponentsInChildren<Graphic>();
                            foreach(var graphic in _graphics) {
                                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b,
                                                          transitionEffect.GetValue(displacement));
                            }

                            break;
                    }
                }
            }
        }

        private void OnSwipeGestures() {
            if(_swipeGestures == false && (Input.GetMouseButton(0) || Input.touchCount > 0)) {
                // Set to False.
                ScrollRect.horizontal = ScrollRect.vertical = false;
            } else {
                // Reset.
                if(_movementType == MovementType.Fixed) {
                    ScrollRect.horizontal = (_movementAxis == MovementAxis.Horizontal);
                    ScrollRect.vertical = (_movementAxis == MovementAxis.Vertical);
                } else {
                    ScrollRect.horizontal = ScrollRect.vertical = true;
                }
            }
        }

        public void GoToPanel(int panelNumber) {
            _targetPanel = panelNumber;
            _selected = true;
            onPanelSelected.Invoke();

            if(_pagination != null) {
                for(var i = 0; i < _toggles.Length; i++) {
                    if(_toggles[i] != null) {
                        _toggles[i].isOn = (i == _targetPanel);
                        _toggles[i].interactable = (i != _targetPanel);
                    }
                }
            }

            if(_hardSnap) {
                ScrollRect.inertia = false;
            }
        }

        public void GoToPreviousPanel() {
            _nearestPanel = DetermineNearestPanel();
            if(_nearestPanel != 0) {
                GoToPanel(_nearestPanel - 1);
            } else {
                if(_infinitelyScroll) {
                    GoToPanel(NumberOfPanels - 1);
                } else {
                    GoToPanel(_nearestPanel);
                }
            }
        }

        public void GoToNextPanel() {
            _nearestPanel = DetermineNearestPanel();
            if(_nearestPanel != (NumberOfPanels - 1)) {
                GoToPanel(_nearestPanel + 1);
            } else {
                if(_infinitelyScroll) {
                    GoToPanel(0);
                } else {
                    GoToPanel(_nearestPanel);
                }
            }
        }

        public void AddToFront(GameObject panel) {
            Add(panel, 0);
        }

        public void AddToBack(GameObject panel) {
            Add(panel, NumberOfPanels);
        }

        public void Add(GameObject panel, int index) {
            if(NumberOfPanels != 0 && (index < 0 || index > NumberOfPanels)) {
                Debug.LogError("<b>[SimpleScrollSnap]</b> Index must be an integer from 0 to " + NumberOfPanels + ".",
                               gameObject);
                return;
            }

            panel = Instantiate(panel, Vector2.zero, Quaternion.identity, Content);
            panel.transform.SetSiblingIndex(index);

            if(Validate()) {
                if(_targetPanel <= index) {
                    _startingPanel = _targetPanel;
                } else {
                    _startingPanel = _targetPanel + 1;
                }

                Setup(true);
            }
        }

        public void RemoveFromFront() {
            Remove(0);
        }

        public void RemoveFromBack() {
            if(NumberOfPanels > 0) {
                Remove(NumberOfPanels - 1);
            }
            else {
                Remove(0);
            }
        }

        public void Remove(int index) {
            if(NumberOfPanels == 0) return;

            if(index < 0 || index > (NumberOfPanels - 1)) {
                Debug.LogError("<b>[SimpleScrollSnap]</b> Index must be an integer from 0 to " + (NumberOfPanels - 1) + ".",
                               gameObject);
                return;
            }

            DestroyImmediate(_panels[index]);

            if(Validate()) {
                if(_targetPanel == index) {
                    if(index == NumberOfPanels) {
                        _startingPanel = _targetPanel - 1;
                    } else {
                        _startingPanel = _targetPanel;
                    }
                } else if(_targetPanel < index) {
                    _startingPanel = _targetPanel;
                } else {
                    _startingPanel = _targetPanel - 1;
                }

                Setup(true);
            }
        }

        public void AddVelocity(Vector2 velocity) {
            ScrollRect.velocity += velocity;
            _selected = false;
        }

        #endregion

        #region Event Methods.

        public void OnBeginDrag(PointerEventData eventData) {
            if(NumberOfPanels == 0)
                return;

            if(!_swipeGestures)
                return;

            if(_hardSnap) {
                ScrollRect.inertia = true;
            }

            _selected = false;
            _dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData) {
            if(NumberOfPanels == 0)
                return;

            if(!_swipeGestures)
                return;

            _releaseSpeed = ScrollRect.velocity.magnitude;
            _dragging = false;
        }

        public void OnDrag(PointerEventData eventData) {
            if(NumberOfPanels == 0)
                return;

            if(!_swipeGestures)
                return;

            var position = eventData.position;
            if(position.x != _previousPosition.x && position.y != _previousPosition.y) {
                switch(_movementAxis) {
                    case MovementAxis.Horizontal:
                        _releaseDirection =
                            position.x > _previousPosition.x ? ReleaseDirection.Right : ReleaseDirection.Left;
                        break;
                    case MovementAxis.Vertical:
                        _releaseDirection =
                            position.y > _previousPosition.y ? ReleaseDirection.Up : ReleaseDirection.Down;
                        break;
                }
            }

            _previousPosition = eventData.position;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if(NumberOfPanels == 0)
                return;

            if(_swipeGestures)
                _pressing = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if(NumberOfPanels == 0)
                return;

            if(_swipeGestures)
                _pressing = false;
        }

        #endregion
    }

    [Serializable]
    public class TransitionEffect {
        #region Fields

        [SerializeField] protected float m_minDisplacement,
                                         m_maxDisplacement,
                                         m_minValue,
                                         m_maxValue,
                                         m_defaultMinValue,
                                         m_defaultMaxValue,
                                         m_defaultMinDisplacement,
                                         m_defaultMaxDisplacement;

        [SerializeField] protected bool m_showPanel, m_showDisplacement, m_showValue;
        [SerializeField] private string _label;
        [SerializeField] private AnimationCurve _function;
        [SerializeField] private AnimationCurve _defaultFunction;
        [SerializeField] private ScrollSnapUtil _scrollSnap;

        #endregion

        #region Properties

        public string Label {
            get { return _label; }
            set { _label = value; }
        }

        public float MinValue {
            get { return MinValue; }
            set { m_minValue = value; }
        }

        public float MMaxValue {
            get { return m_maxValue; }
            set { m_maxValue = value; }
        }

        public float MMinDisplacement {
            get { return m_minDisplacement; }
            set { m_minDisplacement = value; }
        }

        public float MMaxDisplacement {
            get { return m_maxDisplacement; }
            set { m_maxDisplacement = value; }
        }

        public AnimationCurve Function {
            get { return _function; }
            set { _function = value; }
        }

        #endregion

        #region Methods

        public TransitionEffect(string label, float mMinValue, float mMaxValue, float mMinDisplacement,
                                float mMaxDisplacement, AnimationCurve function, ScrollSnapUtil scrollSnap) {
            this._label = label;
            this._scrollSnap = scrollSnap;
            this.m_minValue = mMinValue;
            this.m_maxValue = mMaxValue;
            this.m_minDisplacement = mMinDisplacement;
            this.m_maxDisplacement = mMaxDisplacement;
            this._function = function;

            SetDefaultValues(mMinValue, mMaxValue, mMinDisplacement, mMaxDisplacement, function);
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine
                                                                         .SceneManagement.SceneManager
                                                                         .GetActiveScene());
#endif
        }

        private void SetDefaultValues(float minValue, float maxValue, float minDisplacement, float maxDisplacement,
                                      AnimationCurve function) {
            m_defaultMinValue = minValue;
            m_defaultMaxValue = maxValue;
            m_defaultMinDisplacement = minDisplacement;
            m_defaultMaxDisplacement = maxDisplacement;
            _defaultFunction = function;
        }

#if UNITY_EDITOR
        public void Init() {
            GUILayout.BeginVertical("HelpBox");
            m_showPanel = EditorGUILayout.Foldout(m_showPanel, _label, true);
            if(m_showPanel) {
                EditorGUI.indentLevel++;
                float x = m_minDisplacement;
                float y = m_minValue;
                float width = m_maxDisplacement - m_minDisplacement;
                float height = m_maxValue - m_minValue;

                // Min/Max Values
                m_showValue = EditorGUILayout.Foldout(m_showValue, "Value", true);
                if(m_showValue) {
                    EditorGUI.indentLevel++;
                    m_minValue = EditorGUILayout.FloatField(new GUIContent("Min"), m_minValue);
                    m_maxValue = EditorGUILayout.FloatField(new GUIContent("Max"), m_maxValue);
                    EditorGUI.indentLevel--;
                }

                // Min/Max Displacements
                m_showDisplacement = EditorGUILayout.Foldout(m_showDisplacement, "Displacement", true);
                if(m_showDisplacement) {
                    EditorGUI.indentLevel++;
                    m_minDisplacement = EditorGUILayout.FloatField(new GUIContent("Min"), m_minDisplacement);
                    m_maxDisplacement = EditorGUILayout.FloatField(new GUIContent("Max"), m_maxDisplacement);
                    EditorGUI.indentLevel--;
                }

                // Function
                _function = EditorGUILayout.CurveField("Function", _function, Color.white,
                                                       new Rect(x, y, width, height));

                // Reset
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 16);
                if(GUILayout.Button("Reset")) {
                    Reset();
                }

                // Remove
                if(GUILayout.Button("Remove")) {
                    _scrollSnap.transitionEffects.Remove(this);
                }

                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }
#endif

        public void Reset() {
            m_minValue = m_defaultMinValue;
            m_maxValue = m_defaultMaxValue;
            m_minDisplacement = m_defaultMinDisplacement;
            m_maxDisplacement = m_defaultMaxDisplacement;
            _function = _defaultFunction;
        }

        public float GetValue(float displacement) {
            return (_function != null) ? _function.Evaluate(displacement) : 0f;
        }

        #endregion
    }
}