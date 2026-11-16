using UnityEngine;

namespace OpenFrame
{
    public sealed class DefenseStageFit : MonoBehaviour
    {
        private RectTransform rect;
        private RectTransform parent;
        private void OnEnable() => ApplyPortraitLayout();
        private void Update() => Fit();

        public void ApplyPortraitLayout()
        {
            TryGetComponent<RectTransform>(out rect);
            parent = (RectTransform)rect.parent;
            rect.sizeDelta = DefenseArena.StageSize;
            Fit();
        }

        private void Fit()
        {
            float scale = Mathf.Min(parent.rect.width / DefenseArena.StageSize.x, parent.rect.height / DefenseArena.StageSize.y);
            rect.localScale = new Vector3(scale, scale, 1);
        }
    }
}
