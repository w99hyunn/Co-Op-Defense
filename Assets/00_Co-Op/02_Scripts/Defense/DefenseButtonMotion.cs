using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenFrame
{
    [RequireComponent(typeof(Button))]
    public sealed class DefenseButtonMotion : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public RectTransform feedbackTarget;
        public Graphic[] stateContent;
        private Button button;
        private Vector3 restScale;
        private bool held;
        private bool lastInteractable = true;

        private void Awake()
        {
            TryGetComponent<Button>(out button);
            restScale = feedbackTarget.localScale;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && button.IsInteractable()) held = true;
        }
        public void OnPointerUp(PointerEventData data) => held = false;
        public void OnPointerExit(PointerEventData data) => held = false;

        private void Update()
        {
            if (!button.IsInteractable()) held = false;
            var target = restScale * (held ? .96f : 1f);
            feedbackTarget.localScale = Vector3.Lerp(feedbackTarget.localScale, target, 1 - Mathf.Exp(-28 * Time.unscaledDeltaTime));
            bool interactable = button.IsInteractable();
            if (lastInteractable == interactable) return;
            lastInteractable = interactable;
            foreach (var graphic in stateContent) graphic.canvasRenderer.SetAlpha(interactable ? 1 : .5f);
        }

        private void OnDisable()
        {
            held = false;
            feedbackTarget.localScale = restScale;
        }
    }
}
