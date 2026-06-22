using Haptic.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Haptic.UI
{
    public sealed class TouchInputController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        GameplayController gameplay;
        RectTransform knob;
        Vector2 origin;
        Vector2 lastDirection;
        float nextRepeat;
        const float Radius = 58f;

        public void Initialize(GameplayController controller, RectTransform joystickKnob)
        {
            gameplay = controller;
            knob = joystickKnob;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out origin);
            UpdateDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData) => UpdateDrag(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            knob.anchoredPosition = Vector2.zero;
            lastDirection = Vector2.zero;
        }

        void UpdateDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out Vector2 current);
            Vector2 delta = Vector2.ClampMagnitude(current - origin, Radius);
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
    }
}
