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
        private float screenToCanvasScale;
        private bool enableCircle;

        void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
            screenToCanvasScale = GetComponentInParent<Canvas>().GetComponent<RectTransform>().localScale.x;
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            enableCircle = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            enableCircle = false;
        }

        void OnScrollValueChanged(Vector2 v2)
        {
            if (enableCircle)
                return;
            if (scrollRect.content.childCount < 3)
                return;
            RectTransform rtfFirst = scrollRect.content.GetChild(0).GetComponent<RectTransform>();
            Vector2 firstV2 = rtfFirst.rect.size;
            RectTransform rtfLast = scrollRect.content.GetChild(scrollRect.content.childCount - 1).GetComponent<RectTransform>();
            Vector2 listV2 = rtfLast.rect.size;
            Rect contentParentRect = scrollRect.viewport.rect;
            Vector2 contentParentPotsition = scrollRect.viewport.position;
            if (scrollRect.horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = scrollRect.content.GetComponent<HorizontalLayoutGroup>();
                if (Mathf.Abs(rtfLast.position.x - contentParentPotsition.x) * screenToCanvasScale < Mathf.Max(listV2.x, contentParentRect.width) * 0.5f
                    || rtfLast.position.x <= contentParentPotsition.x)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.right * (firstV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
                else if (Mathf.Abs(rtfFirst.position.x - contentParentPotsition.x) * screenToCanvasScale < Mathf.Max(firstV2.x, contentParentRect.width) * 0.5f
                    || rtfFirst.position.x >= contentParentPotsition.x)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.left * (listV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
            }
            else if (scrollRect.vertical)
            {
                VerticalLayoutGroup verticalLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
                if (Mathf.Abs(rtfLast.position.y - contentParentPotsition.y) * screenToCanvasScale < Mathf.Max(listV2.y, contentParentRect.height) * 0.5f
                    || rtfLast.position.y >= contentParentPotsition.y)
                {
                    rtfFirst.SetAsLastSibling();
                    scrollRect.content.localPosition += Vector3.down * (firstV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
                else if (Mathf.Abs(rtfFirst.position.y - contentParentPotsition.y) * screenToCanvasScale < Mathf.Max(firstV2.y, contentParentRect.height) * 0.5f
                    || rtfFirst.position.y <= contentParentPotsition.y)
                {
                    rtfLast.SetAsFirstSibling();
                    scrollRect.content.localPosition += Vector3.up * (listV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
            }
        }
    }
}