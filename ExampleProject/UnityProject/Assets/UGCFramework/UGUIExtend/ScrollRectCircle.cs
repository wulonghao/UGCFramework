using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/ScrollRectCircle")]
    public class ScrollRectCircle : MonoBehaviour
    {
        private ScrollRect scrollRect;
        /// <summary> 切换循环的最小和最大触发点，值基于scrollRect.normalizedPosition </summary>
        [SerializeField] private Vector2 limitPosition = new Vector2(0.2f, 0.8f);
        public Vector2 LimitPosition { get => limitPosition; set => limitPosition = value; }
        public bool IsDraging { get; set; }

        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        void Start()
        {
            UGUIEventListenerContainDrag.Get(scrollRect.gameObject).OnDragStart += delegate { IsDraging = true; };
            UGUIEventListenerContainDrag.Get(scrollRect.gameObject).OnDragEnd += delegate { IsDraging = false; };
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        void OnScrollValueChanged(Vector2 v2)
        {
            if (scrollRect.content.childCount < 3)
                return;
            if (IsDraging)
                return;
            ValueChange(v2);
        }

        void ValueChange(Vector2 v2)
        {
            RectTransform rtfFirst = scrollRect.content.GetChild(0).GetComponent<RectTransform>();
            RectTransform rtfLast = scrollRect.content.GetChild(scrollRect.content.childCount - 1).GetComponent<RectTransform>();
            if (scrollRect.horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = scrollRect.content.GetComponent<HorizontalLayoutGroup>();
                if (scrollRect.content.rect.width < scrollRect.viewport.rect.width * 2)
                    return;
                if (v2.x.CompareTo(LimitPosition.y) > 0)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.right * rtfFirst.rect.width;
                }
                else if (v2.x.CompareTo(LimitPosition.x) < 0)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.left * rtfLast.rect.width;
                }
            }
            else if (scrollRect.vertical)
            {
                VerticalLayoutGroup verticalLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
                if (scrollRect.content.rect.height < scrollRect.viewport.rect.height * 2)
                    return;
                if (v2.y.CompareTo(LimitPosition.x) < 0)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.down * rtfLast.rect.height;
                }
                else if (v2.y.CompareTo(LimitPosition.y) > 0)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.up * rtfFirst.rect.height;
                }
            }
        }
    }
}