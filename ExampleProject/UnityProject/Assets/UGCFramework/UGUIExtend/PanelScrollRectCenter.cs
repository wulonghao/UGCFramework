using System.Collections;
using UGCF.Manager;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class PanelScrollRectCenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public delegate void ScrollRectValueChangeEvent(GameObject go, float scaleSpaceToCenter);
        public ScrollRectValueChangeEvent onValueChanged;
        public delegate void ScrollRectItemChangeEvent(GameObject go);
        public ScrollRectItemChangeEvent onItemChanged;

        public bool vertical;
        public bool horizontal;
        public bool defaultFirst = true;
        public bool isCircle, isAutoLoop;
        public float loopTimeSpace = 5;
        public float dragThresholdTime = 0.3f;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public int speed = 10;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public RectTransform content;
        bool moving;
        float startDragTime;
        float screenToCanvasScale;
        int currentIndex = -1;
        Coroutine coroutineLoopPlay;

        void Start()
        {
            if (isAutoLoop)
                coroutineLoopPlay = UGCFMain.Instance.StartCoroutine(LoopPlay());
            screenToCanvasScale = GetComponentInParent<Canvas>().GetComponent<RectTransform>().localScale.x;
        }

        void OnEnable()
        {
            StartCoroutine(DelayInit());
        }

        IEnumerator DelayInit()
        {
            yield return WaitForUtils.WaitFrame;
            if (defaultFirst)
                SetCenter(null, false);
            else
                SetCenter(GetCenter(), false);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!horizontal && !vertical)
                return;
            startDragTime = Time.realtimeSinceStartup;
            moving = false;
            RefreshCircle(eventData.delta);
            if (coroutineLoopPlay != null)
                UGCFMain.Instance.StopCoroutine(coroutineLoopPlay);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (horizontal && vertical)
            {
                content.localPosition += new Vector3(eventData.delta.x / screenToCanvasScale, eventData.delta.y / screenToCanvasScale);
            }
            else if (horizontal)
            {
                content.localPosition += Vector3.right * eventData.delta.x / screenToCanvasScale;
                CircleChange(eventData.delta.x);
            }
            else if (vertical)
            {
                content.localPosition += Vector3.up * eventData.delta.y / screenToCanvasScale;
                CircleChange(eventData.delta.y);
            }
            else
                return;

            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform crtf = content.GetChild(i).GetComponent<RectTransform>();
                Vector2 diff = crtf.localPosition - transform.localPosition;
                float scale = 0;
                if (horizontal)
                    scale = Mathf.Abs(diff.x) / crtf.rect.width;
                else if (vertical)
                    scale = Mathf.Abs(diff.y) / crtf.rect.height;
                if (scale > 1)
                    scale = 1;
                onValueChanged?.Invoke(crtf.gameObject, scale);
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!horizontal && !vertical)
                return;
            Transform centerTf = GetCenter();
            if (!centerTf || !content)
                return;
            if (Time.realtimeSinceStartup - startDragTime < dragThresholdTime && !(horizontal && vertical))
            {
                Transform targetTf = null;
                if (horizontal)
                {
                    if (eventData.delta.x < 0)
                        targetTf = GetNextEnableItem(centerTf);
                    else if (eventData.delta.x > 0)
                        targetTf = GetLastEnableItem(centerTf);
                    else
                        targetTf = centerTf;
                }
                else if (vertical)
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
            {
                SetCenter(centerTf);
            }
            if (coroutineLoopPlay != null)
                coroutineLoopPlay = UGCFMain.Instance.StartCoroutine(LoopPlay());
        }

        void RefreshCircle(Vector2 direction)
        {
            if (horizontal)
                CircleChange(direction.x);
            else if (vertical)
                CircleChange(direction.y);
        }

        void CircleChange(float scaleSpaceToCenter)
        {
            if (!isCircle || (horizontal && vertical))
                return;
            if (scaleSpaceToCenter == 0)
                return;
            RectTransform rtfFirst = content.GetChild(0).GetComponent<RectTransform>();
            RectTransform rtfLast = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
            RectTransform contentParent = content.parent.GetComponent<RectTransform>();
            if (horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = content.GetComponent<HorizontalLayoutGroup>();
                if (scaleSpaceToCenter < 0
                    && Mathf.Abs(rtfLast.transform.position.x - content.parent.position.x) * screenToCanvasScale < Mathf.Max(rtfLast.rect.width, contentParent.rect.width) * 0.5f)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.right * (rtfFirst.rect.width + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
                else if (scaleSpaceToCenter > 0 
                    && Mathf.Abs(rtfFirst.transform.position.x - content.parent.position.x) * screenToCanvasScale < Mathf.Max(rtfFirst.rect.width, contentParent.rect.width) * 0.5f)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.left * (rtfLast.rect.width + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
            }
            else if (vertical)
            {
                VerticalLayoutGroup verticalLayout = content.GetComponent<VerticalLayoutGroup>();
                if (scaleSpaceToCenter > 0 
                    && Mathf.Abs(rtfLast.transform.position.y - content.parent.position.y) * screenToCanvasScale < Mathf.Max(rtfLast.rect.height, contentParent.rect.height) * 0.5f)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.down * (rtfFirst.rect.height + (verticalLayout ? verticalLayout.spacing : 0));
                }
                else if (scaleSpaceToCenter < 0 
                    && Mathf.Abs(rtfFirst.transform.position.y - content.parent.position.y) * screenToCanvasScale < Mathf.Max(rtfFirst.rect.height, contentParent.rect.height) * 0.5f)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.up * (rtfLast.rect.height + (verticalLayout ? verticalLayout.spacing : 0));
                }
            }
        }

        Transform GetLastEnableItem(Transform current)
        {
            int targetIndex = current.GetSiblingIndex() - 1;
            while (targetIndex >= 0)
            {
                Transform temp = content.GetChild(targetIndex);
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
            while (content.childCount > targetIndex)
            {
                Transform temp = content.GetChild(targetIndex);
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
            for (int i = 0; i < content.childCount; i++)
            {
                Transform tf = content.GetChild(i);
                if (!tf.gameObject.activeInHierarchy)
                    continue;
                if (!centerTf ||
                    Vector2.SqrMagnitude(tf.localPosition + content.localPosition) < Vector2.SqrMagnitude(centerTf.localPosition + content.localPosition))
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
            if (content.childCount == 0)
                return false;

            if (!center)
                center = content.GetComponentsInChildren<RectTransform>(false)[1];

            Vector2 startMovingPosition = content.localPosition;
            Vector2 targetMovingPosition = content.localPosition;

            if (horizontal && vertical)
                targetMovingPosition = new Vector2(-center.localPosition.x, -center.localPosition.y);
            else if (horizontal)
                targetMovingPosition = new Vector2(-center.localPosition.x, startMovingPosition.y);
            else if (vertical)
                targetMovingPosition = new Vector2(startMovingPosition.x, -center.localPosition.y);

            if (isPlayAnimation)
                StartCoroutine(Moving(startMovingPosition, targetMovingPosition));
            else
                content.localPosition = targetMovingPosition;

            onItemChanged?.Invoke(center.gameObject);
            currentIndex = center.GetSiblingIndex();
            return true;
        }

        IEnumerator Moving(Vector2 startMovingPosition, Vector2 targetMovingPosition)
        {
            moving = true;
            float t = 0;
            while (moving)
            {
                if (this && content)
                    content.localPosition = Vector2.Lerp(startMovingPosition, targetMovingPosition, Mathf.Clamp01(t));
                if (t >= 1)
                {
                    moving = false;
                    RefreshCircle(targetMovingPosition - startMovingPosition);
                    break;
                }
                t += Time.deltaTime * speed;
                yield return WaitForUtils.WaitFrame;
            }
        }

        IEnumerator LoopPlay()
        {
            while (isAutoLoop)
            {
                yield return WaitForUtils.WaitForSecondsRealtime(loopTimeSpace);
                if (!this) break;
                do
                {
                    if (currentIndex + 1 >= content.childCount)
                    {
                        if (!isCircle)
                            currentIndex = 0;
                    }
                    else
                        currentIndex++;
                }
                while (content.childCount > 0 && currentIndex < content.childCount && !content.GetChild(currentIndex).gameObject.activeSelf);
                if (currentIndex < content.childCount)
                    SetCenter(content.GetChild(currentIndex));
            }
        }
    }
}