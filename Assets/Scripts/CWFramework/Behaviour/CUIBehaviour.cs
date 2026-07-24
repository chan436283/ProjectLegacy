using UnityEngine;

namespace CWFramework
{
    /// <summary>
    /// RectTransform을 사용하는 UI 컴포넌트의 공통 기반 클래스입니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CUIBehaviour : CBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        public Vector3 AnchoredPosition3D
        {
            get => RectTransform.anchoredPosition3D;
            set => RectTransform.anchoredPosition3D = value;
        }

        public Vector2 SizeDelta
        {
            get => RectTransform.sizeDelta;
            set => RectTransform.sizeDelta = value;
        }
    }
}
