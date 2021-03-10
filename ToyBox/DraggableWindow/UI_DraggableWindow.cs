namespace ToyBoxHHH.DraggableWindowHHH
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Random = UnityEngine.Random;

    /// <summary>
    /// A dirty little script that makes a RectTransform draggable, and prevents it from being dragged outside the game window (if so desired).
    /// 
    /// made by @horatiu665
    /// </summary>
    public class UI_DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Ref to the transform that moves on drag")]
        [SerializeField]
        private RectTransform _dragTarget;
        public RectTransform dragTarget
        {
            get
            {
                if (_dragTarget == null)
                {
                    _dragTarget = GetComponent<RectTransform>();
                }
                return _dragTarget;
            }
        }

        private Camera _mainCamera;
        public Camera mainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        private Canvas _canvas;
        public Canvas canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        private Vector2 _prevPointerPosition;
        private Vector2 _initDragPointerPos;
        bool _realIsDragging;
        public bool isDragging
        {
            get
            {
                return _realIsDragging;
            }
        }

        //[EnumButtons(true)]
        //public RewiredInputMan.InputSource allowDrag = RewiredInputMan.InputSource.Touch;

        [Header("Anchors should be (0,0,0,0) for this to work!!!")]
        public bool canTakeOffScreen = false;
        public bool setAnchorsToZeroZero = true;

        [Header("Do in update? 0 or more. No? -1.")]
        public int keepInScreenUpdateFrames = -1;

        public float minDragForDraggingInScreenPercent = 0.03f;

        private void OnValidate()
        {
            if (dragTarget != null)
            {
            }
        }

        private void OnEnable()
        {
            if (!canTakeOffScreen)
            {
                if (setAnchorsToZeroZero)
                {
                    _dragTarget.anchorMin = _dragTarget.anchorMax = Vector2.zero;
                }

                if (_dragTarget.anchorMin != Vector2.zero || _dragTarget.anchorMax != Vector2.zero)
                {
                    Debug.LogWarning("Warning! Draggable window won't work with anchors set to nonzero on " + dragTarget.name, dragTarget.gameObject);
                }

                this.KeepInsideScreen();
            }
        }

        private void Update()
        {
            if (keepInScreenUpdateFrames >= 0)
            {
                if (keepInScreenUpdateFrames == 0 || (Time.frameCount % keepInScreenUpdateFrames == 0))
                    KeepInsideScreen();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _prevPointerPosition = eventData.position;
            _initDragPointerPos = eventData.position;

            if (minDragForDraggingInScreenPercent == 0)
                _realIsDragging = true;
            else
                _realIsDragging = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // drag begins only when dragging for a certain distance...
            if (!_realIsDragging)
            {
                // this is in screen coords right now
                var distFromInitDrag = eventData.position - _initDragPointerPos;
                if (distFromInitDrag.magnitude <= Screen.height * minDragForDraggingInScreenPercent)
                {
                    return;
                }
                else
                {
                    _realIsDragging = true;
                    _prevPointerPosition = eventData.position;
                }
            }

            // calc delta drag.
            var deltaDrag = eventData.position - _prevPointerPosition;
            _prevPointerPosition = eventData.position;

            // change to canvas space so we can apply the movement to the dragrekt
            deltaDrag = RectTransformUtility.ScreenPositionToCanvasSpace(deltaDrag, canvas);

            // move window by this delta
            dragTarget.anchoredPosition += (Vector2)deltaDrag;

            if (!canTakeOffScreen)
            {
                this.KeepInsideScreen();
            }
        }

        public void KeepInsideScreen()
        {
            RectTransformUtility.KeepInsideScreen(dragTarget, canvas);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _realIsDragging = false;
        }
    }

}