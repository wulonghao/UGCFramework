using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelScrollRectCenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public delegate void ScrollRectValueChangeEvent(GameObject go, float scaleSpaceToCenter);
    public ScrollRectValueChangeEvent onValueChanged;
    public delegate void ScrollRectItemChangeEvent(GameObject go);
    public ScrollRectItemChangeEvent onItemChanged;

    public bool vertical;
    public bool horizontal;
    public float dragThresholdTime = 0.3f;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
    public int speed = 10;//拖拽检测时间阈值，低于阈值取最后一帧操作判断行为，否则以最近元素为目标元素
    public RectTransform content;
    float startDragTime;

    void OnEnable()
    {
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
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (horizontal || vertical)
        {
            if (horizontal)
                content.localPosition += Vector3.right * eventData.delta.x / EternalGameObject.screenWidthScale;
            if (vertical)
                content.localPosition += Vector3.up * eventData.delta.y / EternalGameObject.screenHeightScale;

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
                if (onValueChanged != null)
                    onValueChanged(crtf.gameObject, scale);
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
            EternalGameObject.Instance.StartCoroutine(Moving());
        else
            content.localPosition = targetMovingPosition;
        if (onItemChanged != null)
            onItemChanged(center.gameObject);
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
}
