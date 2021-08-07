using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UGCF.Utils;

public class UGUIEventListener : MonoBehaviour,
                                IPointerClickHandler,
                                IPointerDownHandler,
                                IPointerEnterHandler,
                                IPointerExitHandler,
                                IPointerUpHandler
{
    public delegate void UGUIDelegate(GameObject go);
    public delegate void UGUIDelegateData(GameObject go, PointerEventData eventData);

    public UGUIDelegate onClick;
    public UGUIDelegate onDoubleClick;
    public UGUIDelegate onDown;
    public UGUIDelegate onEnter;
    public UGUIDelegate onExit;
    public UGUIDelegate onUp;
    public UGUIDelegate onLongPress;

    public UGUIDelegateData onClickData;
    public UGUIDelegateData onDoubleClickData;
    public UGUIDelegateData onDownData;
    public UGUIDelegateData onEnterData;
    public UGUIDelegateData onExitData;
    public UGUIDelegateData onUpData;
    public UGUIDelegateData onLongPressData;
    public UGUIDelegateData onStayData;
    Coroutine longPressCoroutine;
    Coroutine stayCoroutine;
    Coroutine clearClickCountCoroutine;
    int continuousClickCount;

    public void OnPointerClick(PointerEventData eventData)
    {
        continuousClickCount++;
        InvokeEvent(onClick);
        InvokeEvent(onClickData, eventData);
        if (continuousClickCount == 2)
        {
            InvokeEvent(onDoubleClick);
            InvokeEvent(onDoubleClickData, eventData);
        }
        if (gameObject.activeInHierarchy)
        {
            if (clearClickCountCoroutine != null)
                StopCoroutine(clearClickCountCoroutine);
            clearClickCountCoroutine = StartCoroutine(ClearClickCount());
        }
    }

    IEnumerator ClearClickCount()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        continuousClickCount = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InvokeEvent(onDown);
        InvokeEvent(onDownData, eventData);
        if (onLongPress != null && gameObject.activeInHierarchy)
        {
            if (longPressCoroutine != null)
                StopCoroutine(longPressCoroutine);
            longPressCoroutine = StartCoroutine(LongPress(eventData));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InvokeEvent(onUp);
        InvokeEvent(onUpData, eventData);
        if (longPressCoroutine != null && gameObject.activeInHierarchy)
            StopCoroutine(longPressCoroutine);
    }

    IEnumerator LongPress(PointerEventData eventData)
    {
        float time = 0;
        while (time < 0.8f)
        {
            if (onLongPress == null)
                yield break;
            time += Time.deltaTime;
            yield return WaitForUtils.WaitFrame;
            if (eventData.IsPointerMoving())
                yield break;
        }
        InvokeEvent(onLongPress);
        InvokeEvent(onLongPressData, eventData);
        longPressCoroutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InvokeEvent(onEnter);
        InvokeEvent(onEnterData, eventData);
        if (onStayData != null && gameObject.activeInHierarchy)
        {
            if (stayCoroutine != null)
                StopCoroutine(stayCoroutine);
            stayCoroutine = StartCoroutine(Stay(eventData));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InvokeEvent(onExit);
        InvokeEvent(onExitData, eventData);
        if (stayCoroutine != null)
            StopCoroutine(stayCoroutine);
    }

    IEnumerator Stay(PointerEventData eventData)
    {
        while (true)
        {
            if (onStayData == null)
                yield break;
            yield return WaitForUtils.WaitFrame;
            InvokeEvent(onStayData, eventData);
        }
    }

    void InvokeEvent(UGUIDelegate uguiDelegate)
    {
        if (uguiDelegate != null)
        {
            Selectable selectable = GetComponent<Selectable>();
            if (!selectable || selectable.interactable)
                uguiDelegate(gameObject);
        }
    }

    void InvokeEvent(UGUIDelegateData uguiDelegateData, PointerEventData eventData)
    {
        if (uguiDelegateData != null)
        {
            Selectable selectable = GetComponent<Selectable>();
            if (!selectable || selectable.interactable)
                uguiDelegateData(gameObject, eventData);
        }
    }

    public static UGUIEventListener Get(GameObject go)
    {
        if (!go) return null;
        UGUIEventListener listener = go.GetComponent<UGUIEventListener>();
        if (listener == null)
            listener = go.AddComponent<UGUIEventListener>();
        return listener;
    }
}