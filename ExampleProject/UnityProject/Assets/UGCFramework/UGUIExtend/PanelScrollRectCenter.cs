using System.Collections;
using UGCF.Manager;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public bool isAutoLoop, isCircle;
        public float loopTimeSpace = 5;
        public float dragThresholdTime = 0.3f;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public int speed = 10;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
        public RectTransform content;
        float startDragTime;
        float screenToCanvasScale;
        int currentIndex = -1;
        Coroutine coroutine;

        void Start()
        {
            if (isAutoLoop)
                coroutine = UGCFMain.Instance.StartCoroutine(LoopPlay());
            screenToCanvasScale = FindObjectOfType<Canvas>().GetComponent<RectTransform>().localScale.x;
        }

        void OnEnable()
        {
            if (defaultFirst)
                StartCoroutine(DelayInit());
        }

        IEnumerator DelayInit()
        {
            yield return WaitForUtils.WaitFrame;
            SetCenter(GetCenter(), false);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            startDragTime = Time.realtimeSinceStartup;
            moving = false;
            if (coroutine != null)
                EternalGameObject.Instance.StopCoroutine(coroutine);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (horizontal || vertical)
            {
                if (horizontal)
                {
                    content.localPosition += Vector3.right * eventData.delta.x / screenToCanvasScale;
                    ValueChange(eventData.delta.x);
                }
                if (vertical)
                {
                    content.localPosition += Vector3.up * eventData.delta.y / screenToCanvasScale;
                    ValueChange(eventData.delta.y);
                }

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
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Transform centerTf = GetCenter();
            if (!centerTf || !content)
                return;
            if (Time.realtimeSinceStartup - startDragTime < dragThresholdTime)
            {
                Transform targetTf = null;
                if (horizontal)
                {
                    if (eventData.delta.x < 0)
                        targetTf = GetRightEnableTransform(centerTf);
                    else if (eventData.delta.x > 0)
                        targetTf = GetLeftEnableTransform(centerTf);
                    else
                        targetTf = centerTf;
                }
                else if (vertical)
                {
                    if (eventData.delta.y > 0)
                        targetTf = GetRightEnableTransform(centerTf);
                    else if (eventData.delta.y < 0)
                        targetTf = GetLeftEnableTransform(centerTf);
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
            if (coroutine != null)
                coroutine = UGCFMain.Instance.StartCoroutine(LoopPlay());
        }

        void ValueChange(float scaleSpaceToCenter)
        {
            if (!isCircle)
                return;
            if (scaleSpaceToCenter == 0)
                return;
            RectTransform rtfFirst = content.GetChild(0).GetComponent<RectTransform>();
            RectTransform rtfLast = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
            if (horizontal)
            {
                if (scaleSpaceToCenter < 0 && Mathf.Abs(rtfFirst.transform.position.x - transform.position.x) * screenToCanvasScale > rtfFirst.rect.width)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.right * rtfFirst.rect.width;
                }
                else if (scaleSpaceToCenter > 0 && Mathf.Abs(rtfLast.transform.position.x - transform.position.x) * screenToCanvasScale > rtfLast.rect.width)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.left * rtfLast.rect.width;
                }
            }
            else if (vertical)
            {
                if (scaleSpaceToCenter > 0 && Mathf.Abs(rtfFirst.transform.position.y - transform.position.y) * screenToCanvasScale > rtfFirst.rect.height)
                {
                    rtfFirst.SetAsLastSibling();
                    content.localPosition += Vector3.down * rtfLast.rect.height;
                }
                else if (scaleSpaceToCenter < 0 && Mathf.Abs(rtfLast.transform.position.y - transform.position.y) * screenToCanvasScale > rtfLast.rect.height)
                {
                    rtfLast.SetAsFirstSibling();
                    content.localPosition += Vector3.up * rtfFirst.rect.height;
                }
            }
        }

        Transform GetLeftEnableTransform(Transform current)
        {
            Transform tf = null;
            int targetIndex = current.GetSiblingIndex() - 1;
            while (targetIndex >= 0)
            {
                Transform temp = content.GetChild(targetIndex);
                if (temp.gameObject.activeInHierarchy)
                    return temp;
                else
                    targetIndex--;
            }
            return tf;
        }

        Transform GetRightEnableTransform(Transform current)
        {
            Transform tf = null;
            int targetIndex = current.GetSiblingIndex() + 1;
            while (content.childCount > targetIndex)
            {
                Transform temp = content.GetChild(targetIndex);
                if (temp.gameObject.activeInHierarchy)
                    return temp;
                else
                    targetIndex++;
            }
            return tf;
        }

        RectTransform GetCenter()
        {
            Transform centerTf = null;
            for (int i = 0; i < content.childCount; i++)
            {
                Transform tf = content.GetChild(i);
                if (!tf.gameObject.activeInHierarchy)
                    continue;
                if (!centerTf || Vector2.SqrMagnitude(tf.localPosition + content.localPosition)
                    < Vector2.SqrMagnitude(centerTf.localPosition + content.localPosition))
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
            if (!center) center = content.GetChild(0).GetComponent<RectTransform>();
            startMovingPosition = content.localPosition;
            if (horizontal)
                targetMovingPosition = new Vector2(-center.localPosition.x, startMovingPosition.y);
            if (vertical)
                targetMovingPosition = new Vector2(startMovingPosition.x, -center.localPosition.y);
            if (isPlayAnimation)
                UGCFMain.Instance.StartCoroutine(Moving());
            else
                content.localPosition = targetMovingPosition;
            onItemChanged?.Invoke(center.gameObject);
            currentIndex = center.GetSiblingIndex();
            return true;
        }

        bool moving = false;
        Vector2 startMovingPosition;
        Vector2 targetMovingPosition;
        IEnumerator Moving()
        {
            moving = true;
            float t = 0;
            while (moving)
            {
                if (!this || !content)
                    break;
                content.localPosition = Vector2.Lerp(startMovingPosition, targetMovingPosition, t);
                if (t >= 1)
                    break;
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
                        currentIndex = 0;
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