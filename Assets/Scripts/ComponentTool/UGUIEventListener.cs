using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using XLua;

[Hotfix]
public class UGUIEventListener : MonoBehaviour,
                                IPointerClickHandler,
                                IPointerDownHandler,
                                IPointerEnterHandler,
                                IPointerExitHandler,
                                IPointerUpHandler
{
    public delegate void VoidDelegate(GameObject go);
    public AudioSoundType m_AudioType;

    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onLongPress;
    //与长按配合使用
    bool isUp = true;
    float time = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            if (m_AudioType != AudioSoundType.None && GetComponent<Button>() != null && GetComponent<Button>().interactable)
                if (!AudioManager.Instance.IsPlayingSound)
                    AudioManager.Instance.PlaySound(m_AudioType);
            if (GetComponent<Button>() == null || GetComponent<Button>().interactable)
                onClick(gameObject);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
        if (isUp) time = Time.realtimeSinceStartup;
        isUp = false;
    }
    public void OnPointerEnter(PointerEventData eventData) { if (onEnter != null)  onEnter(gameObject); }
    public void OnPointerExit(PointerEventData eventData) { if (onExit != null) onExit(gameObject); }
    public void OnPointerUp(PointerEventData eventData) { if (onUp != null) onUp(gameObject); isUp = true; time = 0; }

    public static UGUIEventListener Get(GameObject go, AudioSoundType clickAudio = AudioSoundType.BtnClick)
    {
        UGUIEventListener listener = go.GetComponent<UGUIEventListener>();
        if (listener == null) listener = go.AddComponent<UGUIEventListener>();
        listener.m_AudioType = clickAudio;
        return listener;
    }

    void Update()
    {
        //长按
        if (!isUp && onLongPress != null && Time.realtimeSinceStartup - time > 1)
        {
            onLongPress(gameObject);
            isUp = true;
        }
    }
}