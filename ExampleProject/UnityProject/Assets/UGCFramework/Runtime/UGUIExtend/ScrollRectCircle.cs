using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/ScrollRectCircle")]
    public class ScrollRectCircle : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private ScrollRect scrollRect;
        private float canvasScale;
        private bool enableCircle;

        void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
            canvasScale = GetComponentInParent<Canvas>().GetComponent<RectTransform>().localScale.x;
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            enableCircle = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            enableCircle = true;
        }

        void OnScrollValueChanged(Vector2 v2)
        {
            if (!enableCircle)
                return;
            if (scrollRect.content.childCount < 3)
                return;
            RectTransform rtfFirst = scrollRect.content.GetChild(0).GetComponent<RectTransform>();
            Vector2 firstV2 = rtfFirst.rect.size;
            RectTransform rtfLast = scrollRect.content.GetChild(scrollRect.content.childCount - 1).GetComponent<RectTransform>();
            Vector2 listV2 = rtfLast.rect.size;
            Rect viewportRect = scrollRect.viewport.rect;
            Vector2 viewportPosition = (Vector2)scrollRect.viewport.position - viewportRect.size * (scrollRect.viewport.pivot - Vector2.one * 0.5f) * canvasScale;
            if (scrollRect.horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = scrollRect.content.GetComponent<HorizontalLayoutGroup>();
                if (Mathf.Abs(rtfLast.position.x - viewportPosition.x) * canvasScale < Mathf.Max(listV2.x, viewportRect.width) * 0.5f
                    || rtfLast.position.x <= viewportPosition.x)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.right * (firstV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
                else if (Mathf.Abs(rtfFirst.position.x - viewportPosition.x) * canvasScale < Mathf.Max(firstV2.x, viewportRect.width) * 0.5f
                    || rtfFirst.position.x >= viewportPosition.x)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.left * (listV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
            }
            else if (scrollRect.vertical)
            {
                VerticalLayoutGroup verticalLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
                if (Mathf.Abs(rtfLast.position.y - viewportPosition.y) * canvasScale < Mathf.Max(listV2.y, viewportRect.height) * 0.5f
                    || rtfLast.position.y >= viewportPosition.y)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.down * (firstV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
                else if (Mathf.Abs(rtfFirst.position.y - viewportPosition.y) * canvasScale < Mathf.Max(firstV2.y, viewportRect.height) * 0.5f
                    || rtfFirst.position.y <= viewportPosition.y)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.up * (listV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
            }
        }
    }
}