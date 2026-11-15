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
            rect.sizeDelta = new Vector2(900, 1800);
            Fit();
        }

        private void Fit()
        {
            float scale = Mathf.Min(parent.rect.width / 900f, parent.rect.height / 1800f);
            rect.localScale = new Vector3(scale, scale, 1);
        }
    }
}
