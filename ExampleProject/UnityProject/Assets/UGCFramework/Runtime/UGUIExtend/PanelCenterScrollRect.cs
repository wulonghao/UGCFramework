using System.Collections;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    [AddComponentMenu("UI/PanelCenterScrollRect")]
    public class PanelCenterScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public delegate void ScrollRectItemChangeEvent(GameObject center);
        public ScrollRectItemChangeEvent OnItemChanged { get; set; }

        #region ...字段
        [SerializeField] private bool vertical;
        [SerializeField] private bool horizontal;
        [SerializeField] private bool defaultFirst = true;
        [SerializeField] private bool isCircle;
        [SerializeField] private bool isAutoLoop;
        [SerializeField] private float loopTimeSpace = 5;
        [SerializeField] [Range(0, 2)] private float dragThresholdTime = 0.3f;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        [SerializeField] [Range(0, 100)] private int speed = 10;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        private bool moving;
        private float startDragTime;
        private Vector2 canvasScale;
        private int currentIndex = -1;
        private Coroutine coroutineLoopPlay;
        #endregion

        #region ...属性
        public bool Vertical { get => vertical; set => vertical = value; }
        public bool Horizontal { get => horizontal; set => horizontal = value; }
        public bool DefaultFirst { get => defaultFirst; set => defaultFirst = value; }
        public bool IsCircle { get => isCircle; set => isCircle = value; }
        public bool IsAutoLoop { get => isAutoLoop; set => isAutoLoop = value; }
        public float LoopTimeSpace { get => loopTimeSpace; set => loopTimeSpace = value; }
        public float DragThresholdTime { get => dragThresholdTime; set => dragThresholdTime = value; }
        public int Speed { get => speed; set => speed = value; }
        public RectTransform Viewport { get => viewport; set => viewport = value; }
        public RectTransform Content { get => content; set => content = value; }
        #endregion

        void Start()
        {
            if (isAutoLoop)
                coroutineLoopPlay = StartCoroutine(LoopPlay());
            canvasScale = GetComponentInParent<Canvas>().GetComponent<RectTransform>().localScale;
        }

        void OnEnable()
        {
            StartCoroutine(DelayInit());
        }

        IEnumerator DelayInit()
        {
            yield return WaitForUtils.WaitFrame;
            if (DefaultFirst)
            {
                SetCenter(null, false);
                RefreshCircle(new Vector2(1, -1));
            }
            else
                SetCenter(GetCenter(), false);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!Horizontal && !Vertical)
                return;
            startDragTime = Time.realtimeSinceStartup;
            moving = false;
            RefreshCircle(eventData.delta);
            if (coroutineLoopPlay != null)
                StopCoroutine(coroutineLoopPlay);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (canvasScale.x == 0 || canvasScale.y == 0)
                return;
            if (Horizontal && Vertical)
            {
                content.localPosition += new Vector3(eventData.delta.x / canvasScale.x, eventData.delta.y / canvasScale.y);
            }
            else if (Horizontal)
            {
                content.localPosition += Vector3.right * eventData.delta.x / canvasScale.x;
                CircleChange(eventData.delta.x);
            }
            else if (Vertical)
            {
                content.localPosition += Vector3.up * eventData.delta.y / canvasScale.y;
                CircleChange(eventData.delta.y);
            }
            else
                return;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!Horizontal && !Vertical)
                return;
            Transform centerTf = GetCenter();
            if (!centerTf || !Content)
                return;
            if (Time.realtimeSinceStartup - startDragTime < DragThresholdTime && !(Horizontal && Vertical))
            {
                Transform targetTf = null;
                if (Horizontal)
                {
                    if (eventData.delta.x < 0)
                        targetTf = GetNextEnableItem(centerTf);
                    else if (eventData.delta.x > 0)
                        targetTf = GetLastEnableItem(centerTf);
                    else
                        targetTf = centerTf;
                }
                else if (Vertical)
                {
                    if (eventData.delta.y > 0)
                        targetTf = GetNextEnableItem(centerTf);
                    else if (eventData.delta.y < 0)
                        targetTf = GetLastEnableItem(centerTf);
                    else
                        targetTf = centerTf;
                }
                if (!targetTf)
                    targetTf = centerTf;
                SetCenter(targetTf);
            }
            else
                SetCenter(centerTf);
            if (IsAutoLoop)
                StartCoroutine(LoopPlay());
        }

        void RefreshCircle(Vector2 direction)
        {
            if (Horizontal)
                CircleChange(direction.x);
            else if (Vertical)
                CircleChange(direction.y);
        }

        void CircleChange(float scaleSpaceToCenter)
        {
            if (!IsCircle || (Horizontal && Vertical))
                return;
            if (Content.childCount < 3)
                return;
            RectTransform rtfFirst = Content.GetChild(0).GetComponent<RectTransform>();
            Vector2 firstV2 = rtfFirst.rect.size;
            RectTransform rtfLast = Content.GetChild(Content.childCount - 1).GetComponent<RectTransform>();
            Vector2 listV2 = rtfLast.rect.size;
            Rect viewportRect = Viewport.rect;
            Vector2 viewportPotsition = GetViewportWorldPosition();
            if (Horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = Content.GetComponent<HorizontalLayoutGroup>();
                if (scaleSpaceToCenter < 0
                    && Mathf.Abs(rtfLast.position.x - viewportPotsition.x) * canvasScale.x < Mathf.Max(listV2.x, viewportRect.width) * 0.5f
                    || rtfLast.position.x < viewportPotsition.x)
                {
                    rtfFirst.SetAsLastSibling();
                    Content.localPosition += Vector3.right * (firstV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
                else if (scaleSpaceToCenter > 0
                    && Mathf.Abs(rtfFirst.position.x - viewportPotsition.x) * canvasScale.x < Mathf.Max(firstV2.x, viewportRect.width) * 0.5f
                    || rtfFirst.position.x >= viewportPotsition.x)
                {
                    rtfLast.SetAsFirstSibling();
                    Content.localPosition += Vector3.left * (listV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
            }
            else if (Vertical)
            {
                VerticalLayoutGroup verticalLayout = Content.GetComponent<VerticalLayoutGroup>();
                if (scaleSpaceToCenter > 0
                    && Mathf.Abs(rtfLast.position.y - viewportPotsition.y) * canvasScale.y < Mathf.Max(listV2.y, viewportRect.height) * 0.5f
                    || rtfLast.position.y > viewportPotsition.y)
                {
                    rtfFirst.SetAsLastSibling();
                    Content.localPosition += Vector3.down * (firstV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
                else if (scaleSpaceToCenter < 0
                    && Mathf.Abs(rtfFirst.position.y - viewportPotsition.y) * canvasScale.y < Mathf.Max(firstV2.y, viewportRect.height) * 0.5f
                    || rtfFirst.position.y <= viewportPotsition.y)
                {
                    rtfLast.SetAsFirstSibling();
                    Content.localPosition += Vector3.up * (listV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
            }
        }

        Vector2 GetViewportWorldPosition()
        {
            return (Vector2)Viewport.position - Viewport.rect.size * (Viewport.pivot - Vector2.one * 0.5f) * canvasScale;
        }

        Transform GetLastEnableItem(Transform current)
        {
            int targetIndex = current.GetSiblingIndex() - 1;
            while (targetIndex >= 0)
            {
                Transform temp = Content.GetChild(targetIndex);
                if (temp.gameObject.activeInHierarchy)
                    return temp;
                else
                    targetIndex--;
            }
            return current;
        }

        Transform GetNextEnableItem(Transform current)
        {
            int targetIndex = current.GetSiblingIndex() + 1;
            while (Content.childCount > targetIndex)
            {
                Transform temp = Content.GetChild(targetIndex);
                if (temp.gameObject.activeInHierarchy)
                    return temp;
                else
                    targetIndex++;
            }
            return current;
        }

        RectTransform GetCenter()
        {
            Transform centerTf = null;
            Vector3 viewportPotsition = GetViewportWorldPosition();
            for (int i = 0; i < Content.childCount; i++)
            {
                Transform tf = Content.GetChild(i);
                if (!tf.gameObject.activeInHierarchy)
                    continue;
                if (!centerTf ||
                    Vector2.SqrMagnitude(tf.position - viewportPotsition) < Vector2.SqrMagnitude(centerTf.position - viewportPotsition))
                {
                    centerTf = tf;
                }
            }
            if (centerTf)
                return centerTf.GetComponent<RectTransform>();
            return null;
        }

        public bool SetCenter(Transform center = null, bool isPlayAnimation = true)
        {
            return SetCenter(center.GetComponent<RectTransform>(), isPlayAnimation);
        }

        public bool SetCenter(RectTransform center = null, bool isPlayAnimation = true)
        {
            if (Content.childCount == 0)
                return false;

            if (!center)
                center = Content.GetComponentsInChildren<RectTransform>(false)[1];

            Vector2 startMovingPosition = Content.anchoredPosition;
            Vector2 targetMovingPosition = Content.anchoredPosition;
            Vector2 centerPosition = (Vector2)center.localPosition - center.rect.size * (center.pivot - Vector2.one * 0.5f);

            if (Horizontal && Vertical)
                targetMovingPosition = new Vector2(-centerPosition.x, -centerPosition.y);
            else if (Horizontal)
                targetMovingPosition = new Vector2(-centerPosition.x, startMovingPosition.y);
            else if (Vertical)
                targetMovingPosition = new Vector2(startMovingPosition.x, -centerPosition.y);

            if (isPlayAnimation)
                StartCoroutine(Moving(startMovingPosition, targetMovingPosition));
            else
                Content.anchoredPosition = targetMovingPosition;

            OnItemChanged?.Invoke(center.gameObject);
            currentIndex = center.GetSiblingIndex();
            return true;
        }

        IEnumerator Moving(Vector2 startMovingPosition, Vector2 targetMovingPosition)
        {
            moving = true;
            float t = 0;
            while (moving)
            {
                if (this && Content)
                    Content.anchoredPosition = Vector2.Lerp(startMovingPosition, targetMovingPosition, Mathf.Clamp01(t));
                if (t >= 1)
                {
                    moving = false;
                    RefreshCircle(targetMovingPosition - startMovingPosition);
                    break;
                }
                t += Time.deltaTime * Speed;
                yield return WaitForUtils.WaitFrame;
            }
        }

        IEnumerator LoopPlay()
        {
            while (IsAutoLoop)
            {
                yield return WaitForUtils.WaitForSecondsRealtime(LoopTimeSpace);
                if (!this) break;
                do
                {
                    if (currentIndex + 1 >= Content.childCount)
                    {
                        if (!IsCircle)
                            currentIndex = 0;
                    }
                    else
                        currentIndex++;
                }
                while (Content.childCount > 0 && currentIndex < Content.childCount && !Content.GetChild(currentIndex).gameObject.activeSelf);
                if (currentIndex < Content.childCount)
                    SetCenter(Content.GetChild(currentIndex));
            }
        }
    }
}