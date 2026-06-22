using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Haptic.UI
{
    public sealed class DirectTouchButtonRouter : MonoBehaviour
    {
        readonly List<RaycastResult> results = new();
        GraphicRaycaster raycaster;
        Vector2 pressPosition;
        int trackedFinger = -1;

        public static DirectTouchButtonRouter Instance { get; private set; }

        public void Initialize(GraphicRaycaster canvasRaycaster)
        {
            Instance = this;
            raycaster = canvasRaycaster;
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                BeginPress(Input.mousePosition, -2);
            if (Input.GetMouseButtonUp(0) && trackedFinger == -2)
                EndPress(Input.mousePosition, -2);
#else
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && trackedFinger < 0)
                    BeginPress(touch.position, touch.fingerId);
                else if (touch.fingerId == trackedFinger &&
                         (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                    EndPress(touch.position, touch.fingerId);
            }
#endif
        }

        public bool IsButtonAt(Vector2 screenPosition) => FindButton(screenPosition) != null;

        public void TapAt(Vector2 screenPosition)
        {
            Button button = FindButton(screenPosition);
            if (button != null && button.IsActive() && button.interactable)
                button.onClick.Invoke();
        }

        void BeginPress(Vector2 position, int fingerId)
        {
            trackedFinger = fingerId;
            pressPosition = position;
        }

        void EndPress(Vector2 position, int fingerId)
        {
            if (fingerId == trackedFinger && Vector2.Distance(pressPosition, position) <= 36f)
                TapAt(position);
            trackedFinger = -1;
        }

        Button FindButton(Vector2 screenPosition)
        {
            if (raycaster == null || EventSystem.current == null)
                return null;

            results.Clear();
            var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
            raycaster.Raycast(pointer, results);
            foreach (RaycastResult result in results)
            {
                Button button = result.gameObject.GetComponentInParent<Button>();
                if (button != null)
                    return button;
            }
            return null;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
