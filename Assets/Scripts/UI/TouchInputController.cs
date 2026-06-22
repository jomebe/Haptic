using Haptic.Gameplay;
using UnityEngine;

namespace Haptic.UI
{
    public sealed class TouchInputController : MonoBehaviour
    {
        GameplayController gameplay;
        RectTransform knob;
        Vector2 origin;
        Vector2 lastDirection;
        float nextRepeat;
        int trackedFinger = -1;
        const float Radius = 58f;

        public void Initialize(GameplayController controller, RectTransform joystickKnob)
        {
            gameplay = controller;
            knob = joystickKnob;
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            PollMouse();
#else
            PollTouches();
#endif
        }

        void PollTouches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && trackedFinger < 0)
                {
                    if (DirectTouchButtonRouter.Instance != null &&
                        DirectTouchButtonRouter.Instance.IsButtonAt(touch.position))
                        continue;
                    trackedFinger = touch.fingerId;
                    BeginGesture(touch.position);
                }
                else if (touch.fingerId == trackedFinger &&
                         (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
                {
                    UpdateGesture(touch.position);
                }
                else if (touch.fingerId == trackedFinger &&
                         (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    EndGesture();
                }
            }
        }

        void PollMouse()
        {
            if (Input.GetMouseButtonDown(0) &&
                (DirectTouchButtonRouter.Instance == null ||
                 !DirectTouchButtonRouter.Instance.IsButtonAt(Input.mousePosition)))
                BeginGesture(Input.mousePosition);
            if (Input.GetMouseButton(0) && trackedFinger == -2)
                UpdateGesture(Input.mousePosition);
            if (Input.GetMouseButtonUp(0) && trackedFinger == -2)
                EndGesture();
        }

        public void BeginGesture(Vector2 screenPosition)
        {
            if (trackedFinger < 0)
                trackedFinger = -2;
            origin = screenPosition;
            lastDirection = Vector2.zero;
            nextRepeat = 0f;
        }

        public void UpdateGesture(Vector2 screenPosition)
        {
            if (gameplay == null || knob == null)
                return;

            Vector2 delta = Vector2.ClampMagnitude(screenPosition - origin, Radius);
            knob.anchoredPosition = delta;
            if (delta.magnitude < 28f)
                return;

            Vector2 direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : new Vector2(0f, Mathf.Sign(delta.y));

            if (direction != lastDirection || Time.unscaledTime >= nextRepeat)
            {
                gameplay.TryMove(new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y)));
                lastDirection = direction;
                nextRepeat = Time.unscaledTime + 0.24f;
            }
        }

        public void EndGesture()
        {
            trackedFinger = -1;
            if (knob != null)
                knob.anchoredPosition = Vector2.zero;
            lastDirection = Vector2.zero;
        }

        void OnDisable() => EndGesture();
    }
}
