using System.Collections;
using System.Collections.Generic;
using System.Text;
using UGCF.Manager;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGCF.UGUIExtend
{
    public class PanelCenterScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public delegate void ScrollRectItemChangeEvent(GameObject go);
        public ScrollRectItemChangeEvent onItemChanged;

        public bool vertical;
        public bool horizontal;
        public bool defaultFirst = true;
        public bool isCircle, isAutoLoop;
        public float loopTimeSpace = 5;
        public float dragThresholdTime = 0.3f;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public int speed = 10;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public RectTransform viewport;
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
            if (content.childCount < 3)
                return;
            RectTransform rtfFirst = content.GetChild(0).GetComponent<RectTransform>();
            Vector2 firstV2 = rtfFirst.rect.size;
            RectTransform rtfLast = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
            Vector2 listV2 = rtfLast.rect.size;
            Rect contentParentRect = viewport.rect;
            Vector2 contentParentPotsition = viewport.position;
            if (horizontal)
            {
                HorizontalLayoutGroup horizontalLayout = content.GetComponent<HorizontalLayoutGroup>();
                if ((scaleSpaceToCenter < 0
                    && Mathf.Abs(rtfLast.position.x - contentParentPotsition.x) * screenToCanvasScale < Mathf.Max(listV2.x, contentParentRect.width) * 0.5f)
                    || rtfLast.position.x <= contentParentPotsition.x)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.right * (firstV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
                else if ((scaleSpaceToCenter > 0
                    && Mathf.Abs(rtfFirst.position.x - contentParentPotsition.x) * screenToCanvasScale < Mathf.Max(firstV2.x, contentParentRect.width) * 0.5f)
                    || rtfFirst.position.x >= contentParentPotsition.x)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.left * (listV2.x + (horizontalLayout ? horizontalLayout.spacing : 0));
                }
            }
            else if (vertical)
            {
                VerticalLayoutGroup verticalLayout = content.GetComponent<VerticalLayoutGroup>();
                if (scaleSpaceToCenter > 0
                    && Mathf.Abs(rtfLast.position.y - contentParentPotsition.y) * screenToCanvasScale < Mathf.Max(listV2.y, contentParentRect.height) * 0.5f
                    || rtfLast.position.y >= contentParentPotsition.y)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.down * (firstV2.y + (verticalLayout ? verticalLayout.spacing : 0));
                }
                else if (scaleSpaceToCenter < 0
                    && Mathf.Abs(rtfFirst.position.y - contentParentPotsition.y) * screenToCanvasScale < Mathf.Max(firstV2.y, contentParentRect.height) * 0.5f
                    || rtfFirst.position.y <= contentParentPotsition.y)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.up * (listV2.y + (verticalLayout ? verticalLayout.spacing : 0));
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
            Vector2 centerPosition = (Vector2)center.localPosition - center.rect.size * (center.pivot - Vector2.one * 0.5f);

            if (horizontal && vertical)
                targetMovingPosition = new Vector2(-centerPosition.x, -centerPosition.y);
            else if (horizontal)
                targetMovingPosition = new Vector2(-centerPosition.x, startMovingPosition.y);
            else if (vertical)
                targetMovingPosition = new Vector2(startMovingPosition.x, -centerPosition.y);

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